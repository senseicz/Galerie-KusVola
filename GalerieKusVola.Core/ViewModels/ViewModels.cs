using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using GalerieKusVola.Core.DomainModel;
using System.Linq;
using MongoDB.Bson;

namespace GalerieKusVola.Core.ViewModels
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
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string ParentGalleryId { get; set; }

        [Required]
        public int Order { get; set; }

        public SelectList GalleryList { get; set; }

        public List<GalleryPhoto> Photos { get; set; }
        public List<Diary> Diaries { get; set; }
        public List<GalleryPhoto> PreviewPhotos { get; set; }
        public List<GalleryPhoto> TrashPhotos { get; set; }

        public string PreviewPhotoIds
        {
            get
            {
                var retString = "";
                if (PreviewPhotos != null && PreviewPhotos.Count > 0)
                {
                    retString = PreviewPhotos.OrderBy(p => p.Order).Aggregate(retString, (current, photo) => current + photo.Id.ToString() + ",");
                }

                return retString.TrimEnd(new []{','});
            }
        }

        public string PhotoIds
        {
            get
            {
                var retString = "";
                if (Photos != null && Photos.Count > 0)
                {
                    retString = Photos.OrderBy(p => p.Order).Aggregate(retString, (current, photo) => current + photo.Id.ToString() + ",");
                }

                return retString.TrimEnd(new[] { ',' }); 
            }
        }

        public string TrashPhotoIds
        {
            get
            {
                var retString = "";
                if (TrashPhotos != null && TrashPhotos.Count > 0)
                {
                    retString = TrashPhotos.OrderBy(p => p.Order).Aggregate(retString, (current, photo) => current + photo.Id.ToString() + ",");
                }

                return retString.TrimEnd(new[] { ',' });
            }
        }
    }

    public class PhotoTypeEdit : BaseViewModel
    {
        public string PhotoTypeId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string SystemName { get; set; }

        [Required]
        public string Directory { get; set; }

        [Required]
        public int X { get; set; }

        public int? Y { get; set; }
    }

    public class AdminVM
    {
        public List<Gallery> Galleries { get; set; }
        public List<PhotoType> PhotoTypes { get; set; }
        public Gallery Trash { get; set; }
    }

    public class ProcessUploadedPhotosVM
    {
        public List<OrigPhotosWaiting> PhotosWaiting { get; set; }
        public List<Gallery> Galleries { get; set; }
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
