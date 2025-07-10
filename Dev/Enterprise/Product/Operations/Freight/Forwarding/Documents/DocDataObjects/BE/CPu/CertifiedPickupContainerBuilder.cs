using System;
using System.Linq;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE
{
	sealed class CertifiedPickupContainerBuilder
	{
		public CertifiedPickupContainer Build(CommonContainer containerBO, string formMode)
		{
			if (containerBO == null)
			{
				return null;
			}

			var container = new CertifiedPickupContainer(containerBO.PK, formMode);
			var eventHelper = new CertifiedPickupContainerEventHelper(containerBO);

			container.Number = containerBO.JC_ContainerNum;

			container.ReleaseIdentification = containerBO.JC_ContainerImportDORelease;
			container.CurrentStatus = eventHelper.GetCurrentStatusFromEvents().currentStatus;
			container.Action.IsRevokeMode = formMode == CertifiedPickup.FormModeRevoke;
			if (formMode == CertifiedPickup.FormModeAcceptDecline && container.CurrentStatus == CertifiedPickupConstants.Status.Assigned && container.Action.IsEmpty)
			{
				container.Action.IsAccept = true;
			}

			container.HumanReadableStatus = GetHumanReadableStatus(containerBO);
			container.ReleaseFromName = eventHelper.GetValueOfUSXMLEventContextCollectionFromATHEvents(BelgianPortsConstants.ContextCollectionTypes.ReleaseFromParty);
			container.ReleaseFromId = eventHelper.GetValueOfUSXMLEventContextCollectionFromATHEvents(BelgianPortsConstants.ContextCollectionTypes.ReleaseFromPartyId);
			container.ReleaseFromCode = eventHelper.GetValueOfUSXMLEventContextCollectionFromATHEvents(BelgianPortsConstants.ContextCollectionTypes.ReleaseFromPartyCode);

			container.IsNonOperativeReefer = containerBO.JC_IsNonOperativeReefer;

			return container;
		}

		static ZString GetHumanReadableStatus(CommonContainer containerBO)
		{
			var log = Business.CertifiedPickupContainerEventHelper.GetEventLogsInDescendingOrder(containerBO, false).FirstOrDefault();
			if (log != null && log.Parameters.TryGetValue(Constants.EventReferenceParameters.Codes.MessageType, out var messageType)
				&& (string.Compare(messageType, CertifiedPickupConstants.ParameterMessageTypes.AcceptDecline, StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(messageType, CertifiedPickupConstants.ParameterMessageTypes.Transfer, StringComparison.OrdinalIgnoreCase) == 0)
				&& (log.SL_SE_NKEvent == Events.InterchangeSentCode || log.SL_SE_NKEvent == Events.MessageRejectedCode || log.SL_SE_NKEvent == Events.MessagePendingProcessingCode))
			{
				return log.DisplayEventReference;
			}

			return ZString.Empty;
		}
	}
}
