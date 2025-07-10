using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(ConsolidatedTransportBookingDocManagerInfo))]
	sealed class ConsolidatedTransportBookingDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<CommonConsolidatedTransportBooking>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			CommonConsolidatedTransportBooking booking = Factory.New<CommonConsolidatedTransportBooking>();
			return booking;
		}
	}
}
