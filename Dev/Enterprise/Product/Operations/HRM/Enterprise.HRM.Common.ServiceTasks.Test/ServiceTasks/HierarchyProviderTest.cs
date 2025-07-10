using System;
using System.Collections.Generic;
using CargoWise.Data.Testing;
using CargoWise.Definitions.HR;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
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
	public class HierarchyProviderTest : TestCaseWithFactory
	{
		[DeveloperOnlyTest]
		public void TestProvider()
		{
			// Requires ~BP to have an StmAccessToken with IsPermanentToken=1

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ZYX";

			var filter = ReviewFilterHelper.CreateValidFilter(Factory, staff);

			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var moderator = Factory.NewWithValidTestData<GlbStaff>();

			AddManagerLink(manager, staff);
			AddManagerLink(moderator, manager);

			var process = Factory.NewWithValidTestData<ReviewProcess>();
			process.RPR_S9_EmployeesInReview = filter.PK;

			Factory.Save();

			var serviceTaskUser = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, "~BP");
			Env.Instance.SetUserContext(new UserContext(serviceTaskUser.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK));

			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://localhost/Glow"))
			{
				var provider = new HierarchyProvider();
				var results = provider.DetermineHierarchy(process.PK.ToGuid());

				AssertContainsExactElementsInAnyOrder(new NodeComparer(), new[]
				{
					new ReviewHierarchyNode(staff.PK.ToGuid(), manager.PK.ToGuid(), manager.PK.ToGuid(), true),
					new ReviewHierarchyNode(manager.PK.ToGuid(), null, moderator.PK.ToGuid(), false),
					new ReviewHierarchyNode(moderator.PK.ToGuid(), null, null, false),
				}, results);
			}
		}

		void AddManagerLink(GlbStaff manager, GlbStaff staff, string managerType = "DRM")
		{
			var link = Factory.NewWithValidTestData<GlbStaffManager>();
			link.GSM_GS_Manager = manager.PK;
			link.GSM_GS_Staff = staff.PK;
			link.GSM_ManagerType = managerType;
			link.GSM_EffectiveDate = new ZDateTime(2020, 1, 1);
		}

		class NodeComparer : IEqualityComparer<ReviewHierarchyNode>
		{
			public bool Equals(ReviewHierarchyNode x, ReviewHierarchyNode y)
				=> Equals(x.Staff, y.Staff) && Equals(x.Reviewer, y.Reviewer) && Equals(x.Moderator, y.Moderator) && Equals(x.InReview, y.InReview);

			public int GetHashCode(ReviewHierarchyNode obj)
				=> obj.Staff.GetHashCode();
		}
	}
}
