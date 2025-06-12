using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Dispatcher;
using System.ServiceModel;
using System.Xml;
using System.ServiceModel.Channels;

namespace CargoWise.eHub.Products.AirMessaging.EndpointBehavior.Descartes
{
	class DescartesMessageInspector : IDispatchMessageInspector
	{
		#region IDispatchMessageInspector Members

		public object AfterReceiveRequest(ref System.ServiceModel.Channels.Message request, IClientChannel channel, InstanceContext instanceContext)
		{
			var headerFromindex = request.Headers.FindHeader("From", "http://schemas.xmlsoap.org/ws/2004/03/addressing");
			var headerToindex = request.Headers.FindHeader("To", "http://schemas.xmlsoap.org/ws/2004/03/addressing");
			var senderPIMA = GetPIMAFromHeaderValue(ReadHeaderValue(headerFromindex));
			var recipientPIMA = GetPIMAFromHeaderValue(ReadHeaderValue(headerToindex));

			string PropertiesToPromoteKey = "http://schemas.microsoft.com/BizTalk/2006/01/Adapters/WCF-properties/Promote";

			XmlQualifiedName PropName1 = new XmlQualifiedName(@"SenderPIMA", @"CargoWise.eHub.Products.AirMessaging.Schemas.AirMessagingPropertySchema");
			XmlQualifiedName PropName2 = new XmlQualifiedName(@"RecipientPIMA", @"CargoWise.eHub.Products.AirMessaging.Schemas.AirMessagingPropertySchema");

			List<KeyValuePair<XmlQualifiedName, object>> promoteProps = new List<KeyValuePair<XmlQualifiedName, object>>();
			promoteProps.Add(new KeyValuePair<XmlQualifiedName, object>(PropName1, senderPIMA));
			promoteProps.Add(new KeyValuePair<XmlQualifiedName, object>(PropName2, recipientPIMA));
			request.Properties[PropertiesToPromoteKey] = promoteProps;

			return null;
		}

		private object GetPIMAFromHeaderValue(string input)
		{
			var list = input.Split(':');
			return list.Length > 0 ? list.Last() : string.Empty;
		}

		private static string ReadHeaderValue(int headerFromindex)
		{
			var result = string.Empty;
			XmlDictionaryReader reader = OperationContext.Current.IncomingMessageHeaders.GetReaderAtHeader(headerFromindex);
			XmlDocument d = new XmlDocument();
			d.LoadXml(reader.ReadOuterXml());
			result = d.DocumentElement.InnerText;
			return result;
		}

		public void BeforeSendReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
		{
		}

		#endregion
	}
}
