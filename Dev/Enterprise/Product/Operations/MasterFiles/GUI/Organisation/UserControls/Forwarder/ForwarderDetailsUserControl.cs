using System;
using CargoWise.Windows.UI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ForwarderDetailsUserControl : ZUserControl
	{
		public ForwarderDetailsUserControl()
		{
			InitializeComponent();
			splitContainer1.SizeChanged += SplitContainer1_SizeChanged;
		}

		readonly int splitContainerBorder = ControlDpiScalingHelper.ScaleToCurrentDpiY(4);
		void SplitContainer1_SizeChanged(object sender, EventArgs e)
		{
			var height = splitContainer1.Height - splitContainer1.Panel1.Height - OrgAppointedAgentPortsTabControl.Location.Y - splitContainerBorder;
			ControlDpiScalingHelper.SetHeight(OrgAppointedAgentPortsTabControl, height, false);
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			this.MasterBillAddressOverrideTabControl.PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.DocAddresses, 2);
			var addressesPlugIn = this.MasterBillAddressOverrideTabControl.PlugIns.Instances[0];
			addressesPlugIn.TabPage.Text = Res.GetString("4555c6ee-2904-46a5-a038-ec08437d79ba", "Address Overrides");
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!FreightDataRegistry.Instance.EnablePackageGrouping.Value)
			{
				this.DocumentaryDefaultsGroupBox.Controls.Remove(this.MasterBillPackageGrouping);
				this.ForwarderInfoTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 572, true);
				this.FWOtherGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 583, true);
				this.PrintingBorrowedAWBsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 713, true);
			}
		}
	}
}
