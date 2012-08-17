using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using GalerieKusVola.Managers;
using GalerieKusVola.Models;
using GalerieKusVola.ViewModels;

namespace GalerieKusVola.Web.Controllers
{
    public class AdminController : BaseController
    {
        #region Register
        public ActionResult Register()
        {
            return View(new Register());
        }

        [HttpPost]
        public ActionResult Register(Register regModel)
        {
            if(ModelState.IsValid)
            {
                if(!regModel.Password.Equals(regModel.PasswordAgain))
                {
                    regModel.ErrorMessage = "Hesla se neshodují";
                }
                else if(UserManager.IsEmailTaken(regModel.Email))
                {
                    regModel.ErrorMessage = "Zadaná emailová adresa je již registrovaná";
                }
                else
                {
                    try
                    {
                        UserManager.RegisterNewUser(regModel);
                        regModel.OKMessage = "Registrace proběhla úspěšně.";
                    }
                    catch(Exception ex)
                    {
                        regModel.ErrorMessage = "Při registraci došlo k chybě: " + ex.Message;
                    }
                }
            }
            else
            {
                regModel.ErrorMessage = "Některá povinná položka není vyplněná.";
            }

            return View(regModel);
        }
        #endregion

        #region Login
        public ActionResult Login()
        {
            return View(new Login());
        }

        [HttpPost]
        public ActionResult Login(Login login)
        {
            if(ModelState.IsValid)
            {
                var user = UserManager.GetByEmail(login.Email);

                if(user == null)
                {
                    login.ErrorMessage = "Uživatel s touto emailovou adresou není v systému registrován. <a href=\"/Admin/Register\">Zaregistrujte se</a>.";
                }
                else
                {
                    if(UserManager.IsPasswordOK(login.Password, user))
                    {
                        LoginUser(user);
                        return RedirectToAction("Index", "Admin");
                    }

                    login.ErrorMessage = "Zadané heslo nesouhlasí.";
                }
            }
            else
            {
                login.ErrorMessage = "Některá povinná položka není vyplněná.";
            }

            return View(login);
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Admin");
        }

        private void LoginUser(User user)
        {
            FormsAuthentication.SetAuthCookie(user.Id, true);
        }

        #endregion

        #region AdminIndex
        [Authorize]
        public ActionResult Index()
        {
            var galerie = GalerieManager.GetGalerieForUser(CurrentUserID);
            return View(galerie);
        }
        #endregion

    }
}
