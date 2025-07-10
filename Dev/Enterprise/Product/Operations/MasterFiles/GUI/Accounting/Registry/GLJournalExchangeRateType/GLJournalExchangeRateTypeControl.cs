using Enterprise.Registry.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GLJournalExchangeRateTypeControl : RegistryZUserControl
	{
		public GLJournalExchangeRateTypeControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ProfitAndLossAccountTypeExchangeRateTypeDropEdit.ReadOnly = readOnly;
			BalanceSheetAccountTypeExchangeRateTypeDropEdit.ReadOnly = readOnly;
		}
	}
}

