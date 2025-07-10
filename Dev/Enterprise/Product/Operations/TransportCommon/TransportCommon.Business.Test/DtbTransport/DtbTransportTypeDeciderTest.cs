using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.TransportBooking;
using Enterprise.Integration.TransportConsignment;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.Business.Testing
{
	class DtbTransportTypeDeciderTest : TestCaseWithFactory
	{
		#region TestGetTypeForBinding

		public void TestGetTypeForBinding()
		{
			AssertEquals(typeof(DtbTransport), new DtbTransportTypeDecider().GetTypeForBinding());
		}

		#endregion

		#region TestGetTypeForNew

		public void TestGetTypeForNew()
		{
			AssertEquals(ObjectFactory.GetType<IDtbBooking>(), new DtbTransportTypeDecider().GetTypeForNew());
		}

		#endregion

		#region TestGetTypeForLoad

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
				new DtbTransportTypeDecider().GetTypeForLoad(((IBusinessObjectInternals)booking).Row, new BusinessObjectFactory()));

			AssertEquals(ObjectFactory.GetType<IDtbBookingConsignment>(),
				new DtbTransportTypeDecider().GetTypeForLoad(((IBusinessObjectInternals)consignment).Row, new BusinessObjectFactory()));

			AssertEquals(ObjectFactory.GetType<IDtbBooking>(),
				new DtbTransportTypeDecider().GetTypeForLoad(((IBusinessObjectInternals)quote).Row, new BusinessObjectFactory()));

			AssertEquals(ObjectFactory.GetType<IDtbBooking>(),
				new DtbTransportTypeDecider().GetTypeForLoad(((IBusinessObjectInternals)hvlvBooking).Row, new BusinessObjectFactory()));

			AssertEquals(typeof(DtbTransport),
				new DtbTransportTypeDecider().GetTypeForLoad(((IBusinessObjectInternals)bookingWithNoType).Row, new BusinessObjectFactory()));
		}

		#endregion
	}
}
