using System.Web.Http;

namespace WebApiBasicAuthentication
{
    [Authorize]
    public class SampleController : System.Web.Http.ApiController
    {
        public string Get()
        {
            if (User == null || User.Identity == null)
                return "Not authenticated";

            return User.Identity.Name;
        }
    }
}