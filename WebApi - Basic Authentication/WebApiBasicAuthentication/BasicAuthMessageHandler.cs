using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;

namespace WebApiBasicAuthentication
{
    public abstract class BasicAuthMessageHandler : DelegatingHandler
    {
        private const string BasicAuthResponseHeader = "WWW-Authenticate";
        private const string BasicAuthResponseHeaderValue = "Basic Realm=\"{0}\"";

        protected BasicAuthMessageHandler()
        {
        }

        protected BasicAuthMessageHandler(HttpConfiguration httpConfiguration)
        {
            InnerHandler = new HttpControllerDispatcher(httpConfiguration);
        }

        private static string GetRealm(HttpRequestMessage message)
        {
            return message.RequestUri.Host;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Process request
            AuthenticationHeaderValue authValue = request.Headers.Authorization;
            if (authValue != null && !String.IsNullOrWhiteSpace(authValue.Parameter) && string.Equals(authValue.Scheme, "basic", StringComparison.OrdinalIgnoreCase))
            {
                // Try to authenticate user
                IPrincipal principal = ValidateHeader(authValue.Parameter);
                if (principal != null)
                {
                    request.GetRequestContext().Principal = principal;
                }
            }

            return base.SendAsync(request, cancellationToken)
                .ContinueWith(task =>
                {
                    // Process response
                    var response = task.Result;
                    if (response.StatusCode == HttpStatusCode.Unauthorized && !response.Headers.Contains(BasicAuthResponseHeader))
                    {
                        response.Headers.Add(BasicAuthResponseHeader, string.Format(BasicAuthResponseHeaderValue, GetRealm(request)));
                    }
                    return response;
                }, cancellationToken);
        }

        private IPrincipal ValidateHeader(string authHeader)
        {
            // Decode the authentication header & split it
            var fromBase64String = Convert.FromBase64String(authHeader);
            var lp = Encoding.Default.GetString(fromBase64String);
            if (string.IsNullOrWhiteSpace(lp))
                return null;

            string login;
            string password;
            int pos = lp.IndexOf(':');
            if (pos < 0)
            {
                login = lp;
                password = string.Empty;
            }
            else
            {
                login = lp.Substring(0, pos).Trim();
                password = lp.Substring(pos + 1).Trim();
            }

            return ValidateUser(login, password);
        }

        protected abstract IPrincipal ValidateUser(string userName, string password);
    }
}