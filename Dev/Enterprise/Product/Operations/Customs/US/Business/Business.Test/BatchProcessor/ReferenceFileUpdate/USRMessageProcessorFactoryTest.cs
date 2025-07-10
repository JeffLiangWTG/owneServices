using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.Environment;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USRMessageProcessorFactoryTest : TestCaseWithFactory
	{
		public void TestAllHeldUntilDatesAreUpdatedAfterLockException()
		{
			var message1 = GetADCVDMessage("MSG_001");
			var message2 = GetADCVDMessage("MSG_002");
			var message3 = GetADCVDMessage("MSG_003");
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var processor = new USRIncomingMessageProcessorForLockException();
			processor.ExecuteBatch();

			var factory2 = new BusinessObjectFactory();
			var message1_f2 = factory2.Load<MQEDIMessage>(message1.PK);
			var message2_f2 = factory2.Load<MQEDIMessage>(message2.PK);
			var message3_f2 = factory2.Load<MQEDIMessage>(message3.PK);

			AssertMessageHasHeldUntilDateSet(message1_f2);
			AssertMessageHasHeldUntilDateSet(message2_f2);
			AssertMessageHasHeldUntilDateSet(message3_f2);

			AssertEquals(MQEDIMessage.Status.Queued, message1_f2.EM_Status);
			AssertEquals(MQEDIMessage.Status.Queued, message2_f2.EM_Status);
			AssertEquals(MQEDIMessage.Status.Queued, message3_f2.EM_Status);

			AssertLogs(processor.Logger, ExpectedLogText);
		}

		public void TestMessageTypesToInclude()
		{
			var expectedList = new[]
			{
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
			var actualList = new USRMessageProcessorFactory(new LoggingInformation()).MessageTypesToInclude;
			AssertEquals(expectedList.Length, actualList.Count);
			foreach (var type in expectedList)
			{
				AssertCollectionContains(type, actualList);
			}
		}

		public void TestProcessingGoodMessage()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_MessageText = "B  8888XJ5FO                                               8862                 " +
			"F2111501L177A04YELLOW FREIGHT LINES               042689                        " +
			"F311PO BOX 1801                        WILMINGTON                         NC    " +
			"F41128402    US                                                                 " +
			"F2112402S369A08U S CUSTOMS DISTRICT OFFICE        050189                        " +
			"F311BRIDGE OF THE AMERICAS             EL PASO                            TX    " +
			"F41179905    US                                                                 " +
			"Y  8888XJ5FR06720                                                               ";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			new USRMessageProcessorFactory(new LoggingInformation()).ProcessMessage(message);
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse;
			message.EM_MessageText = "B003910SV9HZ                                               HYEDUSCMT_175424     F110161200000000009999999999NOT ON FILE OR EXPIRED                              Y  3910SV9HZ00000                                                               ";
			message.EM_Status = MQEDIMessage.Status.Queued;
			new USRMessageProcessorFactory(new LoggingInformation()).ProcessMessage(message);
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
		}

		public void TestMultipleInnerMessagesAreProcessed()
		{
			var generator = OutputBlockControlGeneratorTestHelper.GetBlockControlGenerator(ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse,
	"F1100601",
	"V10204422030R0801059999991KG       1SCOTT,BONE IN, FROZEN, LOIN   000000700000",
	"V20204422030000000000000000000000000000015400000000000000000000000000000",
	"V30204422030                    AP2FD3FS2           D E J A+AUCACLILJOMAMXSG",
	"V10303802000R0101061231061KG       7STURGEON ROE, FROZEN          000000000000",
	"V20303802000000015000000000000000000000000000000000030000000000000000000",
	"V30303802000                    FD4FW1              A E J AUCACLILJOMAMXSG",
	"V50303802000SG000000000000000009300000000000000000");
			var message = Factory.New<MQEDIMessage>();
			message.EM_MessageText = generator.Serialise();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			new USRMessageProcessorFactory(new LoggingInformation()).ProcessMessage(message);
			Factory.Save();

			var query = new ZQuery(USCTariffSchema.UE_Tariff, "0204422030");
			query.AddToFilter(USCTariffSchema.UE_DateFrom, new ZDateTime(2005, 08, 01));
			var tariff = Factory.LoadTop1<USCTariff>(query);
			AssertNotNull("Tariff could not be found", tariff);
			AssertEquals("SCOTT,BONE IN, FROZEN, LOIN", tariff.UE_ShortDescription);
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
			message3.EM_MessageType = ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse;
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var filter = new USRMessageProcessorFactory(new LoggingInformation()).MessageFilter;
			AssertEquals("ISF Message should not match", false, message1.MatchesFilter(filter));
			AssertEquals("ISF Message should not match", false, message2.MatchesFilter(filter));
			AssertEquals("Non ISF Message should match", true, message3.MatchesFilter(filter));
		}

		string ExpectedLogText => @"
Processing Message #MSG_001
US reference database needs to be updated but could not acquire lock for 'ACEAntiDumping', skipping until data is available for update";

		void AssertLogs(LoggingInformation logger, string expectedResult)
		{
			var actualResults = new ZStringBuilder();
			foreach (string line in logger.UserLogStrings)
			{
				if (line.Length > 1)
				{
					actualResults.Append(line.Substring(1));
				}
			}

			var actualResult = actualResults.ToStringWithNewLineBetweenAppends();
			AssertMultilineASCIIEquals("Debug Log from MessageProcessor", expectedResult.Trim(), actualResult);
			logger.ClearLogs();
		}

		void AssertMessageHasHeldUntilDateSet(MQEDIMessage message)
		{
			AssertGreaterThan("UtcNow.AddMinutes(2)" + message.EM_MessageNum, message.EM_HeldUntilDate, ZDateTime.UtcNow);
			AssertLessThan("UtcNow.AddMinutes(2)" + message.EM_MessageNum, message.EM_HeldUntilDate, ZDateTime.UtcNow.AddMinutes(5));
		}

		MQEDIMessage GetADCVDMessage(ZString messageNumber)
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_Status = MQEDIMessage.Status.Queued;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ADCVDCaseInformationQueryResponse;
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_MessageNum = messageNumber;
			message.EM_MessageText = "Some Random Text";
			message.EM_HeldUntilDate = ZDateTime.UtcNow.AddHours(-5);

			return message;
		}

		sealed class USRIncomingMessageProcessorForLockException : USRIncomingMessageProcessor
		{
			public USRIncomingMessageProcessorForLockException(bool generateException = true)
				: base()
			{
				this.generateException = generateException;
			}

			readonly bool generateException;

			protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
			{
				return new List<ApplicationTypeMessageProcessor>() { new USRMessageProcessorFactoryForLockException(Logger, generateException) };
			}
		}

		sealed class USRMessageProcessorFactoryForLockException : USRMessageProcessorFactory
		{
			public USRMessageProcessorFactoryForLockException(LoggingInformation logger, bool generateException = true)
				: base(logger)
			{
				this.generateException = generateException;
			}

			readonly bool generateException;

			protected override void ProcessMessageCore(Enterprise.Messaging.Business.EDIMessage message)
			{
				//message.EM_Status = MQEDIMessage.Status.ProcessedOK; // indicates message was touched during testing. Not what would actually happen.

				if (generateException)
				{
					var lockType = CargoWise.Definitions.Customs.US.ReferenceLockType.ACEAntiDumping;
					throw new Enterprise.Messaging.Business.MessageProcessLockException(string.Format("US reference database needs to be updated but could not acquire lock for '{0}', skipping until data is available for update", lockType));
				}

				message.EM_Status = MQEDIMessage.Status.ProcessedOK;
			}
		}
	}
}
