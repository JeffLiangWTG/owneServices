using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Aga.Controls.Tree;
using Aga.Controls.Tree.NodeControls;
using Bunit;
using Enterprise.ZArchitecture.GUI;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorFramework.Extensions;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using static WTG.PlaywrightTesting.PlaywrightTestContext;

namespace Enterprise.Winzor.Architecture.Test;

class TreeViewAdvTest
{
	[Test]
	public async Task TreeViewAdvStyle()
	{
		using var ctx = new EnterpriseTestContext();
		TreeViewAdv tree = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			tree = new TreeViewAdv() { Size = new Size(100, 100) };
			return tree;
		});
		var treeViewAdv = rendered.Find(".treeviewadv");

		Assert.That(treeViewAdv.Attributes["class"].Value, Does.Contain("treeviewadv"));
		Assert.That(tree.UseParentDivForLayout, Is.False);
	}

	[Test, WithPlaywrightPage]
	public async Task TreeViewAdvBorderStyleFixedSingle()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => new TreeViewAdv
		{
			Size = new Size(300, 200),
			BorderStyle = BorderStyle.FixedSingle
		});

		var treeViewAdv = page.Locator(".treeviewadv");

		Assert.That(await treeViewAdv.GetAttributeAsync("class"), Does.Contain("treeviewadv--border-fixedsingle"));
		Assert.That(await treeViewAdv.GetComputedStyleAsync("border"), Is.EqualTo("1px solid rgb(100, 100, 100)"));
	}

	[Test, WithPlaywrightPage]
	public async Task TreeViewAdvBorderStyleFixed3D()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => new TreeViewAdv
		{
			Size = new Size(300, 200),
			BorderStyle = BorderStyle.Fixed3D
		});

		var treeViewAdv = page.Locator(".treeviewadv");

		Assert.That(await treeViewAdv.GetAttributeAsync("class"), Does.Contain("treeviewadv--border-fixed3d"));
		Assert.That(await treeViewAdv.GetComputedStyleAsync("border-top"), Is.EqualTo("2px ridge rgb(160, 160, 160)"));
		Assert.That(await treeViewAdv.GetComputedStyleAsync("border-right"), Is.EqualTo("2px groove rgb(255, 255, 255)"));
		Assert.That(await treeViewAdv.GetComputedStyleAsync("border-bottom"), Is.EqualTo("2px groove rgb(255, 255, 255)"));
		Assert.That(await treeViewAdv.GetComputedStyleAsync("border-left"), Is.EqualTo("2px ridge rgb(160, 160, 160)"));
	}

	[Test, WithPlaywrightPage]
	public async Task TreeViewAdvHeaderComputedStyle()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Size = new Size(600, 400) };
			var treeViewAdv = new TreeViewAdv { Size = new Size(300, 200), Model = CreateTreeModel() };
			CreateTreeViewContent(treeViewAdv);

			treeViewAdv.ExpandAll();
			form.Controls.Add(treeViewAdv);
			return form;
		});
		var treeview = await page.WaitForSelectorAsync(".treeviewadv");
		Assert.That(treeview, Is.Not.Null);

		var headers = await page.WaitForSelectorAsync(".treeviewadv__columnheaders");
		Assert.That(await headers.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("450px"));

		var header = await headers.QuerySelectorAsync(".treeviewadv__columnheader:nth-child(2)");
		Assert.That(await header.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('position')"), Is.EqualTo("absolute"));
		Assert.That(await header.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('display')"), Is.EqualTo("flex"));

		var text = await header.QuerySelectorAsync("p");
		Assert.That(await text.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('overflow')"), Is.EqualTo("hidden"));
		Assert.That(await text.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('text-overflow')"), Is.EqualTo("ellipsis"));
		Assert.That(await text.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('white-space')"), Is.EqualTo("nowrap"));

		var resizer = await header.QuerySelectorAsync(".treeviewadv__columnheader-resizer");
		Assert.That(await resizer.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('position')"), Is.EqualTo("absolute"));
	}

	[Test, WithPlaywrightPage]
	public async Task TreeViewAdvItemComputedStyle()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var treeViewAdv = new TreeViewAdv { Size = new Size(300, 200), Model = CreateTreeModel() };
			CreateTreeViewContent(treeViewAdv);
			treeViewAdv.ExpandAll();
			return treeViewAdv;
		});

		var text = page.Locator(".treeviewadv .treeviewadv__item:nth-child(2) p").First;
		Assert.That(await text.GetComputedStyleAsync("overflow"), Is.EqualTo("hidden"));
		Assert.That(await text.GetComputedStyleAsync("text-overflow"), Is.EqualTo("ellipsis"));
		Assert.That(await text.GetComputedStyleAsync("white-space"), Is.EqualTo("nowrap"));
		Assert.That(await text.GetComputedStyleAsync("line-height"), Is.EqualTo("16px"));
	}

	[Test]
	public async Task TreeViewModel()
	{
		TreeViewAdv treeViewAdv = null;
		var model = CreateTreeModel();
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			treeViewAdv = new TreeViewAdv() { Size = new Size(100, 100) };
			return treeViewAdv;
		});

		await treeViewAdv.InvokeWinzorDispatcherAsync(() => treeViewAdv.Model = model);

		Assert.That(model.Nodes.Count(node => node.Text == "level 1"), Is.EqualTo(2));
		foreach (var child in model.Nodes)
		{
			Assert.That(child.Nodes.Count(node => node.Text == "level 2"), Is.EqualTo(2));
		}
	}

	[Test]
	public async Task TreeViewWithColumnHeaders()
	{
		TreeViewAdv treeViewAdv = null;
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			treeViewAdv = new TreeViewAdv() { Size = new Size(100, 100) };
			CreateTreeViewContent(treeViewAdv);

			return treeViewAdv;
		});

		var headersText = rendered.FindAll(".treeviewadv__columnheaders div p");

		Assert.That(headersText[0].ParentElement.Attributes["style"].Value, Does.Contain("width:100px;"));
		Assert.That(headersText[1].ParentElement.Attributes["style"].Value, Does.Contain("width:150px;"));

		Assert.That(headersText[0].TextContent, Is.EqualTo("Column 1"));
		Assert.That(headersText[1].TextContent, Is.EqualTo("Column 2"));
	}

	[Test, WithPlaywrightPage]
	public async Task TreeViewWithoutWrappedColumnHeaders()
	{
		TreeViewAdv treeViewAdv = null;
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(500, 200) };
			treeViewAdv = new TreeViewAdv() { Size = new Size(100, 100) };
			treeViewAdv.UseColumns = true;
			treeViewAdv.Columns.Add(new TreeColumn() { Width = 100, Header = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Morbi egestas, velit et congue vehicula" });
			treeViewAdv.Columns.Add(new TreeColumn() { Width = 150, Header = "Column 2" });
			form.Controls.Add(treeViewAdv);

			return form;
		});

		var firstColumn = await page.WaitForSelectorAsync(".treeviewadv__columnheaders div:nth-child(1) p");
		Assert.That(async () => await firstColumn.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('height')"), Is.EqualTo("20px"));
	}

	[Test]
	public async Task TreeViewWithColumnHeaderHasCorrectClass()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var treeViewAdv = new TreeViewAdv { Size = new Size(100, 100) };
			treeViewAdv.UseColumns = true;
			treeViewAdv.Columns.Add(new TreeColumn() { Width = 100, Header = "Column 1", SortOrder = SortOrder.Ascending });
			treeViewAdv.Columns.Add(new TreeColumn() { Width = 150, Header = "Column 2", SortOrder = SortOrder.Descending });
			treeViewAdv.Columns.Add(new TreeColumn() { Width = 150, Header = "Column 3", SortOrder = SortOrder.None });
			return treeViewAdv;
		});

		var columnHeaders = rendered.FindAll(".treeviewadv__columnheaders div span");

		Assert.That(columnHeaders.Count, Is.EqualTo(6));
		Assert.That(columnHeaders[0].GetAttribute("class"), Is.EqualTo("treeviewadv__columnheader-sortmark--ascending"));
		Assert.That(columnHeaders[1].GetAttribute("class"), Is.EqualTo("treeviewadv__columnheader-resizer"));
		Assert.That(columnHeaders[2].GetAttribute("class"), Is.EqualTo("treeviewadv__columnheader-sortmark--descending"));
		Assert.That(columnHeaders[3].GetAttribute("class"), Is.EqualTo("treeviewadv__columnheader-resizer"));
		Assert.That(columnHeaders[4].GetAttribute("class"), Is.Empty);
		Assert.That(columnHeaders[5].GetAttribute("class"), Is.EqualTo("treeviewadv__columnheader-resizer"));
	}

	[Test, WithPlaywrightPage]
	public async Task TreeViewWithColumnHeaderHasCorrectSortMark()
	{
		var ascendingImageUrl = "url(\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAkAAAAFCAYAAACXU8ZrAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAHYcAAB2HAY/l8WUAAAAkSURBVBhXY1iwYMF/QpgBBLBJwDBYAQwQVAADBBXAAKYCBgYAMNBHyBV9FTcAAAAASUVORK5CYII=\")";
		var descendingImageUrl = "url(\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAkAAAAFCAYAAACXU8ZrAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAHYcAAB2HAY/l8WUAAAAnSURBVBhXY0AHCxYs+A9lYgcgBTAMFUIFyAqwKsSmAIYJKoDgBf8BlplHyAqLsFwAAAAASUVORK5CYII=\")";

		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(500, 200) };
			var treeViewAdv = new TreeViewAdv() { Size = new Size(100, 100) };

			treeViewAdv.UseColumns = true;
			treeViewAdv.Columns.Add(new TreeColumn() { Width = 100, Header = "Column 1", SortOrder = SortOrder.Ascending });
			treeViewAdv.Columns.Add(new TreeColumn() { Width = 150, Header = "Column 2", SortOrder = SortOrder.Descending });
			form.Controls.Add(treeViewAdv);

			return form;
		});
		var ascendingSortMark = await page.WaitForSelectorAsync(".treeviewadv__columnheaders div:nth-child(1) span");
		var descendingSortMark = await page.WaitForSelectorAsync(".treeviewadv__columnheaders div:nth-child(2) span");

		Assert.That(async () => await ascendingSortMark.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-image')"), Is.EqualTo(ascendingImageUrl));
		Assert.That(async () => await descendingSortMark.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-image')"), Is.EqualTo(descendingImageUrl));
	}

	[Test]
	public async Task TreeViewWithoutColumnHeader()
	{
		TreeViewAdv treeViewAdv = null;
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			treeViewAdv = new TreeViewAdv()
			{
				Size = new Size(100, 100)
			};

			return treeViewAdv;
		});

		var header = rendered.FindAll(".treeviewadv__columnheaders div");
		Assert.That(header.Count, Is.EqualTo(0));
	}

	[Test]
	public async Task TreeViewShouldHaveTreeStructureInRenderedMarkup()
	{
		TreeViewAdv treeViewAdv = null;
		var model = CreateTreeModel();
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			treeViewAdv = new TreeViewAdv()
			{
				Size = new Size(100, 100),
				Model = model
			};

			CreateTreeViewContent(treeViewAdv);
			treeViewAdv.ExpandAll();

			return treeViewAdv;
		});

		var firstLevelRows = rendered.FindAll(".treeviewadv > .treeviewadv__rows > .treeviewadv__row");
		Assert.That(firstLevelRows.Count, Is.EqualTo(model.Nodes.Count));
		Assert.That(firstLevelRows[0].GetElementsByClassName("treeviewadv__row").Count, Is.EqualTo(model.Nodes[0].Nodes.Count));
		Assert.That(firstLevelRows[1].GetElementsByClassName("treeviewadv__row").Count, Is.EqualTo(model.Nodes[1].Nodes.Count));
	}

	[Test]
	public async Task TreeViewNodeContentGetRenderedCorrectly()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var treeViewAdv = new TreeViewAdv { Size = new Size(100, 100), Model = CreateTreeModel() };
			CreateTreeViewContent(treeViewAdv);
			treeViewAdv.ExpandAll();

			return treeViewAdv;
		});

		var rows = rendered.FindAll(".treeviewadv__rowcontent");
		Assert.That(rows.Count, Is.EqualTo(6));

		var rootCount = 2;
		var rowCount = 3;
		for (var i = 0; i < rootCount; i++)
		{
			var rowItems = rows[i * rowCount].GetElementsByClassName("treeviewadv__item");
			Assert.That(rowItems.Count, Is.EqualTo(5));
			Assert.That(rowItems[0].TextContent, Is.EqualTo(string.Empty));
			Assert.That(rowItems[1].TextContent, Is.EqualTo("level 1"));
			Assert.That(rowItems[2].TextContent, Is.EqualTo("level 1"));
			Assert.That(rowItems[3].TextContent, Is.EqualTo("level 1"));
			Assert.That(rowItems[4].TextContent, Is.EqualTo("level 1"));

			rowItems = rows[i * rowCount + 1].GetElementsByClassName("treeviewadv__item");
			Assert.That(rowItems.Count, Is.EqualTo(5));
			Assert.That(rowItems[0].TextContent, Is.EqualTo(string.Empty));
			Assert.That(rowItems[1].TextContent, Is.EqualTo("level 2"));
			Assert.That(rowItems[2].TextContent, Is.EqualTo("level 2"));
			Assert.That(rowItems[3].TextContent, Is.EqualTo("level 2"));
			Assert.That(rowItems[4].TextContent, Is.EqualTo("level 2"));

			rowItems = rows[i * rowCount + 2].GetElementsByClassName("treeviewadv__item");
			Assert.That(rowItems.Count, Is.EqualTo(5));
			Assert.That(rowItems[0].TextContent, Is.EqualTo(string.Empty));
			Assert.That(rowItems[1].TextContent, Is.EqualTo("level 2"));
			Assert.That(rowItems[2].TextContent, Is.EqualTo("level 2"));
			Assert.That(rowItems[3].TextContent, Is.EqualTo("level 2"));
			Assert.That(rowItems[4].TextContent, Is.EqualTo("level 2"));
		}
	}

	[Test, WithPlaywrightPage]
	public async Task TestTreeViewAdvHeaderIsResizable()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Size = new Size(600, 400) };
			var treeViewAdv = new TreeViewAdv() { Size = new Size(400, 200), Model = CreateTreeModel() };
			CreateTreeViewContent(treeViewAdv);

			treeViewAdv.ExpandAll();
			form.Controls.Add(treeViewAdv);
			return form;
		});
		var treeview = await page.WaitForSelectorAsync(".treeviewadv");
		Assert.That(treeview, Is.Not.Null);

		var columnHeaders = await page.QuerySelectorAllAsync(".treeviewadv__columnheaders div");
		var firstColumnResizer = page.Locator(".treeviewadv__columnheaders div:nth-child(1) span.treeviewadv__columnheader-resizer");

		var location = await firstColumnResizer.BoundingBoxAsync();
		await page.Mouse.MoveAsync(location.X + location.Width / 2, location.Y + location.Height / 2);
		await Task.Delay(500);

		await page.Mouse.DownAsync();
		await Task.Delay(500);

		// Test the initial widths and lefts
		Assert.That(async () => await columnHeaders[0].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('left')"), Is.EqualTo("0px"));
		Assert.That(async () => await columnHeaders[0].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("100px"));
		Assert.That(async () => await columnHeaders[1].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('left')"), Is.EqualTo("100px"));
		Assert.That(async () => await columnHeaders[1].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("150px"));
		Assert.That(async () => await columnHeaders[2].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('left')"), Is.EqualTo("250px"));
		Assert.That(async () => await columnHeaders[2].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("200px"));

		await page.Mouse.MoveAsync(location.X + location.Width / 2 + 100, location.Y + location.Height / 2);
		await Task.Delay(500);

		// Test widths and lefts in JS animation
		Assert.That(async () => await columnHeaders[0].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('left')"), Is.EqualTo("0px"));
		Assert.That(async () => await columnHeaders[0].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("200px"));
		Assert.That(async () => await columnHeaders[1].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('left')"), Is.EqualTo("200px"));
		Assert.That(async () => await columnHeaders[1].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("150px"));
		Assert.That(async () => await columnHeaders[2].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('left')"), Is.EqualTo("350px"));
		Assert.That(async () => await columnHeaders[2].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("200px"));

		await page.Mouse.UpAsync();
		await Task.Delay(500);

		// Test the final re-rendered widths and lefts
		Assert.That(async () => await columnHeaders[0].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('left')"), Is.EqualTo("0px"));
		Assert.That(async () => await columnHeaders[0].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("200px"));
		Assert.That(async () => await columnHeaders[1].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('left')"), Is.EqualTo("200px"));
		Assert.That(async () => await columnHeaders[1].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("150px"));
		Assert.That(async () => await columnHeaders[2].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('left')"), Is.EqualTo("350px"));
		Assert.That(async () => await columnHeaders[2].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("200px"));
	}

	[Test, WithPlaywrightPage]
	public async Task TestTreeViewAdvResizingOnlyAffectSingleTree()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var treeOneColumnResized = false;
		var treeTwoColumnResized = false;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Size = new Size(600, 400) };
			var treeViewAdv1 = new TreeViewAdv { Size = new Size(400, 200), Model = CreateTreeModel() };
			var treeViewAdv2 = new TreeViewAdv { Size = new Size(400, 200), Top = 250, Model = CreateTreeModel() };
			CreateTreeViewContent(treeViewAdv1);
			CreateTreeViewContent(treeViewAdv2);

			treeViewAdv1.ColumnWidthChanged += (sender, e) => { treeOneColumnResized = true; };
			treeViewAdv2.ColumnWidthChanged += (sender, e) => { treeTwoColumnResized = true; };

			treeViewAdv1.ExpandAll();
			treeViewAdv2.ExpandAll();
			form.Controls.Add(treeViewAdv1);
			form.Controls.Add(treeViewAdv2);
			return form;
		});

		var treeview = await page.WaitForSelectorAsync(".treeviewadv");
		Assert.That(treeview, Is.Not.Null);

		var firstColumnResizer = await page.WaitForSelectorAsync(".treeviewadv__columnheader-resizer");
		var treeOneCell = await page.WaitForSelectorAsync(".treeviewadv .treeviewadv__item:nth-child(2)");
		var treeTwoCell = await page.WaitForSelectorAsync(".treeviewadv:nth-child(2) .treeviewadv__item:nth-child(2)");

		await MouseMoveOfElementAsync(page, firstColumnResizer, 0, 1);
		await MouseDownAsync(page);
		Assert.That(await treeOneCell.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("77px"));
		Assert.That(await treeTwoCell.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("77px"));

		await MouseMoveOfElementAsync(page, firstColumnResizer, 60, 1);
		await MouseUpAsync(page);

		Assert.That(await treeOneCell.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("137px"));
		Assert.That(await treeTwoCell.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("77px"));

		Assert.That(treeOneColumnResized, Is.True);
		Assert.That(treeTwoColumnResized, Is.False);
	}

	[TestCase(SortOrder.Ascending, ".treeviewadv__columnheader-sortmark--ascending")]
	[TestCase(SortOrder.Descending, ".treeviewadv__columnheader-sortmark--descending")]
	[Test, WithPlaywrightPage]
	public async Task TestTreeViewAdvColumnHeaderSortMarkHasCorrectSize(SortOrder sortOrder, string sortMarkClass)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Size = new Size(600, 400) };
			var treeViewAdv = new TreeViewAdv { Size = new Size(400, 200), Model = CreateTreeModel() };
			treeViewAdv.UseColumns = true;
			treeViewAdv.Columns.Add(new TreeColumn { Width = 100, Header = "Column 1", MinColumnWidth = 10, MaxColumnWidth = 200, SortOrder = sortOrder });
			treeViewAdv.NodeControls.Add(new NodeTextBox { ParentColumn = treeViewAdv.Columns[0], DataPropertyName = "Text" });

			treeViewAdv.ExpandAll();
			form.Controls.Add(treeViewAdv);
			return form;
		});

		var treeview = await page.WaitForSelectorAsync(".treeviewadv");
		Assert.That(treeview, Is.Not.Null);

		var resizer = await page.WaitForSelectorAsync(".treeviewadv__columnheader-resizer");
		var headerText = await page.WaitForSelectorAsync(".treeviewadv__columnheader p");
		var sortMark = await page.WaitForSelectorAsync(sortMarkClass);

		Assert.That(await sortMark.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("25px"));
		Assert.That(await sortMark.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('height')"), Is.EqualTo("5px"));
		Assert.That(await headerText.IsVisibleAsync(), Is.True);

		await MouseMoveOfElementAsync(page, resizer, 1, 1);
		await MouseDownAsync(page);
		await MouseMoveOfElementAsync(page, resizer, -90, 1);
		await MouseUpAsync(page);

		Assert.That(await sortMark.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("25px"));
		Assert.That(await sortMark.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('height')"), Is.EqualTo("5px"));
		Assert.That(await headerText.IsVisibleAsync(), Is.False);
	}

	[Test, WithPlaywrightPage]
	public async Task TestTreeViewAdvItemsAreResizable()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Size = new Size(600, 400) };
			var treeViewAdv = new TreeViewAdv() { Size = new Size(400, 200), Model = CreateTreeModel() };
			CreateTreeViewContent(treeViewAdv);

			treeViewAdv.ExpandAll();
			form.Controls.Add(treeViewAdv);
			return form;
		});
		var treeview = await page.WaitForSelectorAsync(".treeviewadv");
		Assert.That(treeview, Is.Not.Null);

		var secondRowItems = await page.QuerySelectorAllAsync(".treeviewadv__rows li:nth-child(2) div .treeviewadv__item");
		var firstColumnResizer = page.Locator(".treeviewadv__columnheaders div:nth-child(1) span.treeviewadv__columnheader-resizer");

		var location = await firstColumnResizer.BoundingBoxAsync();
		await page.Mouse.MoveAsync(location.X + location.Width / 2, location.Y + location.Height / 2);
		await Task.Delay(500);

		await page.Mouse.DownAsync();
		await Task.Delay(500);

		// Test the initial widths and lefts
		Assert.That(async () => await secondRowItems[0].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('left')"), Is.EqualTo("26px"));
		Assert.That(async () => await secondRowItems[0].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("16px"));
		Assert.That(async () => await secondRowItems[1].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('left')"), Is.EqualTo("42px"));
		Assert.That(async () => await secondRowItems[1].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("58px"));
		Assert.That(async () => await secondRowItems[2].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('left')"), Is.EqualTo("100px"));
		Assert.That(async () => await secondRowItems[2].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("41px"));
		Assert.That(async () => await secondRowItems[3].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('left')"), Is.EqualTo("141px"));
		Assert.That(async () => await secondRowItems[3].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("109px"));
		Assert.That(async () => await secondRowItems[4].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('left')"), Is.EqualTo("250px"));
		Assert.That(async () => await secondRowItems[4].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("200px"));
		
		await page.Mouse.MoveAsync(location.X + location.Width / 2 + 100, location.Y + location.Height / 2);
		await Task.Delay(500);

		// Test widths and lefts in JS animation 
		Assert.That(async () => await secondRowItems[0].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('left')"), Is.EqualTo("26px"));
		Assert.That(async () => await secondRowItems[0].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("16px"));
		Assert.That(async () => await secondRowItems[1].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('left')"), Is.EqualTo("42px"));
		Assert.That(async () => await secondRowItems[1].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("158px"));
		Assert.That(async () => await secondRowItems[2].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('left')"), Is.EqualTo("200px"));
		Assert.That(async () => await secondRowItems[2].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("41px"));
		Assert.That(async () => await secondRowItems[3].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('left')"), Is.EqualTo("241px"));
		Assert.That(async () => await secondRowItems[3].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("109px"));
		Assert.That(async () => await secondRowItems[4].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('left')"), Is.EqualTo("350px"));
		Assert.That(async () => await secondRowItems[4].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("200px"));

		await page.Mouse.UpAsync();
		await Task.Delay(500);

		// Test the final re-rendered widths and lefts
		Assert.That(async () => await secondRowItems[0].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('left')"), Is.EqualTo("26px"));
		Assert.That(async () => await secondRowItems[0].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("16px"));
		Assert.That(async () => await secondRowItems[1].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('left')"), Is.EqualTo("42px"));
		Assert.That(async () => await secondRowItems[1].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("158px"));
		Assert.That(async () => await secondRowItems[2].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('left')"), Is.EqualTo("200px"));
		Assert.That(async () => await secondRowItems[2].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("41px"));
		Assert.That(async () => await secondRowItems[3].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('left')"), Is.EqualTo("241px"));
		Assert.That(async () => await secondRowItems[3].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("109px"));
		Assert.That(async () => await secondRowItems[4].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('left')"), Is.EqualTo("350px"));
		Assert.That(async () => await secondRowItems[4].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("200px"));
	}

	[Test, WithPlaywrightPage]
	public async Task TestTreeViewAdvRowContentStyle()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var treeViewAdv = new TreeViewAdvForTest { Size = new Size(400, 200), Model = CreateTreeModel() };
			CreateTreeViewContent(treeViewAdv);
			treeViewAdv.ExpandAll();

			return treeViewAdv;
		});

		var tree = page.Locator(".treeviewadv");
		var rowContent = tree.Locator(".treeviewadv__rowcontent").First;

		Assert.That(await rowContent.GetComputedStyleAsync("position"), Is.EqualTo("relative"));
		Assert.That(await rowContent.GetComputedStyleAsync("background-color"), Is.EqualTo(await tree.GetComputedStyleAsync("background-color")));
	}

	[Test]
	public async Task TestTreeViewAdvOnMouseUpEvent()
	{
		TreeViewAdvForTest treeViewAdv = null;
		var model = CreateTreeModel();
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			treeViewAdv = new TreeViewAdvForTest()
			{
				Size = new Size(100, 100),
				Model = model
			};

			CreateTreeViewContent(treeViewAdv);
			treeViewAdv.ExpandAll();

			return treeViewAdv;
		});

		var columnHeaders = rendered.FindAll(".treeviewadv__columnheaders div");

		await columnHeaders[0].MouseUpAsync(new WebMouseEventArgs
		{
			Button = 0,
			OffsetX = 10,
			OffsetY = 10
		});
		Assert.That(treeViewAdv.onMouseUpEventReceivedCount, Is.EqualTo(1));
	}

	[Test]
	public async Task NodeCheckBoxRender([Values] bool visible)
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var model = new TreeModel();
			model.Nodes.Add(new Node());
			var tree = new TreeViewAdv { Size = new Size(200, 200), Model = model };

			tree.UseColumns = true;
			tree.Columns.Add(new TreeColumn { Width = 100 });
			var checkBox = new NodeCheckBox { ParentColumn = tree.Columns[0] };
			checkBox.IsVisibleValueNeeded += (s, e) => e.Value = visible;
			tree.NodeControls.Add(checkBox);
			tree.ExpandAll();

			return tree;
		});

		Assert.That(rendered.FindAll(".treeviewadv__checkbox").Count > 0, Is.EqualTo(visible));
	}

	[TestCase(false, CheckState.Unchecked, false, false)]
	[TestCase(false, CheckState.Checked, false, true)]
	[TestCase(true, CheckState.Unchecked, true, false)]
	[TestCase(true, CheckState.Checked, true, true)]
	public async Task NodeCheckBoxHasCorrectCheckedAndDisabledAttribute(bool disabled, CheckState checkState, bool hasDisabled, bool hasChecked)
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var model = new TreeModel();
			model.Nodes.Add(new Node());
			var tree = new TreeViewAdv { Size = new Size(200, 200), Model = model };

			tree.UseColumns = true;
			tree.Columns.Add(new TreeColumn() { Width = 100 });
			var checkBox = new NodeCheckBoxForTest
			{
				ParentColumn = tree.Columns[0],
				EditEnabled = !disabled,
				CheckState = checkState
			};
			tree.NodeControls.Add(checkBox);
			tree.ExpandAll();

			return tree;
		});

		Assert.That(rendered.Find(".treeviewadv__checkbox").InnerHtml.Contains("disabled"), Is.EqualTo(hasDisabled));
		Assert.That(rendered.Find(".treeviewadv__checkbox").InnerHtml.Contains("checked"), Is.EqualTo(hasChecked));
	}

	[TestCase(CheckState.Checked, true, false)]
	[TestCase(CheckState.Unchecked, false, true)]
	public async Task NodeCheckBoxToggle(CheckState initialCheckState, bool hasCheckedBefore, bool hasCheckedAfter)
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var model = new TreeModel();
			model.Nodes.Add(new Node());
			var tree = new TreeViewAdv { Size = new Size(200, 200), Model = model };

			tree.UseColumns = true;
			tree.Columns.Add(new TreeColumn { Width = 100 });
			var checkBox = new NodeCheckBoxForTest
			{
				ParentColumn = tree.Columns[0],
				EditEnabled = true,
				CheckState = initialCheckState
			};
			tree.NodeControls.Add(checkBox);
			tree.ExpandAll();

			return tree;
		});

		Assert.That(rendered.Find(".treeviewadv__checkbox").InnerHtml.Contains("checked"), Is.EqualTo(hasCheckedBefore));

		await rendered.Find(".treeviewadv__checkbox").MouseDownAsync(new WebMouseEventArgs
		{
			Button = 0,
			OffsetX = 10,
			OffsetY = 10
		});

		Assert.That(rendered.Find(".treeviewadv__checkbox").InnerHtml.Contains("checked"), Is.EqualTo(hasCheckedAfter));
	}

	[TestCase(10, FontStyle.Regular, "font-size:10pt;")]
	[TestCase(10, FontStyle.Bold, "font-size:10pt;font-weight:bold;")]
	[TestCase(10, FontStyle.Italic, "font-size:10pt;font-style:italic;")]
	[TestCase(10, FontStyle.Bold | FontStyle.Italic, "font-size:10pt;font-weight:bold;font-style:italic;")]
	public async Task BaseTextControlShouldHaveCorrectFontStyle(float fontSize, FontStyle fontStyle, string styleString)
	{
		TreeViewAdv treeViewAdv = null;
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var model = new TreeModel();
			model.Nodes.Add(new Node());
			treeViewAdv = new TreeViewAdv() { Size = new Size(100, 100), Model = model, ShowPlusMinus = false };

			var nodeTextBox = new NodeTextBox();
			nodeTextBox.DrawText += (s, e) => e.Font = new Font(e.Font.FontFamily, fontSize, fontStyle);
			treeViewAdv.NodeControls.Add(nodeTextBox);

			return treeViewAdv;
		});

		var textBox = rendered.Find(".treeviewadv__rowcontent div p");
		Assert.That(textBox.GetAttribute("style"), Does.Contain(styleString));
	}

	[Test]
	public async Task TestTreeViewAdvOnClickEvent()
	{
		TreeViewAdvForTest treeViewAdv = null;
		var model = CreateTreeModel();
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			treeViewAdv = new TreeViewAdvForTest()
			{
				Size = new Size(100, 100),
				Model = model
			};

			CreateTreeViewContent(treeViewAdv);
			treeViewAdv.ExpandAll();

			return treeViewAdv;
		});

		var rows = rendered.FindAll(".treeviewadv__rowcontent");

		await rows[0].GetElementsByTagName("div")[1].ClickAsync(new WebMouseEventArgs
		{
			Button = 0,
			OffsetX = 10,
			OffsetY = 10
		});
		Assert.That(treeViewAdv.onClickEventReceivedCount, Is.EqualTo(1));
	}

	[Test]
	public async Task TestTreeViewAdvOnDblClickEvent()
	{
		TreeViewAdvForTest treeViewAdv = null;
		var model = CreateTreeModel();
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			treeViewAdv = new TreeViewAdvForTest()
			{
				Size = new Size(100, 100),
				Model = model
			};

			CreateTreeViewContent(treeViewAdv);

			treeViewAdv.ExpandAll();

			return treeViewAdv;
		});

		var rows = rendered.FindAll(".treeviewadv__rowcontent");

		await rows[0].GetElementsByTagName("div")[1].DoubleClickAsync(new WebMouseEventArgs
		{
			Button = 0,
			OffsetX = 10,
			OffsetY = 10
		});
		Assert.That(treeViewAdv.onDblClickEventReceivedCount, Is.EqualTo(1));
	}

	[Test, WithPlaywrightPage]
	public async Task TestTreeViewAdvOverflowX()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(500, 200) };
			form.Controls.Add(new TreeViewAdv() { Size = new Size(100, 100) });

			return form;
		});

		var treeview = await page.WaitForSelectorAsync(".treeviewadv");
		Assert.That(async () => await treeview.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('overflow-x')"), Is.EqualTo("auto"));
	}

	[Test, WithPlaywrightPage]
	[TestCase(3, 1, 20, "Column 2", "Column 3")]
	[TestCase(2, 3, 120, "Column 3", "Column 2")]
	[TestCase(3, 2, -20, "Column 2", "Column 3")]
	[TestCase(2, 3, 500, "Column 3", "Column 2")]
	[TestCase(2, 1, -100, "Column 1", "Column 2")]
	[TestCase(2, 3, 10, "Column 2", "Column 3")]
	[TestCase(3, 2, 100, "Column 3", "Column 2")]
	public async Task TestTreeViewColumnOrderChange(int columnIndex1, int columnIndex2, float movingDistance, string expectText1, string expectText2)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Size = new Size(800, 400) };
			var treeViewAdv = new TreeViewAdv { Size = new Size(400, 200), Model = CreateTreeModel() };
			treeViewAdv.AllowColumnReorder = true;
			CreateTreeViewContent(treeViewAdv);

			treeViewAdv.ExpandAll();
			form.Controls.Add(treeViewAdv);
			return form;
		});
		var treeview = await page.WaitForSelectorAsync(".treeviewadv");
		Assert.That(treeview, Is.Not.Null);

		var column1 = await page.WaitForSelectorAsync($".treeviewadv__columnheader:nth-child({columnIndex1})");
		var column2 = await page.WaitForSelectorAsync($".treeviewadv__columnheader:nth-child({columnIndex2})");

		await page.EvaluateAsync("document.getElementsByClassName('treeviewadv')[0].scrollTo(38, 0);"); // make some noise
		await MouseMoveOfElementAsync(page, column1, 75, 5);
		await MouseDownAsync(page);
		await MouseMoveOfElementAsync(page, column2, movingDistance, 5);
		await MouseUpAsync(page);

		var columnText1 = await (await page.WaitForSelectorAsync($".treeviewadv__columnheader:nth-child({columnIndex1}) p")).InnerHTMLAsync();
		var columnText2 = await (await page.WaitForSelectorAsync($".treeviewadv__columnheader:nth-child({columnIndex2}) p")).InnerHTMLAsync();
		Assert.That(columnText1, Is.EqualTo(expectText1));
		Assert.That(columnText2, Is.EqualTo(expectText2));
	}

	[Test, WithPlaywrightPage]
	public async Task TreeViewAdvGhostImageStyle()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Size = new Size(400, 400) };
			var treeViewAdv = new TreeViewAdv { Size = new Size(400, 200), Model = CreateTreeModel() };
			treeViewAdv.AllowColumnReorder = true;
			CreateTreeViewContent(treeViewAdv);

			treeViewAdv.ExpandAll();
			form.Controls.Add(treeViewAdv);
			return form;
		});

		var treeview = await page.WaitForSelectorAsync(".treeviewadv");
		Assert.That(treeview, Is.Not.Null);

		var column1 = (await page.QuerySelectorAllAsync(".treeviewadv__columnheader"))[1];
		var column2 = (await page.QuerySelectorAllAsync(".treeviewadv__columnheader"))[2];

		// If mouse move <= 4px, nothing will happen
		await MouseMoveOfElementAsync(page, column1, 0, 5);
		await MouseDownAsync(page);
		await MouseMoveOfElementAsync(page, column1, 2, 5);

		var ghostImage = await page.QuerySelectorAsync(".treeviewadv__columnheader-ghostimage");
		Assert.That(ghostImage, Is.Null);

		// Mouse move distance > 4px, ghost image should show up
		await MouseMoveOfElementAsync(page, column1, 20, 5);
		ghostImage = await page.QuerySelectorAsync(".treeviewadv__columnheader-ghostimage");

		Assert.That(ghostImage, Is.Not.Null);
		Assert.That(await ghostImage.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("150px"));
		Assert.That(await ghostImage.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('height')"), Is.EqualTo("19px"));
		Assert.That(await ghostImage.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('left')"), Is.EqualTo("120px"));
		Assert.That(await ghostImage.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('position')"), Is.EqualTo("absolute"));
		Assert.That(await ghostImage.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('opacity')"), Is.EqualTo("0.5"));
		Assert.That(await ghostImage.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('border')"), Is.EqualTo("0px none rgb(0, 0, 0)"));
		Assert.That(await ghostImage.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-color')"), Is.EqualTo("rgb(199, 199, 199)"));

		// Make ghost image partially displayed
		await MouseMoveOfElementAsync(page, column2, 100, 5);
		ghostImage = await page.QuerySelectorAsync(".treeviewadv__columnheader-ghostimage");
		var ghostImageText = await ghostImage.QuerySelectorAsync("p");

		Assert.That(await ghostImage.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("100px"));
		Assert.That(await ghostImage.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('left')"), Is.EqualTo("350px"));
		Assert.That(await ghostImageText.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('text-overflow')"), Is.EqualTo("clip"));

		// Move ghost image out of view
		await MouseMoveOfElementAsync(page, column2, 200, 5);
		ghostImage = await page.QuerySelectorAsync(".treeviewadv__columnheader-ghostimage");
		Assert.That(await ghostImage.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('display')"), Is.EqualTo("none"));

		await MouseUpAsync(page);
	}

	[Test, WithPlaywrightPage]
	public async Task MoveColumnShouldNotTriggerOnColumnClicked()
	{
		var onColumnClickedTriggeredCount = 0;
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Size = new Size(800, 400) };
			var treeViewAdv = new TreeViewAdv { Size = new Size(400, 200), Model = CreateTreeModel() };
			treeViewAdv.AllowColumnReorder = true;
			CreateTreeViewContent(treeViewAdv);

			treeViewAdv.ExpandAll();
			treeViewAdv.ColumnClicked += (sender, e) =>
			{
				if (e.Column.Index == 0)
				{
					onColumnClickedTriggeredCount++;
				}
			};
			form.Controls.Add(treeViewAdv);
			return form;
		});

		Assert.That(await page.WaitForSelectorAsync(".treeviewadv"), Is.Not.Null);

		var firstColumn = await page.WaitForSelectorAsync(".treeviewadv__columnheaders div:first-child");
		await MouseMoveOfElementAsync(page, firstColumn, 75, 5);
		await MouseDownAsync(page);
		await MouseMoveOfElementAsync(page, firstColumn, 120, 5);
		await MouseUpAsync(page);
		Assert.That(onColumnClickedTriggeredCount, Is.Zero);
	}

	[TestCase(".treeviewadv__columnheaders", 0, 10, 10)]
	[TestCase(".treeviewadv__columnheaders", 1, 10, 110)]
	[TestCase(".treeviewadv__rowcontent", 0, 10, 17)]
	[TestCase(".treeviewadv__rowcontent", 1, 10, 33)]
	[TestCase(".treeviewadv__rowcontent", 2, 10, 110)]
	public async Task TreeViewAdvMouseEventShouldHaveCorrectClickOffsetX(string parentClass, int childIndex, int offsetX, int expectedOffsetX)
	{
		TreeViewAdvForTest treeViewAdv = null;
		var model = CreateTreeModel();
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			treeViewAdv = new TreeViewAdvForTest() { Size = new Size(300, 300), Model = model };
			CreateTreeViewContent(treeViewAdv);
			treeViewAdv.ExpandAll();

			return treeViewAdv;
		});

		await rendered.Find(parentClass).Children[childIndex].MouseDownAsync(new WebMouseEventArgs
		{
			Button = 0,
			OffsetX = offsetX,
			OffsetY = treeViewAdv.RowHeight / 2
		});

		Assert.That(treeViewAdv.clickLocation.X, Is.EqualTo(expectedOffsetX));
	}

	[Test]
	public async Task TreeViewNodeShouldHaveCorrectRowClass()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var treeViewAdv = new TreeViewAdv { Size = new Size(100, 100), Model = CreateTreeModel() };
			CreateTreeViewContent(treeViewAdv);
			treeViewAdv.ExpandAll();
			treeViewAdv.SelectedNode = treeViewAdv.AllNodes.First();

			return treeViewAdv;
		});

		var rows = rendered.FindAll(".treeviewadv__rowcontent:nth-child(1)");
		Assert.That(rows[0].GetAttribute("class"), Does.Contain("treeviewadv__rowcontent--selected"));
		Assert.That(rows[1].GetAttribute("class"), Does.Not.Contain("treeviewadv__rowcontent--selected"));
		Assert.That(rows[2].GetAttribute("class"), Does.Not.Contain("treeviewadv__rowcontent--selected"));
	}

	[TestCase(1, "treeviewadv__plusminus", "img")]
	[TestCase(2, "treeviewadv__plusminus", null)]
	public async Task TreeViewNodePlusMinusRender(int level, string cssClass, string childElement)
	{
		TreeViewAdv treeViewAdv = null;
		var model = CreateTreeModel();
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			treeViewAdv = new TreeViewAdv() { Size = new Size(100, 100), Model = model };
			CreateTreeViewContent(treeViewAdv);
			treeViewAdv.ExpandAll();

			return treeViewAdv;
		});

		var rows = rendered.FindAll(".treeviewadv__rowcontent");
		var plusMinus = rows[level - 1].GetElementsByTagName("div")[0];
		Assert.That(plusMinus.Children.FirstOrDefault()?.TagName.ToLower(), Is.EqualTo(childElement));
		Assert.That(plusMinus.GetAttribute("class"), Does.Contain(cssClass));
	}

	[TestCase(100, 450)]
	[TestCase(600, 450)]
	public async Task RowsElementShouldRenderCorrectWidth(int containerWidth, int rowContentWidth)
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var treeViewAdv = new TreeViewAdv { Size = new Size(containerWidth, 100), Model = CreateTreeModel() };
			CreateTreeViewContent(treeViewAdv);
			treeViewAdv.ExpandAll();

			return treeViewAdv;
		});

		var rowContents = rendered.FindAll(".treeviewadv__rowcontent");

		Assert.That(rowContents[0].GetAttribute("style"), Does.Contain($"width:{rowContentWidth}px;"));
		Assert.That(rowContents[1].GetAttribute("style"), Does.Contain($"width:{rowContentWidth}px;"));
		Assert.That(rowContents[2].GetAttribute("style"), Does.Contain($"width:{rowContentWidth}px;"));
	}

	[Test]
	public async Task TreeViewAdvRowDrawEventShouldBeFired()
	{
		var isRowDrawFired = false;
		using var ctx = new EnterpriseTestContext();
		await ctx.RenderControlOnFormAsync(() =>
		{
			var model = new TreeModel();
			model.Nodes.Add(new Node());
			var tree = new TreeViewAdv { Size = new Size(200, 200), Model = model };
			tree.RowDraw += (sender, e) => { isRowDrawFired = true; };
			return tree;
		});
		Assert.That(isRowDrawFired, Is.True);
	}

	[Test]
	public async Task BaseTextControlDrawTextEventShouldBeFired()
	{
		var isDrawTextFired = false;
		using var ctx = new EnterpriseTestContext();
		await ctx.RenderControlOnFormAsync(() =>
		{
			var model = new TreeModel();
			model.Nodes.Add(new Node());
			var tree = new TreeViewAdv { Size = new Size(200, 200), Model = model };
			var textBox = new NodeTextBox();
			textBox.DrawText += (sender, e) => { isDrawTextFired = true; };
			tree.NodeControls.Add(textBox);
			return tree;
		});
		Assert.That(isDrawTextFired, Is.True);
	}

	[Test]
	public async Task RowBackgroundColorShouldBeApplied()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var model = new TreeModel();
			model.Nodes.Add(new Node());
			var tree = new TreeViewAdv { Size = new Size(200, 200), Model = model };
			tree.CurrentNode.RowBackgroundBrush = SystemBrushes.Menu;
			return tree;
		});
		var backColorStyle = (SystemBrushes.Menu as SolidBrush)?.Color.GetColorStyleValue();
		Assert.That(rendered.Find(".treeviewadv__row").GetAttribute("style"), Does.Contain($"background-color:{backColorStyle};"));
	}

	[Test]
	public async Task BaseTextControlTextColorShouldBeApplied()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var model = new TreeModel();
			model.Nodes.Add(new Node());
			var tree = new TreeViewAdv { Size = new Size(200, 200), Model = model };
			var textBox = new NodeTextBox();
			tree.NodeControls.Add(textBox);
			return tree;
		});
		var textColorStyle = SystemColors.ControlText.GetColorStyleValue();
		Assert.That(rendered.Find(".treeviewadv__rowcontent p").GetAttribute("style"), Does.Contain($"color:{textColorStyle};"));
	}

	[Test, WithPlaywrightPage]
	[TestCase(500, 496, 450)]
	[TestCase(200, 450, 450)]
	public async Task TreeViewAdvRowHasCorrectComputedWidth(int treeViewWidth, int expectedRowWidth, int expectedRowContentWidth)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Size = new Size(600, 400) };
			var treeViewAdv = new TreeViewAdv { Size = new Size(treeViewWidth, 200), Model = CreateTreeModel() };
			CreateTreeViewContent(treeViewAdv);
			treeViewAdv.ExpandAll();

			form.Controls.Add(treeViewAdv);
			return form;
		});
		var treeview = await page.WaitForSelectorAsync(".treeviewadv");
		Assert.That(treeview, Is.Not.Null);

		var row = await page.WaitForSelectorAsync(".treeviewadv__row");
		Assert.That(await row.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo($"{expectedRowWidth}px"));

		var rowContent = await row.WaitForSelectorAsync(".treeviewadv__rowcontent");
		Assert.That(await rowContent.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo($"{expectedRowContentWidth}px"));
	}

	[Test]
	public async Task TreeViewAdvHasCorrectColumnIndexAttribute()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var treeViewAdv = new TreeViewAdv { Size = new Size(100, 100), Model = CreateTreeModel() };
			CreateTreeViewContent(treeViewAdv);
			treeViewAdv.ExpandAll();

			return treeViewAdv;
		});

		var columnHeaders = rendered.FindAll(".treeviewadv__columnheaders div");
		Assert.That(columnHeaders.Count, Is.EqualTo(3));
		Assert.That(columnHeaders[0].GetAttribute("columnindex"), Is.EqualTo("0"));
		Assert.That(columnHeaders[1].GetAttribute("columnindex"), Is.EqualTo("2"));
		Assert.That(columnHeaders[2].GetAttribute("columnindex"), Is.EqualTo("3"));

		var rows = rendered.FindAll(".treeviewadv__rowcontent");
		Assert.That(rows.Count, Is.EqualTo(6));

		for (var i = 0; i < rows.Count; i++)
		{
			var items = rows[i].GetElementsByClassName("treeviewadv__item");
			Assert.That(items.Count, Is.EqualTo(5));

			Assert.That(items[0].GetAttribute("columnindex"), Is.Null); // this one should be plusminus node
			Assert.That(items[1].GetAttribute("columnindex"), Is.EqualTo("0"));
			Assert.That(items[2].GetAttribute("columnindex"), Is.EqualTo("2"));
			Assert.That(items[3].GetAttribute("columnindex"), Is.EqualTo("2"));
			Assert.That(items[4].GetAttribute("columnindex"), Is.EqualTo("3"));
		}
	}

	[Test]
	public async Task TreeViewAdvHeadersHaveCorrectMinAndMaxWidth()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var treeViewAdv = new TreeViewAdv { Size = new Size(100, 100), Model = CreateTreeModel() };
			CreateTreeViewContent(treeViewAdv);
			treeViewAdv.ExpandAll();

			return treeViewAdv;
		});

		var columnHeaders = rendered.FindAll(".treeviewadv__columnheaders div");
		Assert.That(columnHeaders.Count, Is.EqualTo(3));

		Assert.That(columnHeaders[0].GetAttribute("style"), Does.Contain("min-width:10px;").And.Contain("max-width:200px;"));
		Assert.That(columnHeaders[1].GetAttribute("style"), Does.Contain("min-width:10px;").And.Contain("max-width:300px;"));
		Assert.That(columnHeaders[2].GetAttribute("style"), Does.Contain("min-width:0px;").And.Contain("max-width:200px;"));
	}

	[Test, WithPlaywrightPage]
	[TestCase(-20, new[] { 130, 41, 89 })]
	[TestCase(-120, new[] { 30, 30, 3 })]
	[TestCase(-300, new[] { 10, 10, 3 })]
	[TestCase(50, new[] { 200, 41, 159 })]
	[TestCase(500, new[] { 300, 41, 259 })]
	public async Task TreeViewAdvNodesHaveCorrectComputedWidthWhenResize(int offset, int[] expectedWidths)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Size = new Size(600, 400) };
			var treeViewAdv = new TreeViewAdv { Size = new Size(400, 200), Model = CreateTreeModel() };
			CreateTreeViewContent(treeViewAdv);

			treeViewAdv.ExpandAll();
			form.Controls.Add(treeViewAdv);
			return form;
		});

		var treeview = await page.WaitForSelectorAsync(".treeviewadv");
		Assert.That(treeview, Is.Not.Null);

		var targetColumn = await page.WaitForSelectorAsync(".treeviewadv__columnheaders div[columnindex='2']");
		var targetColumnResizer = page.Locator(".treeviewadv__columnheaders div[columnindex='2'] span.treeviewadv__columnheader-resizer");
		var itemOne = await page.WaitForSelectorAsync(".treeviewadv__rows li > div .treeviewadv__item:nth-child(3)");
		var itemTwo = await page.WaitForSelectorAsync(".treeviewadv__rows li > div .treeviewadv__item:nth-child(4)");

		var location = await targetColumnResizer.BoundingBoxAsync();
		await page.Mouse.MoveAsync(location.X + location.Width / 2, location.Y + location.Height / 2);
		await Task.Delay(500);

		await page.Mouse.DownAsync();
		await Task.Delay(500);

		// Test the initial values
		Assert.That(await targetColumn.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("150px"));
		Assert.That(await itemOne.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("41px"));
		Assert.That(await itemTwo.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("109px"));

		await page.Mouse.MoveAsync(location.X + location.Width / 2 + offset, location.Y + location.Height / 2);
		await Task.Delay(500);

		// Before mouse up test widths in JS animation, because offsetWidth >= style.width, currently we do not have a node with a computed width of 0
		Assert.That(await targetColumn.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo($"{expectedWidths[0]}px"));
		Assert.That(await itemOne.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo($"{expectedWidths[1]}px"));
		Assert.That(await itemTwo.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo($"{expectedWidths[2]}px"));

		await page.Mouse.UpAsync();
		await Task.Delay(500);

		// Test the final re-rendered widths
		Assert.That(await targetColumn.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo($"{expectedWidths[0]}px"));
		Assert.That(await itemOne.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo($"{expectedWidths[1]}px"));
		Assert.That(await itemTwo.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo($"{expectedWidths[2]}px"));
	}

	[Test]
	public async Task TriggerOnDragOverForAllDropPositionsWhenSelectingRows()
	{
		using var ctx = new EnterpriseTestContext();
		TreeViewAdvForTest tree = null;
		await ctx.RenderControlOnFormAsync(() =>
		{
			var model = new TreeModel();
			model.Nodes.Add(new Node());
			model.Nodes.Add(new Node());
			model.Nodes.Add(new Node());
			tree = new TreeViewAdvForTest { Size = new Size(200, 200), Model = model };
			tree.AllowDrop = true;
			var textBox = new NodeTextBox();
			tree.NodeControls.Add(textBox);
			return tree;
		});

		await tree.InvokeWinzorDispatcherAsync(() => tree.Root.Children[0].IsSelected = true);
		Assert.That(tree.onDragOverTriggeredCount, Is.EqualTo(9));

		await tree.InvokeWinzorDispatcherAsync(() => tree.Root.Children[1].IsSelected = true);
		Assert.That(tree.onDragOverTriggeredCount, Is.EqualTo(18));

		await tree.InvokeWinzorDispatcherAsync(() => tree.Root.Children[2].IsSelected = true);
		Assert.That(tree.onDragOverTriggeredCount, Is.EqualTo(27));
	}

	[Test]
	public async Task DataRowCursorsAttribute()
	{
		using var ctx = new EnterpriseTestContext();
		TreeViewAdvForTest tree = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var model = new TreeModel();
			model.Nodes.Add(new Node());
			model.Nodes.Add(new Node());
			model.Nodes.Add(new Node());
			tree = new TreeViewAdvForTest { Size = new Size(200, 200), Model = model };
			var textBox = new NodeTextBox();
			tree.NodeControls.Add(textBox);

			tree.Root.Children[0].DropPositionEffects.AddRange(new DragDropEffects?[] { DragDropEffects.None, DragDropEffects.Move, DragDropEffects.Move });
			tree.Root.Children[1].DropPositionEffects.AddRange(new DragDropEffects?[] { DragDropEffects.Move, DragDropEffects.Move, DragDropEffects.Move });

			return tree;
		});

		var rows = rendered.FindAll(".treeviewadv__rowcontent");
		Assert.That(rows[0].GetAttribute("data-row-cursors"), Is.EqualTo("not-allowed,move,move"));
		Assert.That(rows[1].GetAttribute("data-row-cursors"), Is.EqualTo("move,move,move"));
		Assert.That(rows[2].GetAttribute("data-row-cursors"), Is.Empty);
	}

	[Test, WithPlaywrightPage]
	[TestCase(true, new[] { 1, 2, 3 })]
	[TestCase(false, new[] { 0, 0, 0 })]
	public async Task DragDropIsCorrectlyTriggered(bool allowDrop, int[] dragDropTriggeredCount)
	{
		TreeViewAdvForTest treeViewAdv = null;
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			treeViewAdv = new TreeViewAdvForTest {
				Size = new Size(400, 300),
				Model = CreateTreeModel(),
				AllowDrop = allowDrop,
				allowDropPositionAfter = false,
				allowDropPositionBefore = false,
			};
			CreateTreeViewContent(treeViewAdv);

			treeViewAdv.ExpandAll();
			return treeViewAdv;
		});

		var row1Item = page.Locator(".treeviewadv__rowcontent > .treeviewadv__item:nth-child(2)").First;
		var row2Item = page.Locator(".treeviewadv__row > .treeviewadv__rows > li > .treeviewadv__rowcontent .treeviewadv__item:nth-child(2)").First;
		var row3Item = page.Locator(".treeviewadv__row > .treeviewadv__rows > li:nth-child(2) > .treeviewadv__rowcontent .treeviewadv__item:nth-child(2)").First;

		await MouseMoveOfElementAsync(page, row1Item, 10, 10);
		await MouseDownAsync(page);
		await MouseMoveOfElementAsync(page, row1Item, 20, 10);
		await MouseUpAsync(page);
		Assert.That(treeViewAdv.onDragDropTriggeredCount, Is.EqualTo(dragDropTriggeredCount[0]));

		await MouseMoveOfElementAsync(page, row1Item, 10, 10);
		await MouseDownAsync(page);
		await MouseMoveOfElementAsync(page, row2Item, 10, 10);
		await MouseUpAsync(page);
		Assert.That(treeViewAdv.onDragDropTriggeredCount, Is.EqualTo(dragDropTriggeredCount[1]));

		await MouseMoveOfElementAsync(page, row1Item, 10, 10);
		await MouseDownAsync(page);
		await MouseMoveOfElementAsync(page, row3Item, 10, 10);
		await MouseUpAsync(page);
		Assert.That(treeViewAdv.onDragDropTriggeredCount, Is.EqualTo(dragDropTriggeredCount[2]));
	}

	[TestCase(14, "height:14px;")]
	[TestCase(16, "height:16px;")]
	public async Task BaseTextControlShouldHaveCorrectBoundsStyle(int rowHeight, string boundsStyleString)
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var model = new TreeModel();
			model.Nodes.Add(new Node());
			var treeViewAdv = new TreeViewAdv { Size = new Size(100, 100), Model = model, RowHeight = rowHeight };
			treeViewAdv.NodeControls.Add(new NodeTextBox());

			return treeViewAdv;
		});

		var textBox = rendered.Find(".treeviewadv__rowcontent div p");
		Assert.That(textBox.GetAttribute("style"), Does.Contain(boundsStyleString));
	}

	[Test]
	public async Task BaseTextControlShouldHaveCorrectOutlineStyle()
	{
		TreeViewAdv treeViewAdv = null;
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var model = new TreeModel();
			model.Nodes.Add(new Node());
			model.Nodes.Add(new Node());
			treeViewAdv = new TreeViewAdv { Size = new Size(100, 100), Model = model, ShowPlusMinus = false };
			treeViewAdv.NodeControls.Add(new NodeTextBox());
			return treeViewAdv;
		});

		Assert.That(rendered.FindAll(".treeviewadv__rowcontent div p")[1].GetAttribute("style"), Does.Not.Contain("outline"));

		await rendered.FindAll(".treeviewadv__rowcontent div p")[1].MouseDownAsync(new WebMouseEventArgs());
		Assert.That(rendered.FindAll(".treeviewadv__rowcontent div p")[1].GetAttribute("style"), Does.Contain($"outline:{SystemColors.InactiveCaption.GetColorStyleValue()} dotted 1px;outline-offset:-1px;"));

		await rendered.Find(".treeviewadv").MouseDownAsync(new WebMouseEventArgs());
		Assert.That(rendered.FindAll(".treeviewadv__rowcontent div p")[1].GetAttribute("style"), Does.Contain($"outline:{SystemColors.ControlText.GetColorStyleValue()} dotted 1px;outline-offset:-1px;"));
	}

	[Test, WithPlaywrightPage]
	public async Task NodePositionBeforeShouldHaveDropMark()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var treeViewAdv = new TreeViewAdvForTest { Size = new Size(400, 300), Model = CreateTreeModel(), AllowDrop = true, allowDropPositionBefore = true };
			CreateTreeViewContent(treeViewAdv);
			treeViewAdv.ExpandAll();
			return treeViewAdv;
		});

		var tree = page.Locator(".treeviewadv");
		var row1Content = tree.Locator(".treeviewadv__row > .treeviewadv__rows > li > .treeviewadv__rowcontent").First;
		var row1Item = row1Content.Locator(".treeviewadv__item:nth-child(2)");
		var row2Content = tree.Locator(".treeviewadv__row > .treeviewadv__rows > li:nth-child(2) > .treeviewadv__rowcontent").First;
		var row2Item = row2Content.Locator(".treeviewadv__item:nth-child(2)");

		await page.EvaluateAsync("document.getElementsByClassName('treeviewadv')[0].scrollTo(38, 0);"); // make some noise
		await MouseMoveOfElementAsync(page, row2Item, 25, 8);
		await MouseDownAsync(page);
		await MouseMoveOfElementAsync(page, row1Item, 25, 1);

		var dropMark = row1Content.Locator(".treeviewadv__dropmark");
		Assert.That(await dropMark.GetComputedStyleAsync("height"), Is.EqualTo("2px"));
		Assert.That(await dropMark.GetComputedStyleAsync("left"), Is.EqualTo("26px"));
		Assert.That(await dropMark.GetComputedStyleAsync("margin"), Is.EqualTo("0px 4px 0px -4px"));
		Assert.That(await dropMark.GetComputedStyleAsync("width"), Is.EqualTo("408px"));
		Assert.That(await dropMark.GetComputedStyleAsync("top"), Is.EqualTo("0px"));
		Assert.That(await dropMark.GetComputedStyleAsync("background-color"), Is.EqualTo("rgb(0, 0, 0)"));
		Assert.That(await dropMark.GetComputedStyleAsync("position"), Is.EqualTo("absolute"));
		Assert.That(await dropMark.GetComputedStyleAsync("pointer-events"), Is.EqualTo("none"));
		Assert.That(await row1Content.GetComputedStyleAsync("background-color"), Is.EqualTo(await tree.GetComputedStyleAsync("background-color")));

		await MouseUpAsync(page);
	}

	[Test, WithPlaywrightPage]
	public async Task NodePositionAfterShouldHaveDropMark()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var treeViewAdv = new TreeViewAdvForTest { Size = new Size(400, 300), Model = CreateTreeModel(), AllowDrop = true, allowDropPositionAfter = true };
			CreateTreeViewContent(treeViewAdv);
			treeViewAdv.ExpandAll();
			return treeViewAdv;
		});

		var tree = page.Locator(".treeviewadv");
		var row1Content = tree.Locator(".treeviewadv__row > .treeviewadv__rows > li > .treeviewadv__rowcontent").First;
		var row1Item = row1Content.Locator(".treeviewadv__item:nth-child(2)");
		var row2Content = tree.Locator(".treeviewadv__row > .treeviewadv__rows > li:nth-child(2) > .treeviewadv__rowcontent").First;
		var row2Item = row2Content.Locator(".treeviewadv__item:nth-child(2)");

		await page.EvaluateAsync("document.getElementsByClassName('treeviewadv')[0].scrollTo(38, 0);"); // make some noise
		await MouseMoveOfElementAsync(page, row2Item, 25, 8);
		await MouseDownAsync(page);
		await MouseMoveOfElementAsync(page, row1Item, 25, 13);

		var dropMark = row1Content.Locator(".treeviewadv__dropmark");
		Assert.That(await dropMark.GetComputedStyleAsync("height"), Is.EqualTo("2px"));
		Assert.That(await dropMark.GetComputedStyleAsync("left"), Is.EqualTo("26px"));
		Assert.That(await dropMark.GetComputedStyleAsync("margin"), Is.EqualTo("0px 4px 0px -4px"));
		Assert.That(await dropMark.GetComputedStyleAsync("width"), Is.EqualTo("408px"));
		Assert.That(await dropMark.GetComputedStyleAsync("top"), Is.EqualTo("16px"));
		Assert.That(await dropMark.GetComputedStyleAsync("background-color"), Is.EqualTo("rgb(0, 0, 0)"));
		Assert.That(await dropMark.GetComputedStyleAsync("position"), Is.EqualTo("absolute"));
		Assert.That(await dropMark.GetComputedStyleAsync("pointer-events"), Is.EqualTo("none"));
		Assert.That(await row1Content.GetComputedStyleAsync("background-color"), Is.EqualTo(await tree.GetComputedStyleAsync("background-color")));

		await MouseUpAsync(page);
	}

	[Test, WithPlaywrightPage]
	public async Task NodeControlShouldHaveZeroValueTopBecauseOfRelativePosition()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(600, 400) };
			var treeViewAdv = new TreeViewAdv() { Size = new Size(100, 100), Model = CreateTreeModel() };
			CreateTreeViewContent(treeViewAdv);
			form.Controls.Add(treeViewAdv);
			return form;
		});

		var row = await page.WaitForSelectorAsync(".treeviewadv__row");
		Assert.That(await row.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('top')"), Is.EqualTo("0px"));
	}

	[Test]
	public async Task AllRowsShouldHaveRowHeightVariable()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var treeViewAdv = new TreeViewAdv() { Size = new Size(100, 100), Model = CreateTreeModel() };
			CreateTreeViewContent(treeViewAdv);
			treeViewAdv.ExpandAll();
			return treeViewAdv;
		});

		foreach (var row in rendered.FindAll(".treeviewadv__row"))
		{
			Assert.That(row.GetAttribute("style"), Does.Contain("--row-height:16px;"));
		}
	}

	[Test]
	public async Task AllRowsShouldHaveLineLeftVariableToReflectFirstItemLeft()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var treeViewAdv = new TreeViewAdv() { Size = new Size(100, 100), Model = CreateTreeModel() };
			CreateTreeViewContent(treeViewAdv);
			treeViewAdv.ExpandAll();
			return treeViewAdv;
		});

		var rows = rendered.FindAll(".treeviewadv__row");

		Assert.That(rows[0].GetAttribute("style"), Does.Contain("--line-left:7px;"));
		Assert.That(rows[1].GetAttribute("style"), Does.Contain("--line-left:26px;"));
		Assert.That(rows[2].GetAttribute("style"), Does.Contain("--line-left:26px;"));
		Assert.That(rows[3].GetAttribute("style"), Does.Contain("--line-left:7px;"));
		Assert.That(rows[4].GetAttribute("style"), Does.Contain("--line-left:26px;"));
		Assert.That(rows[5].GetAttribute("style"), Does.Contain("--line-left:26px;"));
	}

	[Test, WithPlaywrightPage]
	public async Task RowBeforePseudoElement()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(600, 400) };
			var treeViewAdv = new TreeViewAdv() { Size = new Size(100, 100), Model = CreateTreeModel() };
			CreateTreeViewContent(treeViewAdv);
			form.Controls.Add(treeViewAdv);
			return form;
		});

		var row = await page.WaitForSelectorAsync(".treeviewadv__row");
		await Assert.MultipleAsync(async () =>
		{
			Assert.That(await row.EvaluateAsync<string>("e => window.getComputedStyle(e, '::before').getPropertyValue('content')"), Is.EqualTo("\"\""), "content");
			Assert.That(await row.EvaluateAsync<string>("e => window.getComputedStyle(e, '::before').getPropertyValue('height')"), Is.EqualTo("8px"), "height");
			Assert.That(await row.EvaluateAsync<string>("e => window.getComputedStyle(e, '::before').getPropertyValue('position')"), Is.EqualTo("absolute"), "position");
			Assert.That(await row.EvaluateAsync<string>("e => window.getComputedStyle(e, '::before').getPropertyValue('top')"), Is.EqualTo("8px"), "top");
			Assert.That(await row.EvaluateAsync<string>("e => window.getComputedStyle(e, '::before').getPropertyValue('left')"), Is.EqualTo("11px"), "left");
			Assert.That(await row.EvaluateAsync<string>("e => window.getComputedStyle(e, '::before').getPropertyValue('z-index')"), Is.EqualTo("1"), "z-index");
			Assert.That(await row.EvaluateAsync<string>("e => window.getComputedStyle(e, '::before').getPropertyValue('border-right')"), Is.EqualTo("1px solid rgb(171, 171, 171)"), "border-right");
		});
	}

	[Test, WithPlaywrightPage]
	public async Task NodePlusMinusBeforePseudoElement()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(600, 400) };
			var treeViewAdv = new TreeViewAdv() { Size = new Size(100, 100), Model = CreateTreeModel() };
			CreateTreeViewContent(treeViewAdv);
			form.Controls.Add(treeViewAdv);
			return form;
		});

		var row = await page.WaitForSelectorAsync(".treeviewadv__plusminus");
		await Assert.MultipleAsync(async () =>
		{
			Assert.That(await row.EvaluateAsync<string>("e => window.getComputedStyle(e, '::before').getPropertyValue('content')"), Is.EqualTo("\"\""), "content");
			Assert.That(await row.EvaluateAsync<string>("e => window.getComputedStyle(e, '::before').getPropertyValue('width')"), Is.EqualTo("12px"), "width");
			Assert.That(await row.EvaluateAsync<string>("e => window.getComputedStyle(e, '::before').getPropertyValue('position')"), Is.EqualTo("absolute"), "position");
			Assert.That(await row.EvaluateAsync<string>("e => window.getComputedStyle(e, '::before').getPropertyValue('top')"), Is.EqualTo("8px"), "top");
			Assert.That(await row.EvaluateAsync<string>("e => window.getComputedStyle(e, '::before').getPropertyValue('left')"), Is.EqualTo("4px"), "left");
			Assert.That(await row.EvaluateAsync<string>("e => window.getComputedStyle(e, '::before').getPropertyValue('border-bottom')"), Is.EqualTo("1px solid rgb(171, 171, 171)"), "border-right");
		});
	}

	[Test, WithPlaywrightPage]
	[TestCase(1, 1, -10, "treeviewadv__columnheader--border-left")]
	[TestCase(1, 3, -10, "treeviewadv__columnheader--border-left")]
	[TestCase(3, 1, -5, "treeviewadv__columnheader--border-left")]
	[TestCase(1, 3, 110, "treeviewadv__columnheader--border-right")]
	public async Task TestDragColumnHeader(int columnIndex1, int columnIndex2, float movingDistance, string expectClassName)
	{
		var model = CreateTreeModel();
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(800, 400) };
			var treeViewAdv = new TreeViewAdv() { Size = new Size(400, 200), Model = model };
			treeViewAdv.AllowColumnReorder = true;
			CreateTreeViewContent(treeViewAdv);

			treeViewAdv.ExpandAll();
			form.Controls.Add(treeViewAdv);
			return form;
		});

		var treeview = await page.WaitForSelectorAsync(".treeviewadv");
		Assert.That(treeview, Is.Not.Null);

		var column1 = await page.WaitForSelectorAsync($".treeviewadv__columnheaders div:nth-child({columnIndex1})");
		var column2 = await page.WaitForSelectorAsync($".treeviewadv__columnheaders div:nth-child({columnIndex2})");
		await MouseMoveOfElementAsync(page, column1, 75, 5);
		await MouseDownAsync(page);
		await MouseMoveOfElementAsync(page, column2, movingDistance, 5);

		var columnClassName = await (await page.WaitForSelectorAsync($".treeviewadv__columnheaders div:nth-child({columnIndex2})")).GetAttributeAsync("class");
		Assert.That(columnClassName, Does.Contain(expectClassName));
	}

	[Test, WithPlaywrightPage]
	public async Task TreeViewDragDropShouldHaveHoverStyleIfAllowDrop()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var treeViewAdv = new TreeViewAdvForTest { Size = new Size(400, 300), Model = CreateTreeModel(), AllowDrop = true };
			CreateTreeViewContent(treeViewAdv);

			treeViewAdv.Root.Children[0].DropPositionEffects.Add(DragDropEffects.Move);
			treeViewAdv.Root.Children[0].Children[0].DropPositionEffects.Add(DragDropEffects.Move);
			treeViewAdv.Root.Children[0].Children[1].DropPositionEffects.Add(DragDropEffects.Move);
			treeViewAdv.ExpandAll();
			return treeViewAdv;
		});

		var row1Content = page.Locator(".treeviewadv__row > .treeviewadv__rows > li > .treeviewadv__rowcontent").First;
		var row1Item = row1Content.Locator(".treeviewadv__item:nth-child(2)");

		var row2Content = page.Locator(".treeviewadv__row > .treeviewadv__rows > li:nth-child(2) > .treeviewadv__rowcontent").First;
		var row2Item = row2Content.Locator(".treeviewadv__item:nth-child(2)");

		await MouseMoveOfElementAsync(page, row1Item, 25, 8);
		await MouseDownAsync(page);
		await MouseMoveOfElementAsync(page, row2Item, 25, 8);

		Assert.That(await row1Content.GetAttributeAsync("class"), Does.Contain("treeviewadv__rowcontent--ondrag"));
		Assert.That(await row2Content.GetAttributeAsync("class"), Does.Contain("treeviewadv__rowcontent--ondragover"));

		var row2ItemText = row2Item.Locator("p");
		Assert.That(await row2ItemText.GetComputedStyleAsync("background-color"), Is.EqualTo("rgb(0, 120, 215)"));
		Assert.That(await row2ItemText.GetComputedStyleAsync("color"), Is.EqualTo("rgb(255, 255, 255)"));

		var row1ItemText = row1Item.Locator("p");
		Assert.That(await row1ItemText.GetComputedStyleAsync("background-color"), Is.EqualTo("rgb(0, 120, 215)"));
		Assert.That(await row1ItemText.GetComputedStyleAsync("color"), Is.EqualTo("rgb(0, 0, 0)"));

		await MouseUpAsync(page);
	}

	[Test, WithPlaywrightPage]
	public async Task TreeViewDragDropShouldNotHaveHoverStyleIfNotAllowDrop()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var treeViewAdv = new TreeViewAdv { Size = new Size(400, 300), Model = CreateTreeModel(), AllowDrop = false };
			CreateTreeViewContent(treeViewAdv);

			treeViewAdv.Root.Children[0].DropPositionEffects.Add(DragDropEffects.Move);
			treeViewAdv.Root.Children[0].Children[0].DropPositionEffects.Add(DragDropEffects.Move);
			treeViewAdv.Root.Children[0].Children[1].DropPositionEffects.Add(DragDropEffects.Move);
			treeViewAdv.ExpandAll();
			return treeViewAdv;
		});

		var row1Content = page.Locator(".treeviewadv__row > .treeviewadv__rows > li > .treeviewadv__rowcontent").First;
		var row1Item = row1Content.Locator(".treeviewadv__item:nth-child(2)");

		var row2Content = page.Locator(".treeviewadv__row > .treeviewadv__rows > li:nth-child(2) > .treeviewadv__rowcontent").First;
		var row2Item = row2Content.Locator(".treeviewadv__item:nth-child(2)");

		await MouseMoveOfElementAsync(page, row1Item, 25, 8);
		await MouseDownAsync(page);
		await MouseMoveOfElementAsync(page, row2Item, 25, 8);

		var row2ItemText = row2Item.Locator("p");
		Assert.That(await row2ItemText.GetComputedStyleAsync("background-color"), Is.EqualTo("rgba(0, 0, 0, 0)"));
		Assert.That(await row2ItemText.GetComputedStyleAsync("color"), Is.EqualTo("rgb(0, 0, 0)"));

		var row1ItemText = row1Item.Locator("p");
		Assert.That(await row1ItemText.GetComputedStyleAsync("background-color"), Is.EqualTo("rgb(0, 120, 215)"));
		Assert.That(await row1ItemText.GetComputedStyleAsync("color"), Is.EqualTo("rgb(255, 255, 255)"));

		await MouseUpAsync(page);
	}

	[Test, WithPlaywrightPage]
	public async Task ClickingNodeDoesNotTriggerDragDrop()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var dragDropTriggered = false;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Size = new Size(600, 400) };
			var treeViewAdv = new TreeViewAdv { Size = new Size(400, 200), Model = CreateTreeModel() };
			CreateTreeViewContent(treeViewAdv);

			treeViewAdv.DragDrop += (sender, args) =>
			{
				dragDropTriggered = true;
			};

			treeViewAdv.ExpandAll();
			form.Controls.Add(treeViewAdv);
			return form;
		});

		var treeview = await page.WaitForSelectorAsync(".treeviewadv");
		Assert.That(treeview, Is.Not.Null);

		var items = await page.QuerySelectorAllAsync(".treeviewadv__item");
		await items[1].ClickAsync();
		await items[2].ClickAsync();
		await items[3].ClickAsync();
		Assert.That(dragDropTriggered, Is.False);
	}

	[Test, WithPlaywrightPage]
	public async Task CtrlClickRow()
	{
		TreeViewAdv treeViewAdv = null;
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Size = new Size(600, 400) };
			treeViewAdv = new TreeViewAdv { Size = new Size(400, 200), Model = CreateTreeModel() };
			CreateTreeViewContent(treeViewAdv);
			treeViewAdv.SelectionMode = TreeSelectionMode.Multi;
			treeViewAdv.ExpandAll();
			form.Controls.Add(treeViewAdv);
			return form;
		});

		var optionsWithControlKey = new LocatorClickOptions { Modifiers = new[] { KeyboardModifier.Control } };

		await CellLocator(page, 0, 1).ClickAsync(optionsWithControlKey);
		await CellLocator(page, 1, 1).ClickAsync(optionsWithControlKey);
		await CellLocator(page, 2, 1).ClickAsync(optionsWithControlKey);
		await CellLocator(page, 3, 1).ClickAsync(optionsWithControlKey);
		await CellLocator(page, 4, 1).ClickAsync(optionsWithControlKey);
		Assert.That(() => treeViewAdv.SelectedNodes.Select(node => node.Row), Is.EqualTo(new[] { 0, 1, 2, 3, 4 }).After(500, 50));

		await CellLocator(page, 1, 1).ClickAsync(optionsWithControlKey);
		await CellLocator(page, 3, 1).ClickAsync(optionsWithControlKey);
		Assert.That(() => treeViewAdv.SelectedNodes.Select(node => node.Row), Is.EqualTo(new[] { 0, 2, 4 }).After(500, 50));

		await CellLocator(page, 5, 1).ClickAsync();
		Assert.That(() => treeViewAdv.SelectedNodes.Select(node => node.Row), Is.EqualTo(new[] { 5 }).After(500, 50));
	}

	[Test, WithPlaywrightPage]
	public async Task ShiftClickRow()
	{
		TreeViewAdv treeViewAdv = null;
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Size = new Size(600, 400) };
			treeViewAdv = new TreeViewAdv { Size = new Size(600, 400), Model = CreateTreeModel() };
			CreateTreeViewContent(treeViewAdv);
			treeViewAdv.SelectionMode = TreeSelectionMode.Multi;
			treeViewAdv.ExpandAll();
			form.Controls.Add(treeViewAdv);
			return form;
		});

		var optionsWithShiftKey = new LocatorClickOptions { Modifiers = new[] { KeyboardModifier.Shift } };

		await CellLocator(page, 3, 1).ClickAsync(optionsWithShiftKey);
		await CellLocator(page, 5, 1).ClickAsync(optionsWithShiftKey);
		Assert.That(() => treeViewAdv.SelectedNodes.Select(node => node.Row), Is.EqualTo(new[] { 3, 4, 5 }).After(500, 50));

		await CellLocator(page, 4, 1).ClickAsync(optionsWithShiftKey);
		Assert.That(() => treeViewAdv.SelectedNodes.Select(node => node.Row), Is.EqualTo(new[] { 3, 4 }).After(500, 50));

		await CellLocator(page, 1, 1).ClickAsync(optionsWithShiftKey);
		Assert.That(() => treeViewAdv.SelectedNodes.Select(node => node.Row), Is.EqualTo(new[] { 1, 2, 3 }).After(500, 50));

		await CellLocator(page, 0, 1).ClickAsync();
		Assert.That(() => treeViewAdv.SelectedNodes.Select(node => node.Row), Is.EqualTo(new[] { 0 }).After(500, 50));
	}

	void CreateTreeViewContent(TreeViewAdv tree)
	{
		tree.UseColumns = true;
		tree.Columns.Add(new TreeColumn { Width = 100, Header = "Column 1", MinColumnWidth = 10, MaxColumnWidth = 200 });
		tree.Columns.Add(new TreeColumn { Width = 150, Header = "Column 1.5", IsVisible = false });
		tree.Columns.Add(new TreeColumn { Width = 150, Header = "Column 2", MinColumnWidth = 10, MaxColumnWidth = 300 });
		tree.Columns.Add(new TreeColumn { Width = 200, Header = "Column 3", MinColumnWidth = 0, MaxColumnWidth = 200 });

		tree.NodeControls.Add(new NodeTextBox { ParentColumn = tree.Columns[0], DataPropertyName = "Text" });
		tree.NodeControls.Add(new NodeTextBox { ParentColumn = tree.Columns[1], DataPropertyName = "Text" });  // this node should not be rendered since Column 1.5 isn't visible
		tree.NodeControls.Add(new NodeTextBox { ParentColumn = tree.Columns[2], DataPropertyName = "Text" });
		tree.NodeControls.Add(new NodeTextBox { ParentColumn = tree.Columns[2], DataPropertyName = "Text" });
		tree.NodeControls.Add(new NodeTextBox { ParentColumn = tree.Columns[3], DataPropertyName = "Text" });
	}

	TreeModel CreateTreeModel()
	{
		var model = new TreeModel();
		for (var i = 0; i < 2; i++)
		{
			var node = AddRoot(model);
			for (var n = 0; n < 2; n++)
			{
				AddChild(node);
			}
		}

		return model;
	}

	Node AddRoot(TreeModel model)
	{
		var node = new Node("level 1");
		model.Nodes.Add(node);

		return node;
	}

	void AddChild(Node parent)
	{
		var node = new Node("level 2");
		parent.Nodes.Add(node);
	}

	async Task MouseDownAsync(IPage page)
	{
		await page.Mouse.DownAsync();
		await Task.Delay(100);
	}

	async Task MouseMoveOfElementAsync(IPage page, IElementHandle elementHandle, float x, float y)
	{
		var elementRect = await elementHandle.BoundingBoxAsync();
		await page.Mouse.MoveAsync(elementRect.X + x, elementRect.Y + y, new () { Steps = 10 });
		await Task.Delay(100);
	}

	async Task MouseMoveOfElementAsync(IPage page, ILocator locator, float x, float y)
	{
		var elementRect = await locator.BoundingBoxAsync();
		await page.Mouse.MoveAsync(elementRect.X + x, elementRect.Y + y, new () { Steps = 10 });
		await Task.Delay(100);
	}

	async Task MouseUpAsync(IPage page)
	{
		await page.Mouse.UpAsync();
		await Task.Delay(100);
	}

	ILocator CellLocator(IPage page, int row, int column) => page
		.Locator(".treeviewadv")
		.Locator(".treeviewadv__row").Nth(row)
		.Locator(".treeviewadv__item").Nth(column);
}

class NodeCheckBoxForTest : NodeCheckBox
{
	public CheckState CheckState { get; set; }

	protected override CheckState GetCheckState(TreeNodeAdv node) => CheckState;

	protected override void SetCheckState(TreeNodeAdv node, CheckState value) => CheckState = value;
}

class TreeViewAdvForTest : TreeViewAdv
{
	public int onMouseDownEventReceivedCount;

	public int onMouseUpEventReceivedCount;

	public int onClickEventReceivedCount;

	public int onDblClickEventReceivedCount;

	public int onDragOverTriggeredCount;

	public int onDragDropTriggeredCount;

	public bool allowDropPositionBefore;

	public bool allowDropPositionAfter;

	public Point clickLocation;

	internal TreeViewAdvForTest()
	{
		MouseDown += TreeViewAdvForTest_MouseDown;
		MouseUp += TreeViewAdvForTest_MouseUp;
		MouseClick += TreeViewAdvForTest_MouseClick;
		MouseDoubleClick += TreeViewAdvForTest_MouseDoubleClick;
		DragOver += TreeViewAdvForTest_DragOver;
		DragDrop += TreeViewAdvForTest_DragDrop;
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		clickLocation = e.Location;
		base.OnMouseDown(e);
	}

	void TreeViewAdvForTest_DragOver(object sender, DragEventArgs e)
	{
		if (e.Data.GetDataPresent(typeof(TreeNodeAdv[])) && DropPosition.Node != null)
		{
			DropPosition.Node.DropPositionEffects.Add(e.AllowedEffect);
		}
		onDragOverTriggeredCount++;
	}

	void TreeViewAdvForTest_DragDrop(object sender, DragEventArgs e)
	{
		onDragDropTriggeredCount++;
	}

	void TreeViewAdvForTest_MouseDown(object sender, MouseEventArgs e)
	{
		onMouseDownEventReceivedCount++;
	}

	void TreeViewAdvForTest_MouseUp(object sender, MouseEventArgs e)
	{
		onMouseUpEventReceivedCount++;
	}

	void TreeViewAdvForTest_MouseClick(object sender, MouseEventArgs e)
	{
		onClickEventReceivedCount++;
	}

	void TreeViewAdvForTest_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		onDblClickEventReceivedCount++;
	}

	protected override bool AllowDropPositionBefore(TreeNodeAdv node)
	{
		return allowDropPositionBefore;
	}
	protected override bool AllowDropPositionAfter(TreeNodeAdv node)
	{
		return allowDropPositionAfter;
	}
}
