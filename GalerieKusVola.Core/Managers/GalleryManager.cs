using System;
using System.Collections.Generic;
using System.Linq;
using GalerieKusVola.Core.DomainModel;
using GalerieKusVola.Core.Utils;
using MongoDB.Bson;
using MongoDB.Driver.Builders;

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
            var gallery = _galleries.Collection.FindOne(Query.EQ("OwnerId", owner.Id));
            return gallery != null;
        }

        public void CreateRootGallery(User owner)
        {
            var gal = new Gallery
                {
                    DateCreated = DateTime.Now,
                    ParentId = ObjectId.Empty,
                    OwnerId = owner.Id,
                    Name = "Root gallery",
                    Description = "Root gallery for user, cannot be deleted.",
                    Order = 1
                };
            Save(gal);
        }

        public List<Gallery> GetGalerieForUser(ObjectId userId)
        {
            return _galleries.Collection.Find(Query.EQ("OwnerId", userId)).ToList();
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
                else
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
