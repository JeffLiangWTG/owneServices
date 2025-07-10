using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.NZ
{
	sealed class ExportPreAdviceNotificationExtensions : BaseMessagingExtensions
	{
		public ExportPreAdviceNotificationExtensions(IDocument document, ForwardingConsol consol)
		{
			Argument.NotNull(document, nameof(document));
			exportPreAdviceNotification = document?.Data.Value as ExportPreAdviceNotification;
			Argument.NotNull(exportPreAdviceNotification, nameof(exportPreAdviceNotification));
			this.consol = Argument.NotNull(consol, nameof(consol));
		}

		readonly ExportPreAdviceNotification exportPreAdviceNotification;
		readonly ForwardingConsol consol;

		public override bool? ContinueWithSendingMessageWithdrawal(IUserNotifications notifications)
		{
			var notAcceptedNumbers = exportPreAdviceNotification.Containers.Where(c => c.EPANStatus != FreightConstants.NZExportPreAdviceStatus.Codes.Accepted).Select(c => c.Number).ToList();
			if (notAcceptedNumbers.Count > 0)
			{
				var message =
					Res.GetString("08c3992a-9a95-49ba-8b87-0ce7d88cd6fa", "Message can be withdrawn only after you have received an acceptance response to the previous message.") + System.Environment.NewLine +
					Res.GetString("1c06a898-db4f-4053-a271-2a35770c49d8", "Message Acceptance has not been received for container(s): {0}", string.Join(", ", notAcceptedNumbers));
				var information = Res.GetString("69ffa12c-bd06-4a75-8af2-96191054a57b", "Information");
				notifications?.ShowMessage(message, information);
				return false;
			}
			return true;
		}

		public override bool? IsSendingAmendment()
		{
			return exportPreAdviceNotification.Containers.Any(c =>
				c.EPANStatus == FreightConstants.NZExportPreAdviceStatus.Codes.Sent
				|| c.EPANStatus == FreightConstants.NZExportPreAdviceStatus.Codes.Accepted
				|| c.EPANStatus == FreightConstants.NZExportPreAdviceStatus.Codes.WithdrawalSent);
		}

		public override string GetMessageStatus()
		{
			var eventLogs = GetEventLogsInDescendingOrder();
			if (eventLogs.IsNullOrEmpty())
			{
				return Res.GetString("c59ec35e-490a-45de-b0ca-38008bd0f8a2", "No Messages have been Sent");
			}
			var lastLog = eventLogs.First();
			switch (lastLog.SL_SE_NKEvent)
			{
				case AutoEvents.MessageSentCode:
					return Res.GetString("82974cc8-2ce1-4824-899b-f1b3d4466e8b", "Message Sent");
				case AutoEvents.InterchangeRejectedCode:
					{
						string reason;
						StmALog.GetParametersFromReference(lastLog.SL_Reference).TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, out reason);
						return Res.GetString("1f52e974-7cfc-4e69-8006-08a73780b052", "Interchange Rejected because {0}", reason ?? string.Empty);
					}
				case AutoEvents.InterchangeSentCode:
					return Res.GetString("2388054a-e61b-4124-812e-6d98377cb6b0", "Interchange Sent");
				case AutoEvents.MessageAcceptedCode:
					return Res.GetString("e058b95f-fdd2-487c-9b66-532d4c110b28", "Message Accepted");
				case AutoEvents.MessageRejectedCode:
					return Res.GetString("544a1853-e917-4273-9951-512f989020a0", "Message Rejected");
				case AutoEvents.StatusUpdatedCode:
					return Res.GetString("3792405d-51a4-48ac-a8bc-ca5f9f95de1d", "Status Updated");
				case AutoEvents.MessageWithdrawCancelRequestCode:
					return Res.GetString("14389aae-1e64-4ec1-9cdd-e76e6e677947", "Message Withdraw/Cancel Sent");
				case AutoEvents.MessageWithdrawCancelAcceptedCode:
					return Res.GetString("226323e2-a283-406b-88f3-206197ac5665", "Withdrawal Accepted");
				default:
					return Res.GetString("5d2f4274-302f-4719-8fe4-81fd9c0d5e01", "Unknown Message Status");
			}
		}

		#region  Implementation

		IEnumerable<StmALog> GetEventLogsInDescendingOrder()
		{
			var documentDataLoader = CargoWise.Application.ObjectFactory.Get<IVisualizerDocumentDataLoader>();
			var documentData = documentDataLoader.Load(consol, ConsolDocumentDataStoreNames.ExportPreAdviceNotification) as IStmALogParent;
			return documentData?.Logs.GetAllLogs().Cast<StmALog>()
				.Where(log =>
					MessageEventCodes.MessageStatusEventCodes.Contains(log.SL_SE_NKEvent.ToString()) &&
					CheckIsMessageStatusLog(log))
				.OrderByDescending(log => log.SL_PostedTimeUtc);
		}

		bool CheckIsMessageStatusLog(StmALog log)
		{
			return log != null && MessageEventCodes.MessageStatusEventCodes.Contains(log.SL_SE_NKEvent.ToString())
				&& !log.SL_Reference.StartsWith((NoResString)"Propagated: ");
		}

		#endregion
	}
}
