using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class ACEQuotaQueryProcessorTest : ABIProcessorTest<ACEQuotaQueryProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		protected override void EndToEndCore()
		{
			incomingMessage.EM_MessageText =
"B  3901SV9QB                                               HYEDUSCMT_165024     " +
"Q2R90013000002212345        IT           PCS016007071315010117000007000150600   " +
"Q399EDFILLQUOTA FOR TARIFF                        TRQ 000080000000019900123000  " +
"Q40719150721151526                                                              " +
"Q2R01060050808850016        FR       1700PCS250004071315010117000007000026400   " +
"Q399PDOPENQUOTA FOR SILK BLEND                    STA 00008000000002            " +
"Q40719150721151526                                                              " +
"Y  3901SV9QB";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new USRIncomingMessageProcessor().ExecuteBatch();
			incomingMessage.Reload();

			AssertEquals("Should be processed successfully", "RCV", incomingMessage.EM_Status);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("Tariff Number: 9001.30.0000", email.Body);
			AssertContains("Tariff Number: 0106.00.5080", email.Body);

			var quotas = Factory.Load<USCQuota>(new ZQuery(USCQuotaSchema.UT_Code, "9001300000"));
			AssertEquals("There should be only one quota", 1, quotas.Length);

			var quota = quotas[0];

			AssertEquals("Quota Type", QuotaTypeList.Codes.TariffNumber, quota.UT_QuotaType);
			AssertEquals("Quota Limit", 0m, quota.UT_QuotaLimit);
			AssertEquals("UQ", "PCS", quota.UT_QuotaUQ);
			AssertEquals("Textile Conversion Factory", 16.007m, quota.UT_TextileConversionFactor);
			AssertEquals("Quota Period End", "07", quota.UT_QuotaPeriod);
			AssertEquals("Threshold Qty", 150600m, quota.UT_ThresholdQty);
			AssertEquals("Period Processing Date Indicator", PeriodProcessingDateIndicatorList.Codes.ExportDate, quota.UT_PeriodProcessDateIndicator);
			AssertEquals("Quota Status", "FIL", quota.UT_QuotaStatus);
			AssertEquals("Quota Limit Type", "TRQ", quota.UT_QuotaLimitType);
			AssertEquals("Qty to date", 800000m, quota.UT_QtyToDate);
			AssertEquals("Last Transaction Date", new ZDateTime(2015, 07, 19), quota.UT_LastTrasactionDate);
			AssertEquals("Last Update Date", new ZDateTime(2015, 07, 21, 15, 26, 0), quota.UT_LastUpdateDate);

			incomingMessage.EM_Status = "QUE";
			Factory.Save();

			new USRIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();
			incomingMessage.Reload();
			AssertEquals(declaration, incomingMessage.EM_LinkedObject);

			quotas = Factory.Load<USCQuota>(new ZQuery(USCQuotaSchema.UT_Code, "9001300000"));
			AssertEquals("There should be only one quota", 1, quotas.Length);

			quotas = Factory.Load<USCQuota>(new ZQuery(USCQuotaSchema.UT_Code, "0106005080"));
			AssertEquals("There should be only one quota", 1, quotas.Length);
		}

		public void TestFirstNamesakeAdjustment()
		{
			incomingMessage.EM_MessageText =
"B  3901SV9QB                                               HYEDUSCMT_165024     " +
"Q2R123       1234567890X123 IT           PCS016007071315010117000007000150600   " +
"Q399EDFILLQUOTA FOR TARIFF                        TRQ 000080000000019900123000  " +
"Q40719150721151526                                                              " +
"Q2R01060050808850016        FR       1700PCS250004071315010117000007000026400   " +
"Q399PDOPENQUOTA FOR SILK BLEND                    STA 00008000000002            " +
"Q40719150721151526                                                              " +
"Y  3901SV9QB";
			Factory.Save();

			AssertNoExceptionThrown(() =>
			{
				new USRIncomingMessageProcessor().ExecuteBatch();
			});
			incomingMessage.Reload();
			var quotas = Factory.LoadTop1<USCQuota>(new ZQuery(USCQuotaSchema.UT_Code, "123"));
			AssertNotNull(quotas);
			AssertEquals("UT_FirstNamesake should be correct.", quotas.UT_FirstNamesake, "1234567890X123");

			AssertEquals("BeginDate", new ZDateTime(2015, 07, 13), quotas.UT_BeginDate);
			AssertEquals("EndDate", new ZDateTime(2017, 01, 01), quotas.UT_EndDate);
		}

		public void TestEndDateAdjustmentOnQuota()
		{
			incomingMessage.EM_MessageText =
"B  3901SV9QB                                               HYEDUSCMT_165025     " +
"Q2X556677    2233445        FR           PCS250004071315123139000007000150600   " +
"Q399PDPOTFSOME DESCRIPTION FOR QUOTA                  000080000000019900123000  " +
"Q40719150721151526                                                              " +
"Q2X889966    8850016        FR       1700PCS250004071315010117000007000026400   " +
"Q399PDOPENQUOTA FOR SILK BLEND                    STA 000080000000029900123000  " +
"Q40719150721151526                                                              " +
"Y  3901SV9QB";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new USRIncomingMessageProcessor().ExecuteBatch();
			incomingMessage.Reload();
			AssertEquals("Should be processed successfully", "RCV", incomingMessage.EM_Status);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("Textile Category Number: 556677", email.Body);
			AssertContains("Textile Category Number: 889966", email.Body);

			var quotas = new BusinessObjectFactory().Load<USCQuota>(new ZQuery(USCQuotaSchema.UT_Code, "556677"));
			AssertEquals("There should be only one quota", 1, quotas.Length);

			var quota = quotas[0];
			AssertEquals("BeginDate", new ZDateTime(2015, 07, 13), quota.UT_BeginDate);
			AssertEquals("EndDate", new ZDateTime(2039, 12, 31), quota.UT_EndDate);
		}

		public void TestResponseForTextileNumberQuery()
		{
			incomingMessage.EM_MessageText =
"B  3901SV9QB                                               HYEDUSCMT_165024     " +
"Q2X123       2212345        FR           PCS250004071315010117000007000150600   " +
"Q399PDPOTFSOME DESCRIPTION FOR QUOTA                  000080000000019900123000  " +
"Q40719150721151526                                                              " +
"Q2X809       8850016        FR       1700PCS250004071315010117000007000026400   " +
"Q399PDOPENQUOTA FOR SILK BLEND                    STA 000080000000029900123000  " +
"Q40719150721151526                                                              " +
"Y  3901SV9QB";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new USRIncomingMessageProcessor().ExecuteBatch();
			incomingMessage.Reload();
			AssertEquals("Should be processed successfully", "RCV", incomingMessage.EM_Status);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("Textile Category Number: 123", email.Body);
			AssertContains("Textile Category Number: 809", email.Body);

			var quotas = new BusinessObjectFactory().Load<USCQuota>(new ZQuery(USCQuotaSchema.UT_Code, "123"));
			AssertEquals("There should be only one quota", 1, quotas.Length);

			var quota = quotas[0];
			AssertEquals("Quota Type", QuotaTypeList.Codes.TextileCategoryNumber, quota.UT_QuotaType);
			AssertEquals("Quota Limit", 0m, quota.UT_QuotaLimit);
			AssertEquals("UQ", "PCS", quota.UT_QuotaUQ);
			AssertEquals("Textile Conversion Factory", 250.004m, quota.UT_TextileConversionFactor);
			AssertEquals("Quota Period End", "07", quota.UT_QuotaPeriod);
			AssertEquals("Threshold Qty", 150600m, quota.UT_ThresholdQty);
			AssertEquals("Period Processing Date Indicator", PeriodProcessingDateIndicatorList.Codes.PresentationDate, quota.UT_PeriodProcessDateIndicator);
			AssertEquals("Quota Status", QuotaStatusList.Codes.QuotaPotentiallyFilled, quota.UT_QuotaStatus);
			AssertEquals("Quota Type", "", quota.UT_QuotaLimitType);
			AssertEquals("Qty to date", 800000m, quota.UT_QtyToDate);
			AssertEquals("Last Transaction Date", new ZDateTime(2015, 07, 19), quota.UT_LastTrasactionDate);
			AssertEquals("Last Update Date", new ZDateTime(2015, 07, 21, 15, 26, 00), quota.UT_LastUpdateDate);

			incomingMessage.EM_Status = "QUE";
			Factory.Save();

			new USRIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			quotas = new BusinessObjectFactory().Load<USCQuota>(new ZQuery(USCQuotaSchema.UT_Code, "123"));
			AssertEquals("There should be only one quota", 1, quotas.Length);

			quotas = new BusinessObjectFactory().Load<USCQuota>(new ZQuery(USCQuotaSchema.UT_Code, "809"));
			AssertEquals("There should be only one quota", 1, quotas.Length);
		}

		public void TestFailedResponse()
		{
			incomingMessage.EM_MessageText =
"B  3901SV9QB                                               HYEDUSCMT_165024     " +
"Q5X123       FR  Q44  ERROR QUERY CANNOT BE PROCESSED                           " +
"Y  3901SV9QB";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new USRIncomingMessageProcessor().ExecuteBatch();
			incomingMessage.Reload();
			AssertEquals("Should be processed successfully", "RCV", incomingMessage.EM_Status);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("Textile Category Number: 123", email.Body);
			AssertContains("<td>ERROR QUERY CANNOT BE PROCESSED</td>", email.Body);

			var quotas = new BusinessObjectFactory().Load<USCQuota>(new ZQuery(USCQuotaSchema.UT_Code, "123"));
			AssertEquals("There should be no quota created", 0, quotas.Length);
		}

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest
		{
			get { return 2; }
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			outgoing = mock.Object;
			outgoing.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoing.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoing.EM_Status = "SNT";
			outgoing.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.QuotaQuery;
			outgoing.EM_MessageNum = "HYEDUSCMT_165024";
			declaration.Messages.Add(outgoing);

			incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.QuotaQueryResponse;
			incomingMessage.EM_MessageNum = "HYEDUSCMT_165024";
		}

		MQEDIMessage incomingMessage;
		MQEDIMessage outgoing;
		JobDeclaration declaration;
	}
}
