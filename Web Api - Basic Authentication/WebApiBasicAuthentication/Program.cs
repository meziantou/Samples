using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using Owin;

namespace WebApiBasicAuthentication
{
    class Program
    {
        private static void Main(string[] args)
        {
            using (WebApp.Start<Startup>("http://localhost:12345"))
            {
                Console.WriteLine("Started");
                Console.ReadLine();
            }
        }

        public class Startup
        {
            public void Configuration(IAppBuilder app)
            {
                HttpConfiguration config = new HttpConfiguration();
                config.Routes.MapHttpRoute(
                    name: "DefaultApi",
                    routeTemplate: "api/{controller}/{id}",
                    defaults: new { id = RouteParameter.Optional }
                );

                config.MessageHandlers.Add(new SampleBasicAuthenticationMessageHandler());

                app.UseWebApi(config);
            }
        }
    }
}

