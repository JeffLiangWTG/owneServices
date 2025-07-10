using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.TransportBooking;
using Enterprise.Integration.TransportConsignment;
using Enterprise.TransportCommon.Shared;
using Common = Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportBookings.Business.Testing
{
	class DtbBookingConsolidationTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForBinding()
		{
			AssertEquals(typeof(DtbBookingConsolidation), new DtbBookingConsolidationTypeDecider().GetTypeForBinding());
		}

		public void TestGetTypeForNew()
		{
			AssertEquals(ObjectFactory.GetType<IDtbBookingConsolidation>(), new DtbBookingConsolidationTypeDecider().GetTypeForNew());
		}

		public void TestGetTypeForLoad()
		{
			var booking = Factory.New<IDtbBookingConsolidation>();
			var consignment = Factory.New<IDtbConsignmentConsolidation>();

			var bookingTransportConsolidation = Factory.New<IDtbBookingConsolidation>();
			bookingTransportConsolidation.KB_JobType = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;

			var quoteConsolidation = Factory.New<IDtbBookingConsolidation>();
			quoteConsolidation.KB_JobType = TransportConsolidationJobTypes.Codes.QuotedBooking;

			var hvlvConsolidation = Factory.New<IDtbBookingConsolidation>();
			hvlvConsolidation.KB_JobType = TransportConsolidationJobTypes.Codes.HighVolumeLowValue;
			Factory.Save();

			var noJobTypeConsolidation = Factory.New<IDtbBookingConsolidation>();
			noJobTypeConsolidation.KB_JobType = "";

			AssertEquals(ObjectFactory.GetType<IDtbBookingConsolidation>(),
				new DtbBookingConsolidationTypeDecider().GetTypeForLoad(((IBusinessObjectInternals)booking).Row, new BusinessObjectFactory()));

			AssertEquals(ObjectFactory.GetType<IDtbBookingConsolidation>(),
				new DtbBookingConsolidationTypeDecider().GetTypeForLoad(((IBusinessObjectInternals)bookingTransportConsolidation).Row, new BusinessObjectFactory()));

			AssertEquals(ObjectFactory.GetType<IDtbBookingConsolidation>(),
				new DtbBookingConsolidationTypeDecider().GetTypeForLoad(((IBusinessObjectInternals)quoteConsolidation).Row, new BusinessObjectFactory()));

			AssertEquals(ObjectFactory.GetType<IDtbConsignmentConsolidation>(),
				new DtbBookingConsolidationTypeDecider().GetTypeForLoad(((IBusinessObjectInternals)consignment).Row, new BusinessObjectFactory()));

			AssertEquals(ObjectFactory.GetType<IDtbBookingConsolidation>(),
				new DtbBookingConsolidationTypeDecider().GetTypeForLoad(((IBusinessObjectInternals)hvlvConsolidation).Row, new BusinessObjectFactory()));

			AssertEquals(typeof(Common.AutoDtbBookingConsolidation),
				new DtbBookingConsolidationTypeDecider().GetTypeForLoad(((IBusinessObjectInternals)noJobTypeConsolidation).Row, Factory));
		}
	}
}
