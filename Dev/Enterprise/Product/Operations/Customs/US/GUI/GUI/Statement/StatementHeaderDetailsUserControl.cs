using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class StatementHeaderDetailsUserControl : ZUserControl
	{
		public StatementHeaderDetailsUserControl()
		{
			InitializeComponent();
		}

		public void ChangeCheckNoControlVisibility(bool isVisible)
		{
			CheckNoLabel.Visible = isVisible;
			CheckNoTextBox.Visible = isVisible;
		}
	}
}
