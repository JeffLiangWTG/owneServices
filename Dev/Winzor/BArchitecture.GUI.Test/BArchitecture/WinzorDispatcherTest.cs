using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using Moq;
using NUnit.Framework;
using WinzorTestFramework;

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits

namespace WinzorFramework;

class WinzorDispatcherTest
{
	[Test]
	public void InvokeFromAnotherThread()
	{
		using var dispatcher = new WinzorDispatcher(
			Mock.Of<IFormOpener>(),
			Mock.Of<IFormInstanceRegister>()
		);
		var firstThreadId = 0;
		var secondThreadId = 0;
		Assert.That(dispatcher.InvokeAsync(() => firstThreadId = Environment.CurrentManagedThreadId).Wait(TimeSpan.FromSeconds(1)), Is.True);
		Assert.That(Task.Run(() => dispatcher.InvokeAsync(() => secondThreadId = Environment.CurrentManagedThreadId).GetAwaiter().GetResult()).Wait(TimeSpan.FromSeconds(1)), Is.True);
		Assert.That(firstThreadId, Is.Not.EqualTo(0));
		Assert.That(secondThreadId, Is.EqualTo(firstThreadId));
	}

	[Test]
	public void InvokeThrows()
	{
		using var dispatcher = new WinzorDispatcher(
			Mock.Of<IFormOpener>(),
			Mock.Of<IFormInstanceRegister>()
		);
		Assert.That(() => dispatcher.InvokeAsync(() => throw new InvalidOperationException("Fail")).GetAwaiter().GetResult(), Throws.InvalidOperationException.With.Message.EqualTo("Fail"));
	}

	[Test]
	public void InvokeWhileMessageLoopBlocked()
	{
		using var dispatcher = new WinzorDispatcher(
			Mock.Of<IFormOpener>(),
			Mock.Of<IFormInstanceRegister>()
		);
		var firstTcs = new TaskCompletionSource();
		var secondTcs = new TaskCompletionSource();
		var firstTask = dispatcher.InvokeAsync(firstTcs.Task.Wait);
		var secondTask = dispatcher.InvokeAsync(secondTcs.SetResult);
		Assert.That(firstTask.Wait(TimeSpan.FromMilliseconds(100)), Is.False);
		Assert.That(secondTask.Wait(TimeSpan.FromMilliseconds(100)), Is.False);
		Assert.That(secondTcs.Task.IsCompleted, Is.False);
		firstTcs.SetResult();
		Assert.That(firstTask.Wait(TimeSpan.FromMilliseconds(100)), Is.True);
		Assert.That(secondTask.Wait(TimeSpan.FromMilliseconds(100)), Is.True);
		Assert.That(secondTcs.Task.IsCompleted, Is.True);
	}

	[Test]
	public void InvokeRunMessageLoopInvoke()
	{
		using var dispatcher = new WinzorDispatcher(
			Mock.Of<IFormOpener>(),
			Mock.Of<IFormInstanceRegister>()
		);
		using var firstCts = new CancellationTokenSource();
		var firstTask = dispatcher.InvokeAsync(() => dispatcher.RunMessageLoop(firstCts));
		var secondTcs = new TaskCompletionSource();
		var secondTask = dispatcher.InvokeAsync(secondTcs.SetResult);
		Assert.That(firstTask.Wait(TimeSpan.FromMilliseconds(100)), Is.False);
		Assert.That(secondTask.Wait(TimeSpan.FromMilliseconds(100)), Is.True);
		Assert.That(secondTcs.Task.IsCompleted, Is.True);
		firstCts.Cancel();
		Assert.That(firstTask.Wait(TimeSpan.FromMilliseconds(100)), Is.True);
		var thridTcs = new TaskCompletionSource();
		var thirdTask = dispatcher.InvokeAsync(thridTcs.SetResult);
		Assert.That(thirdTask.Wait(TimeSpan.FromMilliseconds(100)), Is.True);
		Assert.That(thridTcs.Task.IsCompleted, Is.True);
	}

	[Test]
	public void ManagedThreadId()
	{
		using var dispatcher = new WinzorDispatcher(
			Mock.Of<IFormOpener>(),
			Mock.Of<IFormInstanceRegister>()
		);
		var invokeThreadId = 0;
		Assert.That(dispatcher.InvokeAsync(() => invokeThreadId = Environment.CurrentManagedThreadId).Wait(TimeSpan.FromSeconds(1)), Is.True);
		Assert.That(invokeThreadId, Is.Not.EqualTo(0));
		Assert.That(dispatcher.ManagedThreadId, Is.EqualTo(invokeThreadId));
	}

	[Test]
	public void DisposeWhileExecutingDoesNotThrow()
	{
		var invokeStartedTcs = new TaskCompletionSource();
		var disposeTcs = new TaskCompletionSource();
		var dispatcher = new WinzorDispatcher(
			Mock.Of<IFormOpener>(),
			Mock.Of<IFormInstanceRegister>()
		);
		dispatcher.RegisterDisposeAction(() => disposeTcs.SetResult());
		var dispatcherTask = dispatcher.InvokeAsync(() =>
		{
			invokeStartedTcs.SetResult();
			while (!dispatcher.IsDisposed)
			{
				Thread.Sleep(100);
			}
		});
		invokeStartedTcs.Task.Wait();
		dispatcher.Dispose();
		disposeTcs.Task.Wait();
		dispatcherTask.Wait();
	}

	[Test]
	public async Task DisposeRaisesApplicationThreadExit()
	{
		var dispatcher = new WinzorDispatcher(
			Mock.Of<IFormOpener>(),
			Mock.Of<IFormInstanceRegister>()
		);
		var threadExitCalled = new TaskCompletionSource();
		var threadExitManagedThreadId = -1;
		Application.ThreadExit += Application_ThreadExit;
		try
		{
			dispatcher.Dispose();
			Assert.That(await threadExitCalled.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);
			Assert.That(threadExitManagedThreadId, Is.EqualTo(dispatcher.ManagedThreadId));
		}
		finally
		{
			Application.ThreadExit -= Application_ThreadExit;
		}

		void Application_ThreadExit(object sender, EventArgs e)
		{
			threadExitManagedThreadId = Environment.CurrentManagedThreadId;
			threadExitCalled.SetResult();
		}
	}

	[Test]
	public async Task ApplicationThreadExitNotRaisedByNonTopLevelRunMessageLoop()
	{
		using var dispatcher = new WinzorDispatcher(
			Mock.Of<IFormOpener>(),
			Mock.Of<IFormInstanceRegister>()
		);
		var threadExitCalled = new TaskCompletionSource();
		Application.ThreadExit += Application_ThreadExit;
		try
		{
			using var cts = new CancellationTokenSource();
			var task = dispatcher.InvokeAsync(() => dispatcher.RunMessageLoop(cts));
			await cts.CancelAsync();
			await task;
			Assert.That(await threadExitCalled.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.False);
		}
		finally
		{
			Application.ThreadExit -= Application_ThreadExit;
		}

		void Application_ThreadExit(object sender, EventArgs e)
		{
			threadExitCalled.SetResult();
		}
	}

	[Test]
	public async Task DisposeWhileRunningNonTopLevelMessageLoop()
	{
		using var dispatcher = new WinzorDispatcher(
			Mock.Of<IFormOpener>(),
			Mock.Of<IFormInstanceRegister>()
		);
		var task = dispatcher.InvokeAsync(() => dispatcher.RunMessageLoop(new CancellationTokenSource()));
		Assert.That(dispatcher.Dispose, Throws.InvalidOperationException);
		await task;
	}

	[Test]
	public async Task DisposeShouldWaitNonTopLevelMessageLoopClosed()
	{
		using var dispatcher = new WinzorDispatcher(
			Mock.Of<IFormOpener>(),
			Mock.Of<IFormInstanceRegister>()
		);
		using var cts = new CancellationTokenSource();
		_ = dispatcher.InvokeAsync(() => dispatcher.RunMessageLoop(cts));
		var task = Task.Run(async () =>
		{
			await Task.Delay(3000);
			await cts.CancelAsync();
		});
		Assert.That(dispatcher.Dispose, Throws.Nothing);
		await task;
	}

	[Test]
	public void DisposeShouldWaitForTopLevelMessageLoopToRaiseApplicationThreadExit()
	{
		var dispatcher = new WinzorDispatcher(
			Mock.Of<IFormOpener>(),
			Mock.Of<IFormInstanceRegister>()
		);
		var threadExitCalled = false;
		var threadExitManagedThreadId = -1;
		Application.ThreadExit += Application_ThreadExit;
		try
		{
			// Make the dispatcher do something slow so it takes its sweet time to cancel the message loop and raise Application.ThreadExit
			_ = dispatcher.InvokeAsync(() => Thread.Sleep(1000));
			dispatcher.Dispose();
			Assert.That(threadExitCalled, Is.True);
			Assert.That(threadExitManagedThreadId, Is.EqualTo(dispatcher.ManagedThreadId));
		}
		finally
		{
			Application.ThreadExit -= Application_ThreadExit;
		}

		void Application_ThreadExit(object sender, EventArgs e)
		{
			threadExitCalled = true;
			threadExitManagedThreadId = Environment.CurrentManagedThreadId;
		}
	}

	[Test]
	public void DisposeShouldCallApplicationThreadExitBeforeDisposeActions()
	{
		var dispatcher = new WinzorDispatcher(
			Mock.Of<IFormOpener>(),
			Mock.Of<IFormInstanceRegister>()
		);
		var threadExitCalled = false;
		var disposeActionCalled = false;
		Application.ThreadExit += Application_ThreadExit;
		dispatcher.RegisterDisposeAction(DisposeAction);

		try
		{
			dispatcher.Dispose();
			Assert.That(threadExitCalled, Is.True);
			Assert.That(disposeActionCalled, Is.True);
		}
		finally
		{
			Application.ThreadExit -= Application_ThreadExit;
		}

		void Application_ThreadExit(object sender, EventArgs e)
		{
			if (!disposeActionCalled)
			{
				threadExitCalled = true;
			}
		}

		void DisposeAction()
		{
			Assert.That(threadExitCalled, Is.True, "Application_ThreadExit should be called before DisposeAction");
			disposeActionCalled = true;
		}
	}

	[Test]
	public void IsSameAsMethodShouldCorrectlyCompare([Values] bool onSameThread)
	{
		using var dispatcherA = new WinzorDispatcher(
			Mock.Of<IFormOpener>(),
			Mock.Of<IFormInstanceRegister>()
		);

		using var dispatcherB = onSameThread
			? dispatcherA
			: new WinzorDispatcher(
				Mock.Of<IFormOpener>(),
				Mock.Of<IFormInstanceRegister>()
			);
		Assert.That(dispatcherA.IsSameAs(dispatcherB), Is.EqualTo(onSameThread));
	}

	[Test]
	public void WinzorDispatcherDisposeTwiceWillNotThrowException()
	{
		var dispatcher = new WinzorDispatcher(
			Mock.Of<IFormOpener>(),
			Mock.Of<IFormInstanceRegister>()
		);
		dispatcher.Dispose();
		Assert.DoesNotThrow(dispatcher.Dispose);
	}

	[Test]
	public async Task WinzorDispatcherDisposeThrowTimeoutExceptionIfNotShutdownWithinLimitTime()
	{
		using var ctx = new WinzorTestContext();
		ctx.WinzorDispatcher.ShutdownTimeLimit = TimeSpan.FromSeconds(2);
		ctx.DeveloperExceptionRaised += _ => true;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var button = new Button { Text = "Test" };
			button.Click += (sender, e) =>
			{
				Thread.Sleep(1000 * 10);
			};
			form.Controls.Add(button);
			return form;
		});
		rendered.Find("button").Click();
		var exception = Assert.Throws<TimeoutException>(() => ctx.WinzorDispatcher.Dispose());
		Assert.That(exception.Message, Is.EqualTo("The message loop did not shutdown within 1 minute. The Application.OnThreadExit event may not have been raised."));
	}

	[Test]
	public void Dispose_DoesNotThrowsException_WhenTryToRunMessageLoop()
	{
		using var dispatcher = new WinzorDispatcher(
			Mock.Of<IFormOpener>(),
			Mock.Of<IFormInstanceRegister>()
		);
		using var cts = new CancellationTokenSource();
		cts.Cancel();
		_ = Task.Run(() =>
		{
			while (!dispatcher.IsDisposed)
			{
				_ = dispatcher.InvokeAsync(() => dispatcher.RunMessageLoop(cts));
			}
		}).Wait(TimeSpan.FromMilliseconds(100));
		Assert.That(dispatcher.Dispose, Throws.Nothing);
	}

	[Test]
	public async Task WinzorDispatcherDisposeThrowInvalidOperationException()
	{
		using var dispatcher = new WinzorDispatcher(
			Mock.Of<IFormOpener>(),
			Mock.Of<IFormInstanceRegister>()
		);
		var task = dispatcher.InvokeAsync(() => dispatcher.RunMessageLoop(new CancellationTokenSource()));
		// By waiting for this action to be processed, we know the inner message loop is running
		await dispatcher.InvokeAsync(() => { });

		var exception = Assert.Throws<InvalidOperationException>(() => dispatcher.Dispose());
		Assert.That(exception.Message, Does.Contain("Attempted to dispose a WinzorDispatcher running an inner message loop. [InnerMessageLoops]:"));
		Assert.That(exception.Message, Does.Contain("WinzorDispatcherTest.cs"));
		Assert.That(exception.Message, Does.Contain("WinzorDispatcherDisposeThrowInvalidOperationException"));

		await task;
	}

	[Test]
	public void DoEventsThrowInvalidOperationException()
	{
		using var dispatcher = new WinzorDispatcher(
			Mock.Of<IFormOpener>(),
			Mock.Of<IFormInstanceRegister>()
		);
		var exception = Assert.Throws<InvalidOperationException>(() => dispatcher.DoEvents());
		Assert.That(exception?.Message, Is.EqualTo("DoEvents can only be called on WinzorDispatcher thread"));

		Assert.DoesNotThrowAsync(() => dispatcher.InvokeAsync(() => dispatcher.DoEvents()));
	}

	[Test]
	public void InvokeAsyncAfterDisposedDoesNotThrow()
	{
		var dispatcher = new WinzorDispatcher(
			Mock.Of<IFormOpener>(),
			Mock.Of<IFormInstanceRegister>()
		);
		dispatcher.Dispose();
		Assert.That(async () => await dispatcher.InvokeAsync(() => { }), Throws.Nothing);
	}

	[Test]
	public async Task ContextStackIsBoundOnlyToWinzorDispatcherThread()
	{
		using var ctx = new WinzorTestContext();
		Form form = null;
		await ctx.RenderFormAsync(() => form = new Form());

		await ctx.WinzorDispatcher.InvokeAsync(() => Assert.DoesNotThrow(() => form.NotifyRenderRequired()));
		var exception = Assert.Throws<InvalidOperationException>(() => form.NotifyRenderRequired());
		Assert.That(exception.Message, Is.EqualTo("ContextStack is bound only to a WinzorDispatcher thread."));
	}
}
