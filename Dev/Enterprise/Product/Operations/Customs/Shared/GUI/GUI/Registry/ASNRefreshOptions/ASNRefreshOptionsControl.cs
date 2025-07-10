using Enterprise.Registry.GUI;

namespace Enterprise.Customs.DataRegistry.GUI
{
	public sealed partial class ASNRefreshOptionsControl : RegistryZUserControl
	{
		public ASNRefreshOptionsControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ASNRefreshOptionsGrid.ReadOnly = readOnly;
		}
	}
}
