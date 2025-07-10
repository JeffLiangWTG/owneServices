using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.NL
{
	sealed class PortbaseExportNotificationMessagingExtensions : BaseMessagingExtensions
	{
		public PortbaseExportNotificationMessagingExtensions(IDocument document, ForwardingConsol consol)
		{
			this.consol = consol;
			portbaseExportNotification = document?.Data.Value as PortbaseExportNotification;
			Argument.NotNull(portbaseExportNotification, nameof(portbaseExportNotification));
		}

		readonly ForwardingConsol consol;
		readonly PortbaseExportNotification portbaseExportNotification;

		#region BaseMessagingExtensions members

		public override bool? ContinueWithSendingMessageWithdrawal(IUserNotifications notifications)
		{
			var notAcceptedMRNs = portbaseExportNotification
				.Documents
				.Where(d => d.IsSelectedToSend && !d.CanWithdraw)
				.Select(d => d.ReferenceNumber)
				.ToArray();

			if (notAcceptedMRNs.Length > 0)
			{
				var message =
					Res.GetString("612fdda9-5e17-46d8-ad22-d4fd9b9f75d1", "Message can be withdrawn only after you have received an acceptance response to the previous message.") + System.Environment.NewLine +
					Res.GetString("483178b9-f365-45d4-a27c-ad90ae20317d", "Message Acceptance not received for MRN: {0}", string.Join(", ", notAcceptedMRNs));
				var information = Res.GetString("cc9a905d-18fd-4f4f-a3ee-3316cbe45cb3", "Information");
				notifications?.ShowMessage(message, information);

				return false;
			}

			return true;
		}

		public override bool? IsSendingAmendment()
		{
			var lastSentMessageEventLog = consol.Logs.GetAllLogs().OfType<StmALog>().Where(log => IsSentExportNotificationMessageLog(log)).OrderByDescending(log => log.SL_PostedTimeUtc).FirstOrDefault();
			if (lastSentMessageEventLog != null && lastSentMessageEventLog.SL_SE_NKEvent == Events.StatusUpdatedCode)
			{
				return false;
			}

			var selectedMRNs = portbaseExportNotification
				.Documents
				.Where(d => d.IsSelectedToSend)
				.Select(d => d.ReferenceNumber)
				.ToArray();

			foreach (var mrn in selectedMRNs)
			{
				var lastEventCode = GetEventLogsForMRNInDescendingOrder(mrn)
					.Where(log => log.SL_SE_NKEvent == Events.MessageAcceptedCode || log.SL_SE_NKEvent == Events.MessageWithdrawCancelAcceptedCode)
					.Select(log => log.SL_SE_NKEvent)
					.FirstOrDefault();
				if (lastEventCode == Events.MessageAcceptedCode)
				{
					return true;
				}
			}

			return false;
		}

		#endregion

		#region Implementation

		IEnumerable<StmALog> GetEventLogsForMRNInDescendingOrder(string mrn)
		{
			return consol
				.Logs?
				.GetAllLogs()
				.OfType<StmALog>()
				.OrderByDescending(log => log.SL_PostedTimeUtc)
				.Where(log => !log.SL_IsCancelled && IsPortbasExportNotificationLog(log) && log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsReferenceNumber, out var mrnlog) && mrnlog.ToUpper() == mrn);
		}

		bool IsPortbasExportNotificationLog(StmALog log)
		{
			switch (log.SL_SE_NKEvent)
			{
				case Events.MessageSentCode:
				case Events.MessageWithdrawCancelRequestCode:
				case Events.InterchangeReceiptAcknowledgedCode:
				case Events.InterchangeRejectedCode:
				case Events.MessageAcceptedCode:
				case Events.MessageReceivedCode:
				case Events.MessageRejectedCode:
				case Events.MessageWithdrawCancelAcceptedCode:
				case Events.StatusUpdatedCode:
					return (string.Compare(log.Parameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department), DutchPortsConstants.Departments.Portbase, StringComparison.OrdinalIgnoreCase) == 0 &&
						string.Compare(log.Parameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType), DutchPortsConstants.MessageTypes.ExportNotification, StringComparison.OrdinalIgnoreCase) == 0);
				default:
					return false;
			}
		}

		bool IsSentExportNotificationMessageLog(StmALog log)
		{
			if (string.Compare(log.Parameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType), ConsolDocumentNames.ExportNotification, StringComparison.OrdinalIgnoreCase) != 0)
			{
				return false;
			}

			switch (log.SL_SE_NKEvent)
			{
				case Events.MessageSentCode:
				case Events.MessageWithdrawCancelRequestCode:
					return true;
				case Events.StatusUpdatedCode:
					return string.Compare(log.Parameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type), Core.Constants.EventReferenceMessageTypes.ResetToOriginal, StringComparison.OrdinalIgnoreCase) == 0;
				default:
					return false;
			}
		}

		#endregion
	}
}
