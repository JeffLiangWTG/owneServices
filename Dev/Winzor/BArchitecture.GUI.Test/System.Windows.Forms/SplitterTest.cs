using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WinzorFramework.JSInterop;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;

using static PlaywrightTestContext;

class SplitterTest
{
	[Test]
	public async Task SplitterVerticalHasCorrectSizeAndPosition()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new Splitter() { Dock = DockStyle.Bottom });
		var splitterElement = rendered.Find(".splitter");
		Assert.That(splitterElement.GetAttribute("style"), Is.EqualTo("position:absolute;width:300px;height:3px;top:297px;left:0px;background-color:var(--color-control);cursor:row-resize;"));
	}

	[Test]
	public async Task PreloadSplitterJSInterop()
	{
		using var ctx = new WinzorTestContext();
		var services = ctx.MockCargoWiseClientServices;
		var interop = new Mock<ISplitterJSInterop>();
		interop.Setup(i => i.PreloadInterop());
		ctx.Services.AddScoped(_ => interop.Object);

		var rendered = await ctx.RenderControlOnFormAsync(() => new Splitter() { Dock = DockStyle.Bottom });

		interop.Verify(e => e.PreloadInterop(), Times.Once());
	}

	[Test]
	public async Task SplitterHorizontalHasCorrectSizeAndPosition()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new Splitter());
		var splitterElement = rendered.Find(".splitter");
		Assert.That(splitterElement.GetAttribute("style"), Is.EqualTo("position:absolute;width:3px;height:300px;top:0px;left:0px;background-color:var(--color-control);cursor:col-resize;"));
	}

	[Test]
	public async Task SplitterAddedAsOnlyControlWontMoveAsDockedLeft()
	{
		using var ctx = new WinzorTestContext();
		Splitter splitter = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => splitter = new Splitter());

		await rendered.Find(".splitter").TriggerEventAsync("onsplittermoved", new SplitterMovedEventArgs()
		{
			xOffset = 100,
			yOffset = 0
		});
		var splitterElement = rendered.Find(".splitter");
		Assert.That(splitterElement.GetAttribute("style"), Is.EqualTo("position:absolute;width:3px;height:300px;top:0px;left:0px;background-color:var(--color-control);cursor:col-resize;"));
	}

	[Test]
	public async Task SplitterAddedAsFirstControl()
	{
		Splitter splitter = null;
		Panel panel = null;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form() { Size = new Size(400, 400), SizeGripStyle = SizeGripStyle.Hide };
			splitter = new Splitter() { MinExtra = 0 };
			panel = new Panel { Size = new Size(300, 400), Dock = DockStyle.Left };
			form.Controls.Add(splitter);
			form.Controls.Add(panel);
			return form;
		});

		await rendered.Find(".splitter").TriggerEventAsync("onsplittermoved", new SplitterMovedEventArgs()
		{
			xOffset = 100,
			yOffset = 0
		});
		var splitterElement = rendered.Find(".splitter");
		Assert.That(splitterElement.GetAttribute("style"), Is.EqualTo("position:absolute;width:3px;height:400px;top:0px;left:397px;background-color:var(--color-control);cursor:col-resize;"));
		Assert.That(((IElement)splitterElement.Parent.ChildNodes.Last(n => n is IElement)).GetAttribute("style"), Is.EqualTo("position:absolute;width:397px;height:400px;top:0px;left:0px;background-color:var(--color-control);overflow:hidden;"));
	}

	[Test]
	public async Task SplitterAddedAsLastControlNothingMoves()
	{
		Splitter splitter = null;
		Panel panel = null;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form() { Size = new Size(303, 400) };
			splitter = new Splitter { Dock = DockStyle.Right };
			panel = new Panel { Size = new Size(300, 400), Dock = DockStyle.Right };
			form.Controls.Add(panel);
			form.Controls.Add(splitter);
			return form;
		});

		await rendered.Find(".splitter").TriggerEventAsync("onsplittermoved", new SplitterMovedEventArgs()
		{
			xOffset = 100,
			yOffset = 0
		});
		var input = rendered.Find(".splitter");
		Assert.That(input.GetAttribute("style"), Is.EqualTo("position:absolute;width:3px;height:400px;top:0px;left:300px;background-color:var(--color-control);cursor:col-resize;"));
		Assert.That(((IElement)input.Parent.FirstChild).GetAttribute("style"), Is.EqualTo("position:absolute;width:300px;height:400px;top:0px;left:0px;background-color:var(--color-control);overflow:hidden;"));
	}

	[Test]
	public async Task Splitter_ContentShouldBeWrappedBySplitter()
	{
		Splitter splitter = null;
		Panel panel = null;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form() { Size = new Size(303, 400) };
			splitter = new Splitter { Dock = DockStyle.Right };
			panel = new Panel { Size = new Size(300, 400), Dock = DockStyle.Right };
			splitter.Controls.Add(new Label() { Text = "Test" });
			form.Controls.Add(panel);
			form.Controls.Add(splitter);
			return form;
		});

		var input = rendered.Find(".splitter");
		Assert.That(input.ChildElementCount, Is.EqualTo(1));
		Assert.That(input.FirstElementChild.ClassName, Does.Contain("label"));
		Assert.That(input.FirstElementChild.InnerHtml, Does.Contain("<span class=\"label__text\""));
		var renderedText = rendered.Find(".label span");
		Assert.That(renderedText.Text, Is.EqualTo("Test"));
	}

	[Test]
	public async Task SplitterAddedAsBetweenControl()
	{
		Splitter splitter = null;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form() { Size = new Size(1003, 800), SizeGripStyle = SizeGripStyle.Hide };
			splitter = new Splitter();
			var fillPanel = new Panel { Dock = DockStyle.Fill };
			var dockedPanel = new Panel { Size = new Size(500, 800), Dock = DockStyle.Left };
			form.Controls.AddRange(new Control[] { fillPanel, splitter, dockedPanel });
			return form;
		});

		var input = rendered.Find(".splitter");
		Assert.That(input.GetAttribute("style"), Is.EqualTo("position:absolute;width:3px;height:800px;top:0px;left:500px;background-color:var(--color-control);cursor:col-resize;"));
		Assert.That(((IElement)input.Parent.FirstChild).GetAttribute("style"), Is.EqualTo("position:absolute;width:500px;height:800px;top:0px;left:503px;background-color:var(--color-control);overflow:hidden;"));
		Assert.That(((IElement)input.Parent.ChildNodes.Last(n => n is IElement)).GetAttribute("style"), Is.EqualTo("position:absolute;width:500px;height:800px;top:0px;left:0px;background-color:var(--color-control);overflow:hidden;"));

		await rendered.Find(".splitter").TriggerEventAsync("onsplittermoved", new SplitterMovedEventArgs()
		{
			xOffset = -100,
			yOffset = 0
		});

		Assert.That(input.GetAttribute("style"), Is.EqualTo("position:absolute;width:3px;height:800px;top:0px;left:400px;background-color:var(--color-control);cursor:col-resize;"));
		Assert.That(((IElement)input.Parent.FirstChild).GetAttribute("style"), Is.EqualTo("position:absolute;width:600px;height:800px;top:0px;left:403px;background-color:var(--color-control);overflow:hidden;"));
		Assert.That(((IElement)input.Parent.ChildNodes.Last(n => n is IElement)).GetAttribute("style"), Is.EqualTo("position:absolute;width:400px;height:800px;top:0px;left:0px;background-color:var(--color-control);overflow:hidden;"));
	}

	[Test]
	public async Task SplitterAddedAsBetweenControlVertical()
	{
		Splitter splitter = null;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form { Size = new Size(1000, 1203), SizeGripStyle = SizeGripStyle.Hide };
			splitter = new Splitter { Dock = DockStyle.Top };
			var fillPanel = new Panel { Dock = DockStyle.Fill };
			var dockedPanel = new Panel { Size = new Size(1000, 600), Dock = DockStyle.Top };
			form.Controls.AddRange(new Control[] { fillPanel, splitter, dockedPanel });
			return form;
		});

		var input = rendered.Find(".splitter:not(.splitter--horizontal)");
		Assert.That(input.GetAttribute("style"), Is.EqualTo("position:absolute;width:1000px;height:3px;top:600px;left:0px;background-color:var(--color-control);cursor:row-resize;"));
		Assert.That(((IElement)input.Parent.FirstChild).GetAttribute("style"), Is.EqualTo("position:absolute;width:1000px;height:600px;top:603px;left:0px;background-color:var(--color-control);overflow:hidden;"));
		Assert.That(((IElement)input.Parent.ChildNodes.Last(n => n is IElement)).GetAttribute("style"), Is.EqualTo("position:absolute;width:1000px;height:600px;top:0px;left:0px;background-color:var(--color-control);overflow:hidden;"));

		await rendered.Find(".splitter").TriggerEventAsync("onsplittermoved", new SplitterMovedEventArgs()
		{
			xOffset = 0,
			yOffset = -100
		});

		Assert.That(input.GetAttribute("style"), Is.EqualTo("position:absolute;width:1000px;height:3px;top:500px;left:0px;background-color:var(--color-control);cursor:row-resize;"));
		Assert.That(((IElement)input.Parent.FirstChild).GetAttribute("style"), Is.EqualTo("position:absolute;width:1000px;height:700px;top:503px;left:0px;background-color:var(--color-control);overflow:hidden;"));
		Assert.That(((IElement)input.Parent.ChildNodes.Last(n => n is IElement)).GetAttribute("style"), Is.EqualTo("position:absolute;width:1000px;height:500px;top:0px;left:0px;background-color:var(--color-control);overflow:hidden;"));
	}

	[Test]
	public async Task SplitterShouldAdjuestPositionByFormSize()
	{
		Splitter splitter = null;
		Panel panel = null;
		using var ctx = new WinzorTestContext();
		Form newForm = null;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			newForm = new Form();
			splitter = new Splitter() { };
			panel = new Panel { Size = new Size(300, 400), Dock = DockStyle.Left };
			newForm.Controls.Add(splitter);
			newForm.Controls.Add(panel);
			return newForm;
		});
		await newForm.InvokeWinzorDispatcherAsync(() => newForm.Size = new Size(500, 500));
		Assert.That(async () => await GetSplitPosition(splitter), Is.EqualTo(300).After(1000, 100));

		await newForm.InvokeWinzorDispatcherAsync(() => newForm.Size = new Size(300, 300));
		Assert.That(async () => await GetSplitPosition(splitter), Is.EqualTo(272).After(1000, 100));
	}

	[WithPlaywrightPage]
	[TestCase(DockStyle.Left, -100, 100, TestName = "DockedLeftSplitterDragLeft")]
	[TestCase(DockStyle.Left, 100, 300, TestName = "DockedLeftSplitterDragRight")]
	[TestCase(DockStyle.Right, -100, 300, TestName = "DockedRightSplitterDragLeft")]
	[TestCase(DockStyle.Right, 100, 100, TestName = "DockedRightSplitterDragRight")]
	[TestCase(DockStyle.Top, -100, 100, TestName = "DockedTopSplitterDragUp")]
	[TestCase(DockStyle.Top, 100, 300, TestName = "DockedTopSplitterDragDown")]
	[TestCase(DockStyle.Bottom, -100, 300, TestName = "DockedBottomSplitterDragUp")]
	[TestCase(DockStyle.Bottom, 100, 100, TestName = "DockedBottomSplitterDragDown")]
	public async Task SplitterDrag(DockStyle dock, int moveDistance, int expectedSize)
	{
		await using var ctx = new InMemoryTestServerContext();
		var splitter = await SetupAndLoadSplitter(ctx, dock);
		var splitterBar = await Page.WaitForSelectorAsync(".splitter");

		await DragSplitter(splitterBar, moveDistance, moveDistance);

		Assert.That(async () => await GetSplitPosition(splitter), Is.EqualTo(expectedSize).After(1000, 100));
	}

	[WithPlaywrightPage]
	[TestCase(DockStyle.Left, -100, 150, TestName = "DockedLeftSplitterRespectsMinSize")]
	[TestCase(DockStyle.Right, 100, 150, TestName = "DockedRightSplitterRespectsMinSize")]
	[TestCase(DockStyle.Top, -100, 150, TestName = "DockedTopSplitterRespectsMinSize")]
	[TestCase(DockStyle.Bottom, 100, 150, TestName = "DockedBottomSplitterRespectsMinSize")]
	public async Task SplitterDragRespectsMinSize(DockStyle dock, int moveDistance, int expectedSize)
	{
		await using var ctx = new InMemoryTestServerContext();
		var splitter = await SetupAndLoadSplitter(ctx, dock, minSize: 150);
		var splitterBar = await Page.WaitForSelectorAsync(".splitter");

		await DragSplitter(splitterBar, moveDistance, moveDistance);

		Assert.That(async () => await GetSplitPosition(splitter), Is.EqualTo(expectedSize).After(1000, 100));
	}

	[WithPlaywrightPage]
	[TestCase(DockStyle.Left, 100, 272, TestName = "DockedLeftSplitterRespectsMinExtra")]
	[TestCase(DockStyle.Right, -100, 272, TestName = "DockedRightSplitterRespectsMinExtra")]
	[TestCase(DockStyle.Top, 100, 272, TestName = "DockedTopSplitterRespectsMinExtra")]
	[TestCase(DockStyle.Bottom, -100, 272, TestName = "DockedBottomSplitterRespectsMinExtra")]
	public async Task SplitterDragRespectsMinExtra(DockStyle dock, int moveDistance, int expectedSize)
	{
		await using var ctx = new InMemoryTestServerContext();
		var splitter = await SetupAndLoadSplitter(ctx, dock, minExtra: 125);
		var splitterBar = await Page.WaitForSelectorAsync(".splitter");

		await DragSplitter(splitterBar, moveDistance, moveDistance);

		Assert.That(async () => await GetSplitPosition(splitter), Is.EqualTo(expectedSize).After(1000, 100));
	}

	[WithPlaywrightPage]
	[TestCase(DockStyle.Top, TestName = "{m}_Vertical")]
	[TestCase(DockStyle.Left, TestName = "{m}_Horizontal")]
	public async Task SplitterGuideShouldHaveCorrectClassesUponDrag(DockStyle dock)
	{
		await using var ctx = new InMemoryTestServerContext();
		var splitter = await SetupAndLoadSplitter(ctx, dock, minExtra: 125);
		var splitterBar = await Page.WaitForSelectorAsync(".splitter");

		await DragSplitter(splitterBar, 1, 1, false);

		var splitterGuide = await Page.WaitForSelectorAsync(".splitter:last-of-type");
		Assert.That(await splitterGuide.GetAttributeAsync("class"), Does.Contain("splitter__guide"));
	}

	[WithPlaywrightPage]
	[TestCase(DockStyle.Top, "ArrowUp", 175, TestName = "{m}_Vertical_ArrowUp")]
	[TestCase(DockStyle.Top, "ArrowDown", 175, TestName = "{m}_Vertical_ArrowDown")]
	[TestCase(DockStyle.Top, "ArrowLeft", 0, TestName = "{m}_Vertical_ArrowLeft")]
	[TestCase(DockStyle.Top, "ArrowRight", 0, TestName = "{m}_Vertical_ArrowRight")]
	[TestCase(DockStyle.Left, "ArrowUp", 0, TestName = "{m}_Horizontal_ArrowUp")]
	[TestCase(DockStyle.Left, "ArrowDown", 0, TestName = "{m}_Horizontal_ArrowDown")]
	[TestCase(DockStyle.Left, "ArrowLeft", 175, TestName = "{m}_Horizontal_ArrowLeft")]
	[TestCase(DockStyle.Left, "ArrowRight", 175, TestName = "{m}_Horizontal_ArrowRight")]
	public async Task ScrollBarCanMoveOnArrowKey(DockStyle dock, string direction, int expectDistance)
	{
		await using var ctx = new InMemoryTestServerContext();
		var splitter = await SetupAndLoadSplitter(ctx, dock, minExtra: 125);
		var splitterBar = await Page.WaitForSelectorAsync(".splitter");
		var position1 = GetLeftValueFromCss(await splitterBar.GetAttributeAsync("style"), direction);

			await SplitterMoveByKey(splitterBar, direction);

		var splitterBar2 = await Page.WaitForSelectorAsync(".splitter");
		var position2 = GetLeftValueFromCss(await splitterBar2.GetAttributeAsync("style"), direction);
		var distance = position1 - position2;

		Assert.That(distance, Is.EqualTo(expectDistance));
	}

		void CalculateSplitterLocation(ElementHandleBoundingBoxResult splitterBoundingBox, out int splitterX, out int splitterY)
		{
			splitterX = (int)(splitterBoundingBox.X + (splitterBoundingBox.Width / 2));
			splitterY = (int)(splitterBoundingBox.Y + (splitterBoundingBox.Height / 2));
		}

		async Task DragSplitter(IElementHandle splitter, int x, int y, bool mouseUp = true)
		{
			var box = await splitter.BoundingBoxAsync();
			CalculateSplitterLocation(box, out var splitterX, out var splitterY);
			await Page.FocusAsync(".splitter");
			await Page.WaitForTimeoutAsync(200);
			await Page.Mouse.MoveAsync(splitterX, splitterY);
			await Page.Mouse.DownAsync();
			await Page.WaitForTimeoutAsync(100);
			await Page.Mouse.MoveAsync(splitterX + x, splitterY + y);
			await Page.WaitForTimeoutAsync(100);
			if (mouseUp)
			{
				await Page.Mouse.UpAsync();
			}
		}

		async Task SplitterMoveByKey(IElementHandle splitter, string direction)
		{
			var box = await splitter.BoundingBoxAsync();
			CalculateSplitterLocation(box, out var splitterX, out var splitterY);
			await Page.Mouse.MoveAsync(splitterX, splitterY);
			await Page.Mouse.DownAsync();
			await Page.WaitForTimeoutAsync(200);
			await Page.Keyboard.PressAsync(direction);
			await Task.Delay(100);
			await Page.Keyboard.UpAsync(direction);
		}

	async Task<Splitter> SetupAndLoadSplitter(InMemoryTestServerContext ctx, DockStyle dock, int minSize = 25 /* Winforms Default */, int minExtra = 25 /* Winforms Default */)
	{
		Form form = null;

		await ctx.LoadFormAsync(() =>
		{
			form = GetFormForTest(dock, minSize, minExtra);
			return form;
		});

		return form.Controls.OfType<Splitter>().FirstOrDefault();
	}

	Form GetFormForTest(DockStyle dock, int minSize, int minExtra)
	{
#pragma warning disable CA2000 // Dispose objects before losing scope
		var form = new Form() { Size = new Size(400, 400) };
#pragma warning restore CA2000 // Dispose objects before losing scope
		var dockedPanel = new Panel();
		dockedPanel.Controls.Add(new Label { Text = "Docked Panel" });
		var fillPanel = new Panel() { Dock = DockStyle.Fill };
		fillPanel.Controls.Add(new Label { Text = "Fill Panel" });
		var splitter = new Splitter() { MinSize = minSize, MinExtra = minExtra };
		form.Controls.AddRange(new Control[] { fillPanel, splitter, dockedPanel });

		switch (dock)
		{
			case DockStyle.Left:
				{
					dockedPanel.Dock = DockStyle.Left;
					dockedPanel.Size = new Size(200, 400);
					return form;
				}
			case DockStyle.Right:
				{
					dockedPanel.Dock = DockStyle.Right;
					dockedPanel.Size = new Size(200, 400);
					splitter.Dock = DockStyle.Right;
					return form;
				}
			case DockStyle.Top:
				{
					dockedPanel.Dock = DockStyle.Top;
					dockedPanel.Size = new Size(400, 200);
					splitter.Dock = DockStyle.Top;
					return form;
				}
				case DockStyle.Bottom:
				{
					dockedPanel.Dock = DockStyle.Bottom;
					dockedPanel.Size = new Size(400, 200);
					splitter.Dock = DockStyle.Bottom;
					return form;
				}
		}
		return null;
	}

	int GetLeftValueFromCss(string cssStyle, string direction)
	{
		var match = Regex.Match(cssStyle, @$"{GetDirectionValue(direction)}:\s*(\d+)px;");

		if (match.Success)
		{
			var distance = match.Groups[1].Value;
			return int.Parse(distance);
		}

		return -1;
	}

	string GetDirectionValue(string direction) => direction switch
	{
		"ArrowLeft" or "ArrowRight" => "left",
		"ArrowDown" or "ArrowUp" => "top",
		_ => "left"

	};

	[Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "Testing")]
	async Task<int> GetSplitPosition(Splitter splitter)
	{
		var splitPosition = -1;
		Console.WriteLine("Polling..");
		await splitter.InvokeWinzorDispatcherAsync(() =>
		{
			splitPosition = splitter.SplitPosition;
		});
		return splitPosition;
	}
}
