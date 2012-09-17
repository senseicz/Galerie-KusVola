using System;
using System.Collections.Generic;
using System.Linq;
using GalerieKusVola.Core.DomainModel;
using GalerieKusVola.Core.Utils;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;

namespace GalerieKusVola.Core.Managers
{
    public class GalleryManager
    {
        private readonly MongoHelper<Gallery> _galleries;

        public GalleryManager()
        {
            _galleries = new MongoHelper<Gallery>();
        }
        
        public List<Gallery> GetAll()
        {
            return _galleries.Collection.FindAll().ToList();
        }

        public Gallery GetById(string id)
        {
            return GetById(ObjectId.Parse(id));
        }

        public Gallery GetById(ObjectId id)
        {
            return _galleries.Collection.FindOneById(id);
        }

        public Gallery GetRootGallery(User owner)
        {
            return _galleries.Collection.AsQueryable().First(g => g.OwnerId == owner.Id && !g.IsTrashGallery && g.ParentId == ObjectId.Empty); 
        }

        public Gallery GetTrashGallery(User owner)
        {
            return _galleries.Collection.AsQueryable().First(g => g.OwnerId == owner.Id && g.IsTrashGallery);
        }

        public List<Gallery> GetGalleryChildrens(string parentGallery)
        {
            return _galleries.Collection.AsQueryable().Where(g => g.ParentId == new ObjectId(parentGallery)).OrderBy(g => g.Order).ToList();
        }

        public Gallery ClearTrashGallery(User owner)
        {
            var trash = GetTrashGallery(owner);
            if(trash != null && trash.Photos != null && trash.Photos.Any())
            {
                trash.Photos.Clear();
                Save(trash);
            }

            return trash;
        }

        public List<Gallery> Find(string keyword)
        {
            List<Gallery> galerie = null;

            if (keyword.Length > 0)
            {
                galerie = _galleries.Collection.Find(Query.EQ("Name", keyword)).ToList();
            }
            else
            {
                galerie = GetAll();
            }

            return galerie;
        }

        public bool IsRootGalleryExistForUser(User owner)
        {
            return _galleries.Collection.AsQueryable().Any(g => g.OwnerId == owner.Id && g.ParentId == ObjectId.Empty);
        }

        public bool IsTrashGalleryExistForUser(User owner)
        {
            return _galleries.Collection.AsQueryable().Any(g => g.OwnerId == owner.Id && g.IsTrashGallery);
        }

        public bool IsPhotoInGallery(ObjectId photoId)
        {
            return _galleries.Collection.AsQueryable().Any(g => g.Photos.Any(p => p.Id == photoId));
        }

        public void CreateRootGallery(User owner, bool isTrashGallery)
        {
            string galleryName;
            string galleryDescription;

            if(isTrashGallery)
            {
                galleryName = "Trash";
                galleryDescription = "Use for photos that does not belong to any gallery";
            }
            else
            {
                galleryName = "Root gallery";
                galleryDescription = "Root gallery for user, cannot be deleted.";
            }

            var gal = new Gallery
                {
                    DateCreated = DateTime.Now,
                    ParentId = ObjectId.Empty,
                    OwnerId = owner.Id,
                    Name = galleryName,
                    Description = galleryDescription,
                    Order = 1,
                    IsTrashGallery = isTrashGallery
                };
            Save(gal);
        }

        public List<Gallery> GetGalerieForUser(User owner)
        {
            return _galleries.Collection.Find(Query.EQ("OwnerId", owner.Id)).ToList();
        }

        public void Save(Gallery gallery)
        {
            _galleries.Collection.Save(gallery);
        }

        public void Delete(Gallery gallery)
        {
            _galleries.Collection.Remove(Query.EQ("_id", gallery.Id));
        }

        public void AddPhotosToGallery(string galleryId, string[] photoIds)
        {
            var gallery = GetById(galleryId);
            if(gallery != null)
            {
                int maxOrder = 0;
                
                if(gallery.Photos == null)
                {
                    gallery.Photos = new List<GalleryPhoto>();
                }
                else if(gallery.Photos.Count > 0)
                {
                    maxOrder = gallery.Photos.Max(p => p.Order) + 1;
                }

                foreach (var photoId in photoIds)
                {
                    if (gallery.Photos.Any(f => f.Id == new ObjectId(photoId)))
                    {
                        continue;
                    }
                    
                    var photo = new PhotoManager().GetPhoto(photoId);
                    gallery.Photos.Add(new GalleryPhoto(photo, maxOrder));
                    maxOrder = maxOrder + 1;
                }

                Save(gallery);
            }
        }
    }
}
