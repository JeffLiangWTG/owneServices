using eServices.Dms.Core.OpsPortal.Components.Pages;

namespace eServices.Dms.Core.OpsPortal.Tests;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class HomeTests : BunitContext
{
	[Test]
	public void HomeExists()
	{
		var cut = Render<Home>();

		cut.FindAll("h1").MarkupMatches("<h1></h1><h1>DMS Ops Portal</h1>");
	}

	[Test]
	public void AboutExists()
	{
		var cut = Render<About>();

		cut.Find("h1").MarkupMatches("<h1>About</h1>");
	}
}
