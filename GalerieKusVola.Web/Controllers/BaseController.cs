using System.Web.Mvc;
using GalerieKusVola.Core.DomainModel;
using GalerieKusVola.Core.Managers;

namespace GalerieKusVola.Web.Controllers
{
    public class BaseController : Controller
    {
        protected readonly UserManager _userManager;
        private User _selectedUser;
        
        public BaseController()
        {
            _userManager = new UserManager();
        }
        
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
                    var user = _userManager.GetByUserId(ControllerContext.HttpContext.User.Identity.Name);
                    //user.UserNameSEO = SEO.ConvertTextForSEOURL(user.UserName);
                    return user;
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
                    return CurrentUser.Id.ToString();
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

        public User SelectedUser
        {
            get
            {
                var selectedUserName = RouteData.Values["username"].ToString();

                if(!string.IsNullOrEmpty(selectedUserName))
                {
                    if(_selectedUser == null)
                    {
                        _selectedUser = _userManager.GetBySeoName(selectedUserName);    
                    }

                    return _selectedUser;
                }
                return null;
            }
        }

    }
}
