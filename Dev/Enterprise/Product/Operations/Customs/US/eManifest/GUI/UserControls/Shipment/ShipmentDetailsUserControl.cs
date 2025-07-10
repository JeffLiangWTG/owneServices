using Enterprise.Customs.US.eManifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.eManifest.GUI
{
	public partial class ShipmentDetailsUserControl : ZUserControl
	{
		public ShipmentDetailsUserControl()
		{
			InitializeComponent();
			BindingSource.SetBindingMember(DescriptionOfCargoControl, Shipment.Schema.B0_DescriptionOfCargo);
		}
	}
}
