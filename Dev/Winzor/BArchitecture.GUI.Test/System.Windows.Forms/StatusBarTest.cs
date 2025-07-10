using System.Drawing;
using System.Threading.Tasks;
using Bunit;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;

using static PlaywrightTestContext;
class StatusBarTest
{
	[Test]
	public async Task MouseDownEvent()
	{
		await ControlAssert.ImplementsEventAsync<StatusBar, MouseEventHandler>(nameof(GroupBox.MouseDown), a => new MouseEventHandler((o, e) => a()), ".statusbar", e => e.MouseDown());
	}

	[Test]
	public async Task StatusBarDefaultValues()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() => {
			var control = new StatusBar();
			Assert.That(control.CanSelect, Is.False);
		});
	}

	[Test]
	public async Task StatusBarDefaultBackColor()
	{
		using var ctx = new WinzorTestContext();
		StatusBar statusBar = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			statusBar = new StatusBar();
			return statusBar;
		});

		Assert.That(statusBar.BackColor, Is.EqualTo(SystemColors.Control));
		Assert.That(rendered.Find(".statusbar").GetAttribute("style"), Does.Contain("background-color:var(--color-control)"));
	}

	[Test]
	public async Task StatusBarCustomBackColor()
	{
		using var ctx = new WinzorTestContext();
		StatusBar statusBar = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			statusBar = new StatusBar();
			statusBar.BackColor = Color.Blue;
			return statusBar;
		});

		Assert.That(statusBar.BackColor, Is.EqualTo(Color.Blue));
		Assert.That(rendered.Find(".statusbar").GetAttribute("style"), Does.Contain($"background-color:#0000FFFF"));
	}

	[Test]
	public async Task StatusBarShouldHaveMarginOnSizingGrip()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			return new StatusBar()
			{
				SizingGrip = true,
			};
		});

		var statusbar = rendered.Find(".statusbar__border");

		Assert.That(statusbar.GetAttribute("style"), Does.Contain("margin-right: 15px"));
	}

	[Test, WithPlaywrightPage]
	public async Task StatusStripSizingGripShouldHaveCorrectImageSizeAndPosition()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var statusBar = new StatusBar()
			{
				SizingGrip = true,
			};

			return statusBar;
		});

		var statusBar = page.Locator(".statusbar");
		var statusBarSizingGrip = page.Locator($".sizing-grip");
		Assert.That(await statusBar.GetComputedStyleAsync("overflow"), Is.EqualTo("hidden"));
		Assert.That(await statusBarSizingGrip.GetComputedStyleAsync("position"), Is.EqualTo("absolute"));
		Assert.That(await statusBarSizingGrip.GetComputedStyleAsync("bottom"), Is.EqualTo("1px"));
		Assert.That(await statusBarSizingGrip.GetComputedStyleAsync("right"), Is.EqualTo("1px"));
		Assert.That(await statusBarSizingGrip.GetComputedStyleAsync("width"), Is.EqualTo("10px"));
		Assert.That(await statusBarSizingGrip.GetComputedStyleAsync("height"), Is.EqualTo("10px"));
		Assert.That(await statusBarSizingGrip.GetComputedStyleAsync("background-image"), Is.EqualTo("url(\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAgAAAAICAYAAADED76LAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsEAAA7BAbiRa+0AAAAoSURBVChTY2TAAfbv3/8fRDOBeZQAFCtgxjo6OjJS0QpsxpJgBQMDAFj4Fb5NXhpbAAAAAElFTkSuQmCC\")"));
		Assert.That(await statusBarSizingGrip.GetComputedStyleAsync("background-repeat"), Is.EqualTo("no-repeat"));
		Assert.That(await statusBarSizingGrip.GetComputedStyleAsync("image-rendering"), Is.EqualTo("pixelated"));
	}

	[Test, WithPlaywrightPage]
	public async Task StatusBarBorderShouldHaveRelativePositioning()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var statusBar = new StatusBar();
			form.Controls.Add(statusBar);
			return form;
		});

		var statusBarBorder = await page.WaitForSelectorAsync($".statusbar__border");
		Assert.That(async () => await statusBarBorder.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('position')"), Is.EqualTo("relative"));
	}

	[Test, WithPlaywrightPage]
	public async Task StatusBarImageShouldHaveNoPointerEvents()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var statusBar = new StatusBar();
			statusBar.SizingGrip = true;
			form.Controls.Add(statusBar);
			return form;
		});

		var statusBarImage = await page.WaitForSelectorAsync($".sizing-grip");
		Assert.That(async () => await statusBarImage.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('pointer-events')"), Is.EqualTo("none"));
	}

	[TestCase(null, 0, null, "System.Windows.Forms.StatusBar, Panels.Count: 0")]
	[TestCase("Sample Panel Text", 1, "Sample Panel Text", "System.Windows.Forms.StatusBar, Panels.Count: 1, Panels[0]: StatusBarPanel: {Sample Panel Text}")]
	[TestCase("12345", 2, "12345", "System.Windows.Forms.StatusBar, Panels.Count: 2, Panels[0]: StatusBarPanel: {12345}")]
	public async Task ToString_ReturnsExpectedForStatusBar(string panelText, int panelCount, string firstPanelText, string expectedOutput)
	{
		using var ctx = new WinzorTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var statusBar = new StatusBar();
			if (panelCount > 0)
			{
				for (int i = 0; i < panelCount; i++)
				{
					statusBar.Panels.Add(new StatusBarPanel
					{
						Text = i == 0 ? firstPanelText : $"Panel {i + 1}"
					});
				}
			}
			return statusBar;
		});

		var statusBar = rendered.GetControl<StatusBar>();
		Assert.That(statusBar.ToString(), Is.EqualTo(expectedOutput));
	}
}
