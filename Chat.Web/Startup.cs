using Chat.Web.Hubs;
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
        public static string RootPath { get; set; }
        private IConfiguration Configuration;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ChatterersDbContext>(opts =>
            {
                opts.UseSqlServer(Configuration.GetConnectionString("Users"));
            });
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
            RootPath = env.ContentRootPath;
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
            app.UseSignalR(routes =>
            {
                routes.MapHub<ChatHub>("/hub");
            });
            app.UseMvc();
        }
    }
}