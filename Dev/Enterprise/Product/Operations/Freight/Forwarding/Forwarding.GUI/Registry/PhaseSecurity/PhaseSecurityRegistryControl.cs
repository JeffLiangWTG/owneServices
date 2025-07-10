using Enterprise.Registry.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class PhaseSecurityRegistryControl : RegistryZUserControl
	{
		public PhaseSecurityRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			IsEnabledCheckBox.ReadOnly = readOnly;
			PhasesGrid.ReadOnly = readOnly;
			RulesGrid.ReadOnly = readOnly;
		}
	}
}
