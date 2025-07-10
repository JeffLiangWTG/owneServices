using System;
using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Documents.GUI
{
	public partial class AlertHyperLinkForm : ZChildForm, IAlertHyperLinkForm
	{
		public AlertHyperLinkForm()
		{
			InitializeComponent();
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		void HyperlinkLabelClick(object sender, LinkLabelLinkClickedEventArgs e)
		{
			WebUrlLauncher.Launch("https://wisetechacademy.com/search?quickstart=c3c83405-4c62-4988-9c24-7dbbe85bf9c5");
		}

		public ZDialogResult ShowDialogAndGetResult()
		{
			ZFormModaliser.ShowDialogWithoutDispose(this);
			return (ZDialogResult)DialogResult;
		}
	}
}
