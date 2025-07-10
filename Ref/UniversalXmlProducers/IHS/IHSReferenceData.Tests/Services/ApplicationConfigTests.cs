using CargoWise.RefDbRepo.IHSReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.IHSReferenceData.Tests.Services
{
	[TestFixture]
	class ApplicationConfigTests
	{
		[Test]
		public void TestOutputDirectory()
		{
			Assert.That(@"..\..\UXmlFiles", Is.EqualTo(ApplicationConfig.OutputPath));
		}

		[Test]
		public void TestVesselListFTPHost()
		{
			Assert.That("mft.ihsmarkit.com", Is.EqualTo(ApplicationConfig.VesselListFTPHost));
		}
	}
}
