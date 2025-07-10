using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(QuotedBookingStmNote))]
	public class QuotedBookingStmNoteTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsParentTemplateRecord()
		{
			var templateRecord = Factory.NewWithValidTestData<StmTemplateRecord>();
			templateRecord.STR_ModuleID = "QuotedBookings";
			var quotedBooking = QuotedBooking.New(Factory, templateRecord);
			var stmNote = quotedBooking.Notes.AddNew();
			Assert(stmNote.IsParentTemplateRecord);
		}

		public void TestNoteSource()
		{
			QuotedBookingStmNote note = Factory.New<QuotedBookingStmNote>();
			note.Master = Factory.New<ForwardingShipment>();
			AssertEquals("Booking", note.ST_NoteSource);
			note.Master = Factory.New<Quote>();
			AssertEquals("Quote", note.ST_NoteSource);
		}

		public void TestIsRelatedInParentView()
		{
			Quote quote = Factory.New<Quote>();
			StmNoteCollectionView noteView = new StmNoteCollectionView(quote);
			QuotedBookingStmNote note = Factory.New<QuotedBookingStmNote>();
			quote.Notes.Add(note);
			AssertEquals("No exception thrown when parent is a Quote", false, ((IStmNoteInternals)note).IsRelatedInParentView);
		}

		public void TestDescription()
		{
			CombineAssertions("prerequisite - quote only note type", () =>
			{
				AssertCollectionNotContains(PredefinedNoteTypes.Instance.QuoteCoverPageText, Booking.NoteTypes);
				AssertCollectionContains(PredefinedNoteTypes.Instance.QuoteCoverPageText, Quote.NoteTypes);
			}

			);
			CombineAssertions("prerequisite - shared note type", () =>
			{
				AssertCollectionContains(PredefinedNoteTypes.Instance.InternalWorkNotes, Booking.NoteTypes);
				AssertCollectionContains(PredefinedNoteTypes.Instance.InternalWorkNotes, Booking.NoteTypes);
			}

			);
			QuotedBookingStmNote note = Factory.New<QuotedBookingStmNote>();
			note.QuotedBooking = QuotedBooking.New(ZGuid.Empty, Booking.PK, Factory);
			note.ST_Description = "zzz";
			AssertEquals(Booking, note.Master);
			note.ST_Description = PredefinedNoteTypes.Instance.QuoteCoverPageText.Description;
			AssertEquals(Booking, note.Master);
			note.ST_Description = PredefinedNoteTypes.Instance.InternalWorkNotes.Description;
			AssertEquals(Booking, note.Master);
			note.QuotedBooking = QuotedBooking.New(Quote.PK, ZGuid.Empty, Factory);
			note.ST_Description = "zzz";
			AssertEquals(Quote, note.Master);
			note.ST_Description = PredefinedNoteTypes.Instance.QuoteCoverPageText.Description;
			AssertEquals(Quote, note.Master);
			note.ST_Description = PredefinedNoteTypes.Instance.InternalWorkNotes.Description;
			AssertEquals(Quote, note.Master);
			note.QuotedBooking = QuotedBooking.New(Quote.PK, Booking.PK, Factory);
			note.ST_Description = "zzz";
			AssertEquals(Booking, note.Master);
			note.ST_Description = PredefinedNoteTypes.Instance.QuoteCoverPageText.Description;
			AssertEquals(Quote, note.Master);
			note.ST_Description = PredefinedNoteTypes.Instance.InternalWorkNotes.Description;
			AssertEquals(Booking, note.Master);
		}

		public void TestDescriptionList()
		{
			QuotedBookingStmNote note = Factory.New<QuotedBookingStmNote>();
			note.QuotedBooking = QuotedBooking.New(ZGuid.Empty, Booking.PK, Factory);
			foreach (var item in Booking.NoteTypes)
			{
				AssertCollectionContains(item, note.ST_Description_List);
			}

			note.QuotedBooking = QuotedBooking.New(Quote.PK, ZGuid.Empty, Factory);
			foreach (var item in Quote.NoteTypes)
			{
				AssertCollectionContains(item, note.ST_Description_List);
			}

			note.QuotedBooking = QuotedBooking.New(Quote.PK, Booking.PK, Factory);
			foreach (var item in Booking.NoteTypes.Cast<PredefinedNoteType>().Concat(Quote.NoteTypes.Cast<PredefinedNoteType>()).Distinct())
			{
				AssertCollectionContains(item, note.ST_Description_List);
			}
		}

		public void TestMaster()
		{
			QuotedBookingStmNote note = Factory.New<QuotedBookingStmNote>();
			note.Master = Booking;
			AssertEquals("JobShipment", note.ST_Table);
			AssertEquals(Booking.PK, note.ST_ParentID);
			note.Master = Quote;
			AssertEquals("RatingHeader", note.ST_Table);
			AssertEquals(Quote.PK, note.ST_ParentID);
			note.Master = null;
			AssertEquals(ZString.Empty, note.ST_Table);
			AssertEquals(ZGuid.Empty, note.ST_ParentID);
		}

		public void TestOverrideValidationMasters()
		{
			var note1 = Factory.New<QuotedBookingStmNote>();
			note1.Master = QuotedBooking.CreateNewBooking(Factory);
			note1.QuotedBooking = QuotedBooking.New(Quote.PK, Booking.PK, Factory);

			AssertCollectionContains(note1.QuotedBooking.Booking, note1.OverrideValidationMasters);
			AssertCollectionContains(note1.QuotedBooking.Quote, note1.OverrideValidationMasters);
		}

		public void TestValidation()
		{
			QuotedBookingStmNote note = Factory.New<QuotedBookingStmNote>();
			AssertEquals(typeof(QuotedBookingStmNoteValidation), note.Validation.GetType());
		}

		public void TestGetClone_QuotedBookingStmNote()
		{
			QuotedBookingStmNote note = Factory.New<QuotedBookingStmNote>();
			note.QuotedBooking = QuotedBooking.New(ZGuid.Empty, Booking.PK, Factory);
			QuotedBookingStmNote clone = (QuotedBookingStmNote)note.GetCloneForPopup();
			AssertEquals(note.QuotedBooking, clone.QuotedBooking);
		}

		public void TestFindMaster_MultilingualST_Description()
		{
			var resKey_quote = ZGuid.NewZGuid().ToStringKey();
			var resKey_booking = ZGuid.NewZGuid().ToStringKey();
			var quotedType = (NoResString)"quote type";
			var bookingType = (NoResString)"booking type";
			var quoteNoteType = new PredefinedNoteType(quotedType, StmNoteVisibility.PUB, true, true, true, true);
			var bookingNoteType = new PredefinedNoteType(bookingType, StmNoteVisibility.PUB, true, true, true, true);
			var customNoteTypes = new NoteTypeCollection();
			customNoteTypes.Add(quoteNoteType);
			customNoteTypes.Add(bookingNoteType);
			var quoteCustomNoteTypes = new NoteTypeCollection();
			quoteCustomNoteTypes.Add(quoteNoteType);
			var bookingCustomNoteTypes = new NoteTypeCollection();
			bookingCustomNoteTypes.Add(bookingNoteType);
			var note = Factory.New<QuotedBookingStmNote>();
			note.ST_Description = "quote type";
			note.Master = QuotedBooking.CreateNewBooking(Factory);
			note.QuotedBooking = QuotedBooking.New(Quote.PK, Booking.PK, Factory);
			((IStmNoteParent)note.QuotedBooking).CustomNoteTypesDelegate += () => customNoteTypes;
			((IStmNoteParent)Quote).CustomNoteTypesDelegate += () => quoteCustomNoteTypes;
			((IStmNoteParent)Booking).CustomNoteTypesDelegate += () => bookingCustomNoteTypes;
			using (Res.TemporarilySwitchLanguage(Core.Constants.Languages.German))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey_quote, new ResourceStringData(resKey_quote, "quote type in German"));
				mockRes.Put(resKey_booking, new ResourceStringData(resKey_booking, "booking type in German"));
				var master = QuotedBookingStmNote.FindMaster(note.QuotedBooking, note);
				AssertEquals("The master should be Quote", Quote.PK, master.NotesParentPK);
				note.ST_Description = "booking type";
				master = QuotedBookingStmNote.FindMaster(note.QuotedBooking, note);
				AssertEquals("The master should be Booking", Booking.PK, master.NotesParentPK);
			}
		}

		#region Implementation
		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var note = Factory.NewWithValidTestData<QuotedBookingStmNote>();
			note.Master = Factory.NewWithValidTestData<ForwardingShipment>();
			return note;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = factory.NewWithValidTestData<QuotedBookingStmNote>();
			result.Master = factory.NewWithValidTestData<ForwardingShipment>();
			return result;
		}

		ForwardingShipment Booking
		{
			get
			{
				return booking ?? (booking = QuotedBooking.CreateNewBooking(Factory));
			}
		}

		ForwardingShipment booking;
		Quote Quote
		{
			get
			{
				return quote ?? (quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted));
			}
		}

		Quote quote;
		#endregion
	}
}
