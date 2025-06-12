using System;
using System.Xml.Linq;
using System.Xml.XPath;

namespace CargoWise.eHub.Share.eHubServices.eHubSender.MessageContext
{
	public class USDISMessageContext : CargoWise.eHub.Share.eHubServices.eHubSender.Common.MessageContext.MessageContext
	{
		public USDISMessageContext(string message, string eHubSenderId, string eHubRecipientId)
			: base(message, eHubSenderId, eHubRecipientId)
		{
		}

		public override void Load()
		{
			var xmlElement = XElement.Parse(Message);
			var messageType = xmlElement.XPathSelectElement("//*[local-name()='MessageHeader']/*[local-name()='MessageType']");
			if (messageType == null) throw new Exception("Invalid Xml. Can't find MessageType in MessageHeader.");
			AddContext("MessageType", messageType.Value);
		}
	}
}