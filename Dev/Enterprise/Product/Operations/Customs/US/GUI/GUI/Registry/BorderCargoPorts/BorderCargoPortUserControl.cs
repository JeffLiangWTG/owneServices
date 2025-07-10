using Enterprise.Registry.GUI;

namespace Enterprise.Customs.US.DataRegistry.GUI
{
	public partial class BorderCargoPortUserControl : RegistryZUserControl
	{
		public BorderCargoPortUserControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			MainGrid.ReadOnly = readOnly;
		}
	}
}
