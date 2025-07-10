using Microsoft.Extensions.Logging;
using Moq;

namespace NetCore.WebInfrastructure.Test;

public class WebConfigurationProviderTests
{
	[TestCaseSource(nameof(TestWebAppPathStringFromFullIISAppPath_TestCaseSource))]
	public void TestWebAppPathStringFromFullIISAppPath(string envIisPhyPath, long? siteId, string? virtualAppPath = null)
	{
		var provider = new WebConfigurationProvider(new Mock<ILogger<WebConfigurationProvider>>().Object);
		var webAppPath = provider.GetWebAppPathFromFullIISAppPath(envIisPhyPath);
		if (siteId is null)
		{
			Assert.That(webAppPath, Is.Null);
		}
		else
		{
			Assert.That(webAppPath, Is.Not.Null);
			Assert.That(webAppPath!.SiteId, Is.EqualTo(siteId.Value));
			Assert.That(webAppPath.VirtualAppPath, Is.EqualTo(virtualAppPath));
		}
	}

	static IEnumerable<TestCaseData> TestWebAppPathStringFromFullIISAppPath_TestCaseSource()
	{
		yield return new TestCaseData("/LM/W3SVC/1/ROOT/Accounting", 1L, "/Accounting");
		yield return new TestCaseData("/LM/W3SVC/22/ROOT/Rating/Accounting", 22L, "/Rating/Accounting");
		yield return new TestCaseData("/LM/W3SVC/333/ROOT/Accounting.Web", 333L, "/Accounting.Web");
		yield return new TestCaseData("/LM/W3SVC/4444/ROOT/Accounting-12-12", 4444L, "/Accounting");
		yield return new TestCaseData("/LM/W3SVC/Unexpected", null, null);
	}
}
