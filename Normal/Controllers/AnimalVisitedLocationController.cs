using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Normal.Controllers
{
    [ApiController]
    public class AnimalVisitedLocationController : Controller
    {
        private readonly ContextClass db;
        public AnimalVisitedLocationController(ContextClass db)
        {
            this.db = db;
        }
        [HttpGet("animals/{animalId}/locations")]
        public async Task<ActionResult> GetAnimalVisitedPoints([FromQuery] long animalId, [FromQuery] DateTime? startDateTime, [FromQuery] DateTime? endDateTime, [FromQuery] int? from, [FromQuery] int? size)
        {
            if (animalId == null || animalId <= 0 || from < 0 || size <= 0) return StatusCode(400);
            if (db.Animals.Find(animalId) == null) return StatusCode(404);
            if (size == null) size = 10;
            if (from == null) from = 0;
            var point = db.AnimalVisitedLocations.AsQueryable();
            if (startDateTime != null) point.Where(x => x.DateTimeOfVisitLocationPoint >= startDateTime);
            if (endDateTime != null) point.Where(x => x.DateTimeOfVisitLocationPoint <= endDateTime);
            if (!point.Any()) return Json(point);

            point = point.Skip((int)from);
            point = point.Take((int)size);
            point = point.OrderBy(x => x.Id);
            return Json(point);
        }

        [HttpPost("animals/{animalId}/locations/{pointId}")]
        public async Task<ActionResult> AddVisitedLocationpointToAnimal([FromQuery] long animalId, [FromQuery] long pointId)
        {
            if (animalId == null || animalId <= 0 || pointId == null || pointId <= 0) return StatusCode(400);
            var animal = db.Animals.Where(x => x.Id == animalId).Include(x => x.VisitedLocations).FirstOrDefault();
            var point = db.LocationPoints.Find(pointId);
            if (animal.LifeStatus.ToLower() == "dead" || animal.VisitedLocations.Contains(db.AnimalVisitedLocations.Where(x => x.LocationPointId == animal.ChippingLocationId).FirstOrDefault()) || animal.ChippingLocationId == point.Id) return StatusCode(400);
            if (!animal.VisitedLocations.IsNullOrEmpty())
                if (animal.VisitedLocations.Last() == db.AnimalVisitedLocations.Where(x => x.LocationPointId == pointId).FirstOrDefault()) return StatusCode(400);
            if (animal == null || point == null) return StatusCode(404);
            var res = db.AnimalVisitedLocations.Add(new AnimalVisitedLocation { DateTimeOfVisitLocationPoint = DateTime.Now, LocationPointId = point.Id }).Entity;
            var an = db.Animals.Where(x => x.Id == animalId).Include(x => x.VisitedLocations).FirstOrDefault();
            an.VisitedLocations.Add(res);
            db.Animals.Update(an);
            db.SaveChanges();
            return new ObjectResult(res) { StatusCode = 201 };
        }

        [HttpPut("animals/{animalId}/locations")]
        public async Task<ActionResult> UpdateAnimalVisitedLocation([FromQuery] long animalId, [FromQuery] long visitedLocationPointId, [FromQuery] long locationPointId)
        {
            if (animalId == null || animalId <= 0 || visitedLocationPointId == null || visitedLocationPointId <= 0 || locationPointId == null || locationPointId <= 0) return StatusCode(400);
            var animal = db.Animals.Where(x => x.Id == animalId).Include(z => z.VisitedLocations).Where(x => x.VisitedLocations.Contains(db.AnimalVisitedLocations.Find(visitedLocationPointId))).FirstOrDefault();
            var oldVisitedLocationPoint = db.AnimalVisitedLocations.Where(x => x.Id == visitedLocationPointId).FirstOrDefault();
            var point = db.LocationPoints.Where(x => x.Id == locationPointId).FirstOrDefault();

            if (animal.VisitedLocations.First().Id == visitedLocationPointId & animal.ChippingLocationId == locationPointId || oldVisitedLocationPoint.LocationPointId == locationPointId
            ) return StatusCode(400);
            if (animal == null || animal.VisitedLocations.IsNullOrEmpty() || point == null) return StatusCode(404);
            oldVisitedLocationPoint.LocationPointId = point.Id;
            db.AnimalVisitedLocations.Update(oldVisitedLocationPoint);
            db.SaveChanges();
            return Json(oldVisitedLocationPoint);
        }

        [HttpDelete("animals/{animalId}/locations/{visitedPointId}")]
        public async Task<ActionResult> DeleteAnimalVisitedPoint([FromQuery] long animalId, [FromQuery] long visitedPointId)
        {
            if (animalId == null || animalId <= 0 || visitedPointId == null || visitedPointId <= 0) return StatusCode(400);
            var animal = db.Animals.Where(x => x.Id == animalId).Include(x => x.VisitedLocations).FirstOrDefault();
            var point = db.AnimalVisitedLocations.Where(x => x.Id == visitedPointId).FirstOrDefault();

            if (animal == null || point == null || !animal.VisitedLocations.Contains(point)) return StatusCode(404);
            if (!animal.VisitedLocations.IsNullOrEmpty())
            {
                if (visitedPointId == animal.VisitedLocations.First().Id & animal.VisitedLocations[1].Id == animal.ChippingLocationId)
                {
                    var toDel = animal.VisitedLocations[1];
                    db.AnimalVisitedLocations.Where(x => x.Id == toDel.Id).ExecuteDelete();
                    db.SaveChanges();
                }
            }
            //animal.VisitedLocations.Remove(db.AnimalVisitedLocations.Where(x => x.Id == visitedPointId).FirstOrDefault());
            //db.Animals.Update(animal);
            db.AnimalVisitedLocations.Where(x => x.Id == visitedPointId).ExecuteDelete();
            db.SaveChanges();
            return StatusCode(200);
        }
    }
}
