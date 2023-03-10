using Normal.Models;
using System.Text.Json.Serialization;

namespace Normal
{
    public class UpdateAccountRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class CreateAnimalRequest
    {
        public long[] AnimalTypes { get; set; }
        public float Weight { get; set; }
        public float height { get; set; }
        public float length { get; set; }
        public string Gender { get; set; }
        public int ChipperId { get; set; }
        public long ChippingLocationId { get; set; }
    }

    public class UpdateAnimalRequest
    {
        public float Weight { get; set; }
        public float height { get; set; }
        public float length { get; set; }
        public string Gender { get; set; }
        public int ChipperId { get; set; }
        public long ChippingLocationId { get; set; }
        public string LifeStatus { get; set; }
    }

    public class UpdateAnimalTypeAnimalRequest
    {
        public long oldTypeId { get; set; }
        public long newTypeId { get; set; }
    }

    public class UpdateAnimalVisitedLocationPoint
    {
        public long VisitedLocationPointId { get; set; }
        public long LocationPointId { get; set; }

    }

    public class PointRequest
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class RegRequest
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }
    }

    public class TypeRequest
    {
        public string Type { get; set; }
    }
    
    public class AnimalResponce
    {
        public long Id { get; set; }

        public long[] AnimalTypes { get; set; }     // id
        public float Weight { get; set; }
        public float Height { get; set; }
        public float Length { get; set; }
        public string? Gender { get; set; }
        public string? LifeStatus { get; set; }
        public DateTime? DeathDateTime { get; set; }
        public int ChipperId { get; set; }
        public DateTime ChippingDateTime { get; set; }
        public long ChippingLocationId { get; set; }
        public long[] VisitedLocations { get; set; }    // id
    }

    public class AnimalResponceDT
    {
        public long Id { get; set; }

        public long[] AnimalTypes { get; set; }     // id типов животных
        public float Weight { get; set; }
        public float Height { get; set; }
        public float Length { get; set; }
        public string? Gender { get; set; }
        public string? LifeStatus { get; set; }
        public string? DeathDateTime { get; set; }
        public int ChipperId { get; set; }
        public string? ChippingDateTime { get; set; }
        public long ChippingLocationId { get; set; }
        public long[]? VisitedLocations { get; set; }
    }

    public class PointDT
    {
        public long Id { get; set; }
        public string DateTimeOfVisitLocationPoint { get; set; }
        public long LocationPointId { get; set; }
    }
}
