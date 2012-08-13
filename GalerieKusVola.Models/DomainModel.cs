using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Norm;

namespace GalerieKusVola.Models
{
    public class Galerie
    {
        public ObjectId Id { get; set; }
        public string Nazev { get; set; }
        public DateTime DatumVytvoreni { get; set; }
        public int Poradi { get; set; }

        public List<Galerie> SubGalerie { get; set; }
        public List<Fotka> Fotky { get; set; }

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

}
