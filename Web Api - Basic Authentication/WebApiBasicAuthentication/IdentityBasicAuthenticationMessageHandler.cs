using System;
using System.Security.Claims;
using System.Security.Principal;
using JetBrains.Annotations;
using Microsoft.AspNet.Identity;

namespace Meziantou.SimpleBlog.Web.Api
{
    public class IdentityBasicAuthenticationMessageHandler<TUser, TKey> : BasicAuthenticationMessageHandler
        where TUser : class, IUser<TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly UserManager<TUser, TKey> _userManager;

        public UserManager<TUser, TKey> UserManager
        {
            get { return _userManager; }
        }

        public IdentityBasicAuthenticationMessageHandler([NotNull] UserManager<TUser, TKey> userManager)
        {
            if (userManager == null) throw new ArgumentNullException("userManager");

            if (!userManager.SupportsUserPassword)
                throw new ArgumentException("UserManager must support user password.");

            _userManager = userManager;
        }


        protected override IPrincipal ValidateUser(string userName, string password)
        {
            var user = _userManager.FindByName(userName);
            if (user == null)
            {
                return null;
            }

            if (_userManager.IsLockedOut(user.Id))
            {
                return null;
            }

            var result = _userManager.CheckPassword(user, password);
            if (result)
            {
                _userManager.ResetAccessFailedCount(user.Id);
                return new ClaimsPrincipal(_userManager.CreateIdentity(user, "HTTP Basic"));
            }
            if (_userManager.SupportsUserLockout)
            {
                _userManager.AccessFailed(user.Id);
            }

            return null;
        }
    }


    public class IdentityBasicAuthenticationMessageHandler<TUser> : IdentityBasicAuthenticationMessageHandler<TUser, string> where TUser : class, IUser<string>
    {
        public IdentityBasicAuthenticationMessageHandler([NotNull] UserManager<TUser, string> userManager)
            : base(userManager)
        {
        }
    }
}