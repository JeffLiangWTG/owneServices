using System;
using System.Collections.Generic;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.HRM.Common;
using Enterprise.HRM.Common.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.HRM.ServiceTasks.Testing
{
	[UseSnapshotProtection(skipTransaction: true)]
	public class ReviewAllocationProviderTest : TestCaseWithFactory
	{
		[DeveloperOnlyTest]
		public void TestProvider()
		{
			// Requires ~BP to have an StmAccessToken with IsPermanentToken=1

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ZYX";

			var filter = ReviewFilterHelper.CreateValidFilter(Factory, staff);

			var process = Factory.NewWithValidTestData<ReviewProcess>();
			process.RPR_S9_EmployeesInReview = filter.PK;

			var demographic = Factory.NewWithValidTestData<ReviewProcessDemographic>();
			demographic.RPD_Priority = 1;
			demographic.RPD_RPR_ReviewProcess = process.PK;
			demographic.RPD_S9_Filter = filter.PK;
			demographic.RPD_StandardIncreasePercent = 42;

			var entitlement = Factory.NewWithValidTestData<ReviewProcessBudget>();
			entitlement.RPB_RPD_Demographic = demographic.PK;
			entitlement.RPB_CanOffer = true;
			entitlement.RPB_Entitlement = "BAS";
			entitlement.RPB_WeightingPercent = 100;

			Factory.Save();

			var serviceTaskUser = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, "~BP");
			Env.Instance.SetUserContext(new UserContext(serviceTaskUser.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK));

			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://localhost/Glow"))
			{
				var provider = new ReviewAllocationProvider();
				var results = provider.LoadBudgetAllocations(process.PK.ToGuid());

				AssertContainsExactElementsInAnyOrder(new AllocationComparer(), new[]
				{
					new BudgetAllocation { BudgetPercent = 42, EntitlementCode = "BAS", StaffPK = staff.PK.ToGuid() }
				}, results);
			}
		}

		class AllocationComparer : IEqualityComparer<BudgetAllocation>
		{
			public bool Equals(BudgetAllocation x, BudgetAllocation y)
				=> (x.StaffPK, x.EntitlementCode, x.BudgetPercent) == (y.StaffPK, y.EntitlementCode, y.BudgetPercent);

			public int GetHashCode(BudgetAllocation obj)
				=> (obj.StaffPK, obj.EntitlementCode, obj.BudgetPercent).GetHashCode();
		}
	}
}
