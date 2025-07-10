using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Blazor.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using Yarp.ReverseProxy.Forwarder;

namespace Enterprise.Winzor.Architecture.Test;

using static PlaywrightTestContext;

[TestFixture]
class ProductionEnvironmentTest
{
	const string SessionToken = "SessionToken";
	const string ProxyUrl = "https://localhost:5001";

	WebApplication authProxy;
	SocketsHttpHandler socketsHttpHandler;
	HttpMessageInvoker httpClient;

	[SetUp]
	public async Task Setup()
	{
		var installCert = Process.Start(new ProcessStartInfo("dotnet", " dev-certs https"));
		// We nee the session token to be included as this test simulates a production environment
		// Adding a session token header using page.SetExtraHTTPHeadersAsync does not work as playwright
		// does not apply the headers to websocket requests https://github.com/microsoft/playwright/issues/28948
		// Instead we will set up a reverse proxy to add the session token header for all requests
		var builder = WebApplication.CreateBuilder();
		builder.WebHost.UseUrls(ProxyUrl);
		builder.Services.AddReverseProxy();
		authProxy = builder.Build();

		socketsHttpHandler = new SocketsHttpHandler()
		{
			UseProxy = false,
			AllowAutoRedirect = false,
			AutomaticDecompression = DecompressionMethods.None,
			UseCookies = false,
			ActivityHeadersPropagator = new ReverseProxyPropagator(DistributedContextPropagator.Current),
			ConnectTimeout = TimeSpan.FromSeconds(15),
			SslOptions = new SslClientAuthenticationOptions
			{
				RemoteCertificateValidationCallback = (_, _, _, _) => true
			}
		};
		httpClient = new HttpMessageInvoker(socketsHttpHandler);
		var requestConfig = new ForwarderRequestConfig { ActivityTimeout = TimeSpan.FromSeconds(100) };
		authProxy.UseRouting();
		authProxy.MapForwarder("/{**catch-all}", "https://localhost:5000/", requestConfig, new AddHeaderTransformer { SessionToken = SessionToken }, httpClient);
		await installCert.WaitForExitAsync();
		await authProxy.StartAsync();
	}

	[TearDown]
	public async Task TearDown()
	{
		if (authProxy != null)
		{
			await authProxy.StopAsync();
			await authProxy.DisposeAsync();
		}

		socketsHttpHandler?.Dispose();
		httpClient?.Dispose();
	}

	[Test, WithPlaywrightPage(IgnoreHTTPSErrors = true)]
	public async Task AppServerThroughProxy_StatusCode200Check()
	{
		var clientServiceProvider = new MockCargoWiseClientSeviceProvider();
		await using var ctx = new InMemoryAppServerTestContext(clientServiceProvider, Environments.Production, new CargoWiseAuthOptions { SessionToken = SessionToken });
		var extraHeader = new List<KeyValuePair<string, string>>();
		extraHeader.Add(new KeyValuePair<string, string>("WTG-UserAgent", "CargoWiseClient"));
		await BrowserContext.SetExtraHTTPHeadersAsync(extraHeader);
		var url = await ctx.InitializeFormAsync(() => new Form());

		var page = await PageHelper.GetPageAsync();
		var response = await page.GotoAsync(ProxyUrl);

		Assert.That(response.Status, Is.EqualTo(200));

		// Need to do something better here to ensure the whole form is loaded without any errors
		// especially as the errors we are expecting will come from JS calls in OnAfterRenderAsync
		await Task.Delay(5000);

		await page.CloseAsync();
	}

	[Test, WithPlaywrightPage(IgnoreHTTPSErrors = true)]
	public async Task BlazorEndCircuitBeaconSentToAppServerOnPageClose()
	{
		var clientServiceProvider = new MockCargoWiseClientSeviceProvider();
		await using var ctx = new InMemoryAppServerTestContext(clientServiceProvider, Environments.Production, new CargoWiseAuthOptions { SessionToken = SessionToken });
		Form form = null;
		var url = await ctx.InitializeFormAsync(() => form = new Form(), ProxyUrl);
		var page = await PageHelper.GetPageAsync();

		/*
		 * The purpose of this test is to ensure that page close beacons are still able to reach the server despite any restrictions we put in place in a production environment
		 * If you need to make any modifications to this test to get the form to load... please consider if these changes should be unset once the form is loaded to ensure the beacon still works
		 */

		// Setup browser to simulate a production environment
		await page.SetExtraHTTPHeadersAsync(new List<KeyValuePair<string, string>>
		{
			new ("WTG-UserAgent", "CargoWiseClient"), // Added by ClientApp - should be unset in this test as there is a change the ClientApp may send a beacon after we remove the request handler which adds the header
		});

		var response = await page.GotoAsync(url.ToString());
		Assert.That(response.Status, Is.EqualTo(200));

		await page.WaitForSelectorAsync(".form");

		// Undo browser setup
		await page.SetExtraHTTPHeadersAsync(new List<KeyValuePair<string, string>>
		{
			new ("WTG-UserAgent", string.Empty)
		});

		Assert.That(form.Proxy, Is.Not.Null);
		await page.CloseAsync(new() { RunBeforeUnload = true });
		Assert.That(() => form.Proxy, Is.Null.After(3000, 100));
	}

	[Test, WithPlaywrightPage(IgnoreHTTPSErrors = true)]
	public async Task TestCSPExists()
	{
		var clientServiceProvider = new MockCargoWiseClientSeviceProvider();
		var authOption = new CargoWiseAuthOptions { SessionToken = "SessionToken" };
		await using var ctx = new InMemoryAppServerTestContext(clientServiceProvider, Environments.Production, authOption);
		await Page.SetExtraHTTPHeadersAsync(new List<KeyValuePair<string, string>>
			{
				new ("WTG-UserAgent", "CargoWiseClient")
			});
		var response = await Page.GotoAsync(ProxyUrl);
		Assert.That(response.Ok);

		var csp = Page.Locator("head > meta[http-equiv=Content-Security-Policy]");
		var content = await csp.GetAttributeAsync("content");
		Assert.That(content, Is.Not.Empty);
		Assert.That(content, Does.Not.Contain("unsafe-eval"));
		Assert.That(content, Does.Contain("upgrade-insecure-requests;"));
		await Page.CloseAsync();
	}

	class AddHeaderTransformer : HttpTransformer
	{
		public string SessionToken { get; init; }

		public override async ValueTask TransformRequestAsync(HttpContext httpContext, HttpRequestMessage proxyRequest, string destinationPrefix, CancellationToken cancellationToken)
		{
			await base.TransformRequestAsync(httpContext, proxyRequest, destinationPrefix, cancellationToken);

			proxyRequest.Headers.Add(CustomHeaders.CWSessionToken, SessionToken);
			// when running in production, the host url will be the ip address of the session broker
			// change the address so that tests which rely on the host of the request will fail
			proxyRequest.Headers.Host = "notlocalhost";
		}
	}
}
