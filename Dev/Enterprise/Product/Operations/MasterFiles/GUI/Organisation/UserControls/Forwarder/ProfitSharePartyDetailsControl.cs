using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ProfitSharePartyDetailsControl : ZUserControl
	{
		public ProfitSharePartyDetailsControl()
		{
			InitializeComponent();
		}

		public void SetGatewayConsolProfitRedistributionTabPageVisibility(bool isVisible)
		{
			gatewayConsolProfitRedistributionTabPage.TabVisible = isVisible;
		}
	}
}
