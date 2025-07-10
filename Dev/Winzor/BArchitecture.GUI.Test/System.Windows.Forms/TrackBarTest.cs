using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WinzorFramework.JSInterop;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using static WTG.PlaywrightTesting.PlaywrightTestContext;

namespace System.Windows.Forms;

public class TrackBarTest
{
	[Test]
	public async Task TrackBarHasImplementedInterface()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var trackBar = new TrackBar();

			Assert.That(trackBar is ISupportInitialize);
			Assert.DoesNotThrow(() => trackBar.BeginInit());
			Assert.DoesNotThrow(() => trackBar.EndInit());
		});
	}

	[Test]
	public async Task PreloadTrackBarJSInterop()
	{
		using var ctx = new WinzorTestContext();
		var services = ctx.MockCargoWiseClientServices;
		var interop = new Mock<ITrackBarJSInterop>();
		interop.Setup(i => i.PreloadInterop());
		ctx.Services.AddScoped(_ => interop.Object);

		var rendered = await ctx.RenderControlOnFormAsync(() => new TrackBar() { Size = new Size(50, 10) });

		interop.Verify(e => e.PreloadInterop(), Times.Once());
	}

	[Test, WithPlaywrightPage]
	public async Task TrackBarShouldIncrementCorrectValueOnKeyPress()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var trackBar = new TrackBar();
			trackBar.LargeChange = 2;
			trackBar.Value = 4;
			trackBar.Location = new Point(10, 10);
			trackBar.Name = "CloseCertaintyTrackBar";
			trackBar.Size = new Size(50, 10);
			form.Controls.Add(trackBar);
			return form;
		});

		var trackbar = page.Locator(".trackbar__input");
		await trackbar.FocusAsync();

		await page.Keyboard.PressAsync("ArrowRight");
		await page.WaitForTimeoutAsync(1000);
		Assert.That(await trackbar.EvaluateAsync<string>(@"e => e.value"), Is.EqualTo("5"));

		await page.Keyboard.PressAsync("ArrowLeft");
		await page.WaitForTimeoutAsync(1000);
		Assert.That(await trackbar.EvaluateAsync<string>(@"e => e.value"), Is.EqualTo("4"));

		await page.Keyboard.PressAsync("PageUp");
		await page.WaitForTimeoutAsync(1000);
		Assert.That(await trackbar.EvaluateAsync<string>(@"e => e.value"), Is.EqualTo("6"));

		await page.Keyboard.PressAsync("PageDown");
		await page.WaitForTimeoutAsync(1000);
		Assert.That(await trackbar.EvaluateAsync<string>(@"e =>e.value"), Is.EqualTo("4"));
	}

	[Test, WithPlaywrightPage]
	public async Task TrackBarShouldGetFocusOnClick()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var trackBar = new TrackBar();
			trackBar.Location = new Point(10, 10);
			trackBar.Name = "CloseCertaintyTrackBar";
			trackBar.Size = new Size(50, 10);
			form.Controls.Add(trackBar);
			return form;
		});
		var trackbar = page.Locator(".trackbar__input");
		await trackbar.EvaluateAsync("e => e.blur()");
		await page.WaitForTimeoutAsync(1000);
		await trackbar.ClickAsync();
		await page.WaitForTimeoutAsync(1000);

		Assert.That(await trackbar.EvaluateAsync<bool>(@"e => {
					var focusedElement = document.activeElement;
					return focusedElement === e;
				}
			"), Is.EqualTo(true));

		var trackbarDiv = page.Locator(".trackbar");
		Assert.That(await trackbarDiv.EvaluateAsync<string>(@"e => window.getComputedStyle(e).getPropertyValue('outline')"), Is.EqualTo("rgba(0, 0, 0, 0.4) dotted 1px"));
	}

	[Test, WithPlaywrightPage]
	public async Task TrackBarShouldChangeValueOnDrag()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var trackBar = new TrackBar();
			trackBar.Location = new Point(10, 10);
			trackBar.Name = "CloseCertaintyTrackBar";
			trackBar.Size = new Size(50, 10);
			form.Controls.Add(trackBar);
			return form;
		});

		var trackbar = page.Locator(".trackbar__input");

		await trackbar.DragToAsync(trackbar, new ()
		{
			SourcePosition = new () { X = 10, Y = 5 },
			TargetPosition = new () { X = 31, Y = 5 },
		});
		await page.WaitForTimeoutAsync(1000);

		Assert.That(await trackbar.EvaluateAsync<string>(@"e => e.value"), Is.EqualTo("6"));
	}

	[Test, WithPlaywrightPage]
	public async Task TrackBarShouldChangeValueOnClick()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var trackBar = new TrackBar();
			trackBar.Location = new Point(10, 10);
			trackBar.Name = "CloseCertaintyTrackBar";
			trackBar.Size = new Size(50, 10);
			form.Controls.Add(trackBar);
			return form;
		});

		var trackbar = page.Locator(".trackbar__input");
		await trackbar.ClickAsync(new ()
		{
			Button = MouseButton.Left,
			Modifiers = new[] { KeyboardModifier.Shift },
			Position = new Position { X = 31, Y = 5 }
		});
		await page.WaitForTimeoutAsync(1000);

		Assert.That(await trackbar.EvaluateAsync<string>(@"e => e.value"), Is.EqualTo("6"));
	}

	[Test, WithPlaywrightPage]
	public async Task TrackBarShouldHaveCorrectBackgroundColor()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var trackBar = new TrackBar();
			trackBar.Location = new Point(10, 10);
			trackBar.Name = "CloseCertaintyTrackBar";
			trackBar.Size = new Size(50, 10);
			form.Controls.Add(trackBar);
			return form;
		});
		var trackbar = page.Locator(".trackbar__input");
		Assert.That(await trackbar.EvaluateAsync<string>(@"e => JSON.stringify(window.getComputedStyle(e))"), Does.Contain("rgb(240, 240, 240)"));
	}
}
