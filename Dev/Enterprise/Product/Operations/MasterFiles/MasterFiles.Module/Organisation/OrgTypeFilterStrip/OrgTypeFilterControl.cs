using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class OrgTypeFilterControl : ZUserControl
	{
		public OrgTypeFilterControl()
		{
			InitializeComponent();

			string consignorShipperTerminology = FreightDataRegistry.Instance.ConsignorShipperTerminology.Value;
			if (consignorShipperTerminology.Length > 0)
			{
				this.ConsignorCheckBox.Text = consignorShipperTerminology;
			}

			if (!OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.Value)
			{
				ControllingAgentCheckBox.Visible = false;
			}

			if (!OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.Value)
			{
				ControllingCustomerCheckBox.Visible = false;
			}
		}
	}
}
