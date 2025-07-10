using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Bunit;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.GUI;
using Enterprise.Main.ModuleTreeLoader;
using Enterprise.MasterFiles.Business;
using Enterprise.Startup;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using WTG.RtfConverter;
using WTG.ToxicNetworkEffect;
using static WTG.PlaywrightTesting.PlaywrightTestContext;
using HorizontalAlignment = System.Windows.Forms.HorizontalAlignment;

namespace Enterprise.Winzor.Architecture.Test;

class DataGridTest
{
	const string ProxyListen = "127.0.0.1:9000";
	const string ProxyUpstream = "127.0.0.1:5000";
	const string ProxyName = "test-proxy";

	[Test]
	public async Task DataBinding()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await TestGridAsync<ZGrid>(ctx, () =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var child1 = factory.New<DummyBaseBusinessObject>();
			child1.Z0_Code = "BAR";
			child1.Z0_Description = "This is the bar";
			dummyBizo.Collection.Add(child1);
			var child2 = factory.New<DummyBaseBusinessObject>();
			child2.Z0_Code = "FOO";
			child2.Z0_Description = "This is some foo";
			dummyBizo.Collection.Add(child2);
			return dummyBizo;
		}, CharacterCasing.Normal);

		var tr = rendered.FindAll("tbody>tr");
		Assert.That(tr, Has.Count.EqualTo(3));
		Assert.That(tr[0].ChildNodes, Has.Length.EqualTo(4));
		Assert.That(tr[0].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));
		var input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		Assert.That(input.Value, Is.EqualTo("BAR"));
		Assert.That(tr[0].ChildNodes[1].Contains(input), Is.True);
		Assert.That(tr[0].ChildNodes[2].TextContent, Is.EqualTo("This is the bar"));
		Assert.That(tr[1].ChildNodes, Has.Length.EqualTo(4));
		Assert.That(tr[1].ChildNodes[0].TextContent, Is.Empty);
		Assert.That(tr[1].ChildNodes[1].TextContent, Is.EqualTo("FOO"));
		Assert.That(tr[1].ChildNodes[2].TextContent, Is.EqualTo("This is some foo"));
		Assert.That(tr[2].ChildNodes[0].TextContent, Is.EqualTo("*"));
	}

	class DummyForHTMLTest : DummyBaseBusinessObject
	{
		public DummyForHTMLTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZBlob Z0_AddInfo_HTML
		{
			get
			{
				return ORtfTextUtil.RtfToHtml(ZBlob.FromUTF8(Z0_AddInfo));
			}

			set
			{
				var htmlToRtfConverter = new HtmlToRtfConverter();
				Z0_AddInfo = htmlToRtfConverter.Convert(value.ToUTF8());
			}
		}
	}

	class DummyCollectionForHTMLTest : BusinessObjectCollection<DummyForHTMLTest>
	{
		public DummyCollectionForHTMLTest(BusinessObjectFactory factory) : base(factory)
		{
		}
	}

	[Test, WithPlaywrightPage]
	public async Task RapidSwitchAfterDeleteContentNotCross()
	{
		ZRichTextBox richTextBox = null;
		ZGrid grid = null;
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = new DummyCollectionForHTMLTest(factory);
			var child1 = factory.New<DummyForHTMLTest>();
			child1.Z0_Code = "BAR";
			child1.Z0_AddInfo = "This is the bar";
			dummyBizo.Add(child1);
			var child2 = factory.New<DummyForHTMLTest>();
			child2.Z0_Code = "FOO";
			child2.Z0_AddInfo = "This is some foo";
			dummyBizo.Add(child2);
			var child3 = factory.New<DummyForHTMLTest>();
			child3.Z0_Code = "ZOO";
			child3.Z0_AddInfo = "This is some zoo";
			dummyBizo.Add(child3);

			var form = new WinzorTestForm() { Height = 1000, Width = 800 };
			grid = new ZGrid() { Height = 500, Width = 800 };
			form.BindingSource.DataSourceType = typeof(DummyCollectionForHTMLTest);
			form.BindingSource.SetBindingMember(grid, ".");
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 200));
			grid.LayoutCategoryPK = System.Guid.NewGuid();
			grid.Dock = DockStyle.Top;
			form.Controls.Add(grid);

			richTextBox = new ZRichTextBox()
			{
				Height = 500,
				Width = 800,
			};

			Assert.That(richTextBox, Is.Not.Null);

			form.BindingSource.SetBindingMember(richTextBox, "Z0_AddInfo");

			richTextBox.Dock = DockStyle.Bottom;
			form.Controls.Add(richTextBox);

			form.DataSourceType = typeof(DummyBusinessObject);
			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		await page.Locator("table").WaitForAsync();
		var trs = page.Locator("tbody tr");
		await SelectRow(trs.Nth(0));
		var client = await RichTextBoxClient.GetClientAsync(richTextBox.RichEdit);
		Assert.That(await client.GetActiveTextAsync(), Is.EqualTo("This is the bar"));

		await client.FocusEditorAsync();
		await page.Keyboard.TypeAsync("this need remove");
		await page.Keyboard.PressAsync("Enter");

		await SelectRow(trs.Nth(1));
		Assert.That(await client.GetActiveTextAsync(), Is.EqualTo("This is some foo"));
		await SelectRow(trs.Nth(0));
		Assert.That(await client.GetActiveTextAsync(), Is.EqualTo("this need remove\nThis is the bar"));
		await SelectRow(trs.Nth(1));
		Assert.That(await client.GetActiveTextAsync(), Is.EqualTo("This is some foo"));
	}

	async Task SelectRow(ILocator row)
	{
		var firstCell = row.Locator("td:nth-child(2)");
		await firstCell.ClickAsync();
		await Task.Delay(500);
	}

	[Test]
	public async Task EditBizoProperty()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await TestGridAsync<ZGrid>(ctx, () =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var child = factory.New<DummyBaseBusinessObject>();
			child.Z0_Code = "FOO";
			child.Z0_Description = "This is some foo";
			dummyBizo.Collection.Add(child);
			return dummyBizo;
		});

		await rendered.FindAll("tbody>tr")[0].Children[1].MouseDownAsync(new WebMouseEventArgs());
		var input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		Assert.That(input.Value, Is.EqualTo("FOO"));
		await rendered.KeyPressAsync(Keys.A, input);
		input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		await input.TriggerEventAsync("oninput", new ChangeEventArgs { Value = "BAR" });
		await input.FocusOutAsync();
		Assert.That(rendered.FindAll("tbody>tr")[0].ChildNodes[1].TextContent, Is.EqualTo("BAR"));
	}

	[Test]
	public async Task EditBizoPropertyWithZCodeFindBox()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			var grid = new ZGrid();
			form.BindingSource.SetBindingMember(grid, "Collection");
			var columnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			columnStyleInfo.ModuleID = DummyModuleIDs.Dummy;
			columnStyleInfo.ColumnName = "Z0_Code";
			columnStyleInfo.BindToList = "Lookups+DummyList";
			grid.ColumnStyles.Add(columnStyleInfo);
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var child = factory.New<DummyEnterpriseBusinessObject>();
			child.Z0_Code = "FOO";
			child.Z0_Description = "This is some foo";
			dummyBizo.Collection.Add(child);
			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		await rendered.FindAll("tbody>tr")[0].Children[1].MouseDownAsync(new WebMouseEventArgs());
		var input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		Assert.That(input.Value, Is.EqualTo("FOO"));
		await input.TriggerEventAsync("oninput", new ChangeEventArgs { Value = "BAR" });
		await input.FocusOutAsync();
		Assert.That(rendered.FindAll("tbody>tr")[0].ChildNodes[1].TextContent, Is.EqualTo("BAR"), rendered.Markup);
	}

	[WithTransaction]
	[Test, ExpectNoExceptions]
	public async Task DataGridShouldNotCreateRenderedContentWhenGridIsDisposed()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(factory);
			Assert.That(config.BufferBoard, Is.Not.Null);
			factory.Save();
			var form = new MainForm();

			form.OpenModule(new MainFormModule(ModuleIDs.BMBoard), false);
			var module = ZCurrentModules.Instance.GetCurrentModule(ModuleIDs.BMBoard);
			return form;
		});
		await rendered.Find("button[title=\"Find\"]").ClickAsync(new WebMouseEventArgs());

		var tr = rendered.FindAll("tbody>tr");
		Assert.That(tr, Has.Count.EqualTo(1));
		await rendered.Find("div[data-name=\"HomeButtonPanel\"] > button").ClickAsync(new WebMouseEventArgs());
	}

	[Test]
	public async Task EditMultipleBizoPropertiesWithZCodeFindBox()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			var grid = new ZGrid();
			form.BindingSource.SetBindingMember(grid, "Collection");
			var columnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			columnStyleInfo1.ModuleID = DummyModuleIDs.Dummy;
			columnStyleInfo1.ColumnName = "Z0_Code";
			columnStyleInfo1.BindToList = "Lookups+DummyList";
			grid.ColumnStyles.Add(columnStyleInfo1);
			var columnStyleInfo2 = new ZCodeFindBoxColumnStyleInfo();
			columnStyleInfo2.ModuleID = DummyModuleIDs.Dummy;
			columnStyleInfo2.ColumnName = "Z0_Description";
			columnStyleInfo2.BindToList = "Lookups+DummyList";
			grid.ColumnStyles.Add(columnStyleInfo2);
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var child = factory.New<DummyEnterpriseBusinessObject>();
			child.Z0_Code = "FOO";
			child.Z0_Description = "This is some foo";
			dummyBizo.Collection.Add(child);
			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		await SimulateClickOnCellForRowChange(rendered.FindAll("tbody>tr")[0].Children[1]);
		var input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		Assert.That(input.Value, Is.EqualTo("FOO"));
		await input.FocusOutAsync();

		await SimulateClickOnCellForRowChange(rendered.FindAll("tbody>tr")[0].Children[2]);
		input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		Assert.That(input.Value, Is.EqualTo("This is some foo").IgnoreCase, rendered.Markup);
		await input.FocusOutAsync();

		await SimulateClickOnCellForRowChange(rendered.FindAll("tbody>tr")[0].Children[1]);
		input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		Assert.That(input.Value, Is.EqualTo("FOO"));
	}

	[Test]
	public async Task AddNewBizo()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await TestGridAsync<ZGrid>(ctx, () =>
		{
			var factory = new BusinessObjectFactory();
			return factory.New<DummyBusinessObject>();
		});

		Assert.That(rendered.FindAll("tbody").Count, Is.EqualTo(1).After(2000, 100), "Table body is not rendered.");

		await rendered.FindAll("tbody>tr")[0].Children[1].MouseDownAsync(new WebMouseEventArgs());
		var input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		Assert.That(input.Value, Is.Empty);
		await rendered.KeyPressAsync(Keys.A, input); // Need to simulate keypress for column to begin editing
		await rendered.Find("input").TriggerEventAsync("oninput", new ChangeEventArgs { Value = "FOO" });
		await input.FocusOutAsync();
		Assert.That(rendered.FindAll("tbody>tr")[0].ChildNodes[1].TextContent, Is.EqualTo("FOO"));
		Assert.That(rendered.FindAll("tbody>tr")[1].ChildNodes[0].TextContent, Is.EqualTo("*"));
	}

	[Test]
	public async Task AddNewAndCancel()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await TestGridAsync<ZGrid>(ctx, () =>
		{
			var factory = new BusinessObjectFactory();
			return factory.New<DummyBusinessObject>();
		});

		Assert.That(rendered.FindAll("tbody").Count, Is.EqualTo(1).After(2000, 100), "Table body is not rendered.");
		
		await rendered.FindAll("tbody>tr")[0].Children[1].MouseDownAsync(new WebMouseEventArgs());
		var input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		Assert.That(input.Value, Is.Empty);
		await input.FocusOutAsync();
		Assert.That(rendered.FindAll("tbody>tr")[0].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));
	}

	[Test]
	public async Task DataGridAddNewRowOnInput()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await TestGridAsync<ZGrid>(ctx, () =>
		{
			var factory = new BusinessObjectFactory();
			return factory.New<DummyBusinessObject>();
		});

		await rendered.FindAll("tbody>tr")[0].Children[1].MouseDownAsync(new WebMouseEventArgs());
		var input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		await rendered.KeyPressAsync(Keys.A, input);
		Assert.That(rendered.FindAll("tbody>tr").Count, Is.EqualTo(2));
	}

	[Test]
	public async Task AllowAddFalse()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await TestGridAsync<ZGrid>(ctx, () =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			dummyBizo.Collection.SetAllowNew(false);
			return dummyBizo;
		});

		Assert.That(rendered.FindAll("tbody>tr"), Is.Empty);
	}

	[Test]
	[WithTransaction]
	public async Task ClickHeaderToSortSingleColumn()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await TestGridAsync<ZGrid>(ctx, () =>
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
			return dummyBizo;
		});

		await rendered.Find("input").FocusOutAsync();

		var thSpan = rendered.FindAll("th span");
		var th = rendered.FindAll("th");
		Assert.That(th, Has.Count.EqualTo(4));
		Assert.That(th[1].TextContent, Is.EqualTo("Code"));
		Assert.That(th[2].TextContent, Is.EqualTo("Description"));
		Assert.That(th[3].TextContent, Is.EqualTo("Decimal"));

		await thSpan[0].MouseDownAsync(new WebMouseEventArgs());
		await thSpan[0].MouseUpAsync(new WebMouseEventArgs());
		DataGridTestHelper.AssertGridSort(rendered, new string[] { "datagrid__header--ascending", string.Empty, string.Empty },
			new string[] { "1", "6", "8", "2", "5", "9", "3", "5", "7" });
		await thSpan[0].MouseDownAsync(new WebMouseEventArgs());
		await thSpan[0].MouseUpAsync(new WebMouseEventArgs());
		DataGridTestHelper.AssertGridSort(rendered, new string[] { "datagrid__header--descending", string.Empty, string.Empty },
			new string[] { "3", "5", "7", "2", "5", "9", "1", "6", "8" });
		await thSpan[2].MouseDownAsync(new WebMouseEventArgs());
		await thSpan[2].MouseUpAsync(new WebMouseEventArgs());
		DataGridTestHelper.AssertGridSort(rendered, new string[] { string.Empty, string.Empty, "datagrid__header--ascending" },
			new string[] { "3", "5", "7", "1", "6", "8", "2", "5", "9" });
		await thSpan[2].MouseDownAsync(new WebMouseEventArgs());
		await thSpan[2].MouseUpAsync(new WebMouseEventArgs());
		DataGridTestHelper.AssertGridSort(rendered, new string[] { string.Empty, string.Empty, "datagrid__header--descending" },
			new string[] { "2", "5", "9", "1", "6", "8", "3", "5", "7" });
	}

	[Test]
	[WithTransaction]
	public async Task ClickHeaderToMultiSort()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await TestGridAsync<ZGrid>(ctx, () =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var child1 = factory.New<DummyBaseBusinessObject>();
			child1.Z0_Code = "1";
			child1.Z0_Description = "6";
			child1.Z0_Decimal = 8;
			dummyBizo.Collection.Add(child1);
			var child2 = factory.New<DummyBaseBusinessObject>();
			child2.Z0_Code = "1";
			child2.Z0_Description = "5";
			child2.Z0_Decimal = 9;
			dummyBizo.Collection.Add(child2);
			var child3 = factory.New<DummyBaseBusinessObject>();
			child3.Z0_Code = "1";
			child3.Z0_Description = "5";
			child3.Z0_Decimal = 7;
			dummyBizo.Collection.Add(child3);
			return dummyBizo;
		});

		var th = rendered.FindAll("th");
		Assert.That(th, Has.Count.EqualTo(4));
		Assert.That(th[1].TextContent, Is.EqualTo("Code"));
		Assert.That(th[2].TextContent, Is.EqualTo("Description"));
		Assert.That(th[3].TextContent, Is.EqualTo("Decimal"));

		await th[1].MouseDownAsync(new WebMouseEventArgs());
		await th[1].MouseUpAsync(new WebMouseEventArgs());
		Assert.That(rendered.FindAll("th")[1].Attributes["class"].Value, Is.EqualTo("datagrid__header--ascending"));
		Assert.That(rendered.FindAll("th")[2].Attributes["class"].Value, Is.Empty);
		Assert.That(rendered.FindAll("th")[3].Attributes["class"].Value, Is.Empty);

		await th[2].MouseDownAsync(new WebMouseEventArgs() { ShiftKey = true });
		await th[2].MouseUpAsync(new WebMouseEventArgs() { ShiftKey = true });
		DataGridTestHelper.AssertGridSort(rendered, new string[] { "datagrid__header--ascending", "datagrid__header--ascending", string.Empty },
			new string[] { "1", "5", "9", "1", "5", "7", "1", "6", "8" });

		await th[2].MouseDownAsync(new WebMouseEventArgs() { ShiftKey = true });
		await th[2].MouseUpAsync(new WebMouseEventArgs() { ShiftKey = true });
		DataGridTestHelper.AssertGridSort(rendered, new string[] { "datagrid__header--ascending", "datagrid__header--descending", string.Empty },
			new string[] { "1", "6", "8", "1", "5", "9", "1", "5", "7" });

		await th[3].MouseDownAsync(new WebMouseEventArgs() { ShiftKey = true });
		await th[3].MouseUpAsync(new WebMouseEventArgs() { ShiftKey = true });
		DataGridTestHelper.AssertGridSort(rendered, new string[] { "datagrid__header--ascending", "datagrid__header--descending", "datagrid__header--ascending" },
			new string[] { "1", "6", "8", "1", "5", "7", "1", "5", "9" });

		await th[3].MouseDownAsync(new WebMouseEventArgs());
		await th[3].MouseUpAsync(new WebMouseEventArgs());
		DataGridTestHelper.AssertGridSort(rendered, new string[] { string.Empty, string.Empty, "datagrid__header--ascending" },
			new string[] { "1", "5", "7", "1", "6", "8", "1", "5", "9" });
	}

	[Test]
	public async Task ClickHeaderToSortEmptyGrid()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await TestGridAsync<ZGrid>(ctx, () =>
		{
			var factory = new BusinessObjectFactory();
			return factory.New<DummyBusinessObject>();
		});

		var th = rendered.FindAll("th span");
		Assert.DoesNotThrowAsync(async () => await th[1].MouseDownAsync(new WebMouseEventArgs() { ShiftKey = true }));
		Assert.DoesNotThrowAsync(async () => await th[2].MouseDownAsync(new WebMouseEventArgs() { ShiftKey = true }));
		Assert.That(rendered.FindAll("th")[1].Attributes["class"].Value, Is.Empty);
		Assert.That(rendered.FindAll("th")[2].Attributes["class"].Value, Is.Empty);
	}

	[Test]
	[WithTransaction]
	public async Task GridColumnSizeChanged()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await TestGridAsync<ZGrid>(ctx, () =>
		{
			var factory = new BusinessObjectFactory();
			return factory.New<DummyBusinessObject>();
		});
		var form = (WinzorTestForm)rendered.Instance.Control;
		var grid = (ZGrid)form.Controls[0];

		Assert.That(rendered.FindAll("th")[1].Attributes["style"].Value, Does.Contain("width: 100px;"));
		Assert.That(rendered.FindAll("th")[2].Attributes["style"].Value, Does.Contain("width: 200px;"));
		Assert.That(rendered.FindAll("th")[3].Attributes["style"].Value, Does.Contain("width: 300px;"));

		await grid.SetColumnSize(0, 50);
		Assert.That(grid.TableStyles[0].GridColumnStyles[0].Width, Is.EqualTo(50));
		await grid.SetColumnSize(1, 40);
		Assert.That(grid.TableStyles[0].GridColumnStyles[1].Width, Is.EqualTo(40));
		await grid.SetColumnSize(2, 30);
		Assert.That(grid.TableStyles[0].GridColumnStyles[2].Width, Is.EqualTo(30));

		await grid.SetColumnSize(99, 30);
		await grid.SetColumnSize(-1, 30);

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			form.Text = "Force re-render";
		});

		Assert.That(rendered.FindAll("th")[1].Attributes["style"].Value, Does.Contain("width: 50px;"));
		Assert.That(rendered.FindAll("th")[2].Attributes["style"].Value, Does.Contain("width: 40px;"));
		Assert.That(rendered.FindAll("th")[3].Attributes["style"].Value, Does.Contain("width: 30px;"));
	}

	[Test]
	public async Task TestGridColumnResizeWithDropEdit()
	{
		using var ctx = new EnterpriseTestContext();
		ctx.JSInterop.Setup<object>("grid.resizeColumn", _ => true).SetResult(null);
		var module = ctx.JSInterop.SetupModule("/js/module/grid.js");
		module.Setup<object>("resizeColumn", _ => true).SetResult(null);
		DataTable table = null;
		ZGrid dataGrid = null;

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			table = new DataTable("data");
			var column1 = new DataColumn("one");
			column1.DataType = typeof(string);
			table.Columns.Add(column1);
			var column2 = new DataColumn("two");
			column2.DataType = typeof(string);
			table.Columns.Add(column2);
			var column3 = new DataColumn("three");
			column3.DataType = typeof(string);
			table.Columns.Add(column3);

			table.Rows.Add("1", "", "");
			table.Rows.Add("2", "", "");

			dataGrid = new ZGrid();
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "data" };

			var columnStyle1 = new ZTextBoxColumnStyle(new ZTextBoxColumnStyleInfo("one", 10))
			{
				MappingName = "one",
				HeaderText = "One Header",
				ReadOnly = true,
				CharacterCasing = CharacterCasing.Upper,
				Alignment = HorizontalAlignment.Left
			};
			tableStyle.GridColumnStyles.Add(columnStyle1);

			var columnStyle2 = new ZDropEditColumnStyle(new ZDropEditColumnStyleInfo("two", 20))
			{
				MappingName = "two",
				HeaderText = "Two Header",
				ReadOnly = false,
				CharacterCasing = CharacterCasing.Upper,
				Alignment = HorizontalAlignment.Left
			};
			tableStyle.GridColumnStyles.Add(columnStyle2);

			var columnStyle3 = new ZTextBoxColumnStyle(new ZTextBoxColumnStyleInfo("three", 30))
			{
				MappingName = "three",
				HeaderText = "Three Header",
				ReadOnly = true,
				CharacterCasing = CharacterCasing.Upper,
				Alignment = HorizontalAlignment.Left
			};
			tableStyle.GridColumnStyles.Add(columnStyle3);

			dataGrid.TableStyles.Add(tableStyle);

			return dataGrid;
		});

		var resizers = rendered.FindAll("th .datagrid__column_resizer");
		Assert.That(resizers, Has.Count.EqualTo(3));

		await SimulateClickOnCellForRowChange(rendered.FindAll("tbody>tr")[0].Children[2]);
		var input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();

		Assert.That(rendered.FindAll("th")[1].Attributes["style"].Value, Does.Contain("width: 10px;"));
		Assert.That(rendered.FindAll("th")[2].Attributes["style"].Value, Does.Contain("width: 20px;"));
		Assert.That(rendered.FindAll("th")[3].Attributes["style"].Value, Does.Contain("width: 30px;"));

		Assert.That(rendered.FindAll("tbody>tr")[0].Children[1].Contains(input), Is.False);
		Assert.That(rendered.FindAll("tbody>tr")[0].Children[2].Contains(input), Is.True);
		Assert.That(rendered.FindAll("tbody>tr")[0].Children[3].Contains(input), Is.False);
		Assert.That(input.Value, Is.EqualTo(""));
		Assert.That(input.ParentElement.GetAttribute("style"), Does.Contain("width:36px"));
		Assert.That(input.ParentElement.GetAttribute("style"), Does.Contain("height:15px"));
		Assert.That(rendered.FindAll("tbody>tr")[0].Children[2].GetAttribute("class"), Does.Contain("datagrid__cell--edit"));
		Assert.That(rendered.FindAll("tbody>tr")[0].GetAttribute("class"), Does.Contain("datagrid__row--edit"));

		Assert.DoesNotThrowAsync(async () => await dataGrid.SetColumnSize(1, 100));
		Assert.That(rendered.FindAll("th")[2].Attributes["style"].Value, Does.Contain("width: 100px;"));

		input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		Assert.That(rendered.FindAll("tbody>tr")[0].Children[1].Contains(input), Is.False);
		Assert.That(rendered.FindAll("tbody>tr")[0].Children[2].Contains(input), Is.True);
		Assert.That(rendered.FindAll("tbody>tr")[0].Children[3].Contains(input), Is.False);
		Assert.That(input.Value, Is.EqualTo(""));
		Assert.That(input.ParentElement.GetAttribute("style"), Does.Contain("width:116px"));
		Assert.That(input.ParentElement.GetAttribute("style"), Does.Contain("height:15px"));
		Assert.That(rendered.FindAll("tbody>tr")[0].Children[2].GetAttribute("class"), Does.Contain("datagrid__cell--edit"));
		Assert.That(rendered.FindAll("tbody>tr")[0].GetAttribute("class"), Does.Contain("datagrid__row--edit"));
	}

	[Test, WithPlaywrightPage]
	public async Task TestGridCellInEditModeAfterSortingOrResizing()
	{
		DataTable table = null;
		ZGrid dataGrid = null;
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm() { Width = 500, Height = 500 };

			table = new DataTable("data");
			var column1 = new DataColumn("one");
			column1.DataType = typeof(string);
			table.Columns.Add(column1);
			var column2 = new DataColumn("two");
			column2.DataType = typeof(string);
			table.Columns.Add(column2);
			var column3 = new DataColumn("three");
			column3.DataType = typeof(string);
			table.Columns.Add(column3);

			table.Rows.Add("1.1", "1.2", "1.3");
			table.Rows.Add("2.1", "2.2", "2.3");

			dataGrid = new ZGrid() { Width = 500, Height = 500 };
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowSorting = true;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "data" };

			var columnStyle1 = new ZTextBoxColumnStyle(new ZTextBoxColumnStyleInfo("one", 50))
			{
				MappingName = "one",
				HeaderText = "One Header",
				ReadOnly = false,
				CharacterCasing = CharacterCasing.Upper,
				Alignment = HorizontalAlignment.Left
			};
			tableStyle.GridColumnStyles.Add(columnStyle1);

			var columnStyle2 = new ZTextBoxColumnStyle(new ZTextBoxColumnStyleInfo("two", 60))
			{
				MappingName = "two",
				HeaderText = "Two Header",
				ReadOnly = false,
				CharacterCasing = CharacterCasing.Upper,
				Alignment = HorizontalAlignment.Left
			};
			tableStyle.GridColumnStyles.Add(columnStyle2);

			var columnStyle3 = new ZDropEditColumnStyle(new ZDropEditColumnStyleInfo("three", 70))
			{
				MappingName = "three",
				HeaderText = "Three Header",
				ReadOnly = false,
				CharacterCasing = CharacterCasing.Upper,
				Alignment = HorizontalAlignment.Left
			};
			tableStyle.GridColumnStyles.Add(columnStyle3);

			dataGrid.TableStyles.Add(tableStyle);

			form.Controls.Add(dataGrid);
			return form;
		});

		// sort by first column
		var columnHeader = await page.Locator(".datagrid table thead th:nth-of-type(2) span").BoundingBoxAsync();
		await SimulateClickEvent(page, columnHeader);
		Assert.That(async () => await page.Locator(".datagrid table thead th:nth-of-type(2)").GetAttributeAsync("class"), Does.Contain("datagrid__header--ascending").After(2000, 100));

		// click first cell to edit
		var row = await page.Locator(".datagrid tbody>tr:nth-of-type(1) td:nth-of-type(2)").BoundingBoxAsync();
		await SimulateClickEvent(page, row);

		Assert.That(async () => await page.Locator(".datagrid table thead th:nth-of-type(2)").GetComputedStyleAsync("width"), Is.EqualTo("50px").After(2000, 100));
		Assert.That(async () => await page.Locator(".datagrid table thead th:nth-of-type(3)").GetComputedStyleAsync("width"), Is.EqualTo("60px").After(2000, 100));
		Assert.That(async () => await page.Locator(".datagrid table thead th:nth-of-type(4)").GetComputedStyleAsync("width"), Is.EqualTo("70px").After(2000, 100));

		var inputs = await page.QuerySelectorAllAsync("input");
		Assert.That(() => inputs.Count, Is.EqualTo(1).After(2000, 100));
		Assert.That(async () => await page.Locator(".datagrid tbody>tr:nth-of-type(1) td:nth-of-type(2) input").InputValueAsync(), Is.EqualTo("1.1").After(2000, 100));
		Assert.That(async () => await page.Locator(".datagrid tbody>tr:nth-of-type(1) td:nth-of-type(2)").GetAttributeAsync("class"), Does.Contain("datagrid__cell--edit"));
		Assert.That(async () => await page.Locator(".datagrid tbody>tr:nth-of-type(1)").GetAttributeAsync("class"), Does.Contain("datagrid__row--edit"));

		// sort
		await SimulateClickEvent(page, columnHeader);
		Assert.That(async () => await page.Locator(".datagrid table thead th:nth-of-type(2)").GetAttributeAsync("class"), Does.Contain("datagrid__header--descending").After(2000, 100));
		inputs = await page.QuerySelectorAllAsync("input");
		Assert.That(() => inputs.Count, Is.EqualTo(1).After(2000, 100));
		Assert.That(async () => await page.Locator(".datagrid tbody>tr:nth-of-type(2) td:nth-of-type(2) input").InputValueAsync(), Is.EqualTo("1.1").After(2000, 100));
		Assert.That(async () => await page.Locator(".datagrid tbody>tr:nth-of-type(2) td:nth-of-type(2)").GetAttributeAsync("class"), Does.Contain("datagrid__cell--edit"));
		Assert.That(async () => await page.Locator(".datagrid tbody>tr:nth-of-type(2)").GetAttributeAsync("class"), Does.Contain("datagrid__row--edit"));

		// editing the last dropdown
		row = await page.Locator(".datagrid tbody>tr:nth-of-type(1) td:nth-of-type(4)").BoundingBoxAsync();
		await SimulateClickEvent(page, row);
		inputs = await page.QuerySelectorAllAsync("input");
		Assert.That(() => inputs.Count, Is.EqualTo(1).After(2000, 100));
		inputs = await page.QuerySelectorAllAsync(".form button");
		Assert.That(() => inputs.Count, Is.EqualTo(1).After(2000, 100));

		Assert.That(async () => await page.Locator(".datagrid tbody>tr:nth-of-type(1) td:nth-of-type(4) input").InputValueAsync(), Is.EqualTo("2.3").After(2000, 100));
		Assert.That(async () => await page.Locator(".datagrid tbody>tr:nth-of-type(1) td:nth-of-type(4) div.datagrid__control--edit > div").GetComputedStyleAsync("width"), Is.EqualTo("86px").After(2000, 100));
		Assert.That(async () => await page.Locator(".datagrid tbody>tr:nth-of-type(1) td:nth-of-type(4) div.datagrid__control--edit > div").GetComputedStyleAsync("height"), Is.EqualTo("15px").After(2000, 100));

		// test dropform
		var button = await page.Locator("button", new PageLocatorOptions { HasText = "keyboard_arrow_down" }).BoundingBoxAsync();
		await SimulateClickEvent(page, button);
		inputs = await page.QuerySelectorAllAsync(".zdropform table");
		Assert.That(inputs.Count, Is.GreaterThan(0));

		// resize
		var columnResizer = await page.QuerySelectorAsync(".datagrid table thead th:nth-of-type(4)");
		var bound = await columnResizer.BoundingBoxAsync();
		await MouseMoveOfElementAsync(page, columnResizer, bound.Width - 2, bound.Height / 2);
		await MouseDownAsync(page);
		await MouseMoveOfElementAsync(page, columnResizer, bound.Width + 98, bound.Height / 2);
		await MouseUpAsync(page);
		Assert.That(async () => await page.Locator(".datagrid table thead th:nth-of-type(4)").GetComputedStyleAsync("width"), Is.EqualTo("170px").After(2000, 100));

		inputs = await page.QuerySelectorAllAsync("input");
		Assert.That(() => inputs.Count, Is.EqualTo(1).After(2000, 100));
		inputs = await page.QuerySelectorAllAsync(".form button");
		Assert.That(() => inputs.Count, Is.EqualTo(1).After(2000, 100));

		Assert.That(async () => await page.Locator(".datagrid tbody>tr:nth-of-type(1) td:nth-of-type(4) input").InputValueAsync(), Is.EqualTo("2.3").After(2000, 100));
		Assert.That(async () => await page.Locator(".datagrid tbody>tr:nth-of-type(1) td:nth-of-type(4) div.datagrid__control--edit > div").GetComputedStyleAsync("width"), Is.EqualTo("186px").After(2000, 100));
		Assert.That(async () => await page.Locator(".datagrid tbody>tr:nth-of-type(1) td:nth-of-type(4) div.datagrid__control--edit > div").GetComputedStyleAsync("height"), Is.EqualTo("15px").After(2000, 100));

		// test dropform
		button = await page.Locator("button", new PageLocatorOptions { HasText = "keyboard_arrow_down" }).BoundingBoxAsync();
		await SimulateClickEvent(page, button);
		inputs = await page.QuerySelectorAllAsync(".zdropform table");
		Assert.That(inputs.Count, Is.GreaterThan(0));
	}

	[Test, WithPlaywrightPage]
	public async Task ZCodeFindBoxFindButtonIsVisible()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			var grid = new ZGrid() { Width = 300, Height = 300 };
			form.BindingSource.SetBindingMember(grid, "Collection");
			var columnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			columnStyleInfo.ModuleID = DummyModuleIDs.Dummy;
			columnStyleInfo.ColumnName = "Z0_Code";
			columnStyleInfo.BindToList = "Lookups+DummyList";
			grid.ColumnStyles.Add(columnStyleInfo);
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var child = factory.New<DummyEnterpriseBusinessObject>();
			child.Z0_Code = "FOO";
			child.Z0_Description = "This is some foo";
			dummyBizo.Collection.Add(child);
			form.SetDataBinding(dummyBizo, "");
			return form;
		});
		var table = await page.WaitForSelectorAsync("table");

		await page.WaitForSelectorAsync("td .textbox");
		Assert.That((await page.EvaluateAsync("document.querySelector('.datagrid__cell--edit').scrollLeft")).Value.GetInt16, Is.EqualTo(0));
		await (await page.QuerySelectorAsync("td button")).ClickAsync();
		// If we had to scroll to click on the button then it was not visible
		Assert.That((await page.EvaluateAsync("document.querySelector('.datagrid__cell--edit').scrollLeft")).Value.GetInt16, Is.EqualTo(0));
	}

	[Test, WithPlaywrightPage]
	public async Task EditCellZIndexShouldLessThanHeaderZIndex()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm();
			var grid = new ZGrid { Width = 300, Height = 300 };
			form.BindingSource.SetBindingMember(grid, "Collection");
			var columnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			columnStyleInfo1.ModuleID = DummyModuleIDs.Dummy;
			columnStyleInfo1.ColumnName = "Z0_Code";
			columnStyleInfo1.BindToList = "Lookups+DummyList";
			grid.ColumnStyles.Add(columnStyleInfo1);
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var child = factory.New<DummyEnterpriseBusinessObject>();
			dummyBizo.Collection.Add(child);
			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		var tr = page.Locator("table tbody tr").First;
		await tr.ClickAsync();

		var editCell = page.Locator("table > tbody > tr.datagrid__row--edit > td.datagrid__cell--edit");
		await editCell.WaitForAsync();
		var editCellZIndex = (await editCell.GetComputedStyleAsync("z-index")).Raw;

		var header = page.Locator("table > thead > tr > th").First;
		await header.WaitForAsync();
		var headerZIndex = (await header.GetComputedStyleAsync("z-index")).Raw;

		Assert.That(editCellZIndex, Is.LessThan(headerZIndex));
	}

	[Test, WithPlaywrightPage]
	public async Task ZCodeFindBoxFindButtonClick()
	{
		var clientServiceProvider = new MockCargoWiseClientSeviceProvider();
		var loadRequestSent = new TaskCompletionSource();
		clientServiceProvider.MockWindowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				loadRequestSent.SetResult();
			});
		await using var ctx = new InMemoryAppServerTestContext(clientServiceProvider);

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			var grid = new ZGrid() { Width = 300, Height = 300 };
			form.BindingSource.SetBindingMember(grid, "Collection");
			var columnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			columnStyleInfo.ModuleID = DummyModuleIDs.Dummy;
			columnStyleInfo.ColumnName = "Z0_Code";
			columnStyleInfo.BindToList = "Lookups+DummyList";
			grid.ColumnStyles.Add(columnStyleInfo);
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var child = factory.New<DummyEnterpriseBusinessObject>();
			child.Z0_Code = "FOO";
			child.Z0_Description = "This is some foo";
			dummyBizo.Collection.Add(child);
			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		await page.GetByRole(AriaRole.Button).ClickAsync();
		Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(30)), Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task TestWholeCharacterStringRenderedInCellAfterPressingKey()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var child1 = factory.New<DummyBaseBusinessObject>();
			var dummyBizo = factory.New<DummyBusinessObject>();
			dummyBizo.Collection.Add(child1);

			var grid = new ZGrid { Width = 200, Height = 200 };
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 100));
			grid.LayoutCategoryPK = Guid.NewGuid();

			var form = new WinzorTestForm { Width = 300, Height = 300 };
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			form.BindingSource.SetBindingMember(grid, "Collection");
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			form.SetDataBinding(dummyBizo, "");

			return form;
		});

		await page.WaitForSelectorAsync("table");

		await page.Locator("table>tbody>tr:nth-of-type(1)>td:nth-of-type(2)").ClickAsync();
		var input = await page.WaitForSelectorAsync("table>tbody>tr:nth-of-type(1)>td:nth-of-type(2) input");
		Assert.That(async () => await input.InputValueAsync(), Is.EqualTo(string.Empty).After(2000, 100));

		await page.Keyboard.PressAsync("Digit1");
		await page.Keyboard.PressAsync("Digit2");
		await page.Keyboard.PressAsync("Digit3");
		Assert.That(async () => await input.InputValueAsync(), Is.EqualTo("123").After(2000, 100));
	}

	[Test]
	public async Task ZGridColorAppliedToRows()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var child1 = factory.New<DummyBaseBusinessObject>();
			child1.Z0_Code = "1";
			child1.Z0_Description = "AAA";
			dummyBizo.Collection.Add(child1);
			var child2 = factory.New<DummyBaseBusinessObject>();
			child2.Z0_Code = "2";
			child2.Z0_Description = "ABC";
			dummyBizo.Collection.Add(child2);

			var form = new WinzorTestForm();
			var grid = new ZGrid();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			form.BindingSource.SetBindingMember(grid, "Collection");
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 100));
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Description", 200));
			grid.ColourDeciding += (sender, a) => a.Colour = Color.Red;

			grid.LayoutCategoryPK = System.Guid.NewGuid();
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			form.SetDataBinding(dummyBizo, "");
			return form;
		});
		Assert.That(rendered.FindAll("tr>td").Where(c => (c.GetAttribute("style") != null && c.GetAttribute("style").Contains("background-color: #FF0000FF;"))).Count, Is.EqualTo(4));
	}

	[Test, WithPlaywrightPage]
	public async Task ZGridRenderCorrectBorderColor()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(500, 200) };
			var dataGrid = CreateNewZGrid();
			dataGrid.ReadOnly = true;
			dataGrid.IsWholeRowSelectedOnClick = true;
			form.Controls.Add(dataGrid);

			return form;
		});

		var table = await page.WaitForSelectorAsync("table");

		var trs = await table.QuerySelectorAllAsync("tbody tr");
		var ths = await table.QuerySelectorAllAsync("th");
		var tds = await table.QuerySelectorAllAsync("td");

		var outerBorderBottomColor = await page.EvaluateAsync<string>("window.getComputedStyle(document.getElementsByTagName('tr')[0].getElementsByTagName('th')[0], null).borderBottomColor");
		Assert.That(outerBorderBottomColor, Is.EqualTo("rgb(214, 216, 217)"));

		var firstOuterBorderLeftColor = await page.EvaluateAsync<string>("window.getComputedStyle(document.getElementsByTagName('tr')[0].getElementsByTagName('th')[0], null).borderLeftColor");
		Assert.That(firstOuterBorderLeftColor, Is.EqualTo("rgb(0, 0, 0)"));

		var innerBorderBottomColor = await page.EvaluateAsync<string>("window.getComputedStyle(document.getElementsByTagName('tr')[1].getElementsByTagName('td')[1], null).borderBottomColor");
		Assert.That(innerBorderBottomColor, Is.EqualTo("rgb(240, 240, 240)"));

		var innerBorderLeftColor = await page.EvaluateAsync<string>("window.getComputedStyle(document.getElementsByTagName('tr')[1].getElementsByTagName('td')[1], null).borderLeftColor");
		Assert.That(innerBorderLeftColor, Is.EqualTo("rgb(240, 240, 240)"));
	}

	[Test, WithPlaywrightPage]
	public async Task TestCheckBoxStateChangedAfterFirstClick()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var child1 = factory.New<DummyBaseBusinessObject>();
			child1.Z0_Code = "1";
			child1.Z0_Bool = false;
			dummyBizo.Collection.Add(child1);

			var grid = new ZGrid();
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 400));
			grid.ColumnStyles.Add(new ZCheckBoxColumnStyleInfo("Z0_Bool", 200));
			grid.LayoutCategoryPK = Guid.NewGuid();
			grid.Dock = DockStyle.Fill;

			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			form.BindingSource.SetBindingMember(grid, "Collection");
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			form.SetDataBinding(dummyBizo, "");
			form.Size = new Size { Width = 600, Height = 400 };

			return form;
		});

		var checkBox = page.GetByRole(AriaRole.Checkbox);
		await Assertions.Expect(checkBox).Not.ToBeCheckedAsync();

		await checkBox.ClickAsync();
		await Assertions.Expect(checkBox).ToBeCheckedAsync();

		await checkBox.ClickAsync();
		await Assertions.Expect(checkBox).Not.ToBeCheckedAsync();
	}

	[Test, WithPlaywrightPage]
	public async Task TestCheckBoxStateChangedInNewRowAfterFirstClick()
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

			var grid = new ZGrid { Height = 300 };
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 100));
			grid.ColumnStyles.Add(new ZCheckBoxColumnStyleInfo("Z0_Bool", 200));
			grid.LayoutCategoryPK = System.Guid.NewGuid();

			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			form.BindingSource.SetBindingMember(grid, "Collection");
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			form.SetDataBinding(dummyBizo, "");

			return form;
		});

		await page.Locator("table").WaitForAsync(new () { State = WaitForSelectorState.Attached });
		Assert.That(() => page.Locator(".datagrid tbody>tr").CountAsync(), Is.EqualTo(2).After(1000, 100));
		Assert.That(await page.Locator(".datagrid tbody>tr:nth-of-type(1)").GetAttributeAsync("class"), Does.Contain("datagrid__row--edit"));
		Assert.That(await page.Locator(".datagrid tbody>tr:nth-of-type(2)").GetAttributeAsync("class"), Does.Contain("datagrid__row-star--new"));
		Assert.That(await page.Locator(".checkbox__input").CountAsync(), Is.EqualTo(1));
		Assert.That(await page.Locator(".checkbox__input").Nth(0).EvaluateAsync<bool>("e => e.getAttributeNames().includes(\"checked\")"), Is.EqualTo(true));

		await page.Locator(".datagrid__row-star--new td:nth-of-type(3)").ClickAsync();
		Assert.That(() => page.Locator(".datagrid tbody>tr").CountAsync(), Is.EqualTo(3).After(1000, 100));
		Assert.That(await page.Locator(".datagrid tbody>tr:nth-of-type(1)").GetAttributeAsync("class"), Does.Not.Contain("datagrid__row--edit"));
		Assert.That(await page.Locator(".datagrid tbody>tr:nth-of-type(2)").GetAttributeAsync("class"), Does.Contain("datagrid__row--edit"));
		Assert.That(await page.Locator(".datagrid tbody>tr:nth-of-type(2)>td:nth-of-type(3)").GetAttributeAsync("class"), Does.Contain("datagrid__cell--edit"));
		Assert.That(await page.Locator(".checkbox__input").CountAsync(), Is.EqualTo(2));
		Assert.That(await page.Locator(".checkbox__input").Nth(0).EvaluateAsync<bool>("e => e.getAttributeNames().includes(\"checked\")"), Is.EqualTo(true));
		Assert.That(await page.Locator(".checkbox__input").Nth(1).EvaluateAsync<bool>("e => e.getAttributeNames().includes(\"checked\")"), Is.EqualTo(false));
		Assert.That(await page.Locator(".datagrid tbody>tr:nth-of-type(3)").GetAttributeAsync("class"), Does.Contain("datagrid__row-star--new"));
	}

	[Test, WithPlaywrightPage]
	public async Task TestSelectAllInNewRowNoTextSelected()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZGrid grid = null;
		ZTextBox textBox = null;
		ZRichTextBox richTextBox = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var child1 = factory.New<DummyBaseBusinessObject>();
			child1.Z0_Code = "1";
			dummyBizo.Collection.Add(child1);

			grid = new ZGrid { Height = 300 };
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 100));
			grid.LayoutCategoryPK = System.Guid.NewGuid();

			textBox = new ZTextBox() { Width = 100, Height = 30, Left = 150, Text = "TextBox" };

			richTextBox = new ZRichTextBox() { Width = 100, Height = 100, Left = 150, Top = 150, ReadOnly = true, Html = "RichTextBox", IsToolBarVisible = false };

			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			form.BindingSource.SetBindingMember(grid, "Collection");
			form.Controls.Add(grid);
			form.Controls.Add(textBox);
			form.Controls.Add(richTextBox);
			form.DataSourceType = typeof(DummyBusinessObject);
			form.SetDataBinding(dummyBizo, "");
			return form;
		});
		await page.Locator("table").WaitForAsync(new() { State = WaitForSelectorState.Attached });
		Assert.That(() => page.Locator(".datagrid tbody>tr").CountAsync(), Is.EqualTo(2).After(1000, 100));
		Assert.That(await page.Locator(".datagrid tbody>tr:nth-of-type(2)").GetAttributeAsync("class"), Does.Contain("datagrid__row-star--new"));
	
		await page.Locator(".datagrid__row-star--new td:nth-of-type(2)").ClickAsync();
		await page.Locator("table > tbody > tr:nth-of-type(2) td:nth-of-type(2) .textbox").WaitForAsync();
		await page.Keyboard.TypeAsync("Input Test");
		await page.Locator("table > tbody > tr:nth-of-type(2) td:nth-of-type(1)").ClickAsync();
		await page.Locator("table > tbody > tr:nth-of-type(2).datagrid__row--selected").WaitForAsync();
		await MouseDownAsync(page);
		await MouseUpAsync(page);
		await page.Keyboard.PressAsync("Control+A");
		await Task.Delay(500);
		Assert.That(grid.GetSelectedRowIndexes(), Is.EqualTo(new[] { 0, 1 }));
		Assert.That(await page.Locator(".form > input").GetComputedStyleAsync("user-select"), Is.EqualTo("none"));
		Assert.That(await page.Locator(".richtextbox__data").GetComputedStyleAsync("user-select"), Is.EqualTo("text"));
		Assert.That(async () => await page.EvaluateAsync<string>("e => window.getSelection().toString()"), Does.Not.Contain("TextBox").After(3000, 100));
	}

	[TestCase(HorizontalAlignment.Left, "text-align: left;", TestName = "{m}_Left")]
	[TestCase(HorizontalAlignment.Center, "text-align: center;", TestName = "{m}_Center")]
	[TestCase(HorizontalAlignment.Right, "text-align: right;", TestName = "{m}_Right")]
	public async Task ZGridTextAlignAppliedToColumns(HorizontalAlignment alignment, string expectedStyleString)
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var child1 = factory.New<DummyBaseBusinessObject>();
			child1.Z0_Code = "1";
			dummyBizo.Collection.Add(child1);
			var child2 = factory.New<DummyBaseBusinessObject>();
			child2.Z0_Code = "2";
			dummyBizo.Collection.Add(child2);
			var form = new WinzorTestForm();
			var grid = new ZGrid();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			form.BindingSource.SetBindingMember(grid, "Collection");
			var columnStyleInfo1 = new ZTextBoxColumnStyleInfo("Z0_Code", 100);
			columnStyleInfo1.TextAlign = alignment;
			grid.ColumnStyles.Add(columnStyleInfo1);

			grid.LayoutCategoryPK = System.Guid.NewGuid();
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			form.SetDataBinding(dummyBizo, "");
			return form;
		});
		Assert.That(rendered.Find("tr:nth-child(1)>td:nth-child(2)").GetAttribute("style"), Does.Contain(expectedStyleString));
		Assert.That(rendered.Find("tr:nth-child(2)>td:nth-child(2)").GetAttribute("style"), Does.Contain(expectedStyleString));
	}

	[Test, WithPlaywrightPage, WithTransaction]
	public async Task ZCodeFindBoxFindButtonClickOnNewRowCommitsSelectedValue()
	{
		var clientServiceProvider = new MockCargoWiseClientSeviceProvider();
		var loadRequestSent = new TaskCompletionSource();
		EmbeddedModulePopup embeddedModulePopupform = null;
		await using var ctx = new InMemoryAppServerTestContext(clientServiceProvider);
		clientServiceProvider.MockWindowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				embeddedModulePopupform = (EmbeddedModulePopup)((RegisteredFormInstances)EnterpriseTestSetup.WinzorDispatcher.FormInstanceRegister).Lookup(createWindowOptions.Uri);
				var clientServices = MockCargoWiseClientServices.MakeMock();
				embeddedModulePopupform.ReadyToRender(clientServices, Guid.NewGuid());
				loadRequestSent.SetResult();
			});

		WinzorTestForm form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyEnterpriseBusinessObject);
			var grid = new ZGrid() { Width = 300, Height = 300 };
			form.BindingSource.SetBindingMember(grid, "Collection");
			var columnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			columnStyleInfo1.ModuleID = DummyModuleIDs.Dummy;
			columnStyleInfo1.ColumnName = "Z0_Code";
			columnStyleInfo1.BindToList = "Lookups+DummyList";
			grid.ColumnStyles.Add(columnStyleInfo1);
			var columnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			columnStyleInfo2.ColumnName = "Z0_Description";
			grid.ColumnStyles.Add(columnStyleInfo2);
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyEnterpriseBusinessObject);
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyEnterpriseBusinessObject>();
			form.SetDataBinding(dummyBizo, "");
			return form;
		});
		var table = await page.WaitForSelectorAsync("table");

		await page.WaitForSelectorAsync("td .textbox");
		await page.Locator("td button").ClickAsync();
		Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(30)), Is.True);
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			dummyBizo.Z0_Code = "FOO";
			factory.Save();
			embeddedModulePopupform.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new BusinessObject[] { dummyBizo });
		});
		await page.Keyboard.PressAsync("ArrowDown");
		Assert.That(async () => await page.Locator(".datagrid tbody>tr:nth-of-type(1)").TextContentAsync(), Does.Contain("FOO").After(2000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task GridDropDownCanClickAndChange()
	{
		var clientServiceProvider = new MockCargoWiseClientSeviceProvider();

		await using var ctx = new InMemoryAppServerTestContext(clientServiceProvider);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(RefAirline);
			var grid = new ZGrid() { Width = 300, Height = 300 };
			form.BindingSource.SetBindingMember(grid, "EFreightStatusCollection");
			var columnStyleInfo1 = new ZDropEditColumnStyleInfo();
			columnStyleInfo1.ColumnName = "RME_EFreightStatus";
			grid.ColumnStyles.Add(columnStyleInfo1);
			form.Controls.Add(grid);
			form.DataSourceType = typeof(RefAirline);
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<RefAirline>();
			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		await page.Locator("td .textbox").ClickAsync();
		await page.GetByRole(AriaRole.Button).ClickAsync();
		await Assertions.Expect(page.Locator(".zdropform")).ToHaveCountAsync(1);
	}

	[Test]
	public async Task GridValidations()
	{
		using var ctx = new EnterpriseTestContext();
		DummyBaseBusinessObject child2 = null;
		var rendered = await TestGridAsync<ZGrid>(ctx, () =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var child1 = factory.New<DummyBaseBusinessObject>();
			child2 = factory.New<DummyBaseBusinessObject>();
			child2.SuspendValidationTesting();
			dummyBizo.Collection.Add(child1);
			dummyBizo.Collection.Add(child2);
			return dummyBizo;
		}, CharacterCasing.Normal);

		Assert.That(rendered.Find("th:first-child").ToMarkup(), Does.Contain("datagrid__columns_dropdown"));
		Assert.That(rendered.FindAll("tbody>tr")[0].Children[0].TextContent, Is.EqualTo("arrow_right"));
		Assert.That(rendered.FindAll("tbody>tr")[1].Children[0].TextContent, Is.Empty);

		NoNotification(rendered, "table");

		await SimulateClickOnCellForRowChange(rendered.FindAll("tbody>tr")[0].Children[2]);
		var input1 = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		await rendered.KeyPressAsync(Keys.A, input1);
		input1 = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		await input1.TriggerEventAsync("oninput", new ChangeEventArgs { Value = "Bad" });
		await input1.FocusOutAsync();

		await SimulateClickOnCellForRowChange(rendered.FindAll("tbody>tr")[1].Children[2]);
		var input2 = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		await rendered.KeyPressAsync(Keys.A, input2);
		input2 = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		await input2.TriggerEventAsync("oninput", new ChangeEventArgs { Value = "Bad" });
		await input2.FocusOutAsync();
		await rendered.GetForm().InvokeWinzorDispatcherAsync(() => child2.Z0_DescriptionInfo.AddWarning("Warning!"));

		await ValidateBalloon(rendered, ".datagrid__notification", "There are errors in the grid.",
			"Click on the icon to navigate to the first error.",
			new List<(string type, string text)> { ("error", "Bad!"), ("warn", "Warning!") });

		await ValidateBalloon(rendered, "tbody>tr:nth-child(1) td:first-child", "There are errors on this row.",
			"These errors may be on another grid or fields that shows further information about this row.",
			new List<(string type, string text)> { ("error", "Bad!") });
		await ValidateBalloon(rendered, "tbody tr:nth-child(1) td:nth-child(3)", "Description", string.Empty,
			new List<(string type, string text)> { ("error", "Bad!") }, "error");

		await ValidateBalloon(rendered, "tbody>tr:nth-child(2) td:first-child", "There are errors on this row.",
			"These errors may be on another grid or fields that shows further information about this row.",
			new List<(string type, string text)> { ("error", "Bad!"), ("warn", "Warning!") });
		await ValidateBalloon(rendered, "tbody tr:nth-child(2) td:nth-child(3)", "Description", string.Empty,
			new List<(string type, string text)> { ("error", "Bad!"), ("warn", "Warning!") }, "error");

		await SimulateClickOnCellForRowChange(rendered.FindAll("tbody>tr")[0].Children[2]);
		var changedInput1 = rendered.Find("tbody>tr:nth-child(1) input");
		await rendered.KeyPressAsync(Keys.A, changedInput1);
		await changedInput1.TriggerEventAsync("oninput", new ChangeEventArgs { Value = "ok" });
		await changedInput1.FocusOutAsync();

		NoNotification(rendered, "tbody tr:nth-child(1) td:nth-child(3)");
		NoNotification(rendered, "tbody>tr:nth-child(1) td:first-child");

		await SimulateClickOnCellForRowChange(rendered.FindAll("tbody>tr")[1].Children[2]);
		var changedInput2 = rendered.Find("tbody>tr:nth-child(2) input");
		await rendered.KeyPressAsync(Keys.A, changedInput2);
		await changedInput2.TriggerEventAsync("oninput", new ChangeEventArgs { Value = "ok" });
		await changedInput2.FocusOutAsync();

		NoNotification(rendered, "tbody tr:nth-child(2) td:nth-child(3)");
		NoNotification(rendered, "tbody>tr:nth-child(2) td:first-child");

		await ctx.WinzorDispatcher.InvokeAsync(() => child2.Z0_DescriptionInfo.ClearAllNotifications());

		NoNotification(rendered, ".datagrid__notification");
	}

	[Test, WithPlaywrightPage]
	public async Task GridNotificationPosition()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZGrid dataGrid = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var form = new ZForm(dummyBizo) { Size = new Size(300, 300) };

			dataGrid = new ZGrid() { Size = new Size(200, 200) };
			dataGrid.BindTo = "Collection";
			dataGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 300) { CharacterCasing = CharacterCasing.Normal, TextAlign = HorizontalAlignment.Left });
			dataGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Decimal", 300) { CharacterCasing = CharacterCasing.Normal, TextAlign = HorizontalAlignment.Left });
			dataGrid.ColumnStyles.Add(new ZCheckBoxColumnStyleInfo("Z0_Bool", 300) { CharacterCasing = CharacterCasing.Normal, TextAlign = HorizontalAlignment.Left });

			form.Controls.Add(dataGrid);
			dataGrid.LayoutCategoryPK = System.Guid.NewGuid();
			dataGrid.ReadOnly = false;
			form.Controls.Add(dataGrid);
			form.DataSourceType = typeof(DummyBusinessObject);

			var child1 = factory.New<DummyBaseBusinessObject>();
			child1.SuspendValidationTesting();
			child1.Z0_Code = "1";
			child1.Z0_Bool = true;
			child1.Z0_BoolInfo.AddError("Error!");
			dummyBizo.Collection.Add(child1);

			var child2 = factory.New<DummyBaseBusinessObject>();
			child2.SuspendValidationTesting();
			child2.Z0_Code = "2";
			child2.Z0_Bool = false;
			child2.Z0_BoolInfo.AddWarning("Warning!");
			child2.Z0_DecimalInfo.AddWarning("Warning!");
			dummyBizo.Collection.Add(child2);
			dummyBizo.Z0_Bool = true;
			form.SetDataBinding(dummyBizo, "");
			return form;
			});

		var notificationLocator = page.Locator(".notification");
		await notificationLocator.First.WaitForAsync();

		var topOfHeader = await notificationLocator.First.GetComputedStyleAsync("top");
		Assert.That(topOfHeader, Is.EqualTo("4px"));

		await notificationLocator.Nth(4).WaitForAsync();
		var positionYOfTextBox = await notificationLocator.Nth(4).GetComputedStyleAsync("background-position-y");
		Assert.That(positionYOfTextBox, Is.EqualTo("2px"));

		await notificationLocator.Nth(2).WaitForAsync();
		var positionYOfCheckBox = await notificationLocator.Nth(2).GetComputedStyleAsync("background-position-y");
		Assert.That(positionYOfCheckBox, Is.EqualTo("2px"));
	}

	[Test]
	public async Task NotificationClickTakesToFirstError()
	{
		using var ctx = new EnterpriseTestContext();
		DummyBaseBusinessObject child2 = null;
		var rendered = await TestGridAsync<ZGrid>(ctx, () =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var child1 = factory.New<DummyBaseBusinessObject>();
			child2 = factory.New<DummyBaseBusinessObject>();
			child2.SuspendValidationTesting();
			dummyBizo.Collection.Add(child1);
			dummyBizo.Collection.Add(child2);
			return dummyBizo;
		}, CharacterCasing.Normal);
		var form = (WinzorTestForm)rendered.Instance.Control;
		var grid = (ZGrid)form.Controls[0];

		Assert.That(rendered.Find("th:first-child").ToMarkup(), Does.Contain("datagrid__columns_dropdown"));
		Assert.That(rendered.FindAll("tbody>tr")[0].Children[0].TextContent, Is.EqualTo("arrow_right"));
		Assert.That(rendered.FindAll("tbody>tr")[1].Children[0].TextContent, Is.Empty);

		NoNotification(rendered, "table");

		await SimulateClickOnCellForRowChange(rendered.FindAll("tbody>tr")[0].Children[2]);
		var input1 = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		await rendered.KeyPressAsync(Keys.A, input1);
		input1 = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		await input1.TriggerEventAsync("oninput", new ChangeEventArgs { Value = "Bad" });
		await input1.FocusOutAsync();

		await rendered.InvokeAsync(() => SimulateClickOnCellForRowChange(rendered.FindAll("tbody>tr")[1].Children[2]));
		var input2 = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		await rendered.KeyPressAsync(Keys.A, input2);
		input2 = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		await input2.TriggerEventAsync("oninput", new ChangeEventArgs { Value = "Bad" });
		await input2.FocusOutAsync();
		await ctx.WinzorDispatcher.InvokeAsync(() => child2.Z0_DescriptionInfo.AddWarning("Warning!"));

		var gridIcon = rendered.WaitForElement(".datagrid__notification .notification");
		await gridIcon.ClickAsync(new WebMouseEventArgs());
		await ValidateBalloon(rendered, ".datagrid__notification", "There are errors in the grid.",
			"Click on the icon to navigate to the first error.",
			new List<(string type, string text)> { ("error", "Bad!") });

		Assert.That(grid.CurrentCell.RowNumber, Is.EqualTo(0));
		Assert.That(grid.CurrentCell.ColumnNumber, Is.EqualTo(1));

		//Fix the error in first row and click will navigate to the second row with the error
		await rendered.FindAll("tbody>tr")[0].Children[2].MouseDownAsync(new WebMouseEventArgs());
		input1 = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		await rendered.KeyPressAsync(Keys.A, input1);
		input1 = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		await input1.TriggerEventAsync("oninput", new ChangeEventArgs { Value = "ok" });
		await input1.FocusOutAsync();

		await gridIcon.ClickAsync(new WebMouseEventArgs());

		Assert.That(grid.CurrentCell.RowNumber, Is.EqualTo(1));
		Assert.That(grid.CurrentCell.ColumnNumber, Is.EqualTo(1));

		await ValidateBalloon(rendered, ".datagrid__notification", "There are errors in the grid.",
			"Click on the icon to navigate to the first error.",
			new List<(string type, string text)> { ("error", "Bad!") });
	}

	[Test]
	public async Task LeftMostNotificationIconAfterAddNewRowShouldBeDisplayed()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await TestGridAsync<ZGrid>(ctx, () =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var child1 = factory.New<DummyBaseBusinessObject>();
			var child2 = factory.New<DummyBaseBusinessObject>();
			child2.SuspendValidationTesting();
			dummyBizo.Collection.Add(child1);
			dummyBizo.Collection.Add(child2);
			return dummyBizo;
		},
		CharacterCasing.Normal,
		height: 400);

		Assert.That(rendered.FindAll("tbody>tr").Count, Is.EqualTo(3));
		NoNotification(rendered, "table");

		await SimulateClickOnCellForRowChange(rendered.FindAll("tbody>tr")[0].Children[2]);
		var input1 = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		await rendered.KeyPressAsync(Keys.A, input1);
		input1 = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		await input1.TriggerEventAsync("oninput", new ChangeEventArgs { Value = "Bad" });
		await input1.FocusOutAsync();

		await SimulateClickOnCellForRowChange(rendered.FindAll("tbody>tr")[1].Children[2]);
		var input2 = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		await rendered.KeyPressAsync(Keys.A, input2);
		input2 = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		await input2.TriggerEventAsync("oninput", new ChangeEventArgs { Value = "Bad" });
		await input2.FocusOutAsync();

		await SimulateClickOnCellForRowChange(rendered.FindAll("tbody>tr")[2].Children[2]);
		var input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		await rendered.KeyPressAsync(Keys.A, input);

		Assert.That(rendered.FindAll("tbody>tr").Count, Is.EqualTo(4));
		Assert.That(rendered.FindAll("tbody>tr")[0].Children[0].InnerHtml, Does.Contain("notification notification--error"));
		Assert.That(rendered.FindAll("tbody>tr")[1].Children[0].InnerHtml, Does.Contain("notification notification--error"));
		Assert.That(rendered.FindAll("tbody>tr")[2].Children[0].InnerHtml, Does.Not.Contain("notification notification--error"));
		Assert.That(rendered.FindAll("tbody>tr")[3].Children[0].InnerHtml, Does.Not.Contain("notification notification--error"));
	}

	[Test, WithPlaywrightPage]
	[TestCase(1, 1, "2\t5\t9\r\n", TestName = "{m}_SingleRow")]
	[TestCase(1, 2, "2\t5\t9\r\n3\t5\t7\r\n", TestName = "{m}_MultiRows")]
	public async Task TestSelectedRowsCopyToClipboard(int startRow, int endRow, string expected)
	{
		await using var ctx = new InMemoryAppServerTestContext();

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

			var grid = new ZGrid { Height = 300 };
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 100));
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Description", 200));
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Decimal", 300));
			grid.LayoutCategoryPK = System.Guid.NewGuid();
			grid.ReadOnly = true;
			grid.IsWholeRowSelectedOnClick = true;

			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			form.BindingSource.SetBindingMember(grid, "Collection");
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			form.SetDataBinding(dummyBizo, "");
			form.Menu = null;

			return form;
		});

		var trs = page.Locator("tbody tr");
		var start = trs.Nth(startRow).Locator("td").Nth(1);
		var end = trs.Nth(endRow).Locator("td").Nth(1);

		await start.WaitForAsync();
		await end.WaitForAsync();

		await MouseMoveOfElementAsync(page, start, 2, 2);
		await MouseDownAsync(page);
		await MouseMoveOfElementAsync(page, end, 2, 12);
		await MouseUpAsync(page);
		await BrowserContext.GrantPermissionsAsync(new[] { "clipboard-write", "clipboard-read" });
		await page.Keyboard.PressAsync("Control+C");
		await Task.Delay(500);

		Assert.That(() => page.EvaluateAsync<string>("async () => await navigator.clipboard.readText()"), Is.EqualTo(expected).After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestDoesNotResetSelectedRowsOnRightClickWhenSelectingMultiRows()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZGrid grid = null;

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

			grid = new ZGrid() { Height = 300 };
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 100));
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Description", 200));
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Decimal", 300));
			grid.LayoutCategoryPK = System.Guid.NewGuid();
			grid.ReadOnly = true;
			grid.IsWholeRowSelectedOnClick = true;

			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			form.BindingSource.SetBindingMember(grid, "Collection");
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			form.SetDataBinding(dummyBizo, "");

			return form;
		});

		var trs = page.Locator("table tbody tr");
		var start = trs.Nth(1).Locator("td").Nth(1);
		var end = trs.Nth(2).Locator("td").Nth(1);

		await start.WaitForAsync();
		await end.WaitForAsync();

		await MouseMoveOfElementAsync(page, start, 2, 2);
		await MouseDownAsync(page);
		await MouseMoveOfElementAsync(page, end, 2, 12);
		await MouseUpAsync(page);
		Assert.That(grid.GetSelectedRowIndexes(), Is.EqualTo(new[] { 1, 2 }));

		await end.ClickAsync(new () { Button = MouseButton.Right });
		Assert.That(grid.GetSelectedRowIndexes(), Is.EqualTo(new[] { 1, 2 }));
	}

	[Test]
	public async Task ColumnStyleAddedAfterRender()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var child = factory.New<DummyBaseBusinessObject>();
			child.Z0_Code = "FOO";
			child.Z0_Description = "This is some foo";
			dummyBizo.Collection.Add(child);
			var form = new WinzorTestForm();
			var grid = new ZGrid();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			form.BindingSource.SetBindingMember(grid, "Collection");
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 100));
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Description", 200) { CharacterCasing = CharacterCasing.Normal, IsVisible = false });
			grid.LayoutCategoryPK = System.Guid.NewGuid();
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			form.SetDataBinding(dummyBizo, "");
			var button = new Button();
			button.Text = "Update";
			button.Click += Button_Click;
			form.Controls.Add(button);
			return form;
		});

		var tr = rendered.FindAll("tbody>tr");
		Assert.That(tr[0].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));
		var input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		Assert.That(input.Value, Is.EqualTo("FOO"));
		Assert.That(tr[0].ChildNodes[1].Contains(input), Is.True);

		await rendered.Find("button").ClickAsync(new WebMouseEventArgs());

		tr = rendered.FindAll("tbody>tr");
		Assert.That(tr[0].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));
		Assert.That((tr[0].ChildNodes[1].ChildNodes[0].ChildNodes[0] as IHtmlInputElement).Value, Is.EqualTo("FOO"));
		Assert.That(tr[0].ChildNodes[2].TextContent, Is.EqualTo("This is some foo"));

		void Button_Click(object sender, EventArgs e)
		{
			var grid = (ZGrid)((Button)sender).Parent.Controls.Single(c => c is ZGrid);
			grid.Columns[1].IsVisible = true;
			grid.RefreshTableStyles();
		}
	}

	[Test, WithPlaywrightPage]
	public async Task DataGridRendersCorrectlyAfterDropDownSelect()
	{
		var clientServiceProvider = new MockCargoWiseClientSeviceProvider();
		await using var ctx = new InMemoryAppServerTestContext(clientServiceProvider);

		var page = await ctx.LoadFormAsync(() =>
		{
			var grid = new ZGrid() { Dock = DockStyle.Fill };
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("RME_OriginLocation", 300));
			grid.ColumnStyles.Add(new ZDropEditColumnStyleInfo("RME_EFreightStatus", 200));

			var form = new WinzorTestForm();
			form.Size = new Size { Width = 600, Height = 300 };
			form.BindingSource.DataSourceType = typeof(RefAirline);
			form.BindingSource.SetBindingMember(grid, "EFreightStatusCollection");
			form.Controls.Add(grid);
			form.DataSourceType = typeof(RefAirline);

			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<RefAirline>();
			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		var dataGrid = page.Locator(".datagrid");
		var rows = page.Locator(".datagrid table tr");
		var tbody = page.Locator("tbody");

		await Assertions.Expect(dataGrid).ToHaveCountAsync(1);
		await Assertions.Expect(rows).ToHaveCountAsync(2);
		await Assertions.Expect(tbody).ToHaveCountAsync(1);

		// Click on cell > dropdown button > third option
		await page.GetByText("NON").ClickAsync();
		await page.GetByRole(AriaRole.Button).ClickAsync();
		await page.Locator(".zdropform table tr").Nth(2).ClickAsync();

		// Assert that expected elements (e.g. row) are present
		await Assertions.Expect(dataGrid).ToHaveCountAsync(1);
		await Assertions.Expect(rows).ToHaveCountAsync(3);
		await Assertions.Expect(tbody).ToHaveCountAsync(1);
	}

	async Task ValidateBalloon(IRenderedFragment rendered, string container, string caption, string description, List<(string type, string text)> items, string validation = "")
	{
		var parent = rendered.WaitForElement($"{container}");
		if (!string.IsNullOrEmpty(validation))
		{
			Assert.That(parent.Attributes["class"].Value, Does.Contain($"datagrid__cell--{validation}"));
		}

		var gridIcon = rendered.WaitForElement($"{container} .notification");
		await gridIcon.TriggerEventAsync("onmouseover", new WebMouseEventArgs());
		rendered.WaitForElement(".balloon");
		var balloonCaption = rendered.Find(".balloon__caption");
		var balloonDescription = rendered.Find(".balloon__description");
		var balloonItems = rendered.FindAll($"li");
		Assert.That(balloonCaption.InnerHtml, Is.EqualTo(caption));
		Assert.That(balloonDescription.InnerHtml, Is.EqualTo(description));

		for (var i = 0; i < items.Count; i++)
		{
			var item = items[i];
			Assert.That(balloonItems[i].Attributes["class"].Value, Does.Contain(item.type));
			Assert.That(balloonItems[i].InnerHtml, Does.Contain(item.text));
		}

		await gridIcon.TriggerEventAsync("onmouseout", null);
		rendered.WaitForState(() => rendered.FindAll(".balloon").Count == 0);
	}

	void NoNotification(IRenderedFragment rendered, string container)
	{
		rendered.WaitForState(() => rendered.FindAll($"{container} .notification").Count == 0);
	}

	[Test, WithPlaywrightPage]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1097:DoNotHardcodeTempOrTmpPaths", Justification = "Not a file path")]
	public async Task DraggableDataGridRow([Values] bool draggable)
	{
		await using var ctx = new InMemoryAppServerTestContext();

		string bizPK = string.Empty, docPK = string.Empty;
		var page = await ctx.LoadFormAsync(() =>
		{
			var data = "data";
			var dataArray = Encoding.UTF32.GetBytes(data);
			var form = new WinzorTestForm();
			var factory = new BusinessObjectFactory();
			var docFactory = new DbBackendDocumentFactory(factory);
			var numFactory = new NumberedBusinessObjectFactory(0, docFactory);
			var parent = factory.New<DummyBusinessObject>();
			var storage = docFactory.New<StorageMain>();
			storage.SM_DB = 0;
			storage.SM_ParentFK = parent.PK;
			bizPK = parent.PK.ToString();
			var file = StorageFile.NewWithParent_DEBUG(numFactory);
			file.SC_ImageData = new ZBlob(dataArray);
			file.SC_FileName = "test";
			file.SC_DataType = "txt";
			storage.eDocs.Add(file);
			docPK = file.PK.ToString();

			var grid = new DocumentsZGrid();
			var column = new ZTextBoxColumnStyleInfo() { ColumnName = "SC_FileNameWithExtension" };
			grid.AllowBeginDrag = draggable;
			grid.CopySelectedRowsAllowed = true;
			grid.AllowDragDropWithChanges = true;
			grid.ReadOnly = true;
			grid.Columns.Add(column);
			form.BindingSource.DataSourceType = typeof(StorageMain);
			form.BindingSource.SetBindingMember(grid, "eDocsView");
			form.SetDataBinding(storage, "");
			form.Controls.Add(grid);
			return form;
		});

		var td = page.Locator("table tbody tr td").First;

		await Assertions.Expect(td).ToHaveCountAsync(1);

		await td.DispatchEventAsync("mousedown");

		var selectedRow = page.Locator("tr.datagrid__row--selected");

		await Assertions.Expect(selectedRow).ToHaveCountAsync(1);

		if (draggable)
		{
			await Assertions.Expect(td).ToHaveAttributeAsync("draggable", "true");

			var dataTransfer = await page.EvaluateHandleAsync("() => new DataTransfer()");
			await td.DispatchEventAsync("dragstart", new { dataTransfer });
			var file = await dataTransfer.EvaluateHandleAsync("(transfer) => transfer.getData('DownloadURL')");

			Assert.That(file.ToString(), Is.EqualTo($"application/octet-stream:test.txt:{ctx.ServerBaseUrl}/edoc/download/test.txt?docPK={docPK}&bizPK={bizPK}"));
			Assert.That(File.Exists(CargoWise.IO.Temp.TempPath + "test.txt"), Is.False);
		}
		else
		{
			await Assertions.Expect(td).Not.ToHaveAttributeAsync("draggable", "true");
		}
	}

	[Test, WithPlaywrightPage, WithSnapshotProtection]
	public async Task ScrollToSingleSelectedRowAfterHeaderClicked()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			var grid = new ZGrid { Width = 300, Height = 200 };
			grid.ReadOnly = true;
			grid.IsWholeRowSelectedOnClick = true;
			form.BindingSource.SetBindingMember(grid, "Collection");

			var columnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			columnStyleInfo.ModuleID = DummyModuleIDs.Dummy;
			columnStyleInfo.ColumnName = "Z0_Code";
			columnStyleInfo.BindToList = "Lookups+DummyList";
			grid.ColumnStyles.Add(columnStyleInfo);

			grid.LayoutCategoryPK = System.Guid.NewGuid();
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			for (int i = 0; i < 50; ++i)
			{
				var child = factory.New<DummyEnterpriseBusinessObject>();
				child.Z0_Code = i <= 9 ? $"0{i}" : $"{i}";
				dummyBizo.Collection.Add(child);
			}
			form.SetDataBinding(dummyBizo, "");

			return form;
		});

		// sort by first column
		var columnHeader = await page.Locator(".datagrid table thead th:nth-of-type(2) span").BoundingBoxAsync();
		await SimulateClickEvent(page, columnHeader);
		Assert.That(async () => await page.Locator(".datagrid table thead th:nth-of-type(2)").GetAttributeAsync("class"), Does.Contain("datagrid__header--ascending").After(2000, 100));

		// get the scrollTop
		await page.EvaluateAsync("document.querySelector('.datagrid').scrollTo(0, 9999);");
		Assert.That(async () => await page.EvaluateAsync<int>("document.querySelector('.datagrid').scrollTop"), Is.GreaterThan(0).After(2000, 100));
		var actualScrollTop = await page.EvaluateAsync<int>("document.querySelector('.datagrid').scrollTop");

		// select first row
		await page.EvaluateAsync("document.querySelector('.datagrid').scrollTo(0, 0);");
		Assert.That(async () => await page.EvaluateAsync<int>("document.querySelector('.datagrid').scrollTop"), Is.EqualTo(0).After(2000, 100));
		await page.Locator(".datagrid tbody>tr:nth-of-type(1)").ClickAsync();
		Assert.That(async () => await page.Locator(".datagrid tbody>tr:nth-of-type(1)").TextContentAsync(), Does.Contain("00").After(2000, 100));
		Assert.That(async () => await page.Locator(".datagrid tbody>tr:nth-of-type(1)").GetAttributeAsync("class"), Does.Contain("datagrid__row--selected").After(2000, 100));
		await page.EvaluateAsync("document.querySelector('.datagrid').scrollTo(0, 400);");
		Assert.That(async () => await page.EvaluateAsync<int>("document.querySelector('.datagrid').scrollTop"), Is.EqualTo(400).After(2000, 100));
		columnHeader = await page.Locator(".datagrid table thead th:nth-of-type(2) span").BoundingBoxAsync();
		await SimulateClickEvent(page, columnHeader);
		// the first row is selected and should be sorted at the last
		Assert.That(async () => await page.Locator(".datagrid table thead th:nth-of-type(2)").GetAttributeAsync("class"), Does.Contain("datagrid__header--descending").After(2000, 100));
		Assert.That(async () => await page.Locator(".datagrid tbody>tr:last-child").TextContentAsync(), Does.Contain("00").After(2000, 100));
		Assert.That(async () => await page.Locator(".datagrid tbody>tr:last-child").GetAttributeAsync("class"), Does.Contain("datagrid__row--selected").After(2000, 100));
		Assert.That(async () => await page.Locator(".datagrid tbody>tr:last-child").IsVisibleAsync(), Is.EqualTo(true).After(2000, 100));
		Assert.That(async () => await page.EvaluateAsync<int>("document.querySelector('.datagrid').scrollTop"), Is.EqualTo(actualScrollTop));

		// sort again, the selected should be the first one
		columnHeader = await page.Locator(".datagrid table thead th:nth-of-type(2) span").BoundingBoxAsync();
		await SimulateClickEvent(page, columnHeader);
		Assert.That(async () => await page.Locator(".datagrid table thead th:nth-of-type(2)").GetAttributeAsync("class"), Does.Contain("datagrid__header--ascending").After(2000, 100));
		Assert.That(async () => await page.Locator(".datagrid tbody>tr:nth-of-type(1)").TextContentAsync(), Does.Contain("00").After(2000, 100));
		Assert.That(async () => await page.Locator(".datagrid tbody>tr:nth-of-type(1)").GetAttributeAsync("class"), Does.Contain("datagrid__row--selected").After(2000, 100));
		Assert.That(async () => await page.Locator(".datagrid tbody>tr:nth-of-type(1)").IsVisibleAsync(), Is.EqualTo(true).After(2000, 100));
		Assert.That(async () => await page.EvaluateAsync<int>("document.querySelector('.datagrid').scrollTop"), Is.EqualTo(0).After(2000, 100));

		// scroll to the bottom to select the last row
		await page.EvaluateAsync("document.querySelector('.datagrid').scrollTo(0, 9999);");
		Assert.That(async () => await page.EvaluateAsync<int>("document.querySelector('.datagrid').scrollTop"), Is.EqualTo(actualScrollTop).After(2000, 100));
		var row = await page.Locator(".datagrid tbody>tr:last-child td:nth-of-type(1)").BoundingBoxAsync();
		await SimulateClickEvent(page, row);
		Assert.That(async () => await page.Locator(".datagrid tbody>tr:last-child").TextContentAsync(), Does.Contain("49").After(2000, 100));
		Assert.That(async () => await page.Locator(".datagrid tbody>tr:last-child").GetAttributeAsync("class"), Does.Contain("datagrid__row--selected").After(2000, 100));
		actualScrollTop = await page.EvaluateAsync<int>("document.querySelector('.datagrid').scrollTop");

		// first sort, the last one will become the first row
		columnHeader = await page.Locator(".datagrid table thead th:nth-of-type(2) span").BoundingBoxAsync();
		await SimulateClickEvent(page, columnHeader);
		Assert.That(async () => await page.Locator(".datagrid table thead th:nth-of-type(2)").GetAttributeAsync("class"), Does.Contain("datagrid__header--descending").After(2000, 100));
		Assert.That(async () => await page.Locator(".datagrid tbody>tr:nth-of-type(1)").TextContentAsync(), Does.Contain("49").After(2000, 100));
		Assert.That(async () => await page.Locator(".datagrid tbody>tr:nth-of-type(1)").GetAttributeAsync("class"), Does.Contain("datagrid__row--selected").After(2000, 100));
		Assert.That(async () => await page.Locator(".datagrid tbody>tr:nth-of-type(1)").IsVisibleAsync(), Is.EqualTo(true).After(2000, 100));
		Assert.That(async () => await page.EvaluateAsync<int>("document.querySelector('.datagrid').scrollTop"), Is.EqualTo(0).After(2000, 100));

		// second sort, the last one will become the last row (previous is the first one)
		columnHeader = await page.Locator(".datagrid table thead th:nth-of-type(2) span").BoundingBoxAsync();
		await SimulateClickEvent(page, columnHeader);
		Assert.That(async () => await page.Locator(".datagrid table thead th:nth-of-type(2)").GetAttributeAsync("class"), Does.Contain("datagrid__header--ascending").After(2000, 100));
		Assert.That(async () => await page.Locator(".datagrid tbody>tr:last-child").TextContentAsync(), Does.Contain("49").After(2000, 100));
		Assert.That(async () => await page.Locator(".datagrid tbody>tr:last-child").GetAttributeAsync("class"), Does.Contain("datagrid__row--selected").After(2000, 100));
		Assert.That(async () => await page.Locator(".datagrid tbody>tr:last-child").IsVisibleAsync(), Is.EqualTo(true).After(2000, 100));
		Assert.That(async () => await page.EvaluateAsync<int>("document.querySelector('.datagrid').scrollTop"), Is.EqualTo(actualScrollTop).After(2000, 100));
	}

	[Test, WithPlaywrightPage, WithSnapshotProtection]
	public async Task ScrollToMultipleSelectedRowAfterHeaderClicked()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			var grid = new ZGrid { Width = 300, Height = 500 };
			grid.ReadOnly = true;
			grid.IsWholeRowSelectedOnClick = true;
			form.BindingSource.SetBindingMember(grid, "Collection");

			var columnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			columnStyleInfo.ModuleID = DummyModuleIDs.Dummy;
			columnStyleInfo.ColumnName = "Z0_Code";
			columnStyleInfo.BindToList = "Lookups+DummyList";
			grid.ColumnStyles.Add(columnStyleInfo);

			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			for (var i = 0; i < 30; ++i)
			{
				var child = factory.New<DummyEnterpriseBusinessObject>();
				child.Z0_Code = $"{i}";
				dummyBizo.Collection.Add(child);
			}
			form.SetDataBinding(dummyBizo, "");

			return form;
		});

		var dataGrid = page.Locator(".datagrid");
		await dataGrid.WaitForAsync();

		const int startRow = 0;
		const int endRow = 9;

		var trs = page.Locator("table tbody tr");
		var startColumn = trs.Nth(startRow).Locator("td").Nth(1);
		var endColumn = trs.Nth(endRow).Locator("td").Nth(1);
		await startColumn.WaitForAsync();
		await endColumn.WaitForAsync();

		await MouseMoveOfElementAsync(page, startColumn, 2, 2);
		await MouseDownAsync(page);
		await MouseMoveOfElementAsync(page, endColumn, 2, 2);
		await MouseUpAsync(page);

		await dataGrid.EvaluateAsync("(e) => e.scrollTo(0, 300);");
		await page.GetByText("1", new () { Exact = true }).WaitForAsync(new () { Timeout = 2000 });

		for (var i = 0; i < 30; i++)
		{
			if (i is <= endRow and >= startRow)
			{
				Assert.That(await trs.Nth(i).GetAttributeAsync("class"), Does.Contain("datagrid__row--selected"));
			}
			else
			{
				Assert.That(await trs.Nth(i).GetAttributeAsync("class"), Does.Not.Contain("datagrid__row--selected"));
			}
		}

		var columnHeader = await page.Locator(".datagrid th > span").BoundingBoxAsync();
		await SortByColumnHeader(columnHeader);
		await dataGrid.EvaluateAsync("(e) => e.scrollTo(0, 300);");
		Assert.That(async () => (await page.QuerySelectorAllAsync("tr.datagrid__row--selected")).Count, Is.EqualTo(10).After(2000, 200));

		await SortByColumnHeader(columnHeader);
		await dataGrid.EvaluateAsync("(e) => e.scrollTo(0, 0);");
		Assert.That(async () => (await page.QuerySelectorAllAsync("tr.datagrid__row--selected")).Count, Is.EqualTo(10).After(2000, 200));

		async Task SortByColumnHeader(LocatorBoundingBoxResult location)
		{
			await page.Mouse.MoveAsync(location.X + location.Width / 2, location.Y + location.Height / 2);
			await Task.Delay(200);
			await MouseDownAsync(page);
			await MouseUpAsync(page);
		}
	}

	[Test, WithPlaywrightPage]
	public async Task LeftAndRightClickOnSingleRowWhenSelectMultiRows()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZGrid dataGrid = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var form = new ZForm(dummyBizo) { Size = new Size(600, 600) };

			dataGrid = new ZGrid { Size = new Size(200, 200) };
			dataGrid.BindTo = "Collection";
			dataGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 300));
			dataGrid.ReadOnly = true;
			dataGrid.IsWholeRowSelectedOnClick = true;
			form.Controls.Add(dataGrid);
			form.DataSourceType = typeof(DummyBusinessObject);

			for (var i = 0; i < 6; i++)
			{
				var child = factory.New<DummyBaseBusinessObject>();
				dummyBizo.Collection.Add(child);
			}
			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		var trs = page.Locator("table tbody tr");
		var start = trs.Nth(0).Locator("td").Nth(1);
		var end = trs.Nth(3).Locator("td").Nth(1);

		await start.WaitForAsync();
		await end.WaitForAsync();

		await MouseMoveOfElementAsync(page, start, 2, 2);
		await MouseDownAsync(page);
		await MouseMoveOfElementAsync(page, end, 2, 12);
		await MouseUpAsync(page);

		Assert.That(dataGrid.CurrentCell.RowNumber, Is.EqualTo(0));
		Assert.That(dataGrid.CurrentCell.ColumnNumber, Is.EqualTo(0));
		AssertNumSelectedRowsInGrid(dataGrid, 2000, 4);

		await end.ClickAsync(new () { Button = MouseButton.Right });
		await Task.Delay(300);

		Assert.That(dataGrid.CurrentCell.RowNumber, Is.EqualTo(0));
		Assert.That(dataGrid.CurrentCell.ColumnNumber, Is.EqualTo(0));
		AssertNumSelectedRowsInGrid(dataGrid, 2000, 4);

		await end.ClickAsync(new () { Button = MouseButton.Left });
		await Task.Delay(300);

		Assert.That(dataGrid.CurrentCell.RowNumber, Is.EqualTo(3));
		Assert.That(dataGrid.CurrentCell.ColumnNumber, Is.EqualTo(0));
		AssertNumSelectedRowsInGrid(dataGrid, 2000, 1);
	}

	[Test, WithPlaywrightPage]
	public async Task LeftClickOnSingleGridCellWhenSelectMultiRows()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZGrid dataGrid = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var form = new ZForm(dummyBizo) { Size = new Size(600, 600) };

			dataGrid = new ZGrid { Size = new Size(400, 400) };
			dataGrid.BindTo = "Collection";
			dataGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 100));
			form.Controls.Add(dataGrid);
			form.DataSourceType = typeof(DummyBusinessObject);

			for (var i = 0; i < 6; i++)
			{
				var child = factory.New<DummyBaseBusinessObject>();
				dummyBizo.Collection.Add(child);
			}
			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		await page.WaitForSelectorAsync("table tbody tr td");
		var tds = await page.QuerySelectorAllAsync("tbody tr td");

		await SimulateClickEvent(page, tds[0]);
		await page.Keyboard.DownAsync("Shift");
		await Task.Delay(500);
		await SimulateClickEvent(page, tds[4]);
		AssertNumSelectedRowsInGrid(dataGrid, 2000, 3);

		await page.Keyboard.UpAsync("Shift");
		await Task.Delay(500);
		await SimulateClickEvent(page, tds[3]);
		AssertNumSelectedRowsInGrid(dataGrid, 2000, 0);

		var numEditCells = await page.QuerySelectorAllAsync(".datagrid__cell--edit");
		Assert.That(dataGrid.CurrentCell.RowNumber, Is.EqualTo(1));
		Assert.That(dataGrid.CurrentCell.ColumnNumber, Is.EqualTo(0));
		Assert.That(numEditCells.Count, Is.EqualTo(1), "Click one row after multiple rows are selected failed");
	}

	[Test, WithPlaywrightPage]
	public async Task RightClickOnSingleGridCellWhenSelectMultiRows()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZGrid dataGrid = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var form = new ZForm(dummyBizo) { Size = new Size(600, 600) };

			dataGrid = new ZGrid { Size = new Size(400, 400) };
			dataGrid.BindTo = "Collection";
			dataGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 100));
			form.Controls.Add(dataGrid);
			form.DataSourceType = typeof(DummyBusinessObject);

			for (var i = 0; i < 6; i++)
			{
				var child = factory.New<DummyBaseBusinessObject>();
				dummyBizo.Collection.Add(child);
			}
			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		var table = await page.WaitForSelectorAsync("table");
		await page.Locator(".datagrid__cell--edit").WaitForAsync();
		var tds = await table.QuerySelectorAllAsync("tbody tr td");

		//right click grid cell among selected rows
		await SimulateClickEvent(page, tds[0]);
		await page.Keyboard.DownAsync("Shift");
		await Task.Delay(500);
		await SimulateClickEvent(page, tds[2]);
		AssertNumSelectedRowsInGrid(dataGrid, 2000, 2);

		await page.Keyboard.UpAsync("Shift");
		await Task.Delay(500);
		await SimulateClickEvent(page, tds[1], MouseButton.Right);
		var numEditCells = await page.QuerySelectorAllAsync(".datagrid__cell--edit");
		Assert.That(dataGrid.CurrentCell.RowNumber, Is.EqualTo(1));
		Assert.That(dataGrid.CurrentCell.ColumnNumber, Is.EqualTo(0));
		Assert.That(numEditCells.Count, Is.EqualTo(0));

		//right click grid cell not among selected rows
		await SimulateClickEvent(page, tds[5], MouseButton.Right);
		numEditCells = await page.QuerySelectorAllAsync(".datagrid__cell--edit");
		Assert.That(dataGrid.CurrentCell.RowNumber, Is.EqualTo(2));
		Assert.That(dataGrid.CurrentCell.ColumnNumber, Is.EqualTo(0));
		Assert.That(numEditCells.Count, Is.EqualTo(1));
	}

	[Test, WithPlaywrightPage]
	public async Task ScrollPositionShouldNotChangeAfterHeaderClicked()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() => SetupBigGrid(true));

		// sort by first column
		var columnHeader = page.Locator(".datagrid table thead th:nth-of-type(2)");
		await columnHeader.ClickAsync();

		await Assertions.Expect(columnHeader).ToHaveClassAsync("datagrid__header--ascending");
		await Assertions.Expect(columnHeader).ToHaveCSSAsync("width", "100px");
		await Assertions.Expect(page.Locator(".datagrid table thead th:nth-of-type(21)")).ToHaveCSSAsync("width", "100px");

		var datagrid = page.Locator(".datagrid");
		await datagrid.WaitForAsync(new() { Timeout = 3000 });
		await datagrid.EvaluateAsync("e => e.scrollTo(400, 0);");
		await Assertions.Expect(datagrid).ToHaveJSPropertyAsync("scrollLeft", 400);
		columnHeader = page.Locator(".datagrid table thead th:nth-of-type(6)");
		await columnHeader.ClickAsync();
		await Assertions.Expect(columnHeader).ToHaveClassAsync("datagrid__header--ascending");
		await Assertions.Expect(datagrid).ToHaveJSPropertyAsync("scrollLeft", 400);

		await columnHeader.ClickAsync();
		await Assertions.Expect(columnHeader).ToHaveClassAsync("datagrid__header--descending");
		await Assertions.Expect(datagrid).ToHaveJSPropertyAsync("scrollLeft", 400);

		// scroll to the right
		await datagrid.EvaluateAsync("e => e.scrollTo(9999, 0);");
		var actualScrollLeft = await datagrid.EvaluateAsync<int>("e => e.scrollLeft");
		Assert.That(() => actualScrollLeft, Is.GreaterThan(400).After(3000, 100));

		columnHeader = page.Locator(".datagrid table thead th:nth-of-type(21)");
		await columnHeader.ClickAsync();
		await Assertions.Expect(columnHeader).ToHaveClassAsync("datagrid__header--ascending");
		Assert.That(async () => await datagrid.EvaluateAsync<int>("e => e.scrollLeft"), Is.EqualTo(actualScrollLeft).After(3000, 100));

		await columnHeader.ClickAsync();
		await Assertions.Expect(columnHeader).ToHaveClassAsync("datagrid__header--descending");
		Assert.That(async () => await datagrid.EvaluateAsync<int>("e => e.scrollLeft"), Is.EqualTo(actualScrollLeft).After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task AutoScrollToTheEndIfEditingColumnIsPartiallyVisible()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			var grid = new ZGrid { Width = 300, Height = 300 };
			form.BindingSource.SetBindingMember(grid, "Collection");

			var dropEditColumn = new ZDropEditColumnStyleInfo
			{
				ColumnName = "Z0_Code",
				BindToList = "Lookups+DummyList",
				Width = 200
			};

			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Description", 100));
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Decimal", 100));
			grid.ColumnStyles.Add(dropEditColumn);

			grid.LayoutCategoryPK = Guid.NewGuid();
			grid.ReadOnly = false;
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		var dataGrid = page.Locator(".datagrid");
		await dataGrid.WaitForAsync();
		var cell = page.Locator("table tbody tr td").Last;
		await cell.ClickAsync();

		Assert.That(async () => await dataGrid.EvaluateAsync<int>("e => e.scrollLeft"), Is.EqualTo(135).Within(1).After(2000, 100));
	}

	[Test, WithPlaywrightPage]
	[TestCase(1, 1, TestName = "{m}_MouseMoveLessThan4")]
	[TestCase(2, 8, TestName = "{m}_MouseMoveGreaterThan4")]
	[TestCase(7, 3, TestName = "{m}_MouseMoveFromBottomToTop")]
	[TestCase(10, 20, TestName = "{m}_MouseMoveWithScrollY")]
	public async Task MouseMoveSelectRows(int startRow, int endRow)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(500, 400) };
			var dataGrid = CreateNewZGrid();
			dataGrid.ReadOnly = true;
			dataGrid.IsWholeRowSelectedOnClick = true;
			form.Controls.Add(dataGrid);
			return form;
		});

		var table = await page.WaitForSelectorAsync("table");
		await table.WaitForSelectorAsync($"tbody tr:nth-of-type({startRow}) td");
		var trs = await table.QuerySelectorAllAsync("tbody tr");
		var startColumn = (await trs[startRow].QuerySelectorAllAsync("td"))[2];
		await startColumn.ScrollIntoViewIfNeededAsync();
		await MouseMoveOfElementAsync(page, startColumn, 20, 8);
		await MouseDownAsync(page);
		var endColumn = (await trs[endRow].QuerySelectorAllAsync("td"))[2];
		await startColumn.ScrollIntoViewIfNeededAsync();
		await MouseMoveOfElementAsync(page, endColumn, 20, 8);
		await MouseUpAsync(page);

		await page.WaitForSelectorAsync("tr.datagrid__row--selected");
		var count = (await page.QuerySelectorAllAsync("tr.datagrid__row--selected")).Count;
		Assert.That(count, Is.EqualTo(Math.Abs(endRow - startRow) + 1));
		for (var i = 0; i < trs.Count; i++)
		{
			if (i <= Math.Max(startRow, endRow) && i >= Math.Min(startRow, endRow))
			{
				Assert.That(await trs[i].GetAttributeAsync("class"), Does.Contain("datagrid__row--selected"));
			}
			else
			{
				Assert.That(await trs[i].TryGetAttributeAsync("class"), Does.Not.Contain("datagrid__row--selected"));
			}
		}
	}

	[Test, WithPlaywrightPage]
	public async Task NotificationClickNavigateToFirstErrorCell()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var form = new ZForm(dummyBizo) { Size = new Size(300, 400) };

			var dataGrid = new ZGrid { Size = new Size(200, 350) };
			dataGrid.BindTo = "Collection";
			dataGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 200) { CharacterCasing = CharacterCasing.Normal, TextAlign = HorizontalAlignment.Left });
			dataGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Description", 200) { CharacterCasing = CharacterCasing.Normal, TextAlign = HorizontalAlignment.Left });
			dataGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Decimal", 300) { CharacterCasing = CharacterCasing.Normal, TextAlign = HorizontalAlignment.Left });

			form.Controls.Add(dataGrid);
			dataGrid.LayoutCategoryPK = Guid.NewGuid();
			dataGrid.ReadOnly = false;
			form.Controls.Add(dataGrid);
			form.DataSourceType = typeof(DummyBusinessObject);

			for (var i = 0; i < 70; i++)
			{
				var child = factory.New<DummyBaseBusinessObject>();
				dummyBizo.Collection.Add(child);
			}

			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		var dataGridDiv = page.Locator(".datagrid");
		await dataGridDiv.WaitForAsync();
		await Task.Delay(500);
		await dataGridDiv.EvaluateAsync("e => e.scrollTo(180, 140);");
		await Assertions.Expect(dataGridDiv).ToHaveJSPropertyAsync("scrollLeft",180);
		await Assertions.Expect(dataGridDiv).ToHaveJSPropertyAsync("scrollTop", 140);

		var trs = page.Locator("table tbody tr");
		var editColumn = trs.Nth(20).Locator("td").Nth(2);
		await editColumn.ClickAsync();

		var input = trs.Locator(".datagrid__control--edit input").First;
		await Assertions.Expect(input).ToHaveValueAsync("Default");
		await input.ClearAsync();
		await input.PressAsync("Shift+KeyB");
		await input.PressAsync("KeyA");
		await input.PressAsync("KeyD");
		await Task.Delay(500);

		await dataGridDiv.EvaluateAsync("e => e.scrollTo(0, 0);");
		await Assertions.Expect(dataGridDiv).ToHaveJSPropertyAsync("scrollLeft", 0);
		await Assertions.Expect(dataGridDiv).ToHaveJSPropertyAsync("scrollTop", 0);

		var startCell = trs.Nth(2).Locator("td").Nth(1);
		await startCell.ClickAsync();
		
		var icon = page.Locator(".notification.notification--error").First;

		await Assertions.Expect(icon).ToHaveCountAsync(1);

		await icon.ClickAsync();

		Assert.That(() => dataGridDiv.EvaluateAsync<int>("e => e.scrollLeft;"), Is.GreaterThan(0).After(3000, 100));
		Assert.That(() => dataGridDiv.EvaluateAsync<int>("e => e.scrollTop;"), Is.GreaterThan(0).After(3000, 100));

		await input.WaitForAsync();
		await page.Locator(".datagrid__row--edit .notification--error").First.WaitForAsync();
		var winzorId = await input.GetAttributeAsync("data-winzor-control-id");
		var focusId = await page.EvaluateAsync<string>("document.activeElement.getAttribute('data-winzor-control-id');");
		Assert.That(winzorId, Is.EqualTo(focusId));
	}

	[Test]
	public async Task DataGridClickOnReadOnlyCellSelectsRow()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await TestGridAsync<ZGrid>(ctx, () =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var child1 = factory.New<DummyBusinessObject>();
			child1.Z0_Code = "11";
			var child2 = factory.New<DummyBusinessObject>();
			child2.Z0_Code = "21";
			dummyBizo.Collection.Add(child1);
			dummyBizo.Collection.Add(child2);
			return dummyBizo;
		}, CharacterCasing.Upper, true);
		var grid = rendered.GetForm().Children.OfType<ZGrid>().First();
		await grid.InvokeWinzorDispatcherAsync(() => grid.IsWholeRowSelectedOnClick = true);

		await rendered.FindAll("tbody tr:last-child td")[2].MouseDownAsync(new WebMouseEventArgs());
		Assert.That(rendered.FindAll("tr.datagrid__row--selected").Count, Is.EqualTo(1));
		Assert.That(rendered.Find("tr.datagrid__row--selected").ChildNodes[1].TextContent, Is.EqualTo("21"));
		Assert.That(rendered.FindAll("tbody>tr")[1].GetAttribute("class"), Does.Contain("datagrid__row--selected"));
		Assert.That(rendered.FindAll("tbody>tr")[1].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));
	}

	[Test]
	public async Task RightClickSelectsRow()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await TestGridAsync<ZGrid>(ctx, () =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var child1 = factory.New<DummyBusinessObject>();
			var child2 = factory.New<DummyBusinessObject>();
			dummyBizo.Collection.Add(child1);
			dummyBizo.Collection.Add(child2);
			return dummyBizo;
		}, CharacterCasing.Upper, true);

		var rows = rendered.FindAll("tbody>tr");
		Assert.That(rows[0].ClassList, Does.Not.Contain("datagrid__row--selected"));
		await rendered.Find("tbody>tr:first-child td").MouseDownAsync(new WebMouseEventArgs { Button = 2 });
		rows = rendered.FindAll("tbody>tr");
		Assert.That(rows[0].ClassList, Does.Contain("datagrid__row--selected"));
		Assert.That(rendered.FindAll("tr.datagrid__row--selected").Count, Is.EqualTo(1));
	}

	[Test]
	public async Task TestWhetherShowSmallTriangleInTheMostLeftColumnHeader()
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await TestGridAsync<ZGrid>(ctx, () =>
		{
			var factory = new BusinessObjectFactory();
			return factory.New<DummyBusinessObject>();
		});
		Assert.That(rendered.FindAll(".datagrid__columns_dropdown").Count, Is.EqualTo(1));

		var rendered2 = await TestGridAsync<ZFilterGrid>(ctx, () =>
		{
			var factory = new BusinessObjectFactory();
			return factory.New<DummyBusinessObject>();
		});
		Assert.That(rendered2.FindAll(".datagrid__columns_dropdown").Count, Is.EqualTo(0));
	}

	[Test]
	public async Task TestDataGridColumnHeaderTextAlignment([Values] HorizontalAlignment alignment)
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await TestGridAsync<ZGrid>(ctx, () =>
		{
			var factory = new BusinessObjectFactory();
			return factory.New<DummyBusinessObject>();
		}, textAlign: alignment);

		Assert.That(rendered.FindAll("th")[1].Attributes["style"].Value, Does.Contain($"text-align: {alignment.ToString().ToLower()}"));
	}

	[Test]
	public async Task PositionSetOnBind()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var child1 = factory.New<DummyBaseBusinessObject>();
			child1.Z0_Code = "BAR";
			child1.Z0_Description = "This is the bar";
			dummyBizo.Collection.Add(child1);
			var child2 = factory.New<DummyBaseBusinessObject>();
			child2.Z0_Code = "FOO";
			child2.Z0_Description = "This is some foo";
			dummyBizo.Collection.Add(child2);

			var form = new WinzorTestForm();
			var grid = new ZGrid();
			grid.AfterBind += (s, e) => grid.ListManager.Position = 2;
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			form.BindingSource.SetBindingMember(grid, "Collection");
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 100));
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Description", 200));
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Decimal", 300));
			grid.LayoutCategoryPK = System.Guid.NewGuid();
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		var tr = rendered.FindAll("tbody>tr");
		Assert.That(tr[0].ChildNodes[0].TextContent, Is.EqualTo("arrow_right"));
		Assert.That(tr[1].ChildNodes[0].TextContent, Is.Empty);
	}

	[Test, WithPlaywrightPage]
	public async Task TestSelectFullTextOfCellOnReadonlyDataGridAfterClick()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var child1 = factory.New<DummyBaseBusinessObject>();
			dummyBizo.ReadOnly = true;
			child1.ReadOnly = true;
			child1.Z0_Code = "111";
			child1.Z0_DateTimeOffset = new ZDateTimeOffset(2023, 1, 1, 2, 1, 1, TimeSpan.FromHours(8));
			dummyBizo.Collection.Add(child1);

			var grid = new ZGrid() { Width = 500, Height = 300 };
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 180));
			grid.ColumnStyles.Add(new ZDateTimeOffsetEditColumnStyleInfo("Z0_DateTimeOffset", 180, ZDateTimePickerFormat.Long));
			grid.LayoutCategoryPK = Guid.NewGuid();

			var form = new WinzorTestForm() { Width = 600, Height = 500 };
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			form.BindingSource.SetBindingMember(grid, "Collection");
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			form.SetDataBinding(dummyBizo, "");
			form.Size = new Size { Width = 600, Height = 400 };

			return form;
		});

		await page.WaitForSelectorAsync(".form");
		await BrowserContext.GrantPermissionsAsync(new[] { "clipboard-write", "clipboard-read" });

		await page.Locator($".datagrid tbody>tr:nth-of-type({1})>td:nth-child({3})").ClickAsync();
		Assert.That(async () => await page.EvaluateAsync<string>("e => window.getSelection().toString()"), Is.EqualTo("01-JAN-23 02:01").After(3000, 100));
		Assert.That(async () => await page.Locator($".datagrid tbody>tr:nth-of-type({1})>td:nth-child({3}) input").EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-color')"), Is.EqualTo("rgb(240, 240, 240)").After(3000, 100));

		await page.Keyboard.PressAsync("Control+C");
		Assert.That(async () => await page.EvaluateAsync<string>("async () => await navigator.clipboard.readText()"), Is.EqualTo("01-JAN-23 02:01").After(3000, 100));

		await page.Locator($".datagrid tbody>tr:nth-of-type({1})>td:nth-child({2})").ClickAsync();
		Assert.That(async () => await page.EvaluateAsync<string>("e => window.getSelection().toString()"), Is.EqualTo("111").After(3000, 100));
		Assert.That(async () => await page.Locator($".datagrid tbody>tr:nth-of-type({1})>td:nth-child({2}) input").EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-color')"), Is.EqualTo("rgb(240, 240, 240)").After(3000, 100));

		await page.Keyboard.PressAsync("Control+C");
		Assert.That(async () => await page.EvaluateAsync<string>("async () => await navigator.clipboard.readText()"), Is.EqualTo("111").After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestSelectFullTextOfCellOnReadonlyDataGridWhenPressingTabKey()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var child1 = factory.New<DummyBaseBusinessObject>();
			child1.Z0_Code = "111";
			child1.Z0_DateTimeOffset = new ZDateTimeOffset(2023, 1, 1, 2, 1, 1, TimeSpan.FromHours(8));
			dummyBizo.Collection.Add(child1);
			dummyBizo.ReadOnly = true;
			dummyBizo.Collection.SetReadOnly(true);
			child1.ReadOnly = true;

			var grid = new ZGrid() { Width = 500, Height = 300 };
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 180));
			grid.ColumnStyles.Add(new ZDateTimeOffsetEditColumnStyleInfo("Z0_DateTimeOffset", 180, ZDateTimePickerFormat.Long));
			grid.LayoutCategoryPK = Guid.NewGuid();

			var form = new WinzorTestForm() { Width = 600, Height = 500 };
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			form.BindingSource.SetBindingMember(grid, "Collection");
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			form.SetDataBinding(dummyBizo, "");
			form.Size = new Size { Width = 600, Height = 400 };

			return form;
		});

		await page.WaitForSelectorAsync(".form");

		var targetInput = page.Locator(".datagrid__control--edit input");
		await Assertions.Expect(targetInput).ToHaveCountAsync(1);
		await Assertions.Expect(targetInput).ToBeFocusedAsync();

		await page.Keyboard.PressAsync("Tab");

		Assert.That(async () => await page.EvaluateAsync<string>("e => window.getSelection().toString()"), Is.EqualTo("01-JAN-23 02:01").After(3000, 100));

		await page.Keyboard.PressAsync("Tab");

		await Assertions.Expect(targetInput).ToHaveCountAsync(1);
		await Assertions.Expect(targetInput).ToBeFocusedAsync();

		Assert.That(async () => await page.EvaluateAsync<string>("e => window.getSelection().toString()"), Is.EqualTo("111").After(3000, 100));
	}

	[Test, Explicit("To be fixed as part of WI00921352")]
	public async Task TestNoVisibleEditControlAfterRefreshTableStyles()
	{
		using var ctx = new EnterpriseTestContext();
		ZGrid grid = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var child = factory.New<DummyBaseBusinessObject>();
			child.Z0_Code = "FOO";
			child.Z0_Description = "This is some foo";

			var dummyBizo = factory.New<DummyBusinessObject>();
			dummyBizo.Collection.Add(child);

			grid = new ZGrid();
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 100));
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Description", 200));
			grid.LayoutCategoryPK = System.Guid.NewGuid();

			var button = new Button();
			button.Text = "Refresh";
			button.Click += ButtonClick;

			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			form.BindingSource.SetBindingMember(grid, "Collection");
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			form.SetDataBinding(dummyBizo, "");
			form.Controls.Add(button);
			return form;
		});

		await rendered.FindAll("tbody>tr>td")[1].MouseDownAsync(new WebMouseEventArgs());
		Assert.That(rendered.FindAll(".datagrid__control--edit").Count, Is.EqualTo(1));

		await rendered.Find("button").ClickAsync(new WebMouseEventArgs());
		Assert.That(rendered.FindAll(".datagrid__control--edit").Count, Is.EqualTo(0));

		void ButtonClick(object sender, EventArgs e)
		{
			var grid = (ZGrid)((Button)sender).Parent.Controls.Single(c => c is ZGrid);
			grid.RefreshTableStyles();
		}
	}

	[Test, WithPlaywrightPage]
	public async Task TestTextBoxColumnWithSpacesAndLineBreaks()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var form = new WinzorTestForm { Width = 500, Height = 500 };
			var dataGrid = new ZGrid() { Size = new Size(300, 300) };
			dataGrid.BindTo = "Collection";
			dataGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_AddInfo", 100) { CharacterCasing = CharacterCasing.Normal });
			dataGrid.LayoutCategoryPK = Guid.NewGuid();
			dataGrid.ReadOnly = false;
			form.Controls.Add(dataGrid);
			form.DataSourceType = typeof(DummyBusinessObject);
			var child1 = factory.New<DummyBaseBusinessObject>();
			child1.SuspendValidationTesting();
			child1.Z0_AddInfo = "1 Space,2  Space\rline2\r\n line3\n\n  line5";
			dummyBizo.Collection.Add(child1);
			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		await page.Locator("table > tbody > tr:nth-of-type(2) td:nth-of-type(2)").ClickAsync();
		var td = page.Locator("table > tbody > tr:first-child > td:nth-child(2)");

		await Assertions.Expect(td).ToHaveTextAsync("1 Space,2  Space line2  line3    line5");
		await Assertions.Expect(td).ToHaveCSSAsync("white-space", "pre");
	}

	[Test, WithPlaywrightPage]
	public async Task TestTextBoxColumnHightWithoutMultiline()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZGrid dataGrid = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var form = new ZForm(dummyBizo) { Size = new Size(300, 300) };
			dataGrid = new ZGrid() { Size = new Size(300, 300) };
			dataGrid.BindTo = "Collection";
			dataGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_AddInfo", 50) { CharacterCasing = CharacterCasing.Normal });
			form.Controls.Add(dataGrid);
			dataGrid.LayoutCategoryPK = System.Guid.NewGuid();
			dataGrid.ReadOnly = false;
			form.DataSourceType = typeof(DummyBusinessObject);
			var child1 = factory.New<DummyBaseBusinessObject>();
			child1.SuspendValidationTesting();
			child1.Z0_AddInfo = "11";
			dummyBizo.Collection.Add(child1);
			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		await (await page.WaitForSelectorAsync("table > tbody > tr:nth-of-type(2) td:nth-of-type(2)")).ClickAsync();
		var activeTextBox = await page.WaitForSelectorAsync("table > tbody > tr:nth-of-type(2) td:nth-of-type(2) .textbox");

		Assert.That(async () => (int)(await activeTextBox.BoundingBoxAsync()).Height, Is.EqualTo(13).After(500, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestTextBoxColumnHightWithMultiline()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZGrid dataGrid = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var form = new ZForm(dummyBizo) { Size = new Size(300, 300) };
			dataGrid = new ZGrid() { Size = new Size(300, 300) };
			dataGrid.BindTo = "Collection";
			dataGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_AddInfo", 50) { CharacterCasing = CharacterCasing.Normal });
			form.Controls.Add(dataGrid);
			dataGrid.LayoutCategoryPK = System.Guid.NewGuid();
			dataGrid.ReadOnly = false;
			new RowResizer(dataGrid, "Z0_AddInfo");
			form.DataSourceType = typeof(DummyBusinessObject);
			var child1 = factory.New<DummyBaseBusinessObject>();
			child1.SuspendValidationTesting();
			child1.Z0_AddInfo = "11";

			dummyBizo.Collection.Add(child1);
			form.SetDataBinding(dummyBizo, "");
			return form;
		});
		await (await page.WaitForSelectorAsync("table > tbody > tr:nth-of-type(1) td:nth-of-type(2)")).ClickAsync();
		var activeTextBox = await page.WaitForSelectorAsync("table > tbody > tr:nth-of-type(1) td:nth-of-type(2) .textbox");
		Assert.That(async () => (int)(await activeTextBox.BoundingBoxAsync()).Height, Is.EqualTo(17).After(500, 100));

		await page.Keyboard.DownAsync("Shift");
		await page.Keyboard.PressAsync("Enter");
		await page.Keyboard.UpAsync("Shift");
		await activeTextBox.PressAsync("9");

		await (await page.WaitForSelectorAsync("table > tbody > tr:nth-of-type(2) td:nth-of-type(2)")).ClickAsync();
		await page.WaitForSelectorAsync("table > tbody > tr:nth-of-type(2) td:nth-of-type(2) .textbox");
		Assert.That(async () => (int)(await (await page.WaitForSelectorAsync("table > tbody > tr:nth-of-type(1) td:nth-of-type(2)")).BoundingBoxAsync()).Height, Is.EqualTo(34).After(500, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task GridHasCorrectHeightOnEdit()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var form = new ZForm(dummyBizo) { Size = new Size(300, 300) };
			var dataGrid = new ZGrid() { Size = new Size(300, 300) };
			dataGrid.BindTo = "Collection";
			dataGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_AddInfo", 50) { CharacterCasing = CharacterCasing.Normal });
			form.Controls.Add(dataGrid);
			dataGrid.LayoutCategoryPK = System.Guid.NewGuid();
			dataGrid.ReadOnly = false;
			new RowResizer(dataGrid, "Z0_AddInfo");
			form.DataSourceType = typeof(DummyBusinessObject);
			var child1 = factory.New<DummyBaseBusinessObject>();
			child1.SuspendValidationTesting();
			child1.Z0_AddInfo = string.Empty;

			dummyBizo.Collection.Add(child1);
			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		var cell = page.Locator("tbody > tr:first-child td:nth-child(2)");
		await Assertions.Expect(cell).ToHaveJSPropertyAsync("clientHeight", 25);

		await InsertMultiLineData();
		await Assertions.Expect(cell).ToHaveJSPropertyAsync("clientHeight", 33);

		await cell.ClickAsync();
		await Assertions.Expect(cell).ToHaveJSPropertyAsync("clientHeight", 33);

		async Task InsertMultiLineData()
		{
			await page.Keyboard.PressAsync("1");
			await Task.Delay(200);
			await page.Keyboard.PressAsync("Shift+Enter");
			await Task.Delay(200);
			await page.Keyboard.PressAsync("2");
			await Task.Delay(200);
			await page.Keyboard.PressAsync("Tab");
			await Task.Delay(200);
		}
	}

	[Test, WithPlaywrightPage]
	[TestCase(50, 29)]
	[TestCase(100, 79)]
	[TestCase(200, 179)]
	[TestCase(250, 198)]
	public async Task DataGridMultilineTextBoxShouldHaveCorrectHeightWhenFocused(int gridHeight, int textareaHeight)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			for (var i = 0; i < 20; i++)
			{
				var child = factory.New<DummyBaseBusinessObject>();
				child.Z0_Description = i.ToString();
				dummyBizo.Collection.Add(child);
			}

			var grid = new ZGrid { Width = 500, Height = gridHeight, ReadOnly = true };
			grid.ColumnStyles.Add(new ZMultiLineTextBoxColumnInfo("Z0_Description", 180));
			grid.LayoutCategoryPK = Guid.NewGuid();

			var form = new WinzorTestForm { Width = 500, Height = 200 };
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			form.BindingSource.SetBindingMember(grid, "Collection");
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			form.SetDataBinding(dummyBizo, "");

			return form;
		});

		var cell = page.Locator(".datagrid__row > td:nth-child(2)").Nth(0);
		var textBox = cell.Locator("textarea");

		await textBox.ClickAsync();
		Assert.That(async () => await textBox.EvaluateAsync<int>("e => e.clientHeight"), Is.EqualTo(textareaHeight).After(3000, 100));
	}

	[Explicit, Test, WithPlaywrightPage]
	public async Task DataGridMultilineTextBoxInSightShouldHaveCorrectHeightOnScroll()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			for (var i = 0; i < 30; i++)
			{
				var child = factory.New<DummyBaseBusinessObject>();
				child.Z0_Description = i.ToString();
				dummyBizo.Collection.Add(child);
			}

			var grid = new ZGrid { Width = 500, Height = 300, ReadOnly = false };
			grid.ColumnStyles.Add(new ZMultiLineTextBoxColumnInfo("Z0_Description", 180));
			grid.LayoutCategoryPK = Guid.NewGuid();

			var form = new WinzorTestForm { Width = 500, Height = 300 };
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			form.BindingSource.SetBindingMember(grid, "Collection");
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			form.SetDataBinding(dummyBizo, "");

			return form;
		});

		await page.WaitForSelectorAsync(".form");

		await page.ClickAsync("tbody>tr:nth-of-type(10)>td:nth-of-type(1)");
		await page.ClickAsync("tbody>tr:nth-of-type(10)>td:nth-of-type(2)");
		await page.WaitForSelectorAsync("textarea");

		await ScrollToRowAsync(page, 0);
		Assert.That(async () => await page.EvaluateAsync<int>("e => document.querySelector('.datagrid__control--edit > textarea').clientHeight"), Is.EqualTo(136).After(3000, 100));

		await ScrollToRowAsync(page, 50);
		Assert.That(async () => await page.EvaluateAsync<int>("e => document.querySelector('.datagrid__control--edit > textarea').clientHeight"), Is.EqualTo(186).After(3000, 100));

		await ScrollToRowAsync(page, 100);
		Assert.That(async () => await page.EvaluateAsync<int>("e => document.querySelector('.datagrid__control--edit > textarea').clientHeight"), Is.EqualTo(200).After(3000, 100));

		await ScrollToRowAsync(page, 150);
		Assert.That(async () => await page.EvaluateAsync<int>("e => document.querySelector('.datagrid__control--edit > textarea').clientHeight"), Is.EqualTo(200).After(3000, 100));

		await ScrollToRowAsync(page, 200);
		Assert.That(async () => await page.EvaluateAsync<int>("e => document.querySelector('.datagrid__control--edit > textarea').clientHeight"), Is.EqualTo(16).After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestTypingIntoTextBoxColumnWontLoseLetter()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZGrid dataGrid = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var form = new ZForm(dummyBizo) { Size = new Size(300, 300) };
			dataGrid = new ZGrid() { Size = new Size(300, 300) };
			dataGrid.BindTo = "Collection";
			dataGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_AddInfo", 50) { CharacterCasing = CharacterCasing.Normal });
			form.Controls.Add(dataGrid);
			dataGrid.LayoutCategoryPK = System.Guid.NewGuid();
			dataGrid.ReadOnly = false;
			form.DataSourceType = typeof(DummyBusinessObject);
			var child1 = factory.New<DummyBaseBusinessObject>();
			child1.SuspendValidationTesting();
			child1.Z0_AddInfo = "11";
			dummyBizo.Collection.Add(child1);
			form.SetDataBinding(dummyBizo, "");
			return form;
		});
		await (await page.WaitForSelectorAsync("table > tbody > tr:nth-of-type(2) td:nth-of-type(2)")).ClickAsync();
		var activeTextBox = await page.WaitForSelectorAsync("table > tbody > tr:nth-of-type(2) td:nth-of-type(2) .textbox");

		await activeTextBox.PressAsync("9");
		await Task.Delay(500);
		await activeTextBox.PressAsync("8");
		await Task.Delay(500);
		await activeTextBox.PressAsync("7");

		Assert.That(await activeTextBox.InputValueAsync(), Is.EqualTo("987"));
	}

	[Test, WithPlaywrightPage]
	public async Task TestTypingEnterWontCreateNewLine()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZGrid dataGrid = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var form = new ZForm(dummyBizo) { Size = new Size(300, 300) };
			dataGrid = new ZGrid() { Size = new Size(300, 300) };
			dataGrid.BindTo = "Collection";
			dataGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_AddInfo", 50) { CharacterCasing = CharacterCasing.Normal });
			form.Controls.Add(dataGrid);
			dataGrid.LayoutCategoryPK = System.Guid.NewGuid();
			dataGrid.ReadOnly = false;
			form.DataSourceType = typeof(DummyBusinessObject);
			var child1 = factory.New<DummyBaseBusinessObject>();
			child1.SuspendValidationTesting();
			child1.Z0_AddInfo = "11";
			dummyBizo.Collection.Add(child1);
			form.SetDataBinding(dummyBizo, "");
			return form;
		});
		await (await page.WaitForSelectorAsync("table > tbody > tr:nth-of-type(2) td:nth-of-type(2)")).ClickAsync();
		await Task.Delay(500);
		await page.Keyboard.PressAsync("Enter");
		await Task.Delay(500);

		Assert.That(await page.EvaluateAsync<int>("document.querySelectorAll('tr').length"), Is.EqualTo(3));
	}

	[Test, WithPlaywrightPage]
	public async Task TestCheckBoxOnCellCanFocusAfterPressingArrowKey()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var child1 = factory.New<DummyBaseBusinessObject>();
			child1.Z0_Code = "11";
			child1.Z0_Bool = true;
			child1.Z0_AddInfo = "13";
			dummyBizo.Collection.Add(child1);
			var child2 = factory.New<DummyBaseBusinessObject>();
			child2.Z0_Code = "21";
			child2.Z0_Bool = false;
			child2.Z0_AddInfo = "23";
			dummyBizo.Collection.Add(child2);

			var grid = new ZGrid { Width = 400, Height = 400 };
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 100) { IsReadOnly = true });
			grid.ColumnStyles.Add(new ZCheckBoxColumnStyleInfo("Z0_Bool", 100));
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_AddInfo", 100));
			grid.LayoutCategoryPK = Guid.NewGuid();

			var form = new WinzorTestForm { Width = 500, Height = 500 };
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			form.BindingSource.SetBindingMember(grid, "Collection");
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		await page.WaitForSelectorAsync("table");

		await page.Locator("table tbody tr:nth-of-type(1) td:nth-of-type(2)").ClickAsync();
		Assert.That(async () => await page.EvaluateAsync<string>("document.activeElement.value"), Is.EqualTo("11").After(2000, 100));

		await page.Keyboard.PressAsync("ArrowRight");
		Assert.That(async () => await page.EvaluateAsync<bool>("document.activeElement.checked"), Is.EqualTo(true).After(2000, 100));

		await page.Keyboard.PressAsync("ArrowRight");
		Assert.That(async () => await page.EvaluateAsync<string>("document.activeElement.value"), Is.EqualTo("13").After(2000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestCheckBoxIsProperlySized()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var child1 = factory.New<DummyBaseBusinessObject>();
			child1.Z0_Bool = true;
			dummyBizo.Collection.Add(child1);

			var grid = new ZGrid { Width = 400, Height = 400 };
			grid.ColumnStyles.Add(new ZCheckBoxColumnStyleInfo("Z0_Bool", 100));
			grid.LayoutCategoryPK = Guid.NewGuid();

			var form = new WinzorTestForm { Width = 500, Height = 500 };
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			form.BindingSource.SetBindingMember(grid, "Collection");
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		await page.Locator("tr.datagrid__row--edit").Locator("td:first-child").ClickAsync();

		var checkbox = page.Locator(".checkbox__input");

		Assert.That(async () => await checkbox.EvaluateAsync<int>("e => e.getBoundingClientRect().height"), Is.EqualTo(14).After(3000, 100));
		Assert.That(async () => await checkbox.EvaluateAsync<int>("e => e.getBoundingClientRect().width"), Is.EqualTo(14));
	}

	[Test, WithPlaywrightPage]
	public async Task TestPressKeyOnCalcEditColumnWontLoseFocus()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZGrid dataGrid = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var form = new ZForm(dummyBizo) { Size = new Size(300, 300) };

			dataGrid = new ZGrid() { Size = new Size(300, 300) };
			dataGrid.BindTo = "Collection";
			dataGrid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo("Z0_Decimal", 50, 0) { CharacterCasing = CharacterCasing.Normal, IsMandatory = true, BindToDecimalPlaces = null });

			form.Controls.Add(dataGrid);
			dataGrid.LayoutCategoryPK = System.Guid.NewGuid();
			dataGrid.ReadOnly = false;
			form.DataSourceType = typeof(DummyBusinessObject);

			var child1 = factory.New<DummyBaseBusinessObject>();
			child1.SuspendValidationTesting();
			child1.Z0_Decimal = 1;
			dummyBizo.Collection.Add(child1);

			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		await (await page.WaitForSelectorAsync("table > tbody > tr:nth-of-type(2) td:nth-of-type(2)")).ClickAsync();
		var activeTextBox = await page.WaitForSelectorAsync("table > tbody > tr:nth-of-type(2) td:nth-of-type(2) .textbox");
		await page.Keyboard.TypeAsync("9");

		Assert.That(async () => await activeTextBox.EvaluateAsync<Boolean>("e => document.activeElement === e"), Is.True.After(1000, 100));
	}

	[Test]
	public async Task EditControlHandleNotDestroyed()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			var grid = new ZGrid();
			form.BindingSource.SetBindingMember(grid, "Collection");
			var columnStyleInfo = new ZDropEditColumnStyleInfo();
			columnStyleInfo.ColumnName = "Z0_Code";
			columnStyleInfo.BindToList = "Lookups+DummyList";
			grid.ColumnStyles.Add(columnStyleInfo);
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var child = factory.New<DummyEnterpriseBusinessObject>();
			child.Z0_Code = "FOO";
			child.Z0_Description = "This is some foo";
			dummyBizo.Collection.Add(child);
			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		await rendered.FindAll("tbody>tr")[0].Children[1].MouseDownAsync(new WebMouseEventArgs());
		var input = rendered.FindAll("input").Cast<IHtmlInputElement>().Single();
		Assert.That(input.Value, Is.EqualTo("FOO"));
		var editControl = rendered.FindComponents<ControlProxyComponent>().Single(c => c.Instance.Control is ZDropEdit).Instance.Control;
		await input.FocusOutAsync();
		Assert.That(editControl.IsHandleCreated, Is.True);
	}

	[Test, WithPlaywrightPage]
	[TestCase(300, 200, 198)]
	[TestCase(200, 200, 181)]
	public async Task TestDropEditButtonPositionIsResponsiveToVisibleGrid(int gridWidth, int columnWidth, int expectedButtonPosition)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			var grid = new ZGrid() { Width = gridWidth, Height = 300 };
			form.BindingSource.SetBindingMember(grid, "Collection");

			var dropEditColumn = new ZDropEditColumnStyleInfo
			{
				ColumnName = "Z0_Code",
				BindToList = "Lookups+DummyList",
				Width = columnWidth
			};

			grid.ColumnStyles.Add(dropEditColumn);
			grid.LayoutCategoryPK = System.Guid.NewGuid();
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		var dropButton = await page.WaitForSelectorAsync("button");
		Assert.That(async () => await dropButton.EvaluateAsync<string>("e=>e.style.left"), Is.EqualTo($"{expectedButtonPosition}px").After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestEditControlShouldHaveSameWidthAsOriginalCellIfAtLastColumn()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(RefAirline);
			var grid = new ZGrid { Width = 300, Height = 300 };
			form.BindingSource.SetBindingMember(grid, "EFreightStatusCollection");

			var columnStyleInfo1 = new ZTextBoxColumnStyleInfo
			{
				ColumnName = "RME_DestinationLocation",
				Width = 200,
				IsReadOnly = false
			};
			grid.ColumnStyles.Add(columnStyleInfo1);

			var columnStyleInfo2 = new ZDropEditColumnStyleInfo
			{
				ColumnName = "RME_EFreightStatus",
				ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode,
				Width = 200
			};

			grid.ColumnStyles.Add(columnStyleInfo2);
			form.Controls.Add(grid);

			form.DataSourceType = typeof(RefAirline);
			var factory = new BusinessObjectFactory();
			var refAirLine = factory.New<RefAirline>();
			form.SetDataBinding(refAirLine, "");
			return form;
		});

		await page.Locator(".datagrid__control--edit input").WaitForAsync();
		await page.Keyboard.InsertTextAsync("SGSIN");

		var cell = page.Locator("table > tbody > tr:nth-of-type(1) td:nth-of-type(3)");
		await cell.WaitForAsync();
		Assert.That(() => cell.EvaluateAsync<int>("el => el.getBoundingClientRect().width"), Is.EqualTo(200).After(1000, 100));

		await cell.ClickAsync();
		var button = page.Locator("button").First;
		Assert.That(() => button.EvaluateAsync<string>("el => el.style.left"), Is.EqualTo("181px").After(1000, 100));

		var editControl = cell.Locator("div div").First;
		await editControl.WaitForAsync();
		Assert.That(() => editControl.EvaluateAsync<int>("el => el.getBoundingClientRect().width"), Is.EqualTo(199).After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	[TestCase(50)]
	[TestCase(100)]
	[TestCase(150)]
	[TestCase(200)]
	public async Task ExpandingTextareaDoesNotIncreaseDatagridScrollHeight(int gridHeight)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			for (var i = 0; i < 20; i++)
			{
				var child = factory.New<DummyBaseBusinessObject>();
				child.Z0_Description = i.ToString();
				dummyBizo.Collection.Add(child);
			}

			var grid = new ZGrid { Width = 500, Height = gridHeight, ReadOnly = true };
			grid.ColumnStyles.Add(new ZMultiLineTextBoxColumnInfo("Z0_Description", 180));
			grid.LayoutCategoryPK = Guid.NewGuid();

			var form = new WinzorTestForm { Width = 500, Height = 200 };
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			form.BindingSource.SetBindingMember(grid, "Collection");
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			form.SetDataBinding(dummyBizo, "");

			return form;
		});

		var dataGrid = page.Locator(".datagrid");
		await dataGrid.WaitForAsync();

		await page.ClickAsync("table tr:nth-child(1) td:nth-child(1)");
		var scrollHeight1 = await dataGrid.EvaluateAsync<int>("e => e.scrollHeight");
		await page.ClickAsync("table tr:nth-child(1) td:nth-child(2)");
		var scrollHeight2 = await dataGrid.EvaluateAsync<int>("e => e.scrollHeight");
		Assert.That(scrollHeight1, Is.EqualTo(scrollHeight2));
	}

	[Test, WithPlaywrightPage]
	public async Task TestEditControlShouldHaveLongerWidthThanOriginalCellIfNotAtLastColumn()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(RefAirline);
			var grid = new ZGrid() { Width = 300, Height = 300 };
			form.BindingSource.SetBindingMember(grid, "EFreightStatusCollection");

			var columnStyleInfo1 = new ZTextBoxColumnStyleInfo
			{
				ColumnName = "RME_DestinationLocation",
				Width = 200,
				IsReadOnly = false
			};
			var columnStyleInfo2 = new ZDropEditColumnStyleInfo
			{
				ColumnName = "RME_EFreightStatus",
				ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode,
				Width = 200
			};
			grid.ColumnStyles.Add(columnStyleInfo2);
			grid.ColumnStyles.Add(columnStyleInfo1);
			form.Controls.Add(grid);

			form.DataSourceType = typeof(RefAirline);
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<RefAirline>();
			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		var editSelector = $"table > tbody > tr:nth-of-type(1) td:nth-of-type(2) div div";
		var cellHandle = await page.WaitForSelectorAsync(editSelector);

		IElementHandle buttonHandler = null;
		Assert.That(async () => (buttonHandler = await page.QuerySelectorAsync("button")), Is.Not.Null);
		Assert.That(async () => await buttonHandler.EvaluateAsync<string>("e => e.style.left"), Is.EqualTo("198px"));
		Assert.That(async () => await cellHandle.EvaluateAsync<int>("el => el.getBoundingClientRect().width"), Is.EqualTo(216).After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestZCheckBoxColumnShouldNotAcceptTabIfReadOnly()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZGrid grid = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var child1 = factory.New<DummyBaseBusinessObject>();
			child1.Z0_Code = "1";
			child1.Z0_Bool = true;
			dummyBizo.Collection.Add(child1);
			var child2 = factory.New<DummyBaseBusinessObject>();
			child2.Z0_Code = "1";
			child2.Z0_Bool = false;
			dummyBizo.Collection.Add(child2);
			var form = new WinzorTestForm();
			grid = new ZGrid();
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 400));
			grid.ColumnStyles.Add(new ZCheckBoxColumnStyleInfo("Z0_Bool", 200) { IsReadOnly = true });
			grid.LayoutCategoryPK = System.Guid.NewGuid();
			grid.Dock = DockStyle.Fill;
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			form.BindingSource.SetBindingMember(grid, "Collection");
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			form.SetDataBinding(dummyBizo, "");
			form.Size = new Size { Width = 600, Height = 400 };
			return form;
		});

		var input = page.Locator("table tr:nth-of-type(1) td:nth-of-type(3) input");
		await input.WaitForAsync(new() { State = WaitForSelectorState.Attached });
		Assert.That(async () => await input.InputValueAsync(), Is.Not.Null);
		Assert.That(async () => await input.GetAttributeAsync("tabIndex"), Is.EqualTo("-1").After(1000, 100));

		var row = await page.Locator("table tr:nth-child(1) td:nth-child(2)").BoundingBoxAsync();
		await SimulateClickEvent(page, row);
		Assert.That(() => grid.CurrentCell.ColumnNumber, Is.EqualTo(0).After(1000, 100));
		Assert.That(() => grid.CurrentCell.RowNumber, Is.EqualTo(0).After(1000, 100));

		await page.Keyboard.DownAsync("Tab");
		Assert.That(() => grid.CurrentCell.ColumnNumber, Is.EqualTo(0).After(1000, 100));
		Assert.That(() => grid.CurrentCell.RowNumber, Is.EqualTo(1).After(1000, 100));
	}

	[Test, WithPlaywrightPage, WithSnapshotProtection]
	public async Task TextShouldBeAbbreviatedAfterResizeColumn()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZGrid dataGrid = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var form = new ZForm(dummyBizo) { Size = new Size(300, 300) };

			dataGrid = new ZGrid() { Size = new Size(300, 300) };
			dataGrid.BindTo = "Collection";
			var zCalcEditColumnStyleInfo = new ZCalcEditColumnStyleInfo("Z0_Decimal", 90, 0) { CharacterCasing = CharacterCasing.Normal, IsMandatory = true, BindToDecimalPlaces = null, CaptionResourceString = Res.GetData("", "short caption", "long long long caption", "") };
			dataGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo);

			form.Controls.Add(dataGrid);
			dataGrid.LayoutCategoryPK = System.Guid.NewGuid();
			dataGrid.ReadOnly = false;
			form.DataSourceType = typeof(DummyBusinessObject);

			var child1 = factory.New<DummyBaseBusinessObject>();
			child1.SuspendValidationTesting();
			child1.Z0_Decimal = 1;
			dummyBizo.Collection.Add(child1);

			form.SetDataBinding(dummyBizo, "");
			return form;
		});
		var columnHeader1 = await page.WaitForSelectorAsync("table thead tr:nth-child(1) th:nth-child(2)");

		Assert.That(async () => await columnHeader1.InnerTextAsync(), Is.EqualTo("short caption").After(500, 50));
		var columnResizer = (await page.QuerySelectorAllAsync("table thead tr:nth-child(1) th:nth-child(2) .datagrid__column_resizer"))[0];

		await MouseMoveOfElementAsync(page, columnResizer, 2, 2);
		await MouseDownAsync(page);
		await MouseMoveOfElementAsync(page, columnResizer, 100, 2);
		await MouseUpAsync(page);
		var columnHeader2 = await page.WaitForSelectorAsync("table thead tr:nth-child(1) th:nth-child(2)");
		Assert.That(async () => await columnHeader2.InnerTextAsync(), Is.EqualTo("long long long caption").After(500, 50));
	}

	[Test, WithPlaywrightPage]
	public async Task IndicatorColumnWidth()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(500, 200) };

			var table = new DataTable("data");

			var column1 = new DataColumn("col1");
			column1.DataType = typeof(string);
			table.Columns.Add(column1);

			var dataGrid = new ZGrid();
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;
			form.Controls.Add(dataGrid);

			return form;
		});

		var th = await page.WaitForSelectorAsync(".datagrid th:first-child");
		Assert.That(await th.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("35px"));
	}

	[Test, WithPlaywrightPage(Headless = false)]
	public async Task TestKeyEventsInBigGrid()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = SetupBigGrid(false); // 20 columns & 50 rows, 1 page has 28 rows (476px height, 19px header, 17px horizontal scrollbar)
			return form;
		});

		var columnHeader = await page.Locator(".datagrid table thead th:nth-of-type(2) span").BoundingBoxAsync();
		await SimulateClickEvent(page, columnHeader);
		Assert.That(async () => await page.Locator(".datagrid table thead th:nth-of-type(2)").GetAttributeAsync("class"), Does.Contain("datagrid__header--ascending").After(2000, 100));

		AssertGridInputValue(page, "10.10");
		await page.Keyboard.PressAsync("End");
		AssertGridInputValue(page, "10.29");
		await page.Keyboard.PressAsync("End");
		AssertGridInputValue(page, "10.29");
		await page.Keyboard.PressAsync("ArrowLeft");
		AssertGridInputValue(page, "10.28");
		await page.Keyboard.PressAsync("ArrowRight");
		AssertGridInputValue(page, "10.29");
		await page.Keyboard.PressAsync("ArrowRight");
		AssertGridInputValue(page, "11.10");

		await page.Keyboard.PressAsync("ArrowUp");
		AssertGridInputValue(page, "10.10");
		await page.Keyboard.PressAsync("Home");
		AssertGridInputValue(page, "10.10");
		await page.Keyboard.PressAsync("ArrowLeft");
		AssertGridInputValue(page, "10.10");
		await page.Keyboard.PressAsync("ArrowRight");
		AssertGridInputValue(page, "10.11");
		await page.Keyboard.PressAsync("ArrowRight");
		AssertGridInputValue(page, "10.12");

		await page.Keyboard.PressAsync("ArrowUp");
		AssertGridInputValue(page, "10.12");
		await page.Keyboard.PressAsync("ArrowDown");
		AssertGridInputValue(page, "11.12");

		await page.Keyboard.PressAsync("PageUp");
		AssertGridInputValue(page, "10.12");
		await page.Keyboard.PressAsync("PageDown");
		AssertGridInputValue(page, "38.12");
		await page.Keyboard.PressAsync("ArrowDown");
		AssertGridInputValue(page, "39.12");
		await page.Keyboard.PressAsync("ArrowDown");
		AssertGridInputValue(page, "40.12");
		await page.Keyboard.PressAsync("PageDown");
		AssertGridInputValue(page, "59.12");
		await page.Keyboard.PressAsync("ArrowUp");
		AssertGridInputValue(page, "58.12");
		await page.Keyboard.PressAsync("ArrowUp");
		AssertGridInputValue(page, "57.12");
		await page.Keyboard.PressAsync("PageUp");
		AssertGridInputValue(page, "29.12");

		var input = page.Locator(".datagrid input");
		await input.FocusAsync();
		await input.FillAsync("12345");
		Assert.That(async () => await page.Locator(".datagrid input").EvaluateAsync<int>("element => element.selectionStart"), Is.EqualTo(5).After(2000, 100));
		await page.Locator(".datagrid input").EvaluateAsync<bool>("element => element.setSelectionRange(0, 0);");
		Assert.That(async () => await page.Locator(".datagrid input").EvaluateAsync<int>("element => element.selectionStart"), Is.EqualTo(0).After(2000, 100));
		await page.Keyboard.PressAsync("End");
		Assert.That(async () => await page.Locator(".datagrid input").EvaluateAsync<int>("element => element.selectionStart"), Is.EqualTo(5).After(2000, 100));
		await page.Keyboard.PressAsync("Home");
		Assert.That(async () => await page.Locator(".datagrid input").EvaluateAsync<int>("element => element.selectionStart"), Is.EqualTo(0).After(5000, 100));

		await page.Locator(".datagrid input").EvaluateAsync<bool>("element => element.setSelectionRange(0, 5);");
		await page.Keyboard.PressAsync("Control+Home");
		AssertGridInputValue(page, "10.10");
		await page.Keyboard.PressAsync("Control+End");
		AssertGridInputValue(page, "59.29");
	}

	void AssertGridInputValue(IPage page, string expectedInput)
	{
		Assert.That(() => page.Locator(".datagrid input").InputValueAsync(), Is.EqualTo(expectedInput).After(2000, 100));
	}

	[Test]
	public async Task CtrlVPasteInZCalcEditColumnStyleDoesNotTriggerPasteFromServer()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();

			var factory = new BusinessObjectFactory();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			form.DataSourceType = typeof(DummyBusinessObject);

			var dummyBizo = factory.New<DummyBusinessObject>();
			form.SetDataBinding(dummyBizo, "");

			var dataGrid = new ZGrid() { Width = 350, Height = 300 };
			form.BindingSource.SetBindingMember(dataGrid, "Collection");
			dataGrid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo("Z0_Decimal", 100, 0, false));
			form.Controls.Add(dataGrid);

			return form;
		});

		var input = rendered.FindAll("table tbody tr:nth-of-type(1) td:nth-of-type(2) div input").Cast<IHtmlInputElement>().Single();
		await rendered.KeyPressAsync(Keys.Control | Keys.V, input);

		Assert.That(ctx.JSInterop.Invocations.Any(i => i.Identifier == "clipboard.paste"), Is.False);
	}

	[Test, WithPlaywrightPage]
	public async Task DoesNotEmitInputInNewRowWhenEditingZCalcEditColumn()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var form = new ZForm(dummyBizo) { Size = new Size(300, 300) };

			var dataGrid = new ZGrid { Size = new Size(300, 300) };
			dataGrid.BindTo = "Collection";
			var zCalcEditColumnStyleInfo = new ZCalcEditColumnStyleInfo("Z0_Decimal", 90, 0) { CharacterCasing = CharacterCasing.Normal, IsMandatory = true };
			dataGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo);

			form.Controls.Add(dataGrid);
			dataGrid.LayoutCategoryPK = Guid.NewGuid();
			dataGrid.ReadOnly = false;
			form.DataSourceType = typeof(DummyBusinessObject);

			var child1 = factory.New<DummyBaseBusinessObject>();
			child1.Z0_Decimal = 1;
			dummyBizo.Collection.Add(child1);

			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		await page.Locator("tbody>tr:nth-of-type(2)>td:nth-child(2)").ClickAsync();
		Assert.That(async () => await page.EvaluateAsync<string>("e => window.getSelection().toString()"), Is.EqualTo("0").After(3000, 100));

		var input = page.Locator("input");
		await input.WaitForAsync();
		await input.PressAsync("8");
		Assert.That(async () => await page.EvaluateAsync<string>("e => window.getSelection().toString()"), Is.EqualTo(string.Empty).After(3000, 100));
		await Assertions.Expect(input).ToHaveValueAsync("8");
	}

	[Test, WithPlaywrightPage]
	[WithLatencyNetworkEffect(Listen = ProxyListen, Upstream = ProxyUpstream, ProxyName = ProxyName, Latency = 150)]
	public async Task TypeWholeWordWithZCustomControlColumnStyle()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizO = factory.New<DummyBusinessObject>();
			var form = new ZForm(dummyBizO);
			var grid = new ZGrid();
			var styleInfo = new ZDynamicMultilineTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_NVarChar, 200);
			styleInfo.CharacterCasing = CharacterCasing.Lower;
			grid.ColumnStyles.Add(styleInfo);
			form.Controls.Add(new Button());
			form.Controls.Add(grid);
			grid.SetDataBinding(dummyBizO.Collection, "");
			return form;
		});

		await page.Locator("tbody>tr>td:nth-child(2)").ClickAsync();
		var inputBox = page.Locator("input");
		await inputBox.WaitForAsync();
		await inputBox.PressSequentiallyAsync("hello", new () { Delay = 100 });

		await Assertions.Expect(inputBox).ToHaveValueAsync("hello");
		Assert.That(() => inputBox.EvaluateAsync<int>("e => e.selectionStart"), Is.EqualTo(5).After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestMouseDoubleClickShouldNotFireWhenEditControlPresentOnCell([Values] bool isEditControlPresent, [Values] bool readOnly)
	{
		var mouseDoubleClickCalled = false;
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
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 400) { IsReadOnly = readOnly });
			grid.ColumnStyles.Add(new ZCheckBoxColumnStyleInfo("Z0_Bool", 200) { IsReadOnly = readOnly });

			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			form.BindingSource.SetBindingMember(grid, "Collection");
			grid.IsWholeRowSelectedOnClick = !isEditControlPresent;
			grid.MouseDoubleClick += (_, __) => { mouseDoubleClickCalled = true; };
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			form.SetDataBinding(dummyBizo, "");
			form.Size = new Size { Width = 600, Height = 400 };
			return form;
		});
		var rowHeader = page.Locator("table tbody tr:nth-of-type(1) td:nth-of-type(1)");
		await rowHeader.DblClickAsync();
		Assert.That(() => mouseDoubleClickCalled, Is.True.After(3000, 100));
		var cell = page.Locator("table tbody tr:nth-of-type(1) td:nth-of-type(2)");
		mouseDoubleClickCalled = false;
		await cell.DblClickAsync();
		Assert.That(() => mouseDoubleClickCalled, Is.Not.EqualTo(isEditControlPresent).After(3000, 100));
	}

	[Test, WithPlaywrightPage, WithSnapshotProtection]
	public async Task DraggableDataShouldMatchWhenVerticalScrollbarExists()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		string bizPK = string.Empty; string[] docPK = new string[50];
		var page = await ctx.LoadFormAsync(() =>
		{
			var data = "data";
			var dataArray = Encoding.UTF32.GetBytes(data);
			var form = new WinzorTestForm();
			var factory = new BusinessObjectFactory();
			var docFactory = new DbBackendDocumentFactory(factory);
			var numFactory = new NumberedBusinessObjectFactory(0, docFactory);
			var parent = factory.New<DummyBusinessObject>();

			var storage = docFactory.New<StorageMain>();
			storage.SM_DB = 0;
			storage.SM_ParentFK = parent.PK;
			bizPK = parent.PK.ToString();

			for (int i = 0; i < 50; i++)
			{
				var file = StorageFile.NewWithParent_DEBUG(numFactory);
				file.SC_ImageData = new ZBlob(dataArray);
				file.SC_FileName = "test" + i.ToString();
				file.SC_DataType = "txt";
				storage.eDocs.Add(file);
				docPK[i] = file.PK.ToString();
			}

			var grid = new DocumentsZGrid() {  Width = 500, Height = 500 };
			var column = new ZTextBoxColumnStyleInfo() { ColumnName = "SC_FileNameWithExtension" };
			grid.AllowBeginDrag = true;
			grid.CopySelectedRowsAllowed = true;
			grid.AllowDragDropWithChanges = true;
			grid.ReadOnly = true;
			grid.Columns.Add(column);
			form.BindingSource.DataSourceType = typeof(StorageMain);
			form.BindingSource.SetBindingMember(grid, "eDocsView");
			form.SetDataBinding(storage, "");
			form.Controls.Add(grid);
			return form;
		});

		var totalRowsInGrid = await page.Locator(".datagrid tbody>tr").CountAsync();
		Assert.That(totalRowsInGrid, Is.LessThan(50));

		await page.EvaluateAsync("document.querySelector('.datagrid').scrollTo(0, 9999);");

		for (int i = 1; i <= totalRowsInGrid; i++)
		{
			var td = page.Locator($"table tbody tr:nth-of-type({i}) td").First;

			await Assertions.Expect(td).ToHaveCountAsync(1);

			var row = await td.BoundingBoxAsync();
			await SimulateClickEvent(page, row);

			var selectedRow = page.Locator("tr.datagrid__row--selected");

			await Assertions.Expect(selectedRow).ToHaveCountAsync(1);

			await Assertions.Expect(td).ToHaveAttributeAsync("draggable", "true");

			var dataRowNumber = await page.Locator($".datagrid tbody>tr:nth-of-type({i})").GetAttributeAsync("data-row-number");

			var dataTransfer = await page.EvaluateHandleAsync("() => new DataTransfer()");
			await td.DispatchEventAsync("dragstart", new { dataTransfer });
			var file = await dataTransfer.EvaluateHandleAsync("(transfer) => transfer.getData('DownloadURL')");

			Assert.That(file.ToString(), Is.EqualTo($"application/octet-stream:test{dataRowNumber}.txt:{ctx.ServerBaseUrl}/edoc/download/test{dataRowNumber}.txt?docPK={docPK[int.Parse(dataRowNumber)]}&bizPK={bizPK}"));
			Assert.That(File.Exists(CargoWise.IO.Temp.TempPath + "test.txt"), Is.False);
		}
	}

	async Task<IRenderedComponent<ControlProxyComponent>> TestGridAsync<T>(EnterpriseTestContext ctx, Func<DummyBusinessObject> bo, CharacterCasing casing = CharacterCasing.Upper, bool readOnly = false, HorizontalAlignment textAlign = HorizontalAlignment.Left, int height = 0) where T : ZGrid, new()
	{
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			var grid = new T();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			form.BindingSource.SetBindingMember(grid, "Collection");
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 100) { CharacterCasing = casing, TextAlign = textAlign });
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Description", 200) { CharacterCasing = casing, TextAlign = textAlign });
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Decimal", 300) { CharacterCasing = casing, TextAlign = textAlign });
			grid.LayoutCategoryPK = System.Guid.NewGuid();
			grid.ReadOnly = readOnly;
			grid.Height = height > 0 ? height : grid.Height;
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			form.SetDataBinding(bo(), "");
			return form;
		});

		return rendered;
	}

	ZGrid CreateNewZGrid()
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

		for (int i = 0; i < 50; i++)
		{
			table.Rows.Add($"cell({i + 1}, 1)", $"cell({i + 1}, 2)", $"cell({i + 1}, 3)");
		}

		var dataGrid = new ZGrid();
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

		var columnStyle3 = new DataGridTextBoxColumn { MappingName = "three", HeaderText = "Three Header" };
		columnStyle3.Width = 180;
		tableStyle.GridColumnStyles.Add(columnStyle3);

		dataGrid.TableStyles.Add(tableStyle);
		dataGrid.Columns.AddTextColumn("One Header", 80);
		dataGrid.Columns.AddTextColumn("Two Header", 180);
		dataGrid.Columns.AddTextColumn("Three Header", 180);

		return dataGrid;
	}

	WinzorTestForm SetupBigGrid(bool readOnly)
	{
		DataTable table = null;
		ZGrid dataGrid = null;
		var form = new WinzorTestForm() { Width = 300, Height = 500 };

		table = new DataTable("data");

		for (int i = 0; i < 20; i++)
		{
			var charValue = ((char)(65 + i)).ToString(); // A is 65
			var column = new DataColumn(charValue);
			column.ColumnName = charValue;
			column.DataType = typeof(string);
			table.Columns.Add(column);
		}

		for (int i = 0; i < 50; i++)
		{
			var row = table.NewRow();
			for (int j = 0; j < 20; j++)
			{
				var charValue = ((char)(65 + j)).ToString();
				row[charValue] = $"{i + 10}.{j + 10}";
			}
			table.Rows.Add(row);
		}

		dataGrid = new ZGrid() { Width = 300, Height = 500 };
		dataGrid.Dock = DockStyle.Fill;
		dataGrid.DataSource = table;
		dataGrid.AllowSorting = true;
		dataGrid.AllowNavigation = false;
		dataGrid.AutoSize = false;
		dataGrid.LayoutCategoryPK = System.Guid.NewGuid();
		var tableStyle = new DataGridTableStyle { MappingName = "data" };

		for (int i = 0; i < 20; i++)
		{
			var charValue = ((char)(65 + i)).ToString();
			var column = new DataColumn();
			column.DataType = typeof(string);
			table.Columns.Add(column);

			var columnStyle = new ZTextBoxColumnStyle(new ZTextBoxColumnStyleInfo(charValue, 100))
			{
				MappingName = charValue,
				ReadOnly = readOnly,
				HeaderText = $"{charValue} Header",
				CharacterCasing = CharacterCasing.Upper,
				Alignment = HorizontalAlignment.Left
			};
			tableStyle.GridColumnStyles.Add(columnStyle);
		}

		dataGrid.TableStyles.Add(tableStyle);

		form.Controls.Add(dataGrid);
		return form;
	}

	void AssertNumSelectedRowsInGrid(ZGrid dataGrid, int pollingDelay, int expectedSelectedRows)
	{
		var field = dataGrid.GetType().BaseType.GetField("numSelectedRows", BindingFlags.Instance | BindingFlags.NonPublic);
		Assert.That(() => (int)field.GetValue(dataGrid), Is.EqualTo(expectedSelectedRows).After(pollingDelay, 100));
	}

	//fixed in WI00696277, where we made the cell change merely on focus in instead of mousedown
	async Task SimulateClickOnCellForRowChange(IElement element)
	{
		await element.TriggerEventAsync("onwinzorfocusin", new WinzorFocusInEventArgs());
	}

	async Task MouseDownAsync(IPage page, MouseDownOptions options = default)
	{
		await page.Mouse.DownAsync(options);
		await Task.Delay(500);
	}

	async Task MouseMoveOfElementAsync(IPage page, IElementHandle elementHandle, float x, float y)
	{
		var elementRect = await elementHandle.BoundingBoxAsync();
		await page.Mouse.MoveAsync(elementRect.X + x, elementRect.Y + y);
		await Task.Delay(500);
	}

	async Task MouseMoveOfElementAsync(IPage page, ILocator locator, float x, float y)
	{
		var elementRect = await locator.BoundingBoxAsync();
		await page.Mouse.MoveAsync(elementRect.X + x, elementRect.Y + y);
		await Task.Delay(500);
	}

	async Task MouseUpAsync(IPage page, MouseUpOptions options = default)
	{
		await page.Mouse.UpAsync(options);
		await Task.Delay(500);
	}

	async Task SimulateClickEvent(IPage page, LocatorBoundingBoxResult location, MouseButton mouseButton = default)
	{
		await page.Mouse.MoveAsync(location.X + location.Width / 2, location.Y + location.Height / 2);
		await Task.Delay(200);
		await MouseDownAsync(page);
		await MouseUpAsync(page);
	}

	async Task SimulateClickEvent(IPage page, IElementHandle elementHandle, MouseButton mouseButton = default)
	{
		var elementRect = await elementHandle.BoundingBoxAsync();
		await page.Mouse.MoveAsync(elementRect.X + elementRect.Width / 2, elementRect.Y + elementRect.Height / 2);
		await Task.Delay(200);
		await MouseDownAsync(page, new MouseDownOptions { Button = mouseButton });
		await MouseUpAsync(page, new MouseUpOptions { Button = mouseButton });
	}

	async Task ScrollToRowAsync(IPage page, int scrollTop)
	{
		await page.EvaluateAsync($"document.getElementsByClassName('datagrid')[0].scrollTo({{left:0, top:{scrollTop}, behavior: 'smooth'}});");
		await Task.Delay(1000);
	}
}
