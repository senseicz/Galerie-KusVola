using System.Collections.Generic;
using System.Linq;
using GalerieKusVola.Core.DomainModel;
using GalerieKusVola.Core.Utils;
using MongoDB.Bson;
using MongoDB.Driver.Builders;

namespace GalerieKusVola.Core.Managers
{
    public class PhotoTypeManager
    {
        private readonly MongoHelper<PhotoType> _photoTypes;

        public PhotoTypeManager()
        {
            _photoTypes = new MongoHelper<PhotoType>();
        }

        public List<PhotoType> GetAll()
        {
            return _photoTypes.Collection.FindAll().ToList();
        }

        public PhotoType GetById(string id)
        {
            return GetById(ObjectId.Parse(id));
        }

        public PhotoType GetById(ObjectId id)
        {
            return _photoTypes.Collection.FindOne(Query.EQ("_id", id));
        }

        public PhotoType GetBySystemName(string name)
        {
            return _photoTypes.Collection.FindOne(Query.EQ("SystemName", name));
        }

        public void Save(PhotoType photoType)
        {
            _photoTypes.Collection.Save(photoType);
        }
    }
}
