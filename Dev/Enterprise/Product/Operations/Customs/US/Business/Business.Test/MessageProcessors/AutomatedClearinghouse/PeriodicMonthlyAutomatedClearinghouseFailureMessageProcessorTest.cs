using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Registry.Business.Customs;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	sealed class PeriodicMonthlyAutomatedClearinghouseFailureMessageProcessorTest : AutomatedClearinghouseFailureMessageProcessorTest<ACSPeriodicMonthlyAutomatedClearinghouseFailureMessageProcessor, APLA, APLB, APLY>
	{
		public void TestProcessMessageOnPZFilerNotAuthorized()
		{
			var statementHeader = Factory.New<CusStatementHeader>();
			statementHeader.B2_EntryFilerCode = "SV9";
			statementHeader.B2_StatementNumber = "3919186000";

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var paymentMessage = mock.Object;
			paymentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			paymentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			paymentMessage.EM_MessageNum = "WOCLAXLAX_2282787";
			paymentMessage.EM_MessageText = "B  3001906RM                                               WOCLAXLAX_2282787     PT00096401906 30222340AW0000029667                                              Y  3001906RM";
			paymentMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ACHDebitAuthorizationEntrySummaryPresentation;
			paymentMessage.EM_LinkedObject = statementHeader;

			var incomingPaymentMessage = Factory.New<MQEDIMessage>();
			incomingPaymentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingPaymentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingPaymentMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.PeriodicDailyStatementACHDebitAuthorizationEntrySummaryPresentationResponse;
			incomingPaymentMessage.EM_Status = EDIMessage.Status.Queued;
			incomingPaymentMessage.EM_MessageNum = "WOCLAXLAX_2282787";
			incomingPaymentMessage.EM_MessageText = "B00                                                        B                    X0 BLOCK  000001 REF ID: 3001 906    RM WOCLAXLAX_2282787                       X1 FX17   FILER NOT AUTHORIZED                                                  X1RF999   BATCH REJECTED                                                        Y           00003";
			Factory.Save();

			new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(incomingPaymentMessage);
			AssertEquals("B2_PaymentStatus", PaymentStatusList.Codes.PaymentFailed, statementHeader.B2_PaymentStatus);
			AssertEquals("EM_Status", EDIMessage.Status.Received, incomingPaymentMessage.EM_Status);
		}

		protected override string ACSapplicationIdentifier
		{
			get
			{
				return ApplicationIdentifierCodeList.Codes.PeriodicDailyStatementACHDebitAuthorizationEntrySummaryPresentation;
			}
		}

		protected override string ACEapplicationIdentifier
		{
			get
			{
				return ACEApplicationIdentifierCodeList.Codes.ACHDebitAuthorizationEntrySummaryPresentation;
			}
		}
		protected override string ResponseSubject => "ACH Debit Authorization";

		protected override ManifestGroupNotificationRegistryItem StatementGroup
		{
			get { return USCustomsDataRegistry.Instance.ABIMessagesGroup; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			USCustomsDataRegistry.Instance.PeriodicMonthlyStatementsMessagesGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				new GroupNotification(Core.Constants.EmailTo.NoEmails, ZGuid.Empty));
		}
	}
}
