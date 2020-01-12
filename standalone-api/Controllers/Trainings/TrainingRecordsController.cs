using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sar.Database.Model;
using Sar.Database.Model.Training;
using Sar.Database.Services;
using Sar.Database.Services.Training;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Kcsara.Database.Api.Controllers
{
  [Authorize]
  public class TrainingRecordsController : Controller
  {
    private readonly ITrainingRecordsService _records;
    private readonly Sar.Database.Services.IAuthorizationService _authz;

    public TrainingRecordsController(ITrainingRecordsService records, Sar.Database.Services.IAuthorizationService authz)
    {
      _records = records;
      _authz = authz;
    }

    [HttpPost("trainingrecords")]
    //[ValidateModelState]
    public async Task<TrainingRecord> CreateNew([FromBody]TrainingRecord record)
    {
      await _authz.EnsureAsync(record.Member.Id, "Create:TrainingRecord@MemberId");
      if (record.Member.Id == Guid.Empty)
      {
        ModelState.AddModelError("Member.Id", "required");
      }
      if (record.Course.Id == Guid.Empty)
      {
        ModelState.AddModelError("Course.Id", "required");
      }

      record = await _records.SaveAsync(record);
      return record;
    }

    [HttpGet("members/{memberId}/trainingrecords")]
    public async Task<ListPermissionWrapper<TrainingStatus>> MemberRecords(Guid memberId)
    {
      await _authz.EnsureAsync(memberId, "Read:TrainingRecord@MemberId");
      return await _records.RecordsForMember(memberId, DateTime.Now);
    }

    [HttpGet("members/{memberId}/requiredtraining")]
    public async Task<List<TrainingStatus>> MemberRequired(Guid memberId)
    {
      await _authz.EnsureAsync(memberId, "Read:TrainingRecord@MemberId");
      return await _records.RequiredTrainingStatusForMember(memberId, DateTime.Now);
    }

    [HttpGet("TrainingRecords/RequiredTraining")]
    public async Task<Dictionary<Guid, List<TrainingStatus>>> Required()
    {
      return await _records.RequiredTrainingStatusForUnit(null, DateTime.Now);
    }

    [HttpPost]
    [Route("TrainingRecords/ParseKcsaraCsv")]
    public async Task<List<ParsedKcsaraCsv>> ParseKcsaraCsv(List<IFormFile> file)
    {
      await _authz.EnsureAsync(null, "Read:TrainingRecord");

      var result = new List<ParsedKcsaraCsv>();
      foreach (var formFile in file)
      {
        if (formFile.Length > 0)
        {
          using (var s = formFile.OpenReadStream())
          {
            result.AddRange(await _records.ParseKcsaraCsv(s));
          }
        }
      }
      return result;
    }
  }
}
