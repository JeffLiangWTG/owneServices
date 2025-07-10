using Enterprise.Registry.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class DefaultEPaymentReasonControl : RegistryZUserControl
	{
		public DefaultEPaymentReasonControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			DefaultReasonGrid.ReadOnly = readOnly;
		}
	}
}
