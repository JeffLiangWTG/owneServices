using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyBookingCollection))]
	internal class BookingCollectionTest : ActiveBusinessObjectCollectionTestCase<AgencyBookingCollection>
	{
	}
}
