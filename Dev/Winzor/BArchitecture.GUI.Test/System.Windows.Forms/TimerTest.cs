using System.Threading.Tasks;
using AngleSharp.Dom;
using Bunit;
using CargoWise.Blazor.Client.Integration.Messaging;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WinzorTestFramework;

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits

namespace System.Windows.Forms;

class TimerTest
{
	[Test]
	public async Task UpdateRenderedControlFromTimer()
	{
		using var ctx = new WinzorTestContext();
		Timer timer = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			ctx.Using(form);
			var textBox = new TextBox();
			form.Controls.Add(textBox);
			timer = new Timer();
			timer.Tick += (s, e) => textBox.BackColor = Drawing.Color.Red;
			form.Disposed += (s, e) => timer.Dispose();
			return form;
		});
		Assert.That(rendered.Find("input").GetAttribute("style"), Does.Not.Contain("background-color:#FF0000FF"));
		await ctx.WinzorDispatcher.InvokeAsync(timer.Start);
		rendered.WaitForState(() => rendered.Find("input").GetAttribute("style").Contains("background-color:#FF0000FF"));
	}

	[Test]
	public async Task OpenFormFromTimer()
	{
		var formInstanceRegister = new Mock<IFormInstanceRegister>();
		using var ctx = new WinzorTestContext(new FormOpener(), formInstanceRegister.Object);

		var isNewActiveForm = false;
		var isNewForm = false;
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

		Timer timer = null;
		Form newForm = null;
		await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			ctx.Using(form);
			var textBox = new TextBox();
			form.Controls.Add(textBox);
			timer = new Timer();
			timer.Tick += (s, e) =>
			{
				(newForm = new Form()).Show();
			};
			form.Disposed += (s, e) => timer.Dispose();
			return form;
		}, clientServices);
		await ctx.WinzorDispatcher.InvokeAsync(timer.Start);
		Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);
		Assert.That(loadRequestUrl, Is.EqualTo(expectedLoadRequestUrl));
		newForm.Dispose();
	}

	[Test]
	public async Task ExceptionFromTickEventHandled()
	{
		using var ctx = new WinzorTestContext();
		var exceptionHandled = new TaskCompletionSource<Exception>();
		ctx.ThreadExceptionExceptionRaised += ex =>
		{
			exceptionHandled.SetResult(ex);
			return true;
		};
		Timer timer = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			ctx.Using(form);
			var textBox = new TextBox();
			form.Controls.Add(textBox);
			timer = new Timer();
			timer.Tick += (s, e) => throw new Exception("Fail");
			form.Disposed += (s, e) => timer.Dispose();
			return form;
		});
		await ctx.WinzorDispatcher.InvokeAsync(timer.Start);
		Assert.That(await exceptionHandled.Task.WithTimeout(TimeSpan.FromSeconds(1)));
		Assert.That((await exceptionHandled.Task).Message, Is.EqualTo("Fail"));
	}

	[Test]
	public async Task ExceptionFromRenderTaskHandled()
	{
		using var ctx = new WinzorTestContext();
		var exceptionHandled = new TaskCompletionSource<Exception>();
		ctx.ThreadExceptionExceptionRaised += ex =>
		{
			exceptionHandled.SetResult(ex);
			return true;
		};
		Timer timer = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			ctx.Using(form);
			var textBox = new TextBox();
			form.Controls.Add(textBox);
			timer = new Timer();
			timer.Tick += (s, e) => ctx.WinzorDispatcher.CurrentContext.RegisterRenderTask(Task.FromException(new Exception("Fail")));
			form.Disposed += (s, e) => timer.Dispose();
			return form;
		});
		await ctx.WinzorDispatcher.InvokeAsync(timer.Start);
		Assert.That(await exceptionHandled.Task.WithTimeout(TimeSpan.FromSeconds(1)));
		Assert.That((await exceptionHandled.Task).Message, Is.EqualTo("Fail"));
	}

	[Test]
	public async Task TimerTriggeredRenderTaskDoesNotDeadlockWithDispatcher()
	{
		using var ctx = new WinzorTestContext();
		var callbackExecuted = new TaskCompletionSource();
		Timer timer = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			ctx.Using(form);
			var textBox = new TextBox();
			form.Controls.Add(textBox);
			timer = new Timer();
			timer.Tick += (s, e) => ctx.WinzorDispatcher.CurrentContext.RegisterRenderTask(ctx.WinzorDispatcher.InvokeAsync(() => callbackExecuted.SetResult()));
			form.Disposed += (s, e) => timer.Dispose();
			return form;
		});
		await ctx.WinzorDispatcher.InvokeAsync(timer.Start);
		Assert.That(await callbackExecuted.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);
	}

	[Test]
	public async Task TimerQueueAfterDispatcherDisposeDoesNotThrow()
	{
		var dispatcher = new WinzorDispatcher(Mock.Of<IFormOpener>(), Mock.Of<IFormInstanceRegister>());
		var callbackExecuted = new TaskCompletionSource();
		await dispatcher.InvokeAsync(() =>
		{
			var timer = new Timer() { Interval = 1000 };
			timer.Tick += (s, e) => callbackExecuted.SetResult();
			timer.Start();
		});
		dispatcher.Dispose();
		Assert.That(await callbackExecuted.Task.WithTimeout(TimeSpan.FromSeconds(2)), Is.False);
	}

	[Test]
	public async Task OnBeforeRenderCalled()
	{
		using var ctx = new WinzorTestContext();
		Timer timer = null;
		ControlWithOnBeforeRender control = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			ctx.Using(form);
			control = new ControlWithOnBeforeRender();
			form.Controls.Add(control);
			timer = new Timer();
			timer.Tick += (s, e) => control.Text = "Updated";
			form.Disposed += (s, e) => timer.Dispose();
			return form;
		});
		control.OnBeforeRenderCalled = false;
		await ctx.WinzorDispatcher.InvokeAsync(timer.Start);
		rendered.WaitForState(() => rendered.Find(".label").GetInnerText() == "Updated");
		Assert.That(control.OnBeforeRenderCalled, Is.True);
	}

	[Test]
	public async Task TimerStopsTickingWhenStopped()
	{
		using var ctx = new WinzorTestContext();
		Timer timer = null;

		try
		{
			var ticks = 0;
			var rendered = await ctx.RenderFormAsync(() =>
			{
				var form = new Form();
				ctx.Using(form);
				timer = new Timer();
				timer.Interval = 10;
				timer.Tick += (s, e) =>
				{
					ticks++;
				};
				timer.Start();
				return form;
			});

			Assert.That(timer.Interval, Is.EqualTo(10));
			Assert.That(() => ticks, Is.GreaterThan(0).After(100, 10));

			await ctx.WinzorDispatcher.InvokeAsync(() =>
			{
				timer.Stop();
			});

			var currentTicks = ticks;

			Assert.That(() => ticks, Is.EqualTo(currentTicks).After(200));
		}
		finally
		{
			timer.Dispose();
		}
	}

	[Test]
	public async Task TimerStopsTickingWhenStoppedInTick()
	{
		using var ctx = new WinzorTestContext();
		Timer timer = null;

		try
		{
			var ticks = 0;
			var rendered = await ctx.RenderFormAsync(() =>
			{
				var form = new Form();
				ctx.Using(form);
				timer = new Timer();
				timer.Interval = 10;
				timer.Tick += (s, e) =>
				{
					ticks++;
					timer.Stop();
				};
				timer.Start();
				return form;
			});

			Assert.That(timer.Interval, Is.EqualTo(10));
			Assert.That(() => ticks, Is.EqualTo(1).After(100, 10));
		}
		finally
		{
			timer.Dispose();
		}
	}

	class ControlWithOnBeforeRender : Label
	{
		protected internal override void OnBeforeRender()
		{
			OnBeforeRenderCalled = true;
		}

		public bool OnBeforeRenderCalled;
	}
}
