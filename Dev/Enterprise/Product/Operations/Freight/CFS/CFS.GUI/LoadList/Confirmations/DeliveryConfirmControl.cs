using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.CFS.GUI
{
	public partial class DeliveryConfirmControl : ZUserControl
	{
		public DeliveryConfirmControl()
		{
			InitializeComponent();
			DestinationCFSDepartureConfirmDetails.DepartureContainerPickupDateEdit.CaptionResourceString = Res.GetData("DeliveryConfirmControl|fe130c70-ca6f-49d0-b256-05b0d088953d", "Dispatched At", "Date the container was dispatched at.");
			ContainersDestinationCFSArrivalConfirmDetails.DepartureContainerPickupDateEdit.CaptionResourceString = Res.GetData("DeliveryConfirmControl|0729d510-2009-4209-b2c6-285bc6837fd8", "Received At", "Date the container was received at.");
		}
	}
}
