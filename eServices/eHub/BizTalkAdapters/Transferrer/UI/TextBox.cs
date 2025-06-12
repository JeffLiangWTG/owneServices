using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CargoWise.eHub.BizTalkAdapters.Transferrer.UI
{
	public partial class TextBox : Form
	{
		public string TextBoxData { get; set; }

		public TextBox()
		{
			InitializeComponent();
		}

		private void TextBox_Load(object sender, EventArgs e)
		{
			txtData.Text = TextBoxData;
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			TextBoxData = txtData.Text;
		}

		private void txtData_TextChanged(object sender, EventArgs e)
		{
			txtData.Text = Regex.Replace(txtData.Text, @"(?<!\r)\n", "\r\n");
		}
	}
}
