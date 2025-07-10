using Enterprise.Registry.GUI;

namespace Enterprise.Customs.US.DataRegistry.GUI
{
	public partial class BrokerAccountsUserControl : RegistryZUserControl
	{
		public BrokerAccountsUserControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			BankAccountGrid.ReadOnly = readOnly;
		}
	}
}
