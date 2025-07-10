using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.US.ACEManifest.Business;
using AsycudaBill = Enterprise.Customs.US.ACEManifest.Business.AsycudaBill;
using AsycudaContainer = Enterprise.Customs.US.ACEManifest.Business.AsycudaContainer;
using AsycudaManifestHeader = Enterprise.Customs.US.ACEManifest.Business.AsycudaManifestHeader;
using AsycudaPack = Enterprise.Customs.US.ACEManifest.Business.AsycudaPack;

namespace Enterprise.Customs.US.ACEManifest.GUI.Testing
{
	class ManifestFormPerformanceTest : ASYCUDA.GUI.Testing.ManifestFormPerformanceTest<AsycudaManifestHeader, AsycudaContainer, AsycudaBill, AsycudaPack, AsycudaPackedItem, ABLEntryNum, AsycudaPackedItemEntryNum>
	{
		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;

		protected override void DecorateManifestHeader(TestData testData, AsycudaManifestHeader header)
		{
			base.DecorateManifestHeader(testData, header);
			header.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.Consolidator;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
		}

		protected override void DecorateBill(TestData testData, AsycudaBill bill, int billLoop)
		{
			base.DecorateBill(testData, bill, billLoop);
			bill.FDAIndicator = ZBool.True;
		}
	}
}
