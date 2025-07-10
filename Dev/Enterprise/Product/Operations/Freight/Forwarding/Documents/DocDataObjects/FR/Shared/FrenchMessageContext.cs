using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Business;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	public class FrenchMessageContext
	{
		public FrenchMessageContext(IStmALogParent logParent, string documentName, string dataStoreName)
		{
			this.logParent = Argument.NotNull(logParent, nameof(logParent));
			this.documentName = documentName;
			this.dataStoreName = dataStoreName;
		}

		readonly IStmALogParent logParent;
		readonly string documentName;
		readonly string dataStoreName;

		IDialog LastDialog => lastDialog ?? (lastDialog = Dialogs?.LastOrDefault());
		IDialog lastDialog;

		IEnumerable<IDialog> Dialogs => dialogs ?? (dialogs = (GetDocumentData() as IStmALogParent)?.GetDialogs(documentName, false));
		IEnumerable<IDialog> dialogs;

		IVisualizerDocumentData GetDocumentData()
		{
			var documentDataLoader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();
			return documentDataLoader.Load((BusinessObject)logParent, dataStoreName);
		}

		public bool? ContinueWithSendingMessageWithdrawal(IUserNotifications notifications)
		{
			var transmissionCode = LastDialog?.TransmissionCode ?? string.Empty;
			if (transmissionCode == Events.MessageSentCode && !LastDialog.HasBeenAccepted())
			{
				var caption = Res.GetString("2cc40705-6abe-4566-bcdf-92defb496828", "Sending Withdraw/Cancel Request");
				var notAcceptedMessage = Res.GetString("061a32ec-f2a0-4bdf-86e5-9a193edf0d1f", "Message can be withdrawn only after you have received an accepted response to the previous message.");
				notifications.ShowMessage(notAcceptedMessage, caption);

				return false;
			}

			return null;
		}

		public bool? ContinueWithSendingMessageAmendment(IUserNotifications notifications)
		{
			var transmissionCode = LastDialog?.TransmissionCode ?? string.Empty;

			if (transmissionCode == Events.MessageSentCode && GetAcceptedCount() == 0)
			{
				var caption = Res.GetString("c05b9d76-1293-4d69-8175-186a635544df", "Warning");
				var notAcceptedMessage = Res.GetString("85bb887b-fcac-4e74-9f56-ec79a767b4f0", "Message can be amended only after you have received an accepted response to the previous message.");
				notifications.ShowMessage(notAcceptedMessage, caption);

				return false;
			}

			return null;
		}

		public bool? ContinueWithResetToOriginal(IUserNotifications notifications)
		{
			var transmissionCode = LastDialog?.TransmissionCode ?? string.Empty;
			if (transmissionCode == Events.MessageSentCode && LastDialog.HasBeenAccepted())
			{
				var caption = Res.GetString("e6bc4f2b-d6ab-4352-9613-5693300b5231", "Information");
				var message = Res.GetString("3cade276-bfd8-4635-af05-706a4651ed72", "You cannot change the status to \"Reset to Original\" as the original message has already been accepted.");
				notifications.ShowMessage(message, caption);

				return false;
			}

			return null;
		}

		int GetAcceptedCount()
		{
			var count = 0;

			foreach (var dialog in Dialogs)
			{
				if (dialog.HasBeenResetToOriginal())
				{
					count = 0;
				}
				else if (dialog.HasBeenAccepted() && !dialog.IsWithdrawal())
				{
					count++;
				}
			}
			return count;
		}
	}
}
