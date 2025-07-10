using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(BookingConsignmentJobService))]
	public class BookingConsignmentJobServiceTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetNewLookups()
		{
			var consignmentJobService = Factory.New<BookingConsignmentJobService>();
			AssertEquals(typeof(BookingConsignmentJobServiceLookups), consignmentJobService.Lookups.GetType());
		}
	}
}
