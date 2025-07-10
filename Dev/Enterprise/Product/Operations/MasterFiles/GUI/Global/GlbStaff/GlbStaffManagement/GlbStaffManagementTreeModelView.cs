using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class GlbStaffManagementTreeModelView : ZTreeModelView<GlbStaffManagementTreeBizObjWrapperBase>
	{
		public GlbStaffManagementTreeModelView(GlbStaffManagementTreeModel treeModel)
			: base(treeModel)
		{
		}

		#region Build Tree

		public void BuildTree()
		{
			InnerModel.BuildTree();
			RefreshView();
		}

		#endregion

		#region Inner Model

		protected new GlbStaffManagementTreeModel InnerModel
		{
			get { return (GlbStaffManagementTreeModel)base.InnerModel; }
		}

		#endregion
	}
}
