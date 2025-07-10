using System;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.InBond.GUI
{
	public partial class USInBondUserControl : ZUserControl
	{
		public USInBondUserControl(CusInBondHeader inBondheader)
		{
			this.inBond = inBondheader;

			InitializeComponent();
			if (inBond != null)
			{
				if (inBond.Parent != null)
				{
					MainTabControl.SelectedTab = DetailsTabPage;
					usInBondHeaderDetailUserControl1.OverrideValuesCheckBox.Visible = !(inBond.Parent is ForwardingConsol);
				}
			}
		}

		readonly CusInBondHeader inBond;

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			if (inBond != null)
			{
				inBond.BH_ImportTransportModeInfo.ValueChanged -= new EventHandler(BH_ImportTransportModeInfo_ValueChanged);
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (CurrentDataItem != null)
			{
				if (inBond != null)
				{
					inBond.BH_ImportTransportModeInfo.ValueChanged += new EventHandler(BH_ImportTransportModeInfo_ValueChanged);
				}
				BH_ImportTransportModeInfo_ValueChanged(this, e);
			}
		}

		void BH_ImportTransportModeInfo_ValueChanged(object sender, EventArgs e)
		{
			var isAir = inBond != null && inBond.IsAir;
			this.BillsTabPage.CheckForChildrenControlsVisibilityChange = !isAir;
			this.MovementTabPage.CheckForChildrenControlsVisibilityChange = !isAir;
			this.AirBillsAndMovementsDetailsTabPage.CheckForChildrenControlsVisibilityChange = isAir;

			this.BillsTabPage.CheckForNotifications = !isAir;
			this.MovementTabPage.CheckForNotifications = !isAir;
			this.AirBillsAndMovementsDetailsTabPage.CheckForNotifications = isAir;

			this.BillsTabPage.TabVisible = !isAir;
			this.MovementTabPage.TabVisible = !isAir;
			this.AirBillsAndMovementsDetailsTabPage.TabVisible = isAir;
		}
	}
}
