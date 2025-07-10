using System.Threading.Tasks;
using Bunit;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Testing;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

class ZGridCustomiseTest
{
	[Test]
	public async Task TestDragOneItemBetweenListBoxes()
	{
		// Arrange
		ZGridCustomiseTester gridCustomise = null;
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			using var grid = new ZGrid();
			using var columns = new ZGridColumns(grid);
			columns.AddBoolColumn("BoolColumn", 100, false, false);
			columns.AddDateColumn("Date1Column", 100, false, false);
			columns.AddTextColumn("TextColumn", 100, false, false);
			columns.AddDateColumn("Date2Column", 100, true, false, ZDateTimePickerFormat.Time);
			columns.AddDateColumn("Date3Column", 100, true, false, ZDateTimePickerFormat.Time);
			columns.AddTextColumn("TextColumn2", 100, true, false);
			return gridCustomise = new ZGridCustomiseTester(columns, columns, grid);
		});

			// Act
			await ctx.WinzorDispatcher.InvokeAsync(() => gridCustomise.AvailableColumnsListBoxExposed.Focus());
			await rendered.FindAll(".listbox")[1].QuerySelector(".listbox__item:nth-child(2)").TriggerEventAsync("onmousedown", new WebMouseEventArgs());
			await rendered.FindAll(".listbox")[0].QuerySelector(".listbox__item:nth-child(1)").TriggerEventAsync("ondragstart", new WebDragEventArgs());
			await rendered.FindAll(".listbox")[0].QuerySelector(".listbox__item:nth-child(1)").TriggerEventAsync("ondragenter", new WebDragEventArgs());
			await rendered.FindAll(".listbox")[0].QuerySelector(".listbox__item:nth-child(1)").TriggerEventAsync("ondrop", new WebDragEventArgs());

		// Assert
		var availableColumnsListBoxTrs = rendered.FindAll(".listbox")[1].QuerySelectorAll(".listbox__item");
		Assert.That(availableColumnsListBoxTrs.Length, Is.EqualTo(3));
		Assert.That(availableColumnsListBoxTrs[0].TextContent, Is.EqualTo("BoolColumn"));
		Assert.That(availableColumnsListBoxTrs[1].TextContent, Is.EqualTo("TextColumn"));
		Assert.That(availableColumnsListBoxTrs[2].TextContent, Is.EqualTo(string.Empty));

		var currentColumnsListBoxTrs = rendered.FindAll(".listbox")[0].QuerySelectorAll(".listbox__item");
		Assert.That(currentColumnsListBoxTrs.Length, Is.EqualTo(5));
		Assert.That(currentColumnsListBoxTrs[0].ClassList, Does.Contain("listbox__item--selected"));
		Assert.That(currentColumnsListBoxTrs[0].TextContent, Is.EqualTo("Date1Column"));
		Assert.That(currentColumnsListBoxTrs[1].TextContent, Is.EqualTo("Date2Column"));
		Assert.That(currentColumnsListBoxTrs[2].TextContent, Is.EqualTo("Date3Column"));
		Assert.That(currentColumnsListBoxTrs[3].TextContent, Is.EqualTo("TextColumn2"));
		Assert.That(currentColumnsListBoxTrs[4].TextContent, Is.EqualTo(string.Empty));
	}

		[Test]
		public async Task TestDragMultipleItemsBetweenListBoxes()
		{
			// Arrange
			ZGridCustomiseTester gridCustomise = null;
			using var ctx = new EnterpriseTestContext();
			var rendered = await ctx.RenderFormAsync(() =>
			{
				using var grid = new ZGrid();
				using var columns = new ZGridColumns(grid);
				columns.AddBoolColumn("BoolColumn", 100, false, false);
				columns.AddDateColumn("Date1Column", 100, false, false);
				columns.AddTextColumn("TextColumn", 100, false, false);
				columns.AddDateColumn("Date2Column", 100, true, false, ZDateTimePickerFormat.Time);
				columns.AddDateColumn("Date3Column", 100, true, false, ZDateTimePickerFormat.Time);
				columns.AddTextColumn("TextColumn2", 100, true, false);
				return gridCustomise = new ZGridCustomiseTester(columns, columns, grid);
			});

			// Act
			await ctx.WinzorDispatcher.InvokeAsync(() => gridCustomise.AvailableColumnsListBoxExposed.Focus());
			await rendered.FindAll(".listbox")[1].QuerySelector(".listbox__item:nth-child(1)").TriggerEventAsync("onmousedown", new WebMouseEventArgs() { CtrlKey = true });
			await rendered.FindAll(".listbox")[1].QuerySelector(".listbox__item:nth-child(2)").TriggerEventAsync("onmousedown", new WebMouseEventArgs() { CtrlKey = true });
			await rendered.FindAll(".listbox")[1].QuerySelector(".listbox__item:nth-child(2)").TriggerEventAsync("ondragstart", new WebDragEventArgs() { CtrlKey = true });
			await rendered.FindAll(".listbox")[0].QuerySelector(".listbox__item:nth-child(2)").TriggerEventAsync("ondragenter", new WebDragEventArgs() { CtrlKey = true });
			await rendered.FindAll(".listbox")[0].QuerySelector(".listbox__item:nth-child(2)").TriggerEventAsync("ondrop", new WebDragEventArgs() { CtrlKey = true });

		// Assert
		var availableColumnsListBoxTrs = rendered.FindAll(".listbox")[1].QuerySelectorAll(".listbox__item");
		Assert.That(availableColumnsListBoxTrs.Length, Is.EqualTo(2));
		Assert.That(availableColumnsListBoxTrs[0].TextContent, Is.EqualTo("TextColumn"));
		Assert.That(availableColumnsListBoxTrs[1].TextContent, Is.EqualTo(string.Empty));

		var currentColumnsListBoxTrs = rendered.FindAll(".listbox")[0].QuerySelectorAll(".listbox__item");
		Assert.That(currentColumnsListBoxTrs.Length, Is.EqualTo(6));
		Assert.That(currentColumnsListBoxTrs[0].TextContent, Is.EqualTo("Date2Column"));
		Assert.That(currentColumnsListBoxTrs[1].ClassList, Does.Contain("listbox__item--selected"));
		Assert.That(currentColumnsListBoxTrs[1].TextContent, Is.EqualTo("BoolColumn"));
		Assert.That(currentColumnsListBoxTrs[2].ClassList, Does.Contain("listbox__item--selected"));
		Assert.That(currentColumnsListBoxTrs[2].TextContent, Is.EqualTo("Date1Column"));
		Assert.That(currentColumnsListBoxTrs[3].TextContent, Is.EqualTo("Date3Column"));
		Assert.That(currentColumnsListBoxTrs[4].TextContent, Is.EqualTo("TextColumn2"));
		Assert.That(currentColumnsListBoxTrs[5].TextContent, Is.EqualTo(string.Empty));
	}

	[Test]
	public async Task TestZGridCustomiseListBoxWithMandatoryItems()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			using var grid = new ZGrid();
			using var columns = new ZGridColumns(grid);
			columns.AddBoolColumn(BoolColumnName, 100, false, true);
			columns.AddDateColumn(Date1ColumnName, 100, false, false);
			columns.AddDateColumn(Date2ColumnName, 100, true, true, ZDateTimePickerFormat.Time);
			columns.AddTextColumn(TextColumnName, 100, true, false);
			return new ZGridCustomiseTester(columns, columns, grid);
		});

		Assert.That(rendered.FindAll(".listbox .listbox__item:nth-child(1)")[0].GetAttribute("style"), Does.Contain("color:rgb(128,128,128);"));
		Assert.That(rendered.FindAll(".listbox .listbox__item:nth-child(1)")[0].TextContent, Is.EqualTo(Date2ColumnName));

		Assert.That(rendered.FindAll(".listbox .listbox__item:nth-child(2)")[0].GetAttribute("style"), Does.Not.Contain("color:rgb(128,128,128);"));
		Assert.That(rendered.FindAll(".listbox .listbox__item:nth-child(2)")[0].TextContent, Is.EqualTo(TextColumnName));

		Assert.That(rendered.FindAll(".listbox .listbox__item:nth-child(1)")[1].GetAttribute("style"), Does.Contain("color:rgb(128,128,128);"));
		Assert.That(rendered.FindAll(".listbox .listbox__item:nth-child(1)")[1].TextContent, Is.EqualTo(BoolColumnName));

		Assert.That(rendered.FindAll(".listbox .listbox__item:nth-child(2)")[1].GetAttribute("style"), Does.Not.Contain("color:rgb(128,128,128);"));
		Assert.That(rendered.FindAll(".listbox .listbox__item:nth-child(2)")[1].TextContent, Is.EqualTo(Date1ColumnName));
	}

	const string BoolColumnName = "BoolColumn";
	const string Date1ColumnName = "Date1Column";
	const string Date2ColumnName = "Date2Column";
	const string TextColumnName = "TextColumn";
}
