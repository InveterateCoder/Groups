using Chat.Web.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Chat.Web.Hubs
{
    public class MessageFromClient
    {
        public string[] To { get; set; }
        public string Text { get; set; }
    }
    public class MessageToClient
    {
        public long SharpTime { get; set; }
        public long JsTime { get; set; }
        public string StringTime { get; set; }
        public string From { get; set; }
        public string[] Peers { get; set; }
        public string Text { get; set; }
    }
    public class Time
    {
        public long SharpTime { get; set; }
        public long JsTime { get; set; }
        public string StringTime { get; set; }
    }
    public class ChatHub : Hub
    {
        private User GetUser()
        {
            User user = Context.GetHttpContext().RequestServices.GetService(typeof(User)) as User;
            if (user == null)
            {
                Context.Abort();
                throw new HubException("User was not identified");
            }
            return user;
        }
        public async Task SignOut()
        {
            var user = GetUser();
            await Clients.OthersInGroup(user.InGroupId.ToString()).SendAsync("signed_out", user.Name);
            await Groups.RemoveFromGroupAsync(user.ConnectionId, user.InGroupId.ToString());
        }
        public async Task<Time> MessageServer(MessageFromClient msg)
        {
            var user = GetUser();
            if (string.IsNullOrEmpty(msg.Text) || msg.Text.Length > 10000)
                throw new HubException("Message cannot be empty or exceed 10000 characters.");
            Time time = new Time();
            var timeNow = DateTime.UtcNow;
            if (user.Chatterer.LastActive > timeNow.Subtract(TimeSpan.FromSeconds(1.5)).Ticks)
                throw new HubException("You're messaging too fast.");
            time.SharpTime = timeNow.Ticks;
            user.Chatterer.LastActive = time.SharpTime;
            time.JsTime = StaticData.TicksToJsMs(time.SharpTime);
            time.StringTime = time.SharpTime.ToString();
            if (msg.To == null)
            {
                ChatterersDb.Chatterer group;
                if (user.InGroupId == user.Chatterer.Id)
                    group = user.Chatterer;
                else
                    group = await user.Chatterers.FindAsync(user.InGroupId);
                if (group == null)
                    throw new HubException("Internal error, try again later");
                else
                {
                    var retMsg = new MessageToClient
                    {
                        SharpTime = time.SharpTime,
                        JsTime = time.JsTime,
                        StringTime = time.StringTime,
                        From = user.Name,
                        Peers = null,
                        Text = msg.Text
                    };
                    await Clients.OthersInGroup(user.InGroupId.ToString()).SendAsync("message_client", retMsg);
                    await user.Database.Messages.AddAsync(new ChatterersDb.Message
                    {
                        SharpTime = time.SharpTime,
                        JsTime = time.JsTime,
                        StringTime = time.StringTime,
                        From = user.Name,
                        Text = msg.Text,
                        GroupId = group.Id
                    });
                }
            }
            else
            {
                if (msg.To.Length > 50)
                    throw new HubException("You can't select more than 50 peers");
                foreach (var p in msg.To)
                    if (p.Length > 64)
                        throw new HubException("Corrupted data detected");
                var retMsg = new MessageToClient
                {
                    SharpTime = time.SharpTime,
                    JsTime = time.JsTime,
                    StringTime = time.StringTime,
                    From = user.Name,
                    Peers = msg.To,
                    Text = msg.Text
                };
                await Clients.Clients(user.Chatterers.Where(c => msg.To.Contains(c.Name)).Select(c => c.ConnectionId).ToArray()).SendAsync("message_client", retMsg);
            }
            await user.SaveAsync();
            return time;
        }
        public async override Task OnConnectedAsync()
        {
            var user = GetUser();
            user.ConnectionId = Context.ConnectionId;
            user.Chatterer.LastNotified = 0;
            user.Chatterer.LastActive = DateTime.UtcNow.Ticks;
            await user.SaveAsync();
            await Groups.AddToGroupAsync(Context.ConnectionId, user.InGroupId.ToString());
            await Clients.OthersInGroup(user.InGroupId.ToString()).SendAsync("go_on", user.Name);
        }
        public async override Task OnDisconnectedAsync(Exception exception)
        {
            var user = GetUser();
            if (user.InGroupId != 0)
            {
                await Clients.OthersInGroup(user.InGroupId.ToString()).SendAsync("go_off", user.Name);
                await Groups.RemoveFromGroupAsync(user.ConnectionId, user.InGroupId.ToString());
            }
            user.ConnectionId = null;
            user.Chatterer.LastActive = DateTime.UtcNow.Ticks;
            await user.SaveAsync();
            await base.OnDisconnectedAsync(exception);
        }
    }
}