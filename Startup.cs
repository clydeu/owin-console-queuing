using Owin;
using owin_console_queuing.Middlewares;
using System.Web.Http;

namespace owin_console_queuing
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host.
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            appBuilder.UseMaxRequests(new MaxRequestsMiddleware(5, 3))
                      .UseWebApi(config);
        }
    }
}