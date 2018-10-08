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

            //global
            config.Routes.MapHttpRoute(
               name: "Global",
               routeTemplate: "api/{controller}/global/{key}",
               defaults: new { id = RouteParameter.Optional }
           );

            config.Routes.MapHttpRoute(
              name: "GlobalOverride",
              routeTemplate: "api/{controller}/global/{key}?overrideLower=true",
              defaults: new { id = RouteParameter.Optional }
          );

            config.Routes.MapHttpRoute(
              name: "GlobalEntity",
              routeTemplate: "api/{controller}/global/{entity id}/{key}",
              defaults: new { id = RouteParameter.Optional }
          );

            config.Routes.MapHttpRoute(
              name: "GlobalEntityOverride",
              routeTemplate: "api/{controller}/global/{entity id}/{key}?overrideLower=true",
              defaults: new { id = RouteParameter.Optional }
          );
            //customer
            config.Routes.MapHttpRoute(
               name: "Customer",
               routeTemplate: "api/{controller}/customer/{key}",
               defaults: new { id = RouteParameter.Optional }
           );

            config.Routes.MapHttpRoute(
              name: "CustomerOverride",
              routeTemplate: "api/{controller}/customer/{key}?overrideLower=true",
              defaults: new { id = RouteParameter.Optional }
          );

            config.Routes.MapHttpRoute(
              name: "CustomerEntity",
              routeTemplate: "api/{controller}/customer/{entity id}/{key}",
              defaults: new { id = RouteParameter.Optional }
          );

            config.Routes.MapHttpRoute(
              name: "CustomerEntityOverride",
              routeTemplate: "api/{controller}/customer/{entity id}/{key}?overrideLower=true",
              defaults: new { id = RouteParameter.Optional }
          );

            //user
            config.Routes.MapHttpRoute(
              name: "User",
              routeTemplate: "api/{controller}/User/{customer id}{key}",
              defaults: new { id = RouteParameter.Optional }
          );          

            config.Routes.MapHttpRoute(
            name: "UserEntity",
            routeTemplate: "api/{controller}/User/{customer id}/{entity id}/{key}",
            defaults: new { id = RouteParameter.Optional }
        );          

            //device
            config.Routes.MapHttpRoute(
              name: "Device",
              routeTemplate: "api/{controller}/device/{customer id}/{key}",
              defaults: new { id = RouteParameter.Optional }
          );

            config.Routes.MapHttpRoute(
              name: "DeviceOverride",
              routeTemplate: "api/{controller}/device/{customer id}/{key}?overrideLower=true",
              defaults: new { id = RouteParameter.Optional }
          );

            config.Routes.MapHttpRoute(
             name: "DeviceEntity",
             routeTemplate: "api/{controller}/device/{customer id}/{entity id}/{key}",
             defaults: new { id = RouteParameter.Optional }
         );

            config.Routes.MapHttpRoute(
            name: "DeviceEntityOverride",
            routeTemplate: "api/{controller}/device/{customer id}/{entity id}/{key}/overrideLower=true",
            defaults: new { id = RouteParameter.Optional }
        );

            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new System.Net.Http.Headers.MediaTypeHeaderValue("text/html"));
        }
    }
}
