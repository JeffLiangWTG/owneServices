using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

using static PlaywrightTestContext;

class ZSearchButtonTest
{
	[Test, WithPlaywrightPage]
	public async Task ZSearchButtonShouldRenderCorrectIcon()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new ZForm();
			var zGroupBox = new ZGroupBox();
			var zGuidSearchEdit = new ZGuidSearchEdit();
			var searchButton = new ZSearchButton();
			zGuidSearchEdit.Controls.Add(searchButton);
			zGroupBox.Controls.Add(zGuidSearchEdit);
			form.Controls.Add(zGroupBox);
			return form;
		});

		var searchImg = await page.WaitForSelectorAsync(".zdropbutton__image");
		var altAttributeValue = await searchImg.GetAttributeAsync("alt");

		Assert.That(altAttributeValue, Is.EqualTo("search icon"));
	}

	[Test, WithPlaywrightPage]
	public async Task ZSearchButtonShouldToggleIconOnClick()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var zGroupBox = new ZGroupBox();
			var zGuidSearchEdit = new ZGuidSearchEdit();
			var searchButton = new ZSearchButton();
			zGuidSearchEdit.Controls.Add(searchButton);
			zGroupBox.Controls.Add(zGuidSearchEdit);
			return zGroupBox;
		});

		var button = page.GetByRole(AriaRole.Button).First;
		await button.ClickAsync();

		var searchImg = page.Locator(".zdropbutton__image").First;
		await Assertions.Expect(searchImg).ToHaveAttributeAsync("alt", "close icon");
	}

	[Test, WithPlaywrightPage]
	public async Task ZSearchButtonShouldRenderCorrectBackgroundColor()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new ZForm();
			var zGroupBox = new ZGroupBox();
			var zGuidSearchEdit = new ZGuidSearchEdit();
			var searchButton = new ZSearchButton();
			zGuidSearchEdit.Controls.Add(searchButton);
			zGroupBox.Controls.Add(zGuidSearchEdit);
			form.Controls.Add(zGroupBox);
			return form;
		});

		var searchButton = await page.WaitForSelectorAsync(".zdropbutton > button");

		Assert.That(await searchButton.GetComputedStyleAsync("background-color"), Is.EqualTo("rgb(253, 253, 253)"));
		await (await page.WaitForSelectorAsync(".zdropbutton > button")).ClickAsync();
		Thread.Sleep(1000);
		Assert.That(await searchButton.GetComputedStyleAsync("background-color"), Is.EqualTo("rgb(253, 253, 253)"));
	}

	[Test]
	public async Task TestDisabledButtonShouldNotFireEvent()
	{
		var eventFired = false;
		ZSearchButton button = null;
		await using var ctx = new InMemoryAppServerTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			button = new ZSearchButton() { Enabled = false };
			button.MouseDown += (_, _) => eventFired = true;
		});

		await button.OnDropButtonClickAsync(new WebMouseEventArgs());
		Assert.That(eventFired, Is.False);
	}
}
