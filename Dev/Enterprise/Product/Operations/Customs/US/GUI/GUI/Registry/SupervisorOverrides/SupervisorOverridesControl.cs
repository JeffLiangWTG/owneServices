using Enterprise.Registry.GUI;

namespace Enterprise.Customs.US.DataRegistry.GUI
{
	public partial class SupervisorOverridesControl : RegistryZUserControl
	{
		public SupervisorOverridesControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			NominErrorsGrid.ReadOnly = readOnly;
		}
	}
}
