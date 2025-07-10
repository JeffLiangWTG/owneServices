using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Business;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.NL
{
	sealed class PortbaseImportNotificationMessagingExtensions : BaseMessagingExtensions
	{
		public PortbaseImportNotificationMessagingExtensions(IStmALogParent logParent, IMessageInstructions messageInstructions)
		{
			this.logParent = Argument.NotNull(logParent, nameof(logParent));
			this.messageInstructions = Argument.NotNull(messageInstructions, nameof(messageInstructions));
		}

		readonly IStmALogParent logParent;
		readonly IMessageInstructions messageInstructions;

		IDialog LastDialog => lastDialog ?? (lastDialog = Dialogs.LastOrDefault());
		IDialog lastDialog;

		IEnumerable<IDialog> Dialogs => dialogs ?? (dialogs = (GetDocumentData() as IStmALogParent)?.GetDialogs(messageInstructions.DocumentName, false));
		IEnumerable<IDialog> dialogs;

		IVisualizerDocumentData GetDocumentData()
		{
			var documentDataLoader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();
			return documentDataLoader.Load((BusinessObject)logParent, ConsolDocumentDataStoreNames.PortbaseImportNotification);
		}

		public override bool? ContinueWithSendingMessageAmendment(IUserNotifications notifications)
		{
			var transmissionCode = LastDialog?.TransmissionCode ?? string.Empty;

			if (transmissionCode == Events.MessageSentCode && GetReceivedCount() == 0)
			{
				var caption = Res.GetString("4D8E5393-501A-4C73-9CA3-0618C50BE160", "Warning");
				var notAcceptedMessage = Res.GetString("0D94CF9C-F14C-4FC9-BC68-C6D5BE2B95BD", "Message can be amended only after you have received a response to the previous message.");
				notifications.ShowMessage(notAcceptedMessage, caption);

				return false;
			}

			return null;
		}
		public override bool? ContinueWithSendingMessageWithdrawal(IUserNotifications notifications) => false;

		int GetReceivedCount()
		{
			var count = 0;

			foreach (var dialog in Dialogs)
			{
				if (dialog.HasBeenResetToOriginal())
				{
					count = 0;
				}
				else if ((dialog.HasBeenAccepted() || dialog.HasBeenRejected()) && !dialog.IsWithdrawal())
				{
					count++;
				}
			}
			return count;
		}
	}
}
