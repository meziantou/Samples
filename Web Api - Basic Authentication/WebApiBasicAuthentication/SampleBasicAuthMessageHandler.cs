using System;
using System.Security.Principal;

namespace WebApiBasicAuthentication
{
    public class SampleBasicAuthenticationMessageHandler : BasicAuthenticationMessageHandler
    {
        protected override IPrincipal ValidateUser(string userName, string password)
        {
            if (string.Equals(userName, "Meziantou", StringComparison.OrdinalIgnoreCase) && password == "123456")
            {
                return new GenericPrincipal(new GenericIdentity(userName, "Basic"), new string[0]);
            }

            return null;
        }
    }
}