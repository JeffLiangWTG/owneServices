using CargoWise.RefDbRepo.LLIReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.LLIReferenceData.Tests.Services;

[TestFixture]
class ApplicationConfigTests
{
	[Test]
	public void TestApiURLs()
	{
		Assert.Multiple(() =>
		{
			Assert.That(ApplicationConfig.VesselApiMainURL, Is.EqualTo("https://api.lloydslistintelligence.com/v1/"));
			Assert.That(ApplicationConfig.VesselListUriPath, Is.EqualTo("vessellist_v3"));
			Assert.That(ApplicationConfig.VesselBasicCharacteristicsUriPath, Is.EqualTo("vesselbasiccharacteristics"));
			Assert.That(ApplicationConfig.VesselAdvancedCharacteristicsUriPath, Is.EqualTo("vesseladvancedcharacteristics_v3"));
		});
	}
}
