using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.Testing;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(QuotedBookingDocManagerInfo))]
	public class QuotedBookingDocManagerInfoTest : ForwardingShipmentDocManagerInfoTest
	{
		public override void TestAllRelatedObjectsRetrieved()
		{
			var quotedBooking = (QuotedBooking)GetPopulatedParentBusinessObject();
			Assert("Should have the transportBooking in the related business objects", ((IList)((IDocManagerSupport)quotedBooking).DocManagerInfo.RelatedObjects).Contains(transportBooking));
		}

		public void TestDocManagerCode()
		{
			var quotedBooking = (QuotedBooking)GetEmptyParentBusinessObject();
			AssertEquals("Doc Manager Code should be SHP", "SHP", ((IDocManagerSupport)quotedBooking).DocManagerInfo.DocManagerCode);
		}

		public override BusinessObject GetEmptyParentBusinessObject()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			return QuotedBooking.New(quote.PK, booking.PK, Factory);
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			var quotedBooking = GetEmptyParentBusinessObject();
			var consolidation = Factory.New(ObjectFactory.GetType<IDtbBookingConsolidation>());
			consolidation[DtbBookingConsolidationSchema.KB_ParentTableCode] = quotedBooking.TablePrefix;
			consolidation[DtbBookingConsolidationSchema.KB_ParentID] = quotedBooking.PK;
			transportBooking = (IDtbBooking)Factory.New(ObjectFactory.GetType<IDtbBooking>());
			transportBooking.KM_KB_Booking = consolidation.PK;
			return quotedBooking;
		}

		IDtbBooking transportBooking;
	}
}
