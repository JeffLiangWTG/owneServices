using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class GlbAccreditationTreeModelView : ZTreeModelView<GlbAccreditationTreeBizObjWrapperBase>
	{
		public GlbAccreditationTreeModelView(ZTreeModel<GlbAccreditationTreeBizObjWrapperBase> inner) : base(inner)
		{
		}

		public void BuildTree()
		{
			InnerModel.BuildTree();
			RefreshView();
		}

		protected new GlbAccreditationTreeModel InnerModel
		{
			get { return (GlbAccreditationTreeModel)base.InnerModel; }
		}
	}
}
