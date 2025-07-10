using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.ILReferenceData.Business
{
	public static class Helpers
	{
		public static string Serialize<T>(T obj)
		{
			Argument.NotNull(obj, nameof(obj));

			XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
			using (StringWriter textWriter = new StringWriter())
			{
				xmlSerializer.Serialize(textWriter, obj);
				return textWriter.ToString();
			}
		}

		public static string CreateSoapEnvelopeWithRawBody(string rawBodyXml, string serviceName, string consumerId, string externalId)
		{
			XNamespace soapenv = "http://www.w3.org/2003/05/soap-envelope";
			XNamespace requestHeader = "http://MalamTeam.Inf.ESB.Schemas.RequestHeader";

			// Parse the raw XML string into an XElement
			XElement bodyContent = string.IsNullOrEmpty(rawBodyXml) ? null : XElement.Parse(rawBodyXml);

			// Extract the namespace URI from the root element of the raw body XML
			XNamespace myns = bodyContent?.Name.NamespaceName;

			// Create the envelope element
			var envelope = myns != null
				? new XElement(soapenv + "Envelope",
					new XAttribute(XNamespace.Xmlns + "soapenv", soapenv),
					new XAttribute(XNamespace.Xmlns + "myns", myns),
					new XAttribute(XNamespace.Xmlns + "mal", requestHeader))
				: new XElement(soapenv + "Envelope",
					new XAttribute(XNamespace.Xmlns + "soapenv", soapenv),
					new XAttribute(XNamespace.Xmlns + "mal", requestHeader));

			// Create and add the header element
			var header = new XElement(soapenv + "Header",
				new XElement(requestHeader + "RequestHeader",
					new XElement(requestHeader + "ServiceName", serviceName),
					new XElement(requestHeader + "SoftwareProvider", Constants.CustomsRequestHeader.SoftwareProvider),
					new XElement(requestHeader + "SoftwareVersion", Constants.CustomsRequestHeader.SoftwareVersion),
					new XElement(requestHeader + "WSDLVersion", Constants.CustomsRequestHeader.WSDLVersion),
					new XElement(requestHeader + "ExternalId", externalId),
					new XElement(requestHeader + "ConsumerId", consumerId)
				)
			);
			envelope.Add(header);

			// Create and add the body element, incorporating the raw XML string
			var body = new XElement(soapenv + "Body", bodyContent);
			envelope.Add(body);

			// Create the XDocument and return the string
			var soapDocument = new XDocument(new XDeclaration("1.0", "utf-8", null), envelope);
			return soapDocument.ToString();
		}

		public static XElement GetRawBodyFromSoapEnvelop(Stream stream)
		{
			XDocument xDocument = XDocument.Load(stream);
			XNamespace s = "http://www.w3.org/2003/05/soap-envelope";
			XElement body = xDocument.Descendants(s + "Body").FirstOrDefault();

			return (XElement)body?.FirstNode;
		}
	}
}
