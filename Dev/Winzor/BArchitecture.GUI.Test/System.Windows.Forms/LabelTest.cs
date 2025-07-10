using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bunit;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorFramework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;

using static PlaywrightTestContext;

[TestFixture]
public class LabelTest
{
	const int stdLabelHeight = 13;
	const int stdLabelWidth = 76;
	const int sansSerifLabelHeight = 13;

	[Test]
	public async Task ClickEvent()
	{
		await ControlAssert.ImplementsEventAsync<Label, EventHandler>(nameof(Label.Click), a => new EventHandler((o, e) => a()), ".label", e => e.Click());
	}

	[Test]
	public async Task LabelCanTriggerNecessaryEvents()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new LabelWithEventAttribute(EventAttribute.MouseDown));
		var attributes = rendered.Find(".label").Attributes.Select(a => a.Name);

		Assert.That(attributes, Does.Contain("blazor:onclick"));
		Assert.That(attributes, Does.Contain("blazor:onmouseenter"));
		Assert.That(attributes, Does.Contain("blazor:onmouseleave"));
		Assert.That(attributes, Does.Contain("blazor:onmousedown"));
		Assert.That(attributes, Does.Not.Contain("blazor:onmousedown:stoppropagation"));
		Assert.That(attributes, Does.Contain("blazor:oncontextmenu"));
	}

	[Test]
	// Most dock styles without autosize should resize in some way to the parent
	[TestCase(DockStyle.Left, false, 0, 0, 50, 1000)]
	[TestCase(DockStyle.Right, false, 950, 0, 50, 1000)]
	[TestCase(DockStyle.Top, false, 0, 0, 1000, 50)]
	[TestCase(DockStyle.Bottom, false, 0, 950, 1000, 50)]
	[TestCase(DockStyle.Fill, false, 0, 0, 1000, 1000)]
	[TestCase(DockStyle.None, false, 0, 0, 50, 50)]
	// With autosize set, all should resize to the size of the text
	[TestCase(DockStyle.Left, true, 0, 0, stdLabelWidth, stdLabelHeight)]
	[TestCase(DockStyle.Right, true, 924, 0, stdLabelWidth, stdLabelHeight)]
	[TestCase(DockStyle.Top, true, 0, 0, stdLabelWidth, stdLabelHeight)]
	[TestCase(DockStyle.Bottom, true, 0, 987, stdLabelWidth, stdLabelHeight)]
	[TestCase(DockStyle.Fill, true, 0, 0, stdLabelWidth, stdLabelHeight)]
	[TestCase(DockStyle.None, true, 0, 0, stdLabelWidth, stdLabelHeight)]
	public async Task TestDockedLabelResizeBehavior(DockStyle dockStyle, bool autoSize, int expX, int expY, int expWidth, int expHeight)
	{
		using var ctx = new WinzorTestContext();
		Control parent = null;
		Label label1 = null;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			parent = new Control();
			parent.Height = 1000;
			parent.Width = 1000;
			label1 = getStandardLabel(dockStyle: dockStyle, autoSize: autoSize);
			parent.Controls.Add(label1);
		});
		Assert.That(label1.Bounds.X, Is.EqualTo(expX));
		Assert.That(label1.Bounds.Y, Is.EqualTo(expY));
		Assert.That(label1.Width, Is.EqualTo(expWidth));
		Assert.That(label1.Height, Is.EqualTo(expHeight));
	}

	[Test]
	// Most dock styles without autosize should resize in some way to the parent
	// default RenderControlOnForm size is 300x300
	[TestCase("ResizeHeight", DockStyle.Left, false, 50, 300)]
	[TestCase("ResizeHeight", DockStyle.Right, false, 50, 300)]
	[TestCase("ResizeWidth", DockStyle.Top, false, 300, 50)]
	[TestCase("ResizeWidth", DockStyle.Bottom, false, 300, 50)]
	[TestCase("ResizeBoth", DockStyle.Fill, false, 300, 300)]
	[TestCase("ResizeNone", DockStyle.None, false, 50, 50)]
	// With autosize set, all should resize to the size of the text
	[TestCase("ResizeToText", DockStyle.Left, true, stdLabelWidth, stdLabelHeight)]
	[TestCase("ResizeToText", DockStyle.Right, true, stdLabelWidth, stdLabelHeight)]
	[TestCase("ResizeToText", DockStyle.Top, true, stdLabelWidth, stdLabelHeight)]
	[TestCase("ResizeToText", DockStyle.Bottom, true, stdLabelWidth, stdLabelHeight)]
	[TestCase("ResizeToText", DockStyle.Fill, true, stdLabelWidth, stdLabelHeight)]
	[TestCase("ResizeToText", DockStyle.None, true, stdLabelWidth, stdLabelHeight)]
	public async Task TestDockedLabelBehaviorRenderedStyle(string expectedResult, DockStyle dockStyle,
		bool autoSize, int expWidth, int expHeight)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var label1 = getStandardLabel(dockStyle: dockStyle, autoSize: autoSize);
			return label1;
		});

		var renderedLabel = rendered.Find(".label");
		var styleStr = renderedLabel.GetAttribute("style");
		Assert.That(styleStr, Does.Contain($"width:{expWidth}px;"));
		Assert.That(styleStr, Does.Contain($"height:{expHeight}px;"));
	}

	[Test]
	// These special cases should resize with the form 
	[TestCase("ResizeWidth", AnchorStyles.Left | AnchorStyles.Right, false, 50, 50, 750, 50)]
	[TestCase("ResizeHeight", AnchorStyles.Top | AnchorStyles.Bottom, false, 50, 50, 50, 750)]
	[TestCase("ResizeWidth", AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top, false, 50, 50, 750, 50)]
	[TestCase("ResizeWidth", AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom, false, 50, 50, 750, 50)]
	[TestCase("ResizeHeight", AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left, false, 50, 50, 50, 750)]
	[TestCase("ResizeHeight", AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right, false, 50, 50, 50, 750)]
	[TestCase("ResizeBoth", AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, false, 50, 50, 750, 750)]
	// These cases should not resize
	[TestCase("ResizeNone", AnchorStyles.Top | AnchorStyles.Left, false, 50, 50, 50, 50)]
	[TestCase("ResizeNone", AnchorStyles.Top | AnchorStyles.Right, false, 50, 50, 50, 50)]
	[TestCase("ResizeNone", AnchorStyles.Bottom | AnchorStyles.Left, false, 50, 50, 50, 50)]
	[TestCase("ResizeNone", AnchorStyles.Bottom | AnchorStyles.Right, false, 50, 50, 50, 50)]
	// These cases should all resize to the actual text size, due to autosize being set
	[TestCase("ResizeToText", AnchorStyles.Left | AnchorStyles.Right, true, stdLabelWidth, stdLabelHeight, stdLabelWidth, stdLabelHeight)]
	[TestCase("ResizeToText", AnchorStyles.Top | AnchorStyles.Bottom, true, stdLabelWidth, stdLabelHeight, stdLabelWidth, stdLabelHeight)]
	[TestCase("ResizeToText", AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top, true, stdLabelWidth, stdLabelHeight, stdLabelWidth, stdLabelHeight)]
	[TestCase("ResizeToText", AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom, true, stdLabelWidth, stdLabelHeight, stdLabelWidth, stdLabelHeight)]
	[TestCase("ResizeToText", AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left, true, stdLabelWidth, stdLabelHeight, stdLabelWidth, stdLabelHeight)]
	[TestCase("ResizeToText", AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right, true, stdLabelWidth, stdLabelHeight, stdLabelWidth, stdLabelHeight)]
	[TestCase("ResizeToText", AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, true, stdLabelWidth, stdLabelHeight, stdLabelWidth, stdLabelHeight)]
	[TestCase("ResizeToText", AnchorStyles.Top | AnchorStyles.Left, true, stdLabelWidth, stdLabelHeight, stdLabelWidth, stdLabelHeight)]
	[TestCase("ResizeToText", AnchorStyles.Top | AnchorStyles.Right, true, stdLabelWidth, stdLabelHeight, stdLabelWidth, stdLabelHeight)]
	[TestCase("ResizeToText", AnchorStyles.Bottom | AnchorStyles.Left, true, stdLabelWidth, stdLabelHeight, stdLabelWidth, stdLabelHeight)]
	[TestCase("ResizeToText", AnchorStyles.Bottom | AnchorStyles.Right, true, stdLabelWidth, stdLabelHeight, stdLabelWidth, stdLabelHeight)]

	public async Task TestAnchoredLabelResizeBehaviorRendered(string expectedResult, AnchorStyles anchorStyle,
		bool autoSize, int w1, int h1, int w2, int h2)
	{
		using var ctx = new WinzorTestContext();
		var renderedForm = await ctx.RenderControlOnFormAsync(() =>
		{
			var label = getStandardLabel(anchorStyle: anchorStyle, autoSize: autoSize);
			return label;
		});
		var form1 = (Form)renderedForm.Instance.Control;
		var label1 = (Label)form1.Controls[0];

		Assert.That(label1.Width, Is.EqualTo(w1));
		Assert.That(label1.Height, Is.EqualTo(h1));
		await form1.InvokeWinzorDispatcherAsync(() =>
		{
			form1.Width = 1000;
			form1.Height = 1000;
		});
		Assert.That(form1.Width, Is.EqualTo(1000));
		Assert.That(form1.Height, Is.EqualTo(1000));
		Assert.That(label1.Width, Is.EqualTo(w2));
		Assert.That(label1.Height, Is.EqualTo(h2));
	}

	Label getStandardLabel(
		AnchorStyles anchorStyle = AnchorStyles.None,
		DockStyle dockStyle = DockStyle.None,
		bool autoSize = false)
	{
		var label = new Label
		{
			Width = 50,
			Height = 50,
			Text = "The New Label",
			Dock = dockStyle,
			AutoSize = autoSize
		};
		if (anchorStyle != AnchorStyles.None)
		{ label.Anchor = anchorStyle; }
		return label;
	}

	[Test]
	[TestCase("blah\r\nblah \r\nblah", "blah\nblah \nblah", TestName = "NewLinesDontChangeToBR")]
	[TestCase("This \t line \t has \t tabs", "This  line  has  tabs", TestName = "RemoveTabs")]
	[TestCase("This \t line \r\n\t has \t tabs", "This  line \n has  tabs", TestName = "RemoveTabsLeaveNewLines")]
	public async Task LabelConvertsLineBreaks(string inputText, string expectedResult = null)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var label1 = getStandardLabel();
			label1.Text = inputText;
			return label1;
		});

		var renderedLabel = rendered.Find(".label__text");
		var renderedLabelInnerHtml = renderedLabel.InnerHtml;
		Assert.That(renderedLabelInnerHtml, Is.EqualTo(expectedResult ?? inputText));
	}

	[Test]
	public async Task LabelElementAndClassCheck()
	{
		var labelText = "The Label";
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var label1 = getStandardLabel();
			label1.Text = labelText;
			return label1;
		});

		var renderedLabel = rendered.Find(".label");
		Assert.That(rendered.Find(".label__text").InnerHtml, Is.EqualTo(labelText));
		Assert.That(renderedLabel.NodeName, Is.EqualTo("DIV"));
		Assert.That(renderedLabel.ClassList, Does.Contain("label"));
	}

	[TestCase(DockStyle.Left, true, 78, sansSerifLabelHeight, TestName = "SansSerifDockLeftHeightCheck")]
	[TestCase(DockStyle.Right, true, 78, sansSerifLabelHeight, TestName = "SansSerifDockRightHeightCheck")]
	[TestCase(DockStyle.Top, true, 78, sansSerifLabelHeight, TestName = "SansSerifDockTopHeightCheck")]
	[TestCase(DockStyle.Bottom, true, 78, sansSerifLabelHeight, TestName = "SansSerifDockBottomHeightCheck")]
	[TestCase(DockStyle.Fill, true, 78, sansSerifLabelHeight, TestName = "SansSerifDockFillHeightCheck")]
	[TestCase(DockStyle.None, true, 78, sansSerifLabelHeight, TestName = "SansSerifDockNoneHeightCheck")]
	public async Task TestDockedLabelFontChange(DockStyle dockStyle,
	bool autoSize, int expWidth, int expHeight)
	{
		using var ctx = new WinzorTestContext();
		Control parent = null;
		Label label1 = null;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			parent = new Control();
			parent.Height = 1000;
			parent.Width = 1000;
			label1 = getStandardLabel(dockStyle: dockStyle, autoSize: autoSize);
			label1.Font = new Font("Microsoft Sans Serif", 8);
			parent.Controls.Add(label1);
		});
		Assert.That(label1.Width, Is.EqualTo(expWidth));
		Assert.That(label1.Height, Is.EqualTo(expHeight));
	}

	[Test]
	public async Task LabelAppliesCorrectFont()
	{
		using var ctx = new WinzorTestContext();
		Label label = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			label = new Label()
			{
				Font = new Font("Microsoft Sans Serif", 12, FontStyle.Bold | FontStyle.Italic | FontStyle.Underline | FontStyle.Strikeout),
				ForeColor = Color.AliceBlue,
				Width = 300,
				Height = 300,
				Text = "Label",
				UseMnemonic = false
			};
			form.Controls.Add(label);
			return form;
		});

		var renderedText = rendered.Find(".label span");
		var styleStr = renderedText.GetAttribute("style");
		Assert.That(styleStr, Does.Contain("color:#F0F8FFFF;font-family:Microsoft Sans Serif;line-height:19px;font-size:12pt;font-style:italic;font-weight:bold;text-decoration: underline line-through;"));

		await label.InvokeWinzorDispatcherAsync(() =>
		{
			label.Font = new Font("Microsoft Sans Serif", 10);
		});

		styleStr = renderedText.GetAttribute("style");
		Assert.That(styleStr, Does.Contain("color:#F0F8FFFF;font-family:Microsoft Sans Serif;line-height:16px;font-size:10pt;"));
	}

	[TestCase(ContentAlignment.TopLeft, "position: absolute; text-align: left; left: 3px; top: 0px;", "position: absolute; text-align: left; left: 3px; top: 0px;")]
	[TestCase(ContentAlignment.TopCenter, "position: absolute; text-align: center; left: 133.5px; top: 0px;", "position: absolute; text-align: center; left: 233.5px; top: 0px;")]
	[TestCase(ContentAlignment.TopRight, "position: absolute; text-align: right; left: 260px; top: 0px;", "position: absolute; text-align: right; left: 460px; top: 0px;")]
	[TestCase(ContentAlignment.MiddleLeft, "position: absolute; text-align: left; left: 3px; top: 142px;", "position: absolute; text-align: left; left: 3px; top: 242px;")]
	[TestCase(ContentAlignment.MiddleCenter, "position: absolute; text-align: center; left: 133.5px; top: 142px;", "position: absolute; text-align: center; left: 233.5px; top: 242px;")]
	[TestCase(ContentAlignment.MiddleRight, "position: absolute; text-align: right; left: 260px; top: 142px;", "position: absolute; text-align: right; left: 460px; top: 242px;")]
	[TestCase(ContentAlignment.BottomLeft, "position: absolute; text-align: left; left: 3px; top: 284px;", "position: absolute; text-align: left; left: 3px; top: 484px;")]
	[TestCase(ContentAlignment.BottomCenter, "position: absolute; text-align: center; left: 133.5px; top: 284px;", "position: absolute; text-align: center; left: 233.5px; top: 484px;")]
	[TestCase(ContentAlignment.BottomRight, "position: absolute; text-align: right; left: 260px; top: 284px;", "position: absolute; text-align: right; left: 460px; top: 484px;")]
	public async Task LabelAppliesCorrectTextAlign(ContentAlignment textAlign, string textStyleStringBefore, string textStyleStringAfter)
	{
		using var ctx = new WinzorTestContext();
		Label label = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			label = new Label()
			{
				Font = new Font("Microsoft Sans Serif", 10),
				Width = 300,
				Height = 300,
				Text = "Label",
				TextAlign = textAlign,
				UseMnemonic = false
			};
			form.Controls.Add(label);
			return form;
		});

		var renderedText = rendered.Find(".label span");
		var styleStr = renderedText.GetAttribute("style");
		Assert.That(styleStr, Does.Contain(textStyleStringBefore));

		await label.InvokeWinzorDispatcherAsync(() =>
		{
			label.Width = 500;
			label.Height = 500;
		});

		renderedText = rendered.Find(".label span");
		styleStr = renderedText.GetAttribute("style");
		Assert.That(styleStr, Does.Contain(textStyleStringAfter));
	}

	[Test]
	public async Task TextAlignChangedEventIsTriggeredWhenTextAlignChanges()
	{
		using var ctx = new WinzorTestContext();
		Label label = null;
		var eventFired = false;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			label = new Label()
			{
				Text = "Label",
				TextAlign = ContentAlignment.TopLeft
			};

			label.TextAlignChanged += (sender, e) =>
			{
				eventFired = true;
			};

			form.Controls.Add(label);
			return form;
		});

		await label.InvokeWinzorDispatcherAsync(() =>
		{
			label.TextAlign = ContentAlignment.BottomCenter;
		});

		Assert.That(eventFired, Is.True, "TextAlignChanged event should be fired when TextAlign is updated.");
	}

	[Test, WithPlaywrightPage]
	[TestCase(false, false)]
	[TestCase(false, true)]
	[TestCase(true, false)]
	[TestCase(true, true)]
	public async Task LabelTextEllipsisCorrectlyApplied(bool autoSize, bool autoEllipsis)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new Label
		{
			Text = "Great Label",
			AutoSize = autoSize,
			AutoEllipsis = autoEllipsis,
			Size = new Size(30, 20)
		});

		var label = page.Locator(".label__text");
		var hasEllipsis = !autoSize && autoEllipsis;

		Assert.That(await label.GetAttributeAsync("class"), hasEllipsis ? Does.Contain("label__text--ellipsis") : Does.Not.Contain("label__text--ellipsis"));
		Assert.That(await label.GetComputedStyleAsync("overflow"), Is.EqualTo(hasEllipsis ? "hidden" : "visible"));
		Assert.That((await label.GetComputedStyleAsync("width")).AsPixels, Is.EqualTo(autoSize ? 53 : 30).Within(1));
		Assert.That(await label.GetComputedStyleAsync("white-space"), Is.EqualTo(hasEllipsis ? "nowrap" : "break-spaces"));
		Assert.That(await label.GetComputedStyleAsync("text-overflow"), Is.EqualTo(hasEllipsis ? "ellipsis" : "clip"));
		Assert.That((await label.GetComputedStyleAsync("padding-right")).AsPixels, Is.EqualTo(hasEllipsis ? 10 : 0).Within(1));
	}

	[Test]
	[TestCase(ContentAlignment.TopLeft, "Microsoft Sans Serif", 10, "left: 3px")]
	[TestCase(ContentAlignment.TopLeft, "Microsoft Sans Serif", 14, "left: 4px")]
	[TestCase(ContentAlignment.TopLeft, "Microsoft Sans Serif", 36, "left: 12px")]
	[TestCase(ContentAlignment.TopLeft, "Arial", 10, "left: 3px")]

	public async Task LabelLeftPaddingForAnyLeftAlign(ContentAlignment textAlign, string font, int size, string leftPadding)
	{
		using var ctx = new WinzorTestContext();
		Label label = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			label = new Label()
			{
				Font = new Font(font, size),
				Width = 300,
				Height = 300,
				Text = "Label",
				TextAlign = textAlign,
				UseMnemonic = false
			};
			form.Controls.Add(label);
			return form;
		});

		var renderedText = rendered.Find(".label span");
		var styleStr = renderedText.GetAttribute("style");
		Assert.That(styleStr, Does.Contain(leftPadding));
	}

	[Test]
	public async Task LabelLeftShouldNotOverflowWhenTextWidthExceedsLabelWidth()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var label1 = getStandardLabel();
			label1.Font = new Font("Microsoft Sans Serif", 10);
			label1.Width = 20;
			label1.Height = 100;
			label1.Text = "This text has height larger than the label's so that we could test if the top is going to overflow when it's aligned to middle";
			label1.TextAlign = ContentAlignment.MiddleCenter;
			return label1;
		});

		var renderedText = rendered.Find(".label span");
		var styleStr = renderedText.GetAttribute("style");
		Assert.That(styleStr, Does.Contain("left: 0px"));
	}

	[Test]
	public async Task LabelTopShouldNotOverflowWhenTextAlignIsAnyMiddle()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var label1 = getStandardLabel();
			label1.Font = new Font("Microsoft Sans Serif", 10);
			label1.Width = 20;
			label1.Height = 30;
			label1.Text = "This text has height larger than the label's so that we could test if the top is going to overflow when it's aligned to middle";
			label1.TextAlign = ContentAlignment.MiddleCenter;
			return label1;
		});

		var renderedText = rendered.Find(".label span");
		var styleStr = renderedText.GetAttribute("style");
		Assert.That(styleStr, Does.Contain("top: 0px"));
	}

	[Test]
	[TestCase(BorderStyle.None, "label--border-none")]
	[TestCase(BorderStyle.FixedSingle, "label--border-fixedsingle")]
	[TestCase(BorderStyle.Fixed3D, "label--border-fixed3d")]
	public async Task TestLabelBorderStyle(BorderStyle borderStyle, string borderStyleClass)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var label1 = getStandardLabel();
			label1.BorderStyle = borderStyle;
			return label1;
		});

		var renderedLabel = rendered.Find(".label");
		Assert.That(renderedLabel.ClassList, Does.Contain(borderStyleClass));
	}

	[Test, WithPlaywrightPage]
	public async Task LabelRendersCorrectStyleFor3DBorder()
	{
		await using var ctx = new InMemoryTestServerContext();

		Label label = null;
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			label = new Label();
			label.BorderStyle = BorderStyle.Fixed3D;
			form.Controls.Add(label);
			return form;
		});

		var selectedLabel = await page.WaitForSelectorAsync(".label");
		var borderColor = await GetCssValue(selectedLabel, "border-color");
		var borderStyle = await GetCssValue(selectedLabel, "border-style");

		Assert.That(borderColor, Is.EqualTo("rgb(160, 160, 160) rgb(255, 255, 255) rgb(255, 255, 255) rgb(160, 160, 160)"));
		Assert.That(borderStyle, Is.EqualTo("solid"));
	}

	[Test, WithPlaywrightPage]
	public async Task ValidateLabelSizeIsIncludingPadding()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			Form form = new Form();
			var denseLabel = new Label();
			denseLabel.AutoSize = true;
			denseLabel.Name = "DenseLabel";
			denseLabel.TabIndex = 2;
			denseLabel.Text = "Hello World";
			denseLabel.Font = new Font("Tahoma", 6.2f);
			denseLabel.Dock = System.Windows.Forms.DockStyle.Left;
			denseLabel.Padding = new Padding(
				left: 1,
				top: 6,
				right: 0,
				bottom: 0);
			form.Controls.Add(denseLabel);
			return form;
		});

		var selectedLabel = await page.WaitForSelectorAsync(".label");
		var height = await GetCssValue(selectedLabel, "height");
		var width = await GetCssValue(selectedLabel, "width");
		Assert.That(height, Is.EqualTo("16px")); // without padding it will be 10px
		Assert.That(width, Is.EqualTo("48px"));

		var selectedLabelText = await page.WaitForSelectorAsync(".label__text");
		Assert.That(await GetCssValue(selectedLabelText, "top"), Is.EqualTo("6px"));
	}

	[Test, WithPlaywrightPage]
	public async Task TestLabelCursorStyle()
	{
		await using var ctx = new InMemoryTestServerContext();

		Label label = null;
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			label = new Label();
			form.Controls.Add(label);
			return form;
		});

		var selectedLabel = await page.WaitForSelectorAsync(".label");
		Assert.That(async () => await GetCssValue(selectedLabel, "user-select"), Is.EqualTo("none"));
	}

	[Test, WithPlaywrightPage]
	public async Task TestLabelHasCorrectStyleWhenResetFont()
	{
		await using var ctx = new InMemoryTestServerContext();

		Label label = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			label = new Label() { Font = new Font("Arial", 13f), TextAlign = ContentAlignment.TopRight, Text = "CW1 Support" };
			form.Controls.Add(label);
			return form;
		});

		var selectedLabelSpan = await page.WaitForSelectorAsync(".label span");
		Assert.That(await selectedLabelSpan.GetComputedStyleAsync("left"), Is.EqualTo("30px"));
		Assert.That(await selectedLabelSpan.GetComputedStyleAsync("font-family"), Is.EqualTo("Arial"));
		Assert.That((await selectedLabelSpan.GetComputedStyleAsync("font-size")).AsPixels, Is.EqualTo(17.3).Within(0.1));

		await label.InvokeWinzorDispatcherAsync(() => label.Font = new Font("Tahoma", 2f));

		Assert.That(await selectedLabelSpan.GetComputedStyleAsync("left"), Is.EqualTo("83px"));
		Assert.That(await selectedLabelSpan.GetComputedStyleAsync("font-family"), Is.EqualTo("Tahoma"));
		Assert.That((await selectedLabelSpan.GetComputedStyleAsync("font-size")).AsPixels, Is.EqualTo(2.7).Within(0.1));
	}

	[Test]
	public async Task TestLabelWithEmptyTextWhenAutoSizeIsEnabaled()
	{
		using var ctx = new WinzorTestContext();

		Label label = null;
		Form form = null;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			form = new Form();
			label = new Label() { AutoSize = true };
			form.Controls.Add(label);
		});

		Assert.That(label.Height, Is.EqualTo(13));
		Assert.That(label.Width, Is.Zero);
	}

	[Test]
	public async Task TestLabelResizeOnVisibleChanged()
	{
		using var ctx = new WinzorTestContext();
		Label label1 = null;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			Control parent = new Control();
			parent.Height = 1100;
			parent.Width = 1000;
			parent.Visible = false;
			label1 = new Label
			{
				Width = 300,
				Height = 400,
				Text = "Label",
				Visible = false,
				Dock = DockStyle.Fill
			};
			parent.Controls.Add(label1);
		});
		Assert.That(label1.Width, Is.EqualTo(300));
		Assert.That(label1.Height, Is.EqualTo(400));

		await label1.InvokeWinzorDispatcherAsync(() => label1.Visible = true);
		Assert.That(label1.Width, Is.EqualTo(1000));
		Assert.That(label1.Height, Is.EqualTo(1100));
	}

	[Test]
	public async Task TestLabelShouldHaveImageSetProperlyIfAny()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new Label { Image = new Bitmap(1, 1) });
		Assert.That(rendered.Find(".label img").GetAttribute("src"), Does.StartWith("data:image/png;base64,"));
	}
	[Test]
	public async Task TestLabelShouldHaveNoImageSetIfImageNull()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new Label());
		Assert.That(rendered.FindAll(".label img").Count, Is.EqualTo(0));
	}
	[Test]
	public async Task TestLabelImageChangedFromServer()
	{
		await ControlAssert.ImplementsChangeFromServerAsync<Label>(c => c.Image = new Bitmap(1, 1));
	}

	async Task<string> GetCssValue(IElementHandle element, string cssProperty) => (await element.EvaluateAsync($"e => window.getComputedStyle(e).getPropertyValue('{cssProperty}')")).Value.ToString();

	[Test]
	public async Task LabelUseMnemonicUpdatesFromServer()
	{
		await ControlAssert.ImplementsChangeFromServerAsync<Label>(c => c.UseMnemonic = false);
	}

	[Test, Explicit] //Explicit until label process mnemonic is implemented
	public async Task LabelProcessMnemonicFocusesNextControl()
	{
		using var ctx = new WinzorTestContext();
		TextBox textBox = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var label = new Label() { Text = "&Test" };
			textBox = new TextBox();
			form.Controls.Add(label);
			form.Controls.Add(textBox);
			return form;
		});

		var form = rendered.Find(".form");
		await rendered.KeyPressAsync(Keys.Alt | Keys.T, form);
		Assert.That(textBox.Focused, Is.True);
	}

	[Test]
	public async Task LabelUseMnemonicFormatsTestWithMnemonic()
	{
		using var ctx = new WinzorTestContext();
		Label label = null;
		var expectHtml = "Test & Test";
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			label = new Label() { Text = expectHtml, UseMnemonic = true };
			form.Controls.Add(label);
			return form;
		});
		var actualHtml = WebUtility.HtmlDecode(rendered.Find(".label__text").InnerHtml);
		Assert.That(actualHtml, Is.EqualTo(expectHtml));
	}

	[Test, WithPlaywrightPage]
	[TestCase(false)]
	[TestCase(true)]
	public async Task TestLabelTextStyle(bool useMnemonic)
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var label = new Label() { Text = "&Test", UseMnemonic = useMnemonic };
			form.Controls.Add(label);

			return form;
		});

		var labelText = await page.WaitForSelectorAsync(".label__text");
		Assert.That(async () => await labelText.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('white-space')"), Is.EqualTo("break-spaces"));
	}

	[Test, WithPlaywrightPage]
	public async Task TestLabelHideOverflowText()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			var label = getStandardLabel();
			form = new Form();
			form.Controls.Add(label);
			return form;
		});

		var selectedLabel = await page.WaitForSelectorAsync(".label");
		Assert.That(async () => await selectedLabel.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('overflow')"), Is.EqualTo("hidden"));
	}

	[Test]
	public async Task TestLabelDisabledShouldHaveDisabledClass()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var label1 = getStandardLabel();
			label1.Enabled = false;
			return label1;
		});

		var renderedLabel = rendered.Find(".label");
		Assert.That(renderedLabel.ClassList, Does.Contain("label-disabled"));
	}

	[Test]
	public async Task TestLabelEnabledShouldNotHaveDisabledClass()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var label1 = getStandardLabel();
			label1.Enabled = true;
			return label1;
		});

		var renderedLabel = rendered.Find(".label");
		Assert.That(renderedLabel.ClassList, Does.Not.Contain("label-disabled"));
	}

	[Test, WithPlaywrightPage]
	public async Task TestLabelDisabledShouldHaveDisabledStyling()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			var label = getStandardLabel();
			label.Enabled = false;
			form = new Form();
			form.Controls.Add(label);
			return form;
		});

		var selectedLabel = await page.WaitForSelectorAsync(".label");
		Assert.That(async () => await selectedLabel.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('color')"), Is.EqualTo("rgb(189, 189, 190)"));
		Assert.That(async () => await selectedLabel.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('border-color')"), Is.EqualTo("rgb(189, 189, 190)"));
	}

	[TestCase(BorderStyle.None, 0)]
	[TestCase(BorderStyle.FixedSingle, 1)]
	[TestCase(BorderStyle.Fixed3D, 1)]

	public async Task TestBorderStyleAdjustForClientSize(BorderStyle bs, int borderSize)
	{
		using var ctx = new WinzorTestContext();
		Label targetLabel = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Size = new Size(500, 500);
			targetLabel = new Label();
			form.Controls.Add(targetLabel);
			targetLabel.Location = new Point(10, 10);
			targetLabel.Size = new Size(200, 50);

			targetLabel.BorderStyle = bs;

			return form;
		});

		var b = targetLabel.Bounds;
		Assert.That(b.X, Is.EqualTo(10));
		Assert.That(b.Y, Is.EqualTo(10));
		Assert.That(targetLabel.ClientSize.Width + 2 * borderSize, Is.EqualTo(b.Width));
		Assert.That(targetLabel.ClientSize.Height + 2 * borderSize, Is.EqualTo(b.Height));
		Assert.That(targetLabel.ClientAreaBounds.X, Is.EqualTo(b.X + borderSize));
		Assert.That(targetLabel.ClientAreaBounds.Y, Is.EqualTo(b.Y + borderSize));
	}

	[TestCase(BorderStyle.None, BorderStyle.FixedSingle, true, 2)]
	[TestCase(BorderStyle.None, BorderStyle.Fixed3D, true, 2)]
	[TestCase(BorderStyle.None, BorderStyle.None, false, 0)]
	public async Task BorderStyleChangeTriggersRecalculation(BorderStyle initial, BorderStyle bs, bool expectChange, int expectedDiff)
	{
		using var ctx = new WinzorTestContext();
		Label targetLabel = null;

		bool clientSizeChanged = false;
		int clientSizeDiff = 0;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Size = new Size(500, 500);
			targetLabel = new Label();
			form.Controls.Add(targetLabel);
			targetLabel.Location = new Point(10, 10);
			targetLabel.Size = new Size(200, 50);

			targetLabel.BorderStyle = BorderStyle.None;

			var clientSizeBefore = targetLabel.ClientSize;
			targetLabel.ClientSizeChanged += (sender, e) =>
			{
				clientSizeChanged = true;
				clientSizeDiff = clientSizeBefore.Width - targetLabel.ClientSize.Width;
			};

			return form;
		});

		clientSizeChanged = false;

		await targetLabel.InvokeWinzorDispatcherAsync(() =>
		{
			targetLabel.BorderStyle = bs;
		});

		Assert.That(clientSizeChanged, Is.EqualTo(expectChange));
		Assert.That(clientSizeDiff, Is.EqualTo(expectedDiff));
	}

	[TestCase(ContentAlignment.TopLeft, "left: 23px; top: 20px;")]
	[TestCase(ContentAlignment.TopCenter, "left: 26px; top: 20px;")]
	[TestCase(ContentAlignment.TopRight, "left: 6px; top: 20px;")]
	[TestCase(ContentAlignment.MiddleLeft, "left: 23px; top: 43.5px;")]
	[TestCase(ContentAlignment.MiddleCenter, "left: 26px; top: 43.5px;")]
	[TestCase(ContentAlignment.MiddleRight, "left: 6px; top: 43.5px;")]
	[TestCase(ContentAlignment.BottomLeft, "left: 23px; top: 47px;")]
	[TestCase(ContentAlignment.BottomCenter, "left: 26px; top: 47px;")]
	[TestCase(ContentAlignment.BottomRight, "left: 6px; top: 47px;")]
	public async Task LabelAppliesCorrectPadding(ContentAlignment alignment, string expectedStyle)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new Label()
		{
			Text = "test string",
			Padding = new Padding(20, 20, 20, 20),
			AutoSize = false,
			Size = new Size(80, 80),
			TextAlign = alignment
		});

		var label = rendered.Find(".label__text");

		Assert.That(label.GetAttribute("style"), Does.Contain(expectedStyle));
	}

	[Test]
	public async Task LabelHasToolTip()
	{
		using var ctx = new WinzorTestContext();
		var toolTipText = "this a tooltop text";
		var rendered = await ctx.RenderControlOnFormAsync(() => new Label { ToolTipText = toolTipText });

		Assert.That(rendered.Find(".label").GetAttribute("title"), Is.EqualTo(toolTipText));
	}

	[Test]
	public async Task LabelBackgroundTransparentWhenParentHasImageAndNoBackColorSet()
	{
		using var ctx = new WinzorTestContext();
		Label label1 = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form() { BackgroundImage = TestImage.GetImage() };
			label1 = getStandardLabel();
			form.Controls.Add(label1);
			return form;
		});

		Assert.That(label1.BackColor, Is.EqualTo(Color.Transparent));
		Assert.That(rendered.Find(".label").GetAttribute("style"), Does.Contain("background-image: inherit; background-repeat: no-repeat;"));
	}

	[Test, WithPlaywrightPage]
	public async Task LabelBackgroundImageCroppedAndCentered()
	{
		await using var ctx = new InMemoryTestServerContext();

		Label label = null;
		Form form = null;
		var labelWidth = 150;
		var labelHeight = 100;
		var image = TestImage.GetImage();

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			label = new Label();
			label.Width = labelWidth;
			label.Height = labelHeight;
			label.Image = image;
			form.Controls.Add(label);
			return form;
		});

		var selectedLabel = await page.WaitForSelectorAsync(".label img");
		var objectFit = await GetCssValue(selectedLabel, "object-fit");
		var width = await GetCssValue(selectedLabel, "width");
		var height = await GetCssValue(selectedLabel, "height");

		Assert.That(objectFit, Is.EqualTo("none"));
		Assert.That(width, Is.EqualTo($"{labelWidth}px"));
	}

	[Test]
	public async Task LabelSizeUpdateWhenFontChange()
	{
		using var ctx = new WinzorTestContext();
		Label label = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			AnchorStyles anchorStyle = AnchorStyles.Left | AnchorStyles.Top;
			DockStyle dockStyle = DockStyle.Left;
			label = getStandardLabel(anchorStyle, dockStyle, true);
			label.Font = new Font("Microsoft Sans Serif", 8);
			label.Width = 100;
			label.Height = 30;
			label.Text = "Test";
			return label;
		});

		Assert.That(label.Height, Is.EqualTo(label.Font.Height));
		Assert.That(label.Height, Is.GreaterThan(8));
		Assert.That(label.Height, Is.LessThan(30));

		await label.InvokeWinzorDispatcherAsync(() => label.Font = new Font("Microsoft Sans Serif", 36));
		Assert.That(label.Height, Is.EqualTo(label.Font.Height));
		Assert.That(label.Height, Is.GreaterThan(36));
	}

	[Test]
	public async Task HtmlInTextIsEscaped()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new Label() { Text = "<img src=\"x\"/>" });
		Assert.That(rendered.FindAll("img"), Is.Empty);
	}

	[Test, WithPlaywrightPage]
	public async Task DoubleClickLabelShouldCopyTextToClipboard()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new Label { Text = "CopyToClipboard" });

		await BrowserContext.GrantPermissionsAsync(new[] { "clipboard-write", "clipboard-read" });
		await page.EvaluateAsync<string>("() => navigator.clipboard.writeText('')");
		await Task.Delay(500);
		var clipboardTextBeforeDblClick = await page.EvaluateAsync<string>("() => navigator.clipboard.readText()");
		Assert.That(clipboardTextBeforeDblClick, Is.Empty);

		var labelElement = page.Locator(".label");
		await labelElement.DblClickAsync();
		await Task.Delay(500);
		var clipboardTextAfterDblClick = await page.EvaluateAsync<string>("() => navigator.clipboard.readText()");
		Assert.That(clipboardTextAfterDblClick, Is.EqualTo("CopyToClipboard"));
	}

	[Test]
	public async Task DoubleClickLabelWithTextNull()
	{
		using var ctx = new WinzorTestContext();
		LabelWithEventAttribute label = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => label = new LabelWithEventAttribute() { Text = null });
		Assert.DoesNotThrow(() => label.OnMouseDoubleClick(null));
	}

	[TestCase(null, "System.Windows.Forms.Label, Text: ")]
	[TestCase("", "System.Windows.Forms.Label, Text: ")]
	[TestCase("Sample Label Text", "System.Windows.Forms.Label, Text: Sample Label Text")]
	[TestCase("12345", "System.Windows.Forms.Label, Text: 12345")]
	public async Task ToString_ReturnsExpectedForLabel(string inputText, string expectedOutput)
	{
		using var ctx = new WinzorTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() => new Label() { Text = inputText });
		var label = rendered.GetControl<Label>();

		Assert.That(label.ToString(), Is.EqualTo(expectedOutput));
	}

	class LabelWithEventAttribute : Label
	{
		readonly EventAttribute eventAttribute;

		public LabelWithEventAttribute() : base() { }

		public LabelWithEventAttribute(EventAttribute eventAttribute)
		{
			this.eventAttribute = eventAttribute;
		}

		protected override EventAttribute EventAttributes => eventAttribute;

		protected internal override bool ShouldRender => Visible;

		public new void OnMouseDoubleClick(MouseEventArgs e)
		{
			base.OnMouseDoubleClick(e);
		}
	}
}
