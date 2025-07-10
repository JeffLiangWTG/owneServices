using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace CargoWise.Blazor.AppServer.Test
{
	using static PlaywrightTestContext;

	[TestFixture, BlazorWithPlaywrightPage]
	public class BlazorPlaywrightBasicTests
	{
		[Test]
		public async Task CargoWiseBlazorIsAccessible()
		{
			//test that a page can reach the cargowise blazor home page
			var requestUri = BlazorWithPlaywrightPageAttribute.BlazorAddress;

			// no need to wait for page load because we're testing the page response, not blazor appserver spin up
			var resp = await Page.GotoAsync(requestUri);

			Assert.That(resp.Ok);
		}

		[Test]
		public async Task CargoWiseBlazorHasNoErrors()
		{
			//test that navigating to the cargowise blazor home page throws no errors in the console
			var requestUri = BlazorWithPlaywrightPageAttribute.BlazorAddress;

			//load page and wait for a complete blazor render cycle to be sure that the channel is up and running
			await Page.GotoAsync(requestUri);
			await Page.WaitForBlazorAppRenderAsync();

			var consoleErrors = ConsoleMessages.FindAll(m => m.Type.Contains("error", StringComparison.OrdinalIgnoreCase));
			foreach (var error in consoleErrors)
			{
				Trace.WriteLine($"Console error: {error}");
			}

			Assert.That(consoleErrors, Has.Count.EqualTo(0));
			Assert.That(PageErrors, Has.Count.EqualTo(0));
		}
	}
}
