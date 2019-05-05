using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using server.Models;
using server.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IO;
using Microsoft.DotNet.PlatformAbstractions;

namespace server
{
  public class Startup
  {
    public Startup(IConfiguration configuration, IHostingEnvironment env)
    {
      Configuration = configuration;
      _env = env;
    }

    public IConfiguration Configuration { get; }
    private IHostingEnvironment _env;

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddDbContext<Context>(options => {
        if (_env.IsDevelopment())
        {
          options.UseSqlServer(Configuration.GetConnectionString("DispenserDatabase"));
        }
        else
        {
          options.UseSqlServer(Configuration["ConnectionStringProd"]);
        }
      });
   
      services.AddTransient<DbService>();
      
      services.AddCors(options => {
        options.AddDefaultPolicy(builder => {
          builder.WithOrigins(
            "http://localhost:80",
            "http://localhost:4200"
          ).AllowCredentials();
          builder.AllowAnyMethod();
          builder.AllowAnyHeader();
        });
      });

      services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie();

      services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

      string appRoot = AppContext.BaseDirectory;
      AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(appRoot, "App_Data"));
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, Context context)
    {
      // Setup database if not created
      context.Database.Migrate();

      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
          // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }

      app.UseHttpsRedirection();
      app.UseDefaultFiles();
      app.UseStaticFiles();
      app.UseCors();
      app.UseAuthentication();
      app.UseMvc(routes => {
        routes.MapRoute("default", "api/{controller}/{action}");
      });
    }
  }
}
