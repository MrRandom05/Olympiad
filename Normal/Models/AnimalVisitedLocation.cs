namespace Normal.Models
{
    public class AnimalVisitedLocation
    {
        public long Id { get; set; }
        public DateTime DateTimeOfVisitLocationPoint { get; set; }
        public long LocationPointId { get; set; }            // id точки локации
    }
}
