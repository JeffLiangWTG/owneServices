using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.Blazor.Client.Integration.Messaging;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WinzorTestFramework;

// This code was copied and modified from the Shared.40 repository
// https://devops.wisetechglobal.com/wtg/CargoWise/_git/Shared?path=%2FShared.40%2FShared.40.Test%2FProcessStatusFormManagerTest.cs&_a=contents&version=GBmaster

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits

namespace Enterprise.Winzor.Architecture.Test;

class ProcessStatusFormManagerTest
{
	List<Exception> unhandledExceptions;

	[SetUp]
	public void TestSetup()
	{
		instances = new List<TestProcessStatusFormManager>();
		unhandledExceptions = new List<Exception>();
		AppDomain.CurrentDomain.UnhandledException += CaptureException;
	}

	void CaptureException(object sender, UnhandledExceptionEventArgs e)
	{
		unhandledExceptions.Add(e.ExceptionObject as Exception);
	}

	[TearDown]
	public void TestCleanup()
	{
		foreach (var instance in instances)
		{
			Assert.That(instance.DispatcherTask == null || instance.DispatcherTask.Wait(TimeSpan.FromSeconds(10)), Is.True);
		}
		AppDomain.CurrentDomain.UnhandledException -= CaptureException;
		Assert.That(unhandledExceptions, Has.Exactly(0).Items, "Unhandled exceptions");
	}

	[Test, WinFormsWinzorTest]
	public void QueuedActionOnForm()
	{
		var numbers = new List<int>();
		var num = 0;

		using (var manager = new TestProcessStatusFormManager(this))
		{
			manager.InvokeOnForm(f => numbers.Add(++num));
			manager.InvokeOnForm(f => numbers.Add(++num));

			manager.Start();
			manager.UpdateStatus("Moon Pie", 10);
			manager.FormShownEvent.WaitOne();
			manager.Form.StatusEvent.WaitOne();

			manager.InvokeOnForm(f => numbers.Add(++num));
			manager.InvokeOnForm(f => numbers.Add(++num));
			manager.InvokeOnForm(f => numbers.Add(++num));
			manager.InvokeOnForm(f => numbers.Add(++num));
			Thread.Sleep(TimeSpan.FromSeconds(1));

			manager.Dispose();
			Thread.Sleep(TimeSpan.FromSeconds(1));

			manager.InvokeOnForm(f => numbers.Add(++num));
			manager.InvokeOnForm(f => numbers.Add(++num));
		}

		Assert.That(string.Join(", ", numbers), Is.EqualTo("1, 2, 3, 4, 5, 6"), "Only the action invoked before the form was is disposed should be invoked.");
	}

	[Test, WinFormsWinzorTest]
	public void StatusUpdates()
	{
		using (var manager = new TestProcessStatusFormManager(this))
		{
			manager.Start();
			manager.UpdateStatus("ten", 10);
			manager.FormShownEvent.WaitOne();
			manager.Form.StatusEvent.WaitOne();
			Assert.That(manager.Form.Text, Is.EqualTo("ten 10"));
			manager.UpdateStatus("twenty", 20);
			manager.Form.StatusEvent.WaitOne();
			Assert.That(manager.Form.Text, Is.EqualTo("twenty 20"));
			manager.UpdateStatus("fifty", 50);
			manager.Form.StatusEvent.WaitOne();
			Assert.That(manager.Form.Text, Is.EqualTo("fifty 50"));
			manager.UpdateStatus("seventy", 70);
			manager.Form.StatusEvent.WaitOne();
			Assert.That(manager.Form.Text, Is.EqualTo("seventy 70"));
		}
	}

	[Test, WinFormsWinzorTest]
	public void StatusAfterInitialDelay()
	{
		using (var manager = new TestProcessStatusFormManager(this))
		{
			manager.InitialDelay = TimeSpan.FromMilliseconds(500);
			manager.Start();
			manager.UpdateStatus("test", 10);
			Thread.Sleep(200);
			Assert.That(manager.Form, Is.Null);
			manager.FormShownEvent.WaitOne();
			Assert.That(!manager.Form.IsDisposed);
			manager.Form.StatusEvent.WaitOne();
			Assert.That(manager.Form.Text, Is.EqualTo("test 10"));
		}
	}

	[Test, WinFormsWinzorTest]
	public void HideBeforeInitialDelay()
	{
		using (var manager = new TestProcessStatusFormManager(this))
		{
			manager.InitialDelay = TimeSpan.FromMilliseconds(500);
			manager.Start();
			manager.UpdateStatus("test", 10);
			Thread.Sleep(200);
			Assert.That(manager.Form, Is.Null);
			manager.HideForm();
			Thread.Sleep(1000);
			Assert.That(manager.Form, Is.Null);

			manager.ShowForm();
			manager.FormShownEvent.WaitOne();
			Assert.That(!manager.Form.IsDisposed);
			manager.Form.StatusEvent.WaitOne();
			Assert.That(manager.Form.Text, Is.EqualTo("test 10"));
		}
	}

	[Test, WinFormsWinzorTest]
	public void DisposeBeforeInitialDelay()
	{
		using (var manager = new TestProcessStatusFormManager(this))
		{
			manager.InitialDelay = TimeSpan.FromSeconds(1);
			manager.Start();
			Assert.That(manager.Form, Is.Null);
		}
	}

	[Test, WinFormsWinzorTest]
	public void FormDisposedExternally()
	{
		using (var manager = new TestProcessStatusFormManager(this))
		{
			manager.Start();
			manager.FormShownEvent.WaitOne();
			manager.Form.BeginInvoke(new Action(manager.Form.Close));
			manager.UpdateStatus("test", 10);
		}

		using (var manager = new TestProcessStatusFormManager(this))
		{
			manager.Start();
			manager.FormShownEvent.WaitOne();
			manager.Form.BeginInvoke(new Action(manager.Form.Close));
			manager.HideForm();
		}
	}

	[Test, WinFormsWinzorTest]
	public void FormClosed()
	{
		Form form1, form2;
		AutoResetEvent form1DisposedEvent, form2DisposedEvent;

		using (var manager = new TestProcessStatusFormManager(this))
		{
			manager.Start();
			manager.FormShownEvent.WaitOne();

			form1 = manager.Form;
			form1.BeginInvoke(new Action(form1.Close));
			form1DisposedEvent = manager.FormDisposedEvent;

			manager.FormShownEvent.WaitOne();
			form2 = manager.Form;
			Assert.That(form2, Is.Not.EqualTo(form1));

			form2DisposedEvent = manager.FormDisposedEvent;
		}

		form1DisposedEvent.WaitOne(500);
		form2DisposedEvent.WaitOne(500);
		Assert.That(form1.IsDisposed, Is.True);
		Assert.That(form2.IsDisposed, Is.True);
	}

	[Test, WinFormsWinzorTest]
	public void StatusDuringCreation()
	{
		using (var manager = new TestProcessStatusFormManager(this))
		{
			manager.CreateHandleSignal = new ManualResetEvent(false);
			manager.Start();
			Thread.Sleep(100);
			manager.UpdateStatus("test", 10);
			manager.CreateHandleSignal.Set();
			manager.FormShownEvent.WaitOne();
			Assert.That(!manager.Form.IsDisposed);
			Assert.That(manager.Form.StatusEvent.WaitOne(500), Is.True);
			Assert.That(manager.Form.Text, Is.EqualTo("test 10"));
		}
	}

	[Test, WinFormsWinzorTest]
	public void HideDuringCreation()
	{
		using (var manager = new TestProcessStatusFormManager(this))
		{
			manager.CreateHandleSignal = new ManualResetEvent(false);
			manager.Start();
			Thread.Sleep(100);
			manager.UpdateStatus("test", 10);
			manager.HideForm();
			manager.CreateHandleSignal.Set();
			manager.FormDisposedEvent.WaitOne();

			manager.FormShownEvent.Reset();
			manager.ShowForm();
			manager.FormShownEvent.WaitOne();
			Assert.That(!manager.Form.IsDisposed);
			manager.Form.StatusEvent.WaitOne();
			Assert.That(manager.Form.Text, Is.EqualTo("test 10"));
		}
	}

	[Test, WinFormsWinzorTest]
	public void StatusAfterHide()
	{
		using (var manager = new TestProcessStatusFormManager(this))
		{
			manager.Start();
			manager.UpdateStatus("test", 10);
			manager.FormShownEvent.WaitOne();
			manager.Form.StatusEvent.WaitOne();
			manager.HideForm();
			manager.FormDisposedEvent.WaitOne();

			manager.FormShownEvent.Reset();
			manager.ShowForm();
			manager.FormShownEvent.WaitOne();
			Assert.That(manager.Form.StatusEvent.WaitOne(TimeSpan.FromSeconds(5)), Is.True);
			Assert.That(manager.Form.Text, Is.EqualTo("test 10"));
		}
	}

	[Test, WinFormsWinzorTest]
	public void DisposeDuringCreation()
	{
		var manager = new TestProcessStatusFormManager(this);
		manager.CreateHandleSignal = new ManualResetEvent(false);
		manager.Start();
		Thread.Sleep(100);
		manager.Dispose();
		manager.FormDisposedEvent.WaitOne();
		manager.CreateHandleSignal.Set();
	}

	[Test, WinFormsWinzorTest]
	public void UpdateStatusDoesNotWait()
	{
		using (var manager = new TestProcessStatusFormManager(this))
		{
			manager.UpdateStatusDelay = 500;
			manager.Start();
			manager.FormShownEvent.WaitOne();
			manager.UpdateStatus("test", 10);
			Assert.That(!manager.Form.StatusEvent.WaitOne(10));
		}
	}

	[Test, WinFormsWinzorTest]
	public void DisposeBeforeStart()
	{
		using (var manager = new TestProcessStatusFormManager(this))
		{
		}
	}

	[Test, WinFormsWinzorTest]
	public void DisposeWhileInFormOnload()
	{
		// This test causes the test runner to crash when not handled correctly
		using (var manager = new TestProcessStatusFormManager(this) { OnLoadDelay = 2000 })
		{
			manager.Start();
			Thread.Sleep(TimeSpan.FromSeconds(1));
		}
		Thread.Sleep(TimeSpan.FromSeconds(2)); // This sleep is essential to ensure any delayed crash happens while still executing witin the test method			
	}

	[Test, WinFormsWinzorTest]
	public void HideUpdateShow()
	{
		using (var manager = new TestProcessStatusFormManager(this))
		{
			manager.Start();
			for (var p = 1; p < 100; p++)
			{
				manager.HideForm();
				Thread.Sleep(1);
				manager.UpdateStatus(p.ToString(), p);
				Thread.Sleep(1);
				manager.ShowForm();
				Thread.Sleep(1);
			}
		}
	}

	[Test, WinFormsWinzorTest]
	public void ExceptionInCreateFormIsHandled()
	{
		var handledExceptions = new List<Exception>();

		using (var manager = new ThrowOnCreateProcessStatusFormManager<ExceptionForTest>(handledExceptions))
		{
			manager.Start();
			Thread.Sleep(100);
		}

		Assert.That(handledExceptions, Has.Exactly(1).Items, "Handler should be called once");
		Assert.That(handledExceptions[0], Is.InstanceOf<ExceptionForTest>());
	}

	[Test, WinFormsWinzorTest]
	public void UpdateStatusDoesNotBlock()
	{
		using (var manager = new TestProcessStatusFormManager(this) { OnLoadDelay = 2000 })
		{
			manager.Start();
			var stopwatch = Stopwatch.StartNew();
			for (var i = 0; i < 10; i++)
			{
				Thread.Sleep(10);
				manager.UpdateStatus(i.ToString(), i * 10);
			}
			Assert.That(stopwatch.Elapsed, Is.LessThan(TimeSpan.FromSeconds(1)));
		}
	}

	[Test, WinFormsWinzorTest]
	public void DisposeSendsCloseRequestOnProgressForm()
	{
		var manager = new TestProcessStatusFormManager(this);
		manager.Start();
		manager.FormShownEvent.WaitOne();
		var mockWindowsService = Mock.Get(manager.Form.CargoWiseClientServices.WindowService);
		manager.Dispose();
		manager.FormDisposedEvent.WaitOne();
		mockWindowsService.Verify(s => s.RequestCloseAsync(), Times.Once());
	}

	[Test]
	[SuppressMessage("Usage", "VSTHRD103:Call async methods when in an async method")]
	public async Task ShowDoesNotTriggerOnBeforeRenderOnMainThread()
	{
		using var ctx = new EnterpriseTestContext();
		var windowService = new Mock<IWindowService>();
		var loadRequestSent = new TaskCompletionSource();
		string loadRequestUrl = null;
		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				loadRequestUrl = createWindowOptions.Uri.ToString();
				loadRequestSent.SetResult();
			});
		var clientServices = MockCargoWiseClientServices.MakeMock(windowService: windowService.Object);
		var renderedForm = await ctx.RenderFormAsync(() =>
		{
			var form = new FormWithOnBeforeRenderCheck();
			var button = new Button();
			button.Click += Button_Click;
			form.Controls.Add(button);
			return form;
		}, clientServices);
		var clickTask = renderedForm.Find("button").ClickAsync(new WebMouseEventArgs());
		Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);
		using var ctx2 = new EnterpriseTestContext();
		var renderedNewForm = ctx2.RenderEntryPointComponent(loadRequestUrl);
		await clickTask;

		void Button_Click(object sender, EventArgs e)
		{
			var form = ((Control)sender).FindForm();
			form.Controls.Add(new Label { Text = "foo" });
			using var processStatusFormManager = new TestProcessStatusFormManager(this);
			processStatusFormManager.Start();
			loadRequestSent.Task.Wait();
		}
	}

	class FormWithOnBeforeRenderCheck : Form
	{
		protected override void OnBeforeRender()
		{
			if (System.Environment.CurrentManagedThreadId != WinzorDispatcher.ManagedThreadId)
			{
				throw new InvalidOperationException("OnBeforeRender called on the wrong thread");
			}
		}
	}

	public class TestProcessStatusFormManager : ProcessStatusFormManager<TestProgressForm>
	{
		public TestProcessStatusFormManager(ProcessStatusFormManagerTest test)
			: base((object sender, ThreadExceptionEventArgs e) => { throw new TargetInvocationException(e.Exception); })
		{
			test.instances.Add(this);
		}

		protected override TestProgressForm CreateForm()
		{
			Form = base.CreateForm();
			Form.CreateHandleSignal = CreateHandleSignal;
			Form.UpdateStatusDelay = UpdateStatusDelay;
			Form.OnLoadDelay = OnLoadDelay;
			Form.Shown += Form_Shown;
			Form.Disposed += Form_Disposed;
			return Form;
		}

		public void SetFormTimerIntervalInMs(int timespanInMs)
		{
			formTimerIntervalInMs = timespanInMs;
		}

		void Form_Shown(object sender, EventArgs e)
		{
			FormShownEvent.Set();
		}

		void Form_Disposed(object sender, EventArgs e)
		{
			FormDisposedEvent.Set();
		}

		public Task DispatcherTask => dispatcherTask;

		public TestProgressForm Form;

		public EventWaitHandle CreateHandleSignal { get; set; }

		public int OnLoadDelay { get; set; }

		public int UpdateStatusDelay { get; set; }

		public AutoResetEvent FormShownEvent = new AutoResetEvent(false);

		public AutoResetEvent FormDisposedEvent = new AutoResetEvent(false);
	}

	List<TestProcessStatusFormManager> instances;

	public class TestProgressForm : Form, CargoWise.IO.IProcessStatus
	{
		public void UpdateStatus(string status, int progressValue)
		{
			if (UpdateStatusDelay > 0)
			{
				Thread.Sleep(UpdateStatusDelay);
			}
			Text = status + " " + progressValue;
			StatusEvent.Set();
		}

		protected override void CreateHandle()
		{
			while (CreateHandleSignal != null && !CreateHandleSignal.WaitOne(TimeSpan.FromMilliseconds(1)))
			{
				Application.DoEvents();
			}
			base.CreateHandle();
		}

		protected override void OnLoad(EventArgs e)
		{
			if (OnLoadDelay > 0)
			{
				Thread.Sleep(OnLoadDelay);
			}
			base.OnLoad(e);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			DisposeEvent.Set();
		}

		public EventWaitHandle CreateHandleSignal { get; set; }

		public int OnLoadDelay { get; set; }

		public int UpdateStatusDelay { get; set; }

		public AutoResetEvent StatusEvent = new AutoResetEvent(false);
		public AutoResetEvent DisposeEvent = new AutoResetEvent(false);
	}

	public class ThrowOnCreateProcessStatusFormManager<TException> : ProcessStatusFormManager<TestProgressForm>
		where TException : Exception
	{
		public ThrowOnCreateProcessStatusFormManager(List<Exception> exceptions)
			: base((s, e) => { exceptions.Add(e.Exception); })
		{
		}

		protected override TestProgressForm CreateForm()
		{
			throw Activator.CreateInstance<TException>();
		}
	}

	public class ExceptionForTest : Exception
	{
	}
}
