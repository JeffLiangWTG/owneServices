using Enterprise.Registry.GUI;

namespace Enterprise.Customs.TR.GUI
{
	public partial class StampDutyLedgerNumberCustomizationRegistryItemUserControl : RegistryZUserControl
	{
		public StampDutyLedgerNumberCustomizationRegistryItemUserControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			StartDateEdit.ReadOnly = EndDateEdit.ReadOnly = StartNumberCalcEdit.ReadOnly = ExpiredYearEdit.ReadOnly = readOnly;
		}
	}
}
