using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class CusInBondCargoDescLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestManifestUnitList()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			var container = moveDetail.Containers.AddNew();
			var commodity = container.Commodities.AddNew();
			AssertEquals(Factory.GetCachedValue<ManifestUnitList>(), commodity.Lookups.ManifestUnitList);

			var list = commodity.Lookups.ManifestUnitList;
			Assert(list.ContainsCode(Enterprise.Customs.US.AMS.Business.ManifestUnitList.Codes.PalletNotUsedInSeaAms));

			header.BH_ImportTransportMode = TransportTypeList.Codes.VesselContainer;
			list = commodity.Lookups.ManifestUnitList;
			Assert(!list.ContainsCode(Enterprise.Customs.US.AMS.Business.ManifestUnitList.Codes.PalletNotUsedInSeaAms));

			header.BH_ImportTransportMode = TransportTypeList.Codes.VesselNonContainer;
			list = commodity.Lookups.ManifestUnitList;
			Assert(!list.ContainsCode(Enterprise.Customs.US.AMS.Business.ManifestUnitList.Codes.PalletNotUsedInSeaAms));

			header.BH_ImportTransportMode = TransportTypeList.Codes.Rail;
			list = commodity.Lookups.ManifestUnitList;
			Assert(list.ContainsCode(Enterprise.Customs.US.AMS.Business.ManifestUnitList.Codes.PalletNotUsedInSeaAms));
		}

		public void TestTariffs()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			var container = moveDetail.Containers.AddNew();
			var commodity = container.Commodities.AddNew();
			AssertEquals(typeof(USCTariffCollection), commodity.Lookups.Tariffs.GetType());
		}

		public void TestWeightUnitList()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			var container = moveDetail.Containers.AddNew();
			var commodity = container.Commodities.AddNew();
			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList(OLookUpEditType.Weight), commodity.Lookups.WeightUnitList);
		}
	}
}
