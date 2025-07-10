using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AccGroupsForm : ZForm
	{
		public AccGroupsForm()
		{
		}

		public AccGroupsForm(AccGroups businessEntity) : base(businessEntity)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, ButtonsUserControl);
		}
	}
}
