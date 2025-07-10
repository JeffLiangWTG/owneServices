using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CargoWise.Blazor.Testing.Common;
using Enterprise.ZArchitecture.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Net.Http.Headers;
using NUnit.Framework;
using Yarp.ReverseProxy.Configuration;

namespace CargoWise.Blazor.SessionBroker.Test
{
	class SessionBrokerReverseProxyTest
	{
		[Test]
		public async Task SessionBrokerShouldAppendXForwardedHeaders()
		{
			using var unconfiguredFactory = new CustomWebApplicationFactory<Startup>();
			using var factory = unconfiguredFactory.WithWebHostBuilder(builder =>
			{
				// WebApplicationFactory does not include an IP Address in the request as it's in memory and not a real network request.
				// We need to intercept the request and add an IP address to ensure that it is forwarded correctly in the  headers.
				builder.ConfigureServices(services => services.AddSingleton<IStartupFilter, ConfigureIPAddressRewriter>());
			});
			await using var dummyAppServer = new DummyAppServerProcess();
			await dummyAppServer.StartAsync();
			AddNodeToCluster(factory.Services, dummyAppServer.UniqueId, dummyAppServer.Address, dummyAppServer.ProcessId);

			var client = factory.CreateClient();
			client.DefaultRequestHeaders.Add(HeaderNames.Cookie, new CookieHeaderValue(AffinityCookieOptionsProvider.DefaultCookieName, GetAffinityCookieValue(factory.Services, dummyAppServer.UniqueId)).ToString());
			client.DefaultRequestHeaders.Add(ForwardedHeadersDefaults.XForwardedHostHeaderName, "dummy-forwarded-host");
			client.DefaultRequestHeaders.Add(ForwardedHeadersDefaults.XForwardedForHeaderName, "dummy-forwarded-for");
			client.DefaultRequestHeaders.Add(ForwardedHeadersDefaults.XForwardedProtoHeaderName, "dummy-forwarded-proto");

			var response = await client.GetAsync("/headers");
			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

			var forwardedRequestHeaders = await response.Content.ReadFromJsonAsync<KeyValuePair<string, string>[]>();

			Assert.That(forwardedRequestHeaders.Single(h => h.Key == ForwardedHeadersDefaults.XForwardedHostHeaderName).Value, Is.EqualTo("dummy-forwarded-host, localhost"));
			Assert.That(forwardedRequestHeaders.Single(h => h.Key == ForwardedHeadersDefaults.XForwardedForHeaderName).Value, Is.EqualTo("dummy-forwarded-for, 127.0.0.1"));
			Assert.That(forwardedRequestHeaders.Single(h => h.Key == ForwardedHeadersDefaults.XForwardedProtoHeaderName).Value, Is.EqualTo("dummy-forwarded-proto, http"));
		}

		static string GetAffinityCookieValue(IServiceProvider services, string nodeId)
		{
			var dataProtectionProvider = services.GetRequiredService<IDataProtectionProvider>();

			var bytes = Encoding.UTF8.GetBytes(nodeId);
			var dataProtector = dataProtectionProvider.CreateProtector("Yarp.ReverseProxy.SessionAffinity.CookieSessionAffinityPolicy");
			return Convert.ToBase64String(dataProtector.Protect(bytes)).TrimEnd('=');
		}

		static void AddNodeToCluster(IServiceProvider services, string nodeId, string address, int processId)
		{
			var proxyConfigProvider = services.GetRequiredService<IProxyConfigProvider>() as InMemoryConfigProvider;
			var sessionSecretStore = services.GetRequiredService<SessionSecretStore>();
			var clusterId = proxyConfigProvider.GetConfig().Clusters.Single().ClusterId;

			proxyConfigProvider.AddNodeToCluster(clusterId, nodeId, address, processId);
			sessionSecretStore[nodeId] = "Dummy_CW_Session_Token";
		}

		public class ConfigureIPAddressRewriter : IStartupFilter
		{
			public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
			{
				return app =>
				{
					app.Use(async (context, next) =>
					{
						context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
						await next();
					});
					next(app);
				};
			}
		}

		sealed class DummyAppServerProcess : IAsyncDisposable
		{
			readonly Process process;
			readonly WebApplication app;

			public int ProcessId => process.Id;

			public string Address { get; } = "http://localhost:5000";

			public string UniqueId { get; } = Guid.NewGuid().ToString();

			public DummyAppServerProcess()
			{
				// A dummy windows process is required for the SessionBroker health checks
				process = Process.Start("CargoWise.Blazor.SessionBroker.Test.MockAppServer.exe", "--wait -1");

				// A simple web app to handle requests
				var builder = WebApplication.CreateBuilder();
				builder.WebHost.UseUrls(Address);
				app = builder.Build();
				app.MapGet("/", () => "Hello World!");
				app.MapGet("/headers", (context) => context.Response.WriteAsJsonAsync(context.Request.Headers.Select(h => new KeyValuePair<string, string>(h.Key, h.Value)).ToArray()));
			}

			public async Task StartAsync()
			{
				await app.StartAsync();
			}

			public async ValueTask DisposeAsync()
			{
				process.Kill();
				await app.StopAsync();
			}
		}
	}
}
