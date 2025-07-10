using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class SecureContainerReleaseContainerEventHelper
	{
		public static string GetCurrentStatusFromEvents(CommonContainer containerBO)
			=> GetCurrentStatusFromEvents(GetEventLogsInDescendingOrder(containerBO)).currentStatus;

		public static (string currentStatus, string eventTypeCode) GetCurrentStatusFromEvents(IEnumerable<StmALog> logs)
		{
			foreach (var log in logs)
			{
				var messageType = log.Parameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType);

				switch (log.SL_SE_NKEvent)
				{
					case Events.AuthorisedCode:
						return (TMiningConstants.SecureContainerReleaseStatus.Assigned, log.SL_SE_NKEvent);

					case Events.MessageAcceptedCode:
						return (TMiningConstants.SecureContainerReleaseStatus.Accepted, log.SL_SE_NKEvent);

					case Events.MessageRejectedCode:
						if (string.Compare(messageType, TMiningConstants.ParameterMessageTypes.SecureContainerReleaseTransfer, StringComparison.OrdinalIgnoreCase) == 0)
						{
							return (TMiningConstants.SecureContainerReleaseStatus.TransferRejected, log.SL_SE_NKEvent);
						}
						else if (string.Compare(messageType, TMiningConstants.ParameterMessageTypes.SecureContainerReleaseRevoke, StringComparison.OrdinalIgnoreCase) == 0)
						{
							return (TMiningConstants.SecureContainerReleaseStatus.RevokeRejected, log.SL_SE_NKEvent);
						}
						break;

					case Events.MessagePendingProcessingCode:
						if (string.Compare(messageType, TMiningConstants.ParameterMessageTypes.SecureContainerReleaseTransfer, StringComparison.OrdinalIgnoreCase) == 0)
						{
							return (TMiningConstants.SecureContainerReleaseStatus.TransferSent, log.SL_SE_NKEvent);
						}
						break;

					case Events.MessageWithdrawCancelRequestCode:
						if (string.Compare(messageType, TMiningConstants.ParameterMessageTypes.SecureContainerReleaseRevoke, StringComparison.OrdinalIgnoreCase) == 0)
						{
							return (TMiningConstants.SecureContainerReleaseStatus.RevokeSent, log.SL_SE_NKEvent);
						}
						break;

					case Events.MessageWithdrawCancelAcceptedCode:
						if (string.Compare(messageType, TMiningConstants.ParameterMessageTypes.SecureContainerReleaseRevoke, StringComparison.OrdinalIgnoreCase) == 0)
						{
							return (TMiningConstants.SecureContainerReleaseStatus.Revoked, log.SL_SE_NKEvent);
						}
						break;

					case Events.MessageSentCode:
					case Events.InterchangeSentCode:
						if (string.Compare(messageType, TMiningConstants.ParameterMessageTypes.SecureContainerReleaseTransfer, StringComparison.OrdinalIgnoreCase) == 0)
						{
							return (TMiningConstants.SecureContainerReleaseStatus.TransferSentAwaitingResponse, log.SL_SE_NKEvent);
						}
						else if (string.Compare(messageType, TMiningConstants.ParameterMessageTypes.SecureContainerReleaseRevoke, StringComparison.OrdinalIgnoreCase) == 0)
						{
							return (TMiningConstants.SecureContainerReleaseStatus.RevokeSentAwaitingResponse, log.SL_SE_NKEvent);
						}
						break;

					default:
						return (TMiningConstants.SecureContainerReleaseStatus.NotApplicable, string.Empty);
				}
			}

			return (TMiningConstants.SecureContainerReleaseStatus.NotApplicable, string.Empty);
		}

		public static IEnumerable<StmALog> GetEventLogsInDescendingOrder(CommonContainer containerBO, bool isSecureContainerReleaseLogOnly = true)
		{
			return containerBO
				.Logs?
				.GetAllLogs()
				.OfType<StmALog>()
				.OrderByDescending(log => log.SL_PostedTimeUtc)
				.Where(log => !log.SL_IsCancelled && (!isSecureContainerReleaseLogOnly || IsSecureContainerReleaseLog(containerBO.JC_ContainerNum, log)));
		}

		static bool IsSecureContainerReleaseLog(string containerNumber, StmALog log)
		{
			if (log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.EquipmentReferenceNumber, out var equipmentReferenceNumber)
				&& equipmentReferenceNumber == containerNumber)
			{
				var messageType = log.Parameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType);

				switch (log.SL_SE_NKEvent)
				{
					case Events.AuthorisedCode:
					case Events.AuthorisationWithdrawnCode:
						return string.Compare(messageType, TMiningConstants.ParameterMessageTypes.SecureContainerRelease, StringComparison.OrdinalIgnoreCase) == 0;
					case Events.MessageSentCode:
					case Events.InterchangeSentCode:
					case Events.MessageAcceptedCode:
					case Events.MessagePendingProcessingCode:
					case Events.MessageWithdrawCancelRequestCode:
					case Events.MessageWithdrawCancelAcceptedCode:
					case Events.MessageRejectedCode:
						return string.Compare(messageType, TMiningConstants.ParameterMessageTypes.SecureContainerReleaseTransfer, StringComparison.OrdinalIgnoreCase) == 0
							|| string.Compare(messageType, TMiningConstants.ParameterMessageTypes.SecureContainerReleaseRevoke, StringComparison.OrdinalIgnoreCase) == 0;

					default:
						return false;
				}
			}

			return false;
		}

		public static bool CheckStatusIsReadyToShowInTransferMode(string containerStatus)
				=> containerStatus == TMiningConstants.SecureContainerReleaseStatus.Assigned
				|| containerStatus == TMiningConstants.SecureContainerReleaseStatus.TransferRejected
				|| containerStatus == TMiningConstants.SecureContainerReleaseStatus.TransferSent
				|| containerStatus == TMiningConstants.SecureContainerReleaseStatus.Revoked
				|| containerStatus == TMiningConstants.SecureContainerReleaseStatus.TransferSentAwaitingResponse;

		public static bool CheckStatusIsReadyToShowInRevokeMode(string containerStatus)
			=> containerStatus == TMiningConstants.SecureContainerReleaseStatus.Accepted
			|| containerStatus == TMiningConstants.SecureContainerReleaseStatus.RevokeRejected
			|| containerStatus == TMiningConstants.SecureContainerReleaseStatus.RevokeSent
			|| containerStatus == TMiningConstants.SecureContainerReleaseStatus.RevokeSentAwaitingResponse;
	}
}
