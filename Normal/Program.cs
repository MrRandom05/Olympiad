using Microsoft.Data;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Net;
using Polly;
using Normal.Controllers;
using System.Runtime.Intrinsics.X86;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Linq;
using Microsoft.IdentityModel.Tokens;
using Azure.Core;

namespace Normal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ContextClass>(options => options.UseSqlServer("Data Source=sql-server-db;Initial Catalog=Base;TrustServerCertificate=true;user id=sa;password=123123Qw*"));

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
           // builder.Services.AddSwaggerGen();

            var app = builder.Build();

           // if (app.Environment.IsDevelopment())
           //{
           //     app.UseSwagger();
           //     app.UseSwaggerUI();
          //  }
            app.UseHttpsRedirection();
            //app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}