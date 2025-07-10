using System;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI
{
	public partial class MiscServicesUserControl : OrganisationSecurityContainerControl
	{
		public MiscServicesUserControl()
		{
			InitializeComponent();
			if (!DesignModeFinder.IsDesigning)
			{
				SetContainerYardCarrierRelatedPartyControl_Visibility();
				OH_IsContainerYardBoundCheckBox.CheckedChanged += ContainerYardCarrierRelatedPartyControl_CheckedChanged;

				RailHeadCheckBox.CheckedChanged += MiscServiceCTOStorageUserControl_CheckedChanged;
				OH_IsSeaCTOBoundCheckBox.CheckedChanged += MiscServiceCTOStorageUserControl_CheckedChanged;
				SetMiscServiceCTOStorageUserControl_Visibility();

				OH_IsAirCTOBoundCheckBox.CheckedChanged += FacilityTab_CheckedChanged;
				OH_IsSeaCTOBoundCheckBox.CheckedChanged += FacilityTab_CheckedChanged;
				RailHeadCheckBox.CheckedChanged += FacilityTab_CheckedChanged;
				RoadFreightDepotCheckBox.CheckedChanged += FacilityTab_CheckedChanged;
				OH_IsContainerYardBoundCheckBox.CheckedChanged += FacilityTab_CheckedChanged;
				OH_IsPackDepotBoundCheckBox.CheckedChanged += FacilityTab_CheckedChanged;
				OH_IsUnpackDepotBoundCheckBox.CheckedChanged += FacilityTab_CheckedChanged;
				SetMiscServiceFacilityUserControl_Visibility();
			}
		}

		void FacilityTab_CheckedChanged(object sender, EventArgs e)
		{
			SetMiscServiceFacilityUserControl_Visibility();
		}

		void SetMiscServiceFacilityUserControl_Visibility()
		{
			var isVisibile = OH_IsAirCTOBoundCheckBox.Checked || OH_IsSeaCTOBoundCheckBox.Checked ||
					RailHeadCheckBox.Checked || RoadFreightDepotCheckBox.Checked || OH_IsContainerYardBoundCheckBox.Checked ||
					OH_IsPackDepotBoundCheckBox.Checked || OH_IsUnpackDepotBoundCheckBox.Checked;

			FacilityTabControl.Visible = isVisibile;
			RefFacilityTabPage.TabVisible = isVisibile;
			MiscServiceFacilityUserControl.Visible = isVisibile;
		}

		void MiscServiceCTOStorageUserControl_CheckedChanged(object sender, EventArgs e)
		{
			SetMiscServiceCTOStorageUserControl_Visibility();
		}

		void SetMiscServiceCTOStorageUserControl_Visibility()
		{
			MiscServiceCTOStorageUserControl.Visible = RailHeadCheckBox.Checked || OH_IsSeaCTOBoundCheckBox.Checked;

			UpdateCTOStorageLayout();
		}

		void UpdateCTOStorageLayout()
		{
			if (ContainerYardCarrierRelatedPartyControl.Visible)
			{
				MiscServiceCTOStorageUserControl.Dock = System.Windows.Forms.DockStyle.Top;
				MiscServiceCTOStorageUserControl.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(Math.Max(MiscServiceCTOStorageUserControl.Parent.Height / 2, 60));
			}
			else
			{
				MiscServiceCTOStorageUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			}
		}

		void ContainerYardCarrierRelatedPartyControl_CheckedChanged(object sender, EventArgs e)
		{
			SetContainerYardCarrierRelatedPartyControl_Visibility();
		}

		void SetContainerYardCarrierRelatedPartyControl_Visibility()
		{
			ContainerYardCarrierRelatedPartyControl.Visible = OH_IsContainerYardBoundCheckBox.Checked;

			UpdateCTOStorageLayout();
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
