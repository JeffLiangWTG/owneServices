using Enterprise.Registry.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class MaximumAllowedTransactionAmountControl : RegistryZUserControl
	{
		public MaximumAllowedTransactionAmountControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			MaximumAllowedLineAmountEdit.ReadOnly = readOnly;
			MaximumAllowedHeaderAmountEdit.ReadOnly = readOnly;
		}
	}
}

