using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Enterprise.Winzor.Architecture;
using Enterprise.ZArchitecture.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace CargoWise.Winzor.AppServer.Helpers;

public static class UserMonitorEndpointExtensions
{
	public static void MapUserMonitoringEndpoint(this IEndpointRouteBuilder builder, IHttpForwarder forwarder, ITransformBuilder transformBuilder, ILogger logger, UserMonitorRegistry userMonitorRegistry)
	{
		var httpClient = new HttpMessageInvoker(new SocketsHttpHandler
		{
			EnableMultipleHttp2Connections = true,
			ActivityHeadersPropagator = new ReverseProxyPropagator(DistributedContextPropagator.Current),
			ConnectTimeout = TimeSpan.FromSeconds(15),
			UseCookies = false,
		});

		var requestConfig = ForwarderRequestConfig.Empty;
		var transformer = transformBuilder.Create(context =>
		{
			context.AddRequestTransform(requestContext =>
			{
				var absolutePath = userMonitorRegistry.MonitoringURI?.AbsolutePath;
				requestContext.Path = absolutePath != "/" ? absolutePath : "/intake/v2/rum/events";
				requestContext.ProxyRequest.Headers.Remove((NoResString)"Cookie");
				return new ValueTask();
			});
		});

		builder.MapPost("/usermonitoring/events", async httpContext =>
		{
			if (userMonitorRegistry.MonitoringEnabled.HasValue && !userMonitorRegistry.MonitoringEnabled.Value)
			{
				var errormessage = (NoResString)"user monitoring is not enabled";
				logger.LogWarning(errormessage);
				httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
				await httpContext.Response.WriteAsync(errormessage);
				return;
			}

			if (userMonitorRegistry.MonitoringURI == null)
			{
				var errormessage = (NoResString)"monitoring server url is empty";
				logger.LogWarning(errormessage);
				httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
				await httpContext.Response.WriteAsync(errormessage);
				return;
			}

			var destinationPrefix = userMonitorRegistry.MonitoringURI?.GetLeftPart(UriPartial.Authority);
			var error = await forwarder.SendAsync(httpContext, destinationPrefix, httpClient, requestConfig, transformer);

			if (error != ForwarderError.None)
			{
				var errorFeature = httpContext.Features.Get<IForwarderErrorFeature>();
				var exception = errorFeature.Exception;
				logger.LogWarning(exception.Message, exception.StackTrace);
				httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
				await httpContext.Response.WriteAsync(exception.Message);
			}
		});
	}
}
