using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sar.Database.Model;
using Sar.Database.Model.Members;
using Sar.Database.Model.Units;
using Sar.Database.Services;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Kcsara.Database.Api.Controllers.Units
{
  [Authorize]
  public class MembershipController : Controller
  {
    private readonly IUnitsService _units;
    private readonly Sar.Database.Services.IAuthorizationService _authz;

    public MembershipController(IUnitsService units, Sar.Database.Services.IAuthorizationService authz)
    {
      _units = units;
      _authz = authz;
    }
    
    [HttpGet("members/{memberId}/memberships")]
    public async Task<ListPermissionWrapper<UnitMembership>> ListForMember(Guid memberId, bool history = false)
    {
      await _authz.EnsureAsync(memberId, "Read:UnitMembership@MemberId");

      DateTimeOffset now = DateTimeOffset.UtcNow;

      Expression<Func<UnitMembership, bool>> predicate = history
        ? (Expression<Func<UnitMembership, bool>> )(f => f.Member.Id == memberId)
        : (f => f.Member.Id == memberId && f.IsActive && (f.End == null || f.End > now));

      return await _units.ListMemberships(predicate, _authz.CanCreateMembershipForMember(memberId));
    }

    [HttpGet("units/{unitId}/memberships")]
    public async Task<ListPermissionWrapper<UnitMembership>> ListForUnit(Guid unitId, bool history = false)
    {
      DateTimeOffset now = DateTimeOffset.UtcNow;

      await _authz.EnsureAsync(unitId, "Read:UnitMembership@UnitId");

      Expression<Func<UnitMembership, bool>> predicate = history
        ? (Expression<Func<UnitMembership, bool>>)(f => f.Unit.Id == unitId)
        : (f => f.Unit.Id == unitId && f.IsActive && (f.End == null || f.End > now));

      return await _units.ListMemberships(predicate, true);
    }

    [HttpGet("units/{unitId}/memberships/byStatus/{statusName}")]
    public async Task<ListPermissionWrapper<UnitMembership>> ListForUnit(Guid unitId, string statusName, bool history = false)
    {
      DateTimeOffset now = DateTimeOffset.UtcNow;

      await _authz.EnsureAsync(unitId, "Read:UnitMembership@UnitId");

      Expression<Func<UnitMembership, bool>> predicate = history
        ? (Expression<Func<UnitMembership, bool>>)(f => f.Unit.Id == unitId && f.Status == statusName)
        : (f => f.Unit.Id == unitId && f.IsActive && (f.End == null || f.End > now) && f.Status == statusName);

      return await _units.ListMemberships(predicate, true);
    }

    [HttpPost("members/{memberId}/memberships")]
    public async Task<UnitMembership> CreateForMember(Guid memberId, [FromBody] UnitMembership membership)
    {
      if (membership.Unit == null) throw new ArgumentException("unit is required");
      await _authz.AuthorizeAsync(membership.Unit.Id, "Create:UnitMembership@UnitId");
      await _authz.AuthorizeAsync(memberId, "Create:UnitMembership@MemberId");

      if (membership.Member == null) membership.Member = new MemberSummary();
      membership.Member.Id = memberId;

      return await _units.CreateMembership(membership);
    }
  }
}
