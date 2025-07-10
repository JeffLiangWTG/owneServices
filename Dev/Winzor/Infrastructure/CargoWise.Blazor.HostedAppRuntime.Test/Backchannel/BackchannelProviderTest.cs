using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.Blazor.Common;
using CargoWise.Blazor.HostedAppRuntime.Backchannel;
using Enterprise.BlazorWinFormsInterop;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace CargoWise.Blazor.HostedAppRuntime.Test.Backchannel
{
	public class BackchannelProviderTest
	{
		// If you're a developer running this test in VS, run ONCE as admin and it will set up the necessary URL ACL.
		// Future runs won't need admin.
		[Test, CancelAfter(120000), Property("DAT:CapabilityRequirements", "ADMIN,GUI")]
		public async Task TestBackchannelSeparateProcess()
		{
			AllowHttpConnection();

			// The first attempt at this test was to call Enterprise.BlazorWinFormsInterop.WinFormsListener
			// directly from the test. That approach does not work because ASP.NET (pre-Core) SignalR *server*
			// cannot run in a .NET Core/.NET 5 runtime. It requires certain crypto libraries (DPAPI)
			// which are only in .NET Framework. (The SignalR *client* can run OK, which this test
			// demonstrates.)
			//
			// Instead, we have to launch a full Cargowise WinForms .NET Framework process and connect to it.
			// This is a better test in that it is more realistic of how hybrid mode will actually run.

			var config = new TestConfiguration();
			var cwOptions = ConfigurationBinder.Get<CargoWiseOptions>(config.GetSection("CargoWiseOptions")) ?? new CargoWiseOptions();

			using var winFormsProcess = Process.Start(new ProcessStartInfo
			{
				FileName = Path.Combine(Assembly.GetExecutingAssembly().Location, "..", "..", "CargoWise.WindowsDesktop.exe"),
				Arguments = $"{cwOptions.DbServerName} {cwOptions.DatabaseName} {Enterprise.Startup.ApplicationArguments.OptionStartBlazorWinFormsInterop}",
				RedirectStandardOutput = true,
			});

			try
			{
				var backchannelUrl = await winFormsProcess.StandardOutput.ReadLineAsync();

				// check process is still running and fail with an explicit message if it's not (winFormsProces may crash or be killed, readline will not throw)
				// hint: check the Windows Application event log for a .NET Runtime error
				if (winFormsProcess.HasExited)
				{
					// check HasExited as accessing these Process properties will throw if HasExited: false (ie, Assert.That(..., message: <exit properties>))
					// include readline result on the off chance something was captured
					Assert.Fail(message: $"WinForms process is expected to run until stopped" +
						$" ({nameof(winFormsProcess.StartTime)}: {winFormsProcess.StartTime}" +
						$", {nameof(winFormsProcess.ExitTime)}: {winFormsProcess.ExitTime}" +
						$", {nameof(winFormsProcess.ExitCode)}: {winFormsProcess.ExitCode}" +
						$", StdOutReadLine: {backchannelUrl})");
				}

				Assert.That(backchannelUrl, Is.Not.Null);
				Assert.That(backchannelUrl, Is.Not.Empty);

				var options = new Mock<IOptions<CargoWiseOptions>>();
				options.Setup(option => option.Value).Returns(new CargoWiseOptions());

				var hubConnector = new HubConnector();

				var backchannelProvider = new BackchannelProvider(options.Object, Mock.Of<ILogger<BackchannelProvider>>(), hubConnector);
				string echoReplyData = null;

				await backchannelProvider.OpenBackchannelAsync(backchannelUrl);

#pragma warning disable VSTHRD103 // Call async methods when in an async method
				Assert.That(backchannelProvider.HubConnector.ConnectionStarted.Task.Wait(60000), Is.True, message: "ConnectionStarted");

				backchannelProvider.HubConnector.HubProxy.On(nameof(IBlazorClient.EchoReply), (string data) => echoReplyData = data);
				Assert.That(backchannelProvider.HubConnector.HubProxy.Invoke(nameof(IBlazorHub.BackchannelConnected)).Wait(10000), Is.True, message: "BackchannelConnected");

				Assert.That(backchannelProvider.HubConnector.HubProxy.Invoke(nameof(IBlazorHub.Echo), "hello world").Wait(10000), Is.True, message: "Echo");
#pragma warning restore VSTHRD103 // Call async methods when in an async method
				Assert.That(echoReplyData, Is.EqualTo("hello world"));

				hubConnector.HubConnection.Stop();
				hubConnector.HubConnection.Dispose();
			}
			finally
			{
				if (!winFormsProcess.HasExited)
				{
					winFormsProcess.Kill();
				}
			}
		}

		[TestCase(typeof(HttpRequestException))]
		[TestCase(typeof(HttpClientException))]
		public void TestBackchannelExceptOnHttpRequestAndHttpClientException(Type exceptionType)
		{
			AllowHttpConnection();

			var backchannelUrl = "https://localhost:5750/example.com";

			var options = new Mock<IOptions<CargoWiseOptions>>();
			options.Setup(option => option.Value).Returns(new CargoWiseOptions());

			var mockHubConnector = new Mock<IHubConnector>();
			var exceptionInstance = (Exception)Activator.CreateInstance(exceptionType);
			mockHubConnector.Setup(hubConnector => hubConnector.TryOpenBackchannelAsync(It.IsAny<string>())).Throws(() => exceptionInstance);

			Assert.DoesNotThrowAsync(() => new BackchannelProvider(options.Object, Mock.Of<ILogger<BackchannelProvider>>(), mockHubConnector.Object).OpenBackchannelAsync(backchannelUrl));
		}

		[Test, CancelAfter(120000)]
		public void TestBackchannelThrowsErrorOnUnhandledException()
		{
			AllowHttpConnection();

			var backchannelUrl = "https://localhost:5750/example.com";

			var options = new Mock<IOptions<CargoWiseOptions>>();
			options.Setup(option => option.Value).Returns(new CargoWiseOptions());

			var mockHubConnector = new Mock<IHubConnector>();
			mockHubConnector.Setup(hubConnector => hubConnector.TryOpenBackchannelAsync(It.IsAny<string>())).Throws<InvalidOperationException>();

			Assert.ThrowsAsync<InvalidOperationException>(() => new BackchannelProvider(options.Object, Mock.Of<ILogger<BackchannelProvider>>(), mockHubConnector.Object).OpenBackchannelAsync(backchannelUrl));
		}

		[Test, CancelAfter(120000)]
		public async Task TestClientAppAndAppServerExitOnBackchannelFailure()
		{
			AllowHttpConnection();

			var backchannelUrl = "https://localhost:5750/example.com";

			var options = new Mock<IOptions<CargoWiseOptions>>();
			options.Setup(option => option.Value).Returns(new CargoWiseOptions());

			var mockLogger = new Mock<ILogger<BackchannelProvider>>();

			var mockHubConnector = new Mock<IHubConnector>();
			mockHubConnector.Setup(hubConnector => hubConnector.TryOpenBackchannelAsync(It.IsAny<string>())).Throws<HttpClientException>();
			await new BackchannelProvider(options.Object, mockLogger.Object, mockHubConnector.Object).OpenBackchannelAsync(backchannelUrl);

			mockLogger.Verify(logger => logger.Log(LogLevel.Information, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().StartsWith("Could not establish backchannel connection after retrying")), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
		}

		// Copied from Enterprise.ServiceManager.Host.Testing.Core.Http.HttpRequestListenerTest
		static void AllowHttpConnection()
		{
			var process = Process.Start("cmd", FormattableString.Invariant($"/C netsh http add urlacl url={WinFormsListener.UrlAclUrlPrefix} user=everyone"));
			process?.WaitForExit();
		}
	}
}
