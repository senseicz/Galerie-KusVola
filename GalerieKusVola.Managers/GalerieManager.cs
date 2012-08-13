using System.Collections.Generic;
using System.Linq;
using GalerieKusVola.Models;
using GalerieKusVola.Repository.Context;

namespace GalerieKusVola.Managers
{
    public static class GalerieManager
    {
        public static List<Galerie> GetAll()
        {
            return DbContext.Current.All<Galerie>().OrderBy(d => d.Poradi).ToList();
        }

        public static List<Galerie> Find(string keyword)
        {
            List<Galerie> dragons = null;

            if (keyword.Length > 0)
            {
                dragons = DbContext.Current.All<Galerie>().Where(d => d.Nazev.ToLower().Contains(keyword.ToLower())).OrderBy(d => d.Nazev).ToList();
            }
            else
            {
                dragons = GetAll();
            }

            return dragons;
        }

        public static void Save(Galerie galerie)
        {
            DbContext.Current.Add(galerie);
        }

        public static void Delete(Galerie galerie)
        {
            DbContext.Current.Delete<Galerie>(d => d.Id == galerie.Id);
        }

    }
}
