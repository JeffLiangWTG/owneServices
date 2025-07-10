using CargoWise.EntityFramework;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.QuotedBookings.Module.Test
{
	public class QuotedBookingControllerForTest : QuotedBookingController
	{
		public QuotedBookingControllerForTest()
			: base()
		{
		}

		public QuotedBookingControllerForTest(QuotedBookingState state)
			: base(state)
		{
		}

		internal QuotedBooking GetLoadedBusinessEntityInLocalFactoryExposed(IBusiness bizO)
		{
			return (QuotedBooking)GetLoadedBusinessEntityInLocalFactory(bizO);
		}

		public IBusiness GetNewBusinessEntityInLocalFactoryForTest()
		{
			return base.GetNewBusinessEntityInLocalFactory();
		}

		public IBusiness GetLoadedBusinessEntityInLocalFactoryForTest(IBusiness iBus)
		{
			return base.GetLoadedBusinessEntityInLocalFactory(iBus);
		}

		public IZForm GetFormForTest(IBusiness businessEntity)
		{
			return base.GetForm(businessEntity);
		}
	}
}
