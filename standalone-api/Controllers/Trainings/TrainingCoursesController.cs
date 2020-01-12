using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sar;
using Sar.Database.Model;
using Sar.Database.Model.Training;
using Sar.Database.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kcsara.Database.Api.Controllers.Training
{
  [Authorize]
  public class TrainingCoursesController : Controller
  {
    private readonly ITrainingCoursesService _courses;
    private readonly Sar.Database.Services.IAuthorizationService _authz;

    public TrainingCoursesController(ITrainingCoursesService courses, Sar.Database.Services.IAuthorizationService authz)
    {
      _courses = courses;
      _authz = authz;
    }

    [HttpGet("training/courses/{courseId}")]
    public async Task<ItemPermissionWrapper<TrainingCourse>> GetCourse(Guid courseId)
    {
      await _authz.EnsureAsync(courseId, "Read:TrainingCourse");
      return await _courses.GetAsync(courseId);
    }

    [HttpGet("training/courses/{courseId}/stats")]
    public async Task<object> GetCourseStats(Guid courseId)
    {
      await _authz.EnsureAsync(courseId, "Read:TrainingCourse");
      return await _courses.GetCourseStats(courseId);
    }

    [HttpGet("training/courses/{courseId}/roster")]
    public async Task<List<TrainingRecord>> ListCourseRoster(Guid courseId)
    {
      await _authz.EnsureAsync(courseId, "Read:TrainingCourse");
      await _authz.EnsureAsync(null, "Read:Member");

      return await _courses.ListRoster(courseId);
    }

    [HttpGet("training/courses")]
    public async Task<ListPermissionWrapper<TrainingCourse>> List()
    {
      await _authz.EnsureAsync(null, "Read:TrainingCourse");
      return await _courses.List();
    }

    [HttpPost("training/courses")]
    //[ValidateModelState]
    public async Task<TrainingCourse> CreateNew([FromBody]TrainingCourse course)
    {
      await _authz.EnsureAsync(null, "Create:TrainingCourse");

      if (course.Id != Guid.Empty)
      {
        throw new UserErrorException("New units shouldn't include an id");
      }

      course = await _courses.SaveAsync(course);
      return course;
    }

    [HttpPut("training/courses/{courseId}")]
    //[ValidateModelState]
    public async Task<TrainingCourse> Save(Guid courseId, [FromBody]TrainingCourse course)
    {
      await _authz.EnsureAsync(courseId, "Update:TrainingCourse");

      if (course.Id != courseId) ModelState.AddModelError("id", "Can not be changed");

      if (!ModelState.IsValid) throw new UserErrorException("Invalid parameters");

      course = await _courses.SaveAsync(course);
      return course;
    }

    [HttpDelete("training/courses/{courseId}")]
    public async Task Delete(Guid courseId)
    {
      await _authz.EnsureAsync(courseId, "Delete:Unit");

      await _courses.DeleteAsync(courseId);
    }
  }
}
