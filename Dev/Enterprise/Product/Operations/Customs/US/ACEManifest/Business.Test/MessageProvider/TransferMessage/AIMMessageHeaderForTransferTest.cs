using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.AIM.Messaging;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.US.AIM.Messaging.Constants;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class AIMMessageHeaderForTransferTest : TestCaseWithFactory
	{
		public void TestIManifestMessageHeaderFromHouseBill()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_MasterBill = "SHA-123456789";
			var bill = manifestHeader.Bills.AddNew();
			bill.ABL_BillNumber = "HAWB001";
			bill.ShipperOrgPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			bill.ConsigneeOrgPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			bill.CustomsEntryNumberType = ACEManifestBillEntryNumberTypes.Codes.Informal;
			bill.CustomsEntryNumber = "12345678901";
			var arrHeader = manifestHeader.ArrivalHeaders.AddNew();
			var transfer = arrHeader.TransferHeaders.AddNew();
			var transferBill = transfer.TransferBills.AddNew();
			transferBill.ATB_ABL_Bill = bill.PK;
			IManifestMessageHeader messageHeader = new AIMMessageHeaderForTransfer(transferBill, AIMMessageSubTypes.FRC, false);
			AssertEquals("MessageType", AIMMessageSubTypes.FRC, messageHeader.MessageType);
			AssertEquals("Reference", "HAWB001", messageHeader.Reference);
			AssertType<AIMCargoControlLocation>("CargoControlLine", messageHeader.CargoControlLine);
			AssertType<AIMAirWaybill>("AirWaybill", messageHeader.AirWaybill);
			AssertType<AIMWaybill>("Waybill", messageHeader.Waybill);
			AssertType<AIMArrival>("Arrival", messageHeader.Arrival);
			AssertNull("Shipper", messageHeader.Shipper);
			AssertNull("Consignee", messageHeader.Consignee);
			var cbpEntryDetail = messageHeader.CBPEntryDetail;
			AssertEquals(ACEManifestBillEntryNumberTypes.Codes.Informal, cbpEntryDetail.EntryType);
			AssertEquals("12345678901", cbpEntryDetail.EntryNumber);
			AssertNull("FDAFreightIndicator", messageHeader.FDAFreightIndicator);
			AssertType<AIMReasonForAmendment>("ReasonForAmendment", messageHeader.ReasonForAmendment);
			AssertEquals("19", messageHeader.ReasonForAmendment.AmendmentCode);
			AssertNull("Agent", messageHeader.Agent);
			AssertType<AIMTransfer>("Transfer", messageHeader.Transfer);

			bill.CustomsEntryNumber = "";
			cbpEntryDetail = new AIMMessageHeaderForTransfer(transferBill, AIMMessageSubTypes.FRC, false).CBPEntryDetail;
			AssertEquals(ACEManifestBillEntryNumberTypes.Codes.Informal, cbpEntryDetail.EntryType);
			AssertEquals("", cbpEntryDetail.EntryNumber);

			bill.CustomsEntryNumberType = "";
			AssertNull(new AIMMessageHeaderForTransfer(transferBill, AIMMessageSubTypes.FRC, false).CBPEntryDetail);
		}

		public void TestIManifestMessageHeaderFromMasterBill()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_MasterBill = "SHA-123456789";
			var bill = manifestHeader.Bills.AddNew();
			bill.ABL_BillNumber = "HAWB001";
			bill.ShipperOrgPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			bill.ConsigneeOrgPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			var masterBill = manifestHeader.MasterBill;
			masterBill.CustomsEntryNumberType = ACEManifestBillEntryNumberTypes.Codes.Informal;
			masterBill.CustomsEntryNumber = "12345678901";
			var arrHeader = manifestHeader.ArrivalHeaders.AddNew();
			var transfer = arrHeader.TransferHeaders.AddNew();
			var transferBill = transfer.TransferBills.AddNew();
			transferBill.ATB_ABL_Bill = masterBill.PK;
			IManifestMessageHeader messageHeader = new AIMMessageHeaderForTransfer(transferBill, AIMMessageSubTypes.FRC, false);
			AssertEquals("MessageType", AIMMessageSubTypes.FRC, messageHeader.MessageType);
			AssertEquals("Reference", "SHA-123456789", messageHeader.Reference);
			AssertType<AIMCargoControlLocation>("CargoControlLine", messageHeader.CargoControlLine);
			AssertType<AIMAirWaybillForManifestMsg>("AirWaybill", messageHeader.AirWaybill);
			AssertEquals(true, messageHeader.AirWaybill.IsMasterAirWaybill);
			AssertType<AIMWaybillForManifestMsg>("Waybill", messageHeader.Waybill);
			AssertEquals("CONSOLIDATION", messageHeader.Waybill.CargoDescription);
			AssertType<AIMArrival>("Arrival", messageHeader.Arrival);
			AssertNull("Shipper", messageHeader.Shipper);
			AssertNull("Consignee", messageHeader.Consignee);
			var cbpEntryDetail = messageHeader.CBPEntryDetail;
			AssertEquals(ACEManifestBillEntryNumberTypes.Codes.Informal, cbpEntryDetail.EntryType);
			AssertEquals("12345678901", cbpEntryDetail.EntryNumber);
			AssertNull("FDAFreightIndicator", messageHeader.FDAFreightIndicator);
			AssertType<AIMReasonForAmendment>("ReasonForAmendment", messageHeader.ReasonForAmendment);
			AssertEquals("19", messageHeader.ReasonForAmendment.AmendmentCode);
			AssertNull("Agent", messageHeader.Agent);
			AssertType<AIMTransfer>("Transfer", messageHeader.Transfer);

			masterBill.CustomsEntryNumber = "";
			cbpEntryDetail = new AIMMessageHeaderForTransfer(transferBill, AIMMessageSubTypes.FRC, false).CBPEntryDetail;
			AssertEquals(ACEManifestBillEntryNumberTypes.Codes.Informal, cbpEntryDetail.EntryType);
			AssertEquals("", cbpEntryDetail.EntryNumber);

			masterBill.CustomsEntryNumberType = "";
			AssertNull(new AIMMessageHeaderForTransfer(transferBill, AIMMessageSubTypes.FRC, false).CBPEntryDetail);
		}
	}
}
