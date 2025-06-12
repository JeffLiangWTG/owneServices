using System;
using System.IO;
using System.Reflection;
using System.Xml;
using CargoWise.eServices.USCustoms.Integration;
using ServiceBroker.Common;

namespace CargoWise.eServices.USCustoms.OutboundProcessingService
{
	public class USCustomsOutboundMessage : USCustomsMessage
	{
		public USCustomsOutboundMessage(Stream messageData) : base(messageData) { }

		protected override void PopulateDataFromStream(Stream messageData)
		{
			var reader = XmlReader.Create(messageData);
			reader.MoveToContent();

			TrackingId = reader.GetAttribute(Constants.MessageAttributes.MessageTrackingId);
			ClientId = reader.GetAttribute(Constants.MessageAttributes.ClientId);
			IsProduction = Convert.ToBoolean(reader.GetAttribute(Constants.MessageAttributes.IsProduction));
			ApplicationCode = reader.GetAttribute(Constants.MessageAttributes.ApplicationCode);
			MessageType = reader.GetAttribute(Constants.MessageAttributes.MessageType);

			switch (ApplicationCode)
			{
				case CargoWise.eHub.Integration.ApplicationCode.AMA:
					messageData.SeekBegin();
					var readerAMA = new XmlTextReader(messageData);
					readerAMA.MoveToContent();
					readerAMA.Read();
					MessageStream = new MemoryStream(MQMessageSender.OutgoingEncoding.GetBytes(readerAMA.Value));
					break;
				default:
					reader.Read();
					MessageStream = new MemoryStream();
					reader.WriteToStream(MessageStream);
					MessageStream.SeekBegin();
					break;
			}
		}
	}
}
