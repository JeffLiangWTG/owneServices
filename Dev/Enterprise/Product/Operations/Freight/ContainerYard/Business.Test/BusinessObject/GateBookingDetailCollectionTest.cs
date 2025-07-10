using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.ContainerYard.Business;
using NUnit.Framework;

namespace Enterprise.Freight.ContainerYard.Testing
{
	[TestedType(typeof(GateBookingDetailCollection))]
	sealed class GateBookingDetailCollectionTest : ActiveBusinessObjectCollectionTestCase<GateBookingDetailCollection>
	{
		#region Implementation

		protected override GateBookingDetailCollection GetCollectionToTest()
		{
			var booking = Factory.NewWithValidTestData<GateBooking>();
			var collection = new GateBookingDetailCollection(booking);

			return collection;
		}

		#endregion
	}
}
