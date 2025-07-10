using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EventReference;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class CertifiedPickupContainerEventHelper
	{
		public static (string currentStatus, string eventTypeCode) GetCurrentStatusFromEvents(IEnumerable<StmALog> logs)
		{
			foreach (var log in logs)
			{
				var statusValue = log.Parameters.GetValueSafe(Constants.EventReferenceParameters.Codes.Status);
				var messageType = log.Parameters.GetValueSafe(Constants.EventReferenceParameters.Codes.MessageType);

				switch (log.SL_SE_NKEvent)
				{
					case Events.MessageAcceptedCode:
						return (CertifiedPickupConstants.Status.Accepted, log.SL_SE_NKEvent);
					case Events.AuthorisedCode:
						return (CertifiedPickupConstants.Status.Assigned, log.SL_SE_NKEvent);
					case Events.MessageSentCode:
						if (string.Compare(messageType, CertifiedPickupConstants.ParameterMessageTypes.Transfer, StringComparison.OrdinalIgnoreCase) == 0
							&& string.Compare(log.Parameters.GetValueSafe(CertifiedPickupConstants.ContainerEventParameter.EventCode), CertifiedPickupConstants.ContainerEventParameter.Values.TransferSentAwaitingResponse, StringComparison.OrdinalIgnoreCase) == 0)
						{
							return (CertifiedPickupConstants.Status.TransferSentAwaitingResponse, log.SL_SE_NKEvent);
						}

						break;
					case Events.MessagePendingProcessingCode:
						if (string.Compare(statusValue, CertifiedPickupConstants.EventStatus.Transferred, StringComparison.OrdinalIgnoreCase) == 0)
						{
							return (CertifiedPickupConstants.Status.TransferSent, log.SL_SE_NKEvent);
						}

						break;
					case Events.MessageWithdrawCancelAcceptedCode:
						if (string.Compare(messageType, CertifiedPickupConstants.ParameterMessageTypes.Revoke, StringComparison.OrdinalIgnoreCase) == 0
							&& string.Compare(statusValue, CertifiedPickupConstants.EventStatus.RevokedByPreviousParty, StringComparison.OrdinalIgnoreCase) == 0)
						{
							return (CertifiedPickupConstants.Status.Revoked, log.SL_SE_NKEvent);
						}

						break;
					case Events.MessageRejectedCode:
						if (string.Compare(messageType, CertifiedPickupConstants.ParameterMessageTypes.AcceptDecline, StringComparison.OrdinalIgnoreCase) == 0)
						{
							if (string.Compare(statusValue, CertifiedPickupConstants.EventStatus.DeclinedByNextParty, StringComparison.OrdinalIgnoreCase) == 0)
							{
								return (CertifiedPickupConstants.Status.DeclinedByNextPartyForAcceptDecline, log.SL_SE_NKEvent);
							}
						}
						else if (string.Compare(messageType, CertifiedPickupConstants.ParameterMessageTypes.Transfer, StringComparison.OrdinalIgnoreCase) == 0)
						{
							if (string.Compare(statusValue, CertifiedPickupConstants.EventStatus.DeclinedByNextParty, StringComparison.OrdinalIgnoreCase) == 0)
							{
								return (CertifiedPickupConstants.Status.DeclinedByNextPartyForTransferRevoke, log.SL_SE_NKEvent);
							}
							else
							{
								return (CertifiedPickupConstants.Status.DeclinedByOtherReason, log.SL_SE_NKEvent);
							}
						}

						break;
					default:
						return (CertifiedPickupConstants.Status.NotApplicable, string.Empty);
				}
			}

			return (CertifiedPickupConstants.Status.NotApplicable, string.Empty);
		}

		public static string GetCurrentStatusFromEvents(CommonContainer containerBO)
			=> GetCurrentStatusFromEvents(GetEventLogsInDescendingOrder(containerBO)).currentStatus;

		public static bool CheckStatusIsReadyToShowInAcceptDeclineMode(CommonContainer containerBO)
			=> CheckStatusIsReadyToShowInAcceptDeclineMode(GetCurrentStatusFromEvents(containerBO));

		public static bool CheckStatusIsReadyToShowInAcceptDeclineMode(string containerCpuStatus)
			=> containerCpuStatus == CertifiedPickupConstants.Status.Assigned;

		public static bool CheckStatusIsReadyToShowInTransferMode(CommonContainer containerBO)
			=> CheckStatusIsReadyToShowInTransferMode(GetCurrentStatusFromEvents(containerBO));

		public static bool CheckStatusIsReadyToShowInTransferMode(string containerCpuStatus)
			=> containerCpuStatus == CertifiedPickupConstants.Status.TransferSentAwaitingResponse
			|| containerCpuStatus == CertifiedPickupConstants.Status.Accepted
			|| containerCpuStatus == CertifiedPickupConstants.Status.Revoked
			|| containerCpuStatus == CertifiedPickupConstants.Status.DeclinedByNextPartyForTransferRevoke
			|| containerCpuStatus == CertifiedPickupConstants.Status.DeclinedByOtherReason;

		public static bool CheckStatusIsReadyToShowInRevokeMode(CommonContainer containerBO)
			=> CheckStatusIsReadyToShowInRevokeMode(GetCurrentStatusFromEvents(containerBO));

		public static bool CheckStatusIsReadyToShowInRevokeMode(string containerCpuStatus)
			=> containerCpuStatus == CertifiedPickupConstants.Status.TransferSent;

		public static IEnumerable<StmALog> GetEventLogsInDescendingOrder(CommonContainer containerBO, bool isCertifiedPickupLogOnly = true)
		{
			return containerBO
				.Logs?
				.GetAllLogs()
				.OfType<StmALog>()
				.OrderByDescending(log => log.SL_PostedTimeUtc)
				.Where(log => !log.SL_IsCancelled && (!isCertifiedPickupLogOnly || IsContainerCertifiedPickupLog(containerBO.JC_ContainerNum, log)));
		}

		static bool IsContainerCertifiedPickupLog(string containerNumber, StmALog log)
		{
			if (log.Parameters.TryGetValue(Constants.EventReferenceParameters.Codes.EquipmentReferenceNumber, out var equipmentReferenceNumber)
				&& equipmentReferenceNumber == containerNumber)
			{
				switch (log.SL_SE_NKEvent)
				{
					case Events.AuthorisedCode:
					case Events.AuthorisationWithdrawnCode:
					case Events.MessageSentCode:
					case Events.MessageAcceptedCode:
					case Events.MessagePendingProcessingCode:
					case Events.MessageWithdrawCancelAcceptedCode:
					case Events.MessageRejectedCode:
						var messageType = log.Parameters.GetValueSafe(Constants.EventReferenceParameters.Codes.MessageType);

						return string.Compare(messageType, CertifiedPickupConstants.ParameterMessageTypes.Transfer, StringComparison.OrdinalIgnoreCase) == 0
							|| string.Compare(messageType, CertifiedPickupConstants.ParameterMessageTypes.AcceptDecline, StringComparison.OrdinalIgnoreCase) == 0
							|| string.Compare(messageType, CertifiedPickupConstants.ParameterMessageTypes.Revoke, StringComparison.OrdinalIgnoreCase) == 0
							|| string.Compare(messageType, CertifiedPickupConstants.ParameterMessageTypes.ReleaseRight, StringComparison.OrdinalIgnoreCase) == 0;

					default:
						return false;
				}
			}

			return false;
		}
	}
}
