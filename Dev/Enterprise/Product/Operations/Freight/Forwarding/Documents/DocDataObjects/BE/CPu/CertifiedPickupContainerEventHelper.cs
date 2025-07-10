using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EventReference;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE
{
	public class CertifiedPickupContainerEventHelper
	{
		public CertifiedPickupContainerEventHelper(CommonContainer containerBO)
		{
			Argument.NotNull(containerBO, nameof(containerBO));
			this.containerBO = containerBO;
		}

		readonly CommonContainer containerBO;

		public (string currentStatus, string eventTypeCode) GetCurrentStatusFromEvents()
			=> Business.CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(CertifiedPickupLogs);

		public string GetValueOfUSXMLEventContextCollectionFromATHEvents(string contextCollectionType)
			=> UEventATH?.ContextCollection.Where(c => c.Type == contextCollectionType).Select(c => c.Value).FirstOrDefault() ?? string.Empty;

		public static string GetMessageTypeByFormMode(string formMode)
		{
			switch (formMode)
			{
				case CertifiedPickup.FormModeAcceptDecline:
					return CertifiedPickupConstants.ParameterMessageTypes.AcceptDecline;
				case CertifiedPickup.FormModeTransfer:
					return CertifiedPickupConstants.ParameterMessageTypes.Transfer;
				case CertifiedPickup.FormModeRevoke:
					return CertifiedPickupConstants.ParameterMessageTypes.Revoke;
				default:
					return null;
			}
		}

		#region Implementation

		public IEnumerable<StmALog> CertifiedPickupLogs => certifiedPickupLogs ?? (certifiedPickupLogs = Business.CertifiedPickupContainerEventHelper.GetEventLogsInDescendingOrder(containerBO));
		IEnumerable<StmALog> certifiedPickupLogs;

		UniversalEvent UEventATH => uEventATH ?? (uEventATH = GetRelatedUniversalEvent(Events.AuthorisedCode));
		UniversalEvent uEventATH;

		UniversalEvent GetRelatedUniversalEvent(string eventTypeCode)
		{
			var relatedEDIMessage = GetLatestReleaseRightEvent(eventTypeCode)?.RelatedEDIMessage;
			if (relatedEDIMessage != null
				&& relatedEDIMessage.Message.EM_MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalEvent)
			{
				return relatedEDIMessage.Message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			}

			return null;
		}

		StmALog GetLatestReleaseRightEvent(string eventTypeCode)
		{
			return CertifiedPickupLogs
				.FirstOrDefault(log => log.SL_SE_NKEvent == eventTypeCode
				&& (log.Parameters.TryGetValue(Constants.EventReferenceParameters.Codes.MessageType, out var messageTypeValue) && (messageTypeValue == CertifiedPickupConstants.ParameterMessageTypes.AcceptDecline || messageTypeValue == CertifiedPickupConstants.ParameterMessageTypes.Transfer || messageTypeValue == CertifiedPickupConstants.ParameterMessageTypes.ReleaseRight)
				|| log.Parameters.TryGetValue(Constants.EventReferenceParameters.Codes.Type, out var typeValue) && typeValue == CertifiedPickupConstants.ParameterTypes.ContainerRelease));
		}

		#endregion
	}
}
