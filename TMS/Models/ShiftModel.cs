using System.Text.Json.Serialization;

namespace TMS.Models
{
    public class ShiftModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("departure")]
        public string Departure { get; set; } = string.Empty;

        [JsonPropertyName("workHours")]
        public string WorkHours { get; set; } = string.Empty;
    }
}
