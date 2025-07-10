using Enterprise.Customs.ASYCUDA.Business;
using AsycudaBill = Enterprise.Customs.SG.Access.Business.AsycudaBill;
using AsycudaContainer = Enterprise.Customs.SG.Access.Business.AsycudaContainer;
using AsycudaManifestHeader = Enterprise.Customs.SG.Access.Business.AsycudaManifestHeader;
using AsycudaPack = Enterprise.Customs.SG.Access.Business.AsycudaPack;
using AsycudaPackedItem = Enterprise.Customs.SG.Access.Business.AsycudaPackedItem;

namespace Enterprise.Customs.SG.Access.GUI.Testing
{
	class ManifestFormPerformanceTest : ASYCUDA.GUI.Testing.ManifestFormPerformanceTest<AsycudaManifestHeader, AsycudaContainer, AsycudaBill, AsycudaPack, AsycudaPackedItem, ABLEntryNum, AsycudaPackedItemEntryNum>
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Singapore;
		protected override void DecorateBill(TestData testData, AsycudaBill bill, int billLoop)
		{
			base.DecorateBill(testData, bill, billLoop);
			bill.SG_PartyID = bill.CountryCode + "SGID32";
			bill.SG_PartyStatus = billLoop.ToString().Substring(0, 1);
			bill.SG_PayeeIndicator = bill.CountryCode.Left(1);
		}

		protected override void DecoratePackedItem(TestData countryData, AsycudaPackedItem packItem, int billLoop, int packLoop)
		{
			base.DecoratePackedItem(countryData, packItem, billLoop, packLoop);
			packItem.GoodsType = countryData.CountryCode + billLoop.ToString() + "GT" + packLoop;
		}
	}
}
