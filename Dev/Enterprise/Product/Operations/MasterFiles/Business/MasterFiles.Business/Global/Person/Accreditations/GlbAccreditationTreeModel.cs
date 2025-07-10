using Enterprise.Integration.Recruiter;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class GlbAccreditationTreeModel : ZTreeModel<GlbAccreditationTreeBizObjWrapperBase>, IGlbAccreditationTreeModel
	{
		readonly ZNodeCollection<GlbAccreditationTreeBizObjWrapperBase> rootNodes = new ZNodeCollection<GlbAccreditationTreeBizObjWrapperBase>();

		public GlbAccreditationTreeModel(IGlbAccreditation accreditation) : base(accreditation.Factory)
		{
			Accreditation = accreditation;
			BuildTree();
		}

		public IGlbAccreditation Accreditation { get; }

		protected override ZNode<GlbAccreditationTreeBizObjWrapperBase> CreateNewNodeCore(ZTreeModel<GlbAccreditationTreeBizObjWrapperBase> treeModel, GlbAccreditationTreeBizObjWrapperBase bizObj)
		{
			return new GlbAccreditationTreeNode((GlbAccreditationTreeModel)treeModel, bizObj);
		}

		public void BuildTree()
		{
			rootNodes.Clear();

			foreach (IGlbAccreditationJobSkillGroup group in Accreditation.Groups)
			{
				rootNodes.Add(new GlbAccreditationTreeNode(this, new GlbAccreditationGroupWrapper(this, group, null)));
			}
		}

		protected override ZNodeCollection<GlbAccreditationTreeBizObjWrapperBase> GetRootNodes()
		{
			return rootNodes;
		}
	}
}
