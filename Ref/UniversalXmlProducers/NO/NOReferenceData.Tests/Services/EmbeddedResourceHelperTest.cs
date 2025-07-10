using CargoWise.RefDbRepo.NOReferenceData.Services.ErrorCodes;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NOReferenceData.Services.Tests
{
	sealed class EmbeddedResourceHelperTest
	{
		[Test]
		public void TestReadManifestResourceContent()
		{
			var resourceData = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Services.Testfiles.Input.valutakurs_notValid.xml");
			Assert.IsNotNull(resourceData);
		}

		[Test]
		public void TestReadDeserializedManifestResourceContent()
		{
			var resourceData = XmlHelper.ReadDeserializedManifestResourceContent<FeilmeldingListe>("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.ReferenceCodes.Testfiles.Input.feilmelding.xml");
			Assert.IsNotNull(resourceData);
		}
	}
}
