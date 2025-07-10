using System.Threading.Tasks;
using AppServer.Pages;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.Blazor.Common;
using CargoWise.Blazor.HostedAppRuntime.Backchannel;
using Enterprise.Winzor.Architecture.Test;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using TestContext = Bunit.TestContext;

namespace CargoWise.Winzor.AppServer.Test.Pages
{
	using static PlaywrightTestContext;

	[TestFixture]
	public class BackChannelTests
	{
		ILifecycleService lifecycleService;
		IBackchannelProvider backchannelProvider;
		IWindowService windowService;
		TestContext testContext;

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			AssemblyResolver.Setup();
		}

		[SetUp]
		public void Setup()
		{
			testContext = new TestContext();

			lifecycleService = Mock.Of<ILifecycleService>();
			testContext.Services.AddSingleton(lifecycleService);

			backchannelProvider = Mock.Of<IBackchannelProvider>();
			var connectionStartedTask = new TaskCompletionSource();
			Mock.Get(backchannelProvider).SetupGet(p => p.HubConnector.ConnectionStarted).Returns(connectionStartedTask);
			testContext.Services.AddSingleton(backchannelProvider);

			windowService = Mock.Of<IWindowService>();
			testContext.Services.AddSingleton(windowService);
		}

		[TearDown]
		public void TearDown()
		{
			testContext?.Dispose();
		}

		[Test]
		public void BackChannelIsNotNull()
		{
			var backChannel = testContext.RenderComponent<Backchannel>();
			Assert.That(backChannel, Is.Not.Null);
		}

		[WithPlaywrightPage]
		[Test]
		public async Task BackChannelLoadsWithoutErrors()
		{
			await using var ctx = new InMemoryAppServerTestContext(new MockCargoWiseClientSeviceProvider());

			await Page.GotoAsync($"{ctx.ServerBaseUrl}/backchannel").ConfigureAwait(false);

			Assert.That(WithPlaywrightPageAttribute.Context.PageErrors, Is.Empty);

			await Page.CloseAsync();
		}

		[Test]
		[WithPlaywrightPage]
		public async Task BackChannelLoadsClientJS()
		{
			await using var ctx = new InMemoryAppServerTestContext(new MockCargoWiseClientSeviceProvider());

			var task = Page.WaitForRequestAsync("**/*client.js*");

			await Page.GotoAsync($"{ctx.ServerBaseUrl}/backchannel").ConfigureAwait(false);

			var request = await task;

			Assert.That(request.Url, Does.StartWith($"{ctx.ServerBaseUrl}/client.js"));
			Assert.That(request.ResourceType, Is.EqualTo("script"));
			Assert.That(request.Failure, Is.Null);

			await Page.CloseAsync();
		}

		[Test]
		[WithPlaywrightPage]
		public async Task PostClientMessageIsAvailableFromBackChannel()
		{
			await using var ctx = new InMemoryAppServerTestContext(new MockCargoWiseClientSeviceProvider());

			await Page.GotoAsync($"{ctx.ServerBaseUrl}/backchannel").ConfigureAwait(false);

			var script = "window.chrome = {webview: { postMessage: () => {}}}";
			await Page.EvaluateAsync(script);

			var result = await Page.EvaluateAsync<string>("postClientMessage", "{}");
			Assert.That(result, Is.EqualTo("MessageSent"));

			await Page.CloseAsync();
		}
	}
}
