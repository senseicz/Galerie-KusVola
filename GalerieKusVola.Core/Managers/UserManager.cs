using System.Collections.Generic;
using System.Linq;
using GalerieKusVola.Core.DomainModel;
using GalerieKusVola.Core.Utils;
using GalerieKusVola.Core.ViewModels;
using MongoDB.Bson;
using MongoDB.Driver.Builders;

namespace GalerieKusVola.Core.Managers
{
    public class UserManager
    {
        private readonly MongoHelper<User> _users;

        public UserManager()
        {
            _users = new MongoHelper<User>();
        }
        
        public List<User> GetAll()
        {
            return _users.Collection.FindAll().ToList();
        }

        public User GetByUserId(string userId)
        {
            return GetByUserId(ObjectId.Parse(userId));
        }

        public User GetByUserId(ObjectId userId)
        {
            return _users.Collection.FindOne(Query.EQ("_id", userId));
        }

        public User GetByEmail(string email)
        {
            return _users.Collection.FindOne(Query.EQ("Email", email));
        }

        public User GetBySeoName(string seoName)
        {
            return _users.Collection.FindOne(Query.EQ("UserNameSEO", seoName));
        }

        public void Save(User user)
        {
            _users.Collection.Save(user);
        }

        public void Delete(User user)
        {
            _users.Collection.Remove(Query.EQ("_id", user.Id));
        }

        public User RegisterNewUser(Register regModel)
        {
            var cryptedPassword = new Crypto().Encrypt(regModel.Password);
            var newUser = new User { Email = regModel.Email, PasswordCrypted = cryptedPassword, UserName = regModel.Name };
            Save(newUser);
            return newUser;
        }

        public bool IsEmailTaken(string email)
        {
            var user = GetByEmail(email);
            return user != null;
        }

        #region Static methods
        public static bool IsPasswordOK(string password, User user)
        {
            var cryptedPassword = new Crypto().Encrypt(password);
            return cryptedPassword.Equals(user.PasswordCrypted);
        }
        #endregion
    }
}
