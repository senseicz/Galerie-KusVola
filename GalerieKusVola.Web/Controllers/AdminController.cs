using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using GalerieKusVola.Core.DomainModel;
using GalerieKusVola.Core.Managers;
using GalerieKusVola.Core.Utils;
using GalerieKusVola.Core.ViewModels;
using MongoDB.Bson;

namespace GalerieKusVola.Web.Controllers
{
    [LayoutInjecter("_LayoutAdmin")]
    public class AdminController : BaseController
    {
        private readonly GalleryManager _galleryManager ;
        private readonly PhotoManager _photoManager;
        private readonly PhotoTypeManager _photoTypeManager;
        
        public AdminController()
        {
            _galleryManager = new GalleryManager();
            _photoManager = new PhotoManager();
            _photoTypeManager = new PhotoTypeManager();
        }
        
        private void AdminBootstrapper()
        {
            //make sure UserNameSEO is set:
            if (string.IsNullOrEmpty(CurrentUser.UserNameSEO))
            {
                var user = CurrentUser;
                user.UserNameSEO = SEO.ConvertTextForSEOURL(CurrentUser.UserName);
                
                _userManager.Save(user);
            }

            //make sure Root gallery exists for current user
            var rootGalExist = _galleryManager.IsRootGalleryExistForUser(CurrentUser);
            if(!rootGalExist)
            {
                _galleryManager.CreateRootGallery(CurrentUser, false);
            }

            //make sure Root gallery exists for current user
            var trashGalExist = _galleryManager.IsTrashGalleryExistForUser(CurrentUser);
            if (!trashGalExist)
            {
                _galleryManager.CreateRootGallery(CurrentUser, true);
            }

            //_photoManager.MovePhotoSiblingsToTrash(CurrentUser);
        }
        
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
                else if(_userManager.IsEmailTaken(regModel.Email))
                {
                    regModel.ErrorMessage = "Zadaná emailová adresa je již registrovaná";
                }
                else
                {
                    try
                    {
                        _userManager.RegisterNewUser(regModel);
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
                var user = _userManager.GetByEmail(login.Email);

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
            FormsAuthentication.SetAuthCookie(user.Id.ToString(), true);
        }

        #endregion

        #region AdminIndex
        [Authorize]
        public ActionResult Index()
        {
            AdminBootstrapper();


            var galleries = _galleryManager.GetGalerieForUser(CurrentUser);
            var photoTypes = _photoTypeManager.GetAll();
            var trash = _galleryManager.GetTrashGallery(CurrentUser);
            var retModel = new AdminVM {Galleries= galleries, PhotoTypes = photoTypes, Trash = trash};

            return View(retModel);
        }
        #endregion

        #region Process uploaded photos
        public ActionResult ProcessUploadedPhotos()
        {
            var photosWaiting = _photoManager.GetWaitingPhotos(CurrentUser);
            var userGalleries = _galleryManager.GetGalerieForUser(CurrentUser);

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
                        _galleryManager.AddPhotosToGallery(galleryId, photoIdsArr);
                        _photoManager.ProcessUploadedPhoto(photoIdsArr);
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
                var gal = _galleryManager.GetById(Id);
                retModel = new GalleryEdit
                    {
                        GalleryId = gal.Id.ToString(),
                        Name = gal.Name,
                        Description = gal.Description,
                        ParentGalleryId = gal.ParentId.ToString(),
                        GalleryList = GetGallerySelectList(gal.ParentId.ToString(), gal.Id.ToString()),
                        Order = gal.Order,
                        Diaries = gal.Diaries,
                        Photos = gal.Photos,
                        PreviewPhotos = gal.PreviewPhotos,
                        TrashPhotos = _galleryManager.GetTrashGallery(CurrentUser).Photos 
                    };
            }
            else
            {
                retModel = new GalleryEdit { GalleryList = GetGallerySelectList(null, null) };
            }

            return View(retModel);
        }

        private SelectList GetGallerySelectList(string selectedGalleryId, string editedGalleryId)
        {
            var galsDb = _galleryManager.GetAll();
            var gals = new List<Gallery>();

            if(string.IsNullOrEmpty(editedGalleryId))
            {
                gals.AddRange(galsDb);
            }
            else
            {
                gals.AddRange(galsDb.Where(g => g.Id.ToString() != editedGalleryId));    
            }

            if (!string.IsNullOrEmpty(selectedGalleryId))
            {
                return new SelectList(gals, "Id", "Name", selectedGalleryId);
            }

            return new SelectList(gals, "Id", "Name");
        }

        [HttpPost]
        public ActionResult GalleryEdit(GalleryEdit galEdit, string hdnPreviewPhotosShadow, string hdnPhotosShadow, string hdnTrashShadow)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var gal = new Gallery
                        {
                            DateCreated = DateTime.Now,
                            Name = galEdit.Name,
                            Description = galEdit.Description,
                            ParentId = galEdit.ParentGalleryId == "0" ? ObjectId.Empty : ObjectId.Parse(galEdit.ParentGalleryId), 
                            Order = galEdit.Order,
                            OwnerId = CurrentUser.Id
                        };

                    if(!string.IsNullOrEmpty(galEdit.GalleryId)) //UPDATE
                    {
                        gal.Id = ObjectId.Parse(galEdit.GalleryId);

                        gal = ProcessGalleryPhotos(gal, hdnPreviewPhotosShadow, hdnPhotosShadow, hdnTrashShadow);

                        _galleryManager.Save(gal);
                        galEdit.OKMessage = string.Format("Update galerie {0} proběhl úspěšně.", gal.Name);
                    }
                    else //INSERT
                    {
                        gal.OwnerId = CurrentUser.Id;
                        _galleryManager.Save(gal);
                        galEdit.OKMessage = string.Format("Uložení nové galerie {0} proběhlo úspěšně.", gal.Name);
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

        [HttpPost]
        public ActionResult GalleryEditCustom(string GalleryId, string hdnPreviewPhotos, string hdnPhotos, string hdnTrash)
        {
            var gallery = _galleryManager.GetById(GalleryId);

            gallery = ProcessGalleryPhotos(gallery, hdnPreviewPhotos, hdnPhotos, hdnTrash);
            _galleryManager.Save(gallery);

            return RedirectToAction("GalleryEdit", new {Id = GalleryId});
        }

        private Gallery ProcessGalleryPhotos(Gallery gallery, string previewPhotos, string photos, string trash)
        {
            var previewPhotosList = new List<GalleryPhoto>();
            var photosList = new List<GalleryPhoto>();
            var separator = new[] { ',' };

            if (!string.IsNullOrEmpty(previewPhotos) && previewPhotos.Length > 1)
            {
                previewPhotos = previewPhotos.Trim(separator);
                var previewPhotosArr = previewPhotos.Split(new[] { ',' });

                for (int i = 0; i < previewPhotosArr.Length; i++)
                {
                    var photo = _photoManager.GetPhoto(previewPhotosArr[i]);
                    if (photo != null)
                    {
                        previewPhotosList.Add(new GalleryPhoto(photo, i + 1));
                    }
                }
            }

            if (!string.IsNullOrEmpty(photos) && photos.Length > 1)
            {
                photos = photos.Trim(separator);
                var photosArr = photos.Split(new[] { ',' });

                for (int i = 0; i < photosArr.Length; i++)
                {
                    var photo = _photoManager.GetPhoto(photosArr[i]);
                    if (photo != null)
                    {
                        photosList.Add(new GalleryPhoto(photo, i + 1));
                    }
                }
            }

            if (!string.IsNullOrEmpty(trash) && trash.Length > 1)
            {
                trash = trash.Trim(separator);
                var trashArr = trash.Split(new[] { ',' });

                if(trashArr.Length > 0)
                {
                    var trashGal = _galleryManager.ClearTrashGallery(CurrentUser);
                    _galleryManager.AddPhotosToGallery(trashGal.Id.ToString(), trashArr);
                }
            }

            gallery.Photos = photosList;
            gallery.PreviewPhotos = previewPhotosList;

            return gallery;
        }

        public ActionResult GetPhotoThumb(string Id)
        {
            var photo = _photoManager.GetPhoto(Id);
            if(photo != null)
            {
                var thumbPath = photo.GetPhotoUrl("minithumb");
                return Json(new {id = Id, path = thumbPath}, JsonRequestBehavior.AllowGet);
            }
            
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetGalleryThumbs(string Id)
        {
            var gallery = _galleryManager.GetById(Id);
            if (gallery != null && gallery.Photos != null && gallery.Photos.Count > 0)
            {
                var retColl = gallery.Photos.Select(photo => new {Id = photo.Id.ToString(), Order = photo.Order, Path = photo.GetPhotoUrl("minithumb")}).ToList();
                return Json(retColl, JsonRequestBehavior.AllowGet);
            }

            return Json(null, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult DeleteGallery(string Id)
        {
            var galerie = _galleryManager.GetById(Id);
            if (galerie != null)
            {
                _galleryManager.Delete(galerie);
            }

            return RedirectToAction("Index");
        }

        public ActionResult ClearTrash()
        {
            _galleryManager.ClearTrashGallery(CurrentUser);

            return RedirectToAction("Index");
        }

        #endregion

        #region PhotoTypeEdit
        public ActionResult PhotoTypeEdit(string Id)
        {
            PhotoTypeEdit retModel;
            
            if(!string.IsNullOrEmpty(Id))
            {
                var typFotky = _photoTypeManager.GetById(Id);
                retModel = new PhotoTypeEdit
                    {
                        PhotoTypeId = typFotky.Id.ToString(),
                        SystemName = typFotky.SystemName,
                        Directory = typFotky.Directory,
                        Name = typFotky.Name,
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
                    var photoType = new PhotoType() {Name = ptEdit.Name, Directory = ptEdit.Directory, SystemName = ptEdit.SystemName, X = ptEdit.X};
                    if(ptEdit.Y.HasValue)
                    {
                        photoType.Y = ptEdit.Y.Value;
                    }

                    if(!string.IsNullOrEmpty(ptEdit.PhotoTypeId))
                    {
                        photoType.Id = ObjectId.Parse(ptEdit.PhotoTypeId);
                    }

                    if (string.IsNullOrEmpty(ptEdit.PhotoTypeId)) //INSERT
                    {
                        _photoTypeManager.Save(photoType);
                        ptEdit.OKMessage = "Uložení nového typu proběhlo úspěšně.";
                    }
                    else //UPDATE
                    {
                        _photoTypeManager.Save(photoType);
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
