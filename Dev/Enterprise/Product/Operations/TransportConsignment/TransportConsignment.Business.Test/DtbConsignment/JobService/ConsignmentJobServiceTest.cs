using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(ConsignmentJobService))]
	sealed class ConsignmentJobServiceTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetNewLookups()
		{
			var consignmentJobService = Factory.New<ConsignmentJobService>();
			AssertEquals(typeof(ConsignmentJobServiceLookups), consignmentJobService.Lookups.GetType());
		}
	}
}
