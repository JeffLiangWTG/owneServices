using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.AIM.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class ManifestMessageHeaderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new BillMessageHeader(null, "FSI"));
		}

		public void TestMessageType()
		{
			AssertEquals(Constants.AIMMessageSubTypes.FSI, manifestMessageHeader.MessageType);
		}

		public void TestReference()
		{
			AssertEquals("AMA001", manifestMessageHeader.Reference);
		}

		public void TestCargoControlLine()
		{
			manifestHeader.AMA_CarrierCode = "AC01";
			AssertType<AIMCargoControlLocation>(manifestMessageHeader.CargoControlLine);
			AssertEquals("AC0", manifestMessageHeader.CargoControlLine.CargoTerminalOperator);
		}

		public void TestAirWaybill()
		{
			manifestHeader.AMA_MasterBill = "SHA001";
			AssertType<AIMAirWaybillForManifestMsg>(manifestMessageHeader.AirWaybill);
			AssertEquals("AirWaybillPrefix", "SHA", manifestMessageHeader.AirWaybill.AirWaybillPrefix);
			AssertEquals("AWBSerialNumber", "001", manifestMessageHeader.AirWaybill.AWBSerialNumber);
		}

		public void TestWaybill()
		{
			additionalMessageInformation.AM_IsConsolidation = false;
			bill.ABL_GoodsDescription = "Test";
			AssertType<AIMWaybillForManifestMsg>(manifestMessageHeader.Waybill);
			AssertEquals("CargoDescription", "Test", manifestMessageHeader.Waybill.CargoDescription);
		}

		public void TestArrival()
		{
			additionalMessageInformation.AM_FlightReference = "FL01";
			AssertType<AIMArrivalForManifestMsg>(manifestMessageHeader.Arrival);
			AssertEquals("PartArrivalReference", "FL01", manifestMessageHeader.Arrival.PartArrivalReference);
		}

		public void TestAgent()
		{
			additionalMessageInformation.AM_Agent = "AA";
			AssertType<AIMAgent>(manifestMessageHeader.Agent);
			AssertEquals("AirAMSParticipantCode", "AA", manifestMessageHeader.Agent.AirAMSParticipantCode);
		}

		public void TestShipper()
		{
			AssertEquals(null, manifestMessageHeader.Shipper);
			bill.ShipperOrgPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			manifestMessageHeader = new ManifestMessageHeader(manifestHeader, bill, Constants.AIMMessageSubTypes.FSI, additionalMessageInformation);
			AssertType<AIMShipper>(manifestMessageHeader.Shipper);
			AssertNotNull(manifestMessageHeader.Shipper);
		}

		public void TestConsignee()
		{
			AssertEquals(null, manifestMessageHeader.Consignee);
			bill.ConsigneeOrgPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			manifestMessageHeader = new ManifestMessageHeader(manifestHeader, bill, Constants.AIMMessageSubTypes.FSI, additionalMessageInformation);
			AssertType<AIMConsignee>(manifestMessageHeader.Consignee);
			AssertNotNull(manifestMessageHeader.Consignee);
		}

		public void TestFDAFreightIndicator()
		{
			AssertType<AIMFDAFreightIndicator>(manifestMessageHeader.FDAFreightIndicator);
		}

		public void TestReasonForAmendment()
		{
			AssertType<AIMReasonForAmendment>(manifestMessageHeader.ReasonForAmendment);
		}

		public void TestSetAmendmentReason()
		{
			manifestMessageHeader.SetAmendmentReason("01", "Desc");
			AssertEquals("01", manifestMessageHeader.ReasonForAmendment.AmendmentCode);
			AssertEquals("Desc", manifestMessageHeader.ReasonForAmendment.AmendmentExplanation);
		}

		public void TestNullProperty()
		{
			AssertNull(manifestMessageHeader.CBPEntryDetail);
			AssertNull(manifestMessageHeader.Transfer);
		}

		protected override void SetUp()
		{
			manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_MasterBill = "AMA001";
			additionalMessageInformation = new AdditionalMessageInformation(manifestHeader);
			bill = manifestHeader.Bills.AddNew();
			bill.ABL_BillNumber = "BIL01";
			manifestHeader.ArrivalHeaders.AddNew();
			manifestMessageHeader = new ManifestMessageHeader(manifestHeader, bill, Constants.AIMMessageSubTypes.FSI, additionalMessageInformation);
		}

		AsycudaBill bill;
		AsycudaManifestHeader manifestHeader;
		ManifestMessageHeader manifestMessageHeader;
		AdditionalMessageInformation additionalMessageInformation;
	}
}
