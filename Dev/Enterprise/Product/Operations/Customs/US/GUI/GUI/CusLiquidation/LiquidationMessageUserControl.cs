using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class LiquidationMessageUserControl : ZUserControl
	{
		public LiquidationMessageUserControl()
		{
			InitializeComponent();
			MessageTabControl.SelectedTab = MessageDetailsTabPage;
		}
	}
}
