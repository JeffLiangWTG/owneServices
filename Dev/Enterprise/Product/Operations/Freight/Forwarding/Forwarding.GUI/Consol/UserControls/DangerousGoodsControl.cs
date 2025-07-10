using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class DangerousGoodsControl : ZUserControl
	{
		public DangerousGoodsControl()
		{
			InitializeComponent();
		}

		void DangerousGoodsCheckBox_CheckStateChanged(object sender, System.EventArgs e)
		{
			if (Env.Security.MaintainConsolAllowEditOfAcceptedDangerousGoodsOnConsol.IsAllowed)
			{
				this.dangerousGoodsDisplayGrid.ReadOnly = !this.dangerousGoodsCheckBox.Checked;
			}
		}
	}
}
