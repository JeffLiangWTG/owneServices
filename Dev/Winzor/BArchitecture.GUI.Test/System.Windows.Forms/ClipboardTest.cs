using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bunit;
using CargoWise.Blazor.Client.Integration;
using Microsoft.JSInterop;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;

using static PlaywrightTestContext;

[Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1088:Do Not Use System.Windows.Forms.Clipboard", Justification = "Is Unit Test")]
sealed class ClipboardTest
{
	[Test]
	public async Task TestClipboardUnicodeText()
	{
		using var ctx = new WinzorTestContext();

		var form = default(Form);
		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			return form;
		});

		const string DemoText = "Hello, World! 你好，世界";
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			var dObj = new DataObject();
			dObj.SetData(DataFormats.UnicodeText, autoConvert: false, DemoText);

			Clipboard.SetDataObject(dObj);
		});

		var setDataObject = ctx.JSInterop.VerifyInvoke("clipboard.setDataObject");
		var clipboardArgs = setDataObject.Arguments[1] as Dictionary<string, object>;
		var unicodeText = clipboardArgs[MediaTypeNames.Text.Plain];

		Assert.That(unicodeText, Is.EqualTo(DemoText));
	}

	[Test]
	public async Task TestClipboardFileDrop()
	{
		var tempFilePath = Path.GetTempFileName();
		Stream stream = null;
		try
		{
			using var ctx = new WinzorTestContext();

			var form = default(Form);
			var rendered = await ctx.RenderFormAsync(() =>
			{
				form = new Form();
				return form;
			});

			await form.InvokeWinzorDispatcherAsync(() =>
			{
				var dObj = new DataObject();
				dObj.SetData(DataFormats.FileDrop, autoConvert: false, new[] { tempFilePath });
				Clipboard.SetDataObject(dObj);
			});

			var setDataObject = ctx.JSInterop.VerifyInvoke("clipboard.setDataObject");
			var dict = setDataObject.Arguments[1] as Dictionary<string, object>;
			Assert.That(dict[CustomMimeTypes.Application.FileDrop], Is.TypeOf<DotNetStreamReference>());

			var dotNetStreamReference = dict[CustomMimeTypes.Application.FileDrop] as DotNetStreamReference;
			stream = dotNetStreamReference.Stream;
		}
		finally
		{
			if (stream != null)
			{
				stream.Close();
			}
			if (File.Exists(tempFilePath))
			{
				File.Delete(tempFilePath);
			}
		}
	}

	[Test]
	public async Task TestClipboardHtml()
	{
		using var ctx = new WinzorTestContext();

		var form = default(Form);
		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			return form;
		});

		const string DemoHtml = "<p>Hello, <strong>World!</strong></p>";
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			var dObj = new DataObject();
			dObj.SetData(DataFormats.Html, autoConvert: false, DemoHtml);
			Clipboard.SetDataObject(dObj);
		});

		var setDataObject = ctx.JSInterop.VerifyInvoke("clipboard.setDataObject");
		var clipboardArgs = setDataObject.Arguments[1] as Dictionary<string, object>;
		var htmlText = clipboardArgs[MediaTypeNames.Text.Html];

		Assert.That(htmlText, Is.EqualTo(DemoHtml));
	}

	[Test]
	public async Task TestClipboardNotSupportType()
	{
		using var ctx = new WinzorTestContext();

		var form = default(Form);
		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			return form;
		});

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			var dObj = 55555;
			var notSupportedException = Assert.Throws<NotSupportedException>(() => Clipboard.SetDataObject(dObj));
			Assert.That(notSupportedException.Message, Is.EqualTo($"Clipboard doesn't support the {dObj.GetType()}!"));
		});
	}

	[Test]
	public async Task TestClipboardRTF2Html()
	{
		using var ctx = new WinzorTestContext();

		var form = default(Form);
		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			return form;
		});

		const string DemoRtf = @"{\rtf1\ansi\deff0 {\fonttbl {\f0 Arial;}}\f0\fs20 Hello, \b World!\b0}";
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			var dObj = new DataObject();
			dObj.SetData(DataFormats.Rtf, autoConvert: false, DemoRtf);
			Clipboard.SetDataObject(dObj);
		});

		var setDataObject = ctx.JSInterop.VerifyInvoke("clipboard.setDataObject");
		var clipboardArgs = setDataObject.Arguments[1] as Dictionary<string, object>;
		var htmlText = clipboardArgs[MediaTypeNames.Text.Html];

		var expectedHtml = @"<p><span style=""font-family: Arial, sans-serif; font-size: 10pt;"">Hello, </span><strong><span style=""font-family: Arial, sans-serif; font-size: 10pt;"">World!</span></strong></p>";
		Assert.That(htmlText, Is.EqualTo(expectedHtml));
	}

	[Test]
	public async Task TestEmptyDataObjectPassedToSetDataObjectDoesNotThrowException()
	{
		using var ctx = new WinzorTestContext();

		var form = default(Form);
		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			return form;
		});

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			var dObj = new DataObject();

			try
			{
				Clipboard.SetDataObject(dObj);
			}
			catch
			{
				Assert.Fail();
			}
		});
	}

	[Test, WithPlaywrightPage]
	public async Task TestClipboardUnicodeTextNoClientSideJavaScriptException()
	{
		await using var ctx = new InMemoryTestServerContext();
		var form = default(Form);
		var page = await ctx.LoadFormAsync(() => form = new Form());

		_ = await page.EvaluateAsync(@"() => {
			navigator.clipboard.write = async (items) => {
				return Promise.resolve();
			};
		}");

		const string DemoText = "Hello, World!";
		var clipboardTask = form.InvokeWinzorDispatcherAsync(() =>
		{
			var dObj = new DataObject();
			dObj.SetData(DataFormats.UnicodeText, autoConvert: false, DemoText);
			Clipboard.SetDataObject(dObj);
		});
		// If Clipboard.SetDataObject raises a JS exception, this will be
		// considered an unhandled exception and will trigger the excepton
		// reporting dialog, which blocks this task from completing.
		var completedWithNoExceptionReporter = await clipboardTask.WithTimeout(TimeSpan.FromSeconds(5));
		Assert.That(completedWithNoExceptionReporter, Is.True);
	}

	[Test, WithPlaywrightPage]
	[TestCase("SmallSizeSendByString", 1024)]
	[TestCase("LargeSizeSendByStream", 1024 * 8 + 1024)]
	[TestCase("GIFHugeSize", 1024 * 1024 * 20)]
	public async Task TestClipboardContentSize(string name, int htmlContentSize)
	{
		const string ClipboardJS = "/_content/WinzorFramework/js/module/clipboard.js";

		await using var ctx = new InMemoryTestServerContext();
		var form = default(Form);
		_ = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			return form;
		});

		Dictionary<string, string> clipboardItemsToSet = new()
		{
			[MediaTypeNames.Text.Plain] = "WI0000000679 GNA Header",
			[MediaTypeNames.Text.Html] = new string('A', htmlContentSize),
		};

		// Inject the mock clipboard script
		await form.CargoWiseClientServices.JSRuntime.InvokeVoidAsync("eval", @"
			window.mockClipboardData = [];
			navigator.clipboard.write = async (items) => {
				window.mockClipboardData = items;
			};
			navigator.clipboard.read = async () => {
				return window.mockClipboardData;
			};
		");

		// Perform clipboard operations directly
		var dotNetReference = DotNetObjectReference.Create(form.CargoWiseClientServices);
		var module = await form.CargoWiseClientServices.JSRuntime.InvokeAsync<IJSObjectReference>("import", ClipboardJS);
		await module.InvokeVoidAsync("clipboard.setDataObject", dotNetReference, clipboardItemsToSet, false);
		await Task.Delay(500);
		await Clipboard.FetchClipboardDataAsync(form.CargoWiseClientServices.JSRuntime);

		var clipboardData = Clipboard.GetDataObjectDict();
		clipboardData[MediaTypeNames.Text.Html] = WebUtility.HtmlDecode(clipboardData[MediaTypeNames.Text.Html]);

		Dictionary<string, string> expectedClipboardData = new()
		{
			[MediaTypeNames.Text.Plain] = clipboardItemsToSet[MediaTypeNames.Text.Plain],
			[MediaTypeNames.Text.Html] = clipboardItemsToSet[MediaTypeNames.Text.Html],
		};
		Assert.That(clipboardData, Is.EqualTo(expectedClipboardData));
	}

	[Test]
	public async Task TestGetDataObjectOnServer()
	{
		using var ctx = new WinzorTestContext();

		var form = default(Form);
		_ = await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			return form;
		});
		var dObj = new DataObject("Text", "ABC");
		DataObject serverData = null;
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			Clipboard.SetDataObject(dObj);
			serverData = (DataObject)Clipboard.GetServerDataObject();
		});
		Assert.That(serverData, Is.EqualTo(dObj));
	}

	[Test, WithPlaywrightPage]
	public async Task TestClipboardCallNavigatorClipboardWithUnicodeTextAndHtml()
	{
		await using var ctx = new InMemoryTestServerContext();
		var form = default(Form);
		_ = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			return form;
		});

		Dictionary<string, string> clipboardItemsToSet = new()
		{
			[MediaTypeNames.Text.Plain] = "WI0000000679 GNA Header",
			[MediaTypeNames.Text.Html] = new string('A', 1024),
		};

		await form.CargoWiseClientServices.JSRuntime.InvokeVoidAsync("eval", @"
			window.mockClipboardData = [];
			navigator.clipboard.write = async (items) => {
				window.mockClipboardData = items;
			};
			navigator.clipboard.read = async () => {
				return window.mockClipboardData;
			};
		");

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			var dObj = new DataObject();
			dObj.SetData(DataFormats.UnicodeText, autoConvert: false, clipboardItemsToSet[MediaTypeNames.Text.Plain]);
			dObj.SetData(DataFormats.Html, autoConvert: false, clipboardItemsToSet[MediaTypeNames.Text.Html]);
			Clipboard.SetDataObject(dObj);
		});

		await Clipboard.FetchClipboardDataAsync(form.CargoWiseClientServices.JSRuntime);
		var clipboardData = Clipboard.GetDataObjectDict();

		Assert.That(clipboardData, Is.EqualTo(clipboardItemsToSet));
	}

	[Test, WithPlaywrightPage]
	public async Task TestClipboardCallNavigatorClipboardWithRtf()
	{
		await using var ctx = new InMemoryTestServerContext();
		var form = default(Form);
		_ = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			return form;
		});

		const string DemoRtf = @"{\rtf1\ansi\deff0 {\fonttbl {\f0 Arial;}}\f0\fs20 Hello, \b World!\b0}";
		const string ExpectedHtml = @"<p><span style=""font-family: Arial, sans-serif; font-size: 10pt;"">Hello, </span><strong><span style=""font-family: Arial, sans-serif; font-size: 10pt;"">World!</span></strong></p>";

		Dictionary<string, string> clipboardItemsToSet = new()
		{
			["Rtf"] = DemoRtf
		};

		await form.CargoWiseClientServices.JSRuntime.InvokeVoidAsync("eval", @"
            window.mockClipboardData = [];
            navigator.clipboard.write = async (items) => {
                window.mockClipboardData = items;
            };
            navigator.clipboard.read = async () => {
                return window.mockClipboardData;
            };
        ");

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			var dObj = new DataObject();
			dObj.SetData(DataFormats.Rtf, autoConvert: false, DemoRtf);
			Clipboard.SetDataObject(dObj);
		});

		await Clipboard.FetchClipboardDataAsync(form.CargoWiseClientServices.JSRuntime);
		var clipboardData = Clipboard.GetDataObjectDict();
		clipboardData[MediaTypeNames.Text.Html] = WebUtility.HtmlDecode(clipboardData[MediaTypeNames.Text.Html]);

		Assert.That(clipboardData[MediaTypeNames.Text.Html], Is.EqualTo(ExpectedHtml));
	}
}
