using Enterprise.Registry.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GLLocalNumberFormatRegistryControl : RegistryZUserControl
	{
		public GLLocalNumberFormatRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			GLLocalNumberFormatGrid.ReadOnly = readOnly;
		}
	}
}
