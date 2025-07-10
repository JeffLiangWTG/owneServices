using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconInterestDataProviderReconOriginalEntryTest : TestCaseWithFactory
	{
		public void TestIReconInterestDataProvider()
		{
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_PreliminaryStatementPrintDate = new ZDate(1999, 9, 15);

			ReconOriginalEntryHeader entry = reconDeclaration.OriginalEntries.AddNew();
			entry.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4000m);
			entry.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4300m);

			entry.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.FreshLimes, 15.25m);

			entry.US_PaymentDate = new ZDate(1999, 1, 5);

			ReconInterestDataProviderReconOriginalEntry dataProvider = new ReconInterestDataProviderReconOriginalEntry(entry);

			AssertEquals(true, dataProvider.HasBeenUnderPaid);
			AssertEquals("OriginalPaymentDate", new ZDate(1999, 1, 5), dataProvider.OriginalPaymentDate);
			AssertEquals("OriginalPayable", 4000m, dataProvider.OriginalPayable);

			AssertEquals("ReconPayable", 4315.25m, dataProvider.ReconPayable);
			AssertEquals("ReconPaymentDate", new ZDate(1999, 9, 15), dataProvider.ReconPaymentDate);

			dataProvider.UpdateOrAddInterestCharge(30m);
			AssertEquals("Interest", 30m, entry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest));
		}
	}
}
