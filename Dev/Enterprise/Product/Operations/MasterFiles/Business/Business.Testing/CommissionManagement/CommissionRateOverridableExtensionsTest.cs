using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CommissionRateOverridableExtensionsTest : TestCaseWithFactory
	{
		public void TestHasOverlappingCommissionPeriods()
		{
			var commissionPeriods = new CommissionPeriodCollection();
			commissionPeriods.AddNew("0-12", (NoResString)"First year only", 0, 12);
			commissionPeriods.AddNew("12-0", (NoResString)"Second year onwards", 12, 0);
			commissionPeriods.AddNew("12-24", (NoResString)"Second year only", 12, 24);
			commissionPeriods.AddNew("0-24", (NoResString)"First two years", 0, 24);
			commissionPeriods.AddNew("0-0", (NoResString)"Indefinite", 0, 0);
			OrganisationsDataRegistry.Instance.CommissionPeriodList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, commissionPeriods);

			AssertHasOverlappingCommissionPeriods("0-12", "12-0", false);
			AssertHasOverlappingCommissionPeriods("0-12", "12-24", false);
			AssertHasOverlappingCommissionPeriods("0-12", "0-24", true);
			AssertHasOverlappingCommissionPeriods("0-12", "0-0", true);

			AssertHasOverlappingCommissionPeriods("12-0", "12-24", true);
			AssertHasOverlappingCommissionPeriods("12-0", "0-24", true);
			AssertHasOverlappingCommissionPeriods("12-0", "0-0", true);

			AssertHasOverlappingCommissionPeriods("12-24", "0-24", true);
			AssertHasOverlappingCommissionPeriods("12-24", "0-0", true);

			AssertHasOverlappingCommissionPeriods("0-24", "0-0", true);
		}

		void AssertHasOverlappingCommissionPeriods(string commissionPeriodA, string commissionPeriodB, bool expectedOverlapping)
		{
			var rateA = new CommissionRateOverridableStub(Factory);
			rateA.CommissionPeriod = commissionPeriodA;
			var rateB = new CommissionRateOverridableStub(Factory);
			rateB.CommissionPeriod = commissionPeriodB;

			AssertEquals(expectedOverlapping, rateA.HasOverlappingCommissionPeriods(rateB));
		}
	}
}
