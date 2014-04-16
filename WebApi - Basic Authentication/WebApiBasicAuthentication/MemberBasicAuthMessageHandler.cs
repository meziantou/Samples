using System.Security.Principal;
using System.Web.Security;

namespace WebApiBasicAuthentication
{
    public class MemberBasicAuthMessageHandler : BasicAuthMessageHandler
    {
        protected override IPrincipal ValidateUser(string userName, string password)
        {
            if (Membership.ValidateUser(userName, password))
            {
                string[] roles = null;
                if (Roles.Enabled)
                {
                    roles = Roles.GetRolesForUser(userName);
                }

                return new GenericPrincipal(new GenericIdentity(userName, "Basic"), roles);
            }

            return null;
        }
    }
}