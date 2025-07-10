using CargoWise.RefDbRepo.ComplianceAlertListReferenceData.CmdLine;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ComplianceAlertListReferenceData.Test
{
	public class AppConfigurationProviderTest
	{
		[Test]
		public void TestAppConfiguration()
		{
			var appConfiguration = AppConfigurationProvider.AppConfiguration;
			Assert.AreEqual(@"..\..\UXmlFiles\", appConfiguration.OutputPath);
			Assert.AreEqual("https://app.borderwise.com", appConfiguration.BWBaseUrl);
		}
	}
}
