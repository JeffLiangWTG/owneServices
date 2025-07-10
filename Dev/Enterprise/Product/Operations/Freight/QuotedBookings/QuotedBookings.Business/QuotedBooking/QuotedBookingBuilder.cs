using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class QuotedBookingBuilder : IQuotedBookingBuilder
	{
		public IQuotedBooking CreateNew(QuoteBookingType quoteBookingType, BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, nameof(factory));
			return QuotedBooking.New(quoteBookingType, factory);
		}

		public IQuotedBooking InitializeFrom(ZGuid bookingPK, BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, nameof(factory));
			return QuotedBooking.New(ZGuid.Empty, bookingPK, factory);
		}

		public IQuotedBooking InitializeFrom(ZGuid quotePK, ZGuid bookingPK, BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, nameof(factory));
			return QuotedBooking.New(quotePK, bookingPK, factory);
		}

		public IQuotedBooking CreateAndPopulateFromOrder(ZGuid orderPK, BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, nameof(factory));
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			var order = factory.Load<Order>(orderPK);

			var populatedBooking = QuotedBookingBuilderHelper.PopulateQuotedBookingFromOrder(order, quotedBooking);
			SetBookingScheduleIfAvailable(populatedBooking, order);

			return populatedBooking;
		}

		public BusinessObject Load(BusinessObjectFactory factory, ZGuid quoteOrBookingPK)
		{
			var viewQuotedBooking = factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.PK, quoteOrBookingPK))
				?? factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_JS, quoteOrBookingPK));

			return viewQuotedBooking?.QuotedBooking;
		}

		public BusinessObject LoadViewQuotedBooking(BusinessObjectFactory factory, ZGuid shipmentPK)
		{
			return factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_JS, shipmentPK));
		}

		#region Implementation

		void SetBookingScheduleIfAvailable(QuotedBooking booking, Order order)
		{
			var sailingSchedule = order.Factory.LoadTop1<JobSailing>(QuotedBookingBuilderHelper.GetSailingScheduleQuery(order));
			if (sailingSchedule != null)
			{
				booking.Booking.JS_JX = sailingSchedule.PK;
			}
		}

		#endregion
	}
}
