using System.Windows.Forms;

namespace CargoWise.eHub.BizTalkAdapters.Transferrer.UI
{
	public partial class PasswordPrompt : Form
	{
		public PasswordPrompt()
		{
			InitializeComponent();
		}

		public string Prompt
		{
			set
			{
				lblPrompt.Text = value;
			}
		}

		public string Password
		{
			get
			{
				return txtPassword.Text;
			}
		}
	}
}
