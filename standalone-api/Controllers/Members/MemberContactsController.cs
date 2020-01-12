using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sar.Database.Model;
using Sar.Database.Model.Members;
using Sar.Database.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kcsara.Database.Api.Controllers.Members
{
  [Authorize]
  public class MemberContactsController : Controller
  {
    private readonly IMembersService _members;
    private readonly Sar.Database.Services.IAuthorizationService _authz;

    public MemberContactsController(IMembersService members, Sar.Database.Services.IAuthorizationService authz)
    {
      _members = members;
      _authz = authz;
    }

    [HttpGet("members/{memberId}/contacts")]
    public async Task<IEnumerable<PersonContact>> ListContacts(Guid memberId)
    {
      await _authz.EnsureAsync(memberId, "Read:Member");

      return await _members.ListMemberContactsAsync(memberId);
    }

    [HttpPost("members/{memberId}/contacts")]
    public async Task<PersonContact> CreateContact(Guid memberId, [FromBody] PersonContact contact)
    {
      await _authz.EnsureAsync(memberId, "Create:MemberContact@MemberId");
      return await _members.AddContact(memberId, contact);
    }

    [HttpGet("members/{memberId}/addresses")]
    public async Task<IEnumerable<MemberAddress>> ListAddresses(Guid memberId)
    {
      await _authz.EnsureAsync(memberId, "Read:Member");

      return await _members.ListMemberAddressesAsync(memberId);
    }

  }
}