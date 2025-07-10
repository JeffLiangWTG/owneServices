using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Application;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	static class BookingConfirmationHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		public static UniversalShipment GetBookingConfirmationUniversalXml(this ForwardingConsol consol)
		{
			var dataImportLogs = consol
				?.Logs
				.GetAllLogs()
				.Cast<StmALog>()
				.Where(log => log.SL_SE_NKEvent == Events.DataLinkedCode)
				.OrderByDescending(log => log.SL_PostedTimeUtc)
				.ToArray();

			foreach (var dataImportLog in dataImportLogs ?? Array.Empty<StmALog>())
			{
				if (dataImportLog.RelatedEDIMessage?.Message is IEDIMessage linkedMessage
					&& linkedMessage.EM_MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalShipment)
				{
					var usxml = dataImportLog.RelatedEDIMessage.Message.GetEM_MessageTextReader().Parse<UniversalShipment>();

					var documentName = usxml
						?.DataContext
						?.DocumentaryOverride
						?.DocumentName;

					const string bookingConfirmationDocumentName = "Booking Confirmation";

					if (bookingConfirmationDocumentName.Equals(documentName, StringComparison.InvariantCultureIgnoreCase))
					{
						return usxml;
					}
				}
			}

			return null;
		}

		public static UniversalShipment GetUniversalXmlFromBookingRequestDialog(this ForwardingConsol consol)
		{
			var documentDataLoader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();
			var documentData = documentDataLoader.Load(consol, ConsolDocumentDataStoreNames.SeaBookingRequest2);
			var lastDialog = (documentData as IStmALogParent)?.GetDialogs(ConsolDocumentNames.BookingRequest, false)?.LastOrDefault();
			var transmissionCode = lastDialog?.TransmissionCode ?? string.Empty;

			if (transmissionCode == Events.MessageSentCode && lastDialog.HasBeenAccepted() && !lastDialog.IsWithdrawal())
			{
				var bookingRequest = lastDialog?.TransmittedMessageText;
				if (!string.IsNullOrEmpty(bookingRequest))
				{
					using (var inputReader = new StringReader(bookingRequest))
					using (var uxml = inputReader.Parse<UniversalShipment>())
					{
						var placeOfReceipt = XDocument.Parse(bookingRequest).XPathSelectElement((NoResString)"//*[local-name()='Shipment']/*[local-name()='PlaceOfReceipt']"); // xml element name
						var placeOfDelivery = XDocument.Parse(bookingRequest).XPathSelectElement((NoResString)"//*[local-name()='Shipment']/*[local-name()='PlaceOfDelivery']"); // xml element name
						var portOfLoading = XDocument.Parse(bookingRequest).XPathSelectElement((NoResString)"//*[local-name()='Shipment']/*[local-name()='PortOfLoading']"); // xml element name
						var portOfDischarge = XDocument.Parse(bookingRequest).XPathSelectElement((NoResString)"//*[local-name()='Shipment']/*[local-name()='PortOfDischarge']"); // xml element name
						uxml.PlaceOfReceipt = GetUNLOCO(placeOfReceipt);
						uxml.PlaceOfDelivery = GetUNLOCO(placeOfDelivery);
						uxml.PortOfLoading = GetUNLOCO(portOfLoading);
						uxml.PortOfDischarge = GetUNLOCO(portOfDischarge);

						var additionalReferences = XDocument.Parse(bookingRequest).XPathSelectElements((NoResString)"//*[local-name()='AdditionalReferenceCollection']/*[local-name()='AdditionalReference']"); // xml element name
						if (additionalReferences?.Any() ?? false)
						{
							uxml.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
							foreach (var reference in additionalReferences)
							{
								var code = reference.XPathSelectElement((NoResString)"*[local-name()='Type']")?.Value;   // xml element name
								var value = reference.XPathSelectElement((NoResString)"*[local-name()='ReferenceNumber']")?.Value;   // xml element name
								uxml.AdditionalReferenceCollection.Add(new AdditionalReference { Type = new EntryType { Code = code }, ReferenceNumber = value });
							}
						}

						var transportLegs = XDocument.Parse(bookingRequest).XPathSelectElements((NoResString)"//*[local-name()='TransportLegCollection']/*[local-name()='TransportLeg']"); // xml element name
						if (transportLegs?.Any() ?? false)
						{
							foreach (var transportLeg in transportLegs)
							{
								var legOrder = transportLeg.XPathSelectElement((NoResString)"*[local-name()='LegOrder']")?.Value; // xml element name
								var transportPortOfLoading = transportLeg.XPathSelectElement((NoResString)"*[local-name()='PortOfLoading']");   // xml element name
								var transportPortOfDischarge = transportLeg.XPathSelectElement((NoResString)"*[local-name()='PortOfDischarge']");   // xml element name

								var transport = uxml.TransportLegCollection.FirstOrDefault(t => t.LegOrder.ToString() == legOrder);
								transport.PortOfLoading = GetUNLOCO(transportPortOfLoading);
								transport.PortOfDischarge = GetUNLOCO(transportPortOfDischarge);
							}
						}
						return uxml;
					}
				}
			}
			return null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Element name")]
		static UNLOCO GetUNLOCO(XElement element)
		{
			if (element != null)
			{
				return new UNLOCO
				{
					Code = element.Value,
					Name = element.Attribute("Name")?.Value
				};
			}

			return null;
		}
	}
}
