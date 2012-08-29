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

        #endregion

        #region I/O operations
        private static FileInfo[] GetFilesInUploadDirectory(string currentUserDir)
        {
            //var uploadDir = HttpContext.Current.Server.MapPath(string.Format(@"{0}\{1}", ConfigurationManager.AppSettings["GalerieRootDir"], ConfigurationManager.AppSettings["UploadDir"]));
            var uploadDir = string.Format(@"{0}\{1}\{2}", ConfigurationManager.AppSettings["GalerieRootDir"], currentUserDir, ConfigurationManager.AppSettings["UploadDir"]);

            var di = new DirectoryInfo(uploadDir);
            if(!di.Exists) { throw new Exception("Upload dir neexituje!");}

            var files = di.GetFiles();

            return files;
        }

        public static List<OrigPhotosWaiting> GetWaitingPhotos(string currentUserDir)
        {
            var retColl = new List<OrigPhotosWaiting>();
            var files = GetFilesInUploadDirectory(currentUserDir);
            if(files.Length > 0)
            {
                for(int i=0; i<files.Length; i++)
                {
                    retColl.Add(new OrigPhotosWaiting{FileName = files[i].FullName, UploadedDate = files[i].CreationTime});
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

        public static void Save(TypFotky typFotky)
        {
            DbContext.Current.Add(typFotky);
        }

        public static void Update(TypFotky updatedItem)
        {
            var originalItem = DbContext.Current.Single<TypFotky>(t => t.Id == updatedItem.Id);
            DbContext.Current.Update(originalItem, updatedItem);
        }

        #endregion

    }
}
