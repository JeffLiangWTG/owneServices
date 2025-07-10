using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using static Enterprise.Customs.US.AIM.Messaging.Constants;

namespace Enterprise.Customs.US.ACEManifest.Business.Messaging.Testing
{
	class FSNMessageSenderTest : TestCaseWithFactory
	{
		public void TestSendFSNMessage()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			header.AMA_ManifestType = "IAM";
			header.AMA_MasterBill = "SHA-123456789";
			header.AMA_RL_NKPortOfDischarge = "USLAX";
			header.AMA_CarrierCode = "VOG";
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "Bill1";
			bill2.ABL_BillNumber = "Bill2";
			var arrHeader1 = header.ArrivalHeaders.AddNew();
			arrHeader1.ATH_Reference = "A";
			arrHeader1.ATH_ETAAtDischargePort = new ZDateTime(2020, 07, 22);
			arrHeader1.ATH_VoyageFlightNo = "VOG1234A";
			var transferHeader1 = arrHeader1.TransferHeaders.AddNew();
			transferHeader1.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			var trasnferBill1 = transferHeader1.TransferBills.AddNew();
			var transferBill2 = transferHeader1.TransferBills.AddNew();
			trasnferBill1.ATB_BillNumber = bill1.ABL_BillNumber;
			transferBill2.ATB_BillNumber = bill2.ABL_BillNumber;
			var sender1 = new FSNMessageSender(trasnferBill1, "3");
			sender1.SendMessage();
			header.Messages.Reload(false);
			var sender2 = new FSNMessageSender(transferBill2, "3");
			sender2.SendMessage();
			header.Messages.Reload(false);
			var message1 = header.Messages[0];
			AssertEquals(ApplicationCodeList.Codes.USAMA, message1.EM_ApplicationCode);
			AssertEquals("Bill1", message1.EM_ApplicationReference);
			AssertEquals(EDIMessageTypeList.Codes.FHL, message1.EM_MessageType);
			AssertEquals(AIMMessageSubTypes.FSN, message1.EM_MessageSubType);
			AssertEquals("Should attach on the AsycudaBill.", bill1.PK, message1.EM_LinkUniqueID);
			AssertEquals("Should attach on the AsycudaBill.", bill1.TableName, message1.EM_LinkTable);
			var expectedMessage = @"FSN
LAXVOG
SHA-12345678-BILL1
ARR/VOG1234A/22JUL-A
ASN3";
			AssertMultilineASCIIEquals(expectedMessage, message1.EM_MessageText);
			var message2 = header.Messages[1];
			AssertEquals(ApplicationCodeList.Codes.USAMA, message2.EM_ApplicationCode);
			AssertEquals("Bill2", message2.EM_ApplicationReference);
			AssertEquals(EDIMessageTypeList.Codes.FHL, message2.EM_MessageType);
			AssertEquals(AIMMessageSubTypes.FSN, message2.EM_MessageSubType);
			AssertEquals("Should attach on the AsycudaBill.", bill2.PK, message2.EM_LinkUniqueID);
			AssertEquals("Should attach on the AsycudaBill.", bill2.TableName, message2.EM_LinkTable);
			AssertEquals(2, header.Messages.Count);
		}

		public void TestSendFSNMessage_PaddedFlightNumber()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			header.AMA_ManifestType = "IAM";
			header.AMA_MasterBill = "SHA-123456789";
			header.AMA_RL_NKPortOfDischarge = "USLAX";
			header.AMA_CarrierCode = "VOG";
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "Bill1";
			var arrHeader1 = header.ArrivalHeaders.AddNew();
			arrHeader1.ATH_Reference = "A";
			arrHeader1.ATH_ETAAtDischargePort = new ZDateTime(2020, 07, 22);
			arrHeader1.ATH_VoyageFlightNo = "VOG2";
			var transferHeader1 = arrHeader1.TransferHeaders.AddNew();
			transferHeader1.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			var trasnferBill1 = transferHeader1.TransferBills.AddNew();
			trasnferBill1.ATB_BillNumber = bill1.ABL_BillNumber;
			var sender1 = new FSNMessageSender(trasnferBill1, "3");
			sender1.SendMessage();
			header.Messages.Reload(false);
			var message1 = header.Messages[0];
			AssertEquals(ApplicationCodeList.Codes.USAMA, message1.EM_ApplicationCode);
			AssertEquals("Bill1", message1.EM_ApplicationReference);
			AssertEquals(EDIMessageTypeList.Codes.FHL, message1.EM_MessageType);
			AssertEquals(AIMMessageSubTypes.FSN, message1.EM_MessageSubType);
			var expectedMessage = @"FSN
LAXVOG
SHA-12345678-BILL1
ARR/VOG002/22JUL-A
ASN3";
			AssertMultilineASCIIEquals(expectedMessage, message1.EM_MessageText);
		}
	}
}
