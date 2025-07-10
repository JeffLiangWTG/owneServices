using System;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Documents.GUI
{
	public partial class DeliverDocumentPopupForm : ZChildForm, IDeliverDocumentPopupForm
	{
		public DeliverDocumentPopupForm(string documentName)
		{
			InitializeComponent();
			SetText(documentName);
		}

		protected DeliverDocumentPopupAction result = DeliverDocumentPopupAction.NoAction;

		void DeliverDocumentButton_Click(object sender, EventArgs e) => ReturnActionAndClose(DeliverDocumentPopupAction.DeliverDocument);

		void SendMessageButton_Click(object sender, EventArgs e) => ReturnActionAndClose(DeliverDocumentPopupAction.SendMessage);

		void ReturnActionAndClose(DeliverDocumentPopupAction action)
		{
			result = action;
			Close();
		}

		void SetText(string formName)
		{
			this.ShowLabel.Text = Res.GetString("DeliverDocumentPopupForm|Information", "Your CargoWise system is configured to send and receive electronic messages with this carrier.\r\nPress \"Send Message\" to send electronic message to the carrier.\r\nPress \"Deliver Document\" to proceed with manual delivery of {0}", formName);
		}

		DeliverDocumentPopupAction IDeliverDocumentPopupForm.ShowDialogAndGetResult()
		{
			ZFormModaliser.ShowDialogWithoutDispose(this);
			return this.result;
		}
	}
}
