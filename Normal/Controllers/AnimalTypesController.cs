using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Normal.Models;
using static Normal.Controllers.AuthenticationController;

namespace Normal.Controllers
{
    [ApiController]
    public class AnimalTypesController : ControllerBase
    {
        private readonly ContextClass db;
        public AnimalTypesController(ContextClass db)
        {
            this.db = db;
        }

        [HttpGet("/animals/types/{typeId}")]
        public async Task<ActionResult> GetType(long typeId)
        {
            long? id;
            Auth.AuthRes auth = Auth.Authorization(HttpContext.Request.Headers["Authorization"], db, out id);
            if (auth == Auth.AuthRes.Error) return StatusCode(401);

            if (typeId == null | typeId <= 0) return StatusCode(400);
            var type = db.AnimalTypes.Find(typeId);
            if (type == null) return StatusCode(404);
            return new ObjectResult(type) { StatusCode = 200 };
        }

        [HttpPost("/animals/types")]
        public async Task<ActionResult> CreateType([FromBody] TypeRequest request)
        {
            var type = request.Type;
            long? id;
            Auth.AuthRes auth = Auth.Authorization(HttpContext.Request.Headers["Authorization"], db, out id);
            if (auth != Auth.AuthRes.Ok) return StatusCode(401);
            if (!type.All(c => Char.IsLetterOrDigit(c) || c == '-')) return StatusCode(400); 
            if (type.IsNullOrEmpty()) return StatusCode(400);
            if (db.AnimalTypes.Where(x => x.Type.ToLower() == type.ToLower()).Any()) return StatusCode(409);
            db.AnimalTypes.Add(new AnimalType { Type = type });
            db.SaveChanges();
            var aType = db.AnimalTypes.Where(x => x.Type.ToLower() == type.ToLower()).FirstOrDefault();
            return new ObjectResult(aType) { StatusCode = 201 };
        }

        [HttpPut("/animals/types/{typeId}")]
        public async Task<ActionResult> UpdateType( long typeId, [FromBody] TypeRequest request)
        {
            var type = request.Type;
            long? id;
            Auth.AuthRes auth = Auth.Authorization(HttpContext.Request.Headers["Authorization"], db, out id);
            if (auth != Auth.AuthRes.Ok) return StatusCode(401);

            if (typeId == null || typeId <= 0 || type.IsNullOrEmpty() || type.Trim().Length == 0) return StatusCode(400);
            var aType = db.AnimalTypes.Find(typeId);
            if (aType == null) return StatusCode(404);
            if (db.AnimalTypes.Where(x => x.Type == type).Any()) return StatusCode(409);
            aType.Type = type;
            db.Update(aType);
            db.SaveChanges();
            return new ObjectResult(aType) { StatusCode = 200 };
        }

        [HttpDelete("/animals/types/{typeId}")]
        public async Task<ActionResult> DeleteType(long typeId)
        {
            long? id;
            Auth.AuthRes auth = Auth.Authorization(HttpContext.Request.Headers["Authorization"], db, out id);
            if (auth != Auth.AuthRes.Ok) return StatusCode(401);

            if (typeId == null | typeId <= 0 | db.AnimalTypes.Where(x => x.Id == typeId & x.Animals.Any()).Any()) return StatusCode(400);
            var aType = db.AnimalTypes.Find(typeId);
            if (aType == null) return StatusCode(404);
            db.AnimalTypes.Where(x => x.Id == typeId).ExecuteDelete();
            return StatusCode(200);
        }

    }
}
