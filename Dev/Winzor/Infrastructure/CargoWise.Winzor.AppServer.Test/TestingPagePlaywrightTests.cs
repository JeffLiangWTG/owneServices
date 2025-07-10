using System.Threading.Tasks;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace CargoWise.Blazor.AppServer.Test
{
	using static PlaywrightTestContext;

	[TestFixture, BlazorWithPlaywrightPage(UseTestClientIntegration = true)]
	public class TestingPagePlaywrightTests
	{
		public static string BlazorAddress => BlazorWithPlaywrightPageAttribute.BlazorAddress;

		[Test]
		public async Task OnAfterRenderAsync_PageLoaded_ShouldRequestNewWindow()
		{
			var requestUri = BlazorAddress + "/testing";

			await Page.GotoAsync(requestUri);

			await Page.WaitForBlazorAppRenderAsync();

			await Page.WaitForClientApplicationMessagePost("LoadMessage, CargoWise.Blazor.Client.Integration");
		}
	}
}
