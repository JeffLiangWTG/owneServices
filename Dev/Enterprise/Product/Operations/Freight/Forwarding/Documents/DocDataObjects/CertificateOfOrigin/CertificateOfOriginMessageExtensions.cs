using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Business;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.CertificateOfOrigin
{
	sealed class CertificateOfOriginMessagingExtensions : BaseMessagingExtensions
	{
		public CertificateOfOriginMessagingExtensions(IStmALogParent logParent, string shipmentDocumentName)
		{
			this.logParent = Argument.NotNull(logParent, nameof(logParent));
			this.shipmentDocumentName = shipmentDocumentName;
		}

		readonly IStmALogParent logParent;

		readonly string shipmentDocumentName;

		public override bool? ContinueWithSendingMessage(IUserNotifications notifications) => CheckContinueWithSendingMessage(notifications);

		public override bool? ContinueWithSendingMessageAmendment(IUserNotifications notifications) => IsSendingAmendment();

		public override bool? IsSendingAmendment() => Events.MessageAcceptedCode.Equals(GetLatestInterestingLogEvent()?.SL_SE_NKEvent, StringComparison.OrdinalIgnoreCase);

		bool? CheckContinueWithSendingMessage(IUserNotifications notifications)
		{
			var latestEvent = GetLatestInterestingLogEvent();
			switch (latestEvent?.SL_SE_NKEvent) 
			{
				case Events.MessageSentCode:
				case Events.InterchangeSentCode:
					notifications?.ShowMessage(
						Res.GetString("4C495411-1FEF-4565-9097-A16CD5D431F2", "This application cannot be sent for Certification as the application review is already in progress."),
						Res.GetString("7A539DD1-6B9A-403A-B5E5-764326CA305C", "Information"));
					return false;

				case Events.MessageAcceptedCode:
					return latestEvent.Parameters != null
						&& latestEvent.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, out var messageType)
						&& latestEvent.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, out var certificateOfOriginNumber)
						&& notifications != null
						&& notifications.ShowConfirmation(
							Res.GetString(
								"b89af2f1-fbd5-44df-9ede-5b5f24c3c8d9",
								"An {0} {1} has already been issued for this shipment. Submitting this new application will cancel and replace your previous application, invalidating the existing certificate.\r\n\r\nThis operation must only be done if the Cancel and Replace implications are understood.",
								messageType,
								certificateOfOriginNumber),
							Res.GetString("d2f14215-12b1-442a-9604-e9dee4918309", "Critical Warning"),
							Res.GetString("72309563-eb2a-43b9-ab4d-db76441a8831", "Please type the following to continue:"),
							Res.GetString("b9c256b1-acf0-49cc-b7b6-ce3c4a25a5a0", "I understand the impact"));

				default:
					return true;
			}
		}

		StmALog GetLatestInterestingLogEvent() => logParent
			.Logs
			.GetAllLogs()
			.OfType<StmALog>()
			.OrderByDescending(log => log.SL_PostedTimeUtc)
			.FirstOrDefault(log => eventsInterestedIn.Any(x => log.SL_SE_NKEvent.EqualsIgnoringCase(x))
				&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, out string messageType)
				&& !string.IsNullOrEmpty(messageType)
				&& messageType.Equals(shipmentDocumentName, StringComparison.OrdinalIgnoreCase));

		readonly IEnumerable<string> eventsInterestedIn = new[]
		{
			Events.MessageSentCode,
			Events.InterchangeSentCode,
			Events.MessageAcceptedCode,
			Events.MessageRejectedCode,
			Events.MessageWithdrawCancelAcceptedCode
		};
	}
}
