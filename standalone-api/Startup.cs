using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Kcsar.Database.Model;
using log4net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Sar;
using Sar.Database.Api.Extensions;
using Sar.Database.Services;
using Sar.Database.Services.Auth;

namespace Kcsara.Database.Api
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

      services.AddSingleton<IAnimalsService, AnimalsService>();
      services.AddSingleton<ITrainingRecordsService, TrainingRecordsService>();
      services.AddSingleton<ITrainingCoursesService, TrainingCoursesService>();
      services.AddSingleton<IMembersService, MembersService>();
      services.AddSingleton<IEventsService, EventsService>();

      CoreExtensionsProvider ext = new CoreExtensionsProvider();
      services.AddSingleton<IExtensionProvider>(ext);
      services.AddSingleton<IRolesService, RolesService>();
      services.AddSingleton<IAuthorizationService, AuthorizationService>();
      services.AddSingleton<IUnitsService, UnitsService>();
      services.AddSingleton<Func<IKcsarContext>>(() => new KcsarContext(Configuration["store:connectionString"]));
      services.AddSingleton(s => log4net.LogManager.GetLogger("some log"));
      services.AddSingleton<IHost, CoreHost>();

      string blobConnectionString = Configuration["store:blob:connectionString"];
      string blobContainer = Configuration["store:blob:container"];
      if (!string.IsNullOrWhiteSpace(blobConnectionString) && !string.IsNullOrWhiteSpace(blobContainer))
      {
        services.AddSingleton<IBlobStorage>(new AzureBlobStorage(blobConnectionString, blobContainer));
      }

      JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
      services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options =>
      {
        options.Authority = "https://login.kingcountysar.org";
        options.Audience = "https://login.kingcountysar.org/resources";
      });

      services.AddMvc()
        .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
        .AddJsonOptions(json =>
        {
          json.SerializerSettings.Converters.Add(new StringEnumConverter());
          json.SerializerSettings.Converters.Add(new ItemPermissionConverter());
          json.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      app.UseCors(policy =>
      {
        policy.WithOrigins(new[] { "http://localhost:4944" }).AllowAnyMethod().AllowCredentials().AllowAnyHeader();
      });

      app.UseAuthentication();

      app.UseMvc();
    }
  }
}
