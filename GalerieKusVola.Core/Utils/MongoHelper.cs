using System.Configuration;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace GalerieKusVola.Core.Utils
{
    public class MongoHelper<T> where T : class
    {
        public MongoCollection<T> Collection { get; private set; }

        public MongoHelper()
        {
            var connString = ConfigurationManager.ConnectionStrings["db"].ConnectionString;

            //var con = new MongoConnectionStringBuilder(connString);

            var server = MongoServer.Create(connString);
            var db = server.GetDatabase("Gallery"); //con.DatabaseName
            Collection = db.GetCollection<T>(typeof(T).Name.ToLower());
        }
    }
}
