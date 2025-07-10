using System.Drawing;
using System.Threading.Tasks;
using Bunit;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;

using static PlaywrightTestContext;
class StatusBarPanelTest
{
	static readonly double PIXELS_PER_POINT = 1.333;

	[Test]
	public async Task MouseDownEvent()
	{
		await ControlAssert.ImplementsEventAsync<StatusBarPanel, MouseEventHandler>(nameof(GroupBox.MouseDown), a => new MouseEventHandler((o, e) => a()), ".statusbar__panel", e => e.MouseDown());
	}

	[Test]
	public async Task StatusBarPanelDefaultForeColorIsBlack()
	{
		using var ctx = new WinzorTestContext();
		var (_, panel) = await ctx.RenderControlOnFormAsync<StatusBarPanel>();
		Assert.That(panel.ForeColor, Is.EqualTo(Color.Black));
	}

	[Test]
	public async Task StatusBarPanelDefaultBackColorIsGrey()
	{
		using var ctx = new WinzorTestContext();
		var (_, panel) = await ctx.RenderControlOnFormAsync<StatusBarPanel>();

		Assert.That(panel.BackColor.ToArgb(), Is.EqualTo(Color.FromArgb(255, 240, 240, 240).ToArgb()));
	}

	[Test]
	public async Task StatusBarPanelDefaultBackColor()
	{
		using var ctx = new WinzorTestContext();
		StatusBarPanel statusBarPanel = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			statusBarPanel = new StatusBarPanel();
			return statusBarPanel;
		});

		Assert.That(statusBarPanel.BackColor, Is.EqualTo(SystemColors.Control));
		Assert.That(rendered.Find(".statusbar__panel").GetAttribute("style"), Does.Contain("background-color:var(--color-control)"));
	}

	[Test]
	public async Task StatusBarPanelCustomBackColor()
	{
		using var ctx = new WinzorTestContext();
		StatusBarPanel statusBarPanel = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			statusBarPanel = new StatusBarPanel();
			statusBarPanel.BackColor = Color.Blue;
			return statusBarPanel;
		});

		Assert.That(statusBarPanel.BackColor, Is.EqualTo(Color.Blue));
		Assert.That(rendered.Find(".statusbar__panel").GetAttribute("style"), Does.Contain($"background-color:#0000FFFF"));
	}

	[Test, WithPlaywrightPage]
	public async Task StatusBarPanelHasWhitespacePre()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var panel = new StatusBarPanel();
			form.Controls.Add(panel);
			return form;
		});

		var statusBarPanel = await page.WaitForSelectorAsync(".statusbar__panel");
		Assert.That(async () => await statusBarPanel.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('white-space')"), Is.EqualTo("pre"));
	}

	[Test, WithPlaywrightPage]
	public async Task StatusBarPanelHasLineHeightPt()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		StatusBarPanel panelObject = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			panelObject = new StatusBarPanel();
			form.Controls.Add(panelObject);
			return form;
		});

		var element = await page.WaitForSelectorAsync(".statusbar__panel");
		Assert.That(await element.GetAttributeAsync("style"), Does.Contain($"line-height:{panelObject?.FontHeight}pt !important"));

		var computedLineHeightString = await element.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('line-height')");
		var computedLineHeightNumber = Double.Parse(computedLineHeightString.Split("px")[0]);
		var expectedLineHeightPx = (panelObject?.FontHeight ?? 0) * PIXELS_PER_POINT;
		var expectedLineHeightFloor = Math.Round(expectedLineHeightPx, 2, MidpointRounding.ToNegativeInfinity);
		Assert.That(computedLineHeightNumber, Is.InRange(expectedLineHeightFloor, expectedLineHeightFloor + 0.02));
	}

	[Test]
	public async Task StatusBarPanelHasHeightInPx()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, panelObject) = await ctx.RenderControlOnFormAsync<StatusBarPanel>();
		var element = rendered.Find(".statusbar__panel");
		Assert.That(element.GetAttribute("style"), Does.Contain($"height:{panelObject.Height}px"));
	}

	[Test]
	public async Task StatusBarPanelHasWidthInPx()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, panelObject) = await ctx.RenderControlOnFormAsync<StatusBarPanel>();
		var element = rendered.Find(".statusbar__panel");
		Assert.That(element.GetAttribute("style"), Does.Contain($"width:{panelObject.Width}px"));
	}

	[TestCase(null, "StatusBarPanel: {}")]
	[TestCase("", "StatusBarPanel: {}")]
	[TestCase("Sample Text", "StatusBarPanel: {Sample Text}")]
	[TestCase("12345", "StatusBarPanel: {12345}")]
	public async Task ToString_ReturnsExpected(string inputText, string expectedOutput)
	{
		using var ctx = new WinzorTestContext();
		StatusBarPanel statusBarPanel = null;

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			statusBarPanel = new StatusBarPanel { Text = inputText };
		});

		Assert.That(statusBarPanel.ToString(), Is.EqualTo(expectedOutput));
	}
}
