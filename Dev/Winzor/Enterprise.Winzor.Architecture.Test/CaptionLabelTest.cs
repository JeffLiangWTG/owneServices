using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using NUnit.Framework;
using WTG.PlaywrightTesting;
using static CargoWise.Windows.UI.LabelCaptionRenderer;

namespace Enterprise.Winzor.Architecture.Test;

using static PlaywrightTestContext;

class CaptionLabelTest
{
	[Test, WithPlaywrightPage]
	public async Task ForeColorControlStyleStringTest()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		CaptionLabel captionLabel = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			captionLabel = new CaptionLabel();
			form.Controls.Add(captionLabel);

			return form;
		});

		await page.WaitForSelectorAsync(".form");
		var captionLabelElement = await page.QuerySelectorAsync(".label");
		Assert.That(await captionLabelElement.GetAttributeAsync("style"), Does.EndWith("color:#000000FF;"));

		await captionLabel.InvokeWinzorDispatcherAsync(() => { captionLabel.ForeColor = Color.Red; });
		Assert.That(async () => await captionLabelElement.GetAttributeAsync("style"), Does.EndWith("color:#FF0000FF;").After(3000,100));
	}

	[Test, WithPlaywrightPage]
	public async Task DoubleClickCaptionLabelShouldNotCopyTextToClipboard()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => new CaptionLabel { Text = "CopyToClipboard" });
		await BrowserContext.GrantPermissionsAsync(new[] { "clipboard-write", "clipboard-read" });

		await page.EvaluateAsync<string>("() => navigator.clipboard.writeText('')");
		await Task.Delay(500);
		var clipboardTextBeforeDblClick = await page.EvaluateAsync<string>("() => navigator.clipboard.readText()");
		Assert.That(clipboardTextBeforeDblClick, Is.Empty);

		var labelElement = page.Locator(".label");
		await labelElement.DblClickAsync();
		await Task.Delay(500);
		var clipboardTextAfterDblClick = await page.EvaluateAsync<string>("() => navigator.clipboard.readText()");
		Assert.That(clipboardTextAfterDblClick, Is.Empty);
	}
}
