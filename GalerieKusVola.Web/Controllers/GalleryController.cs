using System.Web.Mvc;
using GalerieKusVola.Core.Managers;
using GalerieKusVola.Core.ViewModels.UserGallery;

namespace GalerieKusVola.Web.Controllers
{
    public class GalleryController : BaseController
    {
        private readonly GalleryManager _galleryManager ;
       
        public GalleryController()
        {
            _galleryManager = new GalleryManager();
        }
        
        
        public ActionResult Index()
        {
            if(SelectedUser != null)
            {
                var retModel = new IndexVM();
                var userRootGallery = _galleryManager.GetRootGallery(SelectedUser);
                if (userRootGallery != null)
                {
                    var childGalleries = _galleryManager.GetGalleryChildrens(userRootGallery.Id.ToString());
                    retModel.RootGallery = userRootGallery;
                    retModel.RootGalleryChildrens = childGalleries;
                    return View(retModel);
                }
            }

            return RedirectToAction("Index", "Home");
        }

    }
}
