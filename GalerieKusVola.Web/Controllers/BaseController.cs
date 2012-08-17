using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GalerieKusVola.Managers;
using GalerieKusVola.Models;

namespace GalerieKusVola.Web.Controllers
{
    public class BaseController : Controller
    {
        /// <summary>
        /// Indicates whether current user is logged in
        /// </summary>
        public bool IsUserLoggedIn
        {
            get { return Request.IsAuthenticated; }
        }

        /// <summary>
        /// Provides access to a MembershipUser object in case usr is logged in. Returns null otherwise
        /// </summary>
        public User CurrentUser
        {
            get
            {
                if (IsUserLoggedIn)
                {
                    return UserManager.GetByUserId(ControllerContext.HttpContext.User.Identity.Name);
                }

                return null;
            }
        }

        public string CurrentUserID
        {
            get
            {
                if (IsUserLoggedIn)
                {
                    return CurrentUser.Id;
                }

                return string.Empty;
            }
        }

        public string CurrentUserName
        {
            get
            {
                if (IsUserLoggedIn)
                {
                    return CurrentUser.UserName;
                }

                return "";
            }
        }

    }
}
