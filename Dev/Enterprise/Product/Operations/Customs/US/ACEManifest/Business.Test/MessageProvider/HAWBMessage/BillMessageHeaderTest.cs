using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.AIM.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class BillMessageHeaderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new BillMessageHeader(null, "FSI"));
		}

		public void TestMessageType()
		{
			AssertEquals(Constants.AIMMessageSubTypes.FSI, billMessageHeader.MessageType);
		}

		public void TestReference()
		{
			AssertEquals("BIL01", billMessageHeader.Reference);
		}

		public void TestAirWaybill()
		{
			manifestHeader.AMA_MasterBill = "SHA001";
			AssertType<AIMAirWaybill>(billMessageHeader.AirWaybill);
			AssertEquals("AirWaybillPrefix", "SHA", billMessageHeader.AirWaybill.AirWaybillPrefix);
			AssertEquals("AWBSerialNumber", "001", billMessageHeader.AirWaybill.AWBSerialNumber);
		}

		public void TestWaybill()
		{
			bill.ABL_GoodsDescription = "Test";
			AssertType<AIMWaybill>(billMessageHeader.Waybill);
			AssertEquals("CargoDescription", "Test", billMessageHeader.Waybill.CargoDescription);
		}

		public void TestShipper()
		{
			AssertEquals(null, billMessageHeader.Shipper);
			bill.ShipperOrgPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			billMessageHeader = new BillMessageHeader(bill, Constants.AIMMessageSubTypes.FSI);
			AssertType<AIMShipper>(billMessageHeader.Shipper);
			AssertNotNull(billMessageHeader.Shipper);
		}

		public void TestConsignee()
		{
			AssertEquals(null, billMessageHeader.Consignee);
			bill.ConsigneeOrgPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			billMessageHeader = new BillMessageHeader(bill, Constants.AIMMessageSubTypes.FSI);
			AssertType<AIMConsignee>(billMessageHeader.Consignee);
			AssertNotNull(billMessageHeader.Consignee);
		}

		public void TestCBPEntryDetail()
		{
			bill.CustomsEntryNumberType = ACEManifestBillEntryNumberTypes.Codes.Informal;
			bill.CustomsEntryNumber = "12345678901";
			var cbpEntryDetail = billMessageHeader.CBPEntryDetail;
			AssertEquals(ACEManifestBillEntryNumberTypes.Codes.Informal, cbpEntryDetail.EntryType);
			AssertEquals("12345678901", cbpEntryDetail.EntryNumber);

			bill.CustomsEntryNumber = "";
			cbpEntryDetail = new BillMessageHeader(bill, Constants.AIMMessageSubTypes.FSI).CBPEntryDetail;
			AssertEquals(ACEManifestBillEntryNumberTypes.Codes.Informal, cbpEntryDetail.EntryType);
			AssertEquals("", cbpEntryDetail.EntryNumber);

			bill.CustomsEntryNumberType = "";
			AssertNull(new BillMessageHeader(bill, Constants.AIMMessageSubTypes.FSI).CBPEntryDetail);
		}

		public void TestCPBShipmentDescription()
		{
			AssertType<AIMCBPShipmentDescription>(billMessageHeader.CPBShipmentDescription);
		}

		public void TestFDAFreightIndicator()
		{
			AssertType<AIMFDAFreightIndicator>(billMessageHeader.FDAFreightIndicator);
		}

		public void TestReasonForAmendment()
		{
			AssertType<AIMReasonForAmendment>(billMessageHeader.ReasonForAmendment);
		}

		public void TestSetAmendmentReason()
		{
			billMessageHeader.SetAmendmentReason("01", "Desc");
			AssertEquals("01", billMessageHeader.ReasonForAmendment.AmendmentCode);
			AssertEquals("Desc", billMessageHeader.ReasonForAmendment.AmendmentExplanation);
		}

		public void TestNullProperty()
		{
			AssertNull(billMessageHeader.Transfer);
		}

		protected override void SetUp()
		{
			manifestHeader = Factory.New<AsycudaManifestHeader>();
			bill = manifestHeader.Bills.AddNew();
			bill.ABL_BillNumber = "BIL01";
			bill.ABL_GoodsValue = 2m;
			bill.ABL_RX_NKGoodsValueCurrency = "USD";
			manifestHeader.ArrivalHeaders.AddNew();
			billMessageHeader = new BillMessageHeader(bill, Constants.AIMMessageSubTypes.FSI);
		}

		AsycudaBill bill;
		AsycudaManifestHeader manifestHeader;
		BillMessageHeader billMessageHeader;
	}
}
