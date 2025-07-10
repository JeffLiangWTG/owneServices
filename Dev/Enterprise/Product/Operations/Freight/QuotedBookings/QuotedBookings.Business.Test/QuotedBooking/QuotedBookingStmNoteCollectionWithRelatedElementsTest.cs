using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(QuotedBookingStmNoteCollectionWithRelatedElements))]
	public class QuotedBookingStmNoteCollectionWithRelatedElementsTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new QuotedBookingStmNoteCollectionWithRelatedElements(QuotedBooking);
		}

		public void TestRemoveAllRelatedElements()
		{
			QuotedBookingStmNoteCollectionWithRelatedElements collection = new QuotedBookingStmNoteCollectionWithRelatedElements(QuotedBooking.New(ZGuid.Empty, Booking.PK, Factory));

			QuotedBookingStmNote note1 = Factory.New<QuotedBookingStmNote>();
			note1.ST_Table = Booking.TableName;
			note1.ST_ParentID = Booking.PK;

			QuotedBookingStmNote note2 = Factory.New<QuotedBookingStmNote>();
			note2.ST_Table = Booking.TableName;
			note2.ST_ParentID = ZGuid.NewZGuid();

			collection.AddRange(note1, note2);
			collection.RemoveAllRelatedElements();
			AssertContainsExactElementsInAnyOrder(new[] { note1 }, collection);

			QuotedBookingStmNote note3 = Factory.New<QuotedBookingStmNote>();
			note2.ST_Table = Quote.TableName;
			note3.ST_ParentID = Quote.PK;

			collection = new QuotedBookingStmNoteCollectionWithRelatedElements(QuotedBooking.New(Quote.PK, ZGuid.Empty, Factory));
			collection.AddRange(note2, note3);
			collection.RemoveAllRelatedElements();
			AssertContainsExactElementsInAnyOrder(new[] { note3 }, collection);

			collection = new QuotedBookingStmNoteCollectionWithRelatedElements(QuotedBooking.New(Quote.PK, Booking.PK, Factory));
			collection.AddRange(note1, note2, note3);
			collection.RemoveAllRelatedElements();
			AssertContainsExactElementsInAnyOrder(new[] { note1, note3 }, collection);
		}

		[ExpectNoExceptions()]
		public override void TestLoad()
		{
			try
			{
				ZQuery top1Filter = new ZQuery(StmNoteSchema.ST_Table, Booking.TableName);
				top1Filter.MaximumRows = 1;
				Collection.Load(top1Filter);
			}
			catch (NotSupportedException) // Load not supported for this collection
			{
				Assert(true);
			}
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
