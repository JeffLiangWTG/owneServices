using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Blazor.Testing.Common;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Yarp.ReverseProxy.Configuration;
using static CargoWise.Blazor.SessionBroker.Test.TestHelpers;

namespace CargoWise.Blazor.SessionBroker.Test
{
	[KillBlazorAppProcesses]
	public class AffinityTests
	{
		[Test]
		public async Task DifferentClientsGetDifferentAffinityCookies()
		{
			string affinityCookie1 = null, affinityCookie2 = null;
			using var factory = new CustomWebApplicationFactory<Startup>();
			await CreateAffinitisedSession(factory, response => affinityCookie1 = GetAffinityCookie(response, factory.Server.BaseAddress));
			await CreateAffinitisedSession(factory, response => affinityCookie2 = GetAffinityCookie(response, factory.Server.BaseAddress));

			Assert.That(affinityCookie1, Is.Not.Null);
			Assert.That(affinityCookie2, Is.Not.Null);
			Assert.That(affinityCookie1, Is.Not.EqualTo(affinityCookie2));
		}

		[Test]
		public async Task ClientWithAffinityGoesToSameProcess()
		{
			using var factory = new CustomWebApplicationFactory<Startup>();
			var client = await CreateAffinitisedSession(factory);
			var pid1 = await GetProcessId(client);
			var pid2 = await GetProcessId(client);
			Assert.That(pid1, Is.GreaterThan(0));
			Assert.That(pid1, Is.EqualTo(pid2));
		}

		[Test]
		public async Task ClusterConfigRecordsProcessId()
		{
			using var factory = new CustomWebApplicationFactory<Startup>();
			var proxyConfig = factory.Services.GetRequiredService<IProxyConfigProvider>();
			Assert.That(proxyConfig.GetConfig().Clusters.Single().Destinations, Has.Count.Zero);

			var client = await CreateAffinitisedSession(factory);
			var expectedPid = await GetProcessId(client);
			var destination = proxyConfig.GetConfig().Clusters.Single().Destinations.Single().Value;
			Assert.That(destination.Metadata, Has.Exactly(1).Items);
			Assert.That(destination.Metadata, Contains.Key("ProcessId"));
			Assert.That(destination.Metadata, Contains.Value(expectedPid.ToString(CultureInfo.InvariantCulture)));
		}

		async Task<HttpClient> CreateAffinitisedSession(
			WebApplicationFactory<Startup> factory,
			Action<HttpResponseMessage> interceptAffinitsation = null)
		{
			// The flow is
			// 1. GET /
			// as you don't have an affinity cookie yet, this will redirect you to the "login" page, which is currently /Debug
			// 2. GET /Debug (happens automatically as part of 1.)
			// This contains an Antiforgery token which must be posted back to the server
			// 3. POST /Debug
			// This launches a blazor process, and redirects you (302) to /affinitise/{guid}
			// 4. GET /affinitise/{guid} (happens automatically as part of 3.)
			// This manually establishes the YARP session affinity using a cookie, and redirects you back to the home page (/)
			// 5. GET / (also happens automatically as part of 3.)

			// HttpClient will automatically follow redirects and maintain cookies, but it doesn't provide
			// a good way to inspect the response headers of the intermediate redirects
			// Therefore the easiest way to get the cookie value is to look at the cookies sent with the final *request*
			var (client, response) = await CreateTestNodeInCluster(factory);
			interceptAffinitsation?.Invoke(response);
			var processId = await GetProcessId(client);
			runningProcesses.Add(processId);
			return client;
		}

		async Task<int> GetProcessId(HttpClient client)
		{
			var response = await client.GetStringAsync("/DebugDiag");
			Assert.That(response, Does.Contain("processId"), "response should contain json with processId");
			var json = JObject.Parse(response);
			return int.Parse((string)json["processId"], CultureInfo.InvariantCulture);
		}

		[TearDown]
		public void TerminateStartedBlazorProcesses()
		{
			var processesToStop = runningProcesses.ToArray();

			if (processesToStop.Length > 0)
			{
				foreach (var pid in runningProcesses)
				{
					var process = Process.GetProcessById(pid);
					process.Kill(true);
					runningProcesses.Remove(pid);
				}
				Thread.Sleep(500); // give the process time to die and the file to then get deleted
			}
		}

		readonly HashSet<int> runningProcesses = new HashSet<int>();
	}
}
