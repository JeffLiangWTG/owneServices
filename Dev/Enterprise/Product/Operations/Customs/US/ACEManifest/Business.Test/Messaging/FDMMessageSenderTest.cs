using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using static Enterprise.Customs.US.AIM.Messaging.Constants;

namespace Enterprise.Customs.US.ACEManifest.Business.Messaging.Testing
{
	class FDMMessageSenderTest : TestCaseWithFactory
	{
		public void TestSendFDMMessage()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			header.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
			header.AMA_MasterBill = "30102020012";
			header.AMA_Voyage = "VOG1234A";
			header.AMA_E_ARV = new ZDateTime(2020, 7, 25, 06, 30, 00, 00);
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "Bill1";
			var departureTimeUTC = new ZDateTime(2020, 7, 24, 07, 30, 00, 00);
			var additionalMessageInformation = new AdditionalMessageInformation(header);
			additionalMessageInformation.AM_FlightDepartureTime = departureTimeUTC;
			var sender = new FDMMessageSender(additionalMessageInformation);
			sender.SendMessage();
			header.Messages.Reload(false);
			var message = header.Messages[0];
			AssertEquals(ApplicationCodeList.Codes.USAMA, message.EM_ApplicationCode);
			AssertEquals("30102020012", message.EM_ApplicationReference);
			AssertEquals(EDIMessageTypeList.Codes.FHL, message.EM_MessageType);
			AssertEquals(AIMMessageSubTypes.FDM, message.EM_MessageSubType);
			AssertEquals(EDIMessage.Status.Queued, message.EM_Status);
			var expectedMessageText = @"FDM
DEP/VOG1234A/25JUL/24JUL0730
";
			AssertEquals(expectedMessageText, message.EM_MessageText);
		}

		public void TestSendFDMMessage_PaddedFlightNumber()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			header.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
			header.AMA_MasterBill = "30102020012";
			header.AMA_CarrierCode = "VOG";
			header.AMA_Voyage = "VOG1";
			header.AMA_E_ARV = new ZDateTime(2020, 7, 25, 06, 30, 00, 00);
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "Bill1";
			var departureTimeUTC = new ZDateTime(2020, 7, 24, 07, 30, 00, 00);
			var additionalMessageInformation = new AdditionalMessageInformation(header);
			additionalMessageInformation.AM_FlightDepartureTime = departureTimeUTC;
			var sender = new FDMMessageSender(additionalMessageInformation);
			sender.SendMessage();
			header.Messages.Reload(false);
			var message = header.Messages[0];
			AssertEquals(ApplicationCodeList.Codes.USAMA, message.EM_ApplicationCode);
			AssertEquals("30102020012", message.EM_ApplicationReference);
			AssertEquals(EDIMessageTypeList.Codes.FHL, message.EM_MessageType);
			AssertEquals(AIMMessageSubTypes.FDM, message.EM_MessageSubType);
			AssertEquals(EDIMessage.Status.Queued, message.EM_Status);
			var expectedMessageText = @"FDM
DEP/VOG001/25JUL/24JUL0730
";
			AssertEquals(expectedMessageText, message.EM_MessageText);
		}

		public void TestSendFDMMessageUsingArrivalDetails()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			header.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
			header.AMA_MasterBill = "30102020012";
			header.AMA_Voyage = "VOG1234A";
			header.AMA_E_ARV = new ZDateTime(2020, 7, 25, 06, 30, 00, 00);
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "Bill1";
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			arrivalHeader.ATH_VoyageFlightNo = "FL001";
			arrivalHeader.ATH_Reference = "B";
			arrivalHeader.ATH_ETAAtDischargePort = new ZDateTime(2020, 07, 26, 09, 30, 00, 00);
			var arrivalDetail = arrivalHeader.ArrivalDetails.AddNew();
			arrivalDetail.ATL_ABL_AsycudaBill = bill1.PK;
			var departureTimeUTC = new ZDateTime(2020, 7, 24, 07, 30, 00, 00);
			var additionalMessageInformation = new AdditionalMessageInformation(header);
			additionalMessageInformation.AM_FlightDepartureTime = departureTimeUTC;
			var sender = new FDMMessageSender(additionalMessageInformation);
			sender.SendMessage();
			header.Messages.Reload(false);
			var message = header.Messages[0];
			AssertEquals(ApplicationCodeList.Codes.USAMA, message.EM_ApplicationCode);
			AssertEquals("30102020012", message.EM_ApplicationReference);
			AssertEquals(EDIMessageTypeList.Codes.FHL, message.EM_MessageType);
			AssertEquals(AIMMessageSubTypes.FDM, message.EM_MessageSubType);
			AssertEquals(EDIMessage.Status.Queued, message.EM_Status);
			var expectedMessageText = @"FDM
DEP/FL001/26JUL/24JUL0730
";
			AssertEquals(expectedMessageText, message.EM_MessageText);
		}
	}
}
