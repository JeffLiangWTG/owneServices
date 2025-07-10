using Enterprise.Registry.GUI;

namespace Enterprise.Customs.ZA.DataRegistry.GUI
{
	public partial class CustomsDSBCreditorOverrideUserControl : RegistryZUserControl
	{
		public CustomsDSBCreditorOverrideUserControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			MainGrid.ReadOnly = readOnly;
		}
	}
}
