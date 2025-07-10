using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI.Testing;
using AsycudaBill = Enterprise.Customs.ZA.Manifest.Business.AsycudaBill;
using AsycudaContainer = Enterprise.Customs.ZA.Manifest.Business.AsycudaContainer;
using AsycudaManifestHeader = Enterprise.Customs.ZA.Manifest.Business.AsycudaManifestHeader;
using AsycudaPack = Enterprise.Customs.ZA.Manifest.Business.AsycudaPack;

namespace Enterprise.Customs.ZA.Manifest.GUI.Testing
{
	class ManifestFormPerformanceTest : ManifestFormPerformanceTest<AsycudaManifestHeader, AsycudaContainer, AsycudaBill, AsycudaPack, AsycudaPackedItem, ABLEntryNum, AsycudaPackedItemEntryNum>
	{
		protected override string CountryCode => Core.Constants.CountryCodes.SouthAfrica;
		protected override void DecorateManifestHeader(TestData testData, AsycudaManifestHeader header)
		{
			base.DecorateManifestHeader(testData, header);
			header.CARN = testData.CountryCode + "1";
			header.PlaceOfExit = testData.CountryCode + "1";
			header.AMA_Trailer1RegNo = testData.CountryCode + "T1";
			header.AMA_Trailer2RegNo = testData.CountryCode + "T2";
		}
	}
}
