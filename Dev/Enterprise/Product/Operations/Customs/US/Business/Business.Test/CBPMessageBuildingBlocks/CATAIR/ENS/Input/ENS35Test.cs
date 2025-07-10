using System;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class ENS35Test : BIRDHeaderUpdateTest
	{
		protected override IBIRDHeaderRecord[] GetPopulatedHeaderRecords()
		{
			ENS35 ens35 = new ENS35();

			ens35.BondedADDIndicator = "0";
			ens35.BondedCVDIndicator = "0";
			ens35.ADDCVDSuretyCode = "081";

			return new IBIRDHeaderRecord[] { ens35 };
		}

		protected override Type GetTypeOfMessageBlock() => typeof(ENS35);

		protected override void PrepareDeclaration(JobDeclaration declaration, IBIRDHeaderRecord headerRecord)
		{
			base.PrepareDeclaration(declaration, headerRecord);

			declaration.InvoiceLines[0].US_ADDCaseNo = "A";
		}

		protected override string[] GetFieldNameToExcludeForTesting()
		{
			return new string[]
			{
				"BondedADDDuty",//This is a total amount and line level duty is stored
				"PayableADDDuty",//This is a total amount and line level duty is stored
				"BondedCVDDuty",//This is a total amount and line level duty is stored
				"PayableCVDDuty",//This is a total amount and line level duty is stored
			};
		}

		protected override MessageBuilders.EntryHeaderMessageBuilder<ABIInputBlockControlGenerator> GetMessageBuilder(JobDeclaration declaration, IBIRDHeaderRecord headerRecord)
		{
			return new MessageBuilders.EntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
		}
	}
}
