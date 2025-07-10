using System.Drawing;
using System.Threading.Tasks;
using Bunit;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;

using static PlaywrightTestContext;

class SplitContainerTest
{
	[Test]
	public async Task SplitContainerShouldHave2PanelsAndSplitter()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var splitContainer = new SplitContainer();
			splitContainer.Orientation = Orientation.Vertical;
			splitContainer.Width = 100;
			splitContainer.Height = 100;
			Assert.That(splitContainer, Is.Not.Null);
			Assert.That(splitContainer.Panel1, Is.Not.Null);
			Assert.That(splitContainer.Panel2, Is.Not.Null);
			return splitContainer;
		});

		var splitContainer = rendered.Find(".splitcontainer");
		Assert.That(splitContainer.Children, Has.Length.EqualTo(3));
		var panel1 = splitContainer.Children[0];
		Assert.That(panel1.ClassList.Contains("splitterpanel"), Is.True);
		var splitter = splitContainer.Children[1];
		Assert.That(splitter.ClassList.Contains("splitter"), Is.True);
		var panel2 = splitContainer.Children[2];
		Assert.That(panel2.ClassList.Contains("splitterpanel"), Is.True);
	}

	[Test]
	public async Task SplitContainerVerticalOrientationShouldRenderVerticalSplitter()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var splitContainer = new SplitContainer();
			splitContainer.Orientation = Orientation.Vertical;
			splitContainer.Width = 1000;
			splitContainer.Height = 600;
			splitContainer.SplitterDistance = 250;
			return splitContainer;
		});

		var splitContainer = rendered.Find(".splitcontainer");
		Assert.That(splitContainer.GetAttribute("style"), Does.Contain("width:1000px;"));
		Assert.That(splitContainer.GetAttribute("style"), Does.Contain("height:600px;"));
		Assert.That(splitContainer.GetAttribute("style"), Does.Contain("top:0px;"));
		Assert.That(splitContainer.GetAttribute("style"), Does.Contain("left:0px;"));
		var panel1 = splitContainer.Children[0];
		Assert.That(panel1.GetAttribute("style"), Does.Contain("width:250px;"));
		Assert.That(panel1.GetAttribute("style"), Does.Contain("height:600px;"));
		Assert.That(panel1.GetAttribute("style"), Does.Contain("top:0px;"));
		Assert.That(panel1.GetAttribute("style"), Does.Contain("left:0px;"));
		var splitter = splitContainer.Children[1];
		// Vertical orientation has a horizonal splitter
		Assert.That(splitter.GetAttribute("style"), Does.Contain("width:4px;"));
		Assert.That(splitter.GetAttribute("style"), Does.Contain("height:600px;"));
		Assert.That(splitter.GetAttribute("style"), Does.Contain("top:0px;"));
		Assert.That(splitter.GetAttribute("style"), Does.Contain("left:250px;"));
		Assert.That(splitter.GetAttribute("style"), Does.Contain("cursor:ew-resize;"));
		var panel2 = splitContainer.Children[2];
		Assert.That(panel2.GetAttribute("style"), Does.Contain("width:746px;"));
		Assert.That(panel2.GetAttribute("style"), Does.Contain("height:600px;"));
		Assert.That(panel2.GetAttribute("style"), Does.Contain("top:0px;"));
		Assert.That(panel2.GetAttribute("style"), Does.Contain("left:254px;"));
	}

	[Test]
	public async Task SplitContainerHorizontalOrientationShouldRenderHorizontalSplitter()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var splitContainer = new SplitContainer();
			splitContainer.Orientation = Orientation.Horizontal;
			splitContainer.Width = 1000;
			splitContainer.Height = 600;
			splitContainer.SplitterDistance = 250;
			return splitContainer;
		});

		var splitContainer = rendered.Find(".splitcontainer");
		Assert.That(splitContainer.GetAttribute("style"), Does.Contain("width:1000px;"));
		Assert.That(splitContainer.GetAttribute("style"), Does.Contain("height:600px;"));
		Assert.That(splitContainer.GetAttribute("style"), Does.Contain("top:0px;"));
		Assert.That(splitContainer.GetAttribute("style"), Does.Contain("left:0px;"));

		var panel1 = splitContainer.Children[0];
		Assert.That(panel1.GetAttribute("style"), Does.Contain("width:1000px;"));
		Assert.That(panel1.GetAttribute("style"), Does.Contain("height:250px;"));
		Assert.That(panel1.GetAttribute("style"), Does.Contain("top:0px;"));
		Assert.That(panel1.GetAttribute("style"), Does.Contain("left:0px;"));
		var splitter = splitContainer.Children[1];
		// Horizontal orientation has a vertical splitter
		Assert.That(splitter.GetAttribute("style"), Does.Contain("width:1000px;"));
		Assert.That(splitter.GetAttribute("style"), Does.Contain("height:4px;"));
		Assert.That(splitter.GetAttribute("style"), Does.Contain("top:250px;"));
		Assert.That(splitter.GetAttribute("style"), Does.Contain("left:0px;"));
		Assert.That(splitter.GetAttribute("style"), Does.Contain("left:0px;"));
		Assert.That(splitter.GetAttribute("style"), Does.Contain("cursor:ns-resize;"));
		var panel2 = splitContainer.Children[2];
		Assert.That(panel2.GetAttribute("style"), Does.Contain("width:1000px;"));
		Assert.That(panel2.GetAttribute("style"), Does.Contain("height:346px;"));
		Assert.That(panel2.GetAttribute("style"), Does.Contain("top:254px;"));
		Assert.That(panel2.GetAttribute("style"), Does.Contain("left:0px;"));
	}

	[Test]
	public async Task SplitContainerIsFixedShouldRenderFixedSplitter()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var splitContainer = new SplitContainer();
			splitContainer.Orientation = Orientation.Horizontal;
			splitContainer.IsSplitterFixed = true;
			splitContainer.Width = 1000;
			splitContainer.Height = 600;
			splitContainer.SplitterDistance = 250;
			return splitContainer;
		});

		var splitContainer = rendered.Find(".splitcontainer");
		var splitter = splitContainer.Children[1];

		Assert.That(splitter.GetAttribute("style"), Does.Contain("width:1000px;"));
		Assert.That(splitter.GetAttribute("style"), Does.Contain("height:4px;"));
		Assert.That(splitter.GetAttribute("style"), Does.Contain("top:250px;"));
		Assert.That(splitter.GetAttribute("style"), Does.Contain("left:0px;"));
		Assert.That(splitter.GetAttribute("style"), Does.Contain("left:0px;"));
		Assert.That(splitter.GetAttribute("style"), Does.Contain("cursor:default;"));
	}

	[Test]
	public async Task SplitContainerPanel1AndSplitterShouldBeHiddenIfPanel1IsCollapsed()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var splitContainer = new SplitContainer();
			splitContainer.Width = 1000;
			splitContainer.Height = 600;
			splitContainer.SplitterDistance = 250;
			splitContainer.Panel1Collapsed = true;
			splitContainer.Panel2Collapsed = false;
			splitContainer.Panel1.Controls.Add(new Label { Text = "First Panel" });
			splitContainer.Panel2.Controls.Add(new Label { Text = "Second Panel" });
			return splitContainer;
		});

		var splitContainer = rendered.Find(".splitcontainer");
		Assert.That(splitContainer.Children, Has.Length.EqualTo(1));
		var visiblePanel = splitContainer.Children[0];
		Assert.That(visiblePanel.ClassList, Does.Contain("splitterpanel"));
		Assert.That(visiblePanel.InnerHtml, Does.Not.Contain("First Panel"));
		Assert.That(visiblePanel.InnerHtml, Does.Contain("Second Panel"));
	}

	[Test]
	public async Task SplitContainerPanel2AndSplitterShouldBeHiddenIfPanel2IsCollapsed()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var splitContainer = new SplitContainer();
			splitContainer.Width = 1000;
			splitContainer.Height = 600;
			splitContainer.SplitterDistance = 250;
			splitContainer.Panel1Collapsed = false;
			splitContainer.Panel2Collapsed = true;
			splitContainer.Panel1.Controls.Add(new Label { Text = "First Panel" });
			splitContainer.Panel2.Controls.Add(new Label { Text = "Second Panel" });
			return splitContainer;
		});

		var splitContainer = rendered.Find(".splitcontainer");
		Assert.That(splitContainer.Children, Has.Length.EqualTo(1));
		var visiblePanel = splitContainer.Children[0];
		Assert.That(visiblePanel.ClassList, Does.Contain("splitterpanel"));
		Assert.That(visiblePanel.InnerHtml, Does.Contain("First Panel"));
		Assert.That(visiblePanel.InnerHtml, Does.Not.Contain("Second Panel"));
	}

	[Test, WithPlaywrightPage]
	public async Task SplitContainerDraggingHorizontalSplitterUpShouldShrinkPanel1AndExpandPanel2Height()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 1000, Height = 600 };
			form.Controls.Add(new SplitContainer()
			{
				Orientation = Orientation.Horizontal,
				Width = 1000,
				Height = 600,
				SplitterDistance = 250
			});
			return form;
		});

		var panel1 = await page.WaitForSelectorAsync(".splitcontainer .splitterpanel >> nth=0");
		var splitter = await page.WaitForSelectorAsync(".splitcontainer .splitter");
		var panel2 = await page.WaitForSelectorAsync(".splitcontainer .splitterpanel >> nth=1");

		AssertElementSizeAndPosition(panel1, 1000, 250, 0, 0);
		AssertElementSizeAndPosition(splitter, 1000, 4, 0, 250);
		AssertElementSizeAndPosition(panel2, 1000, 346, 0, 254);

		await DragSplitterAsync(page, splitter, -45, -100);

		AssertElementSizeAndPosition(panel1, 1000, 150, 0, 0);
		AssertElementSizeAndPosition(splitter, 1000, 4, 0, 150);
		AssertElementSizeAndPosition(panel2, 1000, 446, 0, 154);
	}
	
	[Test, WithPlaywrightPage]
	public async Task SplitContainerDraggingHorizontalSplitterDownShouldExpandPanel1AndShrinkPanel2Height()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 1000, Height = 600 };
			form.Controls.Add(new SplitContainer()
			{
				Orientation = Orientation.Horizontal,
				Width = 1000,
				Height = 600,
				SplitterDistance = 250
			});
			return form;
		});

		var panel1 = await page.WaitForSelectorAsync(".splitcontainer .splitterpanel >> nth=0");
		var splitter = await page.WaitForSelectorAsync(".splitcontainer .splitter");
		var panel2 = await page.WaitForSelectorAsync(".splitcontainer .splitterpanel >> nth=1");

		AssertElementSizeAndPosition(panel1, 1000, 250, 0, 0);
		AssertElementSizeAndPosition(splitter, 1000, 4, 0, 250);
		AssertElementSizeAndPosition(panel2, 1000, 346, 0, 254);

		await DragSplitterAsync(page, splitter, 45, 100);

		AssertElementSizeAndPosition(panel1, 1000, 350, 0, 0);
		AssertElementSizeAndPosition(splitter, 1000, 4, 0, 350);
		AssertElementSizeAndPosition(panel2, 1000, 246, 0, 354);
	}

	[Test, WithPlaywrightPage]
	public async Task SplitContainerDraggingVerticalSplitterLeftShouldShrinkPanel1AndExpandPanel2Width()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 1000, Height = 600 };
			form.Controls.Add(new SplitContainer()
			{
				Orientation = Orientation.Vertical,
				Width = 1000,
				Height = 600,
				SplitterDistance = 250
			});
			return form;
		});
		var panel1 = await page.WaitForSelectorAsync(".splitcontainer .splitterpanel >> nth=0");
		var splitter = await page.WaitForSelectorAsync(".splitcontainer .splitter");
		var panel2 = await page.WaitForSelectorAsync(".splitcontainer .splitterpanel >> nth=1");

		AssertElementSizeAndPosition(panel1, 250, 600, 0, 0);
		AssertElementSizeAndPosition(splitter, 4, 600, 250, 0);
		AssertElementSizeAndPosition(panel2, 746, 600, 254, 0);

		await DragSplitterAsync(page, splitter, -45, 100);

		AssertElementSizeAndPosition(panel1, 205, 600, 0, 0);
		AssertElementSizeAndPosition(splitter, 4, 600, 205, 0);
		AssertElementSizeAndPosition(panel2, 791, 600, 209, 0);
	}

	[Test, WithPlaywrightPage]
	public async Task SplitContainerDraggingVerticalSplitterRightShouldExpandPanel1AndShrinkPanel2Width()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 1000, Height = 600 };
			form.Controls.Add(new SplitContainer()
			{
				Orientation = Orientation.Vertical,
				Width = 1000,
				Height = 600,
				SplitterDistance = 250
			});
			return form;
		});

		var panel1 = await page.WaitForSelectorAsync(".splitcontainer .splitterpanel >> nth=0");
		var splitter = await page.WaitForSelectorAsync(".splitcontainer .splitter");
		var panel2 = await page.WaitForSelectorAsync(".splitcontainer .splitterpanel >> nth=1");

		AssertElementSizeAndPosition(panel1, 250, 600, 0, 0);
		AssertElementSizeAndPosition(splitter, 4, 600, 250, 0);
		AssertElementSizeAndPosition(panel2, 746, 600, 254, 0);

		await DragSplitterAsync(page, splitter, 45, -100);

		AssertElementSizeAndPosition(panel1, 295, 600, 0, 0);
		AssertElementSizeAndPosition(splitter, 4, 600, 295, 0);
		AssertElementSizeAndPosition(panel2, 701, 600, 299, 0);
	}

	[Test, WithPlaywrightPage]
	public async Task SplitContainerDraggingHorizontalSplitterUpGuideShouldNotMovePastPanel1MinSize()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 1000, Height = 600 };
			form.Controls.Add(new SplitContainer()
			{
				Orientation = Orientation.Horizontal,
				Width = 1000,
				Height = 600,
				SplitterDistance = 250,
				Panel1MinSize = 125,
			});
			return form;
		});

		var splitter = await page.WaitForSelectorAsync(".splitcontainer .splitter");
		await DragSplitterAsync(page, splitter, -300, -200, false);

		var splitterGuide = await page.WaitForSelectorAsync(".splitcontainer .splitter__guide");
		AssertElementSizeAndPosition(splitterGuide, 1000, 4, 0, 125);
		await page.Mouse.UpAsync();

		var panel1 = await page.WaitForSelectorAsync(".splitcontainer .splitterpanel >> nth=0");
		var panel2 = await page.WaitForSelectorAsync(".splitcontainer .splitterpanel >> nth=1");

		AssertElementSizeAndPosition(panel1, 1000, 125, 0, 0);
		AssertElementSizeAndPosition(splitter, 1000, 4, 0, 125);
		AssertElementSizeAndPosition(panel2, 1000, 471, 0, 129);
	}

	[Test, WithPlaywrightPage]
	public async Task SplitContainerDraggingHorizontalSplitterDownGuideShouldNotMovePastPanel2MinSize()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 1000 , Height = 600 };
			form.Controls.Add(new SplitContainer()
			{
				Orientation = Orientation.Horizontal,
				Width = 1000,
				Height = 600,
				SplitterDistance = 250,
				Panel2MinSize = 125,
			});
			return form;
		});

		var splitter = await page.WaitForSelectorAsync(".splitcontainer .splitter");
		await DragSplitterAsync(page, splitter, 300, 300, false);

		var splitterGuide = await page.WaitForSelectorAsync(".splitcontainer .splitter__guide");
		AssertElementSizeAndPosition(splitterGuide, 1000, 4, 0, 471);
		await page.Mouse.UpAsync();

		var panel1 = await page.WaitForSelectorAsync(".splitcontainer .splitterpanel >> nth=0");
		var panel2 = await page.WaitForSelectorAsync(".splitcontainer .splitterpanel >> nth=1");

		AssertElementSizeAndPosition(panel1, 1000, 471, 0, 0);
		AssertElementSizeAndPosition(splitter, 1000, 4, 0, 471);
		AssertElementSizeAndPosition(panel2, 1000, 125, 0, 475);
	}

	[Test, WithPlaywrightPage]
	public async Task SplitContainerDraggingVerticalSplitterLeftGuideShouldNotMovePastPanel1MinSize()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 1000, Height = 600 };
			form.Controls.Add(new SplitContainer()
			{
				Orientation = Orientation.Vertical,
				Width = 1000,
				Height = 600,
				SplitterDistance = 250,
				Panel1MinSize = 125,
			});
			return form;
		});

		var splitter = await page.WaitForSelectorAsync(".splitcontainer .splitter");
		await DragSplitterAsync(page, splitter, -200, 100, false);

		var splitterGuide = await page.WaitForSelectorAsync(".splitcontainer .splitter__guide");
		AssertElementSizeAndPosition(splitterGuide, 4, 600, 125, 0);
		await page.Mouse.UpAsync();

		var panel1 = await page.WaitForSelectorAsync(".splitcontainer .splitterpanel >> nth=0");
		var panel2 = await page.WaitForSelectorAsync(".splitcontainer .splitterpanel >> nth=1");
		AssertElementSizeAndPosition(panel1, 125, 600, 0, 0);
		AssertElementSizeAndPosition(splitter, 4, 600, 125, 0);
		AssertElementSizeAndPosition(panel2, 871, 600, 129, 0);
	}

	[Test, WithPlaywrightPage]
	public async Task SplitContainerDraggingVerticalSplitterRightGuideShouldNotMovePastPanel2MinSize()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 1000, Height = 600 };
			form.Controls.Add(new SplitContainer()
			{
				Orientation = Orientation.Vertical,
				Width = 1000,
				Height = 600,
				SplitterDistance = 250,
				Panel2MinSize = 125,
			});
			return form;
		});

		var splitter = await page.WaitForSelectorAsync(".splitcontainer .splitter");
		await DragSplitterAsync(page, splitter, 700, -100, false);

		var splitterGuide = await page.WaitForSelectorAsync(".splitcontainer .splitter__guide");
		AssertElementSizeAndPosition(splitterGuide, 4, 600, 871, 0);
		await page.Mouse.UpAsync();

		var panel1 = await page.WaitForSelectorAsync(".splitcontainer .splitterpanel >> nth=0");
		var panel2 = await page.WaitForSelectorAsync(".splitcontainer .splitterpanel >> nth=1");
		AssertElementSizeAndPosition(panel1, 871, 600, 0, 0);
		AssertElementSizeAndPosition(splitter, 4, 600, 871, 0);
		AssertElementSizeAndPosition(panel2, 125, 600, 875, 0);
	}

	[Test, WithPlaywrightPage]
	public async Task SplitContainerHorizontalSplitterShouldNotMoveIfNotMovable()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(new SplitContainer()
			{
				Orientation = Orientation.Horizontal,
				Width = 1000,
				Height = 600,
				SplitterDistance = 250,
				Panel1MinSize = 250,
				Panel2MinSize = 350,
			});
			return form;
		});

		var panel1 = await page.WaitForSelectorAsync(".splitcontainer .splitterpanel >> nth=0");
		var fixedSplitter = await page.WaitForSelectorAsync(".splitcontainer .splitter");
		var panel2 = await page.WaitForSelectorAsync(".splitcontainer .splitterpanel >> nth=1");

		Assert.That(fixedSplitter, Is.Not.Null);

		AssertElementSizeAndPosition(panel1, 1000, 250, 0, 0);
		AssertElementSizeAndPosition(fixedSplitter, 1000, 4, 0, 250);
		AssertElementSizeAndPosition(panel2, 1000, 346, 0, 254);

		await DragSplitterAsync(page, fixedSplitter, 45, 100);

		AssertElementSizeAndPosition(panel1, 1000, 250, 0, 0);
		AssertElementSizeAndPosition(fixedSplitter, 1000, 4, 0, 250);
		AssertElementSizeAndPosition(panel2, 1000, 346, 0, 254);
	}

	[Test, WithPlaywrightPage]
	public async Task SplitContainerVerticalSplitterShouldNotMoveIfNotMovable()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(new SplitContainer()
			{
				Orientation = Orientation.Vertical,
				Width = 1000,
				Height = 600,
				SplitterDistance = 250,
				Panel1MinSize = 250,
				Panel2MinSize = 750,
			});
			return form;
		});

		var panel1 = await page.WaitForSelectorAsync(".splitcontainer .splitterpanel >> nth=0");
		var fixedSplitter = await page.WaitForSelectorAsync(".splitcontainer .splitter");
		var panel2 = await page.WaitForSelectorAsync(".splitcontainer .splitterpanel >> nth=1");

		Assert.That(fixedSplitter, Is.Not.Null);

		AssertElementSizeAndPosition(panel1, 246, 600, 0, 0);
		AssertElementSizeAndPosition(fixedSplitter, 4, 600, 246, 0);
		AssertElementSizeAndPosition(panel2, 750, 600, 250, 0);

		await DragSplitterAsync(page, fixedSplitter, 45, -100);

		AssertElementSizeAndPosition(panel1, 246, 600, 0, 0);
		AssertElementSizeAndPosition(fixedSplitter, 4, 600, 246, 0);
		AssertElementSizeAndPosition(panel2, 750, 600, 250, 0);
	}

	[Test]
	public async Task SplitContainerPanelCanReRender()
	{
		using var ctx = new WinzorTestContext();
		SplitContainer splitContainer = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			splitContainer = new SplitContainer();
			splitContainer.Orientation = Orientation.Vertical;
			splitContainer.Width = 100;
			splitContainer.Height = 100;
			form.Controls.Add(splitContainer);
			var button1 = new Button();
			button1.Text = "One";
			button1.Click += Button1_Click;
			form.Controls.Add(button1);
			var button2 = new Button();
			button2.Text = "Two";
			button2.Click += Button2_Click;
			form.Controls.Add(button2);
			return form;
		});

		Assert.That(rendered.Markup, Does.Not.Contain("Foo"));
		Assert.That(rendered.Markup, Does.Not.Contain("Bar"));
		await rendered.Find("button:contains('One')").ClickAsync(new WebMouseEventArgs());
		Assert.That(rendered.Markup, Does.Contain("Foo"));
		Assert.That(rendered.Markup, Does.Not.Contain("Bar"));
		await rendered.Find("button:contains('Two')").ClickAsync(new WebMouseEventArgs());
		Assert.That(rendered.Markup, Does.Contain("Foo"));
		Assert.That(rendered.Markup, Does.Contain("Bar"));

		void Button1_Click(object sender, EventArgs e)
		{
			var label = new Label();
			label.Text = "Foo";
			splitContainer.Panel1.Controls.Add(label);
		}

		void Button2_Click(object sender, EventArgs e)
		{
			var label = new Label();
			label.Text = "Bar";
			splitContainer.Panel2.Controls.Add(label);
		}
	}

	[Test]
	public async Task DefaultBackColor()
	{
		using var ctx = new WinzorTestContext();
		SplitContainer splitContainer = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			splitContainer = new SplitContainer();
			return splitContainer;
		});

		Assert.That(splitContainer.BackColor, Is.EqualTo(SystemColors.Control));
		Assert.That(rendered.Find(".splitcontainer").GetAttribute("style"), Does.Contain("background-color:var(--color-control)"));
	}

	[Test]
	public async Task CustomBackColor()
	{
		using var ctx = new WinzorTestContext();
		SplitContainer splitContainer = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			splitContainer = new SplitContainer();
			splitContainer.BackColor = Color.Blue;
			return splitContainer;
		});

		Assert.That(splitContainer.BackColor, Is.EqualTo(Color.Blue));
		Assert.That(rendered.Find(".splitcontainer").GetAttribute("style"), Does.Contain("background-color:#0000FFFF"));
	}

	void AssertElementSizeAndPosition(IElementHandle element, int width, int height, int left, int top)
	{
		Assert.That(async () => (await element.BoundingBoxAsync()).Width, Is.EqualTo(width).After(3000, 100));
		Assert.That(async () => (await element.BoundingBoxAsync()).Height, Is.EqualTo(height).After(3000, 100));
		Assert.That(async () => (await element.BoundingBoxAsync()).X, Is.EqualTo(left).After(3000, 100));
		Assert.That(async () => (await element.BoundingBoxAsync()).Y, Is.EqualTo(top).After(3000, 100));
	}

		async Task DragSplitterAsync(IPage page, IElementHandle splitter, int xDelta, int yDelta, bool mouseUp = true)
		{
			var splitterBoundingBox = await splitter.BoundingBoxAsync();
			var splitterPositionX = splitterBoundingBox.X + splitterBoundingBox.Width / 2;
			var splitterPositionY = splitterBoundingBox.Y + splitterBoundingBox.Height / 2;
			await page.WaitForTimeoutAsync(200);
			await page.Mouse.MoveAsync(splitterPositionX, splitterPositionY, new MouseMoveOptions());
			await page.Mouse.DownAsync();
			await page.Mouse.MoveAsync(splitterPositionX + xDelta, splitterPositionY + yDelta, new MouseMoveOptions() { Steps = 5 });
			if (mouseUp)
			{
				await page.Mouse.UpAsync();
			}
		}

	[Test]
	public async Task SplitContainerNegativeSplitterDistanceThrowsException()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			Assert.That(() =>
			{
				var splitContainer = new SplitContainer();
				splitContainer.Width = 54;
				splitContainer.Height = 54;
				splitContainer.Panel1MinSize = 200;
				splitContainer.Panel2MinSize = 100;
			}, Throws.InvalidOperationException.With.Message.Contains("SplitterDistance must be between Panel1MinSize and Width - Panel2MinSize"));
		});
	}

	[WithPlaywrightPage]
	[TestCase(true, "-1", TestName = "SplitContainerFixedTabTest")]
	[TestCase(false,"0", TestName = "SplitContainerNotFixedTabTest")]
	public async Task SplitContainerUseTabKey(bool isfixed, string expectedTabIndex)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(new SplitContainer()
			{
				Orientation = Orientation.Vertical,
				Width = 1000,
				Height = 600,
				IsSplitterFixed = isfixed,
				TabStop = true,
				SplitterDistance = 250
			});
			return form;
		});

		var panel1 = await page.WaitForSelectorAsync(".splitcontainer .splitterpanel >> nth=0");
		var splitter = await page.WaitForSelectorAsync(".splitcontainer .splitter");
		var panel2 = await page.WaitForSelectorAsync(".splitcontainer .splitterpanel >> nth=1");

		await page.Keyboard.PressAsync("Tab");

		Assert.That(await splitter.GetAttributeAsync("tabindex"), Is.EqualTo(expectedTabIndex));
	}

	[TestCase(0, 0)]
	[TestCase(5, 5)]
	public async Task SplitContainerVerticalSplitterIsCorrectlyPositioned(int pointX, int pointY)
	{
		using var ctx = new WinzorTestContext();
		SplitContainer splitContainer = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			splitContainer = new SplitContainer()
			{
				Location = new Point(pointX, pointY)
			};
			return splitContainer;
		});

		var splitter = rendered.Find(".splitter");
		Assert.That(splitter.GetAttribute("style"), Does.Contain("top:0px"));
	}

	[TestCase(0, 0)]
	[TestCase(5, 5)]
	public async Task SplitContainerHorizontalSplitterIsCorrectlyPositioned(int pointX, int pointY)
	{
		using var ctx = new WinzorTestContext();
		SplitContainer splitContainer = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			splitContainer = new SplitContainer()
			{
				Orientation = Orientation.Horizontal,
				Location = new Point(pointX, pointY)
			};
			return splitContainer;
		});

		var splitter = rendered.Find(".splitter");
		Assert.That(splitter.GetAttribute("style"), Does.Contain("left:0px"));
	}

	[TestCase(Orientation.Horizontal, false, "splitter splitter--draggable--horizontal")]
	[TestCase(Orientation.Horizontal, true, "splitter")]
	[TestCase(Orientation.Vertical, false, "splitter splitter--draggable--vertical")]
	[TestCase(Orientation.Vertical, true, "splitter")]
	public async Task SplitContainerSplitterShouldHaveCorrectClasses(Orientation orientation, bool isSplitterFixed, string splitterClasses)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new SplitContainer()
		{
			Orientation = orientation,
			IsSplitterFixed = isSplitterFixed
		});
		Assert.That(rendered.Find(".splitter").GetAttribute("class"), Is.EqualTo(splitterClasses));
	}

	[WithPlaywrightPage]
	[TestCase(Orientation.Horizontal)]
	[TestCase(Orientation.Vertical)]
	public async Task SplitContainerSplitterGuideShouldHaveCorrectClassesUponDrag(Orientation orientation)
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 1000, Height = 600 };
			form.Controls.Add(new SplitContainer()
			{
				Orientation = orientation,
				Width = 1000,
				Height = 600,
				SplitterDistance = 250
			});
			return form;
		});

		var splitter = await page.WaitForSelectorAsync(".splitcontainer .splitter");
		await DragSplitterAsync(page, splitter, 1, 1, false);

		var splitterGuide = await page.WaitForSelectorAsync(".splitcontainer .splitter:last-of-type");
		Assert.That(await splitterGuide.GetAttributeAsync("class"), Does.Contain("splitter__guide"));
	}

	[WithPlaywrightPage]
	[TestCase(Orientation.Horizontal, "ArrowUp", "249px")]
	[TestCase(Orientation.Horizontal, "ArrowDown", "251px")]
	[TestCase(Orientation.Vertical, "ArrowLeft", "249px")]
	[TestCase(Orientation.Vertical, "ArrowRight", "251px")]
	public async Task SplitContainerPressArrowKey(Orientation orientation, string key, string expected)
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 1000, Height = 600 };
			form.Controls.Add(new SplitContainer()
			{
				Orientation = orientation,
				Width = 1000,
				Height = 600,
				SplitterDistance = 250
			});
			return form;
		});

		var splitter = await page.WaitForSelectorAsync(".splitcontainer .splitter");
		await DragSplitterAsync(page, splitter, 0, 0, true);
		await page.Keyboard.PressAsync(key);
		await Task.Delay(50);
		var property = Orientation.Horizontal == orientation ? "top" : "left";
		Assert.That(await splitter.EvaluateAsync<string>($"e => window.getComputedStyle(e).getPropertyValue('{property}')"), Is.EqualTo(expected));
	}

	[TestCase(true, true, "-1")]
	[TestCase(true, false, "0")]
	[TestCase(false, false, "-1")]
	public async Task SplitterHasCorrectTabIndex(bool tabStop, bool splitterFixed, string expectedValue)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var panel = new Panel() { TabStop = true };
			var splitContainer = new SplitContainer() { TabStop = tabStop, IsSplitterFixed = splitterFixed };
			panel.Controls.Add(splitContainer);
			return splitContainer;
		});

			var input = rendered.Find(".splitter");
			Assert.That(input.GetAttribute("tabindex"), Is.EqualTo(expectedValue));
	}

	[WithPlaywrightPage]
	[TestCase(Orientation.Horizontal)]
	[TestCase(Orientation.Vertical)]
	public async Task SplitContainerSplitterGuideShouldHaveCorrectClassesWhenClickedFirst(Orientation orientation)
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 1000, Height = 600 };
			form.Controls.Add(new SplitContainer()
			{
				Orientation = orientation,
				Width = 1000,
				Height = 600,
				SplitterDistance = 250
			});
			return form;
		});

		var splitter = await page.WaitForSelectorAsync(".splitcontainer .splitter");
		var splitterBoundingBox = await splitter.BoundingBoxAsync();
		var splitterPositionX = splitterBoundingBox.X + splitterBoundingBox.Width / 2;
		var splitterPositionY = splitterBoundingBox.Y + splitterBoundingBox.Height / 2;

		await page.WaitForTimeoutAsync(200);
		await page.Mouse.MoveAsync(splitterPositionX, splitterPositionY, new MouseMoveOptions());
		await page.Mouse.DownAsync();

		var splitterGuide = await page.WaitForSelectorAsync(".splitcontainer .splitter:last-of-type");
		Assert.That(await splitterGuide.GetAttributeAsync("class"), Does.Contain("splitter__guide"));
	}
}
