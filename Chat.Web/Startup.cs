using Chat.Web.Hubs;
using Chat.Web.Infrastructure;
using Chat.Web.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

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
            services.AddDbContext<ChatterersDb>(opts =>
            {
                opts.UseSqlServer(Configuration.GetConnectionString("Users"));
            });
            services.AddSingleton<ActiveUsers>();
            services.AddScoped<User>();
            services.AddCors(opts =>
            {
                opts.AddPolicy("_allowAll", blder =>
                 {
                     blder.AllowAnyOrigin();
                     blder.AllowAnyHeader();
                     blder.AllowAnyMethod();
                 });
            });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSignalR();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, User user)
        {
            StaticData.RootPath = env.ContentRootPath;
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            app.UseCors("_allowAll");
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseMiddleware<CustomAuthenticationMiddleware>();
            app.UseSignalR(routes =>
            {
                routes.MapHub<ChatHub>("/hub");
            });
            app.UseMvc();
        }
    }
}