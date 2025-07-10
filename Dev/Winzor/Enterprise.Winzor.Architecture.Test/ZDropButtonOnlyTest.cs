using System;
using System.Drawing;
using System.Threading.Tasks;
using Bunit;
using Enterprise.ZArchitecture.GUI;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

using static PlaywrightTestContext;

class ZDropButtonOnlyTest
{
	[Test]
	public async Task ZDropButtonOnlyClickEvent()
	{
		await ControlAssert.ImplementsEventAsync<ZDropButtonOnly, EventHandler>(nameof(ZDropButtonOnly.Click), a => new EventHandler((o, e) => a()), "button", e => e.Click());
	}

	[Test]
	public async Task ZDropButtonOnlyRender()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new ZDropButtonOnly());
		var button = rendered.Find("button.zdropbuttononly");

		Assert.That(button.InnerHtml, Is.EqualTo("keyboard_arrow_down"));
		Assert.That(button.Attributes["data-winzor-control-id"].Value, Is.Not.Empty);
	}

	[Test]
	public async Task ZDropButtonOnlyWidth()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => {
			var zDropButtonOnly = new ZDropButtonOnly();
			zDropButtonOnly.EndInit();
			return zDropButtonOnly;
		});
		var button = rendered.Find("button.zdropbuttononly");

		Assert.That(button.GetAttribute("style"), Does.Contain("width:17px"));
	}

	[Test, WithPlaywrightPage]
	public async Task ZDropButtonOnlyCompleteRender()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		ZDropButtonOnly zDropButtonOnly = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm();
			zDropButtonOnly = new ZDropButtonOnly();
			form.Controls.Add(zDropButtonOnly);
			return form;
		});

		var button = await page.WaitForSelectorAsync("button");
		var zIndex = await getComputedStyle(button, "z-index");
		Assert.That(zIndex, Is.EqualTo("1"));
	}

	[Test, WithPlaywrightPage]
	public async Task TestZDropButtonOnlyBackgroundColor()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		ZDropButtonOnly zDropButtonOnly = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm { Size = new Size(100, 50) };
			var autoFocusedButton = new ZButton();
			zDropButtonOnly = new ZDropButtonOnly { Size = new Size(16, 18) };
			form.Controls.Add(autoFocusedButton);
			form.Controls.Add(zDropButtonOnly);
			return form;
		});

		var button = await page.WaitForSelectorAsync(".zdropbuttononly");
		Assert.That(await getComputedStyle(button, "background-color"), Is.EqualTo("rgb(253, 253, 253)"));
		await button.FocusAsync();
		Assert.That(await getComputedStyle(button, "background-color"), Is.EqualTo("rgb(229, 241, 251)"));
		await page.Mouse.MoveAsync(0, 9);
		await page.Mouse.DownAsync();
		Assert.That(await getComputedStyle(button, "background-color"), Is.EqualTo("rgb(204, 228, 247)"));
	}

	async Task<string> getComputedStyle(IElementHandle e, string property)
	{
		return (await e.EvaluateAsync($"e => window.getComputedStyle(e).getPropertyValue('{property}')")).Value.ToString();
	}
}
