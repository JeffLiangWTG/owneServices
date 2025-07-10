using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconInterestCalculatorTest : TestCaseWithFactory
	{
		public void TestCalculateInterestForeachEntry()
		{
			DeclarationTestHelper.SetReconInterestInRegistry(new ZDate(1999, 1, 1), new ZDate(1999, 03, 31), 7m);//7%
			DeclarationTestHelper.SetReconInterestInRegistry(new ZDate(1999, 4, 1), new ZDate(1999, 09, 30), 8m);//8%

			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_PreliminaryStatementPrintDate = new ZDate(1999, 9, 15);

			//+300
			ReconOriginalEntryHeader entry1 = reconDeclaration.OriginalEntries.AddNew();
			entry1.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4000m);
			entry1.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4300m);
			entry1.US_PaymentDate = new ZDate(1999, 1, 5);

			//-300
			ReconOriginalEntryHeader entry2 = reconDeclaration.OriginalEntries.AddNew();
			entry2.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4000m);
			entry2.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 3700m);
			entry2.US_PaymentDate = new ZDate(1999, 4, 12);

			reconDeclaration.US_IsAggregate = false;
			new ReconInterestCalculator(new ReconInterestDataProviderReconDec(reconDeclaration)).Execute();

			//entry by entry calculation
			AssertEquals("entry1 Interest", 16.42m, entry1.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest));
		}

		public void TestInterestCalculation()
		{
			DeclarationTestHelper.SetReconInterestInRegistry(new ZDate(1999, 1, 1), new ZDate(1999, 03, 31), 7m);//7%
			DeclarationTestHelper.SetReconInterestInRegistry(new ZDate(1999, 4, 1), new ZDate(1999, 09, 30), 8m);//7%

			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
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

			reconDeclaration.US_IsAggregate = false;
			new ReconInterestCalculator(new ReconInterestDataProviderReconDec(reconDeclaration)).Execute();

			//entry by entry calculation
			AssertEquals("entry1 Interest", 16.42m, entry1.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest));
			AssertEquals("entry2 Interest", 23.45m, entry2.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest));
			AssertEquals("entry3 Interest", 14.78m, entry3.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest));
			AssertEquals(0m, reconDeclaration.US_R_AggregateInterest);

			reconDeclaration.US_IsAggregate = true;
			reconDeclaration.US_R_IsNoChangeAgg = false;

			entry1.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4000m);
			entry1.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4300m);
			entry2.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4000m);
			entry2.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4670m);
			entry3.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4000m);
			entry3.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4600m);
			new ReconInterestCalculator(new ReconInterestDataProviderReconDec(reconDeclaration)).Execute();

			AssertEquals("entry1 Interest", 0m, entry1.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest));
			AssertEquals("entry2 Interest", 0m, entry2.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest));
			AssertEquals("entry3 Interest", 0m, entry3.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest));
			AssertEquals("mid point calculation for aggregate recon", 63.58m, reconDeclaration.US_R_AggregateInterest);

			reconDeclaration.US_IsAggregate = false;
			new ReconInterestCalculator(new ReconInterestDataProviderReconDec(reconDeclaration)).Execute();

			AssertEquals("entry1 Interest", 16.42m, entry1.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest));
			AssertEquals("entry2 Interest", 23.45m, entry2.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest));
			AssertEquals("entry3 Interest", 14.78m, entry3.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest));
			AssertEquals("entry by entry calculation", 0m, reconDeclaration.US_R_AggregateInterest);
		}

		public void TestInterestCalculationWhenMPCExists()
		{
			DeclarationTestHelper.SetReconInterestInRegistry(new ZDate(2019, 1, 1), new ZDate(2019, 12, 31), 7m);
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_PreliminaryStatementPrintDate = new ZDate(2019, 11, 15);

			var entry = reconDeclaration.OriginalEntries.AddNew();
			entry.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4000m);
			entry.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 3000m);
			entry.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.MPC, 2000m);
			entry.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4000m);
			entry.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 5000m);
			entry.US_PaymentDate = new ZDate(2019, 11, 5);

			reconDeclaration.US_IsAggregate = false;
			new ReconInterestCalculator(new ReconInterestDataProviderReconDec(reconDeclaration)).Execute();
			AssertEquals("entry Interest", 4.22m, entry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest));
		}
	}
}
