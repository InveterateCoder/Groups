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
        public async Task InvokeAsync(HttpContext context, User user, ChatterersDb dbContext, ActiveUsers activeUsers)
        {
            string token = context.Request.Cookies[StaticData.AuthenticationCookieName];
            if (token != null)
            {
                var chatterer = dbContext.Chatterers.Where(c => c.Token == token).SingleOrDefault();
                if(chatterer == null)
                    context.Response.Cookies.Delete(StaticData.AuthenticationCookieName);
                else
                {
                    if(activeUsers.Users.Any(u=>u.Name == chatterer.Name))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        return;
                    }
                    if (chatterer.InGroup != null)
                    {
                        if (!(chatterer.InGroupPassword == dbContext.Chatterers.Where(c => c.Group == chatterer.InGroup).SingleOrDefault()?.GroupPassword))
                        {
                            chatterer.InGroup = null;
                            chatterer.InGroupPassword = null;
                            await dbContext.SaveChangesAsync();
                        }
                    }
                    user.Name = chatterer.Name;
                    user.Group = chatterer.Group;
                    user.InGroup = chatterer.InGroup;
                    user.Token = token;
                }
            }
            if ((context.Request.Path.StartsWithSegments(PathString.FromUriComponent("/hub"), StringComparison.OrdinalIgnoreCase)
                || context.Request.Path.StartsWithSegments(PathString.FromUriComponent("/api/groups"), StringComparison.OrdinalIgnoreCase)
                || context.Request.Path.StartsWithSegments(PathString.FromUriComponent("/api/account/adm"), StringComparison.OrdinalIgnoreCase))
                && user.Name == null)
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            else
                await _next(context);
        }
    }
}
