using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using Bunit;
using CargoWise.Blazor.Client.Integration.Menus;
using CargoWise.Blazor.Client.Integration.Messaging;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using OpenTelemetry;
using OpenTelemetry.Trace;
using WinzorFramework;
using WinzorFramework.JSInterop;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;
[SuppressMessage("CargoWiseOne", "CW1104:DoNotUseSystemWindowsTabControl", Justification = "Testing")]
class ControlTest
{
	[Test]
	public async Task ControlDefaultValues()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var control = new Control();
			Assert.That(control.CanSelect, Is.True);
			Assert.That(control.CausesValidation, Is.True);
		});
	}

	[Test]
	public async Task TopChangedFromServer()
	{
		await ControlAssert.ImplementsChangeFromServerAsync<Control>(c => c.Top = 200);
	}

	[Test]
	public async Task LeftChangedFromServer()
	{
		await ControlAssert.ImplementsChangeFromServerAsync<Control>(c => c.Left = 200);
	}

	[Test]
	public async Task WidthChangedFromServer()
	{
		int sizeChangedInvokedCount = 0;
		await ControlAssert.ImplementsChangeFromServerAsync<Control>(c =>
		{
			c.SizeChanged += (sender, args) => sizeChangedInvokedCount++;
			c.Width = 200;
		});
		Assert.That(sizeChangedInvokedCount, Is.EqualTo(1));
	}

	[Test]
	public async Task HeightChangedFromServer()
	{
		int sizeChangedInvokedCount = 0;
		await ControlAssert.ImplementsChangeFromServerAsync<Control>(c =>
		{
			c.SizeChanged += (sender, args) => sizeChangedInvokedCount++;
			c.Height = 200;
		});
		Assert.That(sizeChangedInvokedCount, Is.EqualTo(1));
	}

	[Test]
	public async Task SizeChangedFromServer()
	{
		int sizeChangedInvokedCount = 0;
		await ControlAssert.ImplementsChangeFromServerAsync<Control>(c =>
		{
			c.SizeChanged += (sender, args) => sizeChangedInvokedCount++;
			c.Size = new Size(200, 200);
		});
		Assert.That(sizeChangedInvokedCount, Is.EqualTo(1));
	}

	[Test]
	public async Task SizeNotChangedFromServer()
	{
		int sizeChangedInvokedCount = 0;
		await ControlAssert.NoOpFromServerAsync<Control>(c =>
		{
			c.SizeChanged += (sender, args) => sizeChangedInvokedCount++;
			c.Size = new Size(0, 0);
		});

		Assert.That(sizeChangedInvokedCount, Is.EqualTo(0));
	}

	[Test]
	public void ConstructingControlWithoutADispatcherThrows()
	{
		Assert.That(() => new Control(), Throws.InvalidOperationException.With.Message.EqualTo("The current thread is not a WinzorDispatcher"));
	}

	[Test]
	public async Task RunCargoWiseUsesDispatcher()
	{
		using var dispatcher = new WinzorDispatcher(Mock.Of<IFormOpener>(), Mock.Of<IFormInstanceRegister>());
		var dispatcherThreadId = 0;
		var runCargoWiseThreadId = 0;
		await dispatcher.InvokeAsync(() => dispatcherThreadId = Environment.CurrentManagedThreadId);
		TestControl control = null;
		await dispatcher.InvokeAsync(() =>
		{
			var form = new Form();
			control = new TestControl();
			form.Controls.Add(control);
		});
		try
		{
			await control.CallInvokeWinzorDispatcher(() => runCargoWiseThreadId = Environment.CurrentManagedThreadId);
			Assert.That(dispatcherThreadId, Is.Not.EqualTo(0));
			Assert.That(runCargoWiseThreadId, Is.EqualTo(dispatcherThreadId));
		}
		finally
		{
			control?.Dispose();
		}
	}

	[Test]
	public async Task CanRenderWhileModifyingControlsCollection()
	{
		using var ctx = new WinzorTestContext();
		using var cts = new CancellationTokenSource();
		Form form = null;
		var rendered = await ctx.RenderFormAsync(() => form = new Form());
		var cargoWiseTask = form.InvokeWinzorDispatcherAsync(() =>
		{
			form.SuspendLayout();
			while (!cts.IsCancellationRequested)
			{
				form.Controls.Clear();
				for (var i = 0; i < 100; i++)
				{
					form.Controls.Add(new Control());
				}
			}
		});
		try
		{
			for (var i = 0; i < 10; i++)
			{
				ctx.Render(form.RenderFragment);
			}
		}
		finally
		{
			await cts.CancelAsync();
			await cargoWiseTask;
		}
	}

	[Test]
	public async Task NoAdditionalReRenderWhenEventCallbackRunsToCompletion()
	{
		using var ctx = new WinzorTestContext();
		Control control = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => control = new Control() { Width = 100, Height = 50 });
		Assert.That(rendered.RenderCount, Is.EqualTo(1));
		await ctx.Renderer.Dispatcher.InvokeAsync(() => ((IHandleEvent)control).HandleEventAsync(EventCallbackWorkItem.Empty, null));
		Assert.That(rendered.RenderCount, Is.EqualTo(1));
	}

	[Test]
	public async Task SuspendResumeLayout()
	{
		using var ctx = new WinzorTestContext();
		Form form = null;
		var rendered = await ctx.RenderFormAsync(() => form = new Form());
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			form.SuspendLayout();
			for (var i = 0; i < 10; i++)
			{
				form.Controls.Add(new Control());
			}
		});
		Assert.That(rendered.RenderCount, Is.EqualTo(1));
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			form.ResumeLayout();
		});
		Assert.That(rendered.RenderCount, Is.EqualTo(2));
	}

	[Test]
	public async Task PerformLayoutShouldCallNotifyRenderRequired()
	{
		using var ctx = new WinzorTestContext();
		Form form = null;
		var rendered = await ctx.RenderFormAsync(() => form = new Form());
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			form.SuspendLayout();
			for (var i = 0; i < 10; i++)
			{
				form.Controls.Add(new Control());
			}
		});
		Assert.That(rendered.RenderCount, Is.EqualTo(1));
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			form.ResumeLayout(false);
		});
		Assert.That(rendered.RenderCount, Is.EqualTo(1));
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			form.PerformLayout();
		});
		Assert.That(rendered.RenderCount, Is.EqualTo(2));
	}

	[Test]
	public async Task SuspendResumeLayoutMultipleTimes()
	{
		using var ctx = new WinzorTestContext();
		Form form = null;
		var rendered = await ctx.RenderFormAsync(() => form = new Form());
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			form.SuspendLayout();
			form.SuspendLayout();
			for (var i = 0; i < 10; i++)
			{
				form.Controls.Add(new Control());
			}
		});
		Assert.That(rendered.RenderCount, Is.EqualTo(1));
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			form.ResumeLayout();
		});
		Assert.That(rendered.RenderCount, Is.EqualTo(1));
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			form.ResumeLayout();
		});
		Assert.That(rendered.RenderCount, Is.EqualTo(2));
	}

	[Test]
	public async Task SettingSizeRendersOnce()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, control) = await ctx.RenderControlOnFormAsync<Control>();
		await control.InvokeWinzorDispatcherAsync(() => control.Size = new Size(50, 50));
		Assert.That(rendered.RenderCount, Is.EqualTo(2));
	}

	[Test]
	public async Task SettingFormSizeRendersOnce()
	{
		using var ctx = new WinzorTestContext();
		Form form = null;
		var rendered = await ctx.RenderFormAsync(() => form = new Form());
		await form.InvokeWinzorDispatcherAsync(() => form.Size = new Size(50, 50));
		Assert.That(rendered.RenderCount, Is.EqualTo(2));
	}

	[Test]
	public async Task AddingChildRendersOnce()
	{
		using var ctx = new WinzorTestContext();
		Control control = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => control = new Control() { Width = 100, Height = 50 });
		await control.InvokeWinzorDispatcherAsync(() => control.Controls.Add(new Control() { Width = 100, Height = 50 }));
		Assert.That(rendered.RenderCount, Is.EqualTo(2));
	}

	[Test]
	public async Task SettingLocationRendersOnce()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, control) = await ctx.RenderControlOnFormAsync<Control>();
		await control.InvokeWinzorDispatcherAsync(() => control.Location = new Point(50, 50));
		Assert.That(rendered.RenderCount, Is.EqualTo(2));
	}

	[Test]
	public async Task SettingBoundsRendersOnce()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, control) = await ctx.RenderControlOnFormAsync<Control>();
		await control.InvokeWinzorDispatcherAsync(() => control.Bounds = new Rectangle(50, 50, 50, 50));
		Assert.That(rendered.RenderCount, Is.EqualTo(2));
	}

	[Test]
	public async Task TestClientEventsTraces()
	{
		var expectedControlName = "MyButton";
		var expectedWinzorControlId = "";

		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var button = new Button() { Name = expectedControlName };
			expectedWinzorControlId = button.WinzorControlId;
			form.Controls.Add(button);
			return form;
		});

		var traces = new List<Activity>();
		using var tracerProvider = Sdk.CreateTracerProviderBuilder()
		.AddSource("WinzorFramework")
		.AddInMemoryExporter(traces)
		.Build();

		await rendered.Find(".button").ClickAsync(new ());

		var trace = traces.FirstOrDefault(t => t.OperationName == "Form.Button.Handle_MouseEventArgs");
		Assert.That(trace, Is.Not.Null);

		var tags = trace.Tags;
		Assert.That(tags, Is.Not.Null);
		Assert.That(tags.Count, Is.EqualTo(2));
		Assert.That(tags, Has.One.Matches<KeyValuePair<string, string>>(pair => pair.Key == "Control.Name" && pair.Value == expectedControlName));
		Assert.That(tags, Has.One.Matches<KeyValuePair<string, string>>(pair => pair.Key == "WinzorControlId" && pair.Value == expectedWinzorControlId));
	}

	[Test]
	public async Task ChildControlsAreOrderedByTabIndex()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(new TextBox() { TabIndex = 3, Text = "6" });
			form.Controls.Add(new TextBox() { TabIndex = 1, Text = "2" });
			form.Controls.Add(new TextBox() { TabIndex = 0, Text = "1" });
			var groupBox = new GroupBox() { TabIndex = 2, Text = "2" };
			groupBox.Controls.Add(new TextBox() { TabIndex = 2, Text = "4" });
			groupBox.Controls.Add(new TextBox() { TabIndex = 2, Text = "5" });
			groupBox.Controls.Add(new TextBox() { TabIndex = 1, Text = "3" });
			form.Controls.Add(groupBox);
			return form;
		});
		var inputNodes = rendered.FindAll("input");
		Assert.That(inputNodes[0].Attributes["value"].Value, Is.EqualTo("1"));
		Assert.That(inputNodes[1].Attributes["value"].Value, Is.EqualTo("2"));
		Assert.That(inputNodes[2].Attributes["value"].Value, Is.EqualTo("3"));
		Assert.That(inputNodes[3].Attributes["value"].Value, Is.EqualTo("4"));
		Assert.That(inputNodes[4].Attributes["value"].Value, Is.EqualTo("5"));
	}

	[Test]
	public async Task MoveControlInChildControls()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var form = new Form();
			var textBox1 = new TextBox() { Text = "1" };
			var textBox2 = new TextBox() { Text = "2" };
			var textBox3 = new TextBox() { Text = "3" };
			var textBox4 = new TextBox() { Text = "4" };
			form.Controls.Add(textBox1);
			form.Controls.Add(textBox2);
			form.Controls.Add(textBox3);
			form.Controls.Add(textBox4);

			form.Controls.SetChildIndex(textBox1, 3);
			Assert.That(form.Controls[3], Is.EqualTo(textBox1));
			form.Controls.SetChildIndex(textBox2, 3);
			Assert.That(form.Controls[3], Is.EqualTo(textBox2));
			form.Controls.SetChildIndex(textBox3, 2);
			Assert.That(form.Controls[2], Is.EqualTo(textBox3));
			form.Controls.SetChildIndex(textBox4, 0);
			Assert.That(form.Controls[0], Is.EqualTo(textBox4));

			textBox4.SendToBack();
			Assert.That(form.Controls[3], Is.EqualTo(textBox4));
			textBox3.BringToFront();
			Assert.That(form.Controls[0], Is.EqualTo(textBox3));
			textBox2.SendToBack();
			Assert.That(form.Controls[3], Is.EqualTo(textBox2));
			textBox1.BringToFront();
			Assert.That(form.Controls[0], Is.EqualTo(textBox1));
		});
	}

	[Test]
	public async Task ChangeParentUpdatesParentControlCollections()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var form = new Form();
			var groupBox1 = new GroupBox();
			var groupBox2 = new GroupBox();
			form.Controls.Add(groupBox1);
			form.Controls.Add(groupBox2);
			var control = new TextBox();
			control.Parent = groupBox1;
			Assert.That(groupBox1.Controls, Does.Contain(control));
			Assert.That(groupBox2.Controls, Does.Not.Contain(control));
			control.Parent = groupBox2;
			Assert.That(groupBox1.Controls, Does.Not.Contain(control));
			Assert.That(groupBox2.Controls, Does.Contain(control));
		});
	}

	[Test, WithPlaywrightPage]
	public async Task LostFocusNotRaisedOnParentOfControlGettingFocus()
	{
		await using var ctx = new InMemoryTestServerContext();

		var userControlLostFocus = false;
		var textBoxGotFocused = new TaskCompletionSource();
		var buttonClicked = new TaskCompletionSource();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var userControl = new UserControl();
			var textBox = new TextBox();
			textBox.GotFocus += (s, e) => textBoxGotFocused.SetResult();
			userControl.Controls.Add(textBox);
			var button = new Button() { Top = 50 };
			button.Click += (s, e) => buttonClicked.SetResult();
			userControl.Controls.Add(button);
			form.Controls.Add(userControl);
			userControl.LostFocus += (s, e) => { userControlLostFocus = true; };
			return form;
		});

		await page.WaitForSelectorAsync("input");
		await (await page.QuerySelectorAsync("input")).ClickAsync();
		Assert.That(await textBoxGotFocused.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		Assert.That(userControlLostFocus, Is.False);
		await (await page.QuerySelectorAsync("button")).ClickAsync();
		Assert.That(await buttonClicked.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		Assert.That(userControlLostFocus, Is.False);
	}

	[Test, WithPlaywrightPage]
	public async Task LostFocusNotRaisedOnParentControl()
	{
		await using var ctx = new InMemoryTestServerContext();

		var userControlLostFocus = false;
		var textBoxGotFocused = new TaskCompletionSource();
		var buttonClicked = new TaskCompletionSource();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var userControl = new UserControl();
			userControl.Top = 100;
			var textBox = new TextBox();
			textBox.GotFocus += (s, e) => textBoxGotFocused.SetResult();
			userControl.Controls.Add(textBox);
			var button = new Button();
			button.Click += (s, e) => buttonClicked.SetResult();
			form.Controls.Add(button);
			form.Controls.Add(userControl);
			userControl.LostFocus += (s, e) => { userControlLostFocus = true; };
			return form;
		});

		await page.WaitForSelectorAsync("input");
		await (await page.QuerySelectorAsync("input")).ClickAsync();
		Assert.That(await textBoxGotFocused.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		Assert.That(userControlLostFocus, Is.False);
		await (await page.QuerySelectorAsync("button")).ClickAsync();
		Assert.That(await buttonClicked.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		Assert.That(userControlLostFocus, Is.False);
	}

	[Test, WithPlaywrightPage]
	public async Task LostFocusNotRaisedWhenWindowLosesFocus()
	{
		// onfocusout is not raised when switching browser tabs using playwright like it is with chrome
		// therefore this test does not reproduce the original problem, but will keep it as a demonstration of how it should work anyway

		await using var ctx = new InMemoryTestServerContext();

		var textBox1GotFocused = new TaskCompletionSource();
		var textBox1LostFocus = false;
		var textBox2GotFocused = new TaskCompletionSource();
		var page1 = await ctx.LoadFormAsync(() =>
		{
			var form1 = new Form();
			var textBox1 = new TextBox();
			textBox1.Size = new Size(100, 20);
			textBox1.GotFocus += (s, e) => textBox1GotFocused.SetResult();
			textBox1.LostFocus += (s, e) => { textBox1LostFocus = true; };
			form1.Controls.Add(textBox1);
			return form1;
		});

		var page2 = await ctx.LoadFormAsync(() =>
		{
			var form2 = new Form();
			var textBox2 = new TextBox();
			textBox2.Size = new Size(100, 20);
			textBox2.GotFocus += (s, e) => textBox2GotFocused.SetResult();
			form2.Controls.Add(textBox2);
			return form2;
		});

		await page1.BringToFrontAsync();
		await page1.WaitForSelectorAsync("input");
		await (await page1.QuerySelectorAsync("input")).ClickAsync();
		Assert.That(await textBox1GotFocused.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		Assert.That(textBox1LostFocus, Is.False);

		await page2.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
		await page2.WaitForSelectorAsync("input");
		await (await page2.QuerySelectorAsync("input")).ClickAsync();
		Assert.That(await textBox2GotFocused.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		Assert.That(textBox1LostFocus, Is.False);
	}

	[Test]
	public async Task LostFocusCalledWhenChangingFocusOnServer()
	{
		using var ctx = new WinzorTestContext();
		Control control1 = null;
		Control control2 = null;

		var control1GotFocusEventCount = 0;
		var control1LostFocusEventCount = 0;
		var control2GotFocusEventCount = 0;
		var control2LostFocusEventCount = 0;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			control1 = new Control();
			control1.GotFocus += (_, _) => control1GotFocusEventCount++;
			control1.LostFocus += (_, _) => control1LostFocusEventCount++;
			control2 = new Control();
			control2.GotFocus += (_, _) => control2GotFocusEventCount++;
			control2.LostFocus += (_, _) => control2LostFocusEventCount++;
			form.Controls.AddRange(new Control[] { control1, control2 });

			return form;
		});

		Assert.That(control1.Focused, Is.True);
		Assert.That(control1GotFocusEventCount, Is.EqualTo(1));
		Assert.That(control1LostFocusEventCount, Is.EqualTo(0));
		Assert.That(control2.Focused, Is.False);
		Assert.That(control2GotFocusEventCount, Is.EqualTo(0));
		Assert.That(control2LostFocusEventCount, Is.EqualTo(0));

		await control2.InvokeWinzorDispatcherAsync(() => control2.Focus());

		Assert.That(control1.Focused, Is.False);
		Assert.That(control1GotFocusEventCount, Is.EqualTo(1));
		Assert.That(control1LostFocusEventCount, Is.EqualTo(1));
		Assert.That(control2.Focused, Is.True);
		Assert.That(control2GotFocusEventCount, Is.EqualTo(1));
		Assert.That(control2LostFocusEventCount, Is.EqualTo(0));
	}

	[Test]
	public async Task HideFocusedControlShouldFocusNextControl()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var textBox1 = new TextBox() { Text = "TextBox1" };
			form.Controls.Add(textBox1);
			var textBox2 = new TextBox() { Text = "TextBox2" };
			form.Controls.Add(textBox2);
			return form;
		});
		var form = rendered.GetForm();

		var textBox1 = form.Controls.OfType<TextBox>().FirstOrDefault(tb => tb.Text == "TextBox1");
		var textBox2 = form.Controls.OfType<TextBox>().FirstOrDefault(tb => tb.Text == "TextBox2");

		Assert.That(textBox1.Focused, Is.True);
		Assert.That(textBox2.Focused, Is.False);

		await textBox1.InvokeWinzorDispatcherAsync(() => textBox1.Hide());

		Assert.That(textBox1.Focused, Is.False);
		Assert.That(textBox2.Focused, Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task SkipTabbingOnTabStopFalse()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var textBox1 = new TextBox() { Width = 100, Height = 20, Top = 0, Left = 0, Text = "TextBox1" };
			form.Controls.Add(textBox1);
			var textBox2 = new TextBox() { Width = 100, Height = 20, Top = 0, Left = 100, TabStop = false, Text = "TextBox2" };
			form.Controls.Add(textBox2);
			var button = new Button() { Width = 100, Height = 20, Top = 0, Left = 200, TabStop = false };
			form.Controls.Add(button);
			var checkbox = new CheckBox() { Width = 100, Height = 20, Top = 0, Left = 300, TabStop = false };
			form.Controls.Add(checkbox);
			var numericUpDown = new NumericUpDown() { Width = 100, Height = 20, Top = 0, Left = 400, TabStop = false };
			form.Controls.Add(numericUpDown);
			var textBox3 = new TextBox() { Width = 100, Height = 20, Top = 0, Left = 500, Text = "TextBox3" };
			form.Controls.Add(textBox3);
			return form;
		});

		var initialInput = await page.WaitForSelectorAsync("input:first-child");
		Assert.That(await initialInput.InputValueAsync(), Is.EqualTo("TextBox1"));
		await initialInput.ClickAsync();
		await initialInput.FocusAsync();
		await page.Keyboard.PressAsync("Tab");
		var expectedFocusedControl = await page.WaitForSelectorAsync("input:focus");
		Assert.That(await expectedFocusedControl.InputValueAsync(), Is.EqualTo("TextBox3"));
	}

	[Test, WithPlaywrightPage]
	public async Task TabbingOnTabStopTrue()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var textBox1 = new TextBox() { Width = 100, Height = 20, Top = 0, Left = 0, Text = "TextBox1" };
			form.Controls.Add(textBox1);
			var textBox2 = new TextBox() { Width = 100, Height = 20, Top = 0, Left = 100, Text = "TextBox2" };
			form.Controls.Add(textBox2);
			var button = new Button() { Width = 100, Height = 20, Top = 0, Left = 200, Text = "Button1" };
			form.Controls.Add(button);
			var checkbox = new CheckBox() { Width = 100, Height = 20, Top = 0, Left = 300 };
			form.Controls.Add(checkbox);
			var numericUpDown = new NumericUpDown() { Width = 100, Height = 20, Top = 0, Left = 400, Value = 10 };
			form.Controls.Add(numericUpDown);
			var textBox3 = new TextBox() { Width = 100, Height = 20, Top = 0, Left = 500, Text = "TextBox3" };
			form.Controls.Add(textBox3);
			return form;
		});

		var initialInput = await page.WaitForSelectorAsync("input:first-child");
		Assert.That(await initialInput.InputValueAsync(), Is.EqualTo("TextBox1"));
		await initialInput.ClickAsync();
		await initialInput.FocusAsync();
		await page.Keyboard.PressAsync("Tab");
		var expectedFocusedControl = await page.WaitForSelectorAsync("input:focus");
		Assert.That(await expectedFocusedControl.InputValueAsync(), Is.EqualTo("TextBox2"));
		await page.Keyboard.PressAsync("Tab");
		expectedFocusedControl = await page.WaitForSelectorAsync("button:focus");
		Assert.That(await expectedFocusedControl.TextContentAsync(), Is.EqualTo("Button1"));
		await page.Keyboard.PressAsync("Tab");
		expectedFocusedControl = await page.WaitForSelectorAsync("input[type=checkbox]:focus");
		await page.Keyboard.PressAsync("Tab");
		expectedFocusedControl = await page.WaitForSelectorAsync(".numericupdown:focus");
		Assert.That(await expectedFocusedControl.GetAttributeAsync("class"), Is.EqualTo("numericupdown"));
		await page.Keyboard.PressAsync("Tab");
		expectedFocusedControl = await page.WaitForSelectorAsync("input:focus");
		Assert.That(await expectedFocusedControl.InputValueAsync(), Is.EqualTo("10"));
		await page.Keyboard.PressAsync("Tab");
		expectedFocusedControl = await page.WaitForSelectorAsync("input:focus");
		Assert.That(await expectedFocusedControl.InputValueAsync(), Is.EqualTo("TextBox3"));
	}

	[Test]
	public async Task AfterRenderEventsAreNotExecutedInOrderOnDifferentForms()
	{
		using var ctx = new WinzorTestContext();
		TestControl control1 = null;
		TestControl control2 = null;
		var firstTask = new TaskCompletionSource<DateTime>();
		var secondTask = new TaskCompletionSource<DateTime>();
		var form1 = await ctx.RenderControlOnFormAsync(() => control1 = new TestControl());
		var form2 = await ctx.RenderControlOnFormAsync(() => control2 = new TestControl());
		Assert.That(control1, Is.Not.Null);
		Assert.That(control2, Is.Not.Null);
		await control1.InvokeWinzorDispatcherAsync(() =>
		{
			control1.RegisterAfterRenderAction(async () =>
			{
				await Task.Delay(1000);
				firstTask.SetResult(DateTime.UtcNow);
			});
		});
		await control2.InvokeWinzorDispatcherAsync(() =>
		{
			control2.RegisterAfterRenderAction(async () =>
			{
				secondTask.SetResult(DateTime.UtcNow);
				await Task.CompletedTask;
			});
		});
		form1.Render();
		form2.Render();
		Assert.That(form1.RenderCount, Is.EqualTo(2));
		Assert.That(form2.RenderCount, Is.EqualTo(2));
		Assert.That(await secondTask.Task, Is.LessThan(await firstTask.Task), "Second after render action did not complete before the first");
	}

	[Test]
	public async Task FindFormReturnsNullIfParentFormIsNull()
	{
		using var ctx = new WinzorTestContext();
		TestControl control = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(control = new TestControl());
			return form;
		});
		await control.InvokeWinzorDispatcherAsync(() =>
		{
			control.Parent = null;
		});
		Assert.That(control.FindForm(), Is.Null);
	}

	[Test]
	public async Task ControlAfterRenderEventsAreNotExecutedIfFindFormReturnsNull()
	{
		using var ctx = new WinzorTestContext();
		TestControl control = null;
		var numberOfTimesAfterRenderEventsExecuted = 0;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(control = new TestControl());
			return form;
		});
		await control.InvokeWinzorDispatcherAsync(() =>
		{
			control.RegisterAfterRenderAction(async () =>
			{
				numberOfTimesAfterRenderEventsExecuted++;
				await Task.CompletedTask;
			});
		});
		rendered.Render();
		await control.InvokeWinzorDispatcherAsync(() =>
		{
			control.Parent = null;
		});
		rendered.Render();
		Assert.That(numberOfTimesAfterRenderEventsExecuted, Is.EqualTo(1));
	}

	[Test]
	public async Task ZIndex()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var control = new Control() { Width = 100, Height = 50 };
			control.ZIndex = 1;
			return control;
		});
		Assert.That(rendered.FindAll(".form div[data-type='System.Windows.Forms.Control']").Last().GetAttribute("style"), Does.Contain("z-index:1;"));
	}

	[Test]
	public async Task ControlShouldHaveDataAttributes()
	{
		using var ctx = new WinzorTestContext();
		Control control = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			control = new Control() { Width = 100, Height = 50, Name = "TestControl" };
			return control;
		});
		var element = rendered.Find(".form div[data-type='System.Windows.Forms.Control']");
		Assert.That(element.GetAttribute("data-winzor-control-id"), Is.EqualTo(control.WinzorControlId));
		Assert.That(element.GetAttribute("data-name"), Is.EqualTo(control.Name));
		Assert.That(element.GetAttribute("data-layout"), Is.EqualTo(control.DebugAttributesString));
	}

	class TestControl : Control
	{
		public Task CallInvokeWinzorDispatcher(Action action) => InvokeWinzorDispatcherAsync(action);

		public new void RegisterAfterRenderAction(Func<Task> action) => base.RegisterAfterRenderAction(action);

		protected override Size DefaultSize => new Size(100, 50);
	}

	[Test]
	[TestCase(0, 0, 20, 0, 40, 0, 100, 100, 40, 100, TestName = "PointToClient-Basic-1")]
	[TestCase(10, 20, 30, 40, 50, 60, 200, 200, 110, 80, TestName = "PointToClient-Basic-2")]
	public async Task PointToClientTest(int parentX, int parentY,
		int child1X, int child1Y, int child2X, int child2Y,
		int inputX, int inputY, int expectedX, int expextedY)
	{
		using var ctx = new WinzorTestContext();
		(var child1, var child2) =
			await CreateNestedControlsAsync(ctx, parentX, parentY, child1X, child1Y, child2X, child2Y);

		var clientPoint = child2.PointToClient(new Point(inputX, inputY));
		Assert.That(clientPoint.X, Is.EqualTo(expectedX));
		Assert.That(clientPoint.Y, Is.EqualTo(expextedY));
	}

	[Test]
	[TestCase(0, 0, 20, 0, 40, 0, 100, 100, 40, 100, TestName = "PointToClient-NoTopForm-1")]
	[TestCase(10, 20, 30, 40, 50, 60, 200, 200, 120, 100, TestName = "PointToClient-NoTopForm-2")]
	[TestCase(55, 300, 30, 40, 50, 60, 200, 200, 120, 100, TestName = "PointToClient-NoTopForm-3")]
	public async Task PointToClientWithFormTest(int parentX, int parentY,
		int child1X, int child1Y, int child2X, int child2Y,
		int inputX, int inputY, int expectedX, int expextedY)
	{
		using var ctx = new WinzorTestContext();
		(var child1, var child2) =
			await CreateNestedControlsWithFormAsync(ctx, parentX, parentY, child1X, child1Y, child2X, child2Y);

		var clientPoint = child2.PointToClient(new Point(inputX, inputY));
		Assert.That(clientPoint.X, Is.EqualTo(expectedX));
		Assert.That(clientPoint.Y, Is.EqualTo(expextedY));
	}

	[Test]
	[TestCase(0, 0, 20, 0, 40, 0, 60, 0, TestName = "PointToScreen-Basic-1")]
	[TestCase(10, 20, 30, 40, 50, 60, 90, 120, TestName = "PointToScreen-Basic-2")]
	public async Task PointToScreenTest(int parentX, int parentY,
		int child1X, int child1Y, int child2X, int child2Y,
		int expectedX, int expextedY)
	{
		using var ctx = new WinzorTestContext();
		(var child1, var child2) =
			await CreateNestedControlsAsync(ctx, parentX, parentY, child1X, child1Y, child2X, child2Y);

		var screenPoint = child1.PointToScreen(child2.Location);
		Assert.That(screenPoint.X, Is.EqualTo(expectedX));
		Assert.That(screenPoint.Y, Is.EqualTo(expextedY));
	}

	[Test]
	[TestCase(0, 0, 20, 0, 40, 0, 60, 0, TestName = "PointToScreen-NoTopForm-1")]
	[TestCase(10, 20, 30, 40, 50, 60, 80, 100, TestName = "PointToScreen-NoTopForm-2")]
	[TestCase(320, 540, 30, 40, 50, 60, 80, 100, TestName = "PointToScreen-NoTopForm-3")]
	public async Task PointToScreenWithFormTest(int parentX, int parentY,
		int child1X, int child1Y, int child2X, int child2Y,
		int expectedX, int expextedY)
	{
		using var ctx = new WinzorTestContext();
		(var child1, var child2) =
			await CreateNestedControlsWithFormAsync(ctx, parentX, parentY, child1X, child1Y, child2X, child2Y);

		var screenPoint = child1.PointToScreen(child2.Location);
		Assert.That(screenPoint.X, Is.EqualTo(expectedX));
		Assert.That(screenPoint.Y, Is.EqualTo(expextedY));
	}

	[Test]
	public async Task LayoutEventFiresOnPerformLayout()
	{
		await ControlAssert.FiresControlEvent<Control, LayoutEventHandler>(nameof(Control.Layout), a => new LayoutEventHandler((s, e) => a()), o => { o.PerformLayout(); });
	}

	[Test]
	public async Task LocationChangedEventFiresOnSetLocation()
	{
		await ControlAssert.FiresControlEvent<Control, EventHandler>(nameof(Control.LocationChanged), a => new EventHandler((s, e) => a()), o => { o.Location = new Point(20, 30); });
	}

	[Test]
	public async Task ForeColorChangedEventFiresOnSetForeColor()
	{
		await ControlAssert.FiresControlEvent<Control, EventHandler>(nameof(Control.ForeColorChanged), a => new EventHandler((s, e) => a()), o => { o.ForeColor = Color.Red; });
	}

	async Task<(Control, Control)> CreateNestedControlsAsync(WinzorTestContext ctx,
		int parentX, int parentY, int child1X, int child1Y, int child2X, int child2Y)
	{
		Control parent = null;
		Control child1 = null;
		Control child2 = null;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			parent = new Control { Width = 500, Height = 500, Left = parentX, Top = parentY };
			child1 = new Control { Width = 400, Height = 400, Left = child1X, Top = child1Y };
			child2 = new Control { Width = 300, Height = 300, Left = child2X, Top = child2Y };
			parent.Controls.Add(child1);
			child1.Controls.Add(child2);
		});
		return (child1, child2);
	}

	async Task<(Control, Control)> CreateNestedControlsWithFormAsync(WinzorTestContext ctx,
		int parentX, int parentY, int child1X, int child1Y, int child2X, int child2Y)
	{
		Form parent = null;
		Control child1 = null;
		Control child2 = null;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			parent = new Form { Width = 500, Height = 500, Left = parentX, Top = parentY };
			child1 = new Control { Width = 400, Height = 400, Left = child1X, Top = child1Y };
			child2 = new Control { Width = 300, Height = 300, Left = child2X, Top = child2Y };
			parent.Controls.Add(child1);
			child1.Controls.Add(child2);
		});
		return (child1, child2);
	}

	[Test]
	public async Task ControlDisposeResetsDataBindingComponent()
	{
		using var ctx = new WinzorTestContext();
		Control control = null;
		Binding dataBinding = null;
		await ctx.RenderControlOnFormAsync(() =>
		{
			control = new Control();
			dataBinding = new Binding("", new object(), "");
			control.DataBindings.Add(dataBinding);
			return control;
		});

		Assert.That(control.DataBindings.Count, Is.EqualTo(1));
		Assert.That(dataBinding.BindableComponent, Is.EqualTo(control));

		await control.InvokeWinzorDispatcherAsync(() =>
		{
			control.Dispose();
		});

		Assert.That(dataBinding.BindableComponent, Is.EqualTo(null));
	}

	[Test]
	public async Task ControlDoesNotTriggerDisposedEventHandlerIfAlreadyTriggeredOnce()
	{
		using var ctx = new WinzorTestContext();
		Control control = null;
		var disposeCalledCount = 0;
		await ctx.RenderControlOnFormAsync(() =>
		{
			control = new Control();
			control.Disposed += (sender, args) => { disposeCalledCount++; };
			return control;
		});

		await control.InvokeWinzorDispatcherAsync(() =>
		{
			control.Dispose();
			control.Dispose();
		});

		Assert.That(disposeCalledCount, Is.EqualTo(1));
	}

	[Test]
	public async Task ControlShouldDetachFromRendererWhenParentSetToNull()
	{
		using var ctx = new WinzorTestContext();
		Control control = null;
		await ctx.RenderControlOnFormAsync(() => control = new ControlForTest());

		Assert.That(control.Proxy, Is.Not.Null);
		await control.InvokeWinzorDispatcherAsync(() => control.Parent = null);
		Assert.That(control.Proxy, Is.Null);
	}

	[Test]
	public async Task ControlHasCorrecStyleStringApplied()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new Control() { BackColor = Color.Red, Width = 200, Height = 300, Top = 40, Left = 50, });
		Assert.That(rendered.Find("div[data-type='System.Windows.Forms.Control']").GetAttribute("style"), Contains.Substring("position:absolute;width:200px;height:300px;top:40px;left:50px;background-color:#FF0000FF;"));
	}

	[Test]
	public async Task ControlTranslatesDisabledWindowBackColor()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new Control() { BackColor = SystemColors.Window, Enabled = false, Width = 200, Height = 300, Top = 40, Left = 50, });
		Assert.That(rendered.Find("div[data-type='System.Windows.Forms.Control']").GetAttribute("style"), Contains.Substring("position:absolute;width:200px;height:300px;top:40px;left:50px;background-color:var(--color-control);"));
	}

	[Test]
	public async Task ChildControlBackColorShouldBeCorrect()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var child1 = new Control() { Width = 200, Height = 300, BackColor = Color.Red };
			var child2 = new Control() { Width = 100, Height = 150, BackColor = Color.Transparent };
			var child3 = new Control() { Width = 100, Height = 150, BackColor = Color.Blue };

			child1.Controls.Add(child2);
			child2.Controls.Add(child3);
			return child1;
		});

		var controls = rendered.FindAll("div[data-type='System.Windows.Forms.Control']");

		Assert.That(controls[0].GetAttribute("style"), Does.Contain("background-color:#FF0000FF;"));
		Assert.That(controls[1].GetAttribute("style"), Does.Contain("background-color:#FF0000FF;"));
		Assert.That(controls[2].GetAttribute("style"), Does.Contain("background-color:#0000FFFF;"));
	}

	[Test]
	public async Task ControlHasCorrecTitleStringApplied()
	{
		using var ctx = new WinzorTestContext();
		var toolTipText = "tooltip";
		var rendered = await ctx.RenderControlOnFormAsync(() => new Control() { Width = 200, Height = 300, ToolTipText = toolTipText });
		Assert.That(rendered.Find("div[data-type='System.Windows.Forms.Control']").GetAttribute("title"), Is.EqualTo(toolTipText));
	}

	[Test]
	public async Task ControlUsesDefaultBackColorIfEmpty()
	{
		using var ctx = new WinzorTestContext();
		Control control = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => control = new Control() { Width = 100, Height = 100, });

		Assert.That(control.BackColor, Is.EqualTo(SystemColors.Control));
		Assert.That(rendered.Find("div[data-type='System.Windows.Forms.Control']").GetAttribute("style"), Contains.Substring($"background-color:var(--color-control);"));
	}

	[Test]
	public async Task ControlDefaultBackColorShouldBeSystemKnownColor()
	{
		using var ctx = new WinzorTestContext();
		Control control = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => control = new Control() { Width = 100, Height = 100, });

		Assert.That(control.BackColor, Is.EqualTo(SystemColors.Control));
		Assert.That(rendered.Find("div[data-type='System.Windows.Forms.Control']").GetAttribute("style"), Contains.Substring($"background-color:var(--color-control);"));
	}

	[Test]
	public async Task ControlUsesSpecifiedBackColor()
	{
		using var ctx = new WinzorTestContext();
		Control control = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => control = new Control() { BackColor = Color.Blue, Width = 100, Height = 100, });

		Assert.That(control.BackColor, Is.EqualTo(Color.Blue));
		Assert.That(rendered.Find("div[data-type='System.Windows.Forms.Control']").GetAttribute("style"), Contains.Substring($"background-color:#0000FFFF"));
	}

	[Test]
	public async Task ControlUsesParentBackColorIfEmpty()
	{
		using var ctx = new WinzorTestContext();
		Control control = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var parent = new Control() { BackColor = Color.Blue, Width = 100, Height = 100 };
			parent.Controls.Add(control = new Control() { Width = 100, Height = 100 });
			return parent;
		});

		Assert.That(control.BackColor, Is.EqualTo(Color.Blue));
		Assert.That(rendered.FindAll("div[data-type='System.Windows.Forms.Control']")[1].GetAttribute("style"), Contains.Substring($"background-color:#0000FFFF"));
	}

	[Test]
	public async Task RootControlBackColorCanNotBeTransparentWhenRendering()
	{
		using var ctx = new WinzorTestContext();
		Form form = null;
		var rendered = await ctx.RenderFormAsync(() => form = new Form() { BackColor = Color.Red });

		var formElement = rendered.Find(".form");
		Assert.That(formElement.GetAttribute("style"), Does.Contain("background-color:#FF0000FF;"));

		await form.InvokeWinzorDispatcherAsync(() => form.BackColor = Color.Transparent);
		formElement = rendered.Find(".form");
		Assert.That(formElement.GetAttribute("style"), Does.Contain("background-color:var(--color-control);"));
	}

	[Test]
	public async Task ControlNotEnabledIfParentIsNotEnabled()
	{
		using var ctx = new WinzorTestContext();
		Control control = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var parent = new Control() { Enabled = false, Width = 100, Height = 100 };
			parent.Controls.Add(control = new Control() { Enabled = true, Width = 100, Height = 100 });
			return parent;
		});
		Assert.That(control.Enabled, Is.False);
		await control.InvokeWinzorDispatcherAsync(() => control.Enabled = false);
		Assert.That(control.Enabled, Is.False);
	}

	[Test]
	public async Task ControlUsesSpecifiedEnabledPropertyIfParentIsEnabled()
	{
		using var ctx = new WinzorTestContext();
		Control control = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var parent = new Control() { Enabled = true, Width = 100, Height = 100 };
			parent.Controls.Add(control = new Control() { Enabled = true, Width = 100, Height = 100 });
			return parent;
		});
		Assert.That(control.Enabled, Is.True);
		await control.InvokeWinzorDispatcherAsync(() => control.Enabled = false);
		Assert.That(control.Enabled, Is.False);
	}

	[Test]
	public async Task ControlUsesSpecifiedEnabledPropertyIfParentIsNull()
	{
		using var ctx = new WinzorTestContext();
		Control control = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => control = new Control() { Enabled = true, Width = 100, Height = 100, Parent = null });
		Assert.That(control.Enabled, Is.True);
		await control.InvokeWinzorDispatcherAsync(() => control.Enabled = false);
		Assert.That(control.Enabled, Is.False);
	}

	[Test]
	public async Task ControlWithWidthOfZeroShouldNotBeRendered()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new Control() { Width = 100, Height = 100, });
		var renderedControls = rendered.FindAll("div[data-type='System.Windows.Forms.Control']");
		Assert.That(renderedControls.Count, Is.EqualTo(1));

		rendered = await ctx.RenderControlOnFormAsync(() => new Control() { Width = 0, Height = 100, });
		renderedControls = rendered.FindAll("div[data-type='System.Windows.Forms.Control']");
		Assert.That(renderedControls.Count, Is.EqualTo(0));
	}

	[Test]
	public async Task ControlWithHeightOfZeroShouldNotBeRendered()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new Control() { Width = 100, Height = 100, });
		var renderedControls = rendered.FindAll("div[data-type='System.Windows.Forms.Control']");
		Assert.That(renderedControls.Count, Is.EqualTo(1));

		rendered = await ctx.RenderControlOnFormAsync(() => new Control() { Width = 100, Height = 0, });
		renderedControls = rendered.FindAll("div[data-type='System.Windows.Forms.Control']");
		Assert.That(renderedControls.Count, Is.EqualTo(0));
	}

	[Test]
	public async Task ControlClickEventUpdatesSavedMousePosition()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new Button());

		Assert.That(Control.MousePosition.IsEmpty, Is.True);
		await rendered.Find(".button").ClickAsync(new WebMouseEventArgs()
		{
			ClientX = 123,
			ClientY = 456,
		});
		Assert.That(Control.MousePosition.X, Is.EqualTo(123));
		Assert.That(Control.MousePosition.Y, Is.EqualTo(456));
	}

	[Test]
	public void ControlDefaultFont()
	{
		Assert.That(Control.DefaultFont.Name, Is.EqualTo("Tahoma"));
		Assert.That(Control.DefaultFont.Size, Is.EqualTo(8));
		Assert.That(Control.DefaultFont.SizeInPoints, Is.EqualTo(8));
		Assert.That(Control.DefaultFont.Height, Is.EqualTo(13));
	}

	[Test]
	public async Task ControlFontReturnsSpecifiedFont()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var control = new Control() { Font = new Font("Arial", 10) };
			Assert.That(control.Font.Name, Is.EqualTo("Arial"));
			Assert.That(control.Font.Size, Is.EqualTo(10));
		});
	}

	[Test]
	public async Task ControlFontReturnsParentFontIfNoneSpecified()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var parent = new Control() { Font = new Font("Arial", 10) };
			var control = new Control();
			parent.Controls.Add(control);
			Assert.That(control.Font.Name, Is.EqualTo("Arial"));
			Assert.That(control.Font.Size, Is.EqualTo(10));
		});
	}

	[Test]
	public async Task ControlFontReturnsDefaultFontIfNoneSpecifiedAndParentIsNull()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var control = new Control();
			Assert.That(control.Font.Name, Is.EqualTo(Control.DefaultFont.Name));
			Assert.That(control.Font.Size, Is.EqualTo(Control.DefaultFont.Size));
		});
	}

	[Test, WithPlaywrightPage]
	public async Task RenderControlsDoNotRerenderControlWithUnchangedKeys()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form parent = null;
		Button child1 = null;
		Button child2 = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			parent = new Form();
			child1 = new Button { Text = "1", TabIndex = 1, Top = 0 };
			child2 = new Button { Text = "2", TabIndex = 3, Top = 25 };

			parent.Controls.Add(child1);
			parent.Controls.Add(child2);

			return parent;
		});

		await page.WaitForSelectorAsync("'2'");

		var child2Button = page.Locator("button", new () { HasTextString = "2" });

		Assert.That(child2.Focused, Is.False);
		Assert.That(await child2Button.EvaluateAsync<bool>("node => document.activeElement === node"), Is.False);

		await parent.InvokeWinzorDispatcherAsync(() => child2.Focus());

		Assert.That(() => child2.Focused, Is.True.After(2000, 100));
		Assert.That(async () => await child2Button.EvaluateAsync<bool>("node => document.activeElement === node"), Is.True.After(2000, 100));

		await parent.InvokeWinzorDispatcherAsync(() =>
		{
			parent.Controls.Remove(child1);
		});

		Assert.That(() => child2.Focused, Is.True.After(500));
		Assert.That(async () => await child2Button.EvaluateAsync<bool>("node => document.activeElement === node"), Is.True.After(500));
	}

	[Test, WithPlaywrightPage]
	public async Task TestAllowItemDragMakeCursorPointChange()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var control = new Control { Height = 100, Width = 100 };
			control.AllowItemDrag = true;

			var form = new Form();
			form.Controls.Add(control);
			return form;
		});

		var element = page.Locator("div[draggable='true']");
		await element.DragToAsync(element, new()
		{
			SourcePosition = new() { X = 20, Y = 20 },
			TargetPosition = new() { X = 0, Y = 0 },
		});
		Assert.That(() => Cursor.Position, Is.EqualTo(new Point(0, 0)).After(3000, 100));

		await element.DragToAsync(element, new()
		{
			SourcePosition = new() { X = 10, Y = 10 },
			TargetPosition = new() { X = 30, Y = 30 },
		});
		Assert.That(() => Cursor.Position, Is.EqualTo(new Point(30, 30)).After(3000, 200));
	}

	[Test]
	public async Task ControlMinimumSize()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var control = new Control();
			Assert.That(control.MinimumSize, Is.EqualTo(Size.Empty));
			control.Size = new Size(100, 200);
			control.MinimumSize = new Size(123, 456);
			Assert.That(control.MinimumSize, Is.EqualTo(new Size(123, 456)));
			Assert.That(control.Size, Is.EqualTo(new Size(123, 456)));
		});
	}

	[Test]
	public async Task ControlMaximumSize()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var control = new Control();
			Assert.That(control.MaximumSize, Is.EqualTo(Size.Empty));
			control.Size = new Size(1000, 2000);
			control.MaximumSize = new Size(123, 456);
			Assert.That(control.MaximumSize, Is.EqualTo(new Size(123, 456)));
			Assert.That(control.Size, Is.EqualTo(new Size(123, 456)));
		});
	}

	[Test]
	public async Task TestUpdateShouldFirePaint()
	{
		using var ctx = new WinzorTestContext();
		Control control = null;
		var paintFired = false;

		await ctx.RenderControlOnFormAsync(() =>
		{
			control = new Control { Width = 100, Height = 100 };
			return control;
		});

		await control.InvokeWinzorDispatcherAsync(() =>
		{
			control.Paint += (_, _) => paintFired = true;
			Assert.That(paintFired, Is.False);
			control.Update();
		});
		Assert.That(paintFired, Is.True);
	}

	[Test]
	public async Task AddingToSameControlCollectionSendsToBack()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var control = new Control();
			var child1 = new Control();
			var child2 = new Control();

			control.Controls.Add(child1);
			control.Controls.Add(child2);
			Assert.That(control.Controls.Count, Is.EqualTo(2));
			Assert.That(control.Controls[0], Is.EqualTo(child1));
			Assert.That(control.Controls[1], Is.EqualTo(child2));

			control.Controls.Add(child1);
			Assert.That(control.Controls.Count, Is.EqualTo(2));
			Assert.That(control.Controls[0], Is.EqualTo(child2));
			Assert.That(control.Controls[1], Is.EqualTo(child1));
		});
	}

	[Test]
	public async Task BringToFrontAddZIndex()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var parent = new Control();
			var control1 = new Control() { ZIndex = 0 };
			var control2 = new Control() { ZIndex = 1 };
			parent.Controls.AddRange(new Control[] { control1, control2 });

			Assert.That(control1.ZIndex, Is.EqualTo(0));
			Assert.That(control2.ZIndex, Is.EqualTo(1));
			control1.BringToFront();
			Assert.That(control1.ZIndex, Is.EqualTo(2));
			Assert.That(control2.ZIndex, Is.EqualTo(1));
			control2.BringToFront();
			Assert.That(control1.ZIndex, Is.EqualTo(2));
			Assert.That(control2.ZIndex, Is.EqualTo(3));
		});
	}

	[Test]
	public async Task SendToBackZIndexNotLessThanZero()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var parent = new Control();
			var control1 = new Control() { ZIndex = 0 };
			var control2 = new Control() { ZIndex = 1 };
			parent.Controls.AddRange(new Control[] { control1, control2 });

			Assert.That(control1.ZIndex, Is.EqualTo(0));
			Assert.That(control2.ZIndex, Is.EqualTo(1));
			control2.SendToBack();
			Assert.That(control1.ZIndex, Is.EqualTo(0));
			Assert.That(control2.ZIndex, Is.EqualTo(0));
		});
	}

	[Test]
	public async Task BringToFrontWithSiblingChildControlHavingHigherZIndex()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var parent = new Control();
			var control1 = new Control();
			var control2 = new Control();
			var control3 = new Control() { ZIndex = 1 };
			control2.Controls.Add(control3);
			parent.Controls.AddRange(new Control[] { control1, control2 });

			control1.BringToFront();
			Assert.That(control1.ZIndex, Is.EqualTo(2));
		});
	}

	[Test]
	public async Task BringToFrontWithSiblingDescendentControlHavingHigherZIndex()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var parent = new Control();
			var control1 = new Control();
			var control2 = new Control();
			var control3 = new Control() { ZIndex = 1 };
			control2.Controls.Add(control3);
			var control4 = new Control() { ZIndex = 2 };
			control3.Controls.Add(control4);
			parent.Controls.AddRange(new Control[] { control1, control2 });

			control1.BringToFront();
			Assert.That(control1.ZIndex, Is.EqualTo(3));
		});
	}

	[Test]
	public async Task BringToFrontWithSiblingChildControlHavingLowerZIndex()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var parent = new Control();
			var control1 = new Control();
			var control2 = new Control() { ZIndex = 2 };
			var control3 = new Control() { ZIndex = 1 };
			control2.Controls.Add(control3);
			parent.Controls.AddRange(new Control[] { control1, control2 });

			control1.BringToFront();
			Assert.That(control1.ZIndex, Is.EqualTo(3));
		});
	}

	[Test]
	public async Task SendToBackWithSiblingChildControlHavingLowerZIndex()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var parent = new Control();
			var control1 = new Control() { ZIndex = 3 };
			var control2 = new Control() { ZIndex = 2 };
			var control3 = new Control();
			control2.Controls.Add(control3);
			parent.Controls.AddRange(new Control[] { control1, control2 });

			control1.SendToBack();
			Assert.That(control1.ZIndex, Is.EqualTo(0));
		});
	}

	[Test]
	public async Task ControlCollectionSetsNextTabIndexIfNoneSpecified()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var parent = new Control();
			var withNoTabIndex1 = new Control();
			var withNoTabIndex2 = new Control();
			var withSpecifiedTabIndex = new Control() { TabIndex = 5 };
			var withNoTabIndex3 = new Control();
			parent.Controls.AddRange(new Control[] { withNoTabIndex1, withNoTabIndex2, withSpecifiedTabIndex, withNoTabIndex3 });

			Assert.That(withNoTabIndex1.TabIndex, Is.EqualTo(0));
			Assert.That(withNoTabIndex2.TabIndex, Is.EqualTo(1));
			Assert.That(withSpecifiedTabIndex.TabIndex, Is.EqualTo(5));
			Assert.That(withNoTabIndex3.TabIndex, Is.EqualTo(6));
		});
	}

	[Test]
	public async Task RenderDoesNotCrashOnControlWithFontSetAndParentBeingSetToNull()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var button = new Button();
			button.Font = new Font(FontFamily.GenericSerif, 13);
			button.Click += Button_Click;
			form.Controls.Add(button);
			return form;
		});

		for (var i = 0; i < 100; i++)
		{
			await rendered.Find(".button").ClickAsync(new WebMouseEventArgs());
		}

		void Button_Click(object sender, EventArgs e)
		{
			var button = (Button)sender;
			var parent = button.Parent;
			button.Text = "Hello";
			button.Parent = null;
			button.Text = "Goodby";
			button.Parent = parent;
		}
	}

	[Test]
	public async Task UpdatePropertyForColorReturnsFalseForSameRgbValues()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var control = new ControlWithUpdatePropertyExposed<Color>();
			control.field = Color.FromArgb(Color.Black.R, Color.Black.G, Color.Black.B);
			Assert.That(control.CallUpdateProperty(Color.Black), Is.False);
			Assert.That(control.field, Is.EqualTo(Color.Black));
		});
	}

	[Test]
	public async Task QueuedRendererInvocationSkippedWhenControlHasBeenRemoved()
	{
		using var ctx = new WinzorTestContext();
		TextBox textBox;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			textBox = new TextBox();
			form.Controls.Add(textBox);
			var button = new Button();
			button.Click += (s, e) =>
			{
				textBox.InvokeRenderDispatcher(async () =>
				{
					await textBox.FindForm().CargoWiseClientServices.JSRuntime.InvokeAsync<object>("whatever", Array.Empty<object>());
				});
				textBox.Parent = null;
			};
			form.Controls.Add(button);
			return form;
		});

		await rendered.Find(".button").ClickAsync(new WebMouseEventArgs());

		Assert.That(ctx.Renderer.UnhandledException.IsCompleted, Is.False, () => ctx.Renderer.UnhandledException.Result.ToString());
	}

	[Test]
	public async Task ControlStyleStringIsFast()
	{
		const string cssVarToCheck = "line-height";

		using var dispatcher = new WinzorDispatcher(Mock.Of<IFormOpener>(), Mock.Of<IFormInstanceRegister>());
		await dispatcher.InvokeAsync(() =>
		{
			var stopwatch = new Stopwatch();
			var control = new ControlWithControlStyleStringExposed();

			// The first call has a significantly longer execution time, so we run it before measuring (and check that it's a valid value).
			Assert.That(control.StyleString, Does.Contain(cssVarToCheck), $"The control's StyleString does not contain a '{cssVarToCheck}' value. Please ensure that the StyleString is valid.");

			for (var i = 0; i < 1000; i++)
			{
				stopwatch.Start();
				_ = control.StyleString;
				stopwatch.Stop();
			}

			Assert.That(stopwatch.Elapsed, Is.LessThan(TimeSpan.FromMilliseconds(30)));
		});
	}
	
	[Test]
	public async Task SettingSameFontPreviouslyUsedIsFast()
	{
		using var dispatcher = new WinzorDispatcher(Mock.Of<IFormOpener>(), Mock.Of<IFormInstanceRegister>());
		await dispatcher.InvokeAsync(() =>
		{
			var control = new ControlWithControlStyleStringExposed();
			control.Font = new Font(FontFamily.GenericSerif, 10);
			var stopwatch = Stopwatch.StartNew();
			for (var i = 0; i < 1000; i++)
			{
				control = new ControlWithControlStyleStringExposed();
				control.Font = new Font(FontFamily.GenericSerif, 10);
			}
			Assert.That(stopwatch.Elapsed, Is.LessThan(TimeSpan.FromMilliseconds(40)));
		});
	}

	[Test]
	public async Task ControlRenderedOnceInSingleUnitOfWorkInitiatedByControl()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var button = new Button();
			button.Click += (s, e) =>
			{
				button.Text = "Foo";
				button.BackColor = Color.Red;
				button.Text = "Bar";
			};
			return button;
		});

		var buttonComponent = rendered.FindComponents<ControlProxyComponent>().Single(c => c.Instance.Control is Button);
		Assert.That(buttonComponent.RenderCount, Is.EqualTo(1));
		await rendered.Find("button").ClickAsync(new WebMouseEventArgs());
		Assert.That(rendered.Find("button").TextContent, Is.EqualTo("Bar"));
		Assert.That(buttonComponent.RenderCount, Is.EqualTo(2));
	}

	[Test]
	public async Task ContolRenderedOnceInSingleUnitOfWorkInitiatedByDifferentControl()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var button1 = new Button();
			button1.Text = "Click";
			var button2 = new Button();
			button1.Click += (s, e) =>
			{
				button2.Text = "Foo";
				button2.BackColor = Color.Red;
				button2.Text = "Bar";
			};
			form.Controls.Add(button1);
			form.Controls.Add(button2);
			return form;
		});

		var button2Component = rendered.FindComponents<ControlProxyComponent>().Single(c => c.Instance.Control is Button && c.Instance.Control.Text != "Click");
		Assert.That(button2Component.RenderCount, Is.EqualTo(1));
		await rendered.Find("button:contains('Click')").ClickAsync(new WebMouseEventArgs());
		Assert.That(button2Component.Find("button").TextContent, Is.EqualTo("Bar"));
		Assert.That(button2Component.RenderCount, Is.EqualTo(2));
	}

	[Test]
	public async Task ContolTreeRenderedOnceInSingleUnitOfWork()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var panel = new Panel();
			var button = new Button();
			button.Text = "Click";
			button.Click += (s, e) =>
			{
				panel.Controls.Add(new Label { Text = "Hello" });
				button.Text = "Foo";
			};
			panel.Controls.Add(button);
			form.Controls.Add(panel);
			return form;
		});

		var buttonComponent = rendered.FindComponents<ControlProxyComponent>().Single(c => c.Instance.Control is Button);
		Assert.That(buttonComponent.RenderCount, Is.EqualTo(1));
		await rendered.Find("button").ClickAsync(new WebMouseEventArgs());
		Assert.That(rendered.Find("button").TextContent, Is.EqualTo("Foo"));
		Assert.That(rendered.Markup, Does.Contain("Hello"));
		Assert.That(buttonComponent.RenderCount, Is.EqualTo(2));
	}

	[Test]
	public async Task ContolRenderedBeforeDialogIsShown()
	{
		using var ctx = new WinzorTestContext();
		var windowService = new Mock<IWindowService>();
		var closeFormTcs = new TaskCompletionSource();
		Task loadRequestTask = null;
		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				loadRequestTask = Task.Run(() =>
				{
					var form = ctx.WinzorDispatcher.FormInstanceRegister.Lookup(createWindowOptions.Uri);
					form.ReadyToRender(ctx.DefaultClientServices, Guid.NewGuid());
					closeFormTcs.Task.Wait();
					form.Dispose();
				});
			});
		var clientServices = MockCargoWiseClientServices.MakeMock(windowService: windowService.Object);
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var button = new Button();
			button.Click += (s, e) =>
			{
				button.Text = "Foo";
				new Form().ShowDialog();
				button.Text = "Bar";
			};
			form.Controls.Add(button);
			return form;
		}, clientServices);
		var clickTask = rendered.Find("button").ClickAsync(new WebMouseEventArgs());
		rendered.WaitForState(() => rendered.Find("button").TextContent == "Foo");
		closeFormTcs.SetResult();
		Assert.That(await loadRequestTask.WithTimeout(TimeSpan.FromSeconds(10)), Is.True);
		Assert.That(await clickTask.WithTimeout(TimeSpan.FromSeconds(10)), Is.True);
		Assert.That(rendered.Find("button").TextContent, Is.EqualTo("Bar"));
	}

	[Test]
	public async Task ControlKeyProcessingEventsArePropagatedToParent()
	{
		using var ctx = new WinzorTestContext();
		Mock<Control> parentMock = null;
		Control control = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			parentMock = GetControlWithMockedKeyEvents();
			var parent = parentMock.Object;
			control = new Control() { Width = 100, Height = 100, };
			parent.Controls.Add(control);
			return parent;
		});

		await rendered.KeyPressAsync(Keys.A, rendered.Find($"div[data-winzor-control-id=\"{control.WinzorControlId}\"]"));

		parentMock.Protected().Verify("ProcessCmdKey", Times.Once(), ItExpr.Ref<Message>.IsAny, ItExpr.IsAny<Keys>());
		parentMock.Protected().Verify("ProcessKeyPreview", Times.Exactly(3), ItExpr.Ref<Message>.IsAny); // KeyDown, WMChar, KeyUp
		parentMock.Protected().Verify("ProcessDialogKey", Times.Once(), ItExpr.IsAny<Keys>());
		parentMock.Protected().Verify("ProcessDialogChar", Times.Once(), ItExpr.IsAny<char>());
	}

	[Test]
	public async Task ControlEnterKeyPressNoLetterWritten()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(new TextBox() { Text = "test" });
			return form;
		});
		var textBox = rendered.Find("input");

		Assert.That(textBox.Attributes["value"].Value, Is.EqualTo("test"));

		await rendered.KeyPressAsync(Keys.Enter, textBox);
		Assert.That(textBox.Attributes["value"].Value, Is.EqualTo("test"));
	}

	[Test]
	public async Task ControlKeyDownEventsFired()
	{
		using var ctx = new WinzorTestContext();
		Mock<Control> controlMock = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			controlMock = GetControlWithMockedKeyEvents();
			return controlMock.Object;
		});

		var control = controlMock.Object;
		var keyDownFired = false;
		control.KeyDown += (sender, args) => keyDownFired = true;

		await rendered.KeyPressAsync(Keys.A, rendered.Find($"div[data-winzor-control-id=\"{control.WinzorControlId}\"]"));

		Assert.That(keyDownFired, Is.True);
		controlMock.Protected().Verify("ProcessCmdKey", Times.Once(), ItExpr.Ref<Message>.IsAny, ItExpr.IsAny<Keys>());
		controlMock.Protected().Verify("IsInputKey", Times.Once(), ItExpr.IsAny<Keys>());
		controlMock.Protected().Verify("ProcessDialogKey", Times.Once(), ItExpr.IsAny<Keys>());
	}

	[Test]
	public async Task ControlKeyDownCharEventsFired()
	{
		using var ctx = new WinzorTestContext();
		Mock<Control> controlMock = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			controlMock = GetControlWithMockedKeyEvents();
			return controlMock.Object;
		});

		var control = controlMock.Object;
		var keyPressFired = false;
		control.KeyPress += (sender, args) => keyPressFired = true;

		await rendered.KeyPressAsync(Keys.A, rendered.Find($"div[data-winzor-control-id=\"{control.WinzorControlId}\"]"));

		Assert.That(keyPressFired, Is.True);
		controlMock.Protected().Verify("IsInputChar", Times.Once(), ItExpr.IsAny<char>());
		controlMock.Protected().Verify("ProcessDialogChar", Times.Once(), ItExpr.IsAny<char>());
	}

	[Test]
	public async Task ImeKeyDownCharEventsNotFired()
	{
		using var ctx = new WinzorTestContext();
		Mock<Control> controlMock = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			controlMock = GetControlWithMockedKeyEvents();
			return controlMock.Object;
		});

		var control = controlMock.Object;
		var keyPressFired = false;
		control.KeyPress += (sender, args) => keyPressFired = true;

		await rendered.KeyPressAsync(Keys.ProcessKey, rendered.Find($"div[data-winzor-control-id=\"{control.WinzorControlId}\"]"));

		Assert.That(keyPressFired, Is.False);
		controlMock.Protected().Verify("IsInputChar", Times.Never(), ItExpr.IsAny<char>());
		controlMock.Protected().Verify("ProcessDialogChar", Times.Never(), ItExpr.IsAny<char>());
	}

	[Test]
	public async Task ControlKeyDownCharSupressedEventsNotFired()
	{
		using var ctx = new WinzorTestContext();
		Mock<Control> controlMock = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			controlMock = GetControlWithMockedKeyEvents();
			return controlMock.Object;
		});

		var control = controlMock.Object;
		var keyPressFired = false;
		control.KeyDown += (sender, args) => args.SuppressKeyPress = true;
		control.KeyPress += (sender, args) => keyPressFired = true;

		await rendered.KeyPressAsync(Keys.A, rendered.Find($"div[data-winzor-control-id=\"{control.WinzorControlId}\"]"));

		Assert.That(keyPressFired, Is.False);
		controlMock.Protected().Verify("IsInputChar", Times.Never(), ItExpr.IsAny<char>());
		controlMock.Protected().Verify("ProcessDialogChar", Times.Never(), ItExpr.IsAny<char>());
	}

	[Test]
	public async Task ControlKeyUpEventsFired()
	{
		using var ctx = new WinzorTestContext();
		Mock<Control> controlMock = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			controlMock = GetControlWithMockedKeyEvents();
			return controlMock.Object;
		});

		var control = controlMock.Object;
		var keyUpFired = false;
		control.KeyUp += (sender, args) => keyUpFired = true;

		await rendered.KeyPressAsync(Keys.A, rendered.Find($"div[data-winzor-control-id=\"{control.WinzorControlId}\"]"));

		Assert.That(keyUpFired, Is.True);
	}

	Mock<Control> GetControlWithMockedKeyEvents()
	{
		var mock = new Mock<Control>() { CallBase = true };
		mock.Protected().Setup<bool>("ProcessCmdKey", ItExpr.Ref<Message>.IsAny, ItExpr.IsAny<Keys>()).Returns(false);
		mock.Protected().Setup<bool>("ProcessKeyPreview", ItExpr.Ref<Message>.IsAny).Returns(false);
		mock.Protected().Setup<bool>("ProcessDialogKey", ItExpr.IsAny<Keys>()).Returns(false);
		mock.Protected().Setup<bool>("IsInputKey", ItExpr.IsAny<Keys>()).Returns(false);
		mock.Protected().Setup<bool>("IsInputChar", ItExpr.IsAny<char>()).Returns(false);
		mock.Protected().Setup<bool>("ProcessDialogChar", ItExpr.IsAny<char>()).Returns(false);
		mock.Object.Width = 100;
		mock.Object.Height = 100;
		return mock;
	}

	[Test]
	public async Task TestSetSizeFromClientSize()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var controlNoMinimum = new Control()
			{
				Name = "ControlNoMinimum",
				Top = 50,
				Size = new Size(100, 100),
			};
			var controlWithMinimum = new Control()
			{
				Name = "ControlWithMinimum",
				Top = 150,
				Size = new Size(100, 100),
				MinimumSize = new Size(100, 100)
			};

			var button = new Button();

			button.Click += (s, e) =>
			{
				controlNoMinimum.ClientSize = new Size(50, 50);
				controlWithMinimum.ClientSize = new Size(50, 50);
			};

			form.Controls.Add(button);
			form.Controls.Add(controlNoMinimum);
			form.Controls.Add(controlWithMinimum);
			return form;
		});

		var controlNoMinimum = rendered.FindComponents<ControlProxyComponent>().Single(c => c.Instance.Control.Name == "ControlNoMinimum").Instance.Control;
		var controlWithMinimum = rendered.FindComponents<ControlProxyComponent>().Single(c => c.Instance.Control.Name == "ControlWithMinimum").Instance.Control;

		Assert.That(controlNoMinimum.Size, Is.EqualTo(new Size(100, 100)));
		Assert.That(controlNoMinimum.ClientSize, Is.EqualTo(new Size(100, 100)));
		Assert.That(controlWithMinimum.Size, Is.EqualTo(new Size(100, 100)));
		Assert.That(controlWithMinimum.ClientSize, Is.EqualTo(new Size(100, 100)));
		await rendered.Find("button").ClickAsync(new WebMouseEventArgs());
		Assert.That(controlNoMinimum.Size, Is.EqualTo(new Size(50, 50)));
		Assert.That(controlNoMinimum.ClientSize, Is.EqualTo(new Size(50, 50)));
		Assert.That(controlWithMinimum.Size, Is.EqualTo(new Size(100, 100)));
		Assert.That(controlWithMinimum.ClientSize, Is.EqualTo(new Size(50, 50)));
	}

	[Test]
	public async Task TestSetClientSizeFromSize()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var control = new Control()
			{
				Name = "Control",
				Top = 50,
				Size = new Size(1, 1),
			};

			var button = new Button();

			button.Click += (s, e) =>
			{
				control.Size = new Size(50, 50);
			};

			form.Controls.Add(button);
			form.Controls.Add(control);
			return form;
		});

		var control = rendered.FindComponents<ControlProxyComponent>().Single(c => c.Instance.Control.Name == "Control").Instance.Control;

		Assert.That(control.Size, Is.EqualTo(new Size(1, 1)));
		Assert.That(control.ClientSize, Is.EqualTo(new Size(1, 1)));
		await rendered.Find("button").ClickAsync(new WebMouseEventArgs());
		Assert.That(control.Size, Is.EqualTo(new Size(50, 50)));
		Assert.That(control.ClientSize, Is.EqualTo(new Size(50, 50)));
	}

	[Test]
	public async Task InvokeWinzorDispatcherAsync_ExceptionThrown_RaisesThreadExceptionOnDispatcher()
	{
		using var ctx = new WinzorTestContext();
		Control control = null;
		var threadExceptionThrown = new TaskCompletionSource<Exception>();
		ctx.ThreadExceptionExceptionRaised += ex =>
		{
			threadExceptionThrown.SetResult(ex);
			return true;
		};

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			control = new Control { Size = new Size(1, 1) };
			return control;
		});

		await control.InvokeWinzorDispatcherAsync(() =>
		{
			control.InvokeRenderDispatcher(() => throw new InvalidOperationException("Exception thrown from InvokeWinzorDispatcherAsync"));
		});

		Assert.That(await threadExceptionThrown.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		var exception = await threadExceptionThrown.Task;
		Assert.That(exception.Message, Is.EqualTo("Exception thrown from InvokeWinzorDispatcherAsync"));
	}

	[Test]
	public async Task OnAfterRenderAsync_ExceptionThrown_RaisesThreadExceptionOnDispatcher()
	{
		using var ctx = new WinzorTestContext();
		var threadExceptionThrown = new TaskCompletionSource<Exception>();
		ctx.ThreadExceptionExceptionRaised += ex =>
		{
			threadExceptionThrown.SetResult(ex);
			return true;
		};

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			return new ControlWithAsyncExceptions { Size = new Size(1, 1), ThrowOnAfterRenderAsync = true };
		});

		Assert.That(await threadExceptionThrown.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		var exception = await threadExceptionThrown.Task;
		Assert.That(exception.Message, Is.EqualTo("Exception thrown from OnAfterRenderAsync"));
	}

	[Test]
	public async Task OnInitializedAsync_ExceptionThrown_RaisesThreadExceptionOnDispatcher()
	{
		using var ctx = new WinzorTestContext();
		var threadExceptionThrown = new TaskCompletionSource<Exception>();
		ctx.ThreadExceptionExceptionRaised += ex =>
		{
			threadExceptionThrown.SetResult(ex);
			return true;
		};

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			return new ControlWithAsyncExceptions { Size = new Size(1, 1), ThrowOnInitializedAsync = true };
		});

		Assert.That(await threadExceptionThrown.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		var exception = await threadExceptionThrown.Task;
		Assert.That(exception.Message, Is.EqualTo("Exception thrown from OnInitializedAsync"));
	}

	[Test]
	public async Task OnBeforeRender_ExceptionThrown_RaisesThreadExceptionOnDispatcher()
	{
		using var ctx = new WinzorTestContext();
		var threadExceptionThrown = new TaskCompletionSource<Exception>();
		ctx.ThreadExceptionExceptionRaised += ex =>
		{
			threadExceptionThrown.SetResult(ex);
			return true;
		};

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			return new ControlWithAsyncExceptions { Size = new Size(1, 1), ThrowOnBeforeRender = true };
		});

		Assert.That(await threadExceptionThrown.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		var exception = await threadExceptionThrown.Task;
		Assert.That(exception.Message, Is.EqualTo("Exception thrown from OnBeforeRender"));
	}

	[Test]
	public async Task NegativeSizeShouldBeAllowedBeforeRender()
	{
		using var ctx = new WinzorTestContext();
		Control control = null;
		await ctx.WinzorDispatcher.InvokeAsync(() => control = new Control { Width = int.MinValue, Height = int.MinValue });

		Assert.That(control.Bounds.Width, Is.EqualTo(int.MinValue));
		Assert.That(control.Bounds.Height, Is.EqualTo(int.MinValue));
		Assert.That(control.ClientSize.Height, Is.EqualTo(int.MinValue));
		Assert.That(control.ClientSize.Width, Is.EqualTo(int.MinValue));
	}

	[Test]
	public async Task NegativeSizeShouldBeZeroAfterRender()
	{
		using var ctx = new WinzorTestContext();
		Control control = null;
		await ctx.RenderControlOnFormAsync(() => control = new Control { Width = int.MinValue, Height = int.MinValue });

		Assert.That(control.Bounds.Width, Is.EqualTo(0));
		Assert.That(control.Bounds.Height, Is.EqualTo(0));
		Assert.That(control.ClientSize.Height, Is.EqualTo(0));
		Assert.That(control.ClientSize.Width, Is.EqualTo(0));
	}

	[Test]
	public async Task RenderInPortalShouldWrapInPortal()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var parent = new Panel();
			var portal = new PortalControl();
			parent.Controls.Add(portal);
			form.Controls.Add(parent);
			return form;
		});

		var parentElement = rendered.Find(".PortalControl").Parent.Parent as IHtmlElement;
		Assert.That(parentElement.GetAttribute("class"), Does.Contain("form"));
	}

	[Test]
	public async Task TestAssignSiteNullRemainsNull()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			using var c = new Control();
			Assert.That(c.Site, Is.Null);

			c.Site = null;
			Assert.That(c.Site, Is.Null);
		});
	}

	[Test]
	public async Task RenderInPortalDoesNotParticipateInLayout()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var portal = new PortalControl();
			Assert.That(portal.ParticipatesInLayout, Is.False);
		});
	}

	[Test]
	public async Task ControlEventAttributeMouseDownAddedToComponent()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new ControlWithEventAttribute(EventAttribute.MouseDown));

		var attributes = rendered.Find(".ControlWithEventAttribute").Attributes.Select(a => a.Name);
		Assert.That(attributes, Does.Contain("blazor:onmousedown"));
		Assert.That(attributes, Does.Contain("blazor:onmousedown:stoppropagation"));
	}

	[Test]
	public async Task ControlEventAttributeMouseUpAddedToComponent()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new ControlWithEventAttribute(EventAttribute.MouseUp));

		var attributes = rendered.Find(".ControlWithEventAttribute").Attributes.Select(a => a.Name);
		Assert.That(attributes, Does.Contain("blazor:onmouseup"));
		Assert.That(attributes, Does.Contain("blazor:onmouseup:stoppropagation"));
	}

	[Test]
	public async Task ControlEventAttributeClickAddedToComponent()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new ControlWithEventAttribute(EventAttribute.Click));

		var attributes = rendered.Find(".ControlWithEventAttribute").Attributes.Select(a => a.Name);
		Assert.That(attributes, Does.Contain("blazor:onclick"));
		Assert.That(attributes, Does.Contain("blazor:onclick:stoppropagation"));
	}

	[Test]
	public async Task ControlEventAttributeContextMenuAddedToComponent()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new ControlWithEventAttribute(EventAttribute.ContextMenu));

		var attributes = rendered.Find(".ControlWithEventAttribute").Attributes.Select(a => a.Name);
		Assert.That(attributes, Does.Contain("blazor:oncontextmenu"));
		Assert.That(attributes, Does.Contain("blazor:oncontextmenu:stoppropagation"));
		Assert.That(attributes, Does.Contain("blazor:oncontextmenu:preventdefault"));
	}

	[Test]
	public async Task ControlEventAttributeNoneHasFocusEventHandlerAddedToComponent()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new ControlWithEventAttribute(EventAttribute.None));

		var eventAttributes = rendered.Find(".ControlWithEventAttribute").Attributes.Select(a => a.Name);
		Assert.That(eventAttributes, Does.Contain("blazor:onwinzorfocusin"));
		Assert.That(eventAttributes, Does.Contain("blazor:onwinzorfocusin:stoppropagation"));
		Assert.That(eventAttributes, Does.Contain("blazor:onwinzorfocusout"));
		Assert.That(eventAttributes, Does.Contain("blazor:onwinzorfocusout:stoppropagation"));
	}

	[Test, WithPlaywrightPage]
	public async Task ShowContextMenuOnRightClick()
	{
		await using var ctx = new InMemoryTestServerContext();
		var tcs = new TaskCompletionSource();
		Control control = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 1000, Height = 600 };
			control = new ControlWithEventAttribute(EventAttribute.ContextMenu) { Size = new Size(500, 300), Name = "ControlWithMenuItems" };

			control.ContextMenuStrip = new ContextMenuStrip();
			control.ContextMenuStrip.Items.Add(new ToolStripMenuItem { Text = "1st Menu Item" });
			control.ContextMenuStrip.Items.Add(new ToolStripMenuItem { Text = "2st Menu Item" });
			control.ContextMenuStrip.Opened += (object sender, EventArgs e) => tcs.SetResult();

			form.Controls.Add(control);

			return form;
		});

		var div = await page.WaitForSelectorAsync("[data-name=ControlWithMenuItems]");
		await div.ClickAsync(new ElementHandleClickOptions { Button = MouseButton.Right });
		Assert.That(await tcs.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
	}

	[Test]
	public async Task ControlContextMenuShowIfContextMenuSet()
	{
		using var ctx = new WinzorTestContext();
		Control control = null;
		var tcs = new TaskCompletionSource();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			control = new ControlWithEventAttribute(EventAttribute.ContextMenu) { Width = 100, Height = 100, Name = "ControlWithMenu" };
			control.ContextMenuStrip = new ContextMenuStrip();
			control.ContextMenuStrip.Opened += (object sender, EventArgs e) => tcs.SetResult();
			return control;
		});

		Assert.That(control.ContextMenuStrip.Visible, Is.False);
		await rendered.Find("[data-name=ControlWithMenu]").ContextMenuAsync(new WebMouseEventArgs { Button = 2, Type = "contextmenu" });
		Assert.That(await tcs.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
	}

	[Test]
	public async Task ControlContextMenuPropogatedToParentIfNoContextMenuSet()
	{
		using var ctx = new WinzorTestContext();
		Control parent = null;
		var tcs = new TaskCompletionSource();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			parent = new Control() { Width = 100, Height = 100 };
			parent.ContextMenuStrip = new ContextMenuStrip();
			parent.ContextMenuStrip.Opened += (object sender, EventArgs e) => tcs.SetResult();
			parent.Controls.Add(new ControlWithEventAttribute(EventAttribute.ContextMenu) { Width = 100, Height = 100, Name = "ChildControl" });
			return parent;
		});

		Assert.That(parent.ContextMenuStrip.Visible, Is.False);
		await rendered.Find("[data-name=ChildControl]").ContextMenuAsync(new WebMouseEventArgs { Button = 2, Type = "contextmenu" });
		Assert.That(await tcs.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
	}

	[Test]
	[SuppressMessage("Usage", "VSTHRD103:Call async methods when in an async method")]
	public async Task CallbackQueuedWhileControlBeingDisposedDoesNotExecute()
	{
		var tcs = new TaskCompletionSource();
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var button = new Button();
			button.Click += Button_Click;
			return button;
		});

		var firstClickTask = rendered.Find("button").ClickAsync(new WebMouseEventArgs());
		tcs.SetResult();
		var secondClickTask = rendered.Find("button").ClickAsync(new WebMouseEventArgs());
		await firstClickTask;
		await secondClickTask;

		void Button_Click(object sender, EventArgs e)
		{
			tcs.Task.Wait();
			var button = (Button)sender;
			var form = button.Parent;
			button.Parent.Controls.Remove(button);
			button.Dispose();
			button = new Button();
			form.Controls.Add(button);
			button.Click += Button_Click;
		}
	}

	[Test]
	public async Task TaskCancelledExceptionsNotReported()
	{
		var tcs = new TaskCompletionSource();
		using var ctx = new WinzorTestContext();
		Exception developerException = null;
		ctx.DeveloperExceptionRaised += ex =>
		{
			developerException = ex;
			return true;
		};

		using var cts = new CancellationTokenSource();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var control = new Label();
			control.RegisterAfterRenderAction(async () => await Task.Delay(-1, cts.Token));
			return control;
		});
		await cts.CancelAsync();
		Assert.That(() => developerException, Is.Null);
	}

	[Test]
	public async Task InvokeFromAnotherWinzorDispatcherShouldNotBlockFirstWinzorDispatcher()
	{
		using var ctx = new WinzorTestContext();
		using var winzorDispatcher = new WinzorDispatcher(Mock.Of<IFormOpener>(), Mock.Of<IFormInstanceRegister>(), "FormBackgroundThread", true);
		Form form = null;

		await winzorDispatcher.InvokeAsync(() =>
		{
			form = new Form();
		});

		var tcs = new TaskCompletionSource<bool>();
		form.Invoke(() => tcs.SetResult(true));
		Assert.That(await tcs.Task, Is.EqualTo(true));

		tcs = new TaskCompletionSource<bool>();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			form.Invoke(() => tcs.SetResult(true));
		});
		Assert.That(await tcs.Task, Is.EqualTo(true));

		tcs = new TaskCompletionSource<bool>();
		form.Invoke((string dummyArg) => tcs.SetResult(true), new[] { "dummy" });
		Assert.That(await tcs.Task, Is.EqualTo(true));

		tcs = new TaskCompletionSource<bool>();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			form.Invoke((string dummyArg) => tcs.SetResult(true), new[] { "dummy" });
		});
		Assert.That(await tcs.Task, Is.EqualTo(true));
	}

	[Test]
	public async Task OnBeforeRenderCalledOnControlOnAnotherForm()
	{
		using var ctx = new WinzorTestContext();
		ControlWithOnBeforeRender controlOnSecondForm = null;
		Form form = null;
		await ctx.RenderControlOnFormAsync(() => controlOnSecondForm = new ControlWithOnBeforeRender());
		await ctx.RenderFormAsync(() => form = new Form());
		controlOnSecondForm.OnBeforeRenderCalled = false;
		await form.InvokeWinzorDispatcherAsync(() => controlOnSecondForm.Text = "Updated");
		Assert.That(controlOnSecondForm.OnBeforeRenderCalled, Is.True);
	}

	[Test]
	public async Task TestAfterRenderActionsNotExecutedWhenControlUnrendered()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new ControlForTest());
		var form = rendered.GetForm();
		var control = rendered.GetControl<ControlForTest>();

		var afterRenderActionFired = new TaskCompletionSource();
		control.RegisterAfterRenderAction(() =>
		{
			afterRenderActionFired.SetResult();
			return Task.CompletedTask;
		});

		await form.InvokeWinzorDispatcherAsync(() => form.Controls.Remove(control));
		Assert.That(await afterRenderActionFired.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.False);
	}

	[Test]
	public async Task TestOnFocusOutAsync()
	{
		using var ctx = new WinzorTestContext();
		Form form = null;
		ControlWithEventHandlersExposed control = null;
		ControlWithEventHandlersExposed relatedControl = null;
		var targetEventFired = false;

		await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			form.Controls.Add(control = new ControlWithEventHandlersExposed());
			form.Controls.Add(relatedControl = new ControlWithEventHandlersExposed());
			control.LostFocus += (s, e) => targetEventFired = true;
			return form;
		});

		Assert.That(control.Focused, Is.True);

		await control.OnFocusOutAsync(new WinzorFocusOutEventArgs() { TargetWinzorControlId = null, RelatedTargetWinzorControlId = control.WinzorControlId });
		Assert.That(targetEventFired, Is.False);
		Assert.That(control.Focused, Is.True);

		await control.OnFocusOutAsync(new WinzorFocusOutEventArgs() { TargetWinzorControlId = control.WinzorControlId, RelatedTargetWinzorControlId = control.WinzorControlId });
		Assert.That(targetEventFired, Is.False);
		Assert.That(control.Focused, Is.True);

		await control.OnFocusOutAsync(new WinzorFocusOutEventArgs() { TargetWinzorControlId = control.WinzorControlId, RelatedTargetWinzorControlId = control.WinzorControlId, InitiatedFromServer = true });
		Assert.That(targetEventFired, Is.False);
		Assert.That(control.Focused, Is.True);

		await control.OnFocusOutAsync(new WinzorFocusOutEventArgs() { TargetWinzorControlId = control.WinzorControlId, RelatedTargetWinzorControlId = relatedControl.WinzorControlId });
		Assert.That(targetEventFired, Is.True);
		Assert.That(control.Focused, Is.False);

		targetEventFired = false;
		control.Focus();
		await control.OnFocusOutAsync(new WinzorFocusOutEventArgs() { TargetWinzorControlId = control.WinzorControlId, RelatedTargetWinzorControlId = null });
		Assert.That(targetEventFired, Is.True);
		Assert.That(control.Focused, Is.False);
	}

	[Test, WithPlaywrightPage]
	public async Task ShouldNotTriggerFocusOutWhenDocumentHasNoFocus()
	{
		await using var ctx = new InMemoryTestServerContext();
		var lostFocusTriggered = false;

		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var textBox = new TextBox();
			textBox.LostFocus += (s, e) => lostFocusTriggered = true;
			return textBox;
		});

		Assert.That(await page.EvaluateAsync<bool>("document.hasFocus()"), Is.True);
		var textBox = page.Locator(".textbox");
		await textBox.DispatchEventAsync("focusout");
		Assert.That(() => lostFocusTriggered, Is.True.After(1000, 50));

		lostFocusTriggered = false;
		await page.EvaluateAsync("() => document.hasFocus = () => false;");
		Assert.That(await page.EvaluateAsync<bool>("document.hasFocus()"), Is.False);
		await textBox.DispatchEventAsync("focusout");
		await Task.Delay(100);
		Assert.That(lostFocusTriggered, Is.False);
	}

	[Test]
	public async Task TestFocusWithoutForm()
	{
		using var ctx = new WinzorTestContext();
		Control control1 = null;
		Control control2 = null;

		await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			control1 = new Control();
			form.Controls.Add(control1);
			return form;
		});

		control1.Focus();

		Assert.That(control1.Focused, Is.True);

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			control2 = new Control();
		});

		control2.Focus();

		Assert.That(control2.Focused, Is.False);
	}

	[Test]
	public async Task InvokeRenderDispatcherWithExceptionHandledAfterWinzorDispatcherDisposed()
	{
		Task invokeTask;
		var ctxDisposedTcs = new TaskCompletionSource();
		using (var ctx = new WinzorTestContext())
		{
			Control control = null;
			var rendered = await ctx.RenderControlOnFormAsync(() =>
			{
				control = new Control { Size = new Size(1, 1) };
				return control;
			});

			invokeTask = control.InvokeWinzorDispatcherAsync(() =>
			{
				control.InvokeRenderDispatcher(async () =>
				{
					await ctxDisposedTcs.Task;
					throw new InvalidOperationException("Exception thrown from InvokeWinzorDispatcherAsync");
				});
			});
		}
		ctxDisposedTcs.SetResult();
		await invokeTask.WithTimeout(TimeSpan.FromMilliseconds(100));
	}

	[Test]
	public async Task GetChildrenIsFast()
	{
		using var dispatcher = new WinzorDispatcher(Mock.Of<IFormOpener>(), Mock.Of<IFormInstanceRegister>());
		await dispatcher.InvokeAsync(() =>
		{
			var numChildren = 1000;
			var control = new ContainerControl();
			for (var i = 0; i < numChildren; i++)
			{
				control.Controls.Add(new Control());
			}
			var stopwatch = Stopwatch.StartNew();
			for (var i = 0; i < 1000; i++)
			{
				if (control.Children.Count != numChildren)
				{
					Assert.Fail($"Unexpected control.Children.Count {control.Children.Count}, expected \"{numChildren}\"");
				}
			}
			Assert.That(stopwatch.Elapsed, Is.LessThan(TimeSpan.FromMilliseconds(10)));
		});
	}

	[Test]
	public async Task RecreateControlShouldTriggerHandleCreated()
	{
		using var ctx = new WinzorTestContext();
		Control control = null;
		var handleCreatedCount = 0;
		await ctx.RenderControlOnFormAsync(() =>
		{
			control = new Control();
			control.HandleCreated += (sender, e) => handleCreatedCount += 1;

			return control;
		});

		Assert.That(handleCreatedCount, Is.EqualTo(1));

		await control.InvokeWinzorDispatcherAsync(() => control.RecreateHandle());
		Assert.That(() => handleCreatedCount, Is.EqualTo(2).After(3000, 500));
	}

	[Test]
	public async Task GetHandleShouldCreateHandle()
	{
		using var ctx = new WinzorTestContext();
		Control control = null;
		await ctx.WinzorDispatcher.InvokeAsync(() => control = new Control());

		Assert.That(control.IsHandleCreated, Is.False);

		IntPtr? handle = null;
		await ctx.WinzorDispatcher.InvokeAsync(() => handle = control.Handle);
		Assert.That(control.IsHandleCreated, Is.True);
		// Although the Handle was created, Winzor should return IntPtr.Zero as we do not implement the handle value.
		Assert.That(handle, Is.EqualTo(IntPtr.Zero));
	}

	[Test]
	public async Task RecreateControlShouldSetFocusWhenItWasFocused()
	{
		using var ctx = new WinzorTestContext();
		Control control1 = null;
		Control control2 = null;
		Form form = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			control1 = new Control();
			control2 = new Control();
			form.Controls.Add(control1);
			form.Controls.Add(control2);

			return form;
		});

		await form.InvokeWinzorDispatcherAsync(() => control1.Focus());
		Assert.That(control1.Focused, Is.True);
		Assert.That(control2.Focused, Is.False);

		await form.InvokeWinzorDispatcherAsync(() => control2.RecreateHandle());
		Assert.That(control1.Focused, Is.True);
		Assert.That(control2.Focused, Is.False);

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			control2.Focus();
			control2.RecreateHandle();
		});

		Assert.That(control1.Focused, Is.False);
		Assert.That(control2.Focused, Is.True);
	}

	[Test]
	public async Task InvokeRenderDispatcherShouldRegisterAsAfterRenderActionForPreviouslyShownControl()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new ControlForTest());

		var form = rendered.GetForm();
		var control = rendered.GetControl<ControlForTest>();
		Assert.That(control.Proxy, Is.Not.Null);
		Assert.That(control.ElementReference, Is.Not.EqualTo(default(ElementReference)));

		await form.InvokeWinzorDispatcherAsync(() => form.Controls.Remove(control));

		Assert.That(control.Proxy, Is.Null);
		Assert.That(control.ElementReference, Is.EqualTo(default(ElementReference)));

		var invokeRenderDispatcherAction = new TaskCompletionSource();
		await control.InvokeWinzorDispatcherAsync(() =>
		{
			control.InvokeRenderDispatcher(() =>
			{
				invokeRenderDispatcherAction.SetResult();
				return Task.CompletedTask;
			});
		});
		Assert.That(invokeRenderDispatcherAction.Task.IsCompleted, Is.EqualTo(false));

		await form.InvokeWinzorDispatcherAsync(() => form.Controls.Add(control));
		Assert.That(invokeRenderDispatcherAction.Task.IsCompleted, Is.EqualTo(true));
	}

	[Test]
	public async Task ControlShouldNotInvokeEventHandlerWhenDisabled_OnClick()
	{
		await ControlAssert.EventHandlerOnlyInvokedWhenEnabled<ControlWithEventHandlersExposed, EventArgs>("OnClick", async (c) => await c.OnClickAsync(new WebMouseEventArgs()));
	}

	[Test]
	public async Task ControlShouldNotInvokeEventHandlerWhenDisabled_OnDragDrop()
	{
		await ControlAssert.EventHandlerOnlyInvokedWhenEnabled<ControlWithEventHandlersExposed, DragEventArgs>("OnDragDrop", async (c) => await c.OnDragDropAsync(new WinzorDragEventArgs() { ControlID = c.WinzorControlId }));
	}

	[Test]
	public async Task ControlShouldNotInvokeEventHandlerWhenDisabled_OnWinzorDragEnd()
	{
		await ControlAssert.EventHandlerOnlyInvokedWhenEnabled<ControlWithEventHandlersExposed, DragEventArgs>("OnDragEnd", async (c) => await c.OnWinzorDragEndAsync(new WinzorDragEndEventArgs()));
	}

	[Test]
	public async Task ControlShouldNotInvokeEventHandlerWhenDisabled_OnDragLeave()
	{
		await ControlAssert.EventHandlerOnlyInvokedWhenEnabled<ControlWithEventHandlersExposed, EventArgs>("OnDragLeave", async (c) => await c.OnDragLeaveAsync(new WebDragEventArgs()));
	}

	[Test]
	public async Task ControlShouldNotInvokeEventHandlerWhenDisabled_OnDragOver()
	{
		await ControlAssert.EventHandlerOnlyInvokedWhenEnabled<ControlWithEventHandlersExposed, DragEventArgs>("OnDragOver", async (c) => await c.OnDragOverAsync(new WebDragEventArgs()));
	}

	[Test]
	public async Task ControlShouldNotInvokeEventHandlerWhenDisabled_OnDragStart()
	{
		await ControlAssert.EventHandlerOnlyInvokedWhenEnabled<ControlWithEventHandlersExposed, DragEventArgs>("OnDragStart", async (c) => await c.OnDragStartAsync(new WebDragEventArgs()));
	}

	[Test]
	public async Task ControlShouldNotInvokeEventHandlerWhenDisabled_OnMouseDown()
	{
		await ControlAssert.EventHandlerOnlyInvokedWhenEnabled<ControlWithEventHandlersExposed, MouseEventArgs>("OnMouseDown", async (c) => await c.OnMouseDownAsync(new WebMouseEventArgs()));
	}

	[Test]
	public async Task ControlShouldNotInvokeEventHandlerWhenDisabled_OnMouseEnter()
	{
		await ControlAssert.EventHandlerOnlyInvokedWhenEnabled<ControlWithEventHandlersExposed, EventArgs>("OnMouseEnter", async (c) => await c.OnMouseEnterAsync());
	}

	[Test]
	public async Task ControlShouldNotInvokeEventHandlerWhenDisabled_OnMouseLeave()
	{
		await ControlAssert.EventHandlerOnlyInvokedWhenEnabled<ControlWithEventHandlersExposed, EventArgs>("OnMouseLeave", async (c) => await c.OnMouseLeaveAsync());
	}

	[Test]
	public async Task ControlShouldNotInvokeEventHandlerWhenDisabled_OnMouseOver()
	{
		await ControlAssert.EventHandlerOnlyInvokedWhenEnabled<ControlWithEventHandlersExposed, MouseEventArgs>("OnMouseOver", async (c) => await c.OnMouseOverAsync(new WebMouseEventArgs()));
	}

	[Test]
	public async Task ControlShouldNotInvokeEventHandlerWhenDisabled_OnMouseUp()
	{
		await ControlAssert.EventHandlerOnlyInvokedWhenEnabled<ControlWithEventHandlersExposed, MouseEventArgs>("OnMouseUp", async (c) => await c.OnMouseUpAsync(new WebMouseEventArgs()));
	}

	[Test]
	public async Task ControlShouldNotInvokeEventHandlerWhenDisabled_FocusInternal()
	{
		await ControlAssert.EventHandlerOnlyInvokedWhenEnabled<ControlWithEventHandlersExposed>("FocusInternal", async (c) => await c.OnFocusInAsync(new WinzorFocusInEventArgs()), true);
	}

	[Test]
	public async Task ControlShouldNotInvokeEventHandlerWhenDisabled_FocusOutInternal()
	{
		await ControlAssert.EventHandlerOnlyInvokedWhenEnabled<ControlWithEventHandlersExposed>("FocusOutInternal", async (c) => await c.OnFocusOutAsync(new WinzorFocusOutEventArgs() { TargetWinzorControlId = c.WinzorControlId }), ItExpr.IsAny<string>(), ItExpr.IsNull<string>());
	}

	[Test, WithPlaywrightPage]
	public async Task TestInitialFocusedElementIsDecidedByServerSide()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var leftButton = new Button { Text = "left button", Dock = DockStyle.Left };
			var rightButton = new Button { Text = "right button", Dock = DockStyle.Right };

			var form = new Form();
			form.Controls.Add(leftButton);
			form.Controls.Add(rightButton);
			form.ActiveControl = rightButton;
			return form;
		});

		// simulate focusing the first focusable element automatically in browser
		var leftButtonElement = page.Locator("button", new PageLocatorOptions { HasText = "left button" });
		await leftButtonElement.DispatchEventAsync("focusin");

		var rightButtonElement = page.Locator("button", new PageLocatorOptions { HasText = "right button" });
		Assert.That(async () => await rightButtonElement.EvaluateAsync<bool>("element => document.activeElement === element"), Is.True.After(1000, 100));
	}

	[Test]
	public async Task InvokeWinzorDispatcherFromInvokeRenderDispatcherThrows()
	{
		using var ctx = new WinzorTestContext();
		var threadExceptionThrown = new TaskCompletionSource<Exception>();
		ctx.ThreadExceptionExceptionRaised += ex =>
		{
			threadExceptionThrown.SetResult(ex);
			return true;
		};
		Form form = null;
		var rendered = await ctx.RenderFormAsync(() => form = new Form());
		await form.InvokeWinzorDispatcherAsync(() =>
			form.InvokeRenderDispatcher(() => form.InvokeWinzorDispatcherAsync(() => { })));
		Assert.That(await threadExceptionThrown.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		var exception = await threadExceptionThrown.Task;
		Assert.That(exception.Message, Does.Contain("Invalid call to InvokeWinzorDispatcher from InvokeRenderDispatcher or RegisterAfterRenderAction"));
	}

	[Test]
	public async Task InvokeWinzorDispatcherFromRegisterAfterRenderActionThrows()
	{
		using var ctx = new WinzorTestContext();
		var threadExceptionThrown = new TaskCompletionSource<Exception>();
		ctx.ThreadExceptionExceptionRaised += ex =>
		{
			threadExceptionThrown.SetResult(ex);
			return true;
		};
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.RegisterAfterRenderAction(() => form.InvokeWinzorDispatcherAsync(() => { }));
			return form;
		});
		Assert.That(await threadExceptionThrown.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		var exception = await threadExceptionThrown.Task;
		Assert.That(exception.Message, Does.Contain("Invalid call to InvokeWinzorDispatcher from InvokeRenderDispatcher or RegisterAfterRenderAction"));
	}

	[Test]
	public async Task InvokeWinzorDispatcherWhileInvokeRenderDispatcherRunningDoesNotThrow()
	{
		using var ctx = new WinzorTestContext();
		var enterRenderActionTcs = new TaskCompletionSource();
		var afterRenderActionTcs = new TaskCompletionSource();
		Form form = null;
		var rendered = await ctx.RenderFormAsync(() => form = new Form());
		var invokeRenderTask = form.InvokeWinzorDispatcherAsync(() =>
			form.InvokeRenderDispatcher(async () =>
			{
				enterRenderActionTcs.SetResult();
				await afterRenderActionTcs.Task;
			}));
		await enterRenderActionTcs.Task;
		await form.InvokeWinzorDispatcherAsync(() => afterRenderActionTcs.SetResult());
		await invokeRenderTask;
	}

	[Test]
	public async Task InvokeWinzorDispatcherWhileAfterRenderActionRunningDoesNotThrow()
	{
		using var ctx = new WinzorTestContext();
		var enterRenderActionTcs = new TaskCompletionSource();
		var afterRenderActionTcs = new TaskCompletionSource();
		Form form = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			form.RegisterAfterRenderAction(async () =>
			{
				enterRenderActionTcs.SetResult();
				await afterRenderActionTcs.Task;
			});
			return form;
		});
		await enterRenderActionTcs.Task;
		await form.InvokeWinzorDispatcherAsync(() => afterRenderActionTcs.SetResult());
	}

	[Test]
	public async Task FocusFromClientDoesNotInvokeFocusFromServer()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var button1 = new Button { Text = "Foo" };
			form.Controls.Add(button1);
			var button2 = new Button { Text = "Bar" };
			form.Controls.Add(button2);
			return form;
		}, OpenFormAction.BlockUntilShown);
		Assert.That(ctx.JSInterop.Invocations["tryFocusFromServer"], Has.Count.EqualTo(1), "tryFocusFromServer invoked for initial focus");
		await rendered.Find("button:contains('Bar')").TriggerEventAsync("onwinzorfocusin", new WinzorFocusInEventArgs());
		Assert.That(ctx.JSInterop.Invocations["tryFocusFromServer"], Has.Count.EqualTo(1), "tryFocusFromServer should not be invoked for focusin from browser");
	}

	[Test]
	public async Task ControlMouseButtonsUpdatedOnMouseClickUpDown()
	{
		using var ctx = new WinzorTestContext();
		ControlWithEventHandlersExposed control = null;

		await ctx.RenderControlOnFormAsync(() =>
		{
			control = new ControlWithEventHandlersExposed();
			control.MouseClick += (sender, e) => Assert.That(Control.MouseButtons, Is.EqualTo(MouseButtons.Left));
			control.DoubleClick += (sender, e) => Assert.That(Control.MouseButtons, Is.EqualTo(MouseButtons.Left));
			return control;
		});
		Assert.That(Control.MouseButtons, Is.EqualTo(MouseButtons.None));

		await control.OnClickAsync(new WebMouseEventArgs() { Button = 0 });
		Assert.That(Control.MouseButtons, Is.EqualTo(MouseButtons.None));

		await control.OnMouseDownAsync(new WebMouseEventArgs() { Button = 0 });
		Assert.That(Control.MouseButtons, Is.EqualTo(MouseButtons.Left));

		await control.OnMouseUpAsync(new WebMouseEventArgs() { Button = 0 });
		Assert.That(Control.MouseButtons, Is.EqualTo(MouseButtons.None));
	}

	[Test]
	public async Task DifferentControlsOnSameFormRunAfterRenderActionsInParallel()
	{
		using var ctx = new WinzorTestContext();
		var control1EnterAfterRenderAction = new TaskCompletionSource();
		var control2EnterAfterRenderAction = new TaskCompletionSource();
		var control1BlockAfterRenderAction = new TaskCompletionSource();
		var control2BlockAfterRenderAction = new TaskCompletionSource();

		await ctx.RenderFormAsync(() =>
		{
			var form = new Form();

			var control1 = new TextBox();
			control1.RegisterAfterRenderAction(async () =>
			{
				control1EnterAfterRenderAction.SetResult();
				await control1BlockAfterRenderAction.Task;
			});

			var control2 = new DataGrid();
			control2.RegisterAfterRenderAction(async () =>
			{
				control2EnterAfterRenderAction.SetResult();
				await control2BlockAfterRenderAction.Task;
			});

			form.Controls.Add(control1);
			form.Controls.Add(control2);
			return form;
		});

		Assert.That(await control1EnterAfterRenderAction.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		Assert.That(await control2EnterAfterRenderAction.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		control1BlockAfterRenderAction.SetResult();
		control2BlockAfterRenderAction.SetResult();
	}

	[Test]
	public async Task SameControlRunAfterRenderActionsInParallel()
	{
		using var ctx = new WinzorTestContext();
		var enterAfterRenderAction1 = new TaskCompletionSource();
		var enterAfterRenderAction2 = new TaskCompletionSource();
		var blockAfterRenderAction1 = new TaskCompletionSource();
		var blockAfterRenderAction2 = new TaskCompletionSource();

		await ctx.RenderFormAsync(() =>
		{
			var form = new Form();

			var control = new TextBox();
			control.RegisterAfterRenderAction(async () =>
			{
				enterAfterRenderAction1.SetResult();
				await blockAfterRenderAction1.Task;
			});
			control.RegisterAfterRenderAction(async () =>
			{
				enterAfterRenderAction2.SetResult();
				await blockAfterRenderAction2.Task;
			});

			form.Controls.Add(control);
			return form;
		});

		Assert.That(await enterAfterRenderAction1.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		Assert.That(await enterAfterRenderAction2.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		blockAfterRenderAction1.SetResult();
		blockAfterRenderAction2.SetResult();
	}

	[Test]
	public async Task OnMouseHoverInvokedCorrectly()
	{
		using var ctx = new WinzorTestContext();
		ControlWithEventHandlersExposed control = null;

		await ctx.RenderControlOnFormAsync(() =>
		{
			control = new ControlWithEventHandlersExposed();
			control.MouseHover += (sender, e) => control.ControlMouseHover = true;
			return control;
		});

		Assert.That(control.ControlMouseHover, Is.False);
		await control.OnMouseOverAsync(new WebMouseEventArgs() { Button = 0 });
		Assert.That(control.ControlMouseHover, Is.True);
	}

	[Test]
	public async Task InvokeRenderDispatcherDoesNotThrowExceptionWhenControlHasNotBeenRendered()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var control = new Control();
			Assert.DoesNotThrowAsync(async () => await control.InvokeRenderDispatcherAsync(() => Task.CompletedTask));
		});
	}

	[Test]
	public async Task ControlInWinzorSpecificControlsShouldTriggerOnCreateControl()
	{
		ControlForTest winzorSpecificControl = null;
		using var ctx = new WinzorTestContext();
		await ctx.RenderControlOnFormAsync(() =>
		{
			var control = new Control();
			winzorSpecificControl = new ControlForTest();
			control.WinzorSpecificControls.Add(winzorSpecificControl);

			return control;
		});

		Assert.That(winzorSpecificControl.IsTriggerOnCreateControl);
	}

	[Test]
	public async Task HasActiveContextMenuFalseIfNoContextMenuCreated()
	{
		TextBox control = null;

		using var ctx = new WinzorTestContext();
		await ctx.RenderControlOnFormAsync(() => control = new TextBox());

		Assert.That(control, Is.Not.Null);
		Assert.That(control.HasActiveContextMenu, Is.False);
	}

	[Test]
	public async Task HasActiveContextMenuTrueAfterOpeningContextMenu()
	{
		TextBox control = null;

		using var ctx = new WinzorTestContext();
		await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(control = new TextBox());
			control.ContextMenu = new ContextMenu();
			control.ContextMenu.Show(control, new Point(1, 1));

			return form;
		});

		Assert.That(control, Is.Not.Null);
		Assert.That(control.HasActiveContextMenu, Is.True);
	}

	[Test]
	public async Task HasActiveContextMenuFalseAfterClosingContextMenu()
	{
		TextBox control = null;

		using var ctx = new WinzorTestContext();
		var menuDisplayer = new TestMenuDisplayer();
		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: menuDisplayer);
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(control = new TextBox());
			control.ContextMenu = new ContextMenu();
			control.ContextMenu.Show(control, new Point(1, 1));

			return form;
		}, clientServices);

		Assert.That(control, Is.Not.Null);
		Assert.That(control.HasActiveContextMenu, Is.True);

		await menuDisplayer.MenuClosedCallback(new MenuClosedResult(MenuClosedResultCode.NoSelection, Guid.Empty, null));
		Assert.That(control.HasActiveContextMenu, Is.False);
	}

	internal interface IJSInteropForTest : IJSInterop
	{
	}

	[Test]
	public async Task GetJSInteropReturnsRegisteredService()
	{
		using var ctx = new WinzorTestContext();
		ctx.Services.AddScoped(_ => Mock.Of<IJSInteropForTest>());
		var rendered = await ctx.RenderControlOnFormAsync(() => new ControlForTest());
		var control = rendered.GetControl<ControlForTest>();

		var jsInterop = control.GetJSInterop<IJSInteropForTest>();
		Assert.That(jsInterop, Is.Not.Null);
	}

	[Test]
	public async Task GetJSInteropThrowsWhenServiceIsNotRegistered()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new ControlForTest());
		var control = rendered.GetControl<ControlForTest>();

		Assert.That(() => control.GetJSInterop<IJSInteropForTest>(), Throws.Exception);
	}

	[Test]
	public async Task GetJSInteropReturnsNullAndDoesNotThrowWhenNotRendered()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new ControlForTest());
		var control = rendered.GetControl<ControlForTest>();

		// Simulate the control being removed from the render tree
		control.Proxy = null;
		var jsInterop = control.GetJSInterop<IJSInteropForTest>();
		Assert.That(jsInterop, Is.Null);
	}

	[Test]
	public async Task PreventDefaultMouseDownIsFalseByDefaultForControl()
	{
		using var ctx = new WinzorTestContext();
		Control control = null;
		await ctx.RenderControlOnFormAsync(() =>
		{
			control = new Control();
			return control;
		});

		Assert.That(control, Is.Not.Null);
		Assert.That(control.PreventDefaultMouseDown, Is.False);
	}

	[Test, WithPlaywrightPage]
	public async Task DisabledControlSetsPointerEventsNone()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var button = new Button { Enabled = false, Text = "yay" };
			var textBox = new TextBox { Enabled = false };
			var checkBox = new CheckBox { Enabled = false };
			form.Controls.Add(button);
			form.Controls.Add(textBox);
			form.Controls.Add(checkBox);
			return form;
		});

		var button = page.Locator("button", new PageLocatorOptions { HasTextString = "yay" });
		var textBox = page.Locator("input[type='text']");
		var checkBox = page.Locator("input[type='checkbox']");

		Assert.That(await button.GetComputedStyleAsync("pointer-events"), Is.EqualTo("none"));
		Assert.That(await textBox.GetComputedStyleAsync("pointer-events"), Is.EqualTo("none"));
		Assert.That(await checkBox.GetComputedStyleAsync("pointer-events"), Is.EqualTo("none"));
	}

	[Test, WithPlaywrightPage]
	public async Task WinzorSpecificControlShouldNotBeVisibleIfAssignedVisibilityFalse()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var panel = new Panel();
			var label = new Label() { Text = "Test" };
			panel.WinzorSpecificControls.Add(label);
			form.Controls.Add(panel);
			label.Visible = false;
			panel.Visible = true;
			return form;
		});

		var label = page.Locator(".label");
		Assert.That(async () => await label.IsVisibleAsync(), Is.False.After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task WinzorSpecificControlShouldBeVisibleIfParentVisible()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var panel = new Panel();
			var label = new Label() { Text = "Test" };
			panel.WinzorSpecificControls.Add(label);
			form.Controls.Add(panel);
			return form;
		});

		var label = page.Locator(".label");
		Assert.That(async () => await label.IsVisibleAsync(), Is.True.After(3000, 100));
	}

	[Test]
	public async Task WinzorControlIsInputCharShouldReturnFalseByDefault()
	{
		using var ctx = new WinzorTestContext();
		ControlForTest control = null;
		await ctx.RenderControlOnFormAsync(() => control = new ControlForTest());

		Assert.That(control.IsInputCharTest('C'), Is.False);
		Assert.That(control.IsInputCharTest('D'), Is.False);
		Assert.That(control.IsInputCharTest('R'), Is.False);
	}

	[Test]
	public async Task WinzorControlIsInputKeyShouldReturnFalseByDefault()
	{
		using var ctx = new WinzorTestContext();
		ControlForTest control = null;
		await ctx.RenderControlOnFormAsync(() => control = new ControlForTest());

		Assert.That(control.IsInputKeyTest(Keys.C), Is.False);
		Assert.That(control.IsInputKeyTest(Keys.Alt | Keys.C), Is.False);
		Assert.That(control.IsInputKeyTest(Keys.Left | Keys.C), Is.False);
	}

	[Test]
	public async Task FindDescendantByWinzorControlIdCanFindWinzorControlTest()
	{
		using var ctx = new WinzorTestContext();
		await ctx.RenderFormAsync(() =>
		{
			var form = new FormWithFindDescendantByWinzorControlIdExposed();
			var panel = new Panel();
			form.WinzorSpecificControls.Add(panel);
			Assert.That(form.FindDescendant(panel.WinzorControlId), Is.EqualTo(panel));
			return form;
		});
	}

	[Test]
	public async Task OnVisibleChanged_WhenDisposed_ShouldNotCreateControl()
	{
		Control control = null;
		using var ctx = new WinzorTestContext();
		await ctx.RenderControlOnFormAsync(() =>
		{
			control = new Control();
			return control;
		});

		await control.InvokeWinzorDispatcherAsync(() =>
		{
			control.Visible = false;
			control.Dispose();
		});

		Assert.That(control.Created, Is.EqualTo(false), "The Created state should be false after disposing the control");

		await control.InvokeWinzorDispatcherAsync(() =>
		{
			control.Visible = true;
		});

		Assert.That(control.Created, Is.EqualTo(false), "CreateControl should not be called in OnVisibleChanged if Disposing is true");
	}

	[Test]
	public async Task FindDescendantByElementReferenceCanFindWinzorControlTest()
	{
		using var ctx = new WinzorTestContext();
		await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var panel = new Panel();

			var reference = new ElementReference(Guid.NewGuid().ToString());
			panel.ElementReference = reference;
			form.WinzorSpecificControls.Add(panel);

			Assert.That(form.FindDescendantByElementReference(reference), Is.EqualTo(panel));
			return form;
		});
	}

	[Test]
	public async Task AddItemBetweenWinzorSpecificControlsAndControlsTest()
	{
		using var ctx = new WinzorTestContext();
		await ctx.RenderFormAsync(() =>
		{
			var form1 = new Form();
			var form2 = new Form();
			var panel = new Panel();

			form1.WinzorSpecificControls.Add(panel);
			Assert.That(panel.Parent, Is.EqualTo(form1), "panel's parent should be set to form1");

			form2.Controls.Add(panel);
			Assert.That(panel.Parent, Is.EqualTo(form2), "panel's parent should be changed to form2");
			Assert.That(form1.WinzorSpecificControls, Does.Not.Contain(panel), "panel should be removed from form1's winzor controls");

			form1.WinzorSpecificControls.Add(panel);
			Assert.That(panel.Parent, Is.EqualTo(form1), "panel's parent should be changed to form1");
			Assert.That(form2.Controls, Does.Not.Contain(panel), "panel should be removed from form2's controls");

			return form1;
		});
	}

	[Test]
	public async Task ItemShouldBeRemovedFromWinzorControlsIfSetParentNullTest()
	{
		using var ctx = new WinzorTestContext();
		await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var panel = new Panel();

			form.WinzorSpecificControls.Add(panel);
			Assert.That(panel.Parent, Is.EqualTo(form), "panel's parent should be set to form");

			panel.Parent = null;
			Assert.That(form.WinzorSpecificControls, Does.Not.Contain(panel), "panel should be removed from form's winzor controls");

			return form;
		});
	}

	[Test, WithPlaywrightPage]
	public async Task OnCreateControlShouldBeforeOnBindingContextChanged()
	{
		await using var ctx = new InMemoryTestServerContext();
		ControlWithOnBindingContextChanged control = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var panel = new Panel();
			var label = new Label() { Text = "Test" };
			panel.WinzorSpecificControls.Add(label);
			form.Controls.Add(panel);
			control = new ControlWithOnBindingContextChanged();
			form.Controls.Add(control);
			return form;
		});

		var label = page.Locator(".label");
		Assert.That(async () => await label.IsVisibleAsync(), Is.True.After(3000, 100));
		Assert.That(control.IsOnCreateControlBeforeOnBindingContextChanged, Is.True);
	}

	[Test]
	public async Task OnBindingContextChangedShouldBeCalledOnceWhenChildControlCreated()
	{
		using var ctx = new WinzorTestContext();
		ControlWithOnBindingContextChanged control = null;
		ControlWithOnBindingContextChanged childControl = null;

		await ctx.RenderControlOnFormAsync(() =>
		{
			control = new ControlWithOnBindingContextChanged();
			childControl = new ControlWithOnBindingContextChanged();

			control.Controls.Add(childControl);
			return control;
		});

		Assert.That(control.OnBindingContextChangedCount, Is.EqualTo(1));
		Assert.That(childControl.OnBindingContextChangedCount, Is.EqualTo(1));
	}

	[Test]
	public async Task OnBindingContextChangedEventShouldBeCalled()
	{
		using var ctx = new WinzorTestContext();
		ControlWithOnBindingContextChangedEvent control = null;

		await ctx.RenderControlOnFormAsync(() =>
		{
			control = new ControlWithOnBindingContextChangedEvent();
			return control;
		});

		Assert.That(control.OnBindingContextChangedCount, Is.EqualTo(1));
	}

	[Test]
	public async Task BeginInvokeShouldNotRequirePostingBackToTheWinzorDispatcherSynchronizationContext()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var control = new Control();
			var cts = new CancellationTokenSource();
			var invokeTask = control.BeginInvoke(() =>
			{
				cts.Cancel();
			});
			// We only want to run our BeginInvoke action without running any other actions that may be queued behind it on the dispatcher
			// This will ensure that the BeginInvoke task can be completed without running any continuations that may have been posted
			// back to the synchronization context and would require a second dispatcher action to be run before the task is completed
			ctx.WinzorDispatcher.RunMessageLoop(cts);
			Assert.That(invokeTask.IsCompleted, Is.True);
		});
	}

	[Test]
	public async Task OnDragEndDoesNotFireDragEndEvent()
	{
		using var ctx = new WinzorTestContext();
		var complete = new TaskCompletionSource();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var control = new Label() { AllowItemDrag = true };
			control.DragEnd += (_, _) => complete.SetResult();

			return control;
		});

		try
		{
			var label = rendered.Find(".label").TriggerEventAsync("ondragend", new WebDragEventArgs() { ClientX = 123, ClientY = 456 });

			// If the event handler is present, it should not be linked to the DragEnd event.
			// (We have a WinzorDragEnd that should be used instead.) 
			Assert.That(complete.Task.IsCompleted, Is.False);
		}
		catch (MissingEventHandlerException)
		{
			// If the event handler is missing, the test should pass.
		}
	}
}

public class ControlWithEventHandlersExposed : Control
{
	public new Task OnMouseDownAsync(WebMouseEventArgs args) => base.OnMouseDownAsync(args);
	public new Task OnMouseUpAsync(WebMouseEventArgs args) => base.OnMouseUpAsync(args);
	public new Task OnClickAsync(WebMouseEventArgs args) => base.OnClickAsync(args);
	public new Task OnDragStartAsync(WebDragEventArgs args) => base.OnDragStartAsync(args);
	public new Task OnWinzorDragEndAsync(WinzorDragEndEventArgs args) => base.OnWinzorDragEndAsync(args);
	public new Task OnMouseEnterAsync() => base.OnMouseEnterAsync();
	public new Task OnMouseLeaveAsync() => base.OnMouseLeaveAsync();
	public new Task OnMouseOverAsync(WebMouseEventArgs args) => base.OnMouseOverAsync(args);
	public new Task OnDragDropAsync(WinzorDragEventArgs args) => base.OnDragDropAsync(args);
	public new Task OnDragOverAsync(WebDragEventArgs args) => base.OnDragOverAsync(args);
	public new Task OnDragLeaveAsync(WebDragEventArgs args) => base.OnDragLeaveAsync(args);
	public new Task OnFocusInAsync(WinzorFocusInEventArgs args) => base.OnFocusInAsync(args);
	public new Task OnFocusOutAsync(WinzorFocusOutEventArgs args) => base.OnFocusOutAsync(args);

	public bool ControlMouseHover;
}

class ControlProxyComponentForTest : ControlProxyComponent
{
	public static Dictionary<object, Task> WaitTasks { get; set; } = new Dictionary<object, Task>();
	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (WaitTasks.Keys.Any(s => s == Control))
		{
			await WaitTasks[Control];
		}
		await base.OnAfterRenderAsync(firstRender);
	}
}

class ControlForTest : Control
{
	public ControlForTest(bool taskCanceledExceptionWhenDetachFromRenderer = false)
	{
		throwTaskCanceledExceptionWhenDetachFromRenderer = taskCanceledExceptionWhenDetachFromRenderer;
	}

	protected internal override bool ShouldRender => true;

	public override bool CaptureElementReference => true;

	public bool IsTriggerOnCreateControl { get; private set; }

	public bool IsInputCharTest(char charcode)
	{
		return IsInputChar(charcode);
	}

	public bool IsInputKeyTest(Keys key)
	{
		return IsInputKey(key);
	}

	protected override void OnCreateControl()
	{
		base.OnCreateControl();
		IsTriggerOnCreateControl = true;
	}

	readonly bool throwTaskCanceledExceptionWhenDetachFromRenderer;

	public override async Task DetachFromRendererAsync()
	{
		await base.DetachFromRendererAsync();
		if (throwTaskCanceledExceptionWhenDetachFromRenderer)
		{
			throw new TaskCanceledException();
		}
	}
}

class ControlWithUpdatePropertyExposed<T> : Control
{
	public T field;

	public bool CallUpdateProperty(T value) => UpdateProperty(ref field, value);
}

class ControlWithControlStyleStringExposed : Control
{
	public string StyleString => ControlStyleString;
}

class ControlWithAsyncExceptions : Control
{
	public bool ThrowOnAfterRenderAsync { get; set; }

	public bool ThrowOnInitializedAsync { get; set; }

	public bool ThrowOnBeforeRender { get; set; }

	protected internal override Task OnAfterRenderAsync(bool firstRender)
	{
		if (ThrowOnAfterRenderAsync)
		{
			throw new InvalidOperationException("Exception thrown from OnAfterRenderAsync");
		}
		return base.OnAfterRenderAsync(firstRender);
	}

	protected internal override Task OnInitializedAsync()
	{
		if (ThrowOnInitializedAsync)
		{
			throw new InvalidOperationException("Exception thrown from OnInitializedAsync");
		}
		return base.OnInitializedAsync();
	}

	protected internal override void OnBeforeRender()
	{
		if (ThrowOnBeforeRender)
		{
			throw new InvalidOperationException("Exception thrown from OnBeforeRender");
		}
		base.OnBeforeRender();
	}
}

class PortalControl : Control
{
	public override bool RenderInPortal => true;

	protected override string ClassName => "PortalControl";

	protected internal override bool ShouldRender => Visible;
}

class ControlWithEventAttribute : Control
{
	readonly EventAttribute eventAttribute;

	public ControlWithEventAttribute(EventAttribute eventAttribute)
	{
		this.eventAttribute = eventAttribute;
	}

	protected override EventAttribute EventAttributes => eventAttribute;

	protected override string ClassName => "ControlWithEventAttribute";

	protected internal override bool ShouldRender => Visible;

	public override bool Enabled { get => false; set => base.Enabled = value; }
}

class ControlWithOnBeforeRender : Label
{
	protected internal override void OnBeforeRender()
	{
		OnBeforeRenderCalled = true;
	}

	public bool OnBeforeRenderCalled;
}
class ControlWithOnBindingContextChanged : Control
{
	bool IsOnBindingContextChangedCalled { get; set; }

	public int OnBindingContextChangedCount { get; set; }

	public bool IsOnCreateControlBeforeOnBindingContextChanged { get; set; }

	protected override void OnBindingContextChanged(EventArgs e)
	{
		IsOnBindingContextChangedCalled = true;
		OnBindingContextChangedCount++;
		base.OnBindingContextChanged(e);
	}

	protected override void OnCreateControl()
	{
		if (IsOnBindingContextChangedCalled)
		{
			throw new Exception("OnCreateControl must be called before OnBindingContextChanged");
		}
		base.OnCreateControl();
		IsOnCreateControlBeforeOnBindingContextChanged = true;
	}
}
class ControlWithOnBindingContextChangedEvent : Control
{
	public int OnBindingContextChangedCount { get; set; }

	void BindingContextChangedEvent(object sender, EventArgs e)
	{
		OnBindingContextChangedCount++;
	}

	protected override void OnCreateControl()
	{
		base.OnCreateControl();
		BindingContextChanged += BindingContextChangedEvent;
	}
}

class FormWithFindDescendantByWinzorControlIdExposed : Form
{
	public Control FindDescendant(string winzorControlId) => FindDescendantByWinzorControlId(winzorControlId);
}
