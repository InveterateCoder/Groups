using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Chat.Web.Models;
using Microsoft.AspNetCore.Mvc;

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
        [HttpPost("web/push")]
        public async Task<ContentResult> Push([Required, FromBody] string name)
        {
            string ret;
            try
            {
                if (_user.InGroupId == 0)
                    ret = "not_in_group";
                else
                {
                    if (_user.WebSubscription == null)
                        ret = "s_not_subscribed";
                    else
                    {
                        var rcvr = _user.Chatterers.Where(c => c.InGroupId == _user.InGroupId).SingleOrDefault(c => c.Name == name);
                        if (rcvr == null)
                            ret = "not_found";
                        else
                        {
                            if (rcvr.InGroupId != _user.InGroupId)
                                ret = "not_same_group";
                            else
                            {
                                var web_subscription = StaticData.GetPushSubscription(rcvr.WebSubscription);
                                if (web_subscription == null)
                                    ret = "r_not_subscribed";
                                else
                                {
                                    if (rcvr.LastNotified > DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)).Ticks)
                                        ret = "is_notified_hour";
                                    else
                                    {
                                        if (rcvr.ConnectionId != null)
                                            ret = "usr_active";
                                        else
                                        {
                                            Task<ChatterersDb.Chatterer> groupN = _user.Chatterers.FindAsync(rcvr.InGroupId);
                                            WebPush.WebPushClient client = new WebPush.WebPushClient();
                                            WebPush.VapidDetails det = new WebPush.VapidDetails("mailto:splendiferouslife@outlook.com",
                                                "BFnbEjZPGFowzLKbDeFjlJ-o5juCQWiaFUzDH6jb_H3Rid3EO8f59N8PSe5AAMp5KhLMV31u1V79RxBiAmeofH0",
                                                "llDpC8IqKbdgsHqaF00xrcqHVefNt50NAMDQBBQrgNo");
                                            client.SetVapidDetails(det);
                                            var groupName = (await groupN).Group;
                                            var send_task = client.SendNotificationAsync(web_subscription, $"\"{groupName}\" by {_user.Name}");
                                            rcvr.LastNotified = DateTime.UtcNow.Ticks;
                                            var save_task = _user.SaveAsync();
                                            await Task.WhenAll(send_task, save_task);
                                            ret = "OK";
                                        }
                                    }
                                }
                            }
                        }
                    }
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
