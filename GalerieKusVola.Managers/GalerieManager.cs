﻿using System;
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

        public static Galerie GetById(string id)
        {
            return DbContext.Current.Single<Galerie>(g => g.Id == id);
        }

        public static List<Galerie> Find(string keyword)
        {
            List<Galerie> galerie = null;

            if (keyword.Length > 0)
            {
                galerie = DbContext.Current.All<Galerie>().Where(d => d.Nazev.ToLower().Contains(keyword.ToLower())).OrderBy(d => d.Nazev).ToList();
            }
            else
            {
                galerie = GetAll();
            }

            return galerie;
        }

        public static void CreateRootGallery(User owner)
        {
            var gal = new Galerie
                {
                    DatumVytvoreni = DateTime.Now,
                    ParentId = null,
                    OwnerId = owner.Id,
                    Nazev = "Kořenová galerie",
                    Popis = "Kořenová galerie, nelze smazat.",
                    Poradi = 1
                };
            Save(gal);
        }

        public static List<Galerie> GetGalerieForUser(string userId)
        {
            return DbContext.Current.All<Galerie>().Where(d => d.OwnerId == userId).OrderBy(d => d.Nazev).ToList();
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
