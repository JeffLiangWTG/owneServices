#nullable enable
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Bunit;
using CargoWise.Blazor.Client.Integration.Menus;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WinzorFramework.Extensions;
using WinzorFramework.JSInterop;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using WTG.RtfConverter;
using WTG.RtfConverter.Dom;
using WTG.RtfConverter.Html.Extensions;
using Color = System.Drawing.Color;
using DomColor = WTG.RtfConverter.Dom.Color;

namespace System.Windows.Forms;

using static WinzorFramework.JSInterop.RichTextBoxJSInterop;

class RichTextBoxTest
{
	static IEnumerable<TestCaseData> SampleTextList
	{
		get
		{
			var buildTestCaseData = (int i, string name) => new TestCaseData(new string('?', i * 1024)) { TestName = "{m}_" + name };
			yield return buildTestCaseData(7, "smallerThanLimit");
			yield return buildTestCaseData(9, "largerThanLimit");
			yield return buildTestCaseData(97, "largerThanMaxMessageSize");
			yield return buildTestCaseData(513, "largerThanMaxStreamAllowedSize");
		}
	}

	static readonly IEnumerable<string> AnchorTargetOrRelAttributeTestCases = new string[] { string.Empty, "target='_blank'", "rel='noopener'", "target='_blank' rel='noopener'", "target='parent' rel='noreferer'" };

	[Test]
	public async Task MouseDownEvent()
	{
		await ControlAssert.ImplementsEventAsync<RichTextBox, MouseEventHandler>(nameof(GroupBox.MouseDown),
			a => new MouseEventHandler((o, e) => a()), ".richtextbox", e => e.MouseDown());
	}

	[Test]
	public async Task PreloadRichTextBoxJSInterop()
	{
		using var ctx = new WinzorTestContext();
		var services = ctx.MockCargoWiseClientServices;
		var interop = new Mock<IRichTextBoxJSInterop>();
		interop.Setup(i => i.PreloadInterop());
		ctx.Services.AddScoped(_ => interop.Object);

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var textBox = new RichTextBox();
			form.Controls.Add(textBox);
			return form;
		});

		interop.Verify(e => e.PreloadInterop(), Times.Once());
	}

	[Test]
	public async Task RichTextBoxHasCorrectClass()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var textBox = new RichTextBox();
			return textBox;
		});

		var richTextBox = rendered.Find(".richtextbox");

		Assert.That(richTextBox, Is.Not.Null);
	}

	[Test]
	public async Task RichTextBoxRenderChildControls()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var textBox = new RichTextBox();
			textBox.Controls.Add(new Label());
			textBox.Controls.Add(new ListBox());
			return textBox;
		});

		Assert.That(rendered.Find(".richtextbox"), Is.Not.Null);
		Assert.That(rendered.Find(".label"), Is.Not.Null);
		Assert.That(rendered.Find(".listbox"), Is.Not.Null);
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxInvokesFocusEvents()
	{
		await using var ctx = new InMemoryTestServerContext();

		var gotFocusCalled = new TaskCompletionSource<bool>();
		var lostFocusCalled = new TaskCompletionSource<bool>();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(new Label() { Text = "Label", Top = 500 });
			var richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Html = new RtfToHtmlConverter().Convert(@"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang3081{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif;}}{\*\generator Riched20 10.0.19041}\viewkind4\uc1 \pard\f0\fs20 Rich Text\par}"),
			};
			form.Controls.Add(richTextBox);
			richTextBox.GotFocus += (sender, eventArgs) => gotFocusCalled.SetResult(true);
			richTextBox.LostFocus += (sender, eventArgs) => lostFocusCalled.SetResult(true);
			return form;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		await client.FocusEditorAsync();
		Assert.That(await gotFocusCalled.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.EqualTo(true));
		await page.ClickAsync(".label");
		Assert.That(await lostFocusCalled.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.EqualTo(true));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxNativeSelectionWorks()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Text = "Test Content",
			};
			form.Controls.Add(richTextBox);
			return form;
		});

		var contentLocator = page.Locator(".richtextbox__editoranchor");
		await contentLocator.WaitForAsync();

		var client = await RichTextBoxClient.GetClientAsync();
		Assert.That(client.GetActiveTextAsync, Is.EqualTo("Test Content"));

		await client.EvaluateAsync($@"client => {{
				client.setSelection({{ start: 0, end: 3}});
			}}");

		var selectedText = await client.EvaluateAsync<string>($@"client => {{
				return client.getDOMSelection().toString();
			}}");

		Assert.That(selectedText, Is.EqualTo("Tes"));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxSetSelectionContent()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Html = WrapInP("Line 1") + WrapInP("Line 2"),
				IsToolBarVisible = true,
			};
			form.Controls.Add(richTextBox);
			return form;
		});

		var contentLocator = page.Locator(".richtextbox__editoranchor");
		await contentLocator.WaitForAsync();

		var client = await RichTextBoxClient.GetClientAsync();
		Assert.That(client.GetActiveTextAsync, Is.EqualTo("Line 1\nLine 2"));

		Assert.That(async () => (await client.GetActiveHtmlAsync()).Replace("\n", ""), Is.EqualTo(WrapInP("Line 1") + WrapInP("Line 2")).After(1000, 100));

		await client.EvaluateAsync($@"client => {{
				client.setSelection({{ start: 0, end: 6}});
				client.setSelectionContent('<strong>New content</strong>');
			}}");

		// For some reason document.execCommand('insertHTML') wants to add an invisible <br> after the inserted content
		Assert.That(async () => (await client.GetActiveHtmlAsync()).Replace("\n", "").Replace("<br>", ""), Is.EqualTo($"<p><strong>New content</strong></p>{WrapInP("Line 2")}").After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task NestedUnorderedListsHaveDiscStyle([Values] bool isReadOnly)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox
		{
			IsToolBarVisible = true,
			ReadOnly = isReadOnly,
			Html = @"
                <ul>
					<li>Item 1</li>
					<ul>
						<li>Nested Item 1</li>
						<ul>
							<li>Deep Nested Item 1</li>
							<li>Deep Nested Item 2</li>
						</ul>
						<li>Nested Item 2</li>
					</ul>
					<li>Item 2</li>
				</ul>"
		});

		var contentLocator = isReadOnly
			? page.Locator(".richtextbox__data")
			: page.Locator(".richtextbox__editoranchor");
		await contentLocator.WaitForAsync();

		var listLocator = contentLocator.Locator("ul > li");

		await AssertAllListItemsHaveDiscStyle(listLocator);
	}

	[Test, WithPlaywrightPage]
	public async Task NestedUnorderedListsInsideOrderedListsHaveDiscStyle([Values] bool isReadOnly)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox
		{
			IsToolBarVisible = true,
			ReadOnly = isReadOnly,
			Html = @"
                <ol>
					<li>one thing
						<ul>
							<li>unordered</li>
							<li>unordered 2
								<ol>
									<li>another ordered</li>
									<li>ordered 2
										<ul>
											<li>unordered again</li>
										</ul>
									</li>
								</ol>
							</li>
						</ul>
					</li>
					<li>another thing</li>
				</ol>"
		});

		var contentLocator = isReadOnly
			? page.Locator(".richtextbox__data")
			: page.Locator(".richtextbox__editoranchor");
		await contentLocator.WaitForAsync();

		var listLocator = contentLocator.Locator("ul > li");

		await AssertAllListItemsHaveDiscStyle(listLocator);
	}

	async Task AssertAllListItemsHaveDiscStyle(ILocator listLocator)
	{
		var count = await listLocator.CountAsync();
		for (int i = 0; i < count; i++)
		{
			Assert.That(await listLocator.Nth(i).GetComputedStyleAsync("list-style-type"), Is.EqualTo("disc"));
		}
	}

	[TestCase(0, 3, true, false, TestName = "{m}_Length3")]
	[TestCase(0, 0, true, false, TestName = "{m}_ZeroLength")]
	[TestCase(0, 0, true, true, TestName = "{m}_BecomesZeroLength")]
	[TestCase(0, 0, false, false, TestName = "{m}_NoSelectionInit")]
	[WithPlaywrightPage]
	public async Task RichTextBoxRestoresSelection(int selectionStart, int selectionLength, bool initialiseSelection, bool cancelSelection)
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;
		TextBox? textBox = null!;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Text = "Test Content",
				IsToolBarVisible = true
			};
			textBox = new TextBox();
			form.Controls.Add(richTextBox);
			form.Controls.Add(textBox);
			return form;
		});

		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

		var contentLocator = page.Locator(".richtextbox__editoranchor");
		await contentLocator.WaitForAsync();

		var client = await RichTextBoxClient.GetClientAsync();
		Assert.That(client.GetActiveTextAsync, Is.EqualTo("Test Content"));
		var initialMarkup = await client.GetActiveHtmlAsync();

		if (initialiseSelection)
		{
			await client.EvaluateAsync($@"client => {{
				client.setSelection({{ start: {selectionStart}, end: {selectionStart + selectionLength}}});
			}}");
		}

		if (cancelSelection)
		{
			await client.EvaluateAsync($@"client => {{
				client.setSelection({{ start: 0, end: 0}});
			}}");
		}

		var textboxLocator = page.Locator("input.textbox");
		await textboxLocator.ClickAsync();

		var toolbarLocator = page.Locator(".richtextbox__toolbar");
		await toolbarLocator.WaitForAsync();

		ILocator? toolbarButton = null;
		Assert.That(() => toolbarButton = toolbarLocator.GetByLabel("Bold"), Is.Not.Null.After(3000, 100));

		if (toolbarButton is not null)
		{
			await toolbarButton.ClickAsync();
		}

		Assert.That(async () => (await client.GetActiveSelectionAsync()).start, Is.EqualTo(selectionStart).After(3000, 100), "Selection start");
		Assert.That(async () => (await client.GetActiveSelectionAsync()).end, Is.EqualTo(selectionStart + selectionLength), "Selection length");

		if (selectionLength > 0)
		{
			Assert.That(initialMarkup, Is.Not.EqualTo(await client.GetActiveHtmlAsync()));

			var selectedText = await client.EvaluateAsync<string>($@"client => {{
				return client.getDOMSelection().toString();
			}}");
			Assert.That(selectedText, Is.EqualTo("Tes"));
		}
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxScrollsNewContentWhenReadonly()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 500, Height = 500 };
			richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Font = new Font(FontFamily.GenericSansSerif, 12f),
				ReadOnly = true,
				Html = "<p>Rich Text</p>",
			};
			form.Controls.Add(richTextBox);
			return form;
		});

		var contentLocator = page.Locator(".richtextbox__data");
		await contentLocator.WaitForAsync();

		Assert.That(async () => await contentLocator.EvaluateAsync<int>("e => e.scrollTop"), Is.EqualTo(0).After(3000, 100));
		Assert.That(async () => await contentLocator.EvaluateAsync<int>("e => e.scrollHeight"), Is.EqualTo(498).After(3000, 100));

		await richTextBox.InvokeWinzorDispatcherAsync(() =>
		{
			var loremIpsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin tempus rhoncus ornare. Donec vestibulum, tortor quis tristique rhoncus, sapien lorem sodales sapien, nec faucibus velit est vitae enim. Nunc facilisis semper justo fringilla bibendum. Sed orci purus, posuere et urna et, viverra cursus sapien. Duis orci nunc, sodales ultricies interdum in, molestie quis massa. Aenean non iaculis sem. Integer ultrices urna nec porta rutrum.\r\n\r\n";
			richTextBox.AppendText(loremIpsum);
			richTextBox.AppendText(loremIpsum);
			richTextBox.AppendText(loremIpsum);
			richTextBox.AppendText(loremIpsum);
			richTextBox.ScrollToCaret();
		});

		var scrollTop = 0;
		Assert.That(async () => scrollTop = await contentLocator.EvaluateAsync<int>("e => e.scrollTop"), Is.GreaterThan(0).After(1000, 100));
		Assert.That(await contentLocator.EvaluateAsync<int>("e => e.scrollHeight"), Is.EqualTo(scrollTop + 498));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxAppendsLinkDestinationsInPlaintextOnlyMode()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox() { IsToolBarVisible = false });

		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();

		var editorAnchor = page.Locator(".richtextbox__editoranchor");
		await editorAnchor.WaitForAsync();

		await page.MockClipboardRead(new[] {
			new JSClipboardData("text/html", "<p>Some text with a link: <a href='http://example.com'>example</a></p>")
		});

		await editorBody.PressAsync("Control+KeyV");

		var contentEditable = await editorAnchor.GetAttributeAsync("contenteditable");
		Assert.That(contentEditable, Is.EqualTo("true"));
		var expectedContent = "<p>Some text with a link: example &lt;<a href=\"http://example.com\" target=\"_blank\" rel=\"noopener\">http://example.com</a>&gt;</p>";

		Assert.That(async () => await editorAnchor.InnerHTMLAsync(), Is.EqualTo(expectedContent).After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task RTBShouldLoadFontListSuccessfully()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox() { IsToolBarVisible = true });

		var fontMenu = page.GetByLabel("Fonts", new() { Exact = true });
		Assert.That(fontMenu, Is.Not.Null);

		await fontMenu.ClickAsync();

		Assert.That(page.GetByTitle("Fonts", new() { Exact = true }).Locator("option").CountAsync, Is.GreaterThan(1));
	}

	[Test]
	public async Task RichTextBoxHasStylesSet([Values] bool multiline)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var richTextBox = new RichTextBox();
			richTextBox.Multiline = multiline;
			richTextBox.Width = 400;
			richTextBox.Height = 300;
			return richTextBox;
		});

		var richTextBox = rendered.Find(".richtextbox");
		var expectedHeight = multiline ? 300 : 20;
		Assert.That(richTextBox.GetAttribute("style"),
			Is.EqualTo(
				$"position:absolute;width:400px;height:{expectedHeight}px;top:0px;left:0px;background-color:var(--color-window);"));
	}

	[TestCase("Control+KeyB", "<span style=\"font-weight: bold;\">", "</span>", TestName = "{m}_Bold")]
	[TestCase("Control+KeyI", "<span style=\"font-style: italic;\">", "</span>", TestName = "{m}_Italic")]
	[TestCase("Control+KeyU", "<span style=\"text-decoration-line: underline;\">", "</span>", TestName = "{m}_Underline")]
	[TestCase("Control+KeyT", "<span style=\"text-decoration-line: line-through;\">", "</span>", TestName = "{m}_Strikethrough")]
	[WithPlaywrightPage]
	public async Task RichTextBoxKeyboardShortcuts(string keyboardCombo, string openingTag, string closingTag)
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 800, Height = 500 };
			richTextBox = new RichTextBox()
			{
				Width = 800,
				Height = 500,
				Text = "Test Content",
				IsToolBarVisible = true
			};
			form.Controls.Add(richTextBox);
			return form;
		});
		var contentLocator = page.Locator(".richtextbox__editoranchor");
		await contentLocator.WaitForAsync();

		var client = await RichTextBoxClient.GetClientAsync();
		Assert.That(client.GetActiveTextAsync, Is.EqualTo("Test Content"));
		await client.EvaluateAsync($@"client => {{
			client.setSelection({{ start: 0, end: 3}});
		}}");

		await contentLocator.PressAsync(keyboardCombo);
		Assert.That(client.GetEditorHtmlAsync, Is.EqualTo($"<p>{openingTag}Tes{closingTag}t Content</p>"));

		await contentLocator.PressAsync(keyboardCombo);
		Assert.That(client.GetEditorHtmlAsync, Is.EqualTo($"<p>Test Content</p>"));
	}

	public static object[] ToolbarItemButtons =
	{
		new object[] { "Bold" },
		new object[] { "Italic" },
		new object[] { "Underline" },
		new object[] { "Strikethrough" },
		new object[] { "Bullet list" },
		new object[] { "Numbered list" },
		new object[] { "Increase indent" },
		new object[] { "Decrease indent" },
	};

	[TestCaseSource(nameof(ToolbarItemButtons))]
	[WithPlaywrightPage]
	public async Task RichTextBox_Toolbar_Buttons_Hover_Color(string buttonName)
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 800, Height = 500 };
			richTextBox = new RichTextBox()
			{
				Width = 800,
				Height = 500,
				Text = "Test Content",
				IsToolBarVisible = true,
			};
			form.Controls.Add(richTextBox);
			return form;
		});

		var toolbarLocator = page.Locator(".richtextbox__toolbar");
		await toolbarLocator.WaitForAsync();

		ILocator? toolbarButton = null;
		Assert.That(() => toolbarButton = toolbarLocator.GetByLabel(buttonName), Is.Not.Null.After(5000, 100));

		if (toolbarButton is not null)
		{
			await toolbarButton.HoverAsync();

			Assert.That(async () => await toolbarButton.EvaluateAsync<bool>("e => e.matches(':hover')"), Is.True.After(5000, 100), $"{buttonName} button is not hovered.");

			var expectedBackgroundColor = "rgb(219, 235, 248)";
			var expectedBorderColor = "rgb(195, 225, 249)";

			Assert.That(async () => await toolbarButton.GetComputedStyleAsync("background-color"), Is.EqualTo(expectedBackgroundColor).After(5000, 100), "Incorrect background-color:");
			Assert.That(async () => await toolbarButton.GetComputedStyleAsync("border-color"), Is.EqualTo(expectedBorderColor).After(5000, 100), "Incorrect border-color:");
		}
	}

	[TestCaseSource(nameof(ToolbarItemButtons))]
	[WithPlaywrightPage]
	public async Task RichTextBoxToolbarDisabledButtonsStyles(string buttonName)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox
		{
			Width = 800,
			Height = 500,
			Text = "Test Content",
			IsToolBarVisible = true,
			ReadOnly = true,
		});

		var toolbarLocator = page.Locator(".richtextbox__toolbar");
		await toolbarLocator.WaitForAsync();

		var toolbarButton = toolbarLocator.GetByLabel(buttonName);
		var expectedBackgroundColor = "rgb(219, 235, 248)";
		var expectedBorderColor = "rgb(195, 225, 249)";

		Assert.That(await toolbarButton.GetComputedStyleAsync("background-color"), Is.Not.EqualTo(expectedBackgroundColor));
		Assert.That(await toolbarButton.GetComputedStyleAsync("border-color"), Is.Not.EqualTo(expectedBorderColor));
		Assert.That(await toolbarButton.GetComputedStyleAsync("filter"), Is.EqualTo("grayscale(1)"));
	}

	static IEnumerable<TestCaseData> ListToolbarButtonCorrectlyCreatesListData()
	{
		yield return new TestCaseData("Bullet list", WrapInP("Item 1"), $"<ul><li>{WrapInSpan("Item 1")}</li></ul>", 0, 6) { TestName = "{m}_BulletList_SingleLine" };
		yield return new TestCaseData("Bullet list", $"<ul><li>{WrapInSpan("Item 1")}</li></ul>", WrapInP("Item 1"), 0, 6) { TestName = "{m}_BulletList_SingleLine_ToggleOff" };
		yield return new TestCaseData("Bullet list", $"<ol><li>{WrapInSpan("Item 1")}</li></ol>", $"<ul><li>{WrapInSpan("Item 1")}</li></ul>", 0, 6) { TestName = "{m}_BulletList_SingleLine_ChangeMode" };
		yield return new TestCaseData("Bullet list", $"<p style=\"padding-left: 40px;\">{WrapInSpan("Item 1")}</p>", $"<ul><ul><li>{WrapInSpan("Item 1")}</li></ul></ul>", 0, 6) { TestName = "{m}_BulletList_SingleLine_Nested" };
		yield return new TestCaseData("Bullet list", $"<ul><ul><li>{WrapInSpan("Item 1")}</li></ul></ul>", $"<p style=\"padding-left: 40px;\">{WrapInSpan("Item 1")}</p>", 0, 6) { TestName = "{m}_BulletList_SingleLine_Nested_ToggleOff" };
		yield return new TestCaseData("Bullet list", $"<ol><ol><li>{WrapInSpan("Item 1")}</li></ol></ol>", $"<ul><ul><li>{WrapInSpan("Item 1")}</li></ul></ul>", 0, 6) { TestName = "{m}_BulletList_SingleLine_Nested_ChangeMode" };
		yield return new TestCaseData("Bullet list", WrapInP("Item 1") + WrapInP("Item 2") + WrapInP("Item 3"), $"<ul><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ul>", 0, 18) { TestName = "{m}_BulletList_MultipleLines" };
		yield return new TestCaseData("Bullet list", $"<ul><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ul>", WrapInP("Item 1") + WrapInP("Item 2") + WrapInP("Item 3"), 0, 18) { TestName = "{m}_BulletList_MultipleLines_ToggleOff" };
		yield return new TestCaseData("Bullet list", $"<ul><li>{WrapInSpan("Item 1")}</li></ul>{WrapInP("Item 2")}<ul><li>{WrapInSpan("Item 3")}</li></ul>", $"<ul><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ul>", 7, 7) { TestName = "{m}_BulletList_MultipleLines_Partial" };
		yield return new TestCaseData("Bullet list", $"<ul><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ul>", $"<ul><li>{WrapInSpan("Item 1")}</li></ul>{WrapInP("Item 2")}<ul><li>{WrapInSpan("Item 3")}</li></ul>", 7, 7) { TestName = "{m}_BulletList_MultipleLines_Partial_ToggleOff" };
		yield return new TestCaseData("Bullet list", $"<ol><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ol>", $"<ul><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ul>", 0, 18) { TestName = "{m}_BulletList_MultipleLines_ChangeMode" };
		yield return new TestCaseData("Bullet list", $"{WrapInP("Item 1")}<ul><li>{WrapInSpan("Item 2")}</li></ul><ol><li>{WrapInSpan("Item 3")}</li></ol>", $"<ul><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ul>", 0, 18) { TestName = "{m}_BulletList_MultipleLines_Mixed_ChangeMode" };
		yield return new TestCaseData("Bullet list", $"<ol><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ol>", $"<ol><li>{WrapInSpan("Item 1")}</li></ol><ul><li>{WrapInSpan("Item 2")}</li></ul><ol><li>{WrapInSpan("Item 3")}</li></ol>", 7, 7) { TestName = "{m}_BulletList_MultiLines_Partial_ChangeMode" };
		yield return new TestCaseData("Bullet list", $"<p style=\"padding-left: 40px;\">{WrapInSpan("Item 1")}</p><p style=\"padding-left: 40px;\">{WrapInSpan("Item 2")}</p><p style=\"padding-left: 40px;\">{WrapInSpan("Item 3")}</p>", $"<ul><ul><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ul></ul>", 0, 18) { TestName = "{m}_BulletList_MultiLines_Nested" };
		yield return new TestCaseData("Bullet list", $"<ul><ul><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ul></ul>", $"<p style=\"padding-left: 40px;\">{WrapInSpan("Item 1")}</p><p style=\"padding-left: 40px;\">{WrapInSpan("Item 2")}</p><p style=\"padding-left: 40px;\">{WrapInSpan("Item 3")}</p>", 0, 18) { TestName = "{m}_BulletList_MultiLines_Nested_ToggleOff" };
		yield return new TestCaseData("Bullet list", $"<ul><li>{WrapInSpan("Item 1")}</li></ul><p style=\"padding-left: 40px;\">{WrapInSpan("Item 2")}</p><ul><li>{WrapInSpan("Item 3")}</li></ul>", $"<ul><li>{WrapInSpan("Item 1")}<ul><li>{WrapInSpan("Item 2")}</li></ul></li><li>{WrapInSpan("Item 3")}</li></ul>", 7, 7) { TestName = "{m}_BulletList_MultipleLines_Nested_Partial_1" };
		yield return new TestCaseData("Bullet list", $"<ul><li>{WrapInSpan("Item 1")}<ul><li>{WrapInSpan("Item 2")}</li></ul></li><li>{WrapInSpan("Item 3")}</li></ul>", $"<ul><li>{WrapInSpan("Item 1")}</li></ul><p style=\"padding-left: 40px;\">{WrapInSpan("Item 2")}</p><ul><li>{WrapInSpan("Item 3")}</li></ul>", 7, 7) { TestName = "{m}_BulletList_MultipleLines_Nested_Partial_1_ToggleOff" };
		yield return new TestCaseData("Bullet list", $"<ul><ul><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}<ul><ul><ul><li>{WrapInSpan("Item 3")}</li></ul></ul></ul></li></ul></ul><p style=\"padding-left: 160px;\">{WrapInSpan("Item 4")}</p><ul><ul><ul><ul><ul><li>{WrapInSpan("Item 5")}</li></ul></ul><li>{WrapInSpan("Item 6")}</li></ul><li>{WrapInSpan("Item 7")}</li></ul></ul>", $"<ul><ul><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><ul><ul><ul><li>{WrapInSpan("Item 3")}</li><li>{WrapInSpan("Item 4")}</li><li>{WrapInSpan("Item 5")}</li></ul></ul><li>{WrapInSpan("Item 6")}</li></ul><li>{WrapInSpan("Item 7")}</li></ul></ul>", 21, 21) { TestName = "{m}_BulletList_MultipleLines_Nested_Partial_2" };
		yield return new TestCaseData("Bullet list", $"<ul><ul><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><ul><ul><ul><li>{WrapInSpan("Item 3")}</li><li>{WrapInSpan("Item 4")}</li><li>{WrapInSpan("Item 5")}</li></ul></ul><li>{WrapInSpan("Item 6")}</li></ul><li>{WrapInSpan("Item 7")}</li></ul></ul>", $"<ul><ul><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}<ul><ul><ul><li>{WrapInSpan("Item 3")}</li></ul></ul></ul></li></ul></ul><p style=\"padding-left: 160px;\">{WrapInSpan("Item 4")}</p><ul><ul><ul><ul><ul><li>{WrapInSpan("Item 5")}</li></ul></ul><li>{WrapInSpan("Item 6")}</li></ul><li>{WrapInSpan("Item 7")}</li></ul></ul>", 21, 21) { TestName = "{m}_BulletList_MultipleLines_Nested_Partial_2_ToggleOff" };
		yield return new TestCaseData("Bullet list", $"<ol><ol><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ol></ol>", $"<ul><ul><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ul></ul>", 0, 18) { TestName = "{m}_BulletList_MultipleLines_Nested_ChangeMode_1" };
		yield return new TestCaseData("Bullet list", $"<ol><li>{WrapInSpan("Item 1")}</li><ol><li>{WrapInSpan("Item 2")}</li></ol><li>{WrapInSpan("Item 3")}</li></ol>", $"<ul><li>{WrapInSpan("Item 1")}<ul><li>{WrapInSpan("Item 2")}</li></ul></li><li>{WrapInSpan("Item 3")}</li></ul>", 0, 18) { TestName = "{m}_BulletList_MultipleLines_Nested_ChangeMode_2" };
		yield return new TestCaseData("Bullet list", $"<ol><li>{WrapInSpan("Item 1")}</li><ol><li>{WrapInSpan("Item 2")}</li></ol><li>{WrapInSpan("Item 3")}</li></ol>", $"<ol><li>{WrapInSpan("Item 1")}<ul><li>{WrapInSpan("Item 2")}</li></ul></li><li>{WrapInSpan("Item 3")}</li></ol>", 7, 7) { TestName = "{m}_BulletList_MultipleLines_Nested_ChangeMode_Partial" };
		yield return new TestCaseData("Numbered list", WrapInP("Item 1"), $"<ol><li>{WrapInSpan("Item 1")}</li></ol>", 0, 6) { TestName = "{m}_NumberedList_SingleLine" };
		yield return new TestCaseData("Numbered list", $"<ol><li>{WrapInSpan("Item 1")}</li></ol>", WrapInP("Item 1"), 0, 6) { TestName = "{m}_NumberedList_SingleLine_ToggleOff" };
		yield return new TestCaseData("Numbered list", $"<ul><li>{WrapInSpan("Item 1")}</li></ul>", $"<ol><li>{WrapInSpan("Item 1")}</li></ol>", 0, 6) { TestName = "{m}_NumberedList_SingleLine_ChangeMode" };
		yield return new TestCaseData("Numbered list", $"<p style=\"padding-left: 40px;\">{WrapInSpan("Item 1")}</p>", $"<ol><ol><li>{WrapInSpan("Item 1")}</li></ol></ol>", 0, 6) { TestName = "{m}_NumberedList_SingleLine_Nested" };
		yield return new TestCaseData("Numbered list", $"<ol><ol><li>{WrapInSpan("Item 1")}</li></ol></ol>", $"<p style=\"padding-left: 40px;\">{WrapInSpan("Item 1")}</p>", 0, 6) { TestName = "{m}_NumberedList_SingleLine_Nested_ToggleOff" };
		yield return new TestCaseData("Numbered list", $"<ul><ul><li>{WrapInSpan("Item 1")}</li></ul></ul>", $"<ol><ol><li>{WrapInSpan("Item 1")}</li></ol></ol>", 0, 6) { TestName = "{m}_NumberedList_SingleLine_Nested_ChangeMode" };
		yield return new TestCaseData("Numbered list", WrapInP("Item 1") + WrapInP("Item 2") + WrapInP("Item 3"), $"<ol><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ol>", 0, 18) { TestName = "{m}_NumberedList_MultipleLines" };
		yield return new TestCaseData("Numbered list", $"<ol><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ol>", WrapInP("Item 1") + WrapInP("Item 2") + WrapInP("Item 3"), 0, 18) { TestName = "{m}_NumberedList_MultipleLines_ToggleOff" };
		yield return new TestCaseData("Numbered list", $"<ol><li>{WrapInSpan("Item 1")}</li></ol>{WrapInP("Item 2")}<ol><li>{WrapInSpan("Item 3")}</li></ol>", $"<ol><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ol>", 7, 7) { TestName = "{m}_NumberedList_MultipleLines_Partial" };
		yield return new TestCaseData("Numbered list", $"<ol><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ol>", $"<ol><li>{WrapInSpan("Item 1")}</li></ol>{WrapInP("Item 2")}<ol><li>{WrapInSpan("Item 3")}</li></ol>", 7, 7) { TestName = "{m}_NumberedList_MultipleLines_Partial_ToggleOff" };
		yield return new TestCaseData("Numbered list", $"<ul><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ul>", $"<ol><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ol>", 0, 18) { TestName = "{m}_NumberedList_MultipleLines_ChangeMode" };
		yield return new TestCaseData("Numbered list", $"{WrapInP("Item 1")}<ol><li>{WrapInSpan("Item 2")}</li></ol><ul><li>{WrapInSpan("Item 3")}</li></ul>", $"<ol><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ol>", 0, 18) { TestName = "{m}_NumberedList_MultipleLines_Mixed_ChangeMode" };
		yield return new TestCaseData("Numbered list", $"<ul><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ul>", $"<ul><li>{WrapInSpan("Item 1")}</li></ul><ol><li>{WrapInSpan("Item 2")}</li></ol><ul><li>{WrapInSpan("Item 3")}</li></ul>", 7, 7) { TestName = "{m}_NumberedList_MultiLines_Partial_ChangeMode" };
		yield return new TestCaseData("Numbered list", $"<p style=\"padding-left: 40px;\">{WrapInSpan("Item 1")}</p><p style=\"padding-left: 40px;\">{WrapInSpan("Item 2")}</p><p style=\"padding-left: 40px;\">{WrapInSpan("Item 3")}</p>", $"<ol><ol><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ol></ol>", 0, 18) { TestName = "{m}_NumberedList_MultiLines_Nested" };
		yield return new TestCaseData("Numbered list", $"<ol><ol><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ol></ol>", $"<p style=\"padding-left: 40px;\">{WrapInSpan("Item 1")}</p><p style=\"padding-left: 40px;\">{WrapInSpan("Item 2")}</p><p style=\"padding-left: 40px;\">{WrapInSpan("Item 3")}</p>", 0, 18) { TestName = "{m}_NumberedList_MultiLines_Nested_ToggleOff" };
		yield return new TestCaseData("Numbered list", $"<ol><li>{WrapInSpan("Item 1")}</li></ol><p style=\"padding-left: 40px;\">{WrapInSpan("Item 2")}</p><ol><li>{WrapInSpan("Item 3")}</li></ol>", $"<ol><li>{WrapInSpan("Item 1")}<ol><li>{WrapInSpan("Item 2")}</li></ol></li><li>{WrapInSpan("Item 3")}</li></ol>", 7, 7) { TestName = "{m}_NumberedList_MultipleLines_Nested_Partial_1" };
		yield return new TestCaseData("Numbered list", $"<ol><li>{WrapInSpan("Item 1")}<ol><li>{WrapInSpan("Item 2")}</li></ol></li><li>{WrapInSpan("Item 3")}</li></ol>", $"<ol><li>{WrapInSpan("Item 1")}</li></ol><p style=\"padding-left: 40px;\">{WrapInSpan("Item 2")}</p><ol><li>{WrapInSpan("Item 3")}</li></ol>", 7, 7) { TestName = "{m}_NumberedList_MultipleLines_Nested_Partial_1_ToggleOff" };
		yield return new TestCaseData("Numbered list", $"<ol><ol><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}<ol><ol><ol><li>{WrapInSpan("Item 3")}</li></ol></ol></ol></li></ol></ol><p style=\"padding-left: 160px;\">{WrapInSpan("Item 4")}</p><ol><ol><ol><ol><ol><li>{WrapInSpan("Item 5")}</li></ol></ol><li>{WrapInSpan("Item 6")}</li></ol><li>{WrapInSpan("Item 7")}</li></ol></ol>", $"<ol><ol><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><ol><ol><ol><li>{WrapInSpan("Item 3")}</li><li>{WrapInSpan("Item 4")}</li><li>{WrapInSpan("Item 5")}</li></ol></ol><li>{WrapInSpan("Item 6")}</li></ol><li>{WrapInSpan("Item 7")}</li></ol></ol>", 21, 21) { TestName = "{m}_NumberedList_MultipleLines_Nested_Partial_2" };
		yield return new TestCaseData("Numbered list", $"<ol><ol><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><ol><ol><ol><li>{WrapInSpan("Item 3")}</li><li>{WrapInSpan("Item 4")}</li><li>{WrapInSpan("Item 5")}</li></ol></ol><li>{WrapInSpan("Item 6")}</li></ol><li>{WrapInSpan("Item 7")}</li></ol></ol>", $"<ol><ol><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}<ol><ol><ol><li>{WrapInSpan("Item 3")}</li></ol></ol></ol></li></ol></ol><p style=\"padding-left: 160px;\">{WrapInSpan("Item 4")}</p><ol><ol><ol><ol><ol><li>{WrapInSpan("Item 5")}</li></ol></ol><li>{WrapInSpan("Item 6")}</li></ol><li>{WrapInSpan("Item 7")}</li></ol></ol>", 21, 21) { TestName = "{m}_NumberedList_MultipleLines_Nested_Partial_2_ToggleOff" };
		yield return new TestCaseData("Numbered list", $"<ul><ul><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ul></ul>", $"<ol><ol><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ol></ol>", 0, 18) { TestName = "{m}_NumberedList_MultipleLines_Nested_ChangeMode_1" };
		yield return new TestCaseData("Numbered list", $"<ul><li>{WrapInSpan("Item 1")}</li><ul><li>{WrapInSpan("Item 2")}</li></ul><li>{WrapInSpan("Item 3")}</li></ul>", $"<ol><li>{WrapInSpan("Item 1")}<ol><li>{WrapInSpan("Item 2")}</li></ol></li><li>{WrapInSpan("Item 3")}</li></ol>", 0, 18) { TestName = "{m}_NumberedList_MultipleLines_Nested_ChangeMode_2" };
		yield return new TestCaseData("Numbered list", $"<ul><li>{WrapInSpan("Item 1")}</li><ul><li>{WrapInSpan("Item 2")}</li></ul><li>{WrapInSpan("Item 3")}</li></ul>", $"<ul><li>{WrapInSpan("Item 1")}<ol><li>{WrapInSpan("Item 2")}</li></ol></li><li>{WrapInSpan("Item 3")}</li></ul>", 7, 7) { TestName = "{m}_NumberedList_MultipleLines_Nested_ChangeMode_Partial" };
	}

	[TestCaseSource(nameof(ListToolbarButtonCorrectlyCreatesListData))]
	[WithPlaywrightPage]
	public async Task ListToolbarButtonCorrectlyCreatesList(string buttonLabel, string input, string expected, int selectionStart, int selectionEnd)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox
		{
			Width = 300,
			Height = 500,
			Html = input,
			IsToolBarVisible = true,
		});
		var client = await RichTextBoxClient.GetClientAsync();
		await client.EvaluateAsync(@$"client => {{
			client.setSelection({{ start: {selectionStart}, end: {selectionEnd}}});
		}}");
		var toolbarLocator = page.Locator(".richtextbox__toolbar");
		await toolbarLocator.WaitForAsync();
		var toolbarButton = toolbarLocator.GetByLabel(buttonLabel);
		await toolbarButton.ClickAsync();
		var result = await client.GetEditorHtmlAsync();
		Assert.That(result, Is.EqualTo(expected));
	}

	static IEnumerable<TestCaseData> ListToolBarCorrectlyIndentsComplexData() {
		// these do not work yet...
		yield return new TestCaseData($"<ol><li>{WrapInSpan("Item 1")}</li><ol><li>{WrapInSpan("Item 2")}</li></ol><li>{WrapInSpan("Item 3")}</li><li>{WrapInSpan("Item 4")}</li><ol><li>{WrapInSpan("Item 5")}</li><ol><li>{WrapInSpan("Item 6")}</li></ol></ol></ol>",
									$"<ol><ol><li>{WrapInSpan("Item 1")}</li><ol><li>{WrapInSpan("Item 2")}</li></ol><li>{WrapInSpan("Item 3")}</li><li>{WrapInSpan("Item 4")}</li><ol><li>{WrapInSpan("Item 5")}</li><ol><li>{WrapInSpan("Item 6")}</li></ol></ol></ol></ol>",
									0, 48) { TestName = "{m}_Indent_MultiLevelMultiPeak_OrderedList" };
		yield return new TestCaseData($"<ol><li>{WrapInSpan("Item 1")}</li><ol><li>{WrapInSpan("Item 2")}</li></ol><li>{WrapInSpan("Item 3")}</li><li>{WrapInSpan("Item 4")}</li><ol><li>{WrapInSpan("Item 5")}</li></ol><li>{WrapInSpan("Item 6")}</li></ol>",
									$"<ol><li>{WrapInSpan("Item 1")}</li><ol><ol><li>{WrapInSpan("Item 2")}</li></ol><li>{WrapInSpan("Item 3")}</li><li>{WrapInSpan("Item 4")}</li><ol><li>{WrapInSpan("Item 5")}</li></ol></ol><li>{WrapInSpan("Item 6")}</li></ol>",
									7, 28) { TestName = "{m}_Indent_UShapedMiddleBlock" };
		yield return new TestCaseData($"<ol><li>{WrapInSpan("Item 1")}</li><ol><ol><li>{WrapInSpan("Item 2")}</li></ol><li>{WrapInSpan("Item 3")}</li></ol><li>{WrapInSpan("Item 4")}</li><li>{WrapInSpan("Item 5")}</li><li>{WrapInSpan("Item 6")}</li></ol>",
									$"<ol><li>{WrapInSpan("Item 1")}</li><ol><ol><ol><li>{WrapInSpan("Item 2")}</li></ol><li>{WrapInSpan("Item 3")}</li></ol><li>{WrapInSpan("Item 4")}</li></ol><li>{WrapInSpan("Item 5")}</li><li>{WrapInSpan("Item 6")}</li></ol>",
									7, 23) { TestName = "{m}_Indent_DecliningList_TwoLevelsAbovePrevElement" };
	}

	static IEnumerable<TestCaseData> ListToolbarIndentsCorrectlyData()
	{
		yield return new TestCaseData(WrapInP("Item 1"), WrapInPWithPadding("Item 1", 40), 0, 1) { TestName = "{m}_Indent_SingleP" };
		yield return new TestCaseData(WrapInP("Item 1") + WrapInP("Item 2") + WrapInP("Item 3") + WrapInP("Item 4"),
									WrapInPWithPadding("Item 1", 40) + WrapInPWithPadding("Item 2", 40) + WrapInPWithPadding("Item 3", 40) + WrapInPWithPadding("Item 4", 40),
									0, 24) { TestName = "{m}_Indent_Multiple_P" };
		yield return new TestCaseData(WrapInP("Item 1") + WrapInP("Item 2") + WrapInP("Item 3") + WrapInP("Item 4"),
									WrapInP("Item 1") + WrapInPWithPadding("Item 2", 40) + WrapInPWithPadding("Item 3", 40) + WrapInP("Item 4"),
									7, 15) { TestName = "{m}_Indent_SubSelection_P" };
		yield return new TestCaseData(WrapInPWithPadding("Item 1", 40) + WrapInPWithPadding("Item 2", 40) + WrapInPWithPadding("Item 3", 40),
									WrapInPWithPadding("Item 1", 40) + WrapInPWithPadding("Item 2", 80) + WrapInPWithPadding("Item 3", 80),
									7, 15) { TestName = "{m}_Indent_AlreadyIndentedSelection_P" };
		yield return new TestCaseData(WrapInP("Item 1") + WrapInPWithPadding("Item 2", 40) + WrapInPWithPadding("Item 3", 80) + WrapInPWithPadding("Item 4", 120),
									WrapInPWithPadding("Item 1", 40) + WrapInPWithPadding("Item 2", 80) + WrapInPWithPadding("Item 3", 120) + WrapInPWithPadding("Item 4", 160),
									0, 24) { TestName = "{m}_Indent_MultiLevel_P" };
		yield return new TestCaseData(WrapInP("Item 1") + WrapInPWithPadding("Item 2", 40) + WrapInPWithPadding("Item 3", 80) + WrapInPWithPadding("Item 4", 120) + WrapInPWithPadding("Item 5", 80) + WrapInPWithPadding("Item 6", 40) + WrapInP("Item 7"),
									WrapInPWithPadding("Item 1", 40) + WrapInPWithPadding("Item 2", 80) + WrapInPWithPadding("Item 3", 120) + WrapInPWithPadding("Item 4", 160) + WrapInPWithPadding("Item 5", 120) + WrapInPWithPadding("Item 6", 80) + WrapInPWithPadding("Item 7", 40),
									0, 43) { TestName = "{m}_Indent_MultiLevelBellCurve_P" };
		yield return new TestCaseData(WrapInP("Item 1") + WrapInPWithPadding("Item 2", 40) + WrapInPWithPadding("Item 3", 80) + WrapInPWithPadding("Item 4", 40) + WrapInP("Item 5"),
									WrapInP("Item 1") + WrapInPWithPadding("Item 2", 80) + WrapInPWithPadding("Item 3", 120) + WrapInPWithPadding("Item 4", 80) + WrapInP("Item 5"),
									7, 24) { TestName = "{m}_Indent_MultiLevelSubSelection_P" };

		yield return new TestCaseData($"<ol><li>{WrapInSpan("Item 1")}</li></ol>",
									$"<ol><ol><li>{WrapInSpan("Item 1")}</li></ol></ol>",
									0, 6) { TestName = "{m}_Indent_SingleLi_OrderedList" };
		yield return new TestCaseData($"<ol><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ol>",
									$"<ol><ol><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ol></ol>",
									0, 18) { TestName = "{m}_Indent_MultipleLi_OrderedList" };
		yield return new TestCaseData($"<ol><li>{WrapInSpan("Item 1")}</li><ol><li>{WrapInSpan("Item 2")}</li></ol><li>{WrapInSpan("Item 3")}</li></ol>",
									$"<ol><ol><li>{WrapInSpan("Item 1")}</li><ol><li>{WrapInSpan("Item 2")}</li></ol><li>{WrapInSpan("Item 3")}</li></ol></ol>",
									0, 18) { TestName = "{m}_Indent_NestedLi_OrderedList" };
		yield return new TestCaseData($"<ol><ol><li>{WrapInSpan("Item 1")}</li></ol><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ol>",
									$"<ol><ol><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ol></ol>",
									7, 18) { TestName = "{m}_Indent_ListShouldAttachToPreviousList_OrderedList" };
		yield return new TestCaseData($"<ol><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><ol><li>{WrapInSpan("Item 3")}</li></ol></ol>",
									$"<ol><ol><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ol></ol>",
									0, 12) { TestName = "{m}_Indent_ListShouldAttachToNextList_OrderedList" };
		yield return new TestCaseData($"<ol><ol><li>{WrapInSpan("Item 1")}</li></ol><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li><ol><li>{WrapInSpan("Item 4")}</li></ol></ol>",
									$"<ol><ol><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li><li>{WrapInSpan("Item 4")}</li></ol></ol>",
									7, 17) { TestName = "{m}_Indent_ListShouldCombineNextPrevList_OrderedList" };

		yield return new TestCaseData($"<ul><li>{WrapInSpan("Item 1")}</li></ul>",
									$"<ul><ul><li>{WrapInSpan("Item 1")}</li></ul></ul>",
									0, 6) { TestName = "{m}_Indent_SingleLi_UnorderedList" };
		yield return new TestCaseData($"<ul><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ul>",
									$"<ul><ul><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ul></ul>",
									0, 18) { TestName = "{m}_Indent_MultipleLi_UnorderedList" };
		yield return new TestCaseData($"<ul><li>{WrapInSpan("Item 1")}</li><ul><li>{WrapInSpan("Item 2")}</li></ul><li>{WrapInSpan("Item 3")}</li></ul>",
									$"<ul><ul><li>{WrapInSpan("Item 1")}</li><ul><li>{WrapInSpan("Item 2")}</li></ul><li>{WrapInSpan("Item 3")}</li></ul></ul>",
									0, 18) { TestName = "{m}_Indent_NestedLi_UnorderedList" };
		yield return new TestCaseData($"<ul><ul><li>{WrapInSpan("Item 1")}</li></ul><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ul>",
									$"<ul><ul><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ul></ul>",
									7, 18) { TestName = "{m}_Indent_ListShouldAttachToPreviousList_UnorderedList" };
		yield return new TestCaseData($"<ul><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><ul><li>{WrapInSpan("Item 3")}</li></ul></ul>",
									$"<ul><ul><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ul></ul>",
									0, 12) { TestName = "{m}_Indent_ListShouldAttachToNextList_UnorderedList" };
		yield return new TestCaseData($"<ul><ul><li>{WrapInSpan("Item 1")}</li></ul><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li><ul><li>{WrapInSpan("Item 4")}</li></ul></ul>",
									$"<ul><ul><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li><li>{WrapInSpan("Item 4")}</li></ul></ul>",
									7, 17) { TestName = "{m}_Indent_ListShouldCombineNextPrevList_UnorderedList" };
	}

	[TestCaseSource(nameof(ListToolbarIndentsCorrectlyData)), WithPlaywrightPage]
	public async Task ListToolbarIndentsCorrectly(string input, string expected, int selectionStart, int selectionEnd)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox
		{
			Width = 300,
			Height = 500,
			Html = input,
			IsToolBarVisible = true,
		});
		var client = await RichTextBoxClient.GetClientAsync();
		await client.EvaluateAsync(@$"client => {{
			client.setSelection({{ start: {selectionStart}, end: {selectionEnd}}});
		}}");
		var toolbarLocator = page.Locator(".richtextbox__toolbar");
		await toolbarLocator.WaitForAsync();
		var toolbarButton = toolbarLocator.GetByLabel("Increase indent");
		await toolbarButton.ClickAsync();
		var result = await client.GetEditorHtmlAsync();
		Assert.That(result, Is.EqualTo(expected));
	}

	[TestCaseSource(nameof(ListToolBarCorrectlyIndentsComplexData)), WithPlaywrightPage, Explicit]
	public async Task ListToolbarDoesNotIndentCorrectly(string input, string expected, int selectionStart, int selectionEnd)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox
		{
			Width = 300,
			Height = 500,
			Html = input,
			IsToolBarVisible = true,
		});
		var client = await RichTextBoxClient.GetClientAsync();
		await client.EvaluateAsync(@$"client => {{
			client.setSelection({{ start: {selectionStart}, end: {selectionEnd}}});
		}}");
		var toolbarLocator = page.Locator(".richtextbox__toolbar");
		await toolbarLocator.WaitForAsync();
		var toolbarButton = toolbarLocator.GetByLabel("Increase indent");
		await toolbarButton.ClickAsync();
		var result = await client.GetEditorHtmlAsync();
		Assert.That(result, Is.EqualTo(expected));
	}

	static IEnumerable<TestCaseData> ListToolBarCorrectlyOutdentsComplexData()
	{
		yield return new TestCaseData($"<ol><ol><li>{WrapInSpan("Item 1")}</li><ol><li>{WrapInSpan("Item 2")}</li></ol><li>{WrapInSpan("Item 3")}</li><li>{WrapInSpan("Item 4")}</li><ol><li>{WrapInSpan("Item 5")}</li></ol><li>{WrapInSpan("Item 6")}</li></ol></ol>",
										$"<ol><ol><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li></ol><li>{WrapInSpan("Item 3")}</li><li>{WrapInSpan("Item 4")}</li><ol><li>{WrapInSpan("Item 5")}</li><li>{WrapInSpan("Item 6")}</li></ol></ol>",
										7, 28) { TestName = "{m}_Outdent_NestedListOutdent" };
		yield return new TestCaseData($"<ol><li>{WrapInSpan("Item 1")}</li><ol><ol><li>{WrapInSpan("Item 2")}</li></ol><li>{WrapInSpan("Item 3")}</li></ol><li>{WrapInSpan("Item 4")}</li><li>{WrapInSpan("Item 5")}</li><li>{WrapInSpan("Item 6")}</li></ol>",
										$"<ol><li>{WrapInSpan("Item 1")}</li><ol><li>{WrapInSpan("Item 2")}</li></ol><li>{WrapInSpan("Item 3")}</li><li>{WrapInSpan("Item 4")}</li><li>{WrapInSpan("Item 5")}</li><li>{WrapInSpan("Item 6")}</li></ol>",
										7, 12) { TestName = "{m}_Outdent_DecliningListOutdent" };
		yield return new TestCaseData($"<ol><li>{WrapInSpan("Item 1")}</li><ol><li>{WrapInSpan("Item 2")}</li><ol><li>{WrapInSpan("Item 3")}</li></ol></ol><li>{WrapInSpan("Item 4")}</li><li>{WrapInSpan("Item 5")}</li><li>{WrapInSpan("Item 6")}</li></ol>",
										$"<ol><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><ol><li>{WrapInSpan("Item 3")}</li></ol><li>{WrapInSpan("Item 4")}</li><li>{WrapInSpan("Item 5")}</li><li>{WrapInSpan("Item 6")}</li></ol>",
										7, 12) { TestName = "{m}_Outdent_IncliningListOutdent" };
	}

	static IEnumerable<TestCaseData> ListToolbarOutdentsCorrectlyData()
	{
		yield return new TestCaseData(WrapInPWithPadding("Item 1", 40), WrapInP("Item 1"), 0, 1) { TestName = "{m}_Outdent_SingleP" };
		yield return new TestCaseData(WrapInPWithPadding("Item 1", 40) + WrapInPWithPadding("Item 2", 40) + WrapInPWithPadding("Item 3", 40) + WrapInPWithPadding("Item 4", 40),
									WrapInP("Item 1") + WrapInP("Item 2") + WrapInP("Item 3") + WrapInP("Item 4"),
									0, 24) { TestName = "{m}_Outdent_Multiple_P" };
		yield return new TestCaseData(WrapInP("Item 1") + WrapInPWithPadding("Item 2", 40) + WrapInPWithPadding("Item 3", 40) + WrapInP("Item 4"),
									WrapInP("Item 1") + WrapInP("Item 2") + WrapInP("Item 3") + WrapInP("Item 4"),
									7, 15) { TestName = "{m}_Outdent_SubSelection_P" };
		yield return new TestCaseData(WrapInPWithPadding("Item 1", 40) + WrapInPWithPadding("Item 2", 80) + WrapInPWithPadding("Item 3", 80),
									WrapInPWithPadding("Item 1", 40) + WrapInPWithPadding("Item 2", 40) + WrapInPWithPadding("Item 3", 40),
									7, 15) { TestName = "{m}_Outdent_AlreadyIndentedSelection_P" };
		yield return new TestCaseData(WrapInPWithPadding("Item 1", 40) + WrapInPWithPadding("Item 2", 80) + WrapInPWithPadding("Item 3", 120) + WrapInPWithPadding("Item 4", 160),
									WrapInP("Item 1") + WrapInPWithPadding("Item 2", 40) + WrapInPWithPadding("Item 3", 80) + WrapInPWithPadding("Item 4", 120),
									0, 24) { TestName = "{m}_Outdent_MultiLevel_P" };
		yield return new TestCaseData(WrapInPWithPadding("Item 1", 40) + WrapInPWithPadding("Item 2", 80) + WrapInPWithPadding("Item 3", 120) + WrapInPWithPadding("Item 4", 160) + WrapInPWithPadding("Item 5", 120) + WrapInPWithPadding("Item 6", 80) + WrapInPWithPadding("Item 7", 40),
									WrapInP("Item 1") + WrapInPWithPadding("Item 2", 40) + WrapInPWithPadding("Item 3", 80) + WrapInPWithPadding("Item 4", 120) + WrapInPWithPadding("Item 5", 80) + WrapInPWithPadding("Item 6", 40) + WrapInP("Item 7"),
									0, 43) { TestName = "{m}_Outdent_MultiLevelBellCurve_P" };
		yield return new TestCaseData(WrapInP("Item 1") + WrapInPWithPadding("Item 2", 80) + WrapInPWithPadding("Item 3", 120) + WrapInPWithPadding("Item 4", 80) + WrapInP("Item 5"),
									WrapInP("Item 1") + WrapInPWithPadding("Item 2", 40) + WrapInPWithPadding("Item 3", 80) + WrapInPWithPadding("Item 4", 40) + WrapInP("Item 5"),
									7, 24) { TestName = "{m}_Outdent_MultiLevelSubSelection_P" };

		yield return new TestCaseData($"<ol><ol><li>{WrapInSpan("Item 1")}</li></ol></ol>",
									$"<ol><li>{WrapInSpan("Item 1")}</li></ol>",
									0, 6) { TestName = "{m}_Outdent_SingleLi_OrderedList" };
		yield return new TestCaseData($"<ol><ol><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ol></ol>",
									$"<ol><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ol>",
									0, 18) { TestName = "{m}_Outdent_MultipleLi_OrderedList" };
		yield return new TestCaseData($"<ol><ol><li>{WrapInSpan("Item 1")}</li><ol><li>{WrapInSpan("Item 2")}</li></ol><li>{WrapInSpan("Item 3")}</li></ol></ol>",
									$"<ol><ol><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ol></ol>",
									7, 13) { TestName = "{m}_Outdent_MiddleLi_OrderedList" };
		yield return new TestCaseData($"<ol><ol><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ol></ol>",
									$"<ol><ol><li>{WrapInSpan("Item 1")}</li></ol><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ol>",
									7, 18) { TestName = "{m}_Outdent_RightSublist_OrderedList" };
		yield return new TestCaseData($"<ol><ol><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ol></ol>",
									$"<ol><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><ol><li>{WrapInSpan("Item 3")}</li></ol></ol>",
									0, 12) { TestName = "{m}_Outdent_LeftSublist_OrderedList" };
		yield return new TestCaseData($"<ol><ol><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li><li>{WrapInSpan("Item 4")}</li></ol></ol>",
									$"<ol><ol><li>{WrapInSpan("Item 1")}</li></ol><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li><ol><li>{WrapInSpan("Item 4")}</li></ol></ol>",
									7, 17) { TestName = "{m}_Outdent_ListShouldCombineNextPrevList_OrderedList" };

		yield return new TestCaseData($"<ul><ul><li>{WrapInSpan("Item 1")}</li></ul></ul>",
									$"<ul><li>{WrapInSpan("Item 1")}</li></ul>",
									0, 6) { TestName = "{m}_Outdent_SingleLi_UnorderedList" };
		yield return new TestCaseData($"<ul><ul><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ul></ul>",
									$"<ul><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ul>",
									0, 18) { TestName = "{m}_Outdent_MultipleLi_UnorderedList" };
		yield return new TestCaseData($"<ul><ul><li>{WrapInSpan("Item 1")}</li><ul><li>{WrapInSpan("Item 2")}</li></ul><li>{WrapInSpan("Item 3")}</li></ul></ul>",
									$"<ul><ul><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ul></ul>",
									7, 13) { TestName = "{m}_Outdent_MiddleLi_UnorderedList" };
		yield return new TestCaseData($"<ul><ul><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ul></ul>",
									$"<ul><ul><li>{WrapInSpan("Item 1")}</li></ul><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ul>",
									7, 18) { TestName = "{m}_Outdent_RightSublist_UnorderedList" };
		yield return new TestCaseData($"<ul><ul><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li></ul></ul>",
									$"<ul><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><ul><li>{WrapInSpan("Item 3")}</li></ul></ul>",
									0, 12) { TestName = "{m}_Outdent_LeftSublist_UnorderedList" };
		yield return new TestCaseData($"<ul><ul><li>{WrapInSpan("Item 1")}</li><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li><li>{WrapInSpan("Item 4")}</li></ul></ul>",
									$"<ul><ul><li>{WrapInSpan("Item 1")}</li></ul><li>{WrapInSpan("Item 2")}</li><li>{WrapInSpan("Item 3")}</li><ul><li>{WrapInSpan("Item 4")}</li></ul></ul>",
									7, 17) { TestName = "{m}_Outdent_ListShouldCombineNextPrevList_UnorderedList" };
	}

	[TestCaseSource(nameof(ListToolbarOutdentsCorrectlyData)), WithPlaywrightPage]
	public async Task ListToolbarOutdentsCorrectly(string input, string expected, int selectionStart, int selectionEnd)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox
		{
			Width = 300,
			Height = 500,
			Html = input,
			IsToolBarVisible = true,
		});
		var client = await RichTextBoxClient.GetClientAsync();
		await client.EvaluateAsync(@$"client => {{
			client.setSelection({{ start: {selectionStart}, end: {selectionEnd}}});
		}}");
		var toolbarLocator = page.Locator(".richtextbox__toolbar");
		await toolbarLocator.WaitForAsync();
		var toolbarButton = toolbarLocator.GetByLabel("Decrease indent");
		await toolbarButton.ClickAsync();
		var result = await client.GetEditorHtmlAsync();
		Assert.That(result, Is.EqualTo(expected));
	}

	[TestCaseSource(nameof(ListToolBarCorrectlyOutdentsComplexData)), WithPlaywrightPage, Explicit]
	public async Task ListToolbarDoesNotOutdentCorrectly(string input, string expected, int selectionStart, int selectionEnd)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox
		{
			Width = 300,
			Height = 500,
			Html = input,
			IsToolBarVisible = true,
		});
		var client = await RichTextBoxClient.GetClientAsync();
		await client.EvaluateAsync(@$"client => {{
			client.setSelection({{ start: {selectionStart}, end: {selectionEnd}}});
		}}");
		var toolbarLocator = page.Locator(".richtextbox__toolbar");
		await toolbarLocator.WaitForAsync();
		var toolbarButton = toolbarLocator.GetByLabel("Decrease indent");
		await toolbarButton.ClickAsync();
		var result = await client.GetEditorHtmlAsync();
		Assert.That(result, Is.EqualTo(expected));
	}

	[TestCase("Bold", "<span style=\"font-weight: bold;\">", "</span>", true, TestName = "{m}_Bold")]
	[TestCase("Italic", "<span style=\"font-style: italic;\">", "</span>", true, TestName = "{m}_Italic")]
	[TestCase("Underline", "<span style=\"text-decoration-line: underline;\">", "</span>", true, TestName = "{m}_Underline")]
	[TestCase("Strikethrough", "<span style=\"text-decoration-line: line-through;\">", "</span>", true, TestName = "{m}_Strikethrough")]
	[TestCase("Bullet list", "<ul><li>", "</li></ul>", false, TestName = "{m}_BulletList")]
	[TestCase("Numbered list", "<ol><li>", "</li></ol>", false, TestName = "{m}_NumberedList")]
	[TestCase("Increase indent", "<p style=\"padding-left: 40px;\">", "</p>", false, TestName = "{m}_IncreaseIndent")]
	[TestCase("Decrease indent", "<p>", "</p>", false, TestName = "{m}_DecreaseIndent")]
	[WithPlaywrightPage]
	public async Task RichTextBoxToolbarButtons(string toolbarButtonName, string openingTag, string closingTag, bool hasParagraphTag)
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 800, Height = 500 };
			richTextBox = new RichTextBox()
			{
				Width = 800,
				Height = 500,
				Text = "Test Content",
				IsToolBarVisible = true,
			};
			form.Controls.Add(richTextBox);
			return form;
		});

		var contentLocator = page.Locator(".richtextbox__editoranchor");
		var toolbarLocator = page.Locator(".richtextbox__toolbar");
		await contentLocator.WaitForAsync();
		await toolbarLocator.WaitForAsync();

		var client = await RichTextBoxClient.GetClientAsync();
		Assert.That(client.GetActiveTextAsync, Is.EqualTo("Test Content"));
		await client.EvaluateAsync($@"client => {{
			client.setSelection({{ start: 0, end: 3}});
		}}");

		var toolbarButton = toolbarLocator.GetByLabel(toolbarButtonName);
		await toolbarButton.ClickAsync();
		if (hasParagraphTag)
		{
			Assert.That(client.GetEditorHtmlAsync, Is.EqualTo($"<p>{openingTag}Tes{closingTag}t Content</p>"));

			var buttonActive = toolbarLocator.Locator($".richtextbox__toolbar-item--button.richtextbox__toolbar-item--active[aria-label='${toolbarButtonName}']");
			Assert.That(buttonActive, Is.Not.Null);
		}
		else
		{
			Assert.That(client.GetEditorHtmlAsync, Is.EqualTo($"{openingTag}Test Content{closingTag}"));

			switch (toolbarButtonName)
			{
				case "Bullet list":
				case "Numbered list":
					var buttonActive = toolbarLocator.Locator($".richtextbox__toolbar-item--button.richtextbox__toolbar-item--active[aria-label='${toolbarButtonName}']");
					Assert.That(buttonActive, Is.Not.Null);
					break;
			}
		}
	}

	[TestCase("https://example.com/",
		$"<p><a href=\"https://example.com/\" target=\"_blank\" rel=\"noopener\">https://example.com/</a></p>",
		$"<p><a href=\"https://example.com/\" target=\"_blank\" rel=\"noopener\" style=\"text-decoration-line: underline line-through;\">https://example.com/</a></p>",
		TestName = "{m}_SingleHyperlink")]
	[TestCase("abc https://example.com/ abc",
		$"<p>abc <a href=\"https://example.com/\" target=\"_blank\" rel=\"noopener\">https://example.com/</a> abc</p>",
		$"<p><span style=\"text-decoration-line: line-through;\">abc </span><a href=\"https://example.com/\" target=\"_blank\" rel=\"noopener\" style=\"text-decoration-line: underline line-through;\">https://example.com/</a><span style=\"text-decoration-line: line-through;\"> abc</span></p>",
		TestName = "{m}_HyperlinkWithMixedText")]
	[WithPlaywrightPage]
	public async Task RichTextBoxStrikeThroughWorksForHyperlinks(string text, string textHtml, string expectedTextHtmlAfterStrike)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox()
		{
			Width = 800,
			Height = 500,
			Text = text,
			IsToolBarVisible = true
		});

		var client = await RichTextBoxClient.GetClientAsync();
		Assert.That(client.GetEditorHtmlAsync, Is.EqualTo(textHtml));

		await client.EvaluateAsync($@"client => {{
			client.setSelection({{ start: 0, end: {text.Length}}});
		}}");

		var toolbarLocator = page.Locator(".richtextbox__toolbar");
		await toolbarLocator.WaitForAsync();
		var strikeThroughButton = toolbarLocator.GetByLabel("Strikethrough");

		await strikeThroughButton.ClickAsync();
		Assert.That(client.GetEditorHtmlAsync, Is.EqualTo(expectedTextHtmlAfterStrike));

		await strikeThroughButton.ClickAsync(); // Clicking the strikethrough button again should remove the strikethrough formatting and restore the original text appearance
		Assert.That(client.GetEditorHtmlAsync, Is.EqualTo(textHtml));
	}

	[TestCase("Fonts", "Verdana", "<span style=\"font-family: Verdana, sans-serif;\">", "</span>", "Verdana", TestName = "{m}_Verdana")]
	[TestCase("Font sizes", "12", "<span style=\"font-size: 12pt;\">", "</span>", "12", TestName = "{m}_Size12pt")]
	[TestCase("Font sizes", "18", "<span style=\"font-size: 18pt;\">", "</span>", "18", TestName = "{m}_Size18pt")]
	[WithPlaywrightPage]
	public async Task RichTextBoxToolbarMenus(string toolbarMenuName, string toolbarMenuSelection, string openingTag, string closingTag, string expectedToolbarResult)
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 800, Height = 500 };
			richTextBox = new RichTextBox()
			{
				Width = 800,
				Height = 500,
				Text = "Test Content",
				IsToolBarVisible = true,
			};
			form.Controls.Add(richTextBox);
			return form;
		});

		Assert.That(richTextBox, Is.Not.Null);

		var contentLocator = page.Locator(".richtextbox__editoranchor");
		var toolbarLocator = page.Locator(".richtextbox__toolbar");
		await contentLocator.WaitForAsync();
		await toolbarLocator.WaitForAsync();

		var sizeSelector = page.GetByLabel("Font sizes", new() { Exact = true });
		await sizeSelector.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
		Assert.That(async () => await sizeSelector.InputValueAsync(), Is.EqualTo("10").After(1000, 100));

		var client = await RichTextBoxClient.GetClientAsync();
		Assert.That(client.GetActiveTextAsync, Is.EqualTo("Test Content"));
		await client.EvaluateAsync($@"client => {{
			client.setSelection({{ start: 0, end: 3}});
		}}");

		var toolbarMenu = toolbarLocator.GetByLabel(toolbarMenuName, new() { Exact = true });
		await toolbarMenu.FillAsync(toolbarMenuSelection);
		await toolbarMenu.PressAsync("Enter");

		Assert.That(client.GetEditorHtmlAsync, Is.EqualTo($"<p>{openingTag}Tes{closingTag}t Content</p>"));
		Assert.That(async () => await toolbarMenu.InputValueAsync(), Is.EqualTo(expectedToolbarResult).After(500, 100));

		await client.EvaluateAsync($@"client => {{
			client.setSelection({{ start: 4, end: 6}});
		}}");

		Assert.That(async () => await toolbarMenu.InputValueAsync(), Is.Not.EqualTo(expectedToolbarResult).After(500, 100));

		await client.EvaluateAsync($@"client => {{
			client.setSelection({{ start: 1, end: 2}});
		}}");

		Assert.That(async () => await toolbarMenu.InputValueAsync(), Is.EqualTo(expectedToolbarResult).After(500, 100));
	}

	[TestCase("18", "<strong>", "</strong>", "18pt")]
	[TestCase("16", "<em>", "</em>", "16pt")]
	[TestCase("16", "<strike>", "</strike>", "16pt")]
	[TestCase("14", "<b>", "</b>", "14pt")]
	[TestCase("16", "<i>", "</i>", "16pt")]
	[TestCase("18", "<u>", "</u>", "18pt")]
	[TestCase("20", "<s>", "</s>", "20pt")]
	[TestCase("22", "<strong><em>", "</em></strong>", "22pt")]
	[TestCase("24", "<span style=\"font-style: italic; font-weight: bold;\">", "</span>", "24pt")]
	[TestCase("32", "<span style=\"color: red; text-decoration-line: underline;\">", "</span>", "32pt")]
	[WithPlaywrightPage]
	public async Task RichTextBoxUpdatesFontSizeForNonSpanElements(string toolbarMenuSelection, string openingTag, string closingTag, string expectedToolbarResult)
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			richTextBox = new RichTextBox()
			{
				Width = 800,
				Height = 500,
				Html = $"<p>{openingTag}Lorem ipsum{closingTag}</p>",
				IsToolBarVisible = true,
			};
			return richTextBox;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();

		var toolbarLocator = page.Locator(".richtextbox__toolbar");
		await toolbarLocator.WaitForAsync();

		await client.EvaluateAsync($@"client => {{
			client.setSelection({{ start: 0, end: 11}});
		}}");

		var toolbarMenu = toolbarLocator.GetByLabel("Font sizes", new() { Exact = true });
		await toolbarMenu.FillAsync(toolbarMenuSelection);
		await toolbarMenu.PressAsync("Enter");

		Assert.That(client.GetEditorHtmlAsync, Does.Not.Contain("font-size: xxx-large;"));
		Assert.That(client.GetEditorHtmlAsync, Does.Contain($"font-size: {expectedToolbarResult};"));
	}

	[TestCase("Attach a file", false)]
	[TestCase("Insert Image", true)]
	[WithPlaywrightPage]
	public async Task RichTextBox_Toolbar_Uploads(string toolbarButtonName, bool filterInputs)
	{
		await using var ctx = new InMemoryTestServerContext();
		Form? form = null;
		RichTextBox? richTextBox = null!;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form { Width = 800, Height = 500 };
			richTextBox = new RichTextBox()
			{
				Width = 800,
				Height = 500,
				IsToolBarVisible = true,
			};
			form.Controls.Add(richTextBox);
			return form;
		});

		await page.EvaluateAsync(@"() => {
				window.showOpenFilePicker = async (options) => {
					window.filePickerOptions = options;

					return [
						{
							getFile: () => new Promise((resolve, reject) => {
								var blob1 = new Blob(['This is file one.'], {type: 'text/plain'});
								var file1 = new File([blob1], 'File1.txt', {type: 'text/plain', lastModified: 1654862400000});
								file1.blob = blob1;
								resolve(file1);
							})
						}];
				}
			}");

		var contentLocator = page.Locator(".richtextbox__editoranchor");
		var toolbarLocator = page.Locator(".richtextbox__toolbar");
		await contentLocator.WaitForAsync();
		await toolbarLocator.WaitForAsync();

		var toolbarButton = toolbarLocator.GetByLabel(toolbarButtonName);
		await toolbarButton.ClickAsync();

		if (filterInputs)
		{
			Assert.That(await page.EvaluateAsync<bool>("!!window.filePickerOptions.types[0].accept && !!window.filePickerOptions.types[0].description"), Is.True);
			Assert.That(await page.EvaluateAsync<int>("Object.keys(filePickerOptions.types[0].accept).length"), Is.GreaterThanOrEqualTo(3));
		}
		else
		{
			Assert.That(await page.EvaluateAsync<bool>("typeof window.filePickerOptions.types === 'undefined'"), Is.True);
		}

		if (form is null)
		{
			throw new Exception("Form did not initialise");
		}
		else if (form.CargoWiseClientServices is null)
		{
			throw new Exception("CargoWiseClientServices did not initialise");
		}

		var stream = new MemoryStream(Encoding.ASCII.GetBytes("Dummy file contents"));
		var jsStreamReference = new Mock<IJSStreamReference>();
		jsStreamReference.Setup(i => i.OpenReadStreamAsync(It.IsAny<long>(), It.IsAny<CancellationToken>())).Returns(() => ValueTask.FromResult((Stream)stream));

		var uploadedFile = new BrowserFile()
		{
			ContentType = "text/plain",
			LastModified = new DateTimeOffset(),
			Name = "blah.txt",
			FileStream = jsStreamReference.Object,
		};

		var uploadedFiles = new[] { uploadedFile };
		var uploadResults = await form.CargoWiseClientServices.FileService.UploadFilesToServerAsync(uploadedFiles, 52428800, CancellationToken.None);

		Assert.That(uploadedFiles.Count, Is.EqualTo(1));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBox_ColorSelection()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 800, Height = 500 };
			richTextBox = new RichTextBox()
			{
				Width = 800,
				Height = 500,
				Html = WrapInP("Lorem ipsum dolor sit amet, consectetur adipiscing elit.")
						+ WrapInP("Ut consectetur, nisl accumsan malesuada hendrerit, odio magna aliquet nibh, vel placerat ipsum magna non orci.")
						+ WrapInP("Pellentesque in dolor iaculis lectus tristique tristique."),
				IsToolBarVisible = true,
			};
			form.Controls.Add(richTextBox);
			return form;
		});
		var contentLocator = page.Locator(".richtextbox__editoranchor");
		var toolbarLocator = page.Locator(".richtextbox__toolbar");
		await contentLocator.WaitForAsync();
		await toolbarLocator.WaitForAsync();

		var client = await RichTextBoxClient.GetClientAsync();
		Assert.That(client.GetActiveTextAsync, Is.SupersetOf("Lorem ipsum dolor sit amet, consectetur a"));
		await client.EvaluateAsync($@"client => {{
			client.setSelection({{ start: 0, end: 56}});
		}}");

		var colorPickerButton = toolbarLocator.GetByLabel("Text color");
		await richTextBox.InvokeWinzorDispatcherAsync(() =>
		{
			richTextBox.ColorPicked = "#ff0000";
		});
		await colorPickerButton!.Locator("svg").WaitForAsync();

		Assert.That(client.GetEditorHtmlAsync, Is.EqualTo(
			$"<p><span style=\"{standardFontStyle} color: rgb(255, 0, 0);\">Lorem ipsum dolor sit amet, consectetur adipiscing elit.</span></p>" +
			WrapInP("Ut consectetur, nisl accumsan malesuada hendrerit, odio magna aliquet nibh, vel placerat ipsum magna non orci.") +
			WrapInP("Pellentesque in dolor iaculis lectus tristique tristique.")));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxFormatPainterCanPaintPartialTextFragments()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 800, Height = 500 };
			richTextBox = new RichTextBox()
			{
				Width = 800,
				Height = 500,
				Html = WrapInP("Lorem ipsum dolor sit amet, consectetur adipiscing elit.")
						+ WrapInP("Ut consectetur, nisl accumsan malesuada hendrerit, odio magna aliquet nibh, vel placerat ipsum magna non orci.")
						+ WrapInP("Pellentesque in dolor iaculis lectus tristique tristique."),
				IsToolBarVisible = true,
			};
			form.Controls.Add(richTextBox);
			return form;
		});
		Assert.That(richTextBox, Is.Not.Null);

		var editor = page.GetByRole(AriaRole.Textbox);
		await editor.WaitForAsync();

		var destinationText = "Pellentesque in dolor iaculis lectus tristique tristique.";
		var paintStartOffsetText = "Pellentesque";

		var sourceLocator = page.GetByText("Lorem ipsum dolor sit amet, consectetur adipiscing elit.");
		var destinationLocator = page.GetByText(destinationText);

		await sourceLocator.SelectTextAsync();

		await page.GetByLabel("Bold").ClickAsync();
		await page.GetByLabel("Underline").ClickAsync();

		var fontMenu = page.GetByLabel("Fonts", new() { Exact = true });
		await fontMenu.FillAsync("Comic Sans MS");
		await fontMenu.PressAsync("Enter");

		Assert.That(async () => await editor.InnerHTMLAsync(), Is.EqualTo(
			$"<p><span style=\"font-size: 20px; font-weight: bold; text-decoration-line: underline; font-family: &quot;Comic Sans MS&quot;, sans-serif;\">Lorem ipsum dolor sit amet, consectetur adipiscing elit.</span></p>"
			+ WrapInP("Ut consectetur, nisl accumsan malesuada hendrerit, odio magna aliquet nibh, vel placerat ipsum magna non orci.")
			+ WrapInP("Pellentesque in dolor iaculis lectus tristique tristique.")
		).After(2000, 100));

		await page.GetByLabel("Format Painter - Copy Selected Formatting").ClickAsync();

		var coords = (await destinationLocator.EvaluateAsync("(elm, start) => new WTG.SelectionFinder(elm).DOMRangeFor({start: start, end: elm.innerText.length }).getBoundingClientRect()", paintStartOffsetText.Length)).Value!;
		var mouseY = coords.GetProperty("top").GetSingle();
		await page.Mouse.MoveAsync(coords.GetProperty("left").GetSingle(), mouseY);
		await page.Mouse.DownAsync(new MouseDownOptions() { Button = MouseButton.Left });
		await page.Mouse.MoveAsync(coords.GetProperty("right").GetSingle(), mouseY);
		await page.Mouse.UpAsync(new MouseUpOptions() { Button = MouseButton.Left });
		await destinationLocator.HoverAsync(new LocatorHoverOptions { Position = new Position() { X = 0 } });
		await page.Mouse.DownAsync(new MouseDownOptions() { Button = MouseButton.Left });
		await destinationLocator.HoverAsync(new LocatorHoverOptions { Position = new Position() { X = (await destinationLocator.BoundingBoxAsync())!.Width - 1 } });
		await page.Mouse.UpAsync(new MouseUpOptions() { Button = MouseButton.Left });

		Assert.That(async () => await editor.InnerHTMLAsync(), Is.EqualTo(
			$"<p><span style=\"font-size: 20px; font-weight: bold; text-decoration-line: underline; font-family: &quot;Comic Sans MS&quot;, sans-serif;\">Lorem ipsum dolor sit amet, consectetur adipiscing elit.</span></p>"
			+ WrapInP("Ut consectetur, nisl accumsan malesuada hendrerit, odio magna aliquet nibh, vel placerat ipsum magna non orci.")
			+ $"<p><span style=\"font-size: 20px;\"><span style=\"font-family: Tahoma, sans-serif;\">Pellentesque</span><span style=\"font-weight: bold; text-decoration-line: underline; font-family: &quot;Comic Sans MS&quot;, sans-serif;\"> in dolor iaculis lectus tristique tristique.</span></span></p>"
		).After(2000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxFormatPainterWorksWithZeroLengthSelections()
	{
		await using var ctx = new InMemoryTestServerContext();

		RichTextBox richTextBox = null!;
		var page = await ctx.LoadFormAsync(() =>
		{
			richTextBox = new RichTextBox()
			{
				Width = 700,
				Height = 500,
				IsToolBarVisible = true,
				Html = EnumerableTree.Generator(b => b
					.Open<Paragraph>()
						.Open(new Phrase
						{
							Bold = true,
						})
							.Add("Copy Style From Here")
					.Open<Paragraph>()
						.Open(new Phrase
						{
							Italic = true,
						})
							.Add("Apply It Here")
					.End()
				)
					.Reduce(HtmlEncoder.CreateFactory())
					.Markup(),
				SelectionStart = 1,
				SelectionLength = 0,
			};
			var form = new Form
			{
				Width = 700,
				Height = 500
			};
			form.Controls.Add(richTextBox);
			return form;
		});

		var destinationLocator = page.GetByText("Apply It Here");
		var sourceLocator = page.GetByRole(AriaRole.Strong);
		await sourceLocator.WaitForAsync();

		// select a zero-length range in the source

		// click 'Format Painter' button.
		await page.GetByLabel("Format Painter - Copy Selected Formatting").ClickAsync();

		// apply the format at the end of the line so we can type some text
		var coords = (await destinationLocator.EvaluateAsync("elm => { var caret = elm.innerText.length; return new WTG.SelectionFinder(elm).DOMRangeFor({start: caret, end: caret }).getBoundingClientRect();}")).Value!;
		var mouseY = coords.GetProperty("top").GetSingle();
		await page.Mouse.MoveAsync(coords.GetProperty("left").GetSingle(), mouseY);
		await page.Mouse.DownAsync(new MouseDownOptions() { Button = MouseButton.Left });
		await page.Mouse.MoveAsync(coords.GetProperty("right").GetSingle(), mouseY);
		await page.Mouse.UpAsync(new MouseUpOptions() { Button = MouseButton.Left });

		// assert that we didn't color the whole container
		Assert.That(async () => await destinationLocator.GetComputedStyleAsync("font-weight"), Is.EqualTo("400"));
		Assert.That(async () => await destinationLocator.GetComputedStyleAsync("font-style"), Is.EqualTo("italic"));

		// type some text
		await page.Keyboard.TypeAsync("I'm Typed Text");

		// assert that we did color the typed text
		var resultLocator = page.GetByText("I'm Typed Text");
		Assert.That(async () => await resultLocator.GetComputedStyleAsync("font-weight"), Is.EqualTo("700"));
		Assert.That(async () => await resultLocator.GetComputedStyleAsync("font-style"), Is.EqualTo("normal"));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxClickEnterInputModeWithDelay()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 1000, Height = 1000 };
			richTextBox = new RichTextBox()
			{
				Top = 100,
				Left = 200,
				Width = 500,
				Height = 500
			};
			form.Controls.Add(richTextBox);
			return form;
		});

		await page.ClickAsync($".richtextbox--wtgeditor > div.richtextbox__editoranchor");
		await page.Keyboard.PressAsync("1");
		var client = await RichTextBoxClient.GetClientAsync();
		Assert.That(client.GetActiveTextAsync, Is.EqualTo("1"));

		await page.Mouse.ClickAsync(10, 10);    // focus out
		await page.ClickAsync($".richtextbox--wtgeditor > div.richtextbox__editoranchor");
		await Task.Delay(100);
		await page.Keyboard.PressAsync("1");
		Assert.That(client.GetActiveTextAsync, Is.EqualTo("11"));
	}

	[Test, WithPlaywrightPage]
	public async Task LostFocusNotRaisedOnParentOfControlGettingFocus()
	{
		RichTextBox? textBox1 = null!;
		RichTextBox? textBox2 = null!;

		await using var ctx = new InMemoryTestServerContext();

		var userControlLostFocus = false;
		var textBox1GotFocused = new TaskCompletionSource();
		var textBox2GotFocused = new TaskCompletionSource();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var userControl = new UserControl();
			userControl.Size = new Size(300, 300);
			textBox1 = new RichTextBox();
			textBox1.TabIndex = 1;
			textBox1.Size = new Size(100, 100);
			textBox1.Text = "One";
			textBox1.GotFocus += (s, e) => textBox1GotFocused.SetResult();
			userControl.Controls.Add(textBox1);
			textBox2 = new RichTextBox();
			textBox2.TabIndex = 2;
			textBox2.Size = new Size(100, 100);
			textBox2.Left = 101;
			textBox2.Text = "Two";
			textBox2.GotFocus += (s, e) => textBox2GotFocused.SetResult();
			userControl.Controls.Add(textBox2);
			form.Controls.Add(userControl);
			userControl.LostFocus += (s, e) => { userControlLostFocus = true; };
			return form;
		});

		var one = await RichTextBoxClient.GetClientAsync(textBox1);
		Assert.That(one.GetEditorTextAsync, Is.EqualTo("One"));
		await one.FocusEditorAsync();
		Assert.That(await textBox1GotFocused.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		Assert.That(userControlLostFocus, Is.False);

		var two = await RichTextBoxClient.GetClientAsync(textBox2);
		Assert.That(two.GetEditorTextAsync, Is.EqualTo("Two"));
		await two.FocusEditorAsync();
		Assert.That(await textBox2GotFocused.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		Assert.That(userControlLostFocus, Is.False);
	}

	[Test, WithPlaywrightPage]
	public async Task LostFocusNotRaisedOnParentControl()
	{
		await using var ctx = new InMemoryTestServerContext();

		var userControlLostFocus = false;
		RichTextBox? textBox1 = null!;
		RichTextBox? textBox2 = null!;
		var textBox1GotFocused = new TaskCompletionSource();
		var textBox2GotFocused = new TaskCompletionSource();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var userControl = new UserControl();
			userControl.TabIndex = 1;
			userControl.Size = new Size(100, 100);
			textBox1 = new RichTextBox();
			textBox1.Size = new Size(100, 100);
			textBox1.Text = "One";
			textBox1.GotFocus += (s, e) => textBox1GotFocused.SetResult();
			userControl.Controls.Add(textBox1);
			userControl.LostFocus += (s, e) => { userControlLostFocus = true; };
			form.Controls.Add(userControl);
			textBox2 = new RichTextBox();
			textBox2.TabIndex = 3;
			textBox2.Size = new Size(100, 100);
			textBox2.Left = 101;
			textBox2.Text = "Two";
			textBox2.GotFocus += (s, e) => textBox2GotFocused.SetResult();
			form.Controls.Add(textBox2);
			return form;
		});

		var one = await RichTextBoxClient.GetClientAsync(textBox1);
		var two = await RichTextBoxClient.GetClientAsync(textBox2);
		await one.FocusEditorAsync();
		Assert.That(await textBox1GotFocused.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		Assert.That(userControlLostFocus, Is.False);
		await two.FocusEditorAsync();
		Assert.That(await textBox2GotFocused.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		Assert.That(userControlLostFocus, Is.False);
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxReadOnlyAppliedToClient()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Text = "Read Only Text Box",
				ReadOnly = true
			};
			form.Controls.Add(richTextBox);
			return form;
		});

		var childrenLocator = page.Locator($".richtextbox[data-winzor-control-id=\"{richTextBox!.WinzorControlId}\"] > *");
		var dataLocator = page.Locator($".richtextbox[data-winzor-control-id=\"{richTextBox!.WinzorControlId}\"] > .richtextbox__data");
		var anchorLocator = page.Locator($"div#richtextbox__editoranchor__{richTextBox.WinzorControlId}");
		var frameLocator = page.Locator($"iframe#richtextbox__editoranchor__{richTextBox.WinzorControlId}_ifr");

		var client = await RichTextBoxClient.GetClientAsync();

		Assert.That(async () => await client.EvaluateAsync<bool>("client => client.isReadOnly"), Is.EqualTo(true));
		Assert.That(childrenLocator.CountAsync, Is.EqualTo(1));
		Assert.That(dataLocator.CountAsync, Is.EqualTo(1));
		Assert.That(anchorLocator.CountAsync, Is.Zero);
		Assert.That(frameLocator.CountAsync, Is.Zero);
		Assert.That(await dataLocator.IsVisibleAsync(), Is.True);
		Assert.That(client.GetActiveTextAsync, Is.EqualTo("Read Only Text Box").After(3000, 100));

		await richTextBox.InvokeWinzorDispatcherAsync(() => richTextBox.ReadOnly = false);

		Assert.That(async () => await client.EvaluateAsync<bool>("client => client.isReadOnly"), Is.EqualTo(false).After(3000, 100));
		Assert.That(childrenLocator.CountAsync, Is.EqualTo(2));
		Assert.That(dataLocator.CountAsync, Is.EqualTo(1));
		Assert.That(anchorLocator.CountAsync, Is.EqualTo(1));
		Assert.That(frameLocator.CountAsync, Is.Zero);
		Assert.That(await dataLocator.IsVisibleAsync(), Is.False);
		Assert.That(client.GetActiveTextAsync, Is.EqualTo("Read Only Text Box").After(3000, 100));
		Assert.That(client.GetEditorTextAsync, Is.EqualTo("Read Only Text Box"));

		await richTextBox.InvokeWinzorDispatcherAsync(() => richTextBox.ReadOnly = true);

		Assert.That(async () => await client.EvaluateAsync<bool>("client => client.isReadOnly"), Is.EqualTo(true).After(3000, 100));
		Assert.That(childrenLocator.CountAsync, Is.EqualTo(1));
		Assert.That(dataLocator.CountAsync, Is.EqualTo(1));
		Assert.That(anchorLocator.CountAsync, Is.Zero);
		Assert.That(frameLocator.CountAsync, Is.Zero);
		Assert.That(await dataLocator.IsVisibleAsync(), Is.True);
		Assert.That(client.GetActiveTextAsync, Is.EqualTo("Read Only Text Box").After(3000, 100));
	}

	[Test]
	public async Task RichTextBoxReadOnlyValueCannotBeUpdatedFromClient()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new RichTextBox { Text = "This Rich Text Box Is Read Only", ReadOnly = true });
		var richTextBox = rendered.GetControl<RichTextBox>();

		await rendered.Find(".richtextbox").TriggerEventAsync("oncontentchanged", new ContentChangedEventArgs { Content = new EditorContent("<p>Updated Text</p>", null, 0) });
		Assert.That(richTextBox.Text, Is.EqualTo("This Rich Text Box Is Read Only"));
	}

	[Test, WithPlaywrightPage]
	public async Task ColorDialogShouldActAsModal()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBoxForTest? richTextBoxForTest = null!;
		Form? form = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form { Width = 800, Height = 500 };
			richTextBoxForTest = new RichTextBoxForTest()
			{
				Width = 800,
				Height = 500,
				Text = "Test Content",
				IsToolBarVisible = true,
			};

			form.Controls.Add(richTextBoxForTest);
			return form;
		});

		var toolbarLocator = page.Locator(".richtextbox__toolbar");
		await toolbarLocator.WaitForAsync();

		var colorButton = page.Locator("[title='Text color']");
		await colorButton.WaitForAsync();
		await colorButton.ClickAsync();

		Assert.That(() => Form.ActiveForm?.GetType(), Is.EqualTo(typeof(ColorDialog)).After(3000, 200));
		// Dispose ColorDialog to avoid System.InvalidOperationException : Attempted to dispose a WinzorDispatcher running an inner message loop.
		Form.ActiveForm?.Dispose();
	}

	[TestCaseSource(nameof(SampleTextList)), WithPlaywrightPage]
	public async Task RichTextBoxInputLargeSizeContent_ClientToServer(string largeText)
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		var page = await ctx.LoadControlOnFormAsync(() => richTextBox = new RichTextBox()
		{
			Width = 800,
			Height = 500,
		});
		var editor = page.Locator(".richtextbox__editoranchor");
		await editor.FocusAsync();
		richTextBox.ContentIsOutOfSyncWithClient = true;
		await editor.EvaluateAsync($"e => e.innerHTML = '<p>{largeText}</p>'");
		await editor.BlurAsync();
		Assert.That(() => (richTextBox.Text).Length, Is.GreaterThanOrEqualTo(largeText.Length).After(5000, 100));
	}

	[TestCaseSource(nameof(SampleTextList)), WithPlaywrightPage]
	public async Task RichTextBoxInputLargeSizeContent_ServerToClient(string largeText)
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		var page = await ctx.LoadControlOnFormAsync(() => richTextBox = new RichTextBox()
		{
			Width = 800,
			Height = 500,
			Text = largeText,
		});

		var editor = page.Locator(".richtextbox__editoranchor");
		Assert.That(async () => (await editor.InnerTextAsync()).Length, Is.GreaterThanOrEqualTo(largeText.Length).After(5000, 100));
	}

	[TestCase("MouseEvent", "contextmenu", TestName = "{m}_ContextMenu")]
	[TestCase("MouseEvent", "mousedown", TestName = "{m}_MouseDown")]
	[TestCase("KeyboardEvent", "keydown", TestName = "{m}_KeyDown")]
	[TestCase("KeyboardEvent", "keyup", TestName = "{m}_KeyUp")]
	[WithPlaywrightPage]
	public async Task RichTextBoxEventDispatchedOnRichTextBoxControl(string eventClass, string subType)
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			richTextBox = new RichTextBox() { Width = 500, Height = 500, Text = "Test", };
			form.Controls.Add(richTextBox);
			return form;
		});

		Assert.That(richTextBox, Is.Not.Null);

		var client = await RichTextBoxClient.GetClientAsync();
		await client.FocusEditorAsync();

		var result = await client.EvaluateAsync<bool>($@"client => {{
				let target = document.querySelector('.richtextbox');
				let eventClass = {eventClass};
				let subType = '{subType}';
				let options;
				switch(eventClass) {{
					case MouseEvent:
						options = {{ bubbles: true, composed: true, button: 2 }};
						break;
					case KeyboardEvent:
						options = {{ key: 'k' }};
						break;
				}}
				return new Promise((resolve, reject) => {{
					let failTimeout = setTimeout(() => reject(new Error('Failed to receive the expected event after 3 seconds')), 3000);
					target.addEventListener(subType, (e) => {{
						if (e.target !== target) {{
							return;
						}}
						for (const [key, value] of Object.entries(options)) {{
							if (e[key] !== value) {{
								return;
							}}
						}}
						clearTimeout(failTimeout);
						resolve(true);
					}});
					client.getEditorBody().dispatchEvent(new eventClass(subType, options));
				}});
			}}");

		Assert.That(result, Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxRightClickShouldNotMoveCursor()
	{
		var rnd = new Random();
		var initialX = rnd.Next(1, 15);
		var initialY = rnd.Next(1, 20);
		if (initialY < initialX)
		{
			(initialX, initialY) = (initialY, initialX);
		}

		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox()
		{
			Width = 500,
			Height = 500,
			Html = "<p>line one</p><br><p>line two</p><br><p>line three</p>",
		});

		var client = await RichTextBoxClient.GetClientAsync();
		await client.FocusEditorAsync();

		await client.EvaluateAsync($@"client => {{client.setSelection({{ start: {initialX}, end: {initialY}}});}}");

		await page.Mouse.ClickAsync(50, 50, new MouseClickOptions { Button = MouseButton.Right });

		var newPosition = await client.EvaluateAsync<int[]>(@"client => {
			let selection = client.getSelection();
			return [selection.start, selection.end];
		}");

		Assert.That(newPosition[0], Is.EqualTo(initialX));
		Assert.That(newPosition[1], Is.EqualTo(initialY));
	}

	[TestCase("mousedown", MouseButton.Left, TestName = "{m}_LeftClick")]
	[TestCase("contextmenu", MouseButton.Right, TestName = "{m}_RightClick")]
	[WithPlaywrightPage]
	public async Task RichTextBoxReDispatchedMouseEventPositionIsRelativeToForm(string eventType, MouseButton mouseButton)
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Width = 1000, Height = 1000, };
			richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Top = 111,
				Left = 222,
				Text = "Test",
			};
			form.Controls.Add(richTextBox);
			return form;
		});

		Assert.That(richTextBox, Is.Not.Null);
		var client = await RichTextBoxClient.GetClientAsync();

		await client.EvaluateAsync<int[]>(@"(client, eventName) => {
					client.root.addEventListener(eventName, e => window.eventData = [e.clientX, e.clientY]);
				}", eventType);

		await page.Mouse.ClickAsync(500, 400, new MouseClickOptions() { Button = mouseButton });
		Assert.That(async () => await page.EvaluateAsync<int[]>("window.eventData"), Is.EqualTo(new[] { 500, 400 }));
	}

	[Test]
	public async Task RichTextBoxTriggersContextMenuEvent()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			return new RichTextBox { Text = "Lorem ipsum" };
		});
		var form = rendered.GetForm();
		var richTextBox = (RichTextBox)form.Controls[0];

		await rendered.Find(".richtextbox").TriggerEventAsync("onrichtextboxcontextmenu", new RichTextBoxContextMenuEventArgs()
		{
			SelectionStart = 0,
			SelectionLength = 3
		});

		Assert.That(richTextBox.SelectionLength, Is.EqualTo(3));
		Assert.That(rendered, Is.Not.Null);
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxRightClickOpensContextMenu([Values] bool readOnly)
	{
		var clientServiceProvider = new MockCargoWiseClientSeviceProvider();
		var contextMenuOpened = new TaskCompletionSource();
		clientServiceProvider.MockMenuDisplayer
			.Setup(o => o.SendShowMenuRequestAsync(It.IsAny<MenuInteropModel>(),
				It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(),
				It.IsAny<Func<MenuClosedResult, Task>>()))
			.Callback<MenuInteropModel, Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>,
				Func<MenuClosedResult, Task>>(
				(menu, loadSubMenuCallbackAsync, menuClosedCallbackAsync) =>
				{
					if (menu.MenuType == MenuType.ContextMenu)
					{
						contextMenuOpened.SetResult();
					}
				});
		await using var ctx = new InMemoryTestServerContext(clientServiceProvider);
		RichTextBox? richTextBox = null!;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Text = "Test",
				ContextMenu = new ContextMenu(),
				ReadOnly = readOnly
			};
			form.Controls.Add(richTextBox);
			return form;
		});

		Assert.That(richTextBox, Is.Not.Null);
		var client = await RichTextBoxClient.GetClientAsync();

		if (!readOnly)
		{
			var editor = await client.GetEditorBodyAsync();
			Assert.That(editor, Is.Not.Null);
			await editor.ClickAsync(new() { Button = MouseButton.Right });
		}
		else
		{
			var readOnlyBody = await client.GetActiveHtmlAsync();
			Assert.That(readOnlyBody, Is.Not.Null);
			await page.ClickAsync(".richtextbox__data", new PageClickOptions { Button = MouseButton.Right });
			Assert.That(await contextMenuOpened.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		}

		Assert.That(await contextMenuOpened.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxRightClickOpensContextMenuOnReadOnlyMode()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;

		var page = await ctx.LoadControlOnFormAsync(() => richTextBox = new RichTextBox
		{
			Width = 500,
			Height = 500,
			Text = "Test",
			ReadOnly = true,
			IsToolBarVisible = true
		});

		var readOnlyBody = page.Locator(".richtextbox__data");
		await readOnlyBody.WaitForAsync();
		await richTextBox.InvokeWinzorDispatcherAsync(() => richTextBox.SelectAll());
		await page.WaitForFunctionAsync(@"() => window.getSelection().toString().length > 0;");

		var client = await RichTextBoxClient.GetClientAsync();
		var (selectionStart, selectionEnd) = await client.GetActiveSelectionAsync();
		Assert.That(selectionStart, Is.EqualTo(0));
		Assert.That(selectionEnd, Is.EqualTo(4));

		await page.AttachMockClipboardWrite();
		await richTextBox.InvokeWinzorDispatcherAsync(() => richTextBox.Copy());

		var clipboardPlain = await page.GetClipboardPlain();

		Assert.That(() => clipboardPlain, Is.EqualTo("Test").After(1000, 100));
	}

	[WithPlaywrightPage]
	[TestCase(false, "Tab", "nothing", "nothing")]
	[TestCase(true, "Tab", "nothing", "no\tthing")]
	[TestCase(false, "Shift+Tab", "nothing", "nothing")]
	public async Task RichTextBoxPressTabMoveToNextElementOrPerformIndent(bool acceptsTab, string tabKey,
		string textBefore, string textAfter)
	{
		RichTextBox? richTextBox = null!;
		await using var ctx = new InMemoryTestServerContext();
		await ctx.LoadControlOnFormAsync(() => richTextBox = new RichTextBox()
		{
			Width = 500,
			Height = 500,
			AcceptsTab = acceptsTab,
			Text = textBefore
		});

		var client = await RichTextBoxClient.GetClientAsync();
		var editor = await client.GetEditorBodyAsync();

		await editor.FocusAsync();
		await editor.PressAsync("ArrowRight");
		await editor.PressAsync("ArrowRight");
		await editor.PressAsync(tabKey);

		Assert.That(() => client.GetEditorTextAsync(), Is.EqualTo(textAfter).After(3000, 100));
		Assert.That(() => richTextBox.Focused, Is.EqualTo(acceptsTab).After(3000, 100));
	}

	[TestCase(BorderStyle.Fixed3D, "richtextbox--fixed3d")]
	[TestCase(BorderStyle.FixedSingle, "richtextbox--fixedsingle")]
	[TestCase(BorderStyle.None, "richtextbox--none")]

	public async Task RichTextBoxShouldHaveBorderStyle(BorderStyle borderStyle, string expectedClass)
	{
		using var ctx = new WinzorTestContext();

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var richTextBox = new RichTextBox()
			{
				Text = "This is the rich text box example text",
				BorderStyle = borderStyle,
			};
			form.Controls.Add(richTextBox);
			return form;
		});

		var rickTextBoxEle = rendered.Find(".richtextbox");

		Assert.That(rickTextBoxEle.ClassList, Does.Contain(expectedClass));
	}

	[WithPlaywrightPage]
	[TestCase(BorderStyle.Fixed3D, ".richtextbox--fixed3d.richtextbox--readonly .richtextbox__data", "rgba(125, 125, 125, 0.8) rgba(237, 237, 237, 0.1) rgba(237, 237, 237, 0.2) rgba(125, 125, 125, 0.8)", "ridge groove groove ridge", true, TestName = "{m}_Readonly_Fixed3d")]
	[TestCase(BorderStyle.FixedSingle, ".richtextbox--fixedsingle.richtextbox--readonly .richtextbox__data", "rgba(125, 125, 125, 0.8) rgba(237, 237, 237, 0.1) rgba(237, 237, 237, 0.2) rgba(125, 125, 125, 0.8)", "ridge groove groove ridge", true, TestName = "{m}_Readonly_FixedSingle")]
	[TestCase(BorderStyle.None, ".richtextbox--none", "rgb(0, 0, 0)", "none", true, TestName = "{m}_Readonly_None")]
	[TestCase(BorderStyle.Fixed3D, ".richtextbox--fixed3d .richtextbox__editoranchor", "rgba(125, 125, 125, 0.8) rgba(237, 237, 237, 0.1) rgba(237, 237, 237, 0.2) rgba(125, 125, 125, 0.8)", "ridge groove groove ridge", false, TestName = "{m}_EditorAnchor_Fixed3d")]
	[TestCase(BorderStyle.FixedSingle, ".richtextbox--fixedsingle .richtextbox__editoranchor", "rgba(125, 125, 125, 0.8) rgba(237, 237, 237, 0.1) rgba(237, 237, 237, 0.2) rgba(125, 125, 125, 0.8)", "ridge groove groove ridge", false, TestName = "{m}_EditorAnchor_FixedSingle")]
	public async Task RichTextBorderStyleShouldHaveCss(BorderStyle borderStyle, string expectedClass, string expectedBorderColor, string expectedBorderStyle, bool isReadonly)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			RichTextBox richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Text = "Test",
				BorderStyle = borderStyle,
				ReadOnly = isReadonly
			};
			return richTextBox;
		});

		var rtbInstance = page.Locator(expectedClass);
		Assert.That(await rtbInstance.GetComputedStyleAsync("border-color"), Is.EqualTo(expectedBorderColor));
		Assert.That(await rtbInstance.GetComputedStyleAsync("border-style"), Is.EqualTo(expectedBorderStyle));
	}

	[WithPlaywrightPage]
	[TestCase(true, new[] { 1, 1, 1, 1 }, "ridge groove groove ridge")]
	[TestCase(false, new[] { 1, 1, 1, 1 }, "ridge groove groove ridge")]
	public async Task RichTextBoxBorderStyleWhenReadonly(bool isReadonly, int[] borderWidth, string borderStyle)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var richTextBox = new RichTextBox
			{
				Width = 500,
				Height = 500,
				Text = "Test",
				BorderStyle = BorderStyle.Fixed3D,
				ReadOnly = isReadonly
			};

			return richTextBox;
		});

		ILocator rtbLocator;
		if (isReadonly)
		{
			rtbLocator = page.Locator(".richtextbox.richtextbox--readonly .richtextbox__data");
		}
		else
		{
			rtbLocator = page.Locator(".richtextbox .richtextbox__editoranchor");
		}

		Assert.That(await rtbLocator.GetComputedStyleAsync("border-style"), Is.EqualTo(borderStyle));
		Assert.That((await rtbLocator.GetComputedStyleAsync("border-top-width")).AsPixels(), Is.EqualTo(borderWidth[0]).Within(0.1));
		Assert.That((await rtbLocator.GetComputedStyleAsync("border-right-width")).AsPixels(), Is.EqualTo(borderWidth[1]).Within(0.1));
		Assert.That((await rtbLocator.GetComputedStyleAsync("border-bottom-width")).AsPixels(), Is.EqualTo(borderWidth[2]).Within(0.1));
		Assert.That((await rtbLocator.GetComputedStyleAsync("border-left-width")).AsPixels(), Is.EqualTo(borderWidth[3]).Within(0.1));
	}

	[Test, WithPlaywrightPage]
	public async Task TestDetachFromRendererDoesNotThrowException()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBoxForTest? richTextBox = null!;
		var mockInterop = new Mock<IRichTextBoxJSInterop>();
		mockInterop.Setup(i => i.DeleteRichTextBoxClientAsync(It.IsAny<string>())).Throws(new JSDisconnectedException("JSDisconnected"));

		await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			richTextBox = new RichTextBoxForTest()
			{
				Width = 500,
				Height = 500,
				Text = "Test content",
			};
			richTextBox.InteropForTest = (IRichTextBoxJSInterop?)mockInterop.Object;
			form.Controls.Add(richTextBox);
			return form;
		});

		Assert.DoesNotThrowAsync(async () => await richTextBox.DetachFromRendererAsync());
	}

	[Test, WithPlaywrightPage]
	public async Task TestDetachFromRendererDoesNotThrowDisposedException()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBoxForTest? richTextBox = null!;
		var mockInterop = new Mock<IRichTextBoxJSInterop>();
		mockInterop.Setup(i => i.DeleteRichTextBoxClientAsync(It.IsAny<string>())).Throws(new ObjectDisposedException("obj"));

		await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			richTextBox = new RichTextBoxForTest()
			{
				Width = 500,
				Height = 500,
				Text = "Test content",
			};
			richTextBox.InteropForTest = (IRichTextBoxJSInterop?)mockInterop.Object;
			form.Controls.Add(richTextBox);
			return form;
		});

		Assert.DoesNotThrowAsync(async () => await richTextBox.DetachFromRendererAsync());
	}

	[Test]
	public async Task TextAreaSelectionchangeEventIsMissing()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new RichTextBox());

		var textArea = rendered.Find(".richtextbox");

		Assert.That(async () => await textArea.TriggerEventAsync("ontextboxselectionchange", new TextboxSelectionChangeEventArgs { }), Throws.TypeOf<MissingEventHandlerException>());
	}

	[TestCase(BorderStyle.None, 0)]
	[TestCase(BorderStyle.FixedSingle, 1)]
	[TestCase(BorderStyle.Fixed3D, 1)]

	public async Task TestBorderStyleAdjustForClientSize(BorderStyle bs, int borderSize)
	{
		using var ctx = new WinzorTestContext();
		RichTextBox? richTextBox = null!;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Size = new Size(500, 500);
			richTextBox = new RichTextBox();
			form.Controls.Add(richTextBox);
			richTextBox.Location = new Point(10, 10);
			richTextBox.Size = new Size(200, 50);

			richTextBox.BorderStyle = bs;

			return form;
		});

		var b = richTextBox.Bounds;
		Assert.That(b.X, Is.EqualTo(10));
		Assert.That(b.Y, Is.EqualTo(10));
		Assert.That(richTextBox.ClientSize.Width + 2 * borderSize, Is.EqualTo(b.Width));
		Assert.That(richTextBox.ClientSize.Height + 2 * borderSize, Is.EqualTo(b.Height));
		Assert.That(richTextBox.ClientAreaBounds.X, Is.EqualTo(b.X + borderSize));
		Assert.That(richTextBox.ClientAreaBounds.Y, Is.EqualTo(b.Y + borderSize));
	}

	[TestCase(BorderStyle.None, BorderStyle.FixedSingle, true, 2)]
	[TestCase(BorderStyle.None, BorderStyle.Fixed3D, true, 2)]
	[TestCase(BorderStyle.None, BorderStyle.None, false, 0)]
	public async Task BorderStyleChangeTriggersRecalculation(BorderStyle initial, BorderStyle bs, bool expectChange, int expectedDiff)
	{
		using var ctx = new WinzorTestContext();
		RichTextBox? richTextBox = null!;

		bool clientSizeChanged = false;
		int clientSizeDiff = 0;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Size = new Size(500, 500);
			richTextBox = new RichTextBox();
			form.Controls.Add(richTextBox);
			richTextBox.Location = new Point(10, 10);
			richTextBox.Size = new Size(200, 50);

			richTextBox.BorderStyle = BorderStyle.None;

			var clientSizeBefore = richTextBox.ClientSize;
			richTextBox.ClientSizeChanged += (sender, e) =>
			{
				clientSizeChanged = true;
				clientSizeDiff = clientSizeBefore.Width - richTextBox.ClientSize.Width;
			};

			return form;
		});

		clientSizeChanged = false;

		await richTextBox.InvokeWinzorDispatcherAsync(() =>
		{
			richTextBox.BorderStyle = bs;
		});

		Assert.That(clientSizeChanged, Is.EqualTo(expectChange));
		Assert.That(clientSizeDiff, Is.EqualTo(expectedDiff));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxPasteImageFiresOnImageInserted()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;
		var imageInsertedFired = new TaskCompletionSource<string[]>();

		await ctx.LoadControlOnFormAsync(() =>
		{
			richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Text = "Test content",
			};
			richTextBox.ImagesInserted += (sender, args) => imageInsertedFired.SetResult(args);
			return richTextBox;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		var editor = await client.GetEditorBodyAsync();
		Assert.That(editor, Is.Not.Null);
		await client.EvaluateAsync("client => client.setSelection({ start: 5, end: 8 });");
		Assert.That(client.GetActiveSelectionAsync, Is.EqualTo((5, 8)));
		Assert.That(richTextBox.SelectionStart, Is.EqualTo(0));
		Assert.That(richTextBox.SelectionLength, Is.EqualTo(0));

		var imageName = "testImage.png";

		await editor.EvaluateAsync(@$"async editor => {{
const imageData = 'data:image/png;base64, {TestImage.GetImageBase64}';
const response = await fetch(imageData);
const blob = await response.blob();
const file = new File([blob], '{imageName}', {{type: 'image/png'}});
const dataTransfer = new DataTransfer();
dataTransfer.items.add(file);

const pasteEvent = new ClipboardEvent('paste', {{
	bubbles: true,
	cancelable: true,
	clipboardData: dataTransfer,
 }});
editor.dispatchEvent(pasteEvent);
}}");

		Assert.That(await imageInsertedFired.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);

		var uploadedFiles = await imageInsertedFired.Task;
		Assert.That(uploadedFiles.Count, Is.EqualTo(1));
		Assert.That(File.Exists(uploadedFiles.First()), Is.True);
		Assert.That(Path.GetFileName(uploadedFiles.First()), Is.EqualTo(imageName));
		Assert.That(richTextBox.SelectionStart, Is.EqualTo(5));
		Assert.That(richTextBox.SelectionLength, Is.EqualTo(3));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxDropImageFiresOnImageInserted()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;
		var imageInsertedFired = new TaskCompletionSource<string[]>();

		await ctx.LoadControlOnFormAsync(() =>
		{
			richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Text = "Test content",
			};
			richTextBox.ImagesInserted += (sender, args) => imageInsertedFired.SetResult(args);
			return richTextBox;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		var editor = await client.GetEditorBodyAsync();

		Assert.That(editor, Is.Not.Null);
		await client.EvaluateAsync("client => client.setSelection({ start: 5, end: 8 });");
		Assert.That(client.GetActiveSelectionAsync, Is.EqualTo((5, 8)));
		Assert.That(richTextBox.SelectionStart, Is.EqualTo(0));
		Assert.That(richTextBox.SelectionLength, Is.EqualTo(0));

		var imageName = "testImage.png";

		await editor.EvaluateAsync(@$"async editor => {{
const imageData = 'data:image/png;base64, {TestImage.GetImageBase64}';
const response = await fetch(imageData);
const blob = await response.blob();
const file = new File([blob], '{imageName}', {{type: 'image/png'}});
const dataTransfer = new DataTransfer();
dataTransfer.items.add(file);

const dropEvent = new DragEvent('drop', {{
	bubbles: true,
	cancelable: true,
	dataTransfer: dataTransfer,
 }});
editor.dispatchEvent(dropEvent);
}}");

		Assert.That(await imageInsertedFired.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);

		var uploadedFiles = await imageInsertedFired.Task;
		Assert.That(uploadedFiles.Count, Is.EqualTo(1));
		Assert.That(File.Exists(uploadedFiles.First()), Is.True);
		Assert.That(Path.GetFileName(uploadedFiles.First()), Is.EqualTo(imageName));
		Assert.That(richTextBox.SelectionStart, Is.EqualTo(5));
		Assert.That(richTextBox.SelectionLength, Is.EqualTo(3));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxSetSelectionCorrectlyWhileTyping()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		var page = await ctx.LoadControlOnFormAsync(() => richTextBox = new RichTextBox());

		var client = await RichTextBoxClient.GetClientAsync();
		await client.FocusEditorAsync();
		await page.Keyboard.TypeAsync("Test Content");

		Assert.That(client.GetActiveSelectionAsync, Is.EqualTo((12, 12)));

		await richTextBox.InvokeWinzorDispatcherAsync(() =>
		{
			richTextBox.SelectionStart = 5;
			richTextBox.SelectionLength = 3;
		});

		Assert.That(client.GetActiveSelectionAsync, Is.EqualTo((5, 8)));
	}

	public static object[] CaretToDomSelectionConversionCases =
	{
		new object[] { "<p id=\"a\">a</p>", 0, 1, "#a", 0, "#a>text", 1 },
		new object[] { "<p id=\"a\">a</p><p id=\"b\">b</p>", 1, 1, "#a>text", 1, "#b", 0 },
		new object[] { "<p id=\"a\">a</p><p id=\"b\">b</p>", 2, 1, "#b", 0, "#b>text", 1 },
		new object[] { "<ul><li id=\"a\">a</li></ul>", 0, 1, "#a", 0, "#a>text", 1 },
		new object[] { "<ul><li id=\"a\">a</li></ul><ul><li id=\"b\">b</li></ul>", 1, 1, "#a>text", 1, "#b", 0 },
		new object[] { "<ul><li id=\"a\">a</li></ul><ol><li id=\"b\">b</li></ol>", 1, 1, "#a>text", 1, "#b", 0 },
		new object[] { "<ol><li id=\"a\">a</li></ol><ul><li id=\"b\">b</li></ul>", 1, 1, "#a>text", 1, "#b", 0 },
		new object[] { "<ul><li id=\"a\">a</li><li id=\"b\">b</li></ul></li>", 1, 1, "#a>text", 1, "#b", 0 },
		new object[] { "<ul><li id=\"a\"><ul><li id=\"b\">b</li></ul></li></ul>", 1, 1, "#b", 0, "#b>text", 1 },
		new object[] { "<ul><li id=\"a\"><ul><li id=\"b\">b</li></ul></li></ul><p id=\"c\">c</p>", 1, 2, "#b", 0, "#c", 0 },
		new object[] { "<p id=\"a\">a</p><ul><li id=\"b\"><ul><li id=\"c\">c</li></ul></li></ul>", 1, 2, "#a>text", 1, "#c", 0 },
		new object[] { "<p id=\"a\">a</p><ul><li id=\"b\">b</li></ul>", 1, 1, "#a>text", 1, "#b", 0 },
		new object[] { "<ul><li id=\"a\">a</li></ul><p id=\"b\">b</p>", 1, 1, "#a>text", 1, "#b", 0 },
		new object[] { "<p id=\"a\"><br></p><p id=\"b\">b</p>", 1, 1, "#b", 0, "#b>text", 1 },
		new object[] { "<p id=\"a\"><em><br></em></p><p id=\"b\">b</p>", 1, 1, "#b", 0, "#b>text", 1 },
		new object[] { "<p id=\"a\"><br><br></p><p id=\"b\">b</p>", 1, 1, "#a", 1, "#b", 0 },
		new object[] { "<p id=\"a\">a</p>", 2, 1, "body", 1, "body", 1 },
		new object[] { "", 2, 1, "body", 0, "body", 0 },
		new object[] { "<p id=\"a\">abc</p>", 1, 1, "#a>text", 1, "#a>text", 2 },
		new object[] { "abc", 1, 1, "body>text", 1, "body>text", 2 }, // the selection finder doesn't currently allow for direct text properly due to needing to start in p or li where possible
	};

	[TestCaseSource(nameof(CaretToDomSelectionConversionCases))]
	[WithPlaywrightPage]
	public async Task SelectionFinderCorrectlyConvertsSelectionFromCaretToDom(string html, int selectionStart,
		int selectionLength, string startSelector, int startOffset, string endSelector, int endOffset)
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var richTextBox = new RichTextBox() { Width = 500, Height = 500, Text = "", ReadOnly = true };

			form.Controls.Add(richTextBox);
			return form;
		});

		await page.WaitForFunctionAsync("controlId => typeof(WTG) !== 'undefined' && typeof(WTG.SelectionFinder) !== 'undefined'"); // wait for the script to load

		var data = await page.EvaluateAsync<string[]>($@"client => {{
				document.body.innerHTML = '{html}';
				let range = new WTG.SelectionFinder(document.body).DOMRangeFor({{start: '{selectionStart}', end: '{selectionStart + selectionLength}'}});
				let prefix = (prefix, value) => value ? prefix + value : '';
				let basicSelector = (element) => prefix('#', element.id) || element.tagName.toLowerCase();
				let [startSelector, endSelector] = [range.startContainer, range.endContainer]
					.map(container =>
						container.nodeType === Node.TEXT_NODE ?
							basicSelector(container.parentNode) + '>text'
							: basicSelector(container)
					);
				return [startSelector, range.startOffset, endSelector, range.endOffset];
			}}");

		Assert.That(data, Is.EquivalentTo(new[] { startSelector, startOffset.ToString(), endSelector, endOffset.ToString() }));
	}

	public static object[] DomToCaretSelectionConversionCases =
	{
		new object[] { "", "body", 0, "body", 0, 0, 0 },
		// new object[] { "", "body", 1, "body", 1, 0, 0 }, native JS is refusing set up the range here since it's empty text so this test case is moot
		new object[] { "<p id=\"a\">a</p>", "body", 0, "#a>text", 1, 0, 1 },
		new object[] { "<p id=\"a\">a</p>", "#a", 0, "#a>text", 1, 0, 1 },
		new object[] { "<p id=\"a\">a</p>", "#a>text", 0, "#a>text", 1, 0, 1 },
		new object[] { "<p id=\"a\">a</p>", "body", 0, "#a", 1, 0, 1 },
		new object[] { "<p id=\"a\">a</p>", "body", 0, "body", 1, 0, 1 },
		new object[] { "<p id=\"a\">a</p>", "body", 1, "body", 1, 1, 0 },
	};

	[TestCaseSource(nameof(DomToCaretSelectionConversionCases))]
	[WithPlaywrightPage]
	public async Task SelectionFinderCorrectlyConvertsSelectionFromDomToCaret(string html, string startContainerSelector, int startOffset, string endContainerSelector, int endOffset, int selectionStart, int selectionLength)
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;
		var expectedSelectionTask = new TaskCompletionSource<bool>();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();

			richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Text = "",
				ReadOnly = true,
			};

			form.Controls.Add(richTextBox);
			return form;
		});

		await page.WaitForFunctionAsync("controlId => typeof(WTG) !== 'undefined' && typeof(WTG.SelectionFinder) !== 'undefined'"); // wait for the script to load

		var result = await page.EvaluateAsync<int[]>(@$"() => {{
				document.body.innerHTML = '{html}';
				let range = new Range();
				let [startContainer, endContainer] = ['{startContainerSelector}', '{endContainerSelector}']
					.map(selector => selector.replace('/', 'body'))
					.map(selector =>
						selector.indexOf('>text') !== -1 ?
							document.querySelector(selector.split('>text')[0]).childNodes[0]
							: document.querySelector(selector)
					);
				range.setStart(startContainer, {startOffset});
				range.setEnd(endContainer, {endOffset});
				let {{start, end}} = new WTG.SelectionFinder(document.body).caretRangeFor(range);
				return [start, end];
			}}");

		Assert.That(result, Is.EquivalentTo(new[] { selectionStart, selectionStart + selectionLength }));
	}

	[SuppressMessage("CargoWiseOne", "CW1104:DoNotUseSystemWindowsTabControl", Justification = "Testing")]
	[Test, WithPlaywrightPage]
	public async Task RichTextBoxEditorHasCorrectContentAfterSwitchingBackToTab()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var tabControl = new TabControl();
			var mainTabPage = new TabPage { Name = "RichTextBox", Text = "RichTextBox Tab" };
			var richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Text = "Test content"
			};
			mainTabPage.Controls.Add(richTextBox);
			var stubTabPage = new TabPage { Name = "Stub", Text = "Stub Tab" };
			tabControl.TabPages.Add(mainTabPage);
			tabControl.TabPages.Add(stubTabPage);

			form.Controls.Add(tabControl);
			return form;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		Assert.That(client.GetActiveTextAsync, Is.EqualTo("Test content"));

		var stubButton = await page.Locator("button", new PageLocatorOptions() { HasTextString = "Stub Tab" }).ElementHandleAsync();
		var richTextBoxButton = await page.Locator("button", new PageLocatorOptions() { HasTextString = "RichTextBox Tab" }).ElementHandleAsync();

		await stubButton.EvaluateAsync("b => b.click()");
		await richTextBoxButton.EvaluateAsync("b => b.click()");

		Assert.That(client.GetActiveTextAsync, Is.EqualTo("Test content").After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxLoadsEvenIfNoContentIsSet()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
			};
			form.Controls.Add(richTextBox);
			return form;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		Assert.That(client.GetEditorTextAsync, Is.Not.Null);
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxBackgroundIsTransparent()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				BackColor = Color.Blue,
			};

			form.Controls.Add(richTextBox);
			return form;
		});

		var client = await RichTextBoxClient.GetClientAsync();

		var initialValue = await client.EvaluateAsync<string>("client => window.getComputedStyle(client.getEditorBody()).getPropertyValue('background-color')");
		Assert.That(initialValue, Is.EqualTo("rgba(0, 0, 0, 0)"));

		await richTextBox.InvokeWinzorDispatcherAsync(() => richTextBox.BackColor = Color.Red);

		Assert.That(async () => await client.EvaluateAsync<string>("client => window.getComputedStyle(client.getEditorBody()).getPropertyValue('background-color')"), Is.EqualTo("rgba(0, 0, 0, 0)").After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxAcceptsTabUpdatesOnClient()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				AcceptsTab = true,
			};

			form.Controls.Add(richTextBox);
			return form;
		});

		var client = await RichTextBoxClient.GetClientAsync();

		Assert.That(await client.EvaluateAsync<bool>("client => client.acceptsTab"), Is.True);

		await richTextBox.InvokeWinzorDispatcherAsync(() => richTextBox.AcceptsTab = false);

		Assert.That(async () => await client.EvaluateAsync<bool>("client => client.acceptsTab"), Is.False.After(3000, 100));
	}

	[TestCase("selectionchange")]
	[TestCase("input")]
	[TestCase("Undo")]
	[TestCase("Redo")]
	[WithPlaywrightPage]
	public async Task RichTextBoxClientEventsDoesNotTriggerServerEvents(string eventName)
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;
		var textChangeEvent = false;
		var selectionChangeEvent = false;

		await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Text = "foo",
			};

			richTextBox.TextChanged += (s, e) =>
			{
				if (richTextBox.Text == "bar")
				{
					textChangeEvent = true;
				}
			};

			richTextBox.SelectionChanged += (s, e) =>
			{
				if (richTextBox.SelectionStart == 0 && richTextBox.SelectionLength == 3)
				{
					selectionChangeEvent = true;
				}
			};

			form.Controls.Add(richTextBox);
			return form;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		Assert.That(client.GetActiveTextAsync, Is.EqualTo("foo"));

		await client.EvaluateAsync($@"client => {{
				client.setContent('<p>bar</p>');
				client.setSelection({{ start: 0, end: 3}});
				client.getEditorBody().dispatchEvent(new Event('{eventName}'));
			}}");

		Assert.That(() => selectionChangeEvent, Is.False.After(3000, 100));
		Assert.That(() => textChangeEvent, Is.False.After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxOnContentChangedAsyncEvent()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;

		await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Html = WrapInP("Empty"),
			};

			form.Controls.Add(richTextBox);
			return form;
		});

		richTextBox.ContentIsOutOfSyncWithClient = false;
		await richTextBox.OnContentChangedAsync(new ContentChangedEventArgs { Content = new EditorContent(WrapInP("Updated Text"), null, 0) });
		Assert.That(richTextBox.Html, Is.EqualTo(WrapInP("Empty")));

		richTextBox.ContentIsOutOfSyncWithClient = true;
		await richTextBox.OnContentChangedAsync(new ContentChangedEventArgs { Content = new EditorContent(WrapInP("Updated Text"), null, 0) });
		Assert.That(richTextBox.Html, Is.EqualTo(WrapInP("Updated Text")));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxOnContentChangedAsyncReportsDeveloperExceptionWhenSyncContentTimeout()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;
		var tcsException = new TaskCompletionSource<Exception?>();
		_ = await ctx.LoadControlOnFormAsync(() =>
		{
			return richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Html = "<p>Empty</p>",
			};
		});

		var streamMock = new Mock<Stream>();
		_ = streamMock.Setup(s => s.CanRead).Returns(true);
		_ = streamMock.Setup(s => s.ReadAsync(It.IsAny<Memory<byte>>(), It.IsAny<CancellationToken>())).ThrowsAsync(new TimeoutException("Timeout happened."));

		var jsStreamRefMock = new Mock<IJSStreamReference>();
		_ = jsStreamRefMock.Setup(s => s.OpenReadStreamAsync(It.IsAny<long>(), It.IsAny<CancellationToken>())).ReturnsAsync(streamMock.Object);

		Application.DeveloperException += Application_DeveloperException;
		try
		{
			await richTextBox.OnContentChangedAsync(new ContentChangedEventArgs { Content = new EditorContent(null, jsStreamRefMock.Object, 100) });

			Assert.That(await tcsException.Task, Is.TypeOf<TimeoutException>());
		}
		finally
		{
			Application.DeveloperException -= Application_DeveloperException;
		}

		void Application_DeveloperException(object? sender, string? message, Exception? ex)
		{
			tcsException.SetResult(ex);
		}
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxTableCellsAreUsable()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox()
		{
			Html = "<table><tbody><tr><td></td></tr></tbody></table>",
			IsToolBarVisible = true,
		});

		var table = page.Locator(".richtextbox__editoranchor table");
		var tableCell = page.Locator(".richtextbox__editoranchor table td");
		Assert.That((await table!.BoundingBoxAsync())!.Height, Is.GreaterThanOrEqualTo(10), "Table height");
		Assert.That((await tableCell.GetComputedStyleAsync("border-width")).AsPixels(), Is.EqualTo(1), "Border width");
		Assert.That(await tableCell.GetComputedStyleAsync("border-color"), Is.EqualTo("rgb(0, 0, 0)"), "Border color");
		Assert.That(await tableCell.GetComputedStyleAsync("border-style"), Is.EqualTo("solid"), "Border style");
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxTableCellsHaveBackgroundColor()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		var page = await ctx.LoadControlOnFormAsync(() => richTextBox = new RichTextBox()
		{
			Html = EnumerableTree.Generator(b => b
			.Open<Paragraph>()
			.Open(new Phrase
			{
				BackgroundColor = new DomColor
				{
					Red = 255
				}
			})
			.Add("Markup")
			.Close()
		.Close()
		.Open(new Paragraph())
			.Open(new Phrase())
				.Add(new LineBreak())
				.Add(new LineBreak())
			.Close()
		.Close()
			.Open(new Table())
		.Open(new TableRow())
			.Open(new TableCell
			{
				Color = new DomColor
				{
					Red = 255,
					Green = 165
				}
			})
				.Open(new Paragraph())
					.Open(new Phrase())
						.Add("Cell 1")
					.Close()
				.Close()
			.Close()
			.Open(new TableCell
			{
				Color = new DomColor
				{
					Green = 128
				}
			})
				.Open(new Paragraph())
					.Open(new Phrase())
						.Add("Cell 2")
					.Close()
				.Close()
			.Close()
		.Close()
		.Open(new TableRow())
			.Open(new TableCell
			{
				Color = new DomColor
				{
					Red = 255,
					Green = 192,
					Blue = 203
				}
			})
				.Open(new Paragraph())
					.Open(new Phrase())
						.Add("Cell 3")
					.Close()
					.Close()
					.Close()
					.Close()
					.Close()
				.End()
			)
			.Reduce(HtmlEncoder.CreateFactory())
			.Markup(),
		});

		var textLocator = page.GetByText("Markup").First;
		var tableCellLocator1 = page.GetByRole(AriaRole.Cell).Filter(new() { HasText = "Cell 1" });
		var tableCellLocator2 = page.GetByRole(AriaRole.Cell).Filter(new() { HasText = "Cell 2" });
		var tableCellLocator3 = page.GetByRole(AriaRole.Cell).Filter(new() { HasText = "Cell 3" });

		await Assertions.Expect(textLocator).ToHaveCSSAsync("background-color", "rgb(255, 0, 0)");

		await Assertions.Expect(tableCellLocator1).ToHaveCSSAsync("background-color", "rgb(255, 165, 0)");

		await Assertions.Expect(tableCellLocator2).ToHaveCSSAsync("background-color", "rgb(0, 128, 0)");

		await Assertions.Expect(tableCellLocator3).ToHaveCSSAsync("background-color", "rgb(255, 192, 203)");
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxRendersBlankLines([Values] bool readOnly)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox()
		{
			Html = "<p>Test</p><p></p><p>Test</p>",
			IsToolBarVisible = false,
			ReadOnly = readOnly
		});

		var client = await RichTextBoxClient.GetClientAsync();
		var blankLine = page.Locator(".richtextbox p").Nth(1);
		Assert.That(blankLine, Is.Not.Null);
		Assert.That((await blankLine!.BoundingBoxAsync())!.Height, Is.GreaterThanOrEqualTo(10), "Line height");
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxWithoutToolbarGeneratesParagraphs()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form? form = null!;
		TextBox? textBox = null!;
		RichTextBox? richTextBox = null!;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			textBox = new TextBox();
			richTextBox = new RichTextBox() { IsToolBarVisible = false, Top = 20 };
			form.Controls.Add(textBox);
			form.Controls.Add(richTextBox);
			return form;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();

		await page.Keyboard.TypeAsync("Line 1");
		await page.Keyboard.PressAsync("Enter");
		await page.Keyboard.TypeAsync("Line 2");
		await page.Keyboard.PressAsync("Shift+Enter");
		await page.Keyboard.TypeAsync("Line 3");
		await page.Keyboard.PressAsync("Shift+Enter");

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			_ = textBox.Focus();
		});

		Assert.That(await editorBody.InnerHTMLAsync(), Is.EqualTo("<p>Line 1</p><p>Line 2<br>Line 3<br><br></p>"));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxUnderstandsTabs()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;
		var page = await ctx.LoadFormAsync(() =>
		{
			Form form = new Form();
			richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Text = "a\t\tb",
				AcceptsTab = true
			};
			form.Controls.Add(richTextBox);
			return form;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		Assert.That(editorBody.InnerHTMLAsync, Is.EqualTo("<p>a		b</p>").After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxUnderstandsMicrosoftOfficeTabs()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox() { IsToolBarVisible = true, });

		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();
		await page.MockClipboardRead(new[] {
			new JSClipboardData("text/html", "<span style='mso-tab-count:1'>     </span>"),
			new JSClipboardData("text/plain", "Not requested")
		});
		await editorBody.PressAsync("Control+KeyV");

		Assert.That(await editorBody.InnerHTMLAsync(), Is.EqualTo("<p>\t</p>"));

		await editorBody.PressAsync("Enter");

		await page.MockClipboardRead(new[] {
			new JSClipboardData("text/html", "<span style='mso-tab-count:1'>     </span><span style='mso-tab-count:1'>     </span>"),
			new JSClipboardData("text/plain", "Not requested")
		});
		await editorBody.PressAsync("Control+KeyV");

		Assert.That(await editorBody.InnerHTMLAsync(), Is.EqualTo("<p>\t</p><p>\t\t</p>"));

		await editorBody.PressAsync("Enter");

		await page.MockClipboardRead(new[] {
			new JSClipboardData("text/html", "<span style='mso-tab-count:2'>          </span>"),
			new JSClipboardData("text/plain", "Not requested")
		});
		await editorBody.PressAsync("Control+KeyV");

		Assert.That(await editorBody.InnerHTMLAsync(), Is.EqualTo("<p>\t</p><p>\t\t</p><p>\t\t</p>"));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxUnderstandsMicrosoftOfficeTabs_WithTrailingTabs()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox() { IsToolBarVisible = true, });

		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();
		await page.MockClipboardRead(new[] {
			new JSClipboardData("text/html", "\r\n\r\n<p class=\"MsoNormal\"><span style=\"mso-tab-count:1\">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; </span>12312<span style=\"mso-tab-count:1\">&nbsp;&nbsp;&nbsp;&nbsp; </span>11111<span style=\"mso-tab-count:2\">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; </span><o:p></o:p></p>"),
			new JSClipboardData("text/plain", "\t12312\t11111\t\t")
		});
		await editorBody.PressAsync("Control+Shift+KeyV");

		Assert.That(await editorBody.InnerHTMLAsync(), Is.EqualTo("<p>\t12312\t11111\t\t</p>"), "Round 1");

		await editorBody.PressAsync("Control+A");
		await editorBody.PressAsync("Delete");
		await editorBody.PressAsync("Control+KeyV");

		Assert.That(await editorBody.InnerHTMLAsync(), Is.EqualTo("<p>\t12312\t11111\t\t</p>"), "Round 2");
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxUnderstandsMicrosoftOfficeTabs_WithTrailingParagraphs()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox() { IsToolBarVisible = true, });

		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();
		await page.MockClipboardRead(new[] {
			new JSClipboardData("text/html", "<!--StartFragment-->\r\n\r\n<p class=MsoNormal>Something<o:p></o:p></p>\r\n\r\n<p class=MsoNormal><span style='mso-tab-count:1'>               </span><o:p></o:p></p>\r\n\r\n<p class=MsoNormal><span style='mso-tab-count:1'>               </span>One tab\r\nwith words.<o:p></o:p></p>\r\n\r\n<p class=MsoNormal><span style='mso-tab-count:1'>               </span>One tab<span\r\nstyle='mso-tab-count:1'>             </span>another tab.<o:p></o:p></p>\r\n\r\n<p class=MsoNormal><span style='mso-tab-count:2'>                              </span>Two\r\ntabs with words.<o:p></o:p></p>\r\n\r\n<!--EndFragment-->")
		});
		await editorBody.PressAsync("Control+KeyV");

		Assert.That(await editorBody.InnerHTMLAsync(), Is.EqualTo("<p>Something</p><p>\t</p><p>\tOne tab with words.</p><p>\tOne tab\tanother tab.</p><p>\t\tTwo tabs with words.</p>"));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxTabsAreCorrectWidth([Values] bool readOnly)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox()
		{
			Width = 500,
			Height = 500,
			Font = new Font(FontFamily.GenericSansSerif, 10, GraphicsUnit.Point),
			Html = "<p><span style=\"text-decoration: underline;\">\tabc</span></p>",
			AcceptsTab = true,
			IsToolBarVisible = true,
			ReadOnly = readOnly,
		});
		var paragraph = page.Locator(".richtextbox p span").Last;
		await page.WaitForSelectorAsync(".richtextbox p:visible");
		var boundingBox = await paragraph!.BoundingBoxAsync();

		// Width was 50 before this change.
		Assert.That(() => Math.Round(boundingBox!.Width), Is.EqualTo(69).After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxDefaultFontStyle()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox()
		{
			Width = 500,
			Height = 500,
			Html = "<p>Test content</p>",
			ReadOnly = true,
		});

		var rtbData = page.Locator(".richtextbox__data p");
		await Assertions.Expect(rtbData).ToHaveCSSAsync("font-family", "Tahoma");
		await Assertions.Expect(rtbData).ToHaveCSSAsync("font-size", "10.6667px");
		await Assertions.Expect(rtbData).ToHaveCSSAsync("line-height", "13px");
		await Assertions.Expect(rtbData).ToHaveCSSAsync("text-align", "left");
		await Assertions.Expect(rtbData).ToHaveCSSAsync("tab-size", "13.5");
		await Assertions.Expect(rtbData).ToHaveCSSAsync("white-space", "pre-wrap");
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxTabsAreCorrectWidthBeforeLoading()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox()
		{
			Width = 500,
			Height = 500,
			Font = new Font(FontFamily.GenericSansSerif, 10, GraphicsUnit.Point),
			Html = "<p><span style=\"text-decoration: underline;\">\tabc</span></p>",
			AcceptsTab = true,
			IsToolBarVisible = true
		});

		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
		var editorAnchor = page.Locator(".richtextbox__editoranchor");
		await editorAnchor.EvaluateAsync("editorAnchor => editorAnchor.className = 'richtextbox__placeholder'");

		var paragraph = page.Locator(".richtextbox p span").Last;
		var boundingBox = await paragraph!.BoundingBoxAsync();

		// Width was 50 before this change.
		Assert.That(() => Math.Round(boundingBox!.Width), Is.EqualTo(69).After(3000, 100));
	}

	[TestCase(true, true, true)]
	[TestCase(false, true, true)]
	[TestCase(true, false, false)]
	[TestCase(false, false, false)]
	[WithPlaywrightPage]
	public async Task RichTextBoxCanClickLinks(bool readonlyState, bool enabledState, bool shouldClicked)
	{
		var isLinkClicked = false;
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var richTextBox = new RichTextBox()
			{
				ReadOnly = readonlyState,
				Enabled = enabledState,
				Html = "<p><a href=\"https://proget.wtg.zone/feeds\" rel=\"noopener\" target=\"_blank\">Test Site</a></p>"
			};
			richTextBox.LinkClicked += (s, e) => isLinkClicked = true;
			return richTextBox;
		});

		var link = page.GetByRole(AriaRole.Link, new() { Name = "Test Site" });
		await link.WaitForAsync();
		await link.ClickAsync();

		Assert.That(() => isLinkClicked, Is.EqualTo(shouldClicked).After(2000, 200));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxPreventLinkOpenWhenReadonly()
	{
		var isDefaultPrevented = false;
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var richTextBox = new RichTextBox()
			{
				ReadOnly = true,
				Html = "<p><a href=\"https://proget.wtg.zone/feeds\" rel=\"noopener\" target=\"_blank\">Test Site</a></p>"
			};
			return richTextBox;
		});
		await page.Locator(".richtextbox__data p a").WaitForAsync();

		isDefaultPrevented = await page.EvaluateAsync<bool>(@"() => {
			const editorData = document.querySelector('.richtextbox__data');
			const link = editorData.querySelector('a');
			let prevented = false;

			editorData.addEventListener('click', (e) => {
				if (e.target.tagName === 'A') {
					prevented = e.defaultPrevented;
				}
			}, { once: true });

			link.click();
			return prevented;
		}");

		Assert.That(isDefaultPrevented, Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxCanClickPastedLinks()
	{
		string? url = null;
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Font = new Font(FontFamily.GenericSansSerif, 8.25f),
				IsToolBarVisible = true,
				Html = "<p>abc</p>"
			};

			richTextBox.LinkClicked += (s, e) => url = e.LinkText;

			return richTextBox;
		});

		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		var editorAnchor = page.Locator(".richtextbox__editoranchor");
		await client.FocusEditorAsync();

		await editorAnchor.AttachClipboardPaste();

		await page.MockClipboardRead(new[] {
			new JSClipboardData("text/html", "<p><a href=\"https://proget.wtg.zone/\">ProGet</a></p>"),
			new JSClipboardData("text/plain", "NoGet")
		});
		await editorBody.PressAsync("Control+KeyV");

		Assert.That(client.GetActiveTextAsync, Does.StartWith("HYPERLINK \"https://proget.wtg.zone/\" ProGet").After(3000, 100));

		await editorAnchor.FocusAsync();
		var link = (await editorBody.QuerySelectorAsync("a:first-of-type"))!;

		var linkTarget = await link.EvaluateAsync<string>("e => e.target");
		Assert.That(() => linkTarget, Is.EqualTo("_blank").After(3000, 100));

		await link.ClickAsync();

		Assert.That(() => url, Is.EqualTo("https://proget.wtg.zone/").After(3000, 300));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxPastesEmbiggenedInternalLinks()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				IsToolBarVisible = true
			};
			form.Controls.Add(richTextBox);
			return form;
		});

		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		var editorAnchor = page.Locator(".richtextbox__editoranchor");
		await client.FocusEditorAsync();

		await editorAnchor.AttachClipboardPaste();

		await page.MockClipboardRead(new[] {
			new JSClipboardData("text/html", "<a href=\"https://www.wisetechglobal.com/\">WiseTech Global</a>"),
			new JSClipboardData("text/plain", "WiseTech Global")
		});
		await editorBody.PressAsync("Control+KeyV");

		var link = await editorBody.QuerySelectorAsync("a:first-of-type");
		var fontSize = await link!.GetComputedStyleAsync("font-size");

		Assert.That(() => fontSize.AsPoints(), Is.EqualTo(10).After(3000, 100), "External link size");

		await editorBody.PressAsync("Control+KeyA");
		await editorBody.PressAsync("Delete");

		await page.MockClipboardRead(new[] {
			new JSClipboardData("text/html", "<a href=\"edient:Command=ShowEditForm&amp;ControllerID=WorkItem&amp;BusinessEntityPK=3b70b04e-4b8b-4811-bf4d-2f19f142a641&amp;VersionNumber=24.6.26.50&amp;Domain=wtg.zone&amp;Instance=ediProd&amp;Hash=%2bugiCSwtWI0e2CrXzCOVpIjZAin6ilWpm\">WI00736622 - [Tier 3] RTB should paste internal (edient) links with custom styling</a>"),
			new JSClipboardData("text/plain", "WI00736622 - [Tier 3] RTB should paste internal (edient) links with custom styling")
		});
		await editorBody.PressAsync("Control+KeyV");

		link = await editorBody.QuerySelectorAsync("a:first-of-type");
		fontSize = await link!.GetComputedStyleAsync("font-size");

		Assert.That(() => fontSize.AsPoints(), Is.EqualTo(12).After(3000, 100), "Internal link size");
	}

	[Test, WithPlaywrightPage]
	public async Task CleanMarkupLinkAttributes([ValueSource(nameof(AnchorTargetOrRelAttributeTestCases))] string attributes, [Values] bool visibleToolbar)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var richTextBox = new RichTextBox() { IsToolBarVisible = visibleToolbar };
			return richTextBox;
		});

		var anchor = await page.WaitForSelectorAsync(".richtextbox__editoranchor");
		var client = await RichTextBoxClient.GetClientAsync();

		await anchor!.EvaluateAsync($"e => e.innerHTML = \"<a href='https://proget.wtg.zone/' badattribute='bad' {attributes}>link</a>\"");

		var link = (await page.QuerySelectorAsync("a"))!;
		var target = await link.GetAttributeAsync("target");
		var rel = await link.GetAttributeAsync("rel");
		var bad = await link.GetAttributeAsync("badattribute");

		Assert.That(target, Is.EqualTo("_blank"));
		Assert.That(rel, Is.EqualTo("noopener"));
		Assert.That(bad, Is.Null);

		var cleanCount = await client.GetCleanMarkupCount();
		Assert.That(cleanCount, Is.EqualTo(8));
	}

	[Test, WithPlaywrightPage]
	public async Task TextNodeContentBetweenListItemsNotLost()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var richTextBox = new RichTextBox();
			return richTextBox;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();

		var anchor = await page.WaitForSelectorAsync(".richtextbox__editoranchor");
		var beforeCleanup = @"<ul><li>item1</li>text node 1<li>item2</li>text node 2<li>item3</li></ul>";
		var afterCleanup = @"<ul><li>item1<br>text node 1</li><li>item2<br>text node 2</li><li>item3</li></ul>";

		await anchor!.EvaluateAsync($"e => e.innerHTML = `{beforeCleanup}`");

		var content = await client.GetEditorHtmlAsync();
		Assert.That(content, Is.EqualTo(afterCleanup));
	}

	[Test, WithPlaywrightPage]
	public async Task TextNodeContentInsideListItemsRootNotLost()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var richTextBox = new RichTextBox();
			return richTextBox;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();

		var anchor = await page.WaitForSelectorAsync(".richtextbox__editoranchor");
		var beforeCleanup = @"<ul>text node<li>item</li></ul>";
		var afterCleanup = @"<ul><li>text node</li><li>item</li></ul>";
		await anchor!.EvaluateAsync($"e => e.innerHTML = `{beforeCleanup}`");

		var content = await client.GetEditorHtmlAsync();
		Assert.That(content, Is.EqualTo(afterCleanup));
	}

	[Test, WithPlaywrightPage]
	public async Task CleanMarkupWithMultipleLinks()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var richTextBox = new RichTextBox();
			return richTextBox;
		});

		var anchor = await page.WaitForSelectorAsync(".richtextbox__editoranchor");
		var client = await RichTextBoxClient.GetClientAsync();
		var beforeCleanup = "<a href='href1' badattribute='bad'>link1</a>" +
					"<a href='href2' badattribute='bad' target='_blank'>link2</a>" +
					"<a href='href3' badattribute='bad' rel='noopener'>link3</a>" +
					"<a href='href4' badattribute='bad' target='_blank' rel='noopener'>link4</a>";
		var afterCleanup = "<p><a href=\"href1\" target=\"_blank\" rel=\"noopener\">link1</a></p>" +
					"<p><a href=\"href2\" target=\"_blank\" rel=\"noopener\">link2</a></p>" +
					"<p><a href=\"href3\" rel=\"noopener\" target=\"_blank\">link3</a></p>" +
					"<p><a href=\"href4\" target=\"_blank\" rel=\"noopener\">link4</a></p>";

		await anchor!.EvaluateAsync($"e => e.innerHTML = \"{beforeCleanup}\"");

		Assert.That(async () => await anchor.EvaluateAsync<string>("e => e.innerHTML"), Is.EqualTo(afterCleanup).After(1000));

		var cleanCount = await client.GetCleanMarkupCount();
		Assert.That(cleanCount, Is.EqualTo(23));
	}

	[Test, WithPlaywrightPage]
	public async Task EmptyParagraphGetsALineBreak()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var richTextBox = new RichTextBox();
			return richTextBox;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();

		var anchor = await page.WaitForSelectorAsync(".richtextbox__editoranchor");
		await anchor!.EvaluateAsync("e => e.innerHTML = `<p></p>`");

		var content = await client.GetEditorHtmlAsync();
		Assert.That(content, Is.EqualTo("<p><br></p>"));
	}

	[Test, WithPlaywrightPage]
	public async Task FilledParagraphDoesNotGetALineBreak()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox()
		{
			Html = "<p><br></p><p><br></p>",
			IsToolBarVisible = true
		});
		var editorBody = page.Locator(".richtextbox__editoranchor");
		await editorBody.FocusAsync();
		await page.Keyboard.PressAsync("Control+A");
		await page.AttachMockClipboardWrite();
		await editorBody.FakeClipboardCopy();
		await editorBody.FakeClipboardPaste("text/html");
		Assert.That(async () => Regex.Replace(await editorBody.InnerHTMLAsync(), "\\sstyle=\"[^\"]*\"", string.Empty), Is.EqualTo("<p><br></p><p><br></p>").After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxScrollsVerticallyWhenReadOnly()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;
		Form? form = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form { Width = 300, Height = 300 };
			richTextBox = new RichTextBox()
			{
				Width = 300,
				Height = 300,
				ReadOnly = true,
				Html = new RtfToHtmlConverter().Convert(@"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang1033{\fonttbl{\f0\fnil Segoe UI;}}
{\*\generator Riched20 10.0.19041}\viewkind4\uc1
\pard\f0\fs18 Lorem ipsum dolor sit amet, consectetur adipiscing elit. Praesent suscipit turpis vitae vulputate elementum. Mauris nec nisi eu nunc egestas pretium. Nullam massa arcu, gravida et lacus vitae, tempor consectetur lorem. Donec et ipsum nibh. Nunc faucibus erat quis mi suscipit, eu varius quam finibus. Donec tempor sem enim, id finibus tellus pretium sed. Maecenas nec efficitur eros, vel pellentesque odio. Duis nisl metus, efficitur ac sem id, fermentum lobortis turpis. Nulla consectetur erat vel mattis iaculis. Nulla efficitur eget purus et tempus. Ut sit amet molestie turpis. Sed faucibus orci sed augue convallis, sit amet sodales erat congue. Orci varius natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Vestibulum id lorem vitae mauris blandit pulvinar quis ac lacus.\par
\par
Aliquam consectetur finibus diam eget lobortis. Donec pellentesque mollis mauris, vitae interdum nunc maximus et. Vivamus et dolor felis. Aliquam porttitor, eros ac accumsan dictum, velit odio commodo odio, vitae efficitur magna sem id felis. Fusce varius cursus elit, sit amet laoreet tortor pulvinar in. Donec rhoncus, nibh non sollicitudin pellentesque, eros est commodo arcu, nec ullamcorper est velit sit amet libero. Curabitur porttitor hendrerit mauris, eget auctor eros dignissim non. Suspendisse potenti. Curabitur eu fringilla enim, porttitor placerat lacus. Cras eget libero vitae mi placerat commodo dictum sit amet sapien. Nullam nec sodales est, vel porta augue. Quisque et maximus est. Donec quis scelerisque nunc.\par
\par
Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec at dolor ut magna maximus condimentum at ac dui. Aenean euismod cursus aliquam. Suspendisse porttitor purus ut arcu tristique, in rhoncus urna imperdiet. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed id ante mollis, vestibulum urna sed, finibus orci. Maecenas placerat fermentum blandit. Vivamus eget ullamcorper lectus, vel pulvinar nunc. Ut vestibulum, risus at venenatis porta, velit sapien bibendum felis, id ultricies elit libero at urna.\par
\par
Mauris et neque est. Morbi nec purus leo. Ut id risus suscipit, mattis eros sed, ullamcorper justo. Vivamus ullamcorper aliquet auctor. Sed faucibus varius laoreet. Suspendisse ultrices ultrices mauris, a suscipit augue tempor ut. Donec ullamcorper mollis facilisis. Donec laoreet consectetur erat ac blandit. Etiam viverra sodales maximus. Sed volutpat erat dapibus hendrerit sagittis. Sed in leo elit.\par
\par
}
")
			};
			form.Controls.Add(richTextBox);
			return form;
		});
		var rtbData = await page.WaitForSelectorAsync(".richtextbox__data:has(*)");

		if (rtbData is null)
		{
			Assert.That(rtbData, Is.Not.Null);
			return;
		}
		Assert.That(async () => await rtbData.EvaluateAsync<bool>("e => e.offsetWidth != e.clientWidth"), Is.True);
	}

	[WithPlaywrightPage]
	[TestCase("text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text")]
	[TestCase("Loremipsumdolorsitamet,consecteturadipiscingelit.Praesentsuscipitturpisvitaevulputateelementum.Maurisnecnisieununcegestaspretiu.Nullammassaarc,gravidaetlacusvita, tempor consectetur lorem. Donec et ipsum nibh. Nunc faucibus erat quis mi suscipit, eu varius quam finibus. Donec tempor sem enim, id finibus tellus pretium sed. Maecenas nec efficitur eros, vel pellentesque")]
	[TestCase("texttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttexttext")]

	public async Task RichTextBoxScrollsHorizontallyWhenReadOnly(string text)
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;
		Form? form = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form { Width = 300, Height = 300 };
			richTextBox = new RichTextBox()
			{
				Width = 300,
				Height = 300,
				ReadOnly = true,
				WordWrap = false,
				Text = text,
			};
			form.Controls.Add(richTextBox);
			return form;
		});
		var rtbData = await page.WaitForSelectorAsync(".richtextbox__data:has(*)");

		Assert.That(rtbData, Is.Not.Null);
		Assert.That(async () => await rtbData!.EvaluateAsync<int>("e => e.scrollHeight - e.clientHeight"), Is.EqualTo(0), "scrollHeight was not Equal To clientHeight");
		Assert.That(async () => await rtbData!.EvaluateAsync<int>("e => e.scrollWidth - e.clientWidth"), Is.GreaterThan(0), "scrollWidth was not Greater Than clientWidth");
	}

	[Test, WithPlaywrightPage]
	public async Task TextChangedEventRaisedAfterTypingTextAndLosingFocus()
	{
		await using var ctx = new InMemoryTestServerContext();

		var textChangedTcs = new TaskCompletionSource();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var richTextBox = new RichTextBox()
			{
				Width = 250,
				Height = 250,
			};
			richTextBox.TextChanged += (s, e) => textChangedTcs.SetResult();
			form.Controls.Add(richTextBox);
			return form;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		await client.FocusEditorAsync();
		await page.Keyboard.PressAsync("T");
		await Task.Delay(TimeSpan.FromSeconds(1));
		await page.Mouse.ClickAsync(280, 280);

		Assert.That(await textChangedTcs.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);
	}

	static IEnumerable<TestCaseData> LinkDestinationList
	{
		get
		{
			yield return new TestCaseData("callto:0123456789", "callto:0123456789", false) { TestName = "{m}_CallTo" };
			yield return new TestCaseData("file:C:/Windows", "file:C:/Windows", false) { TestName = "{m}_LocalFile" };
			yield return new TestCaseData("ftp://anonymous:user@localhost:21", "ftp://anonymous:user@localhost:21", false) { TestName = "{m}_Ftp" };
			yield return new TestCaseData("https://www.wisetechglobal.com/", "https://www.wisetechglobal.com/", false) { TestName = "{m}_Https" };
			yield return new TestCaseData("https://www.wisetechglobal.com/#winzor", "https://www.wisetechglobal.com/#winzor", false) { TestName = "{m}_AnchoredContent" };
			yield return new TestCaseData("https://wisetechglobal.sharepoint.com/Development/Rotation-Management-Workspace/_layouts/15/Doc.aspx?sourcedoc={guid}&action=edit&wd=some_other_param_value&wdorigin=NavigationUrl", "https://wisetechglobal.sharepoint.com/Development/Rotation-Management-Workspace/_layouts/15/Doc.aspx?sourcedoc={guid}&action=edit&wd=some_other_param_value&wdorigin=NavigationUrl", true) { TestName = "{m}_Sharepoint" };
			yield return new TestCaseData("http://datfiles.wtg.zone/", "http://datfiles.wtg.zone/", false) { TestName = "{m}_HttpWtgZone" };
			yield return new TestCaseData("mailto:user@wisetechglobal.com", "mailto:user@wisetechglobal.com", false) { TestName = "{m}_Email" };
			yield return new TestCaseData("onenote:https://d.docs.live.net", "onenote:https://d.docs.live.net", false) { TestName = "{m}_OneNote" };
			yield return new TestCaseData("outlook:someone@example.com", "outlook:someone@example.com", false) { TestName = "{m}_Outlook" };
			yield return new TestCaseData("tel:0123456789", "tel:0123456789", false) { TestName = "{m}_PhoneNumber" };
			yield return new TestCaseData("telnet:www.example.com:80", "telnet:www.example.com:80", false) { TestName = "{m}_Telnet" };
			yield return new TestCaseData("edient:Command=ShowStorageDoc&BusinessEntityPK=03191fd8-75c5-43f8-96c2-07382c07fa39&StorageDocPK=0770d64a-bf31-45d9-981b-2a45de445f7b&Hash=%2bEGQquIVtAZH0ODXoVAt%2biG2RLtnOnT9F", "edient:Command=ShowStorageDoc&BusinessEntityPK=03191fd8-75c5-43f8-96c2-07382c07fa39&StorageDocPK=0770d64a-bf31-45d9-981b-2a45de445f7b&Hash=%2bEGQquIVtAZH0ODXoVAt%2biG2RLtnOnT9F", true) { TestName = "{m}_CargoWise" };
			yield return new TestCaseData("https://devops.com/singlequote-'-show", "https://devops.com/singlequote-'-show", false) { TestName = "{m}_SingleQuote" };
			yield return new TestCaseData("https://devops.com/&quot;-should-show", "https://devops.com/&quot;-should-show", true) { TestName = "{m}_UrlEscapedQuote" };
			yield return new TestCaseData("https://devops.com/%22-should-show", "https://devops.com/%22-should-show", true) { TestName = "{m}_UrlEscapedNumberEntity" };
			yield return new TestCaseData("https://en.wikipedia.org/wiki/Yery#:~:text=Yeru%20or%20Eru%20(%D0%AB%20%D1%8B,letter%20in%20the%20Cyrillic%20script", "https://en.wikipedia.org/wiki/Yery#:~:text=Yeru%20or%20Eru%20(%D0%AB%20%D1%8B,letter%20in%20the%20Cyrillic%20script", true) { TestName = "{m}_Wikipedia" };
			yield return new TestCaseData("https://eye-test.wtg.ws/s/winzor/app/dashboards#/view/b356b27f-57be-44c2-a8fa-b6fd77f1e4ac?_g=(refreshInterval:(pause:!t,value:60000),time:(from:now-7d%2Fd,to:now))&_a=(query:(language:kuery,query:$0f62d841-f7b1-4bf6-aa8c-9c6a17d46943))", "https://eye-test.wtg.ws/s/winzor/app/dashboards#/view/b356b27f-57be-44c2-a8fa-b6fd77f1e4ac?_g=(refreshInterval:(pause:!t,value:60000),time:(from:now-7d%2Fd,to:now))&_a=(query:(language:kuery,query:$0f62d841-f7b1-4bf6-aa8c-9c6a17d46943))", true) { TestName = "{m}_ElasticUrl" };
			yield return new TestCaseData("https://ediprod.cw.wisetechglobal.com/_content/BArchitecture.GUl/js/module/richTextBox.js?v=VZr4uC40pWe00q4JQw943bc93B|UBYd6MNvxMZ60228", "https://ediprod.cw.wisetechglobal.com/_content/BArchitecture.GUl/js/module/richTextBox.js?v=VZr4uC40pWe00q4JQw943bc93B|UBYd6MNvxMZ60228", true) { TestName = "{m}_PipeCharacter" };
			yield return new TestCaseData("https://devops.wisetechglobal.com/wtg?anchor=**module-mapping-guide**", "https://devops.wisetechglobal.com/wtg?anchor=**module-mapping-guide**", false) { TestName = "{m}_UrlParametersWithSpecialCharacters" };
		}
	}

	[TestCaseSource(nameof(LinkDestinationList)), WithPlaywrightPage]
	public async Task RichTextBoxCanPasteLinks(string pastedLink, string parsedDestination, bool shouldBeEncoded)
	{
		await using var ctx = new InMemoryTestServerContext();
		// Note that setting IsToolBarVisible=true here causes the edient link to have large text and a different colour,
		// which makes it more awkward to test. Tests are otherwise unaffected.
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox() { Text = " Lorem ipsum" });

		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();

		await PastePlainTextToElement(editorBody, pastedLink);

		var pastedLinkEncoded = shouldBeEncoded ? HttpUtility.HtmlEncode(pastedLink) : pastedLink;
		var parsedDestinationEncoded = shouldBeEncoded ? HttpUtility.HtmlEncode(parsedDestination) : parsedDestination;
		var content = await client.GetEditorHtmlAsync();
		var cargowiseFormatting = parsedDestination.StartsWith("edient:") ? " style=\"color: rgb(0, 0, 255); font-size: 12pt;\"" : string.Empty;
		Assert.That(content, Is.EqualTo($"<p><a href=\"{parsedDestinationEncoded}\" target=\"_blank\" rel=\"noopener\"{cargowiseFormatting}>{pastedLinkEncoded}</a> Lorem ipsum</p>"));
	}

	[TestCaseSource(nameof(LinkDestinationList)), WithPlaywrightPage]
	public async Task RichTextBoxCanTypeLinks(string typedLink, string parsedDestination, bool shouldBeEncoded = true)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox() { IsToolBarVisible = true, Text = " Lorem ipsum" });

		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();

		await page.Keyboard.TypeAsync(typedLink);
		await page.Keyboard.PressAsync("Control+End");
		await page.Keyboard.PressAsync("Enter");

		var pastedLinkEncoded = shouldBeEncoded ? HttpUtility.HtmlEncode(typedLink) : typedLink;
		var parsedDestinationEncoded = shouldBeEncoded ? HttpUtility.HtmlEncode(parsedDestination) : parsedDestination;
		var content = await client.GetEditorHtmlAsync();
		Assert.That(async () => await client.GetEditorHtmlAsync(), Is.EqualTo($"<p><a href=\"{parsedDestinationEncoded}\" target=\"_blank\" rel=\"noopener\">{pastedLinkEncoded}</a> Lorem ipsum</p><p><br></p>").After(1000, 100));
	}

	static IEnumerable<TestCaseData> UpdateLinkDestinationList
	{
		get
		{
			yield return new TestCaseData("callto:0123456789", 7, 10, "999", "callto:9993456789", false) { TestName = "{m}_CallTo" };
			yield return new TestCaseData("file:C:/Windows", 5, 6, "D", "file:D:/Windows", false) { TestName = "{m}_LocalFile" };
			yield return new TestCaseData("ftp://anonymous:user@localhost:21", 16, 20, "username", "ftp://anonymous:username@localhost:21", false) { TestName = "{m}_Ftp" };
			yield return new TestCaseData("https://www.wisetechglobal.com/", 3, 5, "p", "http://www.wisetechglobal.com/", false) { TestName = "{m}_HttpsToHttp" };
			yield return new TestCaseData("https://www.wisetechglobal.com/#winzor", 8, 26, "cargowise", "https://cargowise.com/#winzor", false) { TestName = "{m}_AnchoredContent" };
			yield return new TestCaseData("https://wisetechglobal.sharepoint.com/Development/Rotation-Management-Workspace/_layouts/15/Doc.aspx?sourcedoc={guid}&action=edit&wd=some_other_param_value&wdorigin=NavigationUrl", 133, 138, "an", "https://wisetechglobal.sharepoint.com/Development/Rotation-Management-Workspace/_layouts/15/Doc.aspx?sourcedoc={guid}&action=edit&wd=another_param_value&wdorigin=NavigationUrl", true) { TestName = "{m}_Sharepoint" };
			yield return new TestCaseData("http://datfiles.wtg.zone", 4, 4, "s", "https://datfiles.wtg.zone", false) { TestName = "{m}_HttpToHttps" };
			yield return new TestCaseData("mailto:user@wisetechglobal.com", 0, 6, "outlook", "outlook:user@wisetechglobal.com", false) { TestName = "{m}_EmailToOutlook" };
			yield return new TestCaseData("onenote:https://d.docs.live.net", 16, 22, "documents", "onenote:https://documents.live.net", false) { TestName = "{m}_OneNote" };
			yield return new TestCaseData("outlook:someone@example.com", 0, 7, "mailto", "mailto:someone@example.com", false) { TestName = "{m}_OutlookToMailTo" };
			yield return new TestCaseData("tel:0123456789", 4, 7, "999", "tel:9993456789", false) { TestName = "{m}_PhoneNumber" };
			yield return new TestCaseData("telnet:www.example.com:80", 11, 18, "google", "telnet:www.google.com:80", false) { TestName = "{m}_Telnet" };
			yield return new TestCaseData("edient:Command=ShowStorageDoc&BusinessEntityPK=03191fd8-75c5-43f8-96c2-07382c07fa39&StorageDocPK=0770d64a-bf31-45d9-981b-2a45de445f7b&Hash=%2bEGQquIVtAZH0ODXoVAt%2biG2RLtnOnT9F", 15, 19, "List", "edient:Command=ListStorageDoc&BusinessEntityPK=03191fd8-75c5-43f8-96c2-07382c07fa39&StorageDocPK=0770d64a-bf31-45d9-981b-2a45de445f7b&Hash=%2bEGQquIVtAZH0ODXoVAt%2biG2RLtnOnT9F", true) { TestName = "{m}_CargoWise" };
			yield return new TestCaseData("https://devops.com/singlequote-'-show", 8, 8, "test.", "https://test.devops.com/singlequote-'-show", false) { TestName = "{m}_SingleQuote" };
			yield return new TestCaseData("https://devops.com/&quot;-should-show", 8, 8, "test.", "https://test.devops.com/&quot;-should-show", true) { TestName = "{m}_UrlEscapedQuote" };
			yield return new TestCaseData("https://devops.com/%22-should-show", 8, 8, "test.", "https://test.devops.com/%22-should-show", true) { TestName = "{m}_UrlEscapedNumberEntity" };
			yield return new TestCaseData("https://en.wikipedia.org/wiki/Yery#:~:text=Yeru%20or%20Eru%20(%D0%AB%20%D1%8B,letter%20in%20the%20Cyrillic%20script", 8, 8, "test.", "https://test.en.wikipedia.org/wiki/Yery#:~:text=Yeru%20or%20Eru%20(%D0%AB%20%D1%8B,letter%20in%20the%20Cyrillic%20script", true) { TestName = "{m}_Wikipedia" };
			yield return new TestCaseData("https://eye-test.wtg.ws/s/winzor/app/dashboards#/view/b356b27f-57be-44c2-a8fa-b6fd77f1e4ac?_g=(refreshInterval:(pause:!t,value:60000),time:(from:now-7d%2Fd,to:now))&_a=(query:(language:kuery,query:$0f62d841-f7b1-4bf6-aa8c-9c6a17d46943))", 8, 8, "test.", "https://test.eye-test.wtg.ws/s/winzor/app/dashboards#/view/b356b27f-57be-44c2-a8fa-b6fd77f1e4ac?_g=(refreshInterval:(pause:!t,value:60000),time:(from:now-7d%2Fd,to:now))&_a=(query:(language:kuery,query:$0f62d841-f7b1-4bf6-aa8c-9c6a17d46943))", true) { TestName = "{m}_ElasticUrl" };
			yield return new TestCaseData("https://ediprod.cw.wisetechglobal.com/_content/BArchitecture.GUl/js/module/richTextBox.js?v=VZr4uC40pWe00q4JQw943bc93B|UBYd6MNvxMZ60228", 8, 8, "test.", "https://test.ediprod.cw.wisetechglobal.com/_content/BArchitecture.GUl/js/module/richTextBox.js?v=VZr4uC40pWe00q4JQw943bc93B|UBYd6MNvxMZ60228", true) { TestName = "{m}_PipeCharacter" };
			yield return new TestCaseData("https://devops.wisetechglobal.com/wtg?anchor=**module-mapping-guide**", 8, 8, "test.", "https://test.devops.wisetechglobal.com/wtg?anchor=**module-mapping-guide**", false) { TestName = "{m}_UrlParametersWithSpecialCharacters" };
		}
	}

	[TestCaseSource(nameof(UpdateLinkDestinationList)), WithPlaywrightPage]
	public async Task RichTextBoxCanUpdateLinkDestination(string originalLink, int startUpdatingIndex, int endUpdatingIndex, string updatedLink, string expectedDestination, bool shouldBeEncoded)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox() { IsToolBarVisible = true, Text = " Lorem ipsum" });

		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();

		await page.Keyboard.TypeAsync(originalLink);
		await page.Keyboard.PressAsync("Control+End");
		await page.Keyboard.PressAsync("Enter");

		await client.EvaluateAsync($"client => client.setSelection({{ start: {startUpdatingIndex}, end: {endUpdatingIndex} }});");
		await page.Keyboard.TypeAsync(updatedLink);

		var expectedDestinationEncoded = shouldBeEncoded ? HttpUtility.HtmlEncode(expectedDestination) : expectedDestination;
		Assert.That(async () => await client.GetEditorHtmlAsync(), Is.EqualTo($"<p><a href=\"{expectedDestinationEncoded}\" target=\"_blank\" rel=\"noopener\">{expectedDestinationEncoded}</a> Lorem ipsum</p><p><br></p>").After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxCanUpdateSmbLinkDestination()
	{
		PlainTextToHtmlConverter plainTextToHtmlConverter = new()
		{
			DetectUrls = true,
			SanitizeLinks = true
		};

		var smbUrlBefore = @"\\uat-backups.wtg.zone\SQL_Backup\Winzor\HybridMode_latest.bak";
		var fileUrlBefore = "file://uat-backups.wtg.zone/SQL_Backup/Winzor/HybridMode_latest.bak";
		var smbUrlAfter = @"\\uat-backups.wtg.zone\SQL_Backup\Winzor\E2ETestSuiteBackup.bak";
		var fileUrlAfter = "file://uat-backups.wtg.zone/SQL_Backup/Winzor/E2ETestSuiteBackup.bak";

		var htmlBefore = plainTextToHtmlConverter.Convert($"TestRigRestoreFromBackup: {smbUrlBefore}");

		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox()
		{
			IsToolBarVisible = true,
			Font = new Font("Tahoma", 20, GraphicsUnit.Pixel),
			Html = htmlBefore,
			DetectUrls = true
		});

		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();

		Assert.That(async () => await client.GetEditorHtmlAsync(), Is.EqualTo($"<p>{WrapInSpan("TestRigRestoreFromBackup: ")}<a href=\"{fileUrlBefore}\" target=\"_blank\" {standardStyle} rel=\"noopener\">{smbUrlBefore}</a></p>").After(1000, 100));

		await client.EvaluateAsync($"client => client.setSelection({{ start: 67, end: 84 }});");
		await page.Keyboard.TypeAsync("E2ETestSuiteBackup");

		Assert.That(async () => await client.GetEditorHtmlAsync(), Is.EqualTo($"<p>{WrapInSpan("TestRigRestoreFromBackup: ")}<a href=\"{fileUrlAfter}\" target=\"_blank\" {standardStyle} rel=\"noopener\">{smbUrlAfter}</a></p>").After(1000, 100));
	}

	[TestCase("https://google.com.dk", 18, 20, " lin", "https://google.com link")]
	[TestCase("https://google.com", 6, 8, " ", "https: google.com")]
	[WithPlaywrightPage]
	public async Task RichTextBoxShouldNotUpdateLinkDestination_WhenDisplayTextIsInvalidLink(string originalLink, int startUpdatingIndex, int endUpdatingIndex, string typedText, string expectedText)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox() { IsToolBarVisible = true, Text = " Lorem ipsum" });

		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();

		await page.Keyboard.TypeAsync(originalLink);
		await page.Keyboard.PressAsync("Control+End");
		await page.Keyboard.PressAsync("Enter");

		await client.EvaluateAsync($"client => client.setSelection({{ start: {startUpdatingIndex}, end: {endUpdatingIndex} }});");
		await page.Keyboard.TypeAsync(typedText);
		await page.Keyboard.PressAsync("Control+End");
		await page.Keyboard.PressAsync("Enter");

		Assert.That(async () => await client.GetEditorHtmlAsync(), Is.EqualTo($"<p><a href=\"{originalLink}\" target=\"_blank\" rel=\"noopener\">{expectedText}</a> Lorem ipsum</p><p><br></p><p><br></p>").After(1000, 100));
	}

	[TestCase("https://welcome.museum.com/", "Museum page", 7, 7, "welcome ", "Museum welcome page")]
	[TestCase("callto:0123456789", "Contact via phone", 8, 8, "me ", "Contact me via phone")]
	[WithPlaywrightPage]
	public async Task RichTextBoxShouldNotUpdateLinkDestination_WhenDisplayTextIsNotALink(string originalLink, string originalText, int startUpdatingIndex, int endUpdatingIndex, string typedText, string expectedText)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox()
		{
			IsToolBarVisible = true,
			Html = $"<p><a href=\"{originalLink}\" target=\"_blank\" {standardStyle} rel=\"noopener\">{originalText}</a>{WrapInSpan(" Lorem ipsum")}</p>"
		});

		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();

		await client.EvaluateAsync($"client => client.setSelection({{ start: {startUpdatingIndex}, end: {endUpdatingIndex} }});");
		await page.Keyboard.TypeAsync(typedText);
		await page.Keyboard.PressAsync("Control+End");
		await page.Keyboard.PressAsync("Enter");

		Assert.That(async () => await client.GetEditorHtmlAsync(), Is.EqualTo($"<p><a href=\"{originalLink}\" target=\"_blank\" {standardStyle} rel=\"noopener\">{expectedText}</a>{WrapInSpan(" Lorem ipsum")}</p>{WrapInP("<br>")}").After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxAllowsUndoAfterPastingLinks()
	{
		var pastedLink = "http://datfiles.wtg.zone/";
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
			new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Text = " Lorem ipsum",
				IsToolBarVisible = true,
			}
		);
		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();

		await PastePlainTextToElement(editorBody, pastedLink);

		var pastedLinkEncoded = HttpUtility.HtmlEncode(pastedLink);
		var content = await client.GetEditorHtmlAsync();
		Assert.That(content, Is.EqualTo($"<p><a href=\"{pastedLinkEncoded}\" target=\"_blank\" rel=\"noopener\">{pastedLinkEncoded}</a> Lorem ipsum</p>"));

		await page.Keyboard.PressAsync("Control+Z");
		content = await client.GetEditorHtmlAsync();
		Assert.That(content, Is.EqualTo("<p> Lorem ipsum</p>"));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxUndoCorrectlyAfterPastingMultipleHtmlElements()
	{
		const string pastedContent = @"<meta http-equiv=""content-type"" content=""text/html; charset=UTF-8""><div><span style=""color:#0f54d6;"">throw new </span><span style=""color:#fff3d1;"">ArgumentOutOfRangeException</span></div>";
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
			new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Text = " Lorem ipsum",
				IsToolBarVisible = true,
			}
		);
		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();

		await page.MockClipboardRead(new[] {
			new JSClipboardData("text/html", pastedContent),
			new JSClipboardData("text/plain", "Not requested")
		});

		await editorBody.PressAsync("Control+KeyV");
		Assert.That((await client.GetEditorHtmlAsync()), Is.EqualTo(@"<p><span style=""color: rgb(15, 84, 214);"">throw new </span><span style=""color: rgb(255, 243, 209);"">ArgumentOutOfRangeException</span> Lorem ipsum</p>").After(1000));

		await page.Keyboard.PressAsync("Control+Z");
		var content = await client.GetEditorHtmlAsync();
		Assert.That(content, Is.EqualTo("<p> Lorem ipsum</p>"));
	}

	[TestCase("dave@null.com")]
	[TestCase("www.google.com")]
	[WithPlaywrightPage]
	public async Task RichTextBoxShouldNotLinkify(string pastedLink)
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Font = new Font(FontFamily.GenericSansSerif, 8.25f),
				Text = "Lorem ipsum"
			};
			form.Controls.Add(richTextBox);
			return form;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();

		await PastePlainTextToElement(editorBody, pastedLink);
		var pastedLinkEncoded = HttpUtility.HtmlEncode(pastedLink);

		var content = await client.GetEditorHtmlAsync();

		Assert.That(content, Is.EqualTo($"<p>{pastedLinkEncoded}Lorem ipsum</p>"));
	}

	[TestCase(@"https://www.wisetechglobal.com/
http://datfiles.wtg.zone/
dave@null.com
http://www.cargowise.com/ https://wisetechglobal.sharepoint.com/
http://datfiles.wtg.zone/")]
	[WithPlaywrightPage]
	public async Task RichTextBoxCanWriteMultipleLinks(string pastedHtml)
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Font = new Font(FontFamily.GenericSansSerif, 8.25f),
				Text = "Lorem ipsum",
				IsToolBarVisible = true
			};
			form.Controls.Add(richTextBox);
			return form;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();

		await PastePlainTextToElement(editorBody, pastedHtml);

		var expected = @"<p><a href=""https://www.wisetechglobal.com/"" target=""_blank"" rel=""noopener"">https://www.wisetechglobal.com/</a></p>" +
			@"<p><a href=""http://datfiles.wtg.zone/"" target=""_blank"" rel=""noopener"">http://datfiles.wtg.zone/</a></p>" +
			@"<p>dave@null.com</p><p><a href=""http://www.cargowise.com/"" target=""_blank"" rel=""noopener"">http://www.cargowise.com/</a> <a href=""https://wisetechglobal.sharepoint.com/"" target=""_blank"" rel=""noopener"">https://wisetechglobal.sharepoint.com/</a></p>" +
			@"<p><a href=""http://datfiles.wtg.zone/"" target=""_blank"" rel=""noopener"">http://datfiles.wtg.zone/</a>Lorem ipsum</p>";

		Assert.That(() => client.GetEditorHtmlAsync(), Is.EqualTo(expected).After(2000, 100));
	}

	[TestCase("h", "ttps://welcome.museum/")]
	[TestCase("ht", "tp://google.hk")]
	[TestCase("htt", "ps://i.can.drink.a.full.bowl.of.soup/page#heading1")]
	[TestCase("htt", "ps://cn.bing.com/search?q=wisetech&qs=n&form=QBRE&sp=-1&lq=0&pq=wisetech&sc=10-8&sk=&cvid=E936DCC9947440C49CEB4611461B2DC2&ghsh=0&ghacc=0&ghpl=")]
	[Test, WithPlaywrightPage]
	public async Task LinkShouldBeDetected(string urlPrefix, string urlSuffix)
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;

		await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				IsToolBarVisible = true
			};
			form.Controls.Add(richTextBox);
			return form;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();

		await PastePlainTextToElement(editorBody, urlPrefix);
		var content = await client.GetEditorHtmlAsync();
		Assert.That(content, Is.EqualTo($"<p>{urlPrefix}</p>"));

		await PastePlainTextToElement(editorBody, urlSuffix);
		content = await client.GetEditorHtmlAsync();
		// The shown html is '&' which in the correct way, but in JS '&' will be convert to '&amp;'
		content = content.Replace("&amp;", "&");
		Assert.That(content, Is.EqualTo($"<p><a href=\"{urlPrefix + urlSuffix}\" target=\"_blank\" rel=\"noopener\">{urlPrefix + urlSuffix}</a></p>"));
	}

	async Task PastePlainTextToElement(IElementHandle element, string content)
	{
		await element.EvaluateAsync(@$"editorBody => editorBody.insertText(`{content}`);");
	}

	[Test, WithPlaywrightPage]
	public async Task OnTextChangedNotTriggeredWhileTyping()
	{
		var textChangedTcs = new TaskCompletionSource();
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var richTextBox = new RichTextBox();
			richTextBox.TextChanged += (s, e) => textChangedTcs.SetResult();
			return richTextBox;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		await client.FocusEditorAsync();

		await page.Keyboard.PressAsync("T");
		await page.Keyboard.PressAsync("e");
		await page.Keyboard.PressAsync("s");
		await page.Keyboard.PressAsync("t");

		Assert.That(async () => await textChangedTcs.Task.WithTimeout(TimeSpan.FromMilliseconds(200)), Is.False);
		Assert.That(await client.GetEditorHtmlAsync(), Is.EqualTo("<p>Test</p>"));
	}

	[Test, WithPlaywrightPage]
	public async Task InsertHtmlWhileTyping()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		var page = await ctx.LoadControlOnFormAsync(() => richTextBox = new RichTextBox());

		var client = await RichTextBoxClient.GetClientAsync();
		Assert.That(client.GetActiveSelectionAsync, Is.EqualTo((0, 0)));

		await client.FocusEditorAsync();
		await page.Keyboard.TypeAsync("Test ");
		Assert.That(client.GetActiveSelectionAsync, Is.EqualTo((5, 5)));

		await richTextBox.InvokeWinzorDispatcherAsync(() =>
		{
			richTextBox.TryUpdateValueFromClient();
			richTextBox.SelectionStart = 5;
			richTextBox.SelectedHtml = WrapInP("add new line");
		});
		Assert.That(client.GetEditorHtmlAsync, Is.EqualTo($"<p>Test {WrapInSpan("add new line")}</p>"));
	}

	[TestCase("WtgEditorBackedClient")]
	[WithPlaywrightPage]
	public async Task RichTextClientShouldBeCreated(string expectedType)
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			richTextBox = new RichTextBox()
			{
				Width = 250,
				Height = 250,
			};

			form.Controls.Add(richTextBox);
			return form;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		var clientType = await page.EvaluateAsync<string>("client => window.WTG.RichTextBoxClient.name");

		Assert.That(clientType, Is.EqualTo(expectedType));

		await richTextBox.InvokeWinzorDispatcherAsync(() => richTextBox.ClearUndoManager());
		clientType = await page.EvaluateAsync<string>("client => window.WTG.RichTextBoxClient.name");

		Assert.That(() => clientType, Is.EqualTo(expectedType).After(500, 100));
	}

	[SuppressMessage("CargoWiseOne", "CW1104:DoNotUseSystemWindowsTabControl", Justification = "Testing")]
	[Test, WithPlaywrightPage]
	public async Task RichTextBoxWorksAfterBeingImmediatelyHidden([Values(0, 50, 100, 150, 200)] int delayInMs)
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var tabControl = new TabControl();
			tabControl.Dock = DockStyle.Fill;
			var tabPage1 = new TabPage();
			tabPage1.Text = "One";
			tabControl.TabPages.Add(tabPage1);
			tabPage1.Controls.Add(new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Html = WrapInP("Rich Text")
			});
			var tabPage2 = new TabPage();
			tabPage2.Text = "Two";
			tabPage2.Controls.Add(new Label { Text = "Hello" });
			tabControl.TabPages.Add(tabPage2);
			form.Controls.Add(tabControl);
			form.Shown += (s, e) => form.BeginInvoke(() =>
			{
				Thread.Sleep(delayInMs);
				tabControl.SelectedIndex = 1;
			});
			return form;
		});

		await page.WaitForSelectorAsync(".label");

		await page.Locator("button", new PageLocatorOptions() { HasTextString = "One" }).ClickAsync();

		var client = await RichTextBoxClient.GetClientAsync();
		Assert.That(async () => await client.GetEditorHtmlAsync(), Is.EqualTo(WrapInP("Rich Text")).After(3000, 100));
	}

	[SuppressMessage("CargoWiseOne", "CW1104:DoNotUseSystemWindowsTabControl", Justification = "Testing")]
	[Test, WithPlaywrightPage]
	public async Task RichTextBoxDeleteInstance()
	{
		await using var ctx = new InMemoryTestServerContext();

		RichTextBox rtb1 = null!;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var tabControl = new TabControl();
			tabControl.Dock = DockStyle.Fill;
			var tabPage1 = new TabPage { Name = "One", Text = "One" };
			tabControl.TabPages.Add(tabPage1);
			rtb1 = new RichTextBox { Width = 500, Height = 500 };
			tabPage1.Controls.Add(rtb1);
			var tabPage2 = new TabPage { Name = "Two", Text = "Two" };
			var rtb2 = new RichTextBox { Width = 500, Height = 500 };
			tabPage2.Controls.Add(rtb2);
			tabControl.TabPages.Add(tabPage2);
			form.Controls.Add(tabControl);
			return form;
		});

		await page.WaitForFunctionAsync(
			"() => typeof(WTG) !== 'undefined' && typeof(WTG.RichTextBoxClient) !== 'undefined'",
			null,
			new() { PollingInterval = 100, Timeout = 3000, }
		);
		var task = page.WaitForFunctionAsync(
			"controlId => WTG.RichTextBoxClient?.tryGetInstance(controlId)",
			rtb1.WinzorControlId,
			new() { PollingInterval = 100, Timeout = 3000, }
		);
		Assert.That(await task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		await page.Locator("button", new PageLocatorOptions() { HasTextString = "Two" }).ClickAsync();
		await Task.Delay(200);
		task = page.WaitForFunctionAsync(
			"controlId => WTG.RichTextBoxClient?.tryGetInstance(controlId)",
			rtb1.WinzorControlId,
			new() { PollingInterval = 100, Timeout = 3000, }
		);
		Assert.That(await task.WithTimeout(TimeSpan.FromSeconds(3)), Is.False);
	}

	[Test, WithPlaywrightPage, Explicit]
	[TestCase("T", "")]
	[TestCase("Backspace", "TT")]
	public async Task RichTextBoxTriggerOnTextChangedHappensBeforeButtonClick(string keyAction, string initialTextContent)
	{
		await using var ctx = new InMemoryTestServerContext();

		var eventOrderCounter = 0;
		int? textChangedEventOrder1 = null;
		int? buttonClickEventOrder1 = null;
		int? textChangedEventOrder2 = null;
		int? buttonClickEventOrder2 = null;
		var textChangedTcs = new TaskCompletionSource();
		var clickTcs = new TaskCompletionSource();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Width = 200, Height = 400, };
			var richTextBox = new RichTextBox() { Width = 200, Height = 200, Text = initialTextContent, SelectionStart = initialTextContent.Length };

			richTextBox.TextChanged += (s, e) =>
			{
				eventOrderCounter++;
				if (textChangedEventOrder1 is null)
				{
					textChangedEventOrder1 = eventOrderCounter;
				}
				else if (textChangedEventOrder2 is null)
				{
					textChangedEventOrder2 = eventOrderCounter;
				}
				else
				{
					Assert.Fail("extra text event: " + eventOrderCounter);
				}

				textChangedTcs.SetResult();
			};

			form.Controls.Add(richTextBox);

			var saveButton = new Button()
			{
				Text = "Click Me",
				Top = 200,
				Left = 0,
				Width = 200,
				Height = 200,
			};

			saveButton.Click += (s, e) =>
			{
				eventOrderCounter++;
				if (buttonClickEventOrder1 is null)
				{
					buttonClickEventOrder1 = eventOrderCounter;
				}
				else if (buttonClickEventOrder2 is null)
				{
					buttonClickEventOrder2 = eventOrderCounter;
				}
				else
				{
					Assert.Fail("extra button event: " + eventOrderCounter);
				}

				clickTcs.SetResult();
			};

			form.Controls.Add(saveButton);

			return form;
		});

		var buttonLocator = page.GetByRole(AriaRole.Button, new() { Name = "Click Me" });

		var client = await RichTextBoxClient.GetClientAsync();

		var editorBody = await client.GetEditorBodyAsync();

		await editorBody.FocusAsync();
		await editorBody.PressAsync(keyAction);
		await Task.Delay(750);
		await buttonLocator.ClickAsync();
		await Task.WhenAll(textChangedTcs.Task, clickTcs.Task);

		Assert.That(() => textChangedEventOrder1, Is.EqualTo(1).After(1000, 50));
		Assert.That(() => buttonClickEventOrder1, Is.EqualTo(2).After(1000, 50));
		Assert.That(textChangedEventOrder2, Is.Null);
		Assert.That(buttonClickEventOrder2, Is.Null);

		textChangedTcs = new TaskCompletionSource();
		clickTcs = new TaskCompletionSource();
		await editorBody.FocusAsync();
		await editorBody.PressAsync(keyAction);
		await Task.Delay(750);
		await buttonLocator.ClickAsync();
		await Task.WhenAll(textChangedTcs.Task, clickTcs.Task);

		Assert.That(() => textChangedEventOrder1, Is.EqualTo(1).After(1000, 50));
		Assert.That(() => buttonClickEventOrder1, Is.EqualTo(2).After(1000, 50));
		Assert.That(() => textChangedEventOrder2, Is.EqualTo(3).After(1000, 50));
		Assert.That(() => buttonClickEventOrder2, Is.EqualTo(4).After(1000, 50));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxInitEditorDoesNotRaisedExceptionWhenTheElementReferenceIsNULL()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Text = "foo",
			};
			richTextBox.Focus();

			form.Controls.Add(richTextBox);
			return form;
		});

		await RichTextBoxClient.GetClientAsync();
		Assert.DoesNotThrowAsync(async () => await richTextBox.Interop!.LoadEditorAsync(null, DotNetObjectReference.Create(richTextBox), richTextBox.Html, new InitializeParameters()));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxCanFocusBeforeInitialization()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			richTextBox = new RichTextBox() { Html = "<p>abc</p>" };
			form.Controls.Add(richTextBox);
			return form;
		});

		Assert.DoesNotThrowAsync(async () => await richTextBox.Interop!.FocusEditorAsync(richTextBox.WinzorControlId));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextClearUndoMangerNoExceptionRaised()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		await ctx.LoadControlOnFormAsync(() =>
		{
			richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
			};
			return richTextBox;
		});
		await RichTextBoxClient.GetClientAsync();
		Assert.DoesNotThrowAsync(async () => await richTextBox.Interop!.ClearUndoManagerAsync(richTextBox.WinzorControlId));
	}

	[TestCaseSource(nameof(ScrollBarCases))]
	[WithPlaywrightPage]
	public async Task RichTextBoxScrollbarStyle_ReadOnly(RichTextBoxScrollBars scrollBars, bool wordWrap, string overflowX, string overflowY)
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Text = "Hello!",
				ReadOnly = true,
				WordWrap = wordWrap,
				ScrollBars = scrollBars
			};
			form.Controls.Add(richTextBox);
			return form;
		});

		var data = await page.WaitForSelectorAsync(".richtextbox__data");
		if (data is null)
		{
			Assert.That(data, Is.Not.Null);
			return;
		}

		Assert.That(await data.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('overflow-x')"), Is.EqualTo(overflowX));
		Assert.That(await data.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('overflow-y')"), Is.EqualTo(overflowY));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxUpdateScrollbarStyleConsecutively_ReadOnly()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				ReadOnly = true,
				WordWrap = false,
				ScrollBars = RichTextBoxScrollBars.Both,
			};
			form.Controls.Add(richTextBox);
			return form;
		});

		var data = await page.WaitForSelectorAsync(".richtextbox__data");
		if (data is null)
		{
			Assert.That(data, Is.Not.Null);
			return;
		}

		richTextBox.Invoke(() => { richTextBox.ScrollBars = RichTextBoxScrollBars.None; });
		Assert.That(async () => await data.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('overflow-x')"), Is.EqualTo("hidden").After(2000, 200));
		Assert.That(await data.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('overflow-y')"), Is.EqualTo("hidden"));

		richTextBox.Invoke(() => { richTextBox.ScrollBars = RichTextBoxScrollBars.Horizontal; });
		Assert.That(async () => await data.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('overflow-x')"), Is.EqualTo("auto").After(2000, 200));
		Assert.That(await data.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('overflow-y')"), Is.EqualTo("hidden"));

		richTextBox.Invoke(() => { richTextBox.WordWrap = true; });
		Assert.That(async () => await data.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('overflow-x')"), Is.EqualTo("hidden").After(2000, 200));
		Assert.That(await data.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('overflow-y')"), Is.EqualTo("hidden"));

		richTextBox.Invoke(() => { richTextBox.ScrollBars = RichTextBoxScrollBars.Vertical; });
		Assert.That(async () => await data.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('overflow-x')"), Is.EqualTo("hidden").After(2000, 200));
		Assert.That(await data.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('overflow-y')"), Is.EqualTo("auto"));

		richTextBox.Invoke(() => { richTextBox.ScrollBars = RichTextBoxScrollBars.Both; });
		Assert.That(async () => await data.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('overflow-x')"), Is.EqualTo("hidden").After(2000, 200));
		Assert.That(await data.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('overflow-y')"), Is.EqualTo("auto"));

		richTextBox.Invoke(() => { richTextBox.WordWrap = false; });
		Assert.That(async () => await data.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('overflow-x')"), Is.EqualTo("auto").After(2000, 200));
		Assert.That(await data.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('overflow-y')"), Is.EqualTo("auto"));
	}

	[TestCase(true, "break-word")]
	[TestCase(false, "normal")]
	[WithPlaywrightPage]
	public async Task RichTextBoxWordWrapStyle_ReadOnly(bool wordWrap, string expected)
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadControlOnFormAsync(() =>
			new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Text = "Hello!",
				ReadOnly = true,
				WordWrap = wordWrap,
			}
		);

		var data = page.Locator(".richtextbox__data");

		Assert.That(data, Is.Not.Null);
		Assert.That(await data.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('overflow-wrap')"), Is.EqualTo(expected));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxHasNoScrollBarWhenNotMultiline_ReadOnly()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Text = "Hello!",
				ReadOnly = true,
			};
			richTextBox.Multiline = false;
			return richTextBox;
		});

		var data = page.Locator(".richtextbox__data");

		Assert.That(data, Is.Not.Null);
		Assert.That(await data.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('overflow')"), Is.EqualTo("hidden"));
	}

	[TestCaseSource(nameof(ScrollBarCases))]
	[WithPlaywrightPage]
	public async Task RichTextBoxScrollbarStyle_Editable(RichTextBoxScrollBars scrollBars, bool wordWrap, string overflowX, string overflowY)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
			new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Text = "Hello!",
				WordWrap = wordWrap,
				ScrollBars = scrollBars
			}
		);

		var client = await RichTextBoxClient.GetClientAsync();

		var horizontalScroll = await client.EvaluateAsync<string>("client => window.getComputedStyle(client.getEditorBody()).getPropertyValue('overflow-x')");
		var verticalScroll = await client.EvaluateAsync<string>("client => window.getComputedStyle(client.getEditorBody()).getPropertyValue('overflow-y')");
		Assert.That(horizontalScroll, Is.EqualTo(overflowX));
		Assert.That(verticalScroll, Is.EqualTo(overflowY));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxHasNoScrollBarWhenNotMultiline_Editable()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				ScrollBars = RichTextBoxScrollBars.Both
			};
			richTextBox.Multiline = false;
			return richTextBox;
		});

		var client = await RichTextBoxClient.GetClientAsync();

		var horizontalScroll = await client.EvaluateAsync<string>("client => window.getComputedStyle(client.getEditorBody()).getPropertyValue('overflow-x')");
		var verticalScroll = await client.EvaluateAsync<string>("client => window.getComputedStyle(client.getEditorBody()).getPropertyValue('overflow-y')");
		Assert.That(horizontalScroll, Is.EqualTo("hidden"));
		Assert.That(verticalScroll, Is.EqualTo("hidden"));
	}

	[TestCase(true, "break-word")]
	//[TestCase(false, "normal")]
	[WithPlaywrightPage]
	public async Task RichTextBoxWordWrapStyle_Editable(bool wordWrap, string expected)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
			new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Text = "Hello!",
				WordWrap = wordWrap,
			}
		);
		var client = await RichTextBoxClient.GetClientAsync();

		var wordWrapStyle = await client.EvaluateAsync<string>("client => window.getComputedStyle(client.getEditorBody()).getPropertyValue('overflow-wrap')");
		Assert.That(wordWrapStyle, Is.EqualTo(expected));
	}

	static readonly object[] ScrollBarCases =
	{
		new object[] { RichTextBoxScrollBars.None, true, "hidden", "hidden" },
		new object[] { RichTextBoxScrollBars.Horizontal, true, "hidden", "hidden" },
		new object[] { RichTextBoxScrollBars.Vertical, true, "hidden", "auto" },
		new object[] { RichTextBoxScrollBars.Both, true, "hidden", "auto" },
		new object[] { RichTextBoxScrollBars.ForcedHorizontal, true, "hidden", "hidden" },
		new object[] { RichTextBoxScrollBars.ForcedVertical, true, "hidden", "scroll" },
		new object[] { RichTextBoxScrollBars.ForcedBoth, true, "hidden", "scroll" },
		new object[] { RichTextBoxScrollBars.None, false, "hidden", "hidden" },
		new object[] { RichTextBoxScrollBars.Horizontal, false, "auto", "hidden" },
		new object[] { RichTextBoxScrollBars.Vertical, false, "hidden", "auto" },
		new object[] { RichTextBoxScrollBars.Both, false, "auto", "auto" },
		new object[] { RichTextBoxScrollBars.ForcedHorizontal, false, "scroll", "hidden" },
		new object[] { RichTextBoxScrollBars.ForcedVertical, false, "hidden", "scroll" },
		new object[] { RichTextBoxScrollBars.ForcedBoth, false, "scroll", "scroll" }
	};

	[TestCase(0)]
	[TestCase(1)]
	[TestCase(10)]
	[WithPlaywrightPage]
	public async Task RichTextBoxLinkClicksOpenLinkCorrectNumberOfTimes(int count)
	{
		await using var ctx = new InMemoryTestServerContext();

		// Why are we testing a link to https://proget.wtg.zone/? Because external traffic has been blocked in the test environment, and DAT Files has no favicon.
		var linkClickedCount = 0;
		await ctx.LoadControlOnFormAsync(() =>
		{
			var richTextBox = new RichTextBox()
			{
				Size = new Size(500, 500),
				ReadOnly = false,
				Html = @"<p><a href='https://proget.wtg.zone/'>ProGet website</a> followed by text</p>"
			};
			richTextBox.LinkClicked += (s, e) => linkClickedCount++;

			return richTextBox;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		Assert.That(client.GetActiveHtmlAsync, Does.Contain("https://proget.wtg.zone/"));

		IElementHandle link;
		var contentBody = await client.GetEditorBodyAsync();
		await contentBody.FocusAsync();
		link = (await contentBody.QuerySelectorAsync("a:first-of-type"))!;

		for (var i = 0; i < count; i++)
		{
			await link.ClickAsync(new ElementHandleClickOptions()
			{
				Force = true,
				Button = MouseButton.Left
			});
		}

		Assert.That(() => linkClickedCount, Is.EqualTo(count).After(3000, 300));
	}

	[TestCase(MouseButton.Right, TestName = "{m}RightMouseButton_wtgEditor")]
	[TestCase(MouseButton.Middle, TestName = "{m}MiddleMouseButton_wtgEditor")]
	[WithPlaywrightPage]
	public async Task RichTextBoxLinksShouldNotOpenWhenHitting(MouseButton button)
	{
		await using var ctx = new InMemoryTestServerContext();

		// Why are we testing a link to http://datfiles.wtg.zone/? Because external traffic has been blocked in the test environment.
		RichTextBox? richTextBox = null!;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			richTextBox = new RichTextBox()
			{
				Size = new Size(500, 500),
				Html = @"<p><a href='https://datfiles.wtg.zone/'>DAT Files website</a> followed by text</p>"
			};
			form.Controls.Add(richTextBox);
			return form;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		Assert.That(client.GetActiveHtmlAsync, Does.Contain("https://datfiles.wtg.zone").After(2000, 100));
		var contentBody = await client.GetEditorBodyAsync();
		await contentBody.FocusAsync();

		var link = (await contentBody.QuerySelectorAsync("a:first-of-type"))!;

		var popup = false;
		page.Popup += (s, e) => popup = true;

		await link.ClickAsync(new ElementHandleClickOptions()
		{
			ClickCount = 5,
			Delay = 50,
			Force = true,
			Button = button
		});

		Assert.That(() => popup, Is.False.After(5000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxStyleIsCorrect()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			richTextBox = new RichTextBox();
			form.Controls.Add(richTextBox);
			return form;
		});

		var editor = await page.WaitForSelectorAsync(".richtextbox__editoranchor");
		Assert.That(editor, Is.Not.Null);

		Assert.That(await editor!.EvaluateAsync<string>("e => window.getComputedStyle(e).whiteSpace"), Is.EqualTo("pre-wrap"));
		Assert.That(await editor.EvaluateAsync<string>("e => window.getComputedStyle(e).overflow"), Is.EqualTo("hidden auto"));
		Assert.That(await editor.EvaluateAsync<string>("e => window.getComputedStyle(e).lineHeight"), Is.EqualTo("normal"));
		Assert.That(await editor.EvaluateAsync<string>("e => window.getComputedStyle(e).caretColor"), Is.EqualTo("rgb(0, 0, 0)"));
	}

	[Test, WithPlaywrightPage]
	public async Task TabsAreDisplayedAsLiteralTabsInRichTextBox()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			richTextBox = new RichTextBox() { Width = 500, Height = 500, Text = string.Empty, AcceptsTab = true };
			form.Controls.Add(richTextBox);
			return form;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		await client.FocusEditorAsync();
		await page.Keyboard.PressAsync("Tab");
		Assert.That(() => richTextBox.Focused, Is.True);
		await page.Keyboard.PressAsync("A");
		await page.Keyboard.PressAsync("Tab");
		await page.Keyboard.PressAsync("Tab");
		await page.Keyboard.PressAsync("B");
		await page.Keyboard.PressAsync("Tab");

		var text = await page.TextContentAsync(".richtextbox__editoranchor");
		Assert.That(text, Is.EqualTo("\tA\t\tB\t"));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxShouldSupportBasicUndoManagerFeature()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				IsToolBarVisible = true,
				Text = "Test1"
			};
			form.Controls.Add(richTextBox);
			return form;
		});
		var client = await RichTextBoxClient.GetClientAsync();
		await client.FocusEditorAsync();
		await page.Keyboard.TypeAsync("Test2");
		await page.Keyboard.PressAsync("Enter");
		await page.Keyboard.TypeAsync("Test3");
		await page.Keyboard.PressAsync("Enter");
		await page.Keyboard.TypeAsync("Test4");
		await page.Keyboard.PressAsync("Control+Z");
		await page.Keyboard.PressAsync("Control+Z");
		await page.Keyboard.PressAsync("Control+Z");
		Assert.That(client.GetEditorTextAsync, Is.EqualTo("Test1"), "Support Control+Z to undo");
		await page.Keyboard.PressAsync("Control+Shift+Z");
		Assert.That(client.GetEditorTextAsync, Is.EqualTo("Test2\nTest1"), "Support Control+Shift+Z to redo");
		await page.Keyboard.PressAsync("Control+Y");
		Assert.That(client.GetEditorTextAsync, Is.EqualTo("Test2\nTest3\nTest1"), "Support Control+Y to redo");
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxIsDisplayedWhenReadOnly()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Text = "Hello!",
				ReadOnly = true,
			};
			return richTextBox;
		});

		var data = page.Locator(".richtextbox__data");

		Assert.That(data, Is.Not.Null);
		Assert.That(await data.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('display')"), Is.Not.EqualTo("none"));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxShowsLinksInTheCorrectBlue([Values] bool readOnly)
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Html = @"<a href=""https://www.wisetechglobal.com.au/"">WiseTech Global</a>",
				ReadOnly = readOnly,
			};
			return richTextBox;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		Assert.That(client, Is.Not.Null);

		Assert.That(await page.Locator(".richtextbox a").Last.GetComputedStyleAsync("color"), Is.EqualTo("rgb(0, 102, 204)"));
	}

	[TestCase(true, 1)]
	[TestCase(false, 0)]
	public async Task TestOnTextBoxSelectionChanged_WhenEnabledIs(bool enabled, int expectedCount)
	{
		RichTextBoxForTest? textBox = null;
		var eventExecutionCount = 0;

		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			textBox = new RichTextBoxForTest { Enabled = enabled };
			textBox.SetCallback("OnTextBoxSelectionChanged", () => eventExecutionCount++);
		});

		await textBox!.OnTextBoxSelectionChangedAsync(new TextboxSelectionChangeEventArgs { SelectionStart = 0, SelectionEnd = 1 });
		Assert.That(eventExecutionCount, Is.EqualTo(expectedCount));
	}

	[TestCase(true, 1)]
	[TestCase(false, 1)]
	public async Task TestOnTextBoxSelectionChanged_WhenReadOnlyIs(bool readOnly, int expectedCount)
	{
		RichTextBoxForTest? textBox = null;
		var eventExecutionCount = 0;

		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			textBox = new RichTextBoxForTest { ReadOnly = readOnly };
			textBox.SetCallback("OnTextBoxSelectionChanged", () => eventExecutionCount++);
		});

		await textBox!.OnTextBoxSelectionChangedAsync(new TextboxSelectionChangeEventArgs { SelectionStart = 0, SelectionEnd = 1 });
		Assert.That(eventExecutionCount, Is.EqualTo(expectedCount));
		Assert.That(textBox.selectionLength, Is.EqualTo(1));
		Assert.That(textBox.selectionStart, Is.EqualTo(0));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxToolbarDefaultFontAndSizeAndSizeUnit()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox()
		{
			Width = 500,
			Height = 500,
			Text = string.Empty,
			IsToolBarVisible = true
		});

		var client = await RichTextBoxClient.GetClientAsync();
		await client.FocusEditorAsync();

		var fontSelector = page.GetByLabel("Fonts", new() { Exact = true });
		var sizeSelector = page.GetByLabel("Font sizes", new() { Exact = true });
		Assert.That(() => fontSelector.InputValueAsync(), Is.EqualTo("Microsoft Sans Serif").After(1000, 100));
		Assert.That(() => sizeSelector.InputValueAsync(), Is.EqualTo("10").After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxFormatPainterUsesFontAsOverride()
	{
		// Setup
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form
			{
				Width = 700,
				Height = 500
			};
			form.Controls.Add(new RichTextBox()
			{
				Width = 700,
				Height = 500,
				IsToolBarVisible = true,
				Html = EnumerableTree.Generator(b => b
					.Open<Paragraph>()
						.Open(new Phrase
						{
							Font = "Comic Sans MS",
							Size = new Unit(13.33, UnitType.Pixel),
							Bold = true,
							Italic = true,
							Underline = true,
							Strikethrough = true,
						})
							.Add("Copy Style From Here")
					.Open<Paragraph>()
						.Open(new Phrase
						{
							Font = "Arial",
							Size = new Unit(20, UnitType.Point),
							Bold = false,
							Italic = false,
							Underline = false,
							Strikethrough = false,
						})
							.Add("Apply It Here")
					.End()
				)
					.Reduce(HtmlEncoder.CreateFactory())
					.Markup(),
			});
			return form;
		});

		var sourceLocator = page.GetByText("Copy Style From Here");
		var destinationLocator = page.GetByText("Apply It Here");
		var resultLocator = page.GetByText("Typed Text");

		// Assert initial states are as expected

		Assert.That(sourceLocator.CountAsync, Is.EqualTo(1).After(1000, 100));

		//// source state
		Assert.That(
			async () => UnitExtensions.TryParseCssUnit(
				await sourceLocator.GetComputedStyleAsync("font-size"),
				out var result
			)
				? result
				: throw new Exception("Could not parse font size"),
			Is.EqualTo(new Unit(13.33, UnitType.Pixel)),
			"Source font size did not match expectation"
		);
		Assert.That(
			async () => await sourceLocator.GetComputedStyleAsync("font-family"),
			Is.EqualTo("\"Comic Sans MS\", sans-serif"),
			"Source font name did not match expectation"
		);
		Assert.That(
			async () => await sourceLocator.GetComputedStyleAsync("font-weight"),
			Is.EqualTo("700"),
			"Source bold state did not match expectation"
		);
		Assert.That(
			async () => await sourceLocator.GetComputedStyleAsync("font-style"),
			Is.EqualTo("italic"),
			"Source italic state did not match expectation"
		);
		Assert.That(
			async () => (await sourceLocator.GetComputedStyleAsync("text-decoration-line")).ToString(),
			Does.Contain("underline"),
			"Source underline state did not match expectation"
		);
		Assert.That(
			async () => (await sourceLocator.GetComputedStyleAsync("text-decoration-line")).ToString(),
			Does.Contain("line-through"),
			"Source strikethrough state did not match expectation"
		);

		// destination state
		Assert.That(
			async () => UnitExtensions.TryParseCssUnit(
				await destinationLocator.GetComputedStyleAsync("font-size"),
				out var result
			)
				? result
				: throw new Exception("Could not parse font size"),
			Is.EqualTo(new Unit(20, UnitType.Point)),
			"Destination font size did not match expectation"
		);
		Assert.That(
			async () => await destinationLocator.GetComputedStyleAsync("font-family"),
			Is.EqualTo("Arial, sans-serif"),
			"Destination font name did not match expectation"
		);
		Assert.That(
			async () => await destinationLocator.GetComputedStyleAsync("font-weight"),
			Is.EqualTo("400"),
			"Destination bold state did not match expectation"
		);
		Assert.That(
			async () => await destinationLocator.GetComputedStyleAsync("font-style"),
			Is.EqualTo("normal"),
			"Destination italic state did not match expectation"
		);
		Assert.That(
			async () => (await destinationLocator.GetComputedStyleAsync("text-decoration-line")).ToString(),
			Does.Not.Contain("underline"),
			"Destination underline state did not match expectation"
		);
		Assert.That(
			async () => (await destinationLocator.GetComputedStyleAsync("text-decoration-line")).ToString(),
			Does.Not.Contain("line-through"),
			"Destination strikethrough state did not match expectation"
		);

		// select the source text
		await sourceLocator.SelectTextAsync();

		// click 'Format Painter' button.
		await page.GetByLabel("Format Painter - Copy Selected Formatting").ClickAsync();

		// type out some replacement text, specifically to the destination

		// note: the fact that we have to manually control the mouse instead of using clickasync on the accessible/recommended locator is a defect in richTextBox.js
		// WI00839919 - Format Painter Not Handling Font Size Correctly
		await destinationLocator.HoverAsync(new LocatorHoverOptions { Position = new Position() { X = 0 } });
		await page.Mouse.DownAsync(new MouseDownOptions() { Button = MouseButton.Left });
		await destinationLocator.HoverAsync(new LocatorHoverOptions { Position = new Position() { X = (await destinationLocator.BoundingBoxAsync())!.Width } });
		await page.Mouse.UpAsync(new MouseUpOptions() { Button = MouseButton.Left });

		// result destination state
		Assert.That(
			async () => UnitExtensions.TryParseCssUnit(
				await destinationLocator.GetComputedStyleAsync("font-size"),
				out var result
			)
				? result
				: throw new Exception("Could not parse font size"),
			Is.EqualTo(new Unit(13.33, UnitType.Pixel)),
			"Result font size did not match expectation"
		);
		Assert.That(
			async () => await destinationLocator.GetComputedStyleAsync("font-family"),
			Is.EqualTo("\"Comic Sans MS\", sans-serif"),
			"Result font name did not match expectation"
		);
		Assert.That(
			async () => await destinationLocator.GetComputedStyleAsync("font-weight"),
			Is.EqualTo("700"),
			"Result bold state did not match expectation"
		);
		Assert.That(
			async () => await destinationLocator.GetComputedStyleAsync("font-style"),
			Is.EqualTo("italic"),
			"Result italic state did not match expectation"
		);
		Assert.That(
			async () => (await destinationLocator.GetComputedStyleAsync("text-decoration-line")).ToString(),
			Does.Contain("underline"),
			"Result destation underline state did not match expectation"
		);
		Assert.That(
			async () => (await destinationLocator.GetComputedStyleAsync("text-decoration-line")).ToString(),
			Does.Contain("line-through"),
			"Result destination strikethrough state did not match expectation"
		);

		Assert.That(async () => await resultLocator.CountAsync(), Is.Zero);

		await destinationLocator.FillAsync("Typed Text");

		Assert.That(async () => await destinationLocator.CountAsync(), Is.Zero);
		Assert.That(async () => await resultLocator.CountAsync(), Is.EqualTo(1));

		// result state
		Assert.That(
			async () => UnitExtensions.TryParseCssUnit(
				await resultLocator.GetComputedStyleAsync("font-size"),
				out var result
			)
				? result
				: throw new Exception("Could not parse font size"),
			Is.EqualTo(new Unit(13.33, UnitType.Pixel)),
			"Result replacement font size did not match expectation"
		);
		Assert.That(
			async () => await resultLocator.GetComputedStyleAsync("font-family"),
			Is.EqualTo("\"Comic Sans MS\", sans-serif"),
			"Result replacement font name did not match expectation"
		);
		Assert.That(
			async () => await resultLocator.GetComputedStyleAsync("font-weight"),
			Is.EqualTo("700"),
			"Result replacement bold state did not match expectation"
		);
		Assert.That(
			async () => await resultLocator.GetComputedStyleAsync("font-style"),
			Is.EqualTo("italic"),
			"Result replacement italic state did not match expectation"
		);
		Assert.That(
			async () => (await resultLocator.GetComputedStyleAsync("text-decoration-line")).ToString(),
			Does.Contain("underline"),
			"Result replacement underline state did not match expectation"
		);
		Assert.That(
			async () => (await resultLocator.GetComputedStyleAsync("text-decoration-line")).ToString(),
			Does.Contain("line-through"),
			"Result replacement strikethrough state did not match expectation"
		);
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxFormatPainterDoesNotOverrideSubsequentTyping()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form
			{
				Width = 700,
				Height = 500
			};
			form.Controls.Add(new RichTextBox()
			{
				Width = 700,
				Height = 500,
				IsToolBarVisible = true,
				Html = EnumerableTree.Generator(b => b
					.Open<Paragraph>()
						.Open(new Phrase
						{
							Bold = true,
						})
							.Add("Copy Style From Here")
					.Open<Paragraph>()
						.Open(new Phrase
						{
							Italic = true,
						})
							.Add("Apply It Here")
					.Open<Paragraph>()
						.Add("I have no style")
					.End()
				)
					.Reduce(HtmlEncoder.CreateFactory())
					.Markup(),
			});
			return form;
		});

		var sourceLocator = page.Locator(SpanSelectorByTextAndVisible("Copy Style From Here"));
		var destinationLocator = page.Locator(SpanSelectorByTextAndVisible("Apply It Here"));
		var blankLocator = page.Locator(SpanSelectorByTextAndVisible("I have no style"));

		Assert.That(sourceLocator.CountAsync, Is.EqualTo(1).After(1000, 100));

		// select the source text
		await sourceLocator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });
		await sourceLocator.SelectTextAsync();

		// click 'Format Painter' button.
		await page.GetByLabel("Format Painter - Copy Selected Formatting").ClickAsync();

		// apply the format

		// note: the fact that we have to manually control the mouse instead of using clickasync on the accessible/recommended locator is a defect in richTextBox.js
		// WI00839919 - Format Painter Not Handling Font Size Correctly
		await destinationLocator.HoverAsync(new LocatorHoverOptions { Position = new Position() { X = 0 } });
		await page.Mouse.DownAsync(new MouseDownOptions() { Button = MouseButton.Left });
		await destinationLocator.HoverAsync(new LocatorHoverOptions { Position = new Position() { X = (await destinationLocator.BoundingBoxAsync())!.Width } });
		await page.Mouse.UpAsync(new MouseUpOptions() { Button = MouseButton.Left });

		Assert.That(async () => await destinationLocator.GetComputedStyleAsync("font-weight"), Is.EqualTo("700"));
		Assert.That(async () => await destinationLocator.GetComputedStyleAsync("font-style"), Is.EqualTo("normal"));

		// note: for some strange reason the second hover never fires if in headless mode, but works fine with headless false, so I guess we'll go with an auto click?
		await blankLocator.ClickAsync();

		await page.Mouse.UpAsync(new MouseUpOptions() { Button = MouseButton.Left });

		Assert.That(async () => await blankLocator.GetComputedStyleAsync("font-weight"), Is.EqualTo("400"));
		Assert.That(async () => await blankLocator.GetComputedStyleAsync("font-style"), Is.EqualTo("normal"));

		await blankLocator.FillAsync("Still nothing");

		var replacementBlankLocator = page.Locator(SpanSelectorByTextAndVisible("Still nothing"));
		Assert.That(async () => await blankLocator.CountAsync(), Is.Zero);
		Assert.That(async () => await replacementBlankLocator.CountAsync(), Is.EqualTo(1));

		Assert.That(async () => await replacementBlankLocator.GetComputedStyleAsync("font-weight"), Is.EqualTo("400"));
		Assert.That(async () => await replacementBlankLocator.GetComputedStyleAsync("font-style"), Is.EqualTo("normal"));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxFormatPainterUsesCorrectFontIfSelectionIsNotChanged()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form
			{
				Width = 700,
				Height = 500
			};
			form.Controls.Add(new RichTextBox()
			{
				Width = 700,
				Height = 500,
				IsToolBarVisible = true,
				Html = EnumerableTree.Generator(b => b
					.Open<Paragraph>()
						.Open(new Phrase
						{
							Bold = true,
						})
							.Add("Copy Style From Here")
					.Open<Paragraph>()
						.Open(new Phrase
						{
							Italic = true,
						})
							.Add("Apply It Here")
					.Open<Paragraph>()
						.Add("I have no style")
					.End()
				)
					.Reduce(HtmlEncoder.CreateFactory())
					.Markup(),
			});
			return form;
		});

		var editor = page.GetByRole(AriaRole.Textbox);

		var sourceLocator = page.GetByText("Copy Style From Here");
		var destinationLocator = page.GetByText("Apply It Here");

		await editor.WaitForAsync();

		Assert.That(async () => await sourceLocator.GetComputedStyleAsync("font-weight"), Is.EqualTo("700"));

		await sourceLocator.SelectTextAsync();

		await page.GetByLabel("Format Painter - Copy Selected Formatting").ClickAsync();

		// note: the fact that we have to manually control the mouse instead of using clickasync on the accessible/recommended locator is a defect in richTextBox.js
		// WI00839919 - Format Painter Not Handling Font Size Correctly
		await sourceLocator.HoverAsync(new LocatorHoverOptions { Position = new Position() { X = 0 } });
		await page.Mouse.DownAsync(new MouseDownOptions() { Button = MouseButton.Left });
		await sourceLocator.HoverAsync(new LocatorHoverOptions { Position = new Position() { X = (await sourceLocator.BoundingBoxAsync())!.Width } });
		await page.Mouse.UpAsync(new MouseUpOptions() { Button = MouseButton.Left });

		Assert.That(async () => await sourceLocator.GetComputedStyleAsync("font-weight"), Is.EqualTo("700"));

		await destinationLocator.ClickAsync();

		Assert.That(async () => await destinationLocator.GetComputedStyleAsync("font-weight"), Is.EqualTo("400"));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxFormatPainterDoesNotChangeStyleOfSurroundingText()
	{
		// I'm going to assume the intent of this test was the painter only applies once, not after repeated selections or typing.
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form
			{
				Width = 700,
				Height = 500
			};
			form.Controls.Add(new RichTextBox()
			{
				Width = 700,
				Height = 500,
				IsToolBarVisible = true,
				Html = EnumerableTree.Generator(b => b
					.Open<Paragraph>()
						.Add("BeforeTargetAfter")
					.Open<Paragraph>()
						.Open(new Phrase
						{
							Bold = true,
						})
							.Add("Copy Style From Here")
					.End()
				)
					.Reduce(HtmlEncoder.CreateFactory())
					.Markup(),
			});
			return form;
		});

		var sourceLocator = page.Locator(".richtextbox__editoranchor p").Filter(new() { HasText = "Copy Style From Here" });
		var destinationLocator = page.GetByText("BeforeTargetAfter");
		var beforeLocator = page.GetByText("Before");
		var targetLocator = page.GetByText("Target");
		var afterLocator = page.GetByText("After");

		Assert.That(sourceLocator.CountAsync, Is.EqualTo(1).After(1000, 100));

		// select the source text
		await sourceLocator.SelectTextAsync();

		// click 'Format Painter' button.
		await page.GetByLabel("Format Painter - Copy Selected Formatting").ClickAsync();

		// apply the format
		// note: the fact that we have to manually control the mouse instead of using clickasync on the accessible/recommended locator is a defect in richTextBox.js
		// WI00839919 - Format Painter Not Handling Font Size Correctly

		var coords = (await destinationLocator.EvaluateAsync("x => new WTG.SelectionFinder(x).DOMRangeFor({start: 6, end: 12}).getBoundingClientRect()")).Value!;
		var mouseY = coords.GetProperty("top").GetSingle();
		await page.Mouse.MoveAsync(coords.GetProperty("left").GetSingle(), mouseY);
		await page.Mouse.DownAsync(new MouseDownOptions() { Button = MouseButton.Left });
		await page.Mouse.MoveAsync(coords.GetProperty("right").GetSingle(), mouseY);
		await page.Mouse.UpAsync(new MouseUpOptions() { Button = MouseButton.Left });

		Assert.That(async () => await beforeLocator.GetComputedStyleAsync("font-weight"), Is.EqualTo("400"));
		Assert.That(async () => await targetLocator.GetComputedStyleAsync("font-weight"), Is.EqualTo("700"));
		Assert.That(async () => await afterLocator.GetComputedStyleAsync("font-weight"), Is.EqualTo("400"));
	}

	[Test, WithPlaywrightPage]
	[TestCase(@"ab", "<p>ab</p>", 2, TestName = "{m}_SingleLine")]
	[TestCase(@"a\r\nb", "<p>a</p><p>b</p>", 3, TestName = "{m}_Multiline")]
	[TestCase(@"a\r\nb\r\n", "<p>a</p><p>b</p>", 3, TestName = "{m}_Multiline_EndsWithLineBreak")]
	[TestCase(@"a\r\n\r\nb", "<p>a</p><p><br></p><p>b</p>", 4, TestName = "{m}_Multiline_WithEmptyLine")]
	public async Task PastePlainTextIntoEmptyEditor(string plainText, string expectedHtml, int selectionEnd)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox() { IsToolBarVisible = true });
		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();

		await PastePlainTextToElement(editorBody, plainText);

		Assert.That(await client.GetEditorHtmlAsync(), Is.EqualTo(expectedHtml));

		var (_, end) = await client.GetActiveSelectionAsync();
		Assert.That(end, Is.EqualTo(selectionEnd));
	}

	[Test, WithPlaywrightPage]
	[TestCase(@"ab", "<p>Test Conabtent</p>", 10, TestName = "{m}_SingleLine")]
	[TestCase(@"a\r\nb", "<p>Test Cona</p><p>btent</p>", 11, TestName = "{m}_Multiline")]
	[TestCase(@"a\r\nb\r\n", "<p>Test Cona</p><p>btent</p>", 11, TestName = "{m}_Multiline_EndsWithLineBreak")]
	[TestCase(@"a\r\n\r\nb", "<p>Test Cona</p><p><br></p><p>btent</p>", 12, TestName = "{m}_Multiline_WithEmptyLine")]
	public async Task PastePlainTextIntoNonEmptyEditor(string plainText, string expectedHtml, int selectionEnd)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox
		{
			Text = "Test Content",
			IsToolBarVisible = true
		});
		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();

		await client.EvaluateAsync("client => client.setSelection({ start: 8, end: 8 });");
		await PastePlainTextToElement(editorBody, plainText);

		Assert.That(await client.GetEditorHtmlAsync(), Is.EqualTo(expectedHtml));

		var (_, end) = await client.GetActiveSelectionAsync();
		Assert.That(end, Is.EqualTo(selectionEnd));
	}

	[Test, WithPlaywrightPage]
	public async Task NativeCopyPasteHtmlWorks()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox
		{
			Html = $"<p><strong>{WrapInSpan("a")}</strong></p>",
			IsToolBarVisible = true,
		});
		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await page.AttachMockClipboardWrite();
		await client.FocusEditorAsync();

		var editorAnchor = page.Locator(".richtextbox__editoranchor");
		Assert.That(await editorBody.InnerHTMLAsync(), Is.EqualTo($"<p><strong>{WrapInSpan("a")}</strong></p>"));
		await client.EvaluateAsync("client => client.setSelection({ start: 0, end: 1 });");
		await page.Keyboard.PressAsync("Control+C");
		await client.EvaluateAsync("client => client.setSelection({ start: 1, end: 1 });");
		await page.Keyboard.PressAsync("Control+V");
		Assert.That(async () => await editorBody.InnerHTMLAsync(), Is.EqualTo($"<p><strong>{WrapInSpan("a")}</strong><strong {defaultEditorStyle}>{WrapInSpan("a")}</strong></p>").After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task NativeCopyPlainPasteHtmlWorks()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox()
		{
			Html = $"<p><strong>{WrapInSpan("Lorem")}</strong>{WrapInSpan(" ipsum")}</p>",
			IsToolBarVisible = true
		});
		await page.AttachMockClipboardWrite();

		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();

		await client.FocusEditorAsync();

		await page.Keyboard.PressAsync("Control+KeyA");
		await page.Keyboard.PressAsync("Control+KeyC");
		await page.Keyboard.PressAsync("Control+End");
		await page.Keyboard.PressAsync("Control+Shift+KeyV");

		Assert.That(async () => await editorBody.InnerHTMLAsync(), Is.EqualTo($"<p><strong>{WrapInSpan("Lorem")}</strong>{WrapInSpan(" ipsum")}<span {defaultEditorStyle}>Lorem ipsum</span></p>").After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task ShiftInsertShouldKeepFormat()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox()
		{
			Html = $"<p><strong>{WrapInSpan("Lorem")}</strong><u>{WrapInSpan("Lorem")}</u><i>{WrapInSpan("Lorem")}</i></p>",
			IsToolBarVisible = true
		});
		await page.AttachMockClipboardWrite();

		var client = await RichTextBoxClient.GetClientAsync();

		await client.FocusEditorAsync();

		await page.Keyboard.PressAsync("Control+KeyA");
		await page.Keyboard.PressAsync("Control+KeyC");
		await page.Keyboard.PressAsync("Control+End");
		await page.Keyboard.PressAsync("Shift+Insert");
		Assert.That(async () => await client.GetActiveHtmlAsync(), Is.EqualTo($"<p><strong>{WrapInSpan("Lorem")}</strong><span style=\"text-decoration-line: underline; {standardFontStyle}\">Lorem</span><em>{WrapInSpan("Lorem")}</em><strong {defaultEditorStyle}>{WrapInSpan("Lorem")}</strong><span style=\"text-decoration-line: underline; {standardFontStyle}\">Lorem</span><em {defaultEditorStyle}>{WrapInSpan("Lorem")}</em></p>").After(1000, 100));
	}

	static IEnumerable<TestCaseData> ShiftEnterCreatesALineBreakTagAndPlacesCursorAfterItTestData()
	{
		yield return new TestCaseData(WrapInP("Test"), 0, WrapInP("<br>Test"), 1) { TestName = "{m}_AtTheBeginningOfDocument" };
		yield return new TestCaseData(WrapInP("Test"), 4, WrapInP("Test<br><br>"), 5) { TestName = "{m}_AtTheEndOfDocument" };
		yield return new TestCaseData(WrapInP("Test content"), 4, WrapInP("Test<br> content"), 5) { TestName = "{m}_InTheMiddleOfText" };
		// Note: there is a known (minor) bug here. See: WI00846141 - [RtfConverter] HtmlEncoder spits out extra line breaks
		yield return new TestCaseData(WrapInP("First line<br>Second line"), 10, $"<p>{WrapInSpan("First line<br><br>Second line")}<br></p>", 11) { TestName = "{m}_BeforeANewLine" };
		yield return new TestCaseData(WrapInP("First paragraph") + WrapInP("Second line"), 15, WrapInP("First paragraph<br><br>") + WrapInP("Second line"), 16) { TestName = "{m}_BeforeANewParagraph" };
	}

	[TestCaseSource(nameof(ShiftEnterCreatesALineBreakTagAndPlacesCursorAfterItTestData))]
	[WithPlaywrightPage]
	public async Task ShiftEnterCreatesALineBreakTagAndPlacesCursorAfterIt(string originalHtml, int cursorPosition, string expectedHtml, int expectedCursorPosition)
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		var page = await ctx.LoadControlOnFormAsync(() => richTextBox = new RichTextBox
		{
			Html = originalHtml,
			IsToolBarVisible = true,
		});
		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();

		await client.EvaluateAsync($"client => client.setSelection({{ start: {cursorPosition}, end: {cursorPosition} }});");
		await page.Keyboard.PressAsync("Shift+Enter");

		Assert.That(async () => await editorBody.InnerHTMLAsync(), Is.EqualTo(expectedHtml).After(3000, 100));

		Assert.That(async () => await client.GetActiveSelectionAsync(), Is.EqualTo((expectedCursorPosition, expectedCursorPosition)));
	}

	[Test, WithPlaywrightPage]
	public async Task SpicyMarkupGetsCleaned()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		var page = await ctx.LoadControlOnFormAsync(() => richTextBox = new RichTextBox
		{
			IsToolBarVisible = true,
		});

		await page.MockClipboardRead(new[] {
			new JSClipboardData("text/html", @"<header>Header</header><aside>Aside</aside><footer>Footer</footer><p><span style=""border: 1px solid #f00"">Bordered</span></p>"),
			new JSClipboardData("text/plain", "Not requested"),
		});

		var client = await RichTextBoxClient.GetClientAsync();
		await client.FocusEditorAsync();
		await richTextBox.InvokeWinzorDispatcherAsync(() => richTextBox.Paste());

		Assert.That(async () => await client.GetActiveHtmlAsync(), Is.EqualTo("<p>Header</p><p>Aside</p><p>Footer</p><p><span style=\"border-width: 1px; border-style: solid; border-color: rgb(255, 0, 0);\">Bordered</span></p>").After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TextNodeAtRootGetsCleaned()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox
		{
			IsToolBarVisible = true,
		});
		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();

		await page.EvaluateAsync(@"
			document.querySelector('.richtextbox__editoranchor').innerHTML = 'Test<p>content</p>here';
		");

		await editorBody.PressAsync("Control+End");

		Assert.That(await editorBody.InnerHTMLAsync(), Is.EqualTo("<p>Test</p><p>content</p><p>here</p>"));
	}

	[Test, WithPlaywrightPage]
	public async Task WhitespaceGetsCleanedProperly()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox { IsToolBarVisible = true });
		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();

		await page.MockClipboardRead(new[] {
			new JSClipboardData("text/html", @"<pre>Line 1
Line 2</pre>

<p>Times
New Roman</p>"),
			new JSClipboardData("text/plain", "Not requested")
		});

		await editorBody.PressAsync("Control+KeyV");

		Assert.That(async () => await editorBody.InnerHTMLAsync(), Is.EqualTo("<p>Line 1<br>Line 2</p><p>Times New Roman</p>").After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task PreformattedCodeMaintainsLineBreaks()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox { IsToolBarVisible = true });
		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();

		await page.MockClipboardRead(new[] {
			new JSClipboardData("text/html", @"<pre class=""hljs"" style=""box-sizing: border-box; display: block; overflow-x: auto; padding: 15px; color: rgba(0, 0, 0, 0.9); text-size-adjust: none; margin: 0px 0px 16px; font-size: 0.75rem; white-space: pre; overflow-wrap: normal; background-color: var(--palette-black-alpha-6,rgba(0, 0, 0, .06)); border-radius: 2px; font-style: normal; font-variant-ligatures: normal; font-variant-caps: normal; font-weight: 400; letter-spacing: normal; orphans: 2; text-align: start; text-indent: 0px; text-transform: none; widows: 2; word-spacing: 0px; -webkit-text-stroke-width: 0px; text-decoration-thickness: initial; text-decoration-style: initial; text-decoration-color: initial;""><code style=""box-sizing: border-box; font-family: Menlo, Consolas, &quot;Courier New&quot;, monospace; font-size: inherit; background-color: transparent; color: var(--text-primary-color,rgba(0, 0, 0, .9)); padding: 0px; border-radius: 2px;"">https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev/pullrequest/210802

TestRigDatabaseName: SH0WI00515179
TestRigIconName: WI00515179
TestRigRestoreFromBackup: \\\\sydsp-ssql-8.sand.wtg.zone\\SQL_Backup\\HRM_Team\\HRMSALP.bak
TestRigWebSites: Glow,GlowWebClient,Services
TestRigRegistryEntries: GlowEnableDevelopmentMode|bool|true, EnableAdvancedDataAutomationWizard|bool|true
TestRigEnableAudit:true
TestRigServiceTasks: false
TestRigDeployWinzor: true
</code></pre>"),
			new JSClipboardData("text/plain", "Not requested")
		});

		await editorBody.PressAsync("Control+KeyV");

		var expectedString = "<p style=\"font-size: 0.75rem; color: rgba(0, 0, 0, 0.9);\"><span style=\"font-family: Menlo, Consolas, &quot;Courier New&quot;, monospace;\"><a href=\"https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev/pullrequest/210802\" target=\"_blank\" rel=\"noopener\">https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev/pullrequest/210802</a>" +
			"<br>" +
			"<br>" +
			"TestRigDatabaseName: SH0WI00515179<br>" +
			"TestRigIconName: WI00515179<br>" +
			@"TestRigRestoreFromBackup: \\sydsp-ssql-8.sand.wtg.zone\SQL_Backup\HRM_Team\HRMSALP.bak<br>" +
			"TestRigWebSites: Glow,GlowWebClient,Services<br>" +
			"TestRigRegistryEntries: GlowEnableDevelopmentMode|bool|true, EnableAdvancedDataAutomationWizard|bool|true<br>" +
			"TestRigEnableAudit:true<br>" +
			"TestRigServiceTasks: false<br>" +
			"TestRigDeployWinzor: true<br></span></p>";

		Assert.That(await editorBody.InnerHTMLAsync(), Is.EqualTo(expectedString));
	}

	[Test, WithPlaywrightPage]
	public async Task InlineElementsAtRootGetReplaced()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		var page = await ctx.LoadControlOnFormAsync(() => richTextBox = new RichTextBox() { IsToolBarVisible = true });
		await page.MockClipboardRead(new[] {
			new JSClipboardData("text/html", @"<span>Span</span><strong>Strong</strong><em>Emphasis</em><s>Strike</s><u>Underline</u><a href=\""https://www.wisetechglobal.com/\"">WiseTech</a>"),
			new JSClipboardData("text/plain", "Not requested"),
		});

		var client = await RichTextBoxClient.GetClientAsync();
		await client.FocusEditorAsync();
		await richTextBox.InvokeWinzorDispatcherAsync(() => richTextBox.Paste());

		Assert.That(async () => await client.GetActiveHtmlAsync(), Is.EqualTo("<p>Span</p><p style=\"font-weight: 700;\">Strong</p><p style=\"font-style: italic;\">Emphasis</p><p style=\"text-decoration-line: line-through;\">Strike</p><p style=\"text-decoration-line: underline;\">Underline</p><p><a href=\"https://www.wisetechglobal.com/\" target=\"_blank\" rel=\"noopener\">WiseTech</a></p>").After(1000, 100));
	}

	// https://developer.mozilla.org/en-US/docs/Web/HTML/Element#inline_text_semantics
	[Test, WithPlaywrightPage]
	public async Task ExoticInlineElementsStayInlineWhenInjected([Values("cite", "code", "dfn", "kbd", "mark", "q", "rt", "ruby", "samp")] string elementType)
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		var page = await ctx.LoadControlOnFormAsync(() => richTextBox = new RichTextBox() { IsToolBarVisible = true });

		var editor = page.GetByRole(AriaRole.Textbox);
		await editor.EvaluateAsync("(elm, inputHtml) => elm.innerHTML = inputHtml", $"<p><{elementType}>Text</{elementType}> <{elementType}>Text</{elementType}><{elementType}>Text</{elementType}></p>");

		if (elementType == "cite" || elementType == "dfn")
		{
			Assert.That(async () => await editor.InnerHTMLAsync(), Is.EqualTo("<p><span style=\"font-style: italic;\">Text</span> <span style=\"font-style: italic;\">TextText</span></p>").After(1000, 100));
		}
		else
		{
			Assert.That(async () => await editor.InnerHTMLAsync(), Is.EqualTo("<p>Text TextText</p>").After(1000, 100));
		}
	}

	// https://developer.mozilla.org/en-US/docs/Web/HTML/Element#inline_text_semantics
	[Test, WithPlaywrightPage]
	public async Task ExoticInlineElementsStayInlineWhenPasted([Values("cite", "code", "dfn", "kbd", "mark", "q", "rt", "ruby", "samp")] string elementType, [Values] bool rootLevel)
	{
		var html = $"<{elementType}>Text</{elementType}> <{elementType}>Text</{elementType}><{elementType}>Text</{elementType}>";
		var formattedHtml = rootLevel ? html : $"<p>{html}</p>";

		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox
		{
			Html = "<p></p>",
			IsToolBarVisible = true,
		});
		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();

		await page.MockClipboardRead(new[] {
			new JSClipboardData("text/html", formattedHtml),
			new JSClipboardData("text/plain", "Not requested")
		});

		await editorBody.PressAsync("Control+KeyV");

		if (elementType == "cite" || elementType == "dfn")
		{
			Assert.That(await editorBody.InnerHTMLAsync(), Is.EqualTo($"<p>Text Text<span style=\"font-style: italic;\">Text</span></p>"));
		}
		else
		{
			Assert.That(await editorBody.InnerHTMLAsync(), Is.EqualTo($"<p>Text TextText</p>"));
		}
	}

	[Test, WithPlaywrightPage]
	public async Task UnsupportedElementsMaintainFormatting(
		[Values("h1", "h2", "div")] string elementType,
		[Values("font-family: &quot;Comic Sans MS&quot;, sans-serif;", "font-weight: bold;", "font-style: italic;", "text-decoration-line: underline;", "text-decoration-line: underline line-through;", "color: rgb(0, 255, 0);", "font-size: 24pt;")] string styling)
	{
		var html = $@"<{elementType} style=""{styling}"">Text</{elementType}>";

		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox
		{
			Html = "<p></p>",
			IsToolBarVisible = true,
		});
		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();

		await page.MockClipboardRead(new[] {
			new JSClipboardData("text/html", html),
			new JSClipboardData("text/plain", "Not requested")
		});

		await editorBody.PressAsync("Control+KeyV");

		Assert.That(async () => await editorBody.InnerHTMLAsync(), Is.EqualTo(@$"<p style=""{styling}"">Text</p>").After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task NoDivsAfterLists([Values("ul", "ol")] string listType)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox
		{
			Html = $"<{listType}><li>{WrapInSpan("I've got a list of names")}</li></{listType}>",
			IsToolBarVisible = true,
		});
		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();

		await page.Keyboard.PressAsync("Control+End");
		await page.Keyboard.PressAsync("Enter");
		await page.Keyboard.PressAsync("Enter", new KeyboardPressOptions() { Delay = 100 });
		await page.Keyboard.TypeAsync("My markup should be clean, but is it?", new KeyboardTypeOptions() { Delay = 50 });

		Assert.That(async () => await editorBody.InnerHTMLAsync(), Is.EqualTo($"<{listType}><li>{WrapInSpan("I've got a list of names")}</li></{listType}>{WrapInP("My markup should be clean, but is it?")}").After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task PastingContentFromWordIsCleaned()
	{
		// The food things aren't added by Word, but we need to ensure that things like that can't come through.
		var html = "<p class=\"MsoNormal\"><span style=\"eggs:scrambled;-bacon:optional;toast:sourdough;font-size:12.0pt;line-height:107%;font-family: &quot;Times New Roman&quot;,serif;mso-font-kerning:0pt\">Screen #1 (built in laptop screen) 250% scale<o:p></o:p></span></p>";
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox
		{
			Html = "<p></p>",
			IsToolBarVisible = true,
		});
		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();

		await page.MockClipboardRead(new[] {
			new JSClipboardData("text/html", html),
			new JSClipboardData("text/plain", "Not requested")
		});
		await editorBody.PressAsync("Control+KeyV");
		Assert.That(await editorBody.InnerHTMLAsync(), Is.EqualTo("<p><span style=\"font-size: 12pt; font-family: &quot;Times New Roman&quot;, serif;\">Screen #1 (built in laptop screen) 250% scale</span></p>"));
	}

	[Test, WithPlaywrightPage]
	public async Task SpacingsNeedNumbers([Values("margin", "margin-left", "padding", "padding-bottom", "padding-left", "padding-top")] string property, [Values("auto", "inherit", "initial", "revert", "revert-layer", "unset")] string value)
	{
		var html = $"<p style=\"{property}: {value}\">Lorem ipsum dolor sit amet.</p>";
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox
		{
			Html = "<p></p>",
			IsToolBarVisible = true,
		});
		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();

		await page.MockClipboardRead(new[] {
			new JSClipboardData("text/html", html),
			new JSClipboardData("text/plain", "Not requested")
		});
		await editorBody.PressAsync("Control+KeyV");
		Assert.That(await editorBody.InnerHTMLAsync(), Is.EqualTo("<p>Lorem ipsum dolor sit amet.</p>"));
	}

	[Test, WithPlaywrightPage]
	public async Task WidthsNeedNumbers([Values("auto", "fit-content", "fit-content(20em)", "max-content", "min-content", "inherit", "initial", "revert", "revert-layer", "unset")] string width)
	{
		var html = $"<p style=\"width: {width}\">Lorem ipsum dolor sit amet.</p>";
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox
		{
			Html = "<p></p>",
			IsToolBarVisible = true,
		});
		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();

		await page.MockClipboardRead(new[] {
			new JSClipboardData("text/html", html),
			new JSClipboardData("text/plain", "Not requested")
		});
		await editorBody.PressAsync("Control+KeyV");
		Assert.That(await editorBody.InnerHTMLAsync(), Is.EqualTo("<p>Lorem ipsum dolor sit amet.</p>"));
	}

	[TestCase(0, 0, false)]
	[TestCase(0, 1, false)]
	[TestCase(0, 2, false)]
	[TestCase(1, 0, false)]
	[TestCase(1, 1, true, "http://datfiles.wtg.zone/#a")]
	[TestCase(1, 3, true, "http://datfiles.wtg.zone/#a")]
	[TestCase(1, 4, false)]
	[TestCase(2, 0, false)]
	[TestCase(2, 1, false)]
	[TestCase(3, 0, false)]
	[TestCase(3, 1, true, "http://datfiles.wtg.zone/#b")]
	[TestCase(3, 10, true, "http://datfiles.wtg.zone/#b")]
	[TestCase(3, 17, false)]
	[TestCase(4, 0, false)]
	[TestCase(4, 1, false)]
	[WithPlaywrightPage]
	public async Task RichTextBoxOpensLinkWhenHittingEnter(int vertical, int horizontal, bool isLink, string? expectedUrl = null)
	{
		const string checkNumberOfParagraphs = "document.getElementsByClassName('richtextbox__editoranchor')[0].getElementsByTagName('p').length";

		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			richTextBox = new RichTextBox()
			{
				IsToolBarVisible = true,
				ReadOnly = false,
				Width = 600,
				Height = 600,
				Html = "<p>Cool text.&nbsp;</p>"
				+ "<p><a href=\"http://datfiles.wtg.zone/#a\">Test</a></p>"
				+ "<p>1234567890</p>"
				+ "<p><a href=\"http://datfiles.wtg.zone/#b\">Link to DAT Files</a></p>"
				+ "<p>Hello world.</p>"
			};
			form.Controls.Add(richTextBox);
			return form;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		await client.FocusEditorAsync();

		var popupPromise = page.WaitForPopupAsync();
		Assert.That(async () => await client.EvaluateAsync<int>(checkNumberOfParagraphs), Is.EqualTo(5).After(2000, 100));

		var openedUrl = "";
		var openedPopups = 0;
		page.Popup += (s, e) =>
		{
			openedUrl = e.Url;
			openedPopups++;
		};

		await page.Keyboard.PressAsync("Home");
		await page.Keyboard.PressAsync("PageUp");
		for (var i = 0; i < vertical; i++)
		{
			await page.Keyboard.PressAsync("ArrowDown");
		}
		for (var i = 0; i < horizontal; i++)
		{
			await page.Keyboard.PressAsync("ArrowRight");
		}
		await page.Keyboard.PressAsync("Enter");

		Assert.That(async () => await client.EvaluateAsync<int>(checkNumberOfParagraphs), Is.EqualTo(isLink ? 5 : 6).After(2000, 100), "Wrong number of paragraphs.");

		if (isLink)
		{
			var popupLoaded = await popupPromise;
			await popupLoaded.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

			Assert.That(() => popupLoaded.Url, Is.EqualTo(expectedUrl).After(3000, 100));
			Assert.That(() => openedPopups, Is.EqualTo(1).After(2000, 100));
			Assert.That(openedUrl, Is.EqualTo(expectedUrl));
		}
		else
		{
			Assert.That(() => openedPopups, Is.EqualTo(0).After(2000, 100));
		}
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxSelectsTextWhenClickingInGutter()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			richTextBox = new RichTextBox()
			{
				Width = 600,
				Height = 500,
				Html = $"<p><span {defaultEditorStyle}>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Praesent imperdiet eu mi eget aliquet. Suspendisse interdum condimentum quam eget ornare. Cras tincidunt ac ante non egestas. Curabitur placerat gravida sagittis. Sed sagittis turpis in dolor auctor, blandit vehicula mi luctus. Phasellus tincidunt, massa sed mollis condimentum, neque lorem maximus purus, eu laoreet ex mi id urna. Praesent venenatis iaculis lectus sed pulvinar. Duis sollicitudin eget urna mattis consectetur.</span></p>"
					+ $"<p><span {defaultEditorStyle}>Phasellus a molestie nibh. Vestibulum vel pellentesque dolor. Etiam lorem erat, mattis vitae erat ut, vestibulum lacinia purus. Phasellus non odio et libero sagittis sollicitudin ut eu felis. In ultrices tellus ut arcu faucibus, et aliquet purus tristique. Fusce pretium tristique eros, ac sodales risus mattis in. Donec nec enim accumsan, vehicula risus non, ultrices augue. Vivamus enim arcu, commodo quis dui eget, sagittis maximus orci. In volutpat, lorem ut ullamcorper maximus, turpis metus aliquam ex, sed pharetra ante neque nec dui. Ut in ante egestas, semper magna quis, mattis quam. Nullam interdum felis eu ultrices porttitor. Suspendisse ut erat est. Nullam nec egestas tortor.</span></p>"
					+ $"<p><span {defaultEditorStyle}>Fusce augue est, tincidunt nec nisl pretium, sodales porta velit. Nunc tincidunt scelerisque feugiat. Etiam aliquet vitae velit ut tempor. Vivamus ipsum tortor, egestas non faucibus non, vulputate quis mi. Cras lacinia mattis ipsum ut rutrum. Donec interdum quis lorem vel lacinia. Pellentesque ut sem sed tellus imperdiet interdum sit amet vel tellus. Vivamus sollicitudin vehicula lacus, eu rutrum nisi congue id. Etiam hendrerit erat vitae augue volutpat lacinia. Sed non maximus nibh. Maecenas porta, felis a tristique faucibus, est diam ultricies nulla, id vulputate felis velit at quam. Pellentesque semper tempor nulla, sit amet tempus tellus fringilla vitae. Fusce tincidunt dolor eget libero ullamcorper sagittis. In pellentesque ac felis et tempus.</span></p>"
			};
			return richTextBox;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		var editor = page.Locator(".richtextbox__editoranchor");

		await editor.ClickAsync(new()
		{
			ClickCount = 1,
			Position = new Position { X = 1, Y = 40 }
		});

		var (selectionStart, selectionEnd) = await client.GetActiveSelectionAsync();
		Assert.That(selectionStart, Is.EqualTo(193));
		Assert.That(selectionEnd, Is.EqualTo(295));

		await Task.Delay(750);

		await editor.ClickAsync(new()
		{
			ClickCount = 2,
			Position = new Position { X = 1, Y = 40 }
		});

		(selectionStart, selectionEnd) = await client.GetActiveSelectionAsync();
		Assert.That(selectionStart, Is.EqualTo(0));
		Assert.That(selectionEnd, Is.EqualTo(484));

		await Task.Delay(750);

		await editor.ClickAsync(new()
		{
			ClickCount = 3,
			Position = new Position { X = 1, Y = 40 }
		});

		(selectionStart, selectionEnd) = await client.GetActiveSelectionAsync();

		Assert.That(selectionStart, Is.EqualTo(0));
		Assert.That(selectionEnd, Is.EqualTo(1934));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxSelectsTextWhenClickingInGutterAtEndOfParagraph()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;

		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			richTextBox = new RichTextBox()
			{
				Width = 800,
				Height = 500,
				Html = $"<p><span {defaultEditorStyle}>wisetech global</span></p>"
				 + $"<p><span {defaultEditorStyle}>wisetech global</span></p>"
				 + $"<p><span {defaultEditorStyle}>wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech&nbsp;</span></p>"
				 + $"<p><span {defaultEditorStyle}>global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global wisetech global&nbsp;</span></p>"
			};
			return richTextBox;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		var editor = page.Locator(".richtextbox__editoranchor");

		var lastParagraph = await editor.GetByText("global").Last.BoundingBoxAsync();

		await editor.ClickAsync(new()
		{
			ClickCount = 1,
			Position = new Position { X = 1, Y = lastParagraph!.Y - 5 }
		});

		var (selectionStart, selectionEnd) = await client.GetActiveSelectionAsync();

		Assert.That(selectionStart, Is.EqualTo(416));
		Assert.That(selectionEnd, Is.EqualTo(538));

		await Task.Delay(750);

		await editor.ClickAsync(new()
		{
			ClickCount = 2,
			Position = new Position { X = 1, Y = lastParagraph!.Y - 5 }
		});

		(selectionStart, selectionEnd) = await client.GetActiveSelectionAsync();

		Assert.That(selectionStart, Is.EqualTo(32));
		Assert.That(selectionEnd, Is.EqualTo(537));

		await Task.Delay(750);

		await editor.ClickAsync(new()
		{
			ClickCount = 3,
			Position = new Position { X = 1, Y = lastParagraph!.Y - 5 }
		});

		(selectionStart, selectionEnd) = await client.GetActiveSelectionAsync();

		Assert.That(selectionStart, Is.EqualTo(0));
		Assert.That(selectionEnd, Is.EqualTo(1217));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxRgbToHexFallsBackToBlack()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
			};
			return richTextBox;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		Assert.That(await client.EvaluateAsync<string>("client => client.rgbToHex('blah');"), Is.EqualTo("#000000"));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxSizingIsRight()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var richTextBox = new RichTextBox()
			{
				Width = 800,
				Height = 800,
				IsToolBarVisible = true,
			};
			return richTextBox;
		});

		var rtbWrapper = page.Locator(".richtextbox");
		var toolbar = page.Locator(".richtextbox__toolbar");
		var editor = page.GetByRole(AriaRole.Textbox);
		await editor.WaitForAsync();

		Assert.That(await toolbar.IsVisibleAsync(), Is.True);
		Assert.That(await editor.IsVisibleAsync(), Is.True);

		var toolbarSize = await toolbar.BoundingBoxAsync();
		Assert.That(toolbarSize!.Height, Is.EqualTo(30));
		Assert.That(toolbarSize!.Width, Is.EqualTo(800));
		Assert.That(toolbarSize!.Y, Is.EqualTo(0));

		var anchorSize = await editor.BoundingBoxAsync();
		Assert.That(anchorSize!.Height, Is.EqualTo(770));
		Assert.That(anchorSize!.Width, Is.EqualTo(800));
		Assert.That(anchorSize!.Y, Is.EqualTo(30));

		var rtbSize = await rtbWrapper.BoundingBoxAsync();
		Assert.That(rtbSize!.Height, Is.EqualTo(800));
		Assert.That(rtbSize!.Width, Is.EqualTo(800));
		Assert.That(rtbSize!.Y, Is.EqualTo(0));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxToolbarIsHidden()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Text = "Hello!",
				IsToolBarVisible = false,
			};
			richTextBox.Multiline = false;
			return richTextBox;
		});
		var toolbar = page.Locator(".richtextbox__toolbar");
		Assert.That(await toolbar.IsVisibleAsync(), Is.False);
	}

	[Test, WithPlaywrightPage]
	[TestCase(true, "<p><span style=\"font-weight: bold; font-style: italic; text-decoration-line: underline;\">Hello!</span></p>", "formats should be applied", TestName = "{m}_WorkWhenEnabled")]
	[TestCase(false, "<p>Hello!</p>", "formats should not be applied", TestName = "{m}_DoNotWorkWhenDisabled")]
	public async Task RichTextBoxStyleShortcuts(bool enableStyleShortcuts, string expectedHtml, string message)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Text = "Hello!",
				EnableStyleShortcuts = enableStyleShortcuts
			};
			return richTextBox;
		});
		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await client.FocusEditorAsync();

		// Formatting shortcuts
		await editorBody.PressAsync("Control+A");
		await editorBody.PressAsync("Control+B"); // bold
		await editorBody.PressAsync("Control+I"); // italic
		await editorBody.PressAsync("Control+U"); // underscore

		var actualHtml = await client.GetActiveHtmlAsync();
		Assert.That(actualHtml, Is.EqualTo(expectedHtml), message);
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxToolbarReadonly()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var richTextBox = new RichTextBox()
			{
				Width = 500,
				Height = 500,
				Text = "Hello!",
				ReadOnly = true,
				IsToolBarVisible = true,
			};
			return richTextBox;
		});

		var toolbar = page.Locator(".richtextbox__toolbar");
		Assert.That(toolbar, Is.Not.Null);

		var buttons = await toolbar.Locator(".richtextbox__toolbar-item--button").AllAsync();
		Assert.That(buttons, Has.Count.GreaterThan(0));
		foreach (var button in buttons)
		{
			var isDisabled = await button.IsDisabledAsync();
			Assert.That(isDisabled, Is.True);
		}

		var fonts = page.GetByLabel("Fonts", new() { Exact = true });
		Assert.That(fonts, Is.Not.Null);
		var isFontsDisabled = await fonts.IsDisabledAsync();
		Assert.That(isFontsDisabled, Is.True);

		var fontsizes = page.GetByLabel("Font sizes", new() { Exact = true });
		Assert.That(fontsizes, Is.Not.Null);
		var isFontsizesDisabled = await fontsizes.IsDisabledAsync();
		Assert.That(isFontsizesDisabled, Is.True);

		var color = page.GetByLabel("Text color");
		Assert.That(color, Is.Not.Null);
		var pickerCount = await color.Locator("input").CountAsync();
		Assert.That(pickerCount, Is.Zero);
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxToolbarDoesNotWrap()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var richTextBox = new RichTextBox()
			{
				Width = 300,
				Height = 300,
				IsToolBarVisible = true,
			};
			return richTextBox;
		});

		var toolbar = page.Locator(".richtextbox__toolbar");
		Assert.That(toolbar, Is.Not.Null);

		var toolbarSize = await toolbar.BoundingBoxAsync();
		Assert.That(toolbarSize!.Height, Is.EqualTo(30));

		var toolbarButtons = page.Locator(".richtextbox__toolbar-item--button").Last;
		Assert.That((await toolbarButtons!.BoundingBoxAsync())!.Y, Is.EqualTo(0));

		Assert.That(await toolbar.GetComputedStyleAsync("display"), Is.EqualTo("flex"));
		Assert.That(await toolbar.GetComputedStyleAsync("flex-direction"), Is.EqualTo("row"));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxFontList()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox { IsToolBarVisible = true });
		var font = page.GetByTitle("Fonts", new() { Exact = true });
		Assert.That(font.Locator("option").CountAsync, Is.GreaterThanOrEqualTo(100).After(2000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxSelectionTest()
	{
		RichTextBox? richTextBox = null!;
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => richTextBox = new RichTextBox()
		{
			Width = 500,
			Height = 500,
			Text = "Test Content"
		});

		var contentLocator = page.Locator(".richtextbox__editoranchor");
		await contentLocator.WaitForAsync();

		var client = await RichTextBoxClient.GetClientAsync();
		Assert.That(client.GetActiveTextAsync, Is.EqualTo("Test Content"));

		await richTextBox.InvokeWinzorDispatcherAsync(() =>
		{
			richTextBox.SelectionStart = 0;
			richTextBox.SelectionLength = 4;
			richTextBox.SelectionColor = Color.Red;
		});
		Assert.That(client.GetEditorHtmlAsync, Is.EqualTo("<p><span style=\"color: rgb(255, 0, 0);\">Test</span> Content</p>").After(1000, 100));

		await richTextBox.InvokeWinzorDispatcherAsync(() =>
		{
			richTextBox.SelectionStart = 5;
			richTextBox.SelectionLength = 7;
			richTextBox.SelectionFont = new Font(FontFamily.GenericSansSerif, 20f);
		});
		Assert.That(client.GetEditorHtmlAsync, Is.EqualTo("<p><span style=\"color: rgb(255, 0, 0);\">Test</span> <span style=\"font-family: &quot;Microsoft Sans Serif&quot;; font-size: 20pt;\">Content</span></p>").After(1000, 100));

		await richTextBox.InvokeWinzorDispatcherAsync(() =>
		{
			richTextBox.SelectionStart = 0;
			richTextBox.SelectionLength = 4;
			richTextBox.SelectedHtml = WrapInP("add new line");
		});
		Assert.That(client.GetEditorHtmlAsync, Is.EqualTo($"<p>{WrapInSpan("add new line")} <span style=\"font-family: &quot;Microsoft Sans Serif&quot;; font-size: 20pt;\">Content</span></p>").After(1000, 100));
	}

	[Test]
	public async Task RichTextBoxPropertiesNotImplementedException()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			return new RichTextBox() { Text = "This Rich Text Box Is Read Only", ReadOnly = true };
		});
		var form = rendered.GetForm();
		var richTextBox = (RichTextBox)form.Controls[0];
#pragma warning disable CS0618 // Type or member is obsolete
		Assert.Throws<NotImplementedException>(() => richTextBox.SelectedRtf = "abc");
		Assert.Throws<NotImplementedException>(() =>
		{
			var rtf = richTextBox.SelectedRtf;
		});
		Assert.Throws<NotImplementedException>(() => richTextBox.SelectionBullet = false);
		Assert.Throws<NotImplementedException>(() =>
		{
			var rtf = richTextBox.SelectionBullet;
		});
		Assert.Throws<NotImplementedException>(() => richTextBox.SelectionIndent = 0);
		Assert.Throws<NotImplementedException>(() =>
		{
			var rtf = richTextBox.SelectionIndent;
		});
		Assert.Throws<NotImplementedException>(() => richTextBox.SelectionNumberedList = false);
		Assert.Throws<NotImplementedException>(() =>
		{
			var rtf = richTextBox.SelectionNumberedList;
		});
		Assert.Throws<NotImplementedException>(() => richTextBox.SelectionNumbered = false);
		Assert.Throws<NotImplementedException>(() =>
		{
			var rtf = richTextBox.SelectionNumbered;
		});
#pragma warning restore CS0618 // Type or member is obsolete
	}

	[Test, WithPlaywrightPage]
	public async Task HoverLinkHasCorrectCursor()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			richTextBox = new RichTextBox()
			{
				Width = 600,
				Height = 600,
				Html = @"<p>Text</p><p><a href=""https://example.com"">Link</a></p>"
			};
			form.Controls.Add(richTextBox);
			return form;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		await client.FocusEditorAsync();

		var link = await page.QuerySelectorAsync(".richtextbox__editoranchor a");
		Assert.That(await link!.EvaluateAsync<string>("e => window.getComputedStyle(e).cursor"), Is.EqualTo("pointer"));

		var text = await page.QuerySelectorAsync(".richtextbox__editoranchor p");
		Assert.That(await text!.EvaluateAsync<string>("e => window.getComputedStyle(e).cursor"), Is.EqualTo("auto"));
	}

	[Test, WithPlaywrightPage]
	[SuppressMessage("CargoWiseOne", "CW1104:Do not use System.Windows.Forms.TabControl Class", Justification = "Testing")]
	public async Task SwitchingTabsDoesNotRevertContent()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var tabControl = new TabControl();
			form.Controls.Add(tabControl);

			var tabPage1 = new TabPage { Name = "TabOne", Text = "Tab One" };
			tabPage1.Controls.Add(new RichTextBox() { Width = 500, Height = 500 });
			tabControl.TabPages.Add(tabPage1);

			var tabPage2 = new TabPage { Name = "TabTwo", Text = "Tab Two" };
			tabPage2.Controls.Add(new RichTextBox() { Width = 500, Height = 500 });
			tabControl.TabPages.Add(tabPage2);

			return form;
		});

		var text = "";

		foreach (var input in new string[] { "cool text!", "123", "abc", "456" })
		{
			var client = await RichTextBoxClient.GetClientAsync();
			Assert.That(client, Is.Not.Null);
			await client.FocusEditorAsync();
			await page.Keyboard.PressAsync("End");
			await page.Keyboard.TypeAsync(input);

			text += input;
			Assert.That(async () => await client.EvaluateAsync<string>("document.getElementsByClassName('richtextbox__editoranchor')[0].innerText;"), Is.EqualTo(text).After(2000, 100));

			var stubButton = await page.Locator("button", new PageLocatorOptions() { HasTextString = "Tab Two" }).ElementHandleAsync();
			await stubButton.ClickAsync();

			var richTextBoxButton = await page.Locator("button", new PageLocatorOptions() { HasTextString = "Tab One" }).ElementHandleAsync();
			await richTextBoxButton.ClickAsync();

			Assert.That(async () => await client.EvaluateAsync<string>("document.getElementsByClassName('richtextbox__editoranchor')[0].innerText;"), Is.EqualTo(text).After(2000, 100));
		}
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxSelectionChangeWhenReadOnly()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;

		await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			richTextBox = new RichTextBox()
			{
				Width = 250,
				Height = 250,
			};

			form.Controls.Add(richTextBox);
			return form;
		});

		var client = await RichTextBoxClient.GetClientAsync();

		await richTextBox.InvokeWinzorDispatcherAsync(() => richTextBox.ReadOnly = true);

		Assert.That(async () => await client.EvaluateAsync<string>(@"client => {
try {
  return (async () => {
	return await client.withBoundaryCheck(() => client.selectionChange());
  })();
} catch (exception) {
  return exception;
}
}"), Is.EqualTo("False").After(3000, 100));
	}

	[TestCase("Color")]
	[TestCase("Font")]
	[TestCase("Content")]
	[Test, WithPlaywrightPage]
	public async Task RichTextBoxSelectionIsNull(string type)
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;
		Exception exception = null!;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			richTextBox = new RichTextBox()
			{
				Width = 250,
				Height = 250,
				Text = "Text Color",
				ReadOnly = true
			};

			form.Controls.Add(richTextBox);
			Application.ThreadException += Application_ThreadException;
			return form;
		});

		void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
		{
			exception = e.Exception;
		}

		var client = await RichTextBoxClient.GetClientAsync();
		Assert.That(client, Is.Not.Null);

		await client.EvaluateAsync(@"client => {
				client.getDOMSelection().removeAllRanges();
			}");
		await richTextBox.InvokeWinzorDispatcherAsync(() =>
		{
			richTextBox.SelectionStart = 0;
			richTextBox.SelectionLength = 4;
			switch (type)
			{
				case "Color":
					richTextBox.SelectionColor = Color.Red;
					break;
				case "Font":
					richTextBox.SelectionFont = new Font(FontFamily.GenericSansSerif, 20f);
					break;
				case "Content":
					richTextBox.SelectedHtml = "RichTextBox";
					break;
			}
		});

		Assert.That(exception, Is.Null);
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxCanSupportAmnestyText()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		var datAmnestyFailure = new
		{
			E8_AssemblyName = "winzor.GUI.Test.dll",
			E2_TestClass = "vstest:System.Windows.Forms.ControlTest",
			E6_MethodName = "InvokeRenderDispatcherWithExceptionHandledAfterWinzorDispatcherDisposed",
			E6_PK = "http://crikey.wtg.zone/failures/testFailureHistory/2b2fbd80-8147-40b5-aad8-c54d3463f9cd"
		};
		var description = string.Format(Globalization.CultureInfo.InvariantCulture, "Assembly: {0}\r\nClass: {1}\r\nMethod: {2}\r\n", datAmnestyFailure.E8_AssemblyName, datAmnestyFailure.E2_TestClass, datAmnestyFailure.E6_MethodName)
			+ datAmnestyFailure.E6_PK
			+ "\r\n\r\n";

		var page = await ctx.LoadControlOnFormAsync(() => richTextBox = new RichTextBox()
		{
			Html = description,
			IsToolBarVisible = true,
			Height = 300,
			Width = 300
		});

		var client = await RichTextBoxClient.GetClientAsync();
		var editorContent = await client.EvaluateAsync<string>("client => client.getEditorContent()");

		var expectedText = "<p>Assembly: winzor.GUI.Test.dll</p>" +
			"<p>Class: vstest:System.Windows.Forms.ControlTest</p>" +
			"<p>Method: InvokeRenderDispatcherWithExceptionHandledAfterWinzorDispatcherDisposed</p>" +
			"<p><a href=\"http://crikey.wtg.zone/failures/testFailureHistory/2b2fbd80-8147-40b5-aad8-c54d3463f9cd\" target=\"_blank\" rel=\"noopener\">http://crikey.wtg.zone/failures/testFailureHistory/2b2fbd80-8147-40b5-aad8-c54d3463f9cd</a></p>" +
			"<p><br></p>" +
			"<p><br></p>";

		Assert.That(() => editorContent, Is.EqualTo(expectedText).After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxDragSelectedTextDoesNotThrowException()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox() { Text = "Some Text to Drag Upon" });
		var editor = page.Locator(".richtextbox__editoranchor");
		await editor.SelectTextAsync();
		await page.Mouse.MoveAsync(5, 5);
		await page.Mouse.DownAsync();
		Assert.That(() => page.Mouse.MoveAsync(10, 10), Throws.Nothing);
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxCanSelectTextRangeFromServer()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		var page = await ctx.LoadControlOnFormAsync(() => richTextBox = new RichTextBox() { Text = "WAIT FOR YOU" });
		Assert.That(async () => await page.Locator(".richtextbox__editoranchor").TextContentAsync(), Is.EqualTo("WAIT FOR YOU").After(3000, 100));

		await richTextBox.InvokeWinzorDispatcherAsync(() =>
		{
			richTextBox.SelectionStart = 1;
			richTextBox.SelectionLength = 10;
			richTextBox.SelectedText = "8 4 ";
		});
		Assert.That(async () => await page.Locator(".richtextbox__editoranchor").TextContentAsync(), Is.EqualTo("W8 4 U").After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxThrowsNoErrorWhenUnavailable()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		var page = await ctx.LoadControlOnFormAsync(() => richTextBox = new RichTextBox()
		{
			Width = 800,
			Height = 500,
			Text = "Test Content",
			IsToolBarVisible = true,
			ReadOnly = true,
		});

		var client = await RichTextBoxClient.GetClientAsync();
		Assert.That(client.GetActiveTextAsync, Is.EqualTo("Test Content").After(3000, 100));

		var editorContent = await richTextBox.Interop!.GetEditorContentAsync(richTextBox.WinzorControlId);
		Assert.That(await editorContent.GetContentAsync(), Is.EqualTo(null));
	}

	[Test]
	public async Task RichTextBoxOnModifiedUpdatesModifiedProperty()
	{
		using var ctx = new WinzorTestContext();
		var modifiedChangedEventWasFired = false;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var richTextBox = new RichTextBox();
			richTextBox.ModifiedChanged += (_, _) => modifiedChangedEventWasFired = true;
			return richTextBox;
		});

		var richTextBox = rendered.GetControl<RichTextBox>();
		Assert.Multiple(() =>
		{
			Assert.That(modifiedChangedEventWasFired, Is.False);
			Assert.That(richTextBox.Modified, Is.False);
		});

		await rendered.Find(".richtextbox").TriggerEventAsync("onmodified", new EventArgs());

		Assert.Multiple(() =>
		{
			Assert.That(modifiedChangedEventWasFired, Is.True);
			Assert.That(richTextBox.Modified, Is.True);
		});
	}

	[TestCaseSource(nameof(HtmlAndText))]
	public async Task RichTextBoxModifiedChangedNotFiredWhenProgrammaticallyUpdated(Action<RichTextBox, string> setContent)
	{
		using var ctx = new WinzorTestContext();
		var modifiedChangedEventWasFired = false;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var richTextBox = new RichTextBox();
			richTextBox.Modified = true;
			richTextBox.ModifiedChanged += (_, _) => modifiedChangedEventWasFired = true;
			return richTextBox;
		});

		var richTextBox = rendered.GetControl<RichTextBox>();
		Assert.That(richTextBox.Modified, Is.True);

		await richTextBox.InvokeWinzorDispatcherAsync(() =>
		{
			richTextBox.ModifiedChanged += (_, _) => modifiedChangedEventWasFired = true;
			setContent(richTextBox, "Update Content");
		});
		Assert.Multiple(() =>
		{
			Assert.That(modifiedChangedEventWasFired, Is.False);
			Assert.That(richTextBox.Modified, Is.False);
		});
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxModifiedEventRaisedForFirstInput()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBoxForTest richTextBox = null!;
		var page = await ctx.LoadControlOnFormAsync(() => richTextBox = new RichTextBoxForTest());

		var editor = page.Locator(".richtextbox__editoranchor");
		await editor.SelectTextAsync();

		Assert.Multiple(() =>
		{
			Assert.That(richTextBox.Modified, Is.False);
			Assert.That(richTextBox.ContentIsOutOfSyncWithClient, Is.False);
			Assert.That(richTextBox.OnModifiedCount, Is.EqualTo(0));
		});

		await page.Keyboard.TypeAsync("I am writing some text.");

		Assert.Multiple(() =>
		{
			Assert.That(() => richTextBox.Modified, Is.True.After(3000, 100));
			Assert.That(() => richTextBox.ContentIsOutOfSyncWithClient, Is.True.After(3000, 100));
			Assert.That(richTextBox.OnModifiedCount, Is.EqualTo(1));
		});

		await editor.BlurAsync();

		Assert.Multiple(() =>
		{
			Assert.That(richTextBox.Modified, Is.True);
			Assert.That(() => richTextBox.ContentIsOutOfSyncWithClient, Is.False.After(3000, 100));
			Assert.That(richTextBox.OnModifiedCount, Is.EqualTo(1));
		});
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxModifiedEventRaisedForFirstInputAfterSendingData()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBoxForTest richTextBox = null!;
		var page = await ctx.LoadControlOnFormAsync(() => richTextBox = new RichTextBoxForTest { Text = "This editor already has some text in it..." });

		var editor = page.Locator(".richtextbox__editoranchor");
		await editor.FocusAsync();
		await editor.SelectTextAsync();
		await page.Keyboard.PressAsync("ArrowRight");

		Assert.Multiple(() =>
		{
			Assert.That(richTextBox.Modified, Is.False);
			Assert.That(richTextBox.ContentIsOutOfSyncWithClient, Is.False);
			Assert.That(richTextBox.OnModifiedCount, Is.EqualTo(0));
		});

		await page.Keyboard.TypeAsync("and I am adding some more!");

		Assert.Multiple(() =>
		{
			Assert.That(() => richTextBox.Modified, Is.True.After(3000, 100));
			Assert.That(() => richTextBox.ContentIsOutOfSyncWithClient, Is.True.After(3000, 100));
			Assert.That(richTextBox.OnModifiedCount, Is.EqualTo(1));
		});

		await editor.BlurAsync();

		Assert.Multiple(() =>
		{
			Assert.That(richTextBox.Modified, Is.True);
			Assert.That(() => richTextBox.ContentIsOutOfSyncWithClient, Is.False.After(3000, 100));
			Assert.That(richTextBox.OnModifiedCount, Is.EqualTo(1));
		});

		await editor.FocusAsync();
		await editor.SelectTextAsync();
		await page.Keyboard.PressAsync("ArrowRight");

		await page.Keyboard.TypeAsync(" Here is even more text.");

		Assert.Multiple(() =>
		{
			Assert.That(() => richTextBox.Modified, Is.True.After(3000, 100));
			Assert.That(() => richTextBox.ContentIsOutOfSyncWithClient, Is.True.After(3000, 100));
			Assert.That(richTextBox.OnModifiedCount, Is.EqualTo(2));
		});
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxModifiedEventRaisedForFirstInputAfterRecievingData()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBoxForTest richTextBox = null!;
		var page = await ctx.LoadControlOnFormAsync(() => richTextBox = new RichTextBoxForTest() { Text = "This editor already has some text in it..." });

		var editor = page.Locator(".richtextbox__editoranchor");
		await editor.FocusAsync();
		await editor.SelectTextAsync();
		await page.Keyboard.PressAsync("ArrowRight");

		Assert.Multiple(() =>
		{
			Assert.That(richTextBox.Modified, Is.False);
			Assert.That(richTextBox.ContentIsOutOfSyncWithClient, Is.False);
			Assert.That(richTextBox.OnModifiedCount, Is.EqualTo(0));
		});

		await page.Keyboard.TypeAsync("and I am adding some more!");

		Assert.Multiple(() =>
		{
			Assert.That(() => richTextBox.Modified, Is.True.After(3000, 100));
			Assert.That(() => richTextBox.ContentIsOutOfSyncWithClient, Is.True.After(3000, 100));
			Assert.That(richTextBox.OnModifiedCount, Is.EqualTo(1));
		});

		await richTextBox.InvokeWinzorDispatcherAsync(() => richTextBox.Text = "The server is updating the text value...");

		Assert.Multiple(() =>
		{
			Assert.That(() => richTextBox.Modified, Is.False.After(3000, 100));
			Assert.That(() => richTextBox.ContentIsOutOfSyncWithClient, Is.False.After(3000, 100));
			Assert.That(richTextBox.OnModifiedCount, Is.EqualTo(1));
		});

		await editor.FocusAsync();
		await editor.SelectTextAsync();
		await page.Keyboard.PressAsync("ArrowRight");
		await page.Keyboard.TypeAsync(" thanks for the update server");

		Assert.Multiple(() =>
		{
			Assert.That(() => richTextBox.Modified, Is.True.After(3000, 100));
			Assert.That(() => richTextBox.ContentIsOutOfSyncWithClient, Is.True.After(3000, 100));
			Assert.That(richTextBox.OnModifiedCount, Is.EqualTo(2));
		});
	}

	[Test, WithPlaywrightPage, Explicit]
	public async Task RichTextBoxOutOfSyncWithClientRemainsCorrectWhenModifiedEventsIsBeforeChangeEvent()
	{
		/*
		 * This test is to handle an important edge case where the server client fires a modified event, a change event and a new modified event
		 * As the change event can be slow due to loading the stream content, it's possible that the server will see the 2 modified events before the change event
		 * We need to handle this case so the server is aware that even though it just received client state, new changes were made during the time it took to reach the server
		 */
		await using var ctx = new InMemoryTestServerContext();
		RichTextBoxForTest richTextBox = null!;
		var page = await ctx.LoadControlOnFormAsync(() => richTextBox = new RichTextBoxForTest() { Text = "This editor already has some text in it..." });
		Assert.That(richTextBox, Is.Not.Null);

		// We will use a task completion source to simulate a stream that is slow to read
		//richTextBox.OnContentChangedTcs = new ();

		var editor = page.Locator(".richtextbox__editoranchor");
		await editor.FocusAsync();
		await editor.SelectTextAsync();
		await page.Keyboard.TypeAsync("This is my edited content");

		Assert.Multiple(() =>
		{
			Assert.That(() => richTextBox.Modified, Is.True.After(3000, 100));
			Assert.That(() => richTextBox.ContentIsOutOfSyncWithClient, Is.True.After(3000, 100));
			Assert.That(() => richTextBox.OnModifiedCount, Is.EqualTo(1).After(3000, 100));
		});

		await editor.BlurAsync();

		// While the change event is still processing, make another change
		await editor.FocusAsync();
		await editor.SelectTextAsync();
		await page.Keyboard.TypeAsync("This is my NEW edited content");

		Assert.Multiple(() =>
		{
			Assert.That(richTextBox.Modified, Is.True);
			Assert.That(richTextBox.ContentIsOutOfSyncWithClient, Is.True);
			Assert.That(richTextBox.OnModifiedCount, Is.EqualTo(2));
		});

		// Let the first change event fire
		//richTextBox.OnContentChangedTcs.SetResult();

		Assert.That(() => richTextBox.Html, Is.EqualTo("<p>This is my edited content</p>").After(3000, 100));
		Assert.Multiple(() =>
		{
			Assert.That(richTextBox.Modified, Is.True);
			Assert.That(richTextBox.ContentIsOutOfSyncWithClient, Is.True);
			Assert.That(richTextBox.OnModifiedCount, Is.EqualTo(2));
		});

		await richTextBox.InvokeWinzorDispatcherAsync(() => richTextBox.TryUpdateValueFromClient());

		Assert.Multiple(() =>
		{
			Assert.That(richTextBox.Modified, Is.True);
			Assert.That(richTextBox.ContentIsOutOfSyncWithClient, Is.False);
			Assert.That(richTextBox.OnModifiedCount, Is.EqualTo(2));
			Assert.That(richTextBox.Html, Is.EqualTo("<p>This is my NEW edited content</p>"));
		});
	}

	[TestCaseSource(nameof(SampleTextList)), WithPlaywrightPage]
	public async Task RichTextBoxGetEditorContentShouldReturnContent(string largeText)
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		var page = await ctx.LoadControlOnFormAsync(() => richTextBox = new RichTextBox() { Text = largeText });

		_ = page.Locator(".richtextbox__editoranchor");
		var client = await RichTextBoxClient.GetClientAsync();
		Assert.That(client.GetActiveTextAsync, Is.EqualTo(largeText).After(3000, 100));

		var editorContent = await richTextBox.Interop!.GetEditorContentAsync(richTextBox.WinzorControlId);
		Assert.That(await editorContent.GetContentAsync(), Is.EqualTo($"<p>{largeText}</p>"));
	}

	[Test]
	public async Task RichTextBoxForceUpdateValueWhileContentIsOutOfSyncWithClientUpdatesContentOnServer()
	{
		using var ctx = new WinzorTestContext();
		var path = "/_content/WinzorFramework/js/module/richTextBox.js";
		var richTextBoxModule = ctx.JSInterop.SetupModule(path);
		richTextBoxModule.Setup<EditorContent>("getEditorContent", new InvocationMatcher(_ => true))
			.SetResult(new EditorContent(WrapInP("This is the new text"), null, 0));

		var textChanged = false;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var richTextBox = new RichTextBox
			{
				Font = new Font("Tahoma", 20, GraphicsUnit.Pixel),
				Text = "This is the old text"
			};
			richTextBox.TextChanged += (_, _) => textChanged = true;
			return richTextBox;
		});
		var richTextBox = rendered.GetControl<RichTextBox>();

		await rendered.Find(".richtextbox").TriggerEventAsync("onmodified", new EventArgs());

		Assert.Multiple(() =>
		{
			Assert.That(richTextBox.Modified, Is.True);
			Assert.That(richTextBox.ContentIsOutOfSyncWithClient, Is.True);
			Assert.That(ctx.JSInterop.Invocations.Select(i => i.Identifier), Has.Exactly(0).EqualTo("getEditorContent"));
			Assert.That(textChanged, Is.False);
		});

		var contentBefore = string.Empty;
		var contentAfter = string.Empty;
		await richTextBox.InvokeWinzorDispatcherAsync(() =>
		{
			contentBefore = richTextBox.Html;
			richTextBox.TryUpdateValueFromClient();
			contentAfter = richTextBox.Html;
		});

		Assert.Multiple(() =>
		{
			Assert.That(richTextBox.Modified, Is.True);
			Assert.That(richTextBox.ContentIsOutOfSyncWithClient, Is.False);
			Assert.That(ctx.JSInterop.Invocations.Select(i => i.Identifier), Has.Exactly(1).EqualTo("getEditorContent"));
			Assert.That(contentBefore, Is.EqualTo("<p>This is the old text</p>"));
			Assert.That(contentAfter, Is.EqualTo(WrapInP("This is the new text")));
			Assert.That(textChanged, Is.True);
		});
	}

	[Test]
	public async Task RichTextBoxForceUpdateValueWhileContentSyncedWithClientDoesNotInvokeJS()
	{
		using var ctx = new WinzorTestContext();
		var textChanged = false;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var richTextBox = new RichTextBox { Text = "This is a rich text box" };
			richTextBox.TextChanged += (_, _) => textChanged = true;
			return richTextBox;
		});
		var richTextBox = rendered.GetControl<RichTextBox>();

		Assert.Multiple(() =>
		{
			Assert.That(richTextBox.Modified, Is.False);
			Assert.That(richTextBox.ContentIsOutOfSyncWithClient, Is.False);
			Assert.That(ctx.JSInterop.Invocations.Select(i => i.Identifier), Has.Exactly(0).EqualTo("getEditorContent"));
			Assert.That(textChanged, Is.False);
		});

		await richTextBox.InvokeWinzorDispatcherAsync(() => richTextBox.TryUpdateValueFromClient());

		Assert.Multiple(() =>
		{
			Assert.That(richTextBox.Modified, Is.False);
			Assert.That(richTextBox.ContentIsOutOfSyncWithClient, Is.False);
			Assert.That(ctx.JSInterop.Invocations.Select(i => i.Identifier), Has.Exactly(0).EqualTo("getEditorContent"));
			Assert.That(textChanged, Is.False);
		});
	}

	static IEnumerable<TestCaseData> HtmlAndText
	{
		get
		{
			yield return new TestCaseData(
				(RichTextBox richTextBox, string value) =>
				{
					richTextBox.Text = value;
				})
			{ TestName = "{m}_Text" };
			yield return new TestCaseData(
				(RichTextBox richTextBox, string value) =>
				{
					richTextBox.Html = $"<p>{value}</p>";
				})
			{ TestName = "{m}_Html" };
		}
	}

	[TestCaseSource(nameof(HtmlAndText))]
	public async Task RichTextBoxShouldNotFireTextChangedEventBeforeControlCreated(Action<RichTextBox, string> setContent)
	{
		using var ctx = new WinzorTestContext();
		RichTextBox? richTextBox = null;
		var textChangedRaised = false;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			richTextBox = new RichTextBox();
			richTextBox.TextChanged += (_, _) => textChangedRaised = true;
			setContent(richTextBox, "this is some content");
		});

		Assert.That(richTextBox, Is.Not.Null);
		Assert.That(textChangedRaised, Is.False);

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			richTextBox!.CreateControl();
			setContent(richTextBox, "this is some different content");
		});
		Assert.That(textChangedRaised, Is.True);
	}

	[TestCaseSource(nameof(HtmlAndText))]
	public async Task RichTextBoxSetSameContentShouldNotUpdateClientWhenNoClientChanges(Action<RichTextBox, string> setContent)
	{
		using var ctx = new WinzorTestContext();
		var path = "/_content/WinzorFramework/js/module/richTextBox.js";
		var richTextBoxModule = ctx.JSInterop.SetupModule(path);
		richTextBoxModule.SetupVoid("setEditorContent", new InvocationMatcher(_ => true));

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var richTextBox = new RichTextBox();
			setContent(richTextBox, "this is some content");
			return richTextBox;
		});
		var richTextBox = rendered.GetControl<RichTextBox>();

		Assert.That(ctx.JSInterop.Invocations.Select(i => i.Identifier), Has.Exactly(0).EqualTo("setEditorContent"));

		await richTextBox.InvokeWinzorDispatcherAsync(() => setContent(richTextBox, "this is some content"));
		Assert.That(ctx.JSInterop.Invocations.Select(i => i.Identifier), Has.Exactly(0).EqualTo("setEditorContent"));
	}

	[TestCaseSource(nameof(HtmlAndText))]
	public async Task RichTextBoxSetSameContentShouldUpdateClientWhenClientHasChanges(Action<RichTextBox, string> setContent)
	{
		using var ctx = new WinzorTestContext();
		var path = "/_content/WinzorFramework/js/module/richTextBox.js";
		var richTextBoxModule = ctx.JSInterop.SetupModule(path);
		richTextBoxModule.SetupVoid("setEditorContent", new InvocationMatcher(_ => true));

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var richTextBox = new RichTextBox();
			setContent(richTextBox, "this is some content");
			return richTextBox;
		});
		var richTextBox = rendered.GetControl<RichTextBox>();

		Assert.That(ctx.JSInterop.Invocations.Select(i => i.Identifier), Has.Exactly(0).EqualTo("setEditorContent"));

		await rendered.Find(".richtextbox").TriggerEventAsync("onmodified", new EventArgs());
		await richTextBox.InvokeWinzorDispatcherAsync(() => setContent(richTextBox, "this is some content"));
		Assert.That(ctx.JSInterop.Invocations.Select(i => i.Identifier), Has.Exactly(1).EqualTo("setEditorContent"));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxCopyShouldNotExecCommand()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		var page = await ctx.LoadControlOnFormAsync(() => richTextBox = new RichTextBox() { Text = "I want to cut this text" });

		var editor = page.Locator(".richtextbox__editoranchor");
		await editor.SelectTextAsync();

		await page.AttachMockClipboardWrite();

		await page.EvaluateAsync(@"document.execCommandCore = document.execCommand;
execCommandInvocations = [];
document.execCommand = (...args) => {
	execCommandInvocations.push(args[0]);
	document.execCommandCore(args);
}");

		await richTextBox.InvokeWinzorDispatcherAsync(() => richTextBox.Copy());
		Assert.That(async () => await page.EvaluateAsync<string[]>("execCommandInvocations"), Has.None.EqualTo("copy").After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxShouldDiscardUnwantedStylesClientSide()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		var page = await ctx.LoadControlOnFormAsync(() => richTextBox = new RichTextBox()
		{
			IsToolBarVisible = true
		});
		await page.MockClipboardRead(new[] {
			new JSClipboardData("text/html", @"<p><span style=""width: auto; color: inherit; font-weight: bold;"">Text</span></p>"),
			new JSClipboardData("text/plain", "Not requested"),
		});

		var editor = page.GetByRole(AriaRole.Textbox);
		await editor.FocusAsync();
		await richTextBox.InvokeWinzorDispatcherAsync(() => richTextBox.Paste());

		Assert.That(async () => await editor.InnerHTMLAsync(), Is.EqualTo("<p><span style=\"font-weight: bold;\">Text</span></p>").After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxCutShouldExecCommand()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		var page = await ctx.LoadControlOnFormAsync(() => richTextBox = new RichTextBox() { Text = "I want to cut this text" });

		var editor = page.Locator(".richtextbox__editoranchor");
		await editor.SelectTextAsync();
		Assert.That(async () => await editor.InnerHTMLAsync(), Is.EqualTo("<p>I want to cut this text</p>"));

		await page.AttachMockClipboardWrite();

		await page.EvaluateAsync(@"document.execCommandCore = document.execCommand;
execCommandInvocations = [];
document.execCommand = (...args) => {
	execCommandInvocations.push(args[0]);
	document.execCommandCore(args);
}");

		await richTextBox.InvokeWinzorDispatcherAsync(() => richTextBox.Cut());

		Assert.That(async () => await page.EvaluateAsync<string[]>("execCommandInvocations"), Has.One.EqualTo("delete"));
		Assert.That(async () => await editor.InnerHTMLAsync(), Is.EqualTo("<p><br></p>").After(3000, 100));
	}

	static IEnumerable<TestCaseData> PasteTestCases
	{
		get
		{
			yield return new TestCaseData(new[] {
				new JSClipboardData("text/html", "<aside>This is some <markup>weird</markup> content</aside>"),
				new JSClipboardData("text/plain", "This is the plain text content which should NOT be pasted"),
			}, "insertHTML", "<p>This is some </p><p>weird</p><p> content</p><p><br></p>")
			{ TestName = "{m}_InsertsFilteredMarkup" };
			yield return new TestCaseData(new[] {
				new JSClipboardData("text/html", "<p><span style=\"background-color: red; color: var(--red); border: 1px solid var(--green); accent-color: pink;\">The</span> <span style=\"cursor: pointer;\">origin</span> <span style=\"background-color: var(--red); color: red; border: 1px solid green;\">contains</span> bad styling.</p>"),
				new JSClipboardData("text/plain", "This is the plain text content which should NOT be pasted"),
			}, "insertHTML", "<p><span style=\"background-color: red;\">The</span> origin <span style=\"color: red; border-width: 1px; border-style: solid; border-color: green;\">contains</span> bad styling.</p>")
			{ TestName = "{m}_FiltersBadStyling" };
			yield return new TestCaseData(new[] {
				new JSClipboardData("text/html", "<img src=\"\" onerror=\"alert('Danger!')\">"),
				new JSClipboardData("text/plain", "This is the plain text content which should NOT be pasted"),
			}, "insertHTML", "<p><br></p>")
			{ TestName = "{m}_FiltersBadAttributes_HTML" };
			yield return new TestCaseData(new[] {
				new JSClipboardData("text/html", "<p style=\"text-align: center;\">Centred text</p><p style=\"text-align: justify;\">Justified text</p>")
			}, "insertHTML", "<p style=\"text-align: center;\">Centred text</p><p>Justified text</p>")
			{ TestName = "{m}_FiltersBadAlignment" };
			yield return new TestCaseData(new[] {
				new JSClipboardData("text/plain", "<img src=\"\" onerror=\"alert('Danger!')\">"),
			}, "insertHTML", "<p>&lt;img src=\"\" onerror=\"alert('Danger!')\"&gt;</p>")
			{ TestName = "{m}_Cleans_Text" };
			yield return new TestCaseData(new[] {
				new JSClipboardData("text/html", "<p>This is some HTML content</p>"),
				new JSClipboardData("text/plain", "This is the plain text content which should NOT be pasted"),
			}, "insertHTML", "<p>This is some HTML content</p>")
			{ TestName = "{m}_WithHTMLandText_HTMLShouldBeInserted" };
			yield return new TestCaseData(new[] {
				new JSClipboardData("text/html", "<p><span style=\"font-size: 13.3333px; font-style: italic; font-weight: 700; text-align: left;\">This is text with some formatting!</span></p>"),
				new JSClipboardData("text/plain", "This is the not the content which should be pasted"),
			}, "insertHTML", "<p><span style=\"font-size: 13.3333px; font-style: italic; font-weight: 700; text-align: left;\">This is text with some formatting!</span></p>")
			{ TestName = "{m}_WithFormattedHTMLandText_FormattedHTMLShouldBeInserted" };
			yield return new TestCaseData(new[] {
				new JSClipboardData("text/html", "<p><span style=\"font-size: 13.3333px; font-style: italic; font-weight: 700; text-align: left;\">This is text with some formatting!</span></p>"),
			}, "insertHTML", "<p><span style=\"font-size: 13.3333px; font-style: italic; font-weight: 700; text-align: left;\">This is text with some formatting!</span></p>")
			{ TestName = "{m}_WithHTML_HTMLShouldBeInserted" };
			yield return new TestCaseData(new[] {
				new JSClipboardData("text/plain", "This is the plain text which should be pasted"),
			}, "insertHTML", "<p>This is the plain text which should be pasted</p>")
			{ TestName = "{m}_WithText_TextShouldBeInserted" };
		}
	}

	[TestCaseSource(nameof(PasteTestCases)), WithPlaywrightPage]
	public async Task RichTextBoxPaste(JSClipboardData[] clipboardData, string expectedCommand, string expectedContent)
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		var page = await ctx.LoadControlOnFormAsync(() => richTextBox = new RichTextBox()
		{
			Html = WrapInP("I want to paste some content here"),
			IsToolBarVisible = true, // pasting with formatting only works with the toolbar showing
		});
		var editor = page.Locator(".richtextbox__editoranchor");
		Assert.That(async () => await editor.InnerHTMLAsync(), Is.EqualTo(WrapInP("I want to paste some content here")).After(3000, 100));

		// Mock the clipboard read API as clipboard is unreliable in DAT
		await page.MockClipboardRead(clipboardData);
		await page.AttachExecCommandListener();

		await page.EvaluateAsync("window.alerts = []; window.alert = (messages) => { window.alerts.push(messages); }");

		await editor.FocusAsync();
		await editor.SelectTextAsync();
		await richTextBox.InvokeWinzorDispatcherAsync(() => richTextBox.Paste());
		Assert.That(async () => await page.ExecCommandInvocations(), Has.One.EqualTo(expectedCommand).After(3000, 100));
		Assert.That(async () => await editor.InnerHTMLAsync(), Is.EqualTo(expectedContent).After(3000, 100));

		// For some insane reason, some code somewhere appears to be running `alert(null)`
		Assert.That(async () => await page.EvaluateAsync<string>("JSON.stringify(window.alerts)"), Is.EqualTo("[null]"));
	}

	[Test, WithPlaywrightPage]
	[TestCase("<p>a</p><p>b</p>", "<p>a</p><p>b</p>", TestName = "{m}_Multiline")]
	[TestCase("<p>a</p>\r\n<p>b</p>\r\n", "<p>a</p><p>b</p>", TestName = "{m}_Multiline_WithLineBreak")]
	[TestCase("<p>a</p><p><br></p><p>b</p>", "<p>a</p><p><br></p><p>b</p>", TestName = "{m}_Multiline_WithOneEmptyLine")]
	[TestCase("<p>a</p><p><br></p><p><br></p><p>b</p>", "<p>a</p><p><br></p><p><br></p><p>b</p>", TestName = "{m}_Multiline_WithTwoEmptyLines")]
	[TestCase("<p>Line 1</p><p>Line 2</p>", "<p>Line 1</p><p>Line 2</p>", TestName = "{m}_Multiline_WithNoEmptyLines")]
	public async Task CopyPasteShouldNotProduceExtraLineBreakElement(string inputHtml, string expectedHtml)
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		var page = await ctx.LoadControlOnFormAsync(() => richTextBox = new RichTextBox() { IsToolBarVisible = true });
		await page.AttachMockClipboardWrite();
		var editor = page.GetByRole(AriaRole.Textbox);
		await editor.EvaluateAsync("(elm, inputHtml) => elm.innerHTML = inputHtml", inputHtml);
		await editor.SelectTextAsync();
		await editor.FakeClipboardCopy();
		await editor.FakeClipboardPaste("text/html");

		Assert.That(async () => await editor.InnerHTMLAsync(), Is.EqualTo(expectedHtml).After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	[TestCase("<p>a</p><p>b</p>", "a\nb", TestName = "{m}_Multiline")]
	[TestCase("<p>a</p>\r\n<p>b</p>\r\n", "a\nb", TestName = "{m}_Multiline_WithLineBreak")]
	[TestCase("<p>a</p><p><br></p><p>b</p>", "a\n\nb", TestName = "{m}_Multiline_WithOneEmptyLine")]
	[TestCase("<p>a</p><p><br></p><p><br></p><p>b</p>", "a\n\n\nb", TestName = "{m}_Multiline_WithTwoEmptyLines")]
	[TestCase("<p>Line 1</p><p>Line 2</p>", "Line 1\nLine 2", TestName = "{m}_Multiline_WithNoEmptyLines")]
	public async Task CopyPastePlainShouldNotProduceExtraLineBreakElement(string inputHtml, string expectedText)
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		var page = await ctx.LoadControlOnFormAsync(() => richTextBox = new RichTextBox() { IsToolBarVisible = true });
		await page.AttachMockClipboardWrite();

		var client = await RichTextBoxClient.GetClientAsync();

		var editor = page.GetByRole(AriaRole.Textbox);
		await editor.EvaluateAsync("(elm, inputHtml) => elm.innerHTML = inputHtml", inputHtml);
		await editor.SelectTextAsync();
		await editor.FakeClipboardCopy();
		await editor.FakeClipboardPaste("text/plain");

		Assert.That(async () => await client.GetActiveTextAsync(), Is.EqualTo(expectedText));
	}

	[Test, WithPlaywrightPage]
	[TestCase("<p><strong>Lorem</strong> ipsum</p>", 0, 5, $"<p><strong>Lorem</strong> ipsum<strong {defaultEditorStyle}>Lorem</strong></p>", TestName = "{m}_Strong_1")]
	[TestCase("<p><strong>Lorem</strong> ipsum</p>", 1, 5, $"<p><strong>Lorem</strong> ipsum<strong {defaultEditorStyle}>orem</strong></p>", TestName = "{m}_Strong_2")]
	[TestCase("<p><strong>Lorem</strong> ipsum</p>", 1, 8, $"<p><strong>Lorem</strong> ipsum<strong {defaultEditorStyle}>orem</strong><span {defaultEditorStyle}> ip</span></p>", TestName = "{m}_Strong_3")]
	[TestCase("<p><b>Lorem</b> ipsum</p>", 0, 4, $"<p><b>Lorem</b> ipsum<b {defaultEditorStyle}>Lore</b></p>", TestName = "{m}_Bold_1")]
	[TestCase("<p><b>Lorem</b> ipsum</p>", 1, 4, $"<p><b>Lorem</b> ipsum<b {defaultEditorStyle}>ore</b></p>", TestName = "{m}_Bold_2")]
	[TestCase("<p><em>Lorem</em> ipsum</p>", 0, 5, $"<p><em>Lorem</em> ipsum<em {defaultEditorStyle}>Lorem</em></p>", TestName = "{m}_Emphasis_1")]
	[TestCase("<p><em>Lorem</em> ipsum</p>", 1, 5, $"<p><em>Lorem</em> ipsum<em {defaultEditorStyle}>orem</em></p>", TestName = "{m}_Emphasis_2")]
	[TestCase("<p><i>Lorem</i> ipsum</p>", 0, 4, $"<p><i>Lorem</i> ipsum<i {defaultEditorStyle}>Lore</i></p>", TestName = "{m}_Italic_1")]
	[TestCase("<p><i>Lorem</i> ipsum</p>", 1, 4, $"<p><i>Lorem</i> ipsum<i {defaultEditorStyle}>ore</i></p>", TestName = "{m}_Italic_2")]
	[TestCase("<p><u>Lorem</u> ipsum</p>", 0, 4, $"<p><u>Lorem</u> ipsum<u {defaultEditorStyle}>Lore</u></p>", TestName = "{m}_Underline_1")]
	[TestCase("<p><u>Lorem</u> ipsum</p>", 1, 4, $"<p><u>Lorem</u> ipsum<u {defaultEditorStyle}>ore</u></p>", TestName = "{m}_Underline_2")]
	[TestCase("<p><a href=\"https://www.wisetechglobal.com\" target=\"_blank\" rel=\"noopener\">WiseTech</a> Global</p>", 0, 8, $"<p><a href=\"https://www.wisetechglobal.com\" target=\"_blank\" rel=\"noopener\">WiseTech</a> Global<a href=\"https://www.wisetechglobal.com\" target=\"_blank\" rel=\"noopener\" {defaultEditorStyle}>WiseTech</a></p>", TestName = "{m}_Link1")]
	[TestCase("<p><a href=\"https://www.wisetechglobal.com\" target=\"_blank\" rel=\"noopener\">WiseTech</a> Global</p>", 1, 8, $"<p><a href=\"https://www.wisetechglobal.com\" target=\"_blank\" rel=\"noopener\">WiseTech</a> Global<a href=\"https://www.wisetechglobal.com\" target=\"_blank\" rel=\"noopener\" {defaultEditorStyle}>iseTech</a></p>", TestName = "{m}_Link2")]
	[TestCase("<p><a href=\"https://www.wisetechglobal.com\" target=\"_blank\" rel=\"noopener\">WiseTech</a> Global</p>", 1, 7, $"<p><a href=\"https://www.wisetechglobal.com\" target=\"_blank\" rel=\"noopener\">WiseTech</a> Global<a href=\"https://www.wisetechglobal.com\" target=\"_blank\" rel=\"noopener\" {defaultEditorStyle}>iseTec</a></p>", TestName = "{m}_Link3")]
	[TestCase("<p><a href=\"https://www.wisetechglobal.com\" target=\"_blank\" rel=\"noopener\">WiseTech</a> Global</p>", 0, 12, $"<p><a href=\"https://www.wisetechglobal.com\" target=\"_blank\" rel=\"noopener\">WiseTech</a> Global<a href=\"https://www.wisetechglobal.com\" target=\"_blank\" rel=\"noopener\" {defaultEditorStyle}>WiseTech</a><span {defaultEditorStyle}> Glo</span></p>", TestName = "{m}_Link4")]
	[TestCase("<p><a href=\"https://www.wisetechglobal.com\" target=\"_blank\" rel=\"noopener\">WiseTech</a> Global</p>", 1, 12, $"<p><a href=\"https://www.wisetechglobal.com\" target=\"_blank\" rel=\"noopener\">WiseTech</a> Global<a href=\"https://www.wisetechglobal.com\" target=\"_blank\" rel=\"noopener\" {defaultEditorStyle}>iseTech</a><span {defaultEditorStyle}> Glo</span></p>", TestName = "{m}_Link5")]
	public async Task CopyPasteShouldMaintainRichText(string inputHtml, int selectionStart, int selectionEnd, string expectedHtml)
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		var page = await ctx.LoadControlOnFormAsync(() => richTextBox = new RichTextBox() { IsToolBarVisible = true });

		await page.AttachMockClipboardWrite();
		var editor = page.GetByRole(AriaRole.Textbox);
		await editor.EvaluateAsync("(elm, inputHtml) => elm.innerHTML = inputHtml", inputHtml);

		var client = await RichTextBoxClient.GetClientAsync();

		await client.EvaluateAsync($@"client => {{
			client.setSelection({{ start: {selectionStart}, end: {selectionEnd}}});
		}}");

		await editor.FakeClipboardPaste("text/plain");
		await editor.FakeClipboardCopy();

		await page.Keyboard.PressAsync("Control+End");

		await editor.FakeClipboardPaste("text/html");
		Assert.That(async () => await editor.InnerHTMLAsync(), Is.EqualTo(expectedHtml));
	}

	[Test, WithPlaywrightPage]
	[TestCase("<p><span style=\"font-weight: bold; color: #f00;\">Lo<span style=\"text-decoration: underline;\">rem</span></span> ipsum</p>", 0, 4, "<p><span style=\"font-weight: bold; color: #f00;\">Lo<span style=\"text-decoration-line: underline;\">rem</span></span> ipsum<span style=\"color: rgb(255, 0, 0); font-weight: bold;\">Lo</span><span style=\"color: rgb(255, 0, 0); font-weight: bold; text-decoration-line: underline;\">re</span></p>", TestName = "{m}_Complex_1")]
	[TestCase("<p><span style=\"font-weight: bold; color: #f00;\">Lo<span style=\"text-decoration: underline;\">rem</span></span> ipsum</p>", 1, 4, $"<p><span style=\"font-weight: bold; color: #f00;\">Lo<span style=\"text-decoration-line: underline;\">rem</span></span> ipsum<span {defaultEditorStyle}>o</span><span style=\"{defaultEditorFontStyle} text-decoration-line: underline;\">re</span></p>", TestName = "{m}_Complex_2")]
	[TestCase("<p><span style=\"font-weight: bold; color: #f00;\">Lorem</span> ipsum</p>", 0, 4, "<p><span style=\"font-weight: bold; color: #f00;\">Lorem</span> ipsum<span style=\"color: rgb(255, 0, 0); font-weight: bold;\">Lore</span></p>", TestName = "{m}_Simple_1")]
	[TestCase("<p><span style=\"font-weight: bold; color: #f00;\">Lorem</span> ipsum</p>", 1, 4, "<p><span style=\"font-weight: bold; color: #f00;\">Lorem</span> ipsum<span style=\"color: rgb(255, 0, 0); font-weight: bold;\">ore</span></p>", TestName = "{m}_Simple_2")]
	public async Task CopyPasteShouldMaintainRichText_Span(string inputHtml, int selectionStart, int selectionEnd, string expectedHtml)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox()
		{
			IsToolBarVisible = true
		});

		var editor = page.GetByRole(AriaRole.Textbox);
		await editor.EvaluateAsync("(elm, inputHtml) => elm.innerHTML = inputHtml", inputHtml);

		var client = await RichTextBoxClient.GetClientAsync();
		await client.EvaluateAsync($@"client => {{
			client.setSelection({{ start: {selectionStart}, end: {selectionEnd}}});
		}}");

		await page.AttachMockClipboardWrite();
		await editor.FakeClipboardCopy();

		await page.Keyboard.PressAsync("Control+End");

		await editor.FakeClipboardPaste("text/html");
		Assert.That(async () => await editor.InnerHTMLAsync(), Is.EqualTo(expectedHtml));
	}

	[Test, WithPlaywrightPage]
	public async Task CopyPasteShouldMoveScrollBar()
	{
		var pasteContent = "Test Content\r\nTest Content\r\nTest Content\r\nTest Content\r\nTest Content\r\nTest Content";
		await using var ctx = new InMemoryTestServerContext();
		await ctx.LoadControlOnFormAsync(() =>
			new RichTextBox()
			{
				Width = 500,
				Height = 30,
				Font = new Font("Tahoma", 20, GraphicsUnit.Pixel),
			}
		);
		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();

		await PastePlainTextToElement(editorBody, pasteContent);

		var text = await client.GetEditorTextAsync();
		var selection = await client.GetActiveSelectionAsync();
		Assert.That(text.Length, Is.EqualTo(selection.start));
		Assert.That(selection.start, Is.EqualTo(selection.end));

		await client.EvaluateAsync("client => client.getEditorBody().scrollTop = 0");

		await PastePlainTextToElement(editorBody, pasteContent);
		var getScrollTopAndScrollHeightScript = @"client => {
			return [client.getEditorBody().scrollTop, client.getEditorBody().scrollHeight, client.getEditorBody().clientHeight];
		}";
		int[]? result = null;
		Assert.That(async () => result = await client.EvaluateAsync<int[]>(getScrollTopAndScrollHeightScript), Is.Not.Null.And.Length.EqualTo(3).After(2000, 100));
		if (result == null)
		{
			throw new InvalidOperationException("Result can't be get.");
		}
		var scrollTop = result[0];
		var scrollHeight = result[1];
		var clientHeight = result[2];
		Assert.That(scrollTop + clientHeight, Is.InRange(scrollHeight - 1, scrollHeight + 1));
	}

	[Test, WithPlaywrightPage]
	public async Task CopyingStrayBulletsKeepsBullets([Values("ol", "ul")] string listType)
	{
		var paragraph = $"{WrapInP("Testing lists.")}<p><br></p>";
		var insertedHtml = $"{paragraph}<{listType}><li>{WrapInSpan("Lorem ipsum dolor sit amet")}</li><li>{WrapInSpan("consectetur adipiscing elit.")}</li><li>{WrapInSpan("Duis urna purus, semper a varius ut")}</li><li>{WrapInSpan("ullamcorper iaculis est.")}</li></{listType}>";
		var expectedHtml = $"{insertedHtml}{WrapInP("<br>")}<{listType}><li>{WrapInSpan("consectetur adipiscing elit.")}</li><li>{WrapInSpan("Duis urna purus, semper")}</li></{listType}>";

		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox()
		{
			Html = insertedHtml,
			IsToolBarVisible = true,
			Width = 300,
			Height = 300
		});
		var editorBody = page.Locator(".richtextbox__editoranchor");

		var client = await RichTextBoxClient.GetClientAsync();
		await client.EvaluateAsync(@"client => {
			client.setSelection({ start: 43, end: 95});
		}");

		await page.AttachMockClipboardWrite();
		await editorBody.FakeClipboardCopy();

		await page.Keyboard.PressAsync("Control+End");
		await page.Keyboard.PressAsync("Enter");
		await page.Keyboard.PressAsync("Enter");
		await page.Keyboard.PressAsync("Enter");

		await editorBody.FakeClipboardPaste("text/html");
		Assert.That(async () => await editorBody.InnerHTMLAsync(), Is.EqualTo(expectedHtml));
	}

	[Test, WithPlaywrightPage]
	public async Task CopyingStrayBulletsWhenCreatingList([Values("ol", "ul")] string listType)
	{
		var paragraph = $"{WrapInP("Testing lists.")}<p><br></p>";
		var loremLines = paragraph + WrapInP("Lorem ipsum dolor sit amet") + WrapInP("consectetur adipiscing elit.") + WrapInP("Duis urna purus, semper a varius ut") + WrapInP("ullamcorper iaculis est.");
		var listItems = $"<li>{WrapInSpan("Lorem ipsum dolor sit amet")}</li><li>{WrapInSpan("consectetur adipiscing elit.")}</li><li>{WrapInSpan("Duis urna purus, semper a varius ut")}</li><li>{WrapInSpan("ullamcorper iaculis est.")}</li>";
		var desiredHtml = $"{paragraph}<{listType}>{listItems}</{listType}>";
		var resultingHtml = $"{desiredHtml}{WrapInP("<br>")}<{listType}>{listItems}</{listType}>";

		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox()
		{
			Html = loremLines,
			IsToolBarVisible = true,
			Width = 300,
			Height = 300
		});
		var editorBody = page.Locator(".richtextbox__editoranchor");

		var client = await RichTextBoxClient.GetClientAsync();
		await client.EvaluateAsync(@"client => {
			client.setSelection({ start: 16, end: 500});
		}");

		var toolbarButton = page.Locator(".richtextbox__toolbar").GetByLabel(listType == "ul" ? "Bullet list" : "Numbered list");
		await toolbarButton.ClickAsync();

		await page.AttachMockClipboardWrite();
		await editorBody.FakeClipboardCopy();

		await page.Keyboard.PressAsync("Control+End");

		await page.Keyboard.PressAsync("Enter");
		await page.Keyboard.PressAsync("Enter");
		await page.Keyboard.PressAsync("Enter");

		await editorBody.FakeClipboardPaste("text/html");

		Assert.That(async () => await editorBody.InnerHTMLAsync(), Is.EqualTo(resultingHtml));
	}

	[Test, WithPlaywrightPage]
	public async Task CopyingFullListKeepsIntegrity([Values("ol", "ul")] string listType)
	{
		var paragraph = WrapInP("Testing lists.");
		var listItems = $"<li>{WrapInSpan("Lorem ipsum dolor sit amet")}</li><li>{WrapInSpan("consectetur adipiscing elit.")}</li><li>{WrapInSpan("Duis urna purus, semper a varius ut")}</li><li>{WrapInSpan("ullamcorper iaculis est.")}</li>";
		var insertedHtml = $"{paragraph}<{listType}>{listItems}</{listType}>";
		var expectedHtml = $"{insertedHtml}{WrapInP("<br>")}{paragraph}<{listType}>{listItems}</{listType}>";

		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox()
		{
			Html = insertedHtml,
			IsToolBarVisible = true,
			Width = 300,
			Height = 300
		});
		var editorBody = page.Locator(".richtextbox__editoranchor");

		var client = await RichTextBoxClient.GetClientAsync();
		await client.EvaluateAsync(@"client => {
			client.setSelection({ start: 0, end: 155});
		}");

		await page.AttachMockClipboardWrite();
		await editorBody.FakeClipboardCopy();

		await page.Keyboard.PressAsync("Control+End");
		await page.Keyboard.PressAsync("Enter");
		await page.Keyboard.PressAsync("Enter");
		await page.Keyboard.PressAsync("Enter");

		await editorBody.FakeClipboardPaste("text/html");
		Assert.That(async () => await editorBody.InnerHTMLAsync(), Is.EqualTo(expectedHtml));
	}

	[Test, WithPlaywrightPage]
	public async Task CopyingPartSentenceFromListItemIsBulletless([Values("ol", "ul")] string listType)
	{
		var listItems = $"<li>{WrapInSpan("Lorem ipsum dolor sit amet")}</li>";
		var insertedHtml = $"<{listType}>{listItems}</{listType}>";

		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox()
		{
			Html = insertedHtml,
			IsToolBarVisible = true,
			Width = 300,
			Height = 300
		});
		var editorBody = page.Locator(".richtextbox__editoranchor");

		var client = await RichTextBoxClient.GetClientAsync();
		await client.EvaluateAsync(@"client => {
			client.setSelection({ start: 6, end: 11});
		}");

		await page.AttachMockClipboardWrite();
		await editorBody.FakeClipboardCopy();

		await page.Keyboard.PressAsync("Control+End");
		await page.Keyboard.PressAsync("Enter");
		await page.Keyboard.PressAsync("Enter");
		await page.Keyboard.PressAsync("Enter");

		await editorBody.FakeClipboardPaste("text/html");
		Assert.That(async () => await editorBody.InnerHTMLAsync(), Is.EqualTo(insertedHtml + WrapInP("<br>") + WrapInP("ipsum")));
	}

	[TestCase("Bullet list", "ul", TestName = "{m}_Unordered")]
	[TestCase("Numbered list", "ol", TestName = "{m}_Ordered")]
	[WithPlaywrightPage]
	public async Task RichTextBoxListButtonsDontGenerateSurplusBlankLines(string listName, string listElement)
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		var page = await ctx.LoadControlOnFormAsync(() => richTextBox = new RichTextBox()
		{
			IsToolBarVisible = true,
			Width = 800,
			Height = 500
		});
		var editorBody = page.Locator(".richtextbox__editoranchor");
		var toolbarButton = page.Locator(".richtextbox__toolbar").GetByLabel(listName);
		var client = await RichTextBoxClient.GetClientAsync();

		await client.EvaluateAsync("async client => await client.readyEditor()");
		await client.FocusEditorAsync();

		var paragraphMarkup = "<p>Line 1</p><p>Line 2</p><p>Line 3</p>";
		var listMarkup = $"<{listElement}><li>Line 1</li><li>Line 2</li><li>Line 3</li></{listElement}>";

		await page.Keyboard.TypeAsync("Line 1");
		await page.Keyboard.PressAsync("Enter");
		await page.Keyboard.TypeAsync("Line 2");
		await page.Keyboard.PressAsync("Enter");
		await page.Keyboard.TypeAsync("Line 3");
		await page.Keyboard.PressAsync("Control+A");

		Assert.That(async () => await editorBody.InnerHTMLAsync(), Is.EqualTo(paragraphMarkup), "Paragraphs");

		await toolbarButton.ClickAsync();
		Assert.That(async () => await editorBody.InnerHTMLAsync(), Is.EqualTo(listMarkup).After(3000, 100), "List items");

		await toolbarButton.ClickAsync();
		Assert.That(async () => await editorBody.InnerHTMLAsync(), Is.EqualTo(paragraphMarkup).After(3000, 100), "Returned to paragraphs");

		await toolbarButton.ClickAsync();
		Assert.That(async () => await editorBody.InnerHTMLAsync(), Is.EqualTo(listMarkup).After(3000, 100), "List items again");
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxClicksOnEndOfListSelectsCorrectLocation()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox? richTextBox = null!;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Width = 800, Height = 500 };
			richTextBox = new RichTextBox()
			{
				Width = 800,
				Height = 500,
				IsToolBarVisible = true,
				Font = new Font("Tahoma", 20, GraphicsUnit.Pixel),
				Html = $"{WrapInP("Expected behavior/appearance. CW1 version:")}{WrapInP("the dialog will adjust size correctly")}<p><br></p><p><br></p><ul><li>IU</li><li>{WrapInSpan("Rain")}</li><li>{WrapInSpan("Super Junior")}</li><li>{WrapInSpan("Girl's Generation")}</li></ul>"
			};
			form.Controls.Add(richTextBox);
			return form;
		});

		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		var editorAnchor = page.Locator(".richtextbox__editoranchor");
		await client.FocusEditorAsync();

		await editorAnchor.AttachClipboardPaste();

		await page.MockClipboardRead(new[] {
			new JSClipboardData("text/plain", "One OK Rock")
		});
		await editorBody.PressAsync("Control+End");
		await editorBody.PressAsync("Control+KeyV");

		var expectedMarkup = $"{WrapInP("Expected behavior/appearance. CW1 version:")}{WrapInP("the dialog will adjust size correctly")}<p><br></p><p><br></p><ul><li>{WrapInSpan("IU")}</li><li>{WrapInSpan("Rain")}</li><li>{WrapInSpan("Super Junior")}</li><li>{WrapInSpan("Girl's Generation")}One OK Rock</li></ul>";
		Assert.That(async () => await client.GetEditorHtmlAsync(), Is.EqualTo(expectedMarkup).After(3000, 100));

		var lastListItem = await editorBody.QuerySelectorAsync("ul li:last-of-type");
		var expectedSelectionPosition = 132;

		await editorBody.PressAsync("Control+End");
		Assert.That(async () => (await client.GetActiveSelectionAsync()).start, Is.EqualTo(expectedSelectionPosition).After(3000, 100), "Selection start");

		var i = 0;
		while (++i < 12)
		{
			await lastListItem!.ClickAsync(options: new ElementHandleClickOptions() { Position = new Position { X = 500 + i * 4, Y = i } });
			Assert.That(async () => (await client.GetActiveSelectionAsync()).start, Is.EqualTo(expectedSelectionPosition).After(3000, 100), $"Selection start - {i}");
			Assert.That(async () => (await client.GetActiveSelectionAsync()).end, Is.EqualTo(expectedSelectionPosition), $"Selection length - {i}");
		}
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxToolbarButtonEventsRegisterOnlyOnce()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		var page = await ctx.LoadControlOnFormAsync(() => richTextBox = new RichTextBox()
		{
			IsToolBarVisible = true,
			Width = 800,
			Height = 500
		});
		var editorBody = page.Locator(".richtextbox__editoranchor");
		var toolbarButton = page.Locator(".richtextbox__toolbar").GetByLabel("Increase Indent");
		var client = await RichTextBoxClient.GetClientAsync();

		await client.EvaluateAsync("async client => await client.readyEditor()");
		await toolbarButton.ClickAsync();

		var blockquoteCount = (await editorBody.InnerHTMLAsync()).Split("</p>").Length - 1;
		Assert.That(blockquoteCount, Is.EqualTo(1));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxShouldNotStealFocusWhenInitializing()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null!;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var control = new TextBox();
			return form;
		}); // Dummy control which should be focused

		await page.EvaluateAsync("document.addEventListener('focusin', e => e.target.dataset['focusCount'] = (parseInt(e.target.dataset['focusCount'])+1) || 1);");

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			form.Controls.Add(new RichTextBox());
		});

		var editor = page.Locator(".richtextbox__editoranchor");
		await editor.WaitForAsync();
		Assert.That(async () => await editor.GetAttributeAsync("data-focus-count"), Is.Null.After(3000, 100));

		await editor.FocusAsync();
		Assert.That(async () => await editor.GetAttributeAsync("data-focus-count"), Is.EqualTo("1").After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxShouldBeFocusedOnClientWhenInitializingIfFocusedOnServer()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		await ctx.LoadControlOnFormAsync(() => richTextBox = new RichTextBox());
		Assert.That(richTextBox.Focused, Is.True);

		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();

		Assert.That(async () => await editorBody.EvaluateAsync<bool>("element => document.activeElement === element"), Is.True.After(3000, 100));
	}

	[Test]
	public async Task RichTextBoxIsInitializedWithCorrectParameters([Values] bool isToolBarVisible)
	{
		using var ctx = new WinzorTestContext();
		using var rendered = await ctx.RenderControlOnFormAsync(() => new RichTextBox { IsToolBarVisible = isToolBarVisible });

		var invocation = ctx.JSInterop.Invocations.Single(i => i.Identifier == "initialize");
		var initializeParameters = invocation.Arguments[3] as InitializeParameters;
		Assert.That(initializeParameters!.EnableToolBar, Is.EqualTo(isToolBarVisible));
	}

	[Test]
	public async Task RichTextBoxInitializeParametersHaveCorrectDefaultValues()
	{
		using var ctx = new WinzorTestContext();
		using var rendered = await ctx.RenderControlOnFormAsync(() => new RichTextBox());

		var invocation = ctx.JSInterop.Invocations.Single(i => i.Identifier == "initialize");
		var initializeParameters = invocation.Arguments[3] as InitializeParameters;
		Assert.That(initializeParameters!.EnableToolBar, Is.False);
	}

	[Test]
	public async Task RichTextBoxActionsShouldBeInvokedAfterEditorInitialized()
	{
		using var ctx = new WinzorTestContext();
		using var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var richTextBox = new RichTextBox();
			richTextBox.ClearUndoManager(); // Do an action on the RTB that will be wrapped with InvokeRenderDispatcher
			return richTextBox;
		});

		var jsInvocations = ctx.JSInterop.Invocations.Select(i => i.Identifier).ToList();
		Assert.That(jsInvocations.IndexOf("clearUndoManager"), Is.GreaterThan(jsInvocations.IndexOf("initialize")));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxFocusedOnClientWhenCallingFocus()
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(new TextBox()); // Control to steal focus on initial render
			richTextBox = new RichTextBox();
			form.Controls.Add(richTextBox);
			return form;
		});

		Assert.That(richTextBox.Focused, Is.False);
		var editor = page.Locator(".richtextbox__editoranchor");
		await editor.WaitForAsync();
		Assert.That(async () => await editor.EvaluateAsync<bool>("element => document.activeElement !== element"), Is.True.After(3000, 100));

		await richTextBox.InvokeWinzorDispatcherAsync(() => richTextBox.Focus());

		Assert.That(richTextBox.Focused, Is.True);
		Assert.That(async () => await editor.EvaluateAsync<bool>("element => document.activeElement === element"), Is.True.After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task SegoeUiSymbolShouldBeConvertedToSegoeUiEmoji([Values] bool readOnly)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox()
		{
			Html = "<p><span style=\"font-family: Segoe UI Symbol, sans-serif; font-size: 10px;\">Don't change me to Segoe UI Emoji, duhh</span><span style=\"font-family: Segoe UI Symbol, sans-serif; font-size: 10px;\">🖤🥹✌✰</span></p>",
			ReadOnly = readOnly
		});
		var client = await RichTextBoxClient.GetClientAsync();
		var editor = readOnly ? page.Locator(".richtextbox__data") : page.Locator(".richtextbox__editoranchor");

		Assert.That(async () => await editor.InnerHTMLAsync(), Is.EqualTo("<p><span style=\"font-family: &quot;Segoe UI Emoji&quot;, &quot;Segoe UI Symbol&quot;, &quot;Segoe UI&quot;, sans-serif; font-size: 10px;\">Don't change me to Segoe UI Emoji, duhh\U0001f5a4\U0001f979✌✰</span></p>"));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxContentPreviewVisibleAfterFirstRender([Values] bool readOnly)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var richTextBox = new Mock<RichTextBox> { CallBase = true };
			richTextBox.Object.Text = "This is some RichTextBox content";
			richTextBox.Object.ReadOnly = readOnly;
			// Mock the RichTextBox OnAfterRenderAsync so that it will never initialize the editor
			richTextBox.Setup(i => i.OnAfterRenderAsync(It.IsAny<bool>())).Returns(Task.CompletedTask);
			return richTextBox.Object;
		});

		Assert.That(async () => await page.Locator(".richtextbox").InnerTextAsync(), Is.EqualTo("This is some RichTextBox content").After(1000, 100));
		Assert.That(async () => await page.Locator(".richtextbox__data").InnerTextAsync(), Is.Empty);
		Assert.That(async () => await page.Locator(".richtextbox__editoranchor").IsVisibleAsync(), Is.False);
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxContentPreviewHiddenWhenEditorInitialized([Values] bool readOnly)
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var richTextBoxMock = new Mock<RichTextBox> { CallBase = true };
			richTextBoxMock.Object.Text = "This is some RichTextBox content";
			richTextBoxMock.Object.ReadOnly = readOnly;
			// Mock the RichTextBox OnAfterRenderAsync so that it will never initialize the editor
			richTextBoxMock.Setup(i => i.OnAfterRenderAsync(It.IsAny<bool>())).Returns(Task.CompletedTask);
			richTextBox = richTextBoxMock.Object;
			return richTextBox;
		});
		var editor = page.Locator(readOnly ? ".richtextbox__data" : ".richtextbox__editoranchor");

		Assert.That(async () => await page.Locator(".richtextbox").InnerTextAsync(), Is.EqualTo("This is some RichTextBox content").After(1000, 100));

		if (readOnly)
		{
			Assert.That(async () => await editor.InnerTextAsync(), Is.Empty);
		}
		else
		{
			Assert.That(async () => await editor.IsVisibleAsync(), Is.False);
		}

		await richTextBox.InvokeRenderDispatcherAsync(async () =>
		{
			var initializeParameters = new InitializeParameters
			{
				WinzorControlId = richTextBox.WinzorControlId,
				Font = richTextBox.Font,
				EnableToolBar = richTextBox.IsToolBarVisible
			};
			await richTextBox.Interop!.LoadEditorAsync(richTextBox.ElementReference.ElementReferenceOrNull(), richTextBox.dotNetObjectReference, richTextBox.Html, initializeParameters);
		});

		Assert.That(async () => await editor.InnerTextAsync(), Is.EqualTo("This is some RichTextBox content").After(3000, 100));
		Assert.That(async () => await page.Locator(".richtextbox").InnerTextAsync(), Is.EqualTo("This is some RichTextBox content").After(1000, 100));
		Assert.That(async () => await page.Locator(".richtextbox__placeholder").IsVisibleAsync(), Is.False);
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxContentPreviewRemovedFromMarkupAfterInitialized([Values] bool readOnly)
	{
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var richTextBoxMock = new Mock<RichTextBox> { CallBase = true };
			richTextBoxMock.Object.Text = "This is some RichTextBox content";
			richTextBoxMock.Object.ReadOnly = readOnly;
			// Mock the RichTextBox OnAfterRenderAsync so that it will never initialize the editor
			richTextBoxMock.Setup(i => i.OnAfterRenderAsync(It.IsAny<bool>())).Returns(Task.CompletedTask);
			richTextBox = richTextBoxMock.Object;
			return richTextBox;
		});

		Assert.That(async () => await page.Locator(".richtextbox__placeholder").CountAsync(), Is.EqualTo(1).After(3000, 100));

		Mock.Get(richTextBox).Setup(i => i.OnAfterRenderAsync(It.IsAny<bool>())).CallBase();
		await richTextBox.InvokeRenderDispatcherAsync(async () => await richTextBox.OnAfterRenderAsync(true));

		Assert.That(async () => await page.Locator(".richtextbox__placeholder").CountAsync(), Is.EqualTo(0).After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task ContentIsSyncedBeforeMouseDownOnAnotherElement()
	{
		var contentIsCorrectTask = new TaskCompletionSource<string>();
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var richTextBox = new RichTextBox();
			form.Controls.Add(richTextBox);

			var button = new Button();
			form.Controls.Add(button);

			button.MouseDown += (_, _) =>
			{
				contentIsCorrectTask.SetResult(richTextBox.Text);
			};

			return form;
		});

		var inputText = "Test input";
		var editorBody = page.Locator(".richtextbox__editoranchor");
		await editorBody.WaitForAsync();
		await editorBody.FocusAsync();
		await editorBody.PressSequentiallyAsync(inputText);
		await page.ClickAsync(".button");
		Assert.That(await contentIsCorrectTask.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		var content = await contentIsCorrectTask.Task;
		Assert.That(content, Is.EqualTo(inputText));
	}

	static IEnumerable<TestCaseData> ContentChangeTestCases
	{
		get
		{
			yield return new(
				$"<p><a href=\"https://wisetechglobal.com\" rel=\"noopener\" {standardStyle}>https://wisetechglobal.com</a></p>",
				$"<p><a href=\"https://wisetechglobal.com\" target=\"_blank\" {standardStyle}>https://wisetechglobal.com</a></p>",
				$"<p><a href=\"https://wisetechglobal.com\" target=\"_blank\" {standardStyle}>https://wisetechglobal.com</a></p>"
			)
			{ TestName = "{m}_LinkTargetBlankChange_Only_IsIgnored" };

			yield return new(
				$"<p><a href=\"https://wisetechglobal.com\" rel=\"noopener\" {standardStyle}>https://wisetechglobal.com</a></p>",
				$"<p><a href=\"https://wisetechglobal.com\" target=\"_blank\" {standardStyle}>https://wisetechglobal.com</a>{WrapInSpan("changed")}</p>",
				$"<p><a href=\"https://wisetechglobal.com\" target=\"_blank\" {standardStyle}>https://wisetechglobal.com</a>{WrapInSpan("changed")}</p>"
			)
			{ TestName = "{m}_LinkTargetBlankChange_WithOtherChanges_IsApplied" };

			yield return new(
				$"<ol><li>{WrapInSpan(" item ")}</li></ol>",
				$"<ol><li>{WrapInSpan(" item ")}</li></ol>",
				$"<ol><li>{WrapInSpan(" item ")}</li></ol>"
			)
			{ TestName = "{m}_SpaceChangeBetweenUnpairedTags_Only_IsIgnored" };

			yield return new(
				$"<ol><li>{WrapInSpan(" item ")}</li></ol>",
				$"<ol><li>{WrapInSpan(" change ")}</li></ol>",
				$"<ol><li>{WrapInSpan(" change ")}</li></ol>"
			)
			{ TestName = "{m}_SpaceChangeBetweenUnpairedTags_WithOtherChanges_IsApplied" };
		}
	}

	[TestCaseSource(nameof(ContentChangeTestCases))]
	public async Task ContentChange(string html, string newHtml, string expectedHtml)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new RichTextBox { Html = html });
		var richTextBox = rendered.GetControl<RichTextBox>();
		await ctx.WinzorDispatcher.InvokeAsync(() => richTextBox.Html = newHtml);
		Assert.That(richTextBox.Html, Is.EqualTo(expectedHtml));
	}

	[Test, WithPlaywrightPage]
	public async Task ScrollToCaret([Values] bool readOnly)
	{
		var textOnEachLine = "123456789";
		var numberOfLines = 20;
		await using var ctx = new InMemoryTestServerContext();
		RichTextBox richTextBox = null!;
		var page = await ctx.LoadControlOnFormAsync(() => richTextBox = new RichTextBox()
		{
			Width = 100,
			Height = 50,
			ReadOnly = readOnly,
			Text = string.Join('\n', Enumerable.Repeat(textOnEachLine, numberOfLines)),
		});
		var div = page.Locator(readOnly ? ".richtextbox__data" : ".richtextbox__editoranchor");
		Assert.That(await div.EvaluateAsync<int>("e => e.scrollTop"), Is.EqualTo(0));

		await richTextBox.InvokeWinzorDispatcherAsync(() =>
		{
			richTextBox.SelectionStart = richTextBox.Text.Length;
			richTextBox.ScrollToCaret();
		});
		Assert.That(() => div.EvaluateAsync<int>("e => e.scrollTop"), Is.EqualTo(readOnly ? 212 : 261).Within(10).After(1000, 100));

		await richTextBox.InvokeWinzorDispatcherAsync(() =>
		{
			richTextBox.SelectionStart = string.Join('\n', Enumerable.Repeat(textOnEachLine, numberOfLines / 2)).Length + 1;
			richTextBox.ScrollToCaret();
		});
		Assert.That(() => div.EvaluateAsync<int>("e => e.scrollTop"), Is.EqualTo(readOnly ? 130 : 155).Within(10).After(1000, 100));

		await richTextBox.InvokeWinzorDispatcherAsync(() =>
		{
			richTextBox.SelectionStart = 0;
			richTextBox.ScrollToCaret();
		});
		Assert.That(() => div.EvaluateAsync<int>("e => e.scrollTop"), Is.EqualTo(0).After(1000, 100));
	}

	[Test]
	public async Task RichTextBoxHtmlIsSanitizedOnSet()
	{
		using var ctx = new WinzorTestContext();
		var mockedInterop = new Mock<IRichTextBoxJSInterop>();
		ctx.Services.AddSingleton(mockedInterop.Object);
		var rendered = await ctx.RenderControlOnFormAsync(() => new RichTextBox
		{
			Html = $"{WrapInP("Hello")}<img src=\"XSS\" onerror=\"console.log('this is an XSS risk')\"></img><script>console.log('this is an XSS risk')</script>{WrapInP("Goodbye")}",
		});
		var richTextBox = rendered.GetControl<RichTextBox>();

		Assert.That(richTextBox.Html, Is.EqualTo(WrapInP("Hello") + WrapInP("Goodbye")));
		mockedInterop.Verify(i => i.LoadEditorAsync(It.IsAny<ElementReference>(), It.IsAny<DotNetObjectReference<RichTextBox>>(), WrapInP("Hello") + WrapInP("Goodbye"), It.IsAny<InitializeParameters>()), Times.Once);
	}

	[Test]
	public async Task RichTextBoxTextIsSanitizedOnSet()
	{
		using var ctx = new WinzorTestContext();
		var mockedInterop = new Mock<IRichTextBoxJSInterop>();
		ctx.Services.AddSingleton(mockedInterop.Object);
		var rendered = await ctx.RenderControlOnFormAsync(() => new RichTextBox
		{
			Text = "Hello\r\nWorld",
		});
		var richTextBox = rendered.GetControl<RichTextBox>()!;

		mockedInterop.Verify(i => i.LoadEditorAsync(It.IsAny<ElementReference>(), It.IsAny<DotNetObjectReference<RichTextBox>>(), "<p>Hello</p><p>World</p>", It.IsAny<InitializeParameters>()), Times.Once);
	}

	[Test]
	public async Task RichTextBoxSelectedHtmlIsSanitizedOnSet()
	{
		using var ctx = new WinzorTestContext();
		var mockedInterop = new Mock<IRichTextBoxJSInterop>();
		ctx.Services.AddSingleton(mockedInterop.Object);
		var rendered = await ctx.RenderControlOnFormAsync(() => new RichTextBox
		{
			SelectedHtml = $"{WrapInP("Hello")}<img src=\"XSS\" onerror=\"console.log('this is an XSS risk')\"></img><script>console.log('this is an XSS risk')</script>{WrapInP("Goodbye")}",
		});
		var richTextBox = rendered.GetControl<RichTextBox>();

		mockedInterop.Verify(i => i.SetSelectionContentAsync(richTextBox.WinzorControlId, 0, 0, WrapInP("Hello") + WrapInP("Goodbye")), Times.Once);
	}

	[Test]
	public async Task RichTextBoxSelectedTextIsSanitizedOnSet()
	{
		using var ctx = new WinzorTestContext();
		var mockedInterop = new Mock<IRichTextBoxJSInterop>();
		ctx.Services.AddSingleton(mockedInterop.Object);
		var rendered = await ctx.RenderControlOnFormAsync(() => new RichTextBox
		{
			SelectedText = "Hello\r\nWorld",
		});
		var richTextBox = rendered.GetControl<RichTextBox>()!;

		mockedInterop.Verify(i => i.SetSelectionContentAsync(richTextBox.WinzorControlId, 0, 0, "<p>Hello</p><p>World</p>"), Times.Once);
	}

	[Test]
	public async Task RichTextBoxContentIsSanitizedWhenRecievedFromClient()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new RichTextBox());
		var richTextBox = rendered.GetControl<RichTextBox>();

		await richTextBox.OnModifiedAsync();
		await richTextBox.OnContentChangedAsync(new ContentChangedEventArgs
		{
			Content = new EditorContent($"{WrapInP("Hello")}<img src=\"XSS\" onerror=\"console.log('this is an XSS risk')\"></img><script>console.log('this is an XSS risk')</script>{WrapInP("Goodbye")}", null, -1),
		});

		Assert.That(richTextBox.Html, Is.EqualTo(WrapInP("Hello") + WrapInP("Goodbye")));
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxSetsDefaultFontSize()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox()
		{
			Width = 300,
			Height = 300,
			IsToolBarVisible = true,
			Html = "<p style=\"font-family: arial\">RichTextBoxSetsDefaultFontSize</p>"
		});

		var sourceLocator = page.Locator(SpanSelectorByTextAndVisible("RichTextBoxSetsDefaultFontSize"));
		await Assertions.Expect(sourceLocator).ToHaveCSSAsync("font-size", "16px");
	}

	[Test, WithPlaywrightPage]
	public async Task RichTextBoxSetsDefaultFont()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox()
		{
			Width = 300,
			Height = 300,
			IsToolBarVisible = true,
			Font = new Font("Arial", 1, GraphicsUnit.Pixel),
			Html = "<p>RichTextBoxSetsDefaultFont</p>",
		});

		var sourceLocator = page.Locator(SpanSelectorByTextAndVisible("RichTextBoxSetsDefaultFont"));
		await Assertions.Expect(sourceLocator).ToHaveCSSAsync("font-family", new Regex("^Arial*"));
		await Assertions.Expect(sourceLocator).ToHaveCSSAsync("font-size", "1px");
	}

	[Test, WithPlaywrightPage]
	public async Task ZoomContentOnCtrlPlusScroll()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new RichTextBox() { Width = 500, Height = 500, Text = "Text for Testing." });

		var client = await RichTextBoxClient.GetClientAsync();
		var editorBody = await client.GetEditorBodyAsync();
		await editorBody.ClickAsync();

		// Simulate Control+ScrollUp
		await page.Mouse.MoveAsync(200, 200);
		await SimulateScrollUp(page);
		Assert.That(async () => await editorBody.GetComputedStyleAsync("zoom"), Is.EqualTo("1.1").After(2000, 100));
		await SimulateScrollUp(page);
		Assert.That(async () => await editorBody.GetComputedStyleAsync("zoom"), Is.EqualTo("1.2").After(2000, 100));

		// Simulate Control+ScrollDown
		await SimulateScrollDown(page);
		Assert.That(async () => await editorBody.GetComputedStyleAsync("zoom"), Is.EqualTo("1.1").After(2000, 100));
		await SimulateScrollDown(page);
		Assert.That(async () => await editorBody.GetComputedStyleAsync("zoom"), Is.EqualTo("1").After(2000, 100));

		async Task SimulateScrollUp(IPage page)
		{
			await page.Keyboard.DownAsync("Control");
			await page.Mouse.WheelAsync(0, -1);
			await page.Keyboard.UpAsync("Control");
		}

		async Task SimulateScrollDown(IPage page)
		{
			await page.Keyboard.DownAsync("Control");
			await page.Mouse.WheelAsync(0, 1);
			await page.Keyboard.UpAsync("Control");
		}
	}

	const string standardFontStyle = "font-family: Tahoma, sans-serif; font-size: 20px;";
	const string standardStyle = $"style=\"{standardFontStyle}\"";
	const string defaultEditorFontStyle = "font-family: &quot;Microsoft Sans Serif&quot;, sans-serif; font-size: 10pt;";
	const string defaultEditorStyle = $"style=\"{defaultEditorFontStyle}\"";
	static string WrapInSpan(string s) => $"<span {standardStyle}>{s}</span>";
	static string WrapInP(string s) => $"<p><span {standardStyle}>{s}</span></p>";
	static string WrapInPWithPadding(string s, int padding) => $"<p style=\"padding-left: {padding}px;\"><span {standardStyle}>{s}</span></p>";
	static string SpanSelectorByTextAndVisible(string str) => $"span:text(\"{str}\"):visible";

	class RichTextBoxForTest : RichTextBox
	{
		internal int GetFontsCount { get; private set; }

		readonly Dictionary<string, Action> Callbacks = new Dictionary<string, Action>();

		internal new Task OnTextBoxSelectionChangedAsync(TextboxSelectionChangeEventArgs args) => base.OnTextBoxSelectionChangedAsync(args);

		internal void SetCallback(string methodName, Action action)
		{
			Callbacks[methodName] = action;
		}

		protected internal override void OnTextBoxSelectionChanged(int newStart, int newLength)
		{
			TryInvokeAction(nameof(OnTextBoxSelectionChanged));
			base.OnTextBoxSelectionChanged(newStart, newLength);
		}

		void TryInvokeAction(string methodName)
		{
			Callbacks.TryGetValue(methodName, out var action);
			action?.Invoke();
		}

		protected override List<string> GetServerInstalledFonts()
		{
			GetFontsCount++;
			return base.GetServerInstalledFonts();
		}

		public int OnModifiedCount { get; private set; }

		internal override async Task OnModifiedAsync()
		{
			OnModifiedCount++;
			await base.OnModifiedAsync();
		}

		public TaskCompletionSource? OnContentChangedTcs { get; set; }

		internal override async Task OnContentChangedAsync(ContentChangedEventArgs args)
		{
			await (OnContentChangedTcs?.Task ?? Task.CompletedTask);
			await base.OnContentChangedAsync(args);
		}

		public override IRichTextBoxJSInterop? Interop => InteropForTest ?? GetJSInterop<IRichTextBoxJSInterop>();

		public IRichTextBoxJSInterop? InteropForTest;
	}
}
