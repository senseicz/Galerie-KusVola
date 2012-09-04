using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using GalerieKusVola.Models;

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
        public string GalleryId { get; set; }
        
        [Required]
        public string Nazev { get; set; }

        [Required]
        public string Popis { get; set; }

        [Required]
        public string ParentGalleryId { get; set; }

        [Required]
        public int Poradi { get; set; }

        public SelectList GalleryList { get; set; }
    }

    public class PhotoTypeEdit : BaseViewModel
    {
        public string PhotoTypeId { get; set; }

        [Required]
        public string NazevTypu { get; set; }

        [Required]
        public string SystemName { get; set; }

        [Required]
        public string Adresar { get; set; }

        [Required]
        public int X { get; set; }

        public int? Y { get; set; }
    }

    public class AdminVM
    {
        public List<Galerie> Galerie { get; set; }
        public List<TypFotky> TypyFotek { get; set; }
    }

    public class ProcessUploadedPhotosVM
    {
        public List<OrigPhotosWaiting> PhotosWaiting { get; set; }
        public List<Galerie> Galleries { get; set; } 
    }

    public class OrigPhotosWaiting
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public DateTime UploadedDate { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string ThumbPath { get; set; }
    }

}
