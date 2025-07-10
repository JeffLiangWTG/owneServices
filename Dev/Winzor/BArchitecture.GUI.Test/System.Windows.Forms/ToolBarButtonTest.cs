using System.Drawing;
using System.Threading.Tasks;
using Bunit;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;

using static PlaywrightTestContext;
class ToolBarButtonTest
{
	[Test]
	public async Task ToolBarButtonText()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var toolBar = new ToolBar();
			toolBar.Buttons.Add(new ToolBarButton { Text = "buttontext" });
			return toolBar;
		});
		Assert.That(rendered.Find(".toolbar__button div").InnerHtml, Is.EqualTo("buttontext"));
	}

	[Test]
	public async Task ToolBarButtonClickEvent()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var toolBar = new ToolBar();
			toolBar.Buttons.Add(new ToolBarButton { Text = "buttontext" });
			return toolBar;
		});
		var form = (Form)rendered.Instance.Control;
		var toolBar = (ToolBar)form.Controls[0];

		var clickEventCount = 0;

		await toolBar.InvokeWinzorDispatcherAsync(() => toolBar.ButtonClick += (e, args) => clickEventCount++);
		await rendered.Find(".toolbar__button div").ClickAsync(new WebMouseEventArgs());
		Assert.That(clickEventCount, Is.EqualTo(1));
	}

	[Test]
	public async Task ToolBarButtonClickDoesNotFireActionWhenDisabledAsync()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var toolBar = new ToolBar();
			toolBar.Buttons.Add(new ToolBarButton() { Text = "buttontext" });
			toolBar.Enabled = false;
			return toolBar;
		});

		var form = (Form)rendered.Instance.Control;
		var toolBarControl = (ToolBar)form.Controls[0];
		var isRemoved = rendered.Find(".toolbar__button").RemoveAttribute("disabled");
		Assert.That(isRemoved, Is.True);

		var eventFired = false;
		await toolBarControl.InvokeWinzorDispatcherAsync(() => toolBarControl.ButtonClick += (e, args) => eventFired = true);

		await rendered.Find(".toolbar__button div").ClickAsync(new WebMouseEventArgs());
		Assert.That(eventFired, Is.False);
	}

	[Test]
	public async Task ToolBarButtonBoxHasClass()
	{
		using var ctx = new WinzorTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var toolBar = new ToolBar();
			toolBar.Buttons.Add(new ToolBarButton());
			return toolBar;
		});

		var toolBarButton = rendered.Find(".toolbar__button");

		Assert.That(toolBarButton, Is.Not.Null);
	}

	[Test]
	public async Task ToolBarButtonTextChangedFromServer()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var toolBar = new ToolBar();
			toolBar.Buttons.Add(new ToolBarButton { Text = "buttontext" });
			return toolBar;
		});
		var form = (Form)rendered.Instance.Control;
		var toolBar = (ToolBar)form.Controls[0];

		var toolBarButton = rendered.Find(".toolbar__button");
		Assert.That(toolBarButton.TextContent, Is.EqualTo("buttontext"));

		await toolBar.InvokeWinzorDispatcherAsync(() => toolBar.Buttons[0].Text = "buttonext updated");

		toolBarButton = rendered.Find(".toolbar__button");
		Assert.That(toolBarButton.TextContent, Is.EqualTo("buttonext updated"));
	}

	[Test]
	public async Task ToolBarButtonPushedChangedFromServer()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var toolBar = new ToolBar();
			toolBar.Buttons.Add(new ToolBarButton { Text = "buttontext", Style = ToolBarButtonStyle.ToggleButton });
			return toolBar;
		});
		var form = (Form)rendered.Instance.Control;
		var toolBar = (ToolBar)form.Controls[0];

		var toolBarButton = rendered.Find(".toolbar__button");
		Assert.That(toolBarButton.ClassList.Contains("toolbar__button-pushed"), Is.False);

		await toolBar.InvokeWinzorDispatcherAsync(() => toolBar.Buttons[0].Pushed = true);

		toolBarButton = rendered.Find(".toolbar__button");
		Assert.That(toolBarButton.ClassList.Contains("toolbar__button-pushed"), Is.True);
	}

	[Test]
	public async Task ToolBarButtonImageIndexChangedFromServer()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var imageList = new ImageList();
			imageList.Images.Add(new Bitmap(16, 16));
			var toolBar = new ToolBar() { ImageList = imageList };
			toolBar.Buttons.Add(new ToolBarButton { Text = "buttontext", Style = ToolBarButtonStyle.ToggleButton, ImageIndex = 0 });
			return toolBar;
		});
		var form = (Form)rendered.Instance.Control;
		var toolBar = (ToolBar)form.Controls[0];

		var toolBarButtonImage = rendered.FindAll(".toolbar__button img");
		Assert.That(toolBarButtonImage.Count, Is.EqualTo(1));

		await toolBar.InvokeWinzorDispatcherAsync(() => toolBar.Buttons[0].ImageIndex = -1);

		toolBarButtonImage = rendered.FindAll(".toolbar__button img");
		Assert.That(toolBarButtonImage.Count, Is.EqualTo(0));
	}

	[Test]
	public async Task ToolBarButtonDisabled()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var toolBar = new ToolBar();
			toolBar.Buttons.Add(new ToolBarButton());
			toolBar.Enabled = false;
			return toolBar;
		});

		var form = (Form)rendered.Instance.Control;
		var toolBarControl = (ToolBar)form.Controls[0];
		var toolBarButton = rendered.Find(".toolbar__button");
		Assert.That(toolBarButton.GetAttribute("disabled"), Is.Not.Null);

		await toolBarControl.InvokeWinzorDispatcherAsync(() => toolBarControl.Enabled = true);
		Assert.That(toolBarButton.GetAttribute("disabled"), Is.Null);
	}

	[Test,WithPlaywrightPage]
	[TestCase(false, "drop-shadow(rgb(0, 0, 0) 0px 0px 0.3px) grayscale(1)", "0.4", "none")]
	[TestCase(true, "none","1","auto")]
	public async Task ToolBarButtonDisabledStyle(bool toolbarEnabled, string expectedFilter,string expectedOpacity,string expectedPointerEvents)
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var toolBar = new ToolBar();
			toolBar.Buttons.Add(new ToolBarButton());
			toolBar.Enabled = toolbarEnabled;
			form.Controls.Add(toolBar);
			return form;
		});

		await page.WaitForSelectorAsync(".toolbar");
		var toolBarButton = await page.WaitForSelectorAsync(".toolbar__button");
		Assert.That(async () => await toolBarButton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('filter')"), Is.EqualTo(expectedFilter));
		Assert.That(async () => await toolBarButton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('opacity')"), Is.EqualTo(expectedOpacity));
		Assert.That(async () => await toolBarButton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('pointer-events')"), Is.EqualTo(expectedPointerEvents));
	}

	[Test, WithPlaywrightPage]
	[TestCase(false, "1px solid rgb(178, 179, 180)")]
	[TestCase(true, "1px solid rgb(134, 135, 136)")]
	public async Task TestToolBarSeparatorStyle(bool toolbarEnabled, string expectedBorderLeft)
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();

			var toolBarButton = new ToolBarButton();
			toolBarButton.Style = ToolBarButtonStyle.Separator;

			var toolBar = new ToolBar();
			toolBar.Buttons.Add(toolBarButton);
			toolBar.Enabled = toolbarEnabled;

			form.Controls.Add(toolBar);
			return form;
		});

		var toolBarSeparator = await page.WaitForSelectorAsync(".toolbar__separator > div");
		Assert.That(async () => await toolBarSeparator.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('border-left')"), Is.EqualTo(expectedBorderLeft));
	}
}
