using System;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class ENS21Test : BIRDHeaderUpdateTest
	{
		protected override IBIRDHeaderRecord[] GetPopulatedHeaderRecords()
		{
			ENS21 result = new ENS21();

			result.BondAmount = 50000m;
			result.BondProducerAccountNumber = "123456";

			return new IBIRDHeaderRecord[] { result };
		}

		protected override void PrepareDeclaration(JobDeclaration declaration, IBIRDHeaderRecord headerRecord)
		{
			base.PrepareDeclaration(declaration, headerRecord);
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
		}

		protected override Type GetTypeOfMessageBlock() => typeof(ENS21);

		protected override MessageBuilders.EntryHeaderMessageBuilder<ABIInputBlockControlGenerator> GetMessageBuilder(JobDeclaration declaration, IBIRDHeaderRecord headerRecord)
		{
			return new MessageBuilders.EntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
		}
	}
}
