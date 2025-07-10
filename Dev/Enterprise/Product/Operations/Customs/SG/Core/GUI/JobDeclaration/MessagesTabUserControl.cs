using System;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging;
using Enterprise.Customs.SG.V4.Business.PermitPrinting;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.GUI
{
	public partial class MessagesTabUserControl : Customs.GUI.BaseMessagesTabUserControl
	{
		public MessagesTabUserControl()
		{
			InitializeComponent();

			MessageDetailsTabPage.Controls.Remove(InterpretedMessageTextBox);
			MessageDetailsTabPage.Controls.Add(InterpretedMessageHtmlBox);

			MessagesGrid.OnResendInterchange = DoResendInterchange;
			MessagesGrid.ContextMenu.MenuItems.Add(new ZMenuItem("Print Permit", new EventHandler(PrintPermit_Click)));
			MessagesGrid.SetColumnCaption(EDIMessageSchema.Constants.EM_ApplicationReference, "URN");
			MessagesGrid.SetColumnVisible(true, EDIMessageSchema.Constants.EM_ApplicationReference);
		}

		void DoResendInterchange()
		{
			if (MessagesGrid.SelectedElements.Length == 1)
			{
				foreach (EDIMessage message in MessagesGrid.SelectedElements)
				{
					if (message.EM_ReceiveTransmit != EDIMessage.Direction.Transmit)
					{
						Globals.Message.ShowError("Only outgoing interchanges can be resent.");
					}
					else if (message.Interchange == null)
					{
						Globals.Message.ShowError("This message has no interchange, thus it cannot be resent.");
					}
					else if (message.Interchange.EI_Status == EDIInterchange.Status.Queued || message.Interchange.EI_Status == EDIInterchange.Status.eHubQueued || message.Interchange.EI_Status == EDIInterchange.Status.eHubPending)
					{
						Globals.Message.Show("This interchange is already queued for sending.");
					}
					else
					{
						var outcome = Business.BatchProcessor.SGInterchangeResender.GetInstance(message.Interchange).Resend(true) ? "successfully queued to be" : "could not be";
						Globals.Message.Show("Interchange " + outcome + " resent.");
					}
				}
			}
			else
			{
				Globals.Message.ShowInformation("Select 1 message before trying to resend the interchange.");
			}
		}

		void PrintPermit_Click(object sender, EventArgs e)
		{
			var printPermitProcessor = new PrintPermitProcessor();

			if (MessagesGrid.SelectedElements.Length > 0)
			{
				foreach (EDIMessage message in MessagesGrid.SelectedElements)
				{
					if (message.EM_MessageType == Cuspmt09bMessageProcessor.MessageType)
					{
						if (PrintPermitProcessor.IsGeneralDocument(message))
						{
							Globals.Message.ShowInformation("This message is a refund permit notification - printing is not applicable.");
						}
						else
						{
							printPermitProcessor.DoSimplePrint(message);
						}
					}
					else
					{
						Globals.Message.ShowInformation("This message is not a permit message.");
					}
				}
			}
			else
			{
				Globals.Message.ShowInformation("Select a message to print.");
			}
		}
	}
}
