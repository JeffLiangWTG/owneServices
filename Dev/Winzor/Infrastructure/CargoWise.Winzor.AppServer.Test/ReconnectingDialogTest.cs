using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.Winzor.Architecture.Test;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace CargoWise.Winzor.AppServer.Test
{
	class ReconnectingDialogTest
	{
		static IEnumerable<TestCaseData> DisconnectTestCases
		{
			get
			{
				yield return new TestCaseData("Exit", "Exit", "exit") { TestName = "{m}_Exit" };
				yield return new TestCaseData("Restart", "Exit and restart", "restart") { TestName = "{m}_Restart" };
			}
		}

		[Test, WithPlaywrightPage, WithDefaultLatencyNetworkEffect]
		public async Task ReconnectingDialogOnlyVisibleWhenReconnecting()
		{
			await using (var ctx = new InMemoryAppServerTestContext())
			{
				var page = await ctx.LoadFormWithScriptAsync(() => new Form(), ToxiProxyHelper.CustomReconnectionOptions(10, TimeSpan.FromMinutes(1)), WithDefaultLatencyNetworkEffect.ServerBaseUrl);

				Assert.That(await page.Locator(".connection-lost-dialog__header").First.IsVisibleAsync(), Is.False);
				Assert.That(await page.Locator(".connection-lost-dialog__content").First.IsVisibleAsync(), Is.False);
				Assert.That(await page.Locator(".connection-lost-dialog__buttons").First.IsVisibleAsync(), Is.False);

				await WithDefaultLatencyNetworkEffect.DisconnectProxyAsync();

				Assert.That(async () => await page.Locator(".connection-lost-dialog__header").First.IsVisibleAsync(), Is.True.After(10000, 500));
				Assert.That(async () => await page.Locator(".connection-lost-dialog__content").First.IsVisibleAsync(), Is.True.After(10000, 500));
				Assert.That(async () => await page.Locator(".connection-lost-dialog__buttons").First.IsVisibleAsync(), Is.True.After(10000, 500));

				Assert.That(async () => await page.Locator(".connection-lost-dialog__header").First.InnerTextAsync(), Is.EqualTo("Connection lost"));
				Assert.That(async () => await page.Locator(".connection-lost-dialog__content").First.InnerTextAsync(), Is.EqualTo("Attempting to reconnect to your session.\n\nPlease check your network connection.\n\nIf you decide to disconnect all unsaved changes will be lost.\n\n1 out of 10 attempts"));
				Assert.That(async () => await page.Locator(".connection-lost-dialog__buttons").First.InnerTextAsync(), Is.EqualTo("Exit\nRestart"));
			}

			ToxiProxyHelper.CleanupPageErrors();
		}

		[TestCaseSource(nameof(DisconnectTestCases)), WithPlaywrightPage, WithDefaultLatencyNetworkEffect]
		public async Task ConfirmationDialogWillShowWhenAttemptingToDisconnectWhileReconnecting(string disconnectButtonText, string confirmButtonText, string disconnectButtonAction)
		{
			await using (var ctx = new InMemoryAppServerTestContext())
			{
				var page = await ctx.LoadFormWithScriptAsync(() => new Form(), ToxiProxyHelper.CustomReconnectionOptions(10, TimeSpan.FromSeconds(2)), WithDefaultLatencyNetworkEffect.ServerBaseUrl);

				await WithDefaultLatencyNetworkEffect.DisconnectProxyAsync();
				Assert.That(async () => await page.Locator(".connection-lost-dialog__header").First.InnerTextAsync(), Is.EqualTo("Connection lost").After(10000, 500));
				await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = disconnectButtonText }).ClickAsync();

				Assert.That(async () => await page.Locator(".connection-lost-dialog__header").First.InnerTextAsync(), Is.EqualTo("Disconnect session").After(3000, 100));
				Assert.That(async () => await page.Locator(".connection-lost-dialog__content").First.InnerTextAsync(), Is.EqualTo("If you decide to disconnect all unsaved changes will be lost.").After(3000, 100));
				Assert.That(async () => await page.Locator(".connection-lost-dialog__buttons").First.InnerTextAsync(), Is.EqualTo($"Continue to wait\n{confirmButtonText}").After(3000, 100));
			}

			ToxiProxyHelper.CleanupPageErrors();
		}

		[TestCaseSource(nameof(DisconnectTestCases)), WithPlaywrightPage, WithDefaultLatencyNetworkEffect]
		public async Task ConfirmationDialogWillReturnToReconnectingDialogWhenContinueWaiting(string disconnectButtonText, string confirmButtonText, string disconnectButtonAction)
		{
			await using (var ctx = new InMemoryAppServerTestContext())
			{
				var page = await ctx.LoadFormWithScriptAsync(() => new Form(), ToxiProxyHelper.CustomReconnectionOptions(10, TimeSpan.FromSeconds(2)), WithDefaultLatencyNetworkEffect.ServerBaseUrl);

				await WithDefaultLatencyNetworkEffect.DisconnectProxyAsync();
				Assert.That(async () => await page.Locator(".connection-lost-dialog__header").First.InnerTextAsync(), Is.EqualTo("Connection lost").After(10000, 500));
				await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = disconnectButtonText }).ClickAsync();

				Assert.That(async () => await page.Locator(".connection-lost-dialog__header").First.InnerTextAsync(), Is.EqualTo("Disconnect session").After(3000, 100));
				await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Continue to wait" }).ClickAsync();

				Assert.That(async () => await page.Locator(".connection-lost-dialog__header").First.InnerTextAsync(), Is.EqualTo("Connection lost").After(3000, 100));
				Assert.That(async () => await page.Locator(".connection-lost-dialog__buttons").First.InnerTextAsync(), Is.EqualTo("Exit\nRestart"));
			}

			ToxiProxyHelper.CleanupPageErrors();
		}

		[TestCaseSource(nameof(DisconnectTestCases)), WithPlaywrightPage, WithDefaultLatencyNetworkEffect]
		public async Task ConfirmationDialogCanDisconnectSession(string disconnectButtonText, string confirmButtonText, string disconnectButtonAction)
		{
			await using (var ctx = new InMemoryAppServerTestContext())
			{
				var page = await ctx.LoadFormWithScriptAsync(() => new Form(), ToxiProxyHelper.CustomReconnectionOptions(10, TimeSpan.FromSeconds(2)), WithDefaultLatencyNetworkEffect.ServerBaseUrl);

				await page.AddScriptTagAsync(new PageAddScriptTagOptions { Content = $"window.disconnectActionRaised = false; window.{disconnectButtonAction} = () => window.disconnectActionRaised = true;" });

				await WithDefaultLatencyNetworkEffect.DisconnectProxyAsync();
				Assert.That(async () => await page.Locator(".connection-lost-dialog__header").First.InnerTextAsync(), Is.EqualTo("Connection lost").After(10000, 500));
				await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = disconnectButtonText }).ClickAsync();

				Assert.That(async () => await page.Locator(".connection-lost-dialog__header").First.InnerTextAsync(), Is.EqualTo("Disconnect session").After(3000, 100));
				await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = confirmButtonText }).ClickAsync();

				Assert.That(async () => await page.EvaluateAsync<bool>("window.disconnectActionRaised"), Is.EqualTo(true).After(3000, 100));
			}

			ToxiProxyHelper.CleanupPageErrors();
		}

		[TestCaseSource(nameof(DisconnectTestCases)), WithPlaywrightPage, WithDefaultLatencyNetworkEffect]
		public async Task ConfirmationDialogHiddenWhenConnectionRestored(string disconnectButtonText, string confirmButtonText, string disconnectButtonAction)
		{
			await using (var ctx = new InMemoryAppServerTestContext())
			{
				var page = await ctx.LoadFormWithScriptAsync(() => new Form(), ToxiProxyHelper.CustomReconnectionOptions(10, TimeSpan.FromSeconds(2)), WithDefaultLatencyNetworkEffect.ServerBaseUrl);

				await WithDefaultLatencyNetworkEffect.DisconnectProxyAsync();
				Assert.That(async () => await page.Locator(".connection-lost-dialog__header").First.InnerTextAsync(), Is.EqualTo("Connection lost").After(3000, 100));
				await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = disconnectButtonText }).ClickAsync();

				Assert.That(async () => await page.Locator(".connection-lost-dialog__header").First.InnerTextAsync(), Is.EqualTo("Disconnect session").After(3000, 100));

				await WithDefaultLatencyNetworkEffect.ReconnectProxyAsync();

				// The .After() in the Assert below will wait until the session is successfully reconnected
				Assert.That(async () => await page.Locator(".connection-lost-dialog").First.IsVisibleAsync(), Is.EqualTo(false).After(10000, 100));
				Assert.That(async () => await page.Locator(".connection-lost-dialog__header").First.IsVisibleAsync(), Is.EqualTo(false));
			}

			ToxiProxyHelper.CleanupPageErrors();
		}

		[TestCaseSource(nameof(DisconnectTestCases)), WithPlaywrightPage, WithDefaultLatencyNetworkEffect]
		public async Task ConfirmationDialogHiddenWhenCannotReconnect(string disconnectButtonText, string confirmButtonText, string disconnectButtonAction)
		{
			await using (var ctx = new InMemoryAppServerTestContext())
			{
				var page = await ctx.LoadFormWithScriptAsync(() => new Form(), ToxiProxyHelper.CustomReconnectionOptions(1, TimeSpan.FromSeconds(2)), WithDefaultLatencyNetworkEffect.ServerBaseUrl);

				await WithDefaultLatencyNetworkEffect.DisconnectProxyAsync();
				Assert.That(async () => await page.Locator(".connection-lost-dialog__header").First.InnerTextAsync(), Is.EqualTo("Connection lost").After(3000, 100));
				await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = disconnectButtonText }).ClickAsync();

				Assert.That(async () => await page.Locator(".connection-lost-dialog__header").First.InnerTextAsync(), Is.EqualTo("Disconnect session").After(3000, 100));

				// The .After() in the Assert below will wait until the reconnect attempts have all failed and the session is not recoverable
				Assert.That(async () => await page.Locator(".connection-lost-dialog__header").First.InnerTextAsync(), Is.EqualTo("Reconnection failed").After(10000, 100));
				Assert.That(async () => await page.Locator(".connection-lost-dialog__buttons").First.InnerTextAsync(), Is.EqualTo("Exit\nRestart"));
			}

			ToxiProxyHelper.CleanupPageErrors();
		}

		[Test, WithPlaywrightPage, WithDefaultLatencyNetworkEffect]
		public async Task DisconnectedDialogVisibleWhenCannotReconnect()
		{
			await using (var ctx = new InMemoryAppServerTestContext())
			{
				var page = await ctx.LoadFormWithScriptAsync(() => new Form(), ToxiProxyHelper.CustomReconnectionOptions(1, TimeSpan.FromSeconds(2)), WithDefaultLatencyNetworkEffect.ServerBaseUrl);

				await WithDefaultLatencyNetworkEffect.DisconnectProxyAsync();
				Assert.That(async () => await page.Locator(".connection-lost-dialog__header").First.InnerTextAsync(), Is.EqualTo("Connection lost").After(3000, 100));

				// The .After() in the Assert below will wait until the reconnect attempts have all failed and the session is not recoverable
				Assert.That(async () => await page.Locator(".connection-lost-dialog__header").First.InnerTextAsync(), Is.EqualTo("Reconnection failed").After(10000, 100));
				Assert.That(async () => await page.Locator(".connection-lost-dialog__content").First.InnerTextAsync(), Is.EqualTo("Could not reconnect to your session.\n\nYou can restart the application.\n\nYour windows will re-open but all unsaved changes have been lost.").After(10000, 100));
				Assert.That(async () => await page.Locator(".connection-lost-dialog__buttons").First.InnerTextAsync(), Is.EqualTo("Exit\nRestart").After(10000, 100));
			}

			ToxiProxyHelper.CleanupPageErrors();
		}

		[TestCaseSource(nameof(DisconnectTestCases)), WithPlaywrightPage, WithDefaultLatencyNetworkEffect]
		public async Task DisconnectedDialogCanExit(string disconnectButtonText, string confirmButtonText, string disconnectButtonAction)
		{
			await using (var ctx = new InMemoryAppServerTestContext())
			{
				var page = await ctx.LoadFormWithScriptAsync(() => new Form(), ToxiProxyHelper.CustomReconnectionOptions(1, TimeSpan.FromSeconds(2)), WithDefaultLatencyNetworkEffect.ServerBaseUrl);

				await page.AddScriptTagAsync(new PageAddScriptTagOptions { Content = $"window.disconnectActionRaised = false; window.{disconnectButtonAction} = () => window.disconnectActionRaised = true;" });

				await WithDefaultLatencyNetworkEffect.DisconnectProxyAsync();
				Assert.That(async () => await page.Locator(".connection-lost-dialog__header").First.InnerTextAsync(), Is.EqualTo("Connection lost").After(3000, 100));

				// The .After() in the Assert below will wait until the reconnect attempts have all failed and the session is not recoverable
				Assert.That(async () => await page.Locator(".connection-lost-dialog__header").First.InnerTextAsync(), Is.EqualTo("Reconnection failed").After(10000, 100));
				await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = disconnectButtonText }).ClickAsync();

				Assert.That(async () => await page.EvaluateAsync<bool>("window.disconnectActionRaised"), Is.EqualTo(true).After(3000, 100));
			}

			ToxiProxyHelper.CleanupPageErrors();
		}

		[TestCaseSource(nameof(DisconnectTestCases)), WithPlaywrightPage, WithDefaultLatencyNetworkEffect]
		public async Task ReconnectingDialogButtonsAreClickableWhenNonRespondingDialogVisible(string disconnectButtonText, string confirmButtonText, string disconnectButtonAction)
		{
			await using (var ctx = new InMemoryAppServerTestContext())
			{
				Form form = null;
				var page = await ctx.LoadFormWithScriptAsync(() => form = new Form(), ToxiProxyHelper.CustomReconnectionOptions(10, TimeSpan.FromSeconds(2)), WithDefaultLatencyNetworkEffect.ServerBaseUrl);

				// Make the dispatcher do something slow so we get a non responding overlay but don't wait for it to complete
				_ = form.InvokeWinzorDispatcherAsync(() => Thread.Sleep(TimeSpan.FromSeconds(10)));

				await page.Locator(".overlay--notresponding").WaitForAsync();

				await WithDefaultLatencyNetworkEffect.DisconnectProxyAsync();
				Assert.That(async () => await page.Locator(".connection-lost-dialog__header").First.InnerTextAsync(), Is.EqualTo("Connection lost").After(3000, 100));
				await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = disconnectButtonText }).ClickAsync();

				Assert.That(async () => await page.Locator(".connection-lost-dialog__header").First.InnerTextAsync(), Is.EqualTo("Disconnect session").After(3000, 100));
			}

			ToxiProxyHelper.CleanupPageErrors();
		}

		[TestCaseSource(nameof(DisconnectTestCases)), WithPlaywrightPage, WithDefaultLatencyNetworkEffect]
		public async Task ReconnectingDialogDisconnectButtonsCallCorrectClientAppAPIs(string disconnectButtonText, string confirmButtonText, string disconnectButtonAction)
		{
			await using (var ctx = new InMemoryAppServerTestContext())
			{
				var page = await ctx.LoadFormWithScriptAsync(() => new Form(), ToxiProxyHelper.CustomReconnectionOptions(10, TimeSpan.FromSeconds(2)), WithDefaultLatencyNetworkEffect.ServerBaseUrl);

				const string clientAppShutdownMethod = "cargoWiseClient.shutDownApplication";
				const string clientAppUriScheme = "cargowiseclient:";
				const string dummyCrashRecoveryUrl = "about:blank?dummyUrlToOpen";

				await page.AddScriptTagAsync(new PageAddScriptTagOptions { Content =
					$"window.clientAppShutdownMethodCalled = false; window.cargoWiseClient = {{}}; window.{clientAppShutdownMethod} = () => window.clientAppShutdownMethodCalled = true;" +
					$"window.openedWindowUrl = ''; window.open = (url) => window.openedWindowUrl = url;" +
					$"localStorage.crashRecoveryUrl = '{dummyCrashRecoveryUrl}';"
				});

				await WithDefaultLatencyNetworkEffect.DisconnectProxyAsync();

				await page.EvaluateAsync($"window.{disconnectButtonAction}()");

				Assert.That(async () => await page.EvaluateAsync<bool>("window.clientAppShutdownMethodCalled"), Is.True.After(3000, 100));

				if (disconnectButtonText == "Restart")
				{
					Assert.That(async () => await page.EvaluateAsync<string>("window.openedWindowUrl"), Is.EqualTo(clientAppUriScheme + dummyCrashRecoveryUrl).After(3000, 100));
				}
			}

			ToxiProxyHelper.CleanupPageErrors();
		}

		[Test, WithPlaywrightPage, WithDefaultLatencyNetworkEffect]
		public async Task ReconnectingDialogRestartWillUseOriginUrlIfNoCrashRecoveryUrlProvided()
		{
			await using (var ctx = new InMemoryAppServerTestContext())
			{
				var page = await ctx.LoadFormWithScriptAsync(() => new Form(), ToxiProxyHelper.CustomReconnectionOptions(10, TimeSpan.FromSeconds(2)), WithDefaultLatencyNetworkEffect.ServerBaseUrl);

				const string clientAppShutdownMethod = "cargoWiseClient.shutDownApplication";
				const string clientAppUriScheme = "cargowiseclient:";

				await page.AddScriptTagAsync(new PageAddScriptTagOptions
				{
					Content =
					$"window.clientAppShutdownMethodCalled = false; window.cargoWiseClient = {{}}; window.{clientAppShutdownMethod} = () => window.clientAppShutdownMethodCalled = true;" +
					$"window.openedWindowUrl = ''; window.open = (url) => window.openedWindowUrl = url;"
				});

				await WithDefaultLatencyNetworkEffect.DisconnectProxyAsync();

				await page.EvaluateAsync($"window.restart()");

				Assert.That(async () => await page.EvaluateAsync<bool>("window.clientAppShutdownMethodCalled"), Is.True.After(3000, 100));
				Assert.That(async () => await page.EvaluateAsync<string>("window.openedWindowUrl"), Is.EqualTo(clientAppUriScheme + "http://127.0.0.1:5001").After(3000, 100));
			}

			ToxiProxyHelper.CleanupPageErrors();
		}

		[Test, WithPlaywrightPage, WithDefaultLatencyNetworkEffect]
		public async Task ReconnectingDialogRendersWithCorrectFont()
		{
			await using (var ctx = new InMemoryAppServerTestContext())
			{
				var page = await ctx.LoadFormWithScriptAsync(() => new Form(), ToxiProxyHelper.CustomReconnectionOptions(10, TimeSpan.FromSeconds(20)), WithDefaultLatencyNetworkEffect.ServerBaseUrl);

				await WithDefaultLatencyNetworkEffect.DisconnectProxyAsync();

				var headerTextSelector = ".connection-lost-dialog__header span:visible";

				await page.Locator(headerTextSelector).WaitForAsync();

				Assert.That(async () => await GetRenderedFontForElement(page, headerTextSelector), Has.Exactly(1).EqualTo("Inter").After(3000, 500));
			}

			ToxiProxyHelper.CleanupPageErrors();
		}

		static async Task<string[]> GetRenderedFontForElement(IPage page, string selector)
		{
			var cdp = await page.Context.NewCDPSessionAsync(page);
			await cdp.SendAsync("DOM.enable");
			await cdp.SendAsync("CSS.enable");
			var document = await cdp.SendAsync("DOM.getDocument");
			var element = await cdp.SendAsync("DOM.querySelector", new () { { "nodeId", document.Value.GetProperty("root").GetProperty("nodeId").GetInt16() }, { "selector", "span" } });
			var platformFonts = await cdp.SendAsync("CSS.getPlatformFontsForNode", new () { { "nodeId", element.Value.GetProperty("nodeId").GetInt16() } });
			var fonts = platformFonts.Value.GetProperty("fonts").EnumerateArray().Select(f => f.GetProperty("familyName").GetString()).ToArray();
			await cdp.DetachAsync();
			return fonts;
		}
	}
}
