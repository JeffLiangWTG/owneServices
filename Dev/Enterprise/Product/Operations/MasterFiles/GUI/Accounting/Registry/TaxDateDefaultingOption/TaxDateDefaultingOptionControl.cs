using Enterprise.Registry.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class TaxDateDefaultingOptionControl : RegistryZUserControl
	{
		public TaxDateDefaultingOptionControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			TaxDateDefaultingOptionGrid.ReadOnly = readOnly;
		}
	}
}
