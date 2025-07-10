using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(BookingConsignmentJobServiceDependentCollection))]
	public class BookingConsignmentJobServiceDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new BookingConsignmentJobServiceDependentCollection(Factory.New<DtbBookingConsignment>(), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<BookingConsignmentJobService>();
		}
	}
}
