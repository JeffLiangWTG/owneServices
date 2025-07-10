using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class ReconEntryFeeIReconciliationImportEntryFeeTest : TestCaseWithFactory
	{
		public void TestFees()
		{
			OriginalEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Duty, 1m);
			OriginalEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 15m);

			OriginalEntry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Duty, 1m);
			OriginalEntry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Avocado, 10m);
			IReconciliationImportEntryFee iFee = new ReconEntryFeeIReconciliationImportEntryFee()
			{
				FeeType = Core.Constants.USCustoms.FeeCodes.Avocado,
				OriginalFee = OriginalEntry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Avocado),
				ReconFee = OriginalEntry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Avocado)
			};
			AssertEquals("OriginalFee", 0m, iFee.OriginalFee);
			AssertEquals("ReconFee", 10m, iFee.EstimatedReconciliationFee);
		}

		ReconOriginalEntryHeader originalEntry;
		ReconOriginalEntryHeader OriginalEntry => originalEntry ?? (originalEntry = new ReconDeclaration(Factory.New<JobDeclaration>()).OriginalEntries.AddNew());
	}
}
