using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Blazor.Client.Integration;
using CargoWise.Blazor.Common;
using CargoWise.Blazor.Common.Testing.Auth;
using CargoWiseNext.Infrastructure.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace CargoWise.Blazor.SessionBroker.Test
{
	[KillMockAppProcesses]
	[KillSessionBrokerProcesses]
	[KillBlazorAppProcesses]
	public class SessionBrokerShutdownTest
	{
		CancellationTokenSource cancellationTokenSource;
		HttpClient client;
		const string host = "127.0.0.1";
		const int sessionBrokerPort = 6398;
		const int versionBrokerPort = 6400;

		string url => $@"http://{host}:{sessionBrokerPort}/";
		string versionbrokerUrl => $@"http://{host}:{versionBrokerPort}/";

		[SetUp]
		public void SetUp()
		{
			cancellationTokenSource = new CancellationTokenSource();
			client = new HttpClient();
		}

		[TearDown]
		public void TearDown()
		{
			cancellationTokenSource.Cancel();
			cancellationTokenSource.Dispose();
			client.Dispose();
		}

		Process StartSessionBrokerProcess()
		{
			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					Arguments = @$"--CargoWiseOptions:AppServerPathOverride .\CargoWise.Blazor.SessionBroker.Test-bin\CargoWise.Blazor.SessionBroker.Test.MockAppServer.exe --environment IntegrationTest --VersionBrokerRegistrationCallback {versionbrokerUrl} --ShutdownOptions:ShutdownThresholdSinceLastActiveAction ""00:00:10"" --ShutdownOptions:IdleWatcherStartDueTime ""00:00:01"" --urls {url} --AdditionalArguments ""--wait -1""",
					FileName = @"..\SessionBroker\CargoWise.Blazor.SessionBroker.exe",
				},
			};
			process.Start();
			return process;
		}

		async Task StartAppServerProcess()
		{
			await using var factory = new CustomWebApplicationFactory<Startup>();
			var cargoWiseOptions = factory.Services.GetRequiredService<IOptions<CargoWiseOptions>>();
			var tokenGenerator = new DebugClientTokenGenerator(cargoWiseOptions);
			var token = tokenGenerator.GenerateClientToken();
			client.DefaultRequestHeaders.Add(RequestHeaders.ClientAppUserAgent, CustomWebApplicationFactory<Startup>.DefaultUserAgentHeaderValue);
			await client.GetAsync($"{url}?{QueryParameters.ClientToken}={token}", cancellationTokenSource.Token).ConfigureAwait(false);
		}

#pragma warning disable CW1055 // Do Not Use Processes.GetProcess or Process.GetProcessByName
		static Process[] GetProcess() => Process.GetProcessesByName("CargoWise.Blazor.SessionBroker.Test.MockAppServer");
#pragma warning restore CW1055 // Do Not Use Processes.GetProcess or Process.GetProcessByName

		[Test]
		public async Task SessionBrokerShutsDownWhenNoLiveAppServerAndNoAppServerStartups([Values(1)] int numProcess)
		{
			// waiting for version broker registration request as session broker ready signal
			var waitReadySignalTask = TCPListenerHelper.WaitReadySignal(IPAddress.Parse(host), versionBrokerPort, TimeSpan.FromSeconds(15), cancellationTokenSource);
			var processesBefore = GetProcess();
			Assert.That(processesBefore, Has.Length.EqualTo(0));
			using var sessionBrokerProcess = StartSessionBrokerProcess();
			await waitReadySignalTask;
			var startAppServerTasks = Enumerable.Range(0, numProcess).Select(_ => StartAppServerProcess());
			await Task.WhenAll(startAppServerTasks);
			var processes = GetProcess();
			Assert.That(processes, Has.Length.EqualTo(numProcess));

			foreach (var process in processes)
			{
				Assert.That(() => sessionBrokerProcess.HasExited, Is.False.After(5000).PollEvery(50));
				process.Kill();
			}

			Assert.That(() => sessionBrokerProcess.HasExited, Is.True.After(30000).PollEvery(50));
		}

		[Test]
		public async Task SessionBrokerDoesNotShutdownWhenAppServersAreActive()
		{
			// waiting for version broker registration request as session broker ready signal
			var waitReadySignalTask = TCPListenerHelper.WaitReadySignal(IPAddress.Parse(host), versionBrokerPort, TimeSpan.FromSeconds(15), cancellationTokenSource);
			var processesBefore = GetProcess();
			Assert.That(processesBefore, Has.Length.EqualTo(0));
			using var sessionBrokerProcess = StartSessionBrokerProcess();

			await waitReadySignalTask;
			await StartAppServerProcess();
			var processes = GetProcess();
			Assert.That(processes, Has.Length.EqualTo(1));

			// We should not shutdown while there are active app servers
			Assert.That(() => sessionBrokerProcess.HasExited, Is.False.After(40000));
		}

		[Test]
		public async Task SessionBrokerShouldKeepActiveAsSettingAfterLastRequest()
		{
			// waiting for version broker registration request as session broker ready signal
			var waitReadySignalTask = TCPListenerHelper.WaitReadySignal(IPAddress.Parse(host), versionBrokerPort, TimeSpan.FromSeconds(15), cancellationTokenSource);
			var processesBefore = GetProcess();
			Assert.That(processesBefore, Has.Length.EqualTo(0));
			using var sessionBrokerProcess = StartSessionBrokerProcess();

			await waitReadySignalTask;
			await StartAppServerProcess();
			var processes = GetProcess();
			Assert.That(processes, Has.Length.EqualTo(1));

			foreach (var process in processes)
			{
				process.Kill();
			}

			Assert.That(() => sessionBrokerProcess.HasExited, Is.False.After(9000));
			await StartAppServerProcess();
			var processes1 = GetProcess();

			foreach (var process in processes1)
			{
				process.Kill();
			}

			Assert.That(() => sessionBrokerProcess.HasExited, Is.False.After(9000));
			Assert.That(() => sessionBrokerProcess.HasExited, Is.True.After(15000));
		}
	}
}
