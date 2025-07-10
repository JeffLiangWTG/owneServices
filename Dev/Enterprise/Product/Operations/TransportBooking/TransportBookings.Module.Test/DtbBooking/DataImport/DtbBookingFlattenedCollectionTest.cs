using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(DtbBookingFlattenedCollection))]
	internal class DtbBookingFlattenedCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DtbBookingFlattenedCollection>
	{
		protected override DtbBookingFlattenedCollection GetCollectionToTest()
		{
			return new DtbBookingFlattenedCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DtbBookingFlattened();
		}
	}
}
