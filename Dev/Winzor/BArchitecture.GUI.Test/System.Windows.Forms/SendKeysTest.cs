using System.Threading.Tasks;
using NUnit.Framework;
using WinzorTestFramework;

namespace System.Windows.Forms;

class SendKeysTest
{
	[Test]
	public async Task SendKeys_ControlReceivesKeyDownEvent()
	{
		var eventFired = false;
		Button button = null;

		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			button = new Button();
			button.KeyDown += (s, e) =>
			{
				eventFired = true;
			};
			form.Controls.Add(button);
			return form;
		});

		SendKeys.Send("A");
		Assert.That(() => eventFired, Is.True.After(1000, 100));
	}

	[Test]
	public async Task SendKeys_ControlReceivesKeyUpEvent()
	{
		var eventFired = false;
		Button button = null;

		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			button = new Button();
			button.KeyUp += (s, e) =>
			{
				eventFired = true;
			};
			form.Controls.Add(button);
			return form;
		});

		SendKeys.Send("A");
		Assert.That(() => eventFired, Is.True.After(1000, 100));
	}

	[TestCase("{ENTER}", Keys.Enter)]
	[TestCase("{ADD}", Keys.Add)]
	[TestCase("{ESC}", Keys.Escape)]
	[TestCase("~", Keys.Enter)]
	[TestCase("3", Keys.D3)]
	[TestCase("A", Keys.A, true)]
	[TestCase("a", Keys.A)]
	[TestCase("+", Keys.ShiftKey, true, false, false)]
	[TestCase("^", Keys.ControlKey, false, true, false)]
	[TestCase("%", Keys.Menu, false, false, true)]
	[TestCase("+a", Keys.A, true, false, false)] //Shift + A
	[TestCase("^a", Keys.A, false, true, false)] //Ctrl + A
	[TestCase("%a", Keys.A, false, false, true)] //Alt + A
	public async Task SendKeys_ApplicationReceivesCorrectKeys(string input, int expectedKeyValue, bool shift = false, bool control = false, bool alt = false)
	{
		KeyEventArgs args = null;

		Form form = null;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			var textBox = new TextBox();
			textBox.KeyDown += (s, e) => { args = e; };

			form.Controls.Add(textBox);
			return form;
		});

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			SendKeys.Send(input);
		});

		Assert.Multiple(() =>
		{
			Assert.That(args, Is.Not.Null);
			Assert.That(args.KeyValue, Is.EqualTo(expectedKeyValue));
			Assert.That(args.Shift, Is.EqualTo(shift));
			Assert.That(args.Control, Is.EqualTo(control));
			Assert.That(args.Alt, Is.EqualTo(alt));
			Assert.That(args.KeyCode, Is.EqualTo((Keys)expectedKeyValue));
		});
	}

	[TestCase("{h 1}", 1)]
	[TestCase("{h 10}", 10)]
	[TestCase("abc", 3)]
	[TestCase("456sg", 5)]
	[TestCase("{H 1}", 2)]
	[TestCase("h{ESC}", 2)]
	public async Task SendKeys_SendMultipleKeyEvents(string input, int expectedEventCount)
	{
		var keyDownCount = 0;
		var keyUpCount = 0;

		Form form = null;
		TextBox textBox = null;

		using var ctx = new WinzorTestContext();
		await ctx.RenderFormAsync(() =>
		{
			textBox = new TextBox();
			textBox.KeyDown += (s, e) => { keyDownCount++; };
			textBox.KeyUp += (s, e) => { keyUpCount++; };

			form = new Form();
			form.Controls.Add(textBox);

			return form;
		});

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			SendKeys.Send(input);
		});

		Assert.That(keyDownCount, Is.EqualTo(expectedEventCount));
		Assert.That(keyUpCount, Is.EqualTo(expectedEventCount));
	}

	[TestCase("", "")]
	[TestCase("{BACKSPACE}", "")]
	[TestCase("abc", "abc")]
	[TestCase("11111", "11111")]
	[TestCase("asdf1234", "asdf1234")]
	[TestCase("asdf{LEFT}1234", "asd1234f")]
	[TestCase("asdf{RIGHT}1234", "asdf1234")]
	[TestCase("asdf{UP}1234", "asd1234f")]
	[TestCase("asdf{DOWN}1234", "asdf1234")]
	[TestCase("asdf{HOME}1234", "1234asdf")]
	[TestCase("asdf{END}1234", "asdf1234")]
	[TestCase("asdf{PGUP}1234", "asdf1234")]
	[TestCase("asdf{PGDN}1234", "asdf1234")]
	[TestCase("{DELETE}", "")]
	public async Task SendKeys_TextBox_TextShouldUpdateFromSendKeysInput(string input, string expected)
	{
		TextBox textBox = null;
		Form form = null;

		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			form.Controls.Add(textBox = new TextBox());
			return form;
		});

		textBox.Focus();

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			SendKeys.Send(input);
		});

		Assert.That(textBox.Text, Is.EqualTo(expected));
	}

	[TestCase(0, "xasdf1234")]
	[TestCase(1, "axsdf1234")]
	[TestCase(2, "asxdf1234")]
	[TestCase(3, "asdxf1234")]
	[TestCase(4, "asdfx1234")]
	[TestCase(5, "asdf1x234")]
	[TestCase(6, "asdf12x34")]
	[TestCase(7, "asdf123x4")]
	[TestCase(8, "asdf1234x")]
	public async Task SendKeys_TextBox_InsertTextAtCursorPosition(int position, string expectedText)
	{
		TextBox textBox = null;
		Form form = null;

		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			form.Controls.Add(textBox = new TextBox() { Text = "asdf1234" });
			return form;
		});

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			textBox.Focus();
			textBox.SelectionStart = position;
			textBox.SelectionLength = 0;
			SendKeys.Send("x");
		});

		Assert.That(textBox.Text, Is.EqualTo(expectedText));
	}

	[TestCase(0, "asdf1234")]
	[TestCase(1, "sdf1234")]
	[TestCase(2, "adf1234")]
	[TestCase(3, "asf1234")]
	[TestCase(4, "asd1234")]
	[TestCase(5, "asdf234")]
	[TestCase(6, "asdf134")]
	[TestCase(7, "asdf124")]
	[TestCase(8, "asdf123")]
	public async Task SendKeys_TextBox_BackspaceRemovesPriorCharacter(int position, string expectedText)
	{
		TextBox textBox = null;
		Form form = null;

		using var ctx = new WinzorTestContext();
		await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			form.Controls.Add(textBox = new TextBox() { Text = "asdf1234" });
			return form;
		});

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			textBox.Focus();
			textBox.SelectionStart = position;
			textBox.SelectionLength = 0;
			SendKeys.Send("{BACKSPACE}");
		});

		Assert.That(textBox.Text, Is.EqualTo(expectedText));
	}

	[TestCase(0, "sdf1234")]
	[TestCase(1, "adf1234")]
	[TestCase(2, "asf1234")]
	[TestCase(3, "asd1234")]
	[TestCase(4, "asdf234")]
	[TestCase(5, "asdf134")]
	[TestCase(6, "asdf124")]
	[TestCase(7, "asdf123")]
	[TestCase(8, "asdf1234")]
	public async Task SendKeys_TextBox_DeleteRemovesNextCharacter(int position, string expectedText)
	{
		TextBox textBox = null;
		Form form = null;

		using var ctx = new WinzorTestContext();
		await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			form.Controls.Add(textBox = new TextBox() { Text = "asdf1234" });
			return form;
		});

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			textBox.Focus();
			textBox.SelectionStart = position;
			textBox.SelectionLength = 0;
			SendKeys.Send("{DELETE}");
		});

		Assert.That(textBox.Text, Is.EqualTo(expectedText));
	}

	[TestCase("{LEFT}", 2)]
	[TestCase("{RIGHT}", 4)]
	public async Task SendKeys_TextBox_ArrowChangesSelectionStart(string input, int expectedPosition)
	{
		TextBox textBox = null;
		Form form = null;

		using var ctx = new WinzorTestContext();
		await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			form.Controls.Add(textBox = new TextBox() { Text = "asdf1234" });
			return form;
		});

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			textBox.Focus();
			textBox.SelectionStart = 3;
			SendKeys.Send(input);
		});

		Assert.That(textBox.SelectionStart, Is.EqualTo(expectedPosition));
	}

	[TestCase(true, TestName = "{m}WithDifferentTabIndexes")]
	[TestCase(false, TestName = "{m}WithIdenticalTabIndexes")]
	public async Task SendKeys_Tab(bool differentTabIndexes)
	{
		Form form = null;
		TextBox textBox1 = null;
		TextBox textBox2 = null;
		TextBox textBox3 = null;

		using var ctx = new WinzorTestContext();
		await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			form.Controls.Add(textBox1 = new TextBox { TabIndex = 0 });
			form.Controls.Add(textBox2 = new TextBox { TabIndex = differentTabIndexes ? 1 : 0 });
			form.Controls.Add(textBox3 = new TextBox { TabIndex = differentTabIndexes ? 2 : 0 });
			return form;
		});

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			textBox1.Focus();
			Application.DoEvents();
			Assert.That(textBox1.Focused, "Initial focus did not work.");

			SendKeys.Send("{Tab}");
			Assert.That(textBox2.Focused, $"Tab event did not focus {nameof(textBox2)}.");

			SendKeys.Send("{Tab}");
			Assert.That(textBox3.Focused, $"Tab event did not focus {nameof(textBox3)}.");

			SendKeys.Send("{Tab}");
			Assert.That(textBox1.Focused, "Tab event did not roll over to first tab.");

			SendKeys.Send("+{Tab}"); //Shift + Tab - should go back, rolling over to last tab
			Assert.That(textBox3.Focused, "Shift+Tab event did not roll over to last tab.");

			SendKeys.Send("+{Tab}"); //Shift + Tab - should go back one tab
			Assert.That(textBox2.Focused, $"Shift+Tab event did not focus {nameof(textBox2)}.");

			SendKeys.Send("+{Tab}"); //Shift + Tab - should go back one tab
			Assert.That(textBox1.Focused, $"Shift+Tab event did not focus {nameof(textBox1)}.");
		});
	}

	[Test]
	[TestCase(true, TestName = "{m}ToSpecifiedControl")]
	[TestCase(false, TestName = "{m}ToActiveControl")]
	public async Task SendWait_ToControlTest(bool specified)
	{
		Form form = null;
		Button initiatorButton = null;
		Button targetButton = null;

		using var ctx = new WinzorTestContext();
		await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			initiatorButton = new Button();
			targetButton = new Button();
			form.Controls.Add(initiatorButton);
			form.Controls.Add(targetButton);
			return form;
		});

		var eventFired = false;
		targetButton.KeyDown += (s, e) =>
		{
			eventFired = true;
		};

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			if (specified)
			{
				initiatorButton.Focus();
				SendKeys.SendWait("A", targetButton);
				Assert.That(eventFired, "Key event should be sent to targetButton as it is explicitly specified.");
			}
			else
			{
				targetButton.Focus();
				SendKeys.SendWait("A");
				Assert.That(eventFired, "Key event should be sent to targetButton as it is the active control.");
			}
		});
	}

	[Test]
	public async Task SendWait_ShouldNotThrowExceptionWhenFormHasNoControls()
	{
		using var ctx = new WinzorTestContext();
		Form form = null;
		await ctx.RenderFormAsync(() => form = new Form());

		await form.InvokeWinzorDispatcherAsync(
			() =>
			{
				Assert.DoesNotThrow(() => SendKeys.SendWait("A"));
			});
	}

	[Test]
	public async Task SendWait_ShouldNotThrowExceptionWhenControlHasNoParentForm()
	{
		using var ctx = new WinzorTestContext();
		var form = (await ctx.RenderFormAsync(() => new Form())).GetForm();

		await form.InvokeWinzorDispatcherAsync(
			() =>
			{
				var btn = new Button();
				Assert.DoesNotThrow(() => SendKeys.SendWait("A", btn));
			});
	}

	[Test]
	public async Task SendWait_ShouldSetText_WhenTargetControlIsExplicitlyAssigned()
	{
		using var ctx = new WinzorTestContext();
		var (textBox1, textBox2) = await CreateTextBoxesInContainerAsync(ctx);

		await textBox1.InvokeWinzorDispatcherAsync(() =>
		{
			SendKeys.SendWait("1", textBox1);
			Application.DoEvents();
			Assert.That(textBox1.Text, Is.EqualTo("1"));
		});
	}

	[Test]
	public async Task SendWait_ShouldSetText_WhenTextBoxIsFocused()
	{
		using var ctx = new WinzorTestContext();
		var (textBox1, textBox2) = await CreateTextBoxesInContainerAsync(ctx);

		await textBox1.InvokeWinzorDispatcherAsync(() =>
		{
			textBox1.Focus();
			SendKeys.SendWait("1");
			Application.DoEvents();
			Assert.That(textBox1.Text, Is.EqualTo("1"));
		});
	}

	[Test]
	public async Task SendWait_ShouldNotSetText_WhenTextBoxIsNotFocused()
	{
		using var ctx = new WinzorTestContext();
		var (textBox1, textBox2) = await CreateTextBoxesInContainerAsync(ctx);

		await textBox1.InvokeWinzorDispatcherAsync(() =>
		{
			textBox2.Focus();
			SendKeys.SendWait("1");
			Application.DoEvents();
			Assert.That(textBox1.Text, Is.EqualTo(string.Empty));
		});
	}

	static async Task<(TextBox textBox1, TextBox textBox2)> CreateTextBoxesInContainerAsync(WinzorTestContext ctx)
	{
		TextBox textBox1 = null;
		TextBox textBox2 = null;

		await ctx.RenderControlOnFormAsync(() =>
		{
			var container = new ContainerControl();
			textBox1 = new TextBox() { Name = "textBox1" };
			textBox2 = new TextBox() { Name = "textBox2" };
			container.Controls.Add(textBox1);
			container.Controls.Add(textBox2);
			return container;
		});

		return (textBox1, textBox2);
	}
}
