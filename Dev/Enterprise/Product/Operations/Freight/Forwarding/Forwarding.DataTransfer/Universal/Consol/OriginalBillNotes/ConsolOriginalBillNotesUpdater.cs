using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	internal static class ConsolOriginalBillNotesUpdater
	{
		public static void OnEventsRelatedToOriginalBillNotes(this ForwardingConsol consol, UniversalEvent eventAdded)
		{
			var relatedUniversalShipment = GetRelatedUniversalShipment(consol);
			if (relatedUniversalShipment != null)
			{
				return;
			}

			var billOfLadingBillStatus = ForwardingConsol.BillStatusUpdatedEventTypeToBillOfLadingBillStatus(eventAdded.EventParameters.Type);
			if (billOfLadingBillStatus.IsEmpty)
			{
				return;
			}

			var newNoteText = new ZStringBuilder(ZDateTimeOffset.UtcNow.ToString());

			var masterBillNumber = eventAdded.ContextCollection?.Find(c => c.Type.Type.Value == "MBOLNumber")?.Value ?? ZString.Empty;
			if (!masterBillNumber.IsEmpty)
			{
				newNoteText.Append(" ").Append(masterBillNumber);
			}

			var billOfLadingBillStatusDescription = billOfLadingBillStatus.IsEmpty ? string.Empty : consol.Lookups.BillOfLadingBillStatusList.GetDescriptionFromCode(billOfLadingBillStatus);
			if (!string.IsNullOrEmpty(billOfLadingBillStatusDescription))
			{
				newNoteText.Append(" ").Append(billOfLadingBillStatusDescription);
			}

			var referenceNumber = eventAdded.EventParameters.ReferenceNumber ?? ZString.Empty;
			if (!referenceNumber.IsEmpty)
			{
				newNoteText.Append(" ").Append($"[Reference:{referenceNumber}]");
			}

			var note = new OriginalBillNotesService(consol.Factory)
				.LoadOrCreateStmNoteForReaderUpdate(consol.PK, JobConsolSchema.Constants.TableName, consol.IsInDatabase, PredefinedNoteTypes.Instance.OriginalBillNotes.ToString());
			if (!note.ST_NoteText.IsEmpty)
			{
				newNoteText.AppendLine().AppendLine().Append(note.ST_NoteText.ToString());
			}

			note.ST_NoteText = newNoteText.ToString();
		}

		static UniversalShipment GetRelatedUniversalShipment(ForwardingConsol consol)
		{
			UniversalShipment uShipment = null;

			var bluLog = consol.Logs.MostRecentLogByEventTime(Events.BillStatusUpdated);
			if (bluLog.RelatedEDIMessage?.Message is IEDIMessage linkedMessage
				&& linkedMessage.EM_MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalEvent
				&& !bluLog.RelatedEDIMessage.Message.EM_EI.IsEmpty)
			{
				var zQuery = new ZQuery();
				zQuery.AddToFilter(EDIMessageSchema.EM_EI, bluLog.RelatedEDIMessage.Message.EM_EI);
				zQuery.AddToFilter(EDIMessageSchema.EM_MessageSubType, EDIMessageSubTypeList.Codes.XmlUniversalShipment);
				var uShipmentEDIMessage = consol.Factory.LoadTop1<IEDIMessage>(zQuery);
				uShipment = uShipmentEDIMessage?.GetEM_MessageTextReader().Parse<UniversalShipment>();
			}

			return uShipment;
		}
	}
}
