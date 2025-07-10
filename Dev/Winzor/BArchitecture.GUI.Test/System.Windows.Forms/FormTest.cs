using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BArchitecture;
using Bunit;
using CargoWise.Blazor.Client.Integration.Messaging;
using Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WinzorFramework.Extensions;
using WinzorFramework.JSInterop;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using BlazorMessaging = CargoWise.Blazor.Client.Integration.Messaging;

namespace System.Windows.Forms;

using static WinzorTestFramework.WinzorTestContext;

public class FormTest
{
	[Test, WithPlaywrightPage]
	public async Task DocumentTitleIsFormText()
	{
		await using var ctx = new InMemoryTestServerContext();

		Form form = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form { Text = "Form Title" };
			return form;
		});

		Assert.That(form, Is.Not.Null);

		await page.WaitForFunctionAsync("document.title === 'Form Title'");
		Assert.That(await page.TitleAsync(), Is.EqualTo("Form Title"));

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			form.Text = "Updated Title";
		});
		await page.WaitForFunctionAsync("document.title === 'Updated Title'");
		Assert.That(await page.TitleAsync(), Is.EqualTo("Updated Title"));
	}

	[Test]
	public async Task ShownEventRaisedAfterFirstRender()
	{
		using var ctx = new WinzorTestContext();
		var shownCts = new TaskCompletionSource();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Shown += Form_Shown;
			return form;
		});
		await shownCts.Task;

		// The form should have rendered twice, once for the initial render and then once due to the Form.Shown event being raised
		Assert.That(rendered.RenderCount, Is.EqualTo(2));

		void Form_Shown(object sender, EventArgs e)
		{
			((Form)sender).Text = "Update to force NotifyRenderRequired";
			shownCts.SetResult();
		}
	}

	[Test]
	public async Task TestFormToString()
	{
		using var ctx = new WinzorTestContext();
		Form form = null;
		var rendered = await ctx.RenderFormAsync(() => form = new Form());
		Assert.That(form.ToString(), Is.EqualTo("System.Windows.Forms.Form, Text: "));
		await form.InvokeWinzorDispatcherAsync(() => form.Text = "Lose Form");
		Assert.That(form.ToString(), Is.EqualTo("System.Windows.Forms.Form, Text: Lose Form"));
	}

	[Test]
	public async Task FormShouldValidateActiveControlOnClose()
	{
		using var ctx = new WinzorTestContext();

		var isValidatingRaised = new TaskCompletionSource();
		var binding = new DummyBinding();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var textBox = new TextBox();
			textBox.Validating += (_, _) => isValidatingRaised.SetResult();
			textBox.DataBindings.Add("Text", binding, "Value", false, DataSourceUpdateMode.OnValidation);
			return textBox;
		});
		var form = rendered.GetForm();
		var control = rendered.GetControl<TextBox>();

		await rendered.Find("input").InputAsync(new ChangeEventArgs { Value = "This is some text." });
		Assert.That(isValidatingRaised.Task.IsCompleted, Is.False);
		Assert.That(binding.Value, Is.EqualTo(string.Empty));

		await form.InvokeWinzorDispatcherAsync(() => form.Close());

		Assert.That(await isValidatingRaised.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);
		Assert.That(binding.Value, Is.EqualTo("This is some text."));
	}

	class DummyBinding
	{
		public string Value { get; set; } = string.Empty;
	}

	[Test]
	public async Task FormHasDefaultFontStyles()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() => new Form());

		var formStyleString = rendered.Find(".form").GetAttribute("style");
		Assert.That(formStyleString, Does.Contain("font-family:Tahoma;"));
		Assert.That(formStyleString, Does.Contain("font-size:8pt"));
		Assert.That(formStyleString, Does.Contain("line-height:13px"));
	}

	[Test]
	public async Task FormHasDefaultScrollStyles()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() => new Form());

		var formStyleString = rendered.Find(".form").GetAttribute("style");
		Assert.That(formStyleString, Does.Contain("overflow:hidden;"));
	}

	[Test]
	public async Task FormShouldClearCargoWiseClientServicesOnDispose()
	{
		using var ctx = new WinzorTestContext();
		Form form = null;
		var rendered = await ctx.RenderFormAsync(() => form = new Form());
		Assert.That(form.CargoWiseClientServices, Is.Not.Null);
		await form.InvokeWinzorDispatcherAsync(() => form.Dispose());
		Assert.That(form.CargoWiseClientServices, Is.Null);
	}

	[Test]
	public async Task TestFormIsFocusedOnShow()
	{
		using var ctx = new WinzorTestContext();
		Form form = null;
		var rendered = await ctx.RenderFormAsync(() => form = new Form());

		Assert.That(form.Focused, Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task ShowMnemonicKeysShouldBeTrueAfterAlt()
	{
		await using var ctx = new InMemoryTestServerContext();
		DummyForm form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new DummyForm();
			var button = new Button { Text = "&Test" };
			form.Controls.Add(button);
			return form;
		});
		var mnemonicKey = await page.WaitForSelectorAsync(".mnemonickey");

		Assert.That(!form.ShowMnemonicKeys());
		Assert.That(async () => await mnemonicKey.EvaluateAsync<string>("e => window.getComputedStyle(e).textDecoration"), Does.Contain("none").After(3000, 100));

		await page.Keyboard.DownAsync("Alt");
		Assert.That(async () => await mnemonicKey.EvaluateAsync<string>("e => window.getComputedStyle(e).textDecoration"), Does.Contain("underline").After(3000, 100));
		Assert.That(form.ShowMnemonicKeys());
	}

	[Test, WithPlaywrightPage]
	public async Task DragFilesOntoFormUploadsFile()
	{
		await using var ctx = new InMemoryTestServerContext();
		var dragDropEventFired = new TaskCompletionSource<DragEventArgs>();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.AllowDrop = true;
			form.DragDrop += (sender, args) => dragDropEventFired.SetResult(args);
			return form;
		});

		await page.WaitForSelectorAsync(".form");

		await page.DispatchEventAsync(".form", "change");
		var dataTransfer = await page.EvaluateHandleAsync("() => new DataTransfer()");
		await dataTransfer.EvaluateAsync("data => data.items.add(new File(['This is file one.'], 'File1.txt', {type: 'text/plain', lastModified: 1654862400000}))");
		await dataTransfer.EvaluateAsync("data => data.items.add(new File(['This is file two.'], 'File2.txt', {type: 'text/plain', lastModified: 1654866000000}))");
		await page.DispatchEventAsync(".form", "dragover", new { dataTransfer });
		await page.DispatchEventAsync(".form", "drop", new { dataTransfer });
		var values = (string[])(await dragDropEventFired.Task).Data.GetData(DataFormats.FileDrop);
		Assert.That(values.Length, Is.EqualTo(2));
		Assert.That(values[0], Does.Contain("File1.txt"));
		Assert.That(values[1], Does.Contain("File2.txt"));
	}

	[Test, WithPlaywrightPage]
	public async Task DragFilesOntoControlTriggersDragDropOnTheControl_IfAllowDropIsTrue()
	{
		await using var ctx = new InMemoryTestServerContext();
		var dragDropEventFired = new TaskCompletionSource<DragEventArgs>();
		var dragOverEventFired = new TaskCompletionSource<DragEventArgs>();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var control = new Control() { Width = 300, Height = 300, Name = "RandomControl" };
			control.AllowDrop = true;
			control.DragOver += (sender, args) => dragOverEventFired.SetResult(args);
			control.DragDrop += (sender, args) => dragDropEventFired.SetResult(args);
			return control;
		});
		var control = page.Locator("[data-name=\"RandomControl\"]");
		var dataTransfer = await page.EvaluateHandleAsync("() => new DataTransfer()");
		await dataTransfer.EvaluateAsync("data => data.items.add(new File(['This is file one.'], 'File1.txt', {type: 'text/plain', lastModified: 1654862400000}))");
		await dataTransfer.EvaluateAsync("data => data.items.add(new File(['This is file two.'], 'File2.txt', {type: 'text/plain', lastModified: 1654866000000}))");

		await control.DispatchEventAsync("drop", new { dataTransfer, clientX = 50, clientY = 200 });

		var dragDropEventArgs = await dragDropEventFired.Task;
		var values = (string[])dragDropEventArgs.Data.GetData(DataFormats.FileDrop);
		Assert.That(values.Length, Is.EqualTo(2));
		Assert.That(values[0], Does.Contain("File1.txt"));
		Assert.That(values[1], Does.Contain("File2.txt"));
		Assert.That(dragDropEventArgs.X, Is.EqualTo(50));
		Assert.That(dragDropEventArgs.Y, Is.EqualTo(200));

		var dragOverEventArgs = await dragOverEventFired.Task;
		Assert.That((string[])dragOverEventArgs.Data.GetData(DataFormats.FileDrop), Is.EquivalentTo(values));
		Assert.That(dragOverEventArgs.X, Is.EqualTo(dragDropEventArgs.X));
		Assert.That(dragOverEventArgs.Y, Is.EqualTo(dragDropEventArgs.Y));
	}

	[Test, WithPlaywrightPage]
	public async Task DragFilesOntoControlPropagatesDragDropToParentControl_IfAllowDropIsFalse()
	{
		await using var ctx = new InMemoryTestServerContext();
		var parentDragDropEventFired = new TaskCompletionSource<DragEventArgs>();
		var childDragDropEventFired = new TaskCompletionSource<DragEventArgs>();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var parent = new Control() { Width = 300, Height = 300, Name = "ParentControl" };
			parent.AllowDrop = true;
			parent.DragDrop += (sender, args) => parentDragDropEventFired.SetResult(args);

			var child = new Control() { Width = 300, Height = 300, Name = "ChildControl" };
			child.AllowDrop = false;
			child.DragDrop += (sender, args) => childDragDropEventFired.SetResult(args);

			parent.Controls.Add(child);
			return parent;
		});
		var child = page.Locator("[data-name=\"ChildControl\"]");
		var dataTransfer = await page.EvaluateHandleAsync("() => new DataTransfer()");
		await dataTransfer.EvaluateAsync("data => data.items.add(new File(['This is file one.'], 'File1.txt', {type: 'text/plain', lastModified: 1654862400000}))");
		await dataTransfer.EvaluateAsync("data => data.items.add(new File(['This is file two.'], 'File2.txt', {type: 'text/plain', lastModified: 1654866000000}))");

		await child.DispatchEventAsync("drop", new { dataTransfer, clientX = 50, clientY = 200 });

		Assert.That(await childDragDropEventFired.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.False);

		var parentDragEventArgs = await parentDragDropEventFired.Task;
		var values = (string[])parentDragEventArgs.Data.GetData(DataFormats.FileDrop);
		Assert.That(values.Length, Is.EqualTo(2));
		Assert.That(values[0], Does.Contain("File1.txt"));
		Assert.That(values[1], Does.Contain("File2.txt"));
		Assert.That(parentDragEventArgs.X, Is.EqualTo(50));
		Assert.That(parentDragEventArgs.Y, Is.EqualTo(200));
	}

	[Test, WithPlaywrightPage]
	public async Task TimeoutWhenDragFilesOntoFormShouldReportDeveloperException()
	{
		await using var ctx = new InMemoryTestServerContext();
		var tcs = new TaskCompletionSource<bool>();
		var tcsException = new TaskCompletionSource<Exception>();
		var form = default(Form);
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			form.AllowDrop = true;
			form.DragDrop += (sender, args) =>
			{
				tcs.SetResult(true);
			};
			return form;
		});

		var fileServiceMock = new Mock<IFileService>();
		_ = fileServiceMock.Setup(i => i.UploadFilesToServerAsync(It.IsAny<BrowserFile[]>(), It.IsAny<long>(), It.IsAny<CancellationToken>())).Throws<TimeoutException>();
		form.CargoWiseClientServices.FileService = fileServiceMock.Object;

		Application.DeveloperException += Application_DeveloperException;
		try
		{
			var dataTransfer = await page.EvaluateHandleAsync("() => new DataTransfer()");
			await dataTransfer.EvaluateAsync("data => data.items.add(new File(['This is file one.'], 'File1.txt', {type: 'text/plain', lastModified: 1654862400000}))");
			await dataTransfer.EvaluateAsync("data => data.items.add(new File(['This is file two.'], 'File2.txt', {type: 'text/plain', lastModified: 1654866000000}))");
			await page.DispatchEventAsync(".form", "dragover", new { dataTransfer });
			await page.DispatchEventAsync(".form", "drop", new { dataTransfer });

			Assert.That(await tcsException.Task, Is.TypeOf<TimeoutException>());
			Assert.That(tcs.Task.IsCompleted, Is.False);
		}
		finally
		{
			Application.DeveloperException -= Application_DeveloperException;
		}

		void Application_DeveloperException(object sender, string message, Exception ex)
		{
			tcsException.SetResult(ex);
		}
	}

	[Test, WithPlaywrightPage]
	public async Task FormUnderlinesMnemonicKeysOnAltKeyPress()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var button = new Button { Text = "&Test" };
			form.Controls.Add(button);
			return form;
		});
		var mnemonicKey = await page.WaitForSelectorAsync(".mnemonickey");
		Assert.That(async () => await mnemonicKey.EvaluateAsync<string>("e => window.getComputedStyle(e).textDecoration"), Does.Contain("none").After(3000, 100));
		await page.Keyboard.DownAsync("Alt");
		Assert.That(async () => await mnemonicKey.EvaluateAsync<string>("e => window.getComputedStyle(e).textDecoration"), Does.Contain("underline").After(3000, 100));
	}

	[Test]
	public async Task CallingCloseShouldDisposeForm()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() => new Form { Text = "Form Title" });
		var form = rendered.GetForm();
		await form.InvokeWinzorDispatcherAsync(form.Close);
		Assert.That(form.IsDisposed, Is.True);
	}

	[Test]
	public async Task DisposeProxyAfterFormCloseShouldResetProxy()
	{
		using var ctx = new WinzorTestContext();
		Form form = null;
		var rendered = await ctx.RenderFormAsync(() => form = new Form());
		Assert.That(form.IsDisposed, Is.False);
		Assert.That(form.CargoWiseClientServices, Is.Not.Null);
		Assert.That(form.Proxy, Is.Not.Null);
		await form.InvokeWinzorDispatcherAsync(form.Close);
		Assert.That(form.IsDisposed, Is.True);
		Assert.That(form.CargoWiseClientServices, Is.Null);
		Assert.That(form.Proxy, Is.Not.Null);
		ctx.MockCargoWiseClientServices.WindowService.Verify(w => w.RequestCloseAsync(), Times.Once);
		await form.Proxy.DisposeAsync();
		Assert.That(form.IsDisposed, Is.True);
		Assert.That(form.CargoWiseClientServices, Is.Null);
	}

	[Test]
	public async Task CallingCloseShouldNotDisposeWhenFormDialog()
	{
		using var ctx = new WinzorTestContext();
		Form form = null;
		var dispatcherContext = new DispatcherContext(ctx, ctx.DefaultClientServices, OpenFormAction.None);
		var showDialogTask = ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			form = new Form();
			using (ctx.WinzorDispatcher.WithContext(dispatcherContext))
			{
				form.ShowDialog();
			}
		});
		Assert.That(() => dispatcherContext.Rendered, Is.Not.Null.After(1000));

		Assert.That(form.Modal, Is.True);
		await form.InvokeWinzorDispatcherAsync(() => form.Close());
		Assert.That(form.IsDisposed, Is.False);
		await showDialogTask;
	}

	static IEnumerable UserInputTestCaseData
	{
		get
		{
			yield return new TestCaseData(async (IPage page) =>
			{
				await (await page.QuerySelectorAsync("div")).ClickAsync();
			})
			{ TestName = "{m}_Mouse" };

			yield return new TestCaseData(async (IPage page) =>
			{
				await page.Keyboard.PressAsync("A");
			})
			{ TestName = "{m}_Keyboard" };
		}
	}

	[TestCaseSource(nameof(UserInputTestCaseData)), WithPlaywrightPage]
	public async Task InvokeUserActivity(Func<IPage, Task> userActivity)
	{
		await using var ctx = new InMemoryTestServerContext();

		var eventFired = new TaskCompletionSource<bool>();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.WinzorDispatcher.UserActivity += new EventHandler((object sender, EventArgs e) =>
			{
				eventFired.TrySetResult(true);
			});
			return form;
		});

		await page.AddInitScriptAsync(@"
			window.cargoWiseClient = {
				setApplicationReadyWithVersion : () => {}
			};
		");

		await page.WaitForSelectorAsync(".form");
		await userActivity(page);
		Assert.That(await eventFired.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
	}

	[Test]
	public async Task NotRespondingOverlay()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var button = new Button { Text = "buttontext" };
			button.Click += (sender, args) =>
			{
				Thread.Sleep(3000);
			};
			return button;
		});
		Assert.That(rendered.FindAll(".overlay--notresponding").Count, Is.EqualTo(0));
		rendered.Find("button").Click(new WebMouseEventArgs());
		Assert.That(() => rendered.FindAll(".overlay--notresponding").Count, Is.EqualTo(1).After(2000));
		Assert.That(() => rendered.FindAll(".overlay--notresponding").Count, Is.EqualTo(0).After(2000));
	}

	[Test]
	public async Task NotRespondingOverlayWhenWinzorDispatcherBlockedOnAnotherForm()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var button = new Button { Text = "buttontext" };
			button.Click += (sender, args) => { };
			return button;
		});
		Assert.That(rendered.FindAll(".overlay--notresponding").Count, Is.EqualTo(0));
		var blockngTask = ctx.WinzorDispatcher.InvokeAsync(() => Thread.Sleep(3000));
		rendered.Find("button").Click(new WebMouseEventArgs());
		Assert.That(() => rendered.FindAll(".overlay--notresponding").Count, Is.EqualTo(1).After(2000));
		Assert.That(() => rendered.FindAll(".overlay--notresponding").Count, Is.EqualTo(0).After(2000));
		await blockngTask;
	}

	[Test]
	public async Task NotRespondingOverlayWhenFormRerendered()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var button = new Button { Text = "buttontext" };
			button.Click += (sender, args) =>
			{
				Thread.Sleep(3000);
				button.FindForm().Controls.Add(new Label());
			};
			return button;
		});
		Assert.That(rendered.FindAll(".overlay--notresponding").Count, Is.EqualTo(0));
		rendered.Find("button").Click(new WebMouseEventArgs());
		Assert.That(() => rendered.FindAll(".overlay--notresponding").Count, Is.EqualTo(1).After(2000));
		Assert.That(() => rendered.FindAll(".overlay--notresponding").Count, Is.EqualTo(0).After(2000));
	}

	[Test]
	public async Task NotRespondingOverlayDoesNotRerenderForm()
	{
		using var ctx = new WinzorTestContext();
		FormWithRenderFullCount form = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new FormWithRenderFullCount();
			var button = new Button { Text = "buttontext" };
			button.Click += (sender, args) =>
			{
				Thread.Sleep(3000);
			};
			form.Controls.Add(button);
			return form;
		});
		Assert.That(form.RenderFullCount, Is.EqualTo(1));
		rendered.Find("button").Click(new WebMouseEventArgs());
		Assert.That(() => rendered.FindAll(".overlay--notresponding").Count, Is.EqualTo(1).After(2000));
		Assert.That(() => rendered.FindAll(".overlay--notresponding").Count, Is.EqualTo(0).After(2000));
		Assert.That(form.RenderFullCount, Is.EqualTo(1));
	}

	[Test]
	public async Task NotRespondingOverlayWithQueuedLongRunningAction()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var button = new Button { Text = "buttontext" };
			button.Click += (sender, args) =>
			{
				Thread.Sleep(3000);
			};
			return button;
		});
		Assert.That(rendered.FindAll(".overlay--notresponding").Count, Is.EqualTo(0));
		rendered.Find("button").Click(new WebMouseEventArgs());
		rendered.Find("button").Click(new WebMouseEventArgs());
		Assert.That(() => rendered.FindAll(".overlay--notresponding").Count, Is.EqualTo(1).After(2000));
		Assert.That(() => rendered.FindAll(".overlay--notresponding").Count, Is.EqualTo(1).After(2000));
		Assert.That(() => rendered.FindAll(".overlay--notresponding").Count, Is.EqualTo(0).After(3000));
	}

	[Test]
	public async Task NotRespondingOverlayTriggersJSCursorRecalculation()
	{
		using var ctx = new WinzorTestContext();
		await using var formInterop = new FormJSInterop(new DummyJSRunTimeWithMonitor(ctx.JSInterop.JSRuntime), new DummyFileVersionHash());
		var path = "/_content/WinzorFramework/js/module/overlay.js";
		var overlayModule = ctx.JSInterop.SetupModule(path);

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var button = new Button { Text = "buttontext" };
			button.Click += (sender, args) =>
			{
				Thread.Sleep(3000);
			};
			return button;
		});
		Assert.That(rendered.FindAll(".overlay--notresponding").Count, Is.EqualTo(0));
		rendered.Find("button").Click(new WebMouseEventArgs());
		Assert.That(() => rendered.FindAll(".overlay--notresponding").Count, Is.EqualTo(1).After(2000));
		Assert.That(() => rendered.FindAll(".overlay--notresponding").Count, Is.EqualTo(0).After(3000));

		Assert.That(overlayModule.Invocations.Identifiers, Contains.Item("forceCursorUpdate"));
	}

	class FormWithRenderFullCount : Form
	{
		protected override void RenderFull(RenderTreeBuilder builder)
		{
			RenderFullCount++;
			base.RenderFull(builder);
		}

		public int RenderFullCount { get; private set; }
	}

	[Test]
	public async Task FormStyleMessageChangedShouldTriggerStyleChangedEvent()
	{
		using var ctx = new WinzorTestContext();
		Form form = null;
		var styleChangedFired = 0;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form() { Width = 200, Height = 200, MaximizeBox = false, MinimizeBox = false, ControlBox = false, FormBorderStyle = FormBorderStyle.None };
			form.StyleChanged += (_, _) => styleChangedFired++;
			return form;
		});

		Assert.That(styleChangedFired, Is.EqualTo(0));
		Assert.That(rendered.RenderCount, Is.EqualTo(1));

		await form.InvokeWinzorDispatcherAsync(() => form.MaximizeBox = true);
		Assert.That(() => styleChangedFired, Is.EqualTo(1).After(3000, 100));
		Assert.That(rendered.RenderCount, Is.EqualTo(2));

		await form.InvokeWinzorDispatcherAsync(() => form.ControlBox = true);
		Assert.That(() => styleChangedFired, Is.EqualTo(2).After(3000, 100));
		Assert.That(rendered.RenderCount, Is.EqualTo(3));

		await form.InvokeWinzorDispatcherAsync(() => form.FormBorderStyle = FormBorderStyle.Sizable);
		Assert.That(() => styleChangedFired, Is.EqualTo(3).After(3000, 100));
		Assert.That(rendered.RenderCount, Is.EqualTo(4));

		await form.InvokeWinzorDispatcherAsync(() => form.MinimizeBox = true);
		Assert.That(() => styleChangedFired, Is.EqualTo(4).After(3000, 100));
		Assert.That(rendered.RenderCount, Is.EqualTo(5));
	}

	[Test]
	public async Task WindowStateMessageChangedShouldTriggerStyleChangedEvent()
	{
		using var ctx = new WinzorTestContext();
		Form form = null;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form() { Width = 200, Height = 200, WindowState = FormWindowState.Maximized };
			return form;
		});

		Assert.That(rendered.RenderCount, Is.EqualTo(1));
		await form.InvokeWinzorDispatcherAsync(() => form.WindowState = FormWindowState.Normal);

		Assert.That(rendered.RenderCount, Is.EqualTo(2));
	}

	[Test]
	public async Task BackgroundImage()
	{
		using var ctx = new WinzorTestContext();
		var imageSrc = TestImage.GetImage();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			return new Form
			{
				BackgroundImage = imageSrc,
			};
		});

		var button = rendered.Find(".form");

		Assert.That(button.GetAttribute("style"), Does.Contain($"background-image: url('{imageSrc.ToBase64DataUrl()}'); background-repeat: no-repeat;"));
	}

	[Test]
	public async Task PressEnterShouldSaveCloseFormWhenAcceptButtonIsAssigned()
	{
		Form form = null;
		TextBox textBox = null;
		var resultString = "test string";

		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var fReturnValue = string.Empty;
			form = new Form { Text = "Form Title" };

			textBox = new TextBox { Text = string.Empty };
			Button ok = new Button { Text = "OK" };
			ok.Click += new EventHandler((sender, e) =>
			{
				textBox.Text = resultString;
				form.Close();
			});

			form.AcceptButton = ok;
			form.Controls.Add(ok);

			form.Controls.Add(textBox);

			return form;
		});

		await rendered.KeyPressAsync(Keys.Enter, rendered.Find(".textbox")); // simulating when the foucs is on the textbox
		Assert.That(textBox.Text, Is.EqualTo(resultString));
		Assert.That(form.IsDisposed, Is.True);
	}

	[Test]
	public async Task PressEscShouldCloseFormWhenCancelButtonIsAssigned()
	{
		Form form = null;
		TextBox textBox = null;

		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var fReturnValue = string.Empty;
			form = new Form { Text = "Form Title" };

			textBox = new TextBox { Text = string.Empty };

			Button cancel = new Button { Text = "Cancel" };
			cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			cancel.Click += new EventHandler((sender, e) => form.Close());

			form.CancelButton = cancel;
			form.Controls.Add(cancel);

			form.Controls.Add(textBox);

			return form;
		});

		await rendered.KeyPressAsync(Keys.Enter, rendered.Find(".textbox")); // simulating when the foucs is on the textbox
		Assert.That(form.IsDisposed, Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task AcceptButtonShouldBeHighlightedWhenFocusIsOnNonEnterKeyinterceptableControl()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			Form form = new Form { Text = "Form Title" };

			TextBox textBox = new TextBox { Text = string.Empty };
			Button ok = new Button { Text = "OK" };

			form.AcceptButton = ok;
			form.Controls.Add(ok);

			form.Controls.Add(textBox);

			return form;
		});

		var acceptButton = await page.WaitForSelectorAsync(".button");

		var actualBorderStyle = await acceptButton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('border')");
		Assert.That(actualBorderStyle, Is.EqualTo("1px solid rgb(57, 150, 224)"));
	}

	[Test]
	public async Task ButtonClosesModalFormIfResultIsNotNone()
	{
		using var ctx = new WinzorTestContext();
		Form form = null;
		var formClosed = new TaskCompletionSource<bool>();
		Form modalform = null;
		var dispatcherContext = new DispatcherContext(ctx, ctx.DefaultClientServices, OpenFormAction.BlockUntilShown);
		await ctx.RenderFormAsync(() => form = new Form() { Text = "Main Form" });
		var shownTCS = new TaskCompletionSource();
		var showDialogTask = ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			modalform = new Form();
			modalform.Closed += (sender, args) => formClosed.SetResult(true);
			modalform.Shown += (sender, args) => shownTCS.SetResult();
			var button = new Button() { Text = "Ok" };
			button.Click += (sender, args) => modalform.DialogResult = DialogResult.OK;
			modalform.Controls.Add(button);
			using (ctx.WinzorDispatcher.WithContext(dispatcherContext))
			{
				modalform.ShowDialog();
			}
		});
		await shownTCS.Task;
		await dispatcherContext.Rendered.Find("button").ClickAsync(new WebMouseEventArgs());
		await showDialogTask;
		Assert.That(await formClosed.Task.WithTimeout(TimeSpan.FromSeconds(5)), Is.True);
	}

	[Test]
	public async Task ClickOnNonActiveFormsShouldBeIgnored()
	{
		using var ctx = new WinzorTestContext();
		Form form = null;
		Form modalform = null;
		var controlClicked = false;
		var dispatcherContext = new DispatcherContext(ctx, ctx.DefaultClientServices, OpenFormAction.BlockUntilShown);

		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			var button = new Button();
			button.Click += (s, e) => controlClicked = true;
			form.Controls.Add(button);
			return form;
		});

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			modalform = new Form();
			using (ctx.WinzorDispatcher.WithContext(dispatcherContext))
			{
				modalform.Show();
				modalform.Activate();
			}
		});

		Assert.That(Form.ActiveForm, Is.EqualTo(modalform));

		await rendered.Find("button").ClickAsync(new WebMouseEventArgs());

		Assert.That(controlClicked, Is.False);
	}

	static IEnumerable<TestCaseData> SystemColorsTestCases
	{
		get
		{
			yield return new TestCaseData("ActiveBorder", Color.FromArgb(0xB4, 0xB4, 0xB4)) { TestName = "{m}_ActiveBorder" };
			yield return new TestCaseData("ActiveCaption", Color.FromArgb(0x99, 0xB4, 0xD1)) { TestName = "{m}_ActiveCaption" };
			yield return new TestCaseData("ActiveCaptionText", Color.FromArgb(0x00, 0x00, 0x00)) { TestName = "{m}_ActiveCaptionText" };
			yield return new TestCaseData("AppWorkspace", Color.FromArgb(0xAB, 0xAB, 0xAB)) { TestName = "{m}_AppWorkspace" };
			yield return new TestCaseData("ButtonFace", Color.FromArgb(0xF0, 0xF0, 0xF0)) { TestName = "{m}_ButtonFace" };
			yield return new TestCaseData("ButtonHighlight", Color.FromArgb(0xFF, 0xFF, 0xFF)) { TestName = "{m}_ButtonHighlight" };
			yield return new TestCaseData("ButtonShadow", Color.FromArgb(0xA0, 0xA0, 0xA0)) { TestName = "{m}_ButtonShadow" };
			yield return new TestCaseData("Control", Color.FromArgb(0xF0, 0xF0, 0xF0)) { TestName = "{m}_Control" };
			yield return new TestCaseData("ControlDark", Color.FromArgb(0xA0, 0xA0, 0xA0)) { TestName = "{m}_ControlDark" };
			yield return new TestCaseData("ControlDarkDark", Color.FromArgb(0x69, 0x69, 0x69)) { TestName = "{m}_ControlDarkDark" };
			yield return new TestCaseData("ControlLight", Color.FromArgb(0xE3, 0xE3, 0xE3)) { TestName = "{m}_ControlLight" };
			yield return new TestCaseData("ControlLightLight", Color.FromArgb(0xFF, 0xFF, 0xFF)) { TestName = "{m}_ControlLightLight" };
			yield return new TestCaseData("ControlText", Color.FromArgb(0x00, 0x00, 0x00)) { TestName = "{m}_ControlText" };
			yield return new TestCaseData("Desktop", Color.FromArgb(0x00, 0x00, 0x00)) { TestName = "{m}_Desktop" };
			yield return new TestCaseData("GradientActiveCaption", Color.FromArgb(0xB9, 0xD1, 0xEA)) { TestName = "{m}_GradientActiveCaption" };
			yield return new TestCaseData("GradientInactiveCaption", Color.FromArgb(0xD7, 0xE4, 0xF2)) { TestName = "{m}_GradientInactiveCaption" };
			yield return new TestCaseData("GrayText", Color.FromArgb(0x6D, 0x6D, 0x6D)) { TestName = "{m}_GrayText" };
			yield return new TestCaseData("Highlight", Color.FromArgb(0x00, 0x78, 0xD7)) { TestName = "{m}_Highlight" };
			yield return new TestCaseData("HighlightText", Color.FromArgb(0xFF, 0xFF, 0xFF)) { TestName = "{m}_HighlightText" };
			yield return new TestCaseData("HotTrack", Color.FromArgb(0x00, 0x66, 0xCC)) { TestName = "{m}_HotTrack" };
			yield return new TestCaseData("InactiveBorder", Color.FromArgb(0xF4, 0xF7, 0xFC)) { TestName = "{m}_InactiveBorder" };
			yield return new TestCaseData("InactiveCaption", Color.FromArgb(0xBF, 0xCD, 0xDB)) { TestName = "{m}_InactiveCaption" };
			yield return new TestCaseData("InactiveCaptionText", Color.FromArgb(0x00, 0x00, 0x00)) { TestName = "{m}_InactiveCaptionText" };
			yield return new TestCaseData("Info", Color.FromArgb(0xFF, 0xFF, 0xE1)) { TestName = "{m}_Info" };
			yield return new TestCaseData("InfoText", Color.FromArgb(0x00, 0x00, 0x00)) { TestName = "{m}_InfoText" };
			yield return new TestCaseData("Menu", Color.FromArgb(0xF0, 0xF0, 0xF0)) { TestName = "{m}_Menu" };
			yield return new TestCaseData("MenuBar", Color.FromArgb(0xF0, 0xF0, 0xF0)) { TestName = "{m}_MenuBar" };
			yield return new TestCaseData("MenuHighlight", Color.FromArgb(0x33, 0x99, 0xFF)) { TestName = "{m}_MenuHighlight" };
			yield return new TestCaseData("MenuText", Color.FromArgb(0x00, 0x00, 0x00)) { TestName = "{m}_MenuText" };
			yield return new TestCaseData("ScrollBar", Color.FromArgb(0xC8, 0xC8, 0xC8)) { TestName = "{m}_ScrollBar" };
			yield return new TestCaseData("Window", Color.FromArgb(0xFF, 0xFF, 0xFF)) { TestName = "{m}_Window" };
			yield return new TestCaseData("WindowFrame", Color.FromArgb(0x64, 0x64, 0x64)) { TestName = "{m}_WindowFrame" };
			yield return new TestCaseData("WindowText", Color.FromArgb(0x00, 0x00, 0x00)) { TestName = "{m}_WindowText" };
		}
	}

	[TestCaseSource(nameof(SystemColorsTestCases))]
	[Test, WithPlaywrightPage]
	public async Task FormShouldSetInheritableCssVariables(string colorName, Color color)
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var textBox = new TextBox();
			return textBox;
		});

		var textbox = page.Locator(".textbox").First;
		var hyphenatedVariable = colorName.ToLowerHyphen();
		await textbox.EvaluateAsync($"textbox => textbox.style.backgroundColor = 'var(--color-{hyphenatedVariable})'");

		var textboxStyle = await textbox.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-color')");
		Assert.That(textboxStyle, Is.EqualTo($"rgb({color.R}, {color.G}, {color.B})"));
	}

	[Test, WithPlaywrightPage]
	public async Task DeactivateAndActivateFromClient()
	{
		await using var ctx = new InMemoryTestServerContext();
		TextBox textBox = null;
		Form form = null;
		var activatedCount = 0;
		var deactivateCount = 0;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			form.Activated += (_, _) => activatedCount++;
			form.Deactivate += (_, _) => deactivateCount++;
			textBox = new TextBox();
			form.Controls.Add(textBox);
			return form;
		});

		Assert.That(activatedCount, Is.EqualTo(0));
		Assert.That(deactivateCount, Is.EqualTo(0));

		await page.EvaluateAsync("window.dispatchEvent(new Event('blur'));");
		Assert.That(() => form.ActiveControl, Is.EqualTo(textBox).After(3000, 100));
		Assert.That(() => deactivateCount, Is.EqualTo(1).After(3000, 100));

		await page.EvaluateAsync("window.dispatchEvent(new Event('focus'));");
		Assert.That(() => form.ActiveControl, Is.EqualTo(textBox).After(3000, 100));
		Assert.That(() => activatedCount, Is.EqualTo(1).After(3000, 100));
	}

	[Test]
	public async Task ActiveForm()
	{
		Form form = null;
		Panel panel = null;
		TextBox textBox = null;

		Assert.That(Form.ActiveForm, Is.Null);

		using var ctx = new WinzorTestContext();
		await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			panel = new Panel();
			textBox = new TextBox();
			panel.Controls.Add(textBox);
			form.Controls.Add(panel);
			return form;
		});

		Assert.That(Form.ActiveForm, Is.EqualTo(form));

		await AssertActiveFormAfter(() => form.Activate());
		await AssertActiveFormAfter(() => panel.Focus());
		await AssertActiveFormAfter(() => textBox.Focus());

		async Task AssertActiveFormAfter(Action action)
		{
			await form.InvokeWinzorDispatcherAsync(() =>
			{
				form.Hide();
				form.Visible = true;
				action();
			});
			Assert.That(Form.ActiveForm, Is.EqualTo(form));
		}
	}

	[Test]
	public async Task FormShouldUseClientSizeForSizeStyle()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() => new Form { ClientSize = new Size(100, 100) });
		var formStyleString = rendered.Find(".form").GetAttribute("style");
		Assert.That(formStyleString, Does.Contain("width:100px;height:100px;"));
	}

	[Test]
	public async Task FormShouldChangeClientSizeOnResize()
	{
		Form form = null;
		using var ctx = new WinzorTestContext();
		await ctx.RenderFormAsync(() => form = new Form { ClientSize = new Size(100, 100) });
		await form.OnBrowserSizeChangedAsync(200, 200);
		Assert.That(form.ClientSize, Is.EqualTo(new Size(200, 200)));
	}

	[Test]
	public async Task FormShouldChangeStateAfterCallOnWindowStateChangeAsync()
	{
		Form form = null;
		using var ctx = new WinzorTestContext();
		await ctx.RenderFormAsync(() => form = new Form { WindowState = FormWindowState.Minimized });
		await form.OnWindowStateChangeAsync("Normal");
		Assert.That(form.WindowState, Is.EqualTo(FormWindowState.Normal));
	}

	[TestCase(false, FormBorderStyle.None)]
	[TestCase(false, FormBorderStyle.FixedSingle)]
	[TestCase(false, FormBorderStyle.FixedDialog)]
	[TestCase(false, FormBorderStyle.Fixed3D)]
	[TestCase(false, FormBorderStyle.FixedToolWindow)]
	[TestCase(true, FormBorderStyle.Sizable)]
	[TestCase(true, FormBorderStyle.SizableToolWindow)]
	public async Task SizingGripShownGivenResizable(bool isSizingShown, FormBorderStyle borderStyle)
	{
		Form form = null;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() => form = new Form { SizeGripStyle = SizeGripStyle.Show, FormBorderStyle = borderStyle, ClientSize = new Size(100, 100) });

		AngleSharp.Dom.IElement sizingGrip = null;

		try
		{
			sizingGrip = rendered.Find(".sizing-grip");
		}
		catch (Exception)
		{
		}

		Assert.That(sizingGrip, isSizingShown ? !Is.Null : Is.Null);
	}

	[Test]
	public async Task FormBorderStyleChangedShouldTriggerOnResizeEvent()
	{
		Form form = null;
		using var ctx = new WinzorTestContext();
		await ctx.RenderFormAsync(() => form = new Form { FormBorderStyle = FormBorderStyle.Sizable });
		Assert.That(form.FormBorderStyle, Is.EqualTo(FormBorderStyle.Sizable));

		var resizeEventTriggered = false;
		form.Resize += (_, _) => resizeEventTriggered = true;
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			form.FormBorderStyle = FormBorderStyle.FixedToolWindow;
		});
		Assert.That(form.FormBorderStyle, Is.EqualTo(FormBorderStyle.FixedToolWindow));
		Assert.That(resizeEventTriggered, Is.True);
	}

	[Test]
	public async Task TestFormShouldRequestCloseWhenCallingDispose()
	{
		using var ctx = new WinzorTestContext();
		var windowService = new Mock<IWindowService>();
		var clientServices = MockCargoWiseClientServices.MakeMock(windowService: windowService.Object);
		FormForTest testForm = null;
		await ctx.RenderFormAsync(() => testForm = new FormForTest(), clientServices);
		testForm.Dispose();
		windowService.Verify(w => w.RequestCloseAsync(), Times.Once);
	}

	[Test, WithPlaywrightPage]
	public async Task TestPressAltKeyTwiceWillNotProduceErrorMessage()
	{
		await using var ctx = new InMemoryTestServerContext();
		var text = string.Empty;

		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var button = new Button { Text = "&Test" };
			return button;
		});

		var mnemonicKey = await page.WaitForSelectorAsync(".mnemonickey");
		Assert.That(mnemonicKey, Is.Not.Null);

		page.Console += (_, msg) =>
		{
			if ("error".Equals(msg.Type))
			{
				text = msg.Text;
			}
		};

		await page.Keyboard.DownAsync("Alt");
		await page.Keyboard.DownAsync("Alt");

		Assert.That(async () => (await mnemonicKey.GetComputedStyleAsync("text-decoration")).ToString(), Does.Contain("underline").After(3000, 100));

		Assert.That(text, Is.EqualTo(string.Empty));
	}

	/// <summary>
	/// It is impossible to trigger zooming through PlayWright at the moment - https://github.com/microsoft/playwright/issues/2497
	/// So, in this test we do not test the actual zoom value, but the fact that our event handler is triggered by <CTRL> + <Wheel>
	/// </summary>
	/// <returns></returns>
	[Test, WithPlaywrightPage]
	public async Task TestCtrlMouseWheelUpDownHandled()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var panel = new Panel();
			var label = new Label()
			{
				Text = "Test text."
			};
			panel.Controls.Add(label);
			form.Controls.Add(panel);
			return form;
		});

		Assert.That(async () => await page.EvaluateAsync<bool>("form.isZoomAttempted"), Is.EqualTo(false).After(1000, 100));
		await page.Keyboard.DownAsync("Control");
		await page.Mouse.WheelAsync(0, 50);
		await page.Keyboard.UpAsync("Control");
		Assert.That(async () => await page.EvaluateAsync<bool>("form.isZoomAttempted"), Is.EqualTo(true).After(1000, 100));
	}

	[Test]
	public async Task TestOnClientSizeChangedEventDispatcher()
	{
		var eventInvokedCount = 0;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync<Form>();
		var form = rendered.GetForm();

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			form.ClientSizeChanged += new EventHandler((sender, e) =>
			{
				eventInvokedCount++;
			});
		});

		Assert.That(eventInvokedCount, Is.EqualTo(0));

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			form.ClientSize = new Size(1, 1);
		});
		Assert.That(eventInvokedCount, Is.EqualTo(1));
	}

	[Test]
	public async Task FormMinimumSize_CalledSetFormSizeOnClient()
	{
		using var ctx = new WinzorTestContext();

		Size? preferUnscaledMinimumSize = Size.Empty;
		MockWindowService_UpdateWindowStyle(ctx, (WindowStyleOptions windowStyleOptions) =>
		{
			preferUnscaledMinimumSize = windowStyleOptions.PreferredMinimumSizeUnscaled;
		});

		Form form = null;
		var rendered = await ctx.RenderFormAsync(() => form = new Form());

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			form.MinimumSize = new Size(100, 100);
		});
		Assert.That(preferUnscaledMinimumSize, Is.EqualTo(new Size(100, 100)));
	}

	[Test]
	public async Task FormMaximumSize_CalledSetFormSizeOnClient()
	{
		using var ctx = new WinzorTestContext();

		Size? preferUnscaledMaximumSize = Size.Empty;
		MockWindowService_UpdateWindowStyle(ctx, (WindowStyleOptions windowStyleOptions) =>
		{
			preferUnscaledMaximumSize = windowStyleOptions.PreferredMaximumSizeUnscaled;
		});

		Form form = null;
		var rendered = await ctx.RenderFormAsync(() => form = new Form());

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			form.MaximumSize = new Size(100, 100);
		});
		Assert.That(preferUnscaledMaximumSize, Is.EqualTo(new Size(100, 100)));
	}

	[TestCase(FormBorderStyle.None, FormBorderStyle.None)]
	[TestCase(FormBorderStyle.FixedSingle, FormBorderStyle.FixedSingle)]
	[TestCase(FormBorderStyle.Fixed3D, FormBorderStyle.Fixed3D)]
	[TestCase(FormBorderStyle.FixedDialog, FormBorderStyle.FixedDialog)]
	[TestCase(FormBorderStyle.Sizable, FormBorderStyle.FixedSingle)]
	[TestCase(FormBorderStyle.FixedToolWindow, FormBorderStyle.FixedToolWindow)]
	[TestCase(FormBorderStyle.SizableToolWindow, FormBorderStyle.FixedToolWindow)]
	public async Task FormAutoSizeMode_CanUpdateFormStyles(FormBorderStyle formBorderStyle, BlazorMessaging.FormBorderStyle expected)
	{
		using var ctx = new WinzorTestContext();

		var preferUnscaledMaximumSize = Size.Empty;

		BlazorMessaging.FormBorderStyle? updatedFormBorderStyle = default;
		MockWindowService_UpdateWindowStyle(ctx, (WindowStyleOptions windowStyleOptions) =>
		{
			updatedFormBorderStyle = windowStyleOptions.FormBorderStyle;
		});

		Form form = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			form.FormBorderStyle = formBorderStyle;
			form.AutoSizeMode = AutoSizeMode.GrowOnly;
			return form;
		});

		await (form as IHandleAfterRender).OnAfterRenderAsync();
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			form.AutoSizeMode = AutoSizeMode.GrowAndShrink;
		});
		await (form as IHandleAfterRender).OnAfterRenderAsync();

		Assert.That(updatedFormBorderStyle, Is.EqualTo(expected));
	}

	[Test]
	public async Task FormAutoSizeModeChanged_CanSetFormSizeOnClient()
	{
		using var ctx = new WinzorTestContext();

		Size? preferUnscaledMinimumSize = Size.Empty, preferUnscaledMaximumSize = Size.Empty;
		MockWindowService_UpdateWindowStyle(ctx, (WindowStyleOptions windowStyleOptions) =>
		{
			preferUnscaledMinimumSize = windowStyleOptions.PreferredMinimumSizeUnscaled;
			preferUnscaledMaximumSize = windowStyleOptions.PreferredMaximumSizeUnscaled;
		});

		Form form = null;
		Button button = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			form.Padding = new Padding(0);
			button = new Button()
			{
				Width = 100,
				Left = 20,
				Height = 50,
				Top = 30,
				Margin = new Padding(0),
			};
			form.Controls.Add(button);

			form.AutoSizeMode = AutoSizeMode.GrowOnly;
			form.AutoSize = true;
			return form;
		});
		Size expectSize = new Size(button.Bounds.Right, button.Bounds.Bottom);

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			form.AutoSizeMode = AutoSizeMode.GrowAndShrink;
		});
		await (form as IHandleAfterRender).OnAfterRenderAsync();
		Assert.That(preferUnscaledMinimumSize, Is.EqualTo(expectSize));
		Assert.That(preferUnscaledMaximumSize, Is.EqualTo(expectSize));

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			form.AutoSizeMode = AutoSizeMode.GrowOnly;
		});
		await (form as IHandleAfterRender).OnAfterRenderAsync();
		Assert.That(preferUnscaledMinimumSize, Is.EqualTo(expectSize));
		Assert.That(preferUnscaledMaximumSize, Is.EqualTo(Size.Empty));
	}

	[TestCase(AutoSizeMode.GrowOnly)]
	[TestCase(AutoSizeMode.GrowAndShrink)]
	public async Task FormAutoSize_CanSetFormSizeOnClient(AutoSizeMode autoSizeMode)
	{
		using var ctx = new WinzorTestContext();

		Size? preferUnscaledMinimumSize = Size.Empty, preferUnscaledMaximumSize = Size.Empty;
		MockWindowService_UpdateWindowStyle(ctx, (WindowStyleOptions windowStyleOptions) =>
		{
			preferUnscaledMinimumSize = windowStyleOptions.PreferredMinimumSizeUnscaled;
			preferUnscaledMaximumSize = windowStyleOptions.PreferredMaximumSizeUnscaled;
		});

		Form form = null;
		Button button = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			form.Padding = new Padding(0);
			button = new Button()
			{
				Width = 100,
				Left = 20,
				Height = 50,
				Top = 30,
				Margin = new Padding(0),
			};
			form.Controls.Add(button);

			form.AutoSizeMode = autoSizeMode;
			form.AutoSize = false;
			return form;
		});
		Size expectSize = new Size(button.Bounds.Right, button.Bounds.Bottom);

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			form.AutoSize = true;
		});
		await (form as IHandleAfterRender).OnAfterRenderAsync();

		Assert.That(preferUnscaledMinimumSize, Is.EqualTo(new Size(120, 80)));
		if (autoSizeMode == AutoSizeMode.GrowAndShrink)
		{
			Assert.That(preferUnscaledMaximumSize, Is.EqualTo(new Size(120, 80)));
		}
		else
		{
			Assert.That(preferUnscaledMaximumSize, Is.EqualTo(Size.Empty));
		}
	}

	[Test]
	public async Task FormRaisingExceptionInDisposeShouldCreateErrorReport()
	{
		using var ctx = new WinzorTestContext();
		var developerException = new TaskCompletionSource<Exception>();
		var threadException = new TaskCompletionSource<Exception>();
		ctx.ThreadExceptionExceptionRaised += (args) => threadException.TrySetResult(args);

		var rendered = await ctx.RenderFormAsync(() => new FormWithDisposeException());
		var form = rendered.GetForm();
		await form.InvokeWinzorDispatcherAsync(form.Close);

		Assert.That(threadException.Task.IsCompleted, Is.True);
		Assert.That(await threadException.Task, Is.TypeOf<Exception>());
	}

	[Test]
	public async Task TestOrderOfEventsInForm()
	{
		using var ctx = new WinzorTestContext();
		var eventOrder = 0;

		var renderedForm = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();

			form.HandleCreated += (_, _) => Assert.That(++eventOrder, Is.EqualTo(1));
			form.Load += (_, _) => Assert.That(++eventOrder, Is.EqualTo(2));
			form.VisibleChanged += (_, _) => Assert.That(++eventOrder, Is.EqualTo(3));
			form.Shown += (_, _) => Assert.That(++eventOrder, Is.EqualTo(4));

			form.Closing += (_, _) => Assert.That(++eventOrder, Is.EqualTo(5));
			form.FormClosing += (_, _) => Assert.That(++eventOrder, Is.EqualTo(6));
			form.Closed += (_, _) => Assert.That(++eventOrder, Is.EqualTo(7));
			form.FormClosed += (_, _) => Assert.That(++eventOrder, Is.EqualTo(8));
			form.Deactivate += (_, _) => Assert.That(++eventOrder, Is.EqualTo(9));

			var button = new Button();
			button.Click += (_, _) => form.Close();
			form.Controls.Add(button);
			return form;
		});

		renderedForm.Find("button").Click();
	}

	[Test, WithPlaywrightPage]
	public async Task TestArrowKeyOnKeyDown()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var button1 = new Button { Text = "button1", Location = new Point(0, 0) };
			var button2 = new Button { Text = "button2", Location = new Point(0, 100) };

			var form = new Form();
			form.Controls.Add(button1);
			form.Controls.Add(button2);
			form.ActiveControl = button1;
			return form;
		});

		var firstButton = page.Locator("button", new PageLocatorOptions { HasText = "button1" });
		var secondButton = page.Locator("button", new PageLocatorOptions { HasText = "button2" });
		Assert.That(async () => await firstButton.EvaluateAsync<bool>("element => document.activeElement === element"), Is.True.After(1000, 100));

		//RightArrowKey
		await (page.Locator("body")).PressAsync("ArrowRight");
		Assert.That(async () => await firstButton.EvaluateAsync<bool>("element => document.activeElement === element"), Is.False.After(1000, 100));
		Assert.That(async () => await secondButton.EvaluateAsync<bool>("element => document.activeElement === element"), Is.True.After(1000, 100));

		await (page.Locator("body")).PressAsync("ArrowRight");
		Assert.That(async () => await firstButton.EvaluateAsync<bool>("element => document.activeElement === element"), Is.True.After(1000, 100));
		Assert.That(async () => await secondButton.EvaluateAsync<bool>("element => document.activeElement === element"), Is.False.After(1000, 100));

		//LeftArrowKey
		await (page.Locator("body")).PressAsync("ArrowLeft");
		Assert.That(async () => await firstButton.EvaluateAsync<bool>("element => document.activeElement === element"), Is.False.After(1000, 100));
		Assert.That(async () => await secondButton.EvaluateAsync<bool>("element => document.activeElement === element"), Is.True.After(1000, 100));

		await (page.Locator("body")).PressAsync("ArrowLeft");
		Assert.That(async () => await firstButton.EvaluateAsync<bool>("element => document.activeElement === element"), Is.True.After(1000, 100));
		Assert.That(async () => await secondButton.EvaluateAsync<bool>("element => document.activeElement === element"), Is.False.After(1000, 100));
	}

	[Test]
	public async Task FormNotRaisingExceptionInDisposeShouldNotCreateErrorReport()
	{
		using var ctx = new WinzorTestContext();
		var developerException = new TaskCompletionSource<Exception>();
		var threadException = new TaskCompletionSource<Exception>();
		ctx.DeveloperExceptionRaised += (args) => developerException.TrySetResult(args);
		ctx.ThreadExceptionExceptionRaised += (args) => threadException.TrySetResult(args);

		var rendered = await ctx.RenderFormAsync(() => new Form());
		var form = rendered.GetForm();
		await form.InvokeWinzorDispatcherAsync(form.Close);

		Assert.That(developerException.Task.IsCompleted, Is.False);
		Assert.That(threadException.Task.IsCompleted, Is.False);
	}

	[Test]
	public async Task ModalFormNotRaisingExceptionInDisposeShouldNotReportDeveloperException()
	{
		using var ctx = new WinzorTestContext();
		var developerException = new TaskCompletionSource<Exception>();
		var threadException = new TaskCompletionSource<Exception>();
		ctx.DeveloperExceptionRaised += (args) => developerException.TrySetResult(args);
		ctx.ThreadExceptionExceptionRaised += (args) => threadException.TrySetResult(args);

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			return form;
		});
		var form = rendered.GetForm();
		await form.InvokeWinzorDispatcherAsync(form.Close);

		Assert.That(developerException.Task.IsCompleted, Is.False);
		Assert.That(threadException.Task.IsCompleted, Is.False);
	}

	[Test]
	public async Task FormRaisingExceptionInDisposeShouldHaveDisposedEventsInvoked()
	{
		using var ctx = new WinzorTestContext();
		var disposedMock = new Mock<EventHandler>();

		var threadException = new TaskCompletionSource<Exception>();
		ctx.ThreadExceptionExceptionRaised += (args) => threadException.TrySetResult(args);

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new FormWithDisposeException();
			form.Disposed += disposedMock.Object;
			return form;
		});
		var form = rendered.GetForm();
		await form.InvokeWinzorDispatcherAsync(form.Close);

		disposedMock.Verify(f => f(It.IsAny<Form>(), It.IsAny<EventArgs>()), Times.Once());

		Assert.That(async () => await threadException.Task, Is.TypeOf<Exception>());
	}

	[Test]
	public async Task ShouldFireAllEventsOnClose()
	{
		using var ctx = new WinzorTestContext();
		var raisedClosing = false;
		var raisedFormClosing = false;
		var raisedClosed = false;
		var raisedFormClosed = false;
		var raisedDisposed = false;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Closing += (sender, args) => raisedClosing = true;
			form.FormClosing += (sender, args) => raisedFormClosing = true;
			form.Closed += (sender, args) => raisedClosed = true;
			form.FormClosed += (sender, args) => raisedFormClosed = true;
			form.Disposed += (sender, args) => raisedDisposed = true;
			return form;
		});
		var form = rendered.GetForm();

		await form.InvokeWinzorDispatcherAsync(() => form.Close());
		Assert.That(raisedClosing, Is.True);
		Assert.That(raisedFormClosing, Is.True);
		Assert.That(raisedClosed, Is.True);
		Assert.That(raisedFormClosed, Is.True);
		Assert.That(raisedDisposed, Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task SetFormSizeFromServerSideShouldTriggerOnBrowserSizeChangedAsync()
	{
		FormForTest form = null;
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new FormForTest();
			return form;
		});

		await page.WaitForSelectorAsync(".form");

		form.TriggerOnBrowserSizeChangedAsyncCount = 0;
		await form.InvokeWinzorDispatcherAsync(() => form.Size = new Size(400, 400));
		await page.SetViewportSizeAsync(300, 300);
		await Task.Delay(2000); // Waiting for the page to finish resizing
		Assert.That(form.TriggerOnBrowserSizeChangedAsyncCount, Is.EqualTo(1));

		await page.SetViewportSizeAsync(500, 500);
		await Task.Delay(2000);
		Assert.That(form.TriggerOnBrowserSizeChangedAsyncCount, Is.EqualTo(2));
	}

	[Test]
	public async Task FormCloseReasonOnFormClosedShouldBeUserClosingWhenShowAndClose()
	{
		Form form = null;
		var closeReasonOnFormClosed = CloseReason.None;
		using var ctx = new WinzorTestContext();
		var dispatcherContext = new DispatcherContext(ctx, ctx.DefaultClientServices, OpenFormAction.None);
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			form = new Form();
			form.FormClosed += (sender, e) => closeReasonOnFormClosed = e.CloseReason;
			using (ctx.WinzorDispatcher.WithContext(dispatcherContext))
			{
				form.Show();
			}
		});

		Assert.That(() => dispatcherContext.Rendered, Is.Not.Null.After(1000));

		await form.InvokeWinzorDispatcherAsync(() => form.Close());
		Assert.That(closeReasonOnFormClosed, Is.EqualTo(CloseReason.UserClosing));
	}

	[Test]
	public async Task FormMinimizeBoxShouldBeTrueByDefaultAndSetCorrectly()
	{
		using var ctx = new WinzorTestContext();
		Form form = null;
		var windowService = new Mock<IWindowService>();
		var clientServices = MockCargoWiseClientServices.MakeMock(windowService: windowService.Object);
		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			Assert.That(form.MinimizeBox, Is.True, "Form.MinimizeBox should be true by default.");
			return form;
		}, clientServices);

		Assert.That(form.MinimizeBox, Is.True);

		await TriggerMinimizeBoxChangeAndAssertTriggerCount(form, windowService, false, 1);
		await TriggerMinimizeBoxChangeAndAssertTriggerCount(form, windowService, false, 0);
		await TriggerMinimizeBoxChangeAndAssertTriggerCount(form, windowService, true, 1);
		await TriggerMinimizeBoxChangeAndAssertTriggerCount(form, windowService, true, 0);
	}

	async Task TriggerMinimizeBoxChangeAndAssertTriggerCount(Form form, Mock<IWindowService> windowService, bool minimizeBoxValue, int expectedTriggerCount)
	{
		windowService.Invocations.Clear();
		await form.InvokeWinzorDispatcherAsync(() => form.MinimizeBox = minimizeBoxValue);
		Assert.That(form.MinimizeBox, minimizeBoxValue ? Is.True : Is.False);
		windowService.Verify(c => c.RequestUpdateWindowStyleAsync(It.Is<WindowStyleOptions>(s => s.MinimizeBox == minimizeBoxValue)), Times.Exactly(expectedTriggerCount));
	}

	[Test, WithPlaywrightPage]
	public async Task FormShouldHaveScrollBar_WhenClientSizeIsSmallerThanMinimumSize([Values(300, 700)] int proposedWidth, [Values(300, 700)] int proposedHeight)
	{
		Form form = null;
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form()
			{
				MinimumSize = new Size(500, 500),
				Width = 700,
				Height = 700
			};
			form.Controls.Add(new Panel()
			{
				Dock = DockStyle.Fill,
			});
			return form;
		});

		await page.SetViewportSizeAsync(proposedWidth, proposedHeight);

		var formElement = page.Locator(".form");
		if (proposedWidth < form.MinimumSize.Width)
		{
			Assert.That(async () => await formElement.GetComputedStyleAsync("overflow"), Is.EqualTo("auto").After(3000, 100));
			Assert.That(async () => await formElement.EvaluateAsync<int>("e => e.clientWidth"), Is.LessThan(await formElement.EvaluateAsync<int>("e => e.offsetWidth")).After(3000, 100));
		}

		if (proposedHeight < form.MinimumSize.Height)
		{
			Assert.That(async () => await formElement.GetComputedStyleAsync("overflow"), Is.EqualTo("auto").After(3000, 100));
			Assert.That(async () => await formElement.EvaluateAsync<int>("e => e.clientHeight"), Is.LessThan(await formElement.EvaluateAsync<int>("e => e.offsetHeight")).After(3000, 100));
		}

		await page.SetViewportSizeAsync(700, 700);
		Assert.That(async () => await formElement.GetComputedStyleAsync("overflow"), Is.EqualTo("hidden").After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task FormShouldNotHaveScrollBar_WhenClientSizeIsEqualToOrGreaterThanMinimumSize([Values(500, 700)] int proposedWidth, [Values(500, 700)] int proposedHeight, [Values(1, 1.25, 1.5, 1.75, 2, 2.25)] double devicePixelRatio)
	{
		Form form = null;
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form()
			{
				MinimumSize = new Size(500, 500),
				Width = 700,
				Height = 700
			};
			form.Controls.Add(new Panel()
			{
				Dock = DockStyle.Fill,
			});
			return form;
		});

		await page.EvaluateAsync($"() => {{window.devicePixelRatio = {devicePixelRatio}}}");

		await page.SetViewportSizeAsync(proposedWidth, proposedHeight);

		var formElement = page.Locator(".form");

		Assert.That(async () => await formElement.GetComputedStyleAsync("overflow"), Is.EqualTo("hidden").After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task FormKeyEventsShouldBeInvokedOnLastFocusedControl_WhenTargetControlIsNotFound()
	{
		await using var ctx = new InMemoryTestServerContext();
		var control = default(Control);
		var form = default(Form);
		var controlId = default(string);
		var formId = default(string);
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			control = new Control() { Width = 100, Height = 100 };
			control.KeyUp += (sender, e) => controlId = (sender as Control).WinzorControlId;
			form.KeyUp += (sender, e) => formId = (sender as Control).WinzorControlId;
			form.Controls.Add(control);
			return form;
		});

		var controlLocator = page.Locator(".form > div");
		var winzorControlId = await controlLocator.GetAttributeAsync("data-winzor-control-id");
		Assert.That(winzorControlId, Is.EqualTo(form.LastFocusedControl.WinzorControlId));

		await page.EvaluateAsync("document.querySelector('.form > div').setAttribute('data-winzor-control-id', 'invalid_id');");
		Assert.That(await controlLocator.GetAttributeAsync("data-winzor-control-id"), Is.EqualTo("invalid_id"));

		await controlLocator.PressAsync("A");
		Assert.That(() => controlId, Is.EqualTo(control.WinzorControlId).After(1000, 100));
		Assert.That(() => formId, Is.Null);
	}

	[Test, WithPlaywrightPage]
	public async Task FormKeyEventsShouldBeInvokedOnCurrentForm_WhenTargetControlIsNotFoundAndLastFocusedControlIsNull()
	{
		await using var ctx = new InMemoryTestServerContext();
		var form = default(Form);
		var controlId = default(string);
		var formId = default(string);
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var control = new Control() { Width = 100, Height = 100 };
			control.KeyUp += (sender, e) => controlId = (sender as Control).WinzorControlId;
			form.KeyUp += (sender, e) => formId = (sender as Control).WinzorControlId;
			form.Controls.Add(control);
			return form;
		});

		var controlLocator = page.Locator(".form > div");
		await controlLocator.DispatchEventAsync("focusout");
		Assert.That(() => form.LastFocusedControl, Is.Null.After(1000, 100));

		await page.Keyboard.PressAsync("A");
		Assert.That(() => controlId, Is.Null);
		Assert.That(() => formId, Is.EqualTo(form.WinzorControlId).After(1000, 100));
	}

	void MockWindowService_UpdateWindowStyle(WinzorTestContext ctx, Action<WindowStyleOptions> callback)
	{
		ctx.MockCargoWiseClientServices.WindowService.Setup(s => s.RequestUpdateWindowStyleAsync(It.IsAny<WindowStyleOptions>())).Callback(callback);
	}

	internal class FormForTest : Form
	{
		internal Task RequestCloseTask { get; set; }

		internal int TriggerOnBrowserSizeChangedAsyncCount { get; set; }

		[JSInvokable]
		public override Task OnBrowserSizeChangedAsync(int jsBrowserWidth, int jsBrowserHeight)
		{
			TriggerOnBrowserSizeChangedAsyncCount++;
			return base.OnBrowserSizeChangedAsync(jsBrowserWidth, jsBrowserHeight);
		}
	}

	class FormWithDisposeException : Form
	{
		bool exceptionThrown;

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing && !exceptionThrown)
			{
				exceptionThrown = true;
				throw new Exception("I do not wish to be disposed");
			}
		}
	}

	class DummyForm : Form
	{
		public bool ShowMnemonicKeys() => showMnemonicKeys;
	}

	[Test, WithPlaywrightPage]
	public async Task TestFormResizeShouldHideScroll()
	{
		await using var ctx = new InMemoryTestServerContext();
		var text = string.Empty;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			return form;
		});

		var formElement = await page.WaitForSelectorAsync(".form");

		await formElement.EvaluateAsync(@"(element) => {
			element.style.width = '800px'; 
			element.style.height = '600px'; 
		}");

		var clientHeight = await formElement.EvaluateAsync<int>("e => e.clientHeight");
		var offsetHeight = await formElement.EvaluateAsync<int>("e => e.offsetHeight");
		var scrollHeight = await formElement.EvaluateAsync<int>("e => e.scrollHeight");

		var clientWidth = await formElement.EvaluateAsync<int>("e => e.clientWidth");
		var offsetWidth = await formElement.EvaluateAsync<int>("e => e.offsetWidth");
		var scrollWidth = await formElement.EvaluateAsync<int>("e => e.scrollWidth");

		Assert.That(scrollHeight, Is.EqualTo(clientHeight));
		Assert.That(scrollHeight, Is.EqualTo(offsetHeight));

		Assert.That(scrollWidth, Is.EqualTo(clientWidth));
		Assert.That(scrollWidth, Is.EqualTo(offsetWidth));
	}

	[Test]
	public async Task KeyEventsRedirectToActiveFormAfterRendered()
	{
		Form form = null;
		Form childForm = null;
		var parentFormKeyDownEventCount = 0;
		var childFormKeyDownEventCount = 0;

		using var ctx = new WinzorTestContext();
		var parentRendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form();

			form.KeyDown += (s, e) =>
			{
				parentFormKeyDownEventCount++;
			};

			return form;
		});

		var childRendered = await ctx.RenderFormAsync(() =>
		{
			childForm = new Form();

			childForm.KeyDown += (s, e) =>
			{
				childFormKeyDownEventCount++;
			};

			return childForm;
		});

		await parentRendered.KeyPressAsync(Keys.Enter, parentRendered.Find(".form"));
		Assert.That(parentFormKeyDownEventCount, Is.EqualTo(0));
		Assert.That(childFormKeyDownEventCount, Is.EqualTo(1));
	}

	[Test]
	public async Task KeyEventsNotRedirectToActiveFormBeforeRendered()
	{
		Form form = null;
		Form childForm = null;
		var parentFormKeyDownEventCount = 0;
		var childFormKeyDownEventCount = 0;

		using var ctx = new WinzorTestContext();
		var parentRendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form();

			form.KeyDown += (s, e) =>
			{
				parentFormKeyDownEventCount++;
			};

			return form;
		});

		var childRendered = await ctx.ShowFormWithoutRenderAsync(() =>
		{
			childForm = new Form();

			childForm.KeyDown += (s, e) =>
			{
				childFormKeyDownEventCount++;
			};

			return childForm;
		});

		await parentRendered.KeyPressAsync(Keys.Enter, parentRendered.Find(".form"));
		Assert.That(parentFormKeyDownEventCount, Is.EqualTo(0));
		Assert.That(childFormKeyDownEventCount, Is.EqualTo(0));
	}

	[Test]
	public async Task TestRaiseFormClosedOnAppExit()
	{
		using var ctx = new WinzorTestContext();
		Form form = null;
		var formClosedRaised = new TaskCompletionSource<FormClosedEventArgs>();
		void formClosedRaisedHandler(object sender, FormClosedEventArgs e)
		{
			formClosedRaised.SetResult(e);
			form.FormClosed -= formClosedRaisedHandler;
		}

		await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			form.FormClosed += formClosedRaisedHandler;
			return form;
		});
		await form.InvokeWinzorDispatcherAsync(() => Application.Exit());
		var args = await formClosedRaised.Task;
		Assert.That(args.CloseReason, Is.EqualTo(CloseReason.ApplicationExitCall));
	}

	[Test, WithPlaywrightPage]
	public async Task SizeChangeTriggerWhenFormInitialized([Values] bool hasMenu)
	{
		await using var ctx = new InMemoryTestServerContext();
		FormForTest form = null;
		var formSize = new Size(1300, 1000);
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new FormForTest { Width = formSize.Width, Height = formSize.Height };
			form.MainMenuStrip = hasMenu ? new MenuStrip() : null;
			Assert.That(form.TriggerOnBrowserSizeChangedAsyncCount, Is.EqualTo(0));
			return form;
		});

		Assert.That(() => form.Size.Width, Is.EqualTo(hasMenu ? formSize.Width : page.ViewportSize.Width).After(3000, 100));
		Assert.That(() => form.Size.Height, Is.EqualTo(hasMenu ? formSize.Height : page.ViewportSize.Height).After(3000, 100));
		Assert.That(form.TriggerOnBrowserSizeChangedAsyncCount, Is.EqualTo(hasMenu ? 0 : 1));
	}

	[Test]
	public async Task LongActionHandlerDisposeAsync_ShouldNotThrowWhenJSDisconnectedExceptionHappened()
	{
		using var ctx = new WinzorTestContext();
		Form form = null;
		await ctx.WinzorDispatcher.InvokeAsync(() => form = new Form());
		var unsetNotRespondingMock = new Mock<IAsyncDisposable>();

		unsetNotRespondingMock.Setup(u => u.DisposeAsync()).ThrowsAsync(new JSDisconnectedException("Circuit is disconnected."));

		await using var handler = new LongActionHandler(form);
		typeof(LongActionHandler)
			.GetField("unsetNotResponding", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
			?.SetValue(handler, unsetNotRespondingMock.Object);

		Assert.DoesNotThrowAsync(async () => await handler.DisposeAsync());
		unsetNotRespondingMock.Verify(u => u.DisposeAsync(), Times.Once);
	}

	[Test]
	public async Task ShowFormMultipleTimesShouldNotRaiseException()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() => new Form());
		var form = rendered.GetForm();
		await rendered.Instance.DisposeAsync();

		Assert.That(async () => await ctx.RenderFormAsync(() => form), Throws.Nothing);
	}

	[Test]
	public async Task ShowFormMultipleTimesShouldOnlyOpenOnce()
	{
		using var ctx = new WinzorTestContext();
		var dispatcherContext = new Mock<IWinzorDispatcherContext>();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			using (ctx.WinzorDispatcher.WithContext(dispatcherContext.Object))
			{
				var form = new Form();
				form.Show();
				form.Show();
				form.Show();
			}
		});
		Assert.That(() => dispatcherContext.Verify(c => c.OpenForm(It.IsAny<Form>()), Times.Once), Throws.Nothing);
	}

	[Test]
	public async Task TestVisibilityWhenReopeningHiddenForm()
	{
		using var ctx = new WinzorTestContext();
		Form form = null;
		Button button = null;
		await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			button = new Button();
			form.Controls.Add(button);
			return form;
		});

		Assert.That(form.Visible, Is.True);
		Assert.That(button.Visible, Is.True);
		await form.InvokeWinzorDispatcherAsync(() => form.Hide());
		Assert.That(form.Visible, Is.False);
		Assert.That(button.Visible, Is.False);

		await form.InvokeWinzorDispatcherAsync(() => form.Show());
		Assert.That(form.Visible, Is.True);
		Assert.That(button.Visible, Is.True);
	}

	[Test]
	public async Task FormShowShouldDeferOnShownDelegateToMessageQueue()
	{
		using var ctx = new WinzorTestContext();
		var dispatcherContext = new DispatcherContext(ctx, ctx.DefaultClientServices, OpenFormAction.None);
		var shownEventRaised = new TaskCompletionSource<bool>();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var isInsideFormShowDispatcherInvoke = true;
			var form = new Form();
			form.Shown += (sender, e) => shownEventRaised.SetResult(isInsideFormShowDispatcherInvoke);
			using (ctx.WinzorDispatcher.WithContext(dispatcherContext))
			{
				form.ReadyToRender();
				form.Show();
			}
			isInsideFormShowDispatcherInvoke = false;
		});
		Assert.That(async () => await shownEventRaised.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		Assert.That(async () => await shownEventRaised.Task, Is.False);
	}

	[Test]
	public async Task ShowDialogOnSameDispatcherShouldRequestModalWindow()
	{
		using var ctx = new WinzorTestContext();
		var contextForm = (await ctx.RenderFormAsync(() => new Form())).GetForm();

		var dispatcherContext = new DispatcherContext(ctx, ctx.DefaultClientServices, OpenFormAction.None, contextForm);
		var showDialogTask = ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			using (ctx.WinzorDispatcher.WithContext(dispatcherContext))
			{
				using var form = new Form();
				form.ShowDialog();
			}
		});
		Assert.That(() => dispatcherContext.Rendered, Is.Not.Null.After(3000, 100));
		var form = dispatcherContext.Rendered.GetForm();

		Assert.That(form.Modal, Is.True);
		Assert.That(form.ModalWindow, Is.True);

		await form.InvokeWinzorDispatcherAsync(form.Close);
		await showDialogTask;
	}

	[Test]
	public async Task ShowDialogOnDifferentDispatcherShouldRequestNonModalWindow()
	{
		using var ctx = new WinzorTestContext();
		var contextForm = (await ctx.RenderFormAsync(() => new Form())).GetForm();

		Task runInAnotherWinFormsThreadAsync = null;
		var dispatcherContext = new DispatcherContext(ctx, ctx.DefaultClientServices, OpenFormAction.None, contextForm);

		using var winzorDispatcher = new WinzorDispatcher(
				ctx.WinzorDispatcher.FormOpener,
				ctx.WinzorDispatcher.FormInstanceRegister,
				"threadName", isBackgroundThread: true);
		runInAnotherWinFormsThreadAsync = winzorDispatcher.InvokeAsync(() =>
		{
			using (winzorDispatcher.WithContext(dispatcherContext))
			{
				using var form = new Form();
				form.ShowDialog();
			}
		});
		Assert.That(() => dispatcherContext.Rendered, Is.Not.Null.After(3000, 100));
		var form = dispatcherContext.Rendered.GetForm();

		Assert.That(form.Modal, Is.True);
		Assert.That(form.ModalWindow, Is.False);

		await form.InvokeWinzorDispatcherAsync(form.Close);
		await runInAnotherWinFormsThreadAsync;
	}

	[Test]
	public async Task TestOpenFormLog()
	{
		var circuitId = new Mock<ICircuitIdProvider>();
		circuitId.Setup(c => c.CircuitId).Returns("CircuitId");
		var logger = new TestLogger<Form>();
		using var ctx = new WinzorTestContext();
		ctx.Services.AddSingleton<ILogger<Form>>(logger);
		ctx.Services.AddSingleton(circuitId.Object);
		await ctx.RenderFormAsync(() => new Form());
		Assert.That(logger.logs, Does.Contain("Initialized form Form on circuit CircuitId"));
	}

	[Test]
	public async Task FormActivatedFromClientShouldSetActiveForm()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() => new Form());
		var form = rendered.GetForm();

		await form.InvokeWinzorDispatcherAsync(() => Form.SetActiveForm(null, form));
		Assert.That(Form.ActiveForm, Is.Not.EqualTo(form));
		await form.ActivatedFromClientAsync();
		Assert.That(Form.ActiveForm, Is.EqualTo(form));
	}

	#region Paste
	[Test, WithPlaywrightPage]
	[TestCase("testImageFile.png", "image/png", "iVBORw0KGgoAAAANSUhEUgAAAD8AAABACAYAAACtK6/LAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAJOgAACToAYJjBRwAAABfSURBVHhe7c8xAQAgDMAwnPEifp6GkPSIgJz7ZlXlVeVV5VXlVeVV5VXlVeVV5VXlVeVV5VXlVeVV5VXlVeVV5VXlVeVV5VXlVeVV5VXlVeVV5VXlVeVV5VXlVXB+9gPiZ42drH9LewAAAABJRU5ErkJggg==")]
	[TestCase("testTextFile.txt", "text/plain", "VGhpcyBpcyBhIG5ldyB0ZXh0IGZpbGUgZm9yIHRlc3RpbmcgdGhlIERvY3VtZW50c1pHcmlkLg==")]
	public async Task PasteOnForm_WhenClipboardContainsFile_ShouldPasteFile(string fileName, string mimeType, string base64Data)
	{
		await using var ctx = new InMemoryTestServerContext();
		PastableDataGrid dataGrid = null;

		var page = await ctx.LoadControlOnFormAsync(() => dataGrid = new PastableDataGrid());
		var form = await page.WaitForSelectorAsync(".form");
		Assert.That(form, Is.Not.Null);

		var files = new List<JSClipboardData> { new JSClipboardData(mimeType, base64Data, fileName) };
		await form.PasteFiles(files);

		Assert.That(await dataGrid.WinzorPasteAsyncCompletion.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task PasteOnForm_WhenClipboardContainsText_ShouldPasteText()
	{
		await using var ctx = new InMemoryTestServerContext();
		PastableDataGrid datagrid = null;

		var page = await ctx.LoadControlOnFormAsync(() => datagrid = new PastableDataGrid());
		var form = await page.WaitForSelectorAsync(".form");
		Assert.That(form, Is.Not.Null);

		await form.PasteText("Paste a text");

		Assert.That(await datagrid.WinzorPasteAsyncCompletion.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task PasteOnForm_WhenClipboardContainsHtml_ShouldPasteHtml()
	{
		await using var ctx = new InMemoryTestServerContext();
		PastableDataGrid datagrid = null;

		var page = await ctx.LoadControlOnFormAsync(() => datagrid = new PastableDataGrid());
		var form = await page.WaitForSelectorAsync(".form");
		Assert.That(form, Is.Not.Null);

		await form.PasteHtml("<div>Test</div>");

		Assert.That(await datagrid.WinzorPasteAsyncCompletion.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task PasteOnForm_WhenFormHasNoPastableControl_ShouldNotPaste()
	{
		await using var ctx = new InMemoryTestServerContext();
		NonPastableDataGrid datagrid = null;

		var page = await ctx.LoadControlOnFormAsync(() => datagrid = new NonPastableDataGrid());
		var form = await page.WaitForSelectorAsync(".form");
		Assert.That(form, Is.Not.Null);

		var files = new List<JSClipboardData> { new JSClipboardData("text/plain", "VGhpcyBpcyBhIG5ldyB0ZXh0IGZpbGUgZm9yIHRlc3RpbmcgdGhlIERvY3VtZW50c1pHcmlkLg==", "testImage1.png") };
		await form.PasteFiles(files);

		Assert.That(await datagrid.WinzorPasteAsyncCompletion.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.False);
	}

	[Test, WithPlaywrightPage]
	public async Task PasteOnPastableControl_ShouldPaste()
	{
		await using var ctx = new InMemoryTestServerContext();
		PastableDataGrid datagrid = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			var richTextBox = new RichTextBox();
			datagrid = new PastableDataGrid();

			var form = new Form();
			form.Controls.Add(datagrid);
			form.Controls.Add(richTextBox);

			return form;
		});

		var expectedDataGrid = await page.WaitForSelectorAsync(".datagrid");
		Assert.That(expectedDataGrid, Is.Not.Null);

		var files = new List<JSClipboardData> { new JSClipboardData("text/plain", "VGhpcyBpcyBhIG5ldyB0ZXh0IGZpbGUgZm9yIHRlc3RpbmcgdGhlIERvY3VtZW50c1pHcmlkLg==", "testImage1.png") };
		await expectedDataGrid.PasteFiles(files);

		Assert.That(await datagrid.WinzorPasteAsyncCompletion.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task PasteOnElementInsidePastableControl_ShouldPaste()
	{
		await using var ctx = new InMemoryTestServerContext();
		PastableDataGrid datagrid = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			var richTextBox = new RichTextBox();
			datagrid = new PastableDataGrid();

			var form = new Form();
			form.Controls.Add(richTextBox);
			form.Controls.Add(datagrid);

			return form;
		});

		var dataGridChild = await page.WaitForSelectorAsync(".datagrid__container");
		Assert.That(dataGridChild, Is.Not.Null);

		var files = new List<JSClipboardData> { new JSClipboardData("text/plain", "VGhpcyBpcyBhIG5ldyB0ZXh0IGZpbGUgZm9yIHRlc3RpbmcgdGhlIERvY3VtZW50c1pHcmlkLg==", "testImage1.png") };
		await dataGridChild.PasteFiles(files);

		Assert.That(await datagrid.WinzorPasteAsyncCompletion.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
	}
	#endregion

	#region WinzorDragEnd event

	[Test, WithPlaywrightPage]
	public async Task FormFiresWinzorDragEndEventsOnControlDragEnd()
	{
		// This is a Form test as the form will take a DragEnd event and fire a corresponding WinzorDragEnd event (in order to fix incorrect coordinates).

		var control1Task = new TaskCompletionSource();
		var control2Task = new TaskCompletionSource();
		var control1DragPosition = new Position();
		var control2DragPosition = new Position();
		ILocator control1Locator = null;
		ILocator control2Locator = null;

		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(300, 300) };
			var control1 = new Control() { Top = 50, Left = 50, Size = new Size(50, 50), AllowItemDrag = true, Name = "Control1" };
			var control2 = new Control() { Top = 150, Left = 150, Size = new Size(50, 50), AllowItemDrag = true, Name = "Control2" };

			control1.DragEnd += (s, e) =>
			{
				control1Task.SetResult();
				control1DragPosition.X = e.X;
				control1DragPosition.Y = e.Y;
			};
			control2.DragEnd += (s, e) =>
			{
				control2Task.SetResult();
				control2DragPosition.X = e.X;
				control2DragPosition.Y = e.Y;
			};

			form.Controls.Add(control1);
			form.Controls.Add(control2);

			return form;
		});

		await page.WaitForSelectorAsync(".form");
		control1Locator = page.Locator("[data-name=\"Control1\"]");
		control2Locator = page.Locator("[data-name=\"Control2\"]");

		await control1Locator.DragToAsync(control1Locator, new LocatorDragToOptions() { TargetPosition = new TargetPosition() { X = 3, Y = 4 } });
		Assert.Multiple(() =>
		{
			Assert.That(() => control1Task.Task.IsCompletedSuccessfully, Is.True.After(3000, 100));
			Assert.That(control1DragPosition.X, Is.EqualTo(53));
			Assert.That(control1DragPosition.Y, Is.EqualTo(54));
			Assert.That(control2Task.Task.IsCompletedSuccessfully, Is.False);
		});

		await control2Locator.DragToAsync(control2Locator, new LocatorDragToOptions() { TargetPosition = new TargetPosition() { X = 5, Y = 6 } });
		Assert.Multiple(() =>
		{
			Assert.That(() => control2Task.Task.IsCompletedSuccessfully, Is.True.After(3000, 100));
			Assert.That(control2DragPosition.X, Is.EqualTo(155));
			Assert.That(control2DragPosition.Y, Is.EqualTo(156));
		});
	}

	#endregion
}

class NonPastableDataGrid : DataGrid
{
	public TaskCompletionSource<bool> WinzorPasteAsyncCompletion { get; } = new TaskCompletionSource<bool>();

	protected override async Task OnWinzorPasteAsync(WinzorPasteEventArgs clipboardData)
	{
		await base.OnWinzorPasteAsync(clipboardData);
		WinzorPasteAsyncCompletion.SetResult(true);
	}
}

class PastableDataGrid : NonPastableDataGrid
{
	protected override bool AllowWinzorPaste => true;
}
