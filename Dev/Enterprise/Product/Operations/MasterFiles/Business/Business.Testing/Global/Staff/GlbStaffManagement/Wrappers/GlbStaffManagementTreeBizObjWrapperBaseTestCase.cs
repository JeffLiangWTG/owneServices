using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class GlbStaffManagementTreeBizObjWrapperBaseTestCase<T> : NonPersistentBusinessObjectTestCase
				where T : GlbStaffManagementTreeBizObjWrapperBase
	{
		protected GlbStaffManagementTreeBizObjWrapperBaseTestCase()
		{
			CreateTestData();
		}

		#region Properties

		protected GlbStaff Staff { get; private set; }

		protected GlbStaff Manager { get; private set; }

		protected GlbStaffManager RoleManager { get; private set; }

		protected GlbStaffManagementTreeModel TreeModel { get; private set; }

		protected GlbBranch Branch { get; private set; }

		#endregion

		#region Implementation

		protected abstract T GetNewWrapper(GlbStaffManagementTreeModel treeModel,
			IEnumerable<GlbStaffManagementTreeBizObjWrapperBase> children);

		protected abstract GlbStaffManagementTreeBizObjWrapperBase GetNewChildWrapper(GlbStaffManagementTreeModel treeModel);

		protected override BusinessObject GetNewBusinessObject()
		{
			var childWrapper = GetNewChildWrapper(TreeModel);
			var children = childWrapper != null ? new[] { childWrapper } : null;
			return GetNewWrapper(TreeModel, children);
		}

		void CreateTestData()
		{
			Staff = Factory.NewWithValidTestData<GlbStaff>();
			Manager = Factory.NewWithValidTestData<GlbStaff>();
			Branch = Factory.NewWithValidTestData<GlbBranch>();
			Manager.GS_GB_HomeBranch = Branch.PK;
			RoleManager = Factory.NewWithValidTestData<GlbStaffManager>();
			RoleManager.GSM_GS_Staff = Staff.PK;
			RoleManager.GSM_GS_Manager = Manager.PK;
			RoleManager.GSM_ManagerType = "HRM";
			TreeModel = new GlbStaffManagementTreeModel(Staff);
		}

		#endregion
	}
}
