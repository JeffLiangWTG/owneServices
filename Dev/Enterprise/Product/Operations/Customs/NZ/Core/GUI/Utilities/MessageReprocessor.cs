using System.Windows.Forms;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Customs.NZ.GUI.Res;

namespace Enterprise.Customs.NZ
{
	public static class MessageReprocessor
	{
		public static bool CanReprocessMessage(EDIMessage messageToReprocess, EDIMessage latestResponseMessage)
		{
			var reprocessMessage = false;
			if (messageToReprocess == null)
			{
				Globals.Message.ShowError("Please select a valid Message.");
			}
			else
			{
				if (messageToReprocess.IsTransmitMessage)
				{
					Globals.Message.ShowError("Cannot reprocess this message. Only response messages can be reprocessed.");
				}
				else if (latestResponseMessage != null)
				{
					if (messageToReprocess != latestResponseMessage)
					{
						if (Globals.Message.Show(Res.GetString("6D489AEE-2392-4AD1-A4C8-1A9CFD186491", "Message {0} is not the most recent response. Are you sure you wish to reprocess this message?", messageToReprocess.EM_MessageNum), Res.GetString("B459B735-D894-415F-B6EA-33E9CB8EEF66", "Reprocess Message?"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
						{
							reprocessMessage = true;
						}
					}
					else
					{
						reprocessMessage = true;
					}
				}
				else
				{
					reprocessMessage = true;
				}
			}
			return reprocessMessage;
		}

		public static void QueueMessageForReprocess(this EDIMessage messageToReprocess)
		{
			messageToReprocess.EM_Status = EDIMessage.Status.Queued;
			messageToReprocess.Logs.AddNew(ZArchitecture.Business.AutoEvents.MessagePendingProcessing, "NZC - Reprocess");
			messageToReprocess.Factory.Save();
		}
	}
}
