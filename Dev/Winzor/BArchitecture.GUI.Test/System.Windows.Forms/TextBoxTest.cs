using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorFramework;
using WinzorFramework.JSInterop;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;

using static PlaywrightTestContext;

class TextBoxTest
{
	static readonly IEnumerable<bool> MultiLineTestCases = new[] { true, false };
	static readonly IEnumerable<HorizontalAlignment> TextAlignments = new[] { HorizontalAlignment.Left, HorizontalAlignment.Center, HorizontalAlignment.Right };

	[Test]
	public async Task OnTextChanged()
	{
		await ControlAssert.ImplementsProtectedOnMethodAsync<TextBox, EventArgs>("OnTextChanged", "input", e => e.TriggerEvent("oninput", new ChangeEventArgs { Value = "foo" }));
	}

	[Test]
	public async Task TextChangedEvent()
	{
		await ControlAssert.ImplementsEventAsync<TextBox, EventHandler>(nameof(TextBox.TextChanged), a => new EventHandler((s, e) => a()), "input", e => e.TriggerEvent("oninput", new ChangeEventArgs { Value = "foo" }));
	}

	[Test]
	public async Task EnterEvent()
	{
		await ControlAssert.ImplementsEventAsync<TextBox, EventHandler>(nameof(TextBox.Enter), a => new EventHandler((s, e) => a()), "input", e => e.TriggerEvent("onwinzorfocusin", new WinzorFocusInEventArgs()));
	}

	[Test]
	public async Task TextChangedFromServer()
	{
		await ControlAssert.ImplementsChangeFromServerAsync<TextBox>(c => c.Text = "foo");
	}

	[Test]
	public async Task ReadOnlyChangedFromServer()
	{
		await ControlAssert.ImplementsChangeFromServerAsync<TextBox>(c => c.ReadOnly = true);
	}

	[Test]
	public async Task EnabledChangedFromServer()
	{
		await ControlAssert.ImplementsChangeFromServerAsync<TextBox>(c => c.Enabled = false);
	}

	[Test]
	public async Task TabIndexChangedFromServer()
	{
		await ControlAssert.ImplementsChangeFromServerAsync<TextBox>(c => c.TabIndex = 11);
	}

	[Test]
	public async Task MaxLengthChangedFromServer()
	{
		await ControlAssert.ImplementsChangeFromServerAsync<TextBox>(c => c.MaxLength = 11);
	}

	[Test]
	public async Task PasswordCharChangedFromServer()
	{
		await ControlAssert.ImplementsChangeFromServerAsync<TextBox>(c => c.PasswordChar = '●');
	}

	[Test]
	public async Task CharacterCasingChangedFromServer()
	{
		await ControlAssert.ImplementsChangeFromServerAsync<TextBox>(c => c.CharacterCasing = CharacterCasing.Lower);
	}

	[Test]
	public async Task MultiLineChangedFromServer()
	{
		await ControlAssert.ImplementsChangeFromServerAsync<TextBox>(c => c.Multiline = true);
	}

	[Test]
	public async Task TextAlignChangedFromServer()
	{
		await ControlAssert.ImplementsChangeFromServerAsync<TextBox>(c => c.TextAlign = HorizontalAlignment.Center);
	}

	[TestCaseSource(nameof(MultiLineTestCases))]
	public async Task InitializeTextBeforeAddingToForm(bool multiline)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var textBox = new TextBox();
			textBox.Text = "foo";
			textBox.Multiline = multiline;
			return textBox;
		});

		var input = rendered.Find(".textbox");

		Assert.That(input.GetAttribute("style"), Does.Contain("position:absolute;width:100px;height:20px;top:0px;left:0px;background-color:var(--color-window);"));
		Assert.That(input.GetAttribute("value"), Is.EqualTo("foo"));
		Assert.That(input.GetAttribute("type"), Is.EqualTo("text"));
		Assert.That(input.GetAttribute("readonly"), Is.Null);
		Assert.That(input.GetAttribute("disabled"), Is.Null);
		Assert.That(input.GetAttribute("tabindex"), Is.EqualTo("0"));
		Assert.That(input.GetAttribute("maxlength"), Is.EqualTo("32767"));
	}

	[Test]
	[SuppressMessage("Usage", "VSTHRD103:Call async methods when in an async method")]
	public async Task RunCargoWiseAwaitsRenderTasks()
	{
		var readyTcs = new TaskCompletionSource();
		var renderTcs = new TaskCompletionSource();
		TextBox textBox = null;
		var renderTask = Task.Run(async () =>
		{
			using var ctx = new WinzorTestContext();
			(_, textBox) = await ctx.RenderControlOnFormAsync<TextBox>();
			readyTcs.SetResult();
			_ = ctx.Renderer.Dispatcher.InvokeAsync(renderTcs.Task.Wait);
		});
		readyTcs.Task.GetAwaiter().GetResult();
		var cargoWiseTask = textBox.InvokeWinzorDispatcherAsync(() =>
		{
			textBox.Text = "foo";
		});
		Assert.That(await cargoWiseTask.WithTimeout(TimeSpan.FromMilliseconds(100)), Is.False);
		renderTcs.SetResult();
		Assert.That(await cargoWiseTask.WithTimeout(TimeSpan.FromMilliseconds(100)), Is.True);
		Assert.That(await renderTask.WithTimeout(TimeSpan.FromMilliseconds(100)), Is.True);
	}

	[TestCaseSource(nameof(MultiLineTestCases)), WithPlaywrightPage]
	public async Task SetTextToDefaultMaxSizeString(bool multiLine)
	{
		var text = new string('1', 32766);
		await using var ctx = new InMemoryTestServerContext();

		TextBox textBox = null;

		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			textBox = new TextBox()
			{
				Width = 500,
				Height = 400,
				Multiline = multiLine
			};
			return textBox;
		});

		await page.WaitForSelectorAsync(".form");

		await textBox.InvokeWinzorDispatcherAsync(() => textBox.Text = text);
		Assert.That(() => textBox.Text, Is.EqualTo(text).After(2000, 200));

		var renderedTextBox = page.Locator(".textbox");

		Assert.That(async () => await renderedTextBox.InputValueAsync(), Is.EqualTo(text).After(2000, 200));
		Assert.That(textBox.SelectionStart, Is.EqualTo(32766));
		Assert.That(textBox.SelectionLength, Is.EqualTo(0));

		await renderedTextBox.PressSequentiallyAsync("a");

		Assert.That(() => textBox.SelectionStart, Is.EqualTo(32767).After(2000, 200));
		Assert.That(() => textBox.Text, Is.EqualTo(text + "a"));
		Assert.That(textBox.SelectionLength, Is.EqualTo(0));
	}

	[Test]
	public async Task RunCargoWiseOnlyAwaitsRenderTasksFromUnitOfWork()
	{
		var readyTcs = new TaskCompletionSource();
		var renderTcs = new TaskCompletionSource();
		Form dialogForm = null;
		TextBox textBox = null;
		using var ctx = new WinzorTestContext();
		var renderTask = Task.Run(async () =>
		{
			(_, textBox) = await ctx.RenderControlOnFormAsync<TextBox>();
			readyTcs.SetResult();
			_ = ctx.Renderer.Dispatcher.InvokeAsync(async () => await renderTcs.Task);
		});
		await readyTcs.Task;
		var cargoWiseTask1 = textBox.InvokeWinzorDispatcherAsync(() =>
		{
			textBox.Text = "foo";
			dialogForm = new Form();
			dialogForm.ShowDialog();
		});
		var cargoWiseTask2 = textBox.InvokeWinzorDispatcherAsync(() => { });

		Assert.That(await cargoWiseTask2.WithTimeout(TimeSpan.FromMilliseconds(100)), Is.True);
		dialogForm.Dispose();
		renderTcs.SetResult();
		Assert.That(await cargoWiseTask1.WithTimeout(TimeSpan.FromMilliseconds(100)), Is.True);
		Assert.That(await renderTask.WithTimeout(TimeSpan.FromMilliseconds(100)), Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task TextBoxSelectTextBySameStartAndLength()
	{
		await using var ctx = new InMemoryTestServerContext();
		TextBox textBox = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			textBox = new TextBox { Text = "A" };
			textBox.Focus();
			textBox.SelectAll();
			var form = new Form();
			form.Controls.Add(textBox);
			return form;
		});
		await page.WaitForSelectorAsync(".form");

		var textBoxInput = await page.WaitForSelectorAsync(".textbox");
		AssertTextAndSelectionOnClient(textBoxInput, "A", 0, 1);

		await textBox.InvokeWinzorDispatcherAsync(() =>
		{
			textBox.Text = "B";
			textBox.Focus();
			textBox.SelectAll();
		});
		textBoxInput = await page.WaitForSelectorAsync(".textbox");
		AssertTextAndSelectionOnClient(textBoxInput, "B", 0, 1);
	}

	[Test, WithPlaywrightPage]
	public async Task InteropHandlesBadElementReference()
	{
		var textBox = default(TextBoxForTest);
		await using var ctx = new InMemoryTestServerContext();
		await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(textBox = new TextBoxForTest());
			return form;
		});

		textBox.ElementReference = default;
		Assert.DoesNotThrowAsync(async () => await textBox.OnAfterRenderAsync(true));
	}

	[Test, WithPlaywrightPage]
	[TestCaseSource(nameof(MultiLineTestCases))]
	public async Task EventHandlersShouldBeAccessedByClient(bool multiline)
	{
		using var ctx = new WinzorTestContext();
		TextBox textBox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => textBox = new TextBox() { Multiline = multiline });

		var textBoxInput = rendered.Find(".textbox");
		var clientSideEventHandlers = textBoxInput.GetAttribute("data-client-side-event-handlers");
		
		Assert.That(Int32.Parse(clientSideEventHandlers), Is.EqualTo(textBox.ClientSideEventHandlers));
	}

	[TestCaseSource(nameof(MultiLineTestCases))]
	public async Task TextBoxReadOnly(bool multiline)
	{
		using var ctx = new WinzorTestContext();
		var readOnlyComponent = await ctx.RenderControlOnFormAsync(() => new TextBox() { ReadOnly = true, Multiline = multiline });
		var readOnlyInput = readOnlyComponent.Find(".textbox");
		await readOnlyInput.TriggerEventAsync("oninput", new ChangeEventArgs() { Value = "a" });

		Assert.That(readOnlyInput.Attributes["readonly"], Is.Not.Null);
		Assert.That(readOnlyInput.Attributes["value"].Value, Is.EqualTo(string.Empty));
	}

	[TestCaseSource(nameof(MultiLineTestCases))]
	public async Task TextboxDisabled(bool multiline)
	{
		using var ctx = new WinzorTestContext();
		var disabledComponent = await ctx.RenderControlOnFormAsync(() => new TextBox() { Enabled = false, Multiline = multiline });
		var disabledInput = disabledComponent.Find(".textbox");
		await disabledInput.TriggerEventAsync("oninput", new ChangeEventArgs() { Value = "a" });

		Assert.That(disabledInput.Attributes["disabled"], Is.Not.Null);
		Assert.That(disabledInput.Attributes["value"].Value, Is.EqualTo(string.Empty));
	}

	[TestCase(true,  1)]
	[TestCase(false, 0)]
	public async Task TestOnTextBoxSelectionChanged_WhenEnabledIs(bool enabled, int expectedCount)
	{
		TextBoxForTest textBox = null;
		var eventExecutionCount = 0;

		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			textBox = new TextBoxForTest { Enabled = enabled };
			textBox.SetCallback("OnTextBoxSelectionChanged", () => eventExecutionCount++);
		});

		await textBox.OnTextBoxSelectionChangedAsync(new TextboxSelectionChangeEventArgs { SelectionStart = 0, SelectionEnd = 1 });
		Assert.That(eventExecutionCount, Is.EqualTo(expectedCount));
	}

	[TestCase(true,  1)]
	[TestCase(false, 1)]
	public async Task TestOnTextBoxSelectionChanged_WhenReadOnlyIs(bool readOnly, int expectedCount)
	{
		TextBoxForTest textBox = null;
		var eventExecutionCount = 0;

		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			textBox = new TextBoxForTest { ReadOnly = readOnly };
			textBox.SetCallback("OnTextBoxSelectionChanged", () => eventExecutionCount++);
		});

		await textBox.OnTextBoxSelectionChangedAsync(new TextboxSelectionChangeEventArgs { SelectionStart = 0, SelectionEnd = 1 });
		Assert.That(eventExecutionCount, Is.EqualTo(expectedCount));
		Assert.That(textBox.selectionStart, Is.EqualTo(0));
		Assert.That(textBox.selectionLength, Is.EqualTo(1));
	}

	[TestCase(true,  1)]
	[TestCase(false, 0)]
	public async Task TestOnInput_WhenEnabledIs(bool enabled, int expectedCount)
	{
		TextBoxForTest textBox = null;
		var eventExecutionCount = 0;

		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			textBox = new TextBoxForTest { Enabled = enabled };
			textBox.SetCallback("OnInput", () => eventExecutionCount++);
		});

		await textBox.OnInputAsync(null);
		Assert.That(eventExecutionCount, Is.EqualTo(expectedCount));
	}

	[TestCase(true,  0)]
	[TestCase(false, 1)]
	public async Task TestOnInput_WhenReadOnlyIs(bool readOnly, int expectedCount)
	{
		TextBoxForTest textBox = null;
		var eventExecutionCount = 0;

		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			textBox = new TextBoxForTest { ReadOnly = readOnly };
			textBox.SetCallback("OnInput", () => eventExecutionCount++);
		});

		await textBox.OnInputAsync(null);
		Assert.That(eventExecutionCount, Is.EqualTo(expectedCount));
	}

	[TestCase(5, "", 1)]
	[TestCase(5, null, 1)]
	[TestCase(5, "12345", 1)]
	[TestCase(5, "123456", 0)]
	[TestCase(0, "12345", 1)]
	public async Task TestOnInput_WhenMaxTextLengthIs(int maxLength, string textInput, int expectedCount)
	{
		TextBoxForTest textBox = null;
		var eventExecutionCount = 0;

		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			textBox = new TextBoxForTest { MaxLength = maxLength };
			textBox.SetCallback("OnInput", () => eventExecutionCount++);
		});

		await textBox.OnInputAsync(textInput);
		Assert.That(eventExecutionCount, Is.EqualTo(expectedCount));
	}

	[Test]
	public async Task TextBoxAsPasswordInput()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var textBox = new TextBox() { PasswordChar = '●' };
			return textBox;
		});

		var input = rendered.Find(".textbox");
		await input.TriggerEventAsync("oninput", new ChangeEventArgs() { Value = "password" });

		Assert.That(input.GetAttribute("value"), Is.EqualTo("password"));
		Assert.That(input.GetAttribute("type"), Is.EqualTo("password"));
	}

	[TestCaseSource(nameof(MultiLineTestCases))]
	public async Task TextBoxInlineStyles(bool multiline)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new TextBox() { Multiline = multiline, Top = 100, Left = 200, Width = 300, Height = 400 });

		var input = rendered.Find(".textbox");
		var expectedHeight = multiline ? 400 : 20;
		Assert.That(input.GetAttribute("style"), Does.Contain($"position:absolute;width:300px;height:{expectedHeight}px;top:100px;left:200px;background-color:var(--color-window);"));
	}

	[TestCase(true, true, true, true, "-1")]
	[TestCase(true, false, true, true, "-1")]
	[TestCase(true, true, false, true, "0")]
	[TestCase(true, false, false, true, "-1")]
	[TestCase(true, true, true, false, "-1")]
	[TestCase(true, false, true, false, "-1")]
	[TestCase(true, true, false, false, "-1")]
	[TestCase(true, false, false, false, "-1")]
	[TestCase(false, true, true, true, "-1")]
	[TestCase(false, false, true, true, "-1")]
	[TestCase(false, true, false, true, "0")]
	[TestCase(false, false, false, true, "-1")]
	[TestCase(false, true, true, false, "-1")]
	[TestCase(false, false, true, false, "-1")]
	[TestCase(false, true, false, false, "-1")]
	[TestCase(false, false, false, false, "-1")]
	public async Task TestTextboxHasCorrectTabIndex(bool multiline, bool enabled, bool readOnly, bool tabStop, string expectedTabIndex)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new TextBox() { TabIndex = 11, Multiline = multiline, Enabled = enabled, ReadOnly = readOnly, TabStop = tabStop });
		var input = rendered.Find(".textbox");

		Assert.That(input.GetAttribute("tabindex"), Is.EqualTo(expectedTabIndex));
	}

	[TestCaseSource(nameof(MultiLineTestCases))]
	public async Task TextboxCharacterCasing(bool multiline)
	{
		using var ctx = new WinzorTestContext();
		TextBox lowerControl = null;
		TextBox upperControl = null;
		var rendererd = await ctx.RenderFormAsync(() =>
		{
			var newForm = new Form();
			lowerControl = new TextBox() { CharacterCasing = CharacterCasing.Lower, Multiline = multiline };
			upperControl = new TextBox() { CharacterCasing = CharacterCasing.Upper, Multiline = multiline };
			newForm.Controls.Add(new TextBox() { CharacterCasing = CharacterCasing.Normal, Multiline = multiline });
			newForm.Controls.Add(lowerControl);
			newForm.Controls.Add(upperControl);
			return newForm;
		});

		var normalInput = rendererd.Find(".textbox.textbox--border-fixed3d");
		var lowerInput = rendererd.Find(".textbox.textbox--lower.textbox--border-fixed3d");
		var upperlInput = rendererd.Find(".textbox.textbox--upper.textbox--border-fixed3d");

		await normalInput.TriggerEventAsync("oninput", new ChangeEventArgs() { Value = "Blah" });
		await lowerInput.TriggerEventAsync("oninput", new ChangeEventArgs() { Value = "Blah" });
		await upperlInput.TriggerEventAsync("oninput", new ChangeEventArgs() { Value = "Blah" });

		Assert.That(normalInput.Attributes["value"].Value, Is.EqualTo("Blah"));
		Assert.That(lowerControl.Text, Is.EqualTo("blah"));
		Assert.That(upperControl.Text, Is.EqualTo("BLAH"));
	}

	[TestCaseSource(nameof(MultiLineTestCases))]
	public async Task ChangeCharacterCasingAfterSettingText(bool multiline)
	{
		using var ctx = new WinzorTestContext();
		TextBox textBox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			textBox = new TextBox();
			textBox.CharacterCasing = CharacterCasing.Upper;
			textBox.Text = "Foo";
			textBox.CharacterCasing = CharacterCasing.Normal;
			textBox.Multiline = multiline;
			return textBox;
		});

		Assert.That(rendered.Find(".textbox").Attributes["value"].Value, Is.EqualTo("Foo"));
		await textBox.InvokeWinzorDispatcherAsync(() => textBox.CharacterCasing = CharacterCasing.Lower);
		Assert.That(textBox.Text, Is.EqualTo("foo"));
	}

	[TestCase(true, 20, TestName = "{m}_True")]
	[TestCase(false, 13, TestName = "{m}_False")]
	public async Task TextboxBorderStyle(bool multiline, int height)
	{
		using var ctx = new WinzorTestContext();
		TextBox textBox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			textBox = new TextBox();
			textBox.BorderStyle = BorderStyle.None;
			textBox.Multiline = multiline;
			return textBox;
		});

		var input = rendered.Find(".textbox");
		Assert.That(input.GetAttribute("style"), Does.Contain($"position:absolute;width:100px;height:{height}px;top:0px;left:0px;background-color:var(--color-window);"));
	}

	[Test]
	public async Task TextBoxPreferredHeight()
	{
		using var ctx = new WinzorTestContext();
		TextBox textBox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => textBox = new TextBox());
		Assert.That(textBox.PreferredHeight, Is.EqualTo(20));

		await textBox.InvokeWinzorDispatcherAsync(() => textBox.BorderStyle = BorderStyle.None);
		Assert.That(textBox.PreferredHeight, Is.EqualTo(13));
	}

	[Test]
	public async Task TextBoxHeightWhenSetAutoSizeIsFalseAfterSize()
	{
		using var ctx = new WinzorTestContext();
		TextBox textBox = null;
		await ctx.RenderControlOnFormAsync(() =>
		{
			textBox = new TextBox();
			textBox.AutoSize = true;
			return textBox;
		});

		Assert.That(textBox.Height, Is.EqualTo(textBox.PreferredHeight));

		await textBox.InvokeWinzorDispatcherAsync(() => textBox.Size = new Size(400, 200));
		Assert.That(textBox.Height, Is.EqualTo(textBox.PreferredHeight));

		await textBox.InvokeWinzorDispatcherAsync(() => textBox.AutoSize = false);
		Assert.That(textBox.Height, Is.EqualTo(200));
	}

	[Test]
	public async Task TextBoxHeightWhenSetSizeIsSameAsPreferredHeight()
	{
		using var ctx = new WinzorTestContext();

		TextBox textBox = null;
		await ctx.RenderControlOnFormAsync(() =>
		{
			textBox = new TextBox();
			textBox.AutoSize = true;
			textBox.Size = new Size(400, 200);
			return textBox;
		});

		Assert.That(textBox.Height, Is.EqualTo(textBox.PreferredHeight));

		await textBox.InvokeWinzorDispatcherAsync(() =>
		{
			textBox.Size = new Size(100, textBox.PreferredHeight);
			textBox.AutoSize = false;
		});
		Assert.That(textBox.Height, Is.EqualTo(textBox.PreferredHeight));
	}

	[Test]
	public async Task TextBoxAutoSizeIsTrueByDefault()
	{
		using var ctx = new WinzorTestContext();
		TextBox textBox = null;
		await ctx.RenderControlOnFormAsync(() => textBox = new TextBox());
		Assert.That(textBox.AutoSize);
	}

	[Test]
	public async Task TextBoxHeightWhenSetMultilineAfterSize()
	{
		using var ctx = new WinzorTestContext();
		TextBox textBox = null;
		await ctx.RenderControlOnFormAsync(() =>
		{
			textBox = new TextBox();
			textBox.AutoSize = true;
			return textBox;
		});
		Assert.That(textBox.Height, Is.EqualTo(textBox.PreferredHeight));

		await textBox.InvokeWinzorDispatcherAsync(() => textBox.Size = new Size(400, 200));

		Assert.That(textBox.Height, Is.EqualTo(textBox.PreferredHeight));

		await textBox.InvokeWinzorDispatcherAsync(() => textBox.Multiline = true);
		Assert.That(textBox.Height, Is.EqualTo(200));

		await textBox.InvokeWinzorDispatcherAsync(() => textBox.Multiline = false);
		Assert.That(textBox.Height, Is.EqualTo(textBox.PreferredHeight));
	}

	[TestCaseSource(nameof(MultiLineTestCases))]
	public async Task TextBoxMaxLength(bool multiline)
	{
		using var ctx = new WinzorTestContext();
		TextBox textBox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => textBox = new TextBox() { Multiline = multiline });
		Assert.That(rendered.Find(".textbox").GetAttribute("maxLength"), Is.EqualTo("32767"));

		await textBox.InvokeWinzorDispatcherAsync(() => textBox.MaxLength = 0);
		Assert.That(rendered.Find(".textbox").GetAttribute("maxLength"), Is.Empty);

		await textBox.InvokeWinzorDispatcherAsync(() => textBox.MaxLength = 67);
		Assert.That(rendered.Find(".textbox").GetAttribute("maxLength"), Is.EqualTo("67"));
	}

	[Test]
	public async Task TextBoxLineBreakWhenAppendText()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var textBox0 = new TextBox();
			textBox0.AppendText("Slower today");
			textBox0.AppendText(Environment.NewLine);
			textBox0.AppendText("Faster forever");

			var textBox1 = new TextBox();
			textBox1.AppendText(Environment.NewLine + "Lead with content");

			form.Controls.Add(textBox0);
			form.Controls.Add(textBox1);

			Assert.That(textBox0.Text, Is.EqualTo("Slower today\r\nFaster forever"));
			Assert.That(textBox1.Text, Is.EqualTo("\r\nLead with content"));
			return form;
		});

		var textBoxList = rendered.FindAll(".textbox");
		Assert.That(textBoxList[0].GetAttribute("value"), Does.Contain("\n").And.Not.Contain("\r"));
		Assert.That(textBoxList[1].GetAttribute("value"), Does.Contain("\n"));
	}

	[TestCaseSource(nameof(TextAlignments))]
	public async Task TextAlign(HorizontalAlignment alignment)
	{
		using var ctx = new WinzorTestContext();
		TextBox textBox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			textBox = new TextBox();
			textBox.TextAlign = alignment;
			return textBox;
		});

		var input = rendered.Find(".textbox");
		if (alignment == HorizontalAlignment.Left)
		{
			Assert.That(input.GetAttribute("style"), Does.Not.Contain($"--textbox-align:"));
		}
		else
		{
			Assert.That(input.GetAttribute("style"), Does.Contain($"--textbox-align:{alignment};"));
		}
	}

	[Test, WithPlaywrightPage]
	[TestCaseSource(nameof(MultiLineTestCases))]
	public async Task TextBoxSelectionChangeEventIsInvoked(bool multiline)
	{
		await using var ctx = new InMemoryTestServerContext();

		TextBox textBox = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			textBox = new TextBox()
			{
				Text = "Test Text",
				Height = 25,
				Width = 100,
				Multiline = multiline,
			};
			var form = new Form();
			form.Controls.Add(textBox);
			return form;
		});

		var textBoxInput = await page.WaitForSelectorAsync(".textbox");
		await textBoxInput.ClickAsync();
		AssertTextAndSelectionOnServer(textBox, "Test Text", 9, 0, string.Empty);

		await page.Keyboard.PressAsync("Shift+ArrowLeft");
		AssertTextAndSelectionOnServer(textBox, "Test Text", 8, 1, "t");

		await page.Keyboard.PressAsync("Shift+ArrowLeft");
		AssertTextAndSelectionOnServer(textBox, "Test Text", 7, 2, "xt");

		await textBoxInput.SelectTextAsync();
		AssertTextAndSelectionOnServer(textBox, "Test Text", 0, 9, "Test Text");
	}

	[Test, WithPlaywrightPage]
	[TestCaseSource(nameof(MultiLineTestCases))]
	public async Task TextBoxSelectionIsSetOnRender(bool multiline)
	{
		await using var ctx = new InMemoryTestServerContext();
		TextBox textBox = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			textBox = new TextBox()
			{
				Text = "Test Text",
				Height = 25,
				Width = 100,
				Multiline = multiline,
			};
			var form = new Form();
			form.Controls.Add(textBox);
			return form;
		});

		var textBoxInput = page.Locator(".textbox");
		await textBoxInput.FocusAsync();
		Assert.That(await textBoxInput.InputValueAsync(), Is.EqualTo("Test Text"));
		await textBox.InvokeWinzorDispatcherAsync(() =>
		{
			textBox.SelectionStart = 1;
			textBox.SelectionLength = 7;
		});
		await page.WaitForTimeoutAsync(100);
		Assert.That(await textBoxInput.EvaluateAsync<int>("i => i.selectionStart"), Is.EqualTo(1));
		Assert.That(await textBoxInput.EvaluateAsync<int>("i => i.selectionEnd"), Is.EqualTo(8));
	}

	[Test]
	public async Task TextBoxSetSelectedTextShouldSetTextBoxText()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var textBox = new TextBox() { Text = "", SelectionStart = 0, SelectionLength = 0, };
			Assert.That(textBox.SelectionStart, Is.EqualTo(0));
			Assert.That(textBox.SelectionLength, Is.EqualTo(0));
			textBox.SelectedText = "Hello";
			Assert.That(textBox.SelectionStart, Is.EqualTo(5));
			Assert.That(textBox.SelectionLength, Is.EqualTo(0));
			Assert.That(textBox.Text, Is.EqualTo("Hello"));
			Assert.That(textBox.SelectedText, Is.EqualTo(string.Empty));

			textBox = new TextBox() { Text = "Hello ", SelectionStart = 6, SelectionLength = 0, };
			Assert.That(textBox.SelectionStart, Is.EqualTo(6));
			Assert.That(textBox.SelectionLength, Is.EqualTo(0));
			textBox.SelectedText = "World";
			Assert.That(textBox.SelectionStart, Is.EqualTo(11));
			Assert.That(textBox.SelectionLength, Is.EqualTo(0));
			Assert.That(textBox.Text, Is.EqualTo("Hello World"));
			Assert.That(textBox.SelectedText, Is.EqualTo(string.Empty));

			textBox = new TextBox() { Text = "My World", SelectionStart = 3, SelectionLength = 0, };
			Assert.That(textBox.SelectionStart, Is.EqualTo(3));
			Assert.That(textBox.SelectionLength, Is.EqualTo(0));
			textBox.SelectedText = "Hello ";
			Assert.That(textBox.SelectionStart, Is.EqualTo(9));
			Assert.That(textBox.SelectionLength, Is.EqualTo(0));
			Assert.That(textBox.Text, Is.EqualTo("My Hello World"));
			Assert.That(textBox.SelectedText, Is.EqualTo(string.Empty));

			textBox = new TextBox() { Text = "My World", SelectionStart = 2, SelectionLength = 1, };
			Assert.That(textBox.SelectionStart, Is.EqualTo(2));
			Assert.That(textBox.SelectionLength, Is.EqualTo(1));
			Assert.That(textBox.SelectedText, Is.EqualTo(" "));
			textBox.SelectedText = " Hello ";
			Assert.That(textBox.SelectionStart, Is.EqualTo(9));
			Assert.That(textBox.SelectionLength, Is.EqualTo(0));
			Assert.That(textBox.Text, Is.EqualTo("My Hello World"));
			Assert.That(textBox.SelectedText, Is.EqualTo(string.Empty));
		});
	}

	[TestCaseSource(nameof(MultiLineTestCases))]
	[Test, WithPlaywrightPage]
	public async Task SetSelectedTextFromForm(bool multiline)
	{
		await using var ctx = new InMemoryTestServerContext();

		TextBox textBox = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			textBox = new TextBox
			{
				Text = "Test Text",
				Height = 25,
				Width = 100,
				Multiline = multiline
			};
			var form = new Form();
			form.Controls.Add(textBox);
			return form;
		});

		await page.WaitForSelectorAsync(".textbox");
		await textBox.InvokeWinzorDispatcherAsync(() =>
		{
			textBox.Focus();
			textBox.SelectionStart = 5;
			textBox.SelectionLength = 4;
		});

		var textBoxInput = page.Locator(".textbox");
		Assert.That(await textBoxInput.InputValueAsync(), Is.EqualTo("Test Text"));

		await page.WaitForTimeoutAsync(100);
		await textBox.InvokeWinzorDispatcherAsync(() =>
		{
			textBox.SelectedText = "Test";
		});

		Assert.That(async () => await textBoxInput.InputValueAsync(), Is.EqualTo("Test Test").After(3000, 100));
		Assert.That(async () => await textBoxInput.EvaluateAsync<int>("i => i.selectionStart"), Is.EqualTo(9).After(3000, 100));
		Assert.That(async () => await textBoxInput.EvaluateAsync<int>("i => i.selectionEnd"), Is.EqualTo(9).After(3000, 100));
	}

	[Test]
	public async Task TextBoxSelectAllSetsTextSelection()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var textBox = new TextBox() { Text = "Hello World", SelectionStart = 1, SelectionLength = 1, };
			Assert.That(textBox.SelectionStart, Is.EqualTo(1));
			Assert.That(textBox.SelectionLength, Is.EqualTo(1));
			textBox.SelectAll();
			Assert.That(textBox.SelectionStart, Is.EqualTo(0));
			Assert.That(textBox.SelectionLength, Is.EqualTo(11));
		});
	}

	[Test, WithPlaywrightPage]
	public async Task AutoSelectAllTextOnGotFocusAfterTextSet()
	{
		await using var ctx = new InMemoryTestServerContext();
		TextBox textBox = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			textBox = new TextBox() { Text = "textBox" };
			form.Controls.Add(textBox);
			return form;
		});

		var textBoxInput = await page.Locator(".textbox").First.ElementHandleAsync();
		AssertTextAndSelectionOnClient(textBoxInput, textBox.Text, 0, textBox.Text.Length);
		AssertTextAndSelectionOnServer(textBox, textBox.Text, 0, textBox.Text.Length, textBox.Text);

		await textBox.InvokeWinzorDispatcherAsync(() =>
		{
			textBox.Select(1,2);
			textBox.WmKillFocus();
			textBox.Text = "textBox";
			textBox.selectionLength = 0;
			textBox.SetFocus();
		});
		AssertTextAndSelectionOnClient(textBoxInput, textBox.Text, 0, textBox.Text.Length);
		AssertTextAndSelectionOnServer(textBox, textBox.Text, 0, textBox.Text.Length, textBox.Text);
	}

	[Test, WithPlaywrightPage]
	public async Task TextBoxParellelSetCalls()
	{	
		TextBox textBox = null;	
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			textBox = new TextBox() { Text = "textBox1" };
			return textBox;
		});
		await Task.WhenAll(
			textBox.InvokeWinzorDispatcherAsync(() => textBox.Text = "textBox1"),
			textBox.InvokeWinzorDispatcherAsync(() => textBox.Text = "textBox1"),
			textBox.InvokeWinzorDispatcherAsync(() => textBox.Text = "textBox1"));

		Assert.That(ctx.JSInterop.Invocations.Count(i => i.Identifier == TextBoxJSInterop.SetTextContentIdentifier), Is.EqualTo(3));
	}

	[Test]
	public async Task ResetSelectionAfterTextSet()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var textBox = new TextBox() { Text = "Hello World", SelectionStart = 2, SelectionLength = 4 };
			Assert.That(textBox.SelectionStart, Is.EqualTo(2));
			Assert.That(textBox.SelectionLength, Is.EqualTo(4));
			Assert.That(textBox.selectionStart, Is.EqualTo(2));
			Assert.That(textBox.selectionLength, Is.EqualTo(4));

			textBox.Text = "NotCreateHandle";
			Assert.That(textBox.SelectionStart, Is.EqualTo(2));
			Assert.That(textBox.SelectionLength, Is.EqualTo(4));
			Assert.That(textBox.selectionStart, Is.EqualTo(2));
			Assert.That(textBox.selectionLength, Is.EqualTo(4));

			textBox.CreateControl();
			textBox.Text = "CreateHandle";
			Assert.That(textBox.SelectionStart, Is.EqualTo(0));
			Assert.That(textBox.SelectionLength, Is.EqualTo(0));
			Assert.That(textBox.selectionStart, Is.EqualTo(0));
			Assert.That(textBox.selectionLength, Is.EqualTo(0));
		});
	}

	[TestCase(1, 3)]
	[TestCase(1, 0)]
	[TestCase(0, 11)]
	public async Task TextBoxSelectionStartPlusSelectionLengthShouldLessOrEqualThanTextStringSizeAfterTextChanged(int selectionStart, int selectionLength)
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var textBox = new TextBox() { Text = "Hello World", SelectionStart = selectionStart, SelectionLength = selectionLength };
			textBox.SelectAll();
			textBox.Text = "";
			Assert.That(textBox.SelectionStart + textBox.SelectionLength, Is.LessThanOrEqualTo(textBox.TextLength));
		});
	}

	[Test, WithPlaywrightPage]
	public async Task BackspaceUpdatesSelectionOnServer()
	{
		await using var ctx = new InMemoryTestServerContext();
		TextBox textBox = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			textBox = new TextBox { Text = "Test Text" };
			var form = new Form();
			form.Controls.Add(textBox);
			return form;
		});

		await page.WaitForSelectorAsync(".textbox");

		await textBox.InvokeWinzorDispatcherAsync(() =>
		{
			textBox.Focus();
			textBox.SelectionStart = 4;
			textBox.SelectionLength = 5;
		});

		var textBoxInput = await page.WaitForSelectorAsync(".textbox");
		AssertTextAndSelectionOnClient(textBoxInput, "Test Text", 4, 9);
		AssertTextAndSelectionOnServer(textBox, "Test Text", 4, 5, " Text");

		await page.Keyboard.PressAsync("Backspace");
		AssertTextAndSelectionOnClient(textBoxInput, "Test", 4, 4);
		AssertTextAndSelectionOnServer(textBox, "Test", 4, 0, string.Empty);

		await page.Keyboard.PressAsync("Backspace");
		AssertTextAndSelectionOnClient(textBoxInput, "Tes", 3, 3);
		AssertTextAndSelectionOnServer(textBox, "Tes", 3, 0, string.Empty);
	}

	[Test, WithPlaywrightPage]
	public async Task TextBoxCanHandleNumPadEnter()
	{
		await using var ctx = new InMemoryTestServerContext();
		TextBox textBox = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			textBox = new TextBox { Text = "Test Text" };
			textBox.Focus();
			var form = new Form();
			form.Controls.Add(textBox);
			return form;
		});

		var textBoxInput = await page.WaitForSelectorAsync(".textbox");

		await page.Keyboard.PressAsync("NumpadEnter");
	}

	[Test, WithPlaywrightPage]
	public async Task CheckMultilineTextBoxScrollBarButtonsHeight()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var textBox = new TextBox
			{
				Text = "Test Text 1",
				Multiline = true,
				Height = 26,
				ScrollBars = ScrollBars.Vertical
			};
			form.Controls.Add(textBox);
			var textBox2 = new TextBox
			{
				Text = "Test Text 2",
				Multiline = true,
				Height = 100,
				ScrollBars = ScrollBars.Vertical,
				Top = 100
			};
			form.Controls.Add(textBox2);
			return form;
		});

		var textBoxes = await page.QuerySelectorAllAsync(".textbox");
		Assert.That(await textBoxes[0].EvaluateAsync<string>("e => window.getComputedStyle(e, '::-webkit-scrollbar-button').height"), Is.EqualTo("49%"));
		Assert.That(await textBoxes[1].EvaluateAsync<string>("e => window.getComputedStyle(e, '::-webkit-scrollbar-button').height"), Is.EqualTo("18px"));
	}

	[Test, WithPlaywrightPage]
	public async Task DeleteUpdatesSelectionOnServer()
	{
		await using var ctx = new InMemoryTestServerContext();
		TextBox textBox = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			textBox = new TextBox { Text = "Test Text!!" };
			var form = new Form();
			form.Controls.Add(textBox);
			return form;
		});

		await page.WaitForSelectorAsync(".textbox");

		await textBox.InvokeWinzorDispatcherAsync(() =>
		{
			textBox.Focus();
			textBox.SelectionStart = 4;
			textBox.SelectionLength = 5;
		});

		var textBoxInput = await page.WaitForSelectorAsync(".textbox");
		AssertTextAndSelectionOnClient(textBoxInput, "Test Text!!", 4, 9);
		AssertTextAndSelectionOnServer(textBox, "Test Text!!", 4, 5, " Text");

		await page.Keyboard.PressAsync("Delete");
		AssertTextAndSelectionOnClient(textBoxInput, "Test!!", 4, 4);
		AssertTextAndSelectionOnServer(textBox, "Test!!", 4, 0, string.Empty);

		await page.Keyboard.PressAsync("Delete");
		AssertTextAndSelectionOnClient(textBoxInput, "Test!", 4, 4);
		AssertTextAndSelectionOnServer(textBox, "Test!", 4, 0, string.Empty);
	}

	[TestCase(',', false)]
	[TestCase(',', true)]
	[WithPlaywrightPage]
	public async Task TextBoxReplaceCharacterOnKeyDown(char replacementChar, bool caretMoved)
	{
		await using var ctx = new InMemoryTestServerContext();
		var delimiterKeyPressReceived = false;
		var replacementCharSuppressed = true;

		var page = await ctx.LoadFormAsync(() =>
		{
			var textbox = new TextBox() { ReplacementCharacters = new Dictionary<char, char>() { { '.', replacementChar } } };

			textbox.KeyPress += (sender, e) =>
			{
				if (e.KeyChar == '.')
				{
					delimiterKeyPressReceived = true;
				}

				if (e.KeyChar == replacementChar)
				{
					replacementCharSuppressed = false;
				}
			};

			var form = new Form { Width = 1000, Height = 1000 };
			form.Controls.Add(textbox);
			return form;
		});
		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

		var textBoxInput = await page.WaitForSelectorAsync("input");
		await page.Mouse.DblClickAsync(10, 10);

		if (caretMoved)
		{
			await page.Keyboard.TypeAsync("15000");
			await page.Keyboard.DownAsync("ArrowLeft");
			await page.Keyboard.DownAsync("ArrowLeft");
			await page.Keyboard.DownAsync("ArrowLeft");
			await page.Keyboard.TypeAsync(".");
		}
		else
		{
			await page.Keyboard.TypeAsync("15.000");
		}

		await page.Mouse.ClickAsync(100, 100);

		Assert.That(async () => await textBoxInput.InputValueAsync(), Is.EqualTo("15,000").After(3000, 100));

		Assert.That(async () => await textBoxInput.EvaluateAsync<int>("i => i.selectionStart"), Is.EqualTo(caretMoved ? 3 : 6).After(3000, 100));
		Assert.That(async () => await textBoxInput.EvaluateAsync<int>("i => i.selectionEnd"), Is.EqualTo(caretMoved ? 3 : 6).After(3000, 100));

		Assert.That(() => delimiterKeyPressReceived, Is.EqualTo(true).After(3000, 100));
		Assert.That(() => replacementCharSuppressed, Is.EqualTo(true).After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TextBoxInputBehavior()
	{
		await using var ctx = new InMemoryTestServerContext();

		TextBox textBox = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			textBox = new TextBox();
			textBox.KeyPress += (sender, e) =>
			{
				if (e.KeyChar == 'o')
				{
					textBox.Text = "override";
					textBox.SelectionStart = 8;
					e.Handled = true;
				}
			};

			form.Controls.Add(textBox);

			return form;
		});

		var textBoxInput = await page.WaitForSelectorAsync("input");
		await textBoxInput.PressAsync("KeyT");
		await textBoxInput.PressAsync("KeyE");
		await textBoxInput.PressAsync("KeyS");
		await textBoxInput.PressAsync("KeyT");

		Assert.That(async () => await textBoxInput.InputValueAsync(), Is.EqualTo("test").After(3000, 100));
		Assert.That(async () => await textBoxInput.EvaluateAsync<int>("i => i.selectionStart"), Is.EqualTo(4).After(3000, 100));
		Assert.That(async () => await textBoxInput.EvaluateAsync<int>("i => i.selectionEnd"), Is.EqualTo(4).After(3000, 100));
		Assert.That(() => textBox.Text, Is.EqualTo("test").After(3000, 100));
		Assert.That(() => textBox.SelectionStart, Is.EqualTo(4).After(3000, 100));
		Assert.That(() => textBox.SelectionLength, Is.EqualTo(0).After(3000, 100));

		await textBoxInput.PressAsync("KeyO");

		Assert.That(async () => await textBoxInput.InputValueAsync(), Is.EqualTo("override").After(3000, 100));
		Assert.That(async () => await textBoxInput.EvaluateAsync<int>("i => i.selectionStart"), Is.EqualTo(8).After(3000, 100));
		Assert.That(async () => await textBoxInput.EvaluateAsync<int>("i => i.selectionEnd"), Is.EqualTo(8).After(3000, 100));
		Assert.That(() => textBox.Text, Is.EqualTo("override").After(3000, 100));
		Assert.That(() => textBox.SelectionStart, Is.EqualTo(8).After(3000, 100));
		Assert.That(() => textBox.SelectionLength, Is.EqualTo(0).After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TextAndSelectionRemainUnchangedOnHandledInput()
	{
		await using var ctx = new InMemoryTestServerContext();

		TextBox textBox = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			textBox = new TextBox();

			textBox.KeyPress += (sender, e) =>
			{
				if (e.KeyChar == 'o')
				{
					e.Handled = true;
				}
			};

			form.Controls.Add(textBox);
			return form;
		});

		var textBoxInput = page.Locator(".textbox");
		// typing "111"
		await textBoxInput.PressSequentiallyAsync("111");
		// select all
		await textBoxInput.PressAsync("Control+A");

		// now the text should be "111", and all selected
		Assert.That(async () => await textBoxInput.InputValueAsync(), Is.EqualTo("111").After(3000, 100));
		Assert.That(async () => await textBoxInput.EvaluateAsync<int>("i => i.selectionStart"), Is.EqualTo(0).After(3000, 100));
		Assert.That(async () => await textBoxInput.EvaluateAsync<int>("i => i.selectionEnd"), Is.EqualTo(3).After(3000, 100));

		// pressing 'o', it should be handled
		await textBoxInput.PressAsync("KeyO");

		// server side should revert to the original text and selection
		Assert.That(() => textBox.Text, Is.EqualTo("111").After(3000, 100));
		Assert.That(() => textBox.SelectionStart, Is.EqualTo(0).After(3000, 100));
		Assert.That(() => textBox.SelectionLength, Is.EqualTo(3).After(3000, 100));

		// client side should be updated as well
		Assert.That(async () => await textBoxInput.InputValueAsync(), Is.EqualTo("111").After(3000, 100));
		Assert.That(async () => await textBoxInput.EvaluateAsync<int>("i => i.selectionStart"), Is.EqualTo(0).After(3000, 100));
		Assert.That(async () => await textBoxInput.EvaluateAsync<int>("i => i.selectionEnd"), Is.EqualTo(3).After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TextAndSelectionRemainUnchangedOnSupressKeyPress()
	{
		await using var ctx = new InMemoryTestServerContext();

		TextBox textBox = null;

		var keyDownRaised = false;
		var keyPressRaised = false;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.KeyPreview = true;

			form.KeyDown += (sender, e) =>
			{
				keyDownRaised = true;
				if (e.KeyCode == Keys.O)
				{
					e.SuppressKeyPress = true;
				}
			};

			textBox = new TextBox();
			textBox.Text = "111";
			textBox.SelectionStart = 4;
			textBox.SelectionLength = 0;

			textBox.KeyPress += (sender, e) => keyPressRaised = true;

			form.Controls.Add(textBox);
			return form;
		});

		var textBoxInput = page.Locator(".textbox");

		await textBoxInput.PressAsync("KeyO");
		Assert.That(() => keyDownRaised, Is.EqualTo(true).After(3000, 100));
		Assert.That(() => keyPressRaised, Is.EqualTo(false).After(3000, 100));
		Assert.That(async () => await textBoxInput.InputValueAsync(), Is.EqualTo("111").After(3000, 100));
		Assert.That(() => textBox.Text, Is.EqualTo("111").After(6000, 100));

		await textBoxInput.PressAsync("KeyX");
		Assert.That(() => keyPressRaised, Is.EqualTo(true).After(3000, 100));
		Assert.That(async () => await textBoxInput.InputValueAsync(), Is.EqualTo("111x").After(3000, 100));
		Assert.That(() => textBox.Text, Is.EqualTo("111x").After(6000, 100));
	}

	[WithPlaywrightPage]
	[TestCase(BorderStyle.Fixed3D, "1px solid rgb(0, 120, 215)", "rgb(255, 255, 255) 0px 0px 0px 1px inset")]
	[TestCase(BorderStyle.None, "0px none rgb(0, 120, 215)", "none")]
	[TestCase(BorderStyle.FixedSingle, "1px solid rgb(0, 120, 215)", "none")]
	public async Task TextBoxStyle(BorderStyle borderStyle, string expectedBorderStyle, string expectedBorderShadowStyle)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new TextBox()
		{
			Text = "Test Text",
			BorderStyle = borderStyle,
		});
		var textBoxInput = await page.WaitForSelectorAsync(".textbox");

		Assert.That(() => textBoxInput.GetComputedStyleAsync("border"), Is.EqualTo(expectedBorderStyle).After(2000, 200));
		Assert.That(() => textBoxInput.GetComputedStyleAsync("box-shadow"), Is.EqualTo(expectedBorderShadowStyle).After(2000, 200));
	}

	[Test, WithPlaywrightPage]
	public async Task TestTextBoxKeyDown()
	{
		await using var ctx = new InMemoryTestServerContext();
		TextBox textBox = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			textBox = new TextBox()
			{
				Text = "test",
				SelectionStart = 4,
				SelectionLength = 0,
			};

			var form = new Form();
			form.Controls.Add(textBox);
			return form;
		});

		var textBoxInput = await page.WaitForSelectorAsync("input");
		Assert.That(async () => await textBoxInput.EvaluateAsync<int>("i => i.selectionStart"), Is.EqualTo(4).After(3000, 100));
		Assert.That(async () => await textBoxInput.EvaluateAsync<int>("i => i.selectionEnd"), Is.EqualTo(4).After(3000, 100));

		await textBoxInput.PressAsync("ArrowLeft");

		Assert.That(async () => await textBoxInput.InputValueAsync(), Is.EqualTo("test").After(3000, 100));
		Assert.That(async () => await textBoxInput.EvaluateAsync<int>("i => i.selectionStart"), Is.EqualTo(3).After(3000, 100));
		Assert.That(async () => await textBoxInput.EvaluateAsync<int>("i => i.selectionEnd"), Is.EqualTo(3).After(3000, 100));
		Assert.That(() => textBox.Text, Is.EqualTo("test").After(3000, 100));
		Assert.That(() => textBox.SelectionStart, Is.EqualTo(3).After(3000, 100));
		Assert.That(() => textBox.SelectionLength, Is.EqualTo(0).After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestTextBoxKeyDownIsHandled()
	{
		await using var ctx = new InMemoryTestServerContext();
		TextBox textBox = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			textBox = new TextBox()
			{
				Text = "test",
				SelectionStart = 4,
				SelectionLength = 0,
			};

			textBox.KeyDown += (sender, e) =>
			{
				e.Handled = true;
			};

			var form = new Form();
			form.Controls.Add(textBox);
			return form;
		});

		var textBoxInput = await page.WaitForSelectorAsync("input");
		Assert.That(async () => await textBoxInput.EvaluateAsync<int>("i => i.selectionStart"), Is.EqualTo(4).After(3000, 100));
		Assert.That(async () => await textBoxInput.EvaluateAsync<int>("i => i.selectionEnd"), Is.EqualTo(4).After(3000, 100));

		await textBoxInput.PressAsync("ArrowLeft");

		Assert.That(async () => await textBoxInput.InputValueAsync(), Is.EqualTo("test").After(3000, 100));
		Assert.That(async () => await textBoxInput.EvaluateAsync<int>("i => i.selectionStart"), Is.EqualTo(4).After(3000, 100));
		Assert.That(async () => await textBoxInput.EvaluateAsync<int>("i => i.selectionEnd"), Is.EqualTo(4).After(3000, 100));
		Assert.That(() => textBox.Text, Is.EqualTo("test").After(3000, 100));
		Assert.That(() => textBox.SelectionStart, Is.EqualTo(4).After(3000, 100));
		Assert.That(() => textBox.SelectionLength, Is.EqualTo(0).After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TextboxDisabledBorderColor()
	{
		await using var ctx = new InMemoryTestServerContext();
		TextBox textBox = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			textBox = new TextBox()
			{
				Enabled = false
			};
			var form = new Form();
			form.Controls.Add(textBox);
			return form;
		});

		var textBoxInput = await page.WaitForSelectorAsync(".textbox");
		var expectedBorderStyle = "1px solid rgb(204, 204, 204)";
		var actualBorderStyle = await textBoxInput.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('border')");
		Assert.That(actualBorderStyle, Is.EqualTo(expectedBorderStyle));
	}

	[Test, WithPlaywrightPage]
	public async Task TextBoxWithNoBorderShouldHaveZeroPadding()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var textBox = new TextBox()
			{
				BorderStyle = BorderStyle.None
			};
			var form = new Form();
			form.Controls.Add(textBox);
			return form;
		});
		var textBoxInput = await page.WaitForSelectorAsync(".textbox");

		var actualPaddingStyle = await textBoxInput.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('padding')");
		Assert.That(actualPaddingStyle, Is.EqualTo("0px"));
	}

	const string OverflowNone = "overflow-x:hidden;overflow-y:auto;";
	const string OverflowScroll = "overflow:scroll;";
	const string OverflowXScroll = "overflow-x:scroll;";
	const string OverflowYScroll = "overflow-y:scroll;";

	[TestCase(ScrollBars.None, true, HorizontalAlignment.Left, OverflowNone, new[] { OverflowScroll, OverflowXScroll, OverflowYScroll })]
	[TestCase(ScrollBars.None, true, HorizontalAlignment.Center, OverflowNone, new[] { OverflowScroll, OverflowXScroll, OverflowYScroll })]
	[TestCase(ScrollBars.None, true, HorizontalAlignment.Right, OverflowNone, new[] { OverflowScroll, OverflowXScroll, OverflowYScroll })]
	[TestCase(ScrollBars.None, false, HorizontalAlignment.Left, OverflowNone, new[] { OverflowScroll, OverflowXScroll, OverflowYScroll })]
	[TestCase(ScrollBars.None, false, HorizontalAlignment.Center, OverflowNone, new[] { OverflowScroll, OverflowXScroll, OverflowYScroll })]
	[TestCase(ScrollBars.None, false, HorizontalAlignment.Right, OverflowNone, new[] { OverflowScroll, OverflowXScroll, OverflowYScroll })]
	[TestCase(ScrollBars.Horizontal, true, HorizontalAlignment.Left, null, new[] { OverflowNone, OverflowScroll, OverflowXScroll, OverflowYScroll })]
	[TestCase(ScrollBars.Horizontal, true, HorizontalAlignment.Center, null, new[] { OverflowNone, OverflowScroll, OverflowXScroll, OverflowYScroll })]
	[TestCase(ScrollBars.Horizontal, true, HorizontalAlignment.Right, null, new[] { OverflowNone, OverflowScroll, OverflowXScroll, OverflowYScroll })]
	[TestCase(ScrollBars.Horizontal, false, HorizontalAlignment.Left, OverflowXScroll, new[] { OverflowNone, OverflowScroll, OverflowYScroll })]
	[TestCase(ScrollBars.Horizontal, false, HorizontalAlignment.Center, null, new[] { OverflowNone, OverflowScroll, OverflowXScroll, OverflowYScroll })]
	[TestCase(ScrollBars.Horizontal, false, HorizontalAlignment.Right, null, new[] { OverflowNone, OverflowScroll, OverflowXScroll, OverflowYScroll })]
	[TestCase(ScrollBars.Vertical, true, HorizontalAlignment.Left, OverflowYScroll, new[] { OverflowNone, OverflowScroll, OverflowXScroll })]
	[TestCase(ScrollBars.Vertical, true, HorizontalAlignment.Center, OverflowYScroll, new[] { OverflowNone, OverflowScroll, OverflowXScroll })]
	[TestCase(ScrollBars.Vertical, true, HorizontalAlignment.Right, OverflowYScroll, new[] { OverflowNone, OverflowScroll, OverflowXScroll })]
	[TestCase(ScrollBars.Vertical, false, HorizontalAlignment.Left, OverflowYScroll, new[] { OverflowNone, OverflowScroll, OverflowXScroll })]
	[TestCase(ScrollBars.Vertical, false, HorizontalAlignment.Center, OverflowYScroll, new[] { OverflowNone, OverflowScroll, OverflowXScroll })]
	[TestCase(ScrollBars.Vertical, false, HorizontalAlignment.Right, OverflowYScroll, new[] { OverflowNone, OverflowScroll, OverflowXScroll })]
	[TestCase(ScrollBars.Both, true, HorizontalAlignment.Left, OverflowYScroll, new[] { OverflowNone, OverflowScroll, OverflowXScroll })]
	[TestCase(ScrollBars.Both, true, HorizontalAlignment.Center, OverflowYScroll, new[] { OverflowNone, OverflowScroll, OverflowXScroll })]
	[TestCase(ScrollBars.Both, true, HorizontalAlignment.Right, OverflowYScroll, new[] { OverflowNone, OverflowScroll, OverflowXScroll })]
	[TestCase(ScrollBars.Both, false, HorizontalAlignment.Left, OverflowScroll, new[] { OverflowNone, OverflowXScroll, OverflowYScroll })]
	[TestCase(ScrollBars.Both, false, HorizontalAlignment.Center, OverflowYScroll, new[] { OverflowNone, OverflowScroll, OverflowXScroll })]
	[TestCase(ScrollBars.Both, false, HorizontalAlignment.Right, OverflowYScroll, new[] { OverflowNone, OverflowScroll, OverflowXScroll })]
	public async Task MultiLineTextBoxScrollBar(ScrollBars scrollBars, bool wordWrap, HorizontalAlignment textAlign, string expectedScrollBar, IEnumerable<string> unexpectedScrollBars)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new TextBox()
		{
			Multiline = true,
			WordWrap = wordWrap,
			TextAlign = textAlign,
			ScrollBars = scrollBars
		});

		var textbox = rendered.Find(".textbox");

		var cssStyle = textbox.GetAttribute("style");
		if (expectedScrollBar != null)
		{
			Assert.That(cssStyle, Does.Contain(expectedScrollBar));
		}
		foreach (var unexpectedScrollBar in unexpectedScrollBars)
		{
			Assert.That(cssStyle, Does.Not.Contain(unexpectedScrollBar));
		}

		var cssClass = textbox.GetAttribute("class");
		if (scrollBars == ScrollBars.None)
		{
			Assert.That(cssClass, Does.Match("textbox--scrollbar(?=\\s|$)"));
		}
		else
		{
			Assert.That(cssClass, Does.Not.Match("textbox--scrollbar(?=\\s|$)"));
		}
	}

	[TestCase(true)]
	[TestCase(false)]
	public async Task TextboxSpellCheckDisabled(bool multilineStatus)
	{
		using var ctx = new WinzorTestContext();
		var testComponent = await ctx.RenderControlOnFormAsync(() => new TextBox() { Multiline = multilineStatus });
		var testInput = testComponent.Find(".textbox");

		Assert.That(testInput.GetAttribute("spellcheck"), Is.EqualTo("false"));
	}

	[TestCase(BorderStyle.None, 0)]
	[TestCase(BorderStyle.FixedSingle, 1)]
	[TestCase(BorderStyle.Fixed3D, 2)]

	public async Task TestBorderStyleAdjustForClientSize(BorderStyle bs, int borderSize)
	{
		using var ctx = new WinzorTestContext();
		TextBox targetTextBox = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Size = new Size(500, 500);
			targetTextBox = new TextBox();
			form.Controls.Add(targetTextBox);
			targetTextBox.Location = new Point(10, 10);
			targetTextBox.Size = new Size(200, 50);

			targetTextBox.BorderStyle = bs;

			return form;
		});

		var b = targetTextBox.Bounds;
		Assert.That(b.X, Is.EqualTo(10));
		Assert.That(b.Y, Is.EqualTo(10));
		Assert.That(targetTextBox.ClientSize.Width + 2 * borderSize, Is.EqualTo(b.Width));
		Assert.That(targetTextBox.ClientSize.Height + 2 * borderSize, Is.EqualTo(b.Height));
		Assert.That(targetTextBox.ClientAreaBounds.X, Is.EqualTo(b.X + borderSize));
		Assert.That(targetTextBox.ClientAreaBounds.Y, Is.EqualTo(b.Y + borderSize));
	}

	[TestCase(ScrollBars.None)]
	[TestCase(ScrollBars.Horizontal)]
	[TestCase(ScrollBars.Vertical)]
	[TestCase(ScrollBars.Both)]
	public async Task TestScrollBarAdjustForClientSizeAsync(ScrollBars scrollBars)
	{
		using var ctx = new WinzorTestContext();
		TextBox targetTextBox = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Size = new Size(500, 500);

			targetTextBox = new TextBox();
			targetTextBox.Multiline = true;
			targetTextBox.Size = new Size(200, 150);
			targetTextBox.ScrollBars = scrollBars;
			targetTextBox.BorderStyle = BorderStyle.FixedSingle;

			form.Controls.Add(targetTextBox);
			return form;
		});

		var bounds = targetTextBox.Bounds;
		var widthDiff = scrollBars == ScrollBars.Vertical || scrollBars == ScrollBars.Both ? SystemInformation.VerticalScrollBarWidth : 0;
		var heightDiff = scrollBars == ScrollBars.Horizontal || scrollBars == ScrollBars.Both ? SystemInformation.HorizontalScrollBarHeight : 0;
		widthDiff += 2;
		heightDiff += 2;
		Assert.That(targetTextBox.ClientSize.Width, Is.EqualTo(bounds.Width - widthDiff));
		Assert.That(targetTextBox.ClientSize.Height, Is.EqualTo(bounds.Height - heightDiff));
	}

	[TestCase(BorderStyle.None, BorderStyle.FixedSingle, true, 2)]
	[TestCase(BorderStyle.None, BorderStyle.Fixed3D, true, 4)]
	[TestCase(BorderStyle.None, BorderStyle.None, false, 0)]
	public async Task BorderStyleChangeTriggersRecalculation(BorderStyle initial, BorderStyle bs, bool expectChange, int expectedDiff)
	{
		using var ctx = new WinzorTestContext();
		TextBox targetTextBox = null;

		bool clientSizeChanged = false;
		int clientSizeDiff = 0;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Size = new Size(500, 500);
			targetTextBox = new TextBox();
			form.Controls.Add(targetTextBox);
			targetTextBox.Location = new Point(10, 10);
			targetTextBox.Size = new Size(200, 50);

			targetTextBox.BorderStyle = BorderStyle.None;

			var clientSizeBefore = targetTextBox.ClientSize;
			targetTextBox.ClientSizeChanged += (sender, e) =>
			{
				clientSizeChanged = true;
				clientSizeDiff = clientSizeBefore.Width - targetTextBox.ClientSize.Width;
			};

			return form;
		});

		clientSizeChanged = false;

		await targetTextBox.InvokeWinzorDispatcherAsync(() =>
		{
			targetTextBox.BorderStyle = bs;
		});

		Assert.That(clientSizeChanged, Is.EqualTo(expectChange));
		Assert.That(clientSizeDiff, Is.EqualTo(expectedDiff));
	}

	[Test, WithPlaywrightPage]
	public async Task TextBoxTextDoesNotDisappearWhenTyping()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var textBox = new TextBox()
			{
				BorderStyle = BorderStyle.None
			};
			var form = new Form();
			form.Controls.Add(textBox);
			return form;
		});

		var inputText = "The quick brown fox jumps over the lazy dog.";
		await page.Locator(".textbox").PressSequentiallyAsync(inputText);

		var finalText = await page.InputValueAsync(".textbox");
		Assert.That(finalText, Is.EqualTo(inputText));
	}

	[Test, WithPlaywrightPage]
	public async Task MultiLineTextBoxUpdateTextAfterHittingEnter()
	{
		await using var ctx = new InMemoryTestServerContext();
		TextBox textBox = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			textBox = new TextBox()
			{
				BorderStyle = BorderStyle.None,
				Multiline = true,
				Size = new Size(100, 100)
			};
			var form = new Form();
			form.Controls.Add(textBox);
			return form;
		});

		var textBoxInput = await page.WaitForSelectorAsync(".textbox");
		var inputText = "The quick brown fox jumps over the lazy dog.";
		await textBoxInput.EvaluateAsync($"a => a.value = \"{inputText}\"");
		await textBoxInput.PressAsync("Enter");

		Assert.That(() => textBox.Text, Is.EqualTo(inputText + "\r\n").After(2000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TextBoxDoesNotChangeZIndexWhenFocused()
	{
		const int originalZIndex = 1;
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var panel = new Panel();
			panel.Controls.Add(new TextBox() { ZIndex = originalZIndex });
			panel.Controls.Add(new TextBox() { ZIndex = originalZIndex });
			form.Controls.Add(panel);
			return form;
		});

		var textBoxInput = await page.WaitForSelectorAsync(".textbox:nth-of-type(2)");
		Assert.That(await textBoxInput.EvaluateAsync<int>("e => window.getComputedStyle(e).getPropertyValue('z-index')"), Is.EqualTo(originalZIndex));

		await textBoxInput.FocusAsync();

		Assert.That(await textBoxInput.EvaluateAsync<int>("e => window.getComputedStyle(e).getPropertyValue('z-index')"), Is.EqualTo(originalZIndex));
	}

	[Test, WithPlaywrightPage]
	public async Task TextBoxAllowDrop()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var panel = new Panel();
			panel.Controls.Add(new TextBox() { AllowDrop = true });
			panel.Controls.Add(new TextBox() { AllowDrop = false });
			form.Controls.Add(panel);
			return form;
		});

		var textbox1 = await page.WaitForSelectorAsync(".textbox:nth-child(1)");
		Assert.That(textbox1, Is.Not.Null);
		Assert.That(await textbox1.GetAttributeAsync("ondragstart"), Is.EqualTo("return true"));

		var textbox2 = await page.WaitForSelectorAsync(".textbox:nth-child(2)");
		Assert.That(textbox2, Is.Not.Null);
		Assert.That(await textbox2.GetAttributeAsync("ondragstart"), Is.EqualTo("return false"));
	}

	[Test, WithPlaywrightPage]
	public async Task TriggerKeyPressForEnterAndEscape()
	{
		var isKeyPressTriggeredForEnter = false;
		var isKeyPressTriggeredForEscape = false;

		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var textBox = new TextBox();

			textBox.KeyPress += (sender, e) =>
			{
				if (e.KeyChar == (char)13)
				{
					isKeyPressTriggeredForEnter = true;
				}
				else if (e.KeyChar == (char)27)
				{
					isKeyPressTriggeredForEscape = true;
				}
			};
			form.Controls.Add(textBox);

			return form;
		});

		var textBoxInput = await page.WaitForSelectorAsync("input");
		Assert.That(isKeyPressTriggeredForEnter, Is.EqualTo(false));
		Assert.That(isKeyPressTriggeredForEscape, Is.EqualTo(false));

		await textBoxInput.PressAsync("Enter");
		await textBoxInput.PressAsync("Escape");

		Assert.That(() => isKeyPressTriggeredForEnter, Is.EqualTo(true).After(1000, 100));
		Assert.That(() => isKeyPressTriggeredForEscape, Is.EqualTo(true).After(1000, 100));
	}

	[Test]
	public async Task MultilineTextBoxRegistersOnMouseDownEvent()
	{
		using var ctx = new WinzorTestContext();
		var menuDisplayer = new TestMenuDisplayer();
		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: menuDisplayer);
		TextBox textarea = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			textarea = new TextBox();
			textarea.Multiline = true;
			textarea.ContextMenuStrip = new ContextMenuStrip();
			form.Controls.Add(textarea);
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);

		Assert.DoesNotThrowAsync(async () => await rendered.Find("textarea").MouseDownAsync(new WebMouseEventArgs { Button = 2, Type = "mousedown" }));
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(0));
	}

	[Test, WithPlaywrightPage]
	public async Task PasswordTextBoxWhenCapsLockThenCapsLockIsOnTooltipDisplayed()
	{
		await using var ctx = new InMemoryTestServerContext();
		TextBox textBox = null;
		TextBox textBoxPassword = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			textBox = new TextBox();
			textBoxPassword = new TextBox
			{
				PasswordChar = '●'
			};
			var form = new Form();
			var panel = new Panel();
			panel.Controls.Add(textBox);
			panel.Controls.Add(textBoxPassword);
			form.Controls.Add(panel);
			return form;
		});

		Keyboard.IsCapsLockOn = true;

		Assert.That(Keyboard.IsKeyToggled(Input.Key.CapsLock), Is.True);
		await page.WaitForSelectorAsync(".form");
	}

	[Test, WithPlaywrightPage]
	public async Task PasswordTextBoxWhenCtrlCPressedThenCopyNotAllowedTooltipDisplayed()
	{
		await using var ctx = new InMemoryTestServerContext();

		TextBox textBox = null;
		TextBox textBoxPassword = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			textBox = new TextBox();
			textBoxPassword = new TextBox
			{
				PasswordChar = '●'
			};
			var form = new Form();
			var panel = new Panel();
			panel.Controls.Add(textBox);
			panel.Controls.Add(textBoxPassword);
			form.Controls.Add(panel);
			return form;
		});

		var textBoxInput = page.Locator("input[type='text']");
		await textBoxInput.PressSequentiallyAsync("MyText");
		await textBoxInput.PressAsync("Control+C");
		await page.WaitForSelectorAsync(".form > .popup", new PageWaitForSelectorOptions { State = WaitForSelectorState.Detached, Timeout = 3000 });

		var textBoxPasswordInput = page.Locator("input[type='password']");
		await textBoxPasswordInput.PressSequentiallyAsync("MyPassword");
		await textBoxPasswordInput.PressAsync("Control+C");
		await page.WaitForSelectorAsync(".form > .popup", new PageWaitForSelectorOptions { State = WaitForSelectorState.Attached, Timeout = 3000 });
	}

	[Test]
	public async Task PasswordTextBoxValueShouldNotBeVisibleInMarkup()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var textBox = new TextBox();
			textBox.PasswordChar = '●';
			textBox.Text = "this-is-a-password";
			return textBox;
		});

		Assert.That(rendered.Markup, Does.Not.Contain("this-is-a-password"));
	}

	[Test]
	public async Task PasswordTextBoxValueShouldBeMaskedAsPlaceholder()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var textBox = new TextBox();
			textBox.PasswordChar = '●';
			textBox.Text = "this-is-a-password";
			return textBox;
		});

		Assert.That(rendered.Markup, Does.Not.Contain("this-is-a-password"));
		Assert.That(rendered.Find("input").GetAttribute("placeholder"), Is.EqualTo("●●●●●●●●●●●●●●●●●●"));
	}

	[Test]
	public async Task PasswordTextBoxValueShouldBeClearedWhenSettingPasswordChar()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var textBox = new TextBox();
			textBox.Text = "this-is-a-password";
			textBox.PasswordChar = '●';
			return textBox;
		});

		Assert.That(rendered.Markup, Does.Not.Contain("this-is-a-password"));
		Assert.That(rendered.Find("input").GetAttribute("value"), Is.EqualTo(string.Empty));
	}

	[Test]
	public async Task PasswordTextBoxValueShouldBeClearedWhenSettingNewValueFromServer()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var textBox = new TextBox();
			textBox.PasswordChar = '●';
			textBox.Text = "this-is-a-password";
			return textBox;
		});
		var textBox = rendered.GetControl<TextBox>();

		Assert.That(rendered.Markup, Does.Not.Contain("this-is-a-password"));
		Assert.That(rendered.Find("input").GetAttribute("value"), Is.EqualTo(string.Empty));

		await rendered.Find("input").InputAsync(new ChangeEventArgs { Value = "my-new-password" });

		Assert.That(rendered.Find("input").GetAttribute("value"), Is.EqualTo("my-new-password"));
		Assert.That(textBox.Text, Is.EqualTo("my-new-password"));

		await textBox.InvokeWinzorDispatcherAsync(() => textBox.Text = "new-server-password");
		Assert.That(rendered.Find("input").GetAttribute("value"), Is.EqualTo(string.Empty));
		Assert.That(rendered.Find("input").GetAttribute("placeholder"), Is.EqualTo("●●●●●●●●●●●●●●●●●●●"));
	}

	[Test]
	public async Task PasswordTextBoxSetSameValueShouldNotForceUpdateOnClient()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var textBox = new TextBox();
			textBox.PasswordChar = '●';
			textBox.Text = "this-is-a-password";
			return textBox;
		});
		var textBox = rendered.GetControl<TextBox>();

		Assert.That(rendered.Markup, Does.Not.Contain("this-is-a-password"));
		Assert.That(rendered.Find("input").GetAttribute("value"), Is.EqualTo(string.Empty));

		await textBox.InvokeWinzorDispatcherAsync(() => textBox.Text = "this-is-a-password");
		Assert.That(ctx.JSInterop.Invocations.Where(i => i.Identifier == TextBoxJSInterop.SetTextContentIdentifier), Is.Empty);
	}

	[Test]
	public async Task UpdateTextAndVerifyValue()
	{
		using var ctx = new WinzorTestContext();
		TextBox textBox = null;

		var rendered = await ctx.RenderControlOnFormAsync(() => textBox = new TextBox() { Text = "new content" });

		var textBoxElement = rendered.Find(".textbox");

		Assert.That(textBox.Text, Is.EqualTo("new content"));
		Assert.That(textBoxElement.GetAttribute("value"), Is.EqualTo("new content"));
	}

	[Test]
	public async Task TextBoxCopyInvokesClipboardInterop()
	{
		using var ctx = new WinzorTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() => new TextBox() { Text = "Hello World" });
		var textBox = rendered.GetControl<TextBox>();

		await textBox.InvokeWinzorDispatcherAsync(() => textBox.Copy());
		var interopInvocations = ctx.JSInterop.Invocations.Where(i => i.Identifier == "clipboard.copy");

		Assert.That(interopInvocations.Count, Is.EqualTo(1));
		Assert.That(interopInvocations.Single().Arguments.First(), Is.EqualTo(textBox.ElementReference));
	}

	[Test]
	public async Task TextBoxCutInvokesClipboardInterop()
	{
		using var ctx = new WinzorTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() => new TextBox() { Text = "Hello World" });
		var textBox = rendered.GetControl<TextBox>();

		await textBox.InvokeWinzorDispatcherAsync(() => textBox.Cut());
		var interopInvocations = ctx.JSInterop.Invocations.Where(i => i.Identifier == "clipboard.cut");

		Assert.That(interopInvocations.Count, Is.EqualTo(1));
		Assert.That(interopInvocations.Single().Arguments.First(), Is.EqualTo(textBox.ElementReference));
	}

	[Test]
	public async Task TextBoxPasteInvokesClipboardInterop()
	{
		using var ctx = new WinzorTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() => new TextBox() { Text = "Hello World" });
		var textBox = rendered.GetControl<TextBox>();

		await textBox.InvokeWinzorDispatcherAsync(() => textBox.Paste());
		var interopInvocations = ctx.JSInterop.Invocations.Where(i => i.Identifier == "clipboard.paste");

		Assert.That(interopInvocations.Count, Is.EqualTo(1));
		Assert.That(interopInvocations.Single().Arguments.First(), Is.EqualTo(textBox.ElementReference));
	}

	[Test, WithPlaywrightPage]
	public async Task TextBoxPasswordCannotBeCopied()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var textBox = new TextBox() { Text = "This is a password!", PasswordChar = '●' };
			var button = new Button() { Text = "Copy Text" };
			button.Click += (s, e) => textBox.Copy();

			return new Form() { Controls = { textBox, button } };
		});

		await page.EvaluateAsync(@"clipboardValue = 'initialvalue';
navigator.clipboard.writeText = (data) => clipboardValue = data;");

		await (await page.WaitForSelectorAsync("input")).SelectTextAsync();
		await (await page.WaitForSelectorAsync("button")).ClickAsync();

		Assert.That(async () => await page.EvaluateAsync<string>("window.clipboardValue"), Is.Not.EqualTo("initialvalue").After(1000, 100));
		Assert.That(async () => await page.EvaluateAsync<string>("window.clipboardValue"), Is.Not.EqualTo("This is a password!").After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TextBoxPasswordCannotBeCut()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var textBox = new TextBox() { Text = "This is a password!", PasswordChar = '●' };
			var button = new Button() { Text = "Cut Text" };
			button.Click += (s, e) => textBox.Cut();

			return new Form() { Controls = { textBox, button } };
		});

		await page.EvaluateAsync(@"clipboardValue = 'initialvalue';
navigator.clipboard.writeText = (data) => clipboardValue = data;");

		await (await page.WaitForSelectorAsync("input")).SelectTextAsync();
		await (await page.WaitForSelectorAsync("button")).ClickAsync();

		Assert.That(async () => await page.EvaluateAsync<string>("window.clipboardValue"), Is.Not.EqualTo("initialvalue").After(1000, 100));
		Assert.That(async () => await page.EvaluateAsync<string>("window.clipboardValue"), Is.Not.EqualTo("This is a password!").After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task SelectionShouldadaptDifferentNewLineChars()
	{
		await using var ctx = new InMemoryTestServerContext();
		var secondLineFirstPosition = 7;
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var textBox = new TextBox() { Text = "Hello\r\nWorld!", Multiline = true };
			textBox.Select(secondLineFirstPosition, 2);
			return textBox;
		});

		var textBoxLocator = page.Locator("textarea");
		await textBoxLocator.WaitForAsync();

		Assert.That(async () => await textBoxLocator.EvaluateAsync<string>("() => window.getSelection().toString()"), Is.EqualTo("Wo").After(2000, 200));
	}

	[Test, WithPlaywrightPage]
	public async Task MultiLineTextBoxSelectAll()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var textBox = new TextBox() { Text = "Hello\r\nWorld!", Multiline = true };
			return textBox;
		});
		await page.Locator("textarea").ClickAsync();
		await page.Keyboard.PressAsync("Control+A");
		Assert.That(async () => await page.EvaluateAsync<string>("e => window.getSelection().toString()"), Is.EqualTo("Hello\nWorld!"));
	}

	[Test, WithPlaywrightPage]
	public async Task MultiLineTextBoxUpdateTextIsNotFlashy()
	{
		await using var ctx = new InMemoryTestServerContext();
		TextBox textBox = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			textBox = new TextBox()
			{
				BorderStyle = BorderStyle.None,
				Multiline = true,
				Size = new Size(100, 100)
			};
			var form = new Form();
			form.Controls.Add(textBox);
			return form;
		});

		Assert.That(textBox.Text, Is.Empty);

		var inputText = @"The quick brown
		kASJDAJKSDBJKASDJKASBDJKBASDJKBASKJD
		ASKJDBAJKSDBAJKSBDBASDKJBASKDJBAJKSBDKJASBDKJABSD";
		await page.Locator(".textbox").PressSequentiallyAsync(inputText, new () { Delay = 200 });

		Assert.That(() => textBox.Text.Replace("\r\n\r\n", "\r\n"), Is.EqualTo(inputText));
	}

	[Test, WithPlaywrightPage]
	public async Task AutoSelectAllTextOnGotFocus()
	{
		await using var ctx = new InMemoryTestServerContext();
		TextBox textBox1 = null;
		TextBox textBox2 = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			textBox1 = new TextBox() { Text = "textBox1" };
			textBox2 = new TextBox() { Text = "textBox2", Top = 30 };
			form.Controls.Add(textBox1);
			form.Controls.Add(textBox2);
			return form;
		});

		var textBoxInput1 = await page.Locator(".textbox").First.ElementHandleAsync();
		AssertTextAndSelectionOnClient(textBoxInput1, textBox1.Text, 0, textBox1.Text.Length);
		AssertTextAndSelectionOnServer(textBox1, textBox1.Text, 0, textBox1.Text.Length, textBox1.Text);

		// focus initiated by mouse click will NOT select all text
		var textBoxInput2 = await page.Locator(".textbox").Last.ElementHandleAsync();
		await textBoxInput2.ClickAsync();
		AssertTextAndSelectionOnClient(textBoxInput2, textBox2.Text, textBox2.Text.Length, textBox2.Text.Length);
		AssertTextAndSelectionOnServer(textBox2, textBox2.Text, textBox2.Text.Length, 0, string.Empty);
	}

	[Test]
	public async Task TextBoxUseSystemCharShouldUpdatePasswordChar()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new TextBox());
		var textBox = rendered.GetControl<TextBox>();

		Assert.That(textBox.PasswordChar, Is.EqualTo('\0'));

		await textBox.InvokeWinzorDispatcherAsync(() => textBox.UseSystemPasswordChar = true);
		Assert.That(textBox.PasswordChar, Is.EqualTo('●'));

		await textBox.InvokeWinzorDispatcherAsync(() => textBox.UseSystemPasswordChar = false);
		Assert.That(textBox.PasswordChar, Is.EqualTo('\0'));
	}

	[TestCase(Keys.Return, true, true, true)]
	[TestCase(Keys.Return, true, false)]
	[TestCase(Keys.Return, false, true)]
	[TestCase(Keys.Return, false, false)]
	[TestCase(Keys.A, true, true)]
	[TestCase(Keys.A, true, false)]
	[TestCase(Keys.A, false, true)]
	[TestCase(Keys.A, false, false)]
	[TestCase(Keys.Space, true, true)]
	[TestCase(Keys.Space, true, false)]
	[TestCase(Keys.Space, false, true)]
	[TestCase(Keys.Space, false, false)]
	[TestCase(Keys.Left, true, true, true)]
	[TestCase(Keys.Left, true, false, true)]
	[TestCase(Keys.Left, false, true, true)]
	[TestCase(Keys.Left, false, false, true)]
	[TestCase(Keys.Right, true, true, true)]
	[TestCase(Keys.Right, true, false, true)]
	[TestCase(Keys.Right, false, true, true)]
	[TestCase(Keys.Right, false, false, true)]
	[TestCase(Keys.Up, true, true, true)]
	[TestCase(Keys.Up, true, false, true)]
	[TestCase(Keys.Up, false, true, true)]
	[TestCase(Keys.Up, false, false, true)]
	[TestCase(Keys.Down, true, true, true)]
	[TestCase(Keys.Down, true, false, true)]
	[TestCase(Keys.Down, false, true, true)]
	[TestCase(Keys.Down, false, false, true)]
	[TestCase(Keys.Alt | Keys.Up, false, false)]
	[TestCase(Keys.Alt | Keys.Up, false, true)]
	[TestCase(Keys.Alt | Keys.Up, true, false)]
	[TestCase(Keys.Alt | Keys.Up, true, true)]

	public async Task TextBoxReturnsCorrectValueForIsInputKey(Keys keys, bool multiline, bool acceptsReturn, bool expectedResult = false)
	{
		TextBoxForTest textBox = null;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => textBox = new TextBoxForTest() { Multiline = multiline, AcceptsReturn = acceptsReturn });

		Assert.That(textBox.IsInputKey(keys), Is.EqualTo(expectedResult));
	}

	[TestCase("Enter", true, true)]
	[TestCase("Enter", true, false, true)]
	[TestCase("Enter", false, true, true)]
	[TestCase("Enter", false, false, true)]
	[TestCase("A", true, true)]
	[TestCase("A", true, false)]
	[TestCase("A", false, true)]
	[TestCase("A", false, false)]
	[TestCase("Space", true, true)]
	[TestCase("Space", true, false)]
	[TestCase("Space", false, true)]
	[TestCase("Space", false, false)]
	[WithPlaywrightPage]
	public async Task HittingKeysOnTextBoxDoesNotIncorrectlyActivateAcceptButton(string key, bool multiline, bool acceptsReturn, bool shouldFire = false)
	{
		TextBoxForTest textBox = null;
		var buttonClickFired = new TaskCompletionSource();

		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(textBox = new TextBoxForTest() { Multiline = multiline, AcceptsReturn = acceptsReturn, Height = 200 });

			var button = new Button() { Top = 300 };
			form.Controls.Add(button);
			form.AcceptButton = button;
			button.Click += (s, e) => buttonClickFired.SetResult();

			return form;
		});

		var input = await page.WaitForSelectorAsync(".textbox");
		await input.PressAsync(key);

		await buttonClickFired.Task.WithTimeout(TimeSpan.FromSeconds(3));
		Assert.That(() => buttonClickFired.Task.IsCompleted, Is.EqualTo(shouldFire).After(5000, 100), "Has the button click fired?");
	}

	[Test, WithPlaywrightPage]
	public async Task ArrowKeyInTextBoxDoesNotShiftFocous()
	{
		await using var ctx = new InMemoryTestServerContext();
		TextBox textBox1 = null;
		TextBox textBox2 = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			textBox1 = new TextBox() { Text = "1" };
			textBox2 = new TextBox() { Text = "2", Top = 30 };
			form.Controls.Add(textBox1);
			form.Controls.Add(textBox2);
			return form;
		});

		Assert.That(textBox1.Focused, Is.True);
		Assert.That(textBox2.Focused, Is.False);

		var textBoxInput2 = page.Locator(".textbox").Last;
		await textBoxInput2.ClickAsync();
		Assert.That(() => textBox2.selectionStart, Is.EqualTo(1).After(1000, 100));

		await textBoxInput2.PressAsync("ArrowLeft");
		Assert.That(() => textBox2.selectionStart, Is.EqualTo(0).After(1000, 100));
		Assert.That(textBox2.Focused, Is.True);
		Assert.That(textBox1.Focused, Is.False);
	}

	void AssertTextAndSelectionOnClient(IElementHandle element, string expectedText, int expectedSelectionStart, int expectedSelectionEnd)
	{
		Assert.That(async () => await element.InputValueAsync(), Is.EqualTo(expectedText).After(3000, 100));
		Assert.That(async () => await element.EvaluateAsync<int>("i => i.selectionStart"), Is.EqualTo(expectedSelectionStart).After(3000, 100));
		Assert.That(async () => await element.EvaluateAsync<int>("i => i.selectionEnd"), Is.EqualTo(expectedSelectionEnd).After(3000, 100));
	}

	void AssertTextAndSelectionOnServer(TextBox textBox, string expectedText, int expectedSelectionStart, int expectedSelectionLength, string expectedSelectedText)
	{
		Assert.That(() => textBox.Text, Is.EqualTo(expectedText).After(3000, 100));
		Assert.That(() => textBox.SelectionStart, Is.EqualTo(expectedSelectionStart).After(3000, 100));
		Assert.That(() => textBox.SelectionLength, Is.EqualTo(expectedSelectionLength).After(3000, 100));
		Assert.That(() => textBox.SelectedText, Is.EqualTo(expectedSelectedText).After(3000, 100));
	}

	[TestCase(true, false)]
	[TestCase(false, true)]
	[WithPlaywrightPage]
	public async Task PasswordTextBoxWhenCapsLockThenTooltipDisplayedBasedOnWindowSize(bool isLargeViewPort, bool isClassPresent)
	{
		await using var ctx = new InMemoryTestServerContext();

		var url = default(Uri);
		TextBox textBoxPassword = null;

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			textBoxPassword = new TextBox
			{
				PasswordChar = '●'
			};
			var form = new Form() { Height = 100, Width = 100 };
			var panel = new Panel();
			panel.Controls.Add(textBoxPassword);
			form.Controls.Add(panel);
			url = ctx.WinzorDispatcher.FormInstanceRegister.Add(new Uri("http://localhost:5000"), form);
		});
		var response = await Page.GotoAsync(url.ToString());
		Assert.That(response.Ok);

		if (isLargeViewPort)
		{
			await Page.SetViewportSizeAsync(800, 800);
		}
		else
		{
			await Page.SetViewportSizeAsync(100, 100);
		}
		Keyboard.IsCapsLockOn = true;
		Assert.That(Keyboard.IsKeyToggled(Input.Key.CapsLock), Is.True);

		var tooltip = Page.Locator(".form > .popup > .balloon-tooltip");
		var isTooltipCompact = await tooltip.EvaluateAsync<bool>("ele => ele.classList.contains('balloon-tooltip--compact')");

		Assert.That(isTooltipCompact, Is.EqualTo(isClassPresent));

		await Page.CloseAsync();
	}

	[Test, WithPlaywrightPage]
	public async Task DeleteAndBackspaceFiresSelectionChange()
	{
		await using var ctx = new InMemoryTestServerContext();
		TextBox textBox = null;
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			textBox = new TextBox()
			{
				Text = "Test",
				SelectionStart = 2,
				SelectionLength = 0
			};
			return textBox;
		});
		var textBoxLocator = page.Locator("input");
		var textBoxHandle = await textBoxLocator.ElementHandleAsync();
		AssertTextAndSelectionOnClient(textBoxHandle, "Test", 2, 2);
		AssertTextAndSelectionOnServer(textBox, "Test", 2, 0, string.Empty);

		await textBoxLocator.PressAsync("Backspace");
		AssertTextAndSelectionOnClient(textBoxHandle, "Tst", 1, 1);
		AssertTextAndSelectionOnServer(textBox, "Tst", 1, 0, string.Empty);

		await textBoxLocator.SelectTextAsync();
		await textBoxLocator.PressAsync("Delete");
		AssertTextAndSelectionOnClient(textBoxHandle, string.Empty, 0, 0);
		AssertTextAndSelectionOnServer(textBox, string.Empty, 0, 0, string.Empty);
	}

	[Test, WithPlaywrightPage]
	public async Task ScrollToCaret()
	{
		var textBox = default(TextBox);
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => textBox = new TextBox()
		{
			Width = 100,
			Text = "123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789"
		});
		var input = page.Locator("input");
		await input.EvaluateAsync("e => e.scrollLeft = 0");
		await textBox.InvokeWinzorDispatcherAsync(() => textBox.Select(0, 0));
		Assert.That(() => input.EvaluateAsync<int>("e => e.scrollTop"), Is.EqualTo(0).After(1000, 100));
		Assert.That(() => input.EvaluateAsync<int>("e => e.selectionStart"), Is.EqualTo(0).After(1000, 100));
		Assert.That(() => input.EvaluateAsync<int>("e => e.selectionEnd"), Is.EqualTo(0).After(1000, 100));

		await textBox.InvokeWinzorDispatcherAsync(() =>
		{
			textBox.SelectionStart = textBox.Text.Length;
			textBox.ScrollToCaret();
		});
		Assert.That(() => input.EvaluateAsync<int>("e => e.scrollLeft"), Is.EqualTo(460).Within(2).After(1000, 100));

		await textBox.InvokeWinzorDispatcherAsync(() =>
		{
			textBox.SelectionStart = textBox.Text.Length / 2;
			textBox.ScrollToCaret();
		});
		Assert.That(() => input.EvaluateAsync<int>("e => e.scrollLeft"), Is.EqualTo(251).Within(2).After(1000, 100));

		await textBox.InvokeWinzorDispatcherAsync(() =>
		{
			textBox.SelectionStart = 0;
			textBox.ScrollToCaret();
		});
		Assert.That(() => input.EvaluateAsync<int>("e => e.scrollLeft"), Is.EqualTo(0).After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task ScrollToCaret_Multiline()
	{
		var textBox = default(TextBox);
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => textBox = new TextBox()
		{
			Width = 100,
			Height = 50,
			Multiline = true,
			Text = "123456789\n123456789\n123456789\n123456789\n123456789\n123456789\n123456789\n123456789\n123456789\n123456789\n123456789\n123456789\n123456789\n123456789\n123456789\n123456789\n123456789\n123456789\n123456789\n123456789\n"
		});
		var textarea = page.Locator("textarea");
		await textarea.EvaluateAsync("e => e.scrollTop = 0");
		await textBox.InvokeWinzorDispatcherAsync(() => textBox.Select(0, 0));
		Assert.That(() => textarea.EvaluateAsync<int>("e => e.scrollTop"), Is.EqualTo(0).After(1000, 100));
		Assert.That(() => textarea.EvaluateAsync<int>("e => e.selectionStart"), Is.EqualTo(0).After(1000, 100));
		Assert.That(() => textarea.EvaluateAsync<int>("e => e.selectionEnd"), Is.EqualTo(0).After(1000, 100));

		await textBox.InvokeWinzorDispatcherAsync(() =>
		{
			textBox.SelectionStart = textBox.Text.Length;
			textBox.ScrollToCaret();
		});
		Assert.That(() => textarea.EvaluateAsync<int>("e => e.scrollTop"), Is.EqualTo(229).Within(2).After(1000, 100));

		await textBox.InvokeWinzorDispatcherAsync(() =>
		{
			textBox.SelectionStart = 0;
			textBox.ScrollToCaret();
		});
		Assert.That(() => textarea.EvaluateAsync<int>("e => e.scrollTop"), Is.EqualTo(0).After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task ScrollToCaretWhenSelectedTextIsSet()
	{
		var textBox = default(TextBox);
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => textBox = new TextBox()
		{
			Width = 100,
			Text = "123456789 123456789 123456789 123456789 123456789"
		});
		var input = page.Locator("input");
		await input.EvaluateAsync("e => e.scrollLeft = 0");
		await textBox.InvokeWinzorDispatcherAsync(() => textBox.Select(0, 0));
		Assert.That(() => input.EvaluateAsync<int>("e => e.scrollTop"), Is.EqualTo(0).After(1000, 100));
		Assert.That(() => input.EvaluateAsync<int>("e => e.selectionStart"), Is.EqualTo(0).After(1000, 100));
		Assert.That(() => input.EvaluateAsync<int>("e => e.selectionEnd"), Is.EqualTo(0).After(1000, 100));

		await textBox.InvokeWinzorDispatcherAsync(() =>
		{
			textBox.SelectionStart = 30;
			textBox.SelectionLength = 9;
			textBox.SelectedText = "selected!";
		});

		Assert.That(() => input.InputValueAsync(), Is.EqualTo("123456789 123456789 123456789 selected! 123456789").After(1000, 100));
		Assert.That(() => input.EvaluateAsync<int>("e => e.scrollLeft"), Is.EqualTo(170).Within(2).After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task CannotSetTextWhenIsOnInput()
	{
		var textBox = default(TextBoxForTest);
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => textBox = new TextBoxForTest()
		{
			Width = 100,
			Text = "A"
		});

		var input = page.Locator("input");
		await textBox.InvokeWinzorDispatcherAsync(() =>
		{
			textBox.SetIsOnInput(true);
			textBox.Text = "B";
		});

		Assert.That(() => input.InputValueAsync(), Is.EqualTo("A").After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task CannotSelectWhenIsOnInput()
	{
		var textBox = default(TextBoxForTest);
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => textBox = new TextBoxForTest()
		{
			Width = 100,
			Text = "ABCD"
		});

		var input = page.Locator("input");
		await textBox.InvokeWinzorDispatcherAsync(() =>
		{
			textBox.Select(1,1);
			textBox.SetIsOnInput(true);
			textBox.SelectAll();
		});

		Assert.That(() => input.InputValueAsync(), Is.EqualTo("ABCD").After(1000, 100));
		Assert.That(() => input.EvaluateAsync<int>("e => e.selectionStart"), Is.EqualTo(1).After(1000, 100));
		Assert.That(() => input.EvaluateAsync<int>("e => e.selectionEnd"), Is.EqualTo(2).After(1000, 100));
	}

	[Test]
	public  async Task TestCharacterCasingDoesNotTriggerTextChangedWhenTextIsEmpty()
	{
		using var ctx = new WinzorTestContext();
		var textChangedCount = 0;

		var component = await ctx.RenderControlOnFormAsync(() =>
		{
			var textBox = new TextBox();
			textBox.TextChanged += (sender, e) => textChangedCount++;

			textBox.CharacterCasing = CharacterCasing.Upper;

			return textBox;
		});

		Assert.That(textChangedCount, Is.EqualTo(0));
	}

	class TextBoxForTest : TextBox
	{
		readonly Dictionary<string, Action> Callbacks = new Dictionary<string, Action>();

		internal new Task OnTextBoxSelectionChangedAsync(TextboxSelectionChangeEventArgs args) => base.OnTextBoxSelectionChangedAsync(args);
		internal new Task OnInputAsync(string value) => base.OnInputAsync(value);

		internal void SetCallback(string methodName, Action action)
		{
			Callbacks[methodName] = action;
		}

		protected internal override void OnTextBoxSelectionChanged(int newStart, int newLength)
		{
			TryInvokeAction(nameof(OnTextBoxSelectionChanged));
			base.OnTextBoxSelectionChanged(newStart, newLength);
		}

		protected internal override void OnInput(string value)
		{
			TryInvokeAction(nameof(OnInput));
			base.OnInput(value);
		}

		void TryInvokeAction(string methodName)
		{
			Callbacks.TryGetValue(methodName, out var action);
			action?.Invoke();
		}

		public new bool IsInputKey(Keys keyData) => base.IsInputKey(keyData);

		public void SetIsOnInput(bool value) => IsOnInput = value;
	}
}
