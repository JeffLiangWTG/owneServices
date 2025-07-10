using System.Drawing;
using System.Threading.Tasks;
using Bunit;
using CargoWise.Blazor.Client.Integration.Menus;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WinzorTestFramework;

using FontStyle = CargoWise.Blazor.Client.Integration.Messaging.FontStyle;

namespace System.Windows.Forms;

[Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1093:DoNotUseSystemWindowsFormsToolStripControls", Justification = "Testing")]
internal class ToolStripMenuItemTest
{
	[Test]
	public async Task ToolStripMenuItemShouldHaveAssociatedStylingClass()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStrip.Items.Add(new ToolStripMenuItem() { Text = "Menu Item Text" });
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripMenuItem = rendered.Find(".toolstrip__menuitem");
		Assert.That(toolStripMenuItem, Is.Not.Null);
	}

	[Test]
	public async Task ToolStripMenuItemShouldHaveTextSetProperly()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStrip.Items.Add(new ToolStripMenuItem() { Text = "Menu Item Text" });
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripMenuItem = rendered.Find(".toolstrip__menuitem");
		Assert.That(toolStripMenuItem.TextContent, Is.EqualTo("Menu Item Text"));
	}

	[Test]
	public async Task TestToolStripMenuItemShortCutKeys()
	{
		var menuDisplayer = new TestMenuDisplayer();
		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: menuDisplayer);

		var clickFlag = false;
		using var ctx = new WinzorTestContext();
		ToolStripMenuItem toolStripMenuItem = null;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStripMenuItem = new ToolStripMenuItem() { Text = "Menu Item Text 2", ShortcutKeys = Keys.Control | Keys.Shift | Keys.B };
			toolStrip.Items.Add(toolStripMenuItem);
			form.Controls.Add(toolStrip);
			toolStripMenuItem.Click += (e, args) => { clickFlag = true; };
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);

		var formElement = rendered.FindAll("div")[0];
		await rendered.KeyPressAsync(Keys.Control | Keys.Shift | Keys.B, formElement);
		Assert.That(clickFlag, Is.EqualTo(true));
	}

	[Test]
	public async Task ToolStripMenuItemShouldShowShortcutString()
	{
		using var ctx = new WinzorTestContext();
		ToolStripMenuItem toolStripMenuItem1 = null;
		ToolStripMenuItem toolStripMenuItem2 = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStripMenuItem1 = new ToolStripMenuItem() { Text = "Menu Item Text 1", ShortcutKeys = Keys.F1, ShowShortcutKeys = true };
			toolStripMenuItem2 = new ToolStripMenuItem() { Text = "Menu Item Text 2", ShortcutKeys = Keys.Control | Keys.Shift | Keys.B };
			toolStrip.Items.Add(toolStripMenuItem1);
			toolStrip.Items.Add(toolStripMenuItem2);
			form.Controls.Add(toolStrip);
			return form;
		});

		Assert.That(toolStripMenuItem1.ShortcutString, Is.Null);
		toolStripMenuItem1.ShowShortcutString();
		Assert.That(toolStripMenuItem1.ShortcutString, Is.EqualTo("F1"));

		Assert.That(toolStripMenuItem2.ShortcutString, Is.Null);
		toolStripMenuItem2.ShowShortcutString();
		Assert.That(toolStripMenuItem2.ShortcutString, Is.EqualTo("Ctrl+Shift+B"));
	}

	[Test]
	public async Task ToolStripDropDownShowShortcutKeysShouldBeTrue()
	{
		using var ctx = new WinzorTestContext();
		ToolStripMenuItem toolStripMenuItem = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStripMenuItem = new ToolStripMenuItem() { Text = "Menu Item Text", ShortcutKeys = Keys.F1 };
			toolStrip.Items.Add(toolStripMenuItem);
			form.Controls.Add(toolStrip);
			return form;
		});

		Assert.That(toolStripMenuItem.ShowShortcutKeys, Is.True);
	}

	[Test]
	public async Task ToolStripMenuItemShouldHaveDisabledSetProperly()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStrip.Items.Add(new ToolStripMenuItem() { Text = "Menu Item Text", Enabled = false });
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripMenuItem = rendered.Find(".toolstrip__menuitem");
		Assert.That(toolStripMenuItem.HasAttribute("disabled"), Is.EqualTo(true));
	}

	[Test]
	public async Task ToolStripDropDownItemShouldHaveVisibleSetProperly()
	{
		using var ctx = new WinzorTestContext();
		ToolStrip toolStrip = null;
		ToolStripMenuItem toolStripMenuItem = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			toolStrip = new ToolStrip();
			toolStripMenuItem = new ToolStripMenuItem() { Text = "Item1" };
			toolStrip.Items.Add(toolStripMenuItem);
			form.Controls.Add(toolStrip);
			return form;
		});

		Assert.That(toolStripMenuItem.Parent, Is.EqualTo(toolStrip));
		Assert.That(rendered.Find(".toolstrip__menuitem").TextContent, Is.EqualTo("Item1"));

		await toolStripMenuItem.InvokeWinzorDispatcherAsync(() =>
		{
			toolStripMenuItem.Visible = false;
		});
		Assert.That(rendered.FindAll(".toolstrip__menuitem").Count, Is.EqualTo(0));

		await toolStripMenuItem.InvokeWinzorDispatcherAsync(() =>
		{
			toolStripMenuItem.Visible = true;
			toolStripMenuItem.Available = false;
		});
		Assert.That(rendered.FindAll(".toolstrip__menuitem").Count, Is.EqualTo(0));

		await toolStripMenuItem.InvokeWinzorDispatcherAsync(() =>
		{
			toolStrip.Visible = false;
			toolStripMenuItem.Visible = true;
			toolStripMenuItem.Available = true;
		});
		Assert.That(rendered.FindAll(".toolstrip__menuitem").Count, Is.EqualTo(0));

		await toolStripMenuItem.InvokeWinzorDispatcherAsync(() =>
		{
			toolStrip.Visible = true;
			toolStripMenuItem.Visible = true;
			toolStripMenuItem.Available = true;
		});
		Assert.That(rendered.FindAll(".toolstrip__menuitem").Count, Is.EqualTo(1));
	}

	[Test]
	public async Task ToolStripMenuItemHasClickEvent()
	{
		await ControlAssert.ImplementsEventAsync<ToolStripMenuItem, EventHandler>(nameof(ToolStripMenuItem.Click), a => new EventHandler((o, e) => a()), ".toolstrip__menuitem", e => e.Click());
	}

	[Test]
	public async Task ToolStripMenuItemShowDropdownOnClick()
	{
		using var ctx = new WinzorTestContext();
		ctx.MockCargoWiseClientServices.MenuDisplayer.Setup(m => m.SendShowMenuRequestAsync(It.IsAny<MenuInteropModel>(), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()));

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			var menuItem = new ToolStripMenuItem { Text = "Menu Item" };
			menuItem.DropDownItems.Add(new ToolStripMenuItem { Text = "Sub Menu Item" });
			toolStrip.Items.Add(menuItem);
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripMenuItem = rendered.Find(".toolstrip__menuitem");
		Assert.That(toolStripMenuItem, Is.Not.Null);

		await toolStripMenuItem.ClickAsync(new WebMouseEventArgs());
		ctx.MockCargoWiseClientServices.MenuDisplayer.Verify(f => f.SendShowMenuRequestAsync(It.IsAny<MenuInteropModel>(), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()), Times.Once());
	}

	[TestCase(DockStyle.Bottom, true, TestName = "{m}_TopOfElement")]
	[TestCase(DockStyle.Top, false, TestName = "{m}_BottomOfElement")]
	public async Task ToolStripItemYPositionOn(DockStyle dockStyle, bool shouldBeOnTop)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var toolStrip = new ToolStrip() { Dock = dockStyle };
			var toolStripDropDown = new ToolStripDropDown();
			var toolStripItem = new ToolStripMenuItem { Text = "Menu Item" };

			toolStripDropDown.Items.Add(toolStripItem);
			toolStrip.Items.Add(toolStripItem);

			var form = new Form();
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripRendered = rendered.Find(".toolstrip");
		Assert.That(toolStripRendered, Is.Not.Null);
		Assert.That(toolStripRendered.GetAttribute("style"),
			shouldBeOnTop ? Does.Not.Contain("top:0px") : Does.Contain("top:0px"));
	}

	[TestCase(TextImageRelation.TextBeforeImage, TestName = "{m}_WhenTextBeforeImage")]
	[TestCase(TextImageRelation.ImageBeforeText, TestName = "{m}_WhenImageBeforeText")]
	public async Task ToolStripMenuItemShouldLoadImageCorrectly(TextImageRelation relation)
	{
		using var ctx = new WinzorTestContext();
		var regexPattern = @"(?<=left:)\d+(?=px)";
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStrip.Items.Add(new ToolStripMenuItem()
			{
				Text = "Menu Item Text",
				Image = new Bitmap(100, 100),
				TextImageRelation = relation
			});
			form.Controls.Add(toolStrip);
			return form;
		});

		var buttonInnerImage = rendered.FindAll(".toolstrip__menuitem > div")[0];
		var menuItemText = rendered.FindAll(".toolstrip__menuitem > div")[1];
		var imageClass = rendered.Find(".toolstrip__menuitem > div > img");
		var textLeft = int.Parse(Text.RegularExpressions.Regex.Match(menuItemText.GetAttribute("style"), regexPattern).Value);
		var imageLeft = int.Parse(Text.RegularExpressions.Regex.Match(buttonInnerImage.GetAttribute("style"), regexPattern).Value);

		Assert.That(textLeft, relation == TextImageRelation.TextBeforeImage ? Is.LessThan(imageLeft) : Is.GreaterThan(imageLeft));
		Assert.That(imageClass.GetAttribute("src"), Is.Not.Null);
	}

	[TestCase(FontStyle.Regular, "font-weight: normal", TestName = "{m}_WhenStyleIsRegular")]
	[TestCase(FontStyle.Bold, "font-weight: bold", TestName = "{m}_WhenStyleIsBold")]
	[TestCase(FontStyle.Italic, "font-style: italic", TestName = "{m}_WhenStyleIsItalic")]
	[TestCase(FontStyle.Underline, "text-decoration: underline;", TestName = "{m}_WhenStyleIsUnderline")]
	[TestCase(FontStyle.Strikeout, "text-decoration: line-through", TestName = "{m}_WhenStyleIsStrikeout")]
	public async Task ToolStripMenuItemShouldHavCorrectFontStyle(FontStyle fontStyle, string expected)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStrip.Items.Add(new ToolStripMenuItem()
			{
				Text = "Menu Item Text",
				Font = new Font("Arial", 8, (Drawing.FontStyle)fontStyle),
			});
			form.Controls.Add(toolStrip);
			return form;
		});

		var menuItemText = rendered.Find(".toolstrip__menuitem");
		Assert.That(menuItemText.GetAttribute("style"), Does.Contain(expected));
	}

	[Test]
	public async Task ToolStripMenuItemOnSelectShouldFireDropDownOpening()
	{
		using var ctx = new WinzorTestContext();
		var isDropDownOpeningFired = false;
		await ctx.RenderControlOnFormAsync(() =>
		{
			var toolStrip = new ToolStrip();
			var menuItem = new ToolStripMenuItem();
			menuItem.DropDownOpening += (sender, e) => isDropDownOpeningFired = true;
			toolStrip.Items.Add(menuItem);
			menuItem.OnSelect();
			return toolStrip;
		});

		Assert.That(isDropDownOpeningFired, Is.True);
	}

	[Test]
	public async Task ToolStripMenuItemCheckedChangedEventTriggeredProperly()
	{
		using var ctx = new WinzorTestContext();
		ToolStripMenuItem toolStripMenuItem = null;
		var checkedChangedTriggered = 0;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStripMenuItem = new ToolStripMenuItem() { Text = "Item1", Checked = false };
			toolStripMenuItem.CheckedChanged += (s, e) =>
			{
				checkedChangedTriggered++;
			};
			toolStrip.Items.Add(toolStripMenuItem);
			form.Controls.Add(toolStrip);
			return form;
		});

		Assert.That(checkedChangedTriggered, Is.EqualTo(0));

		await toolStripMenuItem.InvokeWinzorDispatcherAsync(() =>
		{
			toolStripMenuItem.Checked = true;
		});
		Assert.That(checkedChangedTriggered, Is.EqualTo(1));

		await toolStripMenuItem.InvokeWinzorDispatcherAsync(() =>
		{
			toolStripMenuItem.Checked = true;
		});
		Assert.That(checkedChangedTriggered, Is.EqualTo(1));

		await toolStripMenuItem.InvokeWinzorDispatcherAsync(() =>
		{
			toolStripMenuItem.Checked = false;
		});
		Assert.That(checkedChangedTriggered, Is.EqualTo(2));
	}

	[Test]
	public async Task ToolStripMenuItemCheckedChangedOnClickOnlyIfCheckOnClickIsTrue()
	{
		using var ctx = new WinzorTestContext();
		ToolStripMenuItem toolStripMenuItem = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStripMenuItem = new ToolStripMenuItem() { Text = "Item1", Checked = false, CheckOnClick = true };
			toolStrip.Items.Add(toolStripMenuItem);
			form.Controls.Add(toolStrip);
			return form;
		});

		Assert.That(toolStripMenuItem.Checked, Is.False);

		await toolStripMenuItem.InvokeWinzorDispatcherAsync(() =>
		{
			toolStripMenuItem.PerformClick();
		});
		Assert.That(toolStripMenuItem.Checked, Is.True);

		await toolStripMenuItem.InvokeWinzorDispatcherAsync(() =>
		{
			toolStripMenuItem.CheckOnClick = false;
			toolStripMenuItem.PerformClick();
		});
		Assert.That(toolStripMenuItem.Checked, Is.True);
	}

	[Test]
	public async Task ToolStripMenuItemProcessMnemonicSelectsFirstControl()
	{
		using var ctx = new WinzorTestContext();
		ctx.MockCargoWiseClientServices.MenuDisplayer.Setup(m => m.SendShowMenuRequestAsync(It.IsAny<MenuInteropModel>(), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()));
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			var menuItem = new ToolStripMenuItem() { Text = "&Test" };
			menuItem.DropDownItems.Add(new ToolStripMenuItem { Text = "Sub Menu Item" });
			toolStrip.Items.Add(menuItem);
			form.Controls.Add(toolStrip);
			return form;
		});

		var menuItem = rendered.Find(".toolstrip__menuitem");
		await rendered.KeyPressAsync(Keys.Alt | Keys.T, menuItem);
		ctx.MockCargoWiseClientServices.MenuDisplayer.Verify(f => f.SendShowMenuRequestAsync(It.IsAny<MenuInteropModel>(), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()), Times.Once());
	}

	[Test]
	public async Task ToolStripMenuItemUseMnemonicFormatsTestWithMnemonic()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			var menuItem = new ToolStripMenuItem() { Text = "&Test" };
			toolStrip.Items.Add(menuItem);
			form.Controls.Add(toolStrip);
			return form;
		});
		Assert.That(rendered.Find(".toolstrip__menuitem-text").InnerHtml, Is.EqualTo("<span class=\"mnemonickey\">T</span>est"));
	}

	[Test]
	public async Task ToolStripMenuItemDropdownItemsWithMnemonic()
	{
		using var ctx = new WinzorTestContext();
		var clicked = false;
		ToolStripMenuItem toolStripMenuItem1 = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			var menuItem = new ToolStripMenuItem { Text = "&Main" };
			toolStripMenuItem1 = new ToolStripMenuItem() { Text = "&Test" };
			toolStripMenuItem1.Click += (sender, args) => clicked = true;
			menuItem.DropDownItems.Add(toolStripMenuItem1);
			toolStrip.Items.Add(menuItem);
			form.Controls.Add(toolStrip);
			return form;
		});

		Assert.That(clicked, Is.False);
		var toolStripMenuItem = rendered.Find(".toolstrip__menuitem");
		await rendered.KeyPressAsync(Keys.Alt | Keys.T, toolStripMenuItem);
		Assert.That(clicked, Is.False);

		await rendered.KeyPressAsync(Keys.Alt | Keys.M, toolStripMenuItem);
		await rendered.KeyPressAsync(Keys.Alt | Keys.T, toolStripMenuItem);
		Assert.That(clicked, Is.True);
	}
}
