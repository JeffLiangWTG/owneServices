#if NETFRAMEWORK
using System;
using System.Web.Http.Routing;
using WTG.StaticAnalysis.Annotation;

namespace Microsoft.AspNetCore.Mvc;

[CodeAlive("ServiceHost upgrade to .NET 8. Delete the class once fully upgraded.")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RouteAttribute : Attribute, IRoutePrefix, IDirectRouteFactory, IHttpRouteInfoProvider
{
	public RouteAttribute() => Template = string.Empty;
	public RouteAttribute(string template)
	{
		Prefix = template ?? throw new ArgumentNullException(nameof(template));
		Template = template;
	}

	public string Prefix { get; }

	public string Name { get; set; }

	public int Order { get; set; }

	public string Template { get; }

	RouteEntry IDirectRouteFactory.CreateRoute(DirectRouteFactoryContext context)
	{
		IDirectRouteBuilder builder = context.CreateBuilder(Template);
		builder.Name = Name;
		builder.Order = Order;
		return builder.Build();
	}
}
#endif
