using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.CFS.GUI
{
	public partial class PickupConfirmControl : ZUserControl
	{
		public PickupConfirmControl()
		{
			InitializeComponent();
			ContainersOriginCFSDepartureConfirmDetails.DepartureContainerPickupDateEdit.CaptionResourceString =
				ContainersOriginCFSArrivalConfirmDetails.DepartureContainerPickupDateEdit.CaptionResourceString =
					Res.GetData("PickupConfirmControl|fe130c70-ca6f-49d0-b256-05b0d088953d", "Dispatched", "Dispatched At", "Time the container was dispatched from CFS.");
		}
	}
}
