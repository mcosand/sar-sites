using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sar.Database.Model;
using Sar.Database.Model.Search;
using Sar.Database.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kcsara.Database.Api.Controllers
{
  [Authorize]
  public class SearchController : Controller
  {
    private readonly IEventsService _events;
    private readonly IMembersService _members;

    public SearchController(IEventsService events, IMembersService members, Sar.Database.Services.IAuthorizationService authz)
    {
      _events = events;
      _members = members;
      _authz = authz;
    }

    private static string allTypes = string.Join(",", Enum.GetNames(typeof(SearchResultType)));
    private readonly Sar.Database.Services.IAuthorizationService _authz;

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>used by Angular navigation chrome, /account/detail/{username} link member</remarks>
    /// <param name="q"></param>
    /// <param name="t"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    [HttpGet("/search")]

    public async Task<SearchResult[]> Search(string q, string t = null, int limit = 10)
    {
      var searchTypes = (t ?? allTypes).Split(',').Select(f => (SearchResultType)Enum.Parse(typeof(SearchResultType), f)).ToArray();

      var now = DateTime.Now;
      var last12Months = now.AddMonths(-12);
      var list = new List<SearchResult>();
      if (searchTypes.Any(f => f == SearchResultType.Member) && await _authz.AuthorizeAsync(null, "Read:Member"))
      {
        list.AddRange(await _members.SearchAsync(q));
      }

      if (searchTypes.Any(f => f == SearchResultType.Mission) && await _authz.AuthorizeAsync(null, "Read:Mission"))
      {
        list.AddRange(await _events.SearchMissionsAsync(q));
      }

      return list.OrderByDescending(f => f.Score).Take(limit).ToArray();
    }
  }
}