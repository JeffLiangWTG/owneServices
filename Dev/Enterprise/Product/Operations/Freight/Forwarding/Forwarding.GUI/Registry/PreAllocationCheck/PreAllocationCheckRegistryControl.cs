using Enterprise.Registry.GUI;

namespace Enterprise.Freight.Forwarding.GUI.Registry
{
	public partial class PreAllocationCheckRegistryControl : RegistryZUserControl
	{
		public PreAllocationCheckRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ChecksGrid.ReadOnly = readOnly;
		}
	}
}
