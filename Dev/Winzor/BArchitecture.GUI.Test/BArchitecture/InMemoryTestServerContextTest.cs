using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace WinzorFramework;

using static PlaywrightTestContext;

internal sealed class InMemoryTestServerContextTest
{
	[Test]
	public async Task ShutdownCircuitServiceGetsRemoved()
	{
		await using var ctx = new InMemoryTestServerContext();
		Assert.That(ctx.HostServices.GetService<CircuitHandler>(), Is.EqualTo(null));
	}

	// THIS TEST MUST BE HEADLESS = FALSE OR IT WILL NOT WORK
	[Test, WithPlaywrightPage(Headless = false)]
	public async Task CircuitHandlerDoesNotStopHost()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			return form;
		});
		await page.WaitForSelectorAsync(".form");

		// 7 seconds = 5 seconds for default host shutdown timeout + 2 extra 
		Assert.That(await ctx.WaitForHostShutdownAsync().WithTimeout(TimeSpan.FromSeconds(7)), Is.False);
	}

	[Test, WithPlaywrightPage]
	public async Task BlazorRenderExceptionShowsDetailedExceptionMessage()
	{
		await using var ctx = new InMemoryTestServerContext();
		var url = default(Uri);
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var form = new FormWithRenderingException();
			url = ctx.WinzorDispatcher.FormInstanceRegister.Add(new Uri("http://localhost:5000"), form);
		});
		var response = await Page.GotoAsync(url.ToString());
		Assert.That(response.Ok);

		var consoleMessage = await Page.WaitForConsoleMessageAsync(new PageWaitForConsoleMessageOptions
		{
			Predicate = (message) => message.Type == "error"
		});

		Assert.That(consoleMessage.Text, Does.Contain("InvalidOperationException"));
		Assert.That(consoleMessage.Text, Does.Contain("Test exception message"));

		await Page.CloseAsync();

		ConsoleMessages.Clear();
		PageErrors.Clear();
	}

	class FormWithRenderingException : Form
	{
		protected override void BuildRenderTree(RenderTreeBuilder builder)
		{
			base.BuildRenderTree(builder);
			throw new InvalidOperationException("Test exception message");
		}
	}
}
