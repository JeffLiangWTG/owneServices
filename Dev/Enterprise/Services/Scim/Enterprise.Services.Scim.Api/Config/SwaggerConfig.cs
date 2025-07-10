using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Owin;
using Swashbuckle.Application;
using Swashbuckle.Examples;
using Swashbuckle.Swagger;

namespace Enterprise.Services.Scim.Api.Config
{
	static class SwaggerConfig
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Route")]
		public static void ConfigureSwagger(this HttpConfiguration config)
		{
			var thisAssembly = typeof(SwaggerConfig).Assembly;

			config
				.EnableSwagger(c =>
				{
					c.RootUrl(req => GetRootPath(req));
					c.Schemes(new[] { "http", "https" });
					c.SingleApiVersion("v1", "SCIM Service APIs");
					c.PrettyPrint();
					var commentsFileName = Assembly.GetExecutingAssembly().GetName().Name + ".XML";
					var baseDirectory = CW1ApplicationInitialiser.IsHostedInIIS() ? System.Web.Hosting.HostingEnvironment.MapPath("~/Bin") : AppDomain.CurrentDomain.BaseDirectory;
					var commentsFile = Path.Combine(baseDirectory, commentsFileName);

					c.IncludeXmlComments(commentsFile);
					c.DocumentFilter<IncludeScimAPIsOnly>();

					c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
					c.OperationFilter<ExamplesOperationFilter>();
				})
				.EnableSwaggerUi(c =>
				{
					c.DocExpansion(DocExpansion.List);
				});
		}

		static string GetRootPath(HttpRequestMessage request)
		{
			var context = request.GetOwinContext();
			if (context == null)
			{
				return request.RequestUri.GetLeftPart(UriPartial.Authority);
			}

			var result = context.Request.PathBase == PathString.Empty ? request.RequestUri.GetLeftPart(UriPartial.Authority) : new Uri(request.RequestUri, context.Request.PathBase.ToString()).ToString();
			return result;
		}

		class IncludeScimAPIsOnly : IDocumentFilter
		{
			public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
			{
				IList<ApiDescription> removals = new List<ApiDescription>();

				foreach (var item in apiExplorer.ApiDescriptions)
				{
					var assemblyName = item.ActionDescriptor?.ControllerDescriptor?.ControllerType?.Assembly?.FullName?.ToUpperInvariant();

					if (assemblyName != null && !GetType().Assembly.FullName.ToUpperInvariant().Equals(assemblyName))
					{
						removals.Add(item);
					}
				}

				foreach (var index in removals)
				{
					var apiPath = index.RelativePathSansQueryString();
					swaggerDoc.paths.Remove("/" + apiPath);
				}
			}
		}
	}
}
