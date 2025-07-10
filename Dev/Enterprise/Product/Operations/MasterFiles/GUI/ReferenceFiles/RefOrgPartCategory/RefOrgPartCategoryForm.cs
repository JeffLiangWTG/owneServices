using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefOrgPartCategoryForm : ZForm
	{
		public RefOrgPartCategoryForm()
		{
		}

		public RefOrgPartCategoryForm(OrgPartCategory bizO)
			: base(bizO)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, ButtonsUserControl);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
		}
	}
}
