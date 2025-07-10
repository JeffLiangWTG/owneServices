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
	public class ETradeQueryInspectionLineMessageProcessorTest : TestCaseWithFactory
	{
		readonly TRBranchCustomsMessageProcessor processor = new TRBranchCustomsMessageProcessor { Logger = new LoggingInformation() };

		public void TestUpdateCustomsStatusAndMessageModeWhenProcessMessage()
		{
			var errorMessageText = TRMessageTestHelper.GetFileText("ETradeError.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.ETrade.");
			var errorETradeHeader = CreateHeaderWithBills();
			var errorMessage = CreateMessage(errorMessageText, errorETradeHeader.PK);

			LineProcessor.ProcessMessage(errorMessage);

			AssertEquals(CustomsStatusList.Codes.QLR, errorETradeHeader.RegistrationStatus);
			AssertEquals(TRMessageTypes.Codes.TRL, errorETradeHeader.MessageMode);
			AssertEquals(TRMessageStatusCodeList.Codes.Error, ((IMessageAttachee)errorETradeHeader).MessageStatus);

			var successMessageText = TRMessageTestHelper.GetFileText("ETrade.QueryForInspectionLine.QueryForInspectionLineSuccess.xml");
			var importETradeHeader = CreateHeaderWithBills();
			importETradeHeader.AMA_Nature = ShipmentTypeList.Codes.Import23;
			var importSuccessMessage = CreateMessage(successMessageText, importETradeHeader.PK);

			LineProcessor.ProcessMessage(importSuccessMessage);

			AssertEquals(CustomsStatusList.Codes.QLS, importETradeHeader.RegistrationStatus);
			AssertEquals(TRMessageTypes.Codes.TRB, importETradeHeader.MessageMode);
			AssertEquals(TRMessageStatusCodeList.Codes.Accepted, ((IMessageAttachee)importETradeHeader).MessageStatus);
			var expectedInterpretation = TRMessageTestHelper.GetFileText("ETrade.QueryForInspectionLine.QueryForInspectionLineSuccess.htm");

			AssertEquals("EM_MessageInterpretation", expectedInterpretation, importSuccessMessage.EM_MessageInterpretation);

			var exportETradeHeader = CreateHeaderWithBills();
			exportETradeHeader.AMA_Nature = ShipmentTypeList.Codes.Export22;
			var exportSuccessMessage = CreateMessage(successMessageText, exportETradeHeader.PK);

			LineProcessor.ProcessMessage(exportSuccessMessage);

			AssertEquals(CustomsStatusList.Codes.QLS, exportETradeHeader.RegistrationStatus);
			AssertEquals(TRMessageTypes.Codes.CPL, exportETradeHeader.MessageMode);
			AssertEquals(TRMessageStatusCodeList.Codes.Accepted, ((IMessageAttachee)exportETradeHeader).MessageStatus);
		}

		ETradeHeader CreateHeaderWithBills()
		{
			var header = Factory.New<ETradeHeader>();
			header.AMA_GB = GlbBranch.CurrentBranch.PK;
			header.AMA_JobReference = "ETG0000001";
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;

			var bill = (ETradeBill)((BusinessObjectCollection)header.Bills).AddNew();
			bill.ABL_AMA = header.PK;
			bill.ABL_BillNumber = "6491183";

			bill = (ETradeBill)((BusinessObjectCollection)header.Bills).AddNew();
			bill.ABL_AMA = header.PK;
			bill.ABL_BillNumber = "6514123";
			return header;
		}

		ETradeEDIMessage CreateMessage(ZString messageText, ZGuid headerPk)
		{
			var message = Factory.New<ETradeEDIMessage>();
			message.EM_ApplicationReference = "ETG0000001";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message.EM_MessageType = TRMessageTypes.Codes.TRL;
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

		public void TestProcessSuccessMessage()
		{
			var factory = Factory;
			var header = CreateHeader();
			var bill = CreateBill(header);
			bill.ABL_AMA = header.PK;
			bill.ABL_BillNumber = "6491183";

			var bill2 = CreateBill(header);
			bill2.ABL_AMA = header.PK;
			bill2.ABL_BillNumber = "6514123";

			var bill3 = CreateBill(header);
			bill3.ABL_AMA = header.PK;
			bill3.ABL_BillNumber = "6514124";

			var messageText = TRMessageTestHelper.GetFileText("ETrade.QueryForInspectionLine.QueryForInspectionLineSuccess.xml");
			var responseMessage = CreateResponseMessageWithOrigin(header, factory, messageText);

			factory.Save();

			processor.ExecuteBatch();
			responseMessage.Reload();

			var headerLoad = new BusinessObjectFactory().Load<ETradeHeader>(header.PK);
			var billLoad = new BusinessObjectFactory().Load<ETradeBill>(bill.PK);
			var billLoad2 = new BusinessObjectFactory().Load<ETradeBill>(bill2.PK);
			var billLoad3 = new BusinessObjectFactory().Load<ETradeBill>(bill3.PK);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Inspection Line Message Status Response for " + header.AMA_JobReference);
			AssertNotNull(email);

			var body = email.Body;
			var recipients = email.Recipients;

			CombineAssertions(() =>
			{
				AssertEquals("One recipients", 1, recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", recipients[0].Email);
				Assert(body.Contains("E-Trade Inspection Line Message for job ETG0000001 has been cleared. For details please follow the Link to the E-Trade"));
				Assert("Contains First Column", body.Contains("Bill Number"));
				Assert("Contains Second Column", body.Contains("Inspection Line"));
				Assert("Contains Third Column", body.Contains("Approval Status"));

				AssertEquals("EM_LinkUniqueID", header.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", AsycudaManifestHeaderSchema.Constants.TableName, responseMessage.EM_LinkTable);

				AssertEquals("EM_Status", EDIMessageStatusList.Codes.ProcessedOK, responseMessage.EM_Status);
				AssertEquals("AMA_MessageStatus", TRMessageStatusCodeList.Codes.Accepted, headerLoad.AMA_MessageStatus);
			});

			CombineAssertions("Bill Status", () =>
			{
				AssertEquals("1.Bill ABL_CargoStatus", TRETradeCargoStatusList.Codes.APP, billLoad.ABL_CargoStatus);
				AssertEquals("2.Bill ABL_CargoStatus", TRETradeCargoStatusList.Codes.NAP, billLoad2.ABL_CargoStatus);
				AssertEquals("3.Bill ABL_CargoStatus", TRETradeCargoStatusList.Codes.APP, billLoad3.ABL_CargoStatus);

				AssertEquals("ABL_BillStatus", BillStatusList.Codes.YEL, billLoad.ABL_BillStatus);
				AssertEquals("ABL_MessageStatus", TRMessageStatusCodeList.Codes.Accepted, billLoad.ABL_MessageStatus);
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		public void TestProcessErrorMessage()
		{
			var factory = Factory;
			var header = CreateHeader();
			var bill = CreateBill(header);
			bill.ABL_AMA = header.PK;

			var messageText = TRMessageTestHelper.GetFileText("ETrade.QueryForInspectionLine.QueryForInspectionLineError.xml");

			var responseMessage = CreateResponseMessageWithOrigin(header, factory, messageText);

			processor.ExecuteBatch();
			responseMessage.Reload();

			var headerLoad = new BusinessObjectFactory().Load<ETradeHeader>(header.PK);
			var billLoad = new BusinessObjectFactory().Load<ETradeBill>(bill.PK);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Inspection Line Message Status Response (Failure) for " + header.AMA_JobReference);

			AssertNotNull(email);

			var body = email.Body;
			var recipients = email.Recipients;

			CombineAssertions(() =>
			{
				AssertEquals("One recipient", 1, recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", recipients[0].Email);

				Assert(body.Contains("E-Trade Inspection Line Message for job ETG0000001 has been rejected. For details please follow the Link to the E-Trade"));
				Assert("Contains First Column", body.Contains("Error Code"));
				Assert("Contains Second Column", body.Contains("Error Message"));

				AssertEquals("EM_LinkUniqueID", header.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", AsycudaManifestHeaderSchema.Constants.TableName, responseMessage.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.ProcessedOK, responseMessage.EM_Status);
				AssertEquals("AMA_MessageStatus", TRMessageStatusCodeList.Codes.Error, headerLoad.AMA_MessageStatus);
			});
		}

		public void TestProcessMessageEmptydBody()
		{
			var factory = Factory;
			var header = CreateHeader();

			var messageText = string.Empty;
			var responseMessage = CreateResponseMessageWithOrigin(header, factory, messageText);

			processor.ExecuteBatch();
			responseMessage.Reload();

			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, responseMessage.EM_Status);
		}

		public void TestProcessMessageInvalidBody()
		{
			var factory = Factory;
			var header = CreateHeader();

			var messageText = "Invalid Body";
			var responseMessage = CreateResponseMessageWithOrigin(header, factory, messageText);

			processor.ExecuteBatch();
			responseMessage.Reload();

			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, responseMessage.EM_Status);
		}

		public void TestProcessBillNotFound()
		{
			var factory = Factory;
			var header = CreateHeader();

			var messageText = TRMessageTestHelper.GetFileText("ETrade.QueryForInspectionLine.QueryForInspectionLineSuccess.xml");
			var responseMessage = CreateResponseMessageWithOrigin(header, factory, messageText);

			processor.ExecuteBatch();
			responseMessage.Reload();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Inspection Line Message Status Response for " + header.AMA_JobReference);

			AssertNotNull(email);

			var body = email.Body;
			var recipients = email.Recipients;

			CombineAssertions(() =>
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, responseMessage.EM_Status);
				AssertEquals("One recipient", 1, recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", recipients[0].Email);
				Assert(body.Contains("This bill could not be found"));
			});
		}
		ETradeHeader CreateHeader()
		{
			var header = Factory.New<ETradeHeader>();
			header.AMA_GB = GlbBranch.CurrentBranch.PK;
			header.AMA_JobReference = "ETG0000001";
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;

			Factory.Save();
			return header;
		}

		ETradeBill CreateBill(ETradeHeader header)
		{
			var bill = Factory.New<ETradeBill>();
			bill.ABL_AMA = header.PK;

			Factory.Save();
			return bill;
		}

		GlbStaff Staff1
		{
			get { return staff1 ?? (staff1 = CreateStaff("S09", "S09", "Staff09", "Dummy9@dummy.com")); }
		}
		GlbStaff staff1;

		GlbStaff CreateStaff(ZString code, ZString loginName, ZString fullName, ZString email)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = code;
			staff.GS_LoginName = loginName;
			staff.GS_FullName = fullName;
			staff.GS_EmailAddress = email;

			Factory.Save();
			return staff;
		}

		ETradeEDIMessage CreateResponseMessageWithOrigin(ETradeHeader header, BusinessObjectFactory factory, ZString messageText)
		{
			var sessionGUID = new ZGuid("E7FAB118-139D-4305-B109-91908D2A274F");

			var requestInterchange = MessageTestHelper.CreateInterchange(factory, TRMessageTypes.Codes.TRL, EDIInterchange.Direction.Transmit, EDIInterchange.Status.Sent, sessionGUID, "Test Interchange message");
			var requestMessage = MessageTestHelper.CreateEdiMessage<ETradeEDIMessage>(TRMessageTypes.Codes.TRL, "Body Text", EDIMessage.Status.Queued, EDIInterchange.Direction.Transmit, AsycudaManifestHeaderSchema.Constants.TableName, factory);
			requestMessage.EM_LinkUniqueID = header.PK;
			requestMessage.EM_EI = requestInterchange.PK;
			requestMessage.EM_ApplicationReference = "ETG0000001";
			requestMessage.EM_SystemCreateUser = Staff1.GS_Code;

			var responseInterchange = MessageTestHelper.CreateInterchange(factory, TRMessageTypes.Codes.TRL, EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued, sessionGUID, messageText);
			var responseMessage = MessageTestHelper.CreateEdiMessage<ETradeEDIMessage>(TRMessageTypes.Codes.TRL, messageText, EDIMessage.Status.Queued, EDIInterchange.Status.Received, AsycudaManifestHeaderSchema.Constants.TableName, factory);
			responseMessage.EM_EI = responseInterchange.PK;
			responseMessage.EM_LinkUniqueID = header.PK;
			responseMessage.EM_SystemCreateUser = Staff1.GS_Code;

			factory.Save();

			return responseMessage;
		}

		ETradeQueryInspectionLineMessageProcessor LineProcessor => lineProcessor ?? (lineProcessor = new ETradeQueryInspectionLineMessageProcessor(new LoggingInformation()));
		ETradeQueryInspectionLineMessageProcessor lineProcessor;
	}
}
