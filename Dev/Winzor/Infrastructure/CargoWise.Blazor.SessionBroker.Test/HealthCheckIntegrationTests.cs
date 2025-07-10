using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CargoWise.Blazor.Common;
using CargoWise.Blazor.Testing.Common;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Yarp.ReverseProxy.Configuration;

namespace CargoWise.Blazor.SessionBroker.Test
{
	using static TestHelpers;

	[KillBlazorAppProcesses]
	public class HealthCheckIntegrationTests
	{
		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		const int DelaySecondsForHealthCheck = 20; // health check is supposed to be every 10 seconds, so this seems high,
												   // but by experimentation, the tests don't pass reliably with a lower number

		[Test]
		public async Task TestSessionNoLongerWorksIfProcessIsKilled()
		{
			using var factory = new CustomWebApplicationFactory<Startup>();
			var (client, _) = await CreateTestNodeInCluster(factory);

			var clusterConfig = factory.Services.GetRequiredService<IProxyConfigProvider>().GetConfig();
			var clusterDestination = clusterConfig.Clusters.Single().Destinations.Single().Value;

			var pid = int.Parse(clusterDestination.Metadata.Single().Value, CultureInfo.InvariantCulture);

			Process.GetProcessById(pid).Kill(true);

			await Task.Delay(TimeSpan.FromSeconds(DelaySecondsForHealthCheck));

			var response = await client.GetAsync("/");
			Assert.That(response.RequestMessage.RequestUri.AbsolutePath, Is.EqualTo("/_sessionbroker/auth"));
		}

		[Test]
		public async Task TestSessionNoLongerWorksIfProcessIsReplaced()
		{
			using var oicdConfig = Application.ObjectFactory.Substitute(OIDCConfigSetupHelper.SetOidcConfigTest("http://localhost:3001/", Guid.NewGuid().ToString()));
			using var factory = new CustomWebApplicationFactory<Startup>();
			var (client, _) = await CreateTestNodeInCluster(factory);

			var clusterConfig = factory.Services.GetRequiredService<IProxyConfigProvider>().GetConfig();
			var clusterDestination = clusterConfig.Clusters.Single().Destinations.Single().Value;

			var address = clusterDestination.Address;
			var oldPid = int.Parse(clusterDestination.Metadata.Single().Value, CultureInfo.InvariantCulture);

			Process.GetProcessById(oldPid).Kill(true); // kill the blazor process and replace it with one listening on the same port
			var sessionToken = new SecureSecretGenerator().Generate();
			var psi = new ProcessStartInfo
			{
				FileName = Path.Combine(@"..", BuildFileSystem.AppServerBin.PublishExePath),
				Arguments = $@"--urls {address} --webroot .\\wwwroot --CargoWiseAuthOptions:SessionToken={sessionToken}",
			};
			var replacementProcess = Process.Start(psi);

			Assert.That(replacementProcess.Id, Is.Not.EqualTo(oldPid));
			await Task.Delay(TimeSpan.FromSeconds(DelaySecondsForHealthCheck));
			client.DefaultRequestHeaders.Add(CustomHeaders.CWSessionToken, sessionToken);
			var response = await client.GetAsync("/");
			Assert.That(response.RequestMessage.RequestUri.AbsolutePath, Is.EqualTo("/_sessionbroker/auth"));
		}

		[Test]
		public async Task TestSessionContinuesToWorkAfterAHealthCheck()
		{
			using var factory = new CustomWebApplicationFactory<Startup>();
			var (client, originalResponse) = await CreateTestNodeInCluster(factory);

			var cookie = GetAffinityCookie(originalResponse, factory.Server.BaseAddress);

			var response = await client.GetAsync("/health");
			Assert.That(response.IsSuccessStatusCode, Is.True);
			Assert.That(originalResponse.RequestMessage.RequestUri.LocalPath, Is.EqualTo("/"));
			var newCookie = GetAffinityCookie(response, factory.Server.BaseAddress);
			Assert.That(cookie, Is.EqualTo(newCookie));
		}

		[Test]
		public async Task TestSessionBrokerHealthCheckEndpoint()
		{
			using var factory = new CustomWebApplicationFactory<Startup>();
			var client = factory.CreateClient();
			var response = await client.GetAsync("/health");
			response.EnsureSuccessStatusCode();
			var content = await response.Content.ReadAsStringAsync();
			Assert.That(content, Is.EqualTo("Healthy"));
		}
	}
}
