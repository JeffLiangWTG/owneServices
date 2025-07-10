using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;
class ToolStripDropDownButtonTest
{
	[Test]
	public async Task ToolStripDropDownButtonShouldShowDropDownArrowIfAny()
	{
		using var ctx = new WinzorTestContext();
		ToolStripDropDownButton dropDownButton = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			dropDownButton = new ToolStripDropDownButton() { Text = "Test" };
			dropDownButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			dropDownButton.ShowDropDownArrow = true;
			toolStrip.Items.Add(dropDownButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		Assert.That(rendered.Find(".toolstrip-item--dropdownbutton .dropdownbutton__button__arrow").TextContent, Is.EqualTo("keyboard_arrow_down"));

		await dropDownButton.InvokeWinzorDispatcherAsync(() => dropDownButton.ShowDropDownArrow = false);
		Assert.That(rendered.FindAll(".toolstrip-item--dropdownbutton .dropdownbutton__button__arrow").Count, Is.Zero);
	}

	[Test]
	public async Task ToolStripDropDownShouldHaveImageSetProperlyIfAny()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			var dropDownButton = new ToolStripDropDownButton() { Image = TestImage.GetImage() };
			dropDownButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			toolStrip.Items.Add(dropDownButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		var dropDownButtonImage = rendered.Find(".toolstrip-item--dropdownbutton .dropdownbutton__button__image");
		Assert.That(dropDownButtonImage.GetAttribute("src"), Does.StartWith("data:image/png;base64,"));
	}

	[Test, WithPlaywrightPage]
	public async Task ToolStripDropDownButtonImageShouldHaveMarginOnRight()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var toolStrip = new ToolStrip();
			var dropDownButton = new ToolStripDropDownButton() { Text = "Test", Image = TestImage.GetImage() };
			dropDownButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			toolStrip.Items.Add(dropDownButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		await page.WaitForSelectorAsync(".form");
		var toolStripButtonImage = await page.WaitForSelectorAsync(".toolstrip-item--dropdownbutton .dropdownbutton__button__image");
		Assert.That(async () => await toolStripButtonImage.EvaluateAsync<string>("element => window.getComputedStyle(element).marginRight"), Is.EqualTo("3px"));
	}

	[Test]
	public async Task ToolStripDropDownButtonShouldHaveAssociatedStylingClass()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			var dropDownButton = new ToolStripDropDownButton() { Text = "Test" };
			dropDownButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			toolStrip.Items.Add(dropDownButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		var dropDownButton = rendered.Find(".toolstrip-item--dropdownbutton");
		Assert.That(dropDownButton, Is.Not.Null);
	}

	[Test]
	public async Task ToolStripDropDownButtonShouldHaveButtonImageTextClasses_WithImageAndText()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			var dropDownButton = new ToolStripDropDownButton() { Text = "Test", Image = TestImage.GetImage() };
			dropDownButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			toolStrip.Items.Add(dropDownButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		var dropDownButton = rendered.Find(".toolstrip-item--dropdownbutton .dropdownbutton__button--with-image-and-text");
		Assert.That(dropDownButton, Is.Not.Null);
	}

	[Test]
	public async Task ToolStripDropDownButtonShouldHaveButtonImageTextClasses_WithImageOnly()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			var dropDownButton = new ToolStripDropDownButton() { Image = TestImage.GetImage() };
			dropDownButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			toolStrip.Items.Add(dropDownButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		var dropDownButton = rendered.Find(".toolstrip-item--dropdownbutton .dropdownbutton__button--with-image-only");
		Assert.That(dropDownButton, Is.Not.Null);
	}

	[Test]
	public async Task ToolStripDropDownButtonShouldHaveButtonImageTextClasses_WithTextOnly()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			var dropDownButton = new ToolStripDropDownButton() { Text = "Test" };
			dropDownButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			toolStrip.Items.Add(dropDownButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		var dropDownButton = rendered.Find(".toolstrip-item--dropdownbutton .dropdownbutton__button--with-text-only");
		Assert.That(dropDownButton, Is.Not.Null);
	}

	[Test]
	public async Task ToolStripDropDownButtonShouldHaveTextSetProperly()
	{
		using var ctx = new WinzorTestContext();
		ToolStripDropDownButton dropDownButton = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			dropDownButton = new ToolStripDropDownButton() { Text = "buttontext" };
			dropDownButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			toolStrip.Items.Add(dropDownButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		Assert.That(rendered.Find(".toolstrip-item--dropdownbutton .dropdownbutton__button__text").InnerHtml, Is.EqualTo("buttontext"));

		await dropDownButton.InvokeWinzorDispatcherAsync(() => dropDownButton.Text = "buttonchanged");
		rendered.WaitForState(() => rendered.Find(".toolstrip-item--dropdownbutton .dropdownbutton__button__text").InnerHtml == "buttonchanged");

		Assert.That(rendered.Find(".toolstrip-item--dropdownbutton .dropdownbutton__button__text").InnerHtml, Is.EqualTo("buttonchanged"));
	}

	[Test, WithPlaywrightPage]
	public async Task DropDownButtonShouldNotWrapText()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			ToolStripDropDownButton dropDownButton = new ToolStripDropDownButton() { Text = "buttontext" };
			dropDownButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			toolStrip.Items.Add(dropDownButton);
			form.Controls.Add(toolStrip);
			return form;
		});
		await page.WaitForSelectorAsync(".dropdownbutton__button__text");
		var dropDownButtonEl = await page.QuerySelectorAsync(".dropdownbutton__button__text");
		Assert.That(async () => await dropDownButtonEl.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('white-space')"), Is.EqualTo("nowrap"));
	}

	[Test]
	public async Task ToolStripDropDownButtonCallsMenuDisplayerForDropdown()
	{
		var menuDisplayer = new TestMenuDisplayer();
		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: menuDisplayer);

		using var ctx = new WinzorTestContext();
		var caption1 = "option1";
		var caption2 = "option2";
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolstrip = new ToolStrip();
			var dropDownButton = new ToolStripDropDownButton();
			dropDownButton.DropDownItems.Add(caption1);
			dropDownButton.DropDownItems.Add(caption2);
			toolstrip.Items.Add(dropDownButton);
			form.Controls.Add(toolstrip);
			return form;
		}, clientServices);
		var dropDownButton = rendered.Find(".toolstrip-item--dropdownbutton .dropdownbutton__button");
		await dropDownButton.MouseDownAsync(new WebMouseEventArgs());
		var menuItems = menuDisplayer.Menu.MenuItems.ToList();
		Assert.That(menuItems.ElementAt(0).Text, Is.EqualTo(caption1));
		Assert.That(menuItems.ElementAt(1).Text, Is.EqualTo(caption2));
	}

	[Test]
	public async Task ToolStripDropDownButtonHasMouseEnterEvent()
	{
		await ControlAssert.ImplementsEventAsync<ToolStripDropDownButton, EventHandler>(nameof(ToolStripDropDownButton.MouseEnter), a => new EventHandler((o, e) => a()), ".toolstrip-item--dropdownbutton .dropdownbutton__button", e => e.MouseEnter());
	}

	[Test]
	public async Task ToolStripDropDownButtonHasMouseLeaveEvent()
	{
		await ControlAssert.ImplementsEventAsync<ToolStripDropDownButton, EventHandler>(nameof(ToolStripButton.MouseLeave), a => new EventHandler((o, e) => a()), ".toolstrip-item--dropdownbutton .dropdownbutton__button", e => e.MouseLeave());
	}

	[Test]
	public async Task ToolStripDropDownButtonShouldHaveTitleIfToolTipTextIsPopulated()
	{
		// Arrange
		const string testCaption = "test caption";
		using var ctx = new WinzorTestContext();
		ToolStripDropDownButton dropDownButton = null;
		// Act
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			dropDownButton = new ToolStripDropDownButton() { Text = "Test" };
			dropDownButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			dropDownButton.ToolTipText = testCaption;
			dropDownButton.ShowDropDownArrow = true;
			toolStrip.Items.Add(dropDownButton);
			form.Controls.Add(toolStrip);
			return form;
		});
		// Assert
		Assert.That(rendered.Find(".toolstrip-item--dropdownbutton .dropdownbutton__button").GetAttribute("title"), Is.EqualTo(testCaption));
	}

	[Test]
	public async Task ToolStripDropDownButtonShouldNotHaveTitleIfToolTipTextIsNotPopulated()
	{
		// Arrange
		using var ctx = new WinzorTestContext();
		ToolStripDropDownButton dropDownButton = null;
		// Act
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			dropDownButton = new ToolStripDropDownButton() { Text = "Test" };
			dropDownButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			dropDownButton.ToolTipText = string.Empty;
			dropDownButton.AutoToolTip = false;
			dropDownButton.ShowDropDownArrow = true;
			toolStrip.Items.Add(dropDownButton);
			form.Controls.Add(toolStrip);
			return form;
		});
		// Assert
		Assert.That(rendered.Find(".toolstrip-item--dropdownbutton .dropdownbutton__button").GetAttribute("title"), Is.EqualTo(string.Empty));
	}

	[Test]
	public async Task ToolStripDropDownButtonShouldHaveDefaultTitleIfToolTipTextIsNotPopulatedAndAutoToolTipIsTrue()
	{
		// Arrange
		using var ctx = new WinzorTestContext();
		ToolStripDropDownButton dropDownButton = null;
		const string testText = "Text";
		// Act
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			dropDownButton = new ToolStripDropDownButton() { Text = testText };
			dropDownButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			dropDownButton.ToolTipText = string.Empty;
			dropDownButton.ShowDropDownArrow = true;
			toolStrip.Items.Add(dropDownButton);
			form.Controls.Add(toolStrip);
			return form;
		});
		// Assert
		Assert.That(rendered.Find(".toolstrip-item--dropdownbutton .dropdownbutton__button").GetAttribute("title"), Is.EqualTo(testText));
	}

	[TestCaseSource(nameof(PaddingClassSource))]
	public async Task ToolStripDropDownButtonShouldHavePaddingClass(Padding padding, string expectedClass)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			var dropDownButton = new ToolStripDropDownButton() { Text = "Test", Padding = padding };
			dropDownButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			toolStrip.Items.Add(dropDownButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		var dropDownButton = rendered.Find(".toolstrip-item.toolstrip-item--dropdownbutton button");
		Assert.That(dropDownButton.GetAttribute("class"), Does.Contain(expectedClass));
	}

	static IEnumerable<TestCaseData> PaddingClassSource()
	{
		yield return new TestCaseData(new Padding(4), "p-4");
		yield return new TestCaseData(new Padding(1, 2, 3, 4), "pt-2 pb-4");
	}

	[Test]
	public async Task ToolStripDropDownButtonShouldBindClickMouseDownMouseUpEventsCorrectly()
	{
		using var ctx = new WinzorTestContext();
		bool clickTriggered, mouseDownTriggered, mouseUpTriggered;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			var dropDownButton = new ToolStripDropDownButton();
			dropDownButton.Click += (s, e) => clickTriggered = true;
			dropDownButton.MouseDown += (s, e) => mouseDownTriggered = true;
			dropDownButton.MouseUp += (s, e) => mouseUpTriggered = true;
			toolStrip.Items.Add(dropDownButton);
			form.Controls.Add(toolStrip);
			return form;
		});
		clickTriggered = mouseDownTriggered = mouseUpTriggered = false;

		var dropDownButton = rendered.Find(".toolstrip-item--dropdownbutton .dropdownbutton__button");

		await dropDownButton.ClickAsync(new WebMouseEventArgs());
		Assert.That(clickTriggered, Is.True);

		await dropDownButton.MouseDownAsync(new WebMouseEventArgs());
		Assert.That(mouseDownTriggered, Is.True);

		await dropDownButton.MouseUpAsync(new WebMouseEventArgs());
		Assert.That(mouseUpTriggered, Is.True);
	}

	[Test]
	[TestCase(true, "Enabled Btn", TestName = "{m}_EnabledButton")]
	[TestCase(false, "Disabled Btn", TestName = "{m}_DisabledButton")]
	public async Task ToolStripDropDownButtonShouldReflectEnabledPropertyInStyle(bool enabled, string text)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStrip.Items.Add(new ToolStripDropDownButton { Enabled = enabled, Text = text });
			form.Controls.Add(toolStrip);
			return form;
		});

		var buttonWrapperClass = rendered.Find(".toolstrip-item--dropdownbutton").Attributes["class"]?.Value;
		Assert.That(buttonWrapperClass, enabled ? Does.Not.Contain("disabled") : Does.Contain("disabled"));

		var toolStripButton = rendered.Find(".toolstrip-item--dropdownbutton .dropdownbutton__button");
		Assert.That(toolStripButton.GetAttribute("disabled"), enabled ? Is.Null : Is.Not.Null);
	}

	[Test, WithPlaywrightPage]
	public async Task ToolStripDropButtonButtonCannotTabIn()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStrip.Items.Add(new ToolStripDropDownButton());
			form.Controls.Add(new TextBox());
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripDropDownButton = page.Locator(".dropdownbutton__button");
		Assert.That(await toolStripDropDownButton.EvaluateAsync<bool>("el => el.hasAttribute('tabindex')"), Is.EqualTo(true));
		Assert.That(await toolStripDropDownButton.EvaluateAsync<string>("el => el.getAttribute('tabindex')"), Is.EqualTo("-1"));

		var textbox = page.Locator(".textbox");
		await textbox.FocusAsync();
		await page.Keyboard.PressAsync("Tab");
		Assert.That(async () => (await toolStripDropDownButton.EvaluateAsync("node => document.activeElement === node")).Value.GetBoolean(), Is.False.After(3000, 100));
	}
}
