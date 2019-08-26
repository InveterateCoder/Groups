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
                    if (StaticData.ActiveUsers.Any(kPair => kPair.Value.Name == chatterer.Name))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    }
                    else
                    {
                        if (chatterer.InGroup != null && chatterer.InGroupPassword != dbContext.Chatterers.Where(c => c.Group == chatterer.InGroup).SingleOrDefault()?.GroupPassword)
                        {
                            chatterer.InGroup = null;
                            chatterer.InGroupPassword = null;
                            await dbContext.SaveChangesAsync();
                        }
                        user.Chatterer = chatterer;
                        await Filter(context, true);
                    }
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
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
            else
                await _next(context);
        }
    }
}