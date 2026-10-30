using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace TMS.Models
{
    public class LoginModel
    {
        [Required]
        [StringLength(60)]
        [Display(Name = "userName")]
        [ModelBinder(Name = "userName")]
        [JsonPropertyName("userName")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [DataType(DataType.Password)]
        [Display(Name = "password")]
        [ModelBinder(Name = "password")]
        [JsonIgnore]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "rememberMe")]
        [ModelBinder(Name = "rememberMe")]
        [JsonPropertyName("rememberMe")]
        public bool RememberMe { get; set; }

        [ModelBinder(Name = "returnUrl")]
        [JsonPropertyName("returnUrl")]
        public string? ReturnUrl { get; set; }
    }
}
