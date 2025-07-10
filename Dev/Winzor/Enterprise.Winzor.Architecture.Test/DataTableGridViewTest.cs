using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using CargoWise.Blazor.Client.Integration.Menus;
using Enterprise.ZArchitecture.GUI;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

class DataTableGridViewTest
{
	[Test, WithPlaywrightPage]
	public async Task DataTableGridViewShouldShowContextMenuOnRightClick()
	{
		var showCalled = false;
		var clientServiceProvider = new MockCargoWiseClientSeviceProvider();
		clientServiceProvider.MockMenuDisplayer.Setup(m => m.SendShowMenuRequestAsync(It.IsAny<MenuInteropModel>(), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>())).Callback(() => showCalled = true);
		await using var ctx = new InMemoryAppServerTestContext(clientServiceProvider);
		var page = await ctx.LoadControlOnFormAsync(() => CreateDataTableGridView());
		var editColumn = page.Locator(".datagrid table tbody tr:nth-child(1) td:nth-child(3)");
		await editColumn.ClickAsync(new LocatorClickOptions { Button = MouseButton.Right });
		Assert.That(() => showCalled, Is.EqualTo(true).After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task DataTableGridViewShouldConvertBooleanColumnToCheckBox()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => CreateDataTableGridView());
		var editColumn = page.Locator(".datagrid table tbody tr:nth-child(1) td:nth-child(4) input");
		Assert.That(async () => await editColumn.GetAttributeAsync("type"), Is.EqualTo("checkbox"));
	}

	[Test, WithPlaywrightPage]
	public async Task DataTableGridViewShouldChangeDateTimeColumnToCorrectFormat()
	{
		DataTableGridView dataTableGridView = null;
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			dataTableGridView = CreateDataTableGridView();
			return dataTableGridView;
		});
		Assert.That(() => dataTableGridView, Is.Not.Null);
		await dataTableGridView.InvokeWinzorDispatcherAsync(() =>
		{
			dataTableGridView.SetDateColumnsToSecondsFormat();
		});
		var editColumn = page.Locator(".datagrid table tbody tr:nth-child(2) td:nth-child(5)");
		await page.ClickAsync(".datagrid table tbody tr:nth-child(2) td:nth-child(1)");
		Assert.That(async () => await editColumn.InnerTextAsync(), Is.EqualTo("2024-09-28 13:11:20").After(2000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task DataTableGridViewContextMenuSelectAllShouldSelectAllRows()
	{
		DataTableGridView dataTableGridView = null;
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			dataTableGridView = CreateDataTableGridView();
			return dataTableGridView;
		});
		Assert.That(() => dataTableGridView, Is.Not.Null);

		await dataTableGridView.InvokeWinzorDispatcherAsync(() =>
		{
			dataTableGridView.ContextMenu.Show(dataTableGridView, new Point(0, 0));
			var menu = dataTableGridView.ContextMenu.MenuItems[0];
			menu.PerformClick();
		});

		Assert.That(() => dataTableGridView.SelectedRowCount, Is.EqualTo(dataTableGridView.DataGridRowsLength).After(2000, 100));
	}

	DataTableGridView CreateDataTableGridView()
	{
		var dataTableGridView = new DataTableGridView
		{
			Location = new Point(0, 0),
			Size = new Size(600, 600),
			Name = "DataTableDataGridView",
			ReadOnly = true,
		};
		using var table = new DataTable("data");
		var column1 = new DataColumn("one");
		column1.DataType = typeof(string);
		table.Columns.Add(column1);
		var column2 = new DataColumn("two");
		column2.DataType = typeof(string);
		table.Columns.Add(column2);
		var column3 = new DataColumn("three");
		column3.DataType = typeof(bool);
		table.Columns.Add(column3);
		var column4 = new DataColumn("four");
		column4.DataType = typeof(DateTime);
		table.Columns.Add(column4);
		table.Rows.Add("1.1", "1.2", true, new DateTime(2024, 9, 28, 12, 40, 55));
		table.Rows.Add("2.1", "2.2", true, new DateTime(2024, 9, 28, 13, 11, 20));
		dataTableGridView.InstallDataTableSafe(table, true);
		return dataTableGridView;
	}
}
