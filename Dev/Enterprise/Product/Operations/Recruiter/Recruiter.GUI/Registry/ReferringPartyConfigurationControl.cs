using Enterprise.Registry.GUI;

#pragma warning disable IDE0001 // Simplify names. Designer requires fully qualified names to correctly deserialize properties
#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Recruiter.GUI
{
	public partial class ReferringPartyConfigurationControl : RegistryZUserControl
	{
		public ReferringPartyConfigurationControl()
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
