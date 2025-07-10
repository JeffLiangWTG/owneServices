using Enterprise.Registry.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class CargoIMPPhase2MSUEventsMappingRegistryControl : RegistryZUserControl
	{
		public CargoIMPPhase2MSUEventsMappingRegistryControl()
		{
			InitializeComponent();
		}
		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			CargoIMPPhase2MSUEventsMappingGrid.ReadOnly = readOnly;
		}
	}
}
