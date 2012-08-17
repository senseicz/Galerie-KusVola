using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GalerieKusVola.Repository.Concrete;
using GalerieKusVola.Repository.Interface;
using StructureMap;

namespace GalerieKusVola.Web.App_Start
{
    public class Bootstrapper
    {
        public static void SetupDI()
        {
            // Initializes StructureMap (dependency injector) to setup our concrete database provider.
            ObjectFactory.Initialize(x => x.For<IRepository>().Use<MongoRepository>());
        }

    }
}