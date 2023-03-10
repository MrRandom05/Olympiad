using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using static Normal.Controllers.AuthenticationController;
using Normal.Models;

namespace Normal.Controllers
{
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly ContextClass db;
        public LocationController(ContextClass db) 
        {
            this.db = db;
        }

        [HttpGet("/locations/{pointId}")]
        public async Task<ActionResult> GetLocation(long pointId)
        {
            long? id;
            Auth.AuthRes auth = Auth.Authorization(HttpContext.Request.Headers["Authorization"], db, out id);
            if (auth == Auth.AuthRes.Error) return StatusCode(401);

            if (pointId == null | pointId <= 0) return StatusCode(400);
            var point = db.LocationPoints.Find(pointId);
            if (point == null) return StatusCode(404);
            else return new ObjectResult(point) { StatusCode = 200 };
        }

        [HttpPost("/locations")]
        public async Task<ActionResult> CreateLocation([FromBody] PointRequest request)
        {
            long? id;
            Auth.AuthRes auth = Auth.Authorization(HttpContext.Request.Headers["Authorization"], db, out id);
            if (auth != Auth.AuthRes.Ok) return StatusCode(401);
            var latitude = request.Latitude;
            var longitude = request.Longitude;
            if (latitude == null || latitude < -90 || latitude > 90 || longitude == null || longitude < -180 || longitude > 180) return StatusCode(400);

            if (db.LocationPoints.Where(x => x.Latitude == latitude && x.Longitude == longitude).ToList().Count != 0) return StatusCode(409);
            var point = new LocationPoint { Latitude = latitude, Longitude = longitude };
            db.LocationPoints.Add(new LocationPoint { Latitude = latitude, Longitude = longitude });
            db.SaveChanges();
            point = db.LocationPoints.Where(x => x.Longitude == longitude && x.Latitude == latitude).FirstOrDefault();
            return new ObjectResult(point) { StatusCode = 201 };
        }

        [HttpPut("/locations/{pointId}")]
        public async Task<ActionResult> UpdateLocation(long pointId, [FromBody] PointRequest request)
        {
            long? id;
            Auth.AuthRes auth = Auth.Authorization(HttpContext.Request.Headers["Authorization"], db, out id);
            if (auth != Auth.AuthRes.Ok) return StatusCode(401);
            var latitude = request.Latitude;
            var longitude = request.Longitude;
            if (pointId == null || pointId <= 0) return StatusCode(400);
            
            var point = db.LocationPoints.Find(pointId);
            if (point == null) return StatusCode(404);
            if (latitude == null | latitude < -90 | latitude > 90 | longitude == null | longitude < -180 | longitude > 180) return StatusCode(400);
            if (db.LocationPoints.Where(x => x.Latitude == latitude && x.Longitude == longitude).Any()) return StatusCode(409);
            point.Longitude = longitude;
            point.Latitude = latitude;
            return new ObjectResult(point) { StatusCode = 200 };
        }

        [HttpDelete("/locations/{pointId}")]
        public async Task<ActionResult> DeleteLocation( long pointId)
        {
            long? id;
            Auth.AuthRes auth = Auth.Authorization(HttpContext.Request.Headers["Authorization"], db, out id);
            if (auth != Auth.AuthRes.Ok) return StatusCode(401);

            if (pointId == null | pointId <= 0 | db.AnimalVisitedLocations.Where(x => x.LocationPointId == pointId).Any() || db.Animals.Where(x => x.ChippingLocationId == pointId).Any()) return StatusCode(400);
            var point = db.LocationPoints.Find(pointId);
            if (point == null) return StatusCode(404);
            db.LocationPoints.Where(x => x.Id == pointId).ExecuteDelete();
            return StatusCode(200);
        }

    }
}
