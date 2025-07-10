using System.Windows.Forms;

namespace WinzorFramework.Samples
{
	public partial class WIDetailsForm : Form
	{
		public WIDetailsForm()
		{
			InitializeComponent();
#if WINZOR
			this.richTextBox1.IsToolBarVisible = true;
#endif
		}
	}
}
