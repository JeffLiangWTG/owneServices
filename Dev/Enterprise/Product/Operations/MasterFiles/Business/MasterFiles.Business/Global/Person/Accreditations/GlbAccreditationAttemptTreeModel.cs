using Enterprise.Integration.Recruiter;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class GlbAccreditationAttemptTreeModel : ZTreeModel<GlbAccreditationTreeBizObjWrapperBase>, IGlbAccreditationTreeModel
	{
		readonly ZNodeCollection<GlbAccreditationTreeBizObjWrapperBase> rootNodes = new ZNodeCollection<GlbAccreditationTreeBizObjWrapperBase>();
		readonly GlbPerson person;

		public GlbAccreditationAttemptTreeModel(IGlbAccreditationAttempt attempt, GlbPerson person) : base(attempt.Factory)
		{
			Attempt = attempt;
			this.person = person;
			BuildTree();
		}

		public IGlbAccreditationAttempt Attempt { get; }

		public IGlbAccreditation Accreditation => Attempt.Accreditation;

		protected override ZNode<GlbAccreditationTreeBizObjWrapperBase> CreateNewNodeCore(ZTreeModel<GlbAccreditationTreeBizObjWrapperBase> treeModel, GlbAccreditationTreeBizObjWrapperBase bizObj)
		{
			return new GlbAccreditationTreeNode(treeModel, bizObj);
		}

		public void BuildTree()
		{
			rootNodes.Clear();

			if (Attempt == null || Attempt.Accreditation == null)
			{
				return;
			}

			foreach (IGlbAccreditationJobSkillGroup group in Attempt.Accreditation.Groups)
			{
				rootNodes.Add(new GlbAccreditationTreeNode(this, new GlbAccreditationGroupWrapper(this, group, person, Attempt)));
			}
		}

		protected override ZNodeCollection<GlbAccreditationTreeBizObjWrapperBase> GetRootNodes()
		{
			return rootNodes;
		}
	}
}
