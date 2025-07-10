using CargoWise.EntityFramework.Testing;

namespace Enterprise.TransportConsignment.Business.Testing
{
	class DtbConsignmentVariationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestStatuses()
		{
			var lookups = new DtbConsignmentVariationLookups(Factory.New<DtbConsignmentVariation>());
			var statuses = lookups.Statuses.GetAllCodes();

			AssertContainsExactElementsInAnyOrder(new[] { "OPN", "CLS" }, statuses);
		}

		public void TestTypes()
		{
			var lookups = new DtbConsignmentVariationLookups(Factory.New<DtbConsignmentVariation>());
			var types = lookups.Types.GetAllCodes();

			AssertContainsExactElementsInAnyOrder(new[] { "QTY" }, types);
		}
	}
}
