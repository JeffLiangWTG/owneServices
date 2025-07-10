using System.Drawing;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Bunit;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorFramework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;

using static PlaywrightTestContext;
internal sealed class RadioButtonTest
{
	static readonly PageWaitForSelectorOptions playWrightPageWaitForSelectorOptions = new PageWaitForSelectorOptions { Timeout = 8000 };

	[Test]
	public async Task RadioButtonChangesCheckedOnClick()
	{
		using var ctx = new WinzorTestContext();
		RadioButton radioButton = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			radioButton = new RadioButton() { Text = "&Test" };
			return radioButton;
		});

		Assert.That(radioButton.Checked, Is.False);
		var radiobuttonElement = rendered.Find(".radiobutton");
		await rendered.KeyPressAsync(Keys.Alt | Keys.T, radiobuttonElement);
		Assert.That(radioButton.Checked, Is.True);
	}

	[Test]
	public async Task RadioButtonDoesNotChangeCheckedOnClickIfDisabled()
	{
		using var ctx = new WinzorTestContext();
		RadioButton radioButton = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			radioButton = new RadioButton() { Text = "&Test", Enabled = false };
			return radioButton;
		});

		Assert.That(radioButton.Checked, Is.False);
		await rendered.FindAll(".radiobutton")[0].ClickAsync(new WebMouseEventArgs());
		Assert.That(radioButton.Checked, Is.False);
	}

	[Test]
	public async Task RadioButtonEnterEvent()
	{
		await ControlAssert.ImplementsEventAsync<RadioButton, EventHandler>(nameof(TextBox.Enter), a => new EventHandler((s, e) => a()), "input", e => e.TriggerEvent("onwinzorfocusin", new WinzorFocusInEventArgs()));
	}

	[Test]
	public async Task RadioButtonChangedFromServer()
	{
		await ControlAssert.ImplementsChangeFromServerAsync<RadioButton>(c => c.Checked = true);
	}

	[Test]
	public async Task RadioButtonDisabled()
	{
		using var ctx = new WinzorTestContext();
		RadioButton radioButton = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => radioButton = new RadioButton { Enabled = false });
		Assert.That(rendered.Find(".radiobutton input[type=radio]").GetAttribute("disabled"), Is.Empty);

		await radioButton.InvokeWinzorDispatcherAsync(() => radioButton.Enabled = true);
		Assert.That(rendered.Find(".radiobutton input[type=radio]").GetAttribute("disabled"), Is.Null);
	}

	[Test]
	public async Task RadioButtonUseMnemonicUpdatesFromServer()
	{
		await ControlAssert.ImplementsChangeFromServerAsync<RadioButton>(c => c.UseMnemonic = false);
	}

	[Test]
	public async Task RadioButtonProcessMnemonicPerformsClick()
	{
		using var ctx = new WinzorTestContext();
		var clicked = false;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var radioButton = new RadioButton() { Text = "&Test" };
			radioButton.Click += (sender, args) => clicked = true;
			return radioButton;
		});

		Assert.That(clicked, Is.False);
		var radioButton = rendered.Find(".radiobutton");
		await rendered.KeyPressAsync(Keys.Alt | Keys.T, radioButton);
		Assert.That(clicked, Is.True);

		clicked = false;

		Assert.That(clicked, Is.False);
		var form = rendered.Find(".form");
		await rendered.KeyPressAsync(Keys.Alt | Keys.T, form);
		Assert.That(clicked, Is.True);
	}

	[Test]
	public async Task RadioButtonUseMnemonicFormatsTestWithMnemonic()
	{
		using var ctx = new WinzorTestContext();
		RadioButton radioButton = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => radioButton = new RadioButton() { Text = "&Test", UseMnemonic = false });

		Assert.That(rendered.Find(".radiobutton__text").InnerHtml, Is.EqualTo("Test"));
		await radioButton.InvokeWinzorDispatcherAsync(() => radioButton.UseMnemonic = true);
		Assert.That(rendered.Find(".radiobutton__text").InnerHtml, Is.EqualTo("<span class=\"mnemonickey\">T</span>est"));
	}

	[Test]
	public async Task RadioButtonHasInputTypeRadioButton()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new RadioButton());

		var radiobutton = rendered.Find("input[type=radio]");

		Assert.That(radiobutton, Is.Not.Null);
	}

	[Test]
	public async Task RadioButtonTextWidth()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new RadioButton());

		var radiobutton = rendered.Find(".radiobutton");

		Assert.That(radiobutton.GetAttribute("style"), Does.Contain("width:104px;"));
	}

	[Test]
	public async Task RadioButtonDefaultSize()
	{
		using var ctx = new WinzorTestContext();
		RadioButton radioButton = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => radioButton = new RadioButton());

		Assert.That(radioButton.Size, Is.EqualTo(new Size(104, 24)));
	}

	[Test]
	public async Task RadioButtonToString()
	{
		using var ctx = new WinzorTestContext();
		RadioButton radioButton = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => radioButton = new RadioButton());

		Assert.That(radioButton.ToString(), Is.EqualTo("System.Windows.Forms.RadioButton, Checked: False"));
	}

	[Test, WithPlaywrightPage]
	public async Task RadioButtonTextShouldNotWrap()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var button = new RadioButton() { AutoSize = true, Text = "This is a long text. This is a long text. This is a long text. This is a long text." };
			form.Controls.Add(button);
			return form;
		});

		var text = await page.WaitForSelectorAsync(".radiobutton .radiobutton__text");
		Assert.That(async () => await text.EvaluateAsync<int>("e => e.scrollHeight"), Is.EqualTo(13));
	}

	[Test, WithPlaywrightPage]
	public async Task RadioButtonNormalSize()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var button = new RadioButton()
			{
				Text = "AM/FM"
			};
			form.Controls.Add(button);
			return form;
		});

		var buttonOverlay = await page.WaitForSelectorAsync(".radiobutton input");
		Assert.That(async () => await buttonOverlay.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("11px"));
		Assert.That(async () => await buttonOverlay.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('height')"), Is.EqualTo("11px"));
	}

	[Test]
	public async Task RadioButtonsHaveCorrectSizeAndPositionSet()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var radioButton1 = new RadioButton() { AutoSize = true, Text = "No Temp. Requirement", Top = 5, Left = 10 };
			var radioButton2 = new RadioButton() { AutoSize = true, Text = "Requires Refrigeration", Top = 40, Left = 10 };
			var radioButton3 = new RadioButton() { AutoSize = true, Text = "Requires Freezing" , Top = 70, Left = 10 };

			var panel = new Panel() { Dock = DockStyle.Fill };
			panel.Controls.Add(radioButton1);
			panel.Controls.Add(radioButton2);
			panel.Controls.Add(radioButton3);

			return panel;
		});

		var items = rendered.FindAll(".radiobutton");
		AssertItemPosition(items[0], 134, 17, 10, 5, "absolute");
		AssertItemPosition(items[1], 129, 17, 10, 40, "absolute");
		AssertItemPosition(items[2], 108, 17, 10, 70, "absolute");
		var inputs = rendered.FindAll(".radiobutton input[type=radio]");
		AssertItemPosition(inputs[0], 11, 11, 0, 2, "absolute", "1");
		AssertItemPosition(inputs[1], 11, 11, 0, 2, "absolute", "1");
		AssertItemPosition(inputs[2], 11, 11, 0, 2, "absolute", "1");
		var texts = rendered.FindAll(".radiobutton .radiobutton__text");
		AssertItemPosition(texts[0], 116, 13, 16, 2, "absolute");
		AssertItemPosition(texts[1], 111, 13, 16, 2, "absolute");
		AssertItemPosition(texts[2], 90, 13, 16, 2, "absolute");
	}

	[Test]
	public async Task TabIndexChangedFromServer()
	{
		await ControlAssert.ImplementsChangeFromServerAsync<RadioButton>(c => c.TabIndex = 11);
	}

	[TestCase(Appearance.Normal, "radiobutton--normal")]
	[TestCase(Appearance.Button, "radiobutton--button")]
	public async Task RadioButtonAppearenceClass(Appearance appearance, string appearanceClass)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new RadioButton() { Appearance = appearance });
		var radiobutton = rendered.Find(".radiobutton");
		Assert.That(radiobutton.GetAttribute("class"), Does.Contain(appearanceClass));
	}

	[Test]
	public async Task RadioButtonAppearenceRadioSize()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new RadioButton() { Appearance = Appearance.Button });
		var radio = rendered.Find(".radiobutton input[type=radio]");
		AssertItemPosition(radio, 0, 0, 0, 0, "absolute");
	}

	[TestCase(FlatStyle.Standard, "radiobutton--standard")]
	[TestCase(FlatStyle.System, "radiobutton--system")]
	[TestCase(FlatStyle.Popup, "radiobutton--popup")]
	[TestCase(FlatStyle.Flat, "radiobutton--flat")]
	public async Task RadioButtonWithButtonAppearanceFlatStyleClass(FlatStyle flatStyle, string flatStyleClass)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new RadioButton() { Appearance = Appearance.Button, FlatStyle = flatStyle });
		var radiobutton = rendered.Find(".radiobutton");
		Assert.That(radiobutton.GetAttribute("class"), Does.Contain(flatStyleClass));
	}

	[TestCase(FlatStyle.Standard, "radiobutton--standard--disabled")]
	[TestCase(FlatStyle.System, "radiobutton--system--disabled")]
	[TestCase(FlatStyle.Popup, "radiobutton--popup--disabled")]
	[TestCase(FlatStyle.Flat, "radiobutton--flat--disabled")]
	public async Task RadioButtonWithButtonAppearanceFlatStyleClassWhenDisabled(FlatStyle flatStyle, string disabledFlatStyleClass)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new RadioButton() { Appearance = Appearance.Button, FlatStyle = flatStyle, Enabled = false });
		var radiobutton = rendered.Find(".radiobutton");
		Assert.That(radiobutton.GetAttribute("class"), Does.Contain(disabledFlatStyleClass));
	}

	[TestCase(FlatStyle.Standard, "radiobutton--standard--checked")]
	[TestCase(FlatStyle.System, "radiobutton--system--checked")]
	[TestCase(FlatStyle.Popup, "radiobutton--popup--checked")]
	[TestCase(FlatStyle.Flat, "radiobutton--flat--checked")]
	public async Task RadioButtonWithButtonAppearanceFlatStyleClassWhenChecked(FlatStyle flatStyle, string checkedFlatStyleClass)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new RadioButton() { Appearance = Appearance.Button, FlatStyle = flatStyle, Checked = true });
		var radiobutton = rendered.Find(".radiobutton");
		Assert.That(radiobutton.GetAttribute("class"), Does.Contain(checkedFlatStyleClass));
	}

	[Test]
	public async Task RadioButtonPerformAutoUpdates()
	{
		RadioButton radioButton1 = null;
		RadioButton radioButton2 = null;
		RadioButton radioButton3 = null;

		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			radioButton1 = new RadioButton() { Dock = DockStyle.Top };
			radioButton2 = new RadioButton() { Dock = DockStyle.Top };
			radioButton3 = new RadioButton() { Dock = DockStyle.Top };

			var panel = new Panel() { Dock = DockStyle.Fill };
			panel.Controls.Add(radioButton1);
			panel.Controls.Add(radioButton2);
			panel.Controls.Add(radioButton3);

			return panel;
		});

		Assert.That(radioButton1.Checked, Is.False);
		Assert.That(radioButton2.Checked, Is.False);
		Assert.That(radioButton3.Checked, Is.False);

		await rendered.FindAll(".radiobutton")[0].ClickAsync(new WebMouseEventArgs());
		Assert.That(radioButton1.Checked, Is.True);
		Assert.That(radioButton2.Checked, Is.False);
		Assert.That(radioButton3.Checked, Is.False);

		await rendered.FindAll(".radiobutton")[1].ClickAsync(new WebMouseEventArgs());
		Assert.That(radioButton1.Checked, Is.False);
		Assert.That(radioButton2.Checked, Is.True);
		Assert.That(radioButton3.Checked, Is.False);

		await rendered.FindAll(".radiobutton")[2].ClickAsync(new WebMouseEventArgs());
		Assert.That(radioButton1.Checked, Is.False);
		Assert.That(radioButton2.Checked, Is.False);
		Assert.That(radioButton3.Checked, Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task RadioButtonFocusedWhenClicked()
	{
		await using var ctx = new InMemoryTestServerContext();
		RadioButton radioButton1 = null;
		RadioButton radioButton2 = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			radioButton1 = new RadioButton() { Dock = DockStyle.Right, Text = "Radio 1" };
			radioButton2 = new RadioButton() { Dock = DockStyle.Right, Text = "Radio 2" };
			var form = new Form();
			form.Controls.Add(radioButton1);
			form.Controls.Add(radioButton2);

			Assert.That(radioButton1.Focused, Is.False);
			Assert.That(radioButton2.Focused, Is.False);
			return form;
		});
		await page.WaitForSelectorAsync(".radiobutton + .radiobutton");
		await page.ClickAsync(".radiobutton:first-child input");
		Assert.That(() => radioButton1.Focused, Is.True.After(3000, 100));
		Assert.That(() => radioButton2.Focused, Is.False.After(3000, 100));

		await page.ClickAsync(".radiobutton + .radiobutton input");
		Assert.That(() => radioButton1.Focused, Is.False.After(3000, 100));
		Assert.That(() => radioButton2.Focused, Is.True.After(3000, 100));

		// Sanity check
		Assert.That(async () => await page.EvaluateAsync<int>("window.document.getElementsByClassName('radiobutton').length"), Is.EqualTo(2));
	}

	[Test, WithPlaywrightPage]
	public async Task ActiveRadioButtonTextHaveDottedBlackBorder()
	{
		var keyboardPressOptions = new KeyboardPressOptions { Delay = 500 };
		RadioButton radioButton1 = null;
		RadioButton radioButton2 = null;

		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			radioButton1 = new RadioButton() { Dock = DockStyle.Right, Name = "RadioButton1", Text = "RadioButton 1" };
			radioButton2 = new RadioButton() { Dock = DockStyle.Right, Name = "RadioButton2", Text = "RadioButton 2" };
			var form = new Form();
			form.Controls.Add(radioButton1);
			form.Controls.Add(radioButton2);
			Assert.That(radioButton1.Focused, Is.False);
			Assert.That(radioButton2.Focused, Is.False);
			return form;
		});

		await page.WaitForSelectorAsync(".radiobutton + .radiobutton");
		await page.ClickAsync(".radiobutton:nth-child(2) .radiobutton__text");
		var selectedRadionButton = await page.WaitForSelectorAsync(".radiobutton:nth-child(2) .radiobutton__text");
		Assert.That(async () => await selectedRadionButton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('outline')"), Is.EqualTo("rgb(0, 0, 0) dotted 1px"));
		Assert.That(radioButton1.Focused, Is.False);
		Assert.That(radioButton2.Focused, Is.True);

		await page.ClickAsync(".radiobutton:first-child input");
		selectedRadionButton = await page.WaitForSelectorAsync(".radiobutton:first-child .radiobutton__text");
		Assert.That(async () => await selectedRadionButton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('outline')"), Is.EqualTo("rgb(0, 0, 0) dotted 1px"));
		Assert.That(radioButton1.Focused, Is.True);
		Assert.That(radioButton2.Focused, Is.False);

		await page.Keyboard.PressAsync("ArrowRight", keyboardPressOptions);
		selectedRadionButton = await page.WaitForSelectorAsync(".radiobutton:nth-child(2) .radiobutton__text");
		Assert.That(async () => await selectedRadionButton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('outline')"), Is.EqualTo("rgb(0, 0, 0) dotted 1px"));
		Assert.That(radioButton1.Focused, Is.False);
		Assert.That(radioButton2.Focused, Is.True);
	}

	void AssertItemPosition(IElement item, int width, int height, int left, int top, string position, string margin = null)
	{
		var styleString = item.GetAttribute("style");
		Assert.That(!string.IsNullOrEmpty(styleString));
		styleString = styleString.Replace(" ", string.Empty);
		Assert.That(styleString, Does.Contain($"width:{width}px"));
		Assert.That(styleString, Does.Contain($"height:{height}px"));
		Assert.That(styleString, Does.Contain($"left:{left}px"));
		Assert.That(styleString, Does.Contain($"top:{top}px"));
		Assert.That(styleString, Does.Contain($"position:{position}"));
		if (margin != null)
		{
			Assert.That(styleString, Does.Contain($"margin:{margin}px"));
		}
	}

	[Test, WithPlaywrightPage]
	public async Task RadioButtonPressingTabKeyChangeFocusPressingSpaceKeySelect()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var panel1 = new Panel() { Top = 10, Left = 10, Width = 300, Height = 20 };
			var radioButton1 = new RadioButton() { Dock = DockStyle.Right, Name = "Radio1", Text = "Radio 1", TabStop = true };
			var radioButton2 = new RadioButton() { Dock = DockStyle.Right, Name = "Radio2", Text = "Radio 2", TabStop = true };
			var radioButton3 = new RadioButton() { Dock = DockStyle.Right, Name = "Radio3", Text = "Radio 3", TabStop = true };
			panel1.Controls.Add(radioButton1);
			panel1.Controls.Add(radioButton2);
			panel1.Controls.Add(radioButton3);

			var panel2 = new Panel() { Top = 30, Left = 10, Width = 300, Height = 20 };
			var radioButton4 = new RadioButton() { Dock = DockStyle.Right, Name = "Radio4", Text = "Radio 4", TabStop = true };
			var radioButton5 = new RadioButton() { Dock = DockStyle.Right, Name = "Radio5", Text = "Radio 5", TabStop = true };
			panel2.Controls.Add(radioButton4);
			panel2.Controls.Add(radioButton5);

			var form = new Form();
			form.Controls.Add(panel1);
			form.Controls.Add(panel2);
			return form;
		});

		await page.ClickAsync(".form");
		await PressTabCheckFocus(page, "Tab", "Radio 1");
		await PressTabCheckFocus(page, "Tab", "Radio 4");
		await PressTabCheckFocus(page, "Shift+Tab", "Radio 1");

		await page.Keyboard.PressAsync("Space");
		var inputControl = await page.WaitForSelectorAsync(".radiobutton:nth-child(1) input[type=radio]");
		Assert.That(await inputControl.IsCheckedAsync(), Is.True);

		await PressArrowCheckSelection(page, "ArrowRight", "Radio 2");
		await PressArrowCheckSelection(page, "ArrowRight", "Radio 3");
		await PressTabCheckFocus(page, "Tab", "Radio 4");
		await PressArrowCheckSelection(page, "ArrowLeft", "Radio 5");
		await PressTabCheckFocus(page, "Shift+Tab", "Radio 3");
	}

	async Task PressTabCheckFocus(IPage page, string key, string expectedText)
	{
		await page.Keyboard.PressAsync(key);
		var spanControl = await page.WaitForSelectorAsync("input[type=radio]:focus + span", playWrightPageWaitForSelectorOptions);
		Assert.That(await spanControl.TextContentAsync(), Is.EqualTo(expectedText));
	}

	async Task PressArrowCheckSelection(IPage page, string key, string expectedText)
	{
		await page.Keyboard.PressAsync(key);
		var spanControl = await page.WaitForSelectorAsync("input[type=radio]:focus + span", playWrightPageWaitForSelectorOptions);
		Assert.That(await spanControl.TextContentAsync(), Is.EqualTo(expectedText));
		var inputControl = await page.WaitForSelectorAsync("input[type=radio]:focus", playWrightPageWaitForSelectorOptions);
		Assert.That(await inputControl.IsCheckedAsync(), Is.True);
	}

	[Test]
	public async Task RadioButtonNameEqualToWinzorControlId()
	{
		Panel panel = null;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			panel = new Panel();
			panel.Controls.Add(new RadioButton());
			return panel;
		});
		var radioInput = rendered.Find("input[type=\"radio\"]");
		Assert.That(radioInput.GetAttribute("name"), Is.EqualTo(panel.WinzorControlId));
	}

	[Test, WithPlaywrightPage]
	public async Task DisabledRadioButtonsHaveCorrectStyle()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(new RadioButton() { Enabled = false, Text = "Radio Button 1" });
			form.Controls.Add(new RadioButton() { Enabled = false, Checked = true, Text = "Radio Button 2" });
			return form;
		});

		var radioButton = await page.WaitForSelectorAsync(".radiobutton:first-child");
		Assert.That(await GetComputedStyle(radioButton, "color"), Is.EqualTo("rgb(85, 85, 85)"));

		radioButton = await page.WaitForSelectorAsync(".radiobutton:nth-child(2) > input[type=radio]");
		Assert.That(await GetComputedStyle(radioButton, "box-shadow"), Does.Contain("rgb(204, 204, 204)"));
		Assert.That(await GetComputedStyle(radioButton, "background-color"), Is.EqualTo("rgb(204, 204, 204)"));

		await page.ClickAsync(".radiobutton", new PageClickOptions { Force = true });
		var radioButtonText = await page.WaitForSelectorAsync(".radiobutton .radiobutton__text");
		Assert.That(await GetComputedStyle(radioButtonText, "outline"), Does.Contain("none"));
	}

	async Task<string> GetComputedStyle(IElementHandle e, string property)
	{
		return (await e.EvaluateAsync($"e => window.getComputedStyle(e).getPropertyValue('{property}')")).Value.ToString();
	}

	[Test]
	public async Task RadioButtonShouldBeCheckedWhenFocus()
	{
		RadioButton radioButton = null;
		using var ctx = new WinzorTestContext();
		await ctx.RenderControlOnFormAsync(() => radioButton = new RadioButton());

		Assert.That(radioButton.Checked, Is.False);
		await radioButton.InvokeWinzorDispatcherAsync(() => radioButton.Focus());
		Assert.That(radioButton.Checked, Is.True);
	}

	[TestCase(Keys.Tab, true, false, TestName = "RadioButtonShouldNotBeCheckedWhenTabbing")]
	[TestCase(Keys.Right, false, true, TestName = "RadioButtonShouldBeCheckedWhenUsingArrowKeys")]
	public async Task RadioButtonSelectionBehavior(Keys keys, bool radioButton1Checked, bool radioButton2Checked)
	{
		Form form = null;
		RadioButton radioButton1 = null;
		RadioButton radioButton2 = null;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			radioButton1 = new RadioButton();
			radioButton2 = new RadioButton();
			form.Controls.Add(radioButton1);
			form.Controls.Add(radioButton2);
			return form;
		});

		await form.InvokeWinzorDispatcherAsync(() => radioButton1.Focus());
		Assert.That(radioButton1.Checked, Is.True);
		Assert.That(radioButton2.Checked, Is.False);

		var radioButtonElements = rendered.FindAll(".radiobutton input[type=radio]");
		await rendered.KeyPressAsync(keys, radioButtonElements[0]);

		Assert.That(radioButton1.Checked, Is.EqualTo(radioButton1Checked));
		Assert.That(radioButton2.Checked, Is.EqualTo(radioButton2Checked));
	}
}
