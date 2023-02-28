using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace Normal
{
    public class Animal
    {
        public Animal() 
        {
            AnimalTypes = new List<AnimalType>();
        }
        public long Id { get; set; }

        public List<AnimalType> AnimalTypes { get; set; }     // id типов животных
        public float Weight { get; set; }
        public float Height { get; set; }
        public float Lenght { get; set; }
        public string? Gender { get; set; }
        public string? LifeStatus { get; set; }
        public DateTime? DeathDateTime { get; set; }
        public int ChipperId { get; set; }
        public DateTime ChippingDateTime { get; set; }
        public long ChippingLocationId { get; set; }
        public List<AnimalVisitedLocation> VisitedLocations { get; set; }
    }
}
