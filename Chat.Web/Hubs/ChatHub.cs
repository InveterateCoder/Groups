using Chat.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public Time Time { get; set; }
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
            if (string.IsNullOrEmpty(msg.Text) || msg.Text.Length > 2048)
                throw new HubException("Message cannot be empty or exceed 2048 characters");
            Time time = new Time();
            time.SharpTime = DateTime.UtcNow.Ticks;
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
                        Time = time,
                        From = user.Name,
                        Peers = null,
                        Text = msg.Text
                    };
                    Task sendTask = Clients.OthersInGroup(user.InGroupId.ToString()).SendAsync("message_client", retMsg);
                    await user.Database.Messages.AddAsync(new ChatterersDb.Message
                    {
                        SharpTime = time.SharpTime,
                        JsTime = time.JsTime,
                        StringTime = time.StringTime,
                        From = user.Name,
                        Text = msg.Text,
                        GroupId = group.Id
                    });
                    Task saveTask = user.SaveAsync();
                    await Task.WhenAll(sendTask, saveTask);
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
                    Time = time,
                    From = user.Name,
                    Peers = msg.To,
                    Text = msg.Text
                };
                await Clients.Clients(user.Chatterers.Where(c => msg.To.Contains(c.Name)).Select(c => c.ConnectionId).ToArray()).SendAsync("message_client", retMsg);
            }
            return time;
        }
        public async override Task OnConnectedAsync()
        {
            var user = GetUser(); //todo html encoding messages
            if (user.InGroupId == 0 || user.ConnectionId != null) //todo (doesn't work) fix connection individuality
                Context.Abort();    //todo implement ip identification too
            else
            {
                user.ConnectionId = Context.ConnectionId;
                await user.SaveAsync();
                await Groups.AddToGroupAsync(Context.ConnectionId, user.InGroupId.ToString());
                await Clients.OthersInGroup(user.InGroupId.ToString()).SendAsync("go_on", user.Name);
            }
        }
        //todo need to html encode all traffic
        public async override Task OnDisconnectedAsync(Exception exception)
        {
            //todo inform group about this peer's disconnection, check whether signed out or lost connection
            var user = GetUser();
            if (user.InGroupId != 0)
            {
                await Clients.OthersInGroup(user.InGroupId.ToString()).SendAsync("go_off", user.Name);
                await Groups.RemoveFromGroupAsync(user.ConnectionId, user.InGroupId.ToString());
            }
            user.ConnectionId = null;
            await user.SaveAsync();
            await base.OnDisconnectedAsync(exception);
        }
    }
}