using Chat.Web.Hubs;
using Chat.Web.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chat.Web
{
    public class Startup
    {
        private IConfiguration Configuration;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<GroupsDbContext>(opts => opts.UseMySql(Configuration.GetConnectionString("Users")));
            services.AddIdentity<Chatterer, IdentityRole>(opts =>
            {
                opts.User.AllowedUserNameCharacters = string.Empty;
                opts.User.RequireUniqueEmail = true;
                opts.Password.RequiredLength = 8;
                opts.Password.RequireUppercase = false;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequireDigit = false;
                opts.Password.RequireLowercase = false;
                opts.Password.RequiredUniqueChars = 0;
            }).AddEntityFrameworkStores<GroupsDbContext>();
            services.AddResponseCaching();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSignalR().AddAzureSignalR("Endpoint=https://groups.service.signalr.net;AccessKey=bTamVY4Zx5TCNIa6TvpKtwx6L1BlpJy/yfr+GTKdALI=;Version=1.0;");
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            StaticData.RootPath = env.ContentRootPath;
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseHsts();
            app.UseHttpsRedirection();
            app.UseFileServer();
            app.UseAuthentication();
            app.UseAzureSignalR(routes =>
            {
                routes.MapHub<ChatHub>("/hub");
            });
            app.UseResponseCaching();
            app.UseMvc();
        }
    }
}