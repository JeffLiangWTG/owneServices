using Enterprise.Registry.GUI;

namespace Enterprise.ProcessManagement.GUI
{
	public partial class RecipientSourceFallbackControl : RegistryZUserControl
	{
		public RecipientSourceFallbackControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			FallbackSourcesGrid.ReadOnly = readOnly;
		}
	}
}
