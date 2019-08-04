using Chat.Web.Models;
using Microsoft.AspNetCore.Http;
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
            string token = context.Request.Cookies[StaticData.AuthenticationCookieName];
            if (string.IsNullOrEmpty(token))
                UserInit(user);
            else
            {
                var chatterer = dbContext.Chatterers.Where(c => c.Token == token).FirstOrDefault();
                if(chatterer == null)
                {
                    UserInit(user);
                    context.Response.Cookies.Delete(StaticData.AuthenticationCookieName);
                }
                else
                {
                    if (!string.IsNullOrEmpty(chatterer.InGroup))
                    {
                        if (!(chatterer.InGroupPassword == dbContext.Chatterers.Where(c => c.Group == chatterer.InGroup).FirstOrDefault()?.GroupPassword))
                        {
                            chatterer.InGroup = null;
                            chatterer.InGroupPassword = null;
                            await dbContext.SaveChangesAsync();
                        }
                    }
                    UserInit(user, chatterer.Name, chatterer.Group, chatterer.InGroup, token);
                }
            }
            if (((context.Request.Path.StartsWithSegments(PathString.FromUriComponent("/hub"), StringComparison.OrdinalIgnoreCase)
                || context.Request.Path.StartsWithSegments(PathString.FromUriComponent("/dummy"), StringComparison.OrdinalIgnoreCase))) //include /api/group
                && user.Name == null)
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            else
                await _next(context);
        }
        public void UserInit(User user, string name = null, string group = null, string inGroup = null, string token = null)
        {
            user.Name = name;
            user.Group = group;
            user.InGroup = inGroup;
            user.Token = token;
        }
    }
}
