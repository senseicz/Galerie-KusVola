using System.Linq;
using System.Web.Mvc;
using GalerieKusVola.Core.Managers;
using GalerieKusVola.Web.Controllers;

namespace GalerieKusVola.Web.Areas.Partials.Controllers
{
    public class GalleryLeafController : BaseController
    {
        private readonly GalleryManager _galleryManager ;

        public GalleryLeafController()
        {
            _galleryManager = new GalleryManager();
        }
        
        
        public ActionResult GalleryLeaf(string leafId)
        {
            var gals = _galleryManager.GetGalerieForUser(CurrentUser);

            if(gals != null && gals.Count > 0 && gals.Any(g => g.ParentId.ToString() == leafId))
            {
                var retColl = gals.Where(g => g.ParentId.ToString() == leafId).ToList();
                return View(retColl);
            }

            return new EmptyResult();
        }

        public ActionResult GalleryLeafDroppable(string leafId)
        {
            var gals = _galleryManager.GetGalerieForUser(CurrentUser);

            if (gals != null && gals.Count > 0 && gals.Any(g => g.ParentId.ToString() == leafId))
            {
                var retColl = gals.Where(g => g.ParentId.ToString() == leafId).ToList();
                return View(retColl);
            }

            return new EmptyResult();
        }
    }
}
