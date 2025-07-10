using System.Collections;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bunit;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;

using static PlaywrightTestContext;

[Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1093:DoNotUseSystemWindowsFormsToolStripControls", Justification = "Testing")]
class ToolStripButtonTest
{
	[Test]
	public async Task ToolStripButtonShouldHaveAssociatedStylingClass()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStrip.Items.Add(new ToolStripButton());
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripButton = rendered.Find(".toolstrip-item--button");
		Assert.That(toolStripButton, Is.Not.Null);
	}

	[Test]
	public async Task ToolStripButtonShouldNotGetFocusAfterClicking()
	{
		ToolStrip toolStrip = null;
		Button button = null;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			toolStrip = new ToolStrip();
			toolStrip.Items.Add(new ToolStripButton());
			button = new Button() { Location = new Point(0, 100) };
			form.Controls.Add(toolStrip);
			form.Controls.Add(button);
			return form;
		});

		await rendered.Find(".button").ClickAsync(new WebMouseEventArgs());
		Assert.That(button.Focused, Is.True);

		await rendered.Find(".toolstrip-item--button").MouseDownAsync(new WebMouseEventArgs());
		Assert.That(button.Focused, Is.True);
	}

	[Test]
	public async Task ToolStripButtonShouldHaveButtonImageTextClasses_WithImageAndText()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStrip.Items.Add(new ToolStripButton { Image = TestImage.GetImage(), Text = "New" });
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripButton = rendered.Find(".button__button--with-image-and-text");
		Assert.That(toolStripButton, Is.Not.Null);
	}

	[Test]
	public async Task ToolStripButtonShouldHaveButtonImageTextClasses_WithImageOnly()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStrip.Items.Add(new ToolStripButton { Image = TestImage.GetImage() });
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripButton = rendered.Find(".button__button--with-image-only");
		Assert.That(toolStripButton, Is.Not.Null);
	}

	[Test]
	public async Task ToolStripButtonShouldHaveButtonImageTextClasses_WithTextOnly()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStrip.Items.Add(new ToolStripButton { Text = "New" });
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripButton = rendered.Find(".button__button--with-text-only");
		Assert.That(toolStripButton, Is.Not.Null);
	}

	[Test]
	public async Task ToolStripButtonShouldHaveCorrectAlignmentRight()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip() { AutoSize = false, Width = 300 };
			toolStrip.Items.Add(new ToolStripButton { AutoSize = false, Width = 100, Alignment = ToolStripItemAlignment.Right });
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripButton = rendered.Find(".toolstrip-item--button");
		Assert.That(toolStripButton.GetAttribute("style"), Does.Contain("left:200px;"));
	}

	[Test]
	public async Task ToolStripButtonShouldHaveCorrectAlignmentLeft()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip() { AutoSize = false, Width = 300, GripStyle = ToolStripGripStyle.Hidden };
			toolStrip.Items.Add(new ToolStripButton { AutoSize = false, Width = 100, Alignment = ToolStripItemAlignment.Left });
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripButton = rendered.Find(".toolstrip-item--button");
		Assert.That(toolStripButton.GetAttribute("style"), Does.Contain("left:0px;"));
	}

	[Test]
	public async Task ToolStripButtonShouldHaveCorrectPadding()
	{
		using var ctx = new WinzorTestContext();
		ToolStripButton toolStripButton = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStrip.Items.Add(toolStripButton = new ToolStripButton { Margin = new Padding(5), Padding = new Padding(0, 0, 1, 0) });
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripButtonElm = rendered.Find(".toolstrip-item--button");
		Assert.That(toolStripButtonElm.ClassList, Contains.Item("pl-0"));
		Assert.That(toolStripButtonElm.ClassList, Contains.Item("pt-0"));
		Assert.That(toolStripButtonElm.ClassList, Contains.Item("pr-1"));
		Assert.That(toolStripButtonElm.ClassList, Contains.Item("pb-0"));
	}

	[Test]
	public async Task ToolStripButtonShouldHaveTextSetProperly()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStrip.Items.Add(new ToolStripButton { Text = "Blah" });
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripButton = rendered.Find(".toolstrip-item--button .button__button__text");
		Assert.That(toolStripButton.TextContent, Is.EqualTo("Blah"));
	}

	[Test]
	public async Task ToolStripButtonShouldHaveImageSetProperlyIfAny()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStrip.Items.Add(new ToolStripButton { Image = TestImage.GetImage() });
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripButtonImage = rendered.Find(".toolstrip-item--button .button__button__image");
		Assert.That(toolStripButtonImage.GetAttribute("src"), Does.StartWith("data:image/png;base64,"));
	}

	[Test]
	public async Task ToolStripButtonHasClickEvent()
	{
		await ControlAssert.ImplementsEventAsync<ToolStripButtonForTest, EventHandler>(nameof(ToolStripButtonForTest.Click), a => new EventHandler((o, e) => a()), ".toolstrip-item--button .button__button", e => e.Click());
	}

	[Test]
	public async Task ToolStripButtonHasMouseEnterEvent()
	{
		await ControlAssert.ImplementsEventAsync<ToolStripButton, EventHandler>(nameof(ToolStripButton.MouseEnter), a => new EventHandler((o, e) => a()), ".toolstrip-item--button .button__button", e => e.MouseEnter());
	}

	[Test]
	public async Task ToolStripButtonHasMouseLeaveEvent()
	{
		await ControlAssert.ImplementsEventAsync<ToolStripButton, EventHandler>(nameof(ToolStripButton.MouseLeave), a => new EventHandler((o, e) => a()), ".toolstrip-item--button .button__button", e => e.MouseLeave());
	}

	[Test]
	public async Task ToolStripButtonImageChangedFromServer()
	{
		await ControlAssert.ImplementsChangeFromServerAsync<ToolStripButton>(c => c.Image = TestImage.GetImage());
	}

	[Test]
	public async Task ToolStripButtonTextChangeTriggerOwnerUpdate()
	{
		using var ctx = new WinzorTestContext();
		ToolStripButton button = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			button = new ToolStripButton { Text = "New" };
			toolStrip.Items.Add(button);
			form.Controls.Add(toolStrip);
			return form;
		});

		await button.InvokeWinzorDispatcherAsync(() => { button.Text = "Save"; });
		var toolStripButton = rendered.Find(".toolstrip-item--button .button__button__text");
		Assert.That(toolStripButton.TextContent, Is.EqualTo("Save"));
	}

	[Test]
	[TestCase(true, "Enabled Btn", TestName = "{m}_EnabledButton")]
	[TestCase(false, "Disabled Btn", TestName = "{m}_DisabledButton")]
	public async Task ToolStripButtonDisabledClass(bool enabled, string text)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStrip.Items.Add(new ToolStripButton { Enabled = enabled, Text = text });
			form.Controls.Add(toolStrip);
			return form;
		});

		var buttonWrapperClass = rendered.Find(".toolstrip-item--button").Attributes["class"]?.Value;
		Assert.That(buttonWrapperClass, enabled ? Does.Not.Contain("disabled") : Does.Contain("disabled"));
	}

	[Test, WithPlaywrightPage]
	public async Task ToolStripButtonClickDisabled()
	{
		var clicked = new[] { false, false };
		await using var ctx = new InMemoryTestServerContext();
		ToolStripButton disabledToolStripButton = null;
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var toolStrip = new ToolStrip();

			disabledToolStripButton = new ToolStripButton { Text = "0 Disabled Btn", Enabled = false };
			disabledToolStripButton.Click += (sender, args) => { clicked[0] = true; };
			toolStrip.Items.Add(disabledToolStripButton);

			var enabledToolStripButton = new ToolStripButton { Text = "1 Enabled Btn", Enabled = true };
			enabledToolStripButton.Click += (sender, args) => { clicked[1] = true; };
			toolStrip.Items.Add(enabledToolStripButton);

			return toolStrip;
		});

		await page.WaitForSelectorAsync(".toolstrip-item");
		var toolStripButtons = await page.QuerySelectorAllAsync(".toolstrip-item--button .button__button");

		var clickDisabledButtonOptions = new ElementHandleClickOptions() { Force = true };
		await toolStripButtons[0].ClickAsync(clickDisabledButtonOptions);
		await toolStripButtons[1].ClickAsync();

		Assert.That(() => clicked[1], Is.True.After(1000, 100));
		Assert.That(clicked[0], Is.False);

		// Simulate changing button to Enabled state from server
		_ = ctx.WinzorDispatcher.InvokeAsync(() => disabledToolStripButton.Enabled = true);
		await toolStripButtons[0].ClickAsync(clickDisabledButtonOptions);
		Assert.That(() => clicked[0], Is.True.After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task ToolStripButtonsDoNotOverlap()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip() { Width = 1000 };
			toolStrip.Items.Add(new ToolStripButton { Text = "Button 0" });
			toolStrip.Items.Add(new ToolStripButton { Text = "Button 1", Image = TestImage.GetImage() });
			toolStrip.Items.Add(new ToolStripButton { Text = "Button 2 with long text", Image = TestImage.GetImage() });
			form.Controls.Add(toolStrip);
			return form;
		});

		await page.WaitForSelectorAsync(".toolstrip-item--button");
		var buttons = await page.QuerySelectorAllAsync(".toolstrip-item--button");
		var button0 = await buttons[0].BoundingBoxAsync();
		var button1 = await buttons[1].BoundingBoxAsync();
		var button2 = await buttons[2].BoundingBoxAsync();

		Assert.That(button0.X + button0.Width, Is.Not.GreaterThan(button1.X));
		Assert.That(button1.X + button1.Width, Is.Not.GreaterThan(button2.X));
	}

	[Test]
	public async Task ToolStripButtonsHaveStyleAttribute()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStrip.Items.Add(new ToolStripButton { Text = "Button 0", Image = TestImage.GetImage() });
			form.Controls.Add(toolStrip);
			return form;
		});

		var button = rendered.Find(".toolstrip-item--button");
		var image = rendered.Find(".toolstrip-item--button .button__button__image");

		Assert.That(button.OuterHtml, Does.Contain("style="));
		Assert.That(image.OuterHtml, Does.Contain("style="));
	}

	[Test, WithPlaywrightPage]
	public async Task ToolStripButtonImageAndTextPositionedInsideButton()
	{
		const int minPadding = 1;

		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStrip.Items.Add(new ToolStripButton { Text = "Button 0" });
			toolStrip.Items.Add(new ToolStripButton { Text = "Button 1", Image = TestImage.GetImage() });
			form.Controls.Add(toolStrip);
			return form;
		});
		await page.WaitForSelectorAsync(".toolstrip-item--button");
		var buttons = await page.QuerySelectorAllAsync(".toolstrip-item--button");
		var button = await buttons[0].BoundingBoxAsync();
		var buttonText = await (await buttons[0].WaitForSelectorAsync(".button__button__text")).BoundingBoxAsync();

		TestBoundingBoxIsInside(buttonText, button, minPadding);

		button = await buttons[1].BoundingBoxAsync();
		buttonText = await (await buttons[1].WaitForSelectorAsync(".button__button__text")).BoundingBoxAsync();
		var buttonImage = await (await buttons[1].WaitForSelectorAsync(".button__button__image")).BoundingBoxAsync();

		TestBoundingBoxIsInside(buttonText, button, minPadding);
		TestBoundingBoxIsInside(buttonImage, button, minPadding);

		//Ensure that button image and text have a gap between each other.
		Assert.That(buttonImage.X + buttonImage.Width + minPadding, Is.Not.GreaterThan(buttonText.X));
	}

	[TestCase(true, true, true, "0")]
	[TestCase(false, true, true, "-1")]
	[TestCase(true, false, true, "-1")]
	[TestCase(false, false, true, "-1")]
	[TestCase(true, true, false, "-1")]
	[TestCase(false, true, false, "-1")]
	[TestCase(true, false, false, "-1")]
	[TestCase(false, false, false, "-1")]
	public async Task TestToolStripButtonHasCorrectTabIndex(bool parentTabStop, bool enabled, bool tabStop, string expectedTabIndex)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStrip.TabStop = parentTabStop;
			toolStrip.Items.Add(new ToolStripButton { Enabled = enabled, TabStop = tabStop });
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripButton = rendered.Find(".toolstrip-item--button .button__button");
		Assert.That(toolStripButton.GetAttribute("tabindex"), Is.EqualTo(expectedTabIndex));
	}

	[TestCaseSource(typeof(ToolTipTestCases), nameof(ToolTipTestCases.TestToolTips))]
	public async Task ToolStripButtonShouldShowTooltipOnHover(bool showItemToolTips, bool autoToolTip, string toolTipText, string expectedTitle)
	{
		using var ctx = new WinzorTestContext();
		ToolStripButton button = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip { ShowItemToolTips = showItemToolTips };
			button = new ToolStripButton
			{
				AutoToolTip = autoToolTip,
				Text = "&TestText",
				ToolTipText = toolTipText
			};
			toolStrip.Items.Add(button);
			form.Controls.Add(toolStrip);
			return form;
		});

		var renderedButton = rendered.Find(".toolstrip-item--button .button__button");
		Assert.That(renderedButton.GetAttribute("title"), Is.EqualTo(expectedTitle));
	}

	public class ToolTipTestCases
	{
		public static IEnumerable TestToolTips
		{
			get
			{
				yield return new TestCaseData(true, true, "TestToolTipText", "TestToolTipText");
				yield return new TestCaseData(true, false, "TestToolTipText", "TestToolTipText");
				yield return new TestCaseData(false, true, "TestToolTipText", null);
				yield return new TestCaseData(false, false, "TestToolTipText", null);
				yield return new TestCaseData(true, true, null, "TestText");
				yield return new TestCaseData(true, false, null, null);
				yield return new TestCaseData(false, true, null, null);
				yield return new TestCaseData(false, false, null, null);
			}
		}
	}

	[Test, WithPlaywrightPage]
	public async Task ToolStripButtonsShouldNotBeOffsetByCSSAsync(
		[Values("first", "second")] string buttonTitle,
		[Values("left", "right", "top", "bottom", "horizontal", "vertical", "all")] string style,
		[Values(0, 1, 5, 10)] int marginSize
	)
	{
		await using var ctx = new InMemoryTestServerContext();

		int left = (style == "left" || style == "horizontal" || style == "all") ? marginSize : 0;
		int right = (style == "right" || style == "horizontal" || style == "all") ? marginSize : 0;
		int top = (style == "top" || style == "vertical" || style == "all") ? marginSize : 0;
		var bottom = (style == "bottom" || style == "vertical" || style == "all") ? marginSize : 0;
		var margin = new Padding(left, top, right, bottom);

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();

			var firstButton = new ToolStripButton
			{
				Name = "first",
				Text = "first",
				Margin = margin
			};
			toolStrip.Items.Add(firstButton);

			var secondButton = new ToolStripButton
			{
				Name = "second",
				Text = "second",
				Margin = margin
			};
			toolStrip.Items.Add(secondButton);

			form.Controls.Add(toolStrip);

			return form;
		});

		var toolStripButton = page.Locator(".toolstrip-item", new PageLocatorOptions() { Has = page.Locator($"button[title=\"{buttonTitle}\"]") });
		var buttonStyle = await toolStripButton.GetAttributeAsync("style");

		var leftStyle = Convert.ToInt32(Regex.Match(buttonStyle, @"left:(\d+)px").Groups[1].Value);
		var topStyle = Convert.ToInt32(Regex.Match(buttonStyle, @"top:(\d+)px").Groups[1].Value);
		var widthStyle = Convert.ToInt32(Regex.Match(buttonStyle, @"width:(\d+)px").Groups[1].Value);
		var heightStyle = Convert.ToInt32(Regex.Match(buttonStyle, @"height:(\d+)px").Groups[1].Value);

		var firstBounding = await toolStripButton.BoundingBoxAsync();
		var renderedLeft = (int)firstBounding.X;
		var renderedTop = (int)firstBounding.Y;
		var renderedWidth = (int)firstBounding.Width;
		var renderedHeight = (int)firstBounding.Height;

		Assert.That(renderedLeft, Is.EqualTo(leftStyle), "X position should match server-calculated absolute position");
		Assert.That(renderedTop, Is.EqualTo(topStyle), "Y position should match server-calculated absolute position");
		Assert.That(renderedWidth, Is.EqualTo(widthStyle), "Width should match server-calculated width");
		Assert.That(renderedHeight, Is.EqualTo(heightStyle), "Height should match server-calculated height");
	}

	void TestBoundingBoxIsInside(ElementHandleBoundingBoxResult innerBox, ElementHandleBoundingBoxResult outerBox, int minPadding)
	{
		Assert.That(innerBox.X, Is.GreaterThan(outerBox.X + minPadding));
		Assert.That(innerBox.X + innerBox.Width + minPadding, Is.LessThan(outerBox.X + outerBox.Width));
		Assert.That(innerBox.Y, Is.GreaterThan(outerBox.Y + minPadding));
		Assert.That(innerBox.Y + innerBox.Height + minPadding, Is.LessThan(outerBox.Y + outerBox.Height));
	}

	[TestCase(true, ToolStripItemDisplayStyle.Image, 31, 31)]
	[TestCase(false, ToolStripItemDisplayStyle.Image, 10, 17)]
	[TestCase(false, ToolStripItemDisplayStyle.ImageAndText, 10, 17)]
	[TestCase(false, ToolStripItemDisplayStyle.Text, 0, 0)]
	[TestCase(false, ToolStripItemDisplayStyle.None, 0, 0)]
	public async Task ImageLayoutSizing(bool autoSize, ToolStripItemDisplayStyle display, int correctWidth, int correctHeight)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip { Width = 200, Height = 100, Dock = DockStyle.None,ImageScalingSize = new Size(31,31) };
			toolStrip.Items.Add(new ToolStripButton { AutoSize = autoSize, DisplayStyle = display, Width = 21, Height = 21, Image = TestImage.GetImage() });
			form.Controls.Add(toolStrip);
			return form;
		});

		if ((display & ToolStripItemDisplayStyle.Image) == ToolStripItemDisplayStyle.Image)
		{
			var buttonImageClass = rendered.Find(".button__button__image");
			Assert.That(buttonImageClass.GetAttribute("style"), Does.Contain($"width:{correctWidth}px;"));
			Assert.That(buttonImageClass.GetAttribute("style"), Does.Contain($"height:{correctHeight}px;"));
		}
		else
		{
			Assert.That(rendered.FindAll(".button__button__image").Count, Is.EqualTo(0));
		}
	}

	[Test]
	public async Task ImageAlignChangedFromServer()
	{
		await ControlAssert.ImplementsChangeFromServerAsync<ToolStripButton>(c =>
		{
			Assert.That(c.ImageAlign, Is.EqualTo(ContentAlignment.MiddleCenter));
			c.ImageAlign = ContentAlignment.MiddleLeft;
		});
	}

	[Test]
	public async Task TextAlignChangedFromServer()
	{
		await ControlAssert.ImplementsChangeFromServerAsync<ToolStripButton>(c =>
		{
			Assert.That(c.TextAlign, Is.EqualTo(ContentAlignment.MiddleCenter));
			c.TextAlign = ContentAlignment.MiddleLeft;
		});
	}

	[Test]
	public async Task TextImageRelationChangedFromServer()
	{
		await ControlAssert.ImplementsChangeFromServerAsync<ToolStripButton>(c =>
		{
			Assert.That(c.TextImageRelation, Is.EqualTo(TextImageRelation.ImageBeforeText));
			c.TextImageRelation = TextImageRelation.Overlay;
		});
	}

	[Test]
	public async Task ToolStripButtonCheckedChangedEventTriggeredProperly()
	{
		using var ctx = new WinzorTestContext();
		ToolStripButton toolStripButton = null;
		var checkedChangedTriggered = 0;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStripButton = new ToolStripButton() { Text = "Item1", Checked = false };
			toolStripButton.CheckedChanged += (s, e) =>
			{
				checkedChangedTriggered++;
			};
			toolStrip.Items.Add(toolStripButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		Assert.That(checkedChangedTriggered, Is.EqualTo(0));

		await toolStripButton.InvokeWinzorDispatcherAsync(() =>
		{
			toolStripButton.Checked = true;
		});
		Assert.That(checkedChangedTriggered, Is.EqualTo(1));

		await toolStripButton.InvokeWinzorDispatcherAsync(() =>
		{
			toolStripButton.Checked = true;
		});
		Assert.That(checkedChangedTriggered, Is.EqualTo(1));

		await toolStripButton.InvokeWinzorDispatcherAsync(() =>
		{
			toolStripButton.Checked = false;
		});
		Assert.That(checkedChangedTriggered, Is.EqualTo(2));
	}

	[Test]
	public async Task ToolStripButtonCheckedChangedOnClickOnlyIfCheckOnClickIsTrue()
	{
		using var ctx = new WinzorTestContext();
		ToolStripButton toolStripButton = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStripButton = new ToolStripButton() { Text = "Item1", Checked = false, CheckOnClick = true };
			toolStrip.Items.Add(toolStripButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		Assert.That(toolStripButton.Checked, Is.False);

		await toolStripButton.InvokeWinzorDispatcherAsync(() =>
		{
			toolStripButton.PerformClick();
		});
		Assert.That(toolStripButton.Checked, Is.True);

		await toolStripButton.InvokeWinzorDispatcherAsync(() =>
		{
			toolStripButton.CheckOnClick = false;
			toolStripButton.PerformClick();
		});
		Assert.That(toolStripButton.Checked, Is.True);
	}

	[Test]
	public async Task ToolStripButtonCheckedNotChangedOnClickIfNotEnabled()
	{
		using var ctx = new WinzorTestContext();
		ToolStripButton toolStripButton = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStripButton = new ToolStripButton() { Text = "Item1", Checked = false, CheckOnClick = true, Enabled = false };
			toolStrip.Items.Add(toolStripButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		Assert.That(toolStripButton.Checked, Is.False);

		await toolStripButton.InvokeWinzorDispatcherAsync(() =>
		{
			toolStripButton.PerformClick();
		});
		Assert.That(toolStripButton.Checked, Is.False);

		await toolStripButton.InvokeWinzorDispatcherAsync(() =>
		{
			toolStripButton.Enabled = true;
			toolStripButton.PerformClick();
		});
		Assert.That(toolStripButton.Checked, Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task ToolStripButtonStylesAppliedCorrectlyWhenChecked()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStrip.Items.Add(new ToolStripButton { Text = "Button 0", Checked = true });
			form.Controls.Add(toolStrip);
			return form;
		});

		await page.WaitForSelectorAsync(".toolstrip-item--button");
		var toolStripButtonOverlay = await page.QuerySelectorAsync(".toolstrip-item__visual-overlay");

		Assert.That(async () => await toolStripButtonOverlay.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-color')"), Is.EqualTo("rgb(195, 225, 249)"));
		Assert.That(async () => await toolStripButtonOverlay.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('border')"), Is.EqualTo("1px solid rgb(146, 204, 250)"));
	}

	[Test, WithPlaywrightPage]
	public async Task ToolStripButtonStylesAppliedCorrectlyWhenDisabled()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var toolStrip = new ToolStrip();
			toolStrip.Items.Add(new ToolStripButton { Text = "Button 0", Enabled = false });
			return toolStrip;
		});

		var toolStripButton = await page.WaitForSelectorAsync(".toolstrip-item--button .button__button");
		Assert.That(async () => await toolStripButton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('color')"), Is.EqualTo("rgb(128, 128, 128)"));
	}

	[Test]
	public async Task ToolStripButtonPreventClickEventWhenHandlingAsyncClickEvent()
	{
		using var ctx = new WinzorTestContext();
		int counter = 0;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStripButton = new ToolStripButton { Text = "New" };
			toolStripButton.Click += (sender, args) => counter++;
			var toolStrip = new ToolStrip();
			toolStrip.Items.Add(toolStripButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripButton = rendered.Find(".toolstrip-item--button .button__button");
		int clickCount = 10;
		for (int i = 0; i < clickCount; i++)
		{
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			toolStripButton.ClickAsync(new WebMouseEventArgs());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
		}

		await Task.Delay(100);
		Assert.That(counter, Is.LessThan(clickCount));
	}

	[Test]
	public async Task ToolStripButtonShouldNotHaveDraggableImage()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStrip.Items.Add(new ToolStripButton { Image = TestImage.GetImage() });
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripButtonImage = rendered.Find(".toolstrip-item--button .button__button__image");
		Assert.That(toolStripButtonImage.GetAttribute("draggable"), Is.EqualTo("false"));
	}

	[Test]
	public async Task ToolStripButtonProcessMnemonicSelectsFirstControl()
	{
		using var ctx = new WinzorTestContext();
		var clicked = false;
		ToolStripButton stripButton = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			stripButton = new ToolStripButton() { Text = "&Test" };
			stripButton.Click += (sender, args) => clicked = true;
			toolStrip.Items.Add(stripButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		Assert.That(clicked, Is.False);
		var toolStripItem = rendered.Find(".toolstrip-item");
		await rendered.KeyPressAsync(Keys.Alt | Keys.T, toolStripItem);
		Assert.That(clicked, Is.True);

		clicked = false;

		Assert.That(clicked, Is.False);
		var form = rendered.Find(".form");
		await rendered.KeyPressAsync(Keys.Alt | Keys.T, form);
		Assert.That(clicked, Is.True);
	}

	[Test]
	public async Task ToolStripButtonUseMnemonicFormatsTestWithMnemonic()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			var stripButton = new ToolStripButton() { Text = "&Test" };
			toolStrip.Items.Add(stripButton);
			form.Controls.Add(toolStrip);
			return form;
		});
		Assert.That(rendered.Find(".button__button__text").InnerHtml, Is.EqualTo("<span class=\"mnemonickey\">T</span>est"));
	}

	[Test, WithPlaywrightPage]
	public async Task ToolStripButtonImageMoveWhenClick()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var toolStrip = new ToolStrip();
			var image = TestImage.GetImage();
			toolStrip.Items.Add(new ToolStripButton { Image = image });
			toolStrip.Items.Add(new ToolStripButton { Image = image, Enabled = false });
			return toolStrip;
		});

		await page.WaitForSelectorAsync(".toolstrip-item");
		var toolStripButtons = await page.QuerySelectorAllAsync(".toolstrip-item--button");
		var enabledToolStripButton = toolStripButtons[0];
		var enabledToolStripButtonImage = await enabledToolStripButton.QuerySelectorAsync(".button__button__image");
		var disabledToolStripButton = toolStripButtons[1];
		var disabledToolStripButtonImage = await disabledToolStripButton.QuerySelectorAsync(".button__button__image");

		var clickTask = enabledToolStripButton.ClickAsync(new ElementHandleClickOptions() { Delay = 500 });
		await Task.Delay(100);
		Assert.That(await enabledToolStripButtonImage.EvaluateAsync<string>($"e => window.getComputedStyle(e).getPropertyValue('margin-left')"), Is.EqualTo("1px"));
		await clickTask;

		_ = disabledToolStripButton.ClickAsync(new ElementHandleClickOptions() { Delay = 500 });
		await Task.Delay(100);
		Assert.That(await disabledToolStripButtonImage.EvaluateAsync<string>($"e => window.getComputedStyle(e).getPropertyValue('margin-left')"), Is.EqualTo("0px"));
	}

	[Test, WithPlaywrightPage]
	public async Task ToolStripButtonShouldHaveInheritBackgroundColorIfTransparent()
	{
		await using var ctx = new InMemoryTestServerContext();
		ToolStripButton toolStripButton = null;

		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var toolStrip = new ToolStrip();
			toolStripButton = new ToolStripButton() { Text = "Test", BackColor = Color.Transparent };
			toolStrip.Items.Add(toolStripButton);
			return toolStrip;
		});

		Assert.That(toolStripButton, Is.Not.Null);
		var locatedStripButton = page.Locator(".toolstrip-item--button .button__button");
		var stripButtonParent = page.Locator(".toolstrip-item--button");
		var expectedBackgroundColor = await stripButtonParent.GetComputedStyleAsync("background-color");
		Assert.That(async () => await locatedStripButton.GetComputedStyleAsync("background-color"), Is.EqualTo(expectedBackgroundColor));
	}
}

internal class ToolStripButtonForTest : ToolStripButton
{
	protected override bool IsLeftButtonDown => true;
}
