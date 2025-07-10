using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Blazor.Client.Integration;
using CargoWise.Blazor.Common;
using CargoWise.Blazor.Common.Testing.Auth;
using CargoWiseNext.Infrastructure.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Yarp.ReverseProxy.Configuration;

namespace CargoWise.Blazor.SessionBroker.Test
{
	public class CleanupTests
	{
		[Test]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1055:DoNotUseProcessGetProcess", Justification = "Testing")]
		public async Task TestCleanup()
		{
			using var factory = new CustomWebApplicationFactory<Startup>();
			var cargoWiseOptions = factory.Services.GetRequiredService<IOptions<CargoWiseOptions>>();
			var tokenGenerator = new DebugClientTokenGenerator(cargoWiseOptions);
			var token = tokenGenerator.GenerateClientToken();
			var proxyConfigProvider = factory.Services.GetRequiredService<IProxyConfigProvider>();
			var proxyConfig = proxyConfigProvider.GetConfig();
			Assert.That(proxyConfig.Clusters.Single().Destinations, Is.Empty, "proxy cluster should initially be empty");

			using var client = factory.CreateClient();
			var response = await client.GetAsync($"/?{QueryParameters.ClientToken}={token}"); // this should start an CargoWise.Winzor.AppServer instance
			response.EnsureSuccessStatusCode();
			proxyConfig = proxyConfigProvider.GetConfig();
			Assert.That(proxyConfig.Clusters.Single().Destinations, Has.Exactly(1).Items);

			using var autoResetEvent = new AutoResetEvent(false);
			proxyConfig.ChangeToken.RegisterChangeCallback(_ => autoResetEvent.Set(), null); // wait for the config to be updated

			foreach (var process in Process.GetProcessesByName("CargoWise.Winzor.AppServer"))
			{
				process.Kill(true);
			}

			autoResetEvent.WaitOne();

			proxyConfig = proxyConfigProvider.GetConfig();
			Assert.That(proxyConfig.Clusters.Single().Destinations, Is.Empty, "proxy cluster should be empty after process exits");
		}
	}
}
