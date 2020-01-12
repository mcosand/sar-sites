using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sar;
using Sar.Database.Model;
using Sar.Database.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kcsara.Database.Api.Controllers.Members
{
  [Authorize]
  public class MembersMissionsController : Controller
  {
    private readonly IMembersService _members;
    private readonly Sar.Database.Services.IAuthorizationService _authz;
    private readonly IHost _host;

    public MembersMissionsController(IMembersService members, Sar.Database.Services.IAuthorizationService authz, IHost host)
    {
      _members = members;
      _authz = authz;
      _host = host;
    }

    [HttpGet("members/{memberId}/missions/stats")]
    public async Task<AttendanceStatistics<NameIdPair>> GetMissionStatistics(Guid memberId)
    {
      await _authz.EnsureAsync(memberId, "Read:Member");

      return await _members.GetMissionStatistics(memberId);
    }

    [HttpGet("members/{memberId}/missions")]
    public async Task<List<EventAttendance>> ListMissions(Guid memberId)
    {
      await _authz.EnsureAsync(memberId, "Read:Member");
      await _authz.EnsureAsync(null, "Read:Mission");

      return await _members.GetMissionList(memberId);
    }
  }
}
