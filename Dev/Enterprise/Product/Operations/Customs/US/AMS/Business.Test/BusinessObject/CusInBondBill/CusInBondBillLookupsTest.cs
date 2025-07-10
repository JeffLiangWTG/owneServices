using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	sealed class CusInBondBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBranches()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var branch1 = company.Branches.AddNew();
			var company2 = Factory.New<GlbCompany>();
			var branch2 = company2.Branches.AddNew();
			var header = Factory.New<CusInBondHeader>();
			var branches = header.Lookups.Branches;
			branches.Load();
			AssertCollectionContains(branch1, branches);
			AssertCollectionContains(branch2, branches);
		}

		public void TestShippers()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var collection = bill.Lookups.Shippers;
			AssertNotNull(collection);
		}

		public void TestConsignees()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var collection = bill.Lookups.Consignees;
			AssertNotNull(collection);
		}

		public void TestOrganisations()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var collection = bill.Lookups.Organisations;
			AssertNotNull(collection);
		}

		public void TestManifestUnitList()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var list = bill.Lookups.ManifestUnitList;
			AssertEquals(Factory.GetCachedValue<ManifestUnitList>(), list);
			Assert(list.ContainsCode(Enterprise.Customs.US.AMS.Business.ManifestUnitList.Codes.PalletNotUsedInSeaAms));
			header.BH_ImportTransportMode = TransportTypeList.Codes.VesselContainer;
			list = bill.Lookups.ManifestUnitList;
			Assert(!list.ContainsCode(Enterprise.Customs.US.AMS.Business.ManifestUnitList.Codes.PalletNotUsedInSeaAms));
			header.BH_ImportTransportMode = TransportTypeList.Codes.VesselNonContainer;
			list = bill.Lookups.ManifestUnitList;
			Assert(!list.ContainsCode(Enterprise.Customs.US.AMS.Business.ManifestUnitList.Codes.PalletNotUsedInSeaAms));
			header.BH_ImportTransportMode = TransportTypeList.Codes.Rail;
			list = bill.Lookups.ManifestUnitList;
			Assert(list.ContainsCode(Enterprise.Customs.US.AMS.Business.ManifestUnitList.Codes.PalletNotUsedInSeaAms));
		}

		public void TestStatusIndicatorListForNVOCC()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			AssertEquals(true, header.IsNVOCCHeader);
			var bill = header.Bills.AddNew();
			AssertEquals(false, bill.IsOceanBillType);
			var list = bill.Lookups.StatusIndicatorList;
			AssertEquals(true, list.ContainsCode(BillOfLadingStatusIndicatorList.Codes.RegularBill));
			AssertEquals(false, list.ContainsCode(BillOfLadingStatusIndicatorList.Codes.MasterBill));
			AssertEquals(false, list.ContainsCode(BillOfLadingStatusIndicatorList.Codes.MasterFROB));
			bill.B0_ShipmentType = CusInBondBill.OceanBillType;
			AssertEquals(true, bill.IsOceanBillType);
			list = bill.Lookups.StatusIndicatorList;
			AssertEquals(false, list.ContainsCode(BillOfLadingStatusIndicatorList.Codes.RegularBill));
			AssertEquals(true, list.ContainsCode(BillOfLadingStatusIndicatorList.Codes.MasterBill));
			AssertEquals(true, list.ContainsCode(BillOfLadingStatusIndicatorList.Codes.MasterFROB));
		}

		public void TestStatusIndicatorList()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var list = bill.Lookups.StatusIndicatorList;
			AssertEquals(BillOfLadingStatusIndicatorList.GetCachedValue(Factory, false, false, false), list);
			AssertEquals(false, list.ContainsCode(BillOfLadingStatusIndicatorList.Codes.FROBCargoLadenInForeignE));
		}

		public void TestTransportModeList()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals(Factory.GetCachedValue<TransportTypeList>(), bill.Lookups.TransportModeList);
			AssertEquals("It should be cached", Factory.GetCachedValue<TransportTypeList>(), bill.Lookups.TransportModeList);
		}

		public void TestPaymentMethodCodes()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals(Factory.GetCachedValue<PaymentMethodCodeList>(), bill.Lookups.PaymentMethodCodes);
			AssertEquals("It should be cached", Factory.GetCachedValue<PaymentMethodCodeList>(), bill.Lookups.PaymentMethodCodes);
		}

		public void TestWeightUnitList()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList(OLookUpEditType.Weight), bill.Lookups.WeightUnitList);
		}

		public void TestVolumeUnitList()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList(OLookUpEditType.Volume), bill.Lookups.VolumeUnitList);
		}
	}
}
