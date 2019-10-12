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
            user.Database = dbContext;
            string token = context.Request.Cookies[StaticData.AuthenticationCookieName];
            if (token != null)
            {
                var chatterer = dbContext.Chatterers.Where(c => c.Token == token).SingleOrDefault();
                if (chatterer == null)
                {
                    context.Response.Cookies.Delete(StaticData.AuthenticationCookieName);
                    await Filter(context);
                }
                else if (chatterer.ConnectionId != null && context.Request.Path.StartsWithSegments(PathString.FromUriComponent("/hub")))
                    context.Response.StatusCode = StatusCodes.Status409Conflict;
                else
                {
                    var dateNow = DateTime.UtcNow;
                    chatterer.LastActive = dateNow.Ticks;
                    if (chatterer.InGroupId != 0)
                    {
                        var group = await dbContext.Chatterers.FindAsync(chatterer.InGroupId);
                        if (group == null || chatterer.InGroupPassword != group.GroupPassword)
                        {
                            chatterer.InGroupId = 0;
                            chatterer.InGroupPassword = null;
                        }
                        else if(group.GroupLastCleaned < dateNow.Subtract(TimeSpan.FromDays(30)).Ticks)
                        {
                            var limit = dateNow.Subtract(TimeSpan.FromDays(15)).Ticks;
                            var disposableMsgs = dbContext.Messages.Where(m => m.GroupId == group.Id && m.SharpTime < limit);
                            if (disposableMsgs.Count() > 0)
                                dbContext.Messages.RemoveRange(disposableMsgs);
                            group.GroupLastCleaned = dateNow.Ticks;
                        }
                    }
                    user.Chatterer = chatterer;
                    await dbContext.SaveChangesAsync();
                    await Filter(context, true);
                }
            }
            else
                await Filter(context);
        }
        private async Task Filter(HttpContext context, bool signed = false)
        {
            if (!signed && context.Request.Path != "/" && !(context.Request.Path.StartsWithSegments(PathString.FromUriComponent("/api/account"), StringComparison.OrdinalIgnoreCase)))
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            else
                await _next(context);
        }
    }
}