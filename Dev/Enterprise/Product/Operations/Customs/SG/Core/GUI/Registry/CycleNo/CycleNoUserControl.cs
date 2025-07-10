using Enterprise.Registry.GUI;

namespace Enterprise.Customs.SG.Registry.GUI
{
	public partial class CycleNoUserControl : RegistryZUserControl
	{
		public CycleNoUserControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			CycleNoGrid.ReadOnly = readOnly;
		}
	}
}
