using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Services.ServiceHost
{
	public class UserContextTest : TestCaseWithFactory
	{
		public void TestGenerateContext()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var dept = Factory.NewWithValidTestData<GlbDepartment>();
			Factory.Save();

			using (var context = new DummyUserContext())
			{
				context.GenerateContextExposed(branch.PK.ToGuid(), dept.PK.ToGuid(), staff.PK.ToGuid());
				AssertEquals(staff.PK, GlbStaff.CurrentUser.PK);
				AssertEquals(branch.PK, GlbBranch.CurrentBranch.PK);
				AssertEquals(dept.PK, GlbDepartment.CurrentDepartment.PK);
			}

			AssertNotEquals(staff.PK, GlbStaff.CurrentUser.PK);
			AssertNotEquals(branch.PK, GlbBranch.CurrentBranch.PK);
			AssertNotEquals(dept.PK, GlbDepartment.CurrentDepartment.PK);
		}

		public void TestGenerateContext_CalledMultipleTimes()
		{
			using (var context = new DummyUserContext())
			{
				context.GenerateContextExposed(GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), GlbStaff.CurrentUser.PK.ToGuid());
				AssertExceptionThrown<InvalidOperationException>("Context has already been established", () => context.GenerateContextExposed(GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), GlbStaff.CurrentUser.PK.ToGuid()));
			}
		}

		class DummyUserContext : UserContext
		{
			public void GenerateContextExposed(Guid branch, Guid department, Guid? staff)
			{
				base.GenerateContext(branch, department, staff);
			}
		}
	}
}
