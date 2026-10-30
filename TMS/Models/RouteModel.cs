using System.Text.Json.Serialization;

namespace TMS.Models
{
    public class RouteModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("stops")]
        public string[] Stops { get; set; } = [];

        [JsonPropertyName("distanceKm")]
        public int DistanceKm { get; set; }

        [JsonPropertyName("coordinates")]
        public double[][] Coordinates { get; set; } = [];
    }
}
