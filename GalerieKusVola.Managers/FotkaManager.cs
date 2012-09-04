using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using GalerieKusVola.Models;
using GalerieKusVola.Repository.Context;
using GalerieKusVola.ViewModels;

namespace GalerieKusVola.Managers
{
    public class FotkaManager
    {
        #region DB Operations
        public static Fotka GetFotka(string idFotky)
        {
            return DbContext.Current.Single<Fotka>(f => f.Id == idFotky);
        }

        public static Fotka GetFotkaByFileName(string fileName)
        {
            return DbContext.Current.Single<Fotka>(f => f.NazevSouboru == fileName);
        }

        public static bool FotkaTypExist(Fotka fotka, TypFotky typ)
        {
            return fotka.TypyFotek.Any(tf => tf.Id == typ.Id);
        }

        public static void Save(Fotka fotka)
        {
            DbContext.Current.Add(fotka);
        }

        public static void Delete(Fotka fotka)
        {
            DbContext.Current.Delete<Fotka>(d => d.Id == fotka.Id);
        }

        public static Fotka CreateFotka(string ownerId, string fullFileName, DateTime uploadDate)
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

            var photoToSave = new Fotka {NazevSouboru = photoSystemName, OwnerId = ownerId, DatumUploadu = uploadDate};
            Save(photoToSave);

            return GetFotka(photoToSave.Id);
        }

        public static Fotka AddPhotoTypeToPhoto(Fotka photo, TypFotky type)
        {
            var doSave = false;

            if(photo.TypyFotek == null)
            {
                photo.TypyFotek = new List<TypFotky>{type};
                doSave = true;
            }
            else
            {
                if(photo.TypyFotek.All(p => p.SystemName.ToLower() != type.SystemName.ToLower()))
                {
                    photo.TypyFotek.Add(type);
                    doSave = true;
                }
            }

            if(doSave)
            {
                Save(photo);
            }

            return photo;
        }

        #endregion

        #region I/O operations
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
            var uploadDir = HttpContext.Current.Server.MapPath(string.Format("/{0}/{1}/{2}", ConfigurationManager.AppSettings["GalerieRootDirVirtualPath"], currentUserDir, ConfigurationManager.AppSettings["UploadDir"]));

            var di = new DirectoryInfo(uploadDir);
            if(!di.Exists) { throw new Exception("Upload dir neexituje!");}

            var files = di.GetFiles();

            return files;
        }

        public static List<OrigPhotosWaiting> GetWaitingPhotos(User user)
        {
            var retColl = new List<OrigPhotosWaiting>();
            var currentUserDir = user.UserNameSEO;
            var files = GetFilesInUploadDirectory(currentUserDir);
            if(files.Length > 0)
            {
                var uploadTempType = GetBySystemName("uploadtemp");
                var uploadType = GetBySystemName("upload");
                
                for(int i=0; i<files.Length; i++)
                {
                    var photoAlreadyInDb = GetFotkaByFileName(files[i].Name);

                    string fileName;
                    string id;
                    if(photoAlreadyInDb == null) //fotku sme jeste nezpracovavali
                    {
                        fileName = RenameToSystemFileName(files[i].FullName);
                        var fotka = new Fotka {NazevSouboru = fileName, User = user, DatumUploadu = files[i].CreationTime};

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
                        fileName = photoAlreadyInDb.NazevSouboru;
                        id = photoAlreadyInDb.Id;
                    }

                    var photoWaiting = new OrigPhotosWaiting { FileName = fileName, UploadedDate = files[i].CreationTime, Id = id};

                    //X,Y dimensions of originally uploaded photo - uncomment if you need to diplay it on ProcessUploadPhotos view.
                    /*
                    var origPath = HttpContext.Current.Server.MapPath(string.Format("/{0}/{1}/{2}/{3}", ConfigurationManager.AppSettings["GalerieRootDirVirtualPath"], currentUserDir, uploadType.Adresar, fileName));
                    var origDimensions = ImageProcessingManager.GetImageDimensions(origPath);
                    photoWaiting.X = origDimensions[0];
                    photoWaiting.Y = origDimensions[1];
                    */
                      
                    photoWaiting.ThumbPath = string.Format("/{0}/{1}/{2}/{3}", ConfigurationManager.AppSettings["GalerieRootDirVirtualPath"], currentUserDir, uploadTempType.Adresar, fileName);
                    
                    retColl.Add(photoWaiting);
                }
            }
            return retColl;
        }

        #endregion

        #region Typy fotek
        public static List<TypFotky> GetAll()
        {
            return DbContext.Current.All<TypFotky>().ToList();
        }

        public static TypFotky GetById(string id)
        {
            return DbContext.Current.Single<TypFotky>(g => g.Id == id);
        }

        public static TypFotky GetBySystemName(string name)
        {
            return DbContext.Current.Single<TypFotky>(g => g.SystemName.ToLower() == name.ToLower());
        }

        public static void Save(TypFotky typFotky)
        {
            DbContext.Current.Add(typFotky);
        }

        [Obsolete]
        public static void Update(TypFotky updatedItem)
        {
            var originalItem = DbContext.Current.Single<TypFotky>(t => t.Id == updatedItem.Id);
            DbContext.Current.Update(originalItem, updatedItem);
        }

        #endregion

    }
}
