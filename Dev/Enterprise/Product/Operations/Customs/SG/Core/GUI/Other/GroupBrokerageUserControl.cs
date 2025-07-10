using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.V4.GUI
{
	public partial class GroupBrokerageUserControl : ZUserControl
	{
		public GroupBrokerageUserControl()
		{
			InitializeComponent();

			if (!DesignMode)
			{
				AccessVisibleOnlyToDeveloperTextBox.Visible = GlbStaff.CurrentUser.GS_IsDeveloper;
			}
		}
	}
}
