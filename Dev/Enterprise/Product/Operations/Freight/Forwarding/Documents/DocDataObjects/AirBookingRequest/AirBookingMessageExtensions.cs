using System.Linq;
using System.Security;
using System.Xml.Linq;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	static class AirBookingMessageExtensions
	{
		public static bool HasTermsAndConditions(this AirBookingRequest airBookingRequest)
		{
			return airBookingRequest
				?.TermsAndConditions
				?.Length > 0;
		}

		public static int CalculateLogs(this IVisualizerDocumentData documentData, string eventCode)
		{
			if (documentData is IStmALogParent logParent)
			{
				return logParent
					.Logs
					.GetAllLogs()
					.Cast<IStmALog>()
					.Count(l => l.SL_SE_NKEvent == eventCode);
			}

			return 0;
		}

		public static string AddCancellationNote(this string universalXml, string reasonForSending)
		{
			if (string.IsNullOrWhiteSpace(reasonForSending))
			{
				return universalXml;
			}

			var xml = XElement.Parse(universalXml);

			var shipment = xml
				.Descendants()
				.FirstOrDefault(d => d.Name.LocalName == nameof(UniversalDataBuss.DataObjects.Universal.Shipment));

			if (shipment == null)
			{
				return xml.ToString();
			}

			var notes = shipment
				.Descendants()
				.FirstOrDefault(d => d.Name.LocalName == nameof(UniversalDataBuss.DataObjects.Universal.Shipment.NoteCollection));

			if (notes == null)
			{
				notes = new XElement(xml.Name.Namespace + nameof(UniversalDataBuss.DataObjects.Universal.Shipment.NoteCollection));
				shipment.Add(notes);
			}

			notes.Add(new XElement(xml.Name.Namespace + nameof(UniversalDataBuss.DataObjects.Universal.Note),
				new XElement(xml.Name.Namespace + nameof(UniversalDataBuss.DataObjects.Universal.Note.Description), "ReasonForMessageCancellation"), // programmatic constant
				new XElement(xml.Name.Namespace + nameof(UniversalDataBuss.DataObjects.Universal.Note.IsCustomDescription), (NoResString)"true"), // programmatic constant
				new XElement(xml.Name.Namespace + nameof(UniversalDataBuss.DataObjects.Universal.Note.NoteText), SecurityElement.Escape(reasonForSending))));

			return xml.ToString();
		}
	}
}
