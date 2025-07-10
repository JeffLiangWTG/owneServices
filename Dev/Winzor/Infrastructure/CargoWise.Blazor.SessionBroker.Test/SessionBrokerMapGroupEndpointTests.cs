using System;
using System.IO;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CargoWise.Blazor.SessionBroker.Helpers;
using CargoWise.Blazor.SessionBroker.StaticFiles;
using CargoWiseNext.Infrastructure.Installations;
using CargoWiseNext.Infrastructure.Installations.Testing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Yarp.ReverseProxy.Model;
using static CargoWise.Blazor.SessionBroker.Test.TestHelpers;

namespace CargoWise.Blazor.SessionBroker.Test
{
	[KillSessionBrokerProcesses]
	public class SessionBrokerMapGroupEndpointTests
	{
		[TestCase("111")]
		[TestCase("aaa")]
		[TestCase("_")]
		[TestCase("/")]
		[TestCase("a/1")]
		[TestCase("_/1")]
		[TestCase("/_sb")]
		[TestCase("/_sb/1")]

		public async Task UndefinedEndpointsUnderSessionBrokerPathReturn404Async(string undefined)
		{
			using var factory = new CustomWebApplicationFactory<Startup>();
			using var client = factory.CreateClient();
			var response = await client.GetAsync($"/_sessionbroker/{undefined}");
			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

			var content = await response.Content.ReadAsStringAsync();
			Assert.That(content, Is.EqualTo($"[SessionBroker] No endpoint is defined for request GET /_sessionbroker/{undefined}"));
		}

		[TestCase("cargowiseclient")]
		[TestCase("cargowiseclientsand")]
		public async Task TestWhenProtocolShouldBeCorrectIfSetupExeFileNotExistAsync(string protocol)
		{
			try
			{
				await MockInstallerFileHelper.CreateMockInstallerFile(InstallerPathsForTest.Instance.ClientMsixPackagePath);
				await MockInstallerFileHelper.CreateMockInstallerFile(InstallerPathsForTest.Instance.ClientAppInstallerPath);
				using var factory = new CustomWebApplicationFactory<Startup>();
				var mockUrlHandlerProvider = new Mock<IUrlHandlerProvider>();
				mockUrlHandlerProvider.Setup(m => m.GetUrlHandler()).Returns(protocol);
				factory.PreBuildActions.Add(host =>
				{
					host.ConfigureServices(services =>
					{
						services.AddSingleton(mockUrlHandlerProvider.Object);
					});
				});
				using var client = factory.CreateClient();
				var response = await client.GetAsync("/_sessionbroker/ClientInfo");

				var jsonOption = new JsonSerializerOptions()
				{
					Converters = { new JsonStringEnumConverter() },
				};
				var clientSetupInfo = await response.Content.ReadFromJsonAsync<WinzorClientSetupInformationForMsix>(options: jsonOption);
				var uri = new Uri(clientSetupInfo.ClientAppLaunchUri);
				Assert.That(uri.Scheme, Is.EqualTo(protocol));
			}
			finally
			{
				try
				{
					File.Delete(InstallerPathsForTest.Instance.ClientMsixPackagePath);
				}
				catch
				{
				}
				try
				{
					File.Delete(InstallerPathsForTest.Instance.ClientAppInstallerPath);
				}
				catch
				{
				}
			}
		}

		[TestCase("CargoWise-Client")]
		[TestCase("CargoWise-Client-SAND")]
		public async Task TestGetClientInfoWhenMsixPackageExistsFilesAsync(string msixPackageIdentityName)
		{
			try
			{
				await MockInstallerFileHelper.CreateMockInstallerFile(InstallerPathsForTest.Instance.ClientMsixPackagePath);
				await MockInstallerFileHelper.CreateMockInstallerFile(InstallerPathsForTest.Instance.ClientAppInstallerPath);

				using var factory = new CustomWebApplicationFactory<Startup>();
				var mockUrlHandlerProvider = new Mock<IUrlHandlerProvider>();
				mockUrlHandlerProvider.Setup(m => m.GetUrlHandler()).Returns(UrlHandlers.CargoWiseClient);

				factory.PreBuildActions.Add(host =>
				{
					host.ConfigureServices(services =>
					{
						services.AddSingleton(mockUrlHandlerProvider.Object);

						var mockAppInstallerHandler = new Mock<IAppInstallerHandler>();
						mockAppInstallerHandler.Setup(h => h.GetAppInstallerInfo()).Returns(new AppInstallerInfo(msixPackageIdentityName, "WiseTech", new Version("0.0.0.0")));
						services.AddSingleton(mockAppInstallerHandler.Object);
					});
				});
				using var client = factory.CreateClient();
				var response = await client.GetAsync("/_sessionbroker/ClientInfo");
				Assert.That(response.IsSuccessStatusCode, Is.True);

				var jsonOption = new JsonSerializerOptions()
				{
					Converters = { new JsonStringEnumConverter() },
				};
				var clientSetupInfo = await response.Content.ReadFromJsonAsync<WinzorClientSetupInformationForMsix>(options: jsonOption);

				Assert.That(clientSetupInfo, Is.Not.Null);
				Assert.That(clientSetupInfo.RequiredVersion, Is.EqualTo("0.0.0.0"));
				Assert.That(clientSetupInfo.Error, Is.Null);
				Assert.That(clientSetupInfo.MsixPackageName, Is.EqualTo(msixPackageIdentityName));
				Assert.That(clientSetupInfo.MsixPackageUrl, Contains.Substring(Arguments.ClientMsixPackageDownloadUrl));
				Assert.That(clientSetupInfo.MsixAppInstallerUrl, Contains.Substring(Arguments.ClientAppInstallerDownloadUrl));
				Assert.That(clientSetupInfo.ClientAppLaunchUri, Contains.Substring("cargowiseclient:http://localhost/"));
				Assert.That(clientSetupInfo.InstallType, Is.EqualTo(WinzorClientInstallType.MSIX));
			}
			finally
			{
				try
				{
					File.Delete(InstallerPathsForTest.Instance.ClientMsixPackagePath);
				}
				catch
				{
				}
				try
				{
					File.Delete(InstallerPathsForTest.Instance.ClientAppInstallerPath);
				}
				catch
				{
				}
			}
		}

		[Test]
		public async Task TestGetClientInfoIsNotCacheable()
		{
			using var factory = new CustomWebApplicationFactory<Startup>();
			using var client = factory.CreateClient();
			var response = await client.GetAsync("/_sessionbroker/ClientInfo");

			Assert.That(response.IsSuccessStatusCode, Is.True);
			Assert.That(response.Headers.CacheControl!.NoCache, Is.True);
		}

		[Test]
		public async Task TestHealthCheckEndpointWithoutCookie()
		{
			using var factory = new CustomWebApplicationFactory<Startup>();
			using var client = factory.CreateClient();
			var response = await client.GetAsync("/_sessionbroker/health");

			Assert.That(response.IsSuccessStatusCode, Is.True);
			var healthCheck = await response.Content.ReadFromJsonAsync<SessionBrokerMapGroupEndpointExtensions.HealthCheckResponse>();
			Assert.That(healthCheck.AppServerCookieIsValid, Is.False);
			Assert.That(healthCheck.CargoWiseVersion, Contains.Substring("App="));
			Assert.That(healthCheck.CargoWiseVersion, Contains.Substring("DB="));
		}

		[Test]
		public async Task TestHealthCheckEndpointWithCookie()
		{
			using var factory = new CustomWebApplicationFactory<Startup>();
			var (client, _) = await CreateTestNodeInCluster(factory);
			var response = await client.GetAsync("/_sessionbroker/health");

			Assert.That(response.IsSuccessStatusCode, Is.True);
			var healthCheck = await response.Content.ReadFromJsonAsync<SessionBrokerMapGroupEndpointExtensions.HealthCheckResponse>();
			Assert.That(healthCheck.AppServerCookieIsValid, Is.True);
			Assert.That(healthCheck.CargoWiseVersion, Contains.Substring("App="));
			Assert.That(healthCheck.CargoWiseVersion, Contains.Substring("DB="));

			client.Dispose();
		}

		[Test]
		public async Task TestHealthCheckEndpointWithCookieButDestinationCrashed()
		{
			using var factory = new CustomWebApplicationFactory<Startup>();
			var (client, _) = await CreateTestNodeInCluster(factory);
			UpdatedestinationStates(factory, DestinationHealth.Unhealthy);

			var response = await client.GetAsync("/_sessionbroker/health");

			Assert.That(response.IsSuccessStatusCode, Is.True);
			var healthCheck = await response.Content.ReadFromJsonAsync<SessionBrokerMapGroupEndpointExtensions.HealthCheckResponse>();
			Assert.That(healthCheck.AppServerCookieIsValid, Is.False);
			Assert.That(healthCheck.CargoWiseVersion, Contains.Substring("App="));
			Assert.That(healthCheck.CargoWiseVersion, Contains.Substring("DB="));

			client.Dispose();
		}
	}

	class WinzorClientSetupInformation
	{
		public string RequiredVersion { get; set; }

		public string SetupExeDownloadUrl { get; set; }

		public string SetupExeCommandLineArguments { get; set; }

		public string Error { get; set; }

		public string MsixPackageName { get; set; }
		public string MsixAppInstallerUrl { get; set; }
		public string MsixPackageUrl { get; set; }

		public string ClientAppLaunchUri { get; set; }

		public WinzorClientInstallType InstallType { get; set; }
	}

	class WinzorClientSetupInformationForMsix
	{
		public string RequiredVersion { get; set; }
		public string Error { get; set; }
		public string MsixPackageName { get; set; }
		public string MsixAppInstallerUrl { get; set; }
		public string MsixPackageUrl { get; set; }
		public string ClientAppLaunchUri { get; set; }
		public WinzorClientInstallType InstallType { get; set; }
	}
}
