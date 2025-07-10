using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using CargoWise.Blazor.Common;
using CargoWise.Blazor.Common.Testing.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace CargoWise.Blazor.SessionBroker.Test
{
	[KillBlazorAppProcesses]
	public class AppInstanceServiceTests
	{
		[Test]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "Testing")]
		public void ItLaunchesAProcessSuccessfully()
		{
			Console.WriteLine(AppDomain.CurrentDomain.BaseDirectory);
			using var factory = new CustomWebApplicationFactory<Startup>();
			var cargoWiseOptions = factory.Services.GetRequiredService<IOptions<CargoWiseOptions>>();
			var token = new DebugClientTokenGenerator(cargoWiseOptions).GenerateClientToken();
			var appInstanceService = factory.Services.GetRequiredService<AppInstanceService>();
			var id = appInstanceService.CreateWithStmAccessToken(null, token, null);
			Assert.That(id, Is.Not.Null);
			Assert.That(Guid.TryParse(id, out var g), Is.True);
			Assert.That(g, Is.Not.EqualTo(Guid.Empty));
		}

		[Test]
		[CancelAfter(60000)]
		public async Task BackChannelConnectionEstablishedWhenLaunchingWithUriAsScope()
		{
			string backchannelUrl = $"http://localhost:5548/";
			using var listener = new HttpListener();
			listener.Prefixes.Add(backchannelUrl);
			listener.Start();
			var developerUserId = Guid.Parse("ABE1D8D8-A709-4BFA-88E3-53997AA925E2");

			using var factory = new CustomWebApplicationFactory<Startup>();
			var cargoWiseOptions = factory.Services.GetRequiredService<IOptions<CargoWiseOptions>>();
			var clientToken = new DebugClientTokenGenerator(cargoWiseOptions).GenerateClientToken(parentId: developerUserId, scope: backchannelUrl);
			Assert.That(clientToken, Is.Not.Empty);
			var appInstanceService = factory.Services.GetRequiredService<AppInstanceService>();

			var id = appInstanceService.CreateWithStmAccessToken(null, clientToken, null);

			var context = await listener.GetContextAsync();
			Assert.That(id, Is.Not.Null);
			Assert.That(context.Request.Url.PathAndQuery, Contains.Substring("signalr/negotiate"));
		}

		[Test]
		public void SharedSecretIsAddedToSessionStoreAfterSuccessfulLaunch()
		{
			using var factory = new CustomWebApplicationFactory<Startup>();
			var cargoWiseOptions = factory.Services.GetRequiredService<IOptions<CargoWiseOptions>>();
			var token = new DebugClientTokenGenerator(cargoWiseOptions).GenerateClientToken();
			var appInstanceService = factory.Services.GetRequiredService<AppInstanceService>();
			var sessionSecretStore = factory.Services.GetRequiredService<SessionSecretStore>();
			Assert.That(sessionSecretStore.Count, Is.EqualTo(0));

			var id = appInstanceService.CreateWithStmAccessToken(null, token, null);

			Assert.That(id, Is.Not.Empty);
			Assert.That(sessionSecretStore[id], Is.Not.Null);
			Assert.That(sessionSecretStore[id].Length, Is.EqualTo(88));
		}

		[Test]
		public void SharedSecretIsRemovedFromSessionStoreAfterProcessExit()
		{
			using var originalFactory = new CustomWebApplicationFactory<Startup>();
			using var factory = originalFactory.WithWebHostBuilder(builder =>
				{
					builder.ConfigureServices(services =>
					{
						services.PostConfigure<CargoWiseOptions>(options =>
						{
							options.AppServerPathOverride = Path.Combine("CargoWise.Blazor.SessionBroker.Test.MockAppServer-bin", "CargoWise.Blazor.SessionBroker.Test.MockAppServer.exe");
						});
					});
					builder.UseSetting("AdditionalArguments", "--wait 100");
				});
			var appInstanceService = factory.Services.GetRequiredService<AppInstanceService>();
			var sessionSecretStore = factory.Services.GetRequiredService<SessionSecretStore>();

			var id = appInstanceService.CreateWithStmAccessToken(null, null, null);

			Assert.That(id, Is.Not.Empty);
			Assert.That(() => sessionSecretStore.ContainsKey(id), Is.True);
			Assert.That(() => sessionSecretStore.ContainsKey(id), Is.False.After(10000).PollEvery(100));
		}

		[Test]
		public void AfterCreatingASessionABlazorProcessIsRunning()
		{
			Assert.That(GetBlazorProcesses(), Is.Empty);
			using var factory = new CustomWebApplicationFactory<Startup>();
			var cargoWiseOptions = factory.Services.GetRequiredService<IOptions<CargoWiseOptions>>();
			var token = new DebugClientTokenGenerator(cargoWiseOptions).GenerateClientToken();
			var appInstanceService = factory.Services.GetRequiredService<AppInstanceService>();
			var id = appInstanceService.CreateWithStmAccessToken(null, token, null);
			Assert.That(GetBlazorProcesses(), Has.Exactly(1).Items);
		}

		[Test]
		public void UsesISecureSecretGeneratorToCreateSessionToken()
		{
			var mockSecureSecretGenerator = new Mock<ISecureSecretGenerator>();
			mockSecureSecretGenerator.Setup(g => g.Generate()).Returns("sessionToken");

			using var originalFactory = new CustomWebApplicationFactory<Startup>();
			using var factory = originalFactory.WithWebHostBuilder((builder) =>
				{
					builder.ConfigureServices(services => services.AddSingleton(mockSecureSecretGenerator.Object));
				});
			var appInstanceService = factory.Services.GetRequiredService<AppInstanceService>();
			var appServerId = appInstanceService.CreateWithStmAccessToken(string.Empty, string.Empty, null);

			mockSecureSecretGenerator.Verify(g => g.Generate(), Times.Once);
			Assert.That(appServerId, Is.Not.Empty);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1055:DoNotUseProcessGetProcess", Justification = "Testing")]
		Process[] GetBlazorProcesses() => Process.GetProcessesByName(Path.GetFileNameWithoutExtension(BuildFileSystem.AppServerBin.PublishExecutableFileName));
	}
}
