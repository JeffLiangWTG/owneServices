using Enterprise.Registry.GUI;

namespace Enterprise.TransportCommon.GUI.Registry
{
	public partial class PackageVersionOverrideRegistryControl : RegistryZUserControl
	{
		public PackageVersionOverrideRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			packageVersionOverrideGrid.ReadOnly = readOnly;
		}
	}
}
