using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class JobMiscOrgsControl : ZUserControl
	{
		public JobMiscOrgsControl()
		{
			InitializeComponent();
			InvoicerAddressControl.Parse = AddressParser.GetAddressPKFromMatchingMIDCode;
		}

		internal void ACERelatedOrgControlVisibilityChanged(bool isACSCargoCertificationMode)
		{
			CBPBrokerOrgControl.Visible = isACSCargoCertificationMode;
			InvoicerAddressControl.Visible = isACSCargoCertificationMode;
			SellingAgentOrgControl.Visible = isACSCargoCertificationMode;
			BuyerAgentOrgControl.Visible = isACSCargoCertificationMode;
			ShipperAddressControl.Visible = !isACSCargoCertificationMode;
			DistributorAddressControl.Visible = !isACSCargoCertificationMode;
			PackagerAddressControl.Visible = !isACSCargoCertificationMode;
		}
	}
}
