using Chat.Web.Hubs;
using Chat.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Chat.Web.Infrastructure
{
    public class CustomAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        public CustomAuthenticationMiddleware(RequestDelegate next) => _next = next;
        public async Task InvokeAsync(HttpContext context, User user, ChatterersDb dbContext)
        {
            user.Database = dbContext;
            string token = context.Request.Cookies[StaticData.AuthenticationCookieName];
            if (token != null)
            {
                var chatterer = dbContext.Chatterers.Where(c => c.Token == token).SingleOrDefault();
                if(chatterer == null)
                {
                    context.Response.Cookies.Delete(StaticData.AuthenticationCookieName);
                    await Filter(context);
                }
                else
                {
                    //todo make possible to sign in from another application - switch tokens
                    //todo clean old (15 days) group messages
                    chatterer.LastActive = DateTime.UtcNow.Ticks;
                    if (chatterer.InGroupId != 0)
                    {
                        var group = await dbContext.Chatterers.FindAsync(chatterer.InGroupId);
                        if (group == null || chatterer.InGroupPassword != group.GroupPassword)
                        {
                            chatterer.InGroupId = 0;
                            chatterer.InGroupPassword = null;
                        }
                    }   
                    await dbContext.SaveChangesAsync();
                    user.Chatterer = chatterer;
                    await Filter(context, true);
                }
            }
            else
                await Filter(context);
        }
        private async Task Filter(HttpContext context, bool signed = false)
        {
            if (!signed && (context.Request.Path.StartsWithSegments(PathString.FromUriComponent("/hub"), StringComparison.OrdinalIgnoreCase)
                || context.Request.Path.StartsWithSegments(PathString.FromUriComponent("/api/groups"), StringComparison.OrdinalIgnoreCase)
                || context.Request.Path.StartsWithSegments(PathString.FromUriComponent("/api/account/user"), StringComparison.OrdinalIgnoreCase)))
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            else
                await _next(context);
        }
    }
}