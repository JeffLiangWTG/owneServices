using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Organisation.UserControls.Receivables
{
	public partial class TermsUserControl : ZUserControl
	{
		public TermsUserControl()
		{
			InitializeComponent();
		}

		public TermsUserControl(bool hideCycleInfo, bool dockTermsGroupBox = false)
				: this()
		{
			if (!dockTermsGroupBox)
			{
				TermsGroupBox.Dock = DockStyle.None;
				TermsGroupBox.Anchor = ((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right;
			}

			ARPaymentCycleGroupBox.Visible = !hideCycleInfo;
			ARTermsCycleGroupBox.Visible = !hideCycleInfo;
		}
	}
}
