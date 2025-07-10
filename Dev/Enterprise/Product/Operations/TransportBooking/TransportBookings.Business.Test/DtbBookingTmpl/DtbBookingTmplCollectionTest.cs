using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(DtbBookingTmplCollection))]
	class DtbBookingTmplCollectionTest : ActiveBusinessObjectCollectionTestCase<DtbBookingTmplCollection>
	{
		protected override DtbBookingTmplCollection GetCollectionToTest()
		{
			return new DtbBookingTmplCollection(Factory);
		}
	}
}
