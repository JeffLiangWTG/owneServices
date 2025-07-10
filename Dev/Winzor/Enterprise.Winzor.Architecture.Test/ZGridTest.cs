using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Bunit;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Balloons;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Testing;
using Extensions;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.Winzor.Architecture.Test;

using static PlaywrightTestContext;

class ZGridTest
{
	[TestCase(true, false, false, false, TestName = "{m}_Checked")]
	[TestCase(false, false, false, false, TestName = "{m}_UnChecked")]
	[TestCase(true, true, false, false, TestName = "{m}_Checked_ReadOnlyGrid")]
	[TestCase(false, true, false, false, TestName = "{m}_UnChecked_ReadOnlyGrid")]
	[TestCase(true, false, true, false, TestName = "{m}_Checked_ReadOnlyColumn")]
	[TestCase(false, false, true, false, TestName = "{m}_UnChecked_ReadOnlyColumn")]
	[TestCase(true, false, false, true, TestName = "{m}_Checked_ReadOnlyCell")]
	[TestCase(false, false, false, true, TestName = "{m}_UnChecked_ReadOnlyCell")]
	public async Task CheckBoxMarkupTests(bool checkedValue, bool gridReadOnly, bool columnReadOnly, bool cellReadOnly)
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			var dummy1 = collection.AddNew();
			dummy1.Z0_Description = "One";
			dummy1.Z0_Bool = checkedValue;
			var form = new WinzorTestForm();
			var grid = new ZGrid { Dock = DockStyle.Fill };
			grid.ReadOnly = gridReadOnly;
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Description, 80) { CharacterCasing = CharacterCasing.Normal });
			grid.ColumnStyles.Add(new ZCheckBoxColumnStyleInfoForTest(DummyBizoSchema.Constants.Z0_Bool, 80, cellReadOnly) { IsReadOnly = columnReadOnly });
			form.Controls.Add(grid);
			grid.SetDataBinding(collection, "");
			return form;
		});

		var isChecked = checkedValue ? " checked=\"\"" : "";
		var isDisabled = (gridReadOnly || columnReadOnly || cellReadOnly) ? " onclick=\"return false;\"" : "";
		var checkboxHtml = string.Empty;
		if (gridReadOnly || columnReadOnly || cellReadOnly)
		{
			checkboxHtml = $"<input class=\"checkbox__input checkbox__input--align-middle\" type=\"checkbox\"{isChecked}{isDisabled} tabindex=\"-1\">";
		}
		else
		{
			checkboxHtml = $"<input class=\"checkbox__input checkbox__input--align-middle checkbox__input--editable\" type=\"checkbox\"{isChecked}{isDisabled}>";
		}
		var td = rendered.FindAll("tbody>tr>td");
		Assert.That(td[2].InnerHtml, Is.EqualTo(checkboxHtml));
	}

	[Test, WithPlaywrightPage]
	public async Task CheckBoxBackgroundInGridShouldNotBeRed()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			var dummy1 = collection.AddNew();
			dummy1.Z0_Code = "code";
			dummy1.Z0_Description = "textbox";
			dummy1.Z0_Bool = true;
			dummy1.SuspendValidationTesting();
			dummy1.Z0_BoolInfo.AddError("Error! CheckBox");
			dummy1.Z0_DescriptionInfo.AddError("Error! TextBox");
			var form = new WinzorTestForm();
			var grid = new ZGrid { Dock = DockStyle.Fill };
			grid.ReadOnly = false;
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Code, 80) { CharacterCasing = CharacterCasing.Normal });
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Description, 80) { CharacterCasing = CharacterCasing.Normal });
			grid.ColumnStyles.Add(new ZCheckBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Bool, 80) { IsReadOnly = false });
			form.Controls.Add(grid);
			grid.SetDataBinding(collection, "");
			return form;
		});

		var textBoxCell = page.Locator("tr.datagrid__row--edit > td:nth-child(3)");
		await Assertions.Expect(textBoxCell.GetByRole(AriaRole.Checkbox)).ToHaveCountAsync(0);
		await Assertions.Expect(textBoxCell).ToHaveCSSAsync("background-color", "rgb(255, 215, 215)");

		var checkBoxCell = page.Locator("tr.datagrid__row--edit > td:nth-child(4)");
		await Assertions.Expect(checkBoxCell.GetByRole(AriaRole.Checkbox)).ToHaveCountAsync(1);
		await Assertions.Expect(checkBoxCell).ToHaveCSSAsync("background-color", "rgba(0, 0, 0, 0)");
	}

	[Test]
	public async Task CheckBoxCenteredStyleTest()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			var dummy1 = collection.AddNew();
			dummy1.Z0_Description = "One";
			dummy1.Z0_Bool = true;
			var form = new WinzorTestForm();
			var grid = new ZGrid { Dock = DockStyle.Fill };
			grid.ReadOnly = false;
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Description, 80) { CharacterCasing = CharacterCasing.Normal });
			grid.ColumnStyles.Add(new ZCheckBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Bool, 80) { IsReadOnly = false });
			form.Controls.Add(grid);
			grid.SetDataBinding(collection, "");
			return form;
		});

		var td = rendered.FindAll("tbody>tr>td");
		Assert.That(td[2].GetAttribute("style"), Does.Contain("text-align:center;"));
	}

	[Test, WithPlaywrightPage]
	public async Task CheckBoxColumnHasMinWidthWhenResizing()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			var dummy1 = collection.AddNew();
			dummy1.Z0_Description = "One";
			dummy1.Z0_Bool = true;
			var grid = new ZGridWithoutSaveUserLayoutSettings { Dock = DockStyle.Fill };
			grid.ReadOnly = false;
			grid.ColumnStyles.Add(new ZCheckBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Bool, 80) { IsReadOnly = false });

			var form = new WinzorTestForm();
			form.Controls.Add(grid);
			grid.SetDataBinding(collection, "");
			return form;
		});

		var columnHeader = page.Locator("table th:nth-child(2)");
		await Assertions.Expect(columnHeader).ToHaveCSSAsync("width", "80px");

		var resizer = page.Locator("th .datagrid__column_resizer");
		await resizer.WaitForAsync();

		// make newWidth greater than minWidth, should set Width to newWidth
		await ElementMouseMoveWithDelayAsync(resizer, 0, 2);
		await MouseDownAsync();
		await ElementMouseMoveWithDelayAsync(resizer, 80, 2);
		await MouseUpAsync();
		await Assertions.Expect(columnHeader).ToHaveCSSAsync("width", "160px");

		// make newWidth less than minWidth, should set Width to minWidth, minWidth = 40
		await ElementMouseMoveWithDelayAsync(resizer, 0, 2);
		await MouseDownAsync();
		await ElementMouseMoveWithDelayAsync(resizer, -155, 2);
		await MouseUpAsync();
		await Assertions.Expect(columnHeader).ToHaveCSSAsync("width", "40px");
	}

	[Test, WithPlaywrightPage]
	public async Task TestGridWithColorSchemeDisplayCorrectly()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZGridWithCustomScheme grid = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			var dummy1 = collection.AddNew();
			dummy1.Z0_Description = "One";
			dummy1.Z0_Bool = true;

			grid = new ZGridWithCustomScheme { Dock = DockStyle.Fill };
			grid.ReadOnly = false;
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Description, 80) { CharacterCasing = CharacterCasing.Normal });
			var scheme1 = new BusinessObjectFactory().New<GridColourScheme>();
			var colorstrip = new GridColourStripBusinessObject(grid.StripControl.FilterBusinessObject, scheme1, null);
			scheme1.ColourStrips.Add(colorstrip);
			grid.SchemeForTest = scheme1;

			var form = new WinzorTestForm();
			form.Controls.Add(grid);
			grid.SetDataBinding(collection, "");
			grid.MouseDown += (s, e) =>	grid.StartLoadBackgroundColour();
			return form;
		});

		var gridRows = page.Locator(".datagrid__container tbody tr");
		var targetCell = page.Locator(".datagrid__row-star--new td:nth-child(2)");
		await Assertions.Expect(gridRows).ToHaveCountAsync(2);

		await targetCell.ClickAsync();
		await page.Mouse.ClickAsync(150, 150);
		await grid.InvokeWinzorDispatcherAsync(() =>
		{
			grid.NotifyRenderRequiredExposed();
		});
		await Assertions.Expect(gridRows).ToHaveCountAsync(2);
	}

	[Test, WithPlaywrightPage]
	public async Task CheckZCheckEditColumnStyleCellBackgroundColor()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var child1 = factory.New<DummyBaseBusinessObject>();
			child1.Z0_Code = "1";
			child1.Z0_Bool = true;
			dummyBizo.Collection.Add(child1);

			var grid = new ZGrid();
			grid.ColumnStyles.Add(new ZCheckBoxColumnStyleInfo("Z0_Bool", 100));
			grid.LayoutCategoryPK = Guid.NewGuid();

			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			form.BindingSource.SetBindingMember(grid, "Collection");
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			form.SetDataBinding(dummyBizo, "");

			return form;
		});

		var checkBoxCell = page.Locator("td:has(input[type='checkbox'])");
		await Assertions.Expect(checkBoxCell).ToHaveCSSAsync("background-color", "rgba(0, 0, 0, 0)");
	}

	[Test, WithPlaywrightPage]
	public async Task CheckZCheckEditColumnStyleAlignment()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var child1 = factory.New<DummyBaseBusinessObject>();
			child1.Z0_Code = "1";
			child1.Z0_Bool = true;
			dummyBizo.Collection.Add(child1);

			var grid = new ZGrid();
			grid.ColumnStyles.Add(new ZCheckBoxColumnStyleInfo("Z0_Bool", 100));
			grid.LayoutCategoryPK = Guid.NewGuid();

			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			form.BindingSource.SetBindingMember(grid, "Collection");
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			form.SetDataBinding(dummyBizo, "");

			return form;
		});

		var checkBoxCell = page.Locator(".datagrid tbody tr:nth-child(1) > td:nth-child(2)");
		await checkBoxCell.WaitForAsync();
		var input = checkBoxCell.GetByRole(AriaRole.Checkbox);
		await input.WaitForAsync();

		var cellBox = await checkBoxCell.BoundingBoxAsync();
		var inputBox = await input.BoundingBoxAsync();

		Assert.That(inputBox.Y, Is.GreaterThan(cellBox.Y));
		Assert.That(inputBox.Y, Is.EqualTo(cellBox.Y + 0.5));
	}

	[Test, WithPlaywrightPage]
	public async Task CheckZCheckDoesNotMove()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var child1 = factory.New<DummyBaseBusinessObject>();
			child1.Z0_Code = "1";
			dummyBizo.Collection.Add(child1);

			var grid = new ZGrid();
			grid.ColumnStyles.Add(new ZCheckBoxColumnStyleInfo("Z0_Bool", 100));
			grid.LayoutCategoryPK = Guid.NewGuid();

			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			form.BindingSource.SetBindingMember(grid, "Collection");
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			form.SetDataBinding(dummyBizo, "");

			var textbox = new TextBox();
			form.Controls.Add(textbox);

			return form;
		});

		var controlEdit = page.Locator(".datagrid__control--edit");
		await Assertions.Expect(controlEdit).ToHaveCountAsync(1);

		var input = page.GetByRole(AriaRole.Checkbox);
		await input.WaitForAsync();
		var inputBox = await input.BoundingBoxAsync();
		Assert.That(inputBox.Height, Is.EqualTo(14).Within(0.1));
		Assert.That(inputBox.Width, Is.EqualTo(14).Within(0.1));

		var statusBar = page.Locator(".statusbar");
		await statusBar.ClickAsync();
		await Assertions.Expect(controlEdit).ToHaveCountAsync(0);

		await input.WaitForAsync();
		var inputBox_after = await input.BoundingBoxAsync();
		Assert.That(inputBox.Y, Is.EqualTo(inputBox_after.Y).Within(0.1));
		Assert.That(inputBox_after.Height, Is.EqualTo(14).Within(0.1));
		Assert.That(inputBox_after.Width, Is.EqualTo(14).Within(0.1));
	}

	class ZGridWithoutSaveUserLayoutSettings : ZGrid
	{
		public override void SaveUserLayoutSettings()
		{
		}
	}

	[Test]
	public async Task TextBoxHtmlEncodedTest()
	{
		var htmlStr = "<h1>Test</h1>";
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			var dummy1 = collection.AddNew();
			dummy1.Z0_Description = htmlStr;
			dummy1.Z0_Bool = true;
			var form = new WinzorTestForm();
			var grid = new ZGrid { Dock = DockStyle.Fill };
			grid.ReadOnly = false;
			grid.ColumnStyles.Add(new ZCheckBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Bool, 80) { IsReadOnly = false });
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Description, 80) { CharacterCasing = CharacterCasing.Normal });
			form.Controls.Add(grid);
			grid.SetDataBinding(collection, "");
			return form;
		});

		var td = rendered.FindAll("tbody>tr>td");
		Assert.That(td[2].InnerHtml, Is.EqualTo($"{WebUtility.HtmlEncode(htmlStr)}"));
	}

	[Test]
	public async Task MultiLineTextBoxTest()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			var dummy1 = collection.AddNew();
			var dummy2 = collection.AddNew();

			dummy1.Z0_Description = "One 1111111111\r\n2222222222222";
			dummy2.Z0_Description = "Two 2222222222222\r\n1111111111";

			var form = new WinzorTestForm();
			var grid = new ZGrid { Dock = DockStyle.Fill };

			grid.ReadOnly = false;
			grid.ColumnStyles.Add(new ZMultiLineTextBoxColumnInfo(DummyBizoSchema.Constants.Z0_Description, 300) { CharacterCasing = CharacterCasing.Normal });
			grid.ColumnStyles.Add(new ZMultiLineTextBoxColumnInfo(DummyBizoSchema.Constants.Z0_Description, 300) { IsReadOnly = false });
			form.Controls.Add(grid);
			grid.SetDataBinding(collection, "");

			return form;
		});

		Assert.That(rendered.Find(".datagrid__control--edit .textbox").GetAttribute("value"), Is.EqualTo("One 1111111111\n2222222222222"));
		Assert.That(rendered.Find("tbody>tr:nth-child(2)>td:nth-child(2)").TextContent, Is.EqualTo("Two 2222222222222"));
	}

	[Test]
	public async Task DateTextBoxTest()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			var dummy1 = collection.AddNew();

			dummy1.Z0_Date = new DateTime(2022, 1, 1);

			var form = new WinzorTestForm();
			var grid = new ZGrid { Dock = DockStyle.Fill };

			grid.ReadOnly = false;
			grid.ColumnStyles.Add(new ZDateEditColumnStyleInfo(DummyBizoSchema.Constants.Z0_Date, 300) { DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Long });
			form.Controls.Add(grid);
			grid.SetDataBinding(collection, "");

			return form;
		});

		var dateTextBox = rendered.Find(".datagrid__control--edit .textbox");
		Assert.That(dateTextBox.GetAttribute("value"), Is.EqualTo("01-JAN-22 00:00"));

		await dateTextBox.FocusOutAsync();
		Assert.That(rendered.Find("tbody>tr>td:nth-child(2)").TextContent, Is.EqualTo("01-JAN-22 00:00"));
	}

	[Test, WithPlaywrightPage]
	public async Task ValidateNumericOnlyForZCalcEditColumnStyleInfoClientSide()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			var dummy1 = collection.AddNew();
			dummy1.Z0_Number = 5;

			var form = new WinzorTestForm();
			var grid = new ZGrid { Dock = DockStyle.Fill };

			var zCalcEditColumnStyleInfo = new ZCalcEditColumnStyleInfo(DummyBizoSchema.Constants.Z0_Number, 80, 0, false);

			grid.ReadOnly = false;
			grid.ColumnStyles.Add(zCalcEditColumnStyleInfo);
			form.Controls.Add(grid);

			grid.SetDataBinding(collection, "");

			return form;
		});

		var cell = page.Locator("table tr td:nth-child(2)").First;
		await cell.DblClickAsync();

		var input = cell.GetByRole(AriaRole.Textbox);
		await input.WaitForAsync();
		await input.PressSequentiallyAsync("1abc5");
		await input.BlurAsync();

		await Assertions.Expect(cell).ToHaveTextAsync("15");
	}

	[Test, WithPlaywrightPage]
	public async Task ColumnHeaderDragReorderTest()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Size = new Size(400, 300) };
			var panel = new Panel
			{
				Dock = DockStyle.None,
				Top = 10,
				Left = 10,
				Width = 300,
				Height = 200
			};
			panel.Controls.Add(CreateNewZGrid<ZGrid>());
			form.Controls.Add(panel);
			return form;
		});

		var grid = page.Locator(".datagrid").First;
		await grid.WaitForAsync();
		var columnHeader = page.Locator("thead tr");
		await columnHeader.WaitForAsync();

		var columns = columnHeader.Locator("th");
		var column1Span = page.Locator("thead tr th span").First;
		
		await Assertions.Expect(column1Span).ToHaveTextAsync("One Header");
		await Assertions.Expect(columns.Nth(1)).ToHaveTextAsync("One Header");
		await Assertions.Expect(columns.Nth(2)).ToHaveTextAsync("Two Header");
		await Assertions.Expect(columns.Nth(3)).ToHaveTextAsync("Three Header");

		// if mouse didn't move, drag event wasn't triggered
		await ElementMouseMoveWithDelayAsync(columns.Nth(1), 20, 5, 50);
		await MouseDownAsync();

		var dragMask = page.Locator(".dragMask");
		await Assertions.Expect(dragMask).ToHaveCountAsync(0);
		await Assertions.Expect(columnHeader).Not.ToHaveClassAsync("datagrid__column--dragged");
		await Assertions.Expect(columns.Nth(1)).ToHaveCSSAsync("border", "");
		await Assertions.Expect(columns.Nth(1)).ToHaveCSSAsync("color", "rgb(0, 0, 0)");
		await Assertions.Expect(columns.Nth(2)).ToHaveCSSAsync("background-color", "rgb(244, 246, 247)");
		await Assertions.Expect(columns.Nth(2)).ToHaveCSSAsync("color", "rgb(0, 0, 0)");
		await Assertions.Expect(columns.Nth(3)).ToHaveCSSAsync("background-color", "rgb(244, 246, 247)");
		await Assertions.Expect(columns.Nth(3)).ToHaveCSSAsync("color", "rgb(0, 0, 0)");

		// scroll down and mouse moved on column2
		// should show dragMask and column2 should have a readonly backcolor
		await grid.EvaluateAsync("e => e.scrollTo(50, 100);");
		await Assertions.Expect(grid).ToHaveJSPropertyAsync("scrollLeft", 50);
		await Assertions.Expect(grid).ToHaveJSPropertyAsync("scrollTop", 100);
		await ElementMouseMoveWithDelayAsync(columns.Nth(2), 20, 5, 50);

		await Assertions.Expect(dragMask).ToHaveCountAsync(1);
		await Assertions.Expect(columnHeader).ToHaveClassAsync("datagrid__column--dragged");
		await Assertions.Expect(dragMask).ToHaveCSSAsync("position", "absolute");
		await Assertions.Expect(dragMask).ToHaveCSSAsync("pointer-events", "none");
		await Assertions.Expect(dragMask).ToHaveCSSAsync("background-color", "rgb(0, 168, 225)");
		await Assertions.Expect(dragMask).ToHaveCSSAsync("width", "80px");
		await Assertions.Expect(dragMask).ToHaveCSSAsync("height", "21px");
		await Assertions.Expect(dragMask).ToHaveCSSAsync("padding-top", "2px");
		await Assertions.Expect(dragMask).ToHaveCSSAsync("top", "0px");
		await Assertions.Expect(columns.Nth(1)).ToHaveCSSAsync("border", "1px solid rgb(0, 0, 0)");
		await Assertions.Expect(columns.Nth(1)).ToHaveCSSAsync("color", "rgb(128, 128, 128)");
		var scrollLeft = await grid.EvaluateAsync<int>("e => e.scrollLeft");
		Assert.That(await dragMask.EvaluateAsync<int>("e => e.getBoundingClientRect().left") + scrollLeft, Is.EqualTo(145).Within(2));

		// scroll up and mouse moved on column3
		// should show dragMask and column3 should have a readonly backcolor
		await grid.EvaluateAsync("e => e.scrollTo(50, 20);");
		await Assertions.Expect(grid).ToHaveJSPropertyAsync("scrollTop", 20);
		await ElementMouseMoveWithDelayAsync(columns.Nth(3), 20, 5, 50);

		await Assertions.Expect(columnHeader).ToHaveClassAsync("datagrid__column--dragged");
		await Assertions.Expect(dragMask).ToHaveCSSAsync("position", "absolute");
		await Assertions.Expect(dragMask).ToHaveCSSAsync("pointer-events", "none");
		await Assertions.Expect(dragMask).ToHaveCSSAsync("background-color", "rgb(0, 168, 225)");
		await Assertions.Expect(dragMask).ToHaveCSSAsync("width", "80px");
		await Assertions.Expect(dragMask).ToHaveCSSAsync("height", "21px");
		await Assertions.Expect(dragMask).ToHaveCSSAsync("padding-top", "2px");
		await Assertions.Expect(dragMask).ToHaveCSSAsync("top", "0px");
		await Assertions.Expect(columns.Nth(1)).ToHaveCSSAsync("border", "1px solid rgb(0, 0, 0)");
		await Assertions.Expect(columns.Nth(1)).ToHaveCSSAsync("color", "rgb(128, 128, 128)");
		scrollLeft = await grid.EvaluateAsync<int>("e => e.scrollLeft");
		Assert.That(await dragMask.EvaluateAsync<int>("e => e.getBoundingClientRect().left") + scrollLeft, Is.EqualTo(325).Within(2));

		// mouse up on column3
		// should move column1 to last
		await MouseUpAsync();
		await Assertions.Expect(dragMask).ToHaveCountAsync(0);
		await Assertions.Expect(columns.Nth(1)).Not.ToHaveClassAsync("datagrid__column--dragged");
		await Assertions.Expect(columns.Nth(1)).ToHaveCSSAsync("border", "");
		await Assertions.Expect(columns.Nth(1)).ToHaveCSSAsync("color", "rgb(0, 0, 0)");
		await Assertions.Expect(columns.Nth(1)).ToHaveTextAsync("Two Header");
		await Assertions.Expect(columns.Nth(2)).ToHaveTextAsync("Three Header");
		await Assertions.Expect(columns.Nth(3)).ToHaveTextAsync("One Header");
	}

	[TestCase("en-ZA")]
	[TestCase("de-DE")]
	[TestCase("en-US")]
	[TestCase("en-AU")]
	[Test, WithPlaywrightPage]
	public async Task EnsureDotDelimiterChangeToCommaInCaretPosition(string countryID)
	{
		await using var ctx = new InMemoryAppServerTestContext();

		string currencySeparator = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(CultureInfo.CreateSpecificCulture(countryID)))
			{
				StaticCurrentFetcher.Instance.CurrentCompany.SetCountry(countryID.Split('-')[1]);
				var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
				var dummy1 = collection.AddNew();
				dummy1.Z0_Decimal = 0.00;

				var form = new WinzorTestForm();
				var grid = new ZGrid { Dock = DockStyle.Fill };

				var zCalcEditColumnStyleInfo = new ZCalcEditColumnStyleInfo(DummyBizoSchema.Constants.Z0_Decimal, 100, 2, false);

				grid.ReadOnly = false;
				grid.ColumnStyles.Add(zCalcEditColumnStyleInfo);
				form.Controls.Add(grid);

				grid.SetDataBinding(collection, "");

				currencySeparator = Culture.CurrentCompanyCountryCulture.NumberFormat.NumberDecimalSeparator;

				return form;
			}
		});

		var cell = page.Locator("table tr td:nth-child(2)").First;
		await cell.DblClickAsync();

		var input = cell.GetByRole(AriaRole.Textbox);
		await input.WaitForAsync();
		await input.PressSequentiallyAsync("101.00");
		await input.BlurAsync();

		Assert.That(currencySeparator, Is.Not.Null);
		await Assertions.Expect(cell).ToHaveTextAsync("101" + currencySeparator + "00");
	}

	[Test, WithPlaywrightPage]
	public async Task EnterNextRowWhenPressEnterKey()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => CreateNewZGrid<ZGrid>());

		var cell = page.Locator("table tr:nth-child(9) td:nth-child(3)");
		await cell.DblClickAsync();

		var input = cell.GetByRole(AriaRole.Textbox);
		await input.WaitForAsync();
		await input.PressAsync("Enter");

		Assert.That(async () => await page.EvaluateAsync<string>("e => window.getSelection().toString()"), Is.EqualTo("cell(10, 1)").After(3000, 100));
	}

	[Test]
	public async Task ClickDifferentRowsSameColumnChangeWinzorControlId()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			return CreateNewZGrid<ZGrid>();
		});

		await SimulateClickOnCellForRowChange(rendered.FindAll("tbody>tr")[1].Children[2]);
		var input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		var id = input.GetAttribute("data-winzor-control-id");

		await SimulateClickOnCellForRowChange(rendered.FindAll("tbody>tr")[2].Children[2]);
		var input2 = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		var id2 = input2.GetAttribute("data-winzor-control-id");

		Assert.That(id, Is.Not.EqualTo(id2));
	}

	[Test]
	public async Task ClickTopLeftCornerShowCustomiseColumnsTest()
	{
		Task loadRequestTask = null;
		var windowService = new Mock<IWindowService>();
		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				loadRequestTask = Task.Run(() =>
				{
					using var ctxMock = new EnterpriseTestContext();
					var renderedNewForm = ctxMock.RenderEntryPointComponent(createWindowOptions.Uri.ToString());
					renderedNewForm.WaitForState(() => renderedNewForm.Instance.Form != null);
					Assert.That(renderedNewForm.Instance.Form, Is.InstanceOf<ZGridCustomise>());
					_ = ctxMock.WinzorDispatcher.InvokeAsync(renderedNewForm.Instance.Form.Dispose);
				});
			});

		var clientServices = MockCargoWiseClientServices.MakeMock(windowService: windowService.Object);
		WinzorTestForm form = null;
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new WinzorTestForm();

			var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			var grid = new ZGrid { Dock = DockStyle.Fill };

			grid.ReadOnly = false;
			grid.ColumnStyles.Add(new ZMultiLineTextBoxColumnInfo(DummyBizoSchema.Constants.Z0_Description, 300) { CharacterCasing = CharacterCasing.Normal });
			grid.ColumnStyles.Add(new ZMultiLineTextBoxColumnInfo(DummyBizoSchema.Constants.Z0_Description, 300) { IsReadOnly = false });
			form.Controls.Add(grid);
			grid.SetDataBinding(collection, "");

			return form;
		}, clientServices);

		var topLeft = rendered.WaitForElement("thead tr th div");
		await topLeft.MouseDownAsync(new WebMouseEventArgs());

		Assert.That(await loadRequestTask.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task StyleInGridTest()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => CreateNewZGrid<ZGrid>());

		var tdInGrid = page.Locator(".datagrid td").First;
		await Assertions.Expect(tdInGrid).ToHaveCSSAsync("white-space", "nowrap");
		await Assertions.Expect(tdInGrid).ToHaveCSSAsync("padding", "0px 2px");
	}

	[Test, WithPlaywrightPage]
	public async Task DatagridShoudlRenderCellsWhileAddChildData()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		DummyGridBussinessObjectCollection collection = null;
		ZGridForTest grid = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			collection = new DummyGridBussinessObjectCollection(new BusinessObjectFactory());
			var dummy1 = collection.AddNew();
			dummy1.Z0_Code = "code1";
			dummy1.AddChildDataField = "field1";
			dummy1.SuspendValidationTesting();
			grid = new ZGridForTest { Dock = DockStyle.Fill };
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Code, 80) { CharacterCasing = CharacterCasing.Normal });
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("AddChildDataField", 80) { CharacterCasing = CharacterCasing.Normal });
			var form = new WinzorTestForm();
			form.Controls.Add(grid);
			grid.SetDataBinding(collection, "");
			return form;
		});

		var gridRowHeader = page.Locator("tr:first-child > td:nth-child(1)");
		var textBoxCell1 = page.Locator("tr:first-child > td:nth-child(2)");
		var textBoxCell2 = page.Locator("tr:first-child > td:nth-child(3)");
		var textBoxCell3 = page.Locator("tr:nth-child(2) > td:nth-child(2)");
		var textBoxCell4 = page.Locator("tr:nth-child(2) > td:nth-child(3)");
		var orginalSetDataCount = grid.SetDataGridRowsCount;
		await gridRowHeader.ClickAsync();

		Assert.That(async () => await textBoxCell1.InnerTextAsync(), Is.EqualTo("code1").After(2000, 200));
		Assert.That(async () => await textBoxCell2.InnerTextAsync(), Is.EqualTo("field1").After(2000, 200));
		Assert.That(async () => await textBoxCell3.InnerTextAsync(), Is.EqualTo(string.Empty).After(2000, 200));
		Assert.That(async () => await textBoxCell4.InnerTextAsync(), Is.EqualTo(string.Empty).After(2000, 200));

		await grid.InvokeWinzorDispatcherAsync(() =>
		{
			var dummy2 = collection.AddNew();
			dummy2.Z0_Code = "code2";
			dummy2.AddChildDataField = "field2";
		});

		Assert.That(async () => await textBoxCell1.InnerTextAsync(), Is.EqualTo("code1").After(2000, 200));
		Assert.That(async () => await textBoxCell2.InnerTextAsync(), Is.EqualTo("field1").After(2000, 200));
		Assert.That(async () => await textBoxCell3.InnerTextAsync(), Is.EqualTo("code2").After(2000, 200));
		Assert.That(async () => await textBoxCell4.InnerTextAsync(), Is.EqualTo("field2").After(2000, 200));
		Assert.That(grid.SetDataGridRowsCount, Is.GreaterThan(orginalSetDataCount));
	}

	[Test, WithPlaywrightPage]
	public async Task DatagridShoudlRenderCellsAfterRefresh()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		DummyActiveBusinessObjectCollection collection = null;
		ZGridForTest grid = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			collection = new DummyActiveBusinessObjectCollection(new BusinessObjectFactory());
			var dummy1 = collection.AddNew();
			dummy1.Z0_Code = "code1";
			dummy1.RefreshDataField = "field1";
			dummy1.SuspendValidationTesting();
			grid = new ZGridForTest { Dock = DockStyle.Fill };
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Code, 80) { CharacterCasing = CharacterCasing.Normal });
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("RefreshDataField", 80) { CharacterCasing = CharacterCasing.Normal });
			var form = new WinzorTestForm();
			form.Controls.Add(grid);
			grid.SetDataBinding(collection, "");
			return form;
		});

		var gridRowHeader = page.Locator("tr:first-child > td:nth-child(1)");
		var textBoxCell1 = page.Locator("tr:first-child > td:nth-child(2)");
		var textBoxCell2 = page.Locator("tr:first-child > td:nth-child(3)");
		var textBoxCell3 = page.Locator("tr:nth-child(2) > td:nth-child(2)");
		var textBoxCell4 = page.Locator("tr:nth-child(2) > td:nth-child(3)");
		var orginalSetDataCount = grid.SetDataGridRowsCount;
		await gridRowHeader.ClickAsync();

		Assert.That(async () => await textBoxCell1.InnerTextAsync(), Is.EqualTo("code1").After(2000, 200));
		Assert.That(async () => await textBoxCell2.InnerTextAsync(), Is.EqualTo("field1").After(2000, 200));
		Assert.That(async () => await textBoxCell3.InnerTextAsync(), Is.EqualTo(string.Empty).After(2000, 200));
		Assert.That(async () => await textBoxCell4.InnerTextAsync(), Is.EqualTo(string.Empty).After(2000, 200));

		await grid.InvokeWinzorDispatcherAsync(() =>
		{
			var dummy2 = collection.AddNew();
			dummy2.Z0_Code = "code2";
			dummy2.RefreshDataField = "field2";
		});

		Assert.That(async () => await textBoxCell1.InnerTextAsync(), Is.EqualTo("code1").After(2000, 200));
		Assert.That(async () => await textBoxCell2.InnerTextAsync(), Is.EqualTo("field1").After(2000, 200));
		Assert.That(async () => await textBoxCell3.InnerTextAsync(), Is.EqualTo("code2").After(2000, 200));
		Assert.That(async () => await textBoxCell4.InnerTextAsync(), Is.EqualTo("field2").After(2000, 200));
		Assert.That(grid.SetDataGridRowsCount, Is.GreaterThan(orginalSetDataCount));
	}

	[Test, WithPlaywrightPage]
	public async Task NotificationStyleInCheckBoxTest()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			var dummy1 = collection.AddNew();
			dummy1.Z0_Description = "One";
			dummy1.Z0_Bool = true;
			dummy1.SuspendValidationTesting();
			dummy1.Z0_BoolInfo.AddWarning("Warning!");
			var form = new WinzorTestForm();
			var grid = new ZGrid { Dock = DockStyle.Fill };
			grid.ReadOnly = false;
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Description, 80) { CharacterCasing = CharacterCasing.Normal });
			grid.ColumnStyles.Add(new ZCheckBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Bool, 80) { IsReadOnly = false });
			form.Controls.Add(grid);
			grid.SetDataBinding(collection, "");
			return form;
		});

		var warnTdCell = page.Locator("td.datagrid__cell--warning");
		await Assertions.Expect(warnTdCell).ToHaveCSSAsync("background-color", "rgba(0, 0, 0, 0)");

		var firstCell = page.Locator(".datagrid td:nth-child(2)").First;
		await firstCell.ClickAsync();
		await page.Keyboard.PressAsync("Tab");
		await Assertions.Expect(page.Locator(".datagrid__cell--warning.datagrid__cell--edit")).ToHaveCountAsync(1);
		
		var warnIcon = page.Locator(".datagrid .datagrid__cell--warning.datagrid__cell--edit .notification.notification--warning");
		await Assertions.Expect(warnIcon).ToHaveCSSAsync("background-color", "rgba(0, 0, 0, 0)");
	}

	[Test, WithPlaywrightPage, WithTransaction]
	public async Task ShowBalloonInCorrectPositionWhenReadonlyCellFocused()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZGrid grid = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			EnvProxy.Instance.Registry.TraningModeEnabled = true;
			// init data
			var message = "Hello baba la";
			var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			var dummy1 = collection.AddNew();
			dummy1.Z0_Bool = true;
			dummy1.SuspendValidationTesting();
			dummy1.Z0_BoolInfo.AddInformation(message);

			// init form and grid
			var form = new WinzorTestForm() { Width = 500, Height = 500 };
			grid = new ZGrid { Dock = DockStyle.Fill };
			grid.ReadOnly = false;
			// important
			grid.ColumnStyles.Add(new ZCheckBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Bool, 80) { IsReadOnly = true });
			form.Controls.Add(grid);
			grid.SetDataBinding(collection, "");
			return form;
		});

		Assert.That(grid, Is.Not.Null);
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			Assert.That(FindEditControl(grid),
				Is.EqualTo(Balloon.Instance.BalloonWindow.GetAnchorControlForTest()));
		});
	}

	Control FindEditControl(ZGrid grid)
	{
		var columnStyle = grid.TableStyles[0].GridColumnStyles[grid.CurrentCell.ColumnNumber];
		if (columnStyle is ZTextBoxColumnStyle currentColumnStyle)
		{
			return currentColumnStyle.EditControl;
		}
		return null;
	}

	public static IEnumerable<TestCaseData> NotificationTypes
	{
		get
		{
			yield return new TestCaseData(NotificationType.Error, (ZPropertyInfo propertyInfo) => propertyInfo.AddError("Error")) { TestName = "{m}_Error" };
			yield return new TestCaseData(NotificationType.Warning, (ZPropertyInfo propertyInfo) => propertyInfo.AddWarning("Warning")) { TestName = "{m}_Warning" };
			yield return new TestCaseData(NotificationType.Information, (ZPropertyInfo propertyInfo) => propertyInfo.AddInformation("Information")) { TestName = "{m}_Information" };
			yield return new TestCaseData(CargoWise.EntityFramework.NotificationType.MessageError, (ZPropertyInfo propertyInfo) => propertyInfo.AddMessageError("Message Error")) { TestName = "{m}_MessageError" };
		}
	}

	[TestCaseSource(nameof(NotificationTypes)), WithPlaywrightPage]
	public async Task NotificationStyleInDescriptionTest(INotificationType notificationType, Action<ZPropertyInfo> addNotification)
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			var dummy1 = collection.AddNew();
			dummy1.Z0_Bool = true;
			dummy1.Z0_Description = "One";
			dummy1.SuspendValidationTesting();
			addNotification(dummy1.Z0_DescriptionInfo);
			var form = new WinzorTestForm();
			var grid = new ZGrid { Dock = DockStyle.Fill };
			grid.ReadOnly = false;
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Description, 80) { CharacterCasing = CharacterCasing.Normal });
			grid.ColumnStyles.Add(new ZCheckBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Bool, 80) { IsReadOnly = false });
			form.Controls.Add(grid);
			grid.SetDataBinding(collection, "");

			return form;
		});

		await Assertions.Expect(page.Locator($"td.datagrid__cell--{notificationType.EnumValueName.ToLowerHyphen()}")).ToHaveCountAsync(1);

		var firstCell = page.Locator(".datagrid td:nth-child(2)").First;
		await firstCell.ClickAsync();
		await Assertions.Expect(page.Locator($".datagrid__cell--{notificationType.EnumValueName.ToLowerHyphen()}.datagrid__cell--edit")).ToHaveCountAsync(1);
	}

	[Test, WithPlaywrightPage]
	public async Task BackgroundClipInCheckBox()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			var dummy1 = collection.AddNew();
			dummy1.Z0_Description = "One";
			dummy1.Z0_Bool = false;
			dummy1.SuspendValidationTesting();
			dummy1.Z0_BoolInfo.AddWarning("Warning!");

			var dummy2 = collection.AddNew();
			dummy2.Z0_Description = "Two";
			dummy2.Z0_Bool = true;
			dummy2.SuspendValidationTesting();
			dummy2.Z0_BoolInfo.AddWarning("Warning!");

			var form = new WinzorTestForm();
			var grid = new ZGrid { Dock = DockStyle.Fill };
			grid.ColourDeciding += Grid_ColourDeciding;
			grid.ReadOnly = false;
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Description, 80) { CharacterCasing = CharacterCasing.Normal });
			grid.ColumnStyles.Add(new ZCheckBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Bool, 80) { IsReadOnly = false });
			form.Controls.Add(grid);
			grid.SetDataBinding(collection, "");
			return form;

			void Grid_ColourDeciding(object sender, ColourDecidingEventArgs e)
			{
				// Set a custom background colour for the second row of the simulation
				if (e.Pk == dummy2.PK)
				{
					e.Colour = Color.Red;
				}
			}
		});

		var checkBoxCell1 = page.Locator("tbody > tr:nth-of-type(1) > td:nth-child(3)");
		var checkBoxCell2 = page.Locator("tbody > tr:nth-of-type(2) > td:nth-child(3)");
		var rowHeader1 = page.Locator("tbody > tr:nth-of-type(1) > td:nth-child(1)");
		var rowHeader2 = page.Locator("tbody > tr:nth-of-type(2) > td:nth-child(1)");

		// Test the backgroundClip value without any action.
		await Assertions.Expect(checkBoxCell1).ToHaveCSSAsync("background-clip", "border-box");
		await Assertions.Expect(checkBoxCell2).ToHaveCSSAsync("background-clip", "border-box");

		// Test the backgroundClip value with hover action.
		await checkBoxCell1.HoverAsync();
		await Assertions.Expect(checkBoxCell1).ToHaveCSSAsync("background-clip", "content-box");

		// Test the backgroundClip value with custom row background colour
		await checkBoxCell2.HoverAsync();
		await Assertions.Expect(checkBoxCell2).ToHaveCSSAsync("background-clip", "border-box");

		// Test the backgroundClip value with selected row action.
		await rowHeader1.WaitForAsync();
		await SelectRow(rowHeader1);
		await checkBoxCell1.HoverAsync();
		await Assertions.Expect(checkBoxCell1).ToHaveCSSAsync("background-clip", "border-box");

		await rowHeader2.WaitForAsync();
		await SelectRow(rowHeader2);
		await checkBoxCell2.HoverAsync();
		await Assertions.Expect(checkBoxCell2).ToHaveCSSAsync("background-clip", "border-box");
	}

	[Test, WithPlaywrightPage]
	public async Task TdOverflowAttributeTest()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			var dummy1 = collection.AddNew();
			dummy1.Z0_Code = "One";
			dummy1.Z0_Description = "One";
			dummy1.Z0_Bool = true;
			dummy1.SuspendValidationTesting();
			dummy1.Z0_BoolInfo.AddWarning("Warning!");
			dummy1.Z0_DescriptionInfo.AddError("Error!");
			var form = new WinzorTestForm();
			var grid = new ZGrid { Dock = DockStyle.Fill };
			grid.ReadOnly = false;
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Code, 80) { CharacterCasing = CharacterCasing.Normal });
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Description, 80) { CharacterCasing = CharacterCasing.Normal });
			grid.ColumnStyles.Add(new ZCheckBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Bool, 80) { IsReadOnly = false });
			form.Controls.Add(grid);
			grid.SetDataBinding(collection, "");
			return form;
		});

		var warnTdCell = page.Locator("td.datagrid__cell--warning");
		await Assertions.Expect(warnTdCell).ToHaveCSSAsync("overflow", "hidden");

		var errorTdCell = page.Locator("td.datagrid__cell--error");
		await Assertions.Expect(errorTdCell).ToHaveCSSAsync("overflow", "hidden");
	}

	[Test, WithPlaywrightPage]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:Do Not Leave In Debug Messages", Justification = "<Pending>")]
	public async Task MouseCursorStyleWhenDrag()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			var dummy1 = collection.AddNew();
			dummy1.Z0_Description = "One";
			dummy1.Z0_Bool = true;
			var form = new WinzorTestForm();
			var grid = new ZGrid { Dock = DockStyle.Fill };
			grid.ReadOnly = false;
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Description, 80) { CharacterCasing = CharacterCasing.Normal });
			grid.ColumnStyles.Add(new ZCheckBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Bool, 80) { IsReadOnly = false });
			form.Controls.Add(grid);
			grid.SetDataBinding(collection, "");
			return form;
		});

		var dragTdCell = page.Locator("tr:nth-of-type(1) td:first-child");
		await dragTdCell.WaitForAsync();
		await ElementMouseMoveWithDelayAsync(dragTdCell, 1, 1);
		await MouseDownAsync();
		await MouseUpAsync();
		await Assertions.Expect(page.Locator("tr.datagrid__row--selected")).ToHaveCountAsync(1);

		await MouseDownAsync();
		await ElementMouseMoveWithDelayAsync(dragTdCell, 10, 10);
		await Assertions.Expect(dragTdCell).ToHaveCSSAsync("cursor", "not-allowed");
	}

	[Test, WithPlaywrightPage]
	public async Task ZGridCellFirstRightClickShowContextMenu()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZGridForTest grid = null;
		var page = await ctx.LoadControlOnFormAsync(() => grid = CreateNewZGrid<ZGridForTest>());

		var cell = page.GetByText("Cell(8, 2)");
		await cell.ClickAsync(new () { Button = MouseButton.Right });
		Assert.That(() => grid.Count, Is.EqualTo(1).After(1000, 100));
	}

	[WithPlaywrightPage]
	[TestCase(MouseButton.Left, false, true, true)]
	[TestCase(MouseButton.Right, false, false, false)]
	[TestCase(MouseButton.Middle, false, false, false)]
	public async Task GridColumnHeaderDragDropTest(MouseButton mouseButton, bool dragStartedOnMouseDown, bool dragStartedOnMouseMove, bool droppedOnMouseUp)
	{
		var expected = (dragStartedOnMouseDown, dragStartedOnMouseMove, droppedOnMouseUp);
		var actualDragStart = false;
		var actualDragDrop = false;

		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var grid = CreateNewZGrid<ZGridForTest>();
			grid.DragStart += (s, e) =>
			{
				if (actualDragStart)
				{
					Assert.Fail("OnDragStart should NOT be called more than once.");
				}
				actualDragStart = true;
			};

			grid.DragDrop += (s, e) =>
			{
				if (!actualDragStart)
				{
					Assert.Fail("OnDragDrop should NOT be called if drag has not been started.");
				}

				if (actualDragDrop)
				{
					Assert.Fail("OnDragDrop should NOT be called more than once.");
				}
				actualDragDrop = true;
			};

			return grid;
		});

		var headers = page.Locator("thead tr th");

		await headers.GetByText("One Header").HoverAsync();
		await MouseDownAsync(new MouseDownOptions { Button = mouseButton });
		Assert.That(actualDragStart, Is.EqualTo(expected.dragStartedOnMouseDown));

		await headers.GetByText("Two Header").HoverAsync();
		await WaitForTheServer();
		Assert.That(actualDragStart, Is.EqualTo(expected.dragStartedOnMouseMove));

		await MouseUpAsync(new MouseUpOptions { Button = mouseButton });
		Assert.That(actualDragDrop, Is.EqualTo(expected.droppedOnMouseUp));
	}

	[Test, WithPlaywrightPage]
	public async Task TestErrorMessageWhenNoDataBound()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>  new ZGrid());

		var dropdownMenu = page.Locator(".datagrid__columns_dropdown");
		await dropdownMenu.ClickAsync();

		var errorMessage = "Grid is not bound to data.";
		var hasError = UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(errorMessage);
		Assert.That(hasError, Is.False);
	}

	[Test, WithPlaywrightPage]
	public async Task RenderZTextBoxWithCustomCellFont()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			var dummy1 = collection.AddNew();
			dummy1.Z0_Description = "This text should be bold";
			var form = new WinzorTestForm();
			var grid = new ZGrid { Dock = DockStyle.Fill };
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Description, 80) { CharacterCasing = CharacterCasing.Normal });
			grid.FontDeciding += (sender, e) => e.Font = new Font(e.OriginalFont, System.Drawing.FontStyle.Bold);
			form.Controls.Add(grid);
			grid.SetDataBinding(collection, "");
			return form;
		});

		var textBoxCell = page.Locator("tr:first-child > td:nth-child(2)");
		await Assertions.Expect(textBoxCell).ToHaveCSSAsync("font-weight", "700");
	}

	[Test, WithPlaywrightPage]
	public async Task CurrentEditingContentShouldBeSavedWhenSwitchingToAnotherCellInGrid()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(500, 500) };
			var grid1 = CreateNewZGrid<ZGrid>(3);
			var grid2 = CreateNewZGrid<ZGrid>(3);

			grid1.Height = 200;
			grid1.Dock = DockStyle.Top;
			grid2.Height = 200;
			grid2.Dock = DockStyle.Top;
			form.Controls.Add(grid1);
			form.Controls.Add(grid2);

			return form;
		});

		var editCell = page.Locator("div:nth-child(2) > .datagrid > .datagrid__container > table > tbody > tr:nth-child(4) > td:nth-child(2)");
		await editCell.ClickAsync();

		var input = editCell.GetByRole(AriaRole.Textbox);
		await input.ClickAsync();
		await input.PressAsync("A");

		await page.Locator("tr:nth-child(4) > td:nth-child(2)").First.ClickAsync();
		await Assertions.Expect(editCell).ToHaveTextAsync("A");
	}

	[Test, WithPlaywrightPage]
	public async Task ZGridSortByFirstColumn()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var dataGrid = CreateNewZGrid<ZGridForTest>(20);
			dataGrid.ReadOnly = true;
			dataGrid.IsWholeRowSelectedOnClick = true;
			return dataGrid;
		});

		var columnHeader = page.Locator(".datagrid table thead th:nth-of-type(2)");

		await columnHeader.ClickAsync();
		await Assertions.Expect(columnHeader).ToHaveClassAsync("datagrid__header--ascending");
	}

	[Test, WithPlaywrightPage]
	public async Task NotificationIconShouldPropagationMouseDownToGrid()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZGridForTestNotificationIcon grid = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			var dummy1 = collection.AddNew();
			dummy1.Z0_Description = "One";
			dummy1.Z0_Bool = true;
			dummy1.SuspendValidationTesting();
			dummy1.Z0_BoolInfo.AddWarning("Warning!");
			var form = new WinzorTestForm();
			grid = new ZGridForTestNotificationIcon { Dock = DockStyle.Fill };
			grid.ReadOnly = false;
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Description, 80) { CharacterCasing = CharacterCasing.Normal });
			grid.ColumnStyles.Add(new ZCheckBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Bool, 80) { IsReadOnly = false });
			form.Controls.Add(grid);
			grid.SetDataBinding(collection, "");
			return form; 
		});

		var warnTdCell = page.Locator("td.datagrid__cell--warning");
		await Assertions.Expect(warnTdCell).ToHaveCSSAsync("background-color", "rgba(0, 0, 0, 0)");

		var warnIcon = page.Locator(".notification.notification--warning").First;
		await Assertions.Expect(warnIcon).ToHaveCSSAsync("background-color", "rgba(0, 0, 0, 0)");

		await warnIcon.ClickAsync();
		Assert.That(() => grid.IsMouseDown, Is.True.After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task ZGridHandleDeleteShouldNotThrowExceptionWhenDataSourceNull()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZGridForTest dataGrid = null;
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			dataGrid = CreateNewZGrid<ZGridForTest>(1);
			dataGrid.ReadOnly = true;
			dataGrid.IsWholeRowSelectedOnClick = true;
			dataGrid.RowsDeleting += (s, e) => dataGrid.DataSource = null;
			return dataGrid;
		});

		var grid = page.Locator(".datagrid");
		await grid.WaitForAsync();
		Assert.That(dataGrid, Is.Not.Null);
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			dataGrid.Select(0);
			var deleteTaskMenuItem = dataGrid.ContextMenu.MenuItems.FindByText("Delete");
			Assert.That(deleteTaskMenuItem, Is.Not.Null);
			Assert.DoesNotThrow(() => deleteTaskMenuItem.PerformClick());
		});
	}

	[Test, WithPlaywrightPage]
	public async Task TestClickBlankInGridDoesNotChangeBoundsOfDropEditControlUntilAfterItIsHidden()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		ZGridForTest grid = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			var table = new DataTable("data");

			var column1 = new DataColumn("one");
			column1.DataType = typeof(string);
			table.Columns.Add(column1);

			var column2 = new DataColumn("two");
			column2.DataType = typeof(string);
			table.Columns.Add(column2);

			var column3 = new DataColumn("three");
			column3.DataType = typeof(string);
			table.Columns.Add(column3);

			for (var i = 0; i < 3; i++)
			{
				table.Rows.Add($"cell({i + 1}, 1)", $"cell({i + 1}, 2)", $"cell({i + 1}, 3)");
			}

			var dataGrid = new ZGridForTest();
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnInfo = new ZTextBoxColumnStyleInfo();
			var dropEditColumnInfo = new ZDropEditColumnStyleInfo();

			var columnStyle1 = new ZTextBoxColumnStyle(columnInfo) { MappingName = "one", HeaderText = "One Header" };
			columnStyle1.Width = 80;
			tableStyle.GridColumnStyles.Add(columnStyle1);

			var columnStyle2 = new ZTextBoxColumnStyle(columnInfo) { MappingName = "two", HeaderText = "Two Header" };
			columnStyle2.Width = 180;
			tableStyle.GridColumnStyles.Add(columnStyle2);

			var columnStyle3 = new ZDropEditColumnStyle(dropEditColumnInfo) { MappingName = "three", HeaderText = "Three Header" };
			columnStyle3.Width = 180;
			tableStyle.GridColumnStyles.Add(columnStyle3);

			dataGrid.TableStyles.Add(tableStyle);
			dataGrid.Columns.AddTextColumn("One Header", 80);
			dataGrid.Columns.AddTextColumn("Two Header", 180);
			dataGrid.Columns.AddTextColumn("Three Header", 180);

			var form = new Form() { Size = new Size(500, 500) };

			grid = dataGrid;

			form.Controls.Add(dataGrid);
			return form;
		});

		var editCell = page.Locator("div:nth-child(1) > .datagrid > .datagrid__container > table > tbody > tr:nth-child(2) > td:nth-child(4)");
		await editCell.ClickAsync();

		var input = editCell.GetByRole(AriaRole.Textbox);
		await input.ClickAsync();
		await input.FillAsync("TEST");

		var initialBoundsOfEditControl = grid.LastFocusedColumn.EditControl.Bounds;
		var postValidationBoundsOfEditControl = Rectangle.Empty;
		grid.Validated += (s, e) =>
		{
			postValidationBoundsOfEditControl = grid.LastFocusedColumn.EditControl.Bounds;
		};

		// Click the center of the datagrid control which should not have any cells due to the size of datagrid
		var gridLocator = page.Locator(".datagrid");
		var pos = await gridLocator.BoundingBoxAsync();
		await page.Mouse.ClickAsync(pos.X + pos.Width / 2, pos.Y + pos.Height / 2);

		Assert.That(() => grid.LastFocusedColumn.EditControl.Bounds, Is.EqualTo(Rectangle.Empty).After(1000, 100));

		Assert.That(initialBoundsOfEditControl, Is.Not.EqualTo(Rectangle.Empty));
		Assert.That(postValidationBoundsOfEditControl, Is.Not.EqualTo(Rectangle.Empty));
		Assert.That(initialBoundsOfEditControl, Is.EqualTo(postValidationBoundsOfEditControl));
	}

	T CreateNewZGrid<T>(int rowCount = 50) where T : ZGrid, new()
	{
		var table = new DataTable("data");

		var column1 = new DataColumn("one");
		column1.DataType = typeof(string);
		table.Columns.Add(column1);

		var column2 = new DataColumn("two");
		column2.DataType = typeof(string);
		table.Columns.Add(column2);

		var column3 = new DataColumn("three");
		column3.DataType = typeof(string);
		table.Columns.Add(column3);

		for (var i = 0; i < rowCount; i++)
		{
			table.Rows.Add($"cell({i + 1}, 1)", $"cell({i + 1}, 2)", $"cell({i + 1}, 3)");
		}

		var dataGrid = new T();
		dataGrid.Dock = DockStyle.Fill;
		dataGrid.DataSource = table;
		dataGrid.AllowNavigation = false;
		var tableStyle = new DataGridTableStyle { MappingName = "data" };
		var columnInfo = new ZTextBoxColumnStyleInfo();

		var columnStyle1 = new ZTextBoxColumnStyle(columnInfo) { MappingName = "one", HeaderText = "One Header" };
		columnStyle1.Width = 80;
		tableStyle.GridColumnStyles.Add(columnStyle1);

		var columnStyle2 = new ZTextBoxColumnStyle(columnInfo) { MappingName = "two", HeaderText = "Two Header" };
		columnStyle2.Width = 180;
		tableStyle.GridColumnStyles.Add(columnStyle2);

		var columnStyle3 = new ZTextBoxColumnStyle(columnInfo) { MappingName = "three", HeaderText = "Three Header" };
		columnStyle3.Width = 180;
		tableStyle.GridColumnStyles.Add(columnStyle3);

		dataGrid.TableStyles.Add(tableStyle);
		dataGrid.Columns.AddTextColumn("One Header", 80);
		dataGrid.Columns.AddTextColumn("Two Header", 180);
		dataGrid.Columns.AddTextColumn("Three Header", 180);

		return dataGrid;
	}

	//fixed in WI00696277, where we made the cell change merely on focus in instead of mousedown
	async Task SimulateClickOnCellForRowChange(IElement element)
	{
		await element.TriggerEventAsync("onwinzorfocusin", new WinzorFocusInEventArgs());
	}

	async Task WaitForTheServer()
	{
		await Task.Delay(TimeSpan.FromMilliseconds(350));
	}

	async Task MouseDownAsync(MouseDownOptions options = default)
	{
		await Page.Mouse.DownAsync(options);
		await WaitForTheServer();
	}

	async Task ElementMouseMoveWithDelayAsync(ILocator locator, int x, int y, int steps = 10)
	{
		var elementRect = await locator.BoundingBoxAsync();
		await Page.Mouse.MoveAsync(elementRect.X + x, elementRect.Y + y, new () { Steps = steps });
		await WaitForTheServer();
	}

	async Task MouseUpAsync(MouseUpOptions options = default)
	{
		await Page.Mouse.UpAsync(options);
		await WaitForTheServer();
	}

	async Task SelectRow(ILocator row)
	{
		await ElementMouseMoveWithDelayAsync(row, 5, 5);
		await MouseDownAsync();
		await MouseUpAsync();
	}

	class ZCheckBoxColumnStyleInfoForTest : ZCheckBoxColumnStyleInfo
	{
		internal bool isCurrentCellReadOnly;

		public ZCheckBoxColumnStyleInfoForTest(string columnName, int width, bool isCurrentCellReadOnly) : base(columnName, width)
		{
			this.isCurrentCellReadOnly = isCurrentCellReadOnly;
		}

		public override Type ColumnStyleType => typeof(ZCheckBoxColumnStyleForTest);
	}

	class ZCheckBoxColumnStyleForTest : ZCheckBoxColumnStyle
	{
		public ZCheckBoxColumnStyleForTest(ZCheckBoxColumnStyleInfoForTest columnInfo) : base(columnInfo)
		{
		}

		protected override bool IsCellReadOnlyCore(object current)
		{
			return ((ZCheckBoxColumnStyleInfoForTest)ColumnInfo).isCurrentCellReadOnly;
		}
	}

	class ZGridForTest : ZGrid
	{
		public int Count { get; set; }

		public int SetDataGridRowsCount { get; set; }

		protected override int RowUnderMouse()
		{
			Count++;
			return base.RowUnderMouse();
		}

		protected override void OnAfterSetDataGridRows()
		{
			SetDataGridRowsCount++;
			base.OnAfterSetDataGridRows();
		}
	}

	class DummyGridBussinessObject : DummyBusinessObject
	{
		public DummyGridBussinessObject(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		public String RefreshDataField
		{
			get
			{
				if (!attributesInitialised)
				{
					attributesInitialised = true;
					ActiveBusinessObjectCollection<DummyGridBussinessObject>.RefreshAll(Factory);
				}
				return refreshDataField;
			}
			set
			{
				refreshDataField = value;
			}
		}

		public String AddChildDataField
		{
			get
			{
				if (string.IsNullOrEmpty(addCgukdDataField))
				{
					return string.Empty;
				}
				IBusinessObjectState child = this;
				child.IncrementReadOnlyIncludingChildren();
				RegisterEditableChildObject(new DummyBusinessObjectCollection(new BusinessObjectFactory()));
				child.DecrementReadOnlyIncludingChildren();
				return addCgukdDataField;
			}
			set
			{
				addCgukdDataField = value;
			}
		}

		bool attributesInitialised;
		String refreshDataField;
		String addCgukdDataField;
	}

	class DummyGridBussinessObjectCollection : BusinessObjectCollection<DummyGridBussinessObject>
	{
		public DummyGridBussinessObjectCollection(BusinessObjectFactory factory) : base(factory) { }
	}

	class DummyActiveBusinessObjectCollection : ActiveBusinessObjectCollection<DummyGridBussinessObject>
	{
		public DummyActiveBusinessObjectCollection(BusinessObjectFactory factory) : base(factory) { }
	}

	class ZGridForTestNotificationIcon : ZGrid
	{
		public bool IsMouseDown { get; set; }

		protected override void OnMouseDown(MouseEventArgs e)
		{
			IsMouseDown = true;
		}
	}

	class ZGridWithCustomScheme : ZGrid
	{
		public override GridColourScheme GetLastUsedColourSchemeForCurrentUser => SchemeForTest;

		public GridColourScheme SchemeForTest { get; set; }

		public FilterStripControlForTest StripControl
		{
			get
			{
				if (fStripControl == null)
				{
					var bo = new FilterStripBusinessObjectForTest();
					var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
					fStripControl = new FilterStripControlForTest(collection, bo);
				}
				return fStripControl;
			}
		}
		FilterStripControlForTest fStripControl;

		public void NotifyRenderRequiredExposed(bool invalidated = false)
		{
			base.NotifyRenderRequired(invalidated);
		}
	}
}
