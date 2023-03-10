using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Normal.Models;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;

namespace Normal.Controllers
{
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly ContextClass db;
        public AuthenticationController(ContextClass db)
        {
            this.db = db;
        }
        

        [HttpPost]
        [Route("/registration")]
        public async Task<ActionResult> Registrate([FromBody] RegRequest request)
        {
            long? id;
            Auth.AuthRes auth = Auth.Authorization(HttpContext.Request.Headers["Authorization"], db, out id);
            if (auth == Auth.AuthRes.Ok) return StatusCode(403);
            var firstName = request.FirstName;
            var lastName = request.LastName;
            var email = request.Email;
            var password = request.Password;
            if (email.IsNullOrEmpty() || firstName.IsNullOrEmpty() || lastName.IsNullOrEmpty() || password.IsNullOrEmpty() || firstName.Trim().Length == 0 || lastName.Trim().Length == 0 || email.Trim().Length == 0 || password.Trim().Length == 0)
            {
                return StatusCode(400);
            }
            var em = new EmailAddressAttribute();
            if (em.IsValid(email) == false) return StatusCode(400);
            var acc = new Account { Password = password, Email = email, FirstName = firstName, LastName = lastName };
            if (db.Accounts.Where(x => x.Email == email).Any())
            {
                return StatusCode(409);
            }
            db.Accounts.Add(acc);
            db.SaveChanges();
            acc.Password = null;
            acc = db.Accounts.Where(x => x.Email == email).FirstOrDefault();
            return new ObjectResult(acc) { StatusCode = 201 };
        }
    }

    public class Auth
    {
        public enum AuthRes
        {
            Ok,
            Not,
            Error
        }

        public static AuthRes Authorization(string? logpass, ContextClass db, out long? id)
        {

            id = null;
            if (logpass == null) return AuthRes.Not;
            string[] firstSplited = logpass.Split(' ');
            if (firstSplited[0] != "Basic") return AuthRes.Error;
            string[]? splited;
            try
            {
                var converted = Encoding.UTF8.GetString(Convert.FromBase64String(firstSplited[1]));
                splited = converted.Split(':');
            }
            catch (FormatException ex)
            {
                return AuthRes.Error;
            }
            Account? account = db.Accounts.Where(x => x.Email == splited[0] && x.Password == splited[1]).FirstOrDefault();
            if (account == null) return AuthRes.Error;
            else
            {
                id = account.Id;
                return AuthRes.Ok;
            }
        }
        
    }
}
