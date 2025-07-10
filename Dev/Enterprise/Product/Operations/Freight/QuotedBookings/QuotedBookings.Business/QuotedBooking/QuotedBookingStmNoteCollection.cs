using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public sealed class QuotedBookingStmNoteCollection : BusinessObjectCollection<QuotedBookingStmNote>, IBusinessObjectCollectionWithMaster
	{
		public QuotedBookingStmNoteCollection(QuotedBooking quotedBooking)
			: base(quotedBooking.Factory)
		{
			this.quotedBooking = quotedBooking;
		}

		readonly QuotedBooking quotedBooking;

		public BusinessObject Master => quotedBooking;

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(QuotedBookingStmNote);
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			return new StmNoteQuery();
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return BuildRelationshipQuery(quotedBooking);
		}

		internal static ZQuery BuildRelationshipQuery(QuotedBooking quotedBooking)
		{
			ZQuery bookingNotesQuery = new ZQuery();

			if (quotedBooking.Booking != null)
			{
				bookingNotesQuery.AddToFilter(StmNoteSchema.ST_ParentID, quotedBooking.Booking.PK);
				bookingNotesQuery.AddToFilter(StmNoteSchema.ST_Table, ForwardingShipment.Schema.TableName);
			}

			ZQuery quoteNotesQuery = new ZQuery();

			if (quotedBooking.Quote != null)
			{
				quoteNotesQuery.AddToFilter(StmNoteSchema.ST_ParentID, quotedBooking.Quote.PK);
				quoteNotesQuery.AddToFilter(StmNoteSchema.ST_Table, Quote.Schema.TableName);
			}

			ZQuery bookingQuoteQuery = new ZQuery();
			bookingQuoteQuery.AddToFilter(bookingNotesQuery);
			bookingQuoteQuery.AddToFilter(quoteNotesQuery, JoinCondition.Or);

			ZQuery query = new StmNoteQuery();
			query.AddToFilter(bookingQuoteQuery);
			return query;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			QuotedBookingStmNote note = (QuotedBookingStmNote)child;
			note.QuotedBooking = quotedBooking;
			IStmNoteParent master = QuotedBookingStmNote.FindMaster(quotedBooking, note);
			note.ST_Table = master.NotesParentTableName;
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);

			QuotedBookingStmNote note = (QuotedBookingStmNote)child;
			note.QuotedBooking = quotedBooking;
			note.Master = QuotedBookingStmNote.FindMaster(quotedBooking, note);
		}

		public override void Add(BusinessObject businessObject)
		{
			base.Add(businessObject);

			QuotedBookingStmNote note = (QuotedBookingStmNote)businessObject;

			if (note.Master != null && note.Master == quotedBooking.Booking)
			{
				var reloadedNote = businessObject.Factory.Load<ForwardingShipmentStmNote>(note.PK);
				quotedBooking.Booking.Notes.Add(reloadedNote);
			}
			else if (note.Master != null && note.Master == quotedBooking.Quote)
			{
				quotedBooking.Quote.Notes.Add(note);
			}
		}
	}
}
