using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class EURMessageProcessorTest : ExportUnionMessageProcessorBaseTest<EURMessageProcessor>
	{
		public void TestGetCorrectBranchPK()
		{
			var messageObjects = CreateMessageWithOrigins(GetDefaultMessageText(), "EUR001");
			var incomingMessage = messageObjects.incomingMessage;
			Processor.PreProcessMessage(incomingMessage);

			AssertEquals("GetCorrectBranchPK should have returned the correct BranchPK", messageObjects.messageAttachee.JE_GB, incomingMessage.EM_GB);
		}

		public void TestProcessMessageSuccess()
		{
			var messageObjects = CreateMessageWithOrigins(GetDefaultMessageText(), "EUR001", TRMessageTypes.Codes.EUR);
			var incomingMessage = messageObjects.incomingMessage;
			incomingMessage.EM_Status = EDIMessage.Status.PreProcessedOK;
			var messageText = incomingMessage.EM_MessageText;

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			var cusEntryHeader = incomingMessage.EM_LinkedObject as CusEntryHeader;
			CombineAssertions("Updated Fields", () =>
			{
				AssertEquals("Union Rec.Num", "2024-03-00024162", cusEntryHeader.ExportUnionPayInfo.C9_PaymentReference);
				AssertEquals("Union Appr.Code", "1438 3148 0263 4408", cusEntryHeader.ExportUnionPayInfo.C9_IncomingPayResponseNo);
				AssertEquals("TPS Reference", "24153097211086048431", cusEntryHeader.ExportUnionPayInfo.C9_BankAccount);
				AssertEquals("Current Loan", 2709.1m, cusEntryHeader.ExportUnionPayInfo.C9_PaymentAmount);
				AssertEquals("Total Payment", 612.50m, cusEntryHeader.ExportUnionCharges.C1_ChargeAmount);
				AssertEquals("Payment Type", "C", cusEntryHeader.ExportUnionCharges.C1_MethodOfPayment);
			});

			var interpretation = incomingMessage.EM_MessageInterpretation.GetBodyText().RemoveLineBreakingsAndIndents();
			CombineAssertions("Message Process Result", () =>
			{
				AssertEquals("EM_MessageInterpretation should contain has been accepted", true, interpretation.Contains("Export Union message for job B00001000 has been accepted."));
				Assert("EM_MessageInterpretation should contains 'Column Labels'", interpretation.Contains(@"<th>Label</th><th>Value</th>"));
				Assert("EM_MessageInterpretation should contains 'Current Loan'", interpretation.Contains("<tr><td>Current Loan:</td><td>2709.1</td></tr>"));
				Assert("EM_MessageInterpretation should contains 'Total Payment'", interpretation.Contains("<tr><td>Total Payment:</td><td>612.50</td></tr>"));
				Assert("EM_MessageInterpretation should contains 'Payment Type'", interpretation.Contains("<tr><td>Payment Type:</td><td>C</td></tr>"));
				Assert("EM_MessageInterpretation should contains 'Union Rec.Num'", interpretation.Contains("<tr><td>Union Rec.Num.:</td><td>2024-03-00024162</td></tr>"));
				Assert("EM_MessageInterpretation should contains 'Union Appr.Code'", interpretation.Contains("<tr><td>Union Appr.Code:</td><td>1438 3148 0263 4408</td></tr>"));
				Assert("EM_MessageInterpretation should contains 'TPS Reference'", interpretation.Contains("<tr><td>TPS Reference:</td><td>24153097211086048431</td></tr>"));
			});

			AssertStatusesMessage(incomingMessage, TRMessageStatusCodeList.Codes.Accepted, EntryStatusTypeList.Codes.REG, EDIMessage.Status.ProcessedOK);
		}

		public void TestProcessMessageError()
		{
			var messageError = TRMessageTestHelper.GetFileText("EURError.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.ExportUnion.Incoming.");
			var messageObjects = CreateMessageWithOrigins(messageError, "EUR001", TRMessageTypes.Codes.EUR);
			var incomingMessage = messageObjects.incomingMessage;
			incomingMessage.EM_Status = EDIMessage.Status.PreProcessedOK;
			var messageText = incomingMessage.EM_MessageText;

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			var cusEntryHeader = incomingMessage.EM_LinkedObject as CusEntryHeader;
			var interpretation = incomingMessage.EM_MessageInterpretation.GetBodyText().RemoveLineBreakingsAndIndents();
			CombineAssertions("Message Process Result", () =>
			{
				AssertEquals("EM_MessageInterpretation should contain has been error", true, interpretation.Contains("Export Union message for job B00001000 has been error."));
				Assert("EM_MessageInterpretation should contains 'Column Labels'", interpretation.Contains(@"<th>Error Code</th><th>Error Description</th>"));
				Assert("EM_MessageInterpretation should contains 'Error Code'", interpretation.Contains("<td>CODEERROR</td>"));
				Assert("EM_MessageInterpretation should contains 'Error Description'", interpretation.Contains("<td>1.KALEMİN imalat&#231;ısı Tanımlı Değil</td>"));
			});

			AssertStatusesMessage(incomingMessage, TRMessageStatusCodeList.Codes.Error, EntryStatusTypeList.Codes.ERR, EDIMessage.Status.ProcessedOK);
		}

		void AssertStatusesMessage(EDIMessage incomingMessage, ZString messageStatus, ZString customsStatus, ZString ediMessageStatus)
		{
			var cusEntryHeader = incomingMessage.EM_LinkedObject as CusEntryHeader;
			var messageAttachee = (cusEntryHeader as IMessageAttachee);
			CombineAssertions("Statuses of Message", () =>
			{
				AssertEquals("Message Status", messageStatus, messageAttachee.MessageStatus);
				AssertEquals("Customs Status", customsStatus, messageAttachee.CustomsStatus);
				AssertEquals("message.EM_Status", ediMessageStatus, incomingMessage.EM_Status);
			});
		}

		public void TestGenerateEmail()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";
			var messageObjects = CreateMessageWithOrigins(GetDefaultMessageText(), "EUR001", TRMessageTypes.Codes.EUR);
			var outgoingMessage = messageObjects.originalMessage;
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;
			var incomingMessage = messageObjects.incomingMessage;

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			var cusEntryHeader = incomingMessage.EM_LinkedObject as CusEntryHeader;
			var declarationReference = cusEntryHeader.Declaration.JE_DeclarationReference;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(declarationReference));
			var bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The original sender should be notify", user.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains cleared Information", bodyText.Contains("A response message has been received from Customs."));
				Assert("Contains cleared Information", bodyText.Contains("Shown below is a summary of relevant information received in the message."));
				Assert("Current Loan", bodyText.Contains("<tr><td>Current Loan:</td><td>2709.1</td></tr>"));
				Assert("Total Payment", bodyText.Contains("<tr><td>Total Payment:</td><td>612.50</td></tr>"));
				Assert("Payment Type", bodyText.Contains("<tr><td>Payment Type:</td><td>C</td></tr>"));
				Assert("Union Rec.Num", bodyText.Contains("<tr><td>Union Rec.Num.:</td><td>2024-03-00024162</td></tr>"));
				Assert("Union Appr.Code", bodyText.Contains("<tr><td>Union Appr.Code:</td><td>1438 3148 0263 4408</td></tr>"));
				Assert("TPS Reference", bodyText.Contains("<tr><td>TPS Reference:</td><td>24153097211086048431</td></tr>"));
				Assert("Contains Accepted", bodyText.Contains("has been Accepted"));
			});
		}

		public void TestCreateOrUpdatePayInfo()
		{
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = newBranch.PK;
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CH_BGMReference = "3-B00001000";
			var crypto = "cryprto Infomation";
			var unionRecordNumber = "union Rec 1";
			var tps = "tps";
			var currentLoan = 100.12m;

			var payInfo = cusEntryHeader.ExportUnionPayInfo;
			AssertNull(payInfo);

			EURMessageProcessor.CreateOrUpdatePayInfo(cusEntryHeader, crypto, unionRecordNumber, tps, currentLoan.ToString());
			payInfo = cusEntryHeader.ExportUnionPayInfo;
			CombineAssertions("Pay Info Record | New", () =>
			{
				AssertEquals("Union Record Number", unionRecordNumber, payInfo.C9_PaymentReference);
				AssertEquals("Crypto", crypto, payInfo.C9_IncomingPayResponseNo);
				AssertEquals("Exporter Union", "EXU", payInfo.C9_PaymentParty);
				AssertEquals("TPS", tps, payInfo.C9_BankAccount);
				AssertEquals("Current Loan Value", currentLoan, payInfo.C9_PaymentAmount);
			});

			crypto = "cryprto Infomation2";
			unionRecordNumber = "union Rec 2";
			tps = "tps2";
			currentLoan = 200.12m;
			EURMessageProcessor.CreateOrUpdatePayInfo(cusEntryHeader, crypto, unionRecordNumber, tps, currentLoan.ToString());
			payInfo = cusEntryHeader.ExportUnionPayInfo;
			CombineAssertions("Pay Info Record | Update", () =>
			{
				AssertEquals("Union Record Number", unionRecordNumber, payInfo.C9_PaymentReference);
				AssertEquals("Crypto", crypto, payInfo.C9_IncomingPayResponseNo);
				AssertEquals("Exporter Union", "EXU", payInfo.C9_PaymentParty);
				AssertEquals("TPS", tps, payInfo.C9_BankAccount);
				AssertEquals("Current Loan Value", currentLoan, payInfo.C9_PaymentAmount);
			});
		}

		public void TestCreateOrUpdateCharges()
		{
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = newBranch.PK;
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CH_BGMReference = "3-B00001000";
			var totalPayment = 100m;
			var paymentType = "C";

			var charges = cusEntryHeader.ExportUnionCharges;
			AssertNull(charges);

			EURMessageProcessor.CreateOrUpdateCharges(cusEntryHeader, totalPayment.ToString(), paymentType);
			charges = cusEntryHeader.ExportUnionCharges;
			CombineAssertions("Charges Record | New", () =>
			{
				AssertEquals("Exporter Union", "EXU", charges.C1_ChargeType);
				AssertEquals("Payment Type", paymentType, charges.C1_MethodOfPayment);
				AssertEquals("Total Payment", totalPayment, charges.C1_ChargeAmount);
			});

			totalPayment = 200m;
			paymentType = "P";
			EURMessageProcessor.CreateOrUpdateCharges(cusEntryHeader, totalPayment.ToString(), paymentType);
			charges = cusEntryHeader.ExportUnionCharges;
			CombineAssertions("Charges Record | Update", () =>
			{
				AssertEquals("Exporter Union", "EXU", charges.C1_ChargeType);
				AssertEquals("Payment Type", paymentType, charges.C1_MethodOfPayment);
				AssertEquals("Total Payment", totalPayment, charges.C1_ChargeAmount);
			});
		}

		protected override EURMessageProcessor Processor => new EURMessageProcessor(logger);
		protected override ZString DefaultMessageType => TRMessageTypes.Codes.EUR;
		ZString GetDefaultMessageText() => TRMessageTestHelper.GetFileText("EURSuccess.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.ExportUnion.Incoming.");
	}
}
