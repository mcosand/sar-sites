using Kcsara.Database.Api.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sar;
using Sar.Database.Model;
using Sar.Database.Model.Members;
using Sar.Database.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Kcsara.Database.Api.Controllers
{
  [Authorize]
  public class MembersController : Controller
  {
    private readonly IMembersService _members;
    private readonly IConfiguration config;
    private readonly Sar.Database.Services.IAuthorizationService _authz;
    private readonly ILogger<MembersController> log;

    public MembersController(IMembersService members, IConfiguration config, Sar.Database.Services.IAuthorizationService authz, ILogger<MembersController> log)
    {
      _members = members;
      this.config = config;
      _authz = authz;
      this.log = log;
    }

    [HttpGet("members/{id}")]
    public async Task<MemberInfo> Get(Guid id)
    {
      await _authz.EnsureAsync(id, "Read:Member");

      return await _members.GetMember(id);
    }

    [HttpGet("members/{memberId}/photo")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    public async Task<object> MemberPhoto(Guid memberId, [FromServices] IBlobStorage blobs, [FromServices] IHost host)
    {
      await _authz.EnsureAsync(memberId, "Read:Member");
      var member = await _members.GetMember(memberId);
      object result = null;
      if (!string.IsNullOrWhiteSpace(member?.Photo))
      {
        string filename = "members/" + member.Photo;
        try
        {
          using (var ms = new MemoryStream())
          {
            DateTime now = DateTime.UtcNow;
            await blobs.Download(filename, ms);
            log.LogDebug($"Downloaded from Azure in {(DateTime.UtcNow - now).TotalMilliseconds}ms");
            now = DateTime.UtcNow;
            result = StreamToData(ms);
            log.LogDebug($"Encoded to base64 in {(DateTime.UtcNow - now).TotalMilliseconds}ms");
            return result;
          }
        }
        catch (Exception e)
        {
          log.LogWarning($"Couldn't download {filename}: ", e);
        }
      }

      using (var stream = host.OpenFile("nophoto.jpg"))
      {
        using (var ms = new MemoryStream())
        {
          await stream.CopyToAsync(ms);
          return StreamToData(ms);
        }
      }
    }

    private object StreamToData(MemoryStream ms)
    {
      return new { Data = "data:image/jpeg;base64," + Convert.ToBase64String(ms.ToArray()) };
    }

    [HttpGet("members/{memberId}/emergencycontacts/count")]
    public async Task<object> EmergencyContactStatus(Guid memberId)
    {
      await _authz.EnsureAsync(memberId, "Read:Member");

      return new { Count = await _members.GetEmergencyContactCountAsync(memberId) };
    }

    [HttpPost("members")]
    public async Task<MemberInfo> Provision(MemberInfo body)
    {
      if (!User.Claims.Any(f => f.Type == "scope" && f.Value == "db-w-members"))
      {
        throw new ForbiddenException();
      }

      // Create the member
      var member = await _members.CreateMember(body);

      return await Get(member.Id);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("Members/ByWorkerNumber/{id}")]
    public async Task<IEnumerable<MemberSummary>> ByWorkerNumber(string id)
    {
      await _authz.EnsureAsync(null, "Read:Member");

      return await _members.ByWorkerNumber(id);
    }

    [HttpGet]
    [Route("Members/ByPhoneNumber/{id}")]
    public async Task<IEnumerable<MemberSummary>> ByPhoneNumber(string id)
    {
      await _authz.EnsureAsync(null, "Read:Member");

      return await _members.ByPhoneNumber(id);
    }

    [HttpGet]
    [Route("Members/ByEmail/{id}")]
    public async Task<IEnumerable<MemberSummary>> ByEmail(string id)
    {
      await _authz.EnsureAsync(null, "Read:Member");

      return await _members.ByEmail(id);
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("auth-support/byemail/{id}")]
    public async Task<IEnumerable<ApiAuthMember>> ByEmailForAuth(string id)
    {
      VerifyKey();
      return (await _members.GetAuthSiteMembers(m => m.PrimaryEmail == id)).Select(ToAuthMember);
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("auth-support/byphone/{id}")]
    public async Task<IEnumerable<ApiAuthMember>> ByPhoneForAuth(string id)
    {
      VerifyKey();

      return (await _members.GetAuthSiteMembers(m => m.PrimaryPhone == id)).Select(ToAuthMember);
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("auth-support/{id}")]
    public async Task<ApiAuthMember> GetMemberForAuth(Guid id)
    {
      VerifyKey();
      return (await _members.GetAuthSiteMembers(m => m.Id == id)).Select(ToAuthMember).SingleOrDefault();
    }

    private ApiAuthMember ToAuthMember(AuthSiteMember serviceModel)
    {
      return new ApiAuthMember
      {
        Id = serviceModel.Id,
        Firstname = serviceModel.Firstname,
        Lastname = serviceModel.Lastname,
        PrimaryEmail = serviceModel.PrimaryEmail,
        PrimaryPhone = serviceModel.PrimaryPhone,
        Units = serviceModel.Units.Select(f => new NameIdPair { Id = f.Id, Name = f.Name }).ToList()
      };
    }

    private void VerifyKey()
    {
      var value = Request.Headers.Where(f => f.Key == "X-Auth-Service-Key").Select(f => f.Value).FirstOrDefault().FirstOrDefault();
      if (!string.IsNullOrWhiteSpace(value) && value != config["auth-site-key"]) throw new ForbiddenException();
    }

    public class ApiAuthMember
    {
      public Guid Id { get; set; }
      public string Firstname { get; set; }
      public string Lastname { get; set; }
      public string PrimaryEmail { get; set; }
      public string PrimaryPhone { get; set; }
      public List<NameIdPair> Units { get; set; }
    }
  }
}
