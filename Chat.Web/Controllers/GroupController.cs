using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Chat.Web.Models;
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
                var members = _user.Chatterers.Where(c => c.InGroupId == _user.InGroupId);
                var onl = members.Where(c => c.ConnectionId != null).Select(c => c.Name).ToArray();
                var ofl = members.Where(c => c.ConnectionId == null).Select(c => c.Name).ToArray();
                return new JsonResult(new { online = onl, offline = ofl });
            }
            catch(Exception e)
            {
                return new JsonResult(e.Message);
            }
        }
        [HttpGet("msgs/{ticks}/{quantity:int:range(1,100)}")]
        public JsonResult Messages(string ticks, int quantity)
        {
            try
            {
                long lticks = long.Parse(ticks);
                var limit = DateTime.UtcNow.Subtract(TimeSpan.FromDays(15)).Ticks;
                if (lticks == 0) lticks = DateTime.UtcNow.Ticks;
                else if (lticks < limit)
                    return new JsonResult(-1);
                IQueryable<ChatterersDb.Message> messages;
                messages = _user.Database.Messages.Where(m => m.GroupId == _user.InGroupId && m.SharpTime < lticks && m.SharpTime >= limit).OrderBy(m => m.SharpTime);
                if (messages.Count() <= 0)
                    return new JsonResult(0);
                if (messages.Count() <= quantity)
                    return new JsonResult(messages);
                else
                    return new JsonResult(messages.Skip(messages.Count() - quantity));
            }
            catch(Exception e)
            {
                return new JsonResult(e.Message);
            }
        }
        [HttpGet("msgs/missed/{ticks}")]
        public JsonResult MessagesMissed(string ticks)
        {
            try
            {
                long lticks = long.Parse(ticks);
                if (lticks < DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks)
                    return new JsonResult(-1);
                else
                {
                    var messages = _user.Database.Messages.Where(m => m.GroupId == _user.InGroupId && m.SharpTime > lticks).OrderBy(m => m.SharpTime);
                    if (messages.Count() <= 0)
                        return new JsonResult(0);
                    else
                        return new JsonResult(messages);
                }
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
                    _user.Chatterer.GroupLastCleaned = DateTime.UtcNow.Ticks;
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
            if (_user.InGroupId != 0)
                ret = "already_signed";
            else
            {
                try
                {
                    if(request.Name == _user.Group)
                    {
                        _user.InGroupId = _user.Chatterer.Id;
                        _user.InGroupPassword = _user.GroupPassword;
                        await _user.SaveAsync();
                        ret = "OK";
                    }
                    else
                    {
                        var entity = _user.Chatterers.FirstOrDefault(c => c.Group == request.Name);
                        if (entity == null)
                            ret = "not_found";
                        else
                        {
                            if (entity.GroupPassword != request.Password)
                                ret = "wrong_password";
                            else
                            {
                                _user.InGroupId = entity.Id;
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
                if (_user.InGroupId == 0)
                    ret = "not_signed";
                else
                {
                    _user.InGroupId = 0;
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
                    _user.Database.Messages.RemoveRange(_user.Database.Messages.Where(m => m.GroupId == _user.Chatterer.Id));
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
