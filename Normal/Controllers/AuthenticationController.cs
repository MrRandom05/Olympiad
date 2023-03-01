using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Normal.Controllers
{
    [ApiController]
    public class AuthenticationController : Controller
    {
        private readonly ContextClass db;
        public AuthenticationController(ContextClass db)
        {
            this.db = db;
        }
        public enum AuthRes
        {
            Ok,
            Not,
            Error
        }

        [HttpPost]
        [Route("/registration")]
        public async Task<ActionResult> Registrate([FromQuery] string? firstName, [FromQuery] string? lastName, [FromQuery] string? email, [FromQuery] string? password)
        {
            AuthRes auth = Authorization(HttpContext.Request.Headers["Authorization"], db, out _);
            if (auth == AuthRes.Ok) return StatusCode(403);

            var acc = new Account { Password = password, Email = email, FirstName = firstName, LastName = lastName };
            if (string.IsNullOrEmpty(acc.Email) | string.IsNullOrEmpty(acc.FirstName) | string.IsNullOrEmpty(acc.LastName) | string.IsNullOrEmpty(acc.Password))
            {
                return StatusCode(400);
            }
            else if (db.Accounts.Any(x => x.Email.Contains(acc.Email)))
            {
                return StatusCode(409);
            }
            if (acc.Email.Contains("@"))
            {
                db.Accounts.Add(acc);
                db.SaveChanges();
                acc.Password = null;
                acc = db.Accounts.Where(x => x.Email == email).FirstOrDefault();
            }
            else return StatusCode(400);
            return new ObjectResult(acc) { StatusCode = 201 };
        }


        public static AuthRes Authorization(string? logpass, ContextClass db, out long? id)
        {

            id = null;
            if (logpass == null) return AuthRes.Not;
            string[] firstSplited = logpass.Split(' ');
            if (firstSplited[0] != "Basic") return AuthRes.Error;
            var splited = Encoding.UTF8.GetString(Convert.FromBase64String(firstSplited[1])).Split(':');

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
