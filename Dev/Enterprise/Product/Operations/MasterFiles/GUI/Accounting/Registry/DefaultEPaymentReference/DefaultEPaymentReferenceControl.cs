using Enterprise.Registry.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class DefaultEPaymentReferenceControl : RegistryZUserControl
	{
		public DefaultEPaymentReferenceControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			PaymentReferenceGrid.ReadOnly = readOnly;
		}
	}
}
