using System.ComponentModel.DataAnnotations;

namespace GalerieKusVola.ViewModels
{
    public class BaseViewModel
    {
        public string OKMessage { get; set; }
        public string ErrorMessage { get; set; }
    }
    
    public class Register : BaseViewModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string PasswordAgain { get; set; }
        [Required]
        public string Name { get; set; }
    }

    public class Login : BaseViewModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class GalleryEdit : BaseViewModel
    {
        [Required]
        public string Nazev { get; set; }

        [Required]
        public string Popis { get; set; }
    }
}
