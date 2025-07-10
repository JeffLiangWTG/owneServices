using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

using static PlaywrightTestContext;

class ZDataSetListBoxWithGridTest
{
	[Test]
	public async Task GridIsNotEmpty()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var table = new DataTable();
			table.Columns.Add("Column1", typeof(string));
			var row1 = table.NewRow();
			row1["Column1"] = "Cell(1, 1)";
			table.Rows.Add(row1);
			var control = new ZDataSetListBoxWithGrid();
			control.DataSource = table;
			return control;
		});
		var headers = rendered.FindAll(".datagrid thead th");
		var rows = rendered.FindAll(".datagrid tbody tr");
		Assert.That(headers.Count, Is.EqualTo(3));
		Assert.That(rows.Count, Is.EqualTo(2));
		Assert.That(headers[1].TextContent, Is.EqualTo("Column1"));
		Assert.That(rows[0].QuerySelector("input").GetAttribute("value"), Is.EqualTo("Cell(1, 1)"));
	}

	[Test]
	public async Task TestDisposeWithLargeDataSet()
	{
		Form form = null;
		try
		{
			ZDataSetListBoxWithGrid listGrid = null;
			using var ctx = new EnterpriseTestContext();
			await ctx.RenderFormAsync(() =>
			{
				var table = new DataTable();
				table.Columns.Add("Col1");
				for (int i = 0; i < 10000; i++)
				{
					var row = table.NewRow();
					row["Col1"] = $"Testval{i}";
					table.Rows.Add(row);
				}

				listGrid = new ZDataSetListBoxWithGrid() { Dock = DockStyle.Fill, DataSource = table };
				form = new Form();
				form.Controls.Add(listGrid);
				return form;
			});

			Assert.That(form, Is.Not.Null);
			Assert.That(listGrid, Is.Not.Null);

			await listGrid.InvokeWinzorDispatcherAsync(() => listGrid.GridViewForTest.BeginEdit(listGrid.GridViewForTest.GetCurrentColumnStyle(), listGrid.GridViewForTest.CurrentRowIndex));
		}
		finally
		{
			Assert.DoesNotThrow(() => form.Dispose());
		}
	}

	[Test, WithPlaywrightPage]
	public async Task TestCopyDateValues()
	{
		ZDataSetListBoxWithGrid listGrid = null;
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var table = new DataTable("TestTable");
			table.Columns.Add("ZDateTimeCol", typeof(ZDateTime));
			table.Columns.Add("DateTimeCol", typeof(DateTime));
			var row1 = table.NewRow();
			row1["ZDateTimeCol"] = new ZDateTime(2015, 12, 1, 15, 5, 10);
			row1["DateTimeCol"] = new DateTime(2015, 12, 1, 15, 5, 10);
			table.Rows.Add(row1);
			table.AcceptChanges();
			listGrid = new ZDataSetListBoxWithGrid();
			listGrid.DataSource = table;
			var form = new Form { Size = new Size(1000, 1000) };
			form.Controls.Add(listGrid);
			return form;
		});

		await BrowserContext.GrantPermissionsAsync(new[] { "clipboard-write", "clipboard-read" });
		Assert.That(listGrid, Is.Not.Null);

		await listGrid.InvokeWinzorDispatcherAsync(() =>
		{
			listGrid.GridViewForTest.ContextMenu.Show(listGrid, new Point(0, 0));
			var menu = listGrid.GridViewForTest.ContextMenu.MenuItems[0];
			menu.PerformClick();
		});

		await listGrid.InvokeWinzorDispatcherAsync(() =>
		{
			listGrid.GridViewForTest.ContextMenu.Show(listGrid, new Point(0, 0));
			var menu = listGrid.GridViewForTest.ContextMenu.MenuItems[4];
			menu.PerformClick();
		});
		var expected = @"""ZDateTimeCol"", ""DateTimeCol""
""2015-12-01 15:05:10"", ""2015-12-01 15:05:10""
"""", """"
";
		Assert.That(async () => await page.EvaluateAsync<string>("async () => await navigator.clipboard.readText()"), Is.EqualTo(expected).After(2000, 100), "Expected date columns to be formatted correctly, null values cause no problems");

		await listGrid.InvokeWinzorDispatcherAsync(() =>
		{
			listGrid.GridViewForTest.ContextMenu.Show(listGrid, new Point(0, 0));
			var menu = listGrid.GridViewForTest.ContextMenu.MenuItems[5];
			menu.PerformClick();
		});
		expected = @"INSERT INTO TestTable
(ZDateTimeCol, DateTimeCol)
VALUES
('2015-12-01 15:05:10', '2015-12-01 15:05:10'),
(Null, Null)";
		Assert.That(async () => await page.EvaluateAsync<string>("async () => await navigator.clipboard.readText()"), Is.EqualTo(expected).After(2000, 100), "SQL should work ask well");
	}

	[Test, WithPlaywrightPage]
	public async Task TestSqlBoolValues()
	{
		ZDataSetListBoxWithGrid listGrid = null;
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var table = new DataTable("TestTable");
			table.Columns.Add("Column1", typeof(bool));
			table.Columns.Add("Column2", typeof(bool));
			var row1 = table.NewRow();
			row1["Column1"] = "True";
			row1["Column2"] = "False";
			table.Rows.Add(row1);
			table.AcceptChanges();
			listGrid = new ZDataSetListBoxWithGrid();
			listGrid.DataSource = table;
			var form = new Form { Size = new Size(1000, 1000) };
			form.Controls.Add(listGrid);
			return form;
		});

		await BrowserContext.GrantPermissionsAsync(new[] { "clipboard-write", "clipboard-read" });
		Assert.That(listGrid, Is.Not.Null);

		await listGrid.InvokeWinzorDispatcherAsync(() =>
		{
			listGrid.GridViewForTest.ContextMenu.Show(listGrid, new Point(0, 0));
			var menu = listGrid.GridViewForTest.ContextMenu.MenuItems[0];
			menu.PerformClick();
		});

		await listGrid.InvokeWinzorDispatcherAsync(() =>
		{
			listGrid.GridViewForTest.ContextMenu.Show(listGrid, new Point(0, 0));
			var menu = listGrid.GridViewForTest.ContextMenu.MenuItems[5];
			menu.PerformClick();
		});
		var expected = @"INSERT INTO TestTable
(Column1, Column2)
VALUES
('1', '0'),
(Null, Null)";
		Assert.That(async () => await page.EvaluateAsync<string>("async () => await navigator.clipboard.readText()"), Is.EqualTo(expected).After(2000, 100), "For boolean columns stored as different datacolumns in the db, we expect the SQL output to be different");
	}

	[Test, WithPlaywrightPage]
	public async Task TestCsvSelection()
	{
		ZDataSetListBoxWithGrid listGrid = null;
		var clientServiceProvider = new MockCargoWiseClientSeviceProvider();
		await using var ctx = new InMemoryAppServerTestContext(clientServiceProvider);
		var page = await ctx.LoadFormAsync(() =>
		{
			var table = new DataTable("TestTable");
			table.Columns.Add("Column1", typeof(ZString));
			table.Columns.Add("Column2", typeof(ZString));
			var row1 = table.NewRow();
			var row2 = table.NewRow();
			row1["Column1"] = new ZString("SelectedRow");
			row1["Column2"] = new ZString("SelectedRow");
			row2["Column1"] = new ZString("NotSelectedRow");
			row2["Column2"] = new ZString("NotSelectedRow");
			table.Rows.Add(row1);
			table.Rows.Add(row2);
			table.AcceptChanges();
			listGrid = new ZDataSetListBoxWithGrid();
			listGrid.DataSource = table;
			listGrid.GridViewForTest.Select(0);
			listGrid.GridViewForTest.ContextMenu.Show(listGrid.GridViewForTest, new Point(0, 0), true);
			var form = new Form { Size = new Size(1000, 1000) };
			form.Controls.Add(listGrid);
			return form;
		});

		await BrowserContext.GrantPermissionsAsync(new[] { "clipboard-write", "clipboard-read" });
		Assert.That(listGrid, Is.Not.Null);

		await listGrid.InvokeWinzorDispatcherAsync(() =>
		{
			listGrid.GridViewForTest.ContextMenu.Show(listGrid, new Point(0, 0));
			var menu = listGrid.GridViewForTest.ContextMenu.MenuItems[4];
			menu.PerformClick();
		});
		var expected = @"""Column1"", ""Column2""
""SelectedRow"", ""SelectedRow""
";
		Assert.That(async () => await page.EvaluateAsync<string>("async () => await navigator.clipboard.readText()"), Is.EqualTo(expected).After(2000, 100), "Only expected to include columns selected");
	}

	[Test, WithPlaywrightPage]
	public async Task TestCopyingValuesDoesNotIncludeRowStateColumnNameOrValues()
	{
		ZDataSetListBoxWithGrid listGrid = null;
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var table = new DataTable("TestTable");
			table.Columns.Add("Column1", typeof(ZString));
			var row1 = table.NewRow();
			row1["Column1"] = new ZString("ABCD");
			table.Rows.Add(row1);
			table.AcceptChanges();
			listGrid = new ZDataSetListBoxWithGrid();
			listGrid.DataSource = table;
			var form = new Form { Size = new Size(1000, 1000) };
			form.Controls.Add(listGrid);
			return form;
		});

		await BrowserContext.GrantPermissionsAsync(new[] { "clipboard-write", "clipboard-read" });
		Assert.That(listGrid, Is.Not.Null);

		await listGrid.InvokeWinzorDispatcherAsync(() =>
		{
			listGrid.GridViewForTest.ContextMenu.Show(listGrid, new Point(0, 0));
			var menu = listGrid.GridViewForTest.ContextMenu.MenuItems[0];
			menu.PerformClick();
		});

		Assert.That(listGrid.GridViewForTest.TableStyles[0].GridColumnStyles.ToList<DataGridColumnStyle>().Select((c) => c.HeaderText), Does.Contain("Row State"), "Row state column should be added by default for this data type");
		await listGrid.InvokeWinzorDispatcherAsync(() =>
		{
			listGrid.GridViewForTest.ContextMenu.Show(listGrid, new Point(0, 0));
			var menu = listGrid.GridViewForTest.ContextMenu.MenuItems[4];
			menu.PerformClick();
		});
		var expected = @"""Column1""
""ABCD""
""""
";
		Assert.That(async () => await page.EvaluateAsync<string>("async () => await navigator.clipboard.readText()"), Is.EqualTo(expected).After(2000, 100), "Should not contain any row state info");

		await listGrid.InvokeWinzorDispatcherAsync(() =>
		{
			listGrid.GridViewForTest.ContextMenu.Show(listGrid, new Point(0, 0));
			var menu = listGrid.GridViewForTest.ContextMenu.MenuItems[5];
			menu.PerformClick();
		});
		expected = @"INSERT INTO TestTable
(Column1)
VALUES
('ABCD'),
(Null)";
		Assert.That(async () => await page.EvaluateAsync<string>("async () => await navigator.clipboard.readText()"), Is.EqualTo(expected).After(2000, 100), "Should not contain any row state info");
	}

	[Test, WithPlaywrightPage]
	public async Task TestCopyWithoutHeader()
	{
		ZDataSetListBoxWithGrid listGrid = null;
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var table = new DataTable("TestTable");
			table.Columns.Add("Column1", typeof(bool));
			table.Columns.Add("Column2", typeof(ZString));
			var row1 = table.NewRow();
			row1["Column1"] = "True";
			row1["Column2"] = new ZString("Test");
			table.Rows.Add(row1);
			table.AcceptChanges();
			listGrid = new ZDataSetListBoxWithGrid();
			listGrid.DataSource = table;
			var form = new Form { Size = new Size(1000, 1000) };
			form.Controls.Add(listGrid);
			return form;
		});

		await BrowserContext.GrantPermissionsAsync(new[] { "clipboard-write", "clipboard-read" });
		Assert.That(listGrid, Is.Not.Null);

		await listGrid.InvokeWinzorDispatcherAsync(() =>
		{
			listGrid.GridViewForTest.ContextMenu.Show(listGrid, new Point(0, 0));
			var menu = listGrid.GridViewForTest.ContextMenu.MenuItems[0];
			menu.PerformClick();
		});

		await listGrid.InvokeWinzorDispatcherAsync(() =>
		{
			listGrid.GridViewForTest.ContextMenu.Show(listGrid, new Point(0, 0));
			var menu = listGrid.GridViewForTest.ContextMenu.MenuItems[2];
			menu.PerformClick();
		});
		var expected = @"True	Test
	
";
		Assert.That(async () => await page.EvaluateAsync<string>("async () => await navigator.clipboard.readText()"), Is.EqualTo(expected).After(2000, 100), "Copy without Header should copy only the rows");
	}

	[Test, WithPlaywrightPage]
	public async Task TestCopyWithHeader()
	{
		ZDataSetListBoxWithGrid listGrid = null;
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var table = new DataTable("TestTable");
			table.Columns.Add("Column1", typeof(bool));
			table.Columns.Add("Column2", typeof(ZString));
			var row1 = table.NewRow();
			row1["Column1"] = "True";
			row1["Column2"] = new ZString("Test");
			table.Rows.Add(row1);
			table.AcceptChanges();
			listGrid = new ZDataSetListBoxWithGrid();
			listGrid.DataSource = table;
			var form = new Form { Size = new Size(1000, 1000) };
			form.Controls.Add(listGrid);
			return form;
		});

		await BrowserContext.GrantPermissionsAsync(new[] { "clipboard-write", "clipboard-read" });
		Assert.That(listGrid, Is.Not.Null);

		await listGrid.InvokeWinzorDispatcherAsync(() =>
		{
			listGrid.GridViewForTest.ContextMenu.Show(listGrid, new Point(0, 0));
			var menu = listGrid.GridViewForTest.ContextMenu.MenuItems[0];
			menu.PerformClick();
		});

		await listGrid.InvokeWinzorDispatcherAsync(() =>
		{
			listGrid.GridViewForTest.ContextMenu.Show(listGrid, new Point(0, 0));
			var menu = listGrid.GridViewForTest.ContextMenu.MenuItems[3];
			menu.PerformClick();
		});
		var expected = @"Column1	Column2
True	Test
	
";
		Assert.That(async () => await page.EvaluateAsync<string>("async () => await navigator.clipboard.readText()"), Is.EqualTo(expected).After(2000, 100), "Copy with Header should copy both rows and column");
	}
}
