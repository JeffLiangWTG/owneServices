using System.IO;
using System.Xml;
using CargoWise.eHub.Share.eHubServices.eHubSender.Common.ReplyMessageBuilder;

namespace CargoWise.eHub.Share.eHubServices.eHubSender.ReplyMessageBuilder
{
	public class eLMSReplyMessageBuilder : CargoWise.eHub.Share.eHubServices.eHubSender.Common.ReplyMessageBuilder.ReplyMessageBuilder
	{
		ServiceReply reply;

		public eLMSReplyMessageBuilder(CargoWise.eHub.Share.eHubServices.eHubSender.Common.MessageContext.MessageContext context, ServiceReply reply)
			: base(context)
		{
			this.reply = reply;
		}

		public override string GetReply()
		{
			string replyText = "";

			using (var stream = new MemoryStream())
			{
				var writer = XmlTextWriter.Create(stream, new XmlWriterSettings() { OmitXmlDeclaration = true });
				writer.WriteStartElement("ClientDeliveryNotification", "http://cargowise.com/ehub/product/2013/04");
				writer.WriteElementString("SenderId", Context.eHubRecipientId);
				writer.WriteElementString("RecepientId", Context.eHubSenderId);
				writer.WriteElementString("TargetBOType", "ForwardingShipment");
				writer.WriteElementString("TargetBOKey", Context.GetValue("ShipmentNumber"));
				writer.WriteElementString("EventTypeCode", "MSC");
				writer.WriteElementString("Reference", string.Format("Message processing reference - {0}", reply.ReplyDescription));
				writer.WriteStartElement("ContextCollection");
				writer.WriteEndElement();
				writer.WriteEndElement();
				writer.Flush();
				stream.Position = 0;
				replyText = new StreamReader(stream).ReadToEnd();
			}

			return replyText; ;
		}
	}
}