using CargoWise.RefDbRepo.RefUNLOCORelatedPortReferenceData.CmdLine;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RefUNLOCORelatedPortReferenceData.Test
{
	[TestFixture]
	public class AppConfigurationProviderFixture
	{
		[Test]
		public void TestParsesJsonCorrectly()
		{
			var appConfiguration = AppConfigurationProvider.AppConfiguration;
			Assert.AreEqual("..\\..\\UXmlFiles\\RefUNLOCORelatedPorts.xml", appConfiguration.OutputPath);
			Assert.AreEqual("https://portswebapi.wisecloud.zone/v1/portmappings", appConfiguration.PortMappingsApiUrl);
		}
	}
}
