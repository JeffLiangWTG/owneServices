using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public abstract class OriginalBillNotesUpdater
	{
		protected readonly BusinessObjectFactory factory;

		protected readonly ForwardingShipment shipment;

		protected OriginalBillNotesUpdater(ForwardingShipment shipment)
		{
			this.factory = shipment.Factory;
			this.shipment = shipment;
		}

		public abstract void PopulateOriginalBillNotes();

		protected StmNote LoadOrCreateOriginalBillNotesStmNote()
		{
			return LoadStmNote() ?? CreateNewStmNote();
		}

		StmNote LoadStmNote()
		{
			var query = new ZQuery(StmNoteSchema.ST_ParentID, shipment.PK);
			query.AddToFilter(StmNoteSchema.ST_Table, JobShipmentSchema.Constants.TableName);
			query.AddToFilter(StmNoteSchema.ST_Description, PredefinedNoteTypes.Instance.OriginalBillNotes.ToString());
			query.AddToFilter(StmNoteSchema.ST_NoteType, nameof(CargoWise.Definitions.StmNoteVisibility.INT));
			query.FetchOnlyFromLocalCache = !shipment.IsInDatabase;

			return factory.LoadTop1<ForwardingShipmentStmNote>(query);
		}

		StmNote CreateNewStmNote()
		{
			var note = factory.New<ForwardingShipmentStmNote>();
			note.ST_ParentID = shipment.PK;
			note.ST_Table = JobShipmentSchema.Constants.TableName;
			note.ST_Description = PredefinedNoteTypes.Instance.OriginalBillNotes.ToString();
			note.ST_NoteType = nameof(CargoWise.Definitions.StmNoteVisibility.INT);

			shipment.Notes.Add(note);

			return note;
		}

		protected ZString ConcatOrganizationAddressDetails(ZString companyName, ZString address1, ZString address2, ZString city, ZString state, ZString postCode, ZString country)
		{
			var stringBuilder = new ZStringBuilder();

			stringBuilder.AppendIfNotEmpty(companyName);
			stringBuilder.AppendIfNotEmpty(address1);
			stringBuilder.AppendIfNotEmpty(address2);
			stringBuilder.AppendIfNotEmpty(ZString.Join(" ", new[] { city, state, postCode }));
			stringBuilder.AppendIfNotEmpty(country);

			return stringBuilder.ToStringWithDelimiterBetweenAppends(", ");
		}
	}
}
