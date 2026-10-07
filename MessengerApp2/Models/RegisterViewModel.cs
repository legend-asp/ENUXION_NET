using System.ComponentModel.DataAnnotations;

namespace MessengerApp1.Models
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "UserName")]
        public string UserName { get; set; } // Наш Handle

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; }

        // Эти поля больше не Required, чтобы форма работала без них
        public string? Country { get; set; }
        public DateTime BirthDate { get; set; }
    }
}