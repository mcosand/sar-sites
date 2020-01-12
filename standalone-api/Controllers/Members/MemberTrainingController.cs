using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sar.Database.Model;
using Sar.Database.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kcsara.Database.Api.Controllers.Members
{
  [Authorize]
  public class MembersTrainingController : Controller
  {
    private readonly IMembersService _members;
    private readonly Sar.Database.Services.IAuthorizationService _authz;

    public MembersTrainingController(IMembersService members, Sar.Database.Services.IAuthorizationService authz)
    {
      _members = members;
      _authz = authz;
    }

    [HttpGet("members/{memberId}/trainings/stats")]
    public async Task<AttendanceStatistics<NameIdPair>> GetMissionStatistics(Guid memberId)
    {
      await _authz.EnsureAsync(memberId, "Read:Member");

      return await _members.GetTrainingStatistics(memberId);
    }

    [HttpGet("members/{memberId}/trainings")]
    public async Task<List<EventAttendance>> ListMissions(Guid memberId)
    {
      await _authz.EnsureAsync(memberId, "Read:Member");
      await _authz.EnsureAsync(null, "Read:Training");

      return await _members.GetTrainingList(memberId);
    }
  }
}
