using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageProcessors;
using Enterprise.Customs.US.Business.MessageProcessors.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEJobDeclarationMessageFailureProcessorTest : MessageFailureProcessorTest<AllACEMessageFailureProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		protected override void EndToEndCore()
		{
			ProcessAndAssert(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQuery, ACEApplicationIdentifierCodeList.Descriptions.HarmonizedTariffScheduleExtractReferenceQuery, ZString.Empty, ZString.Empty);
		}

		public void TestACEHarmonizedTariffScheduleQueryFailure()
		{
			ProcessAndAssert(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQuery, ACEApplicationIdentifierCodeList.Descriptions.HarmonizedTariffScheduleQueryTransactionQuery, ZString.Empty, ZString.Empty);
		}

		public void TestACEDrawbackSummaryFailure_Original()
		{
			ProcessAndAssert(ACEApplicationIdentifierCodeList.Codes.DrawbackEntrySummaryQuery, ACEApplicationIdentifierCodeList.Descriptions.DrawbackEntrySummaryQuery, DrawbackSummaryStatusList.Codes.ErrorDrawbackSummaryOriginal, EM_MessageSubTypeList.Codes.DrawbackSummaryOriginal);
		}

		public void TestACEDrawbackSummaryFailure_Replacement()
		{
			ProcessAndAssert(ACEApplicationIdentifierCodeList.Codes.DrawbackEntrySummaryQuery, ACEApplicationIdentifierCodeList.Descriptions.DrawbackEntrySummaryQuery, DrawbackSummaryStatusList.Codes.ErrorDrawbackSummaryReplacement, EM_MessageSubTypeList.Codes.DrawbackSummaryReplacement);
		}

		void ProcessAndAssert(string applicationIdentifier, string subject, string messageStatus, string messageSubType)
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			AssertEquals(ZString.Empty, declaration.JE_MessageStatus);
			ProcessAndAssert(applicationIdentifier, subject, (MQEDIMessage sendigMessage) =>
			{
				declaration.Messages.Add(sendigMessage);
				sendigMessage.EM_MessageSubType = messageSubType;
			}, true);
			declaration.Reload();
			AssertEquals(messageStatus, declaration.JE_MessageStatus);
		}
	}
}
