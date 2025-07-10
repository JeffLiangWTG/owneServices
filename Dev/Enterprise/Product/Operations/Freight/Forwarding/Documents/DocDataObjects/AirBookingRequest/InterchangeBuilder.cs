using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed class InterchangeBuilder
	{
		public InterchangeBuilder(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
		}

		readonly BusinessObjectFactory factory;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		const string eBookingAPI = "eBooking API";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		const string interchangeBody = "Body";
		const string universalShipment = "UniversalShipment"; // programmatic constant
		const string universalEvent = "UniversalEvent"; // programmatic constant

		public enum MessageDirection
		{
			Unknown,
			Transmit,
			Receive
		}

		#region Implementation

		public IXmlEDIInterchange CreateInterchange(string content, MessageDirection messageDirection)
		{
			var interchange = factory.New<IXmlEDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;

			if (messageDirection == MessageDirection.Receive)
			{
				interchange.EI_From = eBookingAPI;
				interchange.EI_To = Env.CurrentCompany.GetLicenceCode();
				interchange.EI_ReceiveTransmit = EDICommunicationsModeCommsDirectionList.Codes.Receive;
				interchange.EI_Status = EDIInterchangeStatusList.Codes.Received;
			}
			else
			{
				interchange.EI_From = Env.CurrentCompany.GetLicenceCode();
				interchange.EI_To = eBookingAPI;
				interchange.EI_ReceiveTransmit = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				interchange.EI_Status = EDIInterchangeStatusList.Codes.Sent;
			}

			interchange.EI_InterchangeType = EDIMessageTypeList.Codes.XMS;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eAdaptor;
			interchange.EI_BodyText = content;

			return interchange;
		}

		public bool TryParseUniversalXml(string xml, MessageDirection messageDirection, out IXmlEDIInterchange interchange)
		{
			interchange = null;

			if (string.IsNullOrWhiteSpace(xml)
				|| messageDirection == MessageDirection.Unknown)
			{
				return false;
			}

			XDocument doc = null;

			try
			{
				doc = XDocument.Parse(xml);
			}
			catch (XmlException)
			{
				return false;
			}

			var body = doc
				.Root?
				.Elements()
				.FirstOrDefault(elem => elem.Name.LocalName == interchangeBody);

			if (body == null
				|| body.IsEmpty)
			{
				return false;
			}

			interchange = CreateInterchange(xml, messageDirection);

			foreach (var uxml in body.Elements())
			{
				var messageSubType = string.Empty;

				switch (uxml.Name.LocalName)
				{
					case universalShipment:
						messageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
						break;

					case universalEvent:
						messageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
						break;

					default:
						continue;
				}

				var message = interchange.AddNeweHubMessage();
				message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
				message.EM_MessageType = EDIMessageTypeList.Codes.XDC;

				if (messageDirection == MessageDirection.Receive)
				{
					message.EM_ReceiveTransmit = EDICommunicationsModeCommsDirectionList.Codes.Receive;
					message.EM_Status = EDIMessageStatusList.Codes.Received;
				}
				else
				{
					message.EM_ReceiveTransmit = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
					message.EM_Status = EDIMessageStatusList.Codes.Sent;
				}

				message.EM_MessageSubType = messageSubType;
				message.EM_MessageText = uxml.ToString();
			}

			return true;
		}

		#endregion
	}
}
