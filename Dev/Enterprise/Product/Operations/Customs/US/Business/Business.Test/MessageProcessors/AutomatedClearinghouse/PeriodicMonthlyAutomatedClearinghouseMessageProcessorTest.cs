using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.Environment;
using Enterprise.Registry.Business.Customs;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	class PeriodicMonthlyAutomatedClearinghouseMessageProcessorTest : AutomatedClearinghouseMessageProcessorTest<PDSPU, PeriodicMonthlyAutomatedClearinghouseMessageProcessor>
	{
		public void TestStatementNumberWithAlphabeticCharacter()
		{
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = MQEDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.PeriodicDailyStatementACHDebitAuthorizationEntrySummaryPresentationResponse;
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_MessageText = "B013001385PZ                              SE               7625                 PU 3910295D92385   0001349579102209Q11DEBIT/SUMM PRESENTATION ACCPTD015342      Y  3001385PZ00001";
			AssertNoExceptionThrown(() => _ = message.MessageBlock.MessageBlocks);

			var pu = message.MessageBlock.MessageBlocks.OfType<PDSPU>().FirstOrDefault();
			AssertEquals("Right-justified", "3910295D92", pu.StatementNumber);
		}

		public void TestNewACHDebitAuthorizationFormat()
		{
			var statementHeader = Factory.New<CusStatementHeader>();
			statementHeader.B2_EntryFilerCode = "SV9";
			statementHeader.B2_StatementNumber = "3919186000";

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var paymentMessage = mock.Object;
			paymentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			paymentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			paymentMessage.EM_MessageNum = "HYEDUSCMT_203201";
			paymentMessage.EM_MessageText = "B  1101SV9RM                                               HYEDUSCMT_203201     PT12345602SV9 39191860000000033236Y071819                                       Y  1101SV9RM";
			paymentMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ACHDebitAuthorizationEntrySummaryPresentation;
			paymentMessage.EM_LinkedObject = statementHeader;

			var incomingPaymentMessage = Factory.New<MQEDIMessage>();
			incomingPaymentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingPaymentMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.PeriodicDailyStatementACHDebitAuthorizationEntrySummaryPresentationResponse;
			incomingPaymentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingPaymentMessage.EM_MessageNum = "HYEDUSCMT_203201";
			incomingPaymentMessage.EM_Status = EDIMessage.Status.Queued;
			incomingPaymentMessage.EM_MessageText = "B001101SV9PZ                                               HYEDUSCMT_203201     E0 STMTNO 000001 REF ID: 3919186000                                             E1 FD21   AUTHORIZATION DOES NOT EXIST TO NEGATE  SV9  3919186000               E1 FD12   PAYER UNIT NUMBER NOT ON FILE           SV9  3919186000               E1 FD10   INCORRECT PUN FOR FILER                 SV9  3919186000               E1RF998   TRANSACTION DATA REJECTED               SV9  3919186000               Y  1101SV9PZ00005";

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			Factory.Save();
			new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(incomingPaymentMessage);
			AssertEquals(PaymentStatusList.Codes.PaymentAuthorizationDeletionFailed, statementHeader.B2_PaymentStatus);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Statement/Bill No.</th><th>Statement Filer</th><th>Payment Filer</th><th>Payment Amount</th><th>Date Accepted</th><th>Acceptance/Error Details</th><th>Payer&#39;s Unit No.</th></tr></thead><tr><td>3919186000</td><td>SV9</td><td>&nbsp;</td><td>332.36</td><td>&nbsp;</td><td>D21 - AUTHORIZATION DOES NOT EXIST TO NEGATE</td><td>123456</td></tr><tr><td>3919186000</td><td>SV9</td><td>&nbsp;</td><td>332.36</td><td>&nbsp;</td><td>D12 - PAYER UNIT NUMBER NOT ON FILE</td><td>123456</td></tr><tr><td>3919186000</td><td>SV9</td><td>&nbsp;</td><td>332.36</td><td>&nbsp;</td><td>D10 - INCORRECT PUN FOR FILER</td><td>123456</td></tr><tr><td>3919186000</td><td>SV9</td><td>&nbsp;</td><td>332.36</td><td>&nbsp;</td><td>998 - TRANSACTION DATA REJECTED</td><td>123456</td></tr></table>", email.Body);
		}

		public void TestDeletePaymentAuthorization()
		{
			var statementHeader = Factory.New<CusStatementHeader>();
			statementHeader.B2_EntryFilerCode = "SV9";
			statementHeader.B2_StatementNumber = "3919186000";
			statementHeader.B2_AccountNo = "HXU241025";
			statementHeader.B2_PaymentParty = PaymentPartyList.Codes.Broker;

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var paymentMessage = mock.Object;
			paymentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			paymentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			paymentMessage.EM_MessageNum = "HYEDUSCMT_203201";
			paymentMessage.EM_MessageText = "B  1101SV9RM                                               HYEDUSCMT_203201     PT12345602SV9 39191860000000033236Y071819                                       Y  1101SV9RM";
			paymentMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ACHDebitAuthorizationEntrySummaryPresentation;
			paymentMessage.EM_LinkedObject = statementHeader;

			var incomingPaymentMessage = Factory.New<MQEDIMessage>();
			incomingPaymentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingPaymentMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.PeriodicDailyStatementACHDebitAuthorizationEntrySummaryPresentationResponse;
			incomingPaymentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingPaymentMessage.EM_MessageNum = "HYEDUSCMT_203201";
			incomingPaymentMessage.EM_Status = EDIMessage.Status.Queued;
			incomingPaymentMessage.EM_MessageText = "B001101SV9PZ                                               HYEDUSCMT_203201     E0 STMTNO 000001 REF ID: 3919186000                                             E1 FD21   AUTHORIZATION DOES NOT EXIST TO NEGATE  SV9  3919186000               E1 FD12   PAYER UNIT NUMBER NOT ON FILE           SV9  3919186000               E1 FD10   INCORRECT PUN FOR FILER                 SV9  3919186000               E1RF998   TRANSACTION DATA REJECTED               SV9  3919186000   101724      Y  1101SV9PZ00005";

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			Factory.Save();
			new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(incomingPaymentMessage);
			AssertEquals(PaymentStatusList.Codes.PaymentAuthorizationDeleted, statementHeader.B2_PaymentStatus);
			AssertEquals(ZDateTime.Empty, statementHeader.B2_PaymentAuthorizationDate);
			AssertEquals(ZString.Empty, statementHeader.B2_AccountNo);
			AssertEquals(ZString.Empty, statementHeader.B2_PaymentParty);
		}

		protected override string applicationIdentifier
		{
			get { return ApplicationIdentifierCodeList.Codes.PeriodicDailyStatementACHDebitAuthorizationEntrySummaryPresentationResponse; }
		}

		protected override string responseSubject => "ACH Debit Authorization";

		protected override string resposeBlockIdentifier
		{
			get { return "PU"; }
		}

		protected override ManifestGroupNotificationRegistryItem StatementGroup
		{
			get { return USCustomsDataRegistry.Instance.ABIMessagesGroup; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
			USCustomsDataRegistry.Instance.PeriodicMonthlyStatementsMessagesGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				new GroupNotification(Core.Constants.EmailTo.NoEmails, ZGuid.Empty));
		}
	}
}
