using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using GalerieKusVola.Repository.Interface;
using GalerieKusVola.Repository.Helpers;
using MongoDB.Driver;

namespace GalerieKusVola.Repository.Concrete
{
    public class MongoRepository : IRepository
    {
        private MongoServer _provider;
        private MongoDatabase _db { get { return _provider.GetDatabase("kusvola_galerie", SafeMode.True); } }

        public MongoRepository()
        {
            _provider = MongoServer.Create(ConfigurationManager.ConnectionStrings["db"].ConnectionString);
        }

        public void Delete<T>(System.Linq.Expressions.Expression<Func<T, bool>> expression) where T : class, new()
        {
            var items = All<T>().Where(expression);
            foreach (T item in items)
            {
                Delete(item);
            }
        }

        public void Delete<T>(T item) where T : class, new()
        {
            Type g = typeof(T);
            var collectionName = g.Name;
            
            _db.GetCollection<T>(collectionName).Remove()
        }

        public void DeleteAll<T>() where T : class, new()
        {
            _db.DropCollection(typeof(T).Name);
        }

        public T Single<T>(System.Linq.Expressions.Expression<Func<T, bool>> expression) where T : class, new()
        {
            return All<T>().Where(expression).SingleOrDefault();
        }

        public IQueryable<T> All<T>() where T : class, new()
        {
            return _db.GetCollection<T>().AsQueryable();
        }

        public IQueryable<T> All<T>(int page, int pageSize) where T : class, new()
        {
            return PagingExtensions.Page(All<T>(), page, pageSize);
        }

        public void Add<T>(T item) where T : class, new()
        {
            _db.GetCollection<T>().Save(item);
        }

        public void Add<T>(IEnumerable<T> items) where T : class, new()
        {
            foreach (T item in items)
            {
                Add(item);
            }
        }

        public void Update<T, U>(T origItem, U updatedIten) where U : class, new()
        {
            _db.GetCollection<U>().UpdateOne(origItem, updatedIten);
        }

        public void Dispose()
        {
            _provider.Dispose();
        }
    }
}
