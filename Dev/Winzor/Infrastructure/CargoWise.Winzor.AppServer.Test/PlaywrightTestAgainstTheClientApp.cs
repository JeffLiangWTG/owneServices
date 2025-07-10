using CargoWise.Blazor.SessionBroker.Test;
using CargoWise.Blazor.Testing.Common;
using NUnit.Framework;

namespace CargoWise.Winzor.AppServer.Test
{
	[TestFixture]
	[KillBlazorAppProcesses]
	public class PlaywrightTestAgainstTheClientApp
	{
		[Test, Property("DAT:CapabilityRequirements", "ADMIN")]
		[WithClientAppPlaywright]
		public void TestIndexUrl()
		{
			var page = WithClientAppPlaywrightAttribute.Browser.Contexts[0].Pages[0];
			Assert.That(page.Url, Is.EqualTo("https://localhost:5000/"));
		}
	}
}
