using System.Windows.Forms;

namespace WinzorFramework.Samples
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
		}

		void WIDetailsBtn_Click(object sender, System.EventArgs e)
		{
			var form = new WIDetailsForm();
			form.Show();
		}

		void CommonControlBtn_Click(object sender, System.EventArgs e)
		{
			var form = new CommonControlsForm();
			form.Show();
		}
	}
}
