using System;
using System.Collections.Generic;
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

    public class PhotoTypeEdit : BaseViewModel
    {
        public string PhotoTypeId { get; set; }

        [Required]
        public string NazevTypu { get; set; }

        [Required]
        public string Adresar { get; set; }

        [Required]
        public int X { get; set; }

        public int? Y { get; set; }
    }

    public class AdminVM
    {
        public List<Models.Galerie> Galerie { get; set; }
        public List<Models.TypFotky> TypyFotek { get; set; }
        public List<OrigPhotosWaiting> PhotosWaiting { get; set; } 
    }

    public class OrigPhotosWaiting
    {
        public string FileName { get; set; }
        public DateTime UploadedDate { get; set; }
    }

}
