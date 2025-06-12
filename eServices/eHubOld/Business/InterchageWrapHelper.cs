using System;
using System.Xml;
using System.Xml.Linq;
using CargoWise.eServices.eHub.Common;

namespace CargoWise.eServices.eHub.Business
{
	public static class InterchageWrapHelper
	{
		public static XmlDocument WrapIntoInterchange(string document, string documentType)
		{
			return WrapIntoInterchange(document, documentType, string.Empty);
		}

		public static XmlDocument WrapIntoInterchange(string document, string documentType, string interchangeID)
		{
			var xInterchange = new XElement("{http://CargoWise.eServices.eHub.Schema}Interchange", new XAttribute(XNamespace.Xmlns + "ns0", "http://CargoWise.eServices.eHub.Schema"),
				new XElement("InterchangeHeader",
					new XElement("SenderID", string.Empty),
					new XElement("RecipientID", string.Empty),
					new XElement("InterchangeVersion", Constants.InterchangeVersion),
					new XElement("SenderApplicationVersion", Constants.DefaultApplicationVersion),
					new XElement("InterchangeID", string.IsNullOrEmpty(interchangeID) ? Guid.NewGuid().ToString() : interchangeID)),
				new XElement("Payload",
					new XElement("{http://CargoWise.eServices.eHub.Schema}Document", new XAttribute(XNamespace.Xmlns + "ns0", "http://CargoWise.eServices.eHub.Schema"),
						new XAttribute("DocumentType", documentType),
						new XElement("DocumentContent", document))));
			var result = new XmlDocument();
			result.LoadXml(xInterchange.ToString());

			return result;
		}

		public static XmlDocument RetrievePayloadToDocumentEnvelope(string interchange)
		{
			var xDocumentEnvelope = new XElement("{http://CargoWise.eServices.eHub.Schema}DocumentEnvelope", new XAttribute(XNamespace.Xmlns + "ns0", "http://CargoWise.eServices.eHub.Schema"),
				new XElement("Documents",
					XElement.Parse(interchange).Elements("Payload").Elements()));

			var result = new XmlDocument();
			result.LoadXml(xDocumentEnvelope.ToString());

			return result;
		}
	}
}
