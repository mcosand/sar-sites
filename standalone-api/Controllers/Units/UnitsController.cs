using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sar;
using Sar.Database.Api.Extensions;
using Sar.Database.Model;
using Sar.Database.Model.Units;
using Sar.Database.Services;
using System;
using System.Threading.Tasks;

namespace Kcsara.Database.Api.Controllers.Units
{
  [Authorize]
  public class UnitsController : Controller
  {
    private readonly IUnitsService service;
    private readonly Sar.Database.Services.IAuthorizationService authz;

    public UnitsController(IUnitsService service, Sar.Database.Services.IAuthorizationService authz)
    {
      this.service = service;
      this.authz = authz;
    }

    [HttpGet]
    [Route("/units")]
    public async Task<ListPermissionWrapper<Unit>> List()
    {
      await authz.EnsureAsync(null, "Read:Unit");
      return await service.List();
    }

    [HttpGet("units/{id}")]
    public async Task<ItemPermissionWrapper<Unit>> Get(Guid id)
    {
      await authz.EnsureAsync(id, "Read:Unit");
      return await service.Get(id);
    }


    [HttpPost("units")]
    //[ValidateModelState]
    public async Task<Unit> CreateNew([FromBody]Unit unit)
    {
      await authz.EnsureAsync(null, "Create:Unit");

      if (unit.Id != Guid.Empty)
      {
        throw new UserErrorException("New units shouldn't include an id");
      }

      unit = await service.Save(unit);
      return unit;
    }

    [HttpPut("units/{unitId}")]
    //[ValidateModelState]
    public async Task<Unit> Save(Guid unitId, [FromBody]Unit unit)
    {
      await authz.EnsureAsync(unitId, "Update:Unit");

      if (unit.Id != unitId) ModelState.AddModelError("id", "Can not be changed");

      if (!ModelState.IsValid) throw new UserErrorException("Invalid parameters");

      unit = await service.Save(unit);
      return unit;
    }

    [HttpDelete("units/{unitId}")]
    public async Task Delete(Guid unitId)
    {
      await authz.EnsureAsync(unitId, "Delete:Unit");

      await service.Delete(unitId);
    }

    [HttpGet("units/{unitId}/reports")]
    public async Task<UnitReportInfo[]> ListReports(Guid unitId)
    {
      await authz.EnsureAsync(unitId, "Read:Unit");
      return await service.ListReports(unitId);
    }
  }
}