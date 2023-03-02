using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Normal.Controllers.AuthenticationController;

namespace Normal.Controllers
{
    [ApiController]
    public class AnimalTypesController : Controller
    {
        private readonly ContextClass db;
        public AnimalTypesController(ContextClass db)
        {
            this.db = db;
        }

        [HttpGet("/animals/types/{typeId}")]
        public async Task<ActionResult> GetType(long typeId)
        {
            AuthRes auth = Authorization(HttpContext.Request.Headers["Authorization"], db, out _);
            if (auth == AuthRes.Error) return StatusCode(401);

            if (typeId == null | typeId <= 0) return StatusCode(400);
            var point = db.LocationPoints.Find(typeId);
            if (point == null) return StatusCode(404);
            return Json(point);
        }

        [HttpPost("/animals/types")]
        public async Task<ActionResult> CreateType([FromQuery] string type)
        {
            AuthRes auth = Authorization(HttpContext.Request.Headers["Authorization"], db, out _);
            if (auth != AuthRes.Ok) return StatusCode(401);

            if (string.IsNullOrEmpty(type)) return StatusCode(400);
            if (db.AnimalTypes.Where(x => x.Type.ToLower() == type.ToLower()).Any()) return StatusCode(409);
            db.AnimalTypes.Add(new AnimalType { Type = type });
            db.SaveChanges();
            var aType = db.AnimalTypes.Where(x => x.Type.ToLower() == type.ToLower()).ToList()[0];
            return new ObjectResult(aType) { StatusCode = 201 };
        }

        [HttpPut("/animals/types/{typeId}")]
        public async Task<ActionResult> UpdateType([FromQuery] long typeId, [FromQuery] string type)
        {
            AuthRes auth = Authorization(HttpContext.Request.Headers["Authorization"], db, out _);
            if (auth != AuthRes.Ok) return StatusCode(401);

            if (typeId == null | typeId <= 0 | string.IsNullOrEmpty(type)) return StatusCode(400);
            var aType = db.AnimalTypes.Find(typeId);
            if (aType == null) return StatusCode(404);
            aType.Type = type;
            db.Update(aType);
            db.SaveChanges();
            return Json(aType);
        }

        [HttpDelete("/animals/types/{typeId}")]
        public async Task<ActionResult> DeleteType([FromQuery] long typeId)
        {
            AuthRes auth = Authorization(HttpContext.Request.Headers["Authorization"], db, out _);
            if (auth != AuthRes.Ok) return StatusCode(401);

            if (typeId == null | typeId <= 0 | db.AnimalTypes.Where(x => x.Id == typeId & x.Animals.Any()).Any()) return StatusCode(400);
            var aType = db.AnimalTypes.Find(typeId);
            if (aType == null) return StatusCode(404);
            db.AnimalTypes.Where(x => x.Id == typeId).ExecuteDelete();
            return StatusCode(200);
        }

    }
}
