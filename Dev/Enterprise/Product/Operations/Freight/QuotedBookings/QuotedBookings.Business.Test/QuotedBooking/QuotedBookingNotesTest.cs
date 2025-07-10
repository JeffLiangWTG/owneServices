using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(QuotedBookingNotes))]
	public class QuotedBookingNotesTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCreateInstance()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new QuotedBookingNotes(null));
			AssertNoExceptionThrown(() => new QuotedBookingNotes(QuotedBooking));
		}

		public void TestQuotedBookingNotes()
		{
			QuotedBookingNotes notes = new QuotedBookingNotes(QuotedBooking);
			StmNote newNote = notes.AddNew();

			AssertEquals("parent", QuotedBooking, notes.Parent);
			AssertEquals("note type", typeof(QuotedBookingStmNote), newNote.GetType());
			AssertEquals("visible note type", typeof(QuotedBookingStmNote), notes.VisibleNotes.TypeOfElements);
			AssertEquals("all note type", typeof(QuotedBookingStmNote), notes.GetAllNotes().TypeOfElements);

			AssertEquals("visible notes collection type", typeof(QuotedBookingStmNoteCollectionView), notes.VisibleNotes.GetType());
			AssertEquals("all notes collection type", typeof(QuotedBookingStmNoteCollection), notes.GetAllNotes().GetType());
		}

		public void TestDetailedGoodsDescriptionNoteText_GivenValueChanges_ThenNotesShouldMatch()
		{
			var qb = QuotedBooking;

			AssertEquals("Initially there are no notes", 0, qb.Notes.GetAllNotes().Count);
			AssertEquals("Initially there are no notes", 0, qb.Booking.Notes.GetAllNotes().Count);
			AssertNotEquals(qb.Notes, qb.Booking.Notes);
			qb.DetailedGoodsDescriptionNoteText = "Description goes here";

			var qbNotes =
				qb.Notes.GetAllNotes().OfType<StmNote>()
				.Select(s => new
				{
					NoteText = (string)s.ST_NoteText,
					NoteType = (string)s.ST_Description
				}).ToArray();
			var bookingNotes =
				qb.Booking.Notes.GetAllNotes().OfType<StmNote>()
				.Select(s => new
				{
					NoteText = (string)s.ST_NoteText,
					NoteType = (string)s.ST_Description
				}).ToArray();
			var expectedNotes = new[] {
				new {
					NoteText = "Description goes here",
					NoteType = "Detailed Goods Description"
				} };

			AssertSequencesEqual("They contain the same notes", expectedNotes, qbNotes);
			AssertSequencesEqual("They contain the same notes", qbNotes, bookingNotes);
			AssertNotEquals("The actual notes object is different", qb.Notes, qb.Booking.Notes);
		}

		public void TestDetailedGoodDescriptionOnOneOffQuote()
		{
			var quote = Quote;
			var ooq = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);

			AssertEquals("Initially there are no notes", 0, ooq.Notes.GetAllNotes().Count);
			AssertNull("There is no booking", ooq.Booking);
			ooq.DetailedGoodsDescriptionNoteText = "Description goes here";

			AssertEquals("No actual description got saved", string.Empty, ooq.DetailedGoodsDescriptionNoteText);
			AssertEquals("There are still no notes", 0, ooq.Notes.GetAllNotes().Count);
		}

		public void TestConsigorAndConsigneeNotes()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment bookingShipment = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking qouteBooking = QuotedBooking.New(quote.PK, bookingShipment.PK, Factory);

			qouteBooking.TransportMode = Core.Constants.TransportModes.Air;
			qouteBooking.Origin = "AUSYD";
			qouteBooking.Destination = "USLAX";

			OrgHeader consignor = Factory.New<OrgHeader>();
			qouteBooking.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

			OrgHeader consignee = Factory.New<OrgHeader>();
			qouteBooking.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

			consignor.Notes.AddNew(false, PredefinedNoteTypes.Instance.PickupInstructionsNote.Description, "Consignor Pickup Instructions for Export").ST_NoteContextDirection = nameof(StmNoteContextDirection.E);
			consignor.Notes.AddNew(false, PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, "Consignor Delivery Instructions for Import").ST_NoteContextDirection = nameof(StmNoteContextDirection.I);

			consignee.Notes.AddNew(false, PredefinedNoteTypes.Instance.PickupInstructionsNote.Description, "Consignee Pickup Instructions for Export").ST_NoteContextDirection = nameof(StmNoteContextDirection.E);
			consignee.Notes.AddNew(false, PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, "Consignee Delivery Instructions for Import").ST_NoteContextDirection = nameof(StmNoteContextDirection.I);

			AssertEquals("QuotedBooking is an Export, so should show Consignor Export Notes, and Consignee Import Notes ... therefore should only show 2 notes", 2, qouteBooking.Notes.VisibleNotes.Count);
			string[] noteTexts = Array.ConvertAll(qouteBooking.Notes.VisibleNotes.ToArray<StmNote>(), (n) => (string)n.ST_NoteDataAsText);
			AssertContainsExactElementsInAnyOrder(new string[] { "Consignor Pickup Instructions for Export", "Consignee Delivery Instructions for Import" }, noteTexts);

			qouteBooking = new QuotedBookingTest.TestQuotedBookingExposer(quote.PK, bookingShipment.PK, Factory);
			qouteBooking.TransportMode = Core.Constants.TransportModes.Air;
			qouteBooking.Origin = "USLAX";
			qouteBooking.Destination = "AUSYD";

			consignor = Factory.New<OrgHeader>();
			qouteBooking.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

			consignee = Factory.New<OrgHeader>();
			qouteBooking.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

			consignor.Notes.AddNew(false, PredefinedNoteTypes.Instance.PickupInstructionsNote.Description, "Consignor Pickup Instructions for Export").ST_NoteContextDirection = nameof(StmNoteContextDirection.E);
			consignor.Notes.AddNew(false, PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, "Consignor Delivery Instructions for Import").ST_NoteContextDirection = nameof(StmNoteContextDirection.I);

			consignee.Notes.AddNew(false, PredefinedNoteTypes.Instance.PickupInstructionsNote.Description, "Consignee Pickup Instructions for Export").ST_NoteContextDirection = nameof(StmNoteContextDirection.E);
			consignee.Notes.AddNew(false, PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, "Consignee Delivery Instructions for Import").ST_NoteContextDirection = nameof(StmNoteContextDirection.I);

			AssertEquals("QuotedBooking is an Import, so should show Consignor Export Notes, and Consignee Import Notes ... therefore should only show 2 notes", 2, qouteBooking.Notes.VisibleNotes.Count);
			noteTexts = Array.ConvertAll(qouteBooking.Notes.VisibleNotes.ToArray<StmNote>(), (n) => (string)n.ST_NoteDataAsText);
			AssertContainsExactElementsInAnyOrder(new string[] { "Consignor Pickup Instructions for Export", "Consignee Delivery Instructions for Import" }, noteTexts);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return QuotedBooking.Notes;
		}

		#region Implementation

		QuotedBooking QuotedBooking
		{
			get { return quotedBooking ?? (quotedBooking = QuotedBooking.New(Quote.PK, Booking.PK, Factory)); }
		}
		QuotedBooking quotedBooking;

		ForwardingShipment Booking
		{
			get { return booking ?? (booking = QuotedBooking.CreateNewBooking(Factory)); }
		}
		ForwardingShipment booking;

		Quote Quote
		{
			get { return quote ?? (quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted)); }
		}
		Quote quote;

		#endregion
	}
}
