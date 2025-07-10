using Enterprise.Registry.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RevenueRecognitionControl : RegistryZUserControl
	{
		public RevenueRecognitionControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			RevenueRecognitionGrid.ReadOnly = readOnly;
		}
	}
}
