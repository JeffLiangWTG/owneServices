using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WinzorFramework.JSInterop;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using static WTG.PlaywrightTesting.PlaywrightTestContext;

namespace System.Windows.Forms;

public class ListViewTest
{
	[Test]
	public async Task ListViewRendered()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var column1 = new ColumnHeader()
			{
				Text = "First",
				Width = 50,
			};
			var column2 = new ColumnHeader()
			{
				Text = "Second",
				Width = 100,
			};

			var listView = new ListView();
			listView.View = View.Details;
			listView.Columns.Add(column1);
			listView.Columns.Add(column2);

			listView.Items.Add(new ListViewItem("101"));
			listView.Items.Add(new ListViewItem("102"));

			return listView;
		});
		var view = rendered.Find(".listview");
		var th = rendered.FindAll("thead>tr>th");

		Assert.That(view, Is.Not.Null);
		Assert.That(th, Has.Count.EqualTo(2));
		Assert.That(th[0].ToMarkup(), Does.Contain("listview__header"));
		Assert.That(th[0].ToMarkup(), Does.Contain("First"));
		Assert.That(th[1].ToMarkup(), Does.Contain("Second"));

		var tr = rendered.FindAll("tbody>tr");
		Assert.That(tr, Has.Count.EqualTo(2));
		Assert.That(tr[0].ChildNodes[0].TextContent, Is.EqualTo("101"));
	}

	[Test]
	public async Task ListViewShouldRerenderedAfterItemChange()
	{
		using var ctx = new WinzorTestContext();
		ListView listView = null;
		var page = await ctx.RenderControlOnFormAsync(() =>
		{
			var column1 = new ColumnHeader()
			{
				Text = "First",
				Width = 50,
			};
			var column2 = new ColumnHeader()
			{
				Text = "Second",
				Width = 100,
			};

			listView = new ListView();
			listView.View = View.Details;
			listView.Columns.Add(column1);
			listView.Columns.Add(column2);

			listView.Items.Add(new ListViewItem("101"));
			listView.Items.Add(new ListViewItem("102"));

			return listView;
		});
		Assert.That(page.FindAll("tbody>tr").Count, Is.EqualTo(2));
		await listView.InvokeWinzorDispatcherAsync(() =>
		{
			listView.Items.RemoveAt(0);
		});
		Assert.That(page.FindAll("tbody>tr").Count, Is.EqualTo(1));
	}

	[Test, WithPlaywrightPage]
	public async Task ListViewHtmlRendered()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(500, 300) };

			var column1 = new ColumnHeader()
			{
				Text = "First",
				Width = 50,
			};
			var column2 = new ColumnHeader()
			{
				Text = "Second",
				Width = 100,
			};

			var listView = new ListView();
			listView.Size = new Size(400, 200);
			listView.View = View.Details;
			listView.Columns.Add(column1);
			listView.Columns.Add(column2);

			var item1 = new ListViewItem("101");
			item1.SubItems.Add("102");
			listView.Items.Add(item1);
			var item2 = new ListViewItem("201");
			item2.SubItems.Add("202");
			listView.Items.Add(item2);

			form.Controls.Add(listView);
			return form;
		});
		await page.WaitForSelectorAsync("table");

		var tableElementHandle = await page.QuerySelectorAsync("table");
		Assert.That(async () => await tableElementHandle.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("150px"));
		var column1Handle = await tableElementHandle.QuerySelectorAllAsync("thead tr:first-child th");
		Assert.That(async () => await tableElementHandle.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("150px"));
	}

	[TestCaseSource(nameof(BackColorCases))]
	public async Task BackgroundColorShouldBeCorrect(Color? color, string hex)
	{
		using var ctx = new WinzorTestContext();
		ListView listView = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var column1 = new ColumnHeader()
			{
				Text = "First",
				Width = 50,
			};
			var column2 = new ColumnHeader()
			{
				Text = "Second",
				Width = 100,
			};

			if (color == null)
			{
				listView = new ListView();
			}
			else
			{
				listView = new ListView() { BackColor = (Color)color };
			}
			listView.Size = new Size(400, 200);
			listView.View = View.Details;
			listView.Columns.Add(column1);
			listView.Columns.Add(column2);

			return listView;
		});

		var view = rendered.Find(".listview");
		Assert.That(view.GetAttribute("style"), Does.Contain($"background-color:{hex};"));
	}

	[Test]
	public async Task DataRenderedWithVirtualMode()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var column1 = new ColumnHeader()
			{
				Text = "First",
				Width = 50,
			};
			var column2 = new ColumnHeader()
			{
				Text = "Second",
				Width = 100,
			};

			var listView = new ListView();
			listView.Size = new Size(400, 200);
			listView.View = View.Details;
			listView.Columns.Add(column1);
			listView.Columns.Add(column2);

			listView.VirtualMode = true;
			listView.RetrieveVirtualItem += new RetrieveVirtualItemEventHandler(ListView_RetrieveVirtualItem_Mock);
			listView.VirtualListSize = 3;

			return listView;
		});

		var tr = rendered.FindAll("tbody>tr");
		Assert.That(tr, Has.Count.EqualTo(3));
		Assert.That(tr[0].ChildNodes[0].TextContent, Is.EqualTo("Field[0]"));
		Assert.That(tr[2].ChildNodes[0].TextContent, Is.EqualTo("Field[2]"));
		Assert.That(tr[2].ChildNodes[1].TextContent, Is.EqualTo("I am the second value for row 2"));
	}

	[TestCase(BorderStyle.None, null)]
	[TestCase(BorderStyle.FixedSingle, "listview--border-fixedsingle")]
	[TestCase(BorderStyle.Fixed3D, "listview--border-fixed3d")]
	public async Task ListViewBorderStyle(BorderStyle borderStyle, string expectStyleClass)
	{
		using var ctx = new WinzorTestContext();
		(_, ListView listViewInstance, IElement listViewElement) = await GetListViewAndElementAsync(ctx, borderStyle);

		if (expectStyleClass == null)
		{
			Assert.That(listViewElement.GetAttribute("class"), Is.Not.Contain("listview--border-"));
		}
		else
		{
			Assert.That(listViewElement.GetAttribute("class"), Does.Contain(expectStyleClass));
		}
	}

	[TestCase(BorderStyle.None, 0)]
	[TestCase(BorderStyle.FixedSingle, 1)]
	[TestCase(BorderStyle.Fixed3D, 2)]
	public async Task ListViewBorderStyleAdjustForClientSize(BorderStyle borderStyle, int borderSize)
	{
		using var ctx = new WinzorTestContext();
		(_, ListView listViewInstance, IElement listViewElement) = await GetListViewAndElementAsync(ctx, borderStyle);

		var bounds = listViewInstance.Bounds;
		Assert.That(listViewInstance.ClientSize.Width + 2 * borderSize, Is.EqualTo(bounds.Width));
		Assert.That(listViewInstance.ClientSize.Height + 2 * borderSize, Is.EqualTo(bounds.Height));
		Assert.That(listViewInstance.ClientAreaBounds.X, Is.EqualTo(bounds.X + borderSize));
		Assert.That(listViewInstance.ClientAreaBounds.Y, Is.EqualTo(bounds.Y + borderSize));
	}

	[Test, WithPlaywrightPage]
	public async Task ListViewTableHeaderStyle()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(500, 200) };
			form.Controls.Add(GetTestListView());

			return form;
		});
		var header = await page.WaitForSelectorAsync("th");
		var borderStyle = await header.GetComputedStyleAsync("border-right");
		var paddingStyle = await header.GetComputedStyleAsync("padding-left");

		Assert.That(borderStyle.Raw, Is.EqualTo("1px solid rgb(229, 229, 229)"));
		Assert.That(paddingStyle.Raw, Is.EqualTo("4px"));
	}

	[Test]
	public async Task ListViewGridLines()
	{
		using var ctx = new WinzorTestContext();
		(var rendered, var listViewInstance, var listViewElement) = await GetListViewAndElementAsync(ctx);

		Assert.That(listViewElement.GetAttribute("class"), Is.Not.Contain("gridline"));

		await listViewInstance.InvokeWinzorDispatcherAsync(() =>
		{
			listViewInstance.GridLines = true;
		});
		listViewElement = rendered.Find($"div[data-winzor-control-id='{listViewInstance.WinzorControlId}']").FirstElementChild;
		Assert.That(listViewElement.GetAttribute("class"), Does.Contain("gridline"));
	}

	[Test, WithPlaywrightPage]
	public async Task ColumnResizerGuideShouldHaveCorrectClassesUponDrag()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{	
			var form = new Form() { Size = new Size(500, 200) };
			form.Controls.Add(GetTestListView());

			return form;
		});
		var resizer = await page.WaitForSelectorAsync("th .listview__column_resizer");

		await MouseMoveOfElementAsync(page, resizer, 0, 2);
		await MouseDownAsync(page);
		await MouseMoveOfElementAsync(page, resizer, 80, 2);

		var guide = await page.WaitForSelectorAsync("table > div:last-of-type");
		Assert.That(await guide.GetAttributeAsync("class"), Does.Contain("splitter__guide"));
	}

	[Test, WithPlaywrightPage]
	public async Task ChangeColumnWidthByDragResizer()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(500, 200) };
			var listView = GetTestListView();
			listView.Columns[0].Width = 20;

			form.Controls.Add(listView);
			return form;
		});

		var column = await page.WaitForSelectorAsync("th");
		var resizer = await column.WaitForSelectorAsync(".listview__column_resizer");

		await MouseMoveOfElementAsync(page, resizer, 0, 2);
		await MouseDownAsync(page);
		await MouseMoveOfElementAsync(page, resizer, 80, 2);
		await MouseUpAsync(page);

		var columnWidthString = (await column.GetComputedStyleAsync("width")).Raw;
		Assert.That(columnWidthString, Is.EqualTo("100px"));
	}

	[Test]
	public async Task PreloadListViewJSInterop()
	{
		using var ctx = new WinzorTestContext();
		var services = ctx.MockCargoWiseClientServices;
		var interop = new Mock<IListViewJSInterop>();
		interop.Setup(i => i.PreloadInterop());
		ctx.Services.AddScoped(_ => interop.Object);

		var rendered = await ctx.RenderControlOnFormAsync(() => GetTestListView());

		interop.Verify(e => e.PreloadInterop(), Times.Once());
	}

	[Test]
	public async Task SetColumnSize()
	{
		using var ctx = new WinzorTestContext();
		ListView listView = null;

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			listView = GetTestListView();
			return listView;
		});

		Assert.DoesNotThrowAsync(async () => await listView.SetColumnSize(0, 10));
		var column = rendered.FindAll("th")[0];
		var columnWidth = column.ComputeCurrentStyle()["width"];
		Assert.That(columnWidth, Is.EqualTo("10px"));
	}

	[Test]
	public async Task ListViewDraggable([Values] bool allowDrop)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var column1 = new ColumnHeader() { Text = "First",	Width = 50,	};
			var column2 = new ColumnHeader() { Text = "Second",	Width = 100, };

			var listView = new ListView();
			listView.View = View.Details;
			listView.Columns.Add(column1);
			listView.Columns.Add(column2);

			listView.Items.Add(new ListViewItem("101"));
			listView.Items.Add(new ListViewItem("102"));
			listView.AllowDrop = allowDrop;

			return listView;
		});
		
		var tr = rendered.FindAll("tbody>tr");
		Assert.That(tr[0].Attributes["draggable"].Value, Is.EqualTo(allowDrop ? "true" : "false"));
	}

	[Test]
	public async Task ListViewCanRenderImageIcons()
	{
		using var ctx = new WinzorTestContext();
		ListView listView = null;
		var imageSize = new Size(32, 32);
		var imageKey = "TestImage";
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			listView = new ListView();
			listView.View = View.List;
			var imageList = new ImageList();
			imageList.ImageSize = imageSize;
			imageList.Images.Add(imageKey, TestImage.GetImage());
			listView.SmallImageList = imageList;

			listView.Columns.Add("", 50);
			listView.Items.Add(new ListViewItem("Super Long ListViewItem Text And Should Not be Cut-Off") { ImageKey = imageKey });

			return listView;
		});
		Assert.That(rendered.FindAll(".listviewitem > img").Count, Is.EqualTo(1));
		Assert.That(rendered.Find(".listviewitem > img").GetAttribute("width"), Is.EqualTo($"{imageSize.Width}px"));
		Assert.That(rendered.Find("tbody>tr>td").ComputeCurrentStyle()["width"], Is.GreaterThan(rendered.Find("tbody>tr>td>.listviewitem").ComputeCurrentStyle()["width"]));
	}

	[Test, WithPlaywrightPage]
	public async Task ListViewItemStyle()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(500, 200) };
			form.Controls.Add(GetTestListView());

			return form;
		});
		var item = page.Locator(".listviewitem").First;

		Assert.That(async () => await item.GetComputedStyleAsync("display"), Is.EqualTo("flex"));
		Assert.That(async () => await item.GetComputedStyleAsync("align-items"), Is.EqualTo("center"));
		Assert.That(async () => await item.GetComputedStyleAsync("column-gap"), Is.EqualTo("5px"));
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

	async Task MouseDownAsync(IPage page)
	{
		await page.Mouse.DownAsync();
		await Task.Delay(500);
	}

	ListView GetTestListView()
	{
		var result = new ListView();
		result.Columns.Add(new ColumnHeader() { Text = "Key" });
		result.Columns.Add(new ColumnHeader() { Text = "Value" });

		var item1 = new ListViewItem();
		item1.SubItems.Add("Key:1");
		item1.SubItems.Add("Value:1");
		result.Items.Add(item1);

		var item2 = new ListViewItem();
		item2.SubItems.Add("Key:2");
		item2.SubItems.Add("Value:22");
		result.Items.Add(item2);

		result.View = View.Details;
		return result;
	}

	async Task<(IRenderedComponent<ControlProxyComponent>, ListView, IElement)> GetListViewAndElementAsync(WinzorTestContext ctx, BorderStyle? borderStyle = null)
	{
		ListView listView = null;
		IRenderedComponent<ControlProxyComponent> rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			listView = GetTestListView();
			if (borderStyle != null)
			{
				listView.BorderStyle = borderStyle.Value;
			}
			return listView;
		});

		string cssSelector = $"div[data-winzor-control-id='{listView.WinzorControlId}']";
		IElement listViewElement = rendered.Find(cssSelector).FirstElementChild;
		return (rendered, listView, listViewElement);
	}

	static IEnumerable<TestCaseData> BackColorCases()
	{
		yield return new TestCaseData(null, "var(--color-window)");
		yield return new TestCaseData(Color.Green, "#008000FF");
		yield return new TestCaseData(Color.Red, "#FF0000FF");
		yield return new TestCaseData(Color.Yellow, "#FFFF00FF");
		yield return new TestCaseData(Color.Gray, "#808080FF");
	}

	void ListView_RetrieveVirtualItem_Mock(object sender, RetrieveVirtualItemEventArgs e)
	{
		var result = new ListViewItem($"Field[{e.ItemIndex}]");
		result.SubItems.Add($"I am the second value for row {e.ItemIndex}");

		e.Item = result;
	}
}
