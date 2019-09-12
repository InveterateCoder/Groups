using Chat.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
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
        public long Time { get; set; }
        public string From { get; set; }
        public string[] Peers { get; set; }
        public string Text { get; set; }
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
        public async Task MessageServer(MessageFromClient msg)
        {
            var ticksNow = DateTime.UtcNow.Ticks;
            var user = GetUser();
            if (string.IsNullOrEmpty(msg.Text))
                throw new HubException("Message cannot be empty");
            else
            {
                if(msg.To == null)
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
                            Time = StaticData.TicksToJsMs(ticksNow),
                            From = user.Name,
                            Peers = null,
                            Text = msg.Text
                        };
                        Task sendTask = Clients.OthersInGroup(user.InGroupId.ToString()).SendAsync("message_client", retMsg);
                        await user.Database.Messages.AddAsync(new ChatterersDb.Message
                        {
                            Date = ticksNow,
                            From = user.Name,
                            Text = msg.Text,
                            GroupId = group.Id
                        });
                        Task saveTask =  user.SaveAsync();
                        await Task.WhenAll(sendTask, saveTask);
                    }
                }
                else
                {

                }
            }
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