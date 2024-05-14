using System.ComponentModel.DataAnnotations;

namespace MobileBgScraper.Models
{
    public class User
    {
        [Required, Display(Name = "Username")]
        public string UserName { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email")]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required, DataType(DataType.Password), Compare(nameof(Password), ErrorMessage = "Passwords do not match"), Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }
    }
}
