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
        public async Task Test(string message)
        {
            await Clients.Group(StaticData.ActiveUsers[Context.ConnectionId].ActiveGroup).SendAsync("test", StaticData.ActiveUsers.Count());
        }
        public async override Task OnConnectedAsync()
        {
            User user = Context.GetHttpContext().RequestServices.GetService(typeof(User)) as User;
            if (user.InGroup == null || !StaticData.ActiveUsers.TryAdd(Context.ConnectionId, new ActiveUser() { Name = user.Name, ActiveGroup = user.InGroup }))
                Context.Abort();
            await Groups.AddToGroupAsync(Context.ConnectionId, user.InGroup);
        }
        public async override Task OnDisconnectedAsync(Exception exception)
        {
            for (int i = 300; i > 0 && !StaticData.ActiveUsers.TryRemove(Context.ConnectionId, out _); i--) ;
            await base.OnDisconnectedAsync(exception);
        }
    }
}