using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using static Normal.Controllers.AuthenticationController;
using Normal.Models;

namespace Normal.Controllers
{
    [ApiController]
    public class AnimalVisitedLocationController : ControllerBase
    {
        private readonly ContextClass db;
        public AnimalVisitedLocationController(ContextClass db)
        {
            this.db = db;
        }
        [HttpGet("animals/{animalId}/locations")]
        public async Task<ActionResult> GetAnimalVisitedPoints(long animalId, [FromQuery] DateTime? startDateTime, [FromQuery] DateTime? endDateTime, [FromQuery] int? from, [FromQuery] int? size)
        {
            long? id;
            Auth.AuthRes auth = Auth.Authorization(HttpContext.Request.Headers["Authorization"], db, out id);
            if (auth == Auth.AuthRes.Error) return StatusCode(401);

            if (animalId == null || animalId <= 0 || from < 0 || size <= 0) return StatusCode(400);
            if (db.Animals.Find(animalId) == null) return StatusCode(404);
            if (size == null) size = 10;
            if (from == null) from = 0;
            var animal = db.Animals.Include(x => x.VisitedLocations).Where(x => x.Id == animalId).FirstOrDefault();
            var point = animal.VisitedLocations.AsQueryable();
            if (startDateTime != null) point.Where(x => x.DateTimeOfVisitLocationPoint >= startDateTime);
            if (endDateTime != null) point.Where(x => x.DateTimeOfVisitLocationPoint <= endDateTime);
            if (!point.Any()) return new ObjectResult(point) { StatusCode = 200 };

            point = point.Skip((int)from);
            point = point.Take((int)size);
            point = point.OrderBy(x => x.Id);
            var pointArr = point.ToArray();
            List<PointDT> res = new List<PointDT>();
            for(int i = 0; i < point.Count(); i++)
            {
                var p = new PointDT();
                p.Id = pointArr[i].Id;
                p.LocationPointId = pointArr[i].LocationPointId;
                p.DateTimeOfVisitLocationPoint = pointArr[i].DateTimeOfVisitLocationPoint.ToString("yyyy-MM-dd'T'HH:mm:ssZ");
                if (animal.VisitedLocations.Contains(db.AnimalVisitedLocations.Where(x => x.LocationPointId == p.LocationPointId).FirstOrDefault()))
                    res.Add(p);
            }
            for (int i = 0; i < res.Count(); i++)
            {
                if (!animal.VisitedLocations.Contains(db.AnimalVisitedLocations.Where(x => x.LocationPointId == res[i].LocationPointId).FirstOrDefault()))
                    res.Remove(res[i]);
            }
            return new ObjectResult(res) { StatusCode = 200 };
        }

        [HttpPost("animals/{animalId}/locations/{pointId}")]
        public async Task<ActionResult> AddVisitedLocationpointToAnimal(long animalId, long pointId)
        {
            if (animalId == null || animalId <= 0 || pointId == null || pointId <= 0) return StatusCode(400);
            long? id;
            Auth.AuthRes auth = Auth.Authorization(HttpContext.Request.Headers["Authorization"], db, out id);
            if (auth != Auth.AuthRes.Ok) return StatusCode(401);

            var animal = db.Animals.Where(x => x.Id == animalId).Include(x => x.VisitedLocations).FirstOrDefault();
            var point = db.LocationPoints.Find(pointId);
            if (animal == null || point == null) return StatusCode(404);
            if (animal.LifeStatus.ToLower() == "dead"  || animal.ChippingLocationId == point.Id && animal.VisitedLocations.Count() == 0) return StatusCode(400);
            if (!animal.VisitedLocations.IsNullOrEmpty())
                if (animal.VisitedLocations.Last() == db.AnimalVisitedLocations.Where(x => x.LocationPointId == pointId).FirstOrDefault()) return StatusCode(400);
            var res = db.AnimalVisitedLocations.Add(new AnimalVisitedLocation { DateTimeOfVisitLocationPoint = DateTime.Now, LocationPointId = point.Id }).Entity;
            var an = db.Animals.Where(x => x.Id == animalId).Include(x => x.VisitedLocations).FirstOrDefault();
            an.VisitedLocations.Add(res);
            db.Animals.Update(an);
            db.SaveChanges();
            PointDT resPoint = new PointDT { Id = res.Id, LocationPointId = point.Id, DateTimeOfVisitLocationPoint = res.DateTimeOfVisitLocationPoint.ToString("yyyy-MM-dd'T'HH:mm:ssZ") };
            return new ObjectResult(resPoint) { StatusCode = 201 };
        }

        [HttpPut("animals/{animalId}/locations")]
        public async Task<ActionResult> UpdateAnimalVisitedLocation(long animalId, [FromBody] UpdateAnimalVisitedLocationPoint request)
        {
            var visitedLocationPointId = request.VisitedLocationPointId;
            var locationPointId = request.LocationPointId;
            long? id;
            Auth.AuthRes auth = Auth.Authorization(HttpContext.Request.Headers["Authorization"], db, out id);
            if (auth != Auth.AuthRes.Ok) return StatusCode(401);

            if (animalId == null || animalId <= 0 || visitedLocationPointId == null || visitedLocationPointId <= 0 || locationPointId == null || locationPointId <= 0) return StatusCode(400);
            var animal = db.Animals.Where(x => x.Id == animalId).Include(z => z.VisitedLocations).FirstOrDefault();
            var oldVisitedLocationPoint = db.AnimalVisitedLocations.Where(x => x.Id == visitedLocationPointId).FirstOrDefault();
            var point = db.LocationPoints.Where(x => x.Id == locationPointId).FirstOrDefault();
            if (animal == null || oldVisitedLocationPoint == null || !animal.VisitedLocations.Contains(oldVisitedLocationPoint) || point == null) return StatusCode(404);
            if (animal.VisitedLocations.First().Id == visitedLocationPointId & animal.ChippingLocationId == locationPointId || oldVisitedLocationPoint.LocationPointId == locationPointId
            ) return StatusCode(400);

            var i = animal.VisitedLocations.IndexOf(oldVisitedLocationPoint);
            if(animal.VisitedLocations.Count() > 1)
            {
                if (i == 0 && animal.VisitedLocations[i + 1].LocationPointId == locationPointId) return StatusCode(400);
                if (i == animal.VisitedLocations.Count()-1 && animal.VisitedLocations[i - 1].LocationPointId == locationPointId) return StatusCode(400);
                if (i !=0 && i != animal.VisitedLocations.Count() - 1 && (animal.VisitedLocations[i - 1].LocationPointId == locationPointId || animal.VisitedLocations[i + 1].LocationPointId == locationPointId)) return StatusCode(400);
            }

            oldVisitedLocationPoint.LocationPointId = point.Id;
            db.AnimalVisitedLocations.Update(oldVisitedLocationPoint);
            db.SaveChanges();
            var responce = new PointDT { Id = oldVisitedLocationPoint.Id, DateTimeOfVisitLocationPoint = oldVisitedLocationPoint.DateTimeOfVisitLocationPoint.ToString("yyyy-MM-dd'T'HH:mm:ssZ"), LocationPointId = oldVisitedLocationPoint.LocationPointId };
            return new ObjectResult(responce) { StatusCode = 200 };
        }

        [HttpDelete("animals/{animalId}/locations/{visitedPointId}")]
        public async Task<ActionResult> DeleteAnimalVisitedPoint(long animalId, long visitedPointId)
        {
            long? id;
            Auth.AuthRes auth = Auth.Authorization(HttpContext.Request.Headers["Authorization"], db, out id);
            if (auth != Auth.AuthRes.Ok) return StatusCode(401);

            if (animalId == null || animalId <= 0 || visitedPointId == null || visitedPointId <= 0) return StatusCode(400);
            var animal = db.Animals.Where(x => x.Id == animalId).Include(x => x.VisitedLocations).FirstOrDefault();
            var point = db.AnimalVisitedLocations.Where(x => x.Id == visitedPointId).FirstOrDefault();
            
            if (animal == null || point == null || !animal.VisitedLocations.Contains(point)) return StatusCode(404);
            var list = animal.VisitedLocations;
            
            if (!animal.VisitedLocations.IsNullOrEmpty())
            {
                if (animal.VisitedLocations.Count >= 2)
                {
                    if (point == animal.VisitedLocations.First() & animal.VisitedLocations[1].LocationPointId == animal.ChippingLocationId)
                    {
                        var toDel = animal.VisitedLocations[1];
                        list.Remove(toDel);
                    }
                }
            }
            list.Remove(point);
            animal.VisitedLocations = list;
            db.Animals.Update(animal);
            db.SaveChanges();
            return StatusCode(200);
        }
    }
}
