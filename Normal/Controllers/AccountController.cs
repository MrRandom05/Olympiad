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
using Normal.Models;
using System.ComponentModel.DataAnnotations;

namespace Normal.Controllers
{
    [ApiController]
    [Route("/accounts/")]
    public class AccountController : ControllerBase
    {
        private readonly ContextClass db;

        public AccountController(ContextClass db)
        {
            this.db = db;
        }

        

        [HttpGet("{accountId}")]
        public async Task<ActionResult<Account>> Get(int accountId)
           {
            long? id;
            Auth.AuthRes auth = Auth.Authorization(HttpContext.Request.Headers["Authorization"], db, out id);
            if (auth == Auth.AuthRes.Error) return StatusCode(401);
            if (accountId == null | accountId <= 0)  return new StatusCodeResult(StatusCodes.Status400BadRequest);
            var acc = db.Accounts.Where(x => x.Id == accountId).FirstOrDefault();
            if (acc == null) return new StatusCodeResult(StatusCodes.Status404NotFound);
            acc.Password = null;
            return new ObjectResult(acc) { StatusCode = 200 };
           }

        [HttpGet("search")]
        public async Task<ActionResult<Account>> Search([FromQuery] string? firstName, [FromQuery] string? lastName, [FromQuery] string? email, [FromQuery] int? from, [FromQuery] int? size)
        {
            long? id;
            Auth.AuthRes auth = Auth.Authorization(HttpContext.Request.Headers["Authorization"], db, out id);
            if (auth == Auth.AuthRes.Error) return StatusCode(401);

            IQueryable<Account> res = db.Accounts.AsQueryable();
            if (from < 0 | size <= 0) return new StatusCodeResult(StatusCodes.Status400BadRequest);
            if (from == null) from = 0;
            if (size == null) size = 10;
            if (firstName != null)
            {
                res = res.Where(x => x.FirstName.Contains(firstName));
            }
            if (lastName != null)
            {
                res = res.Where(x => x.LastName.Contains(lastName));
            }
            if (email != null)
            {
                res = res.Where(x => x.Email.Contains(email));
            }
            if (!res.Any()) return new ObjectResult(res) { StatusCode = 200 };
            res = res.Skip((int)from);
            res = res.Take((int)size);
            res = res.OrderBy(x => x.Id);
            
            var list = res.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Password = null;
            }
            return new ObjectResult(list) { StatusCode = 200 };
        }

        [HttpPut("{accountId}")]
        public async Task<ActionResult> UpdateAccount(int accountId, [FromBody] UpdateAccountRequest request)
        {
            long? id;
            Auth.AuthRes auth = Auth.Authorization(HttpContext.Request.Headers["Authorization"], db, out id);
            if (auth == Auth.AuthRes.Error || auth == Auth.AuthRes.Not) return StatusCode(401);
            var firstName = request.FirstName;
            var lastName = request.LastName;
            var email = request.Email;
            var password = request.Password;
            if (accountId == null || accountId <= 0 || firstName.IsNullOrEmpty() || lastName.IsNullOrEmpty() || email.IsNullOrEmpty() || password.IsNullOrEmpty() || firstName.Trim().Length == 0 || lastName.Trim().Length == 0 || email.Trim().Length == 0 || password.Trim().Length == 0) return new StatusCodeResult(StatusCodes.Status400BadRequest);
            var em = new EmailAddressAttribute();
            if (em.IsValid(email) == false) return StatusCode(400);
            var acc = db.Accounts.Where(x => x.Id == accountId);
            Account pers = acc.FirstOrDefault();
            if (pers == null || acc == null) return StatusCode(403);
                if (em.IsValid(email) == true && !db.Accounts.Where(x => x.Email == email).Any())
                    pers.Email = email;
                    pers.Password = password;
                    pers.FirstName = firstName;
                    pers.LastName = lastName;
                    db.Accounts.Update(pers);
                    db.SaveChanges();

            if (pers == null || id != pers.Id)
                return new StatusCodeResult(StatusCodes.Status403Forbidden);
            pers.Password = null;
            return new ObjectResult(pers) { StatusCode = 200 };
        }

        [HttpDelete("{accountId}")]
        public async Task<ActionResult> DeleteAccount(int accountId)
        {
            long? id;
            Auth.AuthRes auth = Auth.Authorization(HttpContext.Request.Headers["Authorization"], db, out id);
            if (auth != Auth.AuthRes.Ok) return StatusCode(401);

            if (accountId == null | accountId <= 0 | db.Animals.Where(x => x.ChipperId == accountId).Any()) return new StatusCodeResult(StatusCodes.Status400BadRequest);
            if (db.Accounts.Where(x => x.Id == accountId).FirstOrDefault() == null || db.Accounts.Where(x => x.Id == accountId).FirstOrDefault().Id != id) return new StatusCodeResult(StatusCodes.Status403Forbidden);
            db.Accounts.Where(x => x.Id == accountId).ExecuteDelete();
            db.SaveChanges();
            return new StatusCodeResult(StatusCodes.Status200OK);
        }



    }

}
