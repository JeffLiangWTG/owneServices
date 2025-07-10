using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.MessageProcessors;
using Enterprise.Customs.US.Business.MessageProcessors.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusEntryHeaderMessageFailureProcessorTest : MessageFailureProcessorTest<AllMessageFailureProcessor, APLA, APLB, APLY>
	{
		public void TestStatementDeleteTransactionFailure()
		{
			ProcessAndAssert(ApplicationIdentifierCodeList.Codes.StatementDeleteTransaction, ApplicationIdentifierCodeList.Descriptions.StatementDeleteTransaction, ZString.Empty, ZString.Empty);
		}

		public void TestInbondTransactionFailure()
		{
			ProcessAndAssert(ACEApplicationIdentifierCodeList.Codes.InbondTransaction, ACEApplicationIdentifierCodeList.Descriptions.InbondTransaction, ImportMessageStatusList.Codes.ErrorDepartureOriginal, EM_MessageSubTypeList.Codes.InBondDepartureOriginal, true);
		}

		public void TestConsigneeNameAddressAddFailure()
		{
			ProcessAndAssert(ApplicationIdentifierCodeList.Codes.ConsigneeNameAddressAdd, ApplicationIdentifierCodeList.Descriptions.ConsigneeNameAddressAdd, ImportMessageStatusList.Codes.ErrorConsigneeNameAddressAdd, ZString.Empty);
		}

		public void TestCargoReleaseTransactionsFailure()
		{
			ProcessAndAssert(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions, ApplicationIdentifierCodeList.Descriptions.CargoReleaseTransactions, ImportMessageStatusList.Codes.ErrorCargoReleaseOriginal, EM_MessageSubTypeList.Codes.CargoReleaseAdd);
		}

		protected override void EndToEndCore()
		{
			ProcessAndAssert(ApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQuery, ApplicationIdentifierCodeList.Descriptions.HarmonizedTariffScheduleQuery, ZString.Empty, ZString.Empty);
		}

		void ProcessAndAssert(string applicationIdentifier, string subject, string messageStatus, string messageSubType, bool isACE = false)
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(ZString.Empty, entry.CH_Status);
			ProcessAndAssert(applicationIdentifier, subject, (MQEDIMessage sendigMessage) =>
			{
				entry.Messages.Add(sendigMessage);
				sendigMessage.EM_MessageSubType = messageSubType;
			}, isACE);
			entry.Reload();
			AssertEquals(messageStatus, entry.CH_Status);
		}
	}
}
