using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.NL
{
	public static class CGNExportNotificationMessageValidator
	{
		public static string GetValidationMessage(ForwardingConsol consol)
		{
			var message = string.Empty;

			if (!PortMessagingRegistry.Instance.AllowToSendExportNotificationToCargonaut.Value)
			{
				message = (NoResString)"The registry item 'Freight > Port Messaging > Netherlands > Allow to send Export Notification (755) to Cargonaut' is disabled, so could not send Export Notification (755)."; // This is for logging only.
			}
			else if (consol.JK_TransportMode != TransportModes.Air)
			{
				message = $"Consol {consol.JK_UniqueConsignRef} transport mode {consol.JK_TransportMode} is not Air, so could not send Export Notification (755)."; // This is for logging only.
			}
			else if (!consol.JK_RL_NKLoadPort.StartsWith("NL"))
			{
				message = $"Consol {consol.JK_UniqueConsignRef} first load {consol.JK_RL_NKLoadPort} is not in Netherlands, so could not send Export Notification (755)."; // This is for logging only.
			}

			return message;
		}
	}
}
