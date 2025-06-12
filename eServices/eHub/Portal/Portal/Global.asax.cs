using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace CargoWise.eHub.Portal
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "TransformationSetAjaxFilter",
                "TransformationFilter/{action}/{type}/{client}",
                new { controller = "TransformationFilter" }
            );

            routes.MapRoute(
                "TransformationSetAjaxGrid",
                "TransformationSet/Grid/{sender}/{recipient}",
                new { controller = "TransformationSet", action = "Grid" }
            );

            routes.MapRoute(
                "ClientFilter",
                "Client/Find/{text}",
                new { controller = "Client", action = "Find" }
            );

            routes.MapRoute(
                "ClientCreate",
                "Client/Create/{text}",
                new { controller = "Client", action = "Create" }
            );

            routes.MapRoute(
                "ClientCreateModal",
                "Client/CreateModal/{text}",
                new { controller = "Client", action = "CreateModal" }
            );

            routes.MapRoute(
                "MessageTypeFilter",
                "MessageType/Find/{text}",
                new { controller = "MessageType", action = "Find" }
            );


            routes.MapRoute(
                    "TransformationTypeFind",
                    "TransformationType/Find",
                    new { controller = "TransformationType", action = "Find" }
            );


            routes.MapRoute(
                "TransformationTypeFilter",
                "TransformationType/Find/{id}",
                new { controller = "TransformationType", action = "Find" }
            );

            routes.MapRoute(
                "TransformationTypeAdd",
                "TransformationType/Add/{orderId}",
                new { controller = "TransformationType", action = "Add", orderId = "1" }
            );

            routes.MapRoute(
                    "TransformationSetIndex",
                    "TransformationSet/Index",
                    new { controller = "TransformationSet", action = "Index" }
            );

            routes.MapRoute(
                "TransformationSet",
                "TransformationSet/{action}/{sender}/{recipient}",
                new { controller = "TransformationSet", action = "Index" }
            );

            routes.MapRoute(
                "TransformationSetMessageType",
                "TransformationSet/{action}/{sender}/{recipient}/{messageType}",
                new { controller = "TransformationSet", action = "Index" }
            );

            routes.MapRoute(
                "CodeMapping",
                "CodeMapping/{action}/{sender}/{recipient}",
                new { controller = "CodeMapping", action = "Index" }
            );

            routes.MapRoute(
                "MessageRouting",
                "MessageRouting/Delete/{clientId}/{airlineId}/{messageTypeId}",
                new { controller = "MessageRouting", action = "Delete" }
            );

            routes.MapRoute(
                    "ClientPIMAIndex",
                    "ClientPIMA/Index/{id}/{env}",
                    new { Controller = "ClientPIMA", action = "Index" });

            routes.MapRoute(
                    "ClientPIMAEdit",
                    "ClientPIMA/Edit/{id}/{env}",
                    new { Controller = "ClientPIMA", action = "Edit" });

            routes.MapRoute(
                    "eHubMessageReferenceClientList",
                    "eHubMessageReference/ClientList",
                    new { Controller = "eHubMessageReference", action = "ClientList" });

            routes.MapRoute(
                    "eHubMessageReferenceIndex",
                    "eHubMessageReference/{applicationCode}",
                    new { Controller = "eHubMessageReference", action = "Index", applicationCode = UrlParameter.Optional });

            routes.MapRoute(
                    "eHubMessageReferenceList",
                    "eHubMessageReference/List/{applicationCode}",
                    new { Controller = "eHubMessageReference", action = "List" });

            routes.MapRoute(
                    "eHubMessageReferenceEdit",
                    "eHubMessageReference/Edit/{applicationCode}",
                    new { Controller = "eHubMessageReference", action = "Edit" });

            routes.MapRoute(
                    "eHubMessageReferenceExport",
                    "eHubMessageReference/ExportCSV/{applicationCode}",
                    new { Controller = "eHubMessageReference", action = "ExportCSV" });

            routes.MapRoute(
                    "eHubMessageReferenceImport",
                    "eHubMessageReference/ImportCSV/{applicationCode}",
                    new { Controller = "eHubMessageReference", action = "ImportCSV" });

            routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

        }

        protected void Application_Start()
        {
			//Debug routing
			//RegisterRoutes(RouteTable.Routes);
			//RouteTable.Routes.RouteExistingFiles = false;
			//RouteDebug.RouteDebugger.RewriteRoutesForTesting(RouteTable.Routes);

			//Default routing
			AreaRegistration.RegisterAllAreas();
			GlobalConfiguration.Configure(WebApiConfig.Register);
			RegisterRoutes(RouteTable.Routes);

            MvcHandler.DisableMvcResponseHeader = true;
        }
	}
}
