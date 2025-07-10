using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.TransportBooking;
using Enterprise.Integration.TransportConsignment;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Schema;
using Common = Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportBookings.Business.Testing
{
	class DtbBookingTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForBinding()
		{
			AssertEquals(typeof(DtbBooking), new DtbBookingTypeDecider().GetTypeForBinding());
		}

		public void TestGetTypeForNew()
		{
			AssertEquals(ObjectFactory.GetType<IDtbBooking>(), new DtbBookingTypeDecider().GetTypeForNew());
		}

		public void TestGetTypeForLoad()
		{
			var bookingConsolidation = Factory.New<DtbBookingConsolidation>();
			var booking = bookingConsolidation.Bookings.AddNew();

			var consignmentConsolidation = (DtbTransportConsolidation)Factory.New<IDtbConsignmentConsolidation>();
			var consignment = consignmentConsolidation.Bookings.AddNew();

			var quoteConsolidation = Factory.New<DtbBookingConsolidation>();
			quoteConsolidation.KB_JobType = TransportConsolidationJobTypes.Codes.QuotedBooking;
			var quote = quoteConsolidation.Bookings.AddNew();

			var hvlvConsolidation = Factory.New<DtbBookingConsolidation>();
			hvlvConsolidation.KB_JobType = TransportConsolidationJobTypes.Codes.HighVolumeLowValue;
			var hvlvBooking = hvlvConsolidation.Bookings.AddNew();
			Factory.Save();

			var bookingWithNoType = (DtbBooking)Factory.New<IDtbBooking>();
			bookingWithNoType[DtbBookingSchema.Constants.KM_JobType] = "";

			AssertEquals(ObjectFactory.GetType<IDtbBooking>(),
				new DtbBookingTypeDecider().GetTypeForLoad(((IBusinessObjectInternals)booking).Row, new BusinessObjectFactory()));

			AssertEquals(ObjectFactory.GetType<IDtbBookingConsignment>(),
				new DtbBookingTypeDecider().GetTypeForLoad(((IBusinessObjectInternals)consignment).Row, new BusinessObjectFactory()));

			AssertEquals(ObjectFactory.GetType<IDtbBooking>(),
				new DtbBookingTypeDecider().GetTypeForLoad(((IBusinessObjectInternals)quote).Row, new BusinessObjectFactory()));

			AssertEquals(ObjectFactory.GetType<IDtbBooking>(),
				new DtbBookingTypeDecider().GetTypeForLoad(((IBusinessObjectInternals)hvlvBooking).Row, new BusinessObjectFactory()));

			AssertEquals(typeof(Common.AutoDtbBooking),
				new DtbBookingTypeDecider().GetTypeForLoad(((IBusinessObjectInternals)bookingWithNoType).Row, new BusinessObjectFactory()));
		}
	}
}
