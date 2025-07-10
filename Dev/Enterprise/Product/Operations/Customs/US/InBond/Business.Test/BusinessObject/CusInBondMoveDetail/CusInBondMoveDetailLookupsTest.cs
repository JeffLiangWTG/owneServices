using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class CusInBondMoveDetailLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryTypeList()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = moveHeader.MovementDetails.AddNew();
			InbondCommonTypeList list = moveDetail.Lookups.EntryTypeList;
			AssertEquals(3, list.Count);
			AssertEquals("Immediate Transport", InbondCommonTypeList.Descriptions._1ImmediateTransport, list.GetDescriptionFromCode("61"));
			AssertEquals("Transport and Export", InbondCommonTypeList.Descriptions._2TransportandExport, list.GetDescriptionFromCode("62"));
			AssertEquals("Immediate Export", InbondCommonTypeList.Descriptions._3ImmediateExport, list.GetDescriptionFromCode("63"));
		}

		public void TestPreviousEntryTypeList()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = moveHeader.MovementDetails.AddNew();
			CodeDescriptionPairList previousEntryTypelist = moveDetail.Lookups.PreviousEntryTypeList;
			AssertEquals(5, previousEntryTypelist.Count);
			AssertContains(InbondCommonTypeList.Codes._1ImmediateTransport, previousEntryTypelist.CodesAsString);
			AssertContains(InbondCommonTypeList.Codes._2TransportandExport, previousEntryTypelist.CodesAsString);
			AssertContains(InbondCommonTypeList.Codes._3ImmediateExport, previousEntryTypelist.CodesAsString);
			AssertContains(US.Business.EntryTypeList.Codes.ConsumptionFTZ, previousEntryTypelist.CodesAsString);
			AssertContains(US.Business.EntryTypeList.Codes.ConsumptionFTZ, previousEntryTypelist.CodesAsString);
			AssertContains(US.Business.EntryTypeList.Codes.Warehouse, previousEntryTypelist.CodesAsString);
		}

		public void TestRegionDistrictPorts()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = moveHeader.MovementDetails.AddNew();
			var collection = moveDetail.Lookups.RegionDistrictPorts;
			AssertNotNull(collection);
		}

		public void TestBills()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondBill bill = header.Bills.AddNew();
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = moveHeader.MovementDetails.AddNew();
			IBusinessObjectCollection collection = moveDetail.Lookups.Bills;
			AssertEquals(header.Bills, collection);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(bill, collection);
			moveHeader.BM_BH = ZGuid.Empty;
			collection = moveDetail.Lookups.Bills;
			AssertEquals(typeof(ActiveBusinessObjectCollection<CusInBondBill>), collection.GetType());
			AssertEquals(0, collection.Count);
			moveHeader.BM_BH = header.PK;
			moveDetail.B9_BM = ZGuid.Empty;
			collection = moveDetail.Lookups.Bills;
			AssertEquals(typeof(ActiveBusinessObjectCollection<CusInBondBill>), collection.GetType());
			AssertEquals(0, collection.Count);
		}

		public void TestMoveHeaders()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = moveHeader.MovementDetails.AddNew();
			IBusinessObjectCollection collection = moveDetail.Lookups.MoveHeaders;
			AssertEquals(header.FilteredMovementHeaders, collection);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(moveHeader, collection);
			moveDetail.B9_BM = ZGuid.Empty;
			moveDetail.InBondHeaderPK = ZGuid.Empty;
			collection = moveDetail.Lookups.MoveHeaders;
			AssertEquals(typeof(ActiveBusinessObjectCollection<CusInBondMoveHeader>), collection.GetType());
			AssertEquals(0, collection.Count);
		}

		public void TestMessageStatusList()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = moveHeader.MovementDetails.AddNew();
			AssertEquals(Factory.GetCachedValue<ImportMessageStatusList>(), moveDetail.Lookups.MessageStatusList);
		}

		public void TestCustomsStatusList()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew();
			AssertEquals(Factory.GetCachedValue<ImportMessageStatusList>(), moveDetail.Lookups.CustomsStatusList);
		}

		public void TestMasterBillsAndHouseBills()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = "40";
			var moveHeader = header.MovementHeaders.AddNew();
			var bill1 = header.Bills.AddNew();
			bill1.B0_IssuerCode = "ABCD";
			bill1.B0_MasterBillNumber = "12345";
			bill1.B0_HouseBillNumber = "54321";
			var bill2 = header.Bills.AddNew();
			bill2.B0_IssuerCode = "EFDG";
			bill2.B0_MasterBillNumber = "54321";
			bill2.B0_HouseBillNumber = "12345";
			var moveDetail = moveHeader.MovementDetails.AddNew();
			var list = moveDetail.Lookups.MasterBillsAndHouseBills;
			AssertEquals("There should be 2 billuniquecode", 2, list.Count);
			AssertEquals("BillUniqueCode1", "54321 (ABCD 12345)", list[bill1.PK].Code);
			AssertEquals("BillUniqueCode2", "12345 (EFDG 54321)", list[bill2.PK].Code);
			bill2.B0_MasterBillNumber = "86545";
			list = moveDetail.Lookups.MasterBillsAndHouseBills;
			AssertEquals("There should be 2 billuniquecode", 2, list.Count);
			AssertEquals("BillUniqueCode1", "54321 (ABCD 12345)", list[bill1.PK].Code);
			AssertEquals("BillUniqueCode2", "12345 (EFDG 86545)", list[bill2.PK].Code);
			var bill3 = header.Bills.AddNew();
			bill3.B0_IssuerCode = ZString.Empty;
			bill3.B0_MasterBillNumber = "MB3";
			bill3.B0_HouseBillNumber = "HB3";
			list = moveDetail.Lookups.MasterBillsAndHouseBills;
			AssertEquals("There should be 3", 3, list.Count);
			AssertEquals("BillUniqueCode1", "54321 (ABCD 12345)", list[bill1.PK].Code);
			AssertEquals("BillUniqueCode2", "12345 (EFDG 86545)", list[bill2.PK].Code);
			AssertEquals("BillUniqueCode2", "HB3 (MB3)", list[bill3.PK].Code);
			bill1.Delete();
			list = moveDetail.Lookups.MasterBillsAndHouseBills;
			AssertEquals("There should be 2 billuniquecode", 2, list.Count);
			AssertEquals("BillUniqueCode2", "12345 (EFDG 86545)", list[bill2.PK].Code);
			AssertEquals("BillUniqueCode2", "HB3 (MB3)", list[bill3.PK].Code);
		}
	}
}
