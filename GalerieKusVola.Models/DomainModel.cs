using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using GalerieKusVola.Repository.Context;
using Norm;
using Norm.Attributes;

namespace GalerieKusVola.Models
{
    public class Galerie
    {
        public ObjectId Id { get; set; }
        public ObjectId OwnerId { get; set; }
        public ObjectId ParentId { get; set; }
        public string Nazev { get; set; }
        public string Popis { get; set; }
        public DateTime DatumVytvoreni { get; set; }
        public int Poradi { get; set; }

        public List<Fotka> Fotky { get; set; }

        [MongoIgnore]
        public int PocetFotek
        {
            get
            {
                if (Fotky != null)
                {
                    return Fotky.Count;
                }
                return 0;
            }
        }

        private Galerie _parentGalerie;
        [MongoIgnore]
        public Galerie ParentGalerie
        {
            get
            {
                // Lazy-load.
                if (_parentGalerie == null)
                {
                    _parentGalerie = DbContext.Current.Single<Galerie>(g => g.Id == ParentId);
                }

                return _parentGalerie;
            }
            set
            {
                ParentId = value.Id;
                _parentGalerie = value;
            }
        }

        private User _user;
        [MongoIgnore]
        public User User
        {
            get
            {
                // Lazy-load.
                if (_user == null)
                {
                    _user = DbContext.Current.Single<User>(u => u.Id == OwnerId);
                }

                return _user;
            }
            set
            {
                OwnerId = value.Id;
                _user = value;
            }
        }
    }

    public class Fotka
    {
        public ObjectId Id { get; set; }
        public string Nazev { get; set; }
        public DateTime DatumVytvoreni { get; set; }
        public string Popis { get; set; }

        public List<TypFotky> TypyFotek { get; set; }
    }

    public class TypFotky
    {
        public TypyFotek IdTypu { get; set; }
        public string Adresar { get; set; }
    }

    public enum TypyFotek
    {
        Original = 1,
        Galerie = 2,
        Thumbnail = 3
    }

    public class User
    {
        public ObjectId Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PasswordCrypted { get; set; }
    }

}
