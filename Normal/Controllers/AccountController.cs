using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace Normal.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    public class AccountController : Controller
    {
        private readonly ContextClass context;
        static readonly HttpClient client = new HttpClient();

        public AccountController(ContextClass context)
        {
            this.context = context;
        }
        /*[HttpPost]
        public async Task Registration(HttpRequest request)
        {
            var acc = request.ReadFromJsonAsync<Account>();
            if(acc.Result.Email.Contains("@") & !acc.Result.Email.IsNullOrEmpty() & !context.Accounts.Any(x => x.Email.Contains(acc.Result.Email)))
            {
                var pers = new Account
                {
                    FirstName = acc.Result.FirstName,
                    LastName = acc.Result.LastName,
                    Email = acc.Result.Email,
                    Password = acc.Result.Password
                };
                context.Accounts.AddRange(pers);
                context.SaveChanges();
                await response.WriteAsJsonAsync(acc);
                
                
            }    
            
        }*/
    }
}
