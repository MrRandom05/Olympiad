using System.Text.Json.Serialization;

namespace Normal.Models
{
    public class AnimalType
    {
        public long Id { get; set; }
        public string Type { get; set; }
        [JsonIgnore]
        public List<Animal> Animals { get; set; }
    }
}
