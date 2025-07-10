using Enterprise.Registry.GUI;

namespace Enterprise.Customs.TR.GUI
{
	public partial class NCTSPhase5CredentialsRegistryItemUserControl : RegistryZUserControl
	{
		public NCTSPhase5CredentialsRegistryItemUserControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			BasicAuthUsernameTextBox.ReadOnly = readOnly;
			BasicAuthPasswordTextBox.ReadOnly = readOnly;
			FirmIDTextBox.ReadOnly = readOnly;
			RequestUserIDTextBox.ReadOnly = readOnly;
			RequestPasswordTextBox.ReadOnly = readOnly;
		}
	}
}
