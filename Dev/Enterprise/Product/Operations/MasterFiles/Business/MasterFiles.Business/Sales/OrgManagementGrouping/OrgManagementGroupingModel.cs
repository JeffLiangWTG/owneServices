using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class OrgManagementGroupingModel : SimpleTreeModel<OrgHeader>
	{
		public OrgManagementGroupingModel(OrgHeader master)
			: base(master)
		{
		}

		#region MasterNode

		protected override ZNode<OrgHeader> CreateMasterNode()
		{
			return CreateNewNode(Master);
		}

		#endregion

		#region New Node

		protected override ZNode<OrgHeader> CreateNewNodeCore(ZTreeModel<OrgHeader> treeModel, OrgHeader bizObj)
		{
			return new OrgManagementGroupingNode((OrgManagementGroupingModel)treeModel, bizObj);
		}

		#endregion
	}
}
