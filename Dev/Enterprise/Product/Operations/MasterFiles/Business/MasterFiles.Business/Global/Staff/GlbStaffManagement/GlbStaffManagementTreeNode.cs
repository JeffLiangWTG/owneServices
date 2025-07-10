using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffManagementTreeNode : ZNode<GlbStaffManagementTreeBizObjWrapperBase>
	{
		public GlbStaffManagementTreeNode(GlbStaffManagementTreeModel treeModel, GlbStaffManagementTreeBizObjWrapperBase bizObj)
			: base(treeModel, bizObj)
		{
		}

		public new GlbStaffManagementTreeModel TreeModel => (GlbStaffManagementTreeModel)base.TreeModel;

		#region Properties

		#region Role

		public ZString Role => BizObjForBinding.Role;

		#endregion

		#region JobTitle

		public ZString JobTitle => BizObjForBinding.JobTitle;

		#endregion

		#region EffectiveDate

		public ZString EffectiveDate => BizObjForBinding.EffectiveDate;

		#endregion

		#region Branch

		public ZString Branch => BizObjForBinding.Branch;

		#endregion

		#endregion

		#region Overrides

		protected override ChangeParentOnBizObjResult ChangeParentOnBizObj(GlbStaffManagementTreeBizObjWrapperBase previousParent,
			GlbStaffManagementTreeBizObjWrapperBase newParent, bool checkValid)
		{
			throw new NotImplementedException("Not required as does not support re-ordering");
		}

		protected override IEnumerable<GlbStaffManagementTreeBizObjWrapperBase> LoadChildBizObjs()
		{
			return BizObjForBinding.Children?.Cast<GlbStaffManagementTreeBizObjWrapperBase>() ?? Enumerable.Empty<GlbStaffManagementTreeBizObjWrapperBase>();
		}

		protected override GlbStaffManagementTreeBizObjWrapperBase LoadParentBizObj()
		{
			return null;
		}

		#endregion
	}
}
