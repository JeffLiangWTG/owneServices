using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconAggregateInterestDataProviderTest : TestCaseWithFactory
	{
		public void TestCalculateOnAggregate()
		{
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_IsAggregate = true;
			reconDeclaration.US_PreliminaryStatementPrintDate = new ZDate(1999, 9, 15);

			ReconOriginalEntryHeader entry1 = reconDeclaration.OriginalEntries.AddNew();
			entry1.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4000m);
			entry1.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4300m);
			entry1.US_PaymentDate = new ZDate(1999, 1, 5);

			ReconOriginalEntryHeader entry2 = reconDeclaration.OriginalEntries.AddNew();
			entry2.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4000m);
			entry2.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4670m);
			entry2.US_PaymentDate = new ZDate(1999, 4, 12);

			ReconOriginalEntryHeader entry3 = reconDeclaration.OriginalEntries.AddNew();
			entry3.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4000m);
			entry3.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4600m);
			entry3.US_PaymentDate = new ZDate(1999, 5, 28);

			ReconInterestDataProviderReconDec provider = new ReconInterestDataProviderReconDec(reconDeclaration);
			AssertEquals("IsAggregate", true, provider.IsAggregate);
			AssertEquals("HasBeenUnderPaid", true, provider.HasBeenUnderPaid);

			List<IReconInterestDataProvider> data = new List<IReconInterestDataProvider>(provider.ReconInterestData);
			AssertEquals(3, data.Count);

			AssertEquals("OriginalPaymentDate", new ZDate(1999, 3, 17), provider.OriginalPaymentDate);
			AssertEquals("ReconPaymentDate", new ZDate(1999, 9, 15), provider.ReconPaymentDate);

			AssertEquals("OriginalPaymentAmount", 12000m, provider.OriginalPayable);
			AssertEquals("ReconPayable", 13570m, provider.ReconPayable);

			provider.UpdateOrAddInterestCharge(10m);
			AssertEquals("interest", 10m, reconDeclaration.US_R_AggregateInterest);

			provider.UpdateOrAddInterestCharge(0m);
			AssertEquals("interest", 0m, reconDeclaration.US_R_AggregateInterest);
		}
	}
}
