using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async;
using CargoWise.Blazor.Common;
using CargoWise.Blazor.SessionBroker.Test;
using Newtonsoft.Json;
using NUnit.Framework;
using WTG.ToxicNetworkEffect;

namespace CargoWise.Winzor.AppServer.Test;

[KillBlazorAppProcesses]
public class AppServerIntegrationTests
{
	static string AppserverPath => Path.Combine("..", BuildFileSystem.AppServerBin.PublishExePath);
	const string listen = "localhost:9000";
	const string upstream = "localhost:5000";
	const string proxyName = "appServerProxy";
	Uri GetAppServerUri(bool isProxy = false) => new($"http://{(isProxy ? listen : upstream)}/");

	[Test]
	[CancelAfter(60000)]
	public async Task AppServerRejectsRequestsWithoutSecurityHeader()
	{
		var secret = new SecureSecretGenerator().Generate();
		await StartServer(secret);
		using var client = new HttpClient();

		using var response = await client.GetAsync(GetAppServerUri());

		Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
	}

	[Test]
	[CancelAfter(60000)]
	public async Task AppServerAcceptsRequestsWithSecurityHeader()
	{
		var secret = new SecureSecretGenerator().Generate();
		await StartServer(secret);

		using var client = new HttpClient();

		client.DefaultRequestHeaders.Add(CustomHeaders.CWSessionToken, secret);

		var response = await client.GetAsync(GetAppServerUri());

		Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
	}

	[Test]
	[CancelAfter(60000)]
	public async Task AppServerAcceptsHealthChecksWithoutSecurityHeader()
	{
		var secret = new SecureSecretGenerator().Generate();
		await StartServer(secret);

		using var client = new HttpClient();

		var response = await client.GetAsync(new Uri(GetAppServerUri(), "/health"));

		Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
	}

	[Test]
	[CancelAfter(65000)]
	[WithLatencyNetworkEffect(Listen = listen, Upstream = upstream, ProxyName = proxyName, Latency = 5000)]
	public async Task TestAppServerStartsAndReportsHealthyWithHighLatency()
	{
		var secret = new SecureSecretGenerator().Generate();
		await StartServer(secret);

		using var client = new HttpClient();

		client.DefaultRequestHeaders.Add(CustomHeaders.CWSessionToken, secret);
		var appServerUri = GetAppServerUri(true);
		var reqTask = client.GetAsync(appServerUri);
		Assert.That(() => reqTask.IsCompleted, Is.False.After(4000));
		var response = await reqTask;
		Assert.That(response.IsSuccessStatusCode, Is.True);

		var healthReqTask = client.GetAsync(new Uri(appServerUri, "/health"));
		Assert.That(() => healthReqTask.IsCompleted, Is.False.After(4000));
		var healthResponse = await healthReqTask;
		Assert.That(healthResponse.IsSuccessStatusCode, Is.True);
	}

	[Test]
	public async Task TestAppServerGarbageCollectionMode()
	{
		var secret = new SecureSecretGenerator().Generate();
		var standardOutput = await StartServer(secret);

		Assert.That(standardOutput, Does.Contain("AppServer is currently using : \"Workstation GC\""));
	}

	static async Task<string> StartServer(string sessionToken, string clientToken = null)
	{
		var eventName = Guid.NewGuid().ToString();
		using var ewh = new EventWaitHandle(false, EventResetMode.ManualReset, eventName);

		using var process = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = AppserverPath,
				Arguments = $"--urls http://{upstream} --CargoWiseOptions:ReadConfigFromStdIn=True --CargoWiseOptions:VersionBrokerProcessCorrelationId={Guid.NewGuid()} --CargoWiseOptions:SessionBrokerProcessCorrelationId={Guid.NewGuid()} --SignalEventWhenStarted {eventName}",
				RedirectStandardInput = true
			}
		};

		process.StartInfo.RedirectStandardOutput = true;

		process.Start();

		var authOptions = new
		{
			CargoWiseAuthOptions = new CargoWiseAuthOptions
			{
				ClientToken = clientToken,
				SessionToken = sessionToken,
			}
		};

		await process.StandardInput.WriteLineAsync(JsonConvert.SerializeObject(authOptions));
		process.StandardInput.Close();

		var standardOut = string.Empty;

		while (!(process.StandardOutput.EndOfStream || standardOut.Contains("AppServer is currently using :")))
		{
			standardOut = await process.StandardOutput.ReadLineAsync();
		}

		await ewh.WaitOneAsync();

		return standardOut;
	}
}
