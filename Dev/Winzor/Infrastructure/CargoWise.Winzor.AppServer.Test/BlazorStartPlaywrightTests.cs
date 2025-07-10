using System.Threading.Tasks;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace CargoWise.Blazor.AppServer.Test
{
	using static PlaywrightTestContext;

	[TestFixture, BlazorWithPlaywrightPage(UseTestClientIntegration = true)]
	public class BlazorStartPlaywrightTests
	{
		[Test]
		public async Task OnStartPostsRegisterReady()
		{
			await Page.AddInitScriptAsync("window.cargoWiseClient = {setApplicationReady: () => cargoWiseTesting.notified = true}");
			await Page.GotoAsync(BlazorWithPlaywrightPageAttribute.BlazorAddress + "/testing");
			await Page.WaitForBlazorAppRenderAsync();

			await Page.WaitForFunctionAsync("cargoWiseTesting.notified");
		}

		[Test]
		public async Task OnConnectionDownCallsNotifyApplicationFailure()
		{
			await Page.AddInitScriptAsync("window.cargoWiseClient = {notifyApplicationFailure: () => cargoWiseTesting.notified = true}");
			await Page.GotoAsync(BlazorWithPlaywrightPageAttribute.BlazorAddress + "/testing");
			await Page.WaitForBlazorAppRenderAsync();

			await Page.EvaluateAsync("window.Blazor._internal.forceCloseConnection()");

			await Page.WaitForFunctionAsync("cargoWiseTesting.notified");
		}
	}
}
