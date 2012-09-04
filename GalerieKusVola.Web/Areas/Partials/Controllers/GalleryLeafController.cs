using System.Linq;
using System.Web.Mvc;
using GalerieKusVola.Managers;
using GalerieKusVola.Web.Controllers;

namespace GalerieKusVola.Web.Areas.Partials.Controllers
{
    public class GalleryLeafController : BaseController
    {
        public ActionResult GalleryLeaf(string leafId)
        {
            var gals = GalerieManager.GetGalerieForUser(CurrentUser.Id);

            if(gals != null && gals.Count > 0 && gals.Any(g => g.ParentId == leafId))
            {
                var retColl = gals.Where(g => g.ParentId == leafId).ToList();
                return View(retColl);
            }

            return new EmptyResult();
        }

        public ActionResult GalleryLeafDroppable(string leafId)
        {
            var gals = GalerieManager.GetGalerieForUser(CurrentUser.Id);

            if (gals != null && gals.Count > 0 && gals.Any(g => g.ParentId == leafId))
            {
                var retColl = gals.Where(g => g.ParentId == leafId).ToList();
                return View(retColl);
            }

            return new EmptyResult();
        }
    }
}
