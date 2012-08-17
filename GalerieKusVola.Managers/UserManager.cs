using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalerieKusVola.Managers.utils;
using GalerieKusVola.Models;
using GalerieKusVola.Repository.Context;
using GalerieKusVola.ViewModels;

namespace GalerieKusVola.Managers
{
    public class UserManager
    {
        public static void RegisterNewUser(Register regModel)
        {
            var cryptedPassword = new Crypto().Encrypt(regModel.Password);

            var newUser = new User {Email = regModel.Email, PasswordCrypted = cryptedPassword, UserName = regModel.Name};
            Save(newUser);
            GalerieManager.CreateRootGallery(newUser);
        }

        public static bool IsPasswordOK(string password, User user)
        {
            var cryptedPassword = new Crypto().Encrypt(password);
            return cryptedPassword.Equals(user.PasswordCrypted);
        }
        
        public static List<User> GetAll()
        {
            return DbContext.Current.All<User>().ToList();
        }

        public static User GetByUserId(string userId)
        {
            return DbContext.Current.Single<User>(g => g.Id == userId);
        }

        public static User GetByEmail(string email)
        {
            return DbContext.Current.Single<User>(g => g.Email == email);
        }

        public static bool IsEmailTaken(string email)
        {
            return DbContext.Current.All<User>().Any(u => u.Email == email);
        }

        public static void Save(User user)
        {
            DbContext.Current.Add(user);
        }

        public static void Delete(User user)
        {
            DbContext.Current.Delete<User>(d => d.Id == user.Id);
        }
    }
}
