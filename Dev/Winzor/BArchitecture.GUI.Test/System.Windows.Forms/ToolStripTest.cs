using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Bunit;
using NUnit.Framework;
using WinzorTestFramework;

namespace System.Windows.Forms;

[SuppressMessage("CargoWiseOne", "CW1093:DoNotUseSystemWindowsFormsToolStripControls", Justification = "Testing")]
class ToolStripTest
{
	[Test]
	public async Task MouseDownEvent()
	{
		await ControlAssert.ImplementsEventAsync<ToolStrip, MouseEventHandler>(nameof(GroupBox.MouseDown), a => new MouseEventHandler((o, e) => a()), ".toolstrip", e => e.MouseDown());
	}

	[Test]
	public async Task ToolStripDefaultValues()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var control = new ToolStrip();
			Assert.That(control.CausesValidation, Is.False);
			Assert.That(control.CanSelect, Is.False);
		});
	}

	[Test]
	public async Task ToolStripShouldHaveAssociatedStylingClass()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripWrapper = rendered.Find(".toolstrip");
		Assert.That(toolStripWrapper, Is.Not.Null);
	}

	[Test]
	public async Task ToolStripShouldHaveChildrenItemRendered()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip() { GripStyle = ToolStripGripStyle.Hidden };
			toolStrip.Items.Add(new ToolStripButton());
			toolStrip.Items.Add(new ToolStripButton());
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripWrapper = rendered.Find(".toolstrip");
		Assert.That(toolStripWrapper.Children.Where(c => c.ClassList.Contains("toolstrip-item--button")).ToList(), Has.Count.EqualTo(2));
	}

	[Test]
	public async Task ToolStripShouldHaveCorrectSizeAndLocation()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip { Top = 100, Left = 200, Width = 330, Height = 440, AutoSize = false, Dock = DockStyle.None };
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripWrapper = rendered.Find(".toolstrip");
		Assert.That(toolStripWrapper.GetAttribute("style"), Contains.Substring("position:absolute;width:330px;height:440px;top:100px;left:200px;"));
	}

	[TestCase(KnownColor.Control, "var(--color-control)", TestName = "KnownColor:Control")]
	[TestCase(KnownColor.ActiveCaption, "var(--color-active-caption)", TestName = "KnownColor:ActiveCaption")]
	[TestCase(KnownColor.Window, "var(--color-window)", TestName = "KnownColor:Window")]
	public async Task ToolStripShouldHaveBackgroundColorBasedOnBackColor(KnownColor knownColor, string renderedColor)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var toolStrip = new ToolStrip()
			{
				GripStyle = ToolStripGripStyle.Hidden,
				RenderMode = ToolStripRenderMode.System,
				BackColor = Color.FromKnownColor(knownColor)
			};
			toolStrip.Items.Add(new ToolStripButton() { Text = "Item One" });
			toolStrip.Items.Add(new ToolStripButton() { Text = "Item Two" });
			toolStrip.Items.Add(new ToolStripButton() { Text = "Item Three" });
			return toolStrip;
		});

		var toolStripWrapper = rendered.Find(".toolstrip");
		Assert.That(toolStripWrapper.GetAttribute("style"), Contains.Substring($"background-color:{renderedColor}"));
	}

	[Test]
	public async Task ToolStripHorizontalWithDropDownButtonItemsHaveCorrectSizeAndPositionSet()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var toolStrip = new ToolStrip();
			var dropDownButton = new ToolStripDropDownButton() { Text = "Item One", AutoSize = false, Size = new Size(50, 25), Image = TestImage.GetImage() };
			dropDownButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			toolStrip.Items.Add(dropDownButton);
			dropDownButton = new ToolStripDropDownButton() { Text = "Item Two" };
			dropDownButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			toolStrip.Items.Add(dropDownButton);
			dropDownButton = new ToolStripDropDownButton() { Text = "Item Three" };
			dropDownButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			toolStrip.Items.Add(dropDownButton);
			return toolStrip;
		});

		var items = rendered.FindAll(".toolstrip-item--dropdownbutton");
		AssertItemPosition(items[0], 50, 25, 8, 1);
		AssertItemPosition(items[1], 66, 25, 58, 1);
		AssertItemPosition(items[2], 73, 25, 124, 1);
	}

	[Test]
	public async Task ToolStripHorizontalItemsHaveCorrectSizeAndPositionSet()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var toolStrip = new ToolStrip() { GripStyle = ToolStripGripStyle.Hidden };
			toolStrip.Items.Add(new ToolStripButton() { Text = "Item One", AutoSize = false, Size = new Size(50, 25) });
			toolStrip.Items.Add(new ToolStripButton() { Text = "Item Two" });
			toolStrip.Items.Add(new ToolStripButton() { Text = "Item Three" });
			return toolStrip;
		});

		var items = rendered.FindAll(".toolstrip-item--button");
		AssertItemPosition(items[0], 50, 25, 0, 1);
		AssertItemPosition(items[1], 60, 25, 50, 1);
		AssertItemPosition(items[2], 67, 25, 110, 1);
	}

	[Test]
	public async Task ToolStripVerticalItemsHaveCorrectSizeAndPositionSet()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var toolStrip = new ToolStrip() { LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow, GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.Left };
			toolStrip.Items.Add(new ToolStripButton() { Text = "Item One", AutoSize = false, Size = new Size(50, 25) });
			toolStrip.Items.Add(new ToolStripButton() { Text = "Item Two" });
			toolStrip.Items.Add(new ToolStripButton() { Text = "Item Three" });
			return toolStrip;
		});

		var items = rendered.FindAll(".toolstrip-item--button");
		AssertItemPosition(items[0], 50, 25, 8, 1);
		AssertItemPosition(items[1], 66, 20, 0, 29);
		AssertItemPosition(items[2], 66, 20, 0, 52);
	}

	[Test]
	public async Task ToolStripHasDefaultFontStyles()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStrip.BackColor = Color.FromArgb(255, 237, 123, 49);
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripWrapper = rendered.Find(".toolstrip");
		Assert.That(toolStripWrapper.GetAttribute("style"), Contains.Substring("font-family:Segoe UI"));
		Assert.That(toolStripWrapper.GetAttribute("style"), Contains.Substring("font-size:9pt"));
		Assert.That(toolStripWrapper.GetAttribute("style"), Contains.Substring("line-height:16px"));
	}

	[Test]
	public async Task ItemClickedEventRaisedForToolStripButton()
	{
		using var ctx = new WinzorTestContext();
		ToolStripItem clickedItem = null;
		ToolStripButtonForTest toolStripButton = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStrip.ItemClicked += (s, e) => clickedItem = e.ClickedItem;
			toolStripButton = new ToolStripButtonForTest();
			toolStrip.Items.Add(toolStripButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		await rendered.Find(".toolstrip-item--button .button__button").ClickAsync(new WebMouseEventArgs());
		Assert.That(clickedItem, Is.EqualTo(toolStripButton));
	}

	[Test]
	public async Task ItemClickedEventRaisedForToolStripSplitButton()
	{
		using var ctx = new WinzorTestContext();
		ToolStripItem clickedItem = null;
		ToolStripSplitButton toolStripSplitButton = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStrip.ItemClicked += (s, e) => clickedItem = e.ClickedItem;
			toolStripSplitButton = new ToolStripSplitButton();
			toolStrip.Items.Add(toolStripSplitButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		await rendered.Find(".splitbutton__button").ClickAsync(new WebMouseEventArgs());
		Assert.That(clickedItem, Is.EqualTo(toolStripSplitButton));
	}

	[Test]
	public async Task AddingItemShouldTriggerLayoutRequired()
	{
		using var ctx = new WinzorTestContext();
		await ctx.RenderControlOnFormAsync(() =>
		{
			var toolStrip = new ToolStrip();
			Assert.That(toolStrip.LayoutRequired, Is.False);
			toolStrip.Items.Add(new ToolStripMenuItem { Text = "Good boy" });
			Assert.That(toolStrip.LayoutRequired, Is.True);
			return toolStrip;
		});
	}

	[Test]
	public async Task ToolStripShouldNotGotFocusAfterClicking()
	{
		ToolStrip toolStrip = null;
		Button button = null;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			toolStrip = new ToolStrip();
			button = new Button() { Location = new Point(0, 100) };
			form.Controls.Add(toolStrip);
			form.Controls.Add(button);
			return form;
		});

		await rendered.Find(".button").ClickAsync(new WebMouseEventArgs());
		Assert.That(button.Focused, Is.True);

		await rendered.Find(".toolstrip").MouseDownAsync(new WebMouseEventArgs());
		Assert.That(button.Focused, Is.True);
	}

	[Test]
	public async Task ToolStripItemsAreNotInTheControlsCollection()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var toolStrip = new ToolStrip();
			toolStrip.Items.Add(new ToolStripButton { Text = "Item One" });
			toolStrip.Items.Add(new ToolStripButton { Text = "Item Two" });

			Assert.That(toolStrip.Items.Count, Is.EqualTo(2));
			Assert.That(toolStrip.Controls.Count, Is.EqualTo(0));
		});
	}

	[Test]
	public async Task ToolStripItemsAreDisposed()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var toolStrip = new ToolStrip();
			var item1 = new ToolStripButton { Text = "Item One" };
			var item2 = new ToolStripButton { Text = "Item Two" };
			toolStrip.Items.AddRange(new[] { item1, item2 });

			Assert.That(toolStrip.Items.Count, Is.EqualTo(2));

			toolStrip.Dispose();
			Assert.That(item1.IsDisposed, Is.True);
			Assert.That(item2.IsDisposed, Is.True);
			Assert.That(toolStrip.IsDisposed, Is.True);
			Assert.That(toolStrip.Items.Count, Is.EqualTo(0));
		});
	}

	void AssertItemPosition(IElement item, int width, int height, int left, int top)
	{
		var styleString = item.GetAttribute("style");
		Assert.That(!string.IsNullOrEmpty(styleString));
		Assert.That(styleString, Does.Contain($"width:{width}px"));
		Assert.That(styleString, Does.Contain($"height:{height}px"));
		Assert.That(styleString, Does.Contain($"left:{left}px"));
		Assert.That(styleString, Does.Contain($"top:{top}px"));
	}
}
