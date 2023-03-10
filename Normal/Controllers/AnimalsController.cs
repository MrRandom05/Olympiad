using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Normal.Models;
using System.Globalization;
using static Normal.Controllers.AuthenticationController;

namespace Normal.Controllers
{
    [ApiController]
    [Route("/animals")]
    public class AnimalsController : ControllerBase
    {
        private readonly ContextClass db;
        public AnimalsController(ContextClass db)
        {
            this.db = db;
        }

        [HttpGet("/animals/{animalId}")]
        public async Task<ActionResult> GetAnimal(long animalId)
        {
            Auth.AuthRes auth = Auth.Authorization(HttpContext.Request.Headers["Authorization"], db, out _);
            if (auth == Auth.AuthRes.Error) return StatusCode(401);

            if (animalId == null | animalId <= 0) return StatusCode(400);
            var animal = db.Animals.Where(x => x.Id == animalId).Include(x => x.AnimalTypes).Include(x => x.VisitedLocations).FirstOrDefault();
            if (animal == null) return StatusCode(404);
            AnimalResponceDT responce = new AnimalResponceDT
            {
                Id = animalId,
                AnimalTypes = animal.AnimalTypes.Select(x => x.Id).ToArray(),
                ChipperId = animal.ChipperId,
                ChippingDateTime = animal.ChippingDateTime.ToString("yyyy-MM-dd'T'HH:mm:ssZ"),
                ChippingLocationId = animal.ChippingLocationId,
                Weight = animal.Weight,
                Gender = animal.Gender,
                Height = animal.Height,
                Length = animal.Length,
                LifeStatus = animal.LifeStatus.ToUpper(),
                VisitedLocations = animal.VisitedLocations.Select(x => x.Id).ToArray()
            };
            
            if (responce.LifeStatus.ToLower() == "alive") responce.DeathDateTime = null;
            if (animal.DeathDateTime != null) responce.DeathDateTime = ((DateTime)animal.DeathDateTime).ToString("yyyy-MM-dd'T'HH:mm:ssZ");
            return new ObjectResult(responce) { StatusCode = 200 };
        }

        [HttpGet("/animals/search")]
        public async Task<ActionResult> SearchAnimal([FromQuery] DateTime? startDateTime, [FromQuery] DateTime? endDateTime, [FromQuery] int? chipperId, [FromQuery] long? chippingLocationId, [FromQuery] string? lifeStatus, [FromQuery] string? gender, [FromQuery] int? from, [FromQuery] int? size)
        {
            long ? id;
            Auth.AuthRes auth = Auth.Authorization(HttpContext.Request.Headers["Authorization"], db, out id);
            if (auth == Auth.AuthRes.Error) return StatusCode(401);
            if (from < 0 || size <= 0 || chipperId <= 0 || chippingLocationId <= 0) return StatusCode(400);
            if (lifeStatus != null)
            {
                if (lifeStatus.ToLower() != "alive" & lifeStatus.ToLower() != "dead") return StatusCode(400);
            }
            if (gender != null)
            {
                if (gender.ToLower() != "male" & gender.ToLower() != "female" & gender.ToLower() != "other") return StatusCode(400);
            }
            IQueryable<Animal> animals = db.Animals.AsQueryable();
            if (startDateTime != null)
                animals = animals.Where(x => x.ChippingDateTime > startDateTime);
            if (endDateTime != null)
                animals = animals.Where(x => x.ChippingDateTime < endDateTime);
            if (chipperId != null)
                animals = animals.Where(x => x.ChipperId == chipperId);
            if (chippingLocationId != null)
                animals = animals.Where(x => x.ChippingLocationId == chippingLocationId);
            if (!string.IsNullOrEmpty(lifeStatus))
                animals = animals.Where(x => x.LifeStatus == lifeStatus);
            if (!string.IsNullOrEmpty(gender))
                animals = animals.Where(x => x.Gender == gender);
            if (from == null)
                from = 0;
            if (size == null)
                size = 10;

            if (!animals.Any()) return new ObjectResult(animals) { StatusCode = 200 };

            animals = animals.Skip((int)from);
            animals = animals.Take((int)size);
            animals = animals.OrderBy(x => x.Id);
            animals = animals.Include(x => x.AnimalTypes).Include(x => x.VisitedLocations);
            List<AnimalResponceDT> responce = new List<AnimalResponceDT>();
            var animalsArr = animals.ToArray();
            for (int i = 0; i < animals.Count();i++)
            {
                responce.Add(new AnimalResponceDT
                {
                    AnimalTypes = animalsArr[i].AnimalTypes.Select(x => x.Id).ToArray(),
                    ChipperId = animalsArr[i].ChipperId,
                    ChippingDateTime = animalsArr[i].ChippingDateTime.ToString("yyyy-MM-dd'T'HH:mm:ssZ"),
                    ChippingLocationId = animalsArr[i].ChippingLocationId,
                    Gender= animalsArr[i].Gender,
                    Height = animalsArr[i].Height,
                    Id = animalsArr[i].Id,
                    Length = animalsArr[i].Length,
                    LifeStatus = animalsArr[i].LifeStatus.ToUpper(),
                    VisitedLocations = animalsArr[i].VisitedLocations.Select(x => x.Id).ToArray(),
                    Weight= animalsArr[i].Weight
                });
                if (animalsArr[i].DeathDateTime != null) responce[i].DeathDateTime = ((DateTime)animalsArr[i].DeathDateTime).ToString("yyyy-MM-dd'T'HH:mm:ssZ");
                
                if (animalsArr[i].LifeStatus.ToLower() == "alive") responce[i].DeathDateTime = null;
            }
            return new ObjectResult(responce) { StatusCode = 200 };
        }

        [HttpPost("/animals")]
        public async Task<ActionResult> CreateAnimal([FromBody] CreateAnimalRequest request)
        {
            var animalTypes = request.AnimalTypes;
            var weight = request.Weight;
            var height = request.height;
            var length = request.length;
            var gender = request.Gender;
            var chipperId = request.ChipperId;
            var chippingLocationId = request.ChippingLocationId;
            long? id;
            Auth.AuthRes auth = Auth.Authorization(HttpContext.Request.Headers["Authorization"], db, out id);
            if (auth != Auth.AuthRes.Ok) return StatusCode(401);

            if (animalTypes == null | animalTypes.Length <= 0 | weight == null | weight <= 0 | length == null | length <= 0 | height == null | height <= 0 |
                gender == null | gender.ToLower() != "male" & gender.ToLower() != "female" & gender.ToLower() != "other" | chipperId == null | chipperId <= 0 |
                chippingLocationId == null | chippingLocationId <= 0) return StatusCode(400);

            for (int i = 0; i < animalTypes.Length; i++)
            {
                if (animalTypes[i] == null | animalTypes[i] <= 0) return StatusCode(400);
                if (db.AnimalTypes.Find(animalTypes[i]) == null) return StatusCode(404);
                for (int j = 0; j < animalTypes.Length; j++)
                {
                    if (i != j)
                    {
                        if (animalTypes[i] == animalTypes[j]) return StatusCode(409);
                    }

                }
            }

            if (db.Accounts.Find((long)chipperId) == null) return StatusCode(404);
            if (db.LocationPoints.Find(chippingLocationId) == null) return StatusCode(404);

            var types = db.AnimalTypes.Where(x => animalTypes.Contains(x.Id)).ToList();
            Animal animal = new Animal
            {
                AnimalTypes = types,
                ChipperId = chipperId,
                ChippingDateTime = DateTime.Now,
                ChippingLocationId = chippingLocationId,
                Gender = gender,
                Height = height,
                Length = length,
                Weight = weight,
                LifeStatus = "ALIVE"
            };
            var an = db.Animals.Add(animal).Entity;
            db.SaveChanges();
            AnimalResponceDT responce = new AnimalResponceDT
            {
                Id = an.Id,
                AnimalTypes = animal.AnimalTypes.Select(x => x.Id).ToArray(),
                ChipperId = animal.ChipperId,
                ChippingDateTime = animal.ChippingDateTime.ToString("yyyy-MM-dd'T'HH:mm:ssZ"),
                ChippingLocationId = animal.ChippingLocationId,
                Weight = animal.Weight,
                Gender = animal.Gender,
                Height = animal.Height,
                Length = animal.Length,
                LifeStatus = animal.LifeStatus,
                VisitedLocations = new long[0]
            };
            
            return new ObjectResult(responce) { StatusCode = 201 };
        }

        [HttpPut("/animals/{animalId}")]
        public async Task<ActionResult> UpdateAnimal( long animalId, [FromBody] UpdateAnimalRequest request)
        {
            var weight = request.Weight;
            var height = request.height;
            var length = request.length;
            var gender = request.Gender;
            long chipperId = request.ChipperId;
            var chippingLocationId = request.ChippingLocationId;
            var lifeStatus = request.LifeStatus;
            long? id;
            Auth.AuthRes auth = Auth.Authorization(HttpContext.Request.Headers["Authorization"], db, out id);
            if (auth != Auth.AuthRes.Ok) return StatusCode(401);
            
            if (weight == null | weight <= 0 | length == null | length <= 0 | height == null | height <= 0 | chipperId <= 0 | chipperId == null |
                gender == null | chipperId == null | chipperId <= 0 | chippingLocationId == null | chippingLocationId <= 0 | lifeStatus == null) return StatusCode(400);
            if (gender.ToLower() != "male" & gender.ToLower() != "female" & gender.ToLower() != "other") return StatusCode(400);
            if (lifeStatus.ToLower() != "alive" & lifeStatus.ToLower() != "dead") return StatusCode(400);

            var animal = db.Animals.Include(x => x.VisitedLocations).Include(x => x.AnimalTypes).Where(x => x.Id == animalId).FirstOrDefault();
            if (animal == null | db.Accounts.Where(x => x.Id == chipperId).FirstOrDefault() == null | db.LocationPoints.Where(x => x.Id == chippingLocationId).FirstOrDefault() == null) return StatusCode(404);
            if (animal.LifeStatus.ToLower() != "dead" & animal.LifeStatus.ToLower() != "alive") return StatusCode(400);
            if (!animal.VisitedLocations.IsNullOrEmpty())
                if (chippingLocationId == animal.VisitedLocations.First().LocationPointId) return StatusCode(400);
            animal.Weight = weight;
            animal.Height = height;
            animal.Length = length;
            animal.ChipperId = (int)chipperId;
            animal.ChippingLocationId = chippingLocationId;
            animal.Gender = gender;
            animal.LifeStatus = lifeStatus;
            if (lifeStatus.ToLower() == "dead")
                animal.DeathDateTime = DateTime.Now;
            db.Update(animal);
            db.SaveChanges();
            AnimalResponceDT responce = new AnimalResponceDT
            {
                Id = animalId,
                AnimalTypes = animal.AnimalTypes.Select(x => x.Id).ToArray(),
                ChipperId = animal.ChipperId,
                ChippingDateTime = animal.ChippingDateTime.ToString("yyyy-MM-dd'T'HH:mm:ssZ"),
                ChippingLocationId = animal.ChippingLocationId,
                Weight = animal.Weight,
                Gender = animal.Gender,
                Height = animal.Height,
                Length = animal.Length,
                LifeStatus = animal.LifeStatus.ToUpper(),
                VisitedLocations = animal.VisitedLocations.Select(x => x.Id).ToArray()
            };
            
            if (animal.DeathDateTime == null) responce.DeathDateTime = null;
            if (animal.DeathDateTime != null) responce.DeathDateTime = ((DateTime)animal.DeathDateTime).ToString("yyyy-MM-dd'T'HH:mm:ssZ");
            return new ObjectResult(responce) { StatusCode = 200 };
        }

        [HttpDelete("/animals/{animalId}")]
        public async Task<ActionResult> DeleteAnimal(long animalId)
        {
            long? id;
            Auth.AuthRes auth = Auth.Authorization(HttpContext.Request.Headers["Authorization"], db, out id);
            if (auth != Auth.AuthRes.Ok) return StatusCode(401);

            if (animalId == null | animalId <= 0) return StatusCode(400);
            var animal = db.Animals.Where(x => x.Id == animalId).Include(x => x.VisitedLocations).FirstOrDefault();
            if (animal == null) return StatusCode(404);
            if (animal.VisitedLocations.Where(x => x.Id != animal.ChippingLocationId).Any()) return StatusCode(400);
            db.Animals.Where(x => x.Id == animalId).ExecuteDelete();
            db.SaveChanges();

            return StatusCode(200);
        }

        [HttpPost("/animals/{animalId}/types/{typeId}")]
        public async Task<ActionResult> AddTypeToAnimal(long animalId, long typeId)
        {
            long? id;
            Auth.AuthRes auth = Auth.Authorization(HttpContext.Request.Headers["Authorization"], db, out id);
            if (auth != Auth.AuthRes.Ok) return StatusCode(401);

            if (animalId == null || typeId == null || animalId <= 0 || typeId <= 0) return StatusCode(400);
            if (db.Animals.Find(animalId) == null || db.AnimalTypes.Find(typeId) == null) return StatusCode(404);
            var animal = db.Animals.Where(x => x.Id == animalId).Include(x => x.AnimalTypes).FirstOrDefault();
            animal.AnimalTypes.Add(db.AnimalTypes.Find(typeId));
            db.Animals.Update(animal);
            db.SaveChanges();

            var res = db.Animals.Find(animalId);
            AnimalResponceDT responce = new AnimalResponceDT
            {
                Id = animalId,
                AnimalTypes = res.AnimalTypes.Select(x => x.Id).ToArray(),
                ChipperId = res.ChipperId,
                ChippingDateTime = res.ChippingDateTime.ToString("yyyy-MM-dd'T'HH:mm:ssZ"),
                ChippingLocationId = res.ChippingLocationId,
                Weight = res.Weight,
                Gender = res.Gender,
                Height = res.Height,
                Length = res.Length,
                LifeStatus = res.LifeStatus.ToUpper(),
                VisitedLocations = res.VisitedLocations.Select(x => x.Id).ToArray()
            };
            
            if (responce.LifeStatus.ToLower() == "alive") responce.DeathDateTime = null;
            return new ObjectResult(responce) { StatusCode = 201 };
        }

        [HttpPut("/animals/{animalId}/types")]
        public async Task<ActionResult> UpdateTypeAnimal( long animalId, [FromBody] UpdateAnimalTypeAnimalRequest request)
        {
            var oldTypeId = request.oldTypeId;
            var newTypeId = request.newTypeId;
            long? id;
            Auth.AuthRes auth = Auth.Authorization(HttpContext.Request.Headers["Authorization"], db, out id);
            if (auth != Auth.AuthRes.Ok) return StatusCode(401);

            if (animalId == null || animalId <= 0 || oldTypeId == null || oldTypeId <= 0 || newTypeId == null || newTypeId <= 0) return StatusCode(400);
            var animal = db.Animals.Where(x => x.Id == animalId).Include(x => x.AnimalTypes).FirstOrDefault();
            var oldType = db.AnimalTypes.Find(oldTypeId);
            var newType = db.AnimalTypes.Find(newTypeId);
            if (oldType == null || newType == null || animal == null || !animal.AnimalTypes.Contains(oldType)) return StatusCode(404);
            if (animal.AnimalTypes.Contains(newType)) return StatusCode(409);
            var types = new List<AnimalType>();
            types = animal.AnimalTypes;
            types.Remove(oldType);
            types.Add(newType);
            animal.AnimalTypes = types;
            db.Animals.Update(animal);
            db.SaveChanges();
            AnimalResponceDT responce = new AnimalResponceDT
            {
                Id = animalId,
                AnimalTypes = animal.AnimalTypes.Select(x => x.Id).ToArray(),
                ChipperId = animal.ChipperId,
                ChippingDateTime = animal.ChippingDateTime.ToString("yyyy-MM-dd'T'HH:mm:ssZ"),
                ChippingLocationId = animal.ChippingLocationId,
                Weight = animal.Weight,
                Gender = animal.Gender,
                Height = animal.Height,
                Length = animal.Length,
                LifeStatus = animal.LifeStatus.ToUpper(),
                VisitedLocations = animal.VisitedLocations.Select(x => x.Id).ToArray()
            };
            if (animal.DeathDateTime != null) responce.DeathDateTime = ((DateTime)animal.DeathDateTime).ToString("yyyy-MM-dd'T'HH:mm:ssZ");
            
            if (responce.LifeStatus.ToLower() == "alive") responce.DeathDateTime = null;
            return new ObjectResult(responce) { StatusCode = 200 };
        }

        [HttpDelete("/animals/{animalId}/types/{typeId}")]
        public async Task<ActionResult> DeleteTypeAnimal(long animalId, long typeId)
        {
            long? id;
            Auth.AuthRes auth = Auth.Authorization(HttpContext.Request.Headers["Authorization"], db, out id);
            if (auth != Auth.AuthRes.Ok) return StatusCode(401);

            if (animalId <= 0 || animalId == null || typeId <= 0 || typeId == null) return StatusCode(400);
            var animal = db.Animals.Include(x => x.AnimalTypes).Where(x => x.Id == animalId).FirstOrDefault();
            if (animal == null) return StatusCode(404);
            var type = db.AnimalTypes.Where(x => x.Id == typeId).FirstOrDefault();
            if (type == null || !animal.AnimalTypes.Contains(type)) return StatusCode(404);
            if (animal.AnimalTypes.Contains(type) && animal.AnimalTypes.Count == 1) return StatusCode(400);
            var list = animal.AnimalTypes;
            list.Remove(type);
            animal.AnimalTypes = list;
            db.Animals.Update(animal);
            db.SaveChanges();
            return StatusCode(200);
        }


    }
}
