using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Event = Enterprise.ZArchitecture.Business.Event;

namespace Enterprise.Freight.Agency.DataTransfer
{
	class BillOfLadingStatusDataObjectWriter : BillOfLadingDataObjectWriter
	{
		public BillOfLadingStatusDataObjectWriter(IDataWritingManager manager, BillOfLadingContainer container = null) : base(manager, container)
		{
		}

		public Event Event { get; set; }
		public string ReasonForRejectionNoteText { get; set; }
		public string DataContextDocumentName { get; set; }

		protected override void PopulateShipment(BillOfLading sourceBO, Shipment dataObject)
		{
			base.PopulateShipment(sourceBO, dataObject);

			var purposeList = new CodeDescriptionPairList();
			purposeList.AddPair(Event.Code, Event.Description);
			dataObject.DataContext.SetDocumentaryOverride(DataContextDocumentName, Event.Code, purposeList, ZBool.False, 1, 1);
		}

		protected override void WriteNotes(BillOfLading sourceBO, Shipment dataObject)
		{
			if (!string.IsNullOrEmpty(ReasonForRejectionNoteText))
			{
				var note = new Note
				{
					Description = (NoResString)"Reason for Rejection",   // Hard coded document name.
					NoteText = ReasonForRejectionNoteText,
					IsCustomDescription = false
				};

				if (dataObject.NoteCollection == null)
				{
					dataObject.SetNoteCollection(() => new DataObjectList<Note> { note });
				}
				else
				{
					dataObject.NoteCollection.Add(note);
				}
			}
		}
	}
}
