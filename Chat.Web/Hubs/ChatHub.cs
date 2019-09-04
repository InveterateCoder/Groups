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
                throw new HubException("User wasn not identified");
            }
            return user;
        }
        public async Task Test(string message)
        {
            var user = GetUser();
            await Clients.Group(user.InGroup).SendAsync("test", Context.ConnectionId);
        }
        public async override Task OnConnectedAsync()
        {
            var user = GetUser();
            if (user.InGroup == null || user.ConnectionId != null) //todo (doesn't work) fix connection individuality
                Context.Abort();
            else
            {
                user.ConnectionId = Context.ConnectionId;
                await user.SaveAsync();
                await Groups.AddToGroupAsync(Context.ConnectionId, user.InGroup);
            }
        }
        public async override Task OnDisconnectedAsync(Exception exception)
        {
            var user = GetUser();
            user.ConnectionId = null;
            await user.SaveAsync();
            await base.OnDisconnectedAsync(exception);
        }
    }
}