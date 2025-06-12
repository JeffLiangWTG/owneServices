using System;
using System.Collections.Generic;
using System.Web.Http;

namespace CargoWise.eServices.Authentication.WebService
{
	public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			config.MapHttpAttributeRoutes();

			config.Routes.MapHttpRoute(
			   name: "ValidateCodeApi",
			   routeTemplate: "{controller}/{action}"
			);

			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "{controller}/{id}",
				defaults: new { id = RouteParameter.Optional }
			);
		}
	}
}
