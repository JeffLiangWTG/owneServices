using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.TransportBooking;
using Enterprise.Integration.TransportConsignment;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportCommon.Business.Testing
{
	class DtbTransportConsolidationTypeDeciderTest : TestCaseWithFactory
	{
		#region TestGetTypeForBinding

		public void TestGetTypeForBinding()
		{
			AssertEquals(typeof(DtbTransportConsolidation), new DtbTransportConsolidationTypeDecider().GetTypeForBinding());
		}

		#endregion

		#region TestGetTypeForNew

		public void TestGetTypeForNew()
		{
			AssertExceptionThrown(typeof(NotSupportedException), "Abstract type.", () => new DtbTransportConsolidationTypeDecider().GetTypeForNew());
		}

		#endregion

		#region TestGetTypeForLoad

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
				new DtbTransportConsolidationTypeDecider().GetTypeForLoad(((IBusinessObjectInternals)booking).Row, new BusinessObjectFactory()));

			AssertEquals(ObjectFactory.GetType<IDtbBookingConsolidation>(),
				new DtbTransportConsolidationTypeDecider().GetTypeForLoad(((IBusinessObjectInternals)bookingTransportConsolidation).Row, new BusinessObjectFactory()));

			AssertEquals(ObjectFactory.GetType<IDtbBookingConsolidation>(),
				new DtbTransportConsolidationTypeDecider().GetTypeForLoad(((IBusinessObjectInternals)quoteConsolidation).Row, new BusinessObjectFactory()));

			AssertEquals(ObjectFactory.GetType<IDtbConsignmentConsolidation>(),
				new DtbTransportConsolidationTypeDecider().GetTypeForLoad(((IBusinessObjectInternals)consignment).Row, new BusinessObjectFactory()));

			AssertEquals(ObjectFactory.GetType<IDtbBookingConsolidation>(),
				new DtbTransportConsolidationTypeDecider().GetTypeForLoad(((IBusinessObjectInternals)hvlvConsolidation).Row, new BusinessObjectFactory()));

			AssertEquals(typeof(DtbTransportConsolidation),
				new DtbTransportConsolidationTypeDecider().GetTypeForLoad(((IBusinessObjectInternals)noJobTypeConsolidation).Row, Factory));
		}

		#endregion
	}
}
