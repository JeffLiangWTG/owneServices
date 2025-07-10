using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace WinzorFramework.JSInterop;
class FormInteropTest
{
	[Test, WithPlaywrightPage]
	public async Task ResizeBrowserDimensionsShouldInvokeSizeChanged()
	{
		await using var ctx = new InMemoryTestServerContext();

		var sizeChangedInvokedCount = new TaskCompletionSource<bool>();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 1000, Height = 600 };
			form.SizeChanged += (sender, args) =>
			{
				sizeChangedInvokedCount.SetResult(true);
			};
			form.Controls.Add(new Label { Text = "Resize Test" });
			return form;
		});

		await page.SetViewportSizeAsync(1000, 600);
		await page.WaitForFunctionAsync("window.innerWidth === 1000 && window.innerHeight === 600");
		await page.SetViewportSizeAsync(500, 720);
		await page.WaitForFunctionAsync("window.innerWidth === 500 && window.innerHeight === 720");

		Assert.That(await sizeChangedInvokedCount.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task OnRaisingEventSizeShouldBeCaptured()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 1000, Height = 600 };
			form.MainMenuStrip = new MenuStrip();
			form.Controls.Add(new Label { Text = "Resize Test" });
			return form;
		});
		var form = page.GetForm();

		await page.SetViewportSizeAsync(1000, 600);
		await page.WaitForFunctionAsync("window.innerWidth === 1000 && window.innerHeight === 600");
		await page.SetViewportSizeAsync(500, 720);
		await page.WaitForFunctionAsync("window.innerWidth === 500 && window.innerHeight === 720");

		Assert.That(() => form.Size.Width, Is.EqualTo(500).After(3000, 100));
		Assert.That(() => form.Size.Height, Is.EqualTo(720).After(3000, 100));
	}

	// This unit test previously had the Assert statements run inside an event delegate
	// As a result, the functionality was broken without the test failing as the event
	// was never raised. The test has been rewritten but is now failing. The functionality
	// will need to be fixed so the Explicit attribute can be removed.
	[Test, WithPlaywrightPage, Explicit]
	public async Task MoveBrowserPositionIsSetToFormLocation()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Left = 110, Top = 60, Width = 1000, Height = 600 };
			form.Controls.Add(new Label { Text = "Move Test" });
			return form;
		});
		var form = page.GetForm();

		// As in June 2022, there are no features in playwright to move the browser window
		// hence this test uses dispatchEvent instead to mimic the browser move
		var windowHandle = await page.EvaluateHandleAsync("window");
		await windowHandle.EvaluateAsync("node => node.screenX = 300");
		await windowHandle.EvaluateAsync("node => node.screenY = 200");
		await windowHandle.EvaluateAsync("node => node.dispatchEvent(new Event('browsermove'))");

		Assert.That(form.Location.X, Is.EqualTo(300).After(3000, 100));
		Assert.That(form.Location.Y, Is.EqualTo(200).After(3000, 100));
	}
}
