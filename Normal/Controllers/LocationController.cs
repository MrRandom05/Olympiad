using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using static Normal.Controllers.AuthenticationController;

namespace Normal.Controllers
{
    [ApiController]
    [Route("/locations")]
    public class LocationController : Controller
    {
        private readonly ContextClass db;
        public LocationController(ContextClass db) 
        {
            this.db = db;
        }

        [HttpGet("/{pointId}")]
        public async Task<ActionResult> GetLocation(long pointId)
        {
            AuthRes auth = Authorization(HttpContext.Request.Headers["Authorization"], db, out _);
            if (auth == AuthRes.Error) return StatusCode(401);

            if (pointId == null | pointId <= 0) return StatusCode(400);
            var point = db.LocationPoints.Find(pointId);
            if (point == null) return StatusCode(404);
            else return Json(point);
        }

        [HttpPost]
        public async Task<ActionResult> CreateLocation([FromQuery] double latitude, [FromQuery] double longitude)
        {
            AuthRes auth = Authorization(HttpContext.Request.Headers["Authorization"], db, out _);
            if (auth != AuthRes.Ok) return StatusCode(401);

            if (latitude == null | latitude < -90 | latitude > 90 | longitude == null | longitude < -180 | longitude > 180) return StatusCode(400);
            if (db.LocationPoints.Where(x => x.Latitude == latitude | x.Longitude == longitude).Any()) return StatusCode(409);
            var point = new LocationPoint { Latitude = latitude, Longitude = longitude };
            db.LocationPoints.Add(new LocationPoint { Latitude = latitude, Longitude = longitude });
            db.SaveChanges();
            point = db.LocationPoints.Where(x => x.Longitude == longitude & x.Latitude == latitude).FirstOrDefault();
            return new ObjectResult(point) { StatusCode = 201 };
        }

        [HttpPut("/{poitId}")]
        public async Task<ActionResult> UpdateLocation([FromQuery] long pointId, [FromQuery] double longitude, [FromQuery] double latitude)
        {
            AuthRes auth = Authorization(HttpContext.Request.Headers["Authorization"], db, out _);
            if (auth != AuthRes.Ok) return StatusCode(401);

            var point = db.LocationPoints.Find(pointId);
            if (latitude == null | latitude < -90 | latitude > 90 | longitude == null | longitude < -180 | longitude > 180) return StatusCode(400);
            if (db.LocationPoints.Where(x => x.Latitude == latitude | x.Longitude == longitude).Any()) return StatusCode(409);
            point.Longitude = longitude;
            point.Latitude = latitude;
            return Json(point);
        }

        [HttpDelete("/{pointId}")]
        public async Task<ActionResult> DeleteLocation([FromQuery] long pointId)
        {
            AuthRes auth = Authorization(HttpContext.Request.Headers["Authorization"], db, out _);
            if (auth != AuthRes.Ok) return StatusCode(401);

            if (pointId == null | pointId <= 0 | db.AnimalVisitedLocations.Where(x => x.LocationPointId == pointId).Any()) return StatusCode(400);
            var point = db.LocationPoints.Find(pointId);
            if (point == null) return StatusCode(404);
            db.LocationPoints.Where(x => x.Id == pointId).ExecuteDelete();
            return StatusCode(200);
        }

    }
}
