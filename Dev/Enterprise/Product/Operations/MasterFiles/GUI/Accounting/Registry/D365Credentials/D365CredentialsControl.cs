using Enterprise.Registry.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class D365CredentialsControl : RegistryZUserControl
	{
		public D365CredentialsControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			ClientIdTextBox.ReadOnly = readOnly;
			ClientSecretTextBox.ReadOnly = readOnly;
			TenantIDTextBox.Enabled = !readOnly;
		}
	}
}
