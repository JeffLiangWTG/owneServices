using System.Linq;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	public static class CINExportNotificationMessageValidator
	{
		public static string GetValidationMessage(ForwardingShipment shipment)
		{
			var message = string.Empty;

			if (!PortMessagingRegistry.Instance.AllowToSendExportNotification.Value)
			{
				message = (NoResString)"The registry item 'Freight > Port Messaging > France > Allow to send Export Notification (755) to Cargo Information Network' is disabled, so could not send Export Notification (755)."; // This is for logging only.
			}
			else if (shipment.JS_TransportMode != TransportModes.Air)
			{
				message = $"Shipment {shipment.JS_UniqueConsignRef} transport mode {shipment.JS_TransportMode} is not Air, so could not send Export Notification (755)."; // This is for logging only.
			}
			else if (!shipment.JS_RL_NKOrigin.StartsWith(CountryCodes.France))
			{
				message = $"Shipment {shipment.JS_UniqueConsignRef} origin {shipment.JS_RL_NKOrigin} is not in France, so could not send Export Notification (755)."; // This is for logging only.
			}
			else if (!shipment.CusEntryNumbers.Cast<CusEntryNumber>().Any(n => n.CE_RN_NKCountryCode == CountryCodes.France && n.CE_EntryType == "MRN") && !shipment.Numbers.Cast<CusEntryNumber>().Any(n => n.CE_RN_NKCountryCode == CountryCodes.France && n.CE_EntryType == "MRN"))
			{
				message = $"Shipment {shipment.JS_UniqueConsignRef} has no registered MRN reference number, so could not send Export Notification (755)."; // This is for logging only.
			}
			else if (!shipment.HasCOCAndMatchConsolsCFSAddressCTRNumber)
			{
				message = $"The FR – COC Reference Number registered at the Shipment {shipment.JS_UniqueConsignRef} is different with the value of the FR – CTR Registration number of the departure CFS defined at the consol, so could not send Export Notification (755)."; // This is for logging only.
			}
			else if (!shipment.Consols.Cast<ForwardingConsol>().Any(c => c.JK_TransportMode == TransportModes.Air && !c.JK_MasterBillNum.IsEmpty && c.JK_RL_NKLoadPort.StartsWith(Core.Constants.CountryCodes.France)))
			{
				message = $"No consol with Air transport mode and master bill and 1st Load in France was attached in Shipment {shipment.JS_UniqueConsignRef}, so could not send Export Notification (755)."; // This is for logging only.
			}

			return message;
		}
	}
}
