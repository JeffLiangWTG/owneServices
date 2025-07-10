using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Blazor.Client.Integration;
using CargoWise.Blazor.Common;
using CargoWise.Blazor.Common.Testing.Auth;
using CargoWise.Definitions;
using CargoWiseNext.Infrastructure.Authentication;
using Enterprise.Environment;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace CargoWise.Blazor.SessionBroker.Test
{
	[KillMockAppProcesses]
	[KillSessionBrokerProcesses]
	[KillBlazorAppProcesses]
	public class AppServerExitCodeTests
	{
		//const string url = @"http://127.0.0.1:6397/";
		const string host = "127.0.0.1";
		const int sessionBrokerPort = 6397;
		const int versionBrokerPort = 6400;
		string url => $@"http://{host}:{sessionBrokerPort}/";
		string versionbrokerUrl => $@"http://{host}:{versionBrokerPort}/";
		CancellationTokenSource cancellationTokenSource;

		[SetUp]
		public void SetUp()
		{
			cancellationTokenSource = new CancellationTokenSource();
		}

		[TearDown]
		public void TearDown()
		{
			cancellationTokenSource.Cancel();
			cancellationTokenSource.Dispose();
		}

		async Task SendRequest()
		{
			using var factory = new CustomWebApplicationFactory<Startup>();
			var cargoWiseOptions = factory.Services.GetRequiredService<IOptions<CargoWiseOptions>>();
			var tokenGenerator = new DebugClientTokenGenerator(cargoWiseOptions);
			var token = tokenGenerator.GenerateClientToken();

			using var client = new HttpClient();
			client.DefaultRequestHeaders.Add(RequestHeaders.ClientAppUserAgent, CustomWebApplicationFactory<Startup>.DefaultUserAgentHeaderValue);
			await client.GetAsync($"{url}?{QueryParameters.ClientToken}={token}", cancellationTokenSource.Token).ConfigureAwait(false);
		}

		Process StartProcess(int specifiedExitCode)
		{
			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					Arguments = @$"--CargoWiseOptions:AppServerPathOverride .\CargoWise.Blazor.SessionBroker.Test-bin\CargoWise.Blazor.SessionBroker.Test.MockAppServer.exe --environment IntegrationTest --VersionBrokerRegistrationCallback {versionbrokerUrl} --urls {url} --AdditionalArguments ""--exit {specifiedExitCode}""",
					FileName = @"..\SessionBroker\CargoWise.Blazor.SessionBroker.exe",
				},
			};
			process.Start();
			return process;
		}

		[Test]
		public async Task TestAppServerExitsDueToDatabaseUpgrade_SessionBrokerExitsWithSameExitCode()
		{
			var waitReadySignalTask = TCPListenerHelper.WaitReadySignal(IPAddress.Parse(host), versionBrokerPort, TimeSpan.FromSeconds(10), cancellationTokenSource);
			using var process = StartProcess(ExitCodes.DatabaseUpgraded);
			await waitReadySignalTask;
			await SendRequest();
			Assert.That(() => process.HasExited, Is.True.After(5000).PollEvery(100));
			Assert.That(process.ExitCode, Is.EqualTo(ExitCodes.DatabaseUpgraded));
		}

		[Test]
		public async Task TestAppServerExitsDueToVersionUpgrade_SessionBrokerExitsWithSameExitCode()
		{
			var waitReadySignalTask = TCPListenerHelper.WaitReadySignal(IPAddress.Parse(host), versionBrokerPort, TimeSpan.FromSeconds(10), cancellationTokenSource);
			using var process = StartProcess(ExitCodes.VersionUpgraded);
			await waitReadySignalTask;
			await SendRequest();
			Assert.That(() => process.HasExited, Is.True.After(5000).PollEvery(100));
			Assert.That(process.ExitCode, Is.EqualTo(ExitCodes.VersionUpgraded));
		}

		[Test]
		public async Task TestAppServerExitsDueToOtherExitCode_SessionBrokerDoesntExit()
		{
			using var process = StartProcess(ExitCodes.Unspecified);
			var waitReadySignalTask = TCPListenerHelper.WaitReadySignal(IPAddress.Parse(host), versionBrokerPort, TimeSpan.FromSeconds(10), cancellationTokenSource);
			await waitReadySignalTask;
			Assert.That(() => process.HasExited, Is.False);
			await SendRequest();
			Assert.That(() => process.HasExited, Is.False.After(5000).PollEvery(50));
		}
	}
}
