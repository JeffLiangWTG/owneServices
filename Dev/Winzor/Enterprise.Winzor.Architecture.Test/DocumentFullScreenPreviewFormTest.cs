using System;
using System.Drawing;
using System.Threading.Tasks;
using Enterprise.DocumentScanning.GUI;
using NUnit.Framework;
using WTG.PlaywrightTesting;
using static WTG.PlaywrightTesting.PlaywrightTestContext;

namespace Enterprise.Winzor.Architecture.Test;

class DocumentFullScreenPreviewFormTest
{
	[Test, WithPlaywrightPage]
	[TestCase(200, 100, "Width")]
	[TestCase(100, 200, "Height")] 
	public async Task ImagePanelMustFitToLargerDimension_OnInitialRender(int bitmapWidth, int bitmapHeight, string dimensionToMatch)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new DocumentFullScreenPreviewForm() { Width = 1000, Height = 800 };
			form.DocumentImage = new Bitmap(bitmapWidth, bitmapHeight);
			return form;
		});

		var picturebox = await page.WaitForSelectorAsync(".picturebox");
		await Task.Delay(100);

		// This SetViewportSize is called to mimic the initial rendering of the DocumentFullScreenPreviewForm in Allocate Docs windows
		// The form is first initialised with a small window, then somewhere in the RunMessageLoop, the viewport is maximised.
		await page.SetViewportSizeAsync(2000, 1000);
		await Task.Delay(1000);

		var imageLargerDimension = await page.EvaluateAsync<double>($"() => document.querySelector('.picturebox').client{dimensionToMatch}");
		await Task.Delay(100);
		var browserLargerDimension = await page.EvaluateAsync<double>($"() => window.inner{dimensionToMatch}");
		await Task.Delay(100);

		Assert.That(imageLargerDimension, Is.EqualTo(browserLargerDimension));
	}
}
