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
    public class GroupController : ControllerBase
    {
        private ChatterersDb _dbContext;
        private User _user;
        public GroupController(ChatterersDb dbContext, User user)
        {
            _dbContext = dbContext;
            _user = user;
        }
        [HttpGet("list/{start:int:min(0)}/{quantity:int:range(1,100)?}")]
        public JsonResult Groups(int start, int quantity)
        {
            try
            {
                var groups = _dbContext.Chatterers.Select(c => c.Group).Where(g => g != null);
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
        [HttpGet("find/{query:maxlength(64)}/{max:int:range(1,100)}")]
        public JsonResult Find(string query, int max)
        {
            try
            {
                return new JsonResult(_dbContext.Chatterers.Select(c => c.Group).Where(g => g.StartsWith(query,
                    StringComparison.OrdinalIgnoreCase)).OrderBy(s => s, StringComparer.OrdinalIgnoreCase).Take(max));
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
        public async Task<ContentResult> Register([FromBody]GroupRequest request)
        {
            string ret;
            try
            {
                var groups = _dbContext.Chatterers.Select(c => c.Group).Where(g => g != null);
                if (groups.Any(g => g == request.Name))
                    ret = "name_taken";
                else
                {
                    var chatterer = _dbContext.Chatterers.Where(c => c.Name == _user.Name).Single();
                    chatterer.Group = request.Name;
                    chatterer.GroupPassword = request.Password;
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
