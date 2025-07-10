using System;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefServiceLevelForm : ZForm
	{
		public RefServiceLevelForm()
		{
		}

		public RefServiceLevelForm(RefServiceLevel serviceLevel)
			: base(serviceLevel)
		{
			ControllerID = ControllerID ?? ControllerIDs.ServiceLevel;
			ZFormPostingButtonsStrategy.SetupPosting(this, ButtonsUserControl);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);

			if (!DesignModeFinder.IsDesigning)
			{
				SetDeliveryDueDateVisibility();
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ControlDpiScalingHelper.SetTop(ref ButtonsUserControl, MainStatusBar.Top - ButtonsUserControl.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
		}

		void SetDeliveryDueDateVisibility()
		{
			DDDPanel.Visible = FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsActive;

			if (!FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsActive)
			{
				this.Height -= DDDPanel.Height;
				this.MainTabPage.Height -= DDDPanel.Height;
				this.MainTabPage.Controls.Remove(this.DDDPanel);
			}
			else
			{
				isDeliverOnSaturday.CheckedChanged += DeliverOnSaturday_CheckedChanged;
			}
		}

		void DeliverOnSaturday_CheckedChanged(object sender, EventArgs e)
		{
			var currentCheckBox = sender as ZCheckBox;
			if (currentCheckBox.Checked)
			{
				isDeliverOnSaturday.Checked = true;
				isDeliverOnSunday.ReadOnly = false;
			}
			else
			{
				isDeliverOnSunday.Checked = false;
				isDeliverOnSaturday.Checked = false;
				isDeliverOnSunday.ReadOnly = true;
			}
		}
	}
}
