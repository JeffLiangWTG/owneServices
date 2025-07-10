using System;
using System.Diagnostics;
using System.IO;
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
	public class AppServerProcessArgumentsTest
	{
		const string host = "127.0.0.1";
		const int sessionBrokerPort = 6399;
		const int versionBrokerPort = 6400;
		string url => $@"http://{host}:{sessionBrokerPort}/";
		CancellationTokenSource cancellationTokenSource;
		string versionbrokerUrl => $@"http://{host}:{versionBrokerPort}/";

		readonly Guid versionBrokerProcessCorrelationId = Guid.NewGuid();
		readonly Guid sessionBrokerProcessCorrelationId = Guid.NewGuid();
		readonly string hostname = "testhostname";

		string VersionBrokerProcessCorrelationIdLogFile => Path.Combine(SetUpTests.BlazorAppProcInfoDirectory, "VersionBrokerProcessCorrelationId");
		string SessionBrokerProcessCorrelationIdLogFile => Path.Combine(SetUpTests.BlazorAppProcInfoDirectory, "SessionBrokerProcessCorrelationId");
		string HostnameLogFile => Path.Combine(SetUpTests.BlazorAppProcInfoDirectory, "Hostname");

		[SetUp]
		public void SetUp()
		{
			cancellationTokenSource = new CancellationTokenSource();
		}
		[TearDown]
		public void DeleteLogFiles()
		{
			cancellationTokenSource.Cancel();
			cancellationTokenSource.Dispose();
			if (File.Exists(VersionBrokerProcessCorrelationIdLogFile))
			{
				File.Delete(VersionBrokerProcessCorrelationIdLogFile);
			}
			if (File.Exists(SessionBrokerProcessCorrelationIdLogFile))
			{
				File.Delete(SessionBrokerProcessCorrelationIdLogFile);
			}
			if (File.Exists(HostnameLogFile))
			{
				File.Delete(HostnameLogFile);
			}
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

		Process StartProcess(bool includeVersionBrokerProcessCorrelationId = false, bool includeHostname = false)
		{
			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					Arguments = @"--CargoWiseOptions:AppServerPathOverride .\CargoWise.Blazor.SessionBroker.Test-bin\CargoWise.Blazor.SessionBroker.Test.MockAppServer.exe " +
						$"{(includeVersionBrokerProcessCorrelationId ? $"--CargoWiseOptions:VersionBrokerProcessCorrelationId {versionBrokerProcessCorrelationId} " : "")}" +
						$"--CargoWiseOptions:SessionBrokerProcessCorrelationId {sessionBrokerProcessCorrelationId} " +
						"--environment IntegrationTest " +
						$"{(includeHostname ? $"--CargoWiseOptions:Hostname {hostname} " : "")}" +
						$"--VersionBrokerRegistrationCallback {versionbrokerUrl} " +
						$"--urls {url} ",
					FileName = @"..\SessionBroker\CargoWise.Blazor.SessionBroker.exe",
				},
			};
			process.Start();
			return process;
		}

		async Task AssertLogFileContents(string logFile, string expectedValue)
		{
			Assert.That(File.Exists(logFile), Is.True);

			var readContents = await File.ReadAllTextAsync(logFile);
			Assert.That(readContents, Is.EqualTo(expectedValue));
		}

		[Test]
		public async Task TestProcessCorrelationIdsArePassedToAppServerCorrectly_VersionBrokerIdNonNull()
		{
			using var process = StartProcess(includeVersionBrokerProcessCorrelationId: true);
			var waitReadySignalTask = TCPListenerHelper.WaitReadySignal(IPAddress.Parse(host), versionBrokerPort, TimeSpan.FromSeconds(10), cancellationTokenSource);
			await waitReadySignalTask;
			Assert.That(() => process.HasExited, Is.False);
			await SendRequest();
			Assert.That(() => process.HasExited, Is.False.After(5000).PollEvery(50));

			await AssertLogFileContents(VersionBrokerProcessCorrelationIdLogFile, versionBrokerProcessCorrelationId.ToString());
			await AssertLogFileContents(SessionBrokerProcessCorrelationIdLogFile, sessionBrokerProcessCorrelationId.ToString());
		}

		[Test]
		public async Task TestProcessCorrelationIdsArePassedToAppServerCorrectly_VersionBrokerIdNull()
		{
			using var process = StartProcess(includeVersionBrokerProcessCorrelationId: false);
			var waitReadySignalTask = TCPListenerHelper.WaitReadySignal(IPAddress.Parse(host), versionBrokerPort, TimeSpan.FromSeconds(10), cancellationTokenSource);
			await waitReadySignalTask;
			await SendRequest();
			Assert.That(() => process.HasExited, Is.False.After(5000).PollEvery(50));

			Assert.That(!File.Exists(VersionBrokerProcessCorrelationIdLogFile));
			await AssertLogFileContents(SessionBrokerProcessCorrelationIdLogFile, sessionBrokerProcessCorrelationId.ToString());
		}

		[Test]
		public async Task TestHostnamePassedToAppServerCorrectly_HostnameNonNull()
		{
			using var process = StartProcess(includeHostname: true);
			var waitReadySignalTask = TCPListenerHelper.WaitReadySignal(IPAddress.Parse(host), versionBrokerPort, TimeSpan.FromSeconds(10), cancellationTokenSource);
			await waitReadySignalTask;
			Assert.That(() => process.HasExited, Is.False);
			await SendRequest();
			Assert.That(() => process.HasExited, Is.False.After(5000).PollEvery(50));

			await AssertLogFileContents(HostnameLogFile, hostname);
			await AssertLogFileContents(SessionBrokerProcessCorrelationIdLogFile, sessionBrokerProcessCorrelationId.ToString());
		}

		[Test]
		public async Task TestHostnamePassedToAppServerCorrectly_HostnameNull()
		{
			using var process = StartProcess(includeHostname: false);
			var waitReadySignalTask = TCPListenerHelper.WaitReadySignal(IPAddress.Parse(host), versionBrokerPort, TimeSpan.FromSeconds(10), cancellationTokenSource);
			await waitReadySignalTask;
			Assert.That(() => process.HasExited, Is.False);
			await SendRequest();
			Assert.That(() => process.HasExited, Is.False.After(5000).PollEvery(50));

			Assert.That(!File.Exists(HostnameLogFile));
			await AssertLogFileContents(SessionBrokerProcessCorrelationIdLogFile, sessionBrokerProcessCorrelationId.ToString());
		}
	}
}
