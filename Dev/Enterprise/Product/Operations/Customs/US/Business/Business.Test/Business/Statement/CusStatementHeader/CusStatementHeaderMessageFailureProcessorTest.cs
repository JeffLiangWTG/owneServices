using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageProcessors;
using Enterprise.Customs.US.Business.MessageProcessors.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusStatementHeaderMessageFailureProcessorTest : MessageFailureProcessorTest<AllMessageFailureProcessor, APLA, APLB, APLY>
	{
		protected override void EndToEndCore()
		{
			CusStatementHeader statement = Factory.New<CusStatementHeader>();
			statement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			ProcessAndAssert(ApplicationIdentifierCodeList.Codes.StatementDeleteTransaction, ApplicationIdentifierCodeList.Descriptions.StatementDeleteTransaction, delegate(MQEDIMessage sendigMessage)
			{
				sendigMessage.EM_LinkedObject = statement;
			});
			statement.Reload();
			AssertEquals(StatementHeaderStatusList.Codes.Preliminary, statement.B2_Status);
		}

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest
		{
			get { return 1; }
		}
	}
}
