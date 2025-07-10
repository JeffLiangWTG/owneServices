using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(LinerAndAgencyBookingWebInterfacesHelper))]
	sealed class LinerAndAgencyBookingWebInterfacesHelperTest : NonPersistentBusinessObjectTestCase
	{
		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject()
		{
			return new LinerAndAgencyBookingWebInterfacesHelper(Factory.New<TrackingLinerAndAgencyBooking>());
		}
	}
}
