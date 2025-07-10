using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;
using Constants = CargoWise.EventReference.Constants;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	sealed class CINExportNotificationExtensions : BaseMessagingExtensions
	{
		public CINExportNotificationExtensions(IDocument document, IStmALogParent logParent, IMessageInstructions messageInstructions)
		{
			Argument.NotNull(document, nameof(document));
			exportNotification = document?.Data.Value as CINExportNotification;
			Argument.NotNull(exportNotification, nameof(exportNotification));

			this.logParent = Argument.NotNull(logParent, nameof(logParent));
			this.messageInstructions = Argument.NotNull(messageInstructions, nameof(messageInstructions));
			messageContext = new FrenchMessageContext(logParent, messageInstructions.DocumentName, ShipmentDocumentDataStoreNames.CINExportNotification);
		}

		readonly CINExportNotification exportNotification;
		readonly FrenchMessageContext messageContext;
		readonly IStmALogParent logParent;
		readonly IMessageInstructions messageInstructions;

		public override bool? ContinueWithSendingMessageAmendment(IUserNotifications notifications)
		{
			var result = true;

			if (LogsInDescendingOrder != null)
			{
				foreach (var log in LogsInDescendingOrder)
				{
					if (log.SL_SE_NKEvent == Events.StatusUpdatedCode && log.Parameters.Contains(new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Type, EventReferenceMessageTypes.ResetToOriginal)))
					{
						break;
					}
					else if (log.SL_SE_NKEvent == Events.StatusUpdatedCode && log.Parameters.Contains(new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Type, (NoResString)"FFM and departure message received"))) // It is a part of a EventReference which is not localizable
					{
						var caption = Res.GetString("0e599d74-4405-4ecb-933d-8ab1e9c0abfd", "Warning");
						var notAcceptedMessage = Res.GetString("e03604d8-b07f-4abe-927d-2fc14558f1fc", "Amendment will no longer be accepted by CIN once FFM and departure message are received.");
						notifications.ShowMessage(notAcceptedMessage, caption);
						result = false;
						break;
					}
				}
			}

			return result;
		}

		public override bool? ContinueWithSendingMessageWithdrawal(IUserNotifications notifications)
		{
			if (LogsInDescendingOrder?.Any(log => log.SL_SE_NKEvent == Events.StatusUpdatedCode && log.Parameters.GetValueSafe(Constants.EventReferenceParameters.Codes.Type) == (NoResString)"FFM and departure message received") ?? false)
			{
				var caption = Res.GetString("3c1e0b3a-3fd3-4351-aa2f-5c6c38410fa6", "Warning");
				var notAcceptedMessage = Res.GetString("0a1d27ba-d1fb-4f10-87fe-1a9d2e8ba149", "Withdrawal will no longer be accepted by CIN once FFM and departure message are received.");
				notifications.ShowMessage(notAcceptedMessage, caption);

				return false;
			}

			if (!Dialogs.Any())
			{
				notifications.ShowMessage(Res.GetString("a0d29248-3887-4959-85d2-d66c88434317", "There's no message to withdraw."), Res.GetString("8fc13dd1-3a6a-4084-adfb-d3484d8094e2", "Sending Withdraw/Cancel Request"));

				return false;
			}

			if (!HasReceivedResponse(Dialogs.Last()))
			{
				notifications.ShowMessage(Res.GetString("d1d23f11-73d1-4059-a357-5c6433ed92ff", "Message can be withdrawn only after you have received a response to the previous message."), Res.GetString("812ec34f-d330-47c2-9611-3b1344444cad", "Sending Withdraw/Cancel Request"));

				return false;
			}

			return true;
		}

		public override bool? ContinueWithResetToOriginal(IUserNotifications notifications) => messageContext.ContinueWithResetToOriginal(notifications);

		public override bool? IsSendingAmendment()
		{
			var isSendingAmendment = false;

			if (LogsInDescendingOrder != null)
			{
				foreach (var log in LogsInDescendingOrder)
				{
					if (log.SL_SE_NKEvent == Events.StatusUpdatedCode && log.Parameters.Contains(new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Type, EventReferenceMessageTypes.ResetToOriginal)))
					{
						break;
					}
					else if (MessageEventCodes.SentMessagesEventCodes.Contains(log.SL_SE_NKEvent.ToString()))
					{
						isSendingAmendment = true;
					}
				}

				if (isSendingAmendment
						&& LogsInDescendingOrder.Any(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode)
						&& !LogsInDescendingOrder.Any(l => l.SL_SE_NKEvent == Events.InterchangeReceiptAcknowledgedCode))
				{
					isSendingAmendment = false;
				}
			}

			return isSendingAmendment;
		}

		StmALog[] LogsInDescendingOrder => logsInDescendingOrder ?? (logsInDescendingOrder = GetEventLogsInDescendingOrder().ToArray());
		StmALog[] logsInDescendingOrder;

		IDialog[] Dialogs => dialogs ?? (dialogs = GetDialogs().ToArray());
		IDialog[] dialogs;

		#region Implementation

		public IEnumerable<IDialog> GetDialogs()
		{
			var documentDataLoader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();
			var documentData = documentDataLoader.Load((BusinessObject)logParent, ShipmentDocumentDataStoreNames.CINExportNotification);

			var isDialogInitiatingEvent = (StmALog log) => MessageEventCodes.SentMessagesEventCodes.Contains(log.SL_SE_NKEvent.ToString())
					|| (log.SL_SE_NKEvent == Events.StatusUpdatedCode && log.Parameters.GetValueSafe(Constants.EventReferenceParameters.Codes.Type) == EventReferenceMessageTypes.ResetToOriginal);

			return (documentData as IStmALogParent)?.GetDialogs(messageInstructions.DocumentName, messageInstructions.OrderLogsByLocalTime, isDialogInitiatingEvent)
				?? Enumerable.Empty<IDialog>();
		}

		ZBool HasReceivedResponse(IDialog dialog)
		{
			if (dialog == null)
			{
				return false;
			}

			return dialog.Logs.Any(log => MessageEventCodes.MessageStatusConfirmationEventCodes.Contains(log.SL_SE_NKEvent.ToString())
				|| (log.SL_SE_NKEvent == Events.StatusUpdatedCode && log.Parameters.GetValueSafe(Constants.EventReferenceParameters.Codes.Type) != EventReferenceMessageTypes.ResetToOriginal));
		}

		IEnumerable<StmALog> GetEventLogsInDescendingOrder()
		{
			var documentDataLoader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();
			var documentData = documentDataLoader.Load((BusinessObject)logParent, ShipmentDocumentDataStoreNames.CINExportNotification);

			return (documentData as IStmALogParent)
				.Logs?
				.GetAllLogs()
				.OfType<StmALog>()
				.Where(log => !log.SL_IsCancelled && string.Compare(log.Parameters.GetValueSafe(Constants.EventReferenceParameters.Codes.MessageType), FrenchPortsConstants.DocumentNames.CINExportNotification, StringComparison.OrdinalIgnoreCase) == 0)
				.OrderByDescending(log => log.SL_PostedTimeUtc);
		}

		#endregion
	}
}
