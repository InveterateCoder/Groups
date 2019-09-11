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
            await Clients.OthersInGroup(user.InGroup).SendAsync("signed_out", user.Name);
            await Groups.RemoveFromGroupAsync(user.ConnectionId, user.InGroup);
        }
        public async override Task OnConnectedAsync()
        {
            var user = GetUser(); //todo html encoding messages
            if (user.InGroup == null || user.ConnectionId != null) //todo (doesn't work) fix connection individuality
                Context.Abort();    //todo implement ip identification too
            else
            {
                user.ConnectionId = Context.ConnectionId;
                await user.SaveAsync();
                await Groups.AddToGroupAsync(Context.ConnectionId, user.InGroup);
                await Clients.OthersInGroup(user.InGroup).SendAsync("go_on", user.Name);
            }
        }
        public async override Task OnDisconnectedAsync(Exception exception)
        {
            //todo inform group about this peer's disconnection, check whether signed out or lost connection
            var user = GetUser();
            if (user.InGroup != null)
            {
                await Clients.OthersInGroup(user.InGroup).SendAsync("go_off", user.Name);
                await Groups.RemoveFromGroupAsync(user.ConnectionId, user.InGroup);
            }
            user.ConnectionId = null;
            await user.SaveAsync();
            await base.OnDisconnectedAsync(exception);
        }
    }
}