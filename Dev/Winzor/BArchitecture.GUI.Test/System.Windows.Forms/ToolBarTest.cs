using System.Linq;
using System.Threading.Tasks;
using Bunit;
using NUnit.Framework;
using WinzorTestFramework;

namespace System.Windows.Forms;

class ToolBarTest
{
	[Test]
	public async Task ToolBarShouldHaveAssociatedStylingClass()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolBar = new ToolBar();
			form.Controls.Add(toolBar);
			return form;
		});

		var toolBar = rendered.Find(".toolbar");
		Assert.That(toolBar, Is.Not.Null);
	}

	[Test]
	public async Task ToolBarShouldHaveChildrenItemRendered()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolBar = new ToolBar();
			toolBar.Buttons.Add(new ToolBarButton());
			toolBar.Buttons.Add(new ToolBarButton());
			form.Controls.Add(toolBar);
			return form;
		});

		var toolBar = rendered.Find(".toolbar");
		Assert.That(toolBar.Children.Where(c => c.ClassList.Contains("toolbar__button")).ToList(), Has.Count.EqualTo(2));
	}

	[Test]
	public async Task ToolBarShouldHaveCorrectSizeAndLocation()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolBar = new ToolBar { Top = 100, Left = 200, Width = 300, Height = 440 };
			form.Controls.Add(toolBar);
			return form;
		});

		var toolBar = rendered.Find(".toolbar");
		Assert.That(toolBar.GetAttribute("style"), Contains.Substring("position:absolute;width:300px;height:440px;top:0px;left:0px;background-color:var(--color-control);"));
	}

	[Test]
	public async Task ToolBarShouldHaveCorrectDockStyle()
	{
		using var ctx = new WinzorTestContext();
		ToolBar toolBar = null;

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			toolBar = new ToolBar();
		});

		Assert.That(toolBar.Dock, Is.EqualTo(DockStyle.Top));
	}

	[Test]
	public async Task ToolBarButtonImageShouldNotBeDraggable()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var imageList = new ImageList();
			imageList.Images.Add(TestImage.GetImage());
			var toolBar = new ToolBar() { ImageList = imageList };
			var button = new ToolBarButton { ImageIndex = 0 };
			toolBar.Buttons.Add(button);
			return toolBar;
		});

		Assert.That(rendered.Find("img").GetAttribute("draggable"), Is.EqualTo("false"));
	}
}
