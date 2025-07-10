using Enterprise.Registry.GUI;

namespace Enterprise.Packing.GUI
{
	partial class PalletProviderRegistryControl : RegistryBusinessObjectTemplateZUserControl
	{
		public PalletProviderRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ProvidersGrid.ReadOnly = readOnly;
		}
	}
}
