
namespace AzureCloudService.GettingStarted.WebRole
{
    using System.Web.Http;

    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API 配置和服务

            // Web API 路由
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "VersionApi",
                routeTemplate: "api/{version}/{controller}/{id}",
                defaults: new
                {
                    version = "v1",
                    id = RouteParameter.Optional
                });

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
