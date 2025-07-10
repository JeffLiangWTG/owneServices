using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.AMS.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(CusInBondBillCollection))]
	sealed class CusInBondBillCollectionTest : ActiveBusinessObjectCollectionTestCase<CusInBondBillCollection>
	{
		public void TestAddNew_IssuerCode_BillNumber()
		{
			var header = Factory.New<CusInBondHeader>();
			var collection = header.Bills;
			var bill = collection.AddNew("ISS1", "B1");
			AssertEquals("ISS1", bill.B0_IssuerCode);
			AssertEquals("B1", bill.B0_MasterBillNumber);
		}

		public void TestOceanBillTypeShouldBeExcluded()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			bill2.B0_ShipmentType = CusInBondBill.OceanBillType;
			var collection = new CusInBondBillCollection(header);
			collection.RefreshFromDb();
			AssertEquals("collection.Count", 2, collection.Count);
			AssertCollectionContains(bill1, collection);
			AssertCollectionContains(bill2, collection);
			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			collection = new CusInBondBillCollection(header);
			collection.RefreshFromDb();
			AssertEquals("collection.Count", 1, collection.Count);
			AssertCollectionContains(bill1, collection);
			AssertCollectionNotContains(bill2, collection);
		}

		public void TestHasNVOCCBill()
		{
			var header = Factory.New<CusInBondHeader>();
			var collection = header.Bills;
			AssertEquals(false, collection.HasNVOCCBill);
			var bill = collection.AddNew();
			bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.RegularBill;
			AssertEquals(false, collection.HasNVOCCBill);
			foreach (var billStatus in new[] { BillOfLadingStatusIndicatorList.Codes.HouseBill, BillOfLadingStatusIndicatorList.Codes.FROB, BillOfLadingStatusIndicatorList.Codes.HouseFROBAndISF, BillOfLadingStatusIndicatorList.Codes.HouseBillAndISFWhereMasterBOLIs62_63InBond })
			{
				bill.B0_BillStatus = billStatus;
				AssertEquals(true, collection.HasNVOCCBill);
			}
		}

		public void TestIndexer_IssuerCode_BillNumber()
		{
			var header = Factory.New<CusInBondHeader>();
			var collection = header.Bills;
			var bill1 = collection.AddNew("ISS1", "B1");
			var bill2 = collection.AddNew("ISS2", "B1");
			var bill3 = collection.AddNew("ISS1", "B2");
			AssertNull(collection["", ""]);
			AssertNull(collection["", "B2"]);
			AssertEquals(bill3, collection["ISS1", "B2"]);
			AssertEquals(bill1, collection["ISS1", "B1"]);
			AssertEquals(bill2, collection["ISS2", "B1"]);
		}

		public void TestDefaultFromHeader()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_CarrierSCAC = "OTT2";
			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			header.OceanBill.B0_IssuerCode = "OTT1";
			var collection = new CusInBondBillCollection(header);
			var bill1 = collection.AddNew();
			AssertEquals(2, bill1.SecondaryNotifyParties.Count);
			Assert(bill1.SecondaryNotifyParties.Any(x => x.CY_Data == "OTT2"));
			Assert(bill1.SecondaryNotifyParties.Any(x => x.CY_Data == "OTT1"));
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			header2.BH_CarrierSCAC = "CARR";
			header2.OceanBill.B0_IssuerCode = ZString.Empty;
			var collection2 = new CusInBondBillCollection(header2);
			var bill3 = header2.Bills.AddNew();
			AssertEquals(bill3.SecondaryNotifyParties.Count, 1);
			AssertEquals(bill3.SecondaryNotifyParties[0].CY_Data, "CARR");
		}

		public void TestDataDefault()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_CarrierSCAC = "OTT2";
			var collection = new CusInBondBillCollection(header);
			var bill1 = collection.AddNew();
			AssertEquals("", bill1.B0_IssuerCode);
			AssertEquals(BillOfLadingStatusIndicatorList.Codes.HouseBill, bill1.B0_BillStatus);
			AssertEquals("", bill1.B0_RL_NKPortOfLading);
			AssertEquals("", bill1.B0_PortOfLadingKCode);
			AssertEquals("", bill1.B0_RL_NKLastForeignPort);
			AssertEquals("", bill1.B0_LastForeignPortKCode);
			AssertEquals("", bill1.B0_RL_NKForeignPortOfContract);
			AssertEquals("", bill1.B0_ForeignPortOfContractKCode);
			AssertEquals("", bill1.B0_RL_NKInBondPortOfDest);
			AssertEquals("", bill1.B0_InBondPortOfDestDCode);
			AssertEquals(ZDate.Empty, bill1.B0_DateOfDischarge);
			AssertEquals(1, bill1.SecondaryNotifyParties.Count);
			var snp1 = bill1.SecondaryNotifyParties[0];
			AssertEquals("OTT2", snp1.CY_Data);
			bill1.B0_IssuerCode = "OTT1";
			bill1.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.RegularConsolidationRail;
			bill1.B0_RL_NKPortOfLading = "AUSYD";
			bill1.B0_PortOfLadingKCode = "60268";
			bill1.B0_RL_NKLastForeignPort = "AUMEL";
			bill1.B0_LastForeignPortKCode = "60238";
			bill1.B0_RL_NKForeignPortOfContract = "AUBNE";
			bill1.B0_ForeignPortOfContractKCode = "60210";
			bill1.B0_RL_NKInBondPortOfDest = "AUSYD";
			bill1.B0_InBondPortOfDestDCode = "1111";
			bill1.B0_DateOfDischarge = new ZDate(2013, 3, 11);
			snp1.CY_Data = "OTT3";
			var snp2 = bill1.SecondaryNotifyParties.AddNewIfNotExist("OTT4");
			var bill2 = collection.AddNew();
			AssertEquals("OTT1", bill2.B0_IssuerCode);
			AssertEquals(BillOfLadingStatusIndicatorList.Codes.RegularConsolidationRail, bill2.B0_BillStatus);
			AssertEquals("AUSYD", bill2.B0_RL_NKPortOfLading);
			AssertEquals("60268", bill2.B0_PortOfLadingKCode);
			AssertEquals("AUMEL", bill2.B0_RL_NKLastForeignPort);
			AssertEquals("60238", bill2.B0_LastForeignPortKCode);
			AssertEquals("AUBNE", bill2.B0_RL_NKForeignPortOfContract);
			AssertEquals("60210", bill2.B0_ForeignPortOfContractKCode);
			AssertEquals("AUSYD", bill2.B0_RL_NKInBondPortOfDest);
			AssertEquals("1111", bill2.B0_InBondPortOfDestDCode);
			AssertEquals(new ZDate(2013, 3, 11), bill2.B0_DateOfDischarge);
			AssertEquals(2, bill2.SecondaryNotifyParties.Count);
			var snp3 = bill2.SecondaryNotifyParties.FirstOrDefault(x => x.CY_Data == "OTT3");
			AssertNotNull("OTT3", snp3);
			var snp4 = bill2.SecondaryNotifyParties.FirstOrDefault(x => x.CY_Data == "OTT4");
			AssertNotNull("OTT4", snp4);
			bill2.B0_IssuerCode = "OTT2";
			bill2.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.Sec321SimpleRegularBill;
			bill2.B0_RL_NKPortOfLading = "AUMEL";
			bill2.B0_PortOfLadingKCode = "60238";
			bill2.B0_RL_NKLastForeignPort = "AUBNE";
			bill2.B0_LastForeignPortKCode = "60210";
			bill2.B0_RL_NKForeignPortOfContract = "AUSYD";
			bill2.B0_ForeignPortOfContractKCode = "60268";
			bill2.B0_RL_NKInBondPortOfDest = "AUMEL";
			bill2.B0_InBondPortOfDestDCode = "2222";
			bill2.B0_DateOfDischarge = new ZDate(2013, 3, 12);
			snp3.CY_Data = "OTT5";
			snp4.CY_Data = "OTT6";
			var bill3 = collection.AddNew();
			AssertEquals("OTT2", bill3.B0_IssuerCode);
			AssertEquals(BillOfLadingStatusIndicatorList.Codes.Sec321SimpleRegularBill, bill3.B0_BillStatus);
			AssertEquals("AUMEL", bill3.B0_RL_NKPortOfLading);
			AssertEquals("60238", bill3.B0_PortOfLadingKCode);
			AssertEquals("AUBNE", bill3.B0_RL_NKLastForeignPort);
			AssertEquals("60210", bill3.B0_LastForeignPortKCode);
			AssertEquals("AUSYD", bill3.B0_RL_NKForeignPortOfContract);
			AssertEquals("60268", bill3.B0_ForeignPortOfContractKCode);
			AssertEquals("AUMEL", bill3.B0_RL_NKInBondPortOfDest);
			AssertEquals("2222", bill3.B0_InBondPortOfDestDCode);
			AssertEquals(new ZDate(2013, 3, 12), bill3.B0_DateOfDischarge);
			AssertEquals(2, bill3.SecondaryNotifyParties.Count);
			AssertNotNull("OTT5", bill3.SecondaryNotifyParties.FirstOrDefault(x => x.CY_Data == "OTT5"));
			AssertNotNull("OTT6", bill3.SecondaryNotifyParties.FirstOrDefault(x => x.CY_Data == "OTT6"));
		}

		protected override CusInBondBillCollection GetCollectionToTest()
		{
			var header = Factory.New<CusInBondHeader>();
			return new CusInBondBillCollection(header);
		}
	}
}
