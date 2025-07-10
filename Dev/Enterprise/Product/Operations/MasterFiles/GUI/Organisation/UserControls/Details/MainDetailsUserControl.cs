using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class MainDetailsUserControl : OrganisationSecurityContainerControl
	{
		public MainDetailsUserControl()
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				OH_IsConsignorBoundCheckEdit.Text = FreightDataRegistry.Instance.ConsignorShipperTerminology.Value;

				if (!OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.Value)
				{
					OH_IsControllingCustomerBoundCheckEdit.Visible = false;
				}

				if (DataRegistry.Instance.ProductivityWiseModeEnabled)
				{
					isProductivityWiseModeEnabled = true;
					SetUpControlForProductivityWiseMode();
				}

				if (!OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.Value)
				{
					OH_IsControllingAgentBoundCheckEdit.Visible = false;
				}
				else if (!OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.Value)
				{
					var currentLocation = OH_IsControllingAgentBoundCheckEdit.Location;
					currentLocation.Offset(CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, -22));
					OH_IsControllingAgentBoundCheckEdit.Location = currentLocation;
				}
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			CustomFieldsTabPage.NotifyBindingOrShowing();
		}

		void SetUpControlForProductivityWiseMode()
		{
			var orgTypesToIncludeInProductivityWiseMode = new[] { OH_IsActiveBoundCheckEdit, OH_IsDebtorBoundCheckEdit, OH_IsCreditorBoundCheckEdit, OH_IsSalesLeadBoundCheckEdit };

			foreach (var control in orgTypesflowLayoutPanel.Controls.Cast<Control>())
			{
				control.Visible = orgTypesToIncludeInProductivityWiseMode.Contains(control);
			}

			DetailsTabControl.TabPages.Remove(AutoRatingAndCompanyTariffTabPage);
			DetailsTabControl.TabPages.Remove(RelatedPartiesTabPage);
		}

		readonly bool isProductivityWiseModeEnabled;

		#region GUI Setup

		void MainDetailsUserControl_Load(object sender, EventArgs e)
		{
			if (!isProductivityWiseModeEnabled)
			{
				DetailsTabControl.PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.ExporterScheme, 4);
			}

			if (!DesignMode)
			{
				StaffAssignmentsTabPage.SetupSecurity(Env.Security.OrgDetailsViewCompanysStaffAssignments);
			}
		}

		protected override void ToggleCompetitorIntelligenceForm()
		{
			base.ToggleCompetitorIntelligenceForm();
			OrgTypeGroupBox.Visible = false;
		}

		protected override void ToggleClientIntelligenceForm()
		{
			base.ToggleClientIntelligenceForm();
			OH_IsConsignorBoundCheckEdit.Enabled = OH_IsConsigneeBoundCheckEdit.Enabled = Env.Registry.OrgShowConsigneeConsignorTab;
			OH_IsDebtorBoundCheckEdit.Enabled = Env.Registry.OrgShowARTab;
			OH_IsCreditorBoundCheckEdit.Enabled = false;
			OH_IsForwarderBoundCheckEdit.Enabled = false;
			OH_IsShippingProviderBoundCheckEdit.Enabled = false;
			OH_IsWarehouseClientBoundCheckEdit.Enabled = false;
			OH_IsTransportClientBoundCheckEdit.Enabled = false;
			OH_IsMiscFreightServicesBoundCheckEdit.Enabled = false;
			OH_IsBrokerBoundCheckEdit.Enabled = false;
			OH_IsCompetitorBoundCheckEdit.Enabled = false;
			OH_IsControllingAgentBoundCheckEdit.Enabled = false;
			OH_IsControllingCustomerBoundCheckEdit.Enabled = false;
		}

		#endregion

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

		void DetailsTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (DetailsTabControl.SelectedTab == DetailsTabPage)
			{
				OrgHeader organization = CurrentDataItem as OrgHeader;
				if (organization != null)
				{
					organization.RefreshBinding(); // This refreshes the primary registration number fields.
				}
			}
		}
	}
}
