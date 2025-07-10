using System.Collections.Generic;
using System.Linq;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.Testing.BE
{
	public static class CertifiedPickupContainerEventTestHelper
	{
		public static StmALog AddLogForContainer(ForwardingContainer container, ZString containerStatus)
		{
			var parameters = new Dictionary<string, string>
			{
				[Constants.EventReferenceParameters.Codes.EquipmentReferenceNumber] = container.JC_ContainerNum,
			};

			Event eventToAdd;
			switch (containerStatus)
			{
				case CertifiedPickupConstants.Status.Accepted:
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, CertifiedPickupConstants.ParameterMessageTypes.AcceptDecline);
					eventToAdd = Events.MessageAccepted;
					break;
				case CertifiedPickupConstants.Status.Assigned:
					eventToAdd = Events.Authorised;
					parameters.Add(Constants.EventReferenceParameters.Codes.Type, CertifiedPickupConstants.ParameterTypes.ContainerRelease);
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, CertifiedPickupConstants.ParameterMessageTypes.ReleaseRight);
					break;
				case CertifiedPickupConstants.Status.TransferSentAwaitingResponse:
					eventToAdd = Events.MessageSent;
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, CertifiedPickupConstants.ParameterMessageTypes.Transfer);
					parameters.Add(CertifiedPickupConstants.ContainerEventParameter.EventCode, CertifiedPickupConstants.ContainerEventParameter.Values.TransferSentAwaitingResponse);
					break;
				case CertifiedPickupConstants.Status.TransferSent:
					eventToAdd = Events.MessagePendingProcessing;
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, CertifiedPickupConstants.ParameterMessageTypes.Transfer);
					parameters.Add(Constants.EventReferenceParameters.Codes.Status, CertifiedPickupConstants.EventStatus.Transferred);
					break;
				case CertifiedPickupConstants.Status.Revoked:
					eventToAdd = Events.MessageWithdrawCancelAccepted;
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, CertifiedPickupConstants.ParameterMessageTypes.Revoke);
					parameters.Add(Constants.EventReferenceParameters.Codes.Status, CertifiedPickupConstants.EventStatus.RevokedByPreviousParty);
					break;
				case CertifiedPickupConstants.Status.DeclinedByNextPartyForAcceptDecline:
					eventToAdd = Events.MessageRejected;
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, CertifiedPickupConstants.ParameterMessageTypes.AcceptDecline);
					parameters.Add(Constants.EventReferenceParameters.Codes.Status, CertifiedPickupConstants.EventStatus.DeclinedByNextParty);
					break;
				case CertifiedPickupConstants.Status.DeclinedByNextPartyForTransferRevoke:
					eventToAdd = Events.MessageRejected;
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, CertifiedPickupConstants.ParameterMessageTypes.Transfer);
					parameters.Add(Constants.EventReferenceParameters.Codes.Status, CertifiedPickupConstants.EventStatus.DeclinedByNextParty);
					break;
				case CertifiedPickupConstants.Status.DeclinedByOtherReason:
					eventToAdd = Events.MessageRejected;
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, CertifiedPickupConstants.ParameterMessageTypes.Transfer);
					parameters.Add(Constants.EventReferenceParameters.Codes.Status, "ORG");
					break;
				default:
					eventToAdd = Events.MessageAccepted;
					break;
			}

			return container.Logs.CreateRecreateOrUpdateEventLog(eventToAdd, EstimateActual.Actual, ZDateTimeOffset.Now, "", parameters.ToArray());
		}
	}
}
