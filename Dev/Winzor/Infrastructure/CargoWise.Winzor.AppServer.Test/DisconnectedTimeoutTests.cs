using System.Threading.Tasks;
using CargoWise.Blazor.Common;
using CargoWise.Blazor.Testing.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace CargoWise.Blazor.AppServer.Test
{
	using static PlaywrightTestContext;

	public class DisconnectedTimeoutTests
	{
		const string DisconnectedTimeoutEnvVarName = "CargoWiseOptions__DisconnectedShutdownTimeLimit";
		const string CircuitNeverOpenedTimeoutEnvVarName = "CargoWiseOptions__CircuitNeverOpenedShutdownTimeLimit";
		string existingDisconnectedEnvVarValue, existingCircuitNeverOpenedEnvVarValue;

		[SetUp]
		public void Setup()
		{
			existingDisconnectedEnvVarValue = System.Environment.GetEnvironmentVariable(DisconnectedTimeoutEnvVarName);
			existingCircuitNeverOpenedEnvVarValue = System.Environment.GetEnvironmentVariable(CircuitNeverOpenedTimeoutEnvVarName);
			System.Environment.SetEnvironmentVariable(DisconnectedTimeoutEnvVarName, "00:00:01");
			System.Environment.SetEnvironmentVariable(CircuitNeverOpenedTimeoutEnvVarName, "00:01:00"); // this is long but should never actually wait this long because Chrome/Playwright will spin up and make a request, which causes this wait to be cancelled
		}

		[TearDown]
		public void TearDown()
		{
			System.Environment.SetEnvironmentVariable(DisconnectedTimeoutEnvVarName, existingDisconnectedEnvVarValue);
			System.Environment.SetEnvironmentVariable(CircuitNeverOpenedTimeoutEnvVarName, existingCircuitNeverOpenedEnvVarValue);
		}

		[Test]
		public void AppExitsIfCircuitNeverConnected()
		{
			// this one can be quicker because we don't need to allow Chrome/Playwright to spin up
			System.Environment.SetEnvironmentVariable(CircuitNeverOpenedTimeoutEnvVarName, "00:00:01");

			var config = new TestConfiguration();
			var cwOptions = Options.Create(ConfigurationBinder.Get<CargoWiseOptions>(config.GetSection("CargoWiseOptions")) ?? new CargoWiseOptions());
			var header = $"{cwOptions.Value.DbServerName} {cwOptions.Value.DatabaseName}";
			var tokenGenerator = new DebugClientTokenGenerator(cwOptions);
			var process = TestAppServerProcess.FromTestContext(config).Create(header, tokenGenerator.GenerateClientToken());
			Assert.That(process.Process.WaitForExit(60_000), Is.True, "process should exit"); // this timeout is long, but the process should exit quite quickly
		}

		[Test, BlazorWithPlaywrightPage]
		public async Task AppDoesntExitIfCircuitConnected()
		{
			await Page.GotoAsync(BlazorWithPlaywrightPageAttribute.BlazorAddress);
			await Page.WaitForBlazorAppRenderAsync();
			Assert.That(BlazorWithPlaywrightPageAttribute.BlazorProcess.WaitForExit(2_000), Is.False, "Process should not exit");
		}

		[Test, BlazorWithPlaywrightPage]
		public async Task AppExitsIfLastCircuitDisconnects()
		{
			await Page.GotoAsync(BlazorWithPlaywrightPageAttribute.BlazorAddress);
			await Page.WaitForBlazorAppRenderAsync();
			await Page.CloseAsync();
			Assert.That(BlazorWithPlaywrightPageAttribute.BlazorProcess.WaitForExit(5_000), Is.True, "process should exit");
		}

		[Test, BlazorWithPlaywrightPage]
		public async Task DoesntExitIfOneCircuitClosesButThereIsAnotherOne()
		{
			await Page.GotoAsync(BlazorWithPlaywrightPageAttribute.BlazorAddress);
			await Page.WaitForBlazorAppRenderAsync();
			var secondCircuit = await BrowserContext.NewPageAsync();
			await secondCircuit.GotoAsync(BlazorWithPlaywrightPageAttribute.BlazorAddress);
			await secondCircuit.WaitForBlazorAppRenderAsync();
			await secondCircuit.CloseAsync();
			Assert.That(BlazorWithPlaywrightPageAttribute.BlazorProcess.WaitForExit(2_000), Is.False, "process should not exit");
		}
	}
}
