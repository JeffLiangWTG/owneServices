using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using CargoWise.Blazor.Client.Integration.Menus;
using Microsoft.AspNetCore.Components.Rendering;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WinzorTestFramework;

namespace System.Windows.Forms;
[SuppressMessage("CargoWiseOne", "CW1093:DoNotUseSystemWindowsFormsToolStripControls", Justification = "Testing")]
class ContextMenuTest
{
	TestMenuDisplayer menuDisplayer;
	CargoWiseClientServices clientServices;

	[SetUp]
	public void Setup()
	{
		menuDisplayer = new TestMenuDisplayer();
		clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: menuDisplayer);
	}
	[Test] public async Task TextBoxContextMenuStripContactsClientApp() => await ControlContactsClientApp<TextBox>("input", true);
	[Test] public async Task LabelContextMenuStripContactsClientApp() => await ControlContactsClientApp<Label>(".label", true);
	[Test] public async Task CheckBoxContextMenuStripContactsClientApp() => await ControlContactsClientApp<CheckBox>("label", true);
	[Test] public async Task GroupBoxContextMenuStripContactsClientApp() => await ControlContactsClientApp<GroupBox>(".groupbox", true);
	[Test] public async Task PictureBoxContextMenuStripContactsClientApp() => await ControlContactsClientApp<PictureBox>(".picturebox", true, (t) => t.Image = PictureBoxTest.pngImage.ToImage());
	[Test] public async Task ToolStripContextMenuStripContactsClientApp() => await ControlContactsClientApp<ToolStrip>(".toolstrip", true);
	[Test] public async Task ToolStripButtonContextMenuStripContactsClientApp() => await ControlContactsClientApp<ToolStripButton>(".toolstrip-item--button .button__button", true);
	[Test] public async Task StatusBarContextMenuStripContactsClientApp() => await ControlContactsClientApp<StatusBar>(".statusbar", true);
	[Test] public async Task StatusBarPanelContextMenuStripContactsClientApp() => await ControlContactsClientApp<StatusBarPanel>(".statusbar__panel", true);
	[Test] public async Task DataGridCellContextMenuStripContactsClientApp() => await ControlContactsClientApp<DataGrid>("td", true, dataGrid => SetupDataGrid(dataGrid));

	[Test] public async Task TextBoxContextMenuContactsClientApp() => await ControlContactsClientApp<TextBox>("input", false);
	[Test] public async Task LabelContextMenuContactsClientApp() => await ControlContactsClientApp<Label>(".label", false);
	[Test] public async Task CheckBoxContextMenuContactsClientApp() => await ControlContactsClientApp<CheckBox>("label", false);
	[Test] public async Task GroupBoxContextMenuContactsClientApp() => await ControlContactsClientApp<GroupBox>(".groupbox", false);
	[Test] public async Task PictureBoxContextMenuContactsClientApp() => await ControlContactsClientApp<PictureBox>(".picturebox", false, (t) => t.Image = PictureBoxTest.pngImage.ToImage());
	[Test] public async Task ToolStripContextMenuContactsClientApp() => await ControlContactsClientApp<ToolStrip>(".toolstrip", false);
	[Test] public async Task ToolStripButtonContextMenuContactsClientApp() => await ControlContactsClientApp<ToolStripButton>(".toolstrip-item--button .button__button", false);
	[Test] public async Task StatusBarContextMenuContactsClientApp() => await ControlContactsClientApp<StatusBar>(".statusbar", false);
	[Test] public async Task StatusBarPanelContextMenuContactsClientApp() => await ControlContactsClientApp<StatusBarPanel>(".statusbar__panel", false);
	[Test] public async Task DataGridCellContextMenuContactsClientApp() => await ControlContactsClientApp<DataGrid>("td", false, dataGrid => SetupDataGrid(dataGrid));

	async Task ControlContactsClientApp<T>(string cssSelector, bool setContextMenuStripInsteadOfContextMenu, Action<T> callBack = null) where T : Control, new()
	{
		var mockMenuDisplayer = new Mock<IMenuDisplayer>();
		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: mockMenuDisplayer.Object);
		using var ctx = new WinzorTestContext();

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var ctrl = new T();
			callBack?.Invoke(ctrl);
			if (setContextMenuStripInsteadOfContextMenu)
			{
				ctrl.ContextMenuStrip = new ContextMenuStrip();
			}
			else
			{
				ctrl.ContextMenu = new ContextMenu();
			}
			if (ctrl is ToolStripItem)
			{
				var toolStrip = new ToolStrip();
				toolStrip.Items.Add(ctrl as ToolStripItem);
				form.Controls.Add(toolStrip);
			}
			else
			{
				form.Controls.Add(ctrl);
			}
			return form;
		}, clientServices);

		await rendered.Find(cssSelector).TriggerEventAsync("oncontextmenu", new WebMouseEventArgs { Button = 2, Type = "contextmenu" });

		mockMenuDisplayer.Verify(o => o.SendShowMenuRequestAsync(It.IsAny<MenuInteropModel>(), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()), Times.Once());
	}

	void SetupDataGrid(DataGrid dataGrid)
	{
		var table = new DataTable("data");
		var column = new DataColumn("column");
		column.DataType = typeof(string);
		table.Columns.Add(column);
		table.Rows.Add("data");
		dataGrid.Dock = DockStyle.Fill;
		dataGrid.DataSource = table;
		dataGrid.AllowNavigation = false;
		var tableStyle = new DataGridTableStyle { MappingName = "data" };
		var columnStyle = new DataGridTextBoxColumn { MappingName = "column", HeaderText = "Column Header" };
		columnStyle.Width = 100;
		tableStyle.GridColumnStyles.Add(columnStyle);
		dataGrid.TableStyles.Add(tableStyle);
	}

	[Test]
	public async Task ContextMenuStripLoadFixedSubmenuItemsRepeatedly()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var textBox = new TextBox();
			textBox.ContextMenuStrip = new ContextMenuStrip();
			var test = new ToolStripMenuItem("Test");
			textBox.ContextMenuStrip.Items.Add(test);
			test.DropDownItems.Add(new ToolStripMenuItem("Item 1"));
			test.DropDownItems.Add(new ToolStripMenuItem("Item 2"));
			form.Controls.Add(textBox);
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);
		await rendered.Find("input").TriggerEventAsync("oncontextmenu", new WebMouseEventArgs { Button = 2, Type = "contextmenu" });
		var testMenu = menuDisplayer.Menu.MenuItems.Single(m => m.Text == "Test");
		var subMenuInterop = await menuDisplayer.LoadSubMenuCallback(new SubMenuLoadRequest(menuDisplayer.Menu.Id.Value, testMenu.Id.Value));
		Assert.That(subMenuInterop.Select(o => o.Text), Is.EquivalentTo(new[] { "Item 1", "Item 2" }));
		var subMenuInterop2 = await menuDisplayer.LoadSubMenuCallback(new SubMenuLoadRequest(menuDisplayer.Menu.Id.Value, testMenu.Id.Value));
		Assert.That(subMenuInterop2.Select(o => o.Text), Is.EquivalentTo(new[] { "Item 1", "Item 2" }));
	}

	[Test]
	public async Task ContextMenuStripLoadDynamicMenuItem()
	{
		var dynamicClicked = false;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var textBox = new TextBox();
			textBox.ContextMenuStrip = new ContextMenuStrip();
			var test = new ToolStripMenuItem("Whatever");
			textBox.ContextMenuStrip.Items.Add(test);
			textBox.ContextMenuStrip.Opening += (s, e) =>
			{
				var dynamic = new ToolStripMenuItem("Dynamic");
				dynamic.Click += (s, e) => dynamicClicked = true;
				textBox.ContextMenuStrip.Items.Add(dynamic);
			};
			form.Controls.Add(textBox);
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);
		await rendered.Find("input").TriggerEventAsync("oncontextmenu", new WebMouseEventArgs { Button = 2, Type = "contextmenu" });
		Assert.That(menuDisplayer.Menu.MenuItems.Select(o => o.Text).ToArray(), Is.EqualTo(new[] { "Whatever", "Dynamic" }));
		await menuDisplayer.MenuClosedCallback(new MenuClosedResult(MenuClosedResultCode.SelectionMade, menuDisplayer.Menu.Id.Value, menuDisplayer.Menu.MenuItems.Single(m => m.Text == "Dynamic").Id.Value));
		Assert.That(dynamicClicked, Is.True);
	}

	[Test]
	public async Task ContextMenuStripLoadDynamicSubmenuItem()
	{
		var dynamicClicked = false;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var textBox = new TextBox();
			textBox.ContextMenuStrip = new ContextMenuStrip();
			var test = new ToolStripMenuItem("Test");
			textBox.ContextMenuStrip.Items.Add(test);
			test.DropDownItems.Add(new ToolStripMenuItem("Placeholder"));
			test.DropDown.Opening += (s, e) =>
			{
				test.DropDownItems.Clear();
				var dynamic = new ToolStripMenuItem("Dynamic");
				test.DropDownItems.Add(dynamic);
				dynamic.Click += (s, e) => dynamicClicked = true;
			};
			form.Controls.Add(textBox);
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);
		await rendered.Find("input").TriggerEventAsync("oncontextmenu", new WebMouseEventArgs { Button = 2, Type = "contextmenu" });
		var testMenu = menuDisplayer.Menu.MenuItems.Single(m => m.Text == "Test");
		Assert.That(testMenu.Loadable, Is.True);
		var subMenuInterop = await menuDisplayer.LoadSubMenuCallback(new SubMenuLoadRequest(menuDisplayer.Menu.Id.Value, testMenu.Id.Value));
		Assert.That(subMenuInterop.Select(o => o.Text), Is.EquivalentTo(new[] { "Dynamic" }));
		await menuDisplayer.MenuClosedCallback(new MenuClosedResult(MenuClosedResultCode.SelectionMade, menuDisplayer.Menu.Id.Value, subMenuInterop.Single().Id.Value));
		Assert.That(dynamicClicked, Is.True);
	}

	[Test]
	public async Task ContextMenuStripLoadDynamicSubmenuItemViaDropDownItems()
	{
		var dynamicClicked = false;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var textBox = new TextBox();
			textBox.ContextMenuStrip = new ContextMenuStrip();
			var test = new ToolStripMenuItem("Test");
			textBox.ContextMenuStrip.Items.Add(test);
			test.DropDownItems.Add(new ToolStripMenuItem("Placeholder"));
			test.DropDown.Opening += (s, e) =>
			{
				test.DropDownItems.Clear();
				var dynamic = new ToolStripMenuItem("Dynamic");
				test.DropDown.Items.Add(dynamic);
				dynamic.Click += (s, e) => dynamicClicked = true;
			};
			form.Controls.Add(textBox);
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);
		await rendered.Find("input").TriggerEventAsync("oncontextmenu", new WebMouseEventArgs { Button = 2, Type = "contextmenu" });
		var testMenu = menuDisplayer.Menu.MenuItems.Single(m => m.Text == "Test");
		Assert.That(testMenu.Loadable, Is.True);
		var subMenuInterop = await menuDisplayer.LoadSubMenuCallback(new SubMenuLoadRequest(menuDisplayer.Menu.Id.Value, testMenu.Id.Value));
		Assert.That(subMenuInterop.Select(o => o.Text), Is.EquivalentTo(new[] { "Dynamic" }));
		await menuDisplayer.MenuClosedCallback(new MenuClosedResult(MenuClosedResultCode.SelectionMade, menuDisplayer.Menu.Id.Value, subMenuInterop.Single().Id.Value));
		Assert.That(dynamicClicked, Is.True);
	}

	[Test]
	public async Task ContextMenuStripLoadDynamicSubmenuItemWithExisting()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var textBox = new TextBox();
			textBox.ContextMenuStrip = new ContextMenuStrip();
			var test = new ToolStripMenuItem("Test");
			textBox.ContextMenuStrip.Items.Add(test);
			test.DropDownItems.Add(new ToolStripMenuItem("Permanent"));
			test.DropDown.Opening += (s, e) =>
			{
				var dynamic = new ToolStripMenuItem("Dynamic");
				test.DropDownItems.Add(dynamic);
			};
			form.Controls.Add(textBox);
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);
		await rendered.Find("input").TriggerEventAsync("oncontextmenu", new WebMouseEventArgs { Button = 2, Type = "contextmenu" });
		var testMenu = menuDisplayer.Menu.MenuItems.Single(m => m.Text == "Test");
		var subMenuInterop = await menuDisplayer.LoadSubMenuCallback(new SubMenuLoadRequest(menuDisplayer.Menu.Id.Value, testMenu.Id.Value));
		Assert.That(subMenuInterop.Select(o => o.Text), Is.EquivalentTo(new[] { "Permanent", "Dynamic" }));
		Assert.That(subMenuInterop.Single(m => m.Text == "Permanent").Id, Is.EqualTo(testMenu.SubMenuItems.Single(m => m.Text == "Permanent").Id));
	}

	[Test]
	public async Task ContextMenuStripNoDynamicSubmenuItems()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var textBox = new TextBox();
			textBox.ContextMenuStrip = new ContextMenuStrip();
			var test = new ToolStripMenuItem("Test");
			textBox.ContextMenuStrip.Items.Add(test);
			test.DropDownItems.Add(new ToolStripMenuItem("Whatever"));
			test.DropDown.Opening += (s, e) =>
			{
			};
			form.Controls.Add(textBox);
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);
		await rendered.Find("input").TriggerEventAsync("oncontextmenu", new WebMouseEventArgs { Button = 2, Type = "contextmenu" });
		var testMenu = menuDisplayer.Menu.MenuItems.Single(m => m.Text == "Test");
		var subMenuInterop = await menuDisplayer.LoadSubMenuCallback(new SubMenuLoadRequest(menuDisplayer.Menu.Id.Value, testMenu.Id.Value));
		Assert.That(subMenuInterop.Select(o => o.Text), Is.EqualTo(new[] { "Whatever" }));
	}

	[Test]
	public async Task ContextMenuStripNoExceptionsThrownOnSubMenuSelect()
	{
		using var ctx = new WinzorTestContext();
		Exception exceptionThrown = null;
		ctx.ThreadExceptionExceptionRaised += ex =>
		{
			exceptionThrown = ex;
			return true;
		};

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var textBox = new TextBox();
			textBox.ContextMenuStrip = new ContextMenuStrip();
			var test = new ToolStripMenuItem("Test");
			textBox.ContextMenuStrip.Items.Add(test);
			test.DropDownItems.Add(new ToolStripMenuItem("Whatever"));
			form.Controls.Add(textBox);
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);
		await rendered.Find("input").TriggerEventAsync("oncontextmenu", new WebMouseEventArgs { Button = 2, Type = "contextmenu" });
		var testMenu = menuDisplayer.Menu.MenuItems.Single(m => m.Text == "Test");
		var subMenuInterop = await menuDisplayer.LoadSubMenuCallback(new SubMenuLoadRequest(menuDisplayer.Menu.Id.Value, testMenu.Id.Value));

		Assert.That(exceptionThrown, Is.Null);
	}

	[Test]
	public async Task ContextMenuStripNonVisibleMenuItem()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var textBox = new TextBox();
			textBox.ContextMenuStrip = new ContextMenuStrip();
			var test = new ToolStripMenuItem("Test");
			textBox.ContextMenuStrip.Items.Add(test);
			test.DropDownItems.Add(new ToolStripMenuItem { Text = "Invisible", Visible = false });
			test.DropDownItems.Add(new ToolStripMenuItem { Text = "Visible", Visible = true });
			form.Controls.Add(textBox);
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);
		await rendered.Find("input").TriggerEventAsync("oncontextmenu", new WebMouseEventArgs { Button = 2, Type = "contextmenu" });
		var testMenu = menuDisplayer.Menu.MenuItems.Single(m => m.Text == "Test");
		Assert.That(testMenu.SubMenuItems.Select(m => m.Text), Is.EquivalentTo(new[] { "Visible" }));
	}

	[Test]
	public async Task ContextMenuStripSetVisibleFalseOnOpening()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var textBox = new TextBox();
			textBox.ContextMenuStrip = new ContextMenuStrip();
			var test = new ToolStripMenuItem("Test");
			textBox.ContextMenuStrip.Items.Add(test);
			var invisible = new ToolStripMenuItem("Invisible");
			test.DropDownItems.Add(invisible);
			test.DropDownItems.Add(new ToolStripMenuItem("Visible"));
			test.DropDown.Opening += (s, e) =>
			{
				invisible.Visible = false;
			};
			form.Controls.Add(textBox);
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);
		await rendered.Find("input").TriggerEventAsync("oncontextmenu", new WebMouseEventArgs { Button = 2, Type = "contextmenu" });
		var testMenu = menuDisplayer.Menu.MenuItems.Single(m => m.Text == "Test");
		var subMenuInterop = await menuDisplayer.LoadSubMenuCallback(new SubMenuLoadRequest(menuDisplayer.Menu.Id.Value, testMenu.Id.Value));
		Assert.That(subMenuInterop.Select(o => o.Text), Is.EquivalentTo(new[] { "Visible" }));
	}

	[Test]
	public async Task ContextMenuLoadDynamicMenuItem()
	{
		var dynamicClicked = false;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var textBox = new TextBox();
			textBox.ContextMenu = new ContextMenu();
			textBox.ContextMenu.MenuItems.Add("Whatever");
			textBox.ContextMenu.Popup += (s, e) =>
			{
				var dynamic = textBox.ContextMenu.MenuItems.Add("Dynamic");
				dynamic.Click += (s, e) => dynamicClicked = true;
			};
			form.Controls.Add(textBox);
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);
		await rendered.Find("input").TriggerEventAsync("oncontextmenu", new WebMouseEventArgs { Button = 2, Type = "contextmenu" });
		Assert.That(menuDisplayer.Menu.MenuItems.Select(o => o.Text).ToArray(), Is.EqualTo(new[] { "Whatever", "Dynamic" }));
		await menuDisplayer.MenuClosedCallback(new MenuClosedResult(MenuClosedResultCode.SelectionMade, menuDisplayer.Menu.Id.Value, menuDisplayer.Menu.MenuItems.Single(m => m.Text == "Dynamic").Id.Value));
		Assert.That(dynamicClicked, Is.True);
	}

	[Test]
	public async Task TestContextMenuStripShortCutKeys()
	{
		var dynamicClicked = false;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var textBox = new TextBox();
			textBox.ContextMenu = new ContextMenu();
			var contextMenu = textBox.ContextMenu.MenuItems.Add("Whatever");
			contextMenu.Click += (s, e) => dynamicClicked = true;
			contextMenu.Shortcut = Shortcut.CtrlH;
			form.Controls.Add(textBox);
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);
		var inputElement = rendered.Find("input");
		await rendered.KeyPressAsync(Keys.Control | Keys.H, inputElement);
		Assert.That(dynamicClicked, Is.True);
	}

	[Test]
	public async Task ContextMenuStripDisabledMenuItem()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var textBox = new TextBox();
			textBox.ContextMenuStrip = new ContextMenuStrip();
			textBox.ContextMenuStrip.Items.Add(new ToolStripMenuItem { Text = "Enabled" });
			textBox.ContextMenuStrip.Items.Add(new ToolStripMenuItem { Text = "Disabled", Enabled = false });
			form.Controls.Add(textBox);
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);
		await rendered.Find("input").TriggerEventAsync("oncontextmenu", new WebMouseEventArgs { Button = 2, Type = "contextmenu" });
		Assert.That(menuDisplayer.Menu.MenuItems.FirstOrDefault(m => m.Text == "Enabled").Enabled, Is.True);
		Assert.That(menuDisplayer.Menu.MenuItems.FirstOrDefault(m => m.Text == "Disabled").Enabled, Is.False);
	}

	[Test]
	public async Task ContextMenuDisabledMenuItem()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var textBox = new TextBox();
			textBox.ContextMenu = new ContextMenu();
			textBox.ContextMenu.MenuItems.Add(new MenuItem { Text = "Enabled" });
			textBox.ContextMenu.MenuItems.Add(new MenuItem { Text = "Disabled", Enabled = false });
			form.Controls.Add(textBox);
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);
		await rendered.Find("input").TriggerEventAsync("oncontextmenu", new WebMouseEventArgs { Button = 2, Type = "contextmenu" });
		Assert.That(menuDisplayer.Menu.MenuItems.FirstOrDefault(m => m.Text == "Enabled").Enabled, Is.True);
		Assert.That(menuDisplayer.Menu.MenuItems.FirstOrDefault(m => m.Text == "Disabled").Enabled, Is.False);
	}

	[Test]
	public async Task ContextMenuStripToolStripSeparatorTextAsHyphen()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var textBox = new TextBox();
			textBox.ContextMenuStrip = new ContextMenuStrip();
			textBox.ContextMenuStrip.Items.Add(new ToolStripMenuItem("-"));
			textBox.ContextMenuStrip.Items.Add(new ToolStripSeparator());
			form.Controls.Add(textBox);
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);
		await rendered.Find("input").TriggerEventAsync("oncontextmenu", new WebMouseEventArgs { Button = 2, Type = "contextmenu" });
		Assert.That(menuDisplayer.Menu.MenuItems.Where(m => m.IsSeparator).Count, Is.EqualTo(2));
	}

	[Test]
	public async Task ContextMenuParentIsNull()
	{
		using var ctx = new WinzorTestContext();
		TextBox textBox = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			textBox = new TextBox();
			textBox.ContextMenu = new ContextMenu();
			form.Controls.Add(textBox);
			return form;
		});
		await rendered.Find("input").TriggerEventAsync("oncontextmenu", new WebMouseEventArgs { Button = 2, Type = "contextmenu" });
		Assert.That(textBox.ContextMenu.Parent, Is.Null);
	}

	[Test]
	public async Task ContextMenuStripParentIsNull()
	{
		using var ctx = new WinzorTestContext();
		TextBox textBox = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			textBox = new TextBox();
			textBox.ContextMenuStrip = new ContextMenuStrip();
			form.Controls.Add(textBox);
			return form;
		});
		await rendered.Find("input").TriggerEventAsync("oncontextmenu", new WebMouseEventArgs { Button = 2, Type = "contextmenu" });
		Assert.That(textBox.ContextMenuStrip.Parent, Is.Null);
	}

	[Test]
	public async Task ContextMenuInvokesOnCollapseOnClosed()
	{
		using var ctx = new WinzorTestContext();
		TextBox textBox = null;
		var onCollapseCalled = false;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			textBox = new TextBox();
			textBox.ContextMenu = new ContextMenu();
			textBox.ContextMenu.Collapse += (sender, args) => onCollapseCalled = true;
			form.Controls.Add(textBox);
			return form;
		}, clientServices);
		await rendered.Find("input").TriggerEventAsync("oncontextmenu", new WebMouseEventArgs { Button = 2, Type = "contextmenu" });
		Assert.That(onCollapseCalled, Is.False);
		await menuDisplayer.MenuClosedCallback(new MenuClosedResult(MenuClosedResultCode.NoSelection, Guid.Empty, null));
		Assert.That(onCollapseCalled, Is.True);
	}

	[Test]
	public async Task ClosedContextMenuDoesNotReopenOnMenuItemAddOrRemove()
	{
		using var ctx = new WinzorTestContext();
		TextBox textBox = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			textBox = new TextBox();
			textBox.ContextMenu = new ContextMenu();
			form.Controls.Add(textBox);
			return form;
		}, clientServices);

		await rendered.Find("input").TriggerEventAsync("oncontextmenu", new WebMouseEventArgs { Button = 2, Type = "contextmenu" });
		await menuDisplayer.MenuClosedCallback(new MenuClosedResult(MenuClosedResultCode.NoSelection, Guid.Empty, null));
		await rendered.Find("input").TriggerEventAsync("onmousedown", new WebMouseEventArgs()); //ContextMenu is now closed

		await textBox.InvokeWinzorDispatcherAsync(() => textBox.ContextMenu.MenuItems.Add(new MenuItem("Test")));
		await textBox.InvokeWinzorDispatcherAsync(() => textBox.ContextMenu.MenuItems.RemoveAt(0)); //Add/Remove triggers ContextMenu.OnMenuChanged

		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(1));
	}

	[Test]
	public async Task ContextMenuDoesNotOpenOnMouseDownRightClick()
	{
		using var ctx = new WinzorTestContext();
		ControlWithMouseDownAttribute control = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			control = new ControlWithMouseDownAttribute();
			control.ContextMenu = new ContextMenu();
			form.Controls.Add(control);
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);
		await rendered.Find(".control").MouseDownAsync(new WebMouseEventArgs { Button = 2 });
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(0));
	}

	[Test]
	public async Task ContextMenuStripSourceControlIsSetOnOpen()
	{
		using var ctx = new WinzorTestContext();
		TextBox textBox = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			textBox = new TextBox();
			textBox.ContextMenuStrip = new ContextMenuStrip();
			form.Controls.Add(textBox);
			return form;
		});
		Assert.That(textBox.ContextMenuStrip.SourceControl, Is.Null);
		await rendered.Find("input").TriggerEventAsync("oncontextmenu", new WebMouseEventArgs { Button = 2, Type = "contextmenu" });
		Assert.That(textBox.ContextMenuStrip.SourceControl, Is.EqualTo(textBox));
		await textBox.InvokeWinzorDispatcherAsync(() => textBox.ContextMenuStrip.Close());
		Assert.That(textBox.ContextMenuStrip.SourceControl, Is.Null);
	}

	[Test]
	public async Task Test_MenuChanged_IsTriggered_OnContextMenu_WhenMenuItemTextChanged()
	{
		using var ctx = new WinzorTestContext();
		MenuItem menuItem = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var textBox = new TextBox();
			textBox.ContextMenu = new ContextMenu();

			menuItem = new MenuItem("Initial Value", (s, e) => { menuItem.Text = "Changed Value"; }, Shortcut.CtrlH);
			textBox.ContextMenu.MenuItems.Add(menuItem);

			form.Controls.Add(textBox);
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);

		var inputElement = rendered.Find("input");
		await inputElement.TriggerEventAsync("oncontextmenu", new WebMouseEventArgs { Button = 2, Type = "contextmenu" });
		await rendered.KeyPressAsync(Keys.Control | Keys.H, inputElement);
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(2));
	}

	[Test]
	public async Task MenuChangedTriggeredOnceWhenMenuItemTextChangedAfterShowingMenuMultipleTimes()
	{
		using var ctx = new WinzorTestContext();
		MenuItem menuItem = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var textBox = new TextBox();
			textBox.ContextMenu = new ContextMenu();

			menuItem = new MenuItem("Initial Value", (s, e) => { menuItem.Text = "Changed Value"; }, Shortcut.CtrlH);
			textBox.ContextMenu.MenuItems.Add(menuItem);

			form.Controls.Add(textBox);
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);

		var inputElement = rendered.Find("input");
		await inputElement.TriggerEventAsync("oncontextmenu", new WebMouseEventArgs { Button = 2, Type = "contextmenu" });
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(1));
		await inputElement.TriggerEventAsync("oncontextmenu", new WebMouseEventArgs { Button = 2, Type = "contextmenu" });
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(2));
		await rendered.KeyPressAsync(Keys.Control | Keys.H, inputElement);
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(3));
	}

	[Test]
	public async Task ContextMenuStripClosedEventRaisedWhenParentControlDisposedByMenuAction()
	{
		var contextMenuStripClosedEventRaised = false;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var textBox = new TextBox();
			textBox.ContextMenuStrip = new ContextMenuStrip();
			textBox.ContextMenuStrip.Closed += (s, e) => contextMenuStripClosedEventRaised = true;
			var menuItem = new ToolStripMenuItem("Foo");
			menuItem.Click += (s, e) =>
			{
				textBox.Parent = null;
				textBox.Dispose();
			};
			textBox.ContextMenuStrip.Items.Add(menuItem);
			form.Controls.Add(textBox);
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);
		await rendered.Find("input").TriggerEventAsync("oncontextmenu", new WebMouseEventArgs { Button = 2, Type = "contextmenu" });
		await menuDisplayer.MenuClosedCallback(new MenuClosedResult(MenuClosedResultCode.SelectionMade, menuDisplayer.Menu.Id.Value, menuDisplayer.Menu.MenuItems.Single(m => m.Text == "Foo").Id.Value));
		Assert.That(contextMenuStripClosedEventRaised, Is.True);
	}

	[Test]
	public async Task AddOrRemoveItemInPopupWillNotCauseMultipleShowRequest()
	{
		using var ctx = new WinzorTestContext();

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var textBox = new TextBox();
			var menuItemToBeRemoved = new MenuItem("Item to be removed");
			var addedItem = new MenuItem("Added Item");
			textBox.ContextMenu = new ContextMenu();
			textBox.ContextMenu.MenuItems.Add(new MenuItem("Static Item"));
			textBox.ContextMenu.MenuItems.Add(menuItemToBeRemoved);

			void OnPopupContextMenu(object sender, EventArgs e)
			{
				textBox.ContextMenu.MenuItems.Add(addedItem);
				textBox.ContextMenu.MenuItems.Remove(menuItemToBeRemoved);
			}

			textBox.ContextMenu.Popup += OnPopupContextMenu;

			form.Controls.Add(textBox);
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);

		var inputElement = rendered.Find("input");
		await inputElement.TriggerEventAsync("oncontextmenu", new WebMouseEventArgs { Button = 2, Type = "contextmenu" });
		await inputElement.TriggerEventAsync("oncontextmenu", new WebMouseEventArgs { Button = 2, Type = "contextmenu" });
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(2));
	}

	[Test]
	public async Task RerenderContextMenuAfterRootMenuItemsAreUpdated()
	{
		using var ctx = new WinzorTestContext();
		ContextMenu contextMenu = null;
		MenuItem test = null;
		Form form = null;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			var textBox = new TextBox();
			contextMenu = textBox.ContextMenu = new ContextMenu();
			test = textBox.ContextMenu.MenuItems.Add("Test");
			form.Controls.Add(textBox);
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);

		var inputElement = rendered.Find("input");
		await inputElement.TriggerEventAsync("oncontextmenu", new WebMouseEventArgs { Button = 2, Type = "contextmenu" });
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(1));

		var testMenu = menuDisplayer.Menu.MenuItems.Single(m => m.Text == "Test");
		Assert.That(testMenu.Enabled, Is.True);

		await test.InvokeWinzorDispatcherAsync(() =>
		{
			test.Enabled = false;
		});
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(2));
		rendered.WaitForState(() => !menuDisplayer.Menu.MenuItems.Single(m => m.Text == "Test").Enabled);
		await test.InvokeWinzorDispatcherAsync(() =>
		{
			test.Text = "Text Changed";
		});
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(3));
		rendered.WaitForState(() => menuDisplayer.Menu.MenuItems.Single(m => m.Text == "Text Changed") != null);

		MenuItem added = null;
		MenuItem[] subItems = null;
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			subItems = new MenuItem[2];
			subItems[0] = new MenuItem("Sub Menu Test 1")
			{
				Enabled = false,
			};
			subItems[1] = new MenuItem("Sub Menu Test 2");
			added = contextMenu.MenuItems.Add("Add", subItems);
		});
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(4));
		rendered.WaitForState(() => menuDisplayer.Menu.MenuItems.Length == 2);
		Assert.That(menuDisplayer.Menu.MenuItems.Select(o => o.Text), Is.EquivalentTo(new[] { "Text Changed", "Add" }));
		Assert.That(menuDisplayer.Menu.MenuItems[1].SubMenuItems[0].Enabled, Is.EqualTo(false));

		await subItems[0].InvokeWinzorDispatcherAsync(() =>
		{
			subItems[0].Enabled = true;
		});
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(4));

		await subItems[1].InvokeWinzorDispatcherAsync(() =>
		{
			subItems[1].Text = "Sub Menu Test Change";
		});
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(4));

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			contextMenu.MenuItems.Remove(added);
		});
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(5));
		rendered.WaitForState(() => menuDisplayer.Menu.MenuItems.Length == 1);
		Assert.That(menuDisplayer.Menu.MenuItems.Select(o => o.Text), Is.EquivalentTo(new[] { "Text Changed" }));
	}

	[Test]
	public async Task TopLevelContextMenuNotRerenderedWhenSubmenuChanges()
	{
		using var ctx = new WinzorTestContext();
		MenuItem test = null;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var textBox = new TextBox();
			textBox.ContextMenu = new ContextMenu();
			test = textBox.ContextMenu.MenuItems.Add("Test");
			form.Controls.Add(textBox);
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);

		var inputElement = rendered.Find("input");
		await inputElement.TriggerEventAsync("oncontextmenu", new WebMouseEventArgs { Button = 2, Type = "contextmenu" });
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(1));

		await test.InvokeWinzorDispatcherAsync(() =>
		{
			test.MenuItems.Add("Child");
		});
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(1));

		await test.InvokeWinzorDispatcherAsync(() =>
		{
			test.MenuItems.Single().Text = "Child Changed";
		});
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(1));

		await test.InvokeWinzorDispatcherAsync(() =>
		{
			test.MenuItems.Single().Enabled = false;
		});
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(1));

		await test.InvokeWinzorDispatcherAsync(() =>
		{
			test.MenuItems.Single().Visible = false;
		});
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(1));

		await test.InvokeWinzorDispatcherAsync(() =>
		{
			test.MenuItems.Single().Visible = true;
		});
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(1));

		await test.InvokeWinzorDispatcherAsync(() =>
		{
			test.MenuItems.Clear();
		});
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(1));
	}

	[Test]
	public async Task RepeatedlyShowingContextMenuDoesNotThrowException()
	{
		var mockMenuDisplayer = new Mock<IMenuDisplayer>();
		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: mockMenuDisplayer.Object);
		using var ctx = new WinzorTestContext();

		TextBox textbox = null;
		ContextMenu menu = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();

			textbox = new TextBox();
			form.Controls.Add(textbox);

			menu = new ContextMenu();
			textbox.ContextMenu = menu;

			return form;
		});

		await menu.InvokeWinzorDispatcherAsync(() =>
		{
			for (var i = 0; i < 20; i++)
			{
				Assert.DoesNotThrow(() =>
					menu.Show(textbox, new Point(0, 0)),
					$"Failed on iteration {i}");
			}
		});
	}

	class ControlWithMouseDownAttribute : Control
	{
		protected override void RenderFull(RenderTreeBuilder builder)
		{
			builder.OpenElement(0, "div");
			builder.AddAttribute(1, "class", "control");
			builder.AddAttribute(2, "onmousedown", OnMouseDownAsync);
			builder.CloseElement();
		}

		protected internal override bool ShouldRender => true;
	}
}
