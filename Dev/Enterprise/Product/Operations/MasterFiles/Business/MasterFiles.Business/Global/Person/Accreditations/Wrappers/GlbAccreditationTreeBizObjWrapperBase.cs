using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public abstract class GlbAccreditationTreeBizObjWrapperBase : NonPersistentBusinessObject, IBusiness
	{
		protected GlbAccreditationTreeBizObjWrapperBase(ZTreeModel<GlbAccreditationTreeBizObjWrapperBase> treeModel, IBusiness parent)
			: base(treeModel.Factory)
		{
			TreeModel = treeModel;
			Parent = parent;
		}

		public ZTreeModel<GlbAccreditationTreeBizObjWrapperBase> TreeModel { get; }
		public IBusiness Parent { get; }

		public abstract ZString Description { get; }
		public abstract ZString CommenceDateUtc { get; }
		public abstract ZString CompletionDateUtc { get; }
		public abstract ZString Score { get; }
		public abstract ZString Progress { get; }
		public abstract ZString CompetencyRules { get; }
		public abstract ZString ApplicantEmail { get; }
		public abstract ZString Comment { get; }

		public abstract GlbAccreditationTreeBizObjWrapperBase[] Children { get; }
	}
}
