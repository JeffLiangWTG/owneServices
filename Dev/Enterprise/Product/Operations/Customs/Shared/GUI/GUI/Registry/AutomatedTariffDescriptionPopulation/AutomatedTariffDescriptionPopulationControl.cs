using Enterprise.Registry.GUI;

namespace Enterprise.Customs.DataRegistry.GUI
{
	public partial class AutomatedTariffDescriptionPopulationControl : RegistryZUserControl
	{
		public AutomatedTariffDescriptionPopulationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			AutomatedTariffDescriptionPopulationGroupBox.Enabled = !readOnly;
		}
	}
}
