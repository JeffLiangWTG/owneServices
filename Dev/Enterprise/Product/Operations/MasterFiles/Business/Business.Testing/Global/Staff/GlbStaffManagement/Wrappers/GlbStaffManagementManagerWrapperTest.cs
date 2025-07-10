using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffManagementManagerWrapper))]
	sealed class GlbStaffManagementManagerWrapperTest : GlbStaffManagementTreeBizObjWrapperBaseTestCase<GlbStaffManagementManagerWrapper>
	{
		GlbSecurity GetSecurity(SecurityCheckpoint checkpoint, bool granted, ZGuid staffPk)
		{
			var security = Factory.New<GlbSecurity>();
			security.GU_SecurityRight = checkpoint.Code;
			security.GU_SecurityItemIsAllowed = granted;
			security.GU_GS = staffPk;

			return security;
		}

		void AssertDeniedSecurity(GlbStaffManagementManagerWrapper wrapper)
		{
			var staffDenied = Factory.NewWithValidTestData<GlbStaff>();
			staffDenied.GroupSecurityPermissionsCollectionForBinding.Add(GetSecurity(Env.Security.StaffViewOtherStaffDetails, false, staffDenied.PK));

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffDenied.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					AssertEquals(wrapper.ViewDeniedMessage, wrapper.JobTitle);
					AssertEquals(wrapper.ViewDeniedMessage, wrapper.EffectiveDate);
					AssertEquals(wrapper.ViewDeniedMessage, wrapper.Branch);
				}
			}
		}

		public void TestRole()
		{
			var wrapper = new GlbStaffManagementManagerWrapper(TreeModel, RoleManager);
			AssertEquals($"{Manager.GS_FullName} ({Manager.GS_Code})", wrapper.Role);
		}

		public void TestJobTitle()
		{
			var wrapper = new GlbStaffManagementManagerWrapper(TreeModel, RoleManager);
			AssertEquals(Manager.GS_Title, wrapper.JobTitle);
		}

		public void TestEffectiveDate()
		{
			RoleManager.GSM_EffectiveDate = new ZDate(2019, 1, 1);
			var wrapper = new GlbStaffManagementManagerWrapper(TreeModel, RoleManager);
			AssertEquals("01-Jan-19", wrapper.EffectiveDate);
		}

		public void TestLocation()
		{
			Branch.GB_Code = "BNE";
			Branch.GB_RN_NKCountryCode = "AU";
			var wrapper = new GlbStaffManagementManagerWrapper(TreeModel, RoleManager);
			AssertEquals("BNE (AU)", wrapper.Branch);
		}

		public void TestSecurity()
		{
			var wrapper = new GlbStaffManagementManagerWrapper(TreeModel, RoleManager);
			AssertDeniedSecurity(wrapper);
		}

		#region Overrides

		protected override GlbStaffManagementManagerWrapper GetNewWrapper(GlbStaffManagementTreeModel treeModel, IEnumerable<GlbStaffManagementTreeBizObjWrapperBase> children)
		{
			return new GlbStaffManagementManagerWrapper(treeModel, Factory.New<GlbStaffManager>());
		}

		protected override GlbStaffManagementTreeBizObjWrapperBase GetNewChildWrapper(GlbStaffManagementTreeModel treeModel)
		{
			return null;
		}

		#endregion
	}
}
