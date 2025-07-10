using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;

namespace ReferenceDataUpdateService.Web.Test.WebUI
{
	[TestFixture]
	public class SampleUITest : PlaywrightTest
	{
		[Test]
		public async Task ShouldShowTheSignInButton()
		{
			await Assertions.Expect(Page.GetByText("Sign In")).ToBeVisibleAsync();
		}
	}
}
