using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ActiveUser))]
	sealed class ActiveUserTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCalculateYourLoginTime()
		{
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "XYZ";
			branch.GB_RL_NKHomePort = "USAAZ";  // -7 hours
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var staff = Factory.New<GlbStaff>();
				staff.GS_Code = "XYZ";
				staff.GS_GB_HomeBranch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "BNE").PK;
				Factory.Save();
				var activeUser = new ActiveUser(Factory);
				activeUser.AU_GS = staff.PK;
				activeUser.AU_UTCLoginTime = new ZDateTime(2009, 1, 1, 3, 30, 0);
				AssertEquals(new ZDateTime(2008, 12, 31, 20, 30, 0), activeUser.AU_YourLocalLoginTime);
			}
		}

		public void TestRenamedStaff()
		{
			var user = new ActiveUser(Factory);
			Assert(user.AU_UserLocalLoginTime.IsEmpty);
		}

		public void TestMaxLengthOfAU_FullName()
		{
			var user = new ActiveUser(Factory);
			AssertEquals(GlbStaffSchema.GS_FullName.MaxLength, user.AU_FullNameInfo.MaxLength);
		}

		public void TestCalculateUsersLoginTime()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "XYZ";
			staff.GS_GB_HomeBranch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "BNE").PK;
			Factory.Save();
			var activeUser = new ActiveUser(Factory);
			activeUser.AU_GS = staff.PK;
			activeUser.AU_UTCLoginTime = new ZDateTime(2009, 1, 1, 3, 30, 0);
			AssertEquals(activeUser.AU_UTCLoginTime + new TimeSpan(10, 0, 0), activeUser.AU_UserLocalLoginTime);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ActiveUser(Factory);
		}
	}
}
