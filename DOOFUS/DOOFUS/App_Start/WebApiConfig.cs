using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace DOOFUS
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Web API routes
            config.MapHttpAttributeRoutes();           

            config.Routes.MapHttpRoute(
               name: "global",
               routeTemplate: "api/{controller}/global/",
               defaults: new { id = RouteParameter.Optional }
           );

            config.Routes.MapHttpRoute(
               name: "customer",
               routeTemplate: "api/{controller}/customer/{id}",
               defaults: new { id = RouteParameter.Optional }
           );

            config.Routes.MapHttpRoute(
               name: "device",
               routeTemplate: "api/{controller}/device/{customer id}/{device id}",
               defaults: new { id = RouteParameter.Optional }
           );

            config.Routes.MapHttpRoute(
               name: "device",
               routeTemplate: "api/{controller}/user/{customer id}/{user id}",
               defaults: new { id = RouteParameter.Optional }
           );

            config.Routes.MapHttpRoute(
               name: "device",
               routeTemplate: "api/{controller}/{setting key}",
               defaults: new { id = RouteParameter.Optional }
           );

            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new System.Net.Http.Headers.MediaTypeHeaderValue("text/html"));
        }
    }
}
