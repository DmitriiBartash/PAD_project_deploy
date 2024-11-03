using System.ComponentModel.DataAnnotations;

namespace Manager_App.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username cannot be longer than 50 characters")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Verification code is required")]
        public string VerificationCode { get; set; }
    }
}