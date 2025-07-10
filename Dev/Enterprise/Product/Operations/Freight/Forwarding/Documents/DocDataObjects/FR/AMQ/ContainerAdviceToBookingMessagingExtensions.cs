using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	sealed class ContainerAdviceToBookingMessagingExtensions : BaseMessagingExtensions
	{
		public ContainerAdviceToBookingMessagingExtensions(IDocument document, IStmALogParent logParent, IMessageInstructions messageInstructions)
		{
			Argument.NotNull(document, nameof(document));
			containerAdviceToBooking = document?.Data.Value as ContainerAdviceToBooking;
			Argument.NotNull(containerAdviceToBooking, nameof(containerAdviceToBooking));

			Argument.NotNull(messageInstructions, nameof(messageInstructions));
			messageContext = new FrenchMessageContext(logParent, messageInstructions.DocumentName, ConsolDocumentDataStoreNames.PortsContainerAdviceToBookingAMQ);

			this.logParent = Argument.NotNull(logParent, nameof(logParent));
		}

		readonly ContainerAdviceToBooking containerAdviceToBooking;
		readonly FrenchMessageContext messageContext;
		readonly IStmALogParent logParent;

		public override bool? ContinueWithSendingMessage(IUserNotifications notifications)
		{
			bool dosMessageAccepted = false;

			foreach (var log in GetDOSEventLogsInDescendingOrder())
			{
				switch (log.SL_SE_NKEvent)
				{
					case Events.MessageAcceptedCode:
						dosMessageAccepted = true;
						break;
					default:
						break;
				}
				break;
			}

			if (!dosMessageAccepted)
			{
				var caption = Res.GetString("FA9FD3D7-647E-4B2D-9718-EB8FC4418F92", "Warning");
				var notAcceptedMessage = Res.GetString("981476A7-D0A3-4B2A-AB02-1CC1A5FEF568", "The Container Advice to Booking (AMQ) message cannot be submitted until the \"File Creation Request\" (DOS) message has been accepted by the Terminal.");
				notifications.ShowMessage(notAcceptedMessage, caption);
				return false;
			}

			return null;
		}

		IEnumerable<StmALog> GetDOSEventLogsInDescendingOrder()
		{
			if (logParent is ForwardingConsol consol)
			{
				foreach (var log in consol.Logs.GetAllLogs().OfType<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc))
				{
					var messageType = log.Parameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType);

					if (string.Compare(messageType, FrenchPortsConstants.DocumentNames.DOSExport, System.StringComparison.OrdinalIgnoreCase) == 0 ||
						string.Compare(messageType, FrenchPortsConstants.DocumentNames.DOSImport, System.StringComparison.OrdinalIgnoreCase) == 0)
					{
						yield return log;
					}
				}
			}
		}

		public override bool? ContinueWithSendingMessageAmendment(IUserNotifications notifications) => messageContext.ContinueWithSendingMessageAmendment(notifications);

		public override bool? ContinueWithSendingMessageWithdrawal(IUserNotifications notifications) => messageContext.ContinueWithSendingMessageWithdrawal(notifications);

		public override bool? ContinueWithResetToOriginal(IUserNotifications notifications) => messageContext.ContinueWithResetToOriginal(notifications);
	}
}
