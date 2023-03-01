using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Normal.Controllers
{
    [ApiController]
    [Route("/animals/types")]
    public class AnimalTypesController : Controller
    {
        private readonly ContextClass db;
        public AnimalTypesController(ContextClass db)
        {
            this.db = db;
        }

        [HttpGet("/{typeId}")]
        public async Task<ActionResult> GetType(long typeId)
        {
            if (typeId == null | typeId <= 0) return StatusCode(400);
            var point = db.LocationPoints.Find(typeId);
            if (point == null) return StatusCode(404);
            return Json(point);
        }

        [HttpPost]
        public async Task<ActionResult> CreateType([FromQuery] string type)
        {
            if (string.IsNullOrEmpty(type)) return StatusCode(400);
            if (db.AnimalTypes.Where(x => x.Type.ToLower() == type.ToLower()).Any()) return StatusCode(409);
            db.AnimalTypes.Add(new AnimalType { Type = type });
            db.SaveChanges();
            var aType = db.AnimalTypes.Where(x => x.Type.ToLower() == type.ToLower()).ToList()[0];
            return new ObjectResult(aType) { StatusCode = 201 };
        }

        [HttpPut("/{typeId}")]
        public async Task<ActionResult> UpdateType([FromQuery] long typeId, [FromQuery] string type)
        {
            if (typeId == null | typeId <= 0 | string.IsNullOrEmpty(type)) return StatusCode(400);
            var aType = db.AnimalTypes.Find(typeId);
            if (aType == null) return StatusCode(404);
            aType.Type = type;
            db.Update(aType);
            db.SaveChanges();
            return Json(aType);
        }

        [HttpDelete("/{typeId}")]
        public async Task<ActionResult> DeleteType([FromQuery] long typeId)
        {
            if (typeId == null | typeId <= 0 | db.AnimalTypes.Where(x => x.Id == typeId & x.Animals.Any()).Any()) return StatusCode(400);
            var aType = db.AnimalTypes.Find(typeId);
            if (aType == null) return StatusCode(404);
            db.AnimalTypes.Where(x => x.Id == typeId).ExecuteDelete();
            return StatusCode(200);
        }

    }
}
