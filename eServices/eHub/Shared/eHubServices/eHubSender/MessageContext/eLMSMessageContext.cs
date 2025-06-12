using System;
using System.Xml.Linq;
using System.Xml.XPath;

namespace CargoWise.eHub.Share.eHubServices.eHubSender.MessageContext
{
	public class eLMSMessageContext : CargoWise.eHub.Share.eHubServices.eHubSender.Common.MessageContext.MessageContext
	{
		public eLMSMessageContext(string message, string eHubSenderId, string eHubRecipientId)
			: base(message, eHubSenderId, eHubRecipientId)
		{
		}

		public override void Load()
		{
            var xmlElement = XElement.Parse(Message);
			var shipmentNumberElement = xmlElement.XPathSelectElement("//*[local-name()='JobDetails']/*[local-name()='InvoiceReference']");
			if (shipmentNumberElement == null) throw new Exception("Invalid Xml. Can't find Shipment Number.");
			AddContext("ShipmentNumber", shipmentNumberElement.Value);
		}
	}
}