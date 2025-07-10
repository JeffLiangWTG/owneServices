using System.Linq;
using System.Threading.Tasks;
using Bunit;
using CargoWise.Blazor.Client.Integration.Menus;
using Moq;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;
[Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1093:DoNotUseSystemWindowsFormsToolStripControls", Justification = "Testing")]
class ToolStripSplitButtonTest
{
	[Test, WithPlaywrightPage]
	public async Task ToolStripSplitButtonImageShouldHaveMarginOnRight() {
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var toolStrip = new ToolStrip();
			var splitButton = new ToolStripSplitButton() { Text = "Test", Image = TestImage.GetImage() };
			splitButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			toolStrip.Items.Add(splitButton);
			form.Controls.Add(toolStrip);
			return form;
		});
		await page.WaitForSelectorAsync(".form");
		var toolStripButtonImage = await page.WaitForSelectorAsync(".toolstrip-item--splitbutton .splitbutton__button__image");
		Assert.That(async () => await toolStripButtonImage.EvaluateAsync<string>("element => window.getComputedStyle(element).marginRight"), Is.EqualTo("3px"));
	}

	[Test, WithPlaywrightPage]
	public async Task ToolStripSplitButtonShouldHaveZeroPadding()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var toolStrip = new ToolStrip();
			var splitButton = new ToolStripSplitButton() { Text = "Test", Image = TestImage.GetImage() };
			splitButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			toolStrip.Items.Add(splitButton);
			form.Controls.Add(toolStrip);
			return form;
		});
		await page.WaitForSelectorAsync(".form");
		var toolStripButtonButton = await page.WaitForSelectorAsync(".toolstrip-item--splitbutton .splitbutton__button");
		Assert.That(async () => await toolStripButtonButton.EvaluateAsync<string>("element => window.getComputedStyle(element).padding"), Is.EqualTo("0px"));
	}

	[Test]
	public async Task ToolStripSplitButtonShouldHaveAssociatedStylingClass()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			var splitButton = new ToolStripSplitButton() { Text = "Test" };
			splitButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			toolStrip.Items.Add(splitButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripSplitButton = rendered.Find(".toolstrip-item--splitbutton");
		Assert.That(toolStripSplitButton, Is.Not.Null);
	}

	[Test]
	public async Task ToolStripDropDownShouldHideAfterMenuItemClicked()
	{
		using var ctx = new WinzorTestContext();
		ToolStripSplitButton splitButton = null;
		ToolStripDropDownItem toolStripMenuItem = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			splitButton = new ToolStripSplitButton() { Text = "Test" };
			toolStripMenuItem = new ToolStripDropDownItem() { Text = "DropdownItem" };
			splitButton.DropDownItems.Add(toolStripMenuItem);
			toolStrip.Items.Add(splitButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripSplitButton = rendered.Find(".toolstrip-item--splitbutton");
		Assert.That(toolStripSplitButton, Is.Not.Null);

		await splitButton.InvokeWinzorDispatcherAsync(() =>
		{
			splitButton.ShowDropDown();
			toolStripMenuItem.PerformClick();
			for (var i = 0; i < ToolStripManager.ToolStrips.Count; i++)
			{
				if (ToolStripManager.ToolStrips[i] is ToolStripDropDown toolStripDropDown)
				{
					Assert.That(toolStripDropDown.Visible, Is.False);
				}
			}
		});
	}

	[Test]
	public async Task ToolStripSplitButtonNotHandleClickIfDisabled()
	{
		using var ctx = new WinzorTestContext();
		var eventFired = false;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var toolStrip = new ToolStrip();
			var splitButton = new ToolStripSplitButton() { Text = "Test", Enabled = false };
			splitButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			toolStrip.Items.Add(splitButton);
			splitButton.ButtonClick += (_, _) => eventFired = true;
			return toolStrip;
		});
		var toolStripSplitButton = rendered.Find(".toolstrip-item--splitbutton .splitbutton__button");
		Assert.That(toolStripSplitButton, Is.Not.Null);
		await toolStripSplitButton.ClickAsync(new WebMouseEventArgs());
		Assert.That(eventFired, Is.False);
	}

	[Test]
	public async Task ToolStripSplitButtonNotHandleMouseDownIfDisabled()
	{
		using var ctx = new WinzorTestContext();
		var openingInvoked = false;
		var dropDownInvoked = false;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var toolStrip = new ToolStrip();
			var splitButton = new ToolStripSplitButton() { Text = "Test", Enabled = false };
			splitButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			toolStrip.Items.Add(splitButton);
			splitButton.DropDown.Opening += (s, e) => openingInvoked = true;
			splitButton.DropDownOpening += (s, e) => dropDownInvoked = true;
			return toolStrip;
		});
		var toolStripSplitButton = rendered.Find(".toolstrip-item--splitbutton .splitbutton__dropdownbutton");
		Assert.That(toolStripSplitButton, Is.Not.Null);
		await toolStripSplitButton.MouseDownAsync(new WebMouseEventArgs());
		Assert.That(openingInvoked, Is.False);
		Assert.That(dropDownInvoked, Is.False);
	}

	[Test]
	public async Task ToolStripSplitButtonShouldHaveButtonImageTextClasses_WithImageAndText()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			var splitButton = new ToolStripSplitButton() { Text = "Test", Image = TestImage.GetImage() };
			splitButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			toolStrip.Items.Add(splitButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripSplitButton = rendered.Find(".toolstrip-item--splitbutton .splitbutton__button--with-image-and-text");
		Assert.That(toolStripSplitButton, Is.Not.Null);
	}

	[Test]
	public async Task ToolStripSplitButtonShouldHaveButtonImageTextClasses_WithImageOnly()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			var splitButton = new ToolStripSplitButton() { Image = TestImage.GetImage() };
			splitButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			toolStrip.Items.Add(splitButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripSplitButton = rendered.Find(".toolstrip-item--splitbutton .splitbutton__button--with-image-only");
		Assert.That(toolStripSplitButton, Is.Not.Null);
	}

	[Test]
	public async Task ToolStripSplitButtonShouldHaveButtonImageTextClasses_WithTextOnly()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			var splitButton = new ToolStripSplitButton() { Text = "Test" };
			splitButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			toolStrip.Items.Add(splitButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripSplitButton = rendered.Find(".toolstrip-item--splitbutton .splitbutton__button--with-text-only");
		Assert.That(toolStripSplitButton, Is.Not.Null);
	}

	[Test]
	public async Task ToolStripSplitButtonShouldHaveCorrectAlignmentRight()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip() { AutoSize = false, Width = 300 };
			toolStrip.Items.Add(new ToolStripSplitButton { AutoSize = false, Width = 100, Alignment = ToolStripItemAlignment.Right });
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripButton = rendered.Find(".toolstrip-item--splitbutton");
		Assert.That(toolStripButton.GetAttribute("style"), Does.Contain("left:200px;"));
	}

	[Test]
	public async Task ToolStripSplitButtonShouldHaveCorrectAlignmentLeft()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip() { AutoSize = false, Width = 300, GripStyle = ToolStripGripStyle.Hidden };
			toolStrip.Items.Add(new ToolStripSplitButton { AutoSize = false, Width = 100, Alignment = ToolStripItemAlignment.Left });
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripButton = rendered.Find(".toolstrip-item--splitbutton");
		Assert.That(toolStripButton.GetAttribute("style"), Does.Contain("left:0px;"));
	}

	[Test]
	public async Task ToolStripSplitButtonShouldHaveCorrectdPadding()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			var splitButton = new ToolStripSplitButton() { Text = "Test", Margin = new Padding(5), Padding = new Padding(3, 2, 1, 0) };
			splitButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			toolStrip.Items.Add(splitButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripSplitButton = rendered.Find(".toolstrip-item--splitbutton");
		Assert.That(toolStripSplitButton.ClassList, !Contains.Item("pl-3"));
		Assert.That(toolStripSplitButton.ClassList, Contains.Item("pt-2"));
		Assert.That(toolStripSplitButton.ClassList, !Contains.Item("pr-1"));
		Assert.That(toolStripSplitButton.ClassList, Contains.Item("pb-0"));
	}

	[Test]
	public async Task ToolStripSplitButtonShouldHaveTextSetProperly()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			var splitButton = new ToolStripSplitButton() { Text = "Test" };
			splitButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			toolStrip.Items.Add(splitButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripSplitButton = rendered.Find(".toolstrip-item--splitbutton .splitbutton__button__text");
		Assert.That(toolStripSplitButton.TextContent, Is.EqualTo("Test"));
	}

	[Test]
	public async Task ToolStripSplitButtonShouldHaveImageSetProperlyIfAny()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			var splitButton = new ToolStripSplitButton() { Image = TestImage.GetImage() };
			splitButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			toolStrip.Items.Add(splitButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripSplitButtonImage = rendered.Find(".toolstrip-item--splitbutton .splitbutton__button__image");
		Assert.That(toolStripSplitButtonImage.GetAttribute("src"), Does.StartWith("data:image/png;base64,"));
	}

	[Test]
	public async Task ToolStripSplitButtonHasClickEvent()
	{
		await ControlAssert.ImplementsEventAsync<ToolStripSplitButton, EventHandler>(nameof(ToolStripSplitButton.ButtonClick), a => new EventHandler((o, e) => a()), ".toolstrip-item--splitbutton .splitbutton__button", e => e.Click());
	}

	[Test]
	public async Task ToolStripSplitButtonDropdownButtonHasIcon()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			var splitButton = new ToolStripSplitButton() { Text = "Test" };
			splitButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			toolStrip.Items.Add(splitButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		Assert.That(rendered.Find(".toolstrip-item--splitbutton .splitbutton__dropdownbutton").TextContent, Is.EqualTo("arrow_drop_down"));
	}

	[Test]
	public async Task ToolStripSplitButtonDropdownButtonHasStyleClass()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			var splitButton = new ToolStripSplitButton() { Text = "Test" };
			splitButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			toolStrip.Items.Add(splitButton);
			form.Controls.Add(toolStrip);
			return form;
		});
		Assert.That(rendered.FindAll(".toolstrip-item--splitbutton .splitbutton__dropdownbutton").Count, Is.EqualTo(1));
	}

	[Test]
	public async Task ToolStripSplitButtonCallsMenuDisplayerForDropdown()
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
			var toolstripSplitButton = new ToolStripSplitButton();
			toolstripSplitButton.DropDownItems.Add(caption1);
			toolstripSplitButton.DropDownItems.Add(caption2);
			toolstrip.Items.Add(toolstripSplitButton);
			form.Controls.Add(toolstrip);
			return form;
		}, clientServices);
		var toolStripSplitButton = rendered.Find(".toolstrip-item--splitbutton .splitbutton__dropdownbutton");
		await toolStripSplitButton.MouseDownAsync(new WebMouseEventArgs());
		var menuItems = menuDisplayer.Menu.MenuItems.ToList();
		Assert.That(menuItems.ElementAt(0).Text, Is.EqualTo(caption1));
		Assert.That(menuItems.ElementAt(1).Text, Is.EqualTo(caption2));
	}

	[Test]
	public async Task ToolStripMenuLoadDynamicMenuItem()
	{
		var menuDisplayer = new TestMenuDisplayer();
		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: menuDisplayer);
		var openingInvoked = false;
		var dropDownInvoked = false;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			var splitButton = new ToolStripSplitButton() { Text = "Test" };
			splitButton.DropDownItems.Add(new ToolStripMenuItem("Placeholder"));
			splitButton.DropDown.Opening += (s, e) => openingInvoked = true;
			splitButton.DropDownOpening += (s, e) =>
			{
				splitButton.DropDownItems.Clear();
				var dynamic = new ToolStripMenuItem("Dynamic");
				dynamic.DropDownItems.Add(new ToolStripMenuItem("Dynamic2"));
				splitButton.DropDownItems.Add(dynamic);
				dropDownInvoked = true;
			};
			toolStrip.Items.Add(splitButton);
			form.Controls.Add(toolStrip);

			return form;
		}, clientServices);
		await rendered.Find(".toolstrip-item--splitbutton .splitbutton__dropdownbutton").TriggerEventAsync("onmousedown", new WebMouseEventArgs());
		var testMenu = menuDisplayer.Menu.MenuItems.Single(m => m.Text == "Dynamic");
		Assert.That(testMenu.Loadable, Is.True);
		Assert.That(menuDisplayer.Menu.MenuItems.Select(o => o.Text).ToArray(), Is.EqualTo(new[] { "Dynamic" }));

		Assert.That(openingInvoked, Is.True);
		Assert.That(dropDownInvoked, Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task ToolStripSplitButtonTextAlignment()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var toolStrip = new ToolStrip();
			var splitButton = new ToolStripSplitButton() { Text = "Test", Image = TestImage.GetImage() };
			splitButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			toolStrip.Items.Add(splitButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		var button = await page.WaitForSelectorAsync(".splitbutton__button-wrapper");
		Assert.That(async () => await button.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('justify-content')"), Is.EqualTo("flex-start"));
	}

	[Test, WithPlaywrightPage]
	public async Task ToolStripSplitButtonShouldHaveCorrectStyling()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var toolStrip = new ToolStrip();
			var splitButton = new ToolStripSplitButton() { Text = "Test", Image = TestImage.GetImage() };
			splitButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			toolStrip.Items.Add(splitButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		await page.WaitForSelectorAsync(".form");

		var toolstripSplitButton = await page.WaitForSelectorAsync(".toolstrip-item--splitbutton");
		var toolstripSplitButtonButtonOverlay = await toolstripSplitButton.WaitForSelectorAsync(".splitbutton__button-wrapper .toolstrip-item__visual-overlay");

		await toolstripSplitButton.HoverAsync();

		Assert.That(async () => await toolstripSplitButtonButtonOverlay.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-color')"), Is.EqualTo("rgba(60, 165, 255, 0.133)"));
		Assert.That(async () => await toolstripSplitButtonButtonOverlay.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('border-width')"), Is.EqualTo("1px"));
		Assert.That(async () => await toolstripSplitButtonButtonOverlay.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('border-color')"), Is.EqualTo("rgba(0, 140, 255, 0.2)"));
	}

	[Test, WithPlaywrightPage]
	public async Task ToolStripSplitDropButtonShouldHaveCorrectStyling()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var toolStrip = new ToolStrip();
			var splitButton = new ToolStripSplitButton() { Text = "Test", Image = TestImage.GetImage() };
			splitButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			toolStrip.Items.Add(splitButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		await page.WaitForSelectorAsync(".form");

		var toolstripSplitButton = await page.WaitForSelectorAsync(".toolstrip-item--splitbutton");
		var toolstripSplitButtonDropDownButtonOverlay = await toolstripSplitButton.WaitForSelectorAsync(".splitbutton__dropdownbutton-wrapper .toolstrip-item__visual-overlay");

		await toolstripSplitButton.HoverAsync();
		Assert.That(async () => await toolstripSplitButtonDropDownButtonOverlay.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-color')"), Is.EqualTo("rgba(60, 165, 255, 0.133)"));
		Assert.That(async () => await toolstripSplitButtonDropDownButtonOverlay.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('border-width')"), Is.EqualTo("1px 1px 1px 0px"));
		Assert.That(async () => await toolstripSplitButtonDropDownButtonOverlay.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('border-right-color')"), Is.EqualTo("rgba(0, 140, 255, 0.2)"));
	}

	[TestCaseSource(typeof(ToolStripButtonTest.ToolTipTestCases), nameof(ToolStripButtonTest.ToolTipTestCases.TestToolTips))]
	public async Task ToolStripSplitButtonShouldShowTooltipOnHover(bool showItemToolTips, bool autoToolTip, string toolTipText, string expectedTitle)
	{
		using var ctx = new WinzorTestContext();
		ToolStripSplitButton button = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip { ShowItemToolTips = showItemToolTips };
			button = new ToolStripSplitButton
			{
				AutoToolTip = autoToolTip,
				Text = "&TestText",
				ToolTipText = toolTipText
			};
			toolStrip.Items.Add(button);
			form.Controls.Add(toolStrip);
			return form;
		});

		var renderedButton = rendered.Find(".toolstrip-item--splitbutton .splitbutton__button");
		Assert.That(renderedButton.GetAttribute("title"), Is.EqualTo(expectedTitle));
	}

	[Test, WithPlaywrightPage]
	public async Task ToolStripSplitButtonShouldHaveCorrectBackgroundColor()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var toolStrip = new ToolStrip();
			var splitButton = new ToolStripSplitButton() { Text = "Test" };
			splitButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			toolStrip.Items.Add(splitButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		await page.WaitForSelectorAsync(".form");
		var toolStripButtonButton = await page.WaitForSelectorAsync(".splitbutton__button");
		Assert.That(async () => await toolStripButtonButton.EvaluateAsync<string>("element => window.getComputedStyle(element).backgroundColor"), Is.EqualTo("rgba(0, 0, 0, 0)"));
	}

	[Test, WithPlaywrightPage]
	public async Task ToolStripSplitDropdownButtonShouldHaveCorrectBackgroundColor()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var toolStrip = new ToolStrip();
			var splitButton = new ToolStripSplitButton() { Text = "Test" };
			splitButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			toolStrip.Items.Add(splitButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		await page.WaitForSelectorAsync(".form");
		var toolStripButtonButton = await page.WaitForSelectorAsync(".splitbutton__dropdownbutton");
		Assert.That(async () => await toolStripButtonButton.EvaluateAsync<string>("element => window.getComputedStyle(element).backgroundColor"), Is.EqualTo("rgba(0, 0, 0, 0)"));
	}

	[Test]
	[TestCase(true, "Enabled Btn", TestName = "{m}_EnabledButton")]
	[TestCase(false, "Disabled Btn", TestName = "{m}_DisabledButton")]
	public async Task ToolStripSplitButtonDisabledClassAndAttribute(bool enabled, string text)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			var splitButton = new ToolStripSplitButton() { Enabled = enabled, Text = text, Image = TestImage.GetImage() };
			splitButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			toolStrip.Items.Add(splitButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		var buttonWrapperClass = rendered.Find(".toolstrip-item--splitbutton").Attributes["class"]?.Value;
		Assert.That(buttonWrapperClass, enabled ? Does.Not.Contain("disabled") : Does.Contain("disabled"));

		var toolStripSplitButton = rendered.Find(".splitbutton__button-wrapper .splitbutton__button");
		Assert.That(toolStripSplitButton.GetAttribute("disabled"), enabled ? Is.Null : Is.Not.Null);

		var toolStripSplitDropdownButtonn = rendered.Find(".splitbutton__dropdownbutton-wrapper .splitbutton__dropdownbutton");
		Assert.That(toolStripSplitDropdownButtonn.GetAttribute("disabled"), enabled ? Is.Null : Is.Not.Null);
	}

	[Test, WithPlaywrightPage]
	public async Task ToolStripSplitDropdownButtonImageShouldNotBeDraggable()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var toolStrip = new ToolStrip();
			var splitButton = new ToolStripSplitButton() { Text = "text", Image = TestImage.GetImage() };
			splitButton.DropDownItems.Add(new ToolStripMenuItem() { Text = "DropdownItem" });
			toolStrip.Items.Add(splitButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		await page.WaitForSelectorAsync(".form");
		var toolStripButtonImage = await page.WaitForSelectorAsync(".splitbutton__button__image");
		Assert.That(await toolStripButtonImage.EvaluateAsync<bool>("el => el.hasAttribute('draggable')"), Is.EqualTo(true));
		Assert.That(await toolStripButtonImage.EvaluateAsync<bool>("el => el.getAttribute('draggable')"), Is.EqualTo(false));
	}

	[Test]
	public async Task ToolStripSplitButtonProcessMnemonicSelectsFirstControl()
	{
		using var ctx = new WinzorTestContext();
		var clicked = false;
		ToolStripSplitButton splitButton = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			splitButton = new ToolStripSplitButton() { Text = "&Test" };
			splitButton.ButtonClick += (sender, args) => clicked = true;
			toolStrip.Items.Add(splitButton);
			form.Controls.Add(toolStrip);
			return form;
		});

		Assert.That(clicked, Is.False);
		var toolStripItem = rendered.Find(".toolstrip-item--splitbutton");
		await rendered.KeyPressAsync(Keys.Alt | Keys.T, toolStripItem);
		Assert.That(clicked, Is.True);

		clicked = false;

		Assert.That(clicked, Is.False);
		var form = rendered.Find(".form");
		await rendered.KeyPressAsync(Keys.Alt | Keys.T, form);
		Assert.That(clicked, Is.True);
	}

	[Test]
	public async Task ToolStripSplitButtonUseMnemonicFormatsTestWithMnemonic()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			var splitButton = new ToolStripSplitButton() { Text = "&Test" };
			toolStrip.Items.Add(splitButton);
			form.Controls.Add(toolStrip);
			return form;
		});
		Assert.That(rendered.Find(".splitbutton__button__text").InnerHtml, Is.EqualTo("<span class=\"mnemonickey\">T</span>est"));
	}
}
