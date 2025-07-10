using Enterprise.Registry.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class EPaymentReasonsControl : RegistryZUserControl
	{
		public EPaymentReasonsControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ReasonGrid.ReadOnly = readOnly;
		}
	}
}
