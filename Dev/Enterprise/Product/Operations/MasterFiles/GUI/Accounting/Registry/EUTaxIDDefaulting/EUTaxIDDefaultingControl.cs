using Enterprise.Registry.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class EUTaxIDDefaultingControl : RegistryZUserControl
	{
		public EUTaxIDDefaultingControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			CostGrid.ReadOnly = SellGrid.ReadOnly = readOnly;
		}
	}
}
