using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class ShipmentDetailsUserControl : ZUserControl
	{
		public ShipmentDetailsUserControl()
		{
			InitializeComponent();

			ShipmentDetailsScreeningUserControl.AllowOutsideOfParent();
		}
	}
}
