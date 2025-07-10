using CargoWise.RefDbRepo.ComplianceListReferenceData.CmdLine;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ComplianceListReferenceData.Test
{
	public class AppConfigurationProviderTest
	{
		[Test]
		public void TestAppConfiguration()
		{
			var appConfiguration = AppConfigurationProvider.AppConfiguration;
			Assert.AreEqual(@"..\..\UXmlFiles\", appConfiguration.OutputPath);
			Assert.AreEqual("https://dpsv4.wisegrid.net", appConfiguration.DpsWebServiceBaseUrl);
		}
	}
}
