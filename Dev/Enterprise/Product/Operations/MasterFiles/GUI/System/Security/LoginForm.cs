using CargoWise.Windows.UI;
using Enterprise.Security;

namespace Enterprise.MasterFiles.GUI
{
	public partial class LoginForm : KForm
	{
		public LoginForm()
		{
			InitializeComponent();
		}

		public string Message
		{
			get { return SecurityMessageTextBox.Text; }
			set { SecurityMessageTextBox.Text = value; }
		}

#if DEBUG

		public virtual void DoLoginForTest(string loginName, string password)
		{
			fCredentials = new AlternativeCredentials(loginName, password);
		}

#endif

		public AlternativeCredentials Credentials
		{
			get { return fCredentials; }
		}

		protected virtual void OKButton_Click(object sender, System.EventArgs e)
		{
			fCredentials = new AlternativeCredentials(LoginTextBox.Text, PasswordTextBox.Text);
			Close();
		}

		void CancelXButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		protected AlternativeCredentials fCredentials;
	}
}
