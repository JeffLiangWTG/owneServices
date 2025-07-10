using System.Linq;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffManagementTreeModel : ZTreeModel<GlbStaffManagementTreeBizObjWrapperBase>
	{
		public GlbStaffManagementTreeModel(GlbStaff staff, bool isManager = false)
			: base(staff.Factory)
		{
			Staff = staff;
			IsManager = isManager;
			BuildTree();
		}

		readonly ZNodeCollection<GlbStaffManagementTreeBizObjWrapperBase> rootNodes = new ZNodeCollection<GlbStaffManagementTreeBizObjWrapperBase>();

		public GlbStaff Staff { get; }
		public readonly bool IsManager;

		#region Build Tree

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public void BuildTree()
		{
			rootNodes.Clear();
			LoadRoles();
		}

		void LoadRoles()
		{
			var managementRoles = SystemDataRegistry.Instance.StaffReportingRoles.Value.ToArray().Cast<StaffReportingRole>();

			foreach (var role in managementRoles)
			{
				if (IsManager)
				{
					if (Staff.DirectReports.Any(x => x.GSM_ManagerType == role.Code && (x.GSM_EndDate.IsEmpty || x.GSM_EndDate > ZDateTime.Today)))
					{
						rootNodes.Add(new GlbStaffManagementTreeNode(this, new GlbStaffManagementRoleWrapper(this, Staff, role, isManager: true)));
					}
				}
				else
				{
					if (Staff.Managers.Any(x => x.GSM_ManagerType == role.Code && (x.GSM_EndDate.IsEmpty || x.GSM_EndDate > ZDateTime.Today)))
					{
						rootNodes.Add(new GlbStaffManagementTreeNode(this, new GlbStaffManagementRoleWrapper(this, Staff, role)));
					}
				}
			}
		}

		#endregion

		#region Nodes

		protected override ZNode<GlbStaffManagementTreeBizObjWrapperBase> CreateNewNodeCore(ZTreeModel<GlbStaffManagementTreeBizObjWrapperBase> treeModel, GlbStaffManagementTreeBizObjWrapperBase bizObj)
		{
			return new GlbStaffManagementTreeNode((GlbStaffManagementTreeModel)treeModel, bizObj);
		}

		protected override ZNodeCollection<GlbStaffManagementTreeBizObjWrapperBase> GetRootNodes()
		{
			return rootNodes;
		}

		#endregion
	}
}
