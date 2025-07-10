using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Tools;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

class SpellCheckerTest
{
	[Test, WithPlaywrightPage, WithSnapshotProtection]
	[TestCase(",", ",", TestName = "{m}_InvalidEnglishCharacter")]
	[TestCase(" ", " ", TestName = "{m}_Space")]
	[TestCase("Enter", "</p><p><br>", TestName = "{m}_Enter")]
	[TestCase("Tab", "\t", TestName = "{m}_Tab")]
	public async Task SpellCheckWorksForZRichTextBox(string key, string extraHtml)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(CreateZFormWithTextBoxForTest<ZRichTextBox>);

		var editor = page.Locator(".richtextbox__editoranchor");
		await page.WaitForFunctionAsync("editor => editor.spellCheckEnabled", await editor.ElementHandleAsync(), new() { PollingInterval = 100, Timeout = 2000 });
		await editor.PressSequentiallyAsync("say hella");
		await editor.PressAsync(key);

		var expectedHtml = $"<p>say <span data-squiggle=\"true\">hella</span>{extraHtml}</p>";
		Assert.That(async () => await editor.InnerHTMLAsync(), Is.EqualTo(expectedHtml).After(1000, 100));
	}

	[Test, WithPlaywrightPage, WithSnapshotProtection]
	[TestCase(",", ",", TestName = "{m}_InvalidEnglishCharacter")]
	[TestCase(" ", " ", TestName = "{m}_Space")]
	[TestCase("Enter", "</p><p><br>", TestName = "{m}_Enter")]
	[TestCase("Tab", "\t", TestName = "{m}_Tab")]
	public async Task SpellCheckWorksForKRichTextBox(string key, string extraHtml)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(CreateZFormWithTextBoxForTest<KRichTextBox>);

		var editor = page.Locator(".richtextbox__editoranchor");
		await page.WaitForFunctionAsync("editor => editor.spellCheckEnabled", await editor.ElementHandleAsync(), new() { PollingInterval = 100, Timeout = 2000 });
		await editor.PressSequentiallyAsync("say hella");
		await editor.PressAsync(key);

		var expectedHtml = $"<p>say <span data-squiggle=\"true\">hella</span>{extraHtml}</p>";
		Assert.That(async () => await editor.InnerHTMLAsync(), Is.EqualTo(expectedHtml).After(1000, 100));
	}

	[Test, WithPlaywrightPage, WithSnapshotProtection]
	public async Task CheckSpellingAfterPaste()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(CreateZFormWithTextBoxForTest<ZRichTextBox>);

		var editor = page.Locator(".richtextbox__editoranchor");
		await page.WaitForFunctionAsync("editor => editor.spellCheckEnabled", await editor.ElementHandleAsync(), new() { PollingInterval = 100, Timeout = 2000 });
		await editor.PressSequentiallyAsync("hello ");
		await editor.AttachClipboardPaste();

		await page.MockClipboardRead([new JSClipboardData("text/plain", "hella!")]);
		await editor.PressAsync("Control+KeyV");

		var expectedHtml = "<p>hello <span data-squiggle=\"true\">hella</span>!</p>";
		Assert.That(async () => await editor.InnerHTMLAsync(), Is.EqualTo(expectedHtml).After(1000, 100));
	}

	[Test, WithPlaywrightPage, WithSnapshotProtection]
	public async Task RemoveCurrentSquiggleAfterDeleteInvalidEnglishChars()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(CreateZFormWithTextBoxForTest<ZRichTextBox>);

		var editor = page.Locator(".richtextbox__editoranchor");
		await page.WaitForFunctionAsync("editor => editor.spellCheckEnabled", await editor.ElementHandleAsync(), new() { PollingInterval = 100, Timeout = 2000 });
		await editor.PressSequentiallyAsync("hella!% ");

		var squiggleFormat = "<p><span data-squiggle=\"true\">hella</span>{0}</p>";
		Assert.That(async () => await editor.InnerHTMLAsync(), Is.EqualTo(string.Format(squiggleFormat, "!% ")).After(1000, 100));

		await editor.PressAsync("Backspace");
		Assert.That(async () => await editor.InnerHTMLAsync(), Is.EqualTo(string.Format(squiggleFormat, "!%")).After(1000, 100));

		await editor.PressAsync("Backspace");
		Assert.That(async () => await editor.InnerHTMLAsync(), Is.EqualTo(string.Format(squiggleFormat, "!")).After(1000, 100));

		await editor.PressAsync("Backspace");
		Assert.That(async () => await editor.InnerHTMLAsync(), Is.EqualTo("<p>hella</p>").After(1000, 100));
	}

	[Test, WithPlaywrightPage, WithSnapshotProtection]
	public async Task EnableAndDisableSpellCheckWorksForZRichTextBox()
	{
		var form = default(ZForm);
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() => form = CreateZFormWithTextBoxForTest<ZRichTextBox>());

		var editor = page.Locator(".richtextbox__editoranchor");
		await page.WaitForFunctionAsync("editor => editor.spellCheckEnabled", await editor.ElementHandleAsync(), new() { PollingInterval = 100, Timeout = 2000 });
		await editor.PressSequentiallyAsync("say hella!");

		var expectedHtml = "<p>say <span data-squiggle=\"true\">hella</span>!</p>";
		Assert.That(async () => await editor.InnerHTMLAsync(), Is.EqualTo(expectedHtml).After(1000, 100));

		// Disable SpellChecker
		await form.InvokeWinzorDispatcherAsync(() => SpellCheckerStatusSingleton.Instance.SyncroniseSpellCheckerStatus("SpellCheckerKey|ZRichTextBoxForTest", false));
		await page.WaitForFunctionAsync("editor => !editor.spellCheckEnabled", await editor.ElementHandleAsync(), new() { PollingInterval = 100, Timeout = 2000 });
		await editor.ClearAsync();
		await editor.PressSequentiallyAsync("say hella!");

		expectedHtml = "<p>say hella!</p>";
		Assert.That(async () => await editor.InnerHTMLAsync(), Is.EqualTo(expectedHtml).After(1000, 100));
	}

	[Test, WithPlaywrightPage, WithSnapshotProtection]
	public async Task EnableAndDisableSpellCheckWorksForZTextBox()
	{
		var form = default(ZForm);
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() => form = CreateZFormWithTextBoxForTest<ZTextBox>());

		var editor = page.Locator(".textbox");
		Assert.That(async () => await editor.GetAttributeAsync("spellcheck"), Is.EqualTo("true").After(1000, 100));

		// Disable SpellChecker
		await form.InvokeWinzorDispatcherAsync(() => SpellCheckerStatusSingleton.Instance.SyncroniseSpellCheckerStatus("SpellCheckerKey|ZTextBoxForTest", false));
		Assert.That(async () => await editor.GetAttributeAsync("spellcheck"), Is.EqualTo("false").After(1000, 100));

		// Enable SpellChecker again
		await form.InvokeWinzorDispatcherAsync(() => SpellCheckerStatusSingleton.Instance.SyncroniseSpellCheckerStatus("SpellCheckerKey|ZTextBoxForTest", true));
		Assert.That(async () => await editor.GetAttributeAsync("spellcheck"), Is.EqualTo("true").After(1000, 100));
	}

	[Test, WithPlaywrightPage, WithSnapshotProtection]
	public async Task TypePartialWordAndStopDoesNotCauseTextCursorToJump()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(CreateZFormWithTextBoxForTest<ZRichTextBox>);
		var editor = page.Locator(".richtextbox__editoranchor");
		var editorHandle = await editor.ElementHandleAsync();

		await editor.PressSequentiallyAsync("Typing fast is fine, .");
		await editor.PressAsync("ArrowLeft");
		await editor.PressSequentiallyAsync("Typ");
		Assert.That(await editor.EvaluateAsync<int>("editor => editor.getSelection().start", editorHandle), Is.EqualTo("Typing fast is fine, Typ".Length));

		// it is necessary here due to a 500ms delay before text is sent to server for spellcheck
		await Task.Delay(1000);

		Assert.That(await editor.EvaluateAsync<int>("editor => editor.getSelection().start", editorHandle), Is.EqualTo("Typing fast is fine, Typ".Length));
	}

	[Test, WithPlaywrightPage, WithSnapshotProtection]
	public async Task SquigglesNotDisappearAfterSwitchTabOnKRichTextBox()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var rtb = new KRichTextBox();
			var disposableAction = Db.DisposableActionForDbConnection();
			SpellChecker.InitialiseSpellcheck(rtb, rtb.Name);
			var tabControl = new ZTabControl();
			var tabPage1 = new ZTabPage { Text = "Tab one" };
			var tabPage2 = new ZTabPage { Text = "Tab two" };
			form.Disposed += (s, e) => disposableAction.Dispose();
			tabPage1.Controls.Add(rtb);
			tabControl.TabPages.Add(tabPage1);
			tabControl.TabPages.Add(tabPage2);
			form.Controls.Add(tabControl);
			return form;
		});

		var editor = page.Locator(".richtextbox__editoranchor");
		await page.WaitForFunctionAsync("editor => editor.spellCheckEnabled", await editor.ElementHandleAsync(), new() { PollingInterval = 100, Timeout = 2000 });
		await editor.PressSequentiallyAsync("The catt");
		await editor.PressAsync("Enter");

		var expectedHtml = $"The <span data-squiggle=\"true\">catt</span>";
		Assert.That(async () => await editor.InnerHTMLAsync(), Does.Contain(expectedHtml).After(1000, 100));

		await page.GetByRole(AriaRole.Button, new() { Name = "Tab two" }).ClickAsync();
		//this delay necessary to mock tab switch
		await Task.Delay(100);
		await page.GetByRole(AriaRole.Button, new() { Name = "Tab one" }).ClickAsync();
		Assert.That(async () => await editor.InnerHTMLAsync(), Does.Contain(expectedHtml).After(1000, 100));

		await editor.BlurAsync();
		Assert.That(async () => await editor.InnerHTMLAsync(), Does.Contain(expectedHtml).After(1000, 100));

		await editor.ClickAsync();
		Assert.That(async () => await editor.InnerHTMLAsync(), Does.Contain(expectedHtml).After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task SquiggleSpanPseudoElementStyle()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(CreateZFormWithTextBoxForTest<ZRichTextBox>);
		var editor = page.Locator(".richtextbox__editoranchor");
		await page.WaitForFunctionAsync("editor => editor.spellCheckEnabled", await editor.ElementHandleAsync(), new() { PollingInterval = 100, Timeout = 2000 });

		await editor.PressSequentiallyAsync("say hella ");

		var squiggleSpan = page.Locator(".richtextbox__editoranchor span[data-squiggle='true']");
		await squiggleSpan.WaitForAsync();

		Assert.That(() => squiggleSpan.GetComputedStyleAsync("position"), Is.EqualTo("relative"));
		Assert.That(() => squiggleSpan.GetComputedStyleAsync("::after", "content"), Is.EqualTo($"\"{new string(' ', 1000)}\""));
		Assert.That(() => squiggleSpan.GetComputedStyleAsync("::after", "position"), Is.EqualTo("absolute"));
		Assert.That(() => squiggleSpan.GetComputedStyleAsync("::after", "left"), Is.EqualTo("0px"));
		Assert.That(() => squiggleSpan.GetComputedStyleAsync("::after", "top"), Is.EqualTo("0px"));
		Assert.That(async () => (await squiggleSpan.GetComputedStyleAsync("::after", "width")).AsPixels(),  Is.EqualTo(28).Within(1));
		Assert.That(() => squiggleSpan.GetComputedStyleAsync("::after", "white-space"), Is.EqualTo("pre"));
		Assert.That(() => squiggleSpan.GetComputedStyleAsync("::after", "text-decoration-line"), Is.EqualTo("spelling-error"));
		Assert.That(() => squiggleSpan.GetComputedStyleAsync("::after", "overflow-x"), Is.EqualTo("clip"));
		Assert.That(() => squiggleSpan.GetComputedStyleAsync("::after", "pointer-events"), Is.EqualTo("none"));
	}

	[Test, WithPlaywrightPage]
	public async Task MarkSquiggleAcrossNodes()
	{
		var bold = "<span style=\"font-weight: bold;\">IAmBold</span>";
		var strong = "<strong>IAmStrong</strong>";
		var underlined = "<span style=\"text-decoration-line: underline;\">IAmUnderlined</span>";
		var plain = "IAmPlain";

		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(CreateZFormWithTextBoxForTest<ZRichTextBox>);
		var editor = page.Locator(".richtextbox__editoranchor");

		await editor.EvaluateAsync($"editor => editor.innerHTML = '<p>hello {bold}{strong}{underlined}{plain}</p>'");
		await page.WaitForFunctionAsync("editor => editor.spellCheckEnabled", await editor.ElementHandleAsync(), new() { PollingInterval = 100, Timeout = 2000 });

		await editor.PressAsync("End");
		await editor.PressAsync("!");

		var expectedHtml = $"<p>hello <span data-squiggle=\"true\">{bold}{strong}{underlined}{plain}</span>!</p>";
		Assert.That(() => editor.InnerHTMLAsync(), Is.EqualTo(expectedHtml).After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task NoTextCorruptionWithUnderlinedSpellErrorAndLineBreaks()
	{
		var underlinedSpellError = "<span style=\"text-decoration-line: underline;\">wor</span>";
		var anotherSpellError = "<span style=\"text-decoration-line: underline;\">ld</span>";
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(CreateZFormWithTextBoxForTest<ZRichTextBox>);
		var editor = page.Locator(".richtextbox__editoranchor");

		await editor.EvaluateAsync($"editor => editor.innerHTML = '<p>{underlinedSpellError}</p>'");
		await page.WaitForFunctionAsync("editor => editor.spellCheckEnabled", await editor.ElementHandleAsync(), new() { PollingInterval = 100, Timeout = 2000 });

		await editor.PressAsync("End");
		await editor.PressAsync("Enter");
		await editor.PressAsync("Enter");
		await editor.PressSequentiallyAsync("ld");
		await editor.PressAsync("Enter");

		Assert.That(() => editor.InnerHTMLAsync(), Does.Contain($"<span data-squiggle=\"true\">{underlinedSpellError}</span>").After(1000, 100));
		Assert.That(() => editor.InnerHTMLAsync(), Does.Contain($"<span data-squiggle=\"true\">{anotherSpellError}</span>").After(1000, 100));
	}

	ZForm CreateZFormWithTextBoxForTest<T>() where T : Control, new()
	{
		if (typeof(T) != typeof(ZTextBox) && typeof(T) != typeof(KRichTextBox) && typeof(T) != typeof(ZRichTextBox))
		{
			throw new ArgumentException($"SpellChecker only supports ZTextBox, KRichTextBox and ZRichTextBox, but you passed {typeof(T).Name}.");
		}

		var disposableAction = Db.DisposableActionForDbConnection();
		var form = new ZForm();
		var textBox = new T();
		textBox.Name = typeof(T).Name + "ForTest";

		switch (textBox)
		{
			case ZTextBox zTextBox:
				SpellChecker.InitialiseSpellcheck(zTextBox, textBox.Name);
				break;
			case KRichTextBox kRichTextBox:
				kRichTextBox.AcceptsTab = true;
				SpellChecker.InitialiseSpellcheck(kRichTextBox, textBox.Name);
				break;
			case ZRichTextBox zRichTextBox:
				SpellChecker.InitialiseSpellcheck(zRichTextBox, textBox.Name);
				break;
		}

		form.Disposed += (s, e) => disposableAction.Dispose();
		form.Controls.Add(textBox);
		return form;
	}
}
