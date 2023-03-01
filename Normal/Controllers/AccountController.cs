using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using System.Security.Claims;
using System.Linq;
using Microsoft.Identity.Client;
using System.Text;
using Microsoft.EntityFrameworkCore;
using static Normal.Controllers.AuthenticationController;

namespace Normal.Controllers
{
    [ApiController]
    [Route("/accounts/")]
    public class AccountController : Controller
    {
        private readonly ContextClass db;

        public AccountController(ContextClass db)
        {
            this.db = db;
        }

        

        [HttpGet("{accountId}")]
        public async Task<ActionResult<Account>> Get(long accountId)
           {
            AuthRes auth = Authorization(HttpContext.Request.Headers["Authorization"], db, out _);
            if (auth == AuthRes.Error) return StatusCode(401);
            if (accountId == null | accountId < 0)  return new StatusCodeResult(StatusCodes.Status400BadRequest);
            var acc = db.Accounts.FirstOrDefault(x => x.Id == accountId);
            if (acc == null) return new StatusCodeResult(StatusCodes.Status404NotFound);
            acc.Password = null;
            return Json(acc);
           }

        [HttpGet("search")]
        public async Task<ActionResult<Account>> Search([FromQuery] string? firstName, [FromQuery] string? lastName, [FromQuery] string? email, [FromQuery] int? from, [FromQuery] int? size)
        {
            IQueryable<Account> res = db.Accounts.AsQueryable();
            if (from < 0 | size <= 0) return new StatusCodeResult(StatusCodes.Status400BadRequest);
            if (from == null) from = 0;
            if (size == null) size = 10;
            if (firstName != null)
            {
                res = db.Accounts.Where(x => x.FirstName.ToLower().Contains(firstName.ToLower()));
            }
            if (lastName != null)
            {
                res = db.Accounts.Where(x => x.LastName.ToLower().Contains(lastName.ToLower()));
            }
            if (email != null)
            {
                res = db.Accounts.Where(x => x.Email.ToLower().Contains(email.ToLower()));
            }
            if (!res.Any()) return Json(res);
            res = res.Skip((int)from);
            res = res.Take((int)size);
            res = res.OrderBy(x => x.Id);

            var list = res.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Password = null;
            }
            return Json(list);
        }

        [HttpPut("{accountId}")]
        public async Task<ActionResult> UpdateAccount(int accountId,[FromQuery]string firstName, [FromQuery]string lastName, [FromQuery]string email, [FromQuery] string password)
        {
            long? id;
            AuthRes auth = Authorization(HttpContext.Request.Headers["Authorization"], db, out id);
            if (auth == AuthRes.Error || auth == AuthRes.Not) return StatusCode(401);
            var acc = db.Accounts.Where(x => x.Id == accountId);
            Account pers = acc.FirstOrDefault();
            if (accountId == null | accountId <= 0 | string.IsNullOrEmpty(firstName) | string.IsNullOrEmpty(lastName) | string.IsNullOrEmpty(email) | string.IsNullOrEmpty(password)) return new StatusCodeResult(StatusCodes.Status400BadRequest);
            if (pers.Password == password)
            {
                if (email.Contains("@") & !db.Accounts.Where(x => x.Email == email).Any())
                    pers.Email = email;
                pers.FirstName = firstName;
                pers.LastName = lastName;
                db.Accounts.Update(pers);
                db.SaveChanges();

            }
            if (pers == null || id != pers.Id)
                return new StatusCodeResult(StatusCodes.Status403Forbidden);
            pers.Password = null;
            return Json(pers);
        }

        [HttpDelete("{accountId}")]
        public async Task<ActionResult> DeleteAccount(int accountId)
        {
            long? id;
            AuthRes auth = Authorization(HttpContext.Request.Headers["Authorization"], db, out id);
            if (auth != AuthRes.Ok) return StatusCode(401);

            if (accountId == null | accountId <= 0 | db.Animals.Where(x => x.ChipperId == accountId).Any()) return new StatusCodeResult(StatusCodes.Status400BadRequest);
            if (db.Accounts.Where(x => x.Id == accountId).FirstOrDefault() == null || db.Accounts.Where(x => x.Id == accountId).FirstOrDefault().Id != id) return new StatusCodeResult(StatusCodes.Status403Forbidden);
            db.Accounts.Where(x => x.Id == accountId).ExecuteDelete();
            db.SaveChanges();
            return new StatusCodeResult(StatusCodes.Status200OK);
        }



    }
}
