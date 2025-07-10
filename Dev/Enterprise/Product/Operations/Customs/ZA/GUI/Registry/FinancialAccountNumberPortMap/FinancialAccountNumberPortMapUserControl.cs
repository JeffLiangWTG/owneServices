using Enterprise.Registry.GUI;

namespace Enterprise.Customs.ZA.DataRegistry.GUI
{
	public partial class FinancialAccountNumberPortMapUserControl : RegistryZUserControl
	{
		public FinancialAccountNumberPortMapUserControl()
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
