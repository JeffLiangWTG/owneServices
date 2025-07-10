using System.Collections.Generic;
using System.Linq;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using TMiningConstants = Enterprise.Freight.Forwarding.Business.TMiningConstants;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	public static class SecureContainerReleaseContainerEventTestHelper
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
				case TMiningConstants.SecureContainerReleaseStatus.Assigned:
					eventToAdd = Events.Authorised;
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, TMiningConstants.ParameterMessageTypes.SecureContainerRelease);
					break;

				case TMiningConstants.SecureContainerReleaseStatus.Accepted:
					eventToAdd = Events.MessageAccepted;
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, TMiningConstants.ParameterMessageTypes.SecureContainerReleaseTransfer);
					break;

				case TMiningConstants.SecureContainerReleaseStatus.TransferRejected:
					eventToAdd = Events.MessageRejected;
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, TMiningConstants.ParameterMessageTypes.SecureContainerReleaseTransfer);
					break;

				case TMiningConstants.SecureContainerReleaseStatus.RevokeRejected:
					eventToAdd = Events.MessageRejected;
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, TMiningConstants.ParameterMessageTypes.SecureContainerReleaseRevoke);
					break;

				case TMiningConstants.SecureContainerReleaseStatus.TransferSent:
					eventToAdd = Events.MessagePendingProcessing;
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, TMiningConstants.ParameterMessageTypes.SecureContainerReleaseTransfer);
					break;

				case TMiningConstants.SecureContainerReleaseStatus.RevokeSent:
					eventToAdd = Events.MessageWithdrawCancelRequest;
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, TMiningConstants.ParameterMessageTypes.SecureContainerReleaseRevoke);
					break;

				case TMiningConstants.SecureContainerReleaseStatus.Revoked:
					eventToAdd = Events.MessageWithdrawCancelAccepted;
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, TMiningConstants.ParameterMessageTypes.SecureContainerReleaseRevoke);
					break;

				case TMiningConstants.SecureContainerReleaseStatus.TransferSentAwaitingResponse:
					eventToAdd = Events.MessageSent;
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, TMiningConstants.ParameterMessageTypes.SecureContainerReleaseTransfer);
					break;

				case TMiningConstants.SecureContainerReleaseStatus.RevokeSentAwaitingResponse:
					eventToAdd = Events.MessageSent;
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, TMiningConstants.ParameterMessageTypes.SecureContainerReleaseRevoke);
					break;
				default:
					eventToAdd = Events.MessageAccepted;
					break;
			}

			return container.Logs.CreateRecreateOrUpdateEventLog(eventToAdd, EstimateActual.Actual, ZDateTimeOffset.Now, "", parameters.ToArray());
		}
	}
}
