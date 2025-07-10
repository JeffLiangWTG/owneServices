using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	class UEMEDIMessageSenderTest : TestCaseWithFactory
	{
		public void TestSendUEMMessage()
		{
			var header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			header.AMA_ApplicationCode = Enterprise.Customs.ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			header.AMA_ManifestType = USExportManifestTypes.Codes.EFM;
			header.AMA_Voyage = "VOG1234A";
			header.AMA_E_ARV = new ZDateTime(2020, 7, 25, 06, 30, 00, 00);
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "Bill1";
			var sender = new UEMEDIMessageSender(bill, USExportBillOfLadingActionCodeType.Codes.A);
			sender.SendUEMMessage();
			bill.Messages.Reload(false);
			var message = bill.Messages[0];
			AssertEquals(EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(bill, message.EM_LinkedObject);
			AssertEquals(MessageTypeList.Codes.ExportManifestSubmission, message.EM_MessageType);
			AssertEquals(MessageSubTypeList.Codes.BillOfLadingAdd, message.EM_MessageSubType);
			AssertEquals(EDIMessage.Status.Queued, message.EM_Status);
			AssertContains("BOLNumber", "<Value>Bill1</Value>", message.EM_MessageText);
			AssertContains("BOLClassificationCode", "<Value>M</Value>", message.EM_MessageText);
			AssertContains("MessageReferenceNumber", "<Value>!MessageNumberPlaceHolder!</Value>", message.EM_MessageText);
			AssertContains("FlightTripVoyageNumber", "<Value>VOG1234A</Value>", message.EM_MessageText);
			AssertContains("ActionCode", "<ActionCode>A</ActionCode>", message.EM_MessageText);
		}

		public void TestSendUEMMessage_Action()
		{
			var header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			header.AMA_ManifestType = USExportManifestTypes.Codes.EFM;
			header.AMA_MasterBill = "30102020012";
			header.AMA_Voyage = "VOG1234A";
			header.AMA_E_ARV = new ZDateTime(2020, 7, 25, 06, 30, 00, 00);
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "Bill1";
			var sender = new UEMEDIMessageSender(bill, USExportBillOfLadingActionCodeType.Codes.A);
			sender.SendUEMMessage();
			bill.Messages.Reload(false);
			AssertEquals(1, bill.Messages.Count);
			var message = bill.Messages[0];
			AssertEquals(MessageSubTypeList.Codes.BillOfLadingAdd, message.EM_MessageSubType);
			AssertContains("ActionCode", "<ActionCode>A</ActionCode>", message.EM_MessageText);

			sender = new UEMEDIMessageSender(bill, USExportBillOfLadingActionCodeType.Codes.R);
			sender.SendUEMMessage();
			bill.Messages.Reload(false);
			AssertEquals(2, bill.Messages.Count);
			message = bill.Messages[1];
			AssertEquals(MessageSubTypeList.Codes.BillOfLadingReplace, message.EM_MessageSubType);
			AssertContains("ActionCode", "<ActionCode>R</ActionCode>", message.EM_MessageText);

			sender = new UEMEDIMessageSender(bill, USExportBillOfLadingActionCodeType.Codes.D);
			sender.SendUEMMessage();
			bill.Messages.Reload(false);
			AssertEquals(3, bill.Messages.Count);
			message = bill.Messages[2];
			AssertEquals(MessageSubTypeList.Codes.BillOfLadingDelete, message.EM_MessageSubType);
			AssertContains("ActionCode", "<ActionCode>D</ActionCode>", message.EM_MessageText);
		}

		public void TestSendUEMMessage_BOLReferenceInfoList()
		{
			var bolReferenceInfoList = "<BOLReferenceInfoList>\r\n        <ReferenceTypeCode>\r\n          <Value>{0}</Value>\r\n        </ReferenceTypeCode>\r\n        <ReferenceData>\r\n          <Value>{1}</Value>\r\n        </ReferenceData>\r\n      </BOLReferenceInfoList>";

			var manifestHeader = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			manifestHeader.MasterBOL = "MST401092442";
			var masterBill = manifestHeader.MasterBill;
			masterBill.ABL_BillIssuer = "APLUTEST";
			var bill = manifestHeader.Bills.AddNew();
			bill.ABL_CustomsDischargePort = "ABC";
			bill.ABL_UCRNumber = "TES";

			var sender = new UEMEDIMessageSender(bill, USExportBillOfLadingActionCodeType.Codes.A);
			sender.SendUEMMessage();
			bill.Messages.Reload(false);
			AssertEquals(1, bill.Messages.Count);
			var message = bill.Messages[0];
			AssertContains("BOLClassificationCode", "<BOLClassificationCode>\r\n        <Value>H</Value>\r\n      </BOLClassificationCode>", message.EM_MessageText);
			AssertNotContains("BOLClassificationCode", "<BOLClassificationCode>\r\n        <Value>M</Value>\r\n      </BOLClassificationCode>", message.EM_MessageText);
			AssertContains("BolReferenceInfoList ReferenceTypeCode:CSK", string.Format(bolReferenceInfoList, "CSK","ABC"), message.EM_MessageText);
			AssertContains("BolReferenceInfoList ReferenceTypeCode:OB", string.Format(bolReferenceInfoList, "OB", "APLUMST401092442"), message.EM_MessageText);
			var emptyReferenceData = "<BOLReferenceInfoList>\r\n        <ReferenceTypeCode>\r\n          <Value>TES</Value>\r\n        </ReferenceTypeCode>\r\n        <ReferenceData>\r\n          <Value />\r\n        </ReferenceData>\r\n      </BOLReferenceInfoList>";
			AssertContains("BolReferenceInfoList ReferenceTypeCode:TES", emptyReferenceData, message.EM_MessageText);

			bill.InBondNumbers = "000000001,000000002";
			bill.AESITNNumbers = "X20120112901245,X2012011200001";
			sender = new UEMEDIMessageSender(bill, USExportBillOfLadingActionCodeType.Codes.A);
			sender.SendUEMMessage();
			bill.Messages.Reload(false);
			AssertEquals(2, bill.Messages.Count);
			message = bill.Messages[1];

			AssertContains("BolReferenceInfoList ReferenceTypeCode:TES", emptyReferenceData, message.EM_MessageText);
			AssertContains("BolReferenceInfoList ReferenceTypeCode:IB", string.Format(bolReferenceInfoList, "IB", "000000001"), message.EM_MessageText);
			AssertContains("BolReferenceInfoList ReferenceTypeCode:IB", string.Format(bolReferenceInfoList, "IB", "000000002"), message.EM_MessageText);
			AssertContains("BolReferenceInfoList ReferenceTypeCode:ITN", string.Format(bolReferenceInfoList, "ITN", "X20120112901245"), message.EM_MessageText);
			AssertContains("BolReferenceInfoList ReferenceTypeCode:ITN", string.Format(bolReferenceInfoList, "ITN", "X2012011200001"), message.EM_MessageText);

			manifestHeader.AMA_ApplicationCode = Enterprise.Customs.ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine;
			bill.ABL_BolType = "BOL";
			sender = new UEMEDIMessageSender(bill, USExportBillOfLadingActionCodeType.Codes.A);
			sender.SendUEMMessage();
			bill.Messages.Reload(false);
			AssertEquals(3, bill.Messages.Count);
			message = bill.Messages[2];
			AssertContains("BolReferenceInfoList ReferenceTypeCode:CSK", string.Format(bolReferenceInfoList, "CSK", "ABC"), message.EM_MessageText);
			AssertContains("BolReferenceInfoList ReferenceTypeCode:OB", string.Format(bolReferenceInfoList, "OB", "APLUMST401092442"), message.EM_MessageText);
			AssertContains("BolReferenceInfoList ReferenceTypeCode:TES", emptyReferenceData, message.EM_MessageText);
			AssertContains("BolReferenceInfoList ReferenceTypeCode:IB", string.Format(bolReferenceInfoList, "IB", "000000001"), message.EM_MessageText);
			AssertContains("BolReferenceInfoList ReferenceTypeCode:IB", string.Format(bolReferenceInfoList, "IB", "000000002"), message.EM_MessageText);
			AssertContains("BolReferenceInfoList ReferenceTypeCode:ITN", string.Format(bolReferenceInfoList, "ITN", "X20120112901245"), message.EM_MessageText);
			AssertContains("BolReferenceInfoList ReferenceTypeCode:ITN", string.Format(bolReferenceInfoList, "ITN", "X2012011200001"), message.EM_MessageText);
		}
	}
}
