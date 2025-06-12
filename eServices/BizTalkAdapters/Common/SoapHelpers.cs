using Microsoft.BizTalk.Streaming;
using Microsoft.BizTalk.XPath;
using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;


namespace CargoWise.eHub.BizTalkAdapters.Common
{
	public class SoapHelpers
	{
		// Currently using soap 1.1, could be extended easily to support 1.2 by passing in the desired soap version and specifying a different namespace
		static string soap11Namespace = "http://schemas.xmlsoap.org/soap/envelope/"; 

		public static void WrapBodyInEnvelope(Stream bodyStream, Stream envelopedStream)
		{
			using (var reader = XmlReader.Create(bodyStream))
			using (var writer = XmlWriter.Create(envelopedStream))
			{
				writer.WriteStartElement("s", "Envelope", soap11Namespace);
				writer.WriteStartElement("Body", soap11Namespace);

				reader.MoveToContent();
				writer.WriteNode(reader, true);

				writer.WriteEndElement();
				writer.WriteEndElement();
			}
		}

		public static XElement GetHeaderFromEnvelope(Stream envelopedStream)
		{
			XElement headerElement = new XElement("Headers");

			using (var reader = XmlReader.Create(envelopedStream))
			{
				var xPathCollection = new XPathCollection();
				xPathCollection.Add(string.Format("/*[local-name()='Envelope' and namespace-uri()='{0}']/*[local-name()='Header' and namespace-uri()='{0}']/*", soap11Namespace));
				XPathReader xpathReader = new XPathReader(reader, xPathCollection);
				while (xpathReader.ReadUntilMatch())
				{
					headerElement.Add(XElement.Load(xpathReader.ReadSubtree()));
				}
			}

			envelopedStream.Position = 0;

			return headerElement;
		}

		public static VirtualStream UnwrapBodyFromEnvelope(Stream envelopedStream, string bodyPath)
		{
			var bodyStream = new VirtualStream();

			using (var reader = XmlReader.Create(envelopedStream))
			using (var writer = XmlWriter.Create(bodyStream))
			{
				var xPathCollection = new XPathCollection();
				xPathCollection.Add(string.Format("/*[local-name()='Envelope' and namespace-uri()='{0}']/*[local-name()='Body' and namespace-uri()='{0}']{1}", soap11Namespace, bodyPath));
				XPathReader xpathReader = new XPathReader(reader, xPathCollection);
				while (xpathReader.ReadUntilMatch())
				{
					writer.WriteNode(xpathReader.ReadSubtree(), true);
				}
			}

			bodyStream.Position = 0;
			envelopedStream.Position = 0;

			return bodyStream;
		}

		public static bool ContainsEnvelope(Stream byteStream)
		{
			if (byteStream.Length == 0)
			{
				return false;
			}

			bool containsEnvelope = false;

			using (var reader = XmlReader.Create(byteStream))
			{
				containsEnvelope = reader.ReadToDescendant("Envelope", soap11Namespace);
			}

			byteStream.Position = 0;

			return containsEnvelope;
		}
	}
}
