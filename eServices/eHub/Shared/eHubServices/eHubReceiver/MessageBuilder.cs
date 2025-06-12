using System;
using System.Configuration;
using System.IO;
using System.Xml;

namespace CargoWise.eHub.Share.eHubServices.eHubReceiver
{
	public class MessageBuilder
	{
		string senderId;
		string recepientId;

		public MessageBuilder(Uri endpointUri)
		{
			var urlParts = endpointUri.AbsolutePath.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
			if (urlParts.Length > 0)
			{
				string serviceName = urlParts[0];
				string addresses = ConfigurationManager.AppSettings[serviceName];
				if (!string.IsNullOrWhiteSpace(addresses))
				{
					var addressParts = addresses.Split('|');
					if (addressParts.Length == 2)
					{
						recepientId = addressParts[0];
						senderId = addressParts[1];
					}
				}
			}

			if (string.IsNullOrWhiteSpace(senderId) || string.IsNullOrWhiteSpace(recepientId))
			{
				throw new ArgumentException("Can't find recipient or sender for the endpoint url");
			}
		}

		public Stream CreateMessage(Stream messageStream)
		{
			messageStream.Position = 0;
			Stream outputMessage = new MemoryStream();
			var writer = XmlTextWriter.Create(outputMessage, new XmlWriterSettings() { OmitXmlDeclaration = true });
			writer.WriteStartElement("ClientReceivedInformation", "http://cargowise.com/ehub/product/2013/04");
			writer.WriteElementString("SenderId", senderId);
			writer.WriteElementString("RecepientId", recepientId);
			writer.WriteStartElement("Content");
			writer.WriteRaw("<![CDATA[");
			messageStream.WriteToXmlWriter(writer);
			writer.WriteRaw("]]>");
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.Flush();
			outputMessage.Position = 0;
			return outputMessage;
		}
	}
}