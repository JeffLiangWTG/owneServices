using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ETradeBill = Enterprise.Integration.Customs.ASYCUDA.TRETrade.IAsycudaBill;
using ETradeHeader = Enterprise.Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader;

namespace Enterprise.Customs.TR.Business.Testing
{
	class ETradeQueryRemainingBillsForImportMessageProcessorTest : TestCaseWithFactory
	{
		public void TestUpdateCustomsStatusAndMessageModeWhenProcessMessage()
		{
			var errorMessageText = TRMessageTestHelper.GetFileText("ETradeError.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.ETrade.");
			var errorETradeHeader = CreateHeaderWithBills();
			var errorMessage = CreateMessage(errorMessageText, errorETradeHeader.PK);

			Processor.ProcessMessage(errorMessage);

			AssertEquals(CustomsStatusList.Codes.QBR, errorETradeHeader.RegistrationStatus);
			AssertEquals(TRMessageTypes.Codes.TRB, errorETradeHeader.MessageMode);
			AssertEquals(TRMessageStatusCodeList.Codes.Error, ((IMessageAttachee)errorETradeHeader).MessageStatus);

			var errorMessageText2 = TRMessageTestHelper.GetFileText("ETrade.QueryRemainingBillsForImport.QueryRemainingBillsForInvalid.xml");
			var errorETradeHeader2 = CreateHeaderWithBills();
			var errorMessage2 = CreateMessage(errorMessageText2, errorETradeHeader2.PK);

			Processor.ProcessMessage(errorMessage2);

			CombineAssertions("QueryRemainingBillsForInvalid", () =>
			{
				var remaningMessageText2 = TRMessageTestHelper.GetFileText("ETrade.QueryRemainingBillsForImport.QueryRemainingBillsForInvalid.htm");
				AssertEquals(CustomsStatusList.Codes.QBR, errorETradeHeader.RegistrationStatus);
				AssertEquals(TRMessageTypes.Codes.TRB, errorETradeHeader.MessageMode);
				AssertEquals(TRMessageStatusCodeList.Codes.Error, ((IMessageAttachee)errorETradeHeader2).MessageStatus);
				Assert("EM_MessageInterpretation should contains title", errorMessage2.EM_MessageInterpretation.Contains("has been rejected"));
				Assert("EM_MessageInterpretation should contains 'Label'", errorMessage2.EM_MessageInterpretation.Contains(">Error Message<"));
				Assert("EM_MessageInterpretation should contains 'Description'", errorMessage2.EM_MessageInterpretation.Contains("<td>Sql sorgulama hatası!</td>"));
			});

			var successMessageText = TRMessageTestHelper.GetFileText("ETrade.QueryRemainingBillsForImport.QueryRemainingBillsForImportSuccess.xml");
			var importETradeHeader = CreateHeaderWithBills();
			importETradeHeader.AMA_Nature = ShipmentTypeList.Codes.Import23;
			var importSuccessMessage = CreateMessage(successMessageText, importETradeHeader.PK);

			Processor.ProcessMessage(importSuccessMessage);

			AssertEquals(CustomsStatusList.Codes.QBS, importETradeHeader.RegistrationStatus);
			AssertEquals(TRMessageTypes.Codes.TRD, importETradeHeader.MessageMode);
			AssertEquals(TRMessageStatusCodeList.Codes.Accepted, ((IMessageAttachee)importETradeHeader).MessageStatus);
			var expectedInterpretation = TRMessageTestHelper.GetFileText("ETrade.QueryRemainingBillsForImport.QueryRemainingBillsForImportSuccess.htm");
			AssertEquals("EM_MessageInterpretation", expectedInterpretation, importSuccessMessage.EM_MessageInterpretation);
		}

		public void TestErrorResponseMessageWithSuccesContent()
		{
			var errorMessageText = TRMessageTestHelper.GetFileText("ETrade.QueryRemainingBillsForImport.QueryRemainingBillsForImportErrorFormat2.xml");
			var errorETradeHeader = CreateHeaderWithBills();
			var errorMessage = CreateMessage(errorMessageText, errorETradeHeader.PK);
			errorMessage.EM_MessageType = TRMessageTypes.Codes.TRB;

			Processor.ProcessMessage(errorMessage);
			AssertEquals(CustomsStatusList.Codes.QBS, errorETradeHeader.RegistrationStatus);
			AssertEquals(TRMessageTypes.Codes.TRD, errorETradeHeader.MessageMode);
			AssertEquals(TRMessageStatusCodeList.Codes.Accepted, ((IMessageAttachee)errorETradeHeader).MessageStatus);
		}

		public void TestErrorResponseMessage()
		{
			var errorMessageText = TRMessageTestHelper.GetFileText("ETrade.QueryRemainingBillsForImport.QueryRemainingBillsForImportErrorFormat3.xml");
			var errorETradeHeader = CreateHeaderWithBills();
			var errorMessage = CreateMessage(errorMessageText, errorETradeHeader.PK);
			errorMessage.EM_MessageType = TRMessageTypes.Codes.TRB;

			Processor.ProcessMessage(errorMessage);
			AssertEquals(CustomsStatusList.Codes.QBR, errorETradeHeader.RegistrationStatus);
			AssertEquals(TRMessageTypes.Codes.TRB, errorETradeHeader.MessageMode);
			AssertEquals(TRMessageStatusCodeList.Codes.Error, ((IMessageAttachee)errorETradeHeader).MessageStatus);
		}

		ETradeHeader CreateHeaderWithBills()
		{
			var header = Factory.New<ETradeHeader>();
			header.AMA_GB = GlbBranch.CurrentBranch.PK;
			header.AMA_JobReference = "ETG0000001";
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;

			var bill = (ETradeBill)((BusinessObjectCollection)header.Bills).AddNew();
			bill.ABL_AMA = header.PK;
			bill.ABL_BillNumber = "27";

			bill = (ETradeBill)((BusinessObjectCollection)header.Bills).AddNew();
			bill.ABL_AMA = header.PK;
			bill.ABL_BillNumber = "6514123";
			return header;
		}

		public void TestETradeQueryRemainingBillsProcessMessage()
		{
			var messageText = TRMessageTestHelper.GetBodyText(TRMessageTypes.Codes.TRB);
			var parentNodeList = new List<ZString>() { "TCGBMuayeneDurum", "ayrilanTasimaSenetleri", "AyrilanTasimaSenedi" };
			var remailningBills = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "tasimaSenediNumarasi");
			var reason = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "ayrilmaSebebi");

			var header = Factory.New<ETradeHeader>();
			var bill = Factory.New<ETradeBill>();
			bill.ABL_AMA = header.PK;
			header.AMA_JobReference = "ETG0010107";

			var message = CreateETradeMessage("ETG0010107", "TRB", messageText, "1500");
			message.EM_LinkUniqueID = header.PK;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			SetupOriginalETradeMessageForQueryRemainingBills(header, messageText);
			Factory.Save();
			Processor.ProcessMessage(message);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Query Remaining Bills for Import Declaration Message Status Response for " + header.AMA_JobReference);
			AssertNotNull(email);
			var bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertEquals("One recipient", 1, email.Recipients.Count);
				AssertEquals("Email Address", true, email.Recipients.Contains("Dummy9@dummy.com"));
				Assert("Remaning Bills Number", bodyText.Contains("Remaining Bills Number"));
				AssertEquals("message.EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("27", remailningBills);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			messageText = TRMessageTestHelper.GetBodyText(TRMessageTypes.Codes.TRB, true);
			var message2 = CreateETradeMessage("ETG0010108", "TRB", messageText, "1501");
			message2.EM_LinkUniqueID = header.PK;
			message2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();
			Processor.ProcessMessage(message2);

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Query Remaining Bills for Import Declaration Message Status Response (Failure) for " + header.AMA_JobReference);
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals("One recipient", 1, email.Recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", email.Recipients[0].Email);
			});
			bodyText = email.Body;
			Assert("Contains Error Information", bodyText.Contains("Error Message"));
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			messageText = TRMessageTestHelper.GetFileText("ETrade.QueryRemainingBillsForImport.QueryRemainingBillsForImportErrorFormat2.xml");
			var message3 = CreateETradeMessage("ETG0010108", "TRB", messageText, "1501");
			message3.EM_LinkUniqueID = header.PK;
			message3.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();
			Processor.ProcessMessage(message3);

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Query Remaining Bills for Import Declaration Message Status Response for " + header.AMA_JobReference);
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals("One recipient", 1, email.Recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", email.Recipients[0].Email);
			});
			bodyText = email.Body;
			Assert("Sould not contain Information", !bodyText.Contains("Error Message"));
			Assert("Sould contain Information", bodyText.Contains("Message"));
			Assert("Message details", bodyText.Contains("Ayrılmış taşıma senedi bulunamadı!"));
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			AssertEquals("AMA_MessageStatus", "ACP", header.AMA_MessageStatus);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			messageText = TRMessageTestHelper.GetFileText("ETrade.QueryRemainingBillsForImport.QueryRemainingBillsForImportErrorFormat3.xml");
			var message4 = CreateETradeMessage("ETG0010108", "TRB", messageText, "1501");
			message4.EM_LinkUniqueID = header.PK;
			message4.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();
			Processor.ProcessMessage(message4);

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Query Remaining Bills for Import Declaration Message Status Response (Failure) for " + header.AMA_JobReference);
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals("One recipient", 1, email.Recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", email.Recipients[0].Email);
			});
			bodyText = email.Body;
			Assert("Should contains error info", bodyText.Contains("Error Message"));
			Assert("Should contains error details", bodyText.Contains("Sql sorgulama hatası!"));
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
		}

		ETradeEDIMessage CreateMessage(ZString messageText, ZGuid headerPk)
		{
			var message = Factory.New<ETradeEDIMessage>();
			message.EM_ApplicationReference = "ETG0000001";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message.EM_MessageType = TRMessageTypes.Codes.TRB;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			if (!headerPk.IsEmpty)
			{
				message.EM_LinkUniqueID = headerPk;
			}
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_IsTestMessage = true;
			message.EM_MessageNum = "1";
			message.EM_MessageText = messageText;
			return message;
		}

		ETradeQueryRemainingBillsForImportMessageProcessor Processor => processor ?? (processor = new ETradeQueryRemainingBillsForImportMessageProcessor(new LoggingInformation()));
		ETradeQueryRemainingBillsForImportMessageProcessor processor;

		ETradeEDIMessage CreateETradeMessage(ZString applicationReference, ZString messageType, ZString messageText, string messageNum)
		{
			var message = Factory.New<ETradeEDIMessage>();
			message.EM_ApplicationReference = applicationReference;
			message.EM_Status = EDIMessage.Status.Received;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = messageType;
			message.EM_MessageText = messageText;
			message.EM_MessageNum = messageNum;
			return message;
		}

		void SetupOriginalETradeMessageForQueryRemainingBills(ETradeHeader header, ZString messageText)
		{
			var message1 = Factory.New<ETradeEDIMessage>();
			message1.EM_SystemCreateUser = Staff1.GS_Code;
			message1.EM_ApplicationReference = "ETG0010107";
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message1.EM_Status = EDIMessageStatusList.Codes.Received;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageType = TRMessageTypes.Codes.TRB;
			message1.EM_MessageText = messageText;
			message1.EM_MessageNum = "1500";
			message1.EM_LinkUniqueID = header.PK;
			message1.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();

			var message2 = Factory.New<ETradeEDIMessage>();
			message2.EM_SystemCreateUser = Staff2.GS_Code;
			message2.EM_ApplicationReference = "ETG0010108";
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message2.EM_Status = EDIMessageStatusList.Codes.Received;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = TRMessageTypes.Codes.TRB;
			message2.EM_MessageText = messageText;
			message2.EM_MessageNum = "1501";
			message2.EM_LinkUniqueID = header.PK;
			message2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();
		}

		#region Staffs

		GlbStaff Staff1 => staff1 ?? (staff1 = CreateStaff("S09", "S09", "Staff09", "Dummy9@dummy.com"));
		GlbStaff staff1;

		GlbStaff Staff2 => staff2 ?? (staff2 = CreateStaff("S08", "S08", "Staff08", "Dummy8@dummy.com"));
		GlbStaff staff2;

		GlbStaff CreateStaff(ZString code, ZString loginName, ZString fullName, ZString email)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = code;
			staff.GS_LoginName = loginName;
			staff.GS_FullName = fullName;
			staff.GS_EmailAddress = email;
			return staff;
		}

		#endregion
	}
}
