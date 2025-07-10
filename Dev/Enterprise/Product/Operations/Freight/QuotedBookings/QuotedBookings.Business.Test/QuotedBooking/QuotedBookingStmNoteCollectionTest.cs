using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.TransportBooking;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(QuotedBookingStmNoteCollection))]
	public class QuotedBookingStmNoteCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAddingQuotedBookingStmNote()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			var handlingInstructionsDescription = "Goods Handling Instructions";

			var goodsNote = Factory.NewWithValidTestData<QuotedBookingStmNote>();
			goodsNote.ST_Description = handlingInstructionsDescription;

			_ = quotedBooking.Booking.Notes.GetAllNotes();

			quotedBooking.Notes.Add(goodsNote);

			AssertNoExceptionThrown(() => quotedBooking.Booking.Notes.FindByDescription(handlingInstructionsDescription));
		}

		public void TestCreateInstance()
		{
			AssertExceptionThrown(typeof(NullReferenceException), () => new QuotedBookingStmNoteCollection(null));
			AssertNoExceptionThrown(() => new QuotedBookingStmNoteCollection(QuotedBooking));
		}

		public void TestLoadElements()
		{
			StmNote note1 = Booking.Notes.AddNew();
			QuotedBooking quotedBooking = QuotedBooking.New(ZGuid.Empty, Booking.PK, Factory);
			QuotedBookingStmNoteCollection collection = new QuotedBookingStmNoteCollection(quotedBooking);
			collection.Load();

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(new[] { note1.PK }, collection.Select((elem) => elem.PK));
				AssertEquals(quotedBooking, collection[0].QuotedBooking);
				AssertEquals(Booking, collection[0].Master);
			});

			CombineAssertions("prerequisite - quote only note type", () =>
			{
				AssertCollectionNotContains(PredefinedNoteTypes.Instance.QuoteCoverPageText, Booking.NoteTypes);
				AssertCollectionContains(PredefinedNoteTypes.Instance.QuoteCoverPageText, Quote.NoteTypes);
			});

			CombineAssertions("prerequisite - shared note type", () =>
			{
				AssertCollectionContains(PredefinedNoteTypes.Instance.InternalWorkNotes, Booking.NoteTypes);
				AssertCollectionContains(PredefinedNoteTypes.Instance.InternalWorkNotes, Booking.NoteTypes);
			});

			StmNote note2 = Quote.Notes.AddNew();
			note2.ST_Description = PredefinedNoteTypes.Instance.QuoteCoverPageText.Description;
			StmNote note3 = Quote.Notes.AddNew();
			note3.ST_Description = PredefinedNoteTypes.Instance.InternalWorkNotes.Description;
			quotedBooking = QuotedBooking.New(Quote.PK, ZGuid.Empty, Factory);
			collection = new QuotedBookingStmNoteCollection(quotedBooking);
			collection.Load();

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(new[] { note2.PK, note3.PK }, collection.Select((elem) => elem.PK));
				AssertEquals(quotedBooking, collection[0].QuotedBooking);
				AssertEquals(Quote, collection[0].Master);
				AssertEquals(quotedBooking, collection[1].QuotedBooking);
				AssertEquals(Quote, collection[1].Master);
			});

			quotedBooking = QuotedBooking.New(Quote.PK, Booking.PK, Factory);
			collection = new QuotedBookingStmNoteCollection(quotedBooking);
			collection.Load();

			Func<ZGuid, QuotedBookingStmNote> findNote = (pk) => collection.Cast<QuotedBookingStmNote>().First((note) => note.PK == pk);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(new[] { note1.PK, note2.PK, note3.PK }, collection.Select((elem) => elem.PK));
				AssertEquals(quotedBooking, findNote(note1.PK).QuotedBooking);
				AssertEquals(Booking, findNote(note1.PK).Master);
				AssertEquals(quotedBooking, findNote(note2.PK).QuotedBooking);
				AssertEquals(Quote, findNote(note2.PK).Master);
				AssertEquals(quotedBooking, findNote(note3.PK).QuotedBooking);
				AssertEquals(Booking, findNote(note3.PK).Master);
			});
		}

		public void TestSetDefaults()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			QuotedBookingStmNoteCollection collection = new QuotedBookingStmNoteCollection(quotedBooking);
			QuotedBookingStmNote note = collection.AddNew();
			AssertEquals(booking, note.Master);
			AssertEquals("JobShipment", note.ST_Table);

			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			collection = new QuotedBookingStmNoteCollection(quotedBooking);
			note = collection.AddNew();
			AssertEquals(quote, note.Master);
			AssertEquals("RatingHeader", note.ST_Table);

			quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			collection = new QuotedBookingStmNoteCollection(quotedBooking);
			note = collection.AddNew();
			AssertEquals(booking, note.Master);
			AssertEquals("JobShipment", note.ST_Table);
		}

		public void TestExcludesDocumentNotes()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, shipment.PK, Factory);
			quotedBooking.Notes.GetAllNotes();

			var documentNote = Factory.New<DocumentNote>();
			documentNote.ST_ParentID = shipment.PK;
			(documentNote as IDocumentNote).MainBusinessObject = shipment;

			shipment.OnSaved(true);

			AssertCollectionNotContains
			(
				quotedBooking.Notes.GetAllNotes(),
				note => (note as QuotedBookingStmNote).ST_NoteType.Equals(nameof(StmNoteVisibility.DOC))
			);
		}

		public void TestRemovingQuotedBookingNoteFromRelatedEntityShouldNotThrowAnException()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			quotedBooking.Notes.AddNew(false, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, "marks & nums");

			Factory.Save();

			var factory = new BusinessObjectFactory();

			var dtbBooking = factory.New<IDtbBooking>();
			var dtbConsolidation = factory.New<IDtbBookingConsolidation>();

			dtbConsolidation.KB_ParentID = quotedBooking.PK;
			dtbConsolidation.KB_ParentTableCode = ViewQuotedBookingSchema.Constants.Prefix;

			dtbBooking.KM_KB_Booking = dtbConsolidation.PK;

			var dtbBookingNoteParent = dtbBooking as IStmNoteParent;

			if (dtbBookingNoteParent != null)
			{
				dtbBookingNoteParent.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "handle with care");
			}
			else
			{
				Fail("expected dtbBooking to have notes");
			}

			AssertNoExceptionThrown(factory.Save);

			var reletedNotes = dtbBookingNoteParent.Notes.GetAllRelatedNotesVisibleToCurrentCompany().ToArray();

			AssertEquals("Expected related notes for DtBBooking", 1, reletedNotes.Length);
			AssertType<QuotedBookingStmNote>("Expected related note to be Quoted Booking note", reletedNotes[0]);

			reletedNotes[0].Delete();

			AssertNoExceptionThrown(factory.Save);

			var quoteBookingNotes = quotedBooking.Notes.GetAllNotes();
			AssertEquals("Note was removed", 0, quoteBookingNotes.Count);

			quotedBooking.Booking.JS_GoodsDescription = "frozen ducks";
			AssertNoExceptionThrown(Factory.Save);
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

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new QuotedBookingStmNoteCollection(QuotedBooking);
		}

		#endregion
	}
}
