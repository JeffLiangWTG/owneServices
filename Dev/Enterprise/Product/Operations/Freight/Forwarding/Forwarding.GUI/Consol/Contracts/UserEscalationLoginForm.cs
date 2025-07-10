using CargoWise.Windows.UI;
using Enterprise.Security;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class UserEscalationLoginForm : KForm
	{
		public UserEscalationLoginForm()
		{
			InitializeComponent();
		}

		public string Message
		{
			get { return MessageLabel.Text; }
			set { MessageLabel.Text = value; }
		}

		public AlternativeCredentials Credentials
		{
			get { return credentials; }
		}

		void OKButton_Click(object sender, System.EventArgs e)
		{
			credentials = new AlternativeCredentials(UsernameTextBox.Text, PasswordTextBox.Text);
			Close();
		}

		void CancelXButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		AlternativeCredentials credentials;
	}
}
