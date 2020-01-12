using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sar;
using Sar.Database.Model;
using Sar.Database.Model.Animals;
using Sar.Database.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Kcsara.Database.Api.Controllers.Animals
{
  [Authorize]
  public class AnimalsController : Controller
  {
    private readonly IAnimalsService _animals;
    private readonly Sar.Database.Services.IAuthorizationService _authz;
    private readonly IHost _host;

    public AnimalsController(IAnimalsService animals, Sar.Database.Services.IAuthorizationService authz, IHost host)
    {
      _animals = animals;
      _authz = authz;
      _host = host;
    }
    
    [HttpGet("animals/{animalId}")]
    public async Task<ItemPermissionWrapper<Animal>> GetAnimal(Guid animalId)
    {
      await _authz.EnsureAsync(animalId, "Read:Animal");
      return await _animals.GetAsync(animalId);
    }

    [HttpPost("animals")]
    //[ValidateModelState]
    public async Task<Animal> CreateNew([FromBody]Animal animal)
    {
      await _authz.EnsureAsync(null, "Create:Animal");

      if (animal.Id != Guid.Empty)
      {
        throw new UserErrorException("New animals shouldn't include an id");
      }

      animal = await _animals.Save(animal);
      return animal;
    }

    [HttpPut("animals/{animalId}")]
    //[ValidateModelState]
    public async Task<Animal> Save(Guid animalId, [FromBody]Animal animal)
    {
      await _authz.EnsureAsync(animalId, "Update:Animal");

      if (animal.Id != animalId) ModelState.AddModelError("id", "Can not be changed");

      if (!ModelState.IsValid) throw new UserErrorException("Invalid parameters");

      animal = await _animals.Save(animal);
      return animal;
    }

    [HttpDelete]
    [Route("animals/{animalId}")]
    public async Task Delete(Guid animalId)
    {
      await _authz.EnsureAsync(animalId, "Delete:Animal");

      await _animals.Delete(animalId);
    }

    [HttpGet("animals/{animalId}/photo")]
    [AllowAnonymous]
    public async Task<HttpResponseMessage> Photo(Guid animalId)
    {
      await _authz.EnsureAsync(animalId, "Read:Animal");
      var animal = (await _animals.GetAsync(animalId))?.Item;

      string filename = "content\\images\\nophoto.jpg";
      if (!string.IsNullOrWhiteSpace(animal?.Photo) && _host.FileExists("content\\auth\\animals\\" + animal.Photo))
      {
        filename = "content\\auth\\animals\\" + animal.Photo;
      }

      Stream imageStream = _host.OpenFile(filename);
      var response = new HttpResponseMessage
      {
        Content = new StreamContent(imageStream)
      };
      response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpg");

      return response;
    }

    [HttpGet("animals/{animalId}/owners")]
    public async Task<ListPermissionWrapper<AnimalOwner>> ListOwners(Guid animalId)
    {
      await _authz.EnsureAsync(animalId, "Read:Animal");

      return await _animals.ListOwners(animalId);
    }

    [HttpPut("animals/{animalId}/owners/{ownershipId}")]
    //[ValidateModelState]
    public async Task<AnimalOwner> SaveOwner(Guid animalId, Guid ownershipId, [FromBody]AnimalOwner ownership)
    {
      await _authz.EnsureAsync(ownershipId, "Update:AnimalOwner");

      if (ownership.Animal.Id != animalId) ModelState.AddModelError("animal.id", "Can not be changed");
      if (ownership.Id != ownershipId) ModelState.AddModelError("id", "Can not be changed");

      if (!ModelState.IsValid) throw new UserErrorException("Invalid parameters");

      ownership = await _animals.SaveOwnership(ownership);
      return ownership;
    }

    [HttpPost("animals/{animalId}/owners")]
    //[ValidateModelState]
    public async Task<AnimalOwner> CreateNewOwner(Guid animalId, [FromBody]AnimalOwner ownership)
    {
      await _authz.EnsureAsync(animalId, "Create:AnimalOwner@AnimalId");
      await _authz.EnsureAsync(ownership?.Member?.Id, "Create:AnimalOwner@MemberId");

      if (ownership.Id != Guid.Empty)
      {
        throw new UserErrorException("New animal ownership shouldn't include an id");
      }

      if (ownership.Animal != null && ownership.Animal.Id != animalId)
      {
        throw new UserErrorException("Animal ids do not match", string.Format("Tried to save animal owner with animal id {0} under animal {1}", ownership.Animal.Id, animalId));
      }

      ownership = await _animals.SaveOwnership(ownership);
      return ownership;
    }

    [HttpDelete("animals/{animalId}/owners/{ownershipId}")]
    public async Task DeleteOwner(Guid animalId, Guid ownershipId)
    {
      await _authz.EnsureAsync(animalId, "Delete:AnimalOwner");

      await _animals.DeleteOwnership(ownershipId);
    }

    [HttpGet("animals/{animalId}/missions/stats")]
    public async Task<AttendanceStatistics<NameIdPair>> GetMissionStatistics(Guid animalId)
    {
      await _authz.EnsureAsync(animalId, "Read:Animal");

      return await _animals.GetMissionStatistics(animalId);
    }

    [HttpGet("animals/{animalId}/missions")]
    public async Task<List<EventAttendance>> ListMissions(Guid animalId)
    {
      await _authz.EnsureAsync(animalId, "Read:Animal");
      await _authz.EnsureAsync(null, "Read:Mission");

      return await _animals.GetMissionList(animalId);
    }

    [HttpGet("animals")]
    public async Task<ListPermissionWrapper<Animal>> List()
    {
      await _authz.EnsureAsync(null, "Read:Animal");
      return await _animals.List();
    }

  }
}
