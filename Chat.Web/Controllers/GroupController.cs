using System;
using System.Collections.Generic;
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
        private ChatterersDb _dbContext;
        private User _user;
        public GroupsController(ChatterersDb dbContext, User user)
        {
            _dbContext = dbContext;
            _user = user;
        }
        [HttpGet("list/{query:maxlength(64)}/{start:int:min(0)}/{quantity:int:range(1,100)?}")]
        public JsonResult Groups(string query, int start, int quantity)
        {
            try
            {
                var groups = _dbContext.Chatterers.Select(c => c.Group).Where(g => g != null && g.StartsWith(query, StringComparison.OrdinalIgnoreCase));
                var groupsCount = groups.Count();
                if (groupsCount <= start)
                    return new JsonResult(0);
                if (quantity == 0)
                    return new JsonResult(groupsCount - start);
                if (start + quantity > groupsCount)
                    quantity = groupsCount - start;
                return new JsonResult(groups.OrderBy(o => o, StringComparer.OrdinalIgnoreCase).TakeLast(groupsCount - start).Take(quantity));
            }
            catch(Exception e)
            {
                return new JsonResult(e.Message);
            }
        }
        [HttpGet("members/{start:int:min(0)}/{quantity:int:range(1,100)?}")]
        public JsonResult GroupMembers(int start, int quantity)
        {
            try
            {
                var members = _dbContext.Chatterers.Where(c => c.InGroup == _user.InGroup).Select(c => c.Name);
                var membersCount = members.Count();
                if (membersCount <= start)
                    return new JsonResult(0);
                if (quantity == 0)
                    return new JsonResult(membersCount - start);
                if (start + quantity > membersCount)
                    quantity = membersCount - start;
                return new JsonResult(members.OrderBy(o => o, StringComparer.OrdinalIgnoreCase).TakeLast(membersCount - start).Take(quantity));
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
                var messages = _dbContext.Chatterers.Where(c => c.Group == _user.InGroup).SingleOrDefault()?.GroupMessages?.Where(m => m.Date >= ticksLimit && m.Date < ticks).OrderByDescending(m => m.Date);
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
                var messages = _dbContext.Chatterers.Where(c => c.Group == _user.InGroup).SingleOrDefault()?.GroupMessages?.Where(m => m.Date > ticks).OrderBy(m => m.Date);
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
                else if (_dbContext.Chatterers.Any(c => c.Group == request.GroupName))
                    ret = "name_taken";
                else
                {
                    var chatterer = _dbContext.Chatterers.Where(c => c.Name == _user.Name).Single();
                    if (request.Password != chatterer.Password)
                        ret = "wrong_password";
                    else
                    {
                        chatterer.Group = request.GroupName;
                        chatterer.GroupPassword = request.GroupPassword;
                        await _dbContext.SaveChangesAsync();
                        ret = "OK";
                    }
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
                else
                {
                    var chatterer = _dbContext.Chatterers.Where(c => c.Name == _user.Name).Single();
                    if (chatterer.Password != request.Password)
                        ret = "wrong_password";
                    else
                    {
                        bool name = false, pass = false;
                        if (request.NewGroupName != null && request.NewGroupName != chatterer.Group)
                        {
                            name = true;
                            chatterer.Group = request.NewGroupName;
                        }
                        if (request.NewGroupPassword != chatterer.GroupPassword && (request.NewGroupPassword == null || request.NewGroupPassword.Length >= 8)) 
                        {
                            pass = true;
                            chatterer.GroupPassword = request.NewGroupPassword;
                        }
                        if (name && pass)
                        {
                            await _dbContext.SaveChangesAsync();
                            ret = "name&pass_changed";
                        }
                        else if (name)
                        {
                            await _dbContext.SaveChangesAsync();
                            ret = "name_changed";
                        }
                        else if (pass)
                        {
                            await _dbContext.SaveChangesAsync();
                            ret = "pass_changed";
                        }
                        else
                            ret = "not_changed";
                    }
                }
            }
            catch(Exception e)
            {
                ret = e.Message;
            }
            return Content(ret, "text/plain");
        }
        [HttpPost("sign")]
        public async Task<ContentResult> Sign([FromBody]GroupRequest request)
        {
            string ret;
            try
            {
                var entity = _dbContext.Chatterers.Where(c => c.Group == request.Name).FirstOrDefault();
                if (entity == null)
                    ret = "not_fount";
                else
                {
                    if (entity.GroupPassword != request.Password)
                        ret = "password_incorrect";
                    else
                    {
                        var chatterer = _dbContext.Chatterers.Where(c => c.Name == _user.Name).Single();
                        chatterer.InGroup = request.Name;
                        chatterer.InGroupPassword = request.Password;
                        await _dbContext.SaveChangesAsync();
                        ret = "OK";
                    }
                }
            }
            catch(Exception e)
            {
                ret = e.Message;
            }
            return Content(ret, "text/plain");
        }
        [HttpPost("sign/out")]
        public async Task<ContentResult> SignOut([FromBody]GroupRequest request)
        {
            string ret;
            try
            {
                if (_user.InGroup == null)
                    ret = "not_signed";
                else
                {
                    var chatterer = _dbContext.Chatterers.Where(c => c.Name == _user.Name).Single();
                    chatterer.InGroup = chatterer.InGroupPassword = null;
                    await _dbContext.SaveChangesAsync();
                    ret = "OK";
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
