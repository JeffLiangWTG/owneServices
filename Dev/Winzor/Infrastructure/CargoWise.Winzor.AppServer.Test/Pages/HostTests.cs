using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.Winzor.Architecture.Test;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using WTG.ToxicNetworkEffect;

namespace CargoWise.Winzor.AppServer.Test.Pages
{
	using static PlaywrightTestContext;

	[TestFixture]
	public class HostTests
	{
		const string listen = "localhost:9000";
		const string upstream = "localhost:5000";
		const string proxyName = "appServerProxy";

		[Test, WithPlaywrightPage(UseTestClientIntegration = true)]
		public async Task OnConnectionDownCallsReportError()
		{
			await using var ctx = new InMemoryAppServerTestContext(new MockCargoWiseClientSeviceProvider());

			var url = await SetupTestForms(ctx);
			await Page.AddInitScriptAsync("window.cargoWiseClient = {reportConnectionDown: () => cargoWiseTesting.connectionDownReported = true}");

			var response = await Page.GotoAsync(url.ToString());
			Assert.That(response.Ok);

			await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
			await Page.EvaluateAsync("window.Blazor._internal.forceCloseConnection()");
			await Page.WaitForFunctionAsync("cargoWiseTesting.connectionDownReported");
			var errorReport = await Page.EvaluateAsync("cargoWiseTesting.connectionDownReported");
			var errorReportJsonValue = errorReport.GetValueOrDefault().ValueKind;
			Assert.That(errorReportJsonValue, Is.EqualTo(JsonValueKind.True));
		}

		const string Listen = "127.0.0.1:5001";
		const string Upstream = "127.0.0.1:5000";
		const string ProxyName = "test-proxy";
		const string ServerBaseUrl = "http://" + Listen;

		[Test, WithPlaywrightPage(UseTestClientIntegration = true), WithToxiProxy(Listen = Listen, Upstream = Upstream, ProxyName = ProxyName)]
		public async Task OnConnectionUpCallsReportHealthy()
		{
			await using (var ctx = new InMemoryAppServerTestContext())
			{
				var url = await SetupTestForms(ctx, ServerBaseUrl);
				await Page.AddInitScriptAsync(@"window.cargoWiseClient = {
reportConnectionDown: () => cargoWiseTesting.connectionDownReported = true,
reportConnectionUp: () => cargoWiseTesting.connectionUpReported = true
}");

				var response = await Page.GotoAsync(url.ToString());
				Assert.That(response.Ok);
				await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

				var proxy = await WithToxiProxy.GetProxy(ProxyName);
				proxy.Enabled = false;
				_ = await proxy.UpdateAsync();
				_ = await Page.WaitForFunctionAsync("cargoWiseTesting.connectionDownReported");
				proxy.Enabled = true;
				await proxy.UpdateAsync();

				_ = await Page.WaitForFunctionAsync("cargoWiseTesting.connectionUpReported");
				var errorReport = await Page.EvaluateAsync("cargoWiseTesting.connectionUpReported");
				var errorReportJsonValue = errorReport.GetValueOrDefault().ValueKind;
				Assert.That(errorReportJsonValue, Is.EqualTo(JsonValueKind.True));
			}

			// Because we are purposely disconnecting and reconnecting the page we need to clear errors
			// captured by the playwright test context that were caused by the network connection dropping
			WithPlaywrightPageAttribute.Context.ConsoleMessages.RemoveAll(error =>
				error.Text.Contains("WebSocket closed with status code: 1006") ||
				error.Text.Contains("Failed to load resource: net::ERR_CONNECTION_REFUSED") ||
				error.Text.Contains("Failed to complete negotiation with the server: TypeError: Failed to fetch")
			);

			WithPlaywrightPageAttribute.Context.PageErrors.RemoveAll(error =>
				error.Contains("Cannot send data if the connection is not in the 'Connected' State")
			);
		}

		[Test, WithPlaywrightPage(UseTestClientIntegration = true), WithToxiProxy(Listen = Listen, Upstream = Upstream, ProxyName = ProxyName)]
		public async Task ReconnectModalRemovedAfterSuccess()
		{
			await using var ctx = new InMemoryAppServerTestContext(new MockCargoWiseClientSeviceProvider());
			var url = await SetupTestForms(ctx, ServerBaseUrl);
			var response = await Page.GotoAsync(url.ToString());
			Assert.That(response.Ok, Is.True);

			await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
			Func<IElementHandle, Task<string>> getElDisplayStyle = async element =>
			{
				var displayJson = await element.EvaluateAsync("element => element.style.display");
				var displayStyle = displayJson.GetValueOrDefault().ToString();
				return displayStyle;
			};

			var proxy = await WithToxiProxy.GetProxy(ProxyName);
			proxy.Enabled = false;
			await proxy.UpdateAsync();
			var reconnectionModal = Page.Locator(".components-reconnect-show");
			await reconnectionModal.WaitForAsync();
			Assert.That(async () => await reconnectionModal.First.IsVisibleAsync(), Is.True.After(10000, 100));

			proxy.Enabled = true;
			await proxy.UpdateAsync();
			Assert.That(async () => await reconnectionModal.First.IsVisibleAsync(), Is.True.After(30000, 100));

			// Because we are purposely disconnecting and reconnecting the page we need to clear errors
			// captured by the playwright test context that were caused by the network connection dropping
			WithPlaywrightPageAttribute.Context.ConsoleMessages.RemoveAll(error =>
				error.Text.Contains("WebSocket closed with status code: 1006") ||
				error.Text.Contains("Failed to load resource: net::ERR_CONNECTION_REFUSED") ||
				error.Text.Contains("Failed to complete negotiation with the server: TypeError: Failed to fetch")
			);

			WithPlaywrightPageAttribute.Context.PageErrors.RemoveAll(error =>
				error.Contains("Cannot send data if the connection is not in the 'Connected' State")
			);
		}

		[Test, WithPlaywrightPage]
		[WithLatencyNetworkEffect(Listen = listen, Upstream = upstream, ProxyName = proxyName, Latency = 1000)]
		public async Task TestPageLoadsCorrectlyWithHighLatency()
		{
			await using var ctx = new InMemoryAppServerTestContext(new MockCargoWiseClientSeviceProvider());
			// WithLatencyNetworkEffect create a TCP proxy from listen to upstream and apply Latency to  listen address
			// we don't actually care about upstream here since we can directly register form to listen address
			var url = await SetupTestForms(ctx, $"http://{listen}");
			var goToTask = Page.GotoAsync(url.ToString());
			Assert.That(() => goToTask.IsCompleted, Is.False.After(3000));
			var response = await goToTask;
			Assert.That(response?.Ok, Is.True);

			var formEl = await Page.WaitForSelectorAsync(".form");
			Assert.That(formEl, Is.Not.Null);
			Assert.That(formEl, Is.InstanceOf<IElementHandle>());
		}

		[Test, WithPlaywrightPage]
		public async Task HostOutputUsesCacheBuster()
		{
			await using var ctx = new InMemoryAppServerTestContext(new MockCargoWiseClientSeviceProvider());
			var url = default(Uri);
			await ctx.WinzorDispatcher.InvokeAsync(() =>
			{
				url = new Uri("http://localhost:5000/");
			});
			var response = await Page.GotoAsync(url.ToString());
			Assert.That(response.Ok);

			var scripts = await Page.EvaluateAsync<string[]>("Array.from(document.querySelectorAll('script[src]')).map(s => s.src)");
			Assert.That(scripts.Length, Is.GreaterThan(0));
			foreach (var scriptSrc in scripts)
			{
				if (scriptSrc.Contains("blazor.web.js"))
				{
					// For some reason, the blazor.web.js script won't add a cache buster manually
					// However we manually add a version parameter to the script to differentiate between .net versions of the Blazor script
					Assert.That(scriptSrc, Does.Contain("?version=8"));
				}
				else
				{
					Assert.That(scriptSrc, Does.Contain("?v="));
				}
			}

			var links = await Page.EvaluateAsync<string[]>("Array.from(document.querySelectorAll('link[href]')).map(l => l.href)");
			Assert.That(links.Length, Is.GreaterThan(0));
			foreach (var linkHref in links)
			{
				Assert.That(linkHref, Does.Contain("?v="));
			}

			await Page.CloseAsync();
		}

		async Task<Uri> SetupTestForms(InMemoryAppServerTestContext ctx, string host = $"http://{upstream}")
		{
			var url = default(Uri);
			Form form = null;

			await ctx.WinzorDispatcher.InvokeAsync(() =>
			{
				form = new Form();
				url = ctx.WinzorDispatcher.FormInstanceRegister.Add(new Uri(host), form);
			});
			return url;
		}
	}
}
