using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using GalerieKusVola.Core.Managers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GalerieKusVola.Core.DomainModel
{
    public class DomainModel
    {
        public ObjectId Id { get; set; }
    }

    public class Gallery : DomainModel
    {
        public ObjectId OwnerId { get; set; }
        public ObjectId ParentId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public int Order { get; set; }
        public bool IsTrashGallery { get; set; }

        public List<GalleryPhoto> Photos { get; set; }
        public List<GalleryPhoto> PreviewPhotos { get; set; }
        public List<Diary> Diaries { get; set; }

        [BsonIgnore]
        public int PhotosCount
        {
            get
            {
                if (Photos != null)
                {
                    return Photos.Count;
                }
                return 0;
            }
        }

        private Gallery _parentGallery;
        [BsonIgnore]
        public Gallery ParentGallery
        {
            get
            {
                // Lazy-load.
                if (_parentGallery == null)
                {
                    _parentGallery = new GalleryManager().GetById(ParentId);
                }

                return _parentGallery;
            }
            set
            {
                ParentId = value.Id;
                _parentGallery = value;
            }
        }

        private User _user;
        [BsonIgnore]
        public User User
        {
            get
            {
                // Lazy-load.
                if (_user == null)
                {
                    _user = new UserManager().GetByUserId(OwnerId);
                }

                return _user;
            }
            set
            {
                OwnerId = value.Id;
                _user = value;
            }
        }
    }

    public class GalleryPhoto : Photo
    {
        public int Order { get; set; }

        public GalleryPhoto(Photo photo, int order)
        {
            Id = photo.Id;
            OwnerId = photo.OwnerId;
            FileName = photo.FileName;
            DateUploaded = photo.DateUploaded;
            DateCreated = photo.DateCreated;
            Description = photo.Description;
            PhotoTypes = photo.PhotoTypes;
            Order = order;
        }
    }

    public class Photo : DomainModel
    {
        public ObjectId OwnerId { get; set; }
        public string FileName { get; set; }
        public DateTime DateUploaded { get; set; }
        public DateTime DateCreated { get; set; }
        public string Description { get; set; }
        public List<PhotoType> PhotoTypes { get; set; }

        [BsonIgnore]
        public string BasePhotoVirtualPath
        {
            get { return string.Format("/{0}/{1}", ConfigurationManager.AppSettings["GalleryRootDirVirtualPath"], User.UserNameSEO); }
        }

        private User _user;
        [BsonIgnore]
        public User User
        {
            get
            {
                // Lazy-load.
                if (_user == null)
                {
                    _user = new UserManager().GetByUserId(OwnerId);
                }

                return _user;
            }
            set
            {
                OwnerId = value.Id;
                _user = value;
            }
        }

        public string GetPhotoUrl(string photoTypeSystemName)
        {
            if (PhotoTypes != null && PhotoTypes.Any(t => t.SystemName.ToLower() == photoTypeSystemName.ToLower()))
            {
                return string.Format("{0}/{1}/{2}", BasePhotoVirtualPath, PhotoTypes.First(t => t.SystemName.ToLower() == photoTypeSystemName.ToLower()).Directory, FileName);
            }
            return "";
        }
    }

    public class PhotoType : DomainModel
    {
        public string Name { get; set; }
        public string SystemName { get; set; }
        public string Directory { get; set; }
        public int X { get; set; }
        public int? Y { get; set; }
    }

    public class User : DomainModel
    {
        public string UserName { get; set; }
        public string UserNameSEO { get; set; }
        public string Email { get; set; }
        public string PasswordCrypted { get; set; }
    }

    public class Diary : DomainModel
    {
        public ObjectId OwnerId { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }

        private User _user;
        [BsonIgnore]
        public User User
        {
            get
            {
                // Lazy-load.
                if (_user == null)
                {
                    _user = new UserManager().GetByUserId(OwnerId);
                }

                return _user;
            }
            set
            {
                OwnerId = value.Id;
                _user = value;
            }
        }
    }


}
