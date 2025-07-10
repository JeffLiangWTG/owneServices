using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace CargoWise.Blazor.AppServer.Test
{
	using static PlaywrightTestContext;

	[TestFixture, BlazorWithPlaywrightPage]
	public class PlaywrightStartupTests
	{
		[Test]
		public async Task BlazorAppCanBeConnected()
		{
			var requestUri = BlazorWithPlaywrightPageAttribute.BlazorAddress;

			await Page.GotoAsync(requestUri);
			await Page.WaitForBlazorAppRenderAsync();
		}
	}
}
