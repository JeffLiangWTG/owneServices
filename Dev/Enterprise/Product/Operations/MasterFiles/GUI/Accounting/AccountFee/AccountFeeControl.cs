using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AccountFeeControl : ZUserControl
	{
		public AccountFeeControl()
		{
			InitializeComponent();
			PrepareControls();
			OverrideCheckbox.CheckedChanged += new System.EventHandler((sender, e) => { PrepareControls(); });
		}

		void PrepareControls()
		{
			AccountFeeRuleDropEdit.Enabled = OverrideCheckbox.Checked;
			GLAccountBoundFindBox.Enabled = OverrideCheckbox.Checked;
			AmountCalcFindBox.Enabled = OverrideCheckbox.Checked;
		}
	}
}