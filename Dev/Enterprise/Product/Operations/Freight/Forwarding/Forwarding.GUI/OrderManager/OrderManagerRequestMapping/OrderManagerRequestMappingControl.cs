using Enterprise.Registry.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class OrderManagerRequestMappingControl : RegistryZUserControl
	{
		public OrderManagerRequestMappingControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			orderManagerRequestMappingGrid.ReadOnly = readOnly;
		}
	}
}
