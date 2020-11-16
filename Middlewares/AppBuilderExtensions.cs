using Owin;

namespace owin_console_queuing.Middlewares
{
    public static class AppBuilderExtensions
    {
        public static IAppBuilder UseMaxRequests(this IAppBuilder appBuilder, MaxRequestsMiddleware maxRequestsMiddleware)
        {
            appBuilder.Use(maxRequestsMiddleware.MaxAcceptedRequests())
                .Use(maxRequestsMiddleware.MaxConcurrentRequests());
            return appBuilder;
        }
    }
}