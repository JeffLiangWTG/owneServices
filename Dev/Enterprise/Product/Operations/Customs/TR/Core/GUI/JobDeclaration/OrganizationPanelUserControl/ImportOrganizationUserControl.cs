namespace Enterprise.Customs.TR.GUI
{
	public partial class ImportOrganizationUserControl : EU.GUI.ImportOrganizationUserControl
	{
		public ImportOrganizationUserControl()
		{
			InitializeComponent();
			HideUnnecessaryControls();
			SetAddressesLocation();
		}

		void HideUnnecessaryControls()
		{
			DeclarantOfficeAddressControl.Visible = false;
			RepresentativeAddressControl.Visible = false;
		}

		void SetAddressesLocation()
		{
			ToWarehouseAddressControl.Location = RepresentativeAddressControl.Location;
			FromWarehouseAddressControl.Location = DeclarantOfficeAddressControl.Location;
			TradersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, CargoWise.Windows.UI.ControlDpiScalingHelper.UnscaleFromCurrentDpiY(FromWarehouseAddressControl.Location.Y) + 30, true);
		}
	}
}
