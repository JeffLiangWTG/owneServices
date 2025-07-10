using Enterprise.Registry.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class CargoIMPPhase2RouteMapRegistryControl : RegistryZUserControl
	{
		public CargoIMPPhase2RouteMapRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			CargoIMPPhase2RouteMapGrid.ReadOnly = readOnly;
		}
	}
}
