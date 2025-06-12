using System.IO;
using System.Xml;
using CargoWise.eHub.Share.eHubServices.eHubSender.Common.ReplyMessageBuilder;
using Newtonsoft.Json;

namespace CargoWise.eHub.Share.eHubServices.eHubSender.ReplyMessageBuilder
{
	public class OceanInsightsReplyMessageBuilder : CargoWise.eHub.Share.eHubServices.eHubSender.Common.ReplyMessageBuilder.ReplyMessageBuilder
	{
		readonly ServiceReply reply;

		public OceanInsightsReplyMessageBuilder(CargoWise.eHub.Share.eHubServices.eHubSender.Common.MessageContext.MessageContext context, ServiceReply reply)
			: base(context)
		{
			this.reply = reply;
		}

		public override string GetReply()
		{
			return reply.ReplyAction == ServiceReply.Action.Error ? BuildErrorReply() : BuildSuccessReply();
		}

		string BuildSuccessReply()
		{
			string subscriptionId = GetSubscriptionId(reply.ReplyDescription);


			string replyText = "";
			using (var stream = new MemoryStream())
			{
				var writer = XmlWriter.Create(stream, new XmlWriterSettings() { OmitXmlDeclaration = true });
				writer.WriteStartElement("ClientDeliveryNotification", "http://cargowise.com/ehub/product/2013/04");
				writer.WriteElementString("SenderId", Context.eHubRecipientId);
				writer.WriteElementString("RecepientId", Context.eHubSenderId);
				writer.WriteElementString("TargetBOType", "Container Event Subscription");
				writer.WriteElementString("TargetBOKey", Context.GetValue("Reference"));
				writer.WriteElementString("EventTypeCode", "CRT"); ;
				writer.WriteElementString("Reference", "Subscription created");

				writer.WriteStartElement("ContextCollection");
				writer.WriteStartElement("Context");
				writer.WriteElementString("Type", "SubscriptionId");
				writer.WriteElementString("Value", subscriptionId);
				writer.WriteEndElement();
				writer.WriteEndElement();
				writer.WriteEndElement();
				writer.Flush();
				stream.Position = 0;
				replyText = new StreamReader(stream).ReadToEnd();
			}

			return replyText;

		}

		string GetSubscriptionId(string jsonText)
		{
			var doc = JsonConvert.DeserializeXmlNode("{\"root\":" + jsonText + "}", "root");
			if (doc == null) throw new InvalidDataException(string.Format("Reply on Subscription from OceanInsight is not Json. Contact OceanInsight to fix the problem. {0}", reply.ReplyDescription));

			var idNode = doc.SelectSingleNode("//id");
			if (idNode == null || string.IsNullOrWhiteSpace(idNode.InnerText)) throw new InvalidDataException(string.Format("Reply on Subscription from OceanInsight subscriptionId is empty. Contact OceanInsight to fix the problem. {0}", reply.ReplyDescription));
			return idNode.InnerText;
		}

		string BuildErrorReply()
		{
			string replyText = "";
			using (var stream = new MemoryStream())
			{
				var writer = XmlWriter.Create(stream, new XmlWriterSettings() { OmitXmlDeclaration = true });
				writer.WriteStartElement("ClientDeliveryNotification", "http://cargowise.com/ehub/product/2013/04");
				writer.WriteElementString("SenderId", Context.eHubRecipientId);
				writer.WriteElementString("RecepientId", Context.eHubSenderId);
				writer.WriteElementString("TargetBOType", "Container Event Subscription");
				writer.WriteElementString("TargetBOKey", Context.GetValue("Reference"));
				writer.WriteElementString("EventTypeCode", "REJ");
				writer.WriteElementString("Reference", string.Format("Message Processing Error - {0}", reply.ReplyDescription));
				writer.WriteEndElement();
				writer.Flush();
				stream.Position = 0;
				replyText = new StreamReader(stream).ReadToEnd();
			}

			return replyText;
		}
	}
}