using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Freight.Integration.CFS;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(QuotedBookingProcessTaskCollection))]
	public class QuotedBookingProcessTaskCollectionTest : ProcessTaskCollectionTest<QuotedBookingProcessTaskCollection>
	{
		public void TestParent()
		{
			AssertNotNull("Parent is QuotedBooking or TrackingBooking", Collection.Parent as QuotedBooking);
		}

		public void TestAdditionalFilter()
		{
			var processTaskCollection = new QuotedBookingProcessTaskCollectionForTest(QuotedBooking.New(Integration.QuoteBookingType.BookingWithQuote, Factory));
			var expectedFilter = new ZQuery(ProcessTasksSchema.P9_ParentTableCode, ViewQuotedBookingSchema.Constants.Prefix);
			AssertCollectionContains(expectedFilter, processTaskCollection.AdditionalFilter.GetCompositeParts());
		}

		public void TestDoNotLoadQuotedBookingTasksIntoCFSShipmentCollections()
		{
			var booking = QuotedBooking.New(Integration.QuoteBookingType.QuickBooking, Factory);
			var processTaskCollection = new QuotedBookingProcessTaskCollectionForTest(booking);
			processTaskCollection.Tasks.AddNew();
			Factory.Save();

			var shipment = Factory.Load<ICFSShipment>(booking.Booking.PK);
			AssertEquals(0, ((IWorkflowProvider)shipment).WorkflowItems.Tasks.Count);
		}

		#region OriginCountry / DestinationCountry

		public void TestOriginCountry()
		{
			QuotedBooking.Origin = "AUSYD";
			AssertEquals("AU", Collection.OriginCountry);
		}

		public void TestDestinationCountry()
		{
			QuotedBooking.Destination = "USLOL";
			AssertEquals("US", Collection.DestinationCountry);
		}

		#endregion

		#region Implementation

		protected override QuotedBookingProcessTaskCollection GetCollectionToTestCore()
		{
			return new QuotedBookingProcessTaskCollection(QuotedBooking);
		}

		QuotedBooking QuotedBooking
		{
			get
			{
				if (quotedBooking == null)
				{
					quotedBooking = QuotedBooking.New(Integration.QuoteBookingType.BookingWithQuote, Factory);
				}
				return quotedBooking;
			}
		}
		QuotedBooking quotedBooking;

		class QuotedBookingProcessTaskCollectionForTest : QuotedBookingProcessTaskCollection
		{
			public QuotedBookingProcessTaskCollectionForTest(QuotedBooking quotedBooking)
				: base(quotedBooking)
			{
			}

			public new ZQuery AdditionalFilter
			{
				get { return base.AdditionalFilter; }
			}
		}

		#endregion
	}
}
