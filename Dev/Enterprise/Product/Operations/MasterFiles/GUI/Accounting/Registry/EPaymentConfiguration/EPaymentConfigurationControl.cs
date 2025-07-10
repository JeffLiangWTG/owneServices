using Enterprise.Registry.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class EPaymentConfigurationControl : RegistryZUserControl
	{
		public EPaymentConfigurationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			configGrid.ReadOnly = readOnly;
		}
	}
}
