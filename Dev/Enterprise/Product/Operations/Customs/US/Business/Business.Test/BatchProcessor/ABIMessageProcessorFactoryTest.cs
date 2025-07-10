using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.Business.Testing
{
	public class ABIMessageProcessorFactoryTest : TestCaseWithFactory
	{
		public void TestProcessACEMessageWhereBDoesNotHaveApplicationID()
		{
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_MessageText = "B00                                                        B                    X0 BLOCK       1 REF ID: 8888 SV9    AE 6007772                                 X1 FX17   FILER NOT AUTHORIZED                                                  X1RF999   BATCH REJECTED                                                        Y           00003";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;// This is set when interchanges are processed
			message.EM_Status = MQEDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(message);
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
		}

		public void TestProcessingUnknownMessage()
		{
			DeclarationTestHelper.SetupForSendMessage();
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_MessageText = new DeclarationTestHelper(Factory).GetUnknownMessage();
			message.EM_MessageType = "XX";
			message.EM_Status = MQEDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			ErrorReporter.Clear();
			new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(message);
			AssertEquals(MQEDIMessage.Status.Failed, message.EM_Status);
			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == "Message Response (Failure)"));
			AssertEquals(3, email.Attachments.Count);
		}

		public void TestReferenceFileMessagesAreExcluded()
		{
			var b = new APLB();
			b.UserData = MQEDIMessage.MessageNumberPlaceHolder;

			var message1 = Factory.New<MQEDIMessage>();
			message1.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ADCVDCaseInformationQueryResponse;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var message2 = Factory.New<MQEDIMessage>();
			message2.EM_MessageType = ApplicationIdentifierCodeList.Codes.AntidumpingCountervailingDutyQueryResponse;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var message3 = Factory.New<MQEDIMessage>();
			message3.EM_MessageType = ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse;
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var message6 = Factory.New<MQEDIMessage>();
			message6.EM_MessageType = ApplicationIdentifierCodeList.Codes.HarmonizedSystemUpdate;
			message6.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var message7 = Factory.New<MQEDIMessage>();
			message7.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem;
			message7.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var message8 = Factory.New<MQEDIMessage>();
			message8.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryQuotaResponse;
			message8.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var entrySummaryMessage = Factory.New<MQEDIMessage>();
			entrySummaryMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			entrySummaryMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entrySummaryMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryEntrySummaryResponse;
			entrySummaryMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryQuery;
			entrySummaryMessage.EM_MessageNum = "";
			entrySummaryMessage.EM_MessageText = "B018888XJ5JR                                               58                   " +
				"J18888XJ5 0000011391-013199000010000000040000000000000                  001   B " +
				"J2XJ5 00000113      0000000000000000000000                               808001B" +
				"J3XJ5 00000113112607071126070112546821485211210789130010715000000002400B00001011" +
				"J5XJ5 00000113071120ESP                                                         " +
				"J98888XJ5 00000113BILLING DATA NOT ON FILE                                      " +
				"J98888XJ5 00000113COLLECTION DATA NOT ON FILE                                   " +
				"Y  8888XJ5JR00006";
			entrySummaryMessage.EM_Status = MQEDIMessage.Status.Queued;
			var filter = new ABIMessageProcessorFactory(new LoggingInformation()).MessageFilter;
			AssertEquals("ADCVDCaseInformationQueryResponse Message should not match", false, message1.MatchesFilter(filter));
			AssertEquals("AntidumpingCountervailingDutyQueryResponse Message should not match", false, message2.MatchesFilter(filter));
			AssertEquals("ExtractReferenceFilesResponse Message should not match", false, message3.MatchesFilter(filter));
			AssertEquals("HarmonizedSystemUpdate Message should not match", false, message6.MatchesFilter(filter));
			AssertEquals("QueryHarmonizedSystem Message should not match", false, message7.MatchesFilter(filter));
			AssertEquals("QueryQuotaResponse Message should not match", false, message8.MatchesFilter(filter));
			AssertEquals("EntrySummary Message should match", true, entrySummaryMessage.MatchesFilter(filter));
		}

		public void TestMessageTypesToExclude()
		{
			var expectedList = new[]
			{
				ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse,
				ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingStatusAdvisory,
				ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling,
				ACEApplicationIdentifierCodeList.Codes.ADCVDCaseInformationQueryResponse,
				ApplicationIdentifierCodeList.Codes.AntidumpingCountervailingDutyQueryResponse,
				ApplicationIdentifierCodeList.Codes.ExtractADDCVDCaseFileResponse,
				ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse,
				ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse,
				ApplicationIdentifierCodeList.Codes.HarmonizedSystemUpdate,
				ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem,
				ApplicationIdentifierCodeList.Codes.QueryQuotaResponse,
				ACEApplicationIdentifierCodeList.Codes.QuotaQueryResponse,
				ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse,
				ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQueryResponse,
				ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation,
				ACEApplicationIdentifierCodeList.Codes.DailyStatement,
				ApplicationIdentifierCodeList.Codes.PeriodicDailyStatementACHDebitAuthorizationEntrySummaryPresentationResponse,
				ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement
			};
			var actualList = new ABIMessageProcessorFactory(new LoggingInformation()).MessageTypesToExclude;
			AssertEquals(expectedList.Length, actualList.Count);
			foreach (var type in expectedList)
			{
				AssertCollectionContains(type, actualList);
			}
		}

		public void TestISFMessagesAreExcluded()
		{
			var b = new APLB();
			b.UserData = MQEDIMessage.MessageNumberPlaceHolder;
			var messageText = b.Serialise() + new ISFSF90() { ErrorCode = "789" }.Serialise() + " B";

			var message1 = Factory.New<MQEDIMessage>();
			message1.EM_MessageText = messageText;
			message1.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var message2 = Factory.New<MQEDIMessage>();
			message2.EM_MessageText = messageText;
			message2.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingStatusAdvisory;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var message3 = Factory.New<MQEDIMessage>();
			message3.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message3.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryEntrySummaryResponse;
			message3.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryQuery;
			message3.EM_MessageNum = "";
			message3.EM_MessageText = "B018888XJ5JR                                               58                   " +
				"J18888XJ5 0000011391-013199000010000000040000000000000                  001   B " +
				"J2XJ5 00000113      0000000000000000000000                               808001B" +
				"J3XJ5 00000113112607071126070112546821485211210789130010715000000002400B00001011" +
				"J5XJ5 00000113071120ESP                                                         " +
				"J98888XJ5 00000113BILLING DATA NOT ON FILE                                      " +
				"J98888XJ5 00000113COLLECTION DATA NOT ON FILE                                   " +
				"Y  8888XJ5JR00006";
			message3.EM_Status = MQEDIMessage.Status.Queued;
			var filter = new ABIMessageProcessorFactory(new LoggingInformation()).MessageFilter;
			AssertEquals("ISF Message should not match", false, message1.MatchesFilter(filter));
			AssertEquals("ISF Message should not match", false, message2.MatchesFilter(filter));
			AssertEquals("Non ISF Message should match", true, message3.MatchesFilter(filter));
		}

		public MessageProcessorFactory GetFactoryInstance()
		{
			return new ABIMessageProcessorFactory(new LoggingInformation());
		}
	}
}
