using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using static Enterprise.Customs.US.AIM.Messaging.Constants;

namespace Enterprise.Customs.US.ACEManifest.Business.Messaging.Testing
{
	class FSQMessageSenderTest : TestCaseWithFactory
	{
		public void TestSendFSQMessage()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			header.AMA_ManifestType = "IAM";
			header.AMA_MasterBill = "SHA-123456789";
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "Bill1";
			bill2.ABL_BillNumber = "Bill2";
			var selectionItems = new SplitBillSelectionItem[] { new SplitBillSelectionItem(bill1, null), new SplitBillSelectionItem(bill2, null) };
			var sender1 = new FSQMessageSender(header);
			sender1.SendMessage(selectionItems, ZString.Empty);
			header.Messages.Reload(true);
			var message1 = header.Messages.Find(m => m.EM_ApplicationReference == "Bill1").First();
			AssertEquals(ApplicationCodeList.Codes.USAMA, message1.EM_ApplicationCode);
			AssertEquals(EDIMessageTypeList.Codes.FHL, message1.EM_MessageType);
			AssertEquals(AIMMessageSubTypes.FSQ, message1.EM_MessageSubType);
			AssertEquals("Should attach on the bill.", bill1.PK, message1.EM_LinkUniqueID);
			AssertEquals("Should attach on the bill.", bill1.TableName, message1.EM_LinkTable);
			var expectedMessage = @"FSQ
SHA-12345678-BILL1
FSQ/05";
			AssertMultilineASCIIEquals(expectedMessage, message1.EM_MessageText);
			var message2 = header.Messages.Find(m => m.EM_ApplicationReference == "Bill2").First();
			AssertEquals(ApplicationCodeList.Codes.USAMA, message2.EM_ApplicationCode);
			AssertEquals(EDIMessageTypeList.Codes.FHL, message2.EM_MessageType);
			AssertEquals(AIMMessageSubTypes.FSQ, message2.EM_MessageSubType);
			AssertEquals("Should attach on the bill.", bill2.PK, message2.EM_LinkUniqueID);
			AssertEquals("Should attach on the bill.", bill2.TableName, message2.EM_LinkTable);
			AssertEquals(2, header.Messages.Count);
		}

		public void TestSendFSQMessagesWithReferences()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			header.AMA_ManifestType = "IAM";
			header.AMA_MasterBill = "SHA-123456789";
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "Bill1";
			bill2.ABL_BillNumber = "Bill2";
			var arrivalHeader1 = header.ArrivalHeaders.AddNew();
			arrivalHeader1.ATH_VoyageFlightNo = "FLT1";
			arrivalHeader1.ATH_Reference = "A";
			var arrivalDetail = arrivalHeader1.ArrivalDetails.AddNew();
			arrivalDetail.ATL_ABL_AsycudaBill = bill1.PK;
			var arrivalHeader2 = header.ArrivalHeaders.AddNew();
			arrivalHeader2.ATH_VoyageFlightNo = "FLT2";
			arrivalHeader2.ATH_Reference = "B";
			var arrivalDetail2 = arrivalHeader2.ArrivalDetails.AddNew();
			arrivalDetail2.ATL_ABL_AsycudaBill = bill2.PK;
			var selectionItems = new SplitBillSelectionItem[] { new SplitBillSelectionItem(bill1, arrivalHeader1), new SplitBillSelectionItem(bill2, arrivalHeader2) };
			var sender1 = new FSQMessageSender(header);
			sender1.SendMessage(selectionItems, AIMFreightStatusRequestCodes.Codes.RequestForRoutingInformation);
			header.Messages.Reload(true);
			var message1 = header.Messages.Find(m => m.EM_ApplicationReference == "Bill1").First();
			AssertEquals(ApplicationCodeList.Codes.USAMA, message1.EM_ApplicationCode);
			AssertEquals(EDIMessageTypeList.Codes.FHL, message1.EM_MessageType);
			AssertEquals(AIMMessageSubTypes.FSQ, message1.EM_MessageSubType);
			AssertEquals("Should attach on the bill.", bill1.PK, message1.EM_LinkUniqueID);
			AssertEquals("Should attach on the bill.", bill1.TableName, message1.EM_LinkTable);
			var expectedMessage1 = @"FSQ
SHA-12345678-BILL1-A
FSQ/01";
			AssertMultilineASCIIEquals(expectedMessage1, message1.EM_MessageText);
			var message2 = header.Messages.Find(m => m.EM_ApplicationReference == "Bill2").First();
			AssertEquals(ApplicationCodeList.Codes.USAMA, message2.EM_ApplicationCode);
			AssertEquals(EDIMessageTypeList.Codes.FHL, message2.EM_MessageType);
			AssertEquals(AIMMessageSubTypes.FSQ, message2.EM_MessageSubType);
			AssertEquals("Should attach on the bill.", bill2.PK, message2.EM_LinkUniqueID);
			AssertEquals("Should attach on the bill.", bill2.TableName, message2.EM_LinkTable);
			var expectedMessage2 = @"FSQ
SHA-12345678-BILL2-B
FSQ/01";
			AssertMultilineASCIIEquals(expectedMessage2, message2.EM_MessageText);
			AssertEquals(2, header.Messages.Count);
		}

		public void TestSendFSQMessageForMAWB()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			header.AMA_ManifestType = "IAM";
			header.AMA_MasterBill = "SHA-123456789";
			header.AMA_RL_NKPortOfDischarge = "USLAX";
			header.AMA_CarrierCode = "VOG";
			var selectionItems = new SplitBillSelectionItem[] { new SplitBillSelectionItem((AsycudaBill)header.MasterBill, null) };
			var sender1 = new FSQMessageSender(header);
			sender1.SendMessage(selectionItems, ZString.Empty);
			header.Messages.Reload(true);
			var message1 = header.Messages[0];
			AssertEquals(ApplicationCodeList.Codes.USAMA, message1.EM_ApplicationCode);
			AssertEquals(EDIMessageTypeList.Codes.FHL, message1.EM_MessageType);
			AssertEquals(AIMMessageSubTypes.FSQ, message1.EM_MessageSubType);
			AssertEquals("SHA-123456789", message1.EM_ApplicationReference);
			AssertEquals("Should attach on the header.", header.PK, message1.EM_LinkUniqueID);
			AssertEquals("Should attach on the header.", header.TableName, message1.EM_LinkTable);
			var expectedMessage = @"FSQ
LAXVOG
SHA-12345678
FSQ/05";
			AssertMultilineASCIIEquals(expectedMessage, message1.EM_MessageText);
			AssertEquals(1, header.Messages.Count);
		}

		public void TestSendFSQMessageForMAWBWithReference()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			header.AMA_ManifestType = "IAM";
			header.AMA_MasterBill = "SHA-123456789";
			header.AMA_RL_NKPortOfDischarge = "USLAX";
			header.AMA_CarrierCode = "VOG";
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			arrivalHeader.ATH_VoyageFlightNo = "FLT1";
			arrivalHeader.ATH_Reference = "A";
			var arrivalDetail = arrivalHeader.ArrivalDetails.AddNew();
			arrivalDetail.ATL_ABL_AsycudaBill = header.MasterBill.PK;
			var selectionItems = new SplitBillSelectionItem[] { new SplitBillSelectionItem((AsycudaBill)header.MasterBill, arrivalHeader) };
			var sender = new FSQMessageSender(header);
			sender.SendMessage(selectionItems, AIMFreightStatusRequestCodes.Codes.RequestForRoutingInformation);
			header.Messages.Reload(true);
			var message1 = header.Messages[0];
			AssertEquals(ApplicationCodeList.Codes.USAMA, message1.EM_ApplicationCode);
			AssertEquals(EDIMessageTypeList.Codes.FHL, message1.EM_MessageType);
			AssertEquals(AIMMessageSubTypes.FSQ, message1.EM_MessageSubType);
			AssertEquals("SHA-123456789", message1.EM_ApplicationReference);
			AssertEquals("Should attach on the header.", header.PK, message1.EM_LinkUniqueID);
			AssertEquals("Should attach on the header.", header.TableName, message1.EM_LinkTable);
			var expectedMessage = @"FSQ
LAXVOG
SHA-12345678-A
FSQ/01";
			AssertMultilineASCIIEquals(expectedMessage, message1.EM_MessageText);
			AssertEquals(1, header.Messages.Count);
		}
	}
}
