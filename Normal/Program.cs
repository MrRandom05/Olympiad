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

            builder.Services.AddDbContext<ContextClass>(options => options.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Base;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"));

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

         /*   app.MapPost("/registration", async ([FromQuery] string? firstName, [FromQuery] string? lastName, [FromQuery] string? email, [FromQuery] string? password, ContextClass db) =>
            {
                var acc = new Account { Password= password, Email = email, FirstName = firstName, LastName = lastName };
                if (string.IsNullOrEmpty(acc.Email) | string.IsNullOrEmpty(acc.FirstName) | string.IsNullOrEmpty(acc.LastName) | string.IsNullOrEmpty(acc.Password))
                {
                    return Results.StatusCode(400);
                }
                else if (db.Accounts.Any(x => x.Email.Contains(acc.Email)))
                {
                    return Results.StatusCode(409);
                }
                if (acc.Email.Contains("@"))
                {
                    db.Accounts.Add(acc);
                    db.SaveChanges();
                    acc.Password = null;
                    acc = db.Accounts.Where(x => x.Email == email).ToList()[0];
                }
                return Results.Json(acc);
            });

             app.MapGet("/accounts/{accountId}", async (int accountId, ContextClass db) =>
             {
                 if (accountId == null | accountId < 0) return Results.StatusCode(400);
                 var acc = await db.Accounts.FirstOrDefaultAsync(x => x.Id == accountId);
                 if (acc == null) return Results.StatusCode(404);
                 return Results.Json(acc);
             });

            app.MapGet("/accounts/search", async ([FromQuery]string? firstName, [FromQuery]string? lastName, [FromQuery]string? email, [FromQuery]int? from, [FromQuery] int? size, ContextClass db) =>
            {
                IQueryable<Account> res = db.Accounts.AsQueryable();
                if (from < 0 | size <= 0) return Results.StatusCode(400);
                if (from == null) from = 0;
                if (size == null)  size = 10;
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
                if (!res.Any()) return Results.Json(res);
                res = res.Skip((int)from);
                res = res.Take((int)size);
                res = res.OrderBy(x => x.Id);

                var list = res.ToList();
                for(int i = 0; i < list.Count; i++)
                {
                    list[i].Password = null;
                }
                return Results.Json(list);
            });

            app.MapPut("/accounts/{accountId}", async ([FromQuery] int accountId, [FromQuery]string firstName, [FromQuery]string lastName, [FromQuery]string email, [FromQuery] string password, ContextClass db) =>
            {
                var acc = db.Accounts.Where(x => x.Id == accountId);
                Account pers = new Account();
                foreach (var x in acc)
                {
                    pers = new Account { Id = accountId, Email = x.Email, FirstName = x.FirstName, LastName = x.LastName, Password = x.Password };
                }
                if (accountId == null | accountId <= 0 | string.IsNullOrEmpty(firstName) | string.IsNullOrEmpty(lastName) | string.IsNullOrEmpty(email) | string.IsNullOrEmpty(password)) return Results.StatusCode(400);
                if (pers.Password == password)
                {
                    if(email.Contains("@") & db.Accounts.Where(x => x.Email == email) == acc)
                    pers.Email = email;
                    pers.FirstName = firstName;
                    pers.LastName = lastName;
                    db.Accounts.Update(pers);
                    db.SaveChanges();

                }
                else
                    return Results.StatusCode(403);
                return Results.Json(pers);
            });

            app.MapDelete("/accounts/{accountId}", async ([FromQuery] int accountId, ContextClass db) =>
            {
                if (accountId == null | accountId <= 0 | db.Animals.Where(x => x.ChipperId == accountId).Any()) return Results.StatusCode(400);
                if (db.Accounts.Where(x => x.Id == accountId) == null) return Results.StatusCode(403);
                db.Accounts.Where(x => x.Id == accountId).ExecuteDelete();
                db.SaveChanges();
                return Results.StatusCode(200);
            });

            app.MapGet("/locations/{pointId}", async ( long pointId, ContextClass db) =>
            {
                if (pointId == null | pointId <= 0) return Results.StatusCode(400);
                var point = db.LocationPoints.Find(pointId);
                if (point == null) return Results.StatusCode(404);
                else return Results.Json(point);
            });

            app.MapPost("/locations", async ([FromQuery] double latitude, [FromQuery] double longitude, ContextClass db) =>
            {
                if (latitude == null | latitude < -90 | latitude > 90 | longitude == null | longitude < -180 | longitude > 180) return Results.StatusCode(400);
                if (db.LocationPoints.Where(x => x.Latitude == latitude | x.Longitude == longitude).Any()) return Results.StatusCode(409);
                var point = new LocationPoint { Latitude = latitude, Longitude = longitude };
                db.LocationPoints.Add(new LocationPoint { Latitude = latitude,Longitude = longitude });
                db.SaveChanges();
                point = db.LocationPoints.Where(x => x.Longitude == longitude & x.Latitude == latitude).ToList()[0];
                return Results.Json(point);
            });

            app.MapPut("/locations/{pointId}", async ([FromQuery] long pointId, [FromQuery] double longitude, [FromQuery] double latitude, ContextClass db) =>
            {
                var point = db.LocationPoints.Find(pointId);
                if (latitude == null | latitude < -90 | latitude > 90 | longitude == null | longitude < -180 | longitude > 180) return Results.StatusCode(400);
                if (db.LocationPoints.Where(x => x.Latitude == latitude | x.Longitude == longitude).Any()) return Results.StatusCode(409);
                point.Longitude = longitude;
                point.Latitude = latitude;
                return Results.Json(point);
            });

            app.MapDelete("/locations/{pointId}", async ([FromQuery] long pointId, ContextClass db) =>
            {
                if (pointId == null | pointId <= 0 | db.AnimalVisitedLocations.Where(x => x.LocationPointId == pointId).Any()) return Results.StatusCode(400);
                var point = db.LocationPoints.Find(pointId);
                if (point == null) return Results.StatusCode(404);
                db.LocationPoints.Where(x => x.Id == pointId).ExecuteDelete();
                return Results.StatusCode(200);
            });

            app.MapGet("/animals/types/{typeId}", async ( long typeId, ContextClass db) =>
            {
                if (typeId == null | typeId <= 0) return Results.StatusCode(400);
                var point = db.LocationPoints.Find(typeId);
                if (point == null) return Results.StatusCode(404);
                return Results.Json(point);
            });

            app.MapPost("/animals/types", async ([FromQuery] string type, ContextClass db) =>
            {
                if (string.IsNullOrEmpty(type)) return Results.StatusCode(400);
                if (db.AnimalTypes.Where(x => x.Type.ToLower() == type.ToLower()).Any()) return Results.StatusCode(409);
                db.AnimalTypes.Add(new AnimalType { Type = type });
                db.SaveChanges();
                var aType = db.AnimalTypes.Where(x => x.Type.ToLower() == type.ToLower()).ToList()[0];
                return Results.Json(aType);
            });

            app.MapPut("/animals/types/{typeId}", async ([FromQuery] long typeId, [FromQuery]string type, ContextClass db) =>
            {
                if (typeId == null | typeId <= 0 | string.IsNullOrEmpty(type)) return Results.StatusCode(400);
                var aType = db.AnimalTypes.Find(typeId);
                if (aType == null) return Results.StatusCode(404);
                aType.Type = type;
                db.Update(aType);
                db.SaveChanges();
                return Results.Json(aType);
            });

            app.MapDelete("/animals/types/{typeId}", async ([FromQuery] long typeId, ContextClass db) =>
            {
                if (typeId == null | typeId <= 0 | db.AnimalTypes.Where(x => x.Id == typeId & x.Animals.Any()).Any()) return Results.StatusCode(400);
                var aType = db.AnimalTypes.Find(typeId);
                if (aType == null) return Results.StatusCode(404);
                db.AnimalTypes.Where(x => x.Id == typeId).ExecuteDelete();
                return Results.StatusCode(200);
            });

            app.MapGet("/animals/{animalId}", async ( long animalId, ContextClass db) =>
            {
                if (animalId == null | animalId <= 0) return Results.StatusCode(400);
                var animal = db.Animals.Where(x => x.Id == animalId).Include(x => x.AnimalTypes).Include(x => x.VisitedLocations).FirstOrDefault();
                if (animal == null) return Results.StatusCode(404);

                return Results.Json(animal);
            });

            app.MapGet("/animals/search", async ([FromQuery]DateTime? startDateTime, [FromQuery]DateTime? endDateTime, [FromQuery]int? chipperId, [FromQuery]long? chippingLocationId, [FromQuery]string? lifeStatus, [FromQuery]string? gender, [FromQuery]int? from, [FromQuery]int? size, ContextClass db) =>
            {
                if (from < 0 | size <= 0 | chipperId <= 0 | chippingLocationId <= 0 ) return Results.StatusCode(400);
                //if (startDateTime.ToString() != string.Format($"yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffzzz") | endDateTime.ToString() != string.Format($"yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffzzz")) return Results.StatusCode(400);
                if (lifeStatus != null)
                {
                    if (lifeStatus.ToLower() != "alive" & lifeStatus.ToLower() != "dead") return Results.StatusCode(400);
                }
                if (gender != null)
                {
                    if (gender.ToLower() != "male" & gender.ToLower() != "female" & gender.ToLower() != "other") return Results.StatusCode(400);
                }
                IQueryable<Animal> animals = db.Animals.AsQueryable();
                if (startDateTime != null)
                    animals = animals.Where(x => x.ChippingDateTime > startDateTime);
                if (endDateTime != null)
                    animals = animals.Where(x => x.ChippingDateTime < endDateTime);
                if (chipperId != null)
                    animals = animals.Where(x => x.ChipperId == chipperId);
                if (chippingLocationId != null)
                    animals = animals.Where(x => x.ChippingLocationId == chippingLocationId);
                if (!string.IsNullOrEmpty(lifeStatus))
                    animals = animals.Where(x => x.LifeStatus == lifeStatus);
                if (!string.IsNullOrEmpty(gender))
                    animals = animals.Where(x => x.Gender == gender);
                if (from == null)
                    from = 0;
                if (size == null)
                    size = 10;

                if (!animals.Any()) return Results.Json(animals);

                animals = animals.Skip((int)from);
                animals = animals.Take((int)size);
                animals = animals.OrderBy(x => x.Id);
                animals.Include(x => x.AnimalTypes).Include(x => x.VisitedLocations);
                return Results.Json(animals);

            });

            app.MapPost("/animals", async ([FromQuery] long[] animalTypes, [FromQuery]float weight, [FromQuery]float length, [FromQuery]float height, [FromQuery]string gender, [FromQuery]int chipperId, [FromQuery]long chippingLocationId, ContextClass db) =>
            {
                if (animalTypes == null | animalTypes.Length <= 0 | weight == null | weight <= 0 | length == null | length <= 0 | height == null | height <= 0 |
                gender == null | gender.ToLower() != "male" & gender.ToLower() != "female" & gender.ToLower() != "other" | chipperId == null | chipperId <= 0 |
                chippingLocationId == null | chippingLocationId <= 0) return Results.StatusCode(400);

                    for (int i = 0; i < animalTypes.Length; i++)
                    {
                        if (animalTypes[i] == null | animalTypes[i] <= 0) return Results.StatusCode(400);
                        if (db.AnimalTypes.Find(animalTypes[i]) == null) return Results.StatusCode(404);
                        for (int j =0; j < animalTypes.Length; j++)
                        {
                            if(i != j)
                            {
                                if (animalTypes[i] == animalTypes[j]) return Results.StatusCode(409);
                            }
                            
                        }
                    }

                if (db.Accounts.Find((long)chipperId) == null) return Results.StatusCode(404);
                if (db.LocationPoints.Find(chippingLocationId) == null) return Results.StatusCode(404);

                var types = db.AnimalTypes.Where(x => animalTypes.Contains(x.Id)).ToList();
                Animal animal = new Animal
                {
                    AnimalTypes = types,
                    ChipperId = chipperId,
                    ChippingDateTime = DateTime.Now,
                    ChippingLocationId = chippingLocationId,
                    Gender = gender,
                    Height = height,
                    Lenght = length,
                    Weight = weight,
                    LifeStatus = "Alive"
                };
                var an = db.Animals.Add(animal).Entity;
                db.SaveChanges();
                return Results.Json(an);

            });

            app.MapPut("/animals/{animalId}", async ([FromQuery] long animalId, [FromQuery] float weight, [FromQuery] float length, [FromQuery] float height , [FromQuery] string gender, [FromQuery] string lifeStatus, [FromQuery] int chipperId, [FromQuery] long chippingLocationId , ContextClass db) =>
            {
                if (weight == null | weight <= 0 | length == null | length <= 0 | height == null | height <= 0 | chipperId <= 0 | chipperId == null | lifeStatus.ToLower() != "alive" & lifeStatus.ToLower() != "dead" |
                gender == null | gender.ToLower() != "male" & gender.ToLower() != "female" & gender.ToLower() != "other" | chipperId == null | chipperId <= 0 |
                chippingLocationId == null | chippingLocationId <= 0) return Results.StatusCode(400);
                var animal = db.Animals.Find(animalId);
                if (animal == null | db.Accounts.Find(chipperId) == null | db.LocationPoints.Find(chippingLocationId) == null) return Results.StatusCode(404);
                if (animal.LifeStatus.ToLower() == "dead" & lifeStatus.ToLower() == "alive" ) return Results.StatusCode(400);
                if (chippingLocationId == animal.ChippingLocationId) return Results.StatusCode(400);
                animal.Weight = weight;
                animal.Height = height;
                animal.Lenght = length;
                animal.ChipperId = chipperId;
                animal.ChippingLocationId = chippingLocationId;
                animal.Gender = gender;
                animal.LifeStatus = lifeStatus;
                if(lifeStatus.ToLower() == "dead")
                    animal.DeathDateTime = DateTime.Now;
                db.Update(animal);
                return Results.Json(animal);
            });

            app.MapDelete("/animals/{animalId}", async ([FromQuery] long animalId, ContextClass db) =>
            {
                if (animalId == null | animalId <= 0) return Results.StatusCode(400);
                var animal = db.Animals.Find(animalId);
                if (animal == null) return Results.StatusCode(404);
                if (!animal.VisitedLocations.Any()) return Results.StatusCode(400);
                db.Animals.Where(x => x.Id == animalId).ExecuteDelete();
                db.SaveChanges();

                return Results.StatusCode(200);
            });
            
            app.MapPost("/animals/{animalId}/types/{typeId}", async ([FromQuery] long animalId, [FromQuery] long typeId, ContextClass db) =>
            {
                if (animalId == null || typeId == null || animalId <= 0 || typeId <= 0) return Results.StatusCode(400);
                if (db.Animals.Find(animalId) == null || db.AnimalTypes.Find(typeId) == null) return Results.StatusCode(404);
                var animal = db.Animals.Where(x => x.Id == animalId).Include(x => x.AnimalTypes).FirstOrDefault();
                animal.AnimalTypes.Add(db.AnimalTypes.Find(typeId));
                db.Animals.Update(animal);
                db.SaveChanges();
                
                var res = db.Animals.Find(animalId);
                return Results.Json(res);
            });

            app.MapPut("/animals/{animalId}/types", async ([FromQuery] long animalId, [FromQuery] long oldTypeId, [FromQuery] long newTypeId, ContextClass db) =>
            {
                if (animalId == null || animalId <= 0 || oldTypeId == null || oldTypeId <= 0 || newTypeId == null || newTypeId <= 0) return Results.StatusCode(400);
                var animal = db.Animals.Where(x => x.Id == animalId).Include(x => x.AnimalTypes).FirstOrDefault();
                var oldType = db.AnimalTypes.Find(oldTypeId);
                var newType = db.AnimalTypes.Find(newTypeId);
                if (oldType == null || newType == null) return Results.StatusCode(404);
                var types = new List<AnimalType>();
                types = animal.AnimalTypes;
                types.Remove(oldType); 
                types.Add(newType);
                animal.AnimalTypes = types;
                db.Animals.Update(animal);
                db.SaveChanges();
                return Results.Json(animal);
            });

            app.MapDelete("/animals/{animalId}/types/{typeId}", async ([FromQuery] long animalId, [FromQuery] long typeId, ContextClass db) =>
            {
                if (animalId <= 0 || animalId == null || typeId <= 0 || typeId == null || db.Animals.Find(animalId).AnimalTypes.Contains(db.AnimalTypes.Find(typeId)) && db.Animals.Find(animalId).AnimalTypes.Count == 1) return Results.StatusCode(400);
                if (db.Animals.Find(animalId) == null || db.AnimalTypes.Find(typeId) == null || db.Animals.Where(x => x.Id == animalId).Include(x => x.AnimalTypes).Where(x => x.AnimalTypes.Contains(db.AnimalTypes.Find(typeId))).FirstOrDefault() == null) return Results.StatusCode(404);
                var temp = db.Animals.Where(x => x.Id == animalId).Include(x => x.AnimalTypes).FirstOrDefault();
                var list = temp.AnimalTypes;
                list.Remove(db.AnimalTypes.Find(typeId));
                temp.AnimalTypes = list;
                db.Animals.Update(temp);
                db.SaveChanges();
                return Results.StatusCode(200);
            });

            app.MapGet("/animals/{animalId}/locations", async ([FromQuery] long animalId, [FromQuery] DateTime? startDateTime, [FromQuery] DateTime? endDateTime, [FromQuery] int? from, [FromQuery] int? size, ContextClass db) =>
            {
                if (animalId == null || animalId <= 0 || from < 0 || size <= 0) return Results.StatusCode(400);
                if (db.Animals.Find(animalId) == null) return Results.StatusCode(404);
                if (size == null) size = 10;
                if (from == null) from = 0;
                var point = db.AnimalVisitedLocations.AsQueryable();
                if (startDateTime != null) point.Where(x => x.DateTimeOfVisitLocationPoint >= startDateTime);
                if (endDateTime != null) point.Where(x => x.DateTimeOfVisitLocationPoint <= endDateTime);
                if (!point.Any()) return Results.Json(point);

                point = point.Skip((int)from);
                point = point.Take((int)size);
                point = point.OrderBy(x => x.Id);
                return Results.Json(point);
            });

            app.MapPost("/animals/{animalId}/locations/{pointId}", async ([FromQuery] long animalId, [FromQuery] long pointId, ContextClass db) =>
            {
                if (animalId == null || animalId <= 0 || pointId == null || pointId <= 0) return Results.StatusCode(400);
                var animal = db.Animals.Where(x => x.Id == animalId).Include(x => x.VisitedLocations).FirstOrDefault();
                var point = db.LocationPoints.Find(pointId);
                if (animal.LifeStatus.ToLower() == "dead" || animal.VisitedLocations.Contains(db.AnimalVisitedLocations.Where(x => x.LocationPointId == animal.ChippingLocationId).FirstOrDefault())  || animal.ChippingLocationId == point.Id ) return Results.StatusCode(400);
                if (!animal.VisitedLocations.IsNullOrEmpty())
                    if (animal.VisitedLocations.Last() == db.AnimalVisitedLocations.Where(x => x.LocationPointId == pointId).FirstOrDefault()) return Results.StatusCode(400);
                if (animal == null || point == null) return Results.StatusCode(404);
                var res = db.AnimalVisitedLocations.Add(new AnimalVisitedLocation { DateTimeOfVisitLocationPoint = DateTime.Now, LocationPointId= point.Id }).Entity;
                var an = db.Animals.Where(x => x.Id == animalId).Include(x => x.VisitedLocations).FirstOrDefault();
                an.VisitedLocations.Add(res);
                db.Animals.Update(an);
                db.SaveChanges();
                return Results.Json(res);
            });

            app.MapPut("/animals/{animalId}/locations", async ([FromQuery] long animalId, [FromQuery] long visitedLocationPointId, [FromQuery] long locationPointId, ContextClass db) =>
            {
                if (animalId == null || animalId <= 0 || visitedLocationPointId == null || visitedLocationPointId <= 0 || locationPointId == null || locationPointId <= 0) return Results.StatusCode(400);
                var animal = db.Animals.Where(x => x.Id == animalId).Include(z => z.VisitedLocations).Where(x => x.VisitedLocations.Contains(db.AnimalVisitedLocations.Find(visitedLocationPointId))).FirstOrDefault();
                var oldVisitedLocationPoint = db.AnimalVisitedLocations.Where(x => x.Id == visitedLocationPointId).FirstOrDefault();
                var point = db.LocationPoints.Where(x => x.Id == locationPointId).FirstOrDefault();

                if (animal.VisitedLocations.First().Id == visitedLocationPointId & animal.ChippingLocationId == locationPointId || oldVisitedLocationPoint.LocationPointId == locationPointId
                ) return Results.StatusCode(400);
                if (animal == null || animal.VisitedLocations.IsNullOrEmpty() || point == null) return Results.StatusCode(404);
                oldVisitedLocationPoint.LocationPointId = point.Id;
                db.AnimalVisitedLocations.Update(oldVisitedLocationPoint);
                db.SaveChanges();
                return Results.Json(oldVisitedLocationPoint);
            });

            app.MapDelete("/animals/{animalId}/locations/{visitedPointId}", async ([FromQuery] long animalId, [FromQuery] long visitedPointId, ContextClass db) =>
            {
                if (animalId == null || animalId <= 0 || visitedPointId == null || visitedPointId <= 0) return Results.StatusCode(400);
                var animal = db.Animals.Where(x => x.Id == animalId).Include(x => x.VisitedLocations).FirstOrDefault();
                var point = db.AnimalVisitedLocations.Where(x => x.Id == visitedPointId).FirstOrDefault();

                if (animal == null || point == null || !animal.VisitedLocations.Contains(point)) return Results.StatusCode(404);
                if (!animal.VisitedLocations.IsNullOrEmpty())
                {
                    if (visitedPointId == animal.VisitedLocations.First().Id & animal.VisitedLocations[1].Id == animal.ChippingLocationId)
                    {
                        var toDel = animal.VisitedLocations[1];
                        db.AnimalVisitedLocations.Where(x => x.Id == toDel.Id).ExecuteDelete();
                        db.SaveChanges();
                    }
                }
                //animal.VisitedLocations.Remove(db.AnimalVisitedLocations.Where(x => x.Id == visitedPointId).FirstOrDefault());
                //db.Animals.Update(animal);
                db.AnimalVisitedLocations.Where(x => x.Id == visitedPointId).ExecuteDelete();
                db.SaveChanges();
                return Results.StatusCode(200);

            });
            */


            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}