using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Chat.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Chat.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupsController : ControllerBase
    {
        private User _user;
        public GroupsController(User user)
        {
            _user = user;
        }
        [HttpGet("list/{start:int:min(0)}/{quantity:int:range(1,100)?}/{query:maxlength(64)?}")]
        public JsonResult Groups(int start, int quantity, string query = "")
        {
            try
            {
                var groups = _user.Chatterers.Select(c => c.Group).Where(g => g != null && g.StartsWith(query, StringComparison.OrdinalIgnoreCase));
                var groupsCount = groups.Count();
                if (groupsCount <= start)
                    return new JsonResult(0);
                if (quantity == 0)
                    return new JsonResult(groupsCount - start);
                if (start + quantity > groupsCount)
                    quantity = groupsCount - start;
                return new JsonResult(groups.OrderBy(g => g).Skip(start).Take(quantity).ToArray());
            }
            catch(Exception e)
            {
                return new JsonResult(e.Message);
            }
        }
        [HttpGet("members")]
        public JsonResult GroupMembers()
        {
            try
            {
                var members = _user.Chatterers.Where(c => c.InGroup == _user.InGroup);
                var onl = members.Where(c => c.ConnectionId != null).Select(c => c.Name).ToArray();
                var ofl = members.Where(c => c.ConnectionId == null).Select(c => c.Name).ToArray();
                return new JsonResult(new { online = onl, offline = ofl });
            }
            catch(Exception e)
            {
                return new JsonResult(e.Message);
            }
        }
        [HttpGet("msgs/before/{jsdate:long:min(0)}/{max:int:range(1,100)}")]
        public JsonResult MessagesBefore(long jsdate, int max)
        {
            try
            {
                long ticks = StaticData.JsMsToTicks(jsdate);
                long ticksLimit = DateTime.UtcNow.Date.Ticks - TimeSpan.FromDays(30).Ticks;
                if (ticks < ticksLimit)
                    return new JsonResult(-1);
                var messages = _user.Chatterers.Where(c => c.Group == _user.InGroup).SingleOrDefault()?.GroupMessages?.Where(m => m.Date >= ticksLimit && m.Date < ticks).OrderByDescending(m => m.Date);
                if (messages == null)
                    return new JsonResult(0);
                if (messages.Count() <= max)
                    return new JsonResult(messages);
                return new JsonResult(messages.Take(max));
            }
            catch(Exception e)
            {
                return new JsonResult(e.Message);
            }
        }
        [HttpGet("msgs/after/{jsdate:long:min(0)}/{max:int:range(1,100)}")]
        public JsonResult MessagesAfter(long jsdate, int max)
        {
            try
            {
                long ticks = StaticData.JsMsToTicks(jsdate);
                long ticksLimit = DateTime.UtcNow.Date.Ticks - TimeSpan.FromDays(30).Ticks;
                if (ticks < ticksLimit)
                    return new JsonResult(-1);
                var messages = _user.Chatterers.Where(c => c.Group == _user.InGroup).SingleOrDefault()?.GroupMessages?.Where(m => m.Date > ticks).OrderBy(m => m.Date);
                if (messages == null)
                    return new JsonResult(0);
                if (messages.Count() <= max)
                    return new JsonResult(messages);
                return new JsonResult(messages.Take(max));
            }
            catch(Exception e)
            {
                return new JsonResult(e.Message);
            }
        }
        [HttpPost("reg")]
        public async Task<ContentResult> Register([FromBody]GroupRegRequest request)
        {
            string ret;
            try
            {
                if (_user.Group != null)
                    ret = "has_group";
                else if (_user.Chatterers.Any(c => c.Group == request.GroupName))
                    ret = "name_taken";
                else if (request.Password != _user.Password)
                    ret = "wrong_password";
                else
                {
                    _user.Group = request.GroupName;
                    _user.GroupPassword = request.GroupPassword;
                    await _user.SaveAsync();
                    ret = "OK";
                }
            }
            catch(Exception e)
            {
                ret = e.Message;
            }
            return Content(ret, "text/plain");
        }
        [HttpPost("change")]
        public async Task<ContentResult> Change([FromBody]GroupChangeRequest request)
        {
            string ret;
            try
            {
                if (_user.Group == null)
                    ret = "has_no_group";
                else if (request.NewGroupName == null && request.NewGroupPassword != null && request.NewGroupPassword.Length < 8)
                    ret = "no_change_requested";
                else if (_user.Password != request.Password)
                    ret = "wrong_password";
                else
                {
                    if (request.NewGroupName != null && request.NewGroupName != _user.Group && request.NewGroupPassword != _user.GroupPassword
                            && (request.NewGroupPassword == null || request.NewGroupPassword.Length >= 8))
                    {
                        if (_user.Chatterers.Any(c => c.Group == request.NewGroupName))
                            ret = "group_name_exists";
                        else
                        {
                            _user.Group = request.NewGroupName;
                            _user.GroupPassword = request.NewGroupPassword;
                            await _user.SaveAsync();
                            ret = "name&pass_changed";
                        }

                    }
                    else if (request.NewGroupName != null && request.NewGroupName != _user.Group)
                    {
                        if (_user.Chatterers.Any(c => c.Group == request.NewGroupName))
                            ret = "group_name_exists";
                        else
                        {
                            _user.Group = request.NewGroupName;
                            await _user.SaveAsync();
                            ret = "name_changed";
                        }
                    }
                    else if (request.NewGroupPassword != _user.GroupPassword && (request.NewGroupPassword == null || request.NewGroupPassword.Length >= 8))
                    {
                        _user.GroupPassword = request.NewGroupPassword;
                        await _user.SaveAsync();
                        ret = "pass_changed";
                    }
                    else
                        ret = "not_changed";
                }
            }
            catch(Exception e)
            {
                ret = e.Message;
            }
            return Content(ret, "text/plain");
        }
        [HttpPost("sign")]
        public async Task<ContentResult> Sign([FromBody]GroupSignRequest request)
        {
            string ret;
            if (_user.InGroup != null)
                ret = "already_signed";
            else
            {
                try
                {
                    if(request.Name == _user.Group)
                    {
                        _user.InGroup = _user.Group;
                        _user.InGroupPassword = _user.GroupPassword;
                        await _user.SaveAsync();
                        ret = "OK";
                    }
                    else
                    {
                        var entity = _user.Chatterers.Where(c => c.Group == request.Name).SingleOrDefault();
                        if (entity == null)
                            ret = "not_found";
                        else
                        {
                            if (entity.GroupPassword != request.Password)
                                ret = "wrong_password";
                            else
                            {
                                _user.InGroup = request.Name;
                                _user.InGroupPassword = request.Password;
                                await _user.SaveAsync();
                                ret = "OK";
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    ret = e.Message;
                }
            }
            return Content(ret, "text/plain");
        }
        [HttpPost("sign/out")]
        public async Task<ContentResult> SignOut()
        {
            string ret;
            try
            {
                if (_user.InGroup == null)
                    ret = "not_signed";
                else
                {
                    _user.InGroup = null;
                    _user.InGroupPassword = null;
                    await _user.SaveAsync();
                    ret = "OK";
                }
            }
            catch(Exception e)
            {
                ret = e.Message;
            }
            return Content(ret, "text/plain");
        }
        [HttpPost("del")]
        public async Task<ContentResult> Delete([Required, StringLength(32, MinimumLength = 8), FromBody] string password)
        {
            string ret;
            try
            {
                if (_user.Group == null)
                    ret = "has_no_group";
                else if (_user.Password != password)
                    ret = "wrong_password";
                else
                {
                    _user.Group = null;
                    _user.GroupPassword = null;
                    await _user.SaveAsync();
                    ret = "deleted";
                }
            }
            catch(Exception e)
            {
                ret = e.Message;
            }
            return Content(ret, "text/plain");
        }
    }
}
