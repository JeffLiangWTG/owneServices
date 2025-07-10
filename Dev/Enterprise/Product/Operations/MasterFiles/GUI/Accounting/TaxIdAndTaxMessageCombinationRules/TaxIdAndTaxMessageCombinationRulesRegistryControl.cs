using Enterprise.Registry.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class TaxIdAndTaxMessageCombinationRulesRegistryControl : RegistryZUserControl
	{
		public TaxIdAndTaxMessageCombinationRulesRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ValidationOptionDropEdit.ReadOnly = TaxIdAndTaxMessageCombinationRulesGrid.ReadOnly = readOnly;
		}
	}
}
