using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.Blazor.Client.Integration.Menus;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorFramework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using static WTG.PlaywrightTesting.PlaywrightTestContext;

namespace Enterprise.Winzor.Architecture.Test;

class ZMainItemTest
{
	static readonly IEnumerable<string> editMenuItemsSource = new[] { "Cut", "Copy", "Paste" };
	static readonly IEnumerable<bool> multilineSource = new[] { true, false };

	[Test]
	public async Task EditMenuItems()
	{
		var menuDisplayer = new TestMenuDisplayer();
		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: menuDisplayer);
		using var ctx = new EnterpriseTestContext();
		await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Menu = new MainMenu();
			var edit = new ZEditMenuItem(form);
			form.Menu.MenuItems.Add(edit);
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);
		var editMenu = menuDisplayer.Menu.MenuItems.Single(m => m.Text == "&Edit");
		Assert.That(editMenu.SubMenuItems.Length, Is.EqualTo(3));
		Assert.That(editMenu.SubMenuItems[0].Text, Is.EqualTo("Cut"));
		Assert.That(editMenu.SubMenuItems[1].Text, Is.EqualTo("Copy"));
		Assert.That(editMenu.SubMenuItems[2].Text, Is.EqualTo("Paste"));
	}

	[TestCaseSource(nameof(editMenuItemsSource))]
	public async Task EditMenuActions(string action)
	{
		using var ctx = new EnterpriseTestContext();
		var menuDisplayer = new TestMenuDisplayer();

		var path = "/_content/WinzorFramework/js/module/clipboard.js";
		var module = ctx.JSInterop.SetupModule(path);

		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: menuDisplayer, jsRuntime: ctx.JSInterop.JSRuntime);
		Form form = null;
		await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			form.Menu = new MainMenu();
			var edit = new ZEditMenuItem(form);
			form.Menu.MenuItems.Add(edit);
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);
		var editMenu = menuDisplayer.Menu.MenuItems.Single(m => m.Text == "&Edit");
		var item = editMenu.SubMenuItems.Single(m => m.Text == action);
		var subMenuInterop = await menuDisplayer.LoadSubMenuCallback(new SubMenuLoadRequest(menuDisplayer.Menu.Id.Value, editMenu.Id.Value));
		var submenu = subMenuInterop.First(o => o.Text == action);
		await menuDisplayer.MenuClosedCallback(new MenuClosedResult(MenuClosedResultCode.SelectionMade, menuDisplayer.Menu.Id.Value, submenu.Id.Value));

		Assert.That(module.Invocations.Select(i => i.Identifier), Has.Exactly(1).EqualTo($"clipboard.{action.ToLowerInvariant()}"));
	}

	[TestCaseSource(nameof(multilineSource)), WithPlaywrightPage]
	public async Task EditMenuCopyPaste(bool multiline)
	{
		await using var ctx = new InMemoryAppServerTestContext();

		ZEditMenuItem edit = null;
		WinzorTestForm form = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			InitForm(multiline, out form, out edit);
			return form;
		});

		await BrowserContext.GrantPermissionsAsync(new[] { "clipboard-read", "clipboard-write" });
		
		await page.WaitForSelectorAsync(".form");
		var (fromInput, toInput, shortInput, readonlyInput) = await GetClientInputElementsAsync(page, multiline);
		var (fromTextBox, toTextBox, shortTextBox, readonlyTextBox) = GetFormTextBoxControls(form);

		await AssertTextBoxValueAsync("test", fromInput, fromTextBox);
		await fromInput.SelectTextAsync();
		await ClickMenuAsync(edit, "Copy");
		AssertClipBoard(page, "test");

		await toInput.SelectTextAsync();
		await ClickMenuAsync(edit, "Paste");
		await AssertTextBoxValueAsync("test", toInput, toTextBox);

		await readonlyInput.SelectTextAsync();
		await ClickMenuAsync(edit, "Paste");
		await AssertTextBoxValueAsync("readonly", readonlyInput, readonlyTextBox);

		await shortInput.SelectTextAsync();
		await ClickMenuAsync(edit, "Paste");
		await ClickMenuAsync(edit, "Paste");
		await AssertTextBoxValueAsync("testt", shortInput, shortTextBox);
	}

	[TestCaseSource(nameof(multilineSource)), WithPlaywrightPage]
	public async Task EditMenuCutPaste(bool multiline)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZEditMenuItem edit = null;
		WinzorTestForm form = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			InitForm(multiline, out form, out edit);
			return form;
		});

		await BrowserContext.GrantPermissionsAsync(new[] { "clipboard-read", "clipboard-write" });
		await page.WaitForSelectorAsync(".form");
		var (fromInput, toInput, shortInput, readonlyInput) = await GetClientInputElementsAsync(page, multiline);
		var (fromTextBox, toTextBox, shortTextBox, readonlyTextBox) = GetFormTextBoxControls(form);

		await AssertTextBoxValueAsync("test", fromInput, fromTextBox);
		await fromInput.SelectTextAsync();
		Assert.That(async () => (await toInput.GetPropertyAsync("isPasteOrCut")).ToString(), Is.EqualTo("undefined"));
		Assert.That(async () => (await fromInput.GetPropertyAsync("isPasteOrCut")).ToString(), Is.EqualTo("undefined"));
		await ClickMenuAsync(edit, "Cut");
		Assert.That(async () => (await fromInput.GetPropertyAsync("isPasteOrCut")).ToString(), Is.EqualTo("true"));
		AssertClipBoard(page, "test");

		await toInput.SelectTextAsync();
		await ClickMenuAsync(edit, "Paste");
		Assert.That(async () => (await toInput.GetPropertyAsync("isPasteOrCut")).ToString(), Is.EqualTo("true"));
		await AssertTextBoxValueAsync("test", toInput, toTextBox);

		await readonlyInput.SelectTextAsync();
		await ClickMenuAsync(edit, "Paste");
		await AssertTextBoxValueAsync("readonly", readonlyInput, readonlyTextBox);

		await readonlyInput.SelectTextAsync();
		await ClickMenuAsync(edit, "Cut");
		AssertClipBoard(page, "test");
		await AssertTextBoxValueAsync("readonly", readonlyInput, readonlyTextBox);

		await toInput.FillAsync("newtext");
		await AssertTextBoxValueAsync("newtext", toInput, toTextBox);
		await toInput.SelectTextAsync();
		await ClickMenuAsync(edit, "Paste");
		await AssertTextBoxValueAsync("test", toInput, toTextBox);

		await shortInput.SelectTextAsync();
		await ClickMenuAsync(edit, "Paste");
		await ClickMenuAsync(edit, "Paste");
		await AssertTextBoxValueAsync("testt", shortInput, shortTextBox);
	}

	[Test, WithPlaywrightPage, Explicit]
	public async Task DataGridChangeByCutAndPaste()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		ZEditMenuItem edit = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			edit = new ZEditMenuItem(form);
			var table = new DataTable("data");
			var column1 = new DataColumn("one");
			column1.DataType = typeof(string);
			table.Columns.Add(column1);

			var dataGrid = new DataGrid();
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnStyle1 = new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One Header" };
			columnStyle1.Width = 80;
			tableStyle.GridColumnStyles.Add(columnStyle1);
			dataGrid.TableStyles.Add(tableStyle);
			form.Controls.Add(dataGrid);

			return form;
		});
		await BrowserContext.GrantPermissionsAsync(new[] { "clipboard-read", "clipboard-write" });
		await page.WaitForSelectorAsync("table");
		var input = await page.WaitForSelectorAsync("td .textbox");

		await input.FillAsync("123");
		await page.WaitForTimeoutAsync(1000);
		await (await page.QuerySelectorAllAsync("tbody tr:nth-of-type(2) td"))[1].ClickAsync();
		await (await page.QuerySelectorAllAsync("tbody tr td"))[1].WaitForSelectorAsync(".textbox", options: new ElementHandleWaitForSelectorOptions() { State = WaitForSelectorState.Detached });
		await page.WaitForTimeoutAsync(1000);
		await (await page.QuerySelectorAllAsync("tbody tr td"))[1].ClickAsync();
		input = await page.WaitForSelectorAsync("td .textbox", options: new PageWaitForSelectorOptions() { State = WaitForSelectorState.Visible });
		await (await page.QuerySelectorAllAsync("tbody tr td"))[2].WaitForSelectorAsync(".textbox", options: new ElementHandleWaitForSelectorOptions() { State = WaitForSelectorState.Detached });

		Assert.That(await input.InputValueAsync(), Is.EqualTo("123"));
		await input.SelectTextAsync();
		await ClickMenuAsync(edit, "Cut");
		Assert.That(await input.InputValueAsync(), Is.Empty);
		Assert.That(async () => await (await page.QuerySelectorAllAsync("tbody tr td"))[1].TextContentAsync(), Is.EqualTo(string.Empty).After(2000, 200));
		Assert.That((await page.QuerySelectorAllAsync("tbody tr")).Count, Is.EqualTo(2));

		await (await page.QuerySelectorAllAsync("tbody tr td"))[1].ClickAsync();
		input = await page.WaitForSelectorAsync("td .textbox");
		await input.SelectTextAsync();
		await ClickMenuAsync(edit, "Paste");
		await input.EvaluateAsync("node => node.dispatchEvent(new Event('focusout'))");
		Assert.That(await input.InputValueAsync(), Is.EqualTo("123"));
		Assert.That(async () => await (await page.QuerySelectorAllAsync("tbody tr td"))[1].TextContentAsync(), Is.EqualTo("123").After(2000, 200));
		Assert.That((await page.QuerySelectorAllAsync("tbody tr")).Count, Is.EqualTo(2));
	}

	[Test, WithPlaywrightPage]
	[TestCase(1, 1, "2\t5\t9\r\n", TestName = "{m}_SingleRow")]
	[TestCase(1, 2, "2\t5\t9\r\n3\t5\t7\r\n", TestName = "{m}_MultiRows")]
	public async Task ZGridCopySelectedRows(int startRow, int endRow, string expected)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZEditMenuItem edit = null;
		WinzorTestForm form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var child1 = factory.New<DummyBaseBusinessObject>();
			child1.Z0_Code = "1";
			child1.Z0_Description = "6";
			child1.Z0_Decimal = 8;
			dummyBizo.Collection.Add(child1);
			var child2 = factory.New<DummyBaseBusinessObject>();
			child2.Z0_Code = "2";
			child2.Z0_Description = "5";
			child2.Z0_Decimal = 9;
			dummyBizo.Collection.Add(child2);
			var child3 = factory.New<DummyBaseBusinessObject>();
			child3.Z0_Code = "3";
			child3.Z0_Description = "5";
			child3.Z0_Decimal = 7;
			dummyBizo.Collection.Add(child3);

			var grid = new ZGrid { Width = 500, Height = 300 };
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 100));
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Description", 200));
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Decimal", 300));
			grid.LayoutCategoryPK = System.Guid.NewGuid();
			grid.ReadOnly = true;
			grid.IsWholeRowSelectedOnClick = true;

			form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			form.BindingSource.SetBindingMember(grid, "Collection");
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			form.SetDataBinding(dummyBizo, "");
			edit = new ZEditMenuItem(form);
			form.Controls.Add(edit);
			return form;
		});

		await page.WaitForSelectorAsync("table tbody tr");
		var trs = await page.QuerySelectorAllAsync("table tbody tr");
		Assert.That(trs.Count, Is.EqualTo(3), "grid rows are not fully rendered");

		var start = (await trs[startRow].QuerySelectorAllAsync("td"))[1];
		var end = (await trs[endRow].QuerySelectorAllAsync("td"))[1];
		await MouseMoveOfElementAsync(page, start, 2, 2);
		await MouseDownAsync(page);
		await MouseMoveOfElementAsync(page, end, 2, 12);
		await MouseUpAsync(page);
		await BrowserContext.GrantPermissionsAsync(new[] { "clipboard-write", "clipboard-read" });
		await form.InvokeWinzorDispatcherAsync(() => edit.MenuItems.Single(m => m.Text == "Copy").PerformClick());

		Assert.That(async () => await page.EvaluateAsync<string>("async () => await navigator.clipboard.readText()"), Is.EqualTo(expected).After(1000, 100));
	}

	[Test]
	public async Task CopyWhenMoreThanOneFocusedControl()
	{
		using var ctx = new EnterpriseTestContext();
		var menuDisplayer = new TestMenuDisplayer();

		var path = "/_content/WinzorFramework/js/module/clipboard.js";
		var module = ctx.JSInterop.SetupModule(path);

		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: menuDisplayer, jsRuntime: ctx.JSInterop.JSRuntime);
		Form form = null;
		await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			form.Menu = new MainMenu();
			var edit = new ZEditMenuItem(form);
			form.Menu.MenuItems.Add(edit);
			form.Controls.Add(new ControlWithFocusOverride());
			form.Controls.Add(new ControlWithFocusOverride());
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);
		var editMenu = menuDisplayer.Menu.MenuItems.Single(m => m.Text == "&Edit");
		var item = editMenu.SubMenuItems.Single(m => m.Text == "Copy");
		var subMenuInterop = await menuDisplayer.LoadSubMenuCallback(new SubMenuLoadRequest(menuDisplayer.Menu.Id.Value, editMenu.Id.Value));
		var submenu = subMenuInterop.First(o => o.Text == "Copy");
		await menuDisplayer.MenuClosedCallback(new MenuClosedResult(MenuClosedResultCode.SelectionMade, menuDisplayer.Menu.Id.Value, submenu.Id.Value));

		Assert.That(module.Invocations.Select(i => i.Identifier), Has.Exactly(1).EqualTo("clipboard.copy"));
	}

	class ControlWithFocusOverride : Control
	{
		public override bool Focused => true;
	}

	async Task MouseDownAsync(IPage page)
	{
		await page.Mouse.DownAsync();
		await Task.Delay(500);
	}

	async Task MouseMoveOfElementAsync(IPage page, IElementHandle elementHandle, int x, int y)
	{
		var elementRect = await elementHandle.BoundingBoxAsync();
		await page.Mouse.MoveAsync(elementRect.X + x, elementRect.Y + y);
		await Task.Delay(500);
	}

	async Task MouseUpAsync(IPage page)
	{
		await page.Mouse.UpAsync();
		await Task.Delay(500);
	}

	async Task ClickMenuAsync(ZEditMenuItem menu, string command)
	{
		await menu.InvokeWinzorDispatcherAsync(() => menu.MenuItems.Single(m => m.Text == command).PerformClick());
	}

	void AssertClipBoard(IPage page, string expected)
	{
		Assert.That(async () => await page.EvaluateAsync<string>("async () => await navigator.clipboard.readText()"), Is.EqualTo(expected).After(1000, 100));
	}

	async Task<(IElementHandle, IElementHandle, IElementHandle, IElementHandle)> GetClientInputElementsAsync(IPage page, bool isMultiline)
	{
		var inputs = await page.QuerySelectorAllAsync(isMultiline ? "textarea" : "input");
		return (inputs[0], inputs[1], inputs[2], inputs[3]);
	}

	(TextBox, TextBox, TextBox, TextBox) GetFormTextBoxControls(Form form)
	{
		var textBoxes = form.Controls.OfType<TextBox>().ToArray();
		return (textBoxes[0], textBoxes[1], textBoxes[2], textBoxes[3]);
	}

	async Task AssertTextBoxValueAsync(string text, IElementHandle textBoxElement, TextBox textBoxControl)
	{
		Assert.That(async () => await textBoxElement.InputValueAsync(), Is.EqualTo(text).After(3000, 100), "Value on the client was not equal");
		await textBoxElement.EvaluateAsync("e => e.blur()"); // Focus out of input to fire change event and update the value on the server
		Assert.That(() => textBoxControl.Text, Is.EqualTo(text).After(3000, 100), "Value on the server was not equal");
	}

	void InitForm(bool multiline, out WinzorTestForm form, out ZEditMenuItem menu)
	{
		form = new WinzorTestForm { Width = 1000, Height = 600 };
		menu = new ZEditMenuItem(form);
		form.Controls.Add(new ZTextBox() { Top = 100, Width = 100, Height = 20, CharacterCasing = CharacterCasing.Lower, Multiline = multiline, Text = "test" });
		form.Controls.Add(new ZTextBox() { Top = 200, Width = 100, Height = 20, CharacterCasing = CharacterCasing.Lower, Multiline = multiline });
		form.Controls.Add(new ZTextBox() { Top = 400, Width = 100, Height = 20, CharacterCasing = CharacterCasing.Lower, Multiline = multiline, MaxLength = 5 });
		form.Controls.Add(new ZTextBox() { Top = 300, Width = 100, Height = 20, CharacterCasing = CharacterCasing.Lower, ReadOnly = true, Multiline = multiline, Text = "readonly" });
	}
}
