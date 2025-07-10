using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using WinzorFramework;
using WinzorFramework.Extensions;
using WinzorFramework.JSInterop;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;

using static PlaywrightTestContext;

class DataGridTest
{
	[TestCase(300, 51)]
	[TestCase(500, 51)]
	public async Task DataRendered(int width, int expectedTotalHeight)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form() { Width = width };
			var table = new DataTable("data");
			var column1 = new DataColumn("one");
			column1.DataType = typeof(string);
			table.Columns.Add(column1);
			var column2 = new DataColumn("two");
			column2.DataType = typeof(string);
			table.Columns.Add(column2);
			table.Rows.Add("1.1", "1.2");
			table.Rows.Add("2.1", "2.2");

			var dataGrid = new DataGrid();
			dataGrid.Width = width;
			dataGrid.ReadOnly = true;
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnStyle1 = new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One Header" };
			columnStyle1.Width = 100;
			tableStyle.GridColumnStyles.Add(columnStyle1);
			var columnStyle2 = new DataGridTextBoxColumn { MappingName = "two", HeaderText = "Two Header" };
			columnStyle2.Width = 200;
			tableStyle.GridColumnStyles.Add(columnStyle2);
			dataGrid.TableStyles.Add(tableStyle);
			form.Controls.Add(dataGrid);

			return form;
		});

		var grid = rendered.Find(".datagrid");
		var gridContainer = rendered.Find(".datagrid .datagrid__container");
		var th = rendered.FindAll("thead>tr>th");

		Assert.That(grid, Is.Not.Null);
		Assert.That(gridContainer.Attributes["style"].Value, Is.EqualTo($"height:{expectedTotalHeight}px"));

		Assert.That(th, Has.Count.EqualTo(3));
		Assert.That(th[0].ToMarkup(), Does.Contain("datagrid__columns_dropdown"));
		Assert.That(th[1].ToMarkup(), Does.Contain("One Header"));
		Assert.That(th[1].GetAttribute("style"), Does.Contain("width: 100px;"));
		Assert.That(th[2].ToMarkup(), Does.Contain("Two Header"));
		Assert.That(th[2].GetAttribute("style"), Does.Contain("width: 200px;"));

		var tr = rendered.FindAll("tbody>tr");
		Assert.That(tr, Has.Count.EqualTo(2));
		Assert.That(tr[0].ChildNodes, Has.Length.EqualTo(3));
		Assert.That(tr[0].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));
		var readOnlyInput = tr[0].ChildNodes[1].ChildNodes[0].ChildNodes[0] as IHtmlInputElement;
		Assert.That(readOnlyInput.Value, Is.EqualTo("1.1"));
		Assert.That(readOnlyInput.HasAttribute("readonly"));
		Assert.That(tr[0].ChildNodes[2].TextContent, Is.EqualTo("1.2"));
		Assert.That(tr[1].ChildNodes, Has.Length.EqualTo(3));
		Assert.That(tr[1].ChildNodes[0].TextContent, Is.Empty);
		Assert.That(tr[1].ChildNodes[1].TextContent, Is.EqualTo("2.1"));
		Assert.That(tr[1].ChildNodes[2].TextContent, Is.EqualTo("2.2"));
	}

	[Test]
	public async Task DataRenderedWithDatasourceChanged()
	{
		DataGridForTest dataGrid = null;
		using var ctx = new WinzorTestContext();
		using var data1 = CreateMockData(1, 2);
		using var data2 = CreateMockData(3, 4);

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			dataGrid = new DataGridForTest();
			dataGrid.ReadOnly = true;
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = data1;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnStyle1 = new DataGridTextBoxColumn { MappingName = "col1", HeaderText = "One Header" };
			columnStyle1.Width = 100;
			tableStyle.GridColumnStyles.Add(columnStyle1);
			dataGrid.TableStyles.Add(tableStyle);

			return dataGrid;
		});

		var tr = rendered.FindAll("tbody>tr");
		Assert.That(tr, Has.Count.EqualTo(2));
		var readOnlyInput = tr[0].ChildNodes[1].ChildNodes[0].ChildNodes[0] as IHtmlInputElement;
		Assert.That(readOnlyInput.Value, Is.EqualTo("1"));
		Assert.That(tr[1].ChildNodes[1].TextContent, Is.EqualTo("2"));

		await dataGrid.InvokeWinzorDispatcherAsync(() =>
		{
			dataGrid.DataSource = data2;
		});

		tr = rendered.FindAll("tbody>tr");
		Assert.That(tr[0].ChildNodes[1].TextContent, Is.EqualTo("3"));
		Assert.That(tr[1].ChildNodes[1].TextContent, Is.EqualTo("4"));
	}

	[Test, WithPlaywrightPage]
	public async Task InteropHandlesBadElementReference()
	{
		var dataGrid = default(DataGridForTest);
		await using var ctx = new InMemoryTestServerContext();

		await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(dataGrid = new DataGridForTest());
			return form;
		});

		Assert.DoesNotThrowAsync(async () => await dataGrid.GetJSInterop<IGridJSInterop>()!.InitializeGridEventsAsync(new ElementReference(), DotNetObjectReference.Create(dataGrid as DataGrid)));
	}

	DataTable CreateMockData(int start, int end)
	{
		var table = new DataTable("data");
		var column1 = new DataColumn("col1");
		column1.DataType = typeof(string);
		table.Columns.Add(column1);

		if (start > 0 && end > 0)
		{
			for (int i = start; i <= end; i++)
			{
				table.Rows.Add(i.ToString());
			}
		}

		return table;
	}

	[Test]
	public async Task ParentPanelOverflowShouldBeHidden()
	{
		using var ctx = new WinzorTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var panel = new Panel() { AutoScroll = false, Width = 200, Height = 100 };
			var dataGrid = new DataGrid();

			panel.Controls.Add(dataGrid);
			return panel;
		});

		var panelEl = rendered.Find(".panel");
		Assert.That(panelEl.GetAttribute("style"), Does.Contain("overflow:hidden"));
	}

	[Test]
	public async Task BackgroundColorShouldBeCorrect()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var table = new DataTable("data");
			var column = new DataColumn("data");
			column.DataType = typeof(CustomType);
			table.Columns.Add(column);
			table.Rows.Add(new CustomType { DisplayText = "Row 1" });
			table.Rows.Add(new CustomType { DisplayText = "Row 2" });

			var dataGrid = new DataGrid();
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnStyle =
				new DataGridCustomColumnStyle { MappingName = "data", HeaderText = "Header", ReadOnly = true };
			columnStyle.Width = 100;
			tableStyle.GridColumnStyles.Add(columnStyle);
			dataGrid.TableStyles.Add(tableStyle);

			return dataGrid;
		});

		var grid = rendered.Find(".datagrid");
		Assert.That(grid.GetAttribute("style"), Does.Contain("background-color:var(--color-window);"));
	}

	[Test]
	public async Task CustomColumnStyle()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var table = new DataTable("data");
			var column = new DataColumn("data");
			column.DataType = typeof(CustomType);
			table.Columns.Add(column);
			table.Rows.Add(new CustomType { DisplayText = "Row 1" });
			table.Rows.Add(new CustomType { DisplayText = "Row 2" });

			var dataGrid = new DataGrid();
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnStyle = new DataGridCustomColumnStyle { MappingName = "data", HeaderText = "Header", ReadOnly = true };
			columnStyle.Width = 100;
			tableStyle.GridColumnStyles.Add(columnStyle);
			dataGrid.TableStyles.Add(tableStyle);

			return dataGrid;
		});
		var th = rendered.FindAll("thead>tr>th");
		Assert.That(th, Has.Count.EqualTo(2));
		Assert.That(th[0].ToMarkup(), Does.Contain("datagrid__columns_dropdown"));
		Assert.That(th[1].ToMarkup(), Does.Contain("Header"));

		var tr = rendered.FindAll("tbody>tr");
		Assert.That(tr, Has.Count.EqualTo(3));
		Assert.That(tr[0].ChildNodes, Has.Length.EqualTo(2));

		var input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		Assert.That(tr[0].ChildNodes[1].Contains(input), Is.True);
		Assert.That(input.Value, Is.EqualTo("Row 1"));

		Assert.That(tr[1].ChildNodes, Has.Length.EqualTo(2));
		Assert.That(tr[1].ChildNodes[1].TextContent, Is.EqualTo("Row 2"));
		Assert.That(tr[2].ChildNodes[0].TextContent, Is.EqualTo("*"));
	}

	[Test]
	public async Task PressingEnterKeyDataGridTextBoxColumnNoNewRowAndNewLetter()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var table = new DataTable("data");
			var column = new DataColumn("one");
			column.DataType = typeof(string);
			table.Columns.Add(column);
			table.Rows.Add("This is a test text");

			var dataGrid = new DataGrid();
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnStyle = new DataGridTextBoxColumn { MappingName = "one", HeaderText = "Header" };
			columnStyle.Width = 100;
			tableStyle.GridColumnStyles.Add(columnStyle);
			dataGrid.TableStyles.Add(tableStyle);

			return dataGrid;
		});

		var tr = rendered.FindAll("tbody>tr");
		Assert.That(tr, Has.Count.EqualTo(2));
		Assert.That(tr[0].ChildNodes, Has.Length.EqualTo(2));

		var input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		Assert.That(tr[0].ChildNodes[1].Contains(input), Is.True);
		Assert.That(input.Value, Is.EqualTo("This is a test text"));

		await rendered.FindAll("tbody>tr")[0].Children[1].MouseDownAsync(new WebMouseEventArgs());
		await rendered.KeyPressAsync(Keys.Enter, input);
		await rendered.KeyPressAsync(Keys.Enter, input);
		await rendered.KeyPressAsync(Keys.Enter, input);
		await input.FocusOutAsync();
		tr = rendered.FindAll("tbody>tr");

		Assert.That(tr, Has.Count.EqualTo(2));
		Assert.That(tr[0].ChildNodes[1].TextContent, Is.EqualTo("This is a test text"));
		Assert.That(tr[1].ChildNodes[1].TextContent, Is.EqualTo(string.Empty));
	}

	[Test]
	public async Task PressingEnterKeyDataGridTextBoxColumnWithChangingTextNoNewLetter()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var table = new DataTable("data");
			var column = new DataColumn("one");
			column.DataType = typeof(string);
			table.Columns.Add(column);
			table.Rows.Add("This is a test text");

			var dataGrid = new DataGrid();
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnStyle = new DataGridTextBoxColumn { MappingName = "one", HeaderText = "Header" };
			columnStyle.Width = 100;
			tableStyle.GridColumnStyles.Add(columnStyle);
			dataGrid.TableStyles.Add(tableStyle);

			return dataGrid;
		});

		var tr = rendered.FindAll("tbody>tr");
		Assert.That(tr, Has.Count.EqualTo(2));
		Assert.That(tr[0].ChildNodes, Has.Length.EqualTo(2));

		var input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		Assert.That(tr[0].ChildNodes[1].Contains(input), Is.True);
		Assert.That(input.Value, Is.EqualTo("This is a test text"));

		await StartEditAsync(rendered, input);
		input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		await input.TriggerEventAsync("oninput", new ChangeEventArgs { Value = "This is a new text" });
		await rendered.KeyPressAsync(Keys.Enter, input);
		await input.FocusOutAsync();
		tr = rendered.FindAll("tbody>tr");

		Assert.That(tr, Has.Count.EqualTo(2));
		Assert.That(tr[0].ChildNodes[1].TextContent, Is.EqualTo("This is a new text"));
		Assert.That(tr[1].ChildNodes[1].TextContent, Is.EqualTo(string.Empty));
	}

	[Test]
	public async Task EditDataGridTextBoxColumnByClick()
	{
		using var ctx = new WinzorTestContext();
		DataTable table = null;
		DataGrid dataGrid = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			table = new DataTable("data");
			var column1 = new DataColumn("one");
			column1.DataType = typeof(string);
			table.Columns.Add(column1);
			var column2 = new DataColumn("two");
			column2.DataType = typeof(string);
			table.Columns.Add(column2);
			table.Rows.Add("1.1", "1.2");
			table.Rows.Add("2.1", "2.2");

			dataGrid = new DataGrid();
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnStyle1 = new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One Header" };
			columnStyle1.Width = 80;
			tableStyle.GridColumnStyles.Add(columnStyle1);
			var columnStyle2 = new DataGridTextBoxColumn { MappingName = "two", HeaderText = "Two Header" };
			columnStyle2.Width = 180;
			tableStyle.GridColumnStyles.Add(columnStyle2);
			dataGrid.TableStyles.Add(tableStyle);

			return dataGrid;
		});

		var input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		Assert.That(rendered.FindAll("tbody>tr")[0].Children[1].Contains(input), Is.True);
		Assert.That(input.Value, Is.EqualTo("1.1"));
		Assert.That(input.GetAttribute("style"), Does.Contain("width:77"));
		Assert.That(input.GetAttribute("style"), Does.Contain("height:13"));
		Assert.That(input.ParentElement.GetAttribute("class"), Does.Contain("datagrid__control--edit"));
		Assert.That(rendered.FindAll("tbody>tr")[0].Children[1].GetAttribute("class"), Does.Contain("datagrid__cell--edit"));
		Assert.That(rendered.FindAll("tbody>tr")[0].GetAttribute("class"), Does.Contain("datagrid__row--edit"));
		await rendered.KeyPressAsync(Keys.A, input);
		input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		await input.TriggerEventAsync("oninput", new ChangeEventArgs { Value = "1.1.2" });
		await input.FocusOutAsync();
		Assert.That(rendered.FindAll("tbody>tr")[0].Children[1].TextContent, Is.EqualTo("1.1.2"));
		Assert.That(table.Rows[0][0], Is.EqualTo("1.1.2"));

		await SimulateClickOnCellForRowChange(rendered.FindAll("tbody>tr")[1].Children[2]);
		input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		Assert.That(rendered.FindAll("tbody>tr")[1].Children[2].Contains(input), Is.True);
		Assert.That(input.Value, Is.EqualTo("2.2"));
		Assert.That(input.GetAttribute("style"), Does.Contain("width:177"));
		Assert.That(input.GetAttribute("style"), Does.Contain("height:13"));
		await rendered.KeyPressAsync(Keys.A, input);
		input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		await input.TriggerEventAsync("oninput", new ChangeEventArgs { Value = "2.2.2" });
		await input.FocusOutAsync();
		Assert.That(rendered.FindAll("tbody>tr")[1].ChildNodes[2].TextContent, Is.EqualTo("2.2.2"));
		Assert.That(table.Rows[1][1], Is.EqualTo("2.2.2"));

		await rendered.FindAll(".datagrid td:first-child")[0].TriggerEventAsync("onmousedown", new WebMouseEventArgs());

		Assert.That(rendered.FindAll("tr.datagrid__row--selected").Count, Is.EqualTo(1));
		Assert.That(rendered.FindAll("tbody>tr")[0].GetAttribute("class"), Does.Contain("datagrid__row--selected"));

		await rendered.FindAll(".datagrid td:first-child")[1].TriggerEventAsync("onmousedown", new WebMouseEventArgs());

		Assert.That(rendered.FindAll("tr.datagrid__row--selected").Count, Is.EqualTo(1));
		Assert.That(rendered.FindAll("tbody>tr")[1].GetAttribute("class"), Does.Contain("datagrid__row--selected"));
		Assert.That(rendered.FindAll("tbody>tr")[1].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));
	}

	[Test, WithPlaywrightPage]
	public async Task EditDataGridTextBoxColumnScrollToCorrectPositionWhenWiderThanGrid()
	{
		DataTable table = null;
		DataGrid dataGrid = null;
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(200, 400) };
			table = new DataTable("data");
			var column1 = new DataColumn("one");
			column1.DataType = typeof(string);
			table.Columns.Add(column1);
			var column2 = new DataColumn("two");
			column2.DataType = typeof(string);
			table.Columns.Add(column2);
			table.Rows.Add("1.1", "1.2");
			table.Rows.Add("2.1", "2.2");

			dataGrid = new DataGrid() { Width = 200, Height = 100 };
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnStyle1 = new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One Header" };
			columnStyle1.Width = 80;
			tableStyle.GridColumnStyles.Add(columnStyle1);
			var columnStyle2 = new DataGridTextBoxColumn { MappingName = "two", HeaderText = "Two Header" };
			columnStyle2.Width = 240;
			tableStyle.GridColumnStyles.Add(columnStyle2);
			dataGrid.TableStyles.Add(tableStyle);

			form.Controls.Add(dataGrid);
			return form;
		});

		await page.WaitForSelectorAsync("table tbody tr");
		await dataGrid.InvokeWinzorDispatcherAsync(() => dataGrid.CurrentCell = new DataGridCell(1, 2));
		Assert.That(async () => await page.EvaluateAsync<int>("document.getElementsByClassName('datagrid')[0].scrollTop"), Is.EqualTo(0).After(1000, 100));
		Assert.That(async () => await page.EvaluateAsync<int>("document.getElementsByClassName('datagrid')[0].scrollLeft"), Is.EqualTo(80).Within(2).After(1000, 100));

		var input = await page.WaitForSelectorAsync("input.textbox");
		Assert.That((await input.GetComputedStyleAsync("width")).AsPixels(), Is.EqualTo(237));

		await input.FillAsync("Hello");
		Assert.That(async () => await page.EvaluateAsync<int>("document.getElementsByClassName('datagrid')[0].scrollLeft"), Is.EqualTo(80).Within(2).After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task EnsureVisibleAfterClickWhenCurrentCellAlreadySet()
	{
		DataTable table = null;
		DataGrid dataGrid = null;
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(200, 400) };
			table = new DataTable("data");
			var column1 = new DataColumn("one");
			column1.DataType = typeof(string);
			table.Columns.Add(column1);
			var column2 = new DataColumn("two");
			column2.DataType = typeof(string);
			table.Columns.Add(column2);
			table.Rows.Add("1.1", "1.2");

			dataGrid = new DataGrid() { Width = 200, Height = 100 };
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnStyle1 = new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One Header" };
			columnStyle1.Width = 80;
			tableStyle.GridColumnStyles.Add(columnStyle1);
			var columnStyle2 = new DataGridTextBoxColumn { MappingName = "two", HeaderText = "Two Header" };
			columnStyle2.Width = 240;
			tableStyle.GridColumnStyles.Add(columnStyle2);
			dataGrid.TableStyles.Add(tableStyle);

			form.Controls.Add(dataGrid);
			return form;
		});

		var dataGridElement = page.Locator(".datagrid");
		await dataGridElement.WaitForAsync();
		await dataGrid.InvokeWinzorDispatcherAsync(() => dataGrid.CurrentCell = new DataGridCell(0, 2));
		await Assertions.Expect(dataGridElement).ToHaveJSPropertyAsync("scrollTop", 0);
		await Assertions.Expect(dataGridElement).ToHaveJSPropertyAsync("scrollLeft", 80);

		var boundingBox = await dataGridElement.BoundingBoxAsync();
		var blankPoint = new Point
		{
			X = (int)(boundingBox.X + boundingBox.Width + 10),
			Y = (int)(boundingBox.Y + boundingBox.Height + 10)
		};
		await page.Mouse.ClickAsync(blankPoint.X, blankPoint.Y);
		await ScrollToAsync(page, 100, 0);
		await Assertions.Expect(dataGridElement).ToHaveJSPropertyAsync("scrollLeft", 100);
		var editControls = page.Locator(".datagrid__cell--edit");
		await Assertions.Expect(editControls).ToHaveCountAsync(0);

		var currentCellElement = page.Locator("tr > td:nth-child(3)").First;
		await currentCellElement.FocusAsync();
		await Assertions.Expect(dataGridElement).ToHaveJSPropertyAsync("scrollLeft", 80);
	}

	[Test]
	public async Task DontEditByClickWhenReadOnly()
	{
		using var ctx = new WinzorTestContext();
		DataTable table = null;
		DataGrid dataGrid = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			table = new DataTable("data");
			var column1 = new DataColumn("one");
			column1.DataType = typeof(string);
			table.Columns.Add(column1);
			var column2 = new DataColumn("two");
			column2.DataType = typeof(string);
			table.Columns.Add(column2);
			table.Rows.Add("1.1", "1.2");
			table.Rows.Add("2.1", "2.2");

			dataGrid = new DataGrid();
			dataGrid.ReadOnly = true;
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnStyle1 = new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One Header" };
			columnStyle1.Width = 80;
			tableStyle.GridColumnStyles.Add(columnStyle1);
			var columnStyle2 = new DataGridTextBoxColumn { MappingName = "two", HeaderText = "Two Header" };
			columnStyle2.Width = 180;
			tableStyle.GridColumnStyles.Add(columnStyle2);
			dataGrid.TableStyles.Add(tableStyle);

			return dataGrid;
		});

		var tr = rendered.FindAll("tbody>tr");
		Assert.That(tr, Has.Count.EqualTo(2));
		Assert.That(tr[0].ChildNodes, Has.Length.EqualTo(3));
		Assert.That(tr[0].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));
		var readOnlyInput = tr[0].ChildNodes[1].ChildNodes[0].ChildNodes[0] as IHtmlInputElement;
		Assert.That(readOnlyInput.Value, Is.EqualTo("1.1"));
		Assert.That(readOnlyInput.HasAttribute("readonly"));
		Assert.That(tr[0].ChildNodes[2].TextContent, Is.EqualTo("1.2"));
		Assert.That(tr[1].ChildNodes, Has.Length.EqualTo(3));
		Assert.That(tr[1].ChildNodes[0].TextContent, Is.Empty);
		Assert.That(tr[1].ChildNodes[1].TextContent, Is.EqualTo("2.1"));
		Assert.That(tr[1].ChildNodes[2].TextContent, Is.EqualTo("2.2"));

		await rendered.FindAll(".datagrid td:first-child")[0].TriggerEventAsync("onmousedown", new WebMouseEventArgs());

		Assert.That(rendered.FindAll("tr.datagrid__row--selected").Count, Is.EqualTo(1));
		Assert.That(rendered.FindAll("tbody>tr")[0].GetAttribute("class"), Does.Contain("datagrid__row--selected"));

		await rendered.FindAll(".datagrid td:first-child")[1].TriggerEventAsync("onmousedown", new WebMouseEventArgs());

		Assert.That(rendered.FindAll("tr.datagrid__row--selected").Count, Is.EqualTo(1));
		Assert.That(rendered.FindAll("tbody>tr")[1].GetAttribute("class"), Does.Contain("datagrid__row--selected"));
		Assert.That(rendered.FindAll("tbody>tr")[1].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));
	}

	[Test]
	public async Task DontEditBySelectRowsWhenReadOnly()
	{
		using var ctx = new WinzorTestContext();
		var (component, grid) = await TestGridAsync(ctx);
		var selectedRowMin = 1;
		var selectedRowMax = 2;
		await grid.SelectRowsAsync(selectedRowMin, selectedRowMax);
		CheckSelect(grid, selectedRowMin, selectedRowMax);

		selectedRowMin = 0;
		selectedRowMax = 3;
		await grid.SelectRowsAsync(selectedRowMin, selectedRowMax);
		CheckSelect(grid, selectedRowMin, selectedRowMax);
	}

	void CheckSelect(DataGrid grid, int selectedRowMin, int selectedRowMax)
	{
		for (int i = 0; i < grid.ListManager.Count; i++)
		{
			if (selectedRowMin <= i && i <= selectedRowMax)
			{
				Assert.That(grid.IsSelected(i), Is.True);
			}
			else
			{
				Assert.That(grid.IsSelected(i), Is.False);
			}
		}
	}

	[Test]
	public async Task NewRowClickCellToAddAndEdit()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var table = new DataTable("data");
			var column1 = new DataColumn("one");
			column1.DataType = typeof(string);
			table.Columns.Add(column1);
			var column2 = new DataColumn("two");
			column2.DataType = typeof(string);
			table.Columns.Add(column2);

			var dataGrid = new DataGrid();
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnStyle1 = new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One Header" };
			columnStyle1.Width = 80;
			tableStyle.GridColumnStyles.Add(columnStyle1);
			var columnStyle2 = new DataGridTextBoxColumn { MappingName = "two", HeaderText = "Two Header" };
			columnStyle2.Width = 180;
			tableStyle.GridColumnStyles.Add(columnStyle2);
			dataGrid.TableStyles.Add(tableStyle);

			return dataGrid;
		});

		var tr = rendered.FindAll("tbody>tr");
		Assert.That(tr, Has.Count.EqualTo(1));
		Assert.That(tr[0].OuterHtml, Does.Contain("height:16px"));
		Assert.That(tr[0].ChildNodes, Has.Length.EqualTo(3));
		Assert.That(tr[0].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));
		Assert.That(tr[0].ChildNodes[1].TextContent, Is.Empty);
		Assert.That(tr[0].ChildNodes[2].TextContent, Is.Empty);

		var input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		Assert.That(rendered.FindAll("tbody>tr")[0].Children[1].Contains(input), Is.True);
		Assert.That(rendered.FindAll("tbody>tr"), Has.Count.EqualTo(1));

		await rendered.KeyPressAsync(Keys.A, input);
		await rendered.Find("input").TriggerEventAsync("oninput", new ChangeEventArgs { Value = "1.1" });
		Assert.That(rendered.FindAll("tbody>tr")[0].ChildNodes[0].TextContent, Is.EqualTo("border_color"));
		await input.FocusOutAsync();

		tr = rendered.FindAll("tbody>tr");
		Assert.That(tr, Has.Count.EqualTo(2));
		Assert.That(tr[0].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));
		Assert.That(tr[0].ChildNodes[1].TextContent, Is.EqualTo("1.1"));
		Assert.That(tr[0].ChildNodes[2].TextContent, Is.Empty);

		Assert.That(tr[1].ChildNodes[0].TextContent, Is.EqualTo("*"));
		Assert.That(tr[1].ChildNodes[1].TextContent, Is.Empty);
		Assert.That(tr[1].ChildNodes[2].TextContent, Is.Empty);

		await SimulateClickOnCellForRowChange(tr[1].Children[1]);
		input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		Assert.That(rendered.FindAll("tbody>tr"), Has.Count.EqualTo(2));
		Assert.That(rendered.FindAll("tbody>tr")[1].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));
		Assert.That(rendered.FindAll("tbody>tr")[1].Children[1].Contains(input), Is.True);

		await rendered.KeyPressAsync(Keys.A, input);
		await rendered.Find("input").TriggerEventAsync("oninput", new ChangeEventArgs { Value = "2.1" });
		Assert.That(rendered.FindAll("tbody>tr")[1].ChildNodes[0].TextContent, Is.EqualTo("border_color"));
		await input.FocusOutAsync();

		tr = rendered.FindAll("tbody>tr");
		Assert.That(tr, Has.Count.EqualTo(3));
		Assert.That(tr[1].Children[1].TextContent, Is.EqualTo("2.1"));
		Assert.That(tr[2].Children[0].TextContent, Is.EqualTo("*"));
	}

	[Test]
	public async Task RowIndicatorStyle()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var table = new DataTable("data");
			var column1 = new DataColumn("one");
			column1.DataType = typeof(string);
			table.Columns.Add(column1);
			var column2 = new DataColumn("two");
			column2.DataType = typeof(string);
			table.Columns.Add(column2);

			var dataGrid = new DataGrid();
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnStyle1 = new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One Header" };
			columnStyle1.Width = 80;
			tableStyle.GridColumnStyles.Add(columnStyle1);
			var columnStyle2 = new DataGridTextBoxColumn { MappingName = "two", HeaderText = "Two Header" };
			columnStyle2.Width = 180;
			tableStyle.GridColumnStyles.Add(columnStyle2);
			dataGrid.TableStyles.Add(tableStyle);

			return dataGrid;
		});

		var tr = rendered.FindAll("tbody>tr");
		var input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		await rendered.KeyPressAsync(Keys.A, input);
		await rendered.Find("input").TriggerEventAsync("oninput", new ChangeEventArgs { Value = "1.1" });
		Assert.That(rendered.FindAll("tbody>tr")[0].ChildNodes[0].TextContent, Is.EqualTo("border_color"));
		await input.FocusOutAsync();

		tr = rendered.FindAll("tbody>tr");
		await SimulateClickOnCellForRowChange(tr[1].Children[1]);
		input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		Assert.That(rendered.FindAll("tbody>tr"), Has.Count.EqualTo(2));
		Assert.That(rendered.FindAll("tbody>tr")[0].ChildNodes[0].TextContent, Is.EqualTo(string.Empty));
		Assert.That(rendered.FindAll("tbody>tr")[1].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));

		// click top cell will change arrow_right to star
		tr = rendered.FindAll("tbody>tr");
		await SimulateClickOnCellForRowChange(tr[0].Children[1]);
		input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		Assert.That(rendered.FindAll("tbody>tr"), Has.Count.EqualTo(2));
		Assert.That(rendered.FindAll("tbody>tr")[0].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));
		Assert.That(rendered.FindAll("tbody>tr")[1].ChildNodes[0].TextContent, Is.EqualTo("*"));

		//repeat above step but click the top the arrow should be the same and no changes
		tr = rendered.FindAll("tbody>tr");
		await SimulateClickOnCellForRowChange(tr[1].Children[1]);
		input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		Assert.That(rendered.FindAll("tbody>tr"), Has.Count.EqualTo(2));
		Assert.That(rendered.FindAll("tbody>tr")[0].ChildNodes[0].TextContent, Is.EqualTo(string.Empty));
		Assert.That(rendered.FindAll("tbody>tr")[1].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));

		await SimulateClickOnCellForRowChange(rendered.FindAll("th")[0]);
		input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		Assert.That(rendered.FindAll("tbody>tr"), Has.Count.EqualTo(2));
		Assert.That(rendered.FindAll("tbody>tr")[0].ChildNodes[0].TextContent, Is.EqualTo(string.Empty));
		Assert.That(rendered.FindAll("tbody>tr")[1].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));

		await SimulateClickOnCellForRowChange(rendered.FindAll("th span")[0]);
		input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		Assert.That(rendered.FindAll("tbody>tr"), Has.Count.EqualTo(2));
		Assert.That(rendered.FindAll("tbody>tr")[0].ChildNodes[0].TextContent, Is.EqualTo(string.Empty));
		Assert.That(rendered.FindAll("tbody>tr")[1].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));
	}

	[Test]
	public async Task StylesAppliedCorrectlyOnEdit()
	{
		using var ctx = new WinzorTestContext();
		DataTable table = null;
		DataGrid dataGrid = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			table = new DataTable("data");
			var column = new DataColumn("one");
			column.DataType = typeof(CustomType);
			table.Columns.Add(column);
			table.Rows.Add(new CustomType { DisplayText = "Row 1" });

			dataGrid = new DataGrid();
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnStyle = new DataGridCustomColumnStyle { MappingName = "one", HeaderText = "One Header" };
			columnStyle.Width = 80;
			tableStyle.GridColumnStyles.Add(columnStyle);
			dataGrid.TableStyles.Add(tableStyle);

			return dataGrid;
		});

		await rendered.FindAll("tbody>tr")[0].Children[1].MouseDownAsync(new WebMouseEventArgs());
		var tr = rendered.FindAll("tbody>tr");
		var input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		Assert.That(input.GetAttribute("style"), Does.Contain($"background-color:{Color.Red.GetColorStyleValue()};"));
	}

	[Test, WithPlaywrightPage]
	public async Task ClickRowHeaderShouldTriggerMouseUpEvent()
	{
		await using var ctx = new InMemoryTestServerContext();
		DataGrid dataGrid = null;
		var rowHeaderClicked = false;
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			dataGrid = new DataGrid();
			InitializeTestGrid(dataGrid);
			dataGrid.MouseUp += (_, _) =>
			{
				rowHeaderClicked = true;
			};
			return dataGrid;
		});
		var rowHeader = page.Locator(".datagrid__row--edit > td:first-child");
		await rowHeader.ClickAsync();
		Assert.That(() => rowHeaderClicked, Is.True.After(3000, 100));
	}

	[Test]
	public async Task ClickHeaderToSort()
	{
		using var ctx = new WinzorTestContext();
		DataTable table = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			table = new DataTable("data");
			var column1 = new DataColumn("one");
			column1.DataType = typeof(string);
			table.Columns.Add(column1);
			var column2 = new DataColumn("two");
			column2.DataType = typeof(string);
			table.Columns.Add(column2);
			table.Rows.Add("3", "6");
			table.Rows.Add("1", "5");
			table.Rows.Add("2", "4");

			var dataGrid = new DataGrid();
			dataGrid.ReadOnly = true;
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnStyle1 = new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One Header" };
			columnStyle1.Width = 80;
			tableStyle.GridColumnStyles.Add(columnStyle1);
			var columnStyle2 = new DataGridTextBoxColumn { MappingName = "two", HeaderText = "Two Header" };
			columnStyle2.Width = 180;
			tableStyle.GridColumnStyles.Add(columnStyle2);
			dataGrid.TableStyles.Add(tableStyle);

			return dataGrid;
		});

		var th = rendered.FindAll("th");
		var thSpan = rendered.FindAll("th span");
		Assert.That(th, Has.Count.EqualTo(3));
		Assert.That(th[1].TextContent, Is.EqualTo("One Header"));
		Assert.That(th[2].TextContent, Is.EqualTo("Two Header"));
		DataGridTestHelper.AssertGridSort(rendered, new string[] { string.Empty, string.Empty },
			new string[] { "3", "6", "1", "5", "2", "4" });

		await thSpan[0].MouseDownAsync(new WebMouseEventArgs());
		await thSpan[0].MouseUpAsync(new WebMouseEventArgs());
		DataGridTestHelper.AssertGridSort(rendered, new string[] { "datagrid__header--ascending", string.Empty },
			new string[] { "1", "5", "2", "4", "3", "6" });
		await thSpan[0].MouseDownAsync(new WebMouseEventArgs());
		await thSpan[0].MouseUpAsync(new WebMouseEventArgs());
		DataGridTestHelper.AssertGridSort(rendered, new string[] { "datagrid__header--descending", string.Empty },
			new string[] { "3", "6", "2", "4", "1", "5" });
		await thSpan[0].MouseDownAsync(new WebMouseEventArgs());
		await thSpan[0].MouseUpAsync(new WebMouseEventArgs());
		DataGridTestHelper.AssertGridSort(rendered, new string[] { "datagrid__header--ascending", string.Empty },
			new string[] { "1", "5", "2", "4", "3", "6" });
		await thSpan[1].MouseDownAsync(new WebMouseEventArgs());
		await thSpan[1].MouseUpAsync(new WebMouseEventArgs());
		DataGridTestHelper.AssertGridSort(rendered, new string[] { string.Empty, "datagrid__header--ascending" },
			new string[] { "2", "4", "1", "5", "3", "6" });
		await thSpan[1].MouseDownAsync(new WebMouseEventArgs());
		await thSpan[1].MouseUpAsync(new WebMouseEventArgs());
		DataGridTestHelper.AssertGridSort(rendered, new string[] { string.Empty, "datagrid__header--descending" },
			new string[] { "3", "6", "1", "5", "2", "4" });
	}

	[Test, WithPlaywrightPage]
	public async Task ValidateDataGridSortingIconWidthAndTextPadding()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(200, 100) };
			DataTable table = new DataTable("datatable");
			var column1 = new DataColumn("column1");
			column1.DataType = typeof(string);
			table.Columns.Add(column1);
			table.Rows.Add("three");
			table.Rows.Add("one");
			table.Rows.Add("two");

			var dataGrid = new DataGrid();
			dataGrid.ReadOnly = true;
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "datatable" };
			var columnStyle1 = new DataGridTextBoxColumn { MappingName = "column1", HeaderText = "First Header" };
			columnStyle1.Width = 80;
			tableStyle.GridColumnStyles.Add(columnStyle1);
			dataGrid.TableStyles.Add(tableStyle);
			form.Controls.Add(dataGrid);

			return form;
		});

		await page.WaitForSelectorAsync("table");
		var headerElement = await page.QuerySelectorAsync("table th:has-text('First Header')");
		await headerElement.ClickAsync();
		await page.WaitForSelectorAsync(".datagrid__header--ascending span");
		var headerTextSpanElementAsc = await page.QuerySelectorAsync(".datagrid__header--ascending span");
		await page.WaitForSelectorAsync(".datagrid__header--ascending div:first-of-type");
		var headerIconDivElementAsc = await page.QuerySelectorAsync(".datagrid__header--ascending div:first-of-type");
		Assert.That(async () => await headerIconDivElementAsc.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("18px"));
		Assert.That(async () => await headerTextSpanElementAsc.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('padding-right')"), Is.EqualTo("18px"));
		Assert.That(async () => await headerTextSpanElementAsc.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('padding-left')"), Is.EqualTo("0px"));
		Assert.That(async () => await headerTextSpanElementAsc.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('padding-top')"), Is.EqualTo("0px"));
		Assert.That(async () => await headerTextSpanElementAsc.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('padding-bottom')"), Is.EqualTo("0px"));
		await headerElement.ClickAsync();
		await page.WaitForSelectorAsync(".datagrid__header--descending span");
		var headerTextSpanElementDesc = await page.QuerySelectorAsync(".datagrid__header--descending span");
		await page.WaitForSelectorAsync(".datagrid__header--descending div:first-of-type");
		var headerIconDivElementDesc = await page.QuerySelectorAsync(".datagrid__header--descending div:first-of-type");
		Assert.That(async () => await headerIconDivElementDesc.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("18px"));
		Assert.That(async () => await headerTextSpanElementDesc.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('padding-right')"), Is.EqualTo("18px"));
		Assert.That(async () => await headerTextSpanElementDesc.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('padding-left')"), Is.EqualTo("0px"));
		Assert.That(async () => await headerTextSpanElementDesc.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('padding-top')"), Is.EqualTo("0px"));
		Assert.That(async () => await headerTextSpanElementDesc.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('padding-bottom')"), Is.EqualTo("0px"));
	}

	[Test]
	public async Task ClickHeaderToSortEmptyGrid()
	{
		using var ctx = new WinzorTestContext();
		DataTable table = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			table = new DataTable("data");
			var column1 = new DataColumn("one");
			column1.DataType = typeof(string);
			table.Columns.Add(column1);
			var column2 = new DataColumn("two");
			column2.DataType = typeof(string);
			table.Columns.Add(column2);

			var dataGrid = new DataGrid();
			dataGrid.ReadOnly = true;
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnStyle1 = new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One Header" };
			columnStyle1.Width = 80;
			tableStyle.GridColumnStyles.Add(columnStyle1);
			var columnStyle2 = new DataGridTextBoxColumn { MappingName = "two", HeaderText = "Two Header" };
			columnStyle2.Width = 180;
			tableStyle.GridColumnStyles.Add(columnStyle2);
			dataGrid.TableStyles.Add(tableStyle);

			return dataGrid;
		});

		var th = rendered.FindAll("th span");
		Assert.DoesNotThrowAsync(async () => await th[1].MouseDownAsync(new WebMouseEventArgs()));
		Assert.That(rendered.FindAll("th")[1].Attributes["class"].Value, Is.Empty);
	}

	[Test]
	public async Task OnMouseDownAsyncDoesNotOverrideValueForOnMouseDown()
	{
		DataGrid.HitTestInfo mouseDownHit1 = null;
		DataGrid.HitTestInfo mouseDownHit2 = null;

		using var ctx = new WinzorTestContext();
		var (rendered, grid) = await TestGridAsync(ctx);

		grid.MouseDown += (_, _) =>
		{
			if (mouseDownHit1 == null)
			{
				mouseDownHit1 = grid.HitTest(0, 0);
			}
			else if (mouseDownHit2 == null)
			{
				mouseDownHit2 = grid.HitTest(0, 0);
			}
		};

		var sleep = grid.InvokeWinzorDispatcherAsync(() =>
		{
			//Keep the dispatcher busy to enforce race condition between OnMouseDownAsync calls
			Thread.Sleep(250);
		});

		var mouseDownClick1 = rendered.FindAll("tbody>tr")[0].Children[0].MouseDownAsync(new WebMouseEventArgs());
		var mouseDownClick2 = rendered.FindAll("tbody>tr")[1].Children[0].MouseDownAsync(new WebMouseEventArgs());

		await Task.WhenAll(mouseDownClick1, mouseDownClick2, sleep);

		Assert.That(mouseDownHit1.Row, Is.EqualTo(0));
		Assert.That(mouseDownHit2.Row, Is.EqualTo(1));
	}

	[Test]
	public async Task OnMouseUpAsyncHitTestInfoShouldNotBeHitNone()
	{
		var mouseDownHit = HitNone;
		var mouseUpHit = HitNone;

		using var ctx = new WinzorTestContext();
		var (rendered, grid) = await TestGridAsync(ctx);

		grid.MouseDown += (_, e) =>
		{
			mouseDownHit = grid.HitTest(e.X, e.Y);
		};

		grid.MouseUp += (_, e) =>
		{
			mouseUpHit = grid.HitTest(e.X, e.Y);
		};

		var rowHeader = rendered.FindAll("tbody>tr")[0].Children[0];
		await rowHeader.MouseDownAsync(new WebMouseEventArgs());// click RowHeader
		Assert.That(() => mouseDownHit, Is.Not.EqualTo(HitNone).After(3000, 100));
		Assert.That(() => mouseDownHit.Row, Is.EqualTo(0).After(3000, 100));

		await rowHeader.MouseUpAsync(new WebMouseEventArgs());
		Assert.That(() => mouseUpHit, Is.Not.EqualTo(HitNone).After(3000, 100));
		Assert.That(mouseUpHit.Row, Is.EqualTo(0));//RowHeader's MouseUp should be called here, just make sure that MouseUp's HitTestInfo is not HitNone
		Assert.That(grid.HitTest(0, 0).row, Is.EqualTo(0), "currentHitTest should maintain row selection to 0, rather than reset back to -1");
	}

	[Test]
	public async Task DataGridDoesNotRunTriggeredEventsWhenNotEnabled()
	{
		var mouseDownCalled = false;
		var mouseUpCalled = false;
		var focusInCalled = false;
		var dragStartCalled = false;
		var dropCalled = false;
		var contextMenuCalled = false;

		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var dataGrid = new DataGrid();
			dataGrid.MouseDown += (_, _) => { mouseDownCalled = true; };
			dataGrid.MouseUp += (_, _) => { mouseUpCalled = true; };
			dataGrid.GotFocus += (_, _) => { focusInCalled = true; };
			dataGrid.DragStart += (_, _) => { dragStartCalled = true; };
			dataGrid.DragDrop += (_, _) => { dropCalled = true; };

			var contextMenu = new ContextMenu();
			contextMenu.MenuItems.Add(new MenuItem() { Text = "test" });
			contextMenu.Popup += (_, _) => { contextMenuCalled = true; };
			dataGrid.ContextMenu = contextMenu;

			InitializeTestGrid(dataGrid);
			dataGrid.Enabled = false;
			return dataGrid;
		});

		await rendered.FindAll("tbody>tr")[0].Children[0].MouseDownAsync(new WebMouseEventArgs());
		await rendered.FindAll("tbody>tr")[0].Children[0].MouseUpAsync(new WebMouseEventArgs());
		await rendered.FindAll("tbody>tr")[0].Children[1].TriggerEventAsync("onwinzorfocusin", new WinzorFocusInEventArgs());
		await rendered.FindAll("thead>tr>th>span")[0].DragStartAsync(new WebDragEventArgs());
		await rendered.FindAll("thead>tr>th>span")[0].DropAsync(new WebDragEventArgs());
		await rendered.FindAll("tbody>tr")[0].Children[0].ContextMenuAsync(new WebMouseEventArgs() { Button = 2, Type = "contextmenu" });

		Assert.That(mouseDownCalled, Is.False, "OnMouseDownCore was called on disabled DataGrid");
		Assert.That(mouseUpCalled, Is.False, "OnMouseUpCore was called on disabled DataGrid");
		Assert.That(focusInCalled, Is.False, "FocusInternal was called on disabled DataGrid");
		Assert.That(dragStartCalled, Is.False, "OnDragStart was called on disabled DataGrid");
		Assert.That(dropCalled, Is.False, "OnDragDrop was called on disabled DataGrid");
		Assert.That(contextMenuCalled, Is.False, "ContextMenuCore was called on disabled DataGrid");
	}

	[Test]
	public async Task DataGridDoesNotRunTriggeredDropEvents()
	{	
		var dropCalled = false;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var dataGrid = new DataGrid();
			dataGrid.DragDrop += (_, _) => { dropCalled = true; };
			dataGrid.AllowDrop = true;
			InitializeTestGrid(dataGrid);
			return dataGrid;
		});
		var tr = rendered.FindAll("tbody>tr")[1].ChildNodes[0];
		await rendered.FindAll("tbody>tr")[1].TriggerEventAsync("onwinzordrop", new WinzorDragEventArgs()
		{
			ClientX = 3,
			ClientY = 30,
			ControlID = "1"
		});

		Assert.That(dropCalled, Is.True, "OnDragDrop is not called");
	}

	[Test]
	public async Task ClickShouldSelectSingleRow()
	{
		using var ctx = new WinzorTestContext();
		var (component, grid) = await TestGridAsync(ctx);

		var rows = component.FindAll("tr td:first-child").ToList();

		await rows[0].TriggerEventAsync("onmousedown", new WebMouseEventArgs());
		Assert.That(component.FindAll(".datagrid__row--selected").Count, Is.EqualTo(1));
		Assert.That(component.FindAll(".datagrid__row--selected")[0].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));
		Assert.That(component.FindAll(".datagrid__row--selected")[0].ChildNodes[1].TextContent, Is.EqualTo("row1"));

		await rows[1].TriggerEventAsync("onmousedown", new WebMouseEventArgs());
		Assert.That(component.FindAll(".datagrid__row--selected").Count, Is.EqualTo(1));
		Assert.That(component.FindAll(".datagrid__row--selected")[0].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));
		Assert.That(component.FindAll(".datagrid__row--selected")[0].ChildNodes[1].TextContent, Is.EqualTo("row2"));
	}

	[Test]
	public async Task ControlClickShouldSelectOneOrMoreRows()
	{
		using var ctx = new WinzorTestContext();
		var (component, grid) = await TestGridAsync(ctx);

		var rows = component.FindAll("tr td:first-child").ToList();

		await rows[0].TriggerEventAsync("onmousedown", new WebMouseEventArgs());
		Assert.That(component.FindAll(".datagrid__row--selected").Count, Is.EqualTo(1));
		Assert.That(component.FindAll(".datagrid__row--selected")[0].ChildNodes[1].TextContent, Is.EqualTo("row1"));

		await rows[1].TriggerEventAsync("onmousedown", new WebMouseEventArgs() { CtrlKey = true });
		Assert.That(component.FindAll(".datagrid__row--selected").Count, Is.EqualTo(2));
		Assert.That(component.FindAll(".datagrid__row--selected")[0].ChildNodes[0].TextContent, Is.Empty);
		Assert.That(component.FindAll(".datagrid__row--selected")[0].ChildNodes[1].TextContent, Is.EqualTo("row1"));
		Assert.That(component.FindAll(".datagrid__row--selected")[1].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));
		Assert.That(component.FindAll(".datagrid__row--selected")[1].ChildNodes[1].TextContent, Is.EqualTo("row2"));

		await rows[0].TriggerEventAsync("onmousedown", new WebMouseEventArgs() { CtrlKey = true });
		Assert.That(component.FindAll(".datagrid__row--selected").Count, Is.EqualTo(1));
		Assert.That(component.FindAll(".datagrid__row--selected")[0].ChildNodes[0].TextContent, Is.Empty);
		Assert.That(component.FindAll(".datagrid__row--selected")[0].ChildNodes[1].TextContent, Is.EqualTo("row2"));
		Assert.That(component.FindAll("tr td:first-child")[0].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));

		await rows[1].TriggerEventAsync("onmousedown", new WebMouseEventArgs() { CtrlKey = true });
		Assert.That(component.FindAll(".datagrid__row--selected").Count, Is.EqualTo(0));
		Assert.That(component.FindAll("tr td:first-child")[1].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));
	}

	[Test]
	public async Task ControlClickSelectDeselectSingleRow()
	{
		using var ctx = new WinzorTestContext();
		var (component, grid) = await TestGridAsync(ctx);

		var rows = component.FindAll("tr td:first-child").ToList();

		await rows[0].TriggerEventAsync("onmousedown", new WebMouseEventArgs());
		Assert.That(component.FindAll(".datagrid__row--selected").Count, Is.EqualTo(1));
		Assert.That(component.FindAll(".datagrid__row--selected")[0].ChildNodes[1].TextContent, Is.EqualTo("row1"));

		await rows[0].TriggerEventAsync("onmousedown", new WebMouseEventArgs() { CtrlKey = true });
		Assert.That(component.FindAll(".datagrid__row--selected").Count, Is.EqualTo(0));
		Assert.That(component.FindAll("tr td:first-child")[0].TextContent, Is.EqualTo("arrow_right"));

		await rows[1].TriggerEventAsync("onmousedown", new WebMouseEventArgs());
		Assert.That(component.FindAll(".datagrid__row--selected").Count, Is.EqualTo(1));
		Assert.That(component.FindAll(".datagrid__row--selected")[0].ChildNodes[1].TextContent, Is.EqualTo("row2"));
	}

	[Test]
	public async Task ShiftClickShouldSelectRowRange()
	{
		using var ctx = new WinzorTestContext();
		var (component, grid) = await TestGridAsync(ctx);

		var rows = component.FindAll("tr td:first-child").ToList();

		await rows[0].TriggerEventAsync("onmousedown", new WebMouseEventArgs());
		Assert.That(component.FindAll(".datagrid__row--selected").Count, Is.EqualTo(1));
		Assert.That(component.FindAll(".datagrid__row--selected")[0].ChildNodes[1].TextContent, Is.EqualTo("row1"));

		await rows[1].TriggerEventAsync("onmousedown", new WebMouseEventArgs() { ShiftKey = true });
		Assert.That(component.FindAll(".datagrid__row--selected").Count, Is.EqualTo(2));
		Assert.That(component.FindAll(".datagrid__row--selected")[0].ChildNodes[0].TextContent, Is.Empty);
		Assert.That(component.FindAll(".datagrid__row--selected")[0].ChildNodes[1].TextContent, Is.EqualTo("row1"));
		Assert.That(component.FindAll(".datagrid__row--selected")[1].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));
		Assert.That(component.FindAll(".datagrid__row--selected")[1].ChildNodes[1].TextContent, Is.EqualTo("row2"));

		await ShiftClickUpThenDown(component, grid);
	}

	[Test]
	public async Task ShiftClickOnSameRowShouldSelectRow()
	{
		using var ctx = new WinzorTestContext();
		var (component, grid) = await TestGridAsync(ctx);

		var rows = component.FindAll("tr td:first-child").ToList();

		await rows[0].TriggerEventAsync("onmousedown", new WebMouseEventArgs() { ShiftKey = true });
		Assert.That(component.FindAll(".datagrid__row--selected").Count, Is.EqualTo(1));
		Assert.That(component.FindAll(".datagrid__row--selected")[0].ChildNodes[1].TextContent, Is.EqualTo("row1"));

		await rows[0].TriggerEventAsync("onmousedown", new WebMouseEventArgs() { ShiftKey = true });
		Assert.That(component.FindAll(".datagrid__row--selected").Count, Is.EqualTo(1));
		Assert.That(component.FindAll(".datagrid__row--selected")[0].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));
		Assert.That(component.FindAll(".datagrid__row--selected")[0].ChildNodes[1].TextContent, Is.EqualTo("row1"));

		await ShiftClickUpThenDown(component, grid);
	}

	[Test]
	public async Task ShiftClickAfterShiftClickShouldSelectRowRange()
	{
		using var ctx = new WinzorTestContext();
		var (component, grid) = await TestGridAsync(ctx);

		var rows = component.FindAll("tr td:first-child").ToList();

		await rows[0].TriggerEventAsync("onmousedown", new WebMouseEventArgs() { ShiftKey = true });
		Assert.That(component.FindAll(".datagrid__row--selected").Count, Is.EqualTo(1));
		Assert.That(component.FindAll(".datagrid__row--selected")[0].ChildNodes[1].TextContent, Is.EqualTo("row1"));

		await rows[1].TriggerEventAsync("onmousedown", new WebMouseEventArgs() { ShiftKey = true });
		Assert.That(component.FindAll(".datagrid__row--selected").Count, Is.EqualTo(2));
		Assert.That(component.FindAll(".datagrid__row--selected")[0].ChildNodes[0].TextContent, Is.Empty);
		Assert.That(component.FindAll(".datagrid__row--selected")[0].ChildNodes[1].TextContent, Is.EqualTo("row1"));
		Assert.That(component.FindAll(".datagrid__row--selected")[1].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));
		Assert.That(component.FindAll(".datagrid__row--selected")[1].ChildNodes[1].TextContent, Is.EqualTo("row2"));

		await ShiftClickUpThenDown(component, grid);
	}

	[Test]
	public async Task ShiftClickAfterCtrlClickShouldSelectRowRange()
	{
		using var ctx = new WinzorTestContext();
		var (component, grid) = await TestGridAsync(ctx);

		var rows = component.FindAll("tr td:first-child").ToList();

		await rows[0].TriggerEventAsync("onmousedown", new WebMouseEventArgs() { CtrlKey = true });
		Assert.That(component.FindAll(".datagrid__row--selected").Count, Is.EqualTo(1));
		Assert.That(component.FindAll(".datagrid__row--selected")[0].ChildNodes[1].TextContent, Is.EqualTo("row1"));

		await rows[1].TriggerEventAsync("onmousedown", new WebMouseEventArgs() { ShiftKey = true });
		Assert.That(component.FindAll(".datagrid__row--selected").Count, Is.EqualTo(2));
		Assert.That(component.FindAll(".datagrid__row--selected")[0].ChildNodes[0].TextContent, Is.Empty);
		Assert.That(component.FindAll(".datagrid__row--selected")[0].ChildNodes[1].TextContent, Is.EqualTo("row1"));
		Assert.That(component.FindAll(".datagrid__row--selected")[1].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));
		Assert.That(component.FindAll(".datagrid__row--selected")[1].ChildNodes[1].TextContent, Is.EqualTo("row2"));

		await ShiftClickUpThenDown(component, grid);
	}

	[Test]
	public async Task RangeSelectThenDeselect()
	{
		using var ctx = new WinzorTestContext();
		var (component, grid) = await TestGridAsync(ctx);

		var rows = component.FindAll("tr td:first-child").ToList();

		await rows[1].TriggerEventAsync("onmousedown", new WebMouseEventArgs());
		await rows[3].TriggerEventAsync("onmousedown", new WebMouseEventArgs() { ShiftKey = true });
		Assert.That(component.FindAll(".datagrid__row--selected").Count, Is.EqualTo(3));
		Assert.That(component.FindAll(".datagrid__row--selected")[0].ChildNodes[0].TextContent, Is.Empty);
		Assert.That(component.FindAll(".datagrid__row--selected")[0].ChildNodes[1].TextContent, Is.EqualTo("row2"));
		Assert.That(component.FindAll(".datagrid__row--selected")[1].ChildNodes[0].TextContent, Is.Empty);
		Assert.That(component.FindAll(".datagrid__row--selected")[1].ChildNodes[1].TextContent, Is.EqualTo("row3"));
		Assert.That(component.FindAll(".datagrid__row--selected")[2].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));
		Assert.That(component.FindAll(".datagrid__row--selected")[2].ChildNodes[1].TextContent, Is.EqualTo("row4"));

		await rows[1].TriggerEventAsync("onmousedown", new WebMouseEventArgs() { CtrlKey = true });
		await rows[2].TriggerEventAsync("onmousedown", new WebMouseEventArgs() { CtrlKey = true });
		await rows[3].TriggerEventAsync("onmousedown", new WebMouseEventArgs() { CtrlKey = true });

		Assert.That(component.FindAll(".datagrid__row--selected").Count, Is.EqualTo(0));
	}

	async Task ShiftClickUpThenDown(IRenderedFragment component, DataGrid grid)
	{
		var rows = component.FindAll("tr td:first-child").ToList();
		await rows[0].TriggerEventAsync("onmousedown", new WebMouseEventArgs() { ShiftKey = true });
		Assert.That(component.FindAll(".datagrid__row--selected").Count, Is.EqualTo(1));
		Assert.That(component.FindAll(".datagrid__row--selected")[0].ChildNodes[1].TextContent, Is.EqualTo("row1"));

		await rows[4].TriggerEventAsync("onmousedown", new WebMouseEventArgs() { ShiftKey = true });
		Assert.That(component.FindAll(".datagrid__row--selected").Count, Is.EqualTo(5));
		Assert.That(component.FindAll(".datagrid__row--selected")[0].ChildNodes[1].TextContent, Is.EqualTo("row1"));
		Assert.That(component.FindAll(".datagrid__row--selected")[1].ChildNodes[1].TextContent, Is.EqualTo("row2"));
		Assert.That(component.FindAll(".datagrid__row--selected")[2].ChildNodes[1].TextContent, Is.EqualTo("row3"));
		Assert.That(component.FindAll(".datagrid__row--selected")[3].ChildNodes[1].TextContent, Is.EqualTo("row4"));
		Assert.That(component.FindAll(".datagrid__row--selected")[4].ChildNodes[1].TextContent, Is.EqualTo(""));
	}

	[Test]
	public async Task SetGridColumnSizes()
	{
		using var ctx = new WinzorTestContext();
		ctx.JSInterop.Setup<object>("grid.resizeColumn", _ => true).SetResult(null);
		var module = ctx.JSInterop.SetupModule("/js/module/grid.js");
		module.Setup<object>("resizeColumn", _ => true).SetResult(null);
		DataTable table = null;
		DataGrid dataGrid = null;

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			table = new DataTable("data");
			var column1 = new DataColumn("one");
			column1.DataType = typeof(string);
			table.Columns.Add(column1);
			var column2 = new DataColumn("two");
			column2.DataType = typeof(string);
			table.Columns.Add(column2);

			dataGrid = new DataGrid();
			dataGrid.ReadOnly = true;
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnStyle1 = new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One Header" };
			columnStyle1.Width = 80;
			tableStyle.GridColumnStyles.Add(columnStyle1);
			var columnStyleInHidden = new DataGridTextBoxColumn { MappingName = "hidden-col", HeaderText = "Hidden Header", PropertyDescriptor = null };
			columnStyleInHidden.Width = 180;
			tableStyle.GridColumnStyles.Add(columnStyleInHidden);
			var columnStyle2 = new DataGridTextBoxColumn { MappingName = "two", HeaderText = "Two Header" };
			columnStyle2.Width = 180;
			tableStyle.GridColumnStyles.Add(columnStyle2);
			dataGrid.TableStyles.Add(tableStyle);

			return dataGrid;
		});

		var resizers = rendered.FindAll("th .datagrid__column_resizer");

		Assert.That(resizers, Has.Count.EqualTo(2));

		await resizers[0].TriggerEventAsync("onmousedown", new WebMouseEventArgs() { Button = 0, Detail = 1 });
		await resizers[1].TriggerEventAsync("onmousedown", new WebMouseEventArgs() { Button = 0, Detail = 1 });

		ctx.JSInterop.VerifyInvoke("resizeColumn", 2);

		Assert.DoesNotThrowAsync(async () => await dataGrid.SetColumnSize(0, 10));
		Assert.That(rendered.FindAll("th")[1].Attributes["style"].Value, Does.Contain("width: 10px;"));
		Assert.DoesNotThrowAsync(async () => await dataGrid.SetColumnSize(1, 20));
		Assert.That(rendered.FindAll("th")[2].Attributes["style"].Value, Does.Contain("width: 20px;"));
	}

	[Test]
	public async Task UnderlyingDataObjectsNotAccessedByRenderThread()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var list = new NotThreadSafeList(ctx.WinzorDispatcher);
			list.Add(new NotThreadSafeObject(ctx.WinzorDispatcher) { Value = "One" });

			var dataGrid = new DataGrid() { ReadOnly = true, };
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = list;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle();
			var columnStyle = new DataGridTextBoxColumn { MappingName = "Value", HeaderText = "Value Header" };
			columnStyle.Width = 100;
			tableStyle.GridColumnStyles.Add(columnStyle);
			dataGrid.TableStyles.Add(tableStyle);

			return dataGrid;
		});

		var grid = rendered.Find(".datagrid");
		Assert.That(grid, Is.Not.Null);
		var tr = rendered.FindAll("tbody>tr");
		Assert.That(tr, Has.Count.EqualTo(1));
		Assert.That(tr[0].ChildNodes, Has.Length.EqualTo(2));
		Assert.That(tr[0].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));
		var readOnlyInput = tr[0].ChildNodes[1].ChildNodes[0].ChildNodes[0] as IHtmlInputElement;
		Assert.That(readOnlyInput.Value, Is.EqualTo("One"));
		Assert.That(readOnlyInput.HasAttribute("readonly"));
	}

	[Test]
	public async Task UnderlyingDataObjectsNotAccessedByRenderThreadEdit()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var list = new NotThreadSafeList(ctx.WinzorDispatcher);
			list.Add(new NotThreadSafeObject(ctx.WinzorDispatcher) { Value = "One" });

			var dataGrid = new DataGrid();
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = list;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle();
			var columnStyle = new DataGridTextBoxColumn { MappingName = "Value", HeaderText = "Value Header" };
			columnStyle.Width = 100;
			tableStyle.GridColumnStyles.Add(columnStyle);
			dataGrid.TableStyles.Add(tableStyle);

			return dataGrid;
		});

		await rendered.FindAll("tbody>tr")[0].Children[1].MouseDownAsync(new WebMouseEventArgs());
		var input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		Assert.That(rendered.FindAll("tbody>tr")[0].Children[1].Contains(input), Is.True);
		Assert.That(input.Value, Is.EqualTo("One"));
		await rendered.KeyPressAsync(Keys.A, input);
		input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		await input.TriggerEventAsync("oninput", new ChangeEventArgs { Value = "Two" });
		await input.FocusOutAsync();
		Assert.That(rendered.FindAll("tbody>tr")[0].Children[1].TextContent, Is.EqualTo("Two"));
	}

	[Test]
	public async Task UnderlyingDataObjectsNotAccessedByRenderThreadAdd()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var list = new NotThreadSafeList(ctx.WinzorDispatcher);
			list.Add(new NotThreadSafeObject(ctx.WinzorDispatcher) { Value = "One" });

			var dataGrid = new DataGrid();
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = list;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle();
			var columnStyle = new DataGridTextBoxColumn { MappingName = "Value", HeaderText = "Value Header" };
			columnStyle.Width = 100;
			tableStyle.GridColumnStyles.Add(columnStyle);
			dataGrid.TableStyles.Add(tableStyle);

			return dataGrid;
		});

		var input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		await input.FocusOutAsync();
		await SimulateClickOnCellForRowChange(rendered.FindAll("tbody>tr")[1].Children[1]);
		input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		Assert.That(rendered.FindAll("tbody>tr")[1].Children[1].Contains(input), Is.True);
		Assert.That(input.Value, Is.Empty);
		await rendered.KeyPressAsync(Keys.A, input);
		await rendered.Find("input").TriggerEventAsync("oninput", new ChangeEventArgs { Value = "Two" });
		await rendered.Find("input").FocusOutAsync();
		Assert.That(rendered.FindAll("tbody>tr")[1].Children[1].TextContent, Is.EqualTo("Two"));
	}

	[Test]
	public async Task GridShouldFireWinzorFocusIn()
	{
		using var ctx = new WinzorTestContext();
		bool isFocusFired = false;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var dataGrid = new DataGrid();
			dataGrid.Dock = DockStyle.Fill;
			var tableStyle = new DataGridTableStyle();
			var columnStyle = new DataGridTextBoxColumn();
			columnStyle.Width = 100;
			tableStyle.GridColumnStyles.Add(columnStyle);
			dataGrid.TableStyles.Add(tableStyle);
			dataGrid.GotFocus += (sender, e) => { isFocusFired = true; };
			return dataGrid;
		});

		rendered.Find(".datagrid").TriggerEvent("onwinzorfocusin", new WinzorFocusInEventArgs());
		Assert.That(isFocusFired, Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task GirdClickBlankAreaBehavior()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 400, Height = 300 };
			var table = new DataTable("data");
			var column1 = new DataColumn("one");
			column1.DataType = typeof(string);
			table.Columns.Add(column1);
			var column2 = new DataColumn("two");
			column2.DataType = typeof(string);
			column2.DefaultValue = "content test";
			table.Columns.Add(column2);

			var dataGrid = new DataGrid();
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnStyle1 = new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One Header" };
			columnStyle1.Width = 80;
			columnStyle1.ReadOnly = true;
			tableStyle.GridColumnStyles.Add(columnStyle1);
			var columnStyle2 = new DataGridTextBoxColumn { MappingName = "two", HeaderText = "Two Header" };
			columnStyle2.Width = 180;
			tableStyle.GridColumnStyles.Add(columnStyle2);
			dataGrid.TableStyles.Add(tableStyle);
			form.Controls.Add(dataGrid);
			form.Controls.Add(new TextBox { Dock = DockStyle.Bottom });

			return form;
		});

		var table = await page.WaitForSelectorAsync(".datagrid");
		Assert.That(async () => (await table.QuerySelectorAllAsync("tbody tr")).Count, Is.EqualTo(1).After(2000, 200));

		var firstcell = page.Locator("tr > td:nth-child(2)");
		var secondcell = page.Locator("tr > td:nth-child(3)");
		Assert.That(async () => await firstcell.TextContentAsync(), Is.EqualTo(string.Empty).After(2000, 200));
		Assert.That(async () => await secondcell.TextContentAsync(), Is.EqualTo("content test").After(2000, 200));

		await (page.Locator(".form > input")).ClickAsync();

		Assert.That(async () => (await table.QuerySelectorAllAsync("tbody tr")).Count, Is.EqualTo(1).After(2000, 200));
		Assert.That(async () => await firstcell.TextContentAsync(), Is.EqualTo(string.Empty).After(2000, 200));
		Assert.That(async () => await secondcell.TextContentAsync(), Is.EqualTo(string.Empty).After(2000, 200));

		await page.Mouse.ClickAsync(100, 150);

		Assert.That(async () => (await table.QuerySelectorAllAsync("tbody tr")).Count, Is.EqualTo(1).After(2000, 200));
		Assert.That(async () => await firstcell.TextContentAsync(), Is.EqualTo(string.Empty).After(2000, 200));
		Assert.That(async () => await secondcell.TextContentAsync(), Is.EqualTo("content test").After(2000, 200));
	}

	[Test, WithPlaywrightPage]
	public async Task NewRowBehavior()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var table = new DataTable("data");
			var column1 = new DataColumn("one");
			column1.DataType = typeof(string);
			table.Columns.Add(column1);
			var column2 = new DataColumn("two");
			column2.DataType = typeof(string);
			table.Columns.Add(column2);

			var dataGrid = new DataGrid();
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnStyle1 = new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One Header" };
			columnStyle1.Width = 80;
			tableStyle.GridColumnStyles.Add(columnStyle1);
			var columnStyle2 = new DataGridTextBoxColumn { MappingName = "two", HeaderText = "Two Header" };
			columnStyle2.Width = 180;
			tableStyle.GridColumnStyles.Add(columnStyle2);
			dataGrid.TableStyles.Add(tableStyle);
			form.Controls.Add(dataGrid);

			return form;
		});
		var table = await page.WaitForSelectorAsync("table");

		await page.Locator("td .textbox").PressSequentiallyAsync("111");
		Assert.That(async () => (await page.QuerySelectorAllAsync("tbody tr")).Count, Is.EqualTo(2).After(3000, 100));

		await (await page.QuerySelectorAllAsync("tbody tr td"))[2].ClickAsync();
		await (await page.QuerySelectorAllAsync("tbody tr td"))[1].WaitForSelectorAsync(".textbox", options: new ElementHandleWaitForSelectorOptions() { State = WaitForSelectorState.Detached });
		Assert.That((await page.QuerySelectorAllAsync("tbody tr")).Count, Is.EqualTo(2));

		await page.Locator("td .textbox").PressSequentiallyAsync("222");
		Assert.That((await page.QuerySelectorAllAsync("tbody tr")).Count, Is.EqualTo(2));

		await (await page.QuerySelectorAllAsync("tbody tr td"))[4].ClickAsync();
		await page.WaitForSelectorAsync("td .textbox");
		await (await page.QuerySelectorAllAsync("tbody tr td"))[1].ClickAsync();

		Assert.That((await page.QuerySelectorAllAsync("tbody tr")).Count, Is.EqualTo(2));
	}

	[Test, WithPlaywrightPage]
	public async Task DataGridColumnHeaderSpanTest()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
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

		var table = await page.WaitForSelectorAsync("table");

		var span = await page.WaitForSelectorAsync($"th span");
		Assert.That(async () => await
		span.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('font-size')"), Is.EqualTo("11px"));
		Assert.That(async () => await
		span.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('letter-spacing')"), Is.EqualTo("0.3px"));
	}

	[Test, WithPlaywrightPage]
	public async Task DataGridBorderLineShouldCollapseAndColorDeepGray()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();

			DataTable mockDataTable = new DataTable("data");
			var column1 = new DataColumn("one");
			column1.DataType = typeof(string);
			mockDataTable.Columns.Add(column1);
			var column2 = new DataColumn("two");
			column2.DataType = typeof(string);
			mockDataTable.Columns.Add(column2);
			mockDataTable.Rows.Add("cell11", "cell12");

			DataGrid dataGrid = new DataGrid();
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.ReadOnly = true;
			dataGrid.DataSource = mockDataTable;

			form.Controls.Add(dataGrid);
			return form;
		});

		const string DeepGrayBorderStyle = "1px solid rgb(214, 216, 217)";
		const string NoneBorderStyle = "0px none rgb(0, 0, 0)";

		var dataGrid = await page.WaitForSelectorAsync(".datagrid");
		Assert.That(async () => await dataGrid.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('border')"), Is.EqualTo(DeepGrayBorderStyle));

		var borderLineShouldCollpased = (IReadOnlyList<IElementHandle> list) =>
		{
			Assert.That(async () => await list[0].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('border-left')"), Is.EqualTo(NoneBorderStyle));

			Assert.That(async () => await list[1].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('border-left')"), Is.EqualTo(DeepGrayBorderStyle));
			Assert.That(async () => await list[1].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('border-bottom')"), Is.EqualTo(DeepGrayBorderStyle));

			Assert.That(async () => await list[2].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('border-left')"), Is.EqualTo(DeepGrayBorderStyle));
			Assert.That(async () => await list[2].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('border-bottom')"), Is.EqualTo(DeepGrayBorderStyle));
			Assert.That(async () => await list[2].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('border-right')"), Is.EqualTo(DeepGrayBorderStyle));
		};

		var thList = await dataGrid.QuerySelectorAllAsync("thead th");
		borderLineShouldCollpased(thList);

		var tdList = await dataGrid.QuerySelectorAllAsync("thead tr:first-child td");
		borderLineShouldCollpased(thList);
	}

	[Test, WithPlaywrightPage]
	public async Task TestSelectFullTextInTextBoxOfDataGridCellAfterClick([Values] bool readOnly)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Width = 600, Height = 500 };

			var mockDataTable = new DataTable("data");
			var column1 = new DataColumn("one");
			column1.DataType = typeof(string);
			mockDataTable.Columns.Add(column1);
			var column2 = new DataColumn("two");
			column2.DataType = typeof(string);
			mockDataTable.Columns.Add(column2);
			mockDataTable.Rows.Add("cell11", "cell12");

			var dataGrid = new DataGrid { Width = 500, Height = 300 };
			dataGrid.DataSource = mockDataTable;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnStyle1 = new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One Header" };
			columnStyle1.Width = 180;
			tableStyle.GridColumnStyles.Add(columnStyle1);
			var columnStyle2 = new DataGridTextBoxColumn { MappingName = "two", HeaderText = "Two Header" };
			columnStyle2.Width = 180;
			columnStyle2.ReadOnly = readOnly;
			tableStyle.GridColumnStyles.Add(columnStyle2);
			dataGrid.TableStyles.Add(tableStyle);

			form.Controls.Add(dataGrid);
			return form;
		});

		await page.WaitForSelectorAsync(".form");

		await page.Locator($".datagrid tbody>tr:nth-of-type({1})>td:nth-child({3})").ClickAsync();
		Assert.That(async () => await page.EvaluateAsync<string>("e => window.getSelection().toString()"), Is.EqualTo("cell12").After(3000, 100));

		await page.Locator($".datagrid tbody>tr:nth-of-type({1})>td:nth-child({2})").ClickAsync();
		Assert.That(async () => await page.EvaluateAsync<string>("e => window.getSelection().toString()"), Is.EqualTo("cell11").After(3000, 100));

		await page.Locator($".datagrid tbody>tr:nth-of-type({1})>td:nth-child({3})").ClickAsync();
		Assert.That(async () => await page.EvaluateAsync<string>("e => window.getSelection().toString()"), Is.EqualTo("cell12").After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestSelectTextAfterTextInputNodeAdded()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Width = 600, Height = 500 };

			var mockDataTable = new DataTable("data");
			var dataGrid = new DataGrid { Width = 500, Height = 300 };
			dataGrid.DataSource = mockDataTable;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			dataGrid.TableStyles.Add(tableStyle);

			form.Controls.Add(dataGrid);
			return form;
		});

		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
		await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
		await page.WaitForSelectorAsync(".form");

		var addInput = """
e => {
let i = document.createElement("input");
i.type = "text";
i.value = "test";
let grid = document.querySelector(".datagrid tbody");
grid.appendChild(i);
}
""";
		await page.Locator(".datagrid").EvaluateAsync(addInput);
		Assert.That(async () => await page.EvaluateAsync<string>("e => window.getSelection().toString()"), Is.EqualTo("test").After(3000, 100));
	}

	[Test]
	public async Task DataGridRowIndicator()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var table = new DataTable("data");
			var column1 = new DataColumn("one");
			column1.DataType = typeof(string);
			table.Columns.Add(column1);
			var column2 = new DataColumn("two");
			column2.DataType = typeof(string);
			table.Columns.Add(column2);
			table.Rows.Add("1.1", "1.2");

			var dataGrid = new DataGrid();
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnStyle1 = new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One Header" };
			columnStyle1.Width = 80;
			tableStyle.GridColumnStyles.Add(columnStyle1);
			var columnStyle2 = new DataGridTextBoxColumn { MappingName = "two", HeaderText = "Two Header" };
			columnStyle2.Width = 180;
			tableStyle.GridColumnStyles.Add(columnStyle2);
			dataGrid.TableStyles.Add(tableStyle);

			return dataGrid;
		});

		var tr = rendered.FindAll("tbody>tr");
		Assert.That(tr, Has.Count.EqualTo(2));
		Assert.That(tr[0].ChildNodes, Has.Length.EqualTo(3));
		Assert.That(tr[0].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));
		Assert.That(tr[0].Children[0].GetAttribute("class"), Is.Empty);
		Assert.That(tr[1].ChildNodes, Has.Length.EqualTo(3));
		Assert.That(tr[1].ChildNodes[0].TextContent, Is.EqualTo("*"));
		Assert.That(tr[1].Children[0].GetAttribute("class"), Is.Empty);

		await SimulateClickOnCellForRowChange(tr[1].Children[1]);
		Assert.That(tr[0].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));
		Assert.That(tr[0].Children[0].GetAttribute("class"), Is.Empty);

		var input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		await rendered.KeyPressAsync(Keys.A, input);
		tr = rendered.FindAll("tbody>tr");
		Assert.That(tr[1].ChildNodes[0].TextContent, Is.EqualTo("border_color"));
		Assert.That(tr[1].Children[0].GetAttribute("class"), Is.EqualTo("datagrid__indicator_small"));

		await input.FocusOutAsync();
		tr = rendered.FindAll("tbody>tr");
		Assert.That(tr, Has.Count.EqualTo(3));
		Assert.That(tr[0].Children[0].TextContent, Is.EqualTo(string.Empty));
		Assert.That(tr[1].Children[0].TextContent, Is.EqualTo("arrow_right"));
		Assert.That(tr[2].Children[0].TextContent, Is.EqualTo("*"));
	}

	[Test, WithPlaywrightPage]
	public async Task PressingTabKeyNoOutline()
	{
		await using var ctx = new InMemoryTestServerContext();
		var dataGrid = default(DataGrid);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(dataGrid = SetupDataGrid());
			return form;
		});

		await LoadPageAndForm(page);

		await page.ClickAsync($"tbody>tr >td:nth-of-type(2)", pageClickOptions);
		await page.WaitForSelectorAsync($"tbody>tr >td:nth-of-type(2) input", playWrightPageWaitForSelectorOptions);

		await page.Keyboard.PressAsync("Tab", keyboardPressOptions);

		var cell = await page.WaitForSelectorAsync($".datagrid tbody>tr>td:nth-of-type(3) input:focus", playWrightPageWaitForSelectorOptions);
		Assert.That(async () => await cell.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('outline')"), Does.Contain("none"));
	}

	[Test, WithPlaywrightPage]
	public async Task PressingTabKeyMoveOnTheSameRowNonReadonlyCell()
	{
		await using var ctx = new InMemoryTestServerContext();
		var dataGrid = default(DataGrid);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(dataGrid = SetupDataGrid());
			return form;
		});

		await LoadPageAndAssertKeyPress(page, dataGrid, "Tab", 1, 1, 1, 2);
	}

	[Test, WithPlaywrightPage]
	public async Task PressingTabKeyMoveOnTheSameRowSkippingReadonlyCell()
	{
		await using var ctx = new InMemoryTestServerContext();
		var dataGrid = default(DataGrid);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(dataGrid = SetupDataGrid());
			return form;
		});

		await LoadPageAndAssertKeyPress(page, dataGrid, "Tab", 1, 2, 1, 4);
	}

	[Test, WithPlaywrightPage]
	public async Task PressingTabKeyMoveOnTheNextRowNonReadonlyCell()
	{
		await using var ctx = new InMemoryTestServerContext();
		var dataGrid = default(DataGrid);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(dataGrid = SetupDataGrid());
			return form;
		});

		await LoadPageAndAssertKeyPress(page, dataGrid, "Tab", 1, 4, 2, 1);
	}

	[Test, WithPlaywrightPage]
	public async Task PressingTabKeyMoveOnTheNextRowSkippingReadonlyCell()
	{
		await using var ctx = new InMemoryTestServerContext();
		var dataGrid = default(DataGrid);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			dataGrid = SetupDataGrid();
			dataGrid.TableStyles[0].GridColumnStyles[3].ReadOnly = true;
			form.Controls.Add(dataGrid);
			return form;
		});

		await LoadPageAndAssertKeyPress(page, dataGrid, "Tab", 1, 2, 2, 1);
	}

	[Test, WithPlaywrightPage]
	public async Task PressingTabKeyMoveOnTheNextRowDataGridNewRow()
	{
		await using var ctx = new InMemoryTestServerContext();
		var dataGrid = default(DataGrid);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(dataGrid = SetupDataGrid());
			return form;
		});

		await LoadPageAndAssertKeyPress(page, dataGrid, "Tab", 4, 4, 5, 1, true);
	}

	[Test, WithPlaywrightPage]
	public async Task PressingTabKeyMoveOnTheSameRowDataGridNewRow()
	{
		await using var ctx = new InMemoryTestServerContext();
		var dataGrid = default(DataGrid);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(dataGrid = SetupDataGrid());
			return form;
		});

		await LoadPageAndAssertKeyPress(page, dataGrid, "Tab", 5, 1, 5, 2, true);
	}

	[Test, WithPlaywrightPage]
	public async Task PressingShiftTabKeyMoveOnTheSameRowNonReadonlyCell()
	{
		await using var ctx = new InMemoryTestServerContext();
		var dataGrid = default(DataGrid);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(dataGrid = SetupDataGrid());
			return form;
		});

		await LoadPageAndAssertKeyPress(page, dataGrid, "Shift+Tab", 1, 2, 1, 1);
	}

	[Test, WithPlaywrightPage]
	public async Task PressingShiftTabKeyMoveOnTheSameRowSkippingReadonlyCell()
	{
		await using var ctx = new InMemoryTestServerContext();
		var dataGrid = default(DataGrid);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(dataGrid = SetupDataGrid());
			return form;
		});

		await LoadPageAndAssertKeyPress(page, dataGrid, "Shift+Tab", 1, 4, 1, 2);
	}

	[Test, WithPlaywrightPage]
	public async Task PressingShiftTabKeyMoveOnTheNextRowNonReadonlyCell()
	{
		await using var ctx = new InMemoryTestServerContext();
		var dataGrid = default(DataGrid);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(dataGrid = SetupDataGrid());
			return form;
		});

		await LoadPageAndAssertKeyPress(page, dataGrid, "Shift+Tab", 2, 1, 1, 4);
	}

	[Test, WithPlaywrightPage]
	public async Task PressingShiftTabKeyMoveOnTheNextRowSkippingReadonlyCell()
	{
		await using var ctx = new InMemoryTestServerContext();
		var dataGrid = default(DataGrid);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			dataGrid = SetupDataGrid();
			dataGrid.TableStyles[0].GridColumnStyles[3].ReadOnly = true;
			form.Controls.Add(dataGrid);
			return form;
		});

		await LoadPageAndAssertKeyPress(page, dataGrid, "Shift+Tab", 2, 1, 1, 2);
	}

	[Test, WithPlaywrightPage]
	public async Task PressingShiftTabKeyMoveOnTheNextRowDataGridNewRow()
	{
		await using var ctx = new InMemoryTestServerContext();
		var dataGrid = default(DataGrid);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(dataGrid = SetupDataGrid());
			return form;
		});

		await LoadPageAndAssertKeyPress(page, dataGrid, "Shift+Tab", 5, 1, 4, 4);
	}

	[Test, WithPlaywrightPage]
	public async Task PressingShiftTabKeyMoveOnTheSameRowDataGridNewRow()
	{
		await using var ctx = new InMemoryTestServerContext();
		var dataGrid = default(DataGrid);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(dataGrid = SetupDataGrid());
			return form;
		});

		await LoadPageAndAssertKeyPress(page, dataGrid, "Shift+Tab", 5, 2, 5, 1, true);
	}

	[Test, WithPlaywrightPage, Explicit]
	public async Task PressingTabKeyOnControlBeforeDataGridSelectDataGridFirstCell()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(new TextBox() { Width = 100, Height = 20, Top = 0, Left = 300, Text = "BeforeDataGridControl" });
			form.Controls.Add(SetupDataGrid());
			form.Controls.Add(new TextBox() { Width = 100, Height = 20, Top = 120, Left = 300, Text = "AfterDataGridControl" });
			return form;
		});

		await page.BringToFrontAsync();

		var initialInput = await page.WaitForSelectorAsync("input:first-child", playWrightPageWaitForSelectorOptions);
		Assert.That(await initialInput.InputValueAsync(), Is.EqualTo("BeforeDataGridControl"));
		await initialInput.ClickAsync();
		await initialInput.FocusAsync();
		await page.Keyboard.PressAsync("Tab", keyboardPressOptions);
		var expectedFocusedControl = await page.WaitForSelectorAsync($".datagrid tbody>tr:nth-child({1})>td:nth-child({2}).datagrid__cell--edit input", playWrightPageWaitForSelectorOptions);
		Assert.That(await expectedFocusedControl.InputValueAsync(), Is.EqualTo("cell11"));
	}

	[Test, WithPlaywrightPage, Explicit]
	public async Task PressingTabKeyOnDataGridLastCellMoveToControlAfterDataGrid()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(new TextBox() { Width = 100, Height = 20, Top = 0, Left = 300, Text = "BeforeDataGridControl" });
			form.Controls.Add(SetupDataGrid());
			form.Controls.Add(new TextBox() { Width = 100, Height = 20, Top = 120, Left = 300, Text = "AfterDataGridControl" });
			return form;
		});

		await page.BringToFrontAsync();
		var initialCell = await page.WaitForSelectorAsync($".datagrid tbody>tr:nth-child({5})>td:nth-child({5})", playWrightPageWaitForSelectorOptions);
		await initialCell.ClickAsync();
		var editingInput = await page.WaitForSelectorAsync(".datagrid tbody>tr>td.datagrid__cell--edit input", playWrightPageWaitForSelectorOptions);
		await editingInput.FocusAsync();
		await page.Keyboard.PressAsync("Tab", keyboardPressOptions);
		var expectedFocusedControl = await page.WaitForSelectorAsync(":not(.datagrid) input:focus", playWrightPageWaitForSelectorOptions);
		Assert.That(await expectedFocusedControl.InputValueAsync(), Is.EqualTo("AfterDataGridControl"));
	}

	[Test, WithPlaywrightPage, Explicit]
	public async Task PressingShiftTabKeyOnDataGridFirstCellMoveToControlBeforeDataGrid()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(new TextBox() { Width = 100, Height = 20, Top = 0, Left = 300, Text = "BeforeDataGridControl" });
			form.Controls.Add(SetupDataGrid());
			form.Controls.Add(new TextBox() { Width = 100, Height = 20, Top = 120, Left = 300, Text = "AfterDataGridControl" });
			return form;
		});

		await page.BringToFrontAsync();
		var initialCell = await page.WaitForSelectorAsync($".datagrid tbody>tr:nth-child({1})>td:nth-child({2})", playWrightPageWaitForSelectorOptions);
		await initialCell.ClickAsync();
		var editingInput = await page.WaitForSelectorAsync(".datagrid tbody>tr>td.datagrid__cell--edit input", playWrightPageWaitForSelectorOptions);
		await editingInput.FocusAsync();
		await page.Keyboard.PressAsync("Shift+Tab", keyboardPressOptions);
		var expectedFocusedControl = await page.WaitForSelectorAsync(":not(.datagrid) input:focus", playWrightPageWaitForSelectorOptions);
		Assert.That(await expectedFocusedControl.InputValueAsync(), Is.EqualTo("BeforeDataGridControl"));
	}

	[Test, WithPlaywrightPage, Explicit]
	public async Task PressingShiftTabKeyOnControlAfterDataGridSelectDataGridLastCell()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(new TextBox() { Width = 100, Height = 20, Top = 0, Left = 300, Text = "BeforeDataGridControl" });
			form.Controls.Add(SetupDataGrid());
			form.Controls.Add(new TextBox() { Width = 100, Height = 20, Top = 120, Left = 300, Text = "AfterDataGridControl" });
			return form;
		});

		var initialInput = await page.WaitForSelectorAsync("input:last-child", playWrightPageWaitForSelectorOptions);
		Assert.That(await initialInput.InputValueAsync(), Is.EqualTo("AfterDataGridControl"));
		await initialInput.ClickAsync();
		await initialInput.FocusAsync();
		await page.Keyboard.PressAsync("Shift+Tab", keyboardPressOptions);
		var expectedFocusedControl = await page.WaitForSelectorAsync($".datagrid tbody>tr:nth-child({5})>td:nth-child({5}).datagrid__cell--edit input", playWrightPageWaitForSelectorOptions);
		Assert.That(await expectedFocusedControl.InputValueAsync(), Is.EqualTo(string.Empty));
	}

	[Test, WithPlaywrightPage, Explicit] // Explicit until WI00537341
	public async Task PressingCtrlTabKeyOnDataGridSelectControlAfterDataGrid()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(new TextBox() { Width = 100, Height = 20, Top = 0, Left = 300, Text = "BeforeDataGridControl" });
			form.Controls.Add(SetupDataGrid());
			form.Controls.Add(new TextBox() { Width = 100, Height = 20, Top = 120, Left = 300, Text = "AfterDataGridControl" });
			return form;
		});
		var initialCell = await page.WaitForSelectorAsync($".datagrid tbody>tr:nth-child({2})>td:nth-child({2})", playWrightPageWaitForSelectorOptions);
		await initialCell.ClickAsync();
		var editingInput = await page.WaitForSelectorAsync(".datagrid tbody>tr>td.datagrid__cell--edit input", playWrightPageWaitForSelectorOptions);
		await editingInput.FocusAsync();
		await page.Keyboard.PressAsync("Control+Tab", keyboardPressOptions);
		await Task.Delay(TimeSpan.FromMinutes(3));
		var expectedFocusedControl = await page.WaitForSelectorAsync($"body > div > input:focus", playWrightPageWaitForSelectorOptions);
		Assert.That(await expectedFocusedControl.InputValueAsync(), Is.EqualTo("AfterDataGridControl"));
	}

	[Test, WithPlaywrightPage, Explicit] // Explicit until WI00537341
	public async Task PressingCtrlShiftTabKeyOnDataGridSelectControlBeforeDataGrid()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(new TextBox() { Width = 100, Height = 20, Top = 0, Left = 300, Text = "BeforeDataGridControl" });
			form.Controls.Add(SetupDataGrid());
			form.Controls.Add(new TextBox() { Width = 100, Height = 20, Top = 120, Left = 300, Text = "AfterDataGridControl" });
			return form;
		});

		var initialCell = await page.WaitForSelectorAsync($".datagrid tbody>tr:nth-child({2})>td:nth-child({2})", playWrightPageWaitForSelectorOptions);
		await initialCell.ClickAsync();
		var editingInput = await page.WaitForSelectorAsync(".datagrid tbody>tr>td.datagrid__cell--edit input", playWrightPageWaitForSelectorOptions);
		await editingInput.FocusAsync();
		await page.Keyboard.PressAsync("Control+Shift+Tab", keyboardPressOptions);
		var expectedFocusedControl = await page.WaitForSelectorAsync($"body > div > input:focus", playWrightPageWaitForSelectorOptions);
		Assert.That(await expectedFocusedControl.InputValueAsync(), Is.EqualTo("BeforeDataGridControl"));
	}

	[Test, WithPlaywrightPage, Explicit]
	[SuppressMessage("CargoWiseOne", "CW1104:DoNotUseSystemWindowsTabControl", Justification = "Testing")]
	public async Task PressingTabKeyOnControlBeforeDataGridSelectDataGridFirstCellWithinTabControlAndGroupBox()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var tabControl = new TabControl() { Width = 1000, Height = 500, Top = 0, Left = 0 };
			var tabPage = new TabPage { Name = "TabPage", Text = "The Tab page" };
			tabControl.TabPages.Add(tabPage);
			var groupBox = new GroupBox() { Width = 1000, Height = 300, Top = 0, Left = 0 };
			groupBox.Controls.Add(SetupDataGrid());
			tabPage.Controls.Add(groupBox);
			form.Controls.Add(tabControl);
			return form;
		});
		var initialFocusedElement = await page.WaitForSelectorAsync(".tabcontrol .tabcontrol__button.active", playWrightPageWaitForSelectorOptions);
		Assert.That(await initialFocusedElement.TextContentAsync(), Is.EqualTo("The Tab page"));
		await initialFocusedElement.ClickAsync();
		await initialFocusedElement.FocusAsync();
		await page.Keyboard.PressAsync("Tab", keyboardPressOptions);
		var expectedFocusedControl = await page.WaitForSelectorAsync($".datagrid tbody>tr:nth-child({1})>td:nth-child({2}).datagrid__cell--edit input", playWrightPageWaitForSelectorOptions);
		Assert.That(await expectedFocusedControl.InputValueAsync(), Is.EqualTo("cell11"));
	}

	[Test, WithPlaywrightPage, Explicit]
	[SuppressMessage("CargoWiseOne", "CW1104:DoNotUseSystemWindowsTabControl", Justification = "Testing")]
	public async Task PressingShiftTabKeyOnControlAfterDataGridSelectDataGridLastCellWithinTabControlAndGroupBox()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var tabControl = new TabControl() { Width = 1000, Height = 500, Top = 0, Left = 0 };
			var tabPage = new TabPage { Name = "TabPage", Text = "The Tab page" };
			tabControl.TabPages.Add(tabPage);
			var groupBox = new GroupBox() { Width = 1000, Height = 300, Top = 0, Left = 0 };
			groupBox.Controls.Add(SetupDataGrid());
			tabPage.Controls.Add(groupBox);
			form.Controls.Add(tabControl);
			form.Controls.Add(new TextBox());
			return form;
		});

		await page.FocusAsync("input");
		await page.Keyboard.PressAsync("Shift+Tab", keyboardPressOptions);
		var expectedFocusedControl = await page.WaitForSelectorAsync($".datagrid tbody>tr:nth-child({5})>td:nth-child({5}).datagrid__cell--edit input", playWrightPageWaitForSelectorOptions);
		Assert.That(await expectedFocusedControl.InputValueAsync(), Is.EqualTo(string.Empty));
	}

	[Test, WithPlaywrightPage]
	public async Task PressingCtrlAltTabKeyOnDataGridIsIgnored()
	{
		await using var ctx = new InMemoryTestServerContext();
		var dataGrid = default(DataGrid);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(dataGrid = SetupDataGrid());
			return form;
		});

		await LoadPageAndAssertKeyPress(page, dataGrid, "Control+Alt+Tab", 2, 1, 2, 1);
	}

	[Test, WithPlaywrightPage, Explicit]
	public async Task PressingTabKeyOnControlBeforeReadonlyDataGridSelectControlAfterDataGrid()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(new TextBox() { Width = 100, Height = 20, Top = 0, Left = 300, Text = "BeforeDataGridControl" });
			var dataGrid = SetupDataGrid();
			dataGrid.ReadOnly = true;
			form.Controls.Add(dataGrid);
			form.Controls.Add(new TextBox() { Width = 100, Height = 20, Top = 120, Left = 300, Text = "AfterDataGridControl" });
			return form;
		});

		var initialInput = await page.WaitForSelectorAsync("input:first-child", playWrightPageWaitForSelectorOptions);
		Assert.That(await initialInput.InputValueAsync(), Is.EqualTo("BeforeDataGridControl"));
		await initialInput.ClickAsync();
		await initialInput.FocusAsync();
		await page.Keyboard.PressAsync("Tab", keyboardPressOptions);
		var expectedFocusedControl = await page.WaitForSelectorAsync($":not(.datagrid) input:focus", playWrightPageWaitForSelectorOptions);
		Assert.That(await expectedFocusedControl.InputValueAsync(), Is.EqualTo("AfterDataGridControl"));
	}

	[Test, WithPlaywrightPage]
	[WithDefaultLatencyNetworkEffect(Latency = 300)]
	public async Task RapidPressTabKeyShouldNotRowBlinkAsync()
	{
		await using var ctx = new InMemoryTestServerContext();
		var dataGrid = default(DataGrid);
		var page = await ctx.LoadControlOnFormAsync(() => dataGrid = CreateNewGrid<DataGrid>(10));
		await page.EvaluateAsync(@"() => {
			window.countList = [];
			const observer = new MutationObserver((mutations) => {
				const count = document.querySelectorAll('.datagrid__row').length;
				window.countList.push(count);
			});
			observer.observe(document.querySelector('tbody'), { 
				childList: true
			});
		}");
		for (var j = 0; j < 40; j++)
		{
			await page.Keyboard.PressAsync("Tab");
		}
		for (var j = 0; j < 40; j++)
		{
			await page.Keyboard.PressAsync("Shift+Tab");
		}

		var countList = await page.EvaluateAsync<int[]>("() => window.countList");
		var countHashSet = new HashSet<int>(countList);
		Assert.That(countHashSet, Has.Count.EqualTo(1));
	}

	static DataGrid SetupDataGrid(bool withReadOnlyColumnStyle = false)
	{
		var mockDataTable = new DataTable("data");
		var column1 = new DataColumn("one");
		column1.DataType = typeof(string);
		mockDataTable.Columns.Add(column1);
		var column2 = new DataColumn("two");
		column2.DataType = typeof(string);
		mockDataTable.Columns.Add(column2);
		var column3 = new DataColumn("three");
		column3.DataType = typeof(string);
		mockDataTable.Columns.Add(column3);
		var column4 = new DataColumn("four");
		column4.DataType = typeof(string);
		mockDataTable.Columns.Add(column4);
		mockDataTable.Rows.Add("cell11", "cell12", "cell13", "cell14");
		mockDataTable.Rows.Add("cell21", "cell22", "cell23", "cell24");
		mockDataTable.Rows.Add("cell31", "cell32", "cell33", "cell34");
		mockDataTable.Rows.Add("cell41", "cell42", "cell43", "cell44");

		var dataGrid = new DataGrid();
		dataGrid.Dock = DockStyle.Fill;
		dataGrid.DataSource = mockDataTable;
		dataGrid.AllowNavigation = false;
		var tableStyle = new DataGridTableStyle { MappingName = "data" };
		var columnStyle1 = new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One Header", ReadOnly = withReadOnlyColumnStyle };
		columnStyle1.Width = 80;
		tableStyle.GridColumnStyles.Add(columnStyle1);
		var columnStyle2 = new DataGridTextBoxColumn { MappingName = "two", HeaderText = "Two Header" };
		columnStyle2.Width = 180;
		tableStyle.GridColumnStyles.Add(columnStyle2);
		var columnStyle3 = new DataGridTextBoxColumn { MappingName = "three", HeaderText = "Three Header" };
		columnStyle3.Width = 280;
		columnStyle3.ReadOnly = true;
		tableStyle.GridColumnStyles.Add(columnStyle3);
		var columnStyle4 = new DataGridTextBoxColumn { MappingName = "four", HeaderText = "Four Header", ReadOnly = withReadOnlyColumnStyle };
		columnStyle4.Width = 380;
		tableStyle.GridColumnStyles.Add(columnStyle4);
		dataGrid.TableStyles.Add(tableStyle);

		return dataGrid;
	}

	static async Task AssertRowsSelectedAfterKeyPress(IPage page, int editRow, string key, int[] selectedRows)
	{
		await page.ClickAsync($"tbody>tr:nth-of-type({editRow})>td:nth-child(2)", pageClickOptions);
		await page.Keyboard.PressAsync(key, keyboardPressOptions);

		Assert.That((await page.QuerySelectorAllAsync("tbody>tr.datagrid__row--selected")).Count, Is.EqualTo(selectedRows.Length));

		foreach (var row in selectedRows)
		{
			var tr = await page.WaitForSelectorAsync($"tbody>tr:nth-of-type({row})", playWrightPageWaitForSelectorOptions);
			Assert.That(await tr.GetAttributeAsync("class"), Does.Contain("datagrid__row--selected"));
		}
	}

	static async Task AssertFocusedCellLocationAfterKeyPress(IPage page, DataGrid dataGrid, int initialCellRow, int initialCellColumn, string key, int expectedNextCellRow, int expectedNextCellColumn, bool expectedCellIsEmpty = false)
	{
		await page.ClickAsync($"tbody>tr:nth-of-type({initialCellRow})>td:nth-of-type({initialCellColumn + 1})", pageClickOptions);
		await page.WaitForSelectorAsync($"tbody>tr:nth-of-type({initialCellRow})>td:nth-of-type({initialCellColumn + 1}) input", playWrightPageWaitForSelectorOptions);
		await page.Keyboard.PressAsync(key, keyboardPressOptions);

		var expectedNextCellInput = await page.WaitForSelectorAsync($"tbody>tr:nth-of-type({expectedNextCellRow})>td:nth-of-type({expectedNextCellColumn + 1}).datagrid__cell--edit input", playWrightPageWaitForSelectorOptions);
		if (expectedCellIsEmpty)
		{
			Assert.That(await expectedNextCellInput.InputValueAsync(), Is.EqualTo(string.Empty));
		}
		else
		{
			Assert.That(await expectedNextCellInput.InputValueAsync(), Is.EqualTo($"cell{expectedNextCellRow}{expectedNextCellColumn}"));
		}
	}

	static async Task LoadPageAndForm(IPage page)
	{
		await page.BringToFrontAsync();
		await page.WaitForSelectorAsync(".form");
	}

	static async Task LoadPageAndAssertKeyPress(IPage page, DataGrid dataGrid, string key, int initialCellRow, int initialCellColumn, int expectedNextCellRow, int expectedNextCellColumn, bool expectedCellIsEmpty = false)
	{
		await LoadPageAndForm(page);
		await AssertFocusedCellLocationAfterKeyPress(page, dataGrid, initialCellRow, initialCellColumn, key, expectedNextCellRow, expectedNextCellColumn, expectedCellIsEmpty);
	}

	static readonly PageClickOptions pageClickOptions = new PageClickOptions() { Timeout = 7000 };
	static readonly PageWaitForSelectorOptions playWrightPageWaitForSelectorOptions = new PageWaitForSelectorOptions { Timeout = 8000 };
	static readonly KeyboardPressOptions keyboardPressOptions = new KeyboardPressOptions { Delay = 500 };

	[Test]
	public async Task DataGridCollectionItemModifiedRerendersRow()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, grid) = await TestGridAsync(ctx);

		var rows = rendered.FindAll("tbody>tr");
		Assert.That(rows, Has.Count.EqualTo(5));
		var dataSource = (DataTable)grid.DataSource;
		await grid.InvokeWinzorDispatcherAsync(() =>
		{
			dataSource.Rows[3][0] = "Updated Row 4";
		});
		rows = rendered.FindAll("tbody>tr");
		Assert.That(rows, Has.Count.EqualTo(5));
		Assert.That(rows[3].ChildNodes[1].TextContent, Is.EqualTo("Updated Row 4"));
	}

	[Test]
	public async Task DataGridCollectionItemAddedRerendersGrid()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, grid) = await TestGridAsync(ctx);
		var rows = rendered.FindAll("tbody>tr");
		Assert.That(rows, Has.Count.EqualTo(5));
		var dataSource = (DataTable)grid.DataSource;
		await grid.InvokeWinzorDispatcherAsync(() =>
		{
			dataSource.Rows.Add("row5", "55");
		});
		rows = rendered.FindAll("tbody>tr");
		Assert.That(rows, Has.Count.EqualTo(6));
	}

	[Test]
	public async Task DataGridCollectionItemRemovedRerendersGrid()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, grid) = await TestGridAsync(ctx);
		var rows = rendered.FindAll("tbody>tr");
		Assert.That(rows, Has.Count.EqualTo(5));
		var dataSource = (DataTable)grid.DataSource;
		await grid.InvokeWinzorDispatcherAsync(() =>
		{
			dataSource.Rows.Remove(dataSource.Rows[3]);
		});
		rows = rendered.FindAll("tbody>tr");
		Assert.That(rows, Has.Count.EqualTo(4));
	}

	[Test]
	public async Task DataGridRowStyleStringAppliedToCells()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			return DataGridWithCustomColors();
		});
		var cells = rendered.FindAll("tr>td");
		Assert.That(cells.Where(c => c.GetAttribute("style") == "background-color: #FF0000FF;").Count, Is.EqualTo(4));
		Assert.That(cells.Where(c => c.GetAttribute("style") == "background-color: #0000FFFF;").Count, Is.EqualTo(4));
	}

	[Test, WithPlaywrightPage]
	public async Task DataGridRowSelectionColorOverridesCustomColor()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(DataGridWithCustomColors());
			return form;
		});
		var selectedColor = Color.FromArgb(255, 153, 180, 209);

		var dataGrid = await page.WaitForSelectorAsync(".datagrid");
		await (await page.WaitForSelectorAsync("input")).EvaluateAsync("e => e.blur()");

		await AssertCellColor(page, 1, 2, Color.Red);
		await AssertCellColor(page,1, 3, Color.Blue);
		await AssertCellColor(page,2, 2, Color.Red);
		await AssertCellColor(page,2, 3, Color.Blue);
		await page.Locator($".datagrid tbody>tr:nth-of-type({1})>td:nth-child({1})").ClickAsync();
		await AssertCellColor(page,1, 2, selectedColor);
		await AssertCellColor(page,1, 3, selectedColor);
		await AssertCellColor(page,2, 2, Color.Red);
		await AssertCellColor(page,2, 3, Color.Blue);
		await page.Locator($".datagrid tbody>tr:nth-of-type({2})>td:nth-child({1})").ClickAsync();
		await AssertCellColor(page,1, 2, Color.Red);
		await AssertCellColor(page,1, 3, Color.Blue);
		await AssertCellColor(page,2, 2, selectedColor);
		await AssertCellColor(page,2, 3, selectedColor);
	}

	[Test]
	public async Task DataGridRowStyleChangeInUnitOfWorkAfterOtherGridEvents()
	{
		using var ctx = new WinzorTestContext();
		DataGrid dataGrid = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			return dataGrid = DataGridWithCustomColors();
		});
		var cells = rendered.FindAll("tr>td");
		Assert.That(cells.Where(c => c.GetAttribute("style") == "background-color: #FF0000FF;").Count, Is.EqualTo(4));
		Assert.That(cells.Where(c => c.GetAttribute("style") == "background-color: #0000FFFF;").Count, Is.EqualTo(4));
		await dataGrid.InvokeWinzorDispatcherAsync(() =>
		{
			var table = (DataTable)dataGrid.DataSource;
			table.Rows.Clear();
			table.Rows.Add("newrow1", "11");
			((DataGridColumnCustomColor)dataGrid.TableStyles[0].GridColumnStyles[0]).BackColor = Color.Green;
		});
		cells = rendered.FindAll("tr>td");
		var styles = cells.Select(c => c.GetAttribute("style")).ToArray();
		Assert.That(cells.Where(c => c.GetAttribute("style") == "background-color: #008000FF;").Count, Is.EqualTo(1));
		Assert.That(cells.Where(c => c.GetAttribute("style") == "background-color: #0000FFFF;").Count, Is.EqualTo(1));
	}

	[Test]
	public async Task DataGridRowStyleChangeInUnitOfWorkAfterInvalidateRow()
	{
		using var ctx = new WinzorTestContext();
		DataGrid dataGrid = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			dataGrid = DataGridWithCustomColors();
			dataGrid.ReadOnly = true;
			return dataGrid;
		});
		var cells = rendered.FindAll("tr>td");
		Assert.That(cells.Where(c => c.GetAttribute("style") == "background-color: #FF0000FF;").Count, Is.EqualTo(4));
		Assert.That(cells.Where(c => c.GetAttribute("style") == "background-color: #0000FFFF;").Count, Is.EqualTo(4));
		await dataGrid.InvokeWinzorDispatcherAsync(() =>
		{
			dataGrid.CurrentCell = new DataGridCell(2, 1);
			((DataGridColumnCustomColor)dataGrid.TableStyles[0].GridColumnStyles[0]).BackColor = Color.Green;
		});
		cells = rendered.FindAll("tr>td");
		var styles = cells.Select(c => c.GetAttribute("style")).ToArray();
		Assert.That(cells.Where(c => c.GetAttribute("style") == "background-color: #008000FF;").Count, Is.EqualTo(2), "Invalidated rows should be recreated with new color");
		Assert.That(cells.Where(c => c.GetAttribute("style") == "background-color: #0000FFFF;").Count, Is.EqualTo(4));
	}

	async Task AssertCellColor(IPage page, int row, int column, Color color)
	{
		await page.WaitForFunctionAsync($"window.getComputedStyle(document.querySelector('.datagrid tbody>tr:nth-of-type({row})>:nth-child({column})')).getPropertyValue('background-color') === 'rgb({color.R}, {color.G}, {color.B})'");
	}

	DataGrid DataGridWithCustomColors()
	{
		var dataGrid = new DataGrid();
		dataGrid.Dock = DockStyle.Fill;
		dataGrid.DataSource = MockGridData();
		dataGrid.AllowNavigation = false;
		var tableStyle = new DataGridTableStyle { MappingName = "data" };
		var columnStyle1 = new DataGridColumnCustomColor { MappingName = "one", HeaderText = "One Header", BackColor = Color.Red };
		columnStyle1.Width = 80;
		tableStyle.GridColumnStyles.Add(columnStyle1);
		var columnStyle2 = new DataGridColumnCustomColor { MappingName = "two", HeaderText = "Two Header", BackColor = Color.Blue };
		columnStyle2.Width = 180;
		tableStyle.GridColumnStyles.Add(columnStyle2);
		dataGrid.TableStyles.Add(tableStyle);
		return dataGrid;
	}

	[TestCaseSource(nameof(HitTestCases), new object[] { false })]
	public async Task ContextMenuPopupHitTest(string cssSelector, DataGrid.HitTestInfo expectedHit)
	{
		using var ctx = new WinzorTestContext();
		var (rendered, grid) = await TestGridAsync(ctx);
		DataGrid.HitTestInfo hitTest = null;
		await grid.InvokeWinzorDispatcherAsync(() =>
		{
			grid.ContextMenu = new ContextMenu();
			grid.ContextMenu.MenuItems.Add("Test");
			grid.ContextMenu.Popup += (s, e) => hitTest = grid.HitTest(Control.MousePosition);
		});
		await rendered.Find(cssSelector).TriggerEventAsync("oncontextmenu", new WebMouseEventArgs { Button = 2, Type = "contextmenu" });
		Assert.Multiple(() =>
		{
			Assert.That(hitTest.Type, Is.EqualTo(expectedHit.Type));
			Assert.That(hitTest.Column, Is.EqualTo(expectedHit.Column));
			Assert.That(hitTest.Row, Is.EqualTo(expectedHit.Row));
		});
	}

	[TestCaseSource(nameof(HitTestCases), new object[] { true })]
	public async Task ContextMenuPopupHitTestReadonlyGrid(string cssSelector, DataGrid.HitTestInfo expectedHit)
	{
		using var ctx = new WinzorTestContext();
		var (rendered, grid) = await TestGridAsync(ctx);
		DataGrid.HitTestInfo hitTest = null;
		await grid.InvokeWinzorDispatcherAsync(() =>
		{
			grid.ReadOnly = true;
			grid.ContextMenu = new ContextMenu();
			grid.ContextMenu.MenuItems.Add("Test");
			grid.ContextMenu.Popup += (s, e) => hitTest = grid.HitTest(Control.MousePosition);
		});

		await rendered.Find(cssSelector).TriggerEventAsync("oncontextmenu", new WebMouseEventArgs { Button = 2, Type = "contextmenu" });
		Assert.Multiple(() =>
		{
			Assert.That(hitTest.Type, Is.EqualTo(expectedHit.Type));
			Assert.That(hitTest.Column, Is.EqualTo(expectedHit.Column));
			Assert.That(hitTest.Row, Is.EqualTo(expectedHit.Row));
		});
	}

	[Test]
	public async Task RepeatedlyOpeningDataGridContextMenuDoesNotThrowException()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, grid) = await TestGridAsync(ctx);
		DataGrid.HitTestInfo hitTest = null;
		await grid.InvokeWinzorDispatcherAsync(() =>
		{
			grid.ReadOnly = true;
			grid.ContextMenu = new ContextMenu();
			grid.ContextMenu.MenuItems.Add("Test");
			grid.ContextMenu.Popup += (s, e) => hitTest = grid.HitTest(Control.MousePosition);
		});

		var element = rendered.Find("tbody>tr:nth-of-type(1)>td:nth-of-type(2)");
		for (var i = 0; i < 20; i++)
		{
			Assert.DoesNotThrow(() =>
				element.TriggerEvent("oncontextmenu", new WebMouseEventArgs { Button = 2, Type = "contextmenu" }),
				$"Failed on iteration {i}");
		}
	}

	[Test]
	public async Task ColumnAutoResize()
	{
		DataGrid dataGrid = null;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			dataGrid = new DataGrid() { Height = 300 };
			var table = new DataTable("data");
			var tableStyle = new DataGridTableStyle { MappingName = "data" };

			var column1 = new DataColumn("one");
			column1.DataType = typeof(string);
			table.Columns.Add(column1);

			var column2 = new DataColumn("two");
			column2.DataType = typeof(string);
			table.Columns.Add(column2);

			var columnStyle1 = new DataGridTextBoxColumn { MappingName = "one", HeaderText = "Short Header" };
			columnStyle1.Width = 20;
			tableStyle.GridColumnStyles.Add(columnStyle1);

			var columnStyle2 = new DataGridTextBoxColumn { MappingName = "two", HeaderText = "Long Header" };
			columnStyle2.Width = 300;
			tableStyle.GridColumnStyles.Add(columnStyle2);

			dataGrid.DataSource = table;
			dataGrid.TableStyles.Add(tableStyle);

			return dataGrid;
		});

		var grid = rendered.Find(".datagrid");
		var rows = rendered.FindAll("tbody>tr");
		var headers = rendered.FindAll("thead>tr>th");
		var resizers = rendered.FindAll(".datagrid__column_resizer");
		var webMouseEventArgs = new WebMouseEventArgs() { Button = 0, Detail = 2 };

		Assert.That(rows, Has.Count.EqualTo(1));
		Assert.That(headers, Has.Count.EqualTo(3));
		Assert.That(resizers, Has.Count.EqualTo(2));

		await resizers[0].TriggerEventAsync("onmousedown", webMouseEventArgs);
		await resizers[1].TriggerEventAsync("onmousedown", webMouseEventArgs);

		headers = rendered.FindAll("thead>tr>th");
		Assert.That(headers[1].GetAttribute("style"), Does.Contain($"width: 89px;"));
		Assert.That(headers[2].GetAttribute("style"), Does.Contain($"width: 87px;"));

		var dataSource = (DataTable)dataGrid.DataSource;
		await dataGrid.InvokeWinzorDispatcherAsync(() =>
		{
			dataSource.Rows.Add("short", "long long long long long long long long long ");
			dataSource.Rows.Add("short short", "long long long long long long long long long");
			dataSource.Rows.Add("short", "long long long");
		});

		rows = rendered.FindAll("tbody>tr");
		Assert.That(rows, Has.Count.EqualTo(4));

		await resizers[0].TriggerEventAsync("onmousedown", webMouseEventArgs);
		await resizers[1].TriggerEventAsync("onmousedown", webMouseEventArgs);

		headers = rendered.FindAll("thead>tr>th");
		Assert.That(headers[1].GetAttribute("style"), Does.Contain($"width: 89px;"));
		Assert.That(headers[2].GetAttribute("style"), Does.Contain($"width: 222px;"));
	}

	[Test]
	public async Task ReadOnlyUpdateFromServer()
	{
		await ControlAssert.ImplementsChangeFromServerAsync<DataGrid>(dataGrid => dataGrid.ReadOnly = true);
	}

	[TestCaseSource(nameof(HitTestCases), new object[] { true })]
	public async Task DoubleClickHitTestReadOnlyGrid(string cssSelector, DataGrid.HitTestInfo expectedHit)
	{
		using var ctx = new WinzorTestContext();
		var (rendered, grid) = await TestGridAsync(ctx);
		DataGrid.HitTestInfo hitTest = null;
		await grid.InvokeWinzorDispatcherAsync(() =>
		{
			grid.ReadOnly = true;
			grid.MouseDown += (s, e) =>
			{
				if (e.Clicks == 2)
				{
					hitTest = grid.HitTest(e.Location);
				}
			};
		});

		await rendered.Find(cssSelector).MouseDownAsync(new WebMouseEventArgs() { Detail = 2 });
		Assert.Multiple(() =>
		{
			Assert.That(hitTest.Type, Is.EqualTo(expectedHit.Type));
			Assert.That(hitTest.Column, Is.EqualTo(expectedHit.Column));
			Assert.That(hitTest.Row, Is.EqualTo(expectedHit.Row));
		});
	}

	[TestCaseSource(nameof(HitTestCases), new object[] { false })]
	public async Task DoubleClickHitTestEditableGrid(string cssSelector, DataGrid.HitTestInfo expectedHit)
	{
		using var ctx = new WinzorTestContext();
		var (rendered, grid) = await TestGridAsync(ctx);
		DataGrid.HitTestInfo hitTest = null;
		await grid.InvokeWinzorDispatcherAsync(() =>
		{
			grid.MouseDown += (s, e) =>
			{
				if (e.Clicks == 2)
				{
					hitTest = grid.HitTest(e.Location);
				}
			};
		});

		await rendered.Find(cssSelector).MouseDownAsync(new WebMouseEventArgs() { Detail = 2 });
		Assert.Multiple(() =>
		{
			Assert.That(hitTest.Type, Is.EqualTo(expectedHit.Type));
			Assert.That(hitTest.Column, Is.EqualTo(expectedHit.Column));
			Assert.That(hitTest.Row, Is.EqualTo(expectedHit.Row));
		});
	}

	[Test]
	public async Task DoubleClickCallsOnDoubleClick()
	{
		await ControlAssert.ImplementsProtectedOnMethodAsync<DataGrid, EventArgs>("OnDoubleClick", dataGrid => InitializeTestGrid(dataGrid), "tr:nth-of-type(3)>td:nth-of-type(1)", e => e.MouseDown(2));
	}

	[Test]
	public async Task DoubleClickCallsOnMouseDoubleClick()
	{
		await ControlAssert.ImplementsProtectedOnMethodAsync<DataGrid, MouseEventArgs>("OnMouseDoubleClick", dataGrid => InitializeTestGrid(dataGrid), "tr:nth-of-type(3)>td:nth-of-type(1)", e => e.MouseDown(2));
	}

	[Test]
	public async Task DoubleClickRaisesDoubleClickEvent()
	{
		await ControlAssert.ImplementsEventAsync<DataGrid, EventHandler>(nameof(DataGrid.DoubleClick), dataGrid => InitializeTestGrid(dataGrid), a => new EventHandler((s, e) => a()), "tr:nth-of-type(3)>td:nth-of-type(1)", e => e.MouseDown(2));
	}

	static IEnumerable<TestCaseData> HitTestCases(bool isReadOnly)
	{
		yield return new TestCaseData("thead>tr:nth-of-type(1)>th:nth-of-type(1)", new DataGrid.HitTestInfo() { type = DataGrid.HitTestType.RowHeader | DataGrid.HitTestType.ColumnHeader, col = -1, row = -1 });
		yield return new TestCaseData("thead>tr:nth-of-type(1)>th:nth-of-type(2)", new DataGrid.HitTestInfo() { type = DataGrid.HitTestType.ColumnHeader, col = 0, row = -1 });
		yield return new TestCaseData("thead>tr:nth-of-type(1)>th:nth-of-type(3)", new DataGrid.HitTestInfo() { type = DataGrid.HitTestType.ColumnHeader, col = 1, row = -1 });
		yield return new TestCaseData("tbody>tr:nth-of-type(1)>td:nth-of-type(1)", new DataGrid.HitTestInfo() { type = DataGrid.HitTestType.RowHeader, col = -1, row = 0 });
		yield return new TestCaseData("tbody>tr:nth-of-type(1)>td:nth-of-type(2)", new DataGrid.HitTestInfo() { type = DataGrid.HitTestType.Cell, col = 0, row = 0 });
		yield return new TestCaseData("tbody>tr:nth-of-type(1)>td:nth-of-type(3)", new DataGrid.HitTestInfo() { type = DataGrid.HitTestType.Cell, col = 1, row = 0 });
		yield return new TestCaseData("tbody>tr:nth-of-type(2)>td:nth-of-type(1)", new DataGrid.HitTestInfo() { type = DataGrid.HitTestType.RowHeader, col = -1, row = 1 });
		yield return new TestCaseData("tbody>tr:nth-of-type(2)>td:nth-of-type(2)", new DataGrid.HitTestInfo() { type = DataGrid.HitTestType.Cell, col = 0, row = 1 });
		yield return new TestCaseData("tbody>tr:nth-of-type(2)>td:nth-of-type(3)", new DataGrid.HitTestInfo() { type = DataGrid.HitTestType.Cell, col = 1, row = 1 });
		if (!isReadOnly)
		{
			// read only grids do not have an add new row
			yield return new TestCaseData("tbody>tr:nth-of-type(5)>td:nth-of-type(1)", new DataGrid.HitTestInfo() { type = DataGrid.HitTestType.RowHeader, col = -1, row = 4 });
			yield return new TestCaseData("tbody>tr:nth-of-type(5)>td:nth-of-type(2)", new DataGrid.HitTestInfo() { type = DataGrid.HitTestType.Cell, col = 0, row = 4 });
			yield return new TestCaseData("tbody>tr:nth-of-type(5)>td:nth-of-type(3)", new DataGrid.HitTestInfo() { type = DataGrid.HitTestType.Cell, col = 1, row = 4 });
		}
		yield return new TestCaseData(".datagrid", new DataGrid.HitTestInfo() { type = DataGrid.HitTestType.None, col = -1, row = -1 });
	}

	[Test, WithPlaywrightPage]
	public async Task DoubleClickIntoAnUnfocusedCellShouldPlaceCaretUnderCursor()
	{
		await using var ctx = new InMemoryTestServerContext();
		var dataGrid = default(DataGrid);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(dataGrid = SetupDataGrid());
			return form;
		});

		await page.WaitForSelectorAsync(".datagrid");
		var dataGridLocator = page.Locator(".datagrid");

		var cellLocator = dataGridLocator.Locator($"tbody>tr:nth-of-type({2})>td:nth-child({2})");
		await cellLocator.WaitForAsync();
		var cellBounds = await cellLocator.BoundingBoxAsync();

		var cellContent = await cellLocator.InnerTextAsync();
		Assert.That(cellContent, Is.EqualTo("cell21"));

		await page.ClickAsync(".datagrid", new PageClickOptions()
		{
			ClickCount = 2,
			Delay = 100,
			Position = new Position()
			{
				X = (int)(cellBounds.X + 20),
				Y = (int)(cellBounds.Y + cellBounds.Height / 2)
			}
		});

		var focusedInput = cellLocator.Locator("input");
		await focusedInput.WaitForAsync();
		Assert.That(focusedInput, Is.Not.Null, "Input should be focused after double-click.");

		//Double click into an unfocused cell should place the caret with no highlighted text
		var selectionStart = await focusedInput.EvaluateAsync<int>("e => e.selectionStart");
		var selectionEnd = await focusedInput.EvaluateAsync<int>("e => e.selectionEnd");
		Assert.That(selectionStart, Is.EqualTo(4));
		Assert.That(selectionEnd, Is.EqualTo(4));

		await page.ClickAsync(".datagrid", new PageClickOptions()
		{
			ClickCount = 2,
			Delay = 100,
			Position = new Position()
			{
				X = (int)(cellBounds.X + 20),
				Y = (int)(cellBounds.Y + cellBounds.Height / 2)
			}
		});

		//Double click into a focused cell should highlight the word that was clicked on
		selectionStart = await focusedInput.EvaluateAsync<int>("e => e.selectionStart");
		selectionEnd = await focusedInput.EvaluateAsync<int>("e => e.selectionEnd");
		var valueLength = await focusedInput.EvaluateAsync<int>("e => e.value.length");
		Assert.That(selectionStart, Is.EqualTo(0));
		Assert.That(selectionEnd, Is.EqualTo(valueLength));
	}

	[Test, WithPlaywrightPage]
	public async Task UpdateRowNotificationNotCalledFromRenderThread()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var dataGrid = new NotThreadSafeDataGrid(ctx.WinzorDispatcher);
			InitializeTestGrid(dataGrid);
			form.Controls.Add(dataGrid);
			return form;
		});
		await page.WaitForSelectorAsync(".datagrid");
	}

	[Test, WithPlaywrightPage]
	public async Task IsCurrentCellNotCalledFromRenderThread()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var dataGrid = new DataGrid();
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = MockGridData();
			dataGrid.AllowNavigation = false;

			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnStyle1 = new NotThreadSafeDataGridColumnStyle(ctx.WinzorDispatcher) { MappingName = "one", HeaderText = "One Header", ReadOnly = true };
			columnStyle1.Width = 80;
			tableStyle.GridColumnStyles.Add(columnStyle1);

			var columnStyle2 = new NotThreadSafeDataGridColumnStyle(ctx.WinzorDispatcher) { MappingName = "two", HeaderText = "Two Header" };
			columnStyle2.Width = 180;
			tableStyle.GridColumnStyles.Add(columnStyle2);
			dataGrid.TableStyles.Add(tableStyle);

			form.Controls.Add(dataGrid);
			return form;
		});
		await page.WaitForSelectorAsync(".datagrid");
	}

	async Task<(IRenderedFragment, DataGrid)> TestGridAsync(WinzorTestContext ctx)
	{
		DataGrid dataGrid = null;
		var component = await ctx.RenderControlOnFormAsync(() =>
		{
			dataGrid = new DataGrid();
			InitializeTestGrid(dataGrid);
			return dataGrid;
		});
		return (component, dataGrid);
	}

	void InitializeTestGrid(DataGrid dataGrid)
	{
		dataGrid.Dock = DockStyle.Fill;
		dataGrid.DataSource = MockGridData();
		dataGrid.AllowNavigation = false;
		var tableStyle = new DataGridTableStyle { MappingName = "data" };
		var columnStyle1 = new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One Header" };
		columnStyle1.Width = 80;
		tableStyle.GridColumnStyles.Add(columnStyle1);
		var columnStyle2 = new DataGridTextBoxColumn { MappingName = "two", HeaderText = "Two Header" };
		columnStyle2.Width = 180;
		tableStyle.GridColumnStyles.Add(columnStyle2);
		dataGrid.TableStyles.Add(tableStyle);
	}

	DataTable MockGridData()
	{
		var table = new DataTable("data");
		var column1 = new DataColumn("one");
		column1.DataType = typeof(string);
		table.Columns.Add(column1);
		var column2 = new DataColumn("two");
		column2.DataType = typeof(string);
		table.Columns.Add(column2);
		table.Rows.Add("row1", "11");
		table.Rows.Add("row2", "22");
		table.Rows.Add("row3", "33");
		table.Rows.Add("row4", "44");

		return table;
	}

	[Test]
	public async Task DataGridDoesNotBeginEditIfCellAlreadyEditing()
	{
		using var ctx = new WinzorTestContext();
		DataTable table = null;
		DataGrid dataGrid = null;
		DataGridColumnStyleStub columnStyle1 = null;

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			table = new DataTable("data");
			var column1 = new DataColumn("Origin");
			column1.DataType = typeof(string);
			table.Columns.Add(column1);
			var column2 = new DataColumn("Destination");
			column2.DataType = typeof(string);
			table.Columns.Add(column2);
			var column3 = new DataColumn("e-freight Status");
			column3.DataType = typeof(string);
			table.Columns.Add(column3);

			table.Rows.Add("AAAAA", "", "NON");

			dataGrid = new DataGrid();
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			columnStyle1 = new DataGridColumnStyleStub() { MappingName = "Origin", HeaderText = "Origin" };
			columnStyle1.Width = 180;
			tableStyle.GridColumnStyles.Add(columnStyle1);
			var columnStyle2 = new DataGridTextBoxColumn { MappingName = "Destination", HeaderText = "Destination" };
			columnStyle2.Width = 180;
			tableStyle.GridColumnStyles.Add(columnStyle2);
			var columnStyle3 = new DataGridTextBoxColumn { MappingName = "e-freight Status", HeaderText = "e-freight Status" };
			columnStyle3.Width = 180;
			tableStyle.GridColumnStyles.Add(columnStyle3);
			dataGrid.TableStyles.Add(tableStyle);

			return dataGrid;
		});

		await rendered.FindAll("tbody>tr")[0].Children[1].MouseDownAsync(new WebMouseEventArgs());
		var input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		Assert.That(rendered.FindAll("tbody>tr")[0].Children[1].Contains(input), Is.True);
		Assert.That(input.Value, Is.EqualTo("AAAAA"));

		// Render count should incremement because this click should trigger the begin edit
		Assert.That(columnStyle1.EditCount, Is.EqualTo(1));

		Assert.That(input.ParentElement.GetAttribute("class"), Does.Contain("datagrid__control--edit"));
		Assert.That(rendered.FindAll("tbody>tr")[0].Children[1].GetAttribute("class"), Does.Contain("datagrid__cell--edit"));
		Assert.That(rendered.FindAll("tbody>tr")[0].GetAttribute("class"), Does.Contain("datagrid__row--edit"));

		await rendered.FindAll("tbody>tr")[0].Children[1].MouseDownAsync(new WebMouseEventArgs());
		Assert.That(dataGrid.TableStyles[0].GridColumnStyles[0].HeaderText, Is.EqualTo("Origin"));

		// Render count should not incremement because this click should be a no-op
		Assert.That(columnStyle1.EditCount, Is.EqualTo(1));

		Assert.That(input.Value, Is.EqualTo("AAAAA"));
		Assert.That(input.ParentElement.GetAttribute("class"), Does.Contain("datagrid__control--edit"));
		Assert.That(rendered.FindAll("tbody>tr")[0].Children[1].GetAttribute("class"), Does.Contain("datagrid__cell--edit"));
		Assert.That(rendered.FindAll("tbody>tr")[0].GetAttribute("class"), Does.Contain("datagrid__row--edit"));
	}

	[Test]
	public async Task GetRenderContentCount_CancelNotEditedRowFromNewRow()
	{
		using var ctx = new WinzorTestContext();
		DataGridColumnStyleStub columnStyle = null;

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var table = new DataTable("data");
			var column = new DataColumn("one");
			column.DataType = typeof(string);
			table.Columns.Add(column);

			var dataGrid = new DataGrid();
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;

			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			columnStyle = new DataGridColumnStyleStub { MappingName = "one", HeaderText = "One Header" };
			columnStyle.Width = 80;
			tableStyle.GridColumnStyles.Add(columnStyle);
			dataGrid.TableStyles.Add(tableStyle);

			return dataGrid;
		});

		var tr = rendered.FindAll("tbody>tr");
		Assert.That(tr, Has.Count.EqualTo(1));
		Assert.That(tr[0].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));

		// type something in the first row to create the second row (new row)
		var input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		Assert.That(rendered.FindAll("tbody>tr")[0].Children[1].Contains(input), Is.True);
		await rendered.KeyPressAsync(Keys.A, input);
		await rendered.Find("input").TriggerEventAsync("oninput", new ChangeEventArgs { Value = "1.1" });
		await input.FocusOutAsync();

		// confirm the scenario has been implemented
		tr = rendered.FindAll("tbody>tr");
		Assert.That(tr, Has.Count.EqualTo(2));
		Assert.That(tr[0].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));
		Assert.That(tr[0].ChildNodes[1].TextContent, Is.EqualTo("1.1"));
		Assert.That(tr[1].ChildNodes[0].TextContent, Is.EqualTo("*"));
		Assert.That(tr[1].ChildNodes[1].TextContent, Is.Empty);
		Assert.That(columnStyle.GetRenderContentCount, Is.EqualTo(2));

		// click on the second row (new row)
		columnStyle.GetRenderContentCount = 0;
		await SimulateClickOnCellForRowChange(rendered.FindAll("tbody>tr")[1].Children[1]);

		Assert.That(rendered.FindAll("tbody>tr")[0].ChildNodes[0].TextContent, Is.Empty);
		Assert.That(rendered.FindAll("tbody>tr")[1].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));
		Assert.That(columnStyle.GetRenderContentCount, Is.EqualTo(2));

		// click on the first row, should only render clicked row
		columnStyle.GetRenderContentCount = 0;
		await SimulateClickOnCellForRowChange(rendered.FindAll("tbody>tr")[0].Children[1]);
		Assert.That(rendered.FindAll("tbody>tr")[0].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));
		Assert.That(rendered.FindAll("tbody>tr")[1].ChildNodes[0].TextContent, Is.EqualTo("*"));
		Assert.That(columnStyle.GetRenderContentCount, Is.EqualTo(1));
	}

	[Test, WithPlaywrightPage]
	public async Task ScrollToSelectedRowAfterResizeGridColumn()
	{
		DataGrid dataGrid = null;
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(500, 400) };
			dataGrid = CreateNewGrid<TestDataGrid>();
			dataGrid.ReadOnly = true;
			form.Controls.Add(dataGrid);
			return form;
		});

		var table = await page.WaitForSelectorAsync("table");

		await table.WaitForSelectorAsync("tbody tr");
		var trs = new Func<Task<IReadOnlyList<IElementHandle>>>(async () => await table.QuerySelectorAllAsync("tbody tr"));
		var resizers = await table.QuerySelectorAllAsync("th .datagrid__column_resizer");

		//should scroll up
		await SelectRow((await trs())[0]);
		await ScrollToRowAsync(page, 300);
		await ResizeColumnWidth(page, resizers[0]);
		AssertRowVisible(dataGrid, 0, expectedVisibility: true);

		await SelectRow((await trs())[1]);
		await ScrollToRowAsync(page, 300);
		await ResizeColumnWidth(page, resizers[0]);
		AssertRowVisible(dataGrid, 1, expectedVisibility: true);

		// should not scroll
		await (await trs())[14].ScrollIntoViewIfNeededAsync();
		var scrollTop = await GetDataGridScrollTopAsync(page);
		await SelectRow((await trs())[14]);
		await ResizeColumnWidth(page, resizers[0]);
		AssertRowVisible(dataGrid, 14, expectedVisibility: true);

		await SelectRow((await trs())[15]);
		await ResizeColumnWidth(page, resizers[0]);
		AssertRowVisible(dataGrid, 15, expectedVisibility: true);

		// should scroll down
		await ScrollToRowAsync(page, 9999);
		var rows = await trs();
		await SelectRow(rows[rows.Count - 1]);
		AssertRowVisible(dataGrid, 49, expectedVisibility: true);
		await ScrollToRowAsync(page, 200);
		AssertRowVisible(dataGrid, 49, expectedVisibility: false);
		await ResizeColumnWidth(page, resizers[0]);
		AssertRowVisible(dataGrid, 49, expectedVisibility: true);

		await ScrollToRowAsync(page, 9999);
		rows = await trs();
		await SelectRow((await trs())[rows.Count - 3]);
		await ScrollToRowAsync(page, 200);
		await ResizeColumnWidth(page, resizers[0]);

		// selection of multiple rows
		await ScrollToRowAsync(page, 0);
		await SelectRow((await trs())[1]);
		await SelectRow((await trs())[2]);
		await SelectRow((await trs())[3]);
		await ScrollToRowAsync(page, 200);
		await SelectRow((await trs())[15]);
		await SelectRow((await trs())[16]);
		await ScrollToRowAsync(page, 9999);
		await Task.Delay(500);
		rows = await trs();
		await SelectRow((await trs())[rows.Count - 3]);
		await ScrollToRowAsync(page, 200);
		await ResizeColumnWidth(page, resizers[0]);
		AssertRowVisible(dataGrid, 47, expectedVisibility: true);
	}

	[TestCase(400, 512)]
	[TestCase(600, 496)]
	[Test, WithPlaywrightPage]
	public async Task ScrollTopChangeByScrollBar(int width, int expected)
	{
		await using var ctx = new InMemoryTestServerContext();
		var dataGrid = default(DataGrid);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(width, 300) };
			dataGrid = CreateNewGrid<TestDataGrid>();
			dataGrid.ReadOnly = true;
			form.Controls.Add(dataGrid);

			return form;
		});
		var table = await page.WaitForSelectorAsync("table");

		await table.WaitForSelectorAsync("tbody tr");
		await ScrollToAsync(page, 0, 9999);
		await ScrollToAsync(page, 0, 200);
		await dataGrid.InvokeWinzorDispatcherAsync(() =>
		{
			dataGrid.CurrentCell = new DataGridCell(47, 1);
		});
		Assert.That(async () => await GetDataGridScrollTopAsync(page), Is.EqualTo(expected).After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task ScrollingShouldPersistEditingRow()
	{
		await using var ctx = new InMemoryTestServerContext();
		var dataGrid = default(DataGrid);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(300, 300) };
			dataGrid = CreateNewGrid<TestDataGrid>();
			dataGrid.ReadOnly = true;
			form.Controls.Add(dataGrid);
			return form;
		});

		var table = await page.WaitForSelectorAsync("table");

		var td = await table.WaitForSelectorAsync("tbody tr:nth-child(5) td:nth-child(2)");

		await td.ClickAsync();

		await table.WaitForSelectorAsync("tbody tr:nth-child(5) td input");

		await ScrollToRowAsync(page, 999);
		await table.WaitForSelectorAsync("tbody tr.datagrid__row--hidden", new ElementHandleWaitForSelectorOptions() { State = WaitForSelectorState.Attached });

		await ScrollToRowAsync(page, 0);
		await table.WaitForSelectorAsync("tbody tr:nth-child(5) td input");
	}

	[WithPlaywrightPage]
	[TestCase(50, 2, 2, true)]
	[TestCase(300, 2, 5, true)]
	[TestCase(150, 1, 1, false)]
	[TestCase(250, 2, 2, false)]
	[TestCase(450, 2, 3, false)]
	public async Task ScrollingCallsOnScrollAsyncDuringScroll(int duration, int leastCount, int mostCount, bool useWheel)
	{
		await using var ctx = new InMemoryTestServerContext();
		TestDataGrid dataGrid = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(300, 300) };
			dataGrid = CreateNewGrid<TestDataGrid>();
			dataGrid.ReadOnly = true;
			form.Controls.Add(dataGrid);
			return form;
		});

		// Scroll the datagrid slowly
		await page.EvaluateAsync(@"([duration, useWheel]) => {
			function smoothScroll(element, target, duration) {
				const start = element.scrollTop;
				const change = target - start;
				const startTime = performance.now();

				function animateScroll(currentTime) {
					const timeElapsed = currentTime - startTime;
					const progress = Math.min(timeElapsed / duration, 1);
					element.scrollTop = start + change * progress;

					if (timeElapsed < duration) {
						requestAnimationFrame(animateScroll);
					}
				}

				if (useWheel) {
				    element.dispatchEvent(new WheelEvent('wheel', { deltaY: 10, deltaMode: 0 }));
				}

				requestAnimationFrame(animateScroll);
			}

			const grid = document.getElementsByClassName('datagrid')[0];
			smoothScroll(grid, 547, duration); // Scrolls to 'scrollTop' over 'duration' milliseconds
		}", new object[] { duration, useWheel });

		await Task.Delay(duration + 100); //Wait for scroll to complete

		Assert.That(() => dataGrid.onScrollAsyncCalledCount, Is.AtLeast(leastCount).After(1000, 100));
		Assert.That(() => dataGrid.onScrollAsyncCalledCount, Is.AtMost(mostCount).After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task ColumnResizeShouldNotExceedGridWidth()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(200, 100) };
			var dataGrid = new DataGrid();
			var table = new DataTable("data");
			var column = new DataColumn("one");
			column.DataType = typeof(string);
			table.Columns.Add(column);
			table.Rows.Add("first row");
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnStyle = new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One Header" };
			columnStyle.Width = 80;
			tableStyle.GridColumnStyles.Add(columnStyle);
			dataGrid.TableStyles.Add(tableStyle);
			dataGrid.ReadOnly = true;
			form.Controls.Add(dataGrid);

			return form;
		});

		var table = await page.WaitForSelectorAsync("table");
		var resizer = await table.QuerySelectorAsync("th .datagrid__column_resizer");

		Assert.That(await page.EvaluateAsync<int>("document.getElementsByTagName('th')[0].offsetWidth"), Is.EqualTo(35));
		Assert.That(await page.EvaluateAsync<int>("document.getElementsByTagName('th')[1].offsetWidth"), Is.EqualTo(80));

		await AssertDataGridResizeColumnWidth(page, resizer, 15, 95);

		// the max width will be 200 subtract first column 35 which is 165
		await AssertDataGridResizeColumnWidth(page, resizer, 200, 165);

		await AssertDataGridResizeColumnWidth(page, resizer, 999, 165);
		await AssertDataGridResizeColumnWidth(page, resizer, -100, 65);
		await AssertDataGridResizeColumnWidth(page, resizer, -999, 6);
	}

	[SuppressMessage("CargoWiseOne", "CW1104:DoNotUseSystemWindowsTabControl", Justification = "Testing")]
	[TestCase(20, 100)]
	[TestCase(0, 100)]
	[TestCase(20, 0)]
	[TestCase(0, 0)]
	[WithPlaywrightPage(Headless = false)]
	public async Task ScrollBarKeepThePositionAfterSwitchingTabAndBack(int scrollLeft, int scrollTop)
	{
		DataGrid dataGrid = null;
		TabControl tabControl = null;
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(300, 300) };
			tabControl = new TabControl();
			tabControl.Dock = DockStyle.Fill;
			var tabPage1 = new TabPage();
			tabPage1.Text = "One";
			tabControl.TabPages.Add(tabPage1);
			dataGrid = CreateNewGrid<TestDataGrid>();
			dataGrid.Size = new Size(200, 300);
			tabPage1.Controls.Add(dataGrid);
			var tabPage2 = new TabPage();
			tabPage2.Text = "Two";
			tabControl.TabPages.Add(tabPage2);
			form.Controls.Add(tabControl);
			return form;
		});

		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

		await ScrollToAsync(page, scrollLeft, scrollTop);
		Assert.That(async () => await page.EvaluateAsync<int>("document.getElementsByClassName('datagrid')[0].scrollLeft"), Is.EqualTo(scrollLeft).After(1000, 100));
		Assert.That(async () => await page.EvaluateAsync<int>("document.getElementsByClassName('datagrid')[0].scrollTop"), Is.EqualTo(scrollTop).After(1000, 100));

		await page.GetByRole(AriaRole.Button, new () { Name = "Two" }).ClickAsync();
		await Task.Delay(500);
		await page.GetByRole(AriaRole.Button, new () { Name = "One" }).ClickAsync();
		await Task.Delay(500);

		Assert.That(async () => await page.EvaluateAsync<int>("document.getElementsByClassName('datagrid')[0].scrollLeft"), Is.EqualTo(scrollLeft).After(1000, 100));
		Assert.That(async () => await page.EvaluateAsync<int>("document.getElementsByClassName('datagrid')[0].scrollTop"), Is.EqualTo(scrollTop).After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task ResizeEmptyGridColumnHeader()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
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
			var form = new Form() { Size = new Size(500, 200) };
			dataGrid.ReadOnly = true;
			form.Controls.Add(dataGrid);

			return form;
		});

		var table = await page.WaitForSelectorAsync("table");

		var th = await page.QuerySelectorAllAsync("thead tr th");
		var trs = await table.QuerySelectorAllAsync("tbody tr");
		var resizer = await page.WaitForSelectorAsync("th .datagrid__column_resizer");

		Assert.That(trs.Count, Is.EqualTo(0));

		await MouseMoveOfElementAsync(page, resizer, 0, 2);
		await MouseDownAsync(page);
		await MouseMoveOfElementAsync(page, resizer, 80, 2);
		await MouseUpAsync(page);

		Assert.That(th.Count, Is.EqualTo(2));
		Assert.That(await th[1].GetAttributeAsync("style"), Does.Contain("width: 160px;"));
	}

	[Test, WithPlaywrightPage]
	public async Task ColumnResizerGuideShouldHaveCorrectClassesUponDrag()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
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
			var form = new Form() { Size = new Size(500, 200) };
			dataGrid.ReadOnly = true;
			form.Controls.Add(dataGrid);

			return form;
		});

		var resizer = await page.WaitForSelectorAsync("th .datagrid__column_resizer");
		await MouseMoveOfElementAsync(page, resizer, 0, 2);
		await MouseDownAsync(page);
		await MouseMoveOfElementAsync(page, resizer, 80, 2);

		var guide = await page.WaitForSelectorAsync("table > div:last-of-type");
		Assert.That(await guide.GetAttributeAsync("class"), Does.Contain("splitter__guide"));
	}

	[Test, WithPlaywrightPage]
	public async Task ColumnResizerGuideShouldRemoveWhenBlur()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
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
			var form = new Form() { Size = new Size(500, 200) };
			dataGrid.ReadOnly = true;
			form.Controls.Add(dataGrid);

			return form;
		});

		var resizer = await page.WaitForSelectorAsync("th .datagrid__column_resizer");
		var grid = page.Locator(".datagrid");
		await MouseMoveOfElementAsync(page, resizer, 0, 2);
		await MouseDownAsync(page);
		Assert.That((await page.QuerySelectorAllAsync(".splitter__guide")).Count, Is.EqualTo(1));

		await grid.BlurAsync();
		await MouseMoveOfElementAsync(page, resizer, 80, 2);
		Assert.That((await page.QuerySelectorAllAsync(".splitter__guide")).Count, Is.EqualTo(0));
	}

	[Test]
	public async Task DatagridHeaderClickNoExceptionRaised()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var table = new DataTable("data");
			var column1 = new DataColumn("one");
			column1.DataType = typeof(string);
			table.Columns.Add(column1);
			var column2 = new DataColumn("two");
			column2.DataType = typeof(string);
			table.Columns.Add(column2);
			table.Rows.Add("1.1", "1.2");
			table.Rows.Add("2.1", "2.2");

			var dataGrid = new DataGrid();
			dataGrid.ReadOnly = true;
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnStyle1 = new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One Header" };
			columnStyle1.Width = 100;
			tableStyle.GridColumnStyles.Add(columnStyle1);
			var columnStyle2 = new DataGridTextBoxColumn { MappingName = "two", HeaderText = "Two Header" };
			columnStyle2.Width = 200;
			tableStyle.GridColumnStyles.Add(columnStyle2);
			dataGrid.TableStyles.Add(tableStyle);

			return dataGrid;
		});

		var grid = rendered.Find(".datagrid");
		var th = rendered.FindAll("thead>tr>th");

		Assert.That(grid, Is.Not.Null);
		Assert.That(th, Has.Count.EqualTo(3));

		var tr = rendered.FindAll("tbody>tr");
		Assert.That(tr, Has.Count.EqualTo(2));
		Assert.That(tr[0].ChildNodes, Has.Length.EqualTo(3));

		Assert.DoesNotThrowAsync(async () => await tr[0].Children[1].MouseDownAsync(new WebMouseEventArgs()));
		Assert.DoesNotThrowAsync(async () => await th[0].MouseDownAsync(new WebMouseEventArgs()));

		Assert.DoesNotThrowAsync(async () => await tr[1].Children[2].MouseDownAsync(new WebMouseEventArgs()));
		Assert.DoesNotThrowAsync(async () => await th[1].MouseDownAsync(new WebMouseEventArgs()));
	}

	[Test, WithPlaywrightPage]
	public async Task GridHeaderNotShownThroughOverlappingControl()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
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

			var panel = new Panel();
			panel.Width = 200;
			panel.Height = 200;
			panel.ZIndex = dataGrid.ZIndex + 1;
			form.Controls.Add(panel);

			return form;
		});
		var table = await page.WaitForSelectorAsync("table");

		Assert.That(async () => await table.EvaluateAsync<string>("e => document.elementsFromPoint(10, 10)[0].className"), Is.EqualTo("panel"));
		Assert.That(async () => await table.EvaluateAsync<string>("e => document.elementsFromPoint(20, 20)[0].className"), Is.EqualTo("panel"));
		Assert.That(async () => await table.EvaluateAsync<string>("e => document.elementsFromPoint(30, 30)[0].className"), Is.EqualTo("panel"));
	}

	[Test, WithPlaywrightPage]
	[TestCase("Control+V", TestName = "{m}_CtrlV")]
	[TestCase("Shift+Insert", TestName = "{m}_ShiftInsert")]
	public async Task DataGridTextBoxSupportsPasteShortcuts(string pasteShortcut)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var table = new DataTable("data");
			var column1 = new DataColumn("one");
			column1.DataType = typeof(string);
			table.Columns.Add(column1);

			var dataGrid = new DataGrid();
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;

			var columnStyle1 = new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One Header" };
			columnStyle1.Width = 200;

			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			tableStyle.GridColumnStyles.Add(columnStyle1);

			dataGrid.TableStyles.Add(tableStyle);
			form.Controls.Add(dataGrid);
			return form;
		});

		var dataGridLocator = page.Locator(".datagrid");

		Assert.That(async () => await dataGridLocator.Locator("tr").CountAsync(), Is.EqualTo(2).After(2000, 200));

		await BrowserContext.GrantPermissionsAsync(new[] { "clipboard-write", "clipboard-read" });
		await page.EvaluateAsync("async () => await navigator.clipboard.writeText('test text')");

		await page.Keyboard.PressAsync(pasteShortcut);

		var newRowCell = dataGridLocator.Locator(".datagrid__row-star--new > td:nth-child(2)");
		await newRowCell.ClickAsync();

		Assert.That(async () => await dataGridLocator.Locator("tbody > tr:nth-child(2) > td:nth-child(1)").InnerTextAsync(), Is.EqualTo("arrow_right").After(2000, 200));

		await page.Keyboard.PressAsync(pasteShortcut);
		await newRowCell.ClickAsync();

		Assert.That(async () => await dataGridLocator.Locator("tr").CountAsync(), Is.EqualTo(4).After(2000, 200));
		Assert.That(async () => await dataGridLocator.Locator("tbody > tr:nth-child(1) > td:nth-child(2)").InnerTextAsync(), Is.EqualTo("test text"));
		Assert.That(async () => await dataGridLocator.Locator("tbody > tr:nth-child(2) > td:nth-child(2)").InnerTextAsync(), Is.EqualTo("test text"));
	}

	[Test, WithPlaywrightPage]
	public async Task FiredColumnStartedEditingWhenPressCtrlXAsync()
	{
		await using var ctx = new InMemoryTestServerContext();
		DataTable table = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			table = new DataTable("data");
			var column1 = new DataColumn("one");
			column1.DataType = typeof(string);
			table.Columns.Add(column1);
			table.Rows.Add("ABCD");
			table.Rows.Add("EFG");
			table.Rows.Add("HI");

			var dataGrid = new DataGrid();
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;

			var columnStyle1 = new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One Header" };
			columnStyle1.Width = 200;

			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			tableStyle.GridColumnStyles.Add(columnStyle1);

			dataGrid.TableStyles.Add(tableStyle);
			form.Controls.Add(dataGrid);

			return form;
		});

		await BrowserContext.GrantPermissionsAsync(new[] { "clipboard-read", "clipboard-write" });

		var cell = await page.WaitForSelectorAsync(".datagrid table tbody tr:nth-child(2) td:nth-child(2)");
		var valueBefore = await cell.InnerTextAsync();
		Assert.That(valueBefore, Is.EqualTo("EFG"));

		await cell.ClickAsync();
		await Task.Delay(200);
		await page.Keyboard.PressAsync("Control+x");
		await Task.Delay(200);
		await page.Keyboard.PressAsync("Tab");
		await Task.Delay(200);

		var clipboardText = await page.EvaluateAsync<string>("navigator.clipboard.readText()");
		Assert.That(clipboardText, Is.EqualTo("EFG"));

		var valueAfter = await cell.InnerTextAsync();
		Assert.That(valueAfter, Is.Empty);
	}

	[TestCase(BorderStyle.None, 0)]
	[TestCase(BorderStyle.FixedSingle, 0)]
	[TestCase(BorderStyle.Fixed3D, 0)]
	public async Task TestBorderStyleAdjustForClientSize(BorderStyle bs, int borderSize)
	{
		using var ctx = new WinzorTestContext();
		DataGrid targetDataGrid = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Size = new Size(500, 500);

			var table = new DataTable("data");
			var column1 = new DataColumn("one");
			column1.DataType = typeof(string);
			table.Columns.Add(column1);
			var column2 = new DataColumn("two");
			column2.DataType = typeof(string);
			table.Columns.Add(column2);
			table.Rows.Add("1.1", "1.2");
			table.Rows.Add("2.1", "2.2");

			targetDataGrid = new DataGrid();
			targetDataGrid.ReadOnly = true;
			targetDataGrid.DataSource = table;
			targetDataGrid.AllowNavigation = false;

			form.Controls.Add(targetDataGrid);
			targetDataGrid.Location = new Point(10, 10);

			targetDataGrid.BorderStyle = bs;

			return form;
		});

		var b = targetDataGrid.Bounds;
		Assert.That(b.X, Is.EqualTo(10));
		Assert.That(b.Y, Is.EqualTo(10));
		Assert.That(targetDataGrid.ClientSize.Width + 2 * borderSize, Is.EqualTo(b.Width));
		Assert.That(targetDataGrid.ClientSize.Height + 2 * borderSize, Is.EqualTo(b.Height));
		Assert.That(targetDataGrid.ClientAreaBounds.X, Is.EqualTo(b.X + borderSize));
		Assert.That(targetDataGrid.ClientAreaBounds.Y, Is.EqualTo(b.Y + borderSize));
	}

	[TestCase(BorderStyle.None, BorderStyle.FixedSingle, false, 0)]
	[TestCase(BorderStyle.None, BorderStyle.Fixed3D, false, 0)]
	[TestCase(BorderStyle.None, BorderStyle.None, false, 0)]
	public async Task BorderStyleChangeTriggersRecalculation(BorderStyle initial, BorderStyle bs, bool expectChange, int expectedDiff)
	{
		using var ctx = new WinzorTestContext();
		DataGrid targetDataGrid = null;

		bool clientSizeChanged = false;
		int clientSizeDiff = 0;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Size = new Size(500, 500);

			var table = new DataTable("data");
			var column1 = new DataColumn("one");
			column1.DataType = typeof(string);
			table.Columns.Add(column1);
			var column2 = new DataColumn("two");
			column2.DataType = typeof(string);
			table.Columns.Add(column2);
			table.Rows.Add("1.1", "1.2");
			table.Rows.Add("2.1", "2.2");

			targetDataGrid = new DataGrid();
			targetDataGrid.ReadOnly = true;
			targetDataGrid.DataSource = table;
			targetDataGrid.AllowNavigation = false;

			form.Controls.Add(targetDataGrid);
			targetDataGrid.Location = new Point(10, 10);

			targetDataGrid.BorderStyle = BorderStyle.None;

			var clientSizeBefore = targetDataGrid.ClientSize;
			targetDataGrid.ClientSizeChanged += (sender, e) =>
			{
				clientSizeChanged = true;
				clientSizeDiff = clientSizeBefore.Width - targetDataGrid.ClientSize.Width;
			};

			return form;
		});

		clientSizeChanged = false;

		await targetDataGrid.InvokeWinzorDispatcherAsync(() =>
		{
			targetDataGrid.BorderStyle = bs;
		});

		Assert.That(clientSizeChanged, Is.EqualTo(expectChange));
		Assert.That(clientSizeDiff, Is.EqualTo(expectedDiff));
	}

	// THIS TEST MUST BE HEADLESS = FALSE OR IT WILL NOT WORK
	[TestCase("ArrowLeft", 1, 3, 1, 2, 260)]
	[TestCase("ArrowRight", 0, 0, 0, 1, 0)]
	[TestCase("ArrowUp", 3, 3, 2, 3, 540)]
	[TestCase("ArrowDown", 2, 3, 3, 3, 540)]
	[WithPlaywrightPage(Headless = false)]
	public async Task PressingArrowKeyShouldMoveCorrectly(string key, int initialRow, int initialCol, int targetRow, int targetCol, int? expectedPosition)
	{
		await using var ctx = new InMemoryTestServerContext();
		var dataGrid = default(DataGrid);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Width = 400, Height = 100 };
			dataGrid = SetupDataGrid(true);
			dataGrid.Width = 1000;
			dataGrid.Height = 1000;
			form.Controls.Add(dataGrid);
			return form;
		});

		await page.WaitForSelectorAsync(".datagrid");
		var dataGridLocator = page.Locator(".datagrid");

		await dataGrid.InvokeWinzorDispatcherAsync(() =>
		{
			dataGrid.CurrentCell = new DataGridCell(initialRow, initialCol);
		});

		var initialCellLocator = dataGridLocator.Locator($"tbody>tr:nth-of-type({initialRow + 1})>td:nth-child({initialCol + 2})");
		await initialCellLocator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
		await initialCellLocator.FocusAsync();

		await page.Keyboard.PressAsync(key, new() { Delay = 50 });

		var targetCellLocator = dataGridLocator.Locator($"tbody>tr:nth-of-type({targetRow + 1})>td:nth-child({targetCol + 2})");
		await targetCellLocator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
		await targetCellLocator.FocusAsync();

		var actualScrollLeft = await GetDataGridScrollLeftAsync(page);
		Assert.That(actualScrollLeft, Is.EqualTo(expectedPosition));
	}

	[TestCase(2, 1, 1, 1, TestName = "{m}_OnNonFirstRow_ShouldMoveToTheUpCell_OfLastRow")]
	[TestCase(1, 1, 1, 1, TestName = "{m}_OnFirstRow_DoesNotMove")]
	[WithPlaywrightPage]
	public async Task PressingUpArrowKey(int initialRow, int initialCol, int expectedRow, int expectedCol)
	{
		await using var ctx = new InMemoryTestServerContext();
		var dataGrid = default(DataGrid);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			dataGrid = SetupDataGrid();
			form.Controls.Add(dataGrid);
			return form;
		});

		await LoadPageAndAssertKeyPress(page, dataGrid, "ArrowUp", initialRow, initialCol, expectedRow, expectedCol);
	}

	[TestCase(1, 1, 2, 1, false, TestName = "{m}_OnNonLastRow_ShouldMoveToTheDownCell_OfNextRow")]
	[TestCase(4, 1, 5, 1, true, TestName = "{m}_OnLastRow_ShouldMoveToTheDownCell_OfNewRow")]
	[TestCase(5, 1, 5, 1, true, TestName = "{m}_OnNewRow_DoesNotMove")]
	[WithPlaywrightPage]
	public async Task PressingDownArrowKey(int initialRow, int initialCol, int expectedRow, int expectedCol, bool moveToNewRow)
	{
		await using var ctx = new InMemoryTestServerContext();
		var dataGrid = default(DataGrid);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(dataGrid = SetupDataGrid());
			return form;
		});

		await LoadPageAndAssertKeyPress(page, dataGrid, "ArrowDown", initialRow, initialCol, expectedRow, expectedCol, moveToNewRow);
	}

	[TestCase(1, 2, 1, 2, 6, 5, TestName = "{m}_CursorOnCellRightSide_ShouldMoveCursorLeft_OfCurrentRow")]
	[TestCase(1, 2, 1, 2, 5, 4, TestName = "{m}_CursorOnCellNonSide_ShouldMoveCursorLeft_OfCurrentRow")]
	[TestCase(1, 2, 1, 1, 0, 0, TestName = "{m}_CursorOnCellLeftSide_ShouldMoveToTheLeftCell_OfCurrentRow")]
	[TestCase(2, 1, 1, 4, 0, 0, TestName = "{m}_OnTheFirstCellOfNonFirstRow_ShouldMoveToTheLastCell_OfLastRow")]
	[TestCase(1, 1, 1, 1, 0, 0, TestName = "{m}_OnTheFirstCellOfTheFirstRow_DoesNotMove")]
	[WithPlaywrightPage]
	public async Task PressingLeftArrowKey(int initialRow, int initialCol, int expectedRow, int expectedCol, int initCursor, int expectCursor)
	{
		await using var ctx = new InMemoryTestServerContext();
		var dataGrid = default(DataGrid);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			dataGrid = SetupDataGrid();
			form.Controls.Add(dataGrid);
			return form;
		});

		await page.ClickAsync($"tbody>tr:nth-of-type({initialRow})>td:nth-of-type({initialCol + 1})", pageClickOptions);
		await page.WaitForSelectorAsync($"tbody>tr:nth-of-type({initialRow})>td:nth-of-type({initialCol + 1}) input", playWrightPageWaitForSelectorOptions);

		var textBoxInput = await page.WaitForSelectorAsync("input");
		await textBoxInput.FocusAsync();
		await page.EvaluateAsync(@"(textBoxInput) => {
	textBoxInput.selectionStart = " + initCursor + ";" +
"textBoxInput.selectionEnd =" + initCursor + ";" +
"}", textBoxInput);
		await page.WaitForSelectorAsync(".form");
		Assert.That(async () => await textBoxInput.EvaluateAsync<int>("i => i.selectionStart"), Is.EqualTo(initCursor).After(3000, 100));
		Assert.That(async () => await textBoxInput.EvaluateAsync<int>("i => i.selectionEnd"), Is.EqualTo(initCursor).After(3000, 100));

		await page.Keyboard.PressAsync("ArrowLeft", keyboardPressOptions);

		Assert.That(async () => await textBoxInput.EvaluateAsync<int>("i => i.selectionStart"), Is.EqualTo(expectCursor).After(3000, 100));
		Assert.That(async () => await textBoxInput.EvaluateAsync<int>("i => i.selectionEnd"), Is.EqualTo(expectCursor).After(3000, 100));

		var expectedNextCellInput = await page.WaitForSelectorAsync($"tbody>tr:nth-of-type({expectedRow})>td:nth-of-type({expectedCol + 1}).datagrid__cell--edit input", playWrightPageWaitForSelectorOptions);
		Assert.That(await expectedNextCellInput.InputValueAsync(), Is.EqualTo($"cell{expectedRow}{expectedCol}"));
	}

	[TestCase(1, 1, 1, 1, false, 0, 1, TestName = "{m}_CursorOnCellLeftSide_ShouldMoveCursorToRight_OfCurrentRow")]
	[TestCase(1, 1, 1, 1, false, 1, 2, TestName = "{m}_CursorOnCellNonSide_ShouldMoveCursorToRight_OfCurrentRow")]
	[TestCase(1, 1, 1, 2, false, 6, 6, TestName = "{m}_OnNonLastCell_ShouldMoveToTheRightCell_OfCurrentRow")]
	[TestCase(1, 4, 2, 1, false, 6, 6, TestName = "{m}_OnTheLastCellOfNonLastRow_ShouldMoveToTheFirstCell_OfNextRow")]
	[TestCase(4, 4, 5, 1, true, 6, 6, TestName = "{m}_OnTheLastCellOfTheLastRow_ShouldMoveToTheFirstCell_OfNewRow")]
	[TestCase(5, 4, 5, 4, true, 0, 0, TestName = "{m}_OnTheLastCellOfNewRow_DoesNotMove")]
	[WithPlaywrightPage]
	public async Task PressingRightArrowKey(int initialRow, int initialCol, int expectedRow, int expectedCol, bool moveToNewRow, int initCursor, int expectCursor)
	{
		await using var ctx = new InMemoryTestServerContext();
		var dataGrid = default(DataGrid);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(dataGrid = SetupDataGrid());
			return form;
		});

		await page.ClickAsync($"tbody>tr:nth-of-type({initialRow})>td:nth-of-type({initialCol + 1})", pageClickOptions);
		await page.WaitForSelectorAsync($"tbody>tr:nth-of-type({initialRow})>td:nth-of-type({initialCol + 1}) input", playWrightPageWaitForSelectorOptions);

		var textBoxInput = await page.WaitForSelectorAsync("input");
		await textBoxInput.FocusAsync();
		await page.EvaluateAsync(@"(textBoxInput) => {
	textBoxInput.selectionStart = " + initCursor + ";" +
"textBoxInput.selectionEnd =" + initCursor + ";" +
"}", textBoxInput);
		await page.WaitForSelectorAsync(".form");
		Assert.That(async () => await textBoxInput.EvaluateAsync<int>("i => i.selectionStart"), Is.EqualTo(initCursor).After(3000, 100));
		Assert.That(async () => await textBoxInput.EvaluateAsync<int>("i => i.selectionEnd"), Is.EqualTo(initCursor).After(3000, 100));

		await page.Keyboard.PressAsync("ArrowRight", keyboardPressOptions);

		Assert.That(async () => await textBoxInput.EvaluateAsync<int>("i => i.selectionStart"), Is.EqualTo(expectCursor).After(3000, 100));
		Assert.That(async () => await textBoxInput.EvaluateAsync<int>("i => i.selectionEnd"), Is.EqualTo(expectCursor).After(3000, 100));

		var expectedNextCellInput = await page.WaitForSelectorAsync($"tbody>tr:nth-of-type({expectedRow})>td:nth-of-type({expectedCol + 1}).datagrid__cell--edit input", playWrightPageWaitForSelectorOptions);

		if (moveToNewRow)
		{
			Assert.That(await expectedNextCellInput.InputValueAsync(), Is.EqualTo(string.Empty));
		}
		else
		{
			Assert.That(await expectedNextCellInput.InputValueAsync(), Is.EqualTo($"cell{expectedRow}{expectedCol}"));
		}
	}

	[Test, WithPlaywrightPage]
	public async Task PressingCtrlUpArrowKeyShouldMoveToTheFirstCellOfCurrentColumn()
	{
		await using var ctx = new InMemoryTestServerContext();
		var dataGrid = default(DataGrid);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(dataGrid = SetupDataGrid());
			return form;
		});
		await LoadPageAndForm(page);

		await AssertFocusedCellLocationAfterKeyPress(page, dataGrid, 4, 1, "Control+ArrowUp", 1, 1);
		await AssertFocusedCellLocationAfterKeyPress(page, dataGrid, 1, 1, "Control+ArrowUp", 1, 1);
	}

	[Test, WithPlaywrightPage]
	public async Task PressingCtrlDownArrowKeyShouldMoveToTheLastCellOfCurrentColumn()
	{
		await using var ctx = new InMemoryTestServerContext();
		var dataGrid = default(DataGrid);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(dataGrid = SetupDataGrid());
			return form;
		});
		await LoadPageAndForm(page);

		await AssertFocusedCellLocationAfterKeyPress(page, dataGrid, 1, 1, "Control+ArrowDown", 4, 1);
		await AssertFocusedCellLocationAfterKeyPress(page, dataGrid, 4, 1, "Control+ArrowDown", 4, 1);
	}

	[Test, WithPlaywrightPage]
	public async Task PressingCtrlLeftArrowKeyShouldMoveToTheFirstCellOfCurrentRow()
	{
		await using var ctx = new InMemoryTestServerContext();
		var dataGrid = default(DataGrid);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(dataGrid = SetupDataGrid());
			return form;
		});
		await LoadPageAndForm(page);

		await AssertFocusedCellLocationAfterKeyPress(page, dataGrid, 1, 4, "Control+ArrowLeft", 1, 1);
		await AssertFocusedCellLocationAfterKeyPress(page, dataGrid, 1, 1, "Control+ArrowLeft", 1, 1);
	}

	[Test, WithPlaywrightPage]
	public async Task PressingCtrlRightArrowKeyShouldMoveToTheLastCellOfCurrentRow()
	{
		await using var ctx = new InMemoryTestServerContext();
		var dataGrid = default(DataGrid);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(dataGrid = SetupDataGrid());
			return form;
		});
		await LoadPageAndForm(page);

		await AssertFocusedCellLocationAfterKeyPress(page, dataGrid, 1, 1, "Control+ArrowRight", 1, 4);
		await AssertFocusedCellLocationAfterKeyPress(page, dataGrid, 4, 4, "Control+ArrowRight", 4, 4);
	}

	[Test, WithPlaywrightPage]
	public async Task PressingShiftUpArrowKeyShouldSelectCurrentRowAndTheRowAboveIfAny()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(SetupDataGrid());
			return form;
		});
		await LoadPageAndForm(page);

		await AssertRowsSelectedAfterKeyPress(page, 1, "Shift+ArrowUp", new int[] { 1 });
		await AssertRowsSelectedAfterKeyPress(page, 2, "Shift+ArrowUp", new int[] { 2, 1 });
	}

	[Test, WithPlaywrightPage]
	public async Task PressingShiftDownArrowKeyShouldSelectCurrentRowAndTheRowBelowIfAny()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(SetupDataGrid());
			return form;
		});
		await LoadPageAndForm(page);

		await AssertRowsSelectedAfterKeyPress(page, 3, "Shift+ArrowDown", new int[] { 3, 4 });
		await AssertRowsSelectedAfterKeyPress(page, 4, "Shift+ArrowDown", new int[] { 4 });
	}

	[Test, WithPlaywrightPage]
	public async Task PressingCtrlShiftUpArrowKeyShouldSelectCurrentRowAndAllRowsAboveIfAny()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(SetupDataGrid());
			return form;
		});
		await LoadPageAndForm(page);

		await AssertRowsSelectedAfterKeyPress(page, 1, "Control+Shift+ArrowUp", new int[] { 1 });
		await AssertRowsSelectedAfterKeyPress(page, 4, "Control+Shift+ArrowUp", new int[] { 4, 3, 2, 1 });
	}

	[Test, WithPlaywrightPage]
	public async Task PressingCtrlShiftDownArrowKeyShouldSelectCurrentRowAndAllRowsBelowIfAny()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(SetupDataGrid());
			return form;
		});
		await LoadPageAndForm(page);

		await AssertRowsSelectedAfterKeyPress(page, 1, "Control+Shift+ArrowDown", new int[] { 1, 2, 3, 4 });
		await AssertRowsSelectedAfterKeyPress(page, 4, "Control+Shift+ArrowDown", new int[] { 4 });
	}

		[Test, WithPlaywrightPage]
		public async Task CanScrollToInvisibleRowWhenOnlyRenderVisibleRows()
		{
			await using var ctx = new InMemoryTestServerContext();
			DataGrid dataGrid = null;

			var page = await ctx.LoadFormAsync(() =>
			{
				var form = new Form() { Size = new Size(500, 200) };
				var table = new DataTable("data");
				table.Columns.Add(new DataColumn("one") { DataType = typeof(string) });

			for (var i = 0; i < 50; i++)
			{
				table.Rows.Add($"cell({i + 1}, 1)");
			}

			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One Header", Width = 80, ReadOnly = true });

				dataGrid = new DataGrid { Dock = DockStyle.Fill, DataSource = table, AllowNavigation = false };
				dataGrid.TableStyles.Add(tableStyle);
				form.Controls.Add(dataGrid);
				return form;
			});

			var table = await page.WaitForSelectorAsync("table");
			var trs = await table.QuerySelectorAllAsync("tbody tr");
			var resizers = await table.QuerySelectorAllAsync("th .datagrid__column_resizer");

			await SelectRow(trs[0]);
			await ScrollToAsync(page, 0, 320);
			await ResizeColumnWidth(page, resizers[0]);
			AssertRowVisible(dataGrid, 0, expectedVisibility: true);
		}

	[Test, WithPlaywrightPage, Explicit("only render rows that are either in visible range or currently being edited")]
	public async Task InvisibleRowsRenderCorrectHeight()
	{
		var dataGrid = default(DataGrid);

		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(500, 200) };
			var table = new DataTable("data");
			table.Columns.Add(new DataColumn("one") { DataType = typeof(string) });

			for (var i = 0; i < 50; i++)
			{
				table.Rows.Add($"cell({i + 1}, 1)");
			}

			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One Header", Width = 80, ReadOnly = true });

			dataGrid = new DataGrid { Dock = DockStyle.Fill, DataSource = table, AllowNavigation = false };
			dataGrid.TableStyles.Add(tableStyle);
			form.Controls.Add(dataGrid);

			return form;
		});

		var firstVisibleRow = dataGrid.FirstVisibleRow;
		var visibleRowCount = dataGrid.VisibleRowCount;

		var table = await page.WaitForSelectorAsync("table");
		var trs = await table.QuerySelectorAllAsync("tbody tr");

		var firstInvisibleRow = trs[firstVisibleRow + visibleRowCount + 1];
		Assert.That(await firstInvisibleRow.GetComputedStyleAsync("height"), Is.EqualTo("16px"));
	}

	[SuppressMessage("CargoWiseOne", "CW1104:DoNotUseSystemWindowsTabControl", Justification = "Testing")]
	[Test, WithPlaywrightPage]
	public async Task ScrollToSelectedRowWhileGridBeingHidden([Values(0, 25, 50, 100)] int delayInMs)
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		DataGrid dataGrid = null;
		TabControl tabControl = null;
		Exception threadException = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			Application.ThreadException += Application_ThreadException;
			form = new Form() { Size = new Size(500, 200) };
			tabControl = new TabControl();
			tabControl.Dock = DockStyle.Fill;
			var tabPage1 = new TabPage();
			tabPage1.Text = "One";
			tabControl.TabPages.Add(tabPage1);
			dataGrid = CreateNewGrid<TestDataGrid>();
			dataGrid.ReadOnly = true;
			tabPage1.Controls.Add(dataGrid);
			var tabPage2 = new TabPage();
			tabPage2.Text = "Two";
			tabPage2.Controls.Add(new Label { Text = "Hello" });
			tabControl.TabPages.Add(tabPage2);
			form.Controls.Add(tabControl);
			form.Shown += (s, e) => form.BeginInvoke(() =>
			{
				dataGrid.CurrentCell = new DataGridCell(40, 1);
			});
			form.Shown += (s, e) => form.BeginInvoke(() =>
			{
				Thread.Sleep(delayInMs);
				tabControl.SelectedIndex = 1;
			});
			return form;
		});
		try
		{
			await page.WaitForSelectorAsync(".label");

			await page.Locator("button", new PageLocatorOptions() { HasTextString = "One" }).ClickAsync();
			await page.WaitForSelectorAsync("table");

			Assert.That(threadException, Is.Null);
		}
		finally
		{
			Application.ThreadException -= Application_ThreadException;
		}

		void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
		{
			threadException = e.Exception;
		}
	}

	[Test, WithPlaywrightPage]
	public async Task VisibleRowCountWhenScrollRequired([Values(150, 200, 333, 499)] int height)
	{
		await using var ctx = new InMemoryTestServerContext();
		DataGrid dataGrid = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(500, 500) };
			dataGrid = CreateNewGrid<TestDataGrid>();
			dataGrid.Dock = DockStyle.None;
			dataGrid.Width = 500;
			dataGrid.Height = height;
			form.Controls.Add(dataGrid);

			return form;
		});

		await page.WaitForSelectorAsync("table tbody tr");
		var dataGridElement = await page.QuerySelectorAsync(".datagrid");
		var dataGridElementBounds = await dataGridElement.BoundingBoxAsync();
		var trs = await page.QuerySelectorAllAsync("table tbody tr");
		var actualVisibleRowCount = 0;
		foreach (var tr in trs)
		{
			var trBounds = await tr.BoundingBoxAsync();
			if (trBounds.Y < dataGridElementBounds.Y + dataGridElementBounds.Height)
			{
				actualVisibleRowCount++;
			}
		}

		Assert.That(dataGrid.VisibleRowCount, Is.EqualTo(actualVisibleRowCount));
	}

	[Test]
	public async Task VisibleRowCountWhenNoScrollRequired()
	{
		using var ctx = new WinzorTestContext();
		DataGrid dataGrid = null;
		await ctx.RenderControlOnFormAsync(() =>
		{
			dataGrid = CreateNewGrid<DataGrid>(3);
			dataGrid.ReadOnly = true;
			return dataGrid;
		});

		Assert.That(dataGrid.VisibleRowCount, Is.EqualTo(3));
	}

	[TestCase(0, 9)]
	[TestCase(10, 10)]
	[TestCase(40, 10)]
	public async Task OnlyVisibleRowsRendered(int rowStart, int expectedRenderedRows)
	{
		using var ctx = new WinzorTestContext();
		DataGrid dataGrid = null;
		DataGridColumnStyleStub columnStyle = null;
		var rowIndex = rowStart;

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var table = new DataTable("data");
			var column = new DataColumn("one");
			column.DataType = typeof(string);
			table.Columns.Add(column);

			for (var i = 0; i < 50; i++)
			{
				table.Rows.Add(i.ToString());
			}

			dataGrid = new DataGrid();
			dataGrid.Width = 500;
			dataGrid.Height = 160 + dataGrid.HeaderHeight;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;

			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			columnStyle = new DataGridColumnStyleStub { MappingName = "one", HeaderText = "One Header" };
			columnStyle.Width = 80;
			tableStyle.GridColumnStyles.Add(columnStyle);
			dataGrid.TableStyles.Add(tableStyle);

			return dataGrid;
		});

		await dataGrid.OnScrollAsync(rowStart * 16, 0);

		var rows = rendered.FindAll("tbody tr");

		Assert.That(rows.Count, Is.EqualTo(expectedRenderedRows));

		for (var i = 0; i < expectedRenderedRows; i++)
		{
			var td = rows[i].Children[1];
			var cssClass = td.Attributes["class"].Value;

			if (cssClass.Contains("datagrid__cell--edit"))
			{
				Assert.That(rows[i].ChildNodes[1].ToMarkup(), Does.Contain("value=\"0\""));
				if (rowStart > 0)
				{
					Assert.That(rows[i].Attributes["class"].Value, Does.Contain("datagrid__row--hidden"));
				}
				else
				{
					rowIndex++;
				}
			}
			else
			{
				Assert.That(rows[i].ChildNodes[1].TextContent, Is.EqualTo(rowIndex.ToString()));
				rowIndex++;
			}
		}
	}

	[Test, WithPlaywrightPage]
	public async Task VisibleRowsRenderedAfterScroll([Values(50, 100, 200, 636)] int scrollDeltaX)
	{
		await using var ctx = new InMemoryTestServerContext();
		DataGrid dataGrid = null;
		DataGridColumnStyleStub columnStyle = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(500, 500) };

			var table = new DataTable("data");
			var column = new DataColumn("one");
			column.DataType = typeof(string);
			table.Columns.Add(column);

			for (var i = 0; i < 50; i++)
			{
				table.Rows.Add($"row {i + 1}");
			}

			dataGrid = new DataGrid();
			dataGrid.ReadOnly = true;
			dataGrid.Width = 500;
			dataGrid.Height = 200;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;

			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			columnStyle = new DataGridColumnStyleStub { MappingName = "one", HeaderText = "One Header" };
			columnStyle.Width = 80;
			tableStyle.GridColumnStyles.Add(columnStyle);
			dataGrid.TableStyles.Add(tableStyle);

			form.Controls.Add(dataGrid);
			return form;
		});

		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
		await page.WaitForSelectorAsync("table tbody tr");
		var dataGridElement = await page.QuerySelectorAsync(".datagrid");
		var dataGridElementBounds = await dataGridElement.BoundingBoxAsync();
		var headerElement = await page.QuerySelectorAsync("table tbody tr");
		var headerElementBounds = await headerElement.BoundingBoxAsync();
		await page.Mouse.ClickAsync(150, 150);

		columnStyle.GetRenderContentCount = 0;

		await page.Mouse.WheelAsync(0, scrollDeltaX);

		await page.WaitForTimeoutAsync(500);

		var trs = await page.QuerySelectorAllAsync("table tbody tr");
		var actualVisibleRows = 1;

		var rowNumber = dataGrid.FirstVisibleRow;

		for (var i = 0; i < trs.Count; i++)
		{
			var tr = trs[i];
			var td = await tr.QuerySelectorAsync("td:nth-child(2)");
			var trClass = await tr.GetAttributeAsync("class");
			var tdClass = await td.GetAttributeAsync("class");

			if (!trClass.Contains("datagrid__row--hidden"))
			{
				var value = string.Empty;

				if (trClass.Contains("datagrid__row--edit"))
				{
					var input = await page.WaitForSelectorAsync("input");
					value = await input.InputValueAsync();
				}
				else
				{
					value = await td.InnerTextAsync();
				}

				Assert.That(value, Does.Contain($"row {rowNumber + 1}"));
				rowNumber++;
				actualVisibleRows++;
			}
		}

		Assert.That(columnStyle.GetRenderContentCount, Is.LessThanOrEqualTo(actualVisibleRows));
	}

	[Test, WithPlaywrightPage]
	public async Task HeaderHeight([Values(true, false)] bool gridLayoutConfigurable)
	{
		await using var ctx = new InMemoryTestServerContext();
		DataGridForTest dataGrid = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			dataGrid = new DataGridForTest();
			dataGrid.IsGridLayoutConfigurable = gridLayoutConfigurable;
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = CreateMockData(0, 1);
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnStyle1 = new DataGridTextBoxColumn { MappingName = "col1", HeaderText = "One Header" };
			columnStyle1.Width = 100;
			tableStyle.GridColumnStyles.Add(columnStyle1);
			dataGrid.TableStyles.Add(tableStyle);
			form.Controls.Add(dataGrid);

			return form;
		});

		var th = await page.WaitForSelectorAsync("table thead th");
		Assert.That(dataGrid.HeaderHeight, Is.EqualTo((await th.BoundingBoxAsync()).Height));
	}

	[Test, WithPlaywrightPage(Headless = false)]
	public async Task EnsureVisibleDoesNotTriggerMultiScroll()
	{
		await using var ctx = new InMemoryTestServerContext();
		DataGrid dataGrid = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			dataGrid = CreateNewGrid<TestDataGrid>();
			dataGrid.Dock = DockStyle.Fill;
			var form = new Form() { Size = new Size(300, 250) };
			form.Controls.Add(dataGrid);
			return form;
		});

		await page.WaitForSelectorAsync("table tbody tr");
		await dataGrid.InvokeWinzorDispatcherAsync(() => dataGrid.CurrentCell = new DataGridCell(23, 0));
		Assert.That(async () => await page.EvaluateAsync<int>("document.getElementsByClassName('datagrid')[0].scrollTop"), Is.GreaterThan(100).After(1000, 100));
		var bottomRowScrollTop = await page.EvaluateAsync<int>("document.getElementsByClassName('datagrid')[0].scrollTop");
		Assert.That(async () => await page.EvaluateAsync<int>("document.getElementsByClassName('datagrid')[0].scrollLeft"), Is.EqualTo(0).After(1000, 100));

		await dataGrid.InvokeWinzorDispatcherAsync(() => dataGrid.CurrentCell = new DataGridCell(22, 1));
		Assert.That(async () => await page.EvaluateAsync<int>("document.getElementsByClassName('datagrid')[0].scrollTop"), Is.EqualTo(bottomRowScrollTop).After(1000, 100));
		Assert.That(async () => await page.EvaluateAsync<int>("document.getElementsByClassName('datagrid')[0].scrollLeft"), Is.EqualTo(0).After(1000, 100));

		await dataGrid.InvokeWinzorDispatcherAsync(() => dataGrid.CurrentCell = new DataGridCell(22, 2));
		Assert.That(async () => await page.EvaluateAsync<int>("document.getElementsByClassName('datagrid')[0].scrollTop"), Is.EqualTo(bottomRowScrollTop).After(1000, 100));
		Assert.That(async () => await page.EvaluateAsync<int>("document.getElementsByClassName('datagrid')[0].scrollLeft"), Is.EqualTo(100).Within(2).After(1000, 100));

		await dataGrid.InvokeWinzorDispatcherAsync(() => dataGrid.CurrentCell = new DataGridCell(23, 2));
		Assert.That(async () => await page.EvaluateAsync<int>("document.getElementsByClassName('datagrid')[0].scrollTop"), Is.EqualTo(bottomRowScrollTop).After(1000, 100));
		Assert.That(async () => await page.EvaluateAsync<int>("document.getElementsByClassName('datagrid')[0].scrollLeft"), Is.EqualTo(100).Within(2).After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task EnsureVisibleOnlyScrollToMakeHalfVisibleColumnTotallyVisible()
	{
		await using var ctx = new InMemoryTestServerContext();
		DataGrid dataGrid = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			dataGrid = CreateNewGrid<DataGrid>(5);
			dataGrid.Dock = DockStyle.Fill;
			var form = new Form() { Size = new Size(300, 250) };
			form.Controls.Add(dataGrid);
			return form;
		});

		// the 3rd column is half visible and should scroll one column rightwards
		await dataGrid.InvokeWinzorDispatcherAsync(() => dataGrid.CurrentCell = new DataGridCell(0, 2));
		Assert.That(async () => await page.EvaluateAsync<int>("document.getElementsByClassName('datagrid')[0].scrollLeft"), Is.EqualTo(100).Within(0.2).After(2000, 100));

		// scroll back a little to make the 1st column half visible
		await page.EvaluateAsync($"document.getElementsByClassName('datagrid')[0].scrollLeft = 75");
		Assert.That(async () => await page.EvaluateAsync<int>("document.getElementsByClassName('datagrid')[0].scrollLeft"), Is.EqualTo(75).Within(0.2).After(2000, 100));

		await dataGrid.InvokeWinzorDispatcherAsync(() => dataGrid.CurrentCell = new DataGridCell(0, 0));
		Assert.That(async () => await page.EvaluateAsync<int>("document.getElementsByClassName('datagrid')[0].scrollLeft"), Is.EqualTo(0).Within(0.2).After(2000, 100));
	}

	[Test, WithPlaywrightPage]
	[SuppressMessage("CargoWiseOne", "CW1104:Do not use System.Windows.Forms.TabControl Class")]
	public async Task DataGridRenderedWhenLayoutChangedBeforeInitialRender()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new FormWithBlockingRender();
			var tabControl = new TabControl();
			tabControl.Dock = DockStyle.Fill;
			form.Controls.Add(tabControl);
			var tabPage = new TabPage();
			tabPage.Text = "One";
			tabControl.TabPages.Add(tabPage);
			tabPage = new TabPage();
			tabPage.Text = "Two";
			tabControl.TabPages.Add(tabPage);
			var dataGrid = new DataGrid();
			InitializeTestGrid(dataGrid);
			tabPage.Controls.Add(dataGrid);

			form.Shown += (s, e) => form.BeginInvoke(() =>
			{
				tabControl.SelectedTab = tabPage;
				form.ContinueRenderEvent.Set();
			});

			return form;
		});

		await page.WaitForSelectorAsync("table");

		Assert.That(await page.Locator("tbody > tr:nth-child(1) > td.datagrid__cell--edit input").InputValueAsync(), Is.EqualTo("row1"));
		Assert.That(await page.Locator("tbody > tr:nth-child(1) > td:nth-child(3)").TextContentAsync(), Is.EqualTo("11"));
		Assert.That(await page.Locator("tbody > tr:nth-child(2) > td:nth-child(2)").TextContentAsync(), Is.EqualTo("row2"));
		Assert.That(await page.Locator("tbody > tr:nth-child(2) > td:nth-child(3)").TextContentAsync(), Is.EqualTo("22"));
	}

	[Test, WithPlaywrightPage]
	[SuppressMessage("CargoWiseOne", "CW1104:Do not use System.Windows.Forms.TabControl Class")]
	public async Task DataGridRenderedWhenLayoutChangedDuringInitialRender()
	{
		await using var ctx = new InMemoryTestServerContext();
		DataGridWithBockingOnBeforeRender dataGrid = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new FormWithBlockingRender();
			var tabControl = new TabControl();
			tabControl.Dock = DockStyle.Fill;
			form.Controls.Add(tabControl);
			var tabPage = new TabPage();
			tabPage.Text = "One";
			tabControl.TabPages.Add(tabPage);
			tabPage = new TabPage();
			tabPage.Text = "Two";
			tabControl.TabPages.Add(tabPage);
			dataGrid = new DataGridWithBockingOnBeforeRender();
			InitializeTestGrid(dataGrid);
			tabPage.Controls.Add(dataGrid);

			form.Shown += (s, e) => form.BeginInvoke(() =>
			{
				dataGrid.OnBeforeRenderEntered = form.ContinueRenderEvent;
				tabControl.SelectedTab = tabPage;
			});

			return form;
		});
		await page.WaitForSelectorAsync("table");

		dataGrid.ContinueOnBeforeRender.Set();

		await page.WaitForSelectorAsync("tbody > tr:nth-child(1) > td.datagrid__cell--edit");
		Assert.That(await page.Locator("tbody > tr:nth-child(1) > td.datagrid__cell--edit input").InputValueAsync(), Is.EqualTo("row1"));
		Assert.That(await page.Locator("tbody > tr:nth-child(1) > td:nth-child(3)").TextContentAsync(), Is.EqualTo("11"));
		Assert.That(await page.Locator("tbody > tr:nth-child(2) > td:nth-child(2)").TextContentAsync(), Is.EqualTo("row2"));
		Assert.That(await page.Locator("tbody > tr:nth-child(2) > td:nth-child(3)").TextContentAsync(), Is.EqualTo("22"));
	}

	[Test, WithPlaywrightPage]
	public async Task DataRendered_NoUnwantedScrollBar()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form() { ClientSize = new Size(200, 200) };
			var table = new DataTable("data");
			var column1 = new DataColumn("one");
			column1.DataType = typeof(string);
			table.Columns.Add(column1);
			var column2 = new DataColumn("two");
			column2.DataType = typeof(string);
			table.Columns.Add(column2);
			table.Rows.Add("1.1", "1.2");

			var dataGrid = new DataGrid();
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnStyle1 = new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One Header" };
			columnStyle1.Width = 80;
			var columnStyle2 = new DataGridTextBoxColumn { MappingName = "two", HeaderText = "Two Header" };
			columnStyle2.Width = 80;
			tableStyle.GridColumnStyles.Add(columnStyle1);
			tableStyle.GridColumnStyles.Add(columnStyle2);
			dataGrid.TableStyles.Add(tableStyle);
			form.Controls.Add(dataGrid);
			return form;
		});
		var grid = await page.WaitForSelectorAsync(".datagrid");

		// Column one width(80) add Column two width(80) add Fix column width(40) equals 200, then no scrollbar
		Assert.That(async () => await grid.EvaluateAsync<bool>("e => e.scrollWidth == e.clientWidth"), Is.True);

		// Column one width(80) add Column two width(80) add Fix column width(40) greater than 190, then scrollbar appears
		await form.OnBrowserSizeChangedAsync(190, 190);
		Assert.That(async () => await grid.EvaluateAsync<bool>("e => e.scrollWidth > e.clientWidth"), Is.True);
	}

	[TestCase(200, 49, 12, 0)]
	[TestCase(200, 42, 12, 0)]
	[TestCase(333, 49, 20, 0)]
	[TestCase(333, 42, 20, 0)]
	[Test, WithPlaywrightPage]
	public async Task SetCurrentCellScrollToCorrectRowRange(int height, int selectedRow, int expectedRowNumber, int renderContentCount)
	{
		await using var ctx = new InMemoryTestServerContext();
		DataGrid dataGrid = null;
		DataGridColumnStyleStub columnStyle = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(500, 500) };

			var table = new DataTable("data");
			var column = new DataColumn("one");
			column.DataType = typeof(string);
			table.Columns.Add(column);

			for (var i = 0; i < 50; i++)
			{
				table.Rows.Add($"row {i + 1}");
			}

			dataGrid = new DataGrid();
			dataGrid.ReadOnly = true;
			dataGrid.Width = 500;
			dataGrid.Height = height;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;

			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			columnStyle = new DataGridColumnStyleStub { MappingName = "one", HeaderText = "One Header" };
			columnStyle.Width = 80;
			tableStyle.GridColumnStyles.Add(columnStyle);
			dataGrid.TableStyles.Add(tableStyle);

			form.Controls.Add(dataGrid);
			return form;
		});

		await page.WaitForSelectorAsync("table tbody tr");
		var dataGridElement = await page.QuerySelectorAsync(".datagrid");
		var dataGridElementBounds = await dataGridElement.BoundingBoxAsync();
		var headerElement = await page.QuerySelectorAsync("table tbody tr");
		var headerElementBounds = await headerElement.BoundingBoxAsync();

		await dataGrid.InvokeWinzorDispatcherAsync(() =>
		{
			dataGrid.CurrentCell = new DataGridCell(selectedRow, -1);
			dataGrid.BeginInvoke(() => columnStyle.GetRenderContentCount = 0); // reset count immediatly after this render, to check that no additional rows are render after scroll
		});
		await page.WaitForTimeoutAsync(500);
		var selectedRowVisible = false;
		var trs = await page.QuerySelectorAllAsync("table tbody tr");

		Assert.That(trs.Count, Is.EqualTo(expectedRowNumber));

		var rowNumber = dataGrid.FirstVisibleRow;

		for (var i = 0; i < trs.Count; i++)
		{
			var tr = trs[i];
			var td = await tr.QuerySelectorAsync("td:nth-child(2)");
			var tdClass = await td.GetAttributeAsync("class");

			if (rowNumber == selectedRow && tdClass.Contains("datagrid__cell--edit"))
			{
				selectedRowVisible = true;
			}
			else
			{
				Assert.That(await td.InnerTextAsync(), Does.Contain($"row {rowNumber + 1}"));
			}
			rowNumber++;
		}

		Assert.That(selectedRowVisible, Is.True);
		Assert.That(columnStyle.GetRenderContentCount, Is.EqualTo(renderContentCount));
	}

	[Test]
	public async Task ScrollingUpToCellSetsTargetRowAsFirstVisible()
	{
		using var ctx = new WinzorTestContext();
		DataGrid dataGrid = null;

		await ctx.RenderControlOnFormAsync(() =>
		{
			var table = new DataTable("data");
			var column = new DataColumn("one");
			column.DataType = typeof(string);
			table.Columns.Add(column);

			for (var i = 0; i < 50; i++)
			{
				table.Rows.Add(i.ToString());
			}

			dataGrid = new DataGrid();
			dataGrid.Width = 500;
			dataGrid.Height = 200;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;

			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnStyle = new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One Header" };
			columnStyle.Width = 80;
			tableStyle.GridColumnStyles.Add(columnStyle);
			dataGrid.TableStyles.Add(tableStyle);

			return dataGrid;
		});

		await dataGrid.InvokeWinzorDispatcherAsync(() => dataGrid.CurrentCell = new DataGridCell(49, 0));
		Assert.That(dataGrid.FirstVisibleRow, Is.GreaterThan(10));
		await dataGrid.InvokeWinzorDispatcherAsync(() => dataGrid.CurrentCell = new DataGridCell(3, 0));
		Assert.That(dataGrid.FirstVisibleRow, Is.EqualTo(3));
	}

	[Test]
	public async Task CtrlAShouldSelectAllRows()
	{
		using var ctx = new WinzorTestContext();
		DataGrid dataGrid = null;

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var table = new DataTable("data");
			var column = new DataColumn("one");
			column.DataType = typeof(string);
			table.Columns.Add(column);

			for (var i = 0; i < 10; i++)
			{
				table.Rows.Add(i.ToString());
			}

			dataGrid = new DataGrid { Width = 500, Height = 200, DataSource = table, AllowNavigation = false };

			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnStyle = new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One Header", Width = 80 };

			tableStyle.GridColumnStyles.Add(columnStyle);
			dataGrid.TableStyles.Add(tableStyle);

			return dataGrid;
		});

		Assert.That(rendered.FindAll("tr.datagrid__row--selected").Count, Is.EqualTo(0));

		var input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		await rendered.KeyPressAsync(Keys.Control | Keys.A, input);

		Assert.That(rendered.FindAll("tr.datagrid__row--selected").Count, Is.EqualTo(10));
	}

	[Test, WithPlaywrightPage]
	public async Task VisibleRowsWithPositionChanged()
	{
		await using var ctx = new InMemoryTestServerContext();
		DataGrid dataGrid = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(500, 500) };
			dataGrid = CreateNewGrid<TestDataGrid>();
			dataGrid.Dock = DockStyle.None;
			dataGrid.Width = 500;
			dataGrid.Height = 100;
			form.Controls.Add(dataGrid);

			return form;
		});

		await dataGrid.InvokeWinzorDispatcherAsync(() => dataGrid.ListManager.Position = 49);

		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
		await page.WaitForSelectorAsync("table tbody tr");
		await page.QuerySelectorAsync(".datagrid");
		await page.QuerySelectorAllAsync("table tbody tr");
		Assert.That(dataGrid.VisibleRowCount, Is.GreaterThan(1));
	}

	[Test, WithPlaywrightPage]
	public async Task ScorllToRightRowWhenFirstRenderData()
	{
		await using var ctx = new InMemoryTestServerContext();
		DataGrid dataGrid = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Width = 200, Height = 200 };
			dataGrid = new DataGrid();
			dataGrid.Dock = DockStyle.Fill;
			form.Controls.Add(dataGrid);
			return form;
		});

		Assert.That(dataGrid, Is.Not.Null);
		await dataGrid.InvokeWinzorDispatcherAsync(() =>
		{
			dataGrid.DataSource = CreateNewTableData(50);
			dataGrid.CurrentCell = new DataGridCell(30, 3);
		});

		AssertRowVisible(dataGrid, 30, expectedVisibility: true);
		Assert.That(async () => await GetDataGridScrollTopAsync(page), Is.GreaterThan(0).After(1000, 100));
		Assert.That(async () => await GetDataGridScrollLeftAsync(page), Is.GreaterThan(0).After(1000, 100));
	}

	[WithPlaywrightPage]
	[TestCase(0)]
	[TestCase(1)]
	public async Task DataGridHasCorrectTabIndex(int tabIndex)
	{
		await using var ctx = new InMemoryTestServerContext();
		DataGrid grid = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			grid = new DataGrid() { Top = 100, TabIndex = tabIndex };
			form.Controls.Add(grid);
			form.Controls.Add(new TextBox() { Top = 200 });
			form.Size = new Size { Width = 600, Height = 400 };

			return form;
		});

		var datagrid = await page.WaitForSelectorAsync(".datagrid");

		var datagridTabIndex = await datagrid.EvaluateAsync<int>("(e) => e.tabIndex");
		Assert.That(datagridTabIndex, Is.EqualTo(tabIndex));
	}

	[Test]
	public async Task MultiLineSelectionResetWhenSelectingCell()
	{
		DataGrid dataGrid = null;

		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var table = new DataTable("data");
			table.Columns.Add(new DataColumn("one") { DataType = typeof(string) });
			table.Columns.Add(new DataColumn("two") { DataType = typeof(string) });
			table.Columns.Add(new DataColumn("three") { DataType = typeof(string) });
			table.Columns.Add(new DataColumn("four") { DataType = typeof(string) });
			
			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One", Width = 80 });
			tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn { MappingName = "two", HeaderText = "Two", Width = 80 });
			tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn { MappingName = "three", HeaderText = "Three", Width = 80 });
			tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn { MappingName = "four", HeaderText = "Four", Width = 80 });

			for (var i = 0; i < 10; i++)
			{
				var str = i.ToString();
				table.Rows.Add(str, str, str, str);
			}

			dataGrid = new DataGrid { Width = 500, Height = 200, DataSource = table, AllowNavigation = false };
			dataGrid.TableStyles.Add(tableStyle);

			return dataGrid;
		});

		Assert.That(rendered.FindAll("tr.datagrid__row--selected").Count, Is.EqualTo(0), "Selected row count (on form load) incorrect");
		var input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		await rendered.KeyPressAsync(Keys.Control | Keys.A, input);
		Assert.That(rendered.FindAll("tr.datagrid__row--selected").Count, Is.EqualTo(10), "Selected row count (after select all) incorrect");

		dataGrid.currentCol = 2;
		dataGrid.currentRow = 2;
		await dataGrid.OnCellFocusInAsync(new WinzorFocusInEventArgs() { InitiatedFromServer = false }, 2, 3);
		Assert.That(rendered.FindAll("tr.datagrid__row--selected").Count, Is.EqualTo(0), "Selected row count (after OnCellFocusInAsync) incorrect");
	}

	[Test]
	public async Task DataGridMultiLineSelectionNotLostWhenContextMenuIsActive([Values] bool contextMenuActive)
	{
		DataGrid dataGrid = null;

		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var table = new DataTable("data");
			table.Columns.Add(new DataColumn("one") { DataType = typeof(string) });
			table.Columns.Add(new DataColumn("two") { DataType = typeof(string) });
			table.Columns.Add(new DataColumn("three") { DataType = typeof(string) });
			table.Columns.Add(new DataColumn("four") { DataType = typeof(string) });

			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One", Width = 80 });
			tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn { MappingName = "two", HeaderText = "Two", Width = 80 });
			tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn { MappingName = "three", HeaderText = "Three", Width = 80 });
			tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn { MappingName = "four", HeaderText = "Four", Width = 80 });

			for (var i = 0; i < 10; i++)
			{
				var str = i.ToString();
				table.Rows.Add(str, str, str, str);
			}

			dataGrid = new DataGrid { Width = 500, Height = 200, DataSource = table, AllowNavigation = false };
			dataGrid.TableStyles.Add(tableStyle);

			return dataGrid;
		});

		Assert.That(rendered.FindAll("tr.datagrid__row--selected").Count, Is.EqualTo(0), "Selected row count (on form load) incorrect");

		var input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		await rendered.KeyPressAsync(Keys.Control | Keys.A, input);
		Assert.That(rendered.FindAll("tr.datagrid__row--selected").Count, Is.EqualTo(10), "Selected row count (after select all) incorrect");

		dataGrid.currentCol = 1;
		dataGrid.currentRow = 1;
		dataGrid.HasActiveContextMenu = contextMenuActive;
		await dataGrid.OnCellFocusInAsync(new WinzorFocusInEventArgs() { InitiatedFromServer = false }, 2, 3);

		Assert.That(rendered.FindAll("tr.datagrid__row--selected").Count, Is.EqualTo(contextMenuActive ? 10 : 0), "Selected row count (after OnCellFocusInAsync) incorrect");
	}

	[Test]
	public async Task PreloadDataGridJSInterop()
	{
		using var ctx = new WinzorTestContext();
		var services = ctx.MockCargoWiseClientServices;
		var interop = new Mock<IGridJSInterop>();
		interop.Setup(i => i.PreloadInterop());
		ctx.Services.AddScoped(_ => interop.Object);

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var dataGrid = CreateNewGrid<TestDataGrid>();
			dataGrid.Dock = DockStyle.None;
			dataGrid.Width = 500;
			dataGrid.Height = 100;
			return dataGrid;
		});

		interop.Verify(e => e.PreloadInterop(), Times.Once());
	}

	[Test, WithPlaywrightPage]
	[WithDefaultLatencyNetworkEffect(Latency = 300)]
	public async Task FastPressMultipleArrowKeysWithNetworkLatency()
	{
		await using var ctx = new InMemoryTestServerContext();
		var dataGrid = default(DataGrid);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Width = 600, Height = 300 };
			dataGrid = SetupDataGrid(false);
			dataGrid.Width = 1000;
			dataGrid.Height = 1000;
			form.Controls.Add(dataGrid);
			return form;
		}, serverBaseUrl: WithDefaultLatencyNetworkEffect.ServerBaseUrl);

		await Task.Delay(1000);
		Assert.That(dataGrid.CurrentCell, Is.EqualTo(new DataGridCell(0, 0)));

		await page.Keyboard.PressAsync("ArrowRight");
		await page.Keyboard.PressAsync("ArrowRight");
		await page.Keyboard.PressAsync("ArrowDown");
		await page.Keyboard.PressAsync("ArrowDown");
		Assert.That(() => dataGrid.CurrentCell, Is.EqualTo(new DataGridCell(2, 2)).After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task DataGridMultipleSelectionAfterScrollAsync()
	{
		await using var ctx = new InMemoryTestServerContext();
		DataGrid grid = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var table = new DataTable("data");
			table.Columns.Add(new DataColumn("one") { DataType = typeof(string) });
			for (var i = 0; i < 50; i++)
			{
				table.Rows.Add(i.ToString());
			}

			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One", Width = 80 });

			grid = new DataGridWithWholeRowSelect { Dock = DockStyle.Fill, DataSource = table, ReadOnly = true };
			grid.TableStyles.Add(tableStyle);

			var form = new Form { Width = 400, Height = 300 };
			form.Controls.Add(grid);
			return form;
		});

		await ScrollToAsync(page, 0, 160);												// RowNumbers in visible range: 11...27
		await page.Mouse.MoveAsync(40, 40, new MouseMoveOptions { Steps = 6 });
		await page.Mouse.DownAsync();
		await Task.Delay(200);
		await ScrollToAsync(page, 0, 400);
		await page.Mouse.MoveAsync(40, 200, new MouseMoveOptions { Steps = 6 });	// Ensure select end RowNumber is greater than 27
		await page.Mouse.UpAsync();
		await Task.Delay(300);

		var girdRows = grid.DataGridRows;
		var selectedRowsIndexes = girdRows.Where(row => row.Selected).Select(row => row.RowNumber).Order().ToArray();
		var expected = new int[] { 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36 };
		Assert.That(selectedRowsIndexes, Is.EqualTo(expected), string.Format("Selected row numbers expected: [{0}], actual: [{1}]", string.Join(",", expected), string.Join(",", selectedRowsIndexes)));
	}

	[Test, WithPlaywrightPage]
	public async Task DoubleClickOnScrollbarShouldBeDisabledAsync()
	{
		await using var ctx = new InMemoryTestServerContext();
		DataGrid dataGrid = null;
		var result = string.Empty;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { ClientSize = new Size(200, 200) };
			var table = new DataTable("data");
			var column1 = new DataColumn("one");
			column1.DataType = typeof(string);
			table.Columns.Add(column1);
			var column2 = new DataColumn("two");
			column2.DataType = typeof(string);
			table.Columns.Add(column2);
			table.Rows.Add("1.1", "1.2");

			dataGrid = new DataGrid();
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnStyle1 = new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One Header" };
			columnStyle1.Width = 150;
			var columnStyle2 = new DataGridTextBoxColumn { MappingName = "two", HeaderText = "Two Header" };
			columnStyle2.Width = 150;
			tableStyle.GridColumnStyles.Add(columnStyle1);
			tableStyle.GridColumnStyles.Add(columnStyle2);
			dataGrid.TableStyles.Add(tableStyle);
			dataGrid.MouseDown += (s, e) =>
			{
				if (e.Clicks == 2)
				{
					result = "Double click triggered";
				}
			};
			form.Controls.Add(dataGrid);
			return form;
		});

		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
		var grid = await page.WaitForSelectorAsync(".datagrid");
		Assert.That(async () => await grid.EvaluateAsync<bool>("e => e.scrollWidth > e.clientWidth"), Is.True);

		result = string.Empty;
		await page.Mouse.ClickAsync(100, 100, new MouseClickOptions { ClickCount = 2 });
		await Task.Delay(100);
		Assert.That(result, Is.EqualTo("Double click triggered"));

		result = string.Empty;
		await page.Mouse.ClickAsync(100, 190, new MouseClickOptions { ClickCount = 2 });
		await Task.Delay(100);
		Assert.That(result, Is.EqualTo(string.Empty));
	}

	[Test, WithPlaywrightPage]
	[TestCase(195, 0, 12)]
	[TestCase(165, 30, 12)]
	[TestCase(383, 372, 12)]
	[TestCase(380, 455, 12)]
	public async Task DataGridCorrectGuideLinePositionWthenChangingColumnWidthAfterScrollAsync(int expectedXCoordinate, int scrollX, int sampleYCoordinate)
	{
		await using var ctx = new InMemoryTestServerContext();
		DataGrid grid = null;
		int guideWidth = 3;
		var page = await ctx.LoadFormAsync(() =>
		{
			var table = new DataTable("data");
			for (var i = 0; i < 16; i++)
			{
				table.Columns.Add(new DataColumn(i.ToString()) { DataType = typeof(string) });
			}
			var tableStyle = new DataGridTableStyle { MappingName = "data" };

			for (var i = 0; i < 16; i++)
			{
				tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn { MappingName = i.ToString(), HeaderText = i.ToString(), Width = 80 });
			}
			grid = new DataGridWithWholeRowSelect { Dock = DockStyle.Fill, DataSource = table, ReadOnly = true };
			grid.TableStyles.Add(tableStyle);

			var form = new Form { Width = 400, Height = 300 };
			form.Controls.Add(grid);
			return form;
		});

		// Move cursor to position and MouseDown to get guide line position
		await ScrollToAsync(page, scrollX, 0);
		await page.Mouse.MoveAsync(expectedXCoordinate, sampleYCoordinate);
		await MouseDownAsync(page);
		var guideLine = await page.WaitForSelectorAsync(".splitter__guide");
		var boundingBox = await guideLine.BoundingBoxAsync();
		int actualXCoordinate = (int)Math.Round(boundingBox.X) + guideWidth;
		await MouseUpAsync(page);

		// Assert the x-coordinate of the guide line
		Assert.That(actualXCoordinate , Is.EqualTo(expectedXCoordinate),
			"The x coordinate of the guideLine does not match the expected position after scrolling.");
	}

	[Test]
	public async Task DisposeOldDataGridRowsWhileSettingDataGridRows()
	{
		using var ctx = new WinzorTestContext();
		var (component, grid) = await TestGridAsync(ctx);
		var oldRows = grid.DataGridRows;

		Assert.That(oldRows.All(r => !r.IsDisposed && !r.notificationIcon.IsDisposed), Is.True);

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var newRows = new DataGridRow[2];
			newRows[0] = new DataGridRelationshipRow(grid, grid.myGridTable, 0);
			newRows[1] = new DataGridAddNewRow(grid, grid.myGridTable, 1);
			grid.SetDataGridRows(newRows, newRows.Length);
		});

		Assert.That(oldRows.All(r => r.IsDisposed && r.notificationIcon.IsDisposed), Is.True);
	}

	[Test]
	public async Task DisposeDataGridRowsWhileDisposingDataGrid()
	{
		using var ctx = new WinzorTestContext();
		var (component, grid) = await TestGridAsync(ctx);
		var oldRows = grid.DataGridRows;

		Assert.That(oldRows.All(r => !r.IsDisposed && !r.notificationIcon.IsDisposed), Is.True);

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			grid.Dispose();
		});

		Assert.That(oldRows.All(r => r.IsDisposed && r.notificationIcon.IsDisposed), Is.True);
	}

	[Test]
	public async Task DoesNotDisposeSameDataGridRowsArrayWhileSettingDataGridRows()
	{
		using var ctx = new WinzorTestContext();
		var (component, grid) = await TestGridAsync(ctx);
		var rows = grid.DataGridRows;

		Assert.That(rows.All(r => !r.IsDisposed && !r.notificationIcon.IsDisposed), Is.True);

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			grid.SetDataGridRows(rows, rows.Length);
		});

		Assert.That(rows.All(r => !r.IsDisposed && !r.notificationIcon.IsDisposed), Is.True);
	}

	[Test]
	public async Task DisposeDataGridRowsShouldNotDisposeRowsInUsing()
	{
		using var ctx = new WinzorTestContext();
		var (component, grid) = await TestGridAsync(ctx);
		var oldRows = grid.DataGridRows;

		Assert.That(oldRows.All(r => !r.IsDisposed && !r.notificationIcon.IsDisposed), Is.True);

		DataGridRow oldRow = null;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var newRows = new DataGridRow[2];
			oldRow = grid.DataGridRows[0];
			newRows[0] = oldRow;
			newRows[1] = new DataGridAddNewRow(grid, grid.myGridTable, 1);
			grid.SetDataGridRows(newRows, newRows.Length);
		});

		Assert.That(oldRow.IsDisposed, Is.False);
		Assert.That(oldRow.notificationIcon.IsDisposed, Is.False);
		Assert.That(oldRow.notificationIcon.NotificationAnchorControl, Is.Not.Null);
	}

	[Test]
	public async Task DisposeNotificationIconWhileDisposingDataGrid()
	{
		using var ctx = new WinzorTestContext();
		DataGridForTest dataGrid = null;
		await ctx.RenderControlOnFormAsync(() =>
		{
			dataGrid = new DataGridForTest();
			InitializeTestGrid(dataGrid);
			return dataGrid;
		});

		Assert.That(dataGrid.NotificationIcon.IsDisposed, Is.False);

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			dataGrid.Dispose();
		});

		Assert.That(dataGrid.NotificationIcon.IsDisposed, Is.True);
	}

	[Test]
	public async Task TestMouseDownWhenInvokeWinzorDispatcherAsyncFormDisposed()
	{
		using var ctx = new WinzorTestContext();
		Form form = null;
		DataGridWithWholeRowSelect dataGrid = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			var table = new DataTable("data");
			var column1 = new DataColumn("one");
			column1.DataType = typeof(string);
			table.Columns.Add(column1);
			var column2 = new DataColumn("two");
			column2.DataType = typeof(string);
			table.Columns.Add(column2);
			table.Rows.Add("1.1", "1.2");
			table.Rows.Add("2.1", "2.2");

			dataGrid = new DataGridWithWholeRowSelect();
			dataGrid.ReadOnly = true;
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;

			dataGrid.MouseDown += (_, e) =>
			{
				form.Dispose();
			};

			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnStyle1 = new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One Header" };
			tableStyle.GridColumnStyles.Add(columnStyle1);
			var columnStyle2 = new DataGridTextBoxColumn { MappingName = "two", HeaderText = "Two Header" };
			tableStyle.GridColumnStyles.Add(columnStyle2);
			dataGrid.TableStyles.Add(tableStyle);
			form.Controls.Add(dataGrid);

			return form;
		});

		var grid = rendered.Find(".datagrid");
		var th = rendered.FindAll("thead>tr>th");

		Assert.That(grid, Is.Not.Null);

		Assert.That(th, Has.Count.EqualTo(3));
		Assert.That(th[0].ToMarkup(), Does.Contain("datagrid__columns_dropdown"));
		Assert.That(th[1].ToMarkup(), Does.Contain("One Header"));
		Assert.That(th[2].ToMarkup(), Does.Contain("Two Header"));

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			rendered.FindAll("tbody>tr")[0].Children[0].MouseDown(new WebMouseEventArgs() { Detail = 0 });
		});
	}

	[Test, WithPlaywrightPage]
	public async Task ScrollShouldNotCreateNewRow()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var entered = false;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form() { ClientSize = new Size(180, 180) };
			var table = new DataTable("data");
			var column1 = new DataColumn("one");
			column1.DataType = typeof(string);
			table.Columns.Add(column1);
			var column2 = new DataColumn("two");
			column2.DataType = typeof(string);
			table.Columns.Add(column2);

			var dataGrid = new DataGrid();
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;
			dataGrid.Enter += (sender, args) =>
			{
				entered = true;
			};
			dataGrid.Leave += (sender, args) =>
			{
				entered = false;
			};
			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnStyle1 = new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One Header" };
			columnStyle1.Width = 80;
			var columnStyle2 = new DataGridTextBoxColumn { MappingName = "two", HeaderText = "Two Header" };
			columnStyle2.Width = 80;
			tableStyle.GridColumnStyles.Add(columnStyle1);
			tableStyle.GridColumnStyles.Add(columnStyle2);
			dataGrid.TableStyles.Add(tableStyle);
			form.Controls.Add(dataGrid);
			return form;
		});
		var grid = await page.WaitForSelectorAsync(".datagrid");
		Assert.That(grid, Is.Not.Null);
		Assert.That(entered, Is.True);
		Assert.That(async () => await grid.EvaluateAsync<bool>("e => e.scrollWidth > e.clientWidth"), Is.True);

		await page.Mouse.ClickAsync(200, 200);
		Assert.That(() => entered, Is.False.After(1000, 50));

		var y = await grid.EvaluateAsync<int>("e => e.getBoundingClientRect().top + e.clientHeight");
		await page.Mouse.ClickAsync(50, y + 2);
		await Task.Delay(100);
		Assert.That(entered, Is.False);
	}

	[Test]
	public async Task DataGridRows_ShouldNotBeRecreatedWhenSuspend_ShouldBeRecreatedWhenAccessing()
	{
		using var ctx = new WinzorTestContext();
		DataGridForTest grid = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			grid = CreateNewGrid<DataGridForTest>(1);
			return grid;
		});

		Assert.That(grid, Is.Not.Null);
		var calledCountBefore = grid.CreateDataGridRowsCalledCount;

		await grid.InvokeWinzorDispatcherAsync(() =>
		{
			grid.SuspendLayout();
			grid.RecreateDataGridRows();
		});
		Assert.That(grid.CreateDataGridRowsCalledCount, Is.EqualTo(calledCountBefore));

		await grid.InvokeWinzorDispatcherAsync(() =>
		{
			grid.ResumeLayout();
			_ = grid.DataGridRows;
		});
		Assert.That(grid.CreateDataGridRowsCalledCount, Is.EqualTo(calledCountBefore + 1));
	}
	
	[TestCase(1, 1, DataGrid.HitTestType.None)]
	[TestCase(-1, -1, DataGrid.HitTestType.ColumnHeader)]
	public async Task DataGrid_SetCurrentHitTestInfoWithParamsTest(int row, int col, DataGrid.HitTestType type)
	{
		using var ctx = new WinzorTestContext();
		DataGridForTest grid = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			grid = CreateNewGrid<DataGridForTest>(1);
			return grid;
		});

		var result = new DataGrid.HitTestInfo();
		await grid.InvokeWinzorDispatcherAsync(() =>
		{
			result = grid.SetCurrentHitTestInfo(row, col, type);
		});
		
		Assert.That(result.Row, Is.EqualTo(row));
		Assert.That(result.Column, Is.EqualTo(col));
		Assert.That(result.Type, Is.EqualTo(type));
	}

	[TestCase(99)]
	[TestCase(-99)]
	public async Task DataGrid_SetCurrentHitTestInfoWithoutParamsTest(int row)
	{
		using var ctx = new WinzorTestContext();
		DataGridForTest grid = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			grid = CreateNewGrid<DataGridForTest>(1);
			return grid;
		});

		var result = new DataGrid.HitTestInfo();
		await grid.InvokeWinzorDispatcherAsync(() =>
		{
			result = grid.SetCurrentHitTestInfo(row);
		});

		Assert.That(result.Row, Is.EqualTo(row));
		Assert.That(result.Column, Is.EqualTo(-1));
		Assert.That(result.Type, Is.EqualTo(DataGrid.HitTestType.None));
	}

	[Test]
	public async Task ClearDataGridColumnStylesShouldAssignGridAsActiveControl()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(new TextBox());
			form.Controls.Add(CreateNewGrid<DataGrid>(1));
			return form;
		});
		var form = rendered.GetForm();
		var grid = rendered.GetControl<DataGrid>();

		await grid.InvokeWinzorDispatcherAsync(() => grid.Focus());
		Assert.That(form.ActiveControl, Is.TypeOf<DataGridTextBox>());
		await grid.InvokeWinzorDispatcherAsync(() => grid.TableStyles[0].GridColumnStyles.Clear());
		Assert.That(form.ActiveControl, Is.EqualTo(grid));
	}

	[Test]
	public async Task TestReadOnlyGridCellNotFocusable([Values] bool isWholeRowSelected)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			DataGrid grid = null;
			if (isWholeRowSelected)
			{
				grid = CreateNewGrid<DataGridWithWholeRowSelect>(5);
			}
			else
			{
				grid = CreateNewGrid<DataGrid>(5);
			}
			grid.ReadOnly = true;
			return grid;
		});

		foreach (var cell in rendered.FindAll("tbody>tr>td:not(:first-child)"))
		{
			Assert.That(cell.HasAttribute("tabindex"), Is.EqualTo(!isWholeRowSelected));
		}
	}

	class FormWithBlockingRender : Form
	{
		public ManualResetEvent ContinueRenderEvent { get; } = new ManualResetEvent(false);

		protected override void BuildRenderTree(RenderTreeBuilder builder)
		{
			ContinueRenderEvent.WaitOne();
			base.BuildRenderTree(builder);
		}
	}

	class DataGridWithWholeRowSelect : DataGrid
	{
		public override bool WholeRowSelectedOnClick => true;
	}

	class DataGridWithBockingOnBeforeRender : DataGrid
	{
		public ManualResetEvent ContinueOnBeforeRender { get; } = new ManualResetEvent(false);

		public ManualResetEvent OnBeforeRenderEntered;

		protected internal override void OnBeforeRender()
		{
			OnBeforeRenderEntered?.Set();
			ContinueOnBeforeRender.WaitOne();
			base.OnBeforeRender();
		}
	}

	async Task StartEditAsync(IRenderedComponent<ControlProxyComponent> rendered, IHtmlElement input)
	{
		// Need to simlate keypress for column to begin editing
		// It's have no effect to use mousedown, focusin
		await rendered.KeyPressAsync(Keys.A, input);
	}

	async Task SelectRow(IElementHandle row)
	{
		var firstCell = await row.WaitForSelectorAsync("td:first-child");
		await firstCell.ClickAsync();
		await Task.Delay(500);
	}

	async Task ResizeColumnWidth(IPage page, IElementHandle resizer)
	{
		await MouseMoveOfElementAsync(page, resizer, 0, 0);
		await MouseDownAsync(page);
		await MouseMoveOfElementAsync(page, resizer, 10, 0);
		await MouseUpAsync(page);
	}

	async Task<int> GetDataGridScrollTopAsync(IPage page) => await page.EvaluateAsync<int>("document.getElementsByClassName('datagrid')[0].scrollTop");

	async Task<int> GetDataGridScrollLeftAsync(IPage page) => await page.EvaluateAsync<int>("document.getElementsByClassName('datagrid')[0].scrollLeft");

	async Task AssertDataGridResizeColumnWidth(IPage page, IElementHandle resizer, int moveX, int expected)
	{
		await MouseMoveOfElementAsync(page, resizer, 0, 0);
		await MouseDownAsync(page);
		Assert.That(await resizer.EvaluateAsync<int>("e => window.getComputedStyle(e).getPropertyValue('z-index')"), Is.EqualTo(10));
		await MouseMoveOfElementAsync(page, resizer, moveX, 0);
		await MouseUpAsync(page);
		Assert.That(await page.EvaluateAsync<int>("document.getElementsByTagName('th')[1].offsetWidth"), Is.EqualTo(expected));
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

	T CreateNewGrid<T>() where T : DataGrid, new() => CreateNewGrid<T>(50);

	T CreateNewGrid<T>(int rowCount) where T : DataGrid, new()
	{
		var dataGrid = (T)Activator.CreateInstance(typeof(T));
		dataGrid.Dock = DockStyle.Fill;
		dataGrid.DataSource = CreateNewTableData(rowCount);
		dataGrid.AllowNavigation = false;
		var tableStyle = new DataGridTableStyle { MappingName = "data" };

		var columnStyle1 = new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One Header" };
		columnStyle1.Width = 100;
		var columnStyle2 = new DataGridTextBoxColumn { MappingName = "two", HeaderText = "Two Header" };
		columnStyle2.Width = 100;
		var columnStyle3 = new DataGridTextBoxColumn { MappingName = "three", HeaderText = "Three Header" };
		columnStyle3.Width = 100;
		var columnStyle4 = new DataGridTextBoxColumn { MappingName = "four", HeaderText = "Four Header" };
		columnStyle4.Width = 100;
		tableStyle.GridColumnStyles.Add(columnStyle1);
		tableStyle.GridColumnStyles.Add(columnStyle2);
		tableStyle.GridColumnStyles.Add(columnStyle3);
		tableStyle.GridColumnStyles.Add(columnStyle4);

		dataGrid.TableStyles.Add(tableStyle);

		return dataGrid;
	}

	DataTable CreateNewTableData(int rowCount)
	{
		var table = new DataTable("data");
		table.Columns.Add(new DataColumn("one") { DataType = typeof(string) });
		table.Columns.Add(new DataColumn("two") { DataType = typeof(string) });
		table.Columns.Add(new DataColumn("three") { DataType = typeof(string) });
		table.Columns.Add(new DataColumn("four") { DataType = typeof(string) });

		for (var i = 0; i < rowCount; i++)
		{
			table.Rows.Add($"cell({i + 1}, 1)", $"cell({i + 1}, 2)", $"cell({i + 1}, 3)", $"cell({i + 1}, 4)");
		}
		return table;
	}

	class TestDataGrid : DataGrid
	{
		public int onScrollAsyncCalledCount;

		[JSInvokable("OnScrollAsync")]
		public override Task OnScrollAsync(float scrollTop, float scrollLeft)
		{
			onScrollAsyncCalledCount++;
			return base.OnScrollAsync(scrollTop, scrollLeft);
		}
	}

	void AssertRowVisible(DataGrid dataGrid, int rowNumber, bool expectedVisibility)
	{
		Assert.That(() => rowNumber >= dataGrid.FirstVisibleRow && rowNumber < dataGrid.FirstVisibleRow + dataGrid.VisibleRowCount, Is.EqualTo(expectedVisibility).After(1000, 50));
	}

	async Task ScrollToRowAsync(IPage page, int scrollTop)
	{
		await page.EvaluateAsync($"document.getElementsByClassName('datagrid')[0].scrollTo({{left:0, top:{scrollTop}, behavior: 'smooth'}});");
		await Task.Delay(1000);
	}

	async Task ScrollToAsync(IPage page, int scrollLeft, int scrollTop)
	{
		await page.EvaluateAsync($"document.getElementsByClassName('datagrid')[0].scrollTo({scrollLeft}, {scrollTop});");
		await Task.Delay(500);
	}

	class CustomType
	{
		public string DisplayText { get; set; }
	}

	class DataGridCustomColumnStyle : DataGridTextBoxColumn
	{
		protected internal override MarkupString GetRenderContent(CurrencyManager source, int rowNum, bool alignToRight)
		{
			return (MarkupString)((CustomType)GetColumnValueAtRow(source, rowNum)).DisplayText;
		}

		protected internal override string GetCellStyleString(CurrencyManager source, int rowNum)
		{
			return $"background-color: {Color.Transparent.GetColorStyleValue()};";
		}

		protected internal override string GetEditControlStyleString(CurrencyManager source, int rowNum)
		{
			return $"background-color:{Color.Red.GetColorStyleValue()};";
		}

		protected internal override string GetText(object value)
		{
			return ((CustomType)value).DisplayText;
		}
	}

	class DataGridColumnCustomColor : DataGridTextBoxColumn
	{
		protected internal override MarkupString GetRenderContent(CurrencyManager source, int rowNum, bool alignToRight)
		{
			return (MarkupString)GetColumnValueAtRow(source, rowNum).ToString();
		}

		protected internal override string GetCellStyleString(CurrencyManager source, int rowNum)
		{
			return $"background-color: {BackColor.GetColorStyleValue()};";
		}

		protected internal override string GetEditControlStyleString(CurrencyManager source, int rowNum)
		{
			return "";
		}

		public Color BackColor = Color.Transparent;
	}

	class NotThreadSafeList : IBindingList
	{
		public NotThreadSafeList(WinzorDispatcher dispatcher)
		{
			dispatcherThreadId = dispatcher.ManagedThreadId;
		}

		public object this[int index]
		{
			get
			{
				CheckThread();
				return list[index];
			}
			set
			{
				CheckThread();
				list[index] = (NotThreadSafeObject)value;
				OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, index));
			}
		}

		public bool AllowEdit
		{
			get
			{
				CheckThread();
				return true;
			}
		}

		public bool AllowNew
		{
			get
			{
				CheckThread();
				return true;
			}
		}

		public bool AllowRemove
		{
			get
			{
				CheckThread();
				return true;
			}
		}

		public bool IsSorted
		{
			get
			{
				CheckThread();
				return false;
			}
		}

		public ListSortDirection SortDirection
		{
			get
			{
				CheckThread();
				return sortDirection;
			}
			set
			{
				CheckThread();
				sortDirection = value;
			}
		}
		ListSortDirection sortDirection;

		public PropertyDescriptor SortProperty
		{
			get
			{
				CheckThread();
				return sortProperty;
			}
			set
			{
				CheckThread();
				sortProperty = value;
			}
		}
		PropertyDescriptor sortProperty;

		public bool SupportsChangeNotification
		{
			get
			{
				CheckThread();
				return true;
			}
		}

		public bool SupportsSearching
		{
			get
			{
				CheckThread();
				return true;
			}
		}

		public bool SupportsSorting
		{
			get
			{
				CheckThread();
				return true;
			}
		}

		public bool IsFixedSize
		{
			get
			{
				CheckThread();
				return false;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				CheckThread();
				return false;
			}
		}

		public int Count
		{
			get
			{
				CheckThread();
				return list.Count;
			}
		}

		public bool IsSynchronized
		{
			get
			{
				CheckThread();
				return false;
			}
		}

		public object SyncRoot
		{
			get
			{
				CheckThread();
				throw new NotImplementedException();
			}
		}

		void OnListChanged(ListChangedEventArgs args)
		{
			ListChanged?.Invoke(this, args);
		}

		public event ListChangedEventHandler ListChanged;

		public int Add(object value)
		{
			CheckThread();
			list.Add((NotThreadSafeObject)value);
			var index = list.IndexOf((NotThreadSafeObject)value);
			((NotThreadSafeObject)value).PropertyChanged += OnItemChanged;
			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
			return index;
		}

		public void AddIndex(PropertyDescriptor property)
		{
			CheckThread();
			throw new NotImplementedException();
		}

		public object AddNew()
		{
			var obj = new NotThreadSafeObject(dispatcherThreadId);
			Add(obj);
			return obj;
		}

		public void ApplySort(PropertyDescriptor property, ListSortDirection direction)
		{
			CheckThread();
			sortProperty = property;
			sortDirection = direction;
		}

		public void Clear()
		{
			CheckThread();
			list.Clear();
		}

		public bool Contains(object value)
		{
			CheckThread();
			return list.Contains((NotThreadSafeObject)value);
		}

		public void CopyTo(Array array, int index)
		{
			CheckThread();
			throw new NotImplementedException();
		}

		public int Find(PropertyDescriptor property, object key)
		{
			CheckThread();
			throw new NotImplementedException();
		}

		public IEnumerator GetEnumerator()
		{
			CheckThread();
			return list.GetEnumerator();
		}

		public int IndexOf(object value)
		{
			CheckThread();
			return list.IndexOf((NotThreadSafeObject)value);
		}

		public void Insert(int index, object value)
		{
			CheckThread();
			list.Insert(index, (NotThreadSafeObject)value);
		}

		public void Remove(object value)
		{
			CheckThread();
			list.Remove((NotThreadSafeObject)value);
			((NotThreadSafeObject)value).PropertyChanged -= OnItemChanged;
		}

		public void RemoveAt(int index)
		{
			CheckThread();
			list.RemoveAt(index);
		}

		public void RemoveIndex(PropertyDescriptor property)
		{
			CheckThread();
			throw new NotImplementedException();
		}

		public void RemoveSort()
		{
			CheckThread();
			throw new NotImplementedException();
		}

		void CheckThread()
		{
			if (Environment.CurrentManagedThreadId != dispatcherThreadId)
			{
				throw new InvalidOperationException("The current thread is not the WinzorDispatcher thread");
			}
		}

		void OnItemChanged(object sender, PropertyChangedEventArgs args)
		{
			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, list.IndexOf((NotThreadSafeObject)sender)));
		}

		readonly int dispatcherThreadId;
		readonly List<NotThreadSafeObject> list = new List<NotThreadSafeObject>();
	}

	class NotThreadSafeObject : INotifyPropertyChanged
	{
		public NotThreadSafeObject(WinzorDispatcher dispatcher)
			: this(dispatcher.ManagedThreadId)
		{
		}

		public NotThreadSafeObject(int dispatcherThreadId)
		{
			this.dispatcherThreadId = dispatcherThreadId;
		}

		public string Value
		{
			get
			{
				CheckThread();
				return value;
			}
			set
			{
				CheckThread();
				this.value = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
			}
		}
		string value;

		void CheckThread()
		{
			if (Environment.CurrentManagedThreadId != dispatcherThreadId)
			{
				throw new InvalidOperationException("The current thread is not the WinzorDispatcher thread");
			}
		}

		readonly int dispatcherThreadId;

		public event PropertyChangedEventHandler PropertyChanged;
	}

	class NotThreadSafeDataGrid : DataGrid
	{
		public NotThreadSafeDataGrid(WinzorDispatcher dispatcher)
			: this(dispatcher.ManagedThreadId)
		{
		}

		public NotThreadSafeDataGrid(int dispatcherThreadId)
			: base()
		{
			this.dispatcherThreadId = dispatcherThreadId;
		}

		protected internal override void UpdateRowNotification(int rowIndex, NotificationIcon icon)
		{
			CheckThread();
			base.UpdateRowNotification(rowIndex, icon);
		}

		void CheckThread()
		{
			if (Environment.CurrentManagedThreadId != dispatcherThreadId)
			{
				throw new InvalidOperationException("The current thread is not the WinzorDispatcher thread");
			}
		}

		readonly int dispatcherThreadId;
	}

	class NotThreadSafeDataGridColumnStyle : DataGridTextBoxColumn
	{
		public NotThreadSafeDataGridColumnStyle(WinzorDispatcher dispatcher) : base()
		{
			dispatcherThreadId = dispatcher.ManagedThreadId;
		}

		protected internal override bool IsCurrentCellReadOnly => IsCurrentCellReadOnlyForTest();

		bool IsCurrentCellReadOnlyForTest()
		{
			CheckThread();
			return base.IsCurrentCellReadOnly;
		}

		void CheckThread()
		{
			if (Environment.CurrentManagedThreadId != dispatcherThreadId)
			{
				throw new InvalidOperationException("The current thread is not the WinzorDispatcher thread");
			}
		}

		readonly int dispatcherThreadId;
	}

	class DataGridForTest : DataGrid
	{
		public bool IsGridLayoutConfigurable { get; set; }

		protected override bool GridLayoutConfigurable => IsGridLayoutConfigurable;

		internal new NotificationIcon NotificationIcon => base.NotificationIcon;

		public int CreateDataGridRowsCalledCount { get; private set; }

		internal override void CreateDataGridRows()
		{
			CreateDataGridRowsCalledCount++;
			base.CreateDataGridRows();
		}
	}

	//fixed in WI00696277, where we made the cell change merely on focus in instead of mousedown
	async Task SimulateClickOnCellForRowChange(IElement element)
	{
		await element.TriggerEventAsync("onwinzorfocusin", new WinzorFocusInEventArgs());
	}

	static DataGrid.HitTestInfo HitNone => new() { type = DataGrid.HitTestType.None, col = -1, row = -1 };
}

public class DataGridColumnStyleStub : DataGridTextBoxColumn
{
	public int EditCount;

	protected internal override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly)
	{
		EditCount++;
		base.Edit(source, rowNum, bounds, readOnly);
	}

	protected internal override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string displayText)
	{
		EditCount++;
		base.Edit(source, rowNum, bounds, readOnly, displayText);
	}

	protected internal override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string displayText, bool cellIsVisible)
	{
		EditCount++;
		base.Edit(source, rowNum, bounds, readOnly, displayText, cellIsVisible);
	}

	protected internal override string GetCellStyleString(CurrencyManager source, int rowNum)
	{
		return source.Current.ToString();
	}

	public int GetRenderContentCount;

	protected internal override MarkupString GetRenderContent(CurrencyManager source, int rowNum, bool alignToRight)
	{
		GetRenderContentCount++;
		return base.GetRenderContent(source, rowNum, alignToRight);
	}
}
