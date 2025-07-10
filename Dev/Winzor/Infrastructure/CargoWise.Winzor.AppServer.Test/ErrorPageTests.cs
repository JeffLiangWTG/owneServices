using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Playwright;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace CargoWise.Blazor.AppServer.Test
{
	using static PlaywrightTestContext;

	[TestFixture, BlazorWithPlaywrightPage]
	public class ErrorPageTests
	{
		[Test]
		public async Task ErrorMessageHiddenByDefault()
		{
			var requestUri = BlazorWithPlaywrightPageAttribute.BlazorAddress;

			await Page.GotoAsync(requestUri);
			await Page.WaitForBlazorAppRenderAsync();

			var errorUi = await Page.QuerySelectorAsync("div#blazor-error-ui");

			errorUi.Should().NotBeNull();

			await errorUi.WaitForElementStateAsync(ElementState.Hidden);
		}

		[Test]
		public async Task ErrorMessageShownWhenExceptionThrown()
		{
			var requestUri = BlazorWithPlaywrightPageAttribute.BlazorAddress + @"/error";

			await Page.GotoAsync(requestUri);
			await Page.WaitForBlazorAppRenderAsync();

			IConsoleMessage circuitShutdownMessage = null;
			// force a disconnection and wait for the expected error message (as close to causal as we can reasonably get)
			// we do this to minimise the chance we'll unintentionally eat a circuit shutdown message due to a real problem (eg, the test should fail because something broke)
			await Page.RunAndWaitForConsoleMessageAsync(
				action: () => Page.EvaluateAsync("Blazor.disconnect()"),
				options: new PageRunAndWaitForConsoleMessageOptions
				{
					Predicate = msg =>
					{
						if (msg.Text.Contains("Error: Circuit has been shut down due to error.", StringComparison.Ordinal))
						{
							circuitShutdownMessage = msg;
							return true;
						}
						return false;
					},
				});

			// hacky workaround: mutate the 'console messages log' until we've got a cleaner method of indicating that a specific message was 'expected'
			// this is to prevent the test failing on an 'expected' error
			circuitShutdownMessage.Should().NotBeNull();
			ConsoleMessages.Remove(circuitShutdownMessage).Should().BeTrue("Expecting to remove exactly one instance of the 'expected' error from the log");
			await TestContext.Out.WriteLineAsync($"Removed from {nameof(ConsoleMessages)}: [Console] {circuitShutdownMessage.Type} {circuitShutdownMessage.Text} {circuitShutdownMessage.Location}");

			// fail-fast check that the element exists so we don't wait for visibility if it's not even present in the DOM
			var errorUi = await Page.QuerySelectorAsync("div#blazor-error-ui");
			errorUi.Should().NotBeNull(because: "the blazor error ui element should already be attached to the DOM, waiting to be set visible when an error occurs");

			// wait for error ui to become visible
			await errorUi.WaitForElementStateAsync(ElementState.Visible);
		}
	}
}
