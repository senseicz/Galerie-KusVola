using System.Linq;
using System.Web.Mvc;
using GalerieKusVola.Core.ViewModels.Home;

namespace GalerieKusVola.Web.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            var retModel = new Index();
            retModel.Users = _userManager.GetAll().OrderBy(u => u.UserNameSEO).ToList();
            return View(retModel);
        }

    }
}
