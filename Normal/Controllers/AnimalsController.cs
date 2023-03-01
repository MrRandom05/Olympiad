using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Normal.Controllers
{
    [ApiController]
    [Route("/animals")]
    public class AnimalsController : Controller
    {
        private readonly ContextClass db;
        public AnimalsController(ContextClass db)
        {
            this.db = db;
        }

        [HttpGet("/{animalId}")]
        public async Task<ActionResult> GetAnimal(long animalId)
        {
            if (animalId == null | animalId <= 0) return StatusCode(400);
            var animal = db.Animals.Where(x => x.Id == animalId).Include(x => x.AnimalTypes).Include(x => x.VisitedLocations).FirstOrDefault();
            if (animal == null) return StatusCode(404);
            return Json(animal);
        }

        [HttpGet("/search")]
        public async Task<ActionResult> SearchAnimal([FromQuery] DateTime? startDateTime, [FromQuery] DateTime? endDateTime, [FromQuery] int? chipperId, [FromQuery] long? chippingLocationId, [FromQuery] string? lifeStatus, [FromQuery] string? gender, [FromQuery] int? from, [FromQuery] int? size)
        {
            if (from < 0 | size <= 0 | chipperId <= 0 | chippingLocationId <= 0) return StatusCode(400);
            //if (startDateTime.ToString() != string.Format($"yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffzzz") | endDateTime.ToString() != string.Format($"yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffzzz")) return Results.StatusCode(400);
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

            if (!animals.Any()) return Json(animals);

            animals = animals.Skip((int)from);
            animals = animals.Take((int)size);
            animals = animals.OrderBy(x => x.Id);
            animals.Include(x => x.AnimalTypes).Include(x => x.VisitedLocations);
            return Json(animals);
        }

        [HttpPost]
        public async Task<ActionResult> CreateAnimal([FromQuery] long[] animalTypes, [FromQuery] float weight, [FromQuery] float length, [FromQuery] float height, [FromQuery] string gender, [FromQuery] int chipperId, [FromQuery] long chippingLocationId)
        {
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
                Lenght = length,
                Weight = weight,
                LifeStatus = "Alive"
            };
            var an = db.Animals.Add(animal).Entity;
            db.SaveChanges();
            return new ObjectResult(an) { StatusCode = 201 };
        }

        [HttpPut("/{animalId}")]
        public async Task<ActionResult> UpdateAnimal([FromQuery] long animalId, [FromQuery] float weight, [FromQuery] float length, [FromQuery] float height, [FromQuery] string gender, [FromQuery] string lifeStatus, [FromQuery] int chipperId, [FromQuery] long chippingLocationId)
        {
            if (weight == null | weight <= 0 | length == null | length <= 0 | height == null | height <= 0 | chipperId <= 0 | chipperId == null | lifeStatus.ToLower() != "alive" & lifeStatus.ToLower() != "dead" |
                gender == null | gender.ToLower() != "male" & gender.ToLower() != "female" & gender.ToLower() != "other" | chipperId == null | chipperId <= 0 |
                chippingLocationId == null | chippingLocationId <= 0) return StatusCode(400);
            var animal = db.Animals.Find(animalId);
            if (animal == null | db.Accounts.Find(chipperId) == null | db.LocationPoints.Find(chippingLocationId) == null) return StatusCode(404);
            if (animal.LifeStatus.ToLower() == "dead" & lifeStatus.ToLower() == "alive") return StatusCode(400);
            if (chippingLocationId == animal.ChippingLocationId) return StatusCode(400);
            animal.Weight = weight;
            animal.Height = height;
            animal.Lenght = length;
            animal.ChipperId = chipperId;
            animal.ChippingLocationId = chippingLocationId;
            animal.Gender = gender;
            animal.LifeStatus = lifeStatus;
            if (lifeStatus.ToLower() == "dead")
                animal.DeathDateTime = DateTime.Now;
            db.Update(animal);
            return Json(animal);
        }

        [HttpDelete("/{animalId}")]
        public async Task<ActionResult> DeleteAnimal([FromQuery] long animalId)
        {
            if (animalId == null | animalId <= 0) return StatusCode(400);
            var animal = db.Animals.Find(animalId);
            if (animal == null) return StatusCode(404);
            if (!animal.VisitedLocations.Any()) return StatusCode(400);
            db.Animals.Where(x => x.Id == animalId).ExecuteDelete();
            db.SaveChanges();

            return StatusCode(200);
        }

        [HttpPost("/{animalId}/types/{typeId}")]
        public async Task<ActionResult> AddTypeToAnimal([FromQuery] long animalId, [FromQuery] long typeId)
        {
            if (animalId == null || typeId == null || animalId <= 0 || typeId <= 0) return StatusCode(400);
            if (db.Animals.Find(animalId) == null || db.AnimalTypes.Find(typeId) == null) return StatusCode(404);
            var animal = db.Animals.Where(x => x.Id == animalId).Include(x => x.AnimalTypes).FirstOrDefault();
            animal.AnimalTypes.Add(db.AnimalTypes.Find(typeId));
            db.Animals.Update(animal);
            db.SaveChanges();

            var res = db.Animals.Find(animalId);
            return new ObjectResult(res) { StatusCode = 201 };
        }

        [HttpPut("/{animalId}/types")]
        public async Task<ActionResult> UpdateTypeAnimal([FromQuery] long animalId, [FromQuery] long oldTypeId, [FromQuery] long newTypeId)
        {
            if (animalId == null || animalId <= 0 || oldTypeId == null || oldTypeId <= 0 || newTypeId == null || newTypeId <= 0) return StatusCode(400);
            var animal = db.Animals.Where(x => x.Id == animalId).Include(x => x.AnimalTypes).FirstOrDefault();
            var oldType = db.AnimalTypes.Find(oldTypeId);
            var newType = db.AnimalTypes.Find(newTypeId);
            if (oldType == null || newType == null) return StatusCode(404);
            var types = new List<AnimalType>();
            types = animal.AnimalTypes;
            types.Remove(oldType);
            types.Add(newType);
            animal.AnimalTypes = types;
            db.Animals.Update(animal);
            db.SaveChanges();
            return new ObjectResult(animal) { StatusCode = 201 };
        }

        [HttpDelete("/{animalId}/types/{typeId}")]
        public async Task<ActionResult> DeleteTypeAnimal([FromQuery] long animalId, [FromQuery] long typeId)
        {
            if (animalId <= 0 || animalId == null || typeId <= 0 || typeId == null || db.Animals.Find(animalId).AnimalTypes.Contains(db.AnimalTypes.Find(typeId)) && db.Animals.Find(animalId).AnimalTypes.Count == 1) return StatusCode(400);
            if (db.Animals.Find(animalId) == null || db.AnimalTypes.Find(typeId) == null || db.Animals.Where(x => x.Id == animalId).Include(x => x.AnimalTypes).Where(x => x.AnimalTypes.Contains(db.AnimalTypes.Find(typeId))).FirstOrDefault() == null) return StatusCode(404);
            var temp = db.Animals.Where(x => x.Id == animalId).Include(x => x.AnimalTypes).FirstOrDefault();
            var list = temp.AnimalTypes;
            list.Remove(db.AnimalTypes.Find(typeId));
            temp.AnimalTypes = list;
            db.Animals.Update(temp);
            db.SaveChanges();
            return StatusCode(200);
        }





    }
}
