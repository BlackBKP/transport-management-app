using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace TMS.Models
{
    public class BookingModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "employeeName")]
        [ModelBinder(Name = "employeeName")]
        [JsonPropertyName("employeeName")]
        public string EmployeeName { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        [Display(Name = "employeeId")]
        [ModelBinder(Name = "employeeId")]
        [JsonPropertyName("employeeId")]
        public string EmployeeId { get; set; } = string.Empty;

        [Required]
        [StringLength(60)]
        [Display(Name = "department")]
        [ModelBinder(Name = "department")]
        [JsonPropertyName("department")]
        public string Department { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        [Display(Name = "vehicleId")]
        [ModelBinder(Name = "vehicleId")]
        [JsonPropertyName("vehicleId")]
        public int VehicleId { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "routeId")]
        [ModelBinder(Name = "routeId")]
        [JsonPropertyName("routeId")]
        public int RouteId { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "shiftId")]
        [ModelBinder(Name = "shiftId")]
        [JsonPropertyName("shiftId")]
        public int ShiftId { get; set; }

        [Required]
        [Display(Name = "travelDate")]
        [ModelBinder(Name = "travelDate")]
        [JsonPropertyName("travelDate")]
        public DateOnly TravelDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));

        [Required]
        [StringLength(100)]
        [Display(Name = "pickupPoint")]
        [ModelBinder(Name = "pickupPoint")]
        [JsonPropertyName("pickupPoint")]
        public string PickupPoint { get; set; } = string.Empty;

        [StringLength(300)]
        [Display(Name = "notes")]
        [ModelBinder(Name = "notes")]
        [JsonPropertyName("notes")]
        public string? Notes { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = BookingStatus.Confirmed;
    }
}
