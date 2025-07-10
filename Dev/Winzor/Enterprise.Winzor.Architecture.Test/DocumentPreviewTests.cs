using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.Data.Testing;
using Enterprise.DocumentEngine.GUI;
using Enterprise.RemotePrinting.Engine;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using FlexCel.Core;
using FlexCel.Render;
using FlexCel.XlsAdapter;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using static WTG.PlaywrightTesting.PlaywrightTestContext;
using Point = Blazor.Diagrams.Core.Geometry.Point;
using TextWatermark = Enterprise.RemotePrinting.Engine.TextWatermark;

namespace Enterprise.Winzor.Architecture.Test;

class DocumentPreviewTests
{
	[Test]
	public async Task DocumentSvgPreview()
	{
		using var ctx = new EnterpriseTestContext();
		FlexCelPreviewWithCulture preview = null;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new ZForm();
			preview = GetFlexCelPreviewWithCulture(2);
			form.Controls.Add(preview);

			return form;
		});

		var pages = rendered.FindAll(".document-preview .document-preview__page img");
		var page1Data = pages[0].Attributes["src"].Value;
		var svgData = Uri.UnescapeDataString(page1Data.Replace("data:image/svg+xml;utf8,", ""));

		Assert.That(preview.TotalPages, Is.EqualTo(2));
		Assert.That(pages, Has.Count.EqualTo(2));
		Assert.That(page1Data, Does.StartWith("data:image/svg+xml;utf8,"));
		Assert.That(svgData, Does.StartWith("<svg"));
		Assert.That(svgData, Does.EndWith("</svg>"));
	}

	[TestCase("this", 45, "Arial", 52, FontStyle.Regular, 1, TestName = "{m}_NormalText")]
	[TestCase("a text", 40, "Courier New", 25, FontStyle.Italic, 1, TestName = "{m}_FontItalic")]
	[TestCase("water mark", 30, "Times New Roman", 11, FontStyle.Bold, 1, TestName = "{m}_FontBold")]
	[TestCase("with differ", 50, "Verdana", 11, FontStyle.Strikeout, 1, TestName = "{m}_FontStrikeout")]
	[TestCase("styles", 55, "Courier New", 13, FontStyle.Underline, 1, TestName = "{m}_FontUnderline")]
	[TestCase("new line need\n next \n mark", 60, "Calibri", 50, FontStyle.Regular, 3, TestName = "{m}_LongText")]
	public async Task DocumentSvgPreviewTextWaterMarkStyle(string text, int rotation, string fontFamily, int fontSize, FontStyle fontStyle, int textCount)
	{
		using var ctx = new EnterpriseTestContext();
		FlexCelPreviewWithCulture preview = null;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new ZForm();
			preview = GetFlexCelPreviewWithCulture(1);
			preview.WaterMark = GeneratedWatermark(text, rotation, fontFamily, fontSize, fontStyle, Color.Blue);
			form.Controls.Add(preview);
			return form;
		});

		var pages = rendered.FindAll(".document-preview .document-preview__page img");
		var previewMainDate = pages[0].Attributes["src"].Value;
		var svgData = Uri.UnescapeDataString(previewMainDate.Replace("data:image/svg+xml;utf8,", ""));

		var pattern = @"<g><text.*<\/svg>";
		var match = Regex.Match(svgData, pattern);
		if (match.Success)
		{
			var extractedContent = match.Groups[0].Value;

			Assert.That(extractedContent, Does.Contain(text));
			Assert.That(extractedContent, Does.Contain("-" + rotation));
			Assert.That(extractedContent, Does.Contain(fontFamily));
			Assert.That(extractedContent, Does.Contain("font-size=\"" + fontSize));
			Assert.That(extractedContent, Does.Contain("fill=\"rgba(0, 0, 255, 1)\""));
			Assert.That(extractedContent, Does.Contain("transform=\"rotate"));

			var textPattern = "</text>";
			var matchedSpans = Regex.Matches(extractedContent, textPattern);

			Assert.That(matchedSpans.Count, Is.EqualTo(textCount));
		}
	}

	[TestCase(WatermarkHorizontalAlign.Centre, WatermarkVerticalAlign.Bottom, "158.63779", "771.8898", "307.6378, 801.8898")]
	[TestCase(WatermarkHorizontalAlign.Centre, WatermarkVerticalAlign.Middle, "158.63779", "400.9449", "307.6378, 430.9449")]
	[TestCase(WatermarkHorizontalAlign.Centre, WatermarkVerticalAlign.Top, "158.63779", "10", "307.6378, 40")]
	[TestCase(WatermarkHorizontalAlign.Left, WatermarkVerticalAlign.Bottom, "10", "771.8898", "159, 801.8898")]
	[TestCase(WatermarkHorizontalAlign.Left, WatermarkVerticalAlign.Middle, "10", "400.9449", "159, 430.9449")]
	[TestCase(WatermarkHorizontalAlign.Left, WatermarkVerticalAlign.Top, "10", "10", "159, 40")]
	[TestCase(WatermarkHorizontalAlign.Right, WatermarkVerticalAlign.Bottom, "287.27557", "771.8898", "436.27557, 801.8898")]
	[TestCase(WatermarkHorizontalAlign.Right, WatermarkVerticalAlign.Middle, "287.27557", "400.9449", "436.27557, 430.9449")]
	[TestCase(WatermarkHorizontalAlign.Right, WatermarkVerticalAlign.Top, "287.27557", "10", "436.27557, 40")]
	public async Task DocumentSvgPreviewTextWaterMarkPosition(WatermarkHorizontalAlign horizontalAlign, WatermarkVerticalAlign verticalAlign, string x, string y, string rotate)
	{
		using var ctx = new EnterpriseTestContext();
		FlexCelPreviewWithCulture preview = null;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new ZForm();
			preview = GetFlexCelPreviewWithCulture(1);
			preview.WaterMark = new TextWatermark("Watermark",
				horizontalAlign,
				verticalAlign,
				10f,
				10f,
				45,
				Color.Blue,
				"Arial",
				52,
				FontStyle.Bold);
			form.Controls.Add(preview);
			return form;
		});

		var pages = rendered.FindAll(".document-preview .document-preview__page img");
		var previewMainDate = pages[0].Attributes["src"].Value;
		var svgData = Uri.UnescapeDataString(previewMainDate.Replace("data:image/svg+xml;utf8,", ""));

		var pattern = @"<g><text.*<\/svg>";
		var match = Regex.Match(svgData, pattern);
		Assert.That(match.Success, Is.EqualTo(true));
		var extractedContent = match.Groups[0].Value;
		Assert.That(extractedContent, Does.Contain($"transform=\"rotate(-45, {rotate})\""));
		Assert.That(extractedContent, Does.Contain("font-weight=\"bold\""));
		Assert.That(extractedContent, Does.Contain($"x=\"{x}\""));
		Assert.That(extractedContent, Does.Contain($"y=\"{y}\""));

		pattern = @"<svg.*?width=""(.*?)pt"".*?height=""(.*?)pt""";
		match = Regex.Match(svgData, pattern);
		Assert.That(match.Success, Is.EqualTo(true));
		var width = double.TryParse(match.Groups[1].Value, out var value) ? value : 0;
		var height = double.TryParse(match.Groups[2].Value, out var value2) ? value2 : 0;
		Assert.That(preview.svgPageWidth, Is.EqualTo(width));
		Assert.That(preview.svgPageHeight, Is.EqualTo(height));
	}

	[Test]
	public async Task DocumentSvgPreviewTextHasWaterMark([Values] bool hasWatermark)
	{
		using var ctx = new EnterpriseTestContext();
		FlexCelPreviewWithCulture preview = null;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new ZForm();
			preview = GetFlexCelPreviewWithCulture(1);
			preview.ThumbnailSmall = GetFlexCelPreviewWithCulture(1);
			preview.ThumbnailSmall.WaterMark = hasWatermark ? GeneratedWatermark("test", 45, "Arial", 40, FontStyle.Regular, Color.Blue) : null;
			preview.WaterMark = hasWatermark ? GeneratedWatermark("test", 45, "Arial", 40, FontStyle.Regular, Color.Blue) : null;
			form.Controls.Add(preview);
			form.Controls.Add(preview.ThumbnailSmall);
			return form;
		});

		var pages = rendered.FindAll(".document-preview .document-preview__page img");
		Assert.That(pages.Count, Is.EqualTo(2));

		var previewMainData = pages[0].Attributes["src"].Value;
		var thumbnailData = pages[1].Attributes["src"].Value;
		var previewMainSvgData = Uri.UnescapeDataString(previewMainData.Replace("data:image/svg+xml;utf8,", ""));
		var thumbnailSvgData = Uri.UnescapeDataString(thumbnailData.Replace("data:image/svg+xml;utf8,", ""));
		var expectedContent = "test</text></g></svg>";

		Assert.That(previewMainSvgData, hasWatermark ? Does.EndWith(expectedContent) : Does.Not.EndWith(expectedContent));
		Assert.That(thumbnailSvgData, hasWatermark ? Does.EndWith(expectedContent) : Does.Not.EndWith(expectedContent));
	}

	[Test]
	public async Task DocumentPreviewEmptyWhenExceptionThrown()
	{
		using var ctx = new EnterpriseTestContext();
		FlexCelPreviewWithCulture preview = null;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new ZForm();
			preview = new FlexCelPreviewWithCulture();
			var export = new FlexCelSVGExportForTest();
			var xls = new XlsFile(1, TExcelFileFormat.v2019, true);
			export.Workbook = xls;
			preview.Document = export;
			form.Controls.Add(preview);
			return form;
		});

		Assert.That(preview.TotalPages, Is.EqualTo(0));
		Assert.That(rendered.FindAll(".document-preview .document-preview__page"), Has.Count.EqualTo(0));
		Assert.That(UnitTestUserNotification.Instance.LastMessage.Text, Is.EqualTo(@"An error has occured while previewing the document -
Error message is: RasterizeError"));
	}

	[Test]
	public async Task DocumentPreviewEmptyWhenNoWorkBook()
	{
		using var ctx = new EnterpriseTestContext();
		FlexCelPreviewWithCulture preview = null;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new ZForm();
			preview = new FlexCelPreviewWithCulture();
			form.Controls.Add(preview);
			return form;
		});

		Assert.That(preview.TotalPages, Is.EqualTo(0));
		Assert.That(rendered.FindAll(".document-preview .document-preview__page"), Has.Count.EqualTo(0));
	}

	[Test]
	public async Task DocumentPreviewHasCorrectStartPage()
	{
		using var ctx = new EnterpriseTestContext();
		FlexCelPreviewWithCulture preview = null;

		await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			preview = GetFlexCelPreviewWithCulture(10);
			form.Controls.Add(preview);
			return form;
		});

		Assert.That(preview.TotalPages, Is.EqualTo(10));
		Assert.That(preview.StartPage, Is.EqualTo(1));

		await preview.InvokeWinzorDispatcherAsync(() => preview.StartPage = 5);
		Assert.That(preview.StartPage, Is.EqualTo(5));
		await preview.InvokeWinzorDispatcherAsync(() => preview.StartPage = -1);
		Assert.That(preview.StartPage, Is.EqualTo(1));
		await preview.InvokeWinzorDispatcherAsync(() => preview.StartPage = 11);
		Assert.That(preview.StartPage, Is.EqualTo(10));
	}

	[WithPlaywrightPage]
	[TestCase(0.1)]
	[TestCase(1)]
	[TestCase(1.5)]
	public async Task DocumentPreviewPageImgZoom(double zoom)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var preview = GetFlexCelPreviewWithCulture(1);
			preview.Zoom = zoom;
			return preview;
		});

		var pageLocator = page.Locator(".document-preview__page img");
		Assert.That(await pageLocator.GetComputedStyleAsync("zoom"), Is.EqualTo($"{zoom}"));
	}

	[Test, WithPlaywrightPage]
	public async Task DocumentPreviewStyles()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => GetFlexCelPreviewWithCulture(1));

		var previewLocator = page.Locator(".document-preview");
		Assert.That(await previewLocator.GetComputedStyleAsync("display"), Is.EqualTo("block"));
		Assert.That(await previewLocator.GetComputedStyleAsync("position"), Is.EqualTo("absolute"));
		Assert.That(await previewLocator.GetComputedStyleAsync("left"), Is.EqualTo("0px"));
		Assert.That(await previewLocator.GetComputedStyleAsync("top"), Is.EqualTo("0px"));
		Assert.That(await previewLocator.GetComputedStyleAsync("background-color"), Is.EqualTo("rgb(128, 128, 128)"));
		Assert.That(await previewLocator.GetComputedStyleAsync("overflow"), Is.EqualTo("auto"));
	}

	// Turning off Headless mode because it affects the width of scrollbar,
	// otherwise the results in DAT and local could be different for this test
	[Test, WithPlaywrightPage(Headless = false)]
	public async Task DocumentPreviewOuterStyles()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => GetFlexCelPreviewWithCulture(1));

		var outerLocator = page.Locator(".document-preview__outer");
		Assert.That(await outerLocator.GetComputedStyleAsync("display"), Is.EqualTo("block"));
		Assert.That((await outerLocator.GetComputedStyleAsync("width")).AsPixels, Is.EqualTo(133).Within(1));
		Assert.That(await outerLocator.GetComputedStyleAsync("margin"), Is.EqualTo("0px"));
		Assert.That(await outerLocator.GetComputedStyleAsync("padding"), Is.EqualTo("10px"));
	}

	[Test, WithPlaywrightPage]
	public async Task DocumentPreviewInnerStyles()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => GetFlexCelPreviewWithCulture(1));

		var innerLocator = page.Locator(".document-preview__inner");
		Assert.That(await innerLocator.GetComputedStyleAsync("display"), Is.EqualTo("block"));
		Assert.That((await innerLocator.GetComputedStyleAsync("width")).AsPixels, Is.EqualTo(795).Within(1));
		Assert.That(await innerLocator.GetComputedStyleAsync("margin"), Is.EqualTo("0px"));
		Assert.That(await innerLocator.GetComputedStyleAsync("padding"), Is.EqualTo("0px"));
	}

	[Test, WithPlaywrightPage]
	public async Task DocumentPreviewMainPageStyles()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var preview = GetFlexCelPreviewWithCulture(1);
			preview.ThumbnailSmall = GetFlexCelPreviewWithCulture(1);
			return preview;
		});

		var pageLocator = page.Locator(".document-preview__page");
		Assert.That(await pageLocator.GetComputedStyleAsync("pointer-events"), Is.EqualTo("none"));
		Assert.That(await pageLocator.GetComputedStyleAsync("display"), Is.EqualTo("block"));
		Assert.That(await pageLocator.GetComputedStyleAsync("background-color"), Is.EqualTo("rgb(255, 255, 255)"));
		Assert.That(await pageLocator.GetComputedStyleAsync("border"), Is.EqualTo("1px solid rgb(0, 0, 0)"));
		Assert.That(await pageLocator.GetComputedStyleAsync("box-shadow"), Is.EqualTo("rgb(0, 0, 0) 3px 3px 0px 0px"));
		Assert.That(await pageLocator.GetComputedStyleAsync("outline"), Is.EqualTo("rgb(0, 0, 0) none 0px"));
	}

	[Test, WithPlaywrightPage]
	public async Task DocumentPreviewThumbPageStyles()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var preview = GetFlexCelPreviewWithCulture(1);
			preview.ThumbnailLarge = GetFlexCelPreviewWithCulture(1);
			return preview;
		});

		var pageLocator = page.Locator(".document-preview__page");
		Assert.That(await pageLocator.GetComputedStyleAsync("pointer-events"), Is.EqualTo("none"));
		Assert.That(await pageLocator.GetComputedStyleAsync("display"), Is.EqualTo("block"));
		Assert.That(await pageLocator.GetComputedStyleAsync("background-color"), Is.EqualTo("rgb(255, 255, 255)"));
		Assert.That(await pageLocator.GetComputedStyleAsync("border"), Is.EqualTo("1px solid rgb(0, 0, 0)"));
		Assert.That(await pageLocator.GetComputedStyleAsync("box-shadow"), Is.EqualTo("rgb(0, 0, 0) 3px 3px 0px 0px"));
		Assert.That(await pageLocator.GetComputedStyleAsync("outline"), Is.EqualTo("rgb(0, 0, 128) solid 2px"));
	}

	[Test, WithPlaywrightPage]
	public async Task DocumentPreviewPageNoStyles()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var preview = GetFlexCelPreviewWithCulture(1);
			preview.ThumbnailLarge = GetFlexCelPreviewWithCulture(1);
			preview.ShowThumbsPageNumber = true;
			return preview;
		});

		var pageNoLocator = page.Locator(".document-preview__pageno");
		Assert.That(await pageNoLocator.GetComputedStyleAsync("pointer-events"), Is.EqualTo("none"));
		Assert.That(await pageNoLocator.GetComputedStyleAsync("display"), Is.EqualTo("block"));
		Assert.That(await pageNoLocator.GetComputedStyleAsync("position"), Is.EqualTo("relative"));
		Assert.That(await pageNoLocator.GetComputedStyleAsync("top"), Is.EqualTo("3px"));
		Assert.That(await pageNoLocator.GetComputedStyleAsync("left"), Is.EqualTo("3px"));
		Assert.That((await pageNoLocator.GetComputedStyleAsync("width")).AsPixels, Is.EqualTo(75).Within(1));
		Assert.That(await pageNoLocator.GetComputedStyleAsync("height"), Is.EqualTo("13px"));
		Assert.That(await pageNoLocator.GetComputedStyleAsync("margin"), Is.EqualTo("0px"));
		Assert.That(await pageNoLocator.GetComputedStyleAsync("text-align"), Is.EqualTo("center"));
		Assert.That(await pageNoLocator.GetComputedStyleAsync("color"), Is.EqualTo("rgb(255, 255, 255)"));
		Assert.That(await pageNoLocator.GetComputedStyleAsync("background-color"), Is.EqualTo("rgb(0, 0, 128)"));
	}

	[Test, WithPlaywrightPage]
	public async Task ClickOnPreviewThumbsWillScrollItIntoView()
	{
		FlexCelPreview previewThumb = null;
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			previewThumb = GetFlexCelPreviewWithCulture(10);
			form.Height = previewThumb.Height = 600;
			previewThumb.ThumbnailLarge = GetFlexCelPreviewWithCulture(10);
			form.Controls.Add(previewThumb);
			return form;
		});

		var previewLocator = page.Locator(".document-preview");
		var firstPageLocator = previewLocator.Locator(".document-preview__outer:nth-child(2)");  // fully visbile
		var thirdPageLocator = previewLocator.Locator(".document-preview__outer:nth-child(4)");  // fully visible
		var fourthPageLocator = previewLocator.Locator(".document-preview__outer:nth-child(5)"); // partially visible

		await firstPageLocator.ClickAsync();
		Assert.That(() => previewLocator.EvaluateAsync<int>("e => e.scrollTop"), Is.EqualTo(0).After(500, 50));

		await thirdPageLocator.ClickAsync();
		Assert.That(() => previewLocator.EvaluateAsync<int>("e => e.scrollTop"), Is.EqualTo(0).After(500, 50));

		await fourthPageLocator.ClickAsync();
		var expectedScrollTop = await fourthPageLocator.EvaluateAsync<int>($"e => e.offsetTop + e.offsetHeight + {previewThumb.RealYSep} - e.parentElement.offsetHeight");
		Assert.That(() => previewLocator.EvaluateAsync<int>("e => e.scrollTop"), Is.EqualTo(expectedScrollTop).Within(1).After(500, 50));

		await firstPageLocator.ClickAsync();
		Assert.That(() => previewLocator.EvaluateAsync<int>("e => e.scrollTop"), Is.EqualTo(0).After(500, 50));
	}

	[Test, WithPlaywrightPage]
	public async Task DocumentPreviewSinglePageScrollToBotttom()
	{
		FlexCelPreview preview = null;
		const int totalPages = 1;
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Width = 200, Height = 1000 };
			preview = GetFlexCelPreviewWithCulture(totalPages);
			preview.ThumbnailSmall = GetFlexCelPreviewWithCulture(totalPages);
			preview.Zoom = .5;
			preview.Width = 200;
			preview.Height = 1000;
			form.Controls.Add(preview);
			return form;
		});

		var previewLocator = page.Locator(".document-preview");

		Assert.That(preview.TotalPages, Is.EqualTo(1));
		Assert.That(preview.StartPage, Is.EqualTo(1));

		await preview.InvokeWinzorDispatcherAsync(() => preview.StartPage++); // GoToNextPageButton clicked
		Assert.That(() => previewLocator.EvaluateAsync<int>("e => e.scrollBottom"), Is.EqualTo(0).After(500, 50));

		await preview.InvokeWinzorDispatcherAsync(() => preview.StartPage--); // GoToPrevPageButton clicked
		Assert.That(() => previewLocator.EvaluateAsync<int>("e => e.scrollTop"), Is.EqualTo(0).After(500, 50));

		await preview.InvokeWinzorDispatcherAsync(() => preview.StartPage++); // GoToNextPageButton clicked
		Assert.That(() => previewLocator.EvaluateAsync<int>("e => e.scrollBottom"), Is.EqualTo(0).After(500, 50));

		await preview.InvokeWinzorDispatcherAsync(() => preview.StartPage = 1); // GotoFirstPageButton clicked
		Assert.That(() => previewLocator.EvaluateAsync<int>("e => e.scrollTop"), Is.EqualTo(0).After(500, 50));

		await preview.InvokeWinzorDispatcherAsync(() => preview.StartPage++); // GoToNextPageButton clicked
		Assert.That(() => previewLocator.EvaluateAsync<int>("e => e.scrollBottom"), Is.EqualTo(0).After(500, 50));

		await preview.InvokeWinzorDispatcherAsync(() => preview.StartPage = preview.TotalPages); // GoToLastPageButton clicked
		Assert.That(() => previewLocator.EvaluateAsync<int>("e => e.scrollTop"), Is.EqualTo(0).After(500, 50));
	}

	[Test, WithPlaywrightPage, UseSnapshotProtection]
	public async Task ClickOnPreviewThumbsWillScrollPreviewMainIntoViewAtTop()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var panel = new Panel();
			var previewMain = GetFlexCelPreviewWithCulture(10);
			var previewThumbs = GetFlexCelPreviewWithCulture(10);
			form.Height = panel.Height = previewMain.Height = previewThumbs.Height = 1000;
			previewMain.ThumbnailSmall = previewThumbs;
			previewThumbs.ThumbnailLarge = previewMain;
			panel.Controls.Add(previewMain);
			panel.Controls.Add(previewThumbs);
			form.Controls.Add(panel);
			return form;
		});

		var previewThumbsLocator = page.Locator(".document-preview:nth-child(2)");
		var outerThumbsLocator = previewThumbsLocator.Locator(".document-preview__outer:nth-child(5)");

		await outerThumbsLocator.ClickAsync();

		var previewLocator = page.Locator(".document-preview:nth-child(1)");
		var outerLocator = previewLocator.Locator(".document-preview__outer:nth-child(5)");

		var previewBoxY = 0f;
		async Task<float> UpdatePreviewBoxYAndReturnOuterBoxYAsync()
		{
			previewBoxY = (await previewLocator.BoundingBoxAsync()).Y;
			return (await outerLocator.BoundingBoxAsync()).Y;
		}
		Assert.That(UpdatePreviewBoxYAndReturnOuterBoxYAsync, Is.EqualTo(previewBoxY).Within(1).After(500, 50));
	}

	[Test, WithPlaywrightPage]
	public async Task PreviewThumbsPageSelectedStyles()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var preview = GetFlexCelPreviewWithCulture(1);
			preview.ThumbnailLarge = GetFlexCelPreviewWithCulture(1);
			preview.ShowThumbsPageNumber = true;
			form.Controls.Add(preview);
			return form;
		});

		var outerLocator = page.Locator(".document-preview__outer");
		var pageLocator = page.Locator(".document-preview__page");
		var pageNoLocator = page.Locator(".document-preview__pageno");

		await outerLocator.ClickAsync();

		Assert.That(() => pageLocator.GetComputedStyleAsync("outline"), Is.EqualTo("rgb(0, 0, 128) solid 2px").After(500, 50));
		Assert.That(await pageNoLocator.GetComputedStyleAsync("color"), Is.EqualTo("rgb(255, 255, 255)"));
		Assert.That(await pageNoLocator.GetComputedStyleAsync("background-color"), Is.EqualTo("rgb(0, 0, 128)"));
	}

	[Test, WithPlaywrightPage]
	public async Task TestThumbnailsShouldNotBeDraggable()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var previewThumbs = GetFlexCelPreviewWithCulture(1);
			form.Height = previewThumbs.Height = 1000;
			form.Controls.Add(previewThumbs);
			return form;
		});

		var imgLocator = page.Locator(".document-preview__page img");
		Assert.That(await imgLocator.EvaluateAsync<bool>("el => el.hasAttribute('draggable')"), Is.EqualTo(true));
		Assert.That(await imgLocator.EvaluateAsync<bool>("el => el.getAttribute('draggable')"), Is.EqualTo(false));
	}

	[Test, WithPlaywrightPage]
	public async Task DragScrollPreviewMain()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var preview = GetFlexCelPreviewWithCulture(10);
			form.Height = preview.Height = 1000;
			preview.ThumbnailSmall = GetFlexCelPreviewWithCulture(10);
			form.Controls.Add(preview);
			return form;
		});

		var previewLocator = await Page.WaitForSelectorAsync(".document-preview");
		var previewBox = await previewLocator.BoundingBoxAsync();

		await page.Mouse.MoveAsync(previewBox.X + 10, previewBox.Y + 10);
		await page.Mouse.DownAsync();

		Assert.That(async () => await previewLocator.EvaluateAsync<int>("e => e.scrollTop"), Is.EqualTo(0).After(500, 50));
		Assert.That(async () => await previewLocator.EvaluateAsync<int>("e => e.scrollLeft"), Is.EqualTo(0));

		await page.Mouse.MoveAsync(previewBox.X, previewBox.Y);
		await page.Mouse.UpAsync();

		Assert.That(async () => await previewLocator.EvaluateAsync<int>("e => e.scrollTop"), Is.EqualTo(10).After(500, 50));
		Assert.That(async () => await previewLocator.EvaluateAsync<int>("e => e.scrollLeft"), Is.EqualTo(10));
	}

	[Test, WithPlaywrightPage]
	public async Task Preview_ClickOutsideThenClickInside_ExpectNoScrollOnMouseMove()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var preview = GetFlexCelPreviewWithCulture(10);
			form.Height = preview.Height = 1000;
			form.Width = preview.Width = 750;
			preview.ThumbnailSmall = GetFlexCelPreviewWithCulture(10);
			form.Controls.Add(preview);
			return form;
		});

		var previewLocator = page.Locator(".document-preview");
		var previewBox = await previewLocator.BoundingBoxAsync();

		await page.Mouse.ClickAsync(form.Width + 50, 0); // Click outside the document window.
		await page.Mouse.ClickAsync(previewBox.X + 50, previewBox.Y + 50); // Click inside the document.
		await page.Mouse.MoveAsync(previewBox.X + 50, previewBox.Y + 50); // Move the mouse.
		await page.Mouse.MoveAsync(previewBox.X, previewBox.Y); // Move mouse back to origin.

		Assert.That(async () => await ScrollOfAsync(previewLocator), Is.EqualTo(new Point(0, 0)).After(3000, 10));
	}

	[Test, WithPlaywrightPage]
	public async Task PreviewThumbsKeyboardNavigation()
	{
		FlexCelPreview preview = null;
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			preview = GetFlexCelPreviewWithCulture(10);
			preview.ThumbnailLarge = GetFlexCelPreviewWithCulture(10);
			form.Height = preview.Height = 1000;
			form.Controls.Add(preview);
			return form;
		});

		var previewLocator = page.Locator(".document-preview");
		Assert.That(preview.StartPage, Is.EqualTo(1));

		async Task AssertStartPageAfterKeyPress(string key, int startPage)
		{
			await previewLocator.PressAsync(key);
			Assert.That(() => preview.StartPage, Is.EqualTo(startPage).After(500, 50));
		}

		await AssertStartPageAfterKeyPress("ArrowDown", 2);
		await AssertStartPageAfterKeyPress("ArrowRight", 3);
		await AssertStartPageAfterKeyPress("ArrowUp", 2);
		await AssertStartPageAfterKeyPress("ArrowLeft", 1);
		await AssertStartPageAfterKeyPress("PageDown", 7);
		await AssertStartPageAfterKeyPress("PageUp", 1);
		await AssertStartPageAfterKeyPress("Control+ArrowDown", 7);
		await AssertStartPageAfterKeyPress("Control+ArrowUp", 1);
		await AssertStartPageAfterKeyPress("Control+PageDown", 10);
		await AssertStartPageAfterKeyPress("Control+PageUp", 1);
		await AssertStartPageAfterKeyPress("End", 10);
		await AssertStartPageAfterKeyPress("Home", 1);
		await AssertStartPageAfterKeyPress("Control+End", 10);
		await AssertStartPageAfterKeyPress("Control+Home", 1);
	}

	// Turning off Headless mode because it affects the width of scrollbar,
	// otherwise the results in DAT and local could be different for this test
	[Test, WithSnapshotProtection, WithPlaywrightPage(Headless = false)]
	public async Task PreviewMainKeyboardNavigationAndUpdateStartPageAfterScroll()
	{
		FlexCelPreview preview = null;
		const int totalPages = 5;
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Width = 200, Height = 1000 };
			preview = GetFlexCelPreviewWithCulture(totalPages);
			preview.ThumbnailSmall = GetFlexCelPreviewWithCulture(totalPages);
			preview.Zoom = .5;
			preview.Width = 200;
			preview.Height = 1000;
			form.Controls.Add(preview);
			return form;
		});

		var previewLocator = page.Locator(".document-preview");
		await previewLocator.ClickAsync();

		async Task AssertScrollLeftTopAndStartPageAfterKeyPress(string key, int scrollLeft, int scrollTop, int startPage)
		{
			// 1. Ensure proper focus before key press
			bool hasFocus = await previewLocator.EvaluateAsync<bool>("e => document.activeElement === e");
			if (!hasFocus)
			{
				await previewLocator.FocusAsync();
				await previewLocator.WaitForAsync();
			}

			// 2. Press the key
			await previewLocator.PressAsync(key);

			// 3. Use longer timeouts for the assertions instead of a custom wait
			Assert.That(() => previewLocator.EvaluateAsync<int>("e => e.scrollLeft"),
				Is.EqualTo(scrollLeft).Within(1).After(1000, 100));
			Assert.That(() => previewLocator.EvaluateAsync<int>("e => e.scrollTop"),
				Is.EqualTo(scrollTop).Within(1).After(1000, 100));

			// 4. Wait a bit longer for the StartPage to update
			Assert.That(() => preview.StartPage, Is.EqualTo(startPage).After(3000, 100));
		}

		await Assertions.Expect(page.Locator(".document-preview__outer")).ToHaveCountAsync(totalPages);

		await AssertScrollLeftTopAndStartPageAfterKeyPress("ArrowDown", 0, 10, 1);
		await AssertScrollLeftTopAndStartPageAfterKeyPress("ArrowRight", 10, 10, 1);
		await AssertScrollLeftTopAndStartPageAfterKeyPress("ArrowUp", 10, 0, 1);
		await AssertScrollLeftTopAndStartPageAfterKeyPress("ArrowLeft", 0, 0, 1);

		const int scrollBarWidth = 17;
		var clientWidth = preview.ClientSize.Width - scrollBarWidth;
		var clientHeight = preview.ClientSize.Height - scrollBarWidth;

		await AssertScrollLeftTopAndStartPageAfterKeyPress("Control+ArrowDown", 0, clientHeight, 2);
		await AssertScrollLeftTopAndStartPageAfterKeyPress("Control+ArrowRight", clientWidth, clientHeight, 2);
		await AssertScrollLeftTopAndStartPageAfterKeyPress("Control+ArrowUp", clientWidth, 0, 1);
		await AssertScrollLeftTopAndStartPageAfterKeyPress("Control+ArrowLeft", 0, 0, 1);

		await AssertScrollLeftTopAndStartPageAfterKeyPress("PageDown", 0, clientHeight, 2);
		await AssertScrollLeftTopAndStartPageAfterKeyPress("PageUp", 0, 0, 1);

		var scrollTop = await previewLocator.EvaluateAsync<int>("e => e.scrollHeight") - clientHeight;

		await AssertScrollLeftTopAndStartPageAfterKeyPress("End", 0, scrollTop, totalPages);
		await AssertScrollLeftTopAndStartPageAfterKeyPress("Home", 0, 0, 1);
		await AssertScrollLeftTopAndStartPageAfterKeyPress("Control+PageDown", 0, scrollTop, totalPages);
		await AssertScrollLeftTopAndStartPageAfterKeyPress("Control+PageUp", 0, 0, 1);
		await AssertScrollLeftTopAndStartPageAfterKeyPress("Control+End", 0, scrollTop, totalPages);
		await AssertScrollLeftTopAndStartPageAfterKeyPress("Control+Home", 0, 0, 1);
	}

	[Test, WithPlaywrightPage]
	public async Task NoExceptionWhenPreviewIsUndefined()
	{
		const string DocumentPreviewJS = "/_content/Enterprise.DocumentEngine.GUI/js/documentPreview.js";

		FlexCelPreview preview = null;
		Form form = null;
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			preview = GetFlexCelPreviewWithCulture(10);
			preview.ThumbnailLarge = GetFlexCelPreviewWithCulture(10);
			form.Height = preview.Height = 800;
			form.Width = 1000;
			form.Controls.Add(preview);
			return form;
		});

		using var dotNetReference = DotNetObjectReference.Create(form.CargoWiseClientServices);
		var module = await form.CargoWiseClientServices.JSRuntime.InvokeAsync<IJSObjectReference>("import", DocumentPreviewJS);

		var badElementReference = new ElementReference();

		Assert.Multiple(() =>
		{
			Assert.DoesNotThrowAsync(async () => await module.InvokeVoidAsync("scrollMainPageIntoView", badElementReference, 2));
			Assert.DoesNotThrowAsync(async () => await module.InvokeVoidAsync("scrollThumbPageIntoView", badElementReference, 2, 3, 4));
			Assert.DoesNotThrowAsync(async () => await module.InvokeVoidAsync("startDragScroll", dotNetReference, badElementReference, 2));
			Assert.DoesNotThrowAsync(async () => await module.InvokeVoidAsync("initialiseMainIntersectionObserver", dotNetReference, badElementReference));
		});
	}

	[Test, WithPlaywrightPage]
	public async Task DocumentPreviewUseBlazorVirtualize()
	{
		FlexCelPreview previewThumb = null;
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			previewThumb = GetFlexCelPreviewWithCulture(100);
			form.Height = previewThumb.Height = 600;
			previewThumb.ThumbnailLarge = GetFlexCelPreviewWithCulture(100);
			form.Controls.Add(previewThumb);
			return form;
		});

		var previewLocator = page.Locator(".document-preview");
		await Assertions.Expect(previewLocator.Locator("div:not([class])")).ToHaveCountAsync(2);
		await Assertions.Expect(previewLocator.Locator(".document-preview__outer")).ToHaveCountAsync(11);
	}

	[Test, WithPlaywrightPage]
	public async Task PressControlKeyAndScrollMouseWheelShouldZoomPreview()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var preview = default(FlexCelPreview);
		var page = await ctx.LoadControlOnFormAsync(() => preview  = GetFlexCelPreviewWithCulture(1));
		Assert.That(preview.Zoom, Is.EqualTo(1.0).Within(0.0005));

		await page.Locator(".document-preview").DispatchEventAsync("wheel", new { deltaY = -120, ctrlKey = true });
		Assert.That(() => preview.Zoom, Is.EqualTo(1.1).Within(0.0005).After(1000, 100));

		await page.Locator(".document-preview").DispatchEventAsync("wheel", new { deltaY = 240, ctrlKey = true });
		Assert.That(() => preview.Zoom, Is.EqualTo(0.9).Within(0.0005).After(1000, 100));
	}

	static async Task<Point> ScrollOfAsync(ILocator locator)
	{
		var box = await locator.BoundingBoxAsync()!;
		Assert.That(box, Is.Not.Null);
		double scrollLeft = await locator.EvaluateAsync<double>("control => control.scrollLeft");
		double scrollTop = await locator.EvaluateAsync<double>("control => control.scrollTop");
		return new Point(scrollLeft, scrollTop);
	}

	FlexCelPreviewWithCulture GetFlexCelPreviewWithCulture(int pages)
	{
		var preview = new FlexCelPreviewWithCulture();
		var xls = new XlsFile(1, TExcelFileFormat.v2019, true);
		for (var i = 1; i <= 53 * pages; i++)
		{
			xls.SetCellValue(i, 1, i.ToString());
		}
		var export = new FlexCelSVGExport();
		export.Workbook = xls;
		preview.Document = export;
		return preview;
	}

	TextWatermark GeneratedWatermark(string text, int rotation, string fontName, int fontSize, FontStyle fontStyle, Color color) =>
						new TextWatermark(text,
						WatermarkHorizontalAlign.Centre,
						WatermarkVerticalAlign.Middle,
						0,
						0,
						rotation,
						color,
						fontName,
						fontSize,
						fontStyle);

	class FlexCelSVGExportForTest : FlexCelSVGExport
	{
		public override bool RasterizeSVGImages { get => throw new Exception("RasterizeError"); }
	}
}
