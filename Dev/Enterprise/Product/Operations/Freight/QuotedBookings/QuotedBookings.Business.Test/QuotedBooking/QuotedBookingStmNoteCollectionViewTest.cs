using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(QuotedBookingStmNoteCollectionView))]
	public class QuotedBookingStmNoteCollectionViewTest : BusinessObjectCollectionViewTestCase<QuotedBookingStmNoteCollectionView>
	{
		public void TestCreateInstance()
		{
			AssertExceptionThrown(typeof(NullReferenceException), () => new QuotedBookingStmNoteCollectionView(null));
			AssertNoExceptionThrown(() => new QuotedBookingStmNoteCollectionView(QuotedBooking));
		}

		public void TestIsRelatedNote()
		{
			QuotedBookingStmNoteCollectionViewForTest view = new QuotedBookingStmNoteCollectionViewForTest(QuotedBooking.New(ZGuid.Empty, Booking.PK, Factory));
			QuotedBookingStmNote note = Factory.New<QuotedBookingStmNote>();
			note.ST_Table = Booking.TableName;
			AssertEquals(true, view.IsRelatedNote(note));

			note.ST_ParentID = Booking.PK;
			AssertEquals(false, view.IsRelatedNote(note));

			note.ST_ParentID = ZGuid.NewZGuid();
			AssertEquals(true, view.IsRelatedNote(note));

			view = new QuotedBookingStmNoteCollectionViewForTest(QuotedBooking.New(Quote.PK, ZGuid.Empty, Factory));
			AssertEquals(true, view.IsRelatedNote(note));

			note.ST_ParentID = Quote.PK;
			AssertEquals(false, view.IsRelatedNote(note));

			note.ST_ParentID = ZGuid.NewZGuid();
			AssertEquals(true, view.IsRelatedNote(note));

			view = new QuotedBookingStmNoteCollectionViewForTest(QuotedBooking.New(Quote.PK, Booking.PK, Factory));

			note.ST_ParentID = ZGuid.Empty;
			AssertEquals(true, view.IsRelatedNote(note));

			note.ST_ParentID = Quote.PK;
			AssertEquals(false, view.IsRelatedNote(note));

			note.ST_ParentID = Booking.PK;
			AssertEquals(false, view.IsRelatedNote(note));

			note.ST_ParentID = ZGuid.NewZGuid();
			AssertEquals(true, view.IsRelatedNote(note));
		}

		public void TestParent()
		{
			QuotedBookingStmNoteCollectionView view = new QuotedBookingStmNoteCollectionView(QuotedBooking);
			AssertEquals(view.Parent, QuotedBooking);
		}

		protected override QuotedBookingStmNoteCollectionView GetCollectionToTest()
		{
			return new QuotedBookingStmNoteCollectionView(QuotedBooking);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			QuotedBookingStmNote note = Factory.New<QuotedBookingStmNote>();
			note.ST_Table = QuotedBooking.Booking.TableName;
			note.ST_ParentID = QuotedBooking.Booking.PK;
			return note;
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

		class QuotedBookingStmNoteCollectionViewForTest : QuotedBookingStmNoteCollectionView
		{
			public QuotedBookingStmNoteCollectionViewForTest(QuotedBooking quotedBooking)
				: base(quotedBooking)
			{
			}

			public new bool IsRelatedNote(StmNote note)
			{
				return base.IsRelatedNote(note);
			}
		}

		#endregion
	}
}
