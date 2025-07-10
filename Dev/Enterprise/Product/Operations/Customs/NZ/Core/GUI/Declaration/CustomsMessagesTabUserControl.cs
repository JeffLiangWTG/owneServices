using System;
using System.Windows.Forms;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.Declaration
{
	public partial class CustomsMessagesTabUserControl : Customs.GUI.BaseMessagesTabUserControl
	{
		public delegate void ReprocessMessage();
		public ReprocessMessage OnReprocessMessage;

		public CustomsMessagesTabUserControl()
		{
			InitializeComponent();
			AddContextMenuItems();
		}

		void AddContextMenuItems()
		{
			if (GlbStaff.CurrentUser.IsCurrentUserLocalAdminForThisStaff || GlbStaff.CurrentUser.IsSupportUser)
			{
				MessagesGrid.ContextMenu.MenuItems.Add(new ZMenuItem("Reprocess Message", ReprocessMessage_Click));
				OnReprocessMessage = DoReprocessMessage;
			}
		}

		void ReprocessMessage_Click(object sender, EventArgs e)
		{
			OnReprocessMessage();
		}

		void DoReprocessMessage()
		{
			var message = MessagesGrid.ListManager?.GetCurrent() as EDIMessage;

			if (message != null)
			{
				var entryHeader = message.EM_LinkedObject as CusEntryHeader;

				if (entryHeader == null || !entryHeader.CH_IsActive)
				{
					Globals.Message.ShowError("Cannot reprocess any messages for an inactive entry header.");
				}
				else if (MessageReprocessor.CanReprocessMessage(message, entryHeader.Messages.GetLastMessage(EDIMessage.ApplicationCodes.NewZealandCustoms, NZ.TradeSingleWindow.MessageTypeList.Codes.TWR, EDIMessage.Direction.Receive))
					&& Globals.Message.ShowConfirmation
					(
						Res.GetString("A9F1449A-4FAC-4147-98DF-E7F25131045B", "Reprocessing the selected message may update the entry status and entry number for this job. Are you sure you want to proceed?"),
						Res.GetString("703EDB1E-10BC-4A22-B77A-66A500FBE792", "Reprocess Message"),
						Res.GetString("9AB72617-5A52-4422-91CC-C062B09782EA", "Reprocess"),
						MessageBoxIcon.Warning
					) == DialogResult.OK)
				{
					message.QueueMessageForReprocess();
				}
			}
			else
			{
				Globals.Message.ShowError("A single response message is required to be selected for reprocess.");
			}
		}
	}
}
