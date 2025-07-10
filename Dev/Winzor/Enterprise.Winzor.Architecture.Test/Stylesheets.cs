using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

using static PlaywrightTestContext;
class Stylesheets
{
	const string cssUrl = "/CargoWise.Winzor.combined.css";

	[Test, WithPlaywrightPage]
	public async Task StylesheetHasNoImports()
	{
		await using var ctx = new InMemoryAppServerTestContext(new MockCargoWiseClientSeviceProvider());
		var url = default(Uri);
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			url = new Uri(ctx.ServerBaseUrl + cssUrl);
		});

		var response = await Page.GotoAsync(url.ToString());
		Assert.That(response.Ok);
		await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
		var body = await response.TextAsync();

		Assert.That(body, Does.Not.Contain("@import \"/_content/CargoWise.GUI.TileBar/css/_gen/cargowise.gui.tilebar.css\";"));

		await Page.CloseAsync();
	}

	[Test, WithPlaywrightPage]
	public async Task StylesheetHasNoSuperfluousPrefixing()
	{
		await using var ctx = new InMemoryAppServerTestContext(new MockCargoWiseClientSeviceProvider());
		var url = default(Uri);
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			url = new Uri(ctx.ServerBaseUrl + cssUrl);
		});

		var response = await Page.GotoAsync(url.ToString());
		Assert.That(response.Ok);
		await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
		var body = await response.TextAsync();

		Assert.That(body, Does.Not.Contain("-webkit-box-sizing: border-box;"));
		Assert.That(body, Does.Not.Contain("-o-text-overflow:"));
		Assert.That(body, Does.Not.Contain("-webkit-filter:"));

		await Page.CloseAsync();
	}

	[Test, WithPlaywrightPage]
	public async Task StylesheetIncorporatesExternalClasses()
	{
		await using var ctx = new InMemoryAppServerTestContext(new MockCargoWiseClientSeviceProvider());
		var url = default(Uri);
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			url = new Uri(ctx.ServerBaseUrl + cssUrl);
		});

		var response = await Page.GotoAsync(url.ToString());
		Assert.That(response.Ok);
		await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
		var body = await response.TextAsync();

		Assert.That(body, Does.Contain(".recentitemscontrol{"));
		Assert.That(body, Does.Contain(".tilebarcontrol{"));

		await Page.CloseAsync();
	}
}
