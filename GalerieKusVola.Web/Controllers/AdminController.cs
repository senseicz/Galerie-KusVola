﻿using System;
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
            var typyFotek = FotkaManager.GetAll();
            var retModel = new AdminVM {Galerie = galerie, TypyFotek = typyFotek};

            return View(retModel);
        }
        #endregion

        #region Process uploaded photos
        public ActionResult ProcessUploadedPhotos()
        {
            var photosWaiting = FotkaManager.GetWaitingPhotos(CurrentUser);
            var userGalleries = GalerieManager.GetGalerieForUser(CurrentUser.Id);

            var retModel = new ProcessUploadedPhotosVM{Galleries = userGalleries, PhotosWaiting = photosWaiting};

            return View(retModel);
        }

        [HttpPost]
        public ActionResult ProcessUploadedPhotosCustom()
        {
            var request = Request.Form;

            if (request.Keys.Count > 0)
            {
                foreach (var key in request.Keys)
                {
                    if (key.ToString().StartsWith("addedPhotos-") && request[key.ToString()].Length > 0)
                    {
                        var split = key.ToString().Split(new[] {'-'});
                        var galleryId = split[1];
                        var values = request[key.ToString()];

                        if (values.EndsWith(","))
                        {
                            values = values.Substring(0, values.Length - 1);
                        }

                        var photoIdsArr = values.Split(new[] {','});
                        GalerieManager.AddPhotosToGallery(galleryId, photoIdsArr);
                    }
                }
            }

            return RedirectToAction("ProcessUploadedPhotos");
        }

        #endregion

        #region Gallery
        public ActionResult GalleryEdit(string Id)
        {
            GalleryEdit retModel;
            
            if(!string.IsNullOrEmpty(Id))
            {
                var gal = GalerieManager.GetById(Id);
                retModel = new GalleryEdit
                    {
                        GalleryId = gal.Id,
                        Nazev = gal.Nazev,
                        Popis = gal.Popis,
                        GalleryList = GetGallerySelectList(gal.ParentId),
                        Poradi = gal.Poradi
                    };
            }
            else
            {
                retModel = new GalleryEdit { GalleryList = GetGallerySelectList(null) };
            }

            return View(retModel);
        }


        private static SelectList GetGallerySelectList(string selectedGalleryId)
        {
            var galsDb = GalerieManager.GetAll();
            var gals = new List<Galerie>();

            gals.AddRange(galsDb);

            if (!string.IsNullOrEmpty(selectedGalleryId))
            {
                return new SelectList(gals, "Id", "Nazev", selectedGalleryId);
            }

            return new SelectList(gals, "Id", "Nazev");
        }

        [HttpPost]
        public ActionResult GalleryEdit(GalleryEdit galEdit)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var gal = new Galerie
                        {
                            DatumVytvoreni = DateTime.Now,
                            Nazev = galEdit.Nazev,
                            Popis = galEdit.Popis,
                            ParentId = galEdit.ParentGalleryId == "0" ? null : galEdit.ParentGalleryId, //editing root gallery ==> ParentGalleryId == "0"
                            Poradi = galEdit.Poradi,
                            OwnerId = CurrentUser.Id
                        };

                    if(!string.IsNullOrEmpty(galEdit.GalleryId)) //UPDATE
                    {
                        gal.Id = galEdit.GalleryId;
                        GalerieManager.Save(gal);
                        galEdit.OKMessage = "Update typu proběhl úspěšně.";
                    }
                    else //INSERT
                    {
                        gal.OwnerId = CurrentUser.Id;
                        GalerieManager.Save(gal);
                        galEdit.OKMessage = "Uložení nové galerie proběhlo úspěšně.";
                    }
                }
                catch (Exception ex)
                {
                    galEdit.ErrorMessage = "Při ukládání galerie došlo k chybě: " + ex.Message;
                }
            }
            else
            {
                galEdit.ErrorMessage = "Některá povinná položka není vyplněná.";
            }

            return View(galEdit);
        }


        #endregion

        #region PhotoTypeEdit
        public ActionResult PhotoTypeEdit(string Id)
        {
            PhotoTypeEdit retModel;
            
            if(!string.IsNullOrEmpty(Id))
            {
                var typFotky = FotkaManager.GetById(Id);
                retModel = new PhotoTypeEdit
                    {
                        PhotoTypeId = typFotky.Id,
                        SystemName = typFotky.SystemName,
                        Adresar = typFotky.Adresar,
                        NazevTypu = typFotky.JmenoTypu,
                        X = typFotky.X,
                        Y = typFotky.Y
                    };
            }
            else
            {
                retModel = new PhotoTypeEdit();
            }
            
            return View(retModel);
        }

        [HttpPost]
        public ActionResult PhotoTypeEdit(PhotoTypeEdit ptEdit)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var typFotky = new TypFotky {JmenoTypu = ptEdit.NazevTypu, Adresar = ptEdit.Adresar, SystemName = ptEdit.SystemName, X = ptEdit.X};
                    if(ptEdit.Y.HasValue)
                    {
                        typFotky.Y = ptEdit.Y.Value;
                    }

                    if(!string.IsNullOrEmpty(ptEdit.PhotoTypeId))
                    {
                        typFotky.Id = ptEdit.PhotoTypeId;
                    }

                    if (string.IsNullOrEmpty(ptEdit.PhotoTypeId)) //INSERT
                    {
                        FotkaManager.Save(typFotky);
                        ptEdit.OKMessage = "Uložení nového typu proběhlo úspěšně.";
                    }
                    else //UPDATE
                    {
                        //FotkaManager.Update(typFotky);
                        FotkaManager.Save(typFotky);
                        ptEdit.OKMessage = "Update typu proběhl úspěšně.";
                    }
                }
                catch (Exception ex)
                {
                    ptEdit.ErrorMessage = "Při ukládání typu fotky došlo k chybě: " + ex.Message;
                }
            }
            else
            {
                ptEdit.ErrorMessage = "Některá povinná položka není vyplněná.";
            }

            return View(ptEdit);
        }

        #endregion

    }
}
