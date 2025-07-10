using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static CargoWise.EventReference.Constants;

namespace Enterprise.Warehouse.Transit.Document
{
	public abstract class CIN750NotificationExtensions<TNotification> : BaseMessagingExtensions where TNotification : CIN750Notification
	{
		public CIN750NotificationExtensions(IDocument document, IMessageInstructions messageInstructions)
		{
			Argument.NotNull(document, nameof(document));
			docDataObject = document?.Data.Value as TNotification;
			Argument.NotNull(docDataObject, nameof(docDataObject));

			Argument.NotNull(messageInstructions, nameof(messageInstructions));
		}

		readonly protected TNotification docDataObject;

		public abstract override bool? ContinueWithSendingMessage(IUserNotifications notifications);

		public override bool? ContinueWithSendingMessageAmendment(IUserNotifications notifications) => false;

		public override bool? ContinueWithSendingMessageWithdrawal(IUserNotifications notifications) => false;

		public override string GetMessageStatus()
		{
			var documentDataLoader = CargoWise.Application.ObjectFactory.Get<IVisualizerDocumentDataLoader>();
			var documentData = documentDataLoader.Load(docDataObject.SourceBusinessObject, GetDataStoreName()) as IStmALogParent;

			var lastLog = documentData?.Logs.GetAllLogs().Cast<StmALog>()
				.Where(log => MessageEventCodes.MessageStatusEventCodes.Contains(log.SL_SE_NKEvent.ToString()) && CheckIsMessageStatusLog(log))
				.OrderByDescending(log => log.SL_PostedTimeUtc).FirstOrDefault();
			if (lastLog != null)
			{
				switch (lastLog.SL_SE_NKEvent)
				{
					case AutoEvents.MessageReceivedCode:
						{
							return Res.GetString("03f19a60-70a1-458f-85ce-31477f059b15", "The last CIN Message has been sent successfully.");
						}
					case AutoEvents.MessageRejectedCode:
						{
							var parameters = StmALog.GetParametersFromReference(lastLog.SL_Reference);
							if (parameters.TryGetValue(EventReferenceParameters.Codes.Reason, out var reason))
							{
								return Res.GetString("3b7c70ff-ea85-4a14-9ad7-e84624902186", "The last CIN Message has been sent but been rejected. Reason: {0}", reason);
							}
							else
							{
								return Res.GetString("477e435e-f149-43b0-b774-7f479ef16256", "The last CIN Message has been sent but been rejected.");
							}
						}
					case AutoEvents.MessageSentCode:
						{
							return Res.GetString("ae05e173-6080-4277-95f5-aacf6d7fc688", "The last CIN Message has been sent and is waiting for response.");
						}
					default:
						{
							return Res.GetString("efb14933-ed86-4c2d-9460-089275e12f44", "Unknown Message Status.");
						}
				}
			}
			else
			{
				return Res.GetString("55b05d79-a586-4d4a-a240-15c41672bb68", "No CIN Message has been sent.");
			}
		}

		public override bool? ContinueWithResetToOriginal(IUserNotifications notifications) => false;

		public override bool? IsSendingAmendment() => false;

		protected bool? ContinueWithSendingMessageCore(ZString message, IUserNotifications notifications)
		{
			docDataObject.PopulateCINMessageNote(message);
			if (!message.IsEmpty)
			{
				notifications.ShowMessage(message, MessageTitle);
				return false;
			}
			return true;
		}

		bool CheckIsMessageStatusLog(StmALog log)
		{
			return log != null && MessageEventCodes.MessageStatusEventCodes.Contains(log.SL_SE_NKEvent.ToString())
				&& !log.SL_Reference.StartsWith((NoResString)"Propagated: ");
		}

		protected abstract string GetDataStoreName();

		protected virtual string MessageTitle => Res.GetString("c9969c69-f549-4e77-b5ad-e7d2e75eeb68", "Cannot send Message.");
	}
}
