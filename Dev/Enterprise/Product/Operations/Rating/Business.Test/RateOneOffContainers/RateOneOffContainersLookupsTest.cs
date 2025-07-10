using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business.Testing
{
	internal class RateOneOffContainersLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRefPackTypes_ShouldNotHaveContainer()
		{
			var rateOneOffContainer = Factory.NewWithValidTestData<RateOneOffContainers>();
			AssertEquals(false, rateOneOffContainer.Lookups.RefPackTypes.ContainsCode(RefPackTypeCollection.ReservedContainerType));
		}
	}
}
