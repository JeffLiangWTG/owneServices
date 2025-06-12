using System.Windows.Forms;

namespace CargoWise.eHub.BizTalkAdapters.Common.UI
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
				this.lblPrompt.Text = value;
			}
		}

		public string Password
		{
			get
			{
				return this.txtPassword.Text;
			}
		}
	}
}
