using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.Winzor.Architecture.Test;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace CargoWise.Winzor.AppServer.Test;

internal class GlobalExceptionHandlerTest
{
	[Test, WithPlaywrightPage]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD110:Observe result of async calls", Justification = "<Pending>")]
	public async Task HandleUnobservedException()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var globalExceptionHandler = ctx.HostServices.GetRequiredService<GlobalExceptionHandler>();
		var mockLogger = new Mock<ILogger<GlobalExceptionHandler>>();
		globalExceptionHandler.SetLogger(mockLogger.Object);

		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var button = new Button { Text = "Test" };
			button.Click += Button_Click;
			return button;
		});

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD110:Observe result of async calls", Justification = "<Pending>")]
		void Button_Click(object sender, EventArgs e)
		{
			Task.Run(() =>
			{
				throw new InvalidOperationException("This is a test exception.");
			});
		}
		await page.Locator("button:has-text('Test')").ClickAsync();
		await Task.Delay(1000);
		GC.Collect();
		GC.WaitForPendingFinalizers();
		mockLogger.Verify(
			logger => logger.Log(
				LogLevel.Error,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v, t) => true),
				It.Is<AggregateException>(ex =>
					ex.InnerException is InvalidOperationException &&
					ex.InnerException.TargetSite.DeclaringType.FullName.Contains("CargoWise.Winzor.AppServer.Test.GlobalExceptionHandlerTest") &&
					ex.InnerException.TargetSite.Name.Contains("HandleUnobservedException") &&
					ex.Message == "A Task's exception(s) were not observed either by Waiting on the Task or accessing its Exception property. As a result, the unobserved exception was rethrown by the finalizer thread. (This is a test exception.)"),
				(Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
			Times.Once);
	}

	[Test, WithPlaywrightPage]
	public async Task HandleNullReferenceExceptionOnClientProxyExtensions()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var globalExceptionHandler = ctx.HostServices.GetRequiredService<GlobalExceptionHandler>();
		var mockLogger = new Mock<ILogger<GlobalExceptionHandler>>();
		globalExceptionHandler.SetLogger(mockLogger.Object);
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var form = new Form();
			var button = new Button { Text = "Test" };
			button.Click += (sender, e) =>
			{
				Thread.Sleep(1000);
				button.Text = "Clicked";
			};
			return button;
		});
		await page.Locator("button:has-text('Test')").ClickAsync();
		await Task.Delay(500);
		await page.EvaluateAsync("Blazor.disconnect()");
		await Task.Delay(1000);
		GC.Collect();
		GC.WaitForPendingFinalizers();
		mockLogger.Verify(
			logger => logger.Log(
				LogLevel.Warning,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v, t) => true),
				It.Is<AggregateException>(ex =>
					ex.InnerException is NullReferenceException &&
					ex.InnerException.TargetSite.DeclaringType.FullName == "Microsoft.AspNetCore.SignalR.ClientProxyExtensions" &&
					ex.InnerException.TargetSite.Name == "SendAsync" &&
					ex.Message == "A Task's exception(s) were not observed either by Waiting on the Task or accessing its Exception property. As a result, the unobserved exception was rethrown by the finalizer thread. (Object reference not set to an instance of an object.)"),
				(Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
			Times.Once);
		WithPlaywrightPageAttribute.Context.PageErrors.Clear();
	}

	[Test, WithPlaywrightPage]
	public async Task ShouldNotThrowNullReferenceExceptionOnHandleAltKeyPressAsyncManyTimes()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var globalExceptionHandler = ctx.HostServices.GetRequiredService<GlobalExceptionHandler>();
		var mockLogger = new Mock<ILogger<GlobalExceptionHandler>>();
		globalExceptionHandler.SetLogger(mockLogger.Object);
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var button = new Button { Text = "&Test" };
			return button;
		});

		await page.Keyboard.DownAsync("Alt");
		await page.Keyboard.DownAsync("Alt");
		await page.Keyboard.DownAsync("Alt");
		await Task.Delay(1000);
		GC.Collect();
		GC.WaitForPendingFinalizers();
		var matchAggregateException = (Exception ex) =>
		{
			var firstInnerExceptionTriggered = ex.InnerException is AggregateException aggregateException &&
					aggregateException.TargetSite.DeclaringType.FullName == "System.Threading.Tasks.Task`1" &&
					aggregateException.TargetSite.Name == "GetResultCore" &&
					aggregateException.Message == "One or more errors occurred. (Object reference not set to an instance of an object.)";

			var secondInnerExceptionTriggered = ex.InnerException.InnerException is NullReferenceException nullReferenceException &&
					nullReferenceException.TargetSite.DeclaringType.FullName == "WinzorFramework.JSInterop.RegisteredClientEvent+<DisposeAsync>d__2" &&
					nullReferenceException.TargetSite.Name == "MoveNext" &&
					nullReferenceException.Message == "Object reference not set to an instance of an object.";
			return firstInnerExceptionTriggered && secondInnerExceptionTriggered;
		};

		mockLogger.Verify(
			logger => logger.Log(
				LogLevel.Error,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v, t) => true),
				It.Is<AggregateException>(ex => matchAggregateException(ex)),
				(Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
			Times.Never);
		WithPlaywrightPageAttribute.Context.PageErrors.Clear();
	}
}
