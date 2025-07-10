using System;
using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Documents.GUI
{
	public partial class InformationHyerLinkForm : ZChildForm, IInformationHyerLinkForm
	{
		readonly string MessageBody;
		readonly string MessageLink;

		public InformationHyerLinkForm(string messageBody, string messageLink)
		{
			MessageBody = messageBody;
			MessageLink = messageLink;
			InitializeComponent();
		}

		public ZDialogResult ShowDialogAndGetResult()
		{
			ZFormModaliser.ShowDialogWithoutDispose(this);
			return (ZDialogResult)DialogResult;
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		void HyperlinkLabelClick(object sender, LinkLabelLinkClickedEventArgs e)
		{
			WebUrlLauncher.Launch(MessageLink);
		}
	}
}
