using Enterprise.Registry.GUI;

namespace Enterprise.Customs.US.DataRegistry.GUI
{
	public partial class ReconInterestRatesControl : RegistryZUserControl
	{
		public ReconInterestRatesControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			RatesGrid.ReadOnly = readOnly;
		}
	}
}
