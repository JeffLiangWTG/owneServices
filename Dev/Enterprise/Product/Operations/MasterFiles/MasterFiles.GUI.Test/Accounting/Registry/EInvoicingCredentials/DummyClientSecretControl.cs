using System.Reflection;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class DummyClientSecretControl : EInvoicingCredentialsControl
	{
		public DummyClientSecretControl() : base(EInvoicingCredentialsRegistryItem.Behavior.SetPasswordChar)
		{
		}

		protected override bool IsValidPassword(DeveloperLoginForm loginForm)
		{
			((ZTextBox)typeof(DeveloperLoginForm).GetField("PasswordTextBox", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(loginForm)).Text = LoginPassword;
			return base.IsValidPassword(loginForm);
		}

		public string LoginPassword;
	}
}
