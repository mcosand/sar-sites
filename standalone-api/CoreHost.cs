using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Sar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Kcsara.Database.Api
{
  public class CoreHost : IHost
  {
    private readonly IConfiguration config;
    private readonly IHttpContextAccessor getContext;

    public CoreHost(IConfiguration config, IHttpContextAccessor getContext)
    {
      this.config = config;
      this.getContext = getContext;
    }

    public ClaimsPrincipal User => getContext.HttpContext.User;

    public bool FileExists(string relativePath)
    {
      throw new NotImplementedException();
    }

    public string GetConfig(string key)
    {
      return config[key];
    }

    public Stream OpenFile(string relativePath)
    {
      throw new NotImplementedException();
    }
  }
}
