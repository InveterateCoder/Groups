using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Chat.Web.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Chat.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PushController : ControllerBase
    {
        private User _user;
        public PushController(User user) => _user = user;
        [HttpPost("web/subscribe")]
        public async Task<ContentResult> WebSubscribe([Required, FromBody]Subscription subscription)
        {
            string ret;
            try
            {
                _user.WebSubscription = subscription.ToString();
                await _user.SaveAsync();
                ret = "OK";
            }
            catch(Exception e)
            {
                ret = e.Message;
            }
            return Content(ret, "text/plain");
        }
        [HttpGet("web/unsubscribe")]
        public async Task<ContentResult> WebUnsubscribe()
        {
            string ret;
            try
            {
                _user.WebSubscription = null;
                await _user.SaveAsync();
                ret = "OK";
            }
            catch (Exception e)
            {
                ret = e.Message;
            }
            return Content(ret, "text/plain");
        }
        [HttpGet("web/push")]
        public async Task<ContentResult> Push()
        {
            try
            {

            }
            catch(Exception e)
            {

            }
            return Content("ol");
        }
    }
}
