using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;

namespace CargoWise.RefDbRepo.Staging.DataPublishingProcessor
{
	public class EHubMessageBuilder
	{
		readonly string _recipient;
		readonly string _message;

		public EHubMessageBuilder(string message, string recipient)
		{
			_message = message;
			_recipient = recipient;
		}

		public IeHubMessage Build()
		{
			var settings = new XmlWriterSettings
			{
				OmitXmlDeclaration = true
			};

			var messageStream = new VirtualStream();
#pragma warning disable CA2000 // Dispose objects before losing scope
			var xmlWriter = XmlWriter.Create(messageStream, settings);
#pragma warning restore CA2000 // Dispose objects before losing scope
			var genericMessage = BuildGenericMessage(_message);
			if (genericMessage != null)
			{
				genericMessage.WriteTo(xmlWriter);
				xmlWriter.Flush();

				messageStream.Seek(0, SeekOrigin.Begin);

				return BuildFromStream(messageStream);
			}
			else
			{
				return null;
			}
		}

		protected IeHubMessage BuildFromStream(VirtualStream messageStream)
		{
			return new eHubMessage(
				Guid.NewGuid(),
				ApplicationConfig.Messaging.From,
				_recipient,
				ApplicationConfig.Messaging.SchemaType,
				ApplicationConfig.Messaging.ApplicationCode,
				ApplicationConfig.Messaging.SchemaName,
				messageStream);
		}

		protected XDocument BuildGenericMessage(string message)
		{
			var ns = ApplicationConfig.Messaging.SchemaName.Substring(0, ApplicationConfig.Messaging.SchemaName.LastIndexOf('#'));
			var result = XDocument.Parse($@"<ns0:GenericMessageInterchange xmlns:ns0 = ""{ns}""/>");

			result.Root.Add(
				new XElement(
					"Header",
					new XElement("SenderID", ApplicationConfig.Messaging.From),
					new XElement("RecipientID", _recipient),
					new XElement("InterchangeType", ApplicationConfig.Messaging.InterchangeType),
					new XElement("InterchangeNumber", Guid.NewGuid().ToString())));

			if (!string.IsNullOrEmpty(message))
			{
				var bodyXml = XElement.Parse(message);
				result.Root.Add(
					new XElement("Body", bodyXml)
				);

				return result;
			}
			else
			{
				Console.Error.WriteLine("The system is trying to send an empty message, this should never happen");
				return null;
			}
		}
	}
}
