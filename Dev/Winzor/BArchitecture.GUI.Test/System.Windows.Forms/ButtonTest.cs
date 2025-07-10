using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Threading.Tasks;
using Bunit;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;

using static PlaywrightTestContext;

class ButtonTest
{
	[Test]
	public async Task ClickEvent()
	{
		await ControlAssert.ImplementsEventAsync<Button, EventHandler>(nameof(Button.Click), a => new EventHandler((o, e) => a()), "button", e => e.Click());
	}

	[Test]
	public async Task MouseEnterEvent()
	{
		await ControlAssert.ImplementsEventAsync<Button, EventHandler>(nameof(Button.MouseEnter), a => new EventHandler((o, e) => a()), "button", e => e.MouseEnter());
	}

	[Test]
	public async Task MouseOutEvent()
	{
		await ControlAssert.ImplementsEventAsync<Button, EventHandler>(nameof(Button.MouseLeave), a => new EventHandler((o, e) => a()), "button", e => e.MouseLeave());
	}

	[Test]
	public async Task MouseDownEvent()
	{
		await ControlAssert.ImplementsEventAsync<Button, MouseEventHandler>(nameof(Button.MouseDown), a => new MouseEventHandler((o, e) => a()), "button", e => e.MouseDown());
	}

	[Test]
	public async Task ButtonText()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new Button { Text = "buttontext" });
		Assert.That(rendered.Find("button div div").InnerHtml, Is.EqualTo("buttontext"));
	}

	[Test]
	public async Task ButtonTextShouldNotHaveInheritHeightInlineStyle()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new Button { Text = "buttontext" });
		Assert.That(rendered.Find("button div div").GetAttribute("style"), Does.Not.Contain("height:inherit;"));
	}

	[Test]
	public async Task ButtonDisabled()
	{
		using var ctx = new WinzorTestContext();
		Button button = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => button = new Button { Text = "buttontext", Enabled = false });
		Assert.That(rendered.Find("button").GetAttribute("disabled"), Is.Empty);

		await button.InvokeWinzorDispatcherAsync(() => button.Enabled = true);
		Assert.That(rendered.Find("button").GetAttribute("disabled"), Is.Null);
	}

	[Test]
	public async Task ButtonDisabledStyle()
	{
		using var ctx = new WinzorTestContext();
		Button button = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => button = new Button { ForeColor = Color.Red, BackColor = Color.White, Text = "buttontext", Enabled = false });

		Assert.That(rendered.Find(".button").GetAttribute("class"), Does.Contain("button--standard--disabled"));
		Assert.That(rendered.Find(".button__text").GetAttribute("style"), Does.Contain("color:#555555FF;"));
	}

	[Test]
	public async Task ButtonTextCanChange()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new Button { Text = "buttontext" });
		var form = (Form)rendered.Instance.Control;
		var button = (Button)form.Controls[0];

		Assert.That(rendered.Find("button div div").InnerHtml, Is.EqualTo("buttontext"));

		await button.InvokeWinzorDispatcherAsync(() => button.Text = "buttonchanged");
		rendered.WaitForState(() => rendered.Find("button div div").InnerHtml == "buttonchanged");

		Assert.That(rendered.Find("button div div").InnerHtml, Is.EqualTo("buttonchanged"));
	}

	[Test]
	public async Task ButtonImageIsPng()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new Button { Text = "buttontext", Image = TestImage.GetImage() });

		Assert.That(rendered.Find(".button__image").GetAttribute("style"), Does.Contain("background-image: url(data:image/png;base64,"));
	}

	[Test]
	public async Task ButtonHasCorrectSizeAndDefaultPosition()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			return new Button()
			{
				Width = 100,
				Height = 50,
			};
		});

		var button = rendered.Find(".button");
		Assert.That(button.GetAttribute("style"), Is.EqualTo("position:absolute;width:100px;height:50px;top:0px;left:0px; position: absolute; top: 1px; left: 1px; width: 98px; height: 48px;"));
	}

	[Test]
	public async Task ButtonHasCorrectSizeAndPosition()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			return new Button()
			{
				Width = 100,
				Height = 50,
				Top = 200,
				Left = 300,
			};
		});

		var button = rendered.Find(".button");
		Assert.That(button.GetAttribute("style"), Is.EqualTo("position:absolute;width:100px;height:50px;top:200px;left:300px; position: absolute; top: 201px; left: 301px; width: 98px; height: 48px;"));
	}

	[TestCase(FlatStyle.Flat, "button--flat", TestName = "{m}_Flat")]
	[TestCase(FlatStyle.Popup, "button--popup", TestName = "{m}_Popup")]
	[TestCase(FlatStyle.Standard, "button--standard", TestName = "{m}_Standard")]
	[TestCase(FlatStyle.System, "button--standard", TestName = "{m}_System")]
	public async Task ButtonFlatStyleClassIsApplied(FlatStyle flatStyle, string expectedClass)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			return new Button()
			{
				Width = 100,
				Height = 50,
				FlatStyle = flatStyle,
			};
		});

		var button = rendered.Find(".button");

		Assert.That(button.ClassList, Does.Contain(expectedClass));
	}

	[TestCase(FlatStyle.Flat, "", TestName = "{m}_Flat")]
	[TestCase(FlatStyle.Popup, "", TestName = "{m}_Popup")]
	[TestCase(FlatStyle.Standard, "background-color:#FF0000FF;", TestName = "{m}_Standard")]
	[TestCase(FlatStyle.System, "", TestName = "{m}_System")]
	public async Task ButtonFlatStyleInnerDivStyleStringIsApplied(FlatStyle flatStyle, string expectedStyleString)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			return new Button()
			{
				Width = 100,
				Height = 50,
				FlatStyle = flatStyle,
				BackColor = Color.Red,
			};
		});

		var buttonInnerDiv = rendered.Find(".button > div");

		Assert.That(buttonInnerDiv.GetAttribute("style"), Does.Contain(expectedStyleString));
	}

	[TestCase(ImageLayout.Tile, "backgroundimage--tile", TestName = "{m}_Tile")]
	[TestCase(ImageLayout.Center, "backgroundimage--center", TestName = "{m}_Center")]
	[TestCase(ImageLayout.Stretch, "backgroundimage--stretch", TestName = "{m}_Stretch")]
	[TestCase(ImageLayout.Zoom, "backgroundimage--zoom", TestName = "{m}_Zoom")]
	public async Task ButtonBackgroundImageStyleClassIsApplied(ImageLayout layout, string expectedClass)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			return new Button()
			{
				Width = 100,
				Height = 50,
				BackgroundImage = TestImage.GetImage(),
				BackgroundImageLayout = layout,
			};
		});

		var buttonInnerDiv = rendered.Find(".button > div");

		Assert.That(buttonInnerDiv.ClassList, Does.Contain(expectedClass));
	}

	[Test]
	public async Task ButtonFlatAppearanceIsApplied()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var button = new Button() { Width = 100, Height = 50, FlatStyle = FlatStyle.Flat };
			button.BackColor = Color.Yellow;
			button.FlatAppearance.BorderColor = Color.FromArgb(255, 255, 0, 0);
			button.FlatAppearance.BorderSize = 5;
			button.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 0, 255, 0);
			button.FlatAppearance.MouseDownBackColor = Color.FromArgb(255, 0, 0, 255);
			return button;
		});

		var button = rendered.Find(".button");
		Assert.That(button.GetAttribute("style"), Does.Contain("border-color: #FF0000FF;"));
		Assert.That(button.GetAttribute("style"), Does.Contain("border-width: 5px;"));
		Assert.That(button.GetAttribute("style"), Does.Contain("--flat-button-hover-color: #00FF00FF;"));
		Assert.That(button.GetAttribute("style"), Does.Contain("--flat-button-active-color: #0000FFFF;"));

		var buttonInnerDiv = rendered.Find(".button > div");
		Assert.That(buttonInnerDiv.GetAttribute("style"), Does.Contain("background-color:#FFFF00FF;"));
	}

	[Test]
	public async Task ButtonFlatAppearanceMouseOverDefaultToColorOptionsCalculation()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var button = new Button() { Width = 100, Height = 50, FlatStyle = FlatStyle.Flat };
			button.BackColor = Color.Yellow;
			button.FlatAppearance.BorderColor = Color.FromArgb(255, 255, 0, 0);
			return button;
		});
		var button = rendered.Find(".button");
		Assert.That(button.GetAttribute("style"), Does.Contain("border-color: #FF0000FF;"));
		Assert.That(button.GetAttribute("style"), Does.Contain("border-width: 1px;"));

		// the color #E5E500FF was calculated by ColorOptions(ForeColor, BackColor, Enabled).Calculate() in button.razor
		Assert.That(button.GetAttribute("style"), Does.Contain("--flat-button-hover-color: #E5E500FF;"));

		var buttonInnerDiv = rendered.Find(".button > div");
		Assert.That(buttonInnerDiv.GetAttribute("style"), Does.Contain("background-color:#FFFF00FF;"));
	}

	[Test]
	public async Task ButtonFlatAppearanceMouseDownDefaultToBackColor()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var button = new Button() { Width = 100, Height = 50, FlatStyle = FlatStyle.Flat };
			button.BackColor = Color.Yellow;
			button.FlatAppearance.BorderColor = Color.FromArgb(255, 255, 0, 0);
			return button;
		});
		var button = rendered.Find(".button");
		Assert.That(button.GetAttribute("style"), Does.Contain("border-color: #FF0000FF;"));
		Assert.That(button.GetAttribute("style"), Does.Contain("border-width: 1px;"));
		Assert.That(button.GetAttribute("style"), Does.Contain("--flat-button-active-color: #FFFF00FF;"));

		var buttonInnerDiv = rendered.Find(".button > div");
		Assert.That(buttonInnerDiv.GetAttribute("style"), Does.Contain("background-color:#FFFF00FF;"));
	}

	[TestCase(FlatStyle.Popup, TestName = "{m}_Popup")]
	[TestCase(FlatStyle.Standard, TestName = "{m}_Standard")]
	[TestCase(FlatStyle.System, TestName = "{m}_System")]
	public async Task ButtonFlatAppearanceIsNotAppliedToNonFlatButton(FlatStyle flatStyle)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var button = new Button() { Width = 100, Height = 50, FlatStyle = flatStyle };
			button.BackColor = Color.Yellow;
			button.FlatAppearance.BorderColor = Color.FromArgb(255, 255, 0, 0);
			button.FlatAppearance.BorderSize = 5;
			button.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 0, 255, 0);
			button.FlatAppearance.MouseDownBackColor = Color.FromArgb(255, 0, 0, 255);
			return button;
		});

		var button = rendered.Find(".button");
		Assert.That(button.GetAttribute("style"), Does.Not.Contain("border-color: #FF0000FF;"));
		Assert.That(button.GetAttribute("style"), Does.Not.Contain("border-width: 5px;"));
		Assert.That(button.GetAttribute("style"), Does.Not.Contain("--flat-button-hover-color: #00FF00FF;"));
		Assert.That(button.GetAttribute("style"), Does.Not.Contain("--flat-button-active-color: #0000FFFF;"));

		var buttonInnerDiv = rendered.Find(".button > div");
		if (flatStyle == FlatStyle.System)
		{
			Assert.That(buttonInnerDiv.GetAttribute("style"), Does.Not.Contain("background-color"));
		}
		else
		{
			Assert.That(buttonInnerDiv.GetAttribute("style"), Does.Contain("background-color:#FFFF00FF;"));
		}
	}

	[Test]
	public async Task ButtonBackgroundImageStyleStringIsApplied()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			return new Button()
			{
				Width = 100,
				Height = 50,
				BackgroundImage = TestImage.GetImage(),
			};
		});

		var button = rendered.Find(".button");
		Assert.That(button.GetAttribute("style"), Is.EqualTo("position:absolute;width:100px;height:50px;top:0px;left:0px; position: absolute; top: 1px; left: 1px; width: 98px; height: 48px;"));
	}

	[Test]
	public async Task ButtonNoBackgroundImageStyleStringAndClassNotApplied()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			return new Button()
			{
				Width = 100,
				Height = 50,
				BackgroundImageLayout = ImageLayout.Tile,
			};
		});

		var buttonInnerDiv = rendered.Find(".button > div");

		Assert.That(buttonInnerDiv.GetAttribute("style"), Does.Not.Contain("background-image"));
		Assert.That(buttonInnerDiv.ClassList, Does.Not.Contain("backgroundimage--tile"));
	}

	[Test]
	public async Task ButtonShouldSetDialogResultOfModal()
	{
		using var ctx = new WinzorTestContext();
		Form form = null;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			form.Controls.Add(new Button() { DialogResult = DialogResult.OK });
			return form;
		});

		await rendered.Find("button").ClickAsync(new WebMouseEventArgs());

		Assert.That(form.DialogResult, Is.EqualTo(DialogResult.OK));
	}

	[Test]
	public async Task ButtonDoesNotCloseDialogFormIfResultIsNone()
	{
		using var ctx = new WinzorTestContext();
		Form form = null;
		var formClosed = new TaskCompletionSource<bool>();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			form.Closed += (sender, args) => formClosed.SetResult(true);
			var button = new Button() { DialogResult = DialogResult.OK };
			button.Click += (sender, args) =>
			{
				var form = button.FindForm();
				form.DialogResult = DialogResult.None;
			};
			form.Controls.Add(button);
			return form;
		});

		await rendered.Find("button").ClickAsync(new WebMouseEventArgs());

		Assert.That(await formClosed.Task.WithTimeout(TimeSpan.FromSeconds(5)), Is.False);
	}

	[Test]
	public async Task ButtonDoesNotClickWhenDisable()
	{
		using var ctx = new WinzorTestContext();
		var clicked = false;
		Button button = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			button = new Button() { Enabled = false };
			button.Click += (sender, args) => { clicked = true; };
			form.Controls.Add(button);
			return form;
		});

		await rendered.Find("button").ClickAsync(new WebMouseEventArgs());
		Assert.That(clicked, Is.False);

		await button.InvokeWinzorDispatcherAsync(() => button.Enabled = true);
		await rendered.Find("button").ClickAsync(new WebMouseEventArgs());
		Assert.That(clicked, Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task ButtonOverlayAppliesCorrectBackgroundColor()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var button = new Button()
			{
				Text = "Button Text",
				UseVisualStyleBackColor = false
			};
			form.Controls.Add(button);
			return form;
		});

		var buttonOverlay = await page.WaitForSelectorAsync(".button--overlay");
		Assert.That(async () => await buttonOverlay.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-color')"), Is.EqualTo("rgb(240, 240, 240)"));
	}

	[Test, WithPlaywrightPage]
	public async Task ButtonOverlayAppliesCorrectBackgroundColorUsingVisualStyle()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form()
			{
				BackColor = Color.White
			};
			var button = new Button()
			{
				Text = "Button Text",
				UseVisualStyleBackColor = true
			};
			form.Controls.Add(button);
			return form;
		});

		var buttonOverlay = await page.WaitForSelectorAsync(".button--overlay > div");
		// Seems like rgba 0000 is the default visual style
		Assert.That(async () => await buttonOverlay.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-color')"), Is.EqualTo("rgba(0, 0, 0, 0)"));
	}

	[Test, WithPlaywrightPage]
	public async Task ButtonHasCorrectBackgroundColor()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(new Button() { BackColor = Color.Pink });

			return form;
		});

		var button = await page.WaitForSelectorAsync(".button > div");
		Assert.That(async () => await button.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-color')"), Is.EqualTo("rgb(255, 192, 203)"));
	}

	[Explicit]
	[Test, WithPlaywrightPage(Headless = false)]
	public async Task ButtonDemo()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var button = new Button()
			{
				Width = 100,
				Height = 60,
				Text = "Demo Button",
				FlatStyle = FlatStyle.Flat,
			};
			button.BackColor = Color.Yellow;
			button.FlatAppearance.BorderSize = 2;
			button.FlatAppearance.BorderColor = Color.Red;
			button.FlatAppearance.MouseOverBackColor = Color.Green;
			button.FlatAppearance.MouseDownBackColor = Color.Blue;
			form.Controls.Add(button);
			return form;
		});

		var pageClosed = new TaskCompletionSource<bool>();
		page.Close += (sender, args) => pageClosed.SetResult(true);
		await pageClosed.Task.WithTimeout(TimeSpan.FromMinutes(5));
	}

	[TestCase(40, 40, "width: 32px; height: 32px;")]
	[TestCase(19, 19, "width: 32px; height: 32px")]
	public async Task ButtonImageWidthAndHeight(int width, int height, string expected)
	{
		using var ctx = new WinzorTestContext();
		var image = TestImage.GetImage();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			Button button = new Button()
			{
				Image = image,
				Width = width,
				Height = height
			};
			form.Controls.Add(button);
			return form;
		});
		Assert.That(rendered.Find("button div .button__image").GetAttribute("style"), Does.Contain(expected));
	}

	[Test]
	public async Task ButtonDoesNotRenderWhitespaceText()
	{
		using var ctx = new WinzorTestContext();
		var image = TestImage.GetImage();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			Button button = new Button()
			{
				Text = " "
			};
			form.Controls.Add(button);
			return form;
		});

		Assert.Throws<ElementNotFoundException>(() => rendered.Find("button div .button__text"));
	}

	[Test]
	public async Task ButtonUseMnemonicUpdatesFromServer()
	{
		await ControlAssert.ImplementsChangeFromServerAsync<Button>(c => c.UseMnemonic = false);
	}

	[Test]
	public async Task ButtonProcessMnemonicPerformsClick()
	{
		using var ctx = new WinzorTestContext();
		var clicked = false;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var button = new Button() { Text = "&Test" };
			button.Click += (sender, args) => clicked = true;
			return button;
		});

		Assert.That(clicked, Is.False);
		var button = rendered.Find("button");
		await rendered.KeyPressAsync(Keys.Alt | Keys.T, button);
		Assert.That(clicked, Is.True);

		clicked = false;

		Assert.That(clicked, Is.False);
		var form = rendered.Find(".form");
		await rendered.KeyPressAsync(Keys.Alt | Keys.T, form);
		Assert.That(clicked, Is.True);
	}

	[Test]
	public async Task ButtonUseMnemonicFormatsTestWithMnemonic()
	{
		using var ctx = new WinzorTestContext();
		Button button = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => button = new Button() { Text = "&Test", UseMnemonic = false });

		Assert.That(rendered.Find(".button__text").InnerHtml, Is.EqualTo("Test"));
		await button.InvokeWinzorDispatcherAsync(() => button.UseMnemonic = true);
		Assert.That(rendered.Find(".button__text").InnerHtml, Is.EqualTo("<span class=\"mnemonickey\">T</span>est"));
	}

	[TestCase(TextImageRelation.TextAboveImage, "button--textaboveimage", TestName = "{m}_TextAboveImage")]
	[TestCase(TextImageRelation.TextBeforeImage, "button--textbeforeimage", TestName = "{m}_TextBeforeImage")]
	[TestCase(TextImageRelation.Overlay, "button--overlay", TestName = "{m}_Overlay")]
	[TestCase(TextImageRelation.ImageAboveText, "button--imageabovetext", TestName = "{m}_ImageAboveText")]
	[TestCase(TextImageRelation.ImageBeforeText, "button--imagebeforetext", TestName = "{m}_ImageBeforeText")]
	public async Task ButtonTextImageRelationClassIsApplied(TextImageRelation position, string expectedClass)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			return new Button()
			{
				Width = 100,
				Height = 50,
				TextImageRelation = position,
			};
		});

		var button = rendered.Find(".button > div");

		Assert.That(button.ClassList, Does.Contain(expectedClass));
	}

	[TestCase(ContentAlignment.TopLeft, "top: 0px; left: 0px;")]
	[TestCase(ContentAlignment.TopCenter, "top: 0px; left: 17px;")]
	[TestCase(ContentAlignment.TopRight, "top: 0px; left: 35px;")]
	[TestCase(ContentAlignment.MiddleLeft, "top: -8px; left: 0px;")]
	[TestCase(ContentAlignment.MiddleCenter, "top: -8px; left: 17px;")]
	[TestCase(ContentAlignment.MiddleRight, "top: -8px; left: 35px;")]
	[TestCase(ContentAlignment.BottomLeft, "top: -17px; left: 0px;")]
	[TestCase(ContentAlignment.BottomCenter, "top: -17px; left: 17px;")]
	[TestCase(ContentAlignment.BottomRight, "top: -17px; left: 35px;")]
	public async Task ButtonImageAlignStyleIsApplied(ContentAlignment alignment, string expectedStyle)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			return new Button()
			{
				Image = TestImage.GetImage(),
				ImageAlign = alignment,
			};
		});

		var buttonImage = rendered.Find(".button__image");
		Assert.That(buttonImage.GetAttribute("style"), Does.Contain(expectedStyle));
	}

	[TestCase(TextImageRelation.TextAboveImage, "button--textaboveimage", "top: 0px; left: 31px;", "top: 13px; left: 30px;", TestName = "{m}_TextAboveImage")]
	[TestCase(TextImageRelation.TextBeforeImage, "button--textbeforeimage", "top: 14px; left: 7px;", "top: 5px; left: 51px;", TestName = "{m}_TextBeforeImage")]
	[TestCase(TextImageRelation.Overlay, "button--overlay", "top: 14px; left: 31px;", "top: 5px; left: 30px;", TestName = "{m}_Overlay")]
	[TestCase(TextImageRelation.ImageAboveText, "button--imageabovetext", "top: 29px; left: 31px;", "top: 0px; left: 30px;", TestName = "{m}_ImageAboveText")]
	[TestCase(TextImageRelation.ImageBeforeText, "button--imagebeforetext", "top: 14px; left: 39px;", "top: 5px; left: 8px;", TestName = "{m}_ImageBeforeText")]
	public async Task ButtonTextImageRelationStyleIsApplied(TextImageRelation position, string expectedClass, string expectedTextStyle, string expectedImageStyle)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			return new Button()
			{
				Width = 100,
				Height = 50,
				TextImageRelation = position,
				Text = "Test",
				Image = TestImage.GetImage(),
			};
		});

		var buttonImage = rendered.Find($".{expectedClass} > .button__image");
		var buttonText = rendered.Find($".{expectedClass} > .button__text");

		Assert.That(buttonImage.GetAttribute("style"), Does.Contain(expectedImageStyle));
		Assert.That(buttonText.GetAttribute("style"), Does.Contain(expectedTextStyle));
	}

	[Test, WithPlaywrightPage]
	public async Task ButtonTextSupportLineBreak()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var button = new Button()
			{
				Text = "Button Text",
				Image = TestImage.GetImage(),
				TextImageRelation = TextImageRelation.Overlay,
			};
			form.Controls.Add(button);
			return form;
		});

		var buttonText = await page.WaitForSelectorAsync($".button__text");

		Assert.That(async () => await buttonText.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('white-space')"), Is.EqualTo("pre-wrap"));
	}

	[Test, WithPlaywrightPage]
	[TestCase("Enough space for one line no clip", 200, 30, "none")]
	[TestCase("Not enough space for two lines will trigger clip", 200, 30, "13px")]
	[TestCase("Enough space for two line no clip", 120, 45, "none")]
	[TestCase("Not enough space for two lines will trigger clip", 120, 45, "26px")]
	public async Task ButtonTextShouldNotDisplayClippedLines(string text, int buttonWidth, int buttonHeight, string textMaxHeight)
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var button = new Button()
			{
				Text = text,
				Width = buttonWidth,
				Height = buttonHeight
			};
			form.Controls.Add(button);
			return form;
		});

		var buttonText = await page.WaitForSelectorAsync($".button__text");
		Assert.That(async () => await buttonText.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('max-height')"), Is.EqualTo(textMaxHeight));
	}

	[TestCase(ContentAlignment.TopLeft, "top: 0px; left: -1px;")]
	[TestCase(ContentAlignment.TopCenter, "top: 0px; left: 1px;")]
	[TestCase(ContentAlignment.TopRight, "top: 0px; left: 4px;")]
	[TestCase(ContentAlignment.MiddleLeft, "top: 1px; left: -1px;")]
	[TestCase(ContentAlignment.MiddleCenter, "top: 1px; left: 1px;")]
	[TestCase(ContentAlignment.MiddleRight, "top: 1px; left: 4px;")]
	[TestCase(ContentAlignment.BottomLeft, "top: 2px; left: -1px;")]
	[TestCase(ContentAlignment.BottomCenter, "top: 2px; left: 1px;")]
	[TestCase(ContentAlignment.BottomRight, "top: 2px; left: 4px;")]
	public async Task ButtonTextAlignStyleIsApplied(ContentAlignment alignment, string expectedStyle)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			return new Button()
			{
				Text = "Button Text",
				TextAlign = alignment,
			};
		});

		var buttonText = rendered.Find(".button__text");
		Assert.That(buttonText.GetAttribute("style"), Does.Contain(expectedStyle));
	}

	[Test, WithPlaywrightPage]
	[TestCase(FlatStyle.System)]
	[TestCase(FlatStyle.Flat)]
	[TestCase(FlatStyle.Popup)]
	[TestCase(FlatStyle.Standard)]
	public async Task ButtonTextAndImagePositionAbsolute(FlatStyle flatStyle)
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var button = new Button()
			{
				Text = "Standard Button",
				FlatStyle = flatStyle,
				Image = TestImage.GetImage(),
				Height = 20,
				Width = 100
			};
			form.Controls.Add(button);
			return form;
		});

		var innerDiv = await page.WaitForSelectorAsync("button > div");
		var buttonImage = await page.WaitForSelectorAsync(".button__image");
		var buttonText = await page.WaitForSelectorAsync(".button__text");

		Assert.That(async () => await buttonImage.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('position')"), Is.EqualTo("absolute"));
		Assert.That(async () => await buttonText.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('position')"), Is.EqualTo("absolute"));
		Assert.That(async () => await innerDiv.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('position')"), Is.EqualTo("absolute"));
	}

	[Test, WithPlaywrightPage]
	public async Task StandardButtonStyleIsApplied()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var button = new Button()
			{
				Text = "Standard Button",
				FlatStyle = FlatStyle.Standard,
				Height = 50,
				Width = 40
			};
			form.Controls.Add(button);
			return form;
		});

		var button = await page.WaitForSelectorAsync($".button--standard");

		Assert.That(async () => await button.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('height')"), Is.EqualTo("48px"));
		Assert.That(async () => await button.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("38px"));
	}

	[Test, WithPlaywrightPage]
	public async Task ButtonImageShouldChangeGrayOutOnDisable()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var button = new Button()
			{
				Text = "Attach",
				Enabled = false,
				Image = TestImage.GetImage(),
			};
			form.Controls.Add(button);
			return form;
		});

		var buttonDisableImage = await page.WaitForSelectorAsync(".button:disabled .button__image");
		Assert.That(async () => await buttonDisableImage.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('opacity')"), Is.EqualTo("0.3"));
		Assert.That(async () => await buttonDisableImage.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('filter')"), Is.EqualTo("grayscale(1)"));
	}

	[Test, WithPlaywrightPage]
	public async Task ButtonImagePaddingStyleIsApplied()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var button = new Button()
			{
				Image = TestImage.GetImage(),
				Padding = new Padding(5, 0, 6, 6),
				Width = 100,
				Height = 100,
			};
			form.Controls.Add(button);
			return form;
		});

		var innerDiv = await page.WaitForSelectorAsync(".button--overlay");
		var buttonImage = await page.WaitForSelectorAsync(".button__image");

		Assert.That(async () => await innerDiv.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('padding')"), Is.EqualTo("0px"));
		Assert.That(async () => await innerDiv.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('top')"), Is.EqualTo("2px"));
		Assert.That(async () => await innerDiv.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('left')"), Is.EqualTo("2px"));
		Assert.That(async () => await innerDiv.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('right')"), Is.EqualTo("2px"));
		Assert.That(async () => await innerDiv.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('bottom')"), Is.EqualTo("2px"));
		Assert.That(async () => await buttonImage.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('padding')"), Is.EqualTo("0px"));
		Assert.That(async () => await buttonImage.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('top')"), Is.EqualTo("27px"));
		Assert.That(async () => await buttonImage.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('left')"), Is.EqualTo("29px"));
	}

	[Test]
	public async Task ButtonImageShouldNotBeDraggable()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new Button { Image = TestImage.GetImage() });
		Assert.That(rendered.Find(".button__image").GetAttribute("draggable"), Is.EqualTo("false"));
	}

	[Test, WithPlaywrightPage]
	public async Task HomeButtonShouldHaveHoverBackColor()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var button = new Button()
			{
				FlatStyle = FlatStyle.Flat,
				Width = 100,
				Height = 50,
				Text = "Home Button onHover",
				Image = TestImage.GetImage(),
				TextImageRelation = TextImageRelation.ImageAboveText,
			};
			button.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 0, 255, 0);
			form.Controls.Add(button);
			return form;
		});

		var button = await page.WaitForSelectorAsync(".button > div.button--imageabovetext");

		await button.HoverAsync();
		Assert.That(async () => await button.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-color')"), Is.EqualTo("rgb(0, 255, 0)"));
	}

	[Test, WithPlaywrightPage]
	public async Task ButtonBackgroundImageDoesNotOverflow()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var button = new Button()
			{
				Image = TestImage.GetImage(),
				Padding = new Padding(5, 0, 6, 6),
			};
			form.Controls.Add(button);
			return form;
		});

		var buttonImage = await page.WaitForSelectorAsync(".button--overlay");
		Assert.That(async () => await buttonImage.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('overflow')"), Is.EqualTo("hidden"));
	}

	[Test, WithPlaywrightPage]
	public async Task ButtonDisabledHasCorrectPadding()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var button = new Button()
			{
				Enabled = false
			};
			form.Controls.Add(button);
			return form;
		});

		var buttonImage = await page.WaitForSelectorAsync($".button--standard--disabled");
		Assert.That(async () => await buttonImage.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('padding')"), Is.EqualTo("2px"));
	}

	[Test, WithPlaywrightPage]
	public async Task ButtonClickViaMouseTriggersMouseUpAfterClick()
	{
		await using var ctx = new InMemoryTestServerContext();
		var clickTask = new TaskCompletionSource();
		var clickFirst = false;
		var clickTriggered = false;
		var mouseUpTask = new TaskCompletionSource();
		var mouseUpX = 0;
		var mouseUpY = 0;
		var mouseUpTriggered = false;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();

			var button = new Button
			{
				Text = "Please click me",
				AutoSize = true
			};

			button.Click += (s, e) =>
			{
				clickFirst = !mouseUpTask.Task.IsCompletedSuccessfully;
				clickTriggered = true;
				clickTask.SetResult();
			};
			button.MouseUp += (s, e) =>
			{
				mouseUpX = e.X;
				mouseUpY = e.Y;
				mouseUpTriggered = true;
				mouseUpTask.SetResult();
			};

			form.Controls.Add(button);
			return form;
		});
		await page.WaitForSelectorAsync(".form > .button");
		const int insideButtonX = 20;
		const int insideButtonY = 20;
		await page.Mouse.ClickAsync(insideButtonX, insideButtonY);

		await clickTask.Task;
		await mouseUpTask.Task;

		Assert.That(clickFirst, Is.True);
		Assert.That(clickTriggered, Is.True);
		Assert.That(mouseUpX, Is.EqualTo(insideButtonX));
		Assert.That(mouseUpY, Is.EqualTo(insideButtonY));
		Assert.That(mouseUpTriggered, Is.True);
	}

	[SuppressMessage("CargoWiseOne", "CW1104:DoNotUseSystemWindowsTabControl", Justification = "Testing")]
	[Test, WithPlaywrightPage]
	public async Task ButtonLostFocusWhenSwitchingTabs()
	{
		await using var ctx = new InMemoryTestServerContext();

		var button1GotFocused = new TaskCompletionSource();
		var button1LostFocus = false;
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var tabControl = new TabControl();
			var tabpage1 = new TabPage { Text = "Tab1" };
			var tabpage2 = new TabPage { Text = "Tab2" };

			var button1 = new Button { Text = "button" };
			button1.Size = new Size(100, 20);
			button1.GotFocus += (s, e) => button1GotFocused.SetResult();
			button1.LostFocus += (s, e) => { button1LostFocus = true; };

			tabpage1.Controls.Add(button1);
			tabControl.TabPages.Add(tabpage1);
			tabControl.TabPages.Add(tabpage2);

			return tabControl;
		});

		await page.BringToFrontAsync();

		await ElementMouseMoveWithDelayAsync(page, page.GetByText("Tab1"), 5, 5);
		await MouseDownWithDelayAsync(page);
		await MouseUpWithDelayAsync(page);

		await ElementMouseMoveWithDelayAsync(page, page.GetByText("button"), 5, 5);
		await MouseDownWithDelayAsync(page);
		await MouseUpWithDelayAsync(page);
		Assert.That(() => button1LostFocus, Is.False.After(2000, 100));

		await ElementMouseMoveWithDelayAsync(page, page.GetByText("Tab2"), 5, 5);
		await MouseDownWithDelayAsync(page);
		await MouseUpWithDelayAsync(page);
		Assert.That(() => button1LostFocus, Is.True.After(2000, 100));
	}

	[WithPlaywrightPage()]
	[TestCase("Space")]
	[TestCase("Enter")]
	public async Task ButtonClickViaKeyboardNoMouseUp(string key)
	{
		await using var ctx = new InMemoryTestServerContext();
		var form = default(Form);
		var clickTask = new TaskCompletionSource();
		var mouseUpTask = new TaskCompletionSource();
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();

			var button = new Button
			{
				Text = "Please click me",
				AutoSize = true
			};

			button.Click += (s, e) => clickTask.TrySetResult(); // Fires twice as there's an additional enter key event handler
			button.MouseUp += (s, e) => mouseUpTask.SetResult();

			form.Controls.Add(button);
			return form;
		});

		var button = page.Locator("button", new PageLocatorOptions { HasText = "Please click me" });
		await button.FocusAsync();
		await button.PressAsync(key);

		var clicked = await clickTask.Task.WithTimeout(TimeSpan.FromSeconds(1));
		var mouseUp = await mouseUpTask.Task.WithTimeout(TimeSpan.FromSeconds(1));

		Assert.That(clicked, Is.True);
		Assert.That(mouseUp, Is.False);
	}

	[Test, WithPlaywrightPage]
	public async Task ButtonClickViaJavaScriptNoMouseUp()
	{
		await using var ctx = new InMemoryTestServerContext();
		var form = default(Form);

		var clickTask = new TaskCompletionSource();
		var mouseUpTask = new TaskCompletionSource();
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();

			var button = new Button
			{
				Text = "Please click me",
				AutoSize = true
			};

			button.Click += (s, e) => clickTask.SetResult();
			button.MouseUp += (s, e) => mouseUpTask.SetResult();

			form.Controls.Add(button);
			return form;
		});

		var button = page.Locator("button", new PageLocatorOptions { HasText = "Please click me" });
		await button.EvaluateAsync("button => button.click();");

		var clicked = await clickTask.Task.WithTimeout(TimeSpan.FromSeconds(1));
		var mouseUp = await mouseUpTask.Task.WithTimeout(TimeSpan.FromSeconds(1));

		Assert.That(clicked, Is.True);
		Assert.That(mouseUp, Is.False);
	}

	[Test, WithPlaywrightPage]
	public async Task ButtonClickViaCodeNoMouseUp()
	{
		await using var ctx = new InMemoryTestServerContext();
		var form = default(Form);
		var clickTask = new TaskCompletionSource();
		var mouseUpTask = new TaskCompletionSource();
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();

			var button = new Button
			{
				Text = "Please click me",
				AutoSize = true
			};

			button.Click += (s, e) => clickTask.SetResult();
			button.MouseUp += (s, e) => mouseUpTask.SetResult();

			form.Controls.Add(button);
			return form;
		});

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var button = form.Controls[0] as Button;
			button.PerformClick();
		});

		var clicked = await clickTask.Task.WithTimeout(TimeSpan.FromSeconds(1));
		var mouseUp = await mouseUpTask.Task.WithTimeout(TimeSpan.FromSeconds(1));

		Assert.That(clicked, Is.True);
		Assert.That(mouseUp, Is.False);
	}

	[Test]
	public async Task PerformClickShouldInvokeClickEventOnlyWhenEnabledAndVisible()
	{
		using var ctx = new WinzorTestContext();
		var clickFired = false;
		var button = default(Button);
		await ctx.RenderControlOnFormAsync(() =>
		{
			button = new Button() { Enabled = false, Visible = false };
			button.Click += (_, _) => clickFired = true;
			return button;
		});

		await button.InvokeWinzorDispatcherAsync(() => button.PerformClick());
		Assert.That(clickFired, Is.False);

		clickFired = false;
		await button.InvokeWinzorDispatcherAsync(() => button.Enabled = true);
		await button.InvokeWinzorDispatcherAsync(() => button.PerformClick());
		Assert.That(clickFired, Is.False);

		clickFired = false;
		await button.InvokeWinzorDispatcherAsync(() => button.Visible = true);
		await button.InvokeWinzorDispatcherAsync(() => button.PerformClick());
		Assert.That(clickFired, Is.True);
	}

	[Test]
	public async Task ButtonBackgroundColorStyleStringIsApplied()
	{
		using var ctx = new WinzorTestContext();
		Form form = null;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form()
			{
				BackColor = Color.DarkBlue,
			};
			var btn = new Button()
			{
				Width = 100,
				Height = 50,
				UseVisualStyleBackColor = true,
				BackColor = Color.DarkGray,
				FlatStyle = FlatStyle.Flat,
			};
			form.Controls.Add(btn);
			return form;
		});

		var button = rendered.Find(".button");
		Assert.That(button.GetAttribute("style"), Does.Contain("background-color:#A9A9A9FF"));
	}

	[SuppressMessage("CargoWiseOne", "CW1104:DoNotUseSystemWindowsTabControl", Justification = "Testing")]
	[Test, WithPlaywrightPage]
	public async Task ButtonLostFocusHasCorrectSytle()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var tabControl = new TabControl();
			var tabpage = new TabPage() { Text = "Tab" };
			var button = new Button() { Text = "button" };
			button.Size = new Size(100, 20);

			tabpage.Controls.Add(button);
			tabControl.TabPages.Add(tabpage);
			form.Controls.Add(tabControl);

			return form;
		});

		var button = await page.WaitForSelectorAsync(".form .button");
		await button.ClickAsync();
		Assert.That(async () => await button.GetAttributeAsync("class"), Does.Contain("button--default").After(2000, 100));

		await button.PressAsync("Tab");
		Assert.That(async () => await button.GetAttributeAsync("class"), Does.Not.Contain("button--default").After(2000,100));
	}

	[Test, WithPlaywrightPage]
	public async Task MouseDownPreventDefaultFlagWorks([Values]bool preventDefaultMouseDown)
	{
		await using var ctx = new InMemoryTestServerContext();
		TextBox textBox = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var button = new Button
			{
				PreventDefaultMouseDown = preventDefaultMouseDown,
				Size = new Size(100, 100),
				Location = new Point(0, 0)
			};
			textBox = new TextBox()
			{
				Size = new Size(100, 100),
				Location = new Point(100, 0)
			};
			form.Controls.Add(button);
			form.Controls.Add(textBox);

			return form;
		});

		var input = await page.WaitForSelectorAsync("input");
		await input.FocusAsync();

		Assert.That(() => textBox.Focused, Is.True.After(1000, 10));

		var button = await page.WaitForSelectorAsync("button");
		await button.ClickAsync();

		Assert.That(() => textBox.Focused, Is.EqualTo(preventDefaultMouseDown).After(1000));
	}

	[Test, WithPlaywrightPage]
	public async Task WhenFontIsSymbolAndTextAlignIsMiddleCenter_TextIsVerticallyCentered()
	{
		await using var ctx = new InMemoryTestServerContext();
		Button button = null;

		var page = await ctx.LoadControlOnFormAsync(() => button = new Button()
		{
			Font = new Font("Segoe UI Symbol", 11F),
			TextAlign = ContentAlignment.MiddleCenter,
			Text = "🔎"
		});

		var expectedButton = await page.WaitForSelectorAsync($"[data-winzor-control-id=\"{button.WinzorControlId}\"]");
		Assert.That(expectedButton, Is.Not.Null);

		var expectedButtonTextDiv = await page.WaitForSelectorAsync(".button__text");
		Assert.That(await expectedButtonTextDiv.GetComputedStyleAsync("align-items"), Is.EqualTo("self-end"));
		Assert.That(await expectedButtonTextDiv.GetComputedStyleAsync("display"), Is.EqualTo("flex"));
		Assert.That(await expectedButtonTextDiv.GetComputedStyleAsync("justify-content"), Is.EqualTo("end"));
	}

	[Test, WithPlaywrightPage]
	[TestCase("Segoe UI Symbol", ContentAlignment.TopCenter)]
	[TestCase("Tahoma", ContentAlignment.MiddleCenter)]
	public async Task WhenFontIsNotSymbolOrTextAlignIsNotMiddleCenter_TextIsNotVerticallyCentered(string fontFamilyName, ContentAlignment textAlign)
	{
		await using var ctx = new InMemoryTestServerContext();
		Button button = null;

		var page = await ctx.LoadControlOnFormAsync(() => button = new Button()
		{
			Font = new Font(fontFamilyName, 11F),
			TextAlign = textAlign,
			Text = "x"
		});

		var expectedButton = await page.WaitForSelectorAsync($"[data-winzor-control-id=\"{button.WinzorControlId}\"]");
		Assert.That(expectedButton, Is.Not.Null);

		var expectedButtonTextDiv = await page.WaitForSelectorAsync(".button__text");
		Assert.That(await expectedButtonTextDiv.GetComputedStyleAsync("align-items"), Is.Not.EqualTo("self-end"));
		Assert.That(await expectedButtonTextDiv.GetComputedStyleAsync("display"), Is.Not.EqualTo("flex"));
		Assert.That(await expectedButtonTextDiv.GetComputedStyleAsync("justify-content"), Is.Not.EqualTo("end"));
	}

	async Task MouseDownWithDelayAsync(IPage page, MouseDownOptions options = default, int delay = 350)
	{
		await page.Mouse.DownAsync(options);
		await Task.Delay(TimeSpan.FromMilliseconds(delay));
	}

	async Task ElementMouseMoveWithDelayAsync(IPage page, ILocator locator, int x, int y, int steps = 10, int delay = 350)
	{
		var elementRect = await locator.BoundingBoxAsync();
		await page.Mouse.MoveAsync(elementRect.X + x, elementRect.Y + y, new () { Steps = steps });
		await Task.Delay(TimeSpan.FromMilliseconds(delay));
	}

	async Task MouseUpWithDelayAsync(IPage page, MouseUpOptions options = default, int delay = 350)
	{
		await page.Mouse.UpAsync(options);
		await Task.Delay(TimeSpan.FromMilliseconds(delay));
	}
}
