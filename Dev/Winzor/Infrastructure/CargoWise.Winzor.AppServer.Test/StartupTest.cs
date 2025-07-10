using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using CargoWise.Blazor.Common;
using CargoWise.Winzor.Telemetry;
using Enterprise.Winzor.Architecture;
using Enterprise.Winzor.Architecture.Test;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using NUnit.Framework;
using OpenTelemetry;
using OpenTelemetry.Trace;
using WinzorFramework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace CargoWise.Winzor.AppServer.Test
{
	using static PlaywrightTestContext;

	class StartupTest
	{
		[WithPlaywrightPage]
		[TestCase("/_content/WinzorFramework/js/module/overlay.js", TestName = "{m}_js")]
		[TestCase("/_content/WinzorFramework/js/module/overlay.js?v=111", TestName = "{m}_WrongVersion")]
		[TestCase("/CargoWise.Winzor.combined.css", TestName = "{m}_css")]
		public async Task TestRedirectVersionedStaticFile(string url)
		{
			await using var ctx = new InMemoryAppServerTestContext();

			IResponse redirect = null;
			Page.Response += (_, r) => redirect ??= r;

			await Page.GotoAsync(ctx.ServerBaseUrl + url);
			await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

			Assert.That(redirect.Status, Is.EqualTo(302));
			Assert.That(redirect.Headers, Does.ContainKey("cache-control"));
			Assert.That(redirect.Headers["cache-control"], Does.Contain("private, max-age="));

			await Page.CloseAsync();
		}

		[Test, WithPlaywrightPage]
		public async Task TestNoRedirectVersionedFile()
		{
			await using var ctx = new InMemoryAppServerTestContext();

			var responses = new List<IResponse>();
			Page.Response += (_, r) => responses.Add(r);

			var page = await ctx.LoadFormAsync(() => new Form());
			await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

			responses = responses.Where(r => r.Url.Contains(".js?v=") || r.Url.Contains(".css?v=")).ToList();
			Assert.That(responses, Is.Not.Empty);

			foreach (var r in responses)
			{
				Assert.That(r.Status, Is.EqualTo(200));
			}
		}

		[Test, WithPlaywrightPage]
		[TestCase("/_content/WinzorFramework/js/module/overlay.js", TestName = "{m}_js")]
		[TestCase("/_content/WinzorFramework/js/module/overlay.js?v=111", TestName = "{m}_WrongVersion")]
		[TestCase("/CargoWise.Winzor.combined.css", TestName = "{m}_css")]
		public async Task TestCacheVersionedStaticFile(string url)
		{
			await using var ctx = new InMemoryAppServerTestContext();

			var response = await Page.GotoAsync(ctx.ServerBaseUrl + url);
			await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

			Assert.That(response.Ok);
			var query = HttpUtility.ParseQueryString(new Uri(response.Url).Query);
			Assert.That(query["v"], Is.Not.Null);
			Assert.That(query["v"], Is.Not.Empty);
			Assert.That(response.Headers, Does.ContainKey("cache-control"));
			Assert.That(response.Headers["cache-control"], Does.Contain("public, max-age="));

			await Page.CloseAsync();
		}

		[Test, WithPlaywrightPage]
		[TestCase("/favicon.ico", TestName = "{m}_ico")]
		[TestCase("/_content/CargoWise.GUI.TileBar/images/search.png", TestName = "{m}_png")]
		[TestCase("/_content/WinzorFramework/fonts/material-symbols-outlined.woff2", TestName = "{m}_woff2")]
		[TestCase("/_content/WinzorFramework/images/cursors/uparrow.cur", TestName = "{m}_cur")]
		[TestCase("/_content/CargoWise.NetworkVisualisation.GUI/images/Edit.svg", TestName = "{m}_svg")]
		[TestCase("/_content/CargoWise.NetworkVisualisation.GUI/icons/asterisk.bmp", TestName = "{m}_bmp")]
		public async Task TestCacheNonVersionedStaticFile(string url)
		{
			await using var ctx = new InMemoryAppServerTestContext();

			IResponse response = null;
			try
			{
				Page.Response += (_, r) => response ??= r;
				response = await Page.GotoAsync(ctx.ServerBaseUrl + url, new PageGotoOptions
				{
					WaitUntil = WaitUntilState.Commit
				});
			}
			catch (PlaywrightException)
			{
				//playwright throws net::ERR_ABORTED after downloading a file.
			}

			Assert.That(response.Ok);
			Assert.That(response.Headers, Does.ContainKey("cache-control"));
			Assert.That(response.Headers["cache-control"], Does.Contain("public, max-age="));

			await Page.CloseAsync();
		}

		[Test, WithPlaywrightPage]
		public async Task TestNoCacheOnPage()
		{
			await using var ctx = new InMemoryAppServerTestContext();

			var response = await Page.GotoAsync(ctx.ServerBaseUrl);
			await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

			Assert.That(response.Headers, Does.ContainKey("cache-control"));
			Assert.That(response.Headers["cache-control"], Does.Contain("no-cache"));

			await Page.CloseAsync();
		}

		[Test, WithPlaywrightPage]
		[TestCase("/_content/WinzorFramework/js/module/overlay.js", TestName = "{m}_unversioned_redirects_to_versioned_file")]
		[TestCase("/_content/WinzorFramework/js/module/overlay.js?v=12345", TestName = "{m}_versioned_file_doesnt_redirect")]
		public async Task TestActionRequestDoesNotRedirectInfinitely(string asssetPath)
		{
			await using var ctx = new InMemoryAppServerTestContext(needCargoWiseRuntime: true);
			var testUrl = ctx.ServerBaseUrl + asssetPath;
			var url = default(Uri);
			await ctx.WinzorDispatcher.InvokeAsync(() => { url = new Uri(testUrl); });

			var jsRequestURL = string.Empty;
			var nextRequestURL = string.Empty;

			Page.Request += (_, request) =>
			{
				if (request.Url == testUrl && string.IsNullOrEmpty(jsRequestURL) && string.IsNullOrEmpty(nextRequestURL))
				{
					jsRequestURL = request.Url;
				}
				else if (!string.IsNullOrEmpty(jsRequestURL) && string.IsNullOrEmpty(nextRequestURL))
				{
					nextRequestURL = request.Url;
				}
			};

			var response = await Page.GotoAsync(url.ToString(), new PageGotoOptions
			{
				WaitUntil = WaitUntilState.Commit
			});
			Assert.That(response.Ok);
			await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

			await Task.Delay(1000);

			// Required to confirm that the hook event asserts are actually run
			Assert.That(jsRequestURL, Is.Not.EqualTo(nextRequestURL));

			await Page.CloseAsync();
		}

		[TestCase(typeof(IOpeningFormQueue))]
		[TestCase(typeof(EntryPointPool))]
		[TestCase(typeof(IFormOpener))]
		[TestCase(typeof(IFormInstanceRegister))]
		public void TypeIsRegisteredAsSingletonOnStartup(Type type)
		{
			using var factory = new AppServerWebApplicationFactory();

			var instance1 = factory.Services.GetRequiredService(type);
			var instance2 = factory.Services.GetRequiredService(type);

			Assert.That(instance1, Is.SameAs(instance2));
		}

		[Test]
		public void CorrectFormOpenerRegisteredForPoolingMode([Values] bool enabled)
		{
			using var originalFactory = new AppServerWebApplicationFactory();
			using var factory = originalFactory.WithWebHostBuilder(c => c.UseSetting($"{nameof(EntryPointPoolOptions)}:{nameof(EntryPointPoolOptions.Enabled)}", $"{enabled}"));

			var formOpener = factory.Services.GetRequiredService<IFormOpener>();

			if (enabled)
			{
				Assert.That(formOpener, Is.InstanceOf<PoolingFormOpener>());
			}
			else
			{
				Assert.That(formOpener, Is.InstanceOf<FormOpener>());
			}
		}

		[Test]
		public async Task FormChannelAllowsMultipleReaders()
		{
			await using var factory = new AppServerWebApplicationFactory();
			var channel = factory.Services.GetRequiredService<IOpeningFormQueue>();

			var task1 = channel.ReadAsync();
			var task2 = channel.ReadAsync();

			Form form1 = null;
			Form form2 = null;
			await factory.Services.GetRequiredService<WinzorDispatcher>().InvokeAsync(() =>
			{
				form1 = new Form();
				form2 = new Form();
			});

			channel.Write(form1);
			channel.Write(form2);

			using (form1)
			using (form2)
			{
				var result1 = await task1;
				var result2 = await task2;
				Assert.That(result1, Is.SameAs(form1));
				Assert.That(result2, Is.SameAs(form2));
			}
		}

		[Test]
		public async Task TestEntryPointPoolOptionsHaveCorrectDefaultsConfigured()
		{
			await using var factory = new AppServerWebApplicationFactory();
			var options = factory.Services.GetRequiredService<IOptions<EntryPointPoolOptions>>();
			var value = options.Value;

			Assert.That(value, Is.Not.Null);
			Assert.That(value.Enabled, Is.True);
			Assert.That(value.PoolFillWaitPeriod, Is.EqualTo(TimeSpan.FromMilliseconds(220)));
			Assert.That(value.MinimumPoolSize, Is.EqualTo(2));
		}

		[Test]
		public async Task TestEntryPointPoolOptionsThrowsWhenConfigIsInvalid()
		{
			await using var originalFactory = new AppServerWebApplicationFactory();
			using var factory = originalFactory.WithWebHostBuilder(configuration =>
			{
				ArgumentNullException.ThrowIfNull(configuration);
				configuration.UseSetting($"{nameof(EntryPointPoolOptions)}:{nameof(EntryPointPoolOptions.MinimumPoolSize)}", "0");
			});
			var settings = factory.Services.GetRequiredService<IOptions<EntryPointPoolOptions>>();
			Assert.That(() => settings.Value, Throws.Exception.TypeOf<OptionsValidationException>());
		}

		[WithPlaywrightPage(IgnoreHTTPSErrors = true)]
		// Everything should always be compressed when HTTP
		[TestCase(false, "/", "", true, TestName = "{m}_HTTP_HTML")]
		[TestCase(false, "/_content/WinzorFramework/js/module/overlay.js", "", true, TestName = "{m}_HTTP_JS")]
		[TestCase(false, "/CargoWise.Winzor.combined.css", "", true, TestName = "{m}_HTTP_CSS")]
		[TestCase(false, "/_content/CargoWise.NetworkVisualisation.GUI/images/Cog.svg", "", true, TestName = "{m}_HTTP_SVG")]
		[TestCase(false, "/_content/CargoWise.NetworkVisualisation.GUI/icons/arrow-down.png", "", false, TestName = "{m}_HTTP_PNG")] // An already compressed image should not have response compression
		// Only static assets should be compressed when HTTPS
		[TestCase(true, "/", "", false, TestName = "{m}_HTTPS_HTML")] // Not a static asset - should not be compressed
		[TestCase(true, "/_content/WinzorFramework/js/module/overlay.js", "", true, TestName = "{m}_HTTPS_JS")]
		[TestCase(true, "/CargoWise.Winzor.combined.css", "", true, TestName = "{m}_HTTPS_CSS")]
		[TestCase(true, "/_content/CargoWise.NetworkVisualisation.GUI/images/Cog.svg", "", true, TestName = "{m}_HTTPS_SVG")]
		[TestCase(true, "/_content/CargoWise.NetworkVisualisation.GUI/icons/arrow-down.png", "", false, TestName = "{m}_HTTPS_PNG")] // An already compressed image should not have response compression
		// Only static assets should be compressed when HTTP with forwarded HTTPS Header
		[TestCase(false, "/", "https", false, TestName = "{m}_ForwardedHTTPS_HTML")] // Not a static asset - should not be compressed
		[TestCase(false, "/_content/WinzorFramework/js/module/overlay.js", "https", true, TestName = "{m}_ForwardedHTTPS_JS")]
		[TestCase(false, "/CargoWise.Winzor.combined.css", "https", true, TestName = "{m}_ForwardedHTTPS_CSS")]
		[TestCase(false, "/_content/CargoWise.NetworkVisualisation.GUI/images/Cog.svg", "https", true, TestName = "{m}_ForwardedHTTPS_SVG")]
		[TestCase(false, "/_content/CargoWise.NetworkVisualisation.GUI/icons/arrow-down.png", "https", false, TestName = "{m}_ForwardedHTTPS_PNG")] // An already compressed image should not have response compression
		public async Task TestResponseCompression(bool urlIsHttps, string assetPath, string forwardedProtocol, bool shouldBeCompressed)
		{
			if (!string.IsNullOrEmpty(forwardedProtocol))
			{
				await BrowserContext.SetExtraHTTPHeadersAsync(new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("X-Forwarded-Proto", forwardedProtocol) });
			}

			await using var ctx = new InMemoryAppServerTestContext(useHttps: urlIsHttps);
			var page = await ctx.LoadFormAsync(() => new Form());

			page.Request += (_, request) => { };
			var response = await page.GotoAsync(ctx.ServerBaseUrl + assetPath, new PageGotoOptions { WaitUntil = WaitUntilState.Commit });
			Assert.That(response.Ok);

			if (shouldBeCompressed)
			{
				Assert.That(response.Headers, Does.ContainKey("content-encoding"));

				var encodings = new[] { "br", "gzip" };
				var isCompressed = encodings.Any(encoding => response.Headers["content-encoding"].Contains(encoding));
				Assert.That(isCompressed, Is.True, "The response should be compressed with either 'br' or 'gzip' encoding when compression is expected.");
			}
			else
			{
				Assert.That(response.Headers, Does.Not.ContainKey("content-encoding"), "The 'content-encoding' header should not be present when compression is not expected.");
			}
		}

		[Test]
		public async Task TestTelemetrySetting()
		{
			await using var originalFactory = new AppServerWebApplicationFactory();
			using var factory = originalFactory.WithWebHostBuilder(builder => builder.UseSetting("CargoWiseOptions:DatabaseName", "DatabaseName"));

			var tracer = factory.Services.GetRequiredService<TracerProvider>();
			var resource = tracer.GetResource();
			var attributes = resource.Attributes.ToDictionary(p => p.Key, p => p.Value);
			Assert.That(attributes["cargowise.dbname"], Is.EqualTo("DatabaseName"));
		}

		[Test]
		public async Task TestReadTelemetryOptionsValueFromConfiguration()
		{
			await using var originalFactory = new AppServerWebApplicationFactory();
			using var factory = originalFactory.WithWebHostBuilder(builder =>
			{
				builder.UseEnvironment("OneTimeTest");
				builder.UseSetting("TelemetryOptions:MetricExportInterval", TimeSpan.FromHours(123456).ToString());
			});
			var options = factory.Services.GetRequiredService<IOptions<TelemetryOptions>>();

			Assert.That(options.Value.MetricExportInterval, Is.Not.EqualTo(new TelemetryOptions().MetricExportInterval));
			Assert.That(options.Value.MetricExportInterval, Is.EqualTo(TimeSpan.FromHours(123456)));
		}
	}
}
