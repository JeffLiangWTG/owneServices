using System.Threading;
using System.Threading.Tasks;
using Bunit;
using CargoWise.Blazor.Client.Integration.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;

class ApplicationTest
{
	[Test]
	public async Task UpdateRenderedControlFromApplicationIdle()
	{
		using var ctx = new WinzorTestContext();
		TextBox textBox = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			ctx.Using(form);
			textBox = new TextBox();
			form.Controls.Add(textBox);
			return form;
		});
		Assert.That(rendered.Find("input").GetAttribute("style"), Does.Not.Contain("background-color:#FF0000FF"));
		await ctx.WinzorDispatcher.InvokeAsync(() => Application.Idle += Application_Idle);
		try
		{
			rendered.WaitForState(() => rendered.Find("input").GetAttribute("style").Contains("background-color:#FF0000FF"));
		}
		finally
		{
			await ctx.WinzorDispatcher.InvokeAsync(() => Application.Idle -= Application_Idle);
		}

		void Application_Idle(object sender, EventArgs e)
		{
			textBox.BackColor = Drawing.Color.Red;
		}
	}

	[Test]
	public async Task OpenFormFromApplicationIdle()
	{
		var formInstanceRegister = new Mock<IFormInstanceRegister>();
		using var ctx = new WinzorTestContext(new FormOpener(), formInstanceRegister.Object);

		var isNewForm = false;
		var isNewActiveForm = false;
		var expectedLoadRequestUrl = new Uri(TestNavigationManager.BaseServerUri, "foo");
		formInstanceRegister.Setup(o => o.Add(TestNavigationManager.BaseServerUri, It.IsAny<Form>(), out isNewActiveForm, out isNewForm)).Returns(expectedLoadRequestUrl);

		var windowService = new Mock<IWindowService>();
		var loadRequestSent = new TaskCompletionSource();
		Uri loadRequestUrl = null;
		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				loadRequestUrl = createWindowOptions.Uri;
				loadRequestSent.SetResult();
			});
		var clientServices = MockCargoWiseClientServices.MakeMock(windowService: windowService.Object);

		Form newForm = null;
		await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			ctx.Using(form);
			var textBox = new TextBox();
			form.Controls.Add(textBox);
			return form;
		}, clientServices);
		await ctx.WinzorDispatcher.InvokeAsync(() => Application.Idle += Application_Idle);
		try
		{
			Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);
			Assert.That(loadRequestUrl, Is.EqualTo(expectedLoadRequestUrl));
			newForm.Dispose();
		}
		finally
		{
			await ctx.WinzorDispatcher.InvokeAsync(() => Application.Idle -= Application_Idle);
		}

		void Application_Idle(object sender, EventArgs e)
		{
			if (newForm is null)
			{
				(newForm = new Form()).Show();
			}
		}
	}

	[Test]
	public async Task ExceptionFromApplicationIdleEventHandled()
	{
		using var ctx = new WinzorTestContext();
		var exceptionHandled = new TaskCompletionSource<Exception>();
		ctx.ThreadExceptionExceptionRaised += ex =>
		{
			exceptionHandled.SetResult(ex);
			return true;
		};
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			ctx.Using(form);
			var textBox = new TextBox();
			form.Controls.Add(textBox);
			return form;
		});
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			Application.Idle += Application_Idle;
		});
		try
		{
			Assert.That(await exceptionHandled.Task.WithTimeout(TimeSpan.FromSeconds(1)));
			Assert.That((await exceptionHandled.Task).Message, Is.EqualTo("Fail"));
		}
		finally
		{
			await ctx.WinzorDispatcher.InvokeAsync(() =>
			{
				Application.Idle -= Application_Idle;
			});
		}

		void Application_Idle(object sender, EventArgs e)
		{
			throw new Exception("Fail");
		}
	}

	[Test]
	public async Task ExceptionFromApplicationIdleRenderTaskHandled()
	{
		using var ctx = new WinzorTestContext();
		var exceptionHandled = new TaskCompletionSource<Exception>();
		ctx.ThreadExceptionExceptionRaised += ex =>
		{
			exceptionHandled.SetResult(ex);
			return true;
		};

		ctx.UnhandledExceptionRaised += ex =>
		{
			return ex.Message == "An attempt was made to transition a task to a final state when it had already completed.";
		};

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			ctx.Using(form);
			var textBox = new TextBox();
			form.Controls.Add(textBox);
			return form;
		});
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			Application.Idle += Application_Idle;
		});
		try
		{
			Assert.That(await exceptionHandled.Task.WithTimeout(TimeSpan.FromSeconds(1)));
			Assert.That((await exceptionHandled.Task).Message, Is.EqualTo("Fail"));
		}
		finally
		{
			await ctx.WinzorDispatcher.InvokeAsync(() =>
			{
				Application.Idle -= Application_Idle;
			});
		}

		void Application_Idle(object sender, EventArgs e)
		{
			ctx.WinzorDispatcher.CurrentContext.RegisterRenderTask(Task.FromException(new Exception("Fail")));
		}
	}

	[Test]
	public async Task ApplicationIdleTriggeredRenderTaskDoesNotDeadlockWithDispatcher()
	{
		using var ctx = new WinzorTestContext();
		var callbackExecuted = new TaskCompletionSource();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			ctx.Using(form);
			var textBox = new TextBox();
			form.Controls.Add(textBox);
			return form;
		});
		await ctx.WinzorDispatcher.InvokeAsync(() => Application.Idle += Application_Idle);
		try
		{
			Assert.That(await callbackExecuted.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);
		}
		finally
		{
			await ctx.WinzorDispatcher.InvokeAsync(() => Application.Idle -= Application_Idle);
		}

		void Application_Idle(object sender, EventArgs e)
		{
			ctx.WinzorDispatcher.CurrentContext.RegisterRenderTask(ctx.WinzorDispatcher.InvokeAsync(() => callbackExecuted.TrySetResult()));
		}
	}

	[Test]
	public async Task OpenFormsClearedOnThreadExit()
	{
		var onThreadExit = new TaskCompletionSource();
		Application.ThreadExit += Application_ThreadExit;
		try
		{
			using (var ctx = new WinzorTestContext())
			{
				_ = await ctx.RenderFormAsync(() => new Form());
				Assert.That(Application.OpenForms.Count, Is.EqualTo(1));
			}
			Assert.That(await onThreadExit.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
			Assert.That(Application.OpenForms.Count, Is.EqualTo(0));
		}
		finally
		{
			Application.ThreadExit -= Application_ThreadExit;
		}

		void Application_ThreadExit(object sender, EventArgs e)
		{
			_ = onThreadExit.TrySetResult();
		}
	}

	[Test]
	public async Task ThreadExceptionHandlesExceptionThenExit()
	{
		Exception unhandledException = null;
		var exceptionMessage = nameof(ThreadExceptionHandlesExceptionThenExit);
		var exceptionThrown = new Exception(exceptionMessage);
		using var ctx = new WinzorTestContext();
		var mockService = new Mock<ILifecycleService>();
		var cargowiseClientServices = MockCargoWiseClientServices.MakeMock(lifecycleService: mockService.Object);
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			return form;
		}, cargowiseClientServices);
		ctx.ThreadExceptionExceptionRaised += ex =>
		{
			if (ex.Message == exceptionMessage)
			{
				throw exceptionThrown;
			}
			return false;
		};
		ctx.UnhandledExceptionRaised += ex =>
		{
			unhandledException = ex;
			return true;
		};

		var form = rendered.GetForm();
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			Assert.DoesNotThrow(() => Application.OnThreadException(new Exception(exceptionMessage)));
			mockService.Verify(x => x.RequestApplicationShutDownAsync(), Times.Once());
			Assert.That(unhandledException == exceptionThrown, Is.EqualTo(true));
		});
	}

	[Test]
	public async Task OnUnhandledExceptionWithHandlerWhichThrowsExceptionShouldSwallowExceptionAndExit()
	{
		using var ctx = new WinzorTestContext();
		var mockService = new Mock<ILifecycleService>();
		var cargowiseClientServices = MockCargoWiseClientServices.MakeMock(lifecycleService: mockService.Object);
		var rendered = await ctx.RenderFormAsync(() => new Form(), cargowiseClientServices);
		ctx.UnhandledExceptionRaised += ex =>
		{
			throw new InvalidOperationException("Exception thrown from Application.UnhandledException");
		};

		var form = rendered.GetForm();
		Assert.That(async () => await form.InvokeWinzorDispatcherAsync(() => Application.OnUnhandledException(new InvalidOperationException("Unhandled exception to handle"))), Throws.Nothing);
		mockService.Verify(x => x.RequestApplicationShutDownAsync(), Times.Once());
	}

	[Test]
	public async Task ApplicationIdleOnlyTriggersOnMatchingWinzorDispatcher()
	{
		using var idle1Event = new AutoResetEvent(false);
		using var idle2Event = new AutoResetEvent(false);
		using var dispatcher1 = new WinzorDispatcher(Mock.Of<IFormOpener>(), Mock.Of<IFormInstanceRegister>());
		using var dispatcher2 = new WinzorDispatcher(Mock.Of<IFormOpener>(), Mock.Of<IFormInstanceRegister>());
		await dispatcher1.InvokeAsync(() =>
		{
			Application.Idle += (s, e) => idle1Event.Set();
		});
		await dispatcher2.InvokeAsync(() =>
		{
			Application.Idle += (s, e) => idle2Event.Set();
		});
		Assert.That(idle1Event.WaitOne(TimeSpan.FromMilliseconds(10)), Is.True);
		Assert.That(idle2Event.WaitOne(TimeSpan.FromSeconds(10)), Is.True);
		await dispatcher1.InvokeAsync(() => { });
		Assert.That(idle1Event.WaitOne(TimeSpan.FromMilliseconds(10)), Is.True);
		Assert.That(idle2Event.WaitOne(TimeSpan.FromSeconds(10)), Is.False);
		await dispatcher2.InvokeAsync(() => { });
		Assert.That(idle1Event.WaitOne(TimeSpan.FromMilliseconds(10)), Is.False);
		Assert.That(idle2Event.WaitOne(TimeSpan.FromSeconds(10)), Is.True);
	}

	[Test]
	public async Task RequestApplicationShutDownAsyncIsCalledOnExit()
	{
		using var ctx = new WinzorTestContext();
		var mockService = new Mock<ILifecycleService>();
		var cargowiseClientServices = MockCargoWiseClientServices.MakeMock(new Uri("http://localhost"), mockService.Object);
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			return form;
		}, cargowiseClientServices);

		var form = rendered.GetForm();
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			Application.Exit();
			mockService.Verify(x => x.RequestApplicationShutDownAsync(), Times.Once());
		});
	}

	[Test, WithPlaywrightPage]
	public async Task ApplicationExitShouldNotThowWinzorDispatcherInvalidOperationException()
	{
		await using (var ctx = new InMemoryTestServerContext())
		{
			await ctx.LoadFormAsync(() => new Form());
			var appLifetime = ctx.HostServices.GetRequiredService<IHostApplicationLifetime>();
			appLifetime.ApplicationStopping.Register(() =>
			{
				Assert.DoesNotThrow(() => Application.Exit());
			});
		}
	}
}
