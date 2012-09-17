
using System.Collections.Generic;
using GalerieKusVola.Core.DomainModel;
using System.Linq;
using Newtonsoft.Json;

namespace GalerieKusVola.Core.ViewModels.UserGallery
{
    public class GalleryCycler
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public List<string> Images { get; set; } 
    }
    
    public class IndexVM
    {
        public Gallery RootGallery { get; set; }
        public List<Gallery> RootGalleryChildrens { get; set; }
        
        public bool HasSomePreviewImage
        {
            get { return (RootGallery != null && RootGallery.PreviewPhotos != null && RootGallery.PreviewPhotos.Count > 0); }
        }

        public string FirstPreviewUrl
        {
            get
            {
                if(HasSomePreviewImage)
                {
                    return RootGallery.PreviewPhotos.OrderBy(p => p.Order).First().GetPhotoUrl("orig");
                }

                return "";
            }
        }

        public string GalleryCyclerJson
        {
            get 
            { 
                var cyclerColl = new List<GalleryCycler>();

                if(HasSomePreviewImage)
                {
                    var rootCycler = new GalleryCycler()
                        {
                            Name = "root",
                            Id = RootGallery.Id.ToString(),
                            Images = RootGallery.PreviewPhotos.OrderBy(p => p.Order).Select(photo => photo.GetPhotoUrl("orig")).ToList()
                        };

                    cyclerColl.Add(rootCycler);
                }

                if(RootGalleryChildrens != null && RootGalleryChildrens.Count > 0)
                {
                    foreach (var gallery in RootGalleryChildrens)
                    {
                        var galCycler = new GalleryCycler
                            {
                                Name = gallery.Name,
                                Id = gallery.Id.ToString()
                            };

                        if(gallery.PreviewPhotos != null && gallery.PreviewPhotos.Count > 0)
                        {
                            galCycler.Images = new List<string> {gallery.PreviewPhotos[0].GetPhotoUrl("orig")};
                        }

                        cyclerColl.Add(galCycler);
                    }
                }

                return JsonConvert.SerializeObject(cyclerColl, Formatting.Indented);
            }
        }
    }



}
