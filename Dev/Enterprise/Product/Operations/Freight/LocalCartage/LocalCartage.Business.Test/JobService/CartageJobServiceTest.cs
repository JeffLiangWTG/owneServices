using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(CartageJobService))]
	public class CartageJobServiceTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetNewLookups()
		{
			var cartageJobService = Factory.New<CartageJobService>();
			AssertEquals(typeof(CartageJobServiceLookups), cartageJobService.Lookups.GetType());
		}
	}
}
