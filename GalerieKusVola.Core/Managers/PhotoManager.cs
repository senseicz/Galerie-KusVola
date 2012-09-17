using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using GalerieKusVola.Core.DomainModel;
using GalerieKusVola.Core.Utils;
using GalerieKusVola.Core.ViewModels;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;


namespace GalerieKusVola.Core.Managers
{
    public class PhotoManager
    {
        private readonly MongoHelper<Photo> _photos;

        public PhotoManager()
        {
            _photos = new MongoHelper<Photo>();
        }

        public Photo GetPhoto(string photoId)
        {
            return GetPhoto(ObjectId.Parse(photoId));
        }

        public Photo GetPhoto(ObjectId photoId)
        {
            return _photos.Collection.FindOne(Query.EQ("_id", photoId));
        }

        public Photo GetPhotoByFileName(string fileName)
        {
            return _photos.Collection.FindOne(Query.EQ("FileName", fileName));
        }

        public void MovePhotoSiblingsToTrash(User owner)
        {
            var photos = _photos.Collection.FindAll();
            if(photos != null && photos.Any())
            {
                var galleryManager = new GalleryManager();
                var trashGallery = galleryManager.GetTrashGallery(owner);
                var siblings = (from photo in photos where !galleryManager.IsPhotoInGallery(photo.Id) select photo.Id.ToString()).ToList();

                if (siblings.Count > 0)
                {
                    galleryManager.AddPhotosToGallery(trashGallery.Id.ToString(), siblings.ToArray());
                }
            }
        }

        public void Save(Photo photo)
        {
            _photos.Collection.Save(photo);
        }

        public void Delete(Photo photo)
        {
            _photos.Collection.Remove(Query.EQ("_id", photo.Id));
        }

        public Photo CreateFotka(string ownerId, string fullFileName, DateTime dateUploaded)
        {
            string photoSystemName;
            
            try
            {
                photoSystemName = RenameToSystemFileName(fullFileName);
            }
            catch(Exception ex)
            {
                throw;
            }

            var photoToSave = new Photo {FileName = photoSystemName, OwnerId = new ObjectId(ownerId), DateUploaded = dateUploaded};
            Save(photoToSave);

            return GetPhoto(photoToSave.Id);
        }

        public Photo AddPhotoTypeToPhoto(Photo photo, PhotoType photoType)
        {
            var doSave = false;

            if(photo.PhotoTypes == null)
            {
                photo.PhotoTypes = new List<PhotoType>{photoType};
                doSave = true;
            }
            else
            {
                if(photo.PhotoTypes.All(p => p.SystemName.ToLower() != photoType.SystemName.ToLower()))
                {
                    photo.PhotoTypes.Add(photoType);
                    doSave = true;
                }
            }

            if(doSave)
            {
                Save(photo);
            }

            return photo;
        }

        public Photo RemovePhotoTypeFromPhoto(Photo photo, PhotoType ptToBeRemoved)
        {
            if(photo.PhotoTypes != null)
            {
                if(photo.PhotoTypes.Any(pt => pt.Id == ptToBeRemoved.Id))
                {
                    photo.PhotoTypes.Remove(ptToBeRemoved);
                    Save(photo);
                }
            }

            return photo;
        }

        public void ProcessUploadedPhoto(string[] photoIds)
        {
            foreach (var photoId in photoIds)
            {
                ObjectId objPhotoId;
                if (ObjectId.TryParse(photoId, out objPhotoId))
                {
                    var photo = GetPhoto(objPhotoId);
                    if (photo != null)
                    {
                        var ptManager = new PhotoTypeManager();
                        var origPhotoType = ptManager.GetBySystemName("orig");
                        var uploadPhotoType = ptManager.GetBySystemName("upload");

                        var uploadPath = string.Format("{0}/{1}/{2}", photo.BasePhotoVirtualPath,
                                                       uploadPhotoType.Directory,
                                                       photo.FileName);
                        var uploadPhysicalPath = HttpContext.Current.Server.MapPath(uploadPath);

                        var origPath = string.Format("{0}/{1}/{2}", photo.BasePhotoVirtualPath, origPhotoType.Directory,
                                                     photo.FileName);
                        var origPhysicalPath = HttpContext.Current.Server.MapPath(origPath);

                        try
                        {
                            MoveFile(uploadPhysicalPath, origPhysicalPath);
                        }
                        catch
                        {
                            throw;
                        }

                        RemovePhotoTypeFromPhoto(photo, uploadPhotoType);
                        AddPhotoTypeToPhoto(photo, origPhotoType);
                    }
                }
            }
        }


        public List<OrigPhotosWaiting> GetWaitingPhotos(User user)
        {
            var retColl = new List<OrigPhotosWaiting>();
            var currentUserDir = user.UserNameSEO;
            var files = GetFilesInUploadDirectory(currentUserDir);
            if (files.Length > 0)
            {
                var typeManager = new PhotoTypeManager();
                var uploadTempType = typeManager.GetBySystemName("minithumb");
                var uploadType = typeManager.GetBySystemName("upload");

                for (int i = 0; i < files.Length; i++)
                {
                    var photoAlreadyInDb = GetPhotoByFileName(files[i].Name);

                    string fileName;
                    ObjectId id;
                    if (photoAlreadyInDb == null) //fotku sme jeste nezpracovavali
                    {
                        fileName = RenameToSystemFileName(files[i].FullName);
                        var fotka = new Photo { FileName = fileName, User = user, DateUploaded = files[i].CreationTime };

                        Save(fotka);
                        AddPhotoTypeToPhoto(fotka, uploadType);
                        try
                        {
                            ImageProcessingManager.ResizeImage(fotka, uploadType, uploadTempType);
                            AddPhotoTypeToPhoto(fotka, uploadTempType);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                        id = fotka.Id;
                    }
                    else
                    {
                        fileName = photoAlreadyInDb.FileName;
                        id = photoAlreadyInDb.Id;
                    }

                    var photoWaiting = new OrigPhotosWaiting { FileName = fileName, UploadedDate = files[i].CreationTime, Id = id.ToString() };

                    //X,Y dimensions of originally uploaded photo - uncomment if you need to diplay it on ProcessUploadPhotos view.
                    /*
                    var origPath = HttpContext.Current.Server.MapPath(string.Format("/{0}/{1}/{2}/{3}", ConfigurationManager.AppSettings["GalleryRootDirVirtualPath"], currentUserDir, uploadType.Adresar, fileName));
                    var origDimensions = ImageProcessingManager.GetImageDimensions(origPath);
                    photoWaiting.X = origDimensions[0];
                    photoWaiting.Y = origDimensions[1];
                    */

                    photoWaiting.ThumbPath = string.Format("/{0}/{1}/{2}/{3}", ConfigurationManager.AppSettings["GalleryRootDirVirtualPath"], currentUserDir, uploadTempType.Directory, fileName);

                    retColl.Add(photoWaiting);
                }
            }
            return retColl;
        }



        #region StaticMethods
        public static bool PhotoTypeExist(Photo photo, PhotoType photoType)
        {
            return photo.PhotoTypes.Any(tf => tf.Id == photoType.Id);
        }
        #endregion


        #region I/O operations
        private static void MoveFile(string sourceFileName, string targetFileName)
        {
            var srcFI = new FileInfo(sourceFileName);
            if(srcFI.Exists)
            {
                //double-check that target does not exist:
                var tarFI = new FileInfo(targetFileName);
                if(!tarFI.Exists)
                {
                    srcFI.MoveTo(targetFileName);
                }
                else
                {
                    throw new Exception("Target file already exist!");
                }
            }
        }

        private static string RenameToSystemFileName(string fullFileName)
        {
            var fi = new FileInfo(fullFileName);

            string systemName = "";

            if(fi.Exists)
            {
                var fileName = fi.Name.Substring(0, (fi.Name.Length - fi.Extension.Length));
                systemName = string.Format("{0}{1}", MakeSystemName(fileName), fi.Extension);
                fi.MoveTo(string.Format(@"{0}\{1}", fi.DirectoryName, systemName));
            }

            return systemName;
        }

        private static string MakeSystemName(string fileName)
        {
            const string allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ_1234567890";
            string fileNameUpperCase = fileName.ToUpper();
            string newFileName = "";

            for (int i = 0; i < fileNameUpperCase.Length; i++)
            {
                if (allowedChars.Contains(fileNameUpperCase[i].ToString()))
                {
                    newFileName = newFileName + fileNameUpperCase[i];
                }
            }

            newFileName = string.Format("{0}_{1}_{2}", DateTime.Now.ToString("ddMMyy"), DateTime.Now.ToString("HHmm"), newFileName);

            return newFileName;
        }


        private static FileInfo[] GetFilesInUploadDirectory(string currentUserDir)
        {
            var uploadDir = HttpContext.Current.Server.MapPath(string.Format("/{0}/{1}/{2}", ConfigurationManager.AppSettings["GalleryRootDirVirtualPath"], currentUserDir, ConfigurationManager.AppSettings["UploadDir"]));

            var di = new DirectoryInfo(uploadDir);
            if(!di.Exists) { throw new Exception("Upload dir neexituje!");}

            var files = di.GetFiles();

            return files;
        }

       

        #endregion
    }
}
