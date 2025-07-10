using Enterprise.Registry.GUI;

namespace Enterprise.Customs.US.DataRegistry.GUI
{
	public partial class EntryProcessingPortsMappingUserControl : RegistryZUserControl
	{
		public EntryProcessingPortsMappingUserControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			PortMappingsGrid.ReadOnly = readOnly;
		}
	}
}
