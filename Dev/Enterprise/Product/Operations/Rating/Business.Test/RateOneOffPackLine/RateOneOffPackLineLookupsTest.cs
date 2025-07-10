using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business.Test
{
	public class RateOneOffPackLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRefPackTypes_ShouldNotHaveContainer()
		{
			var looseCargo = Factory.NewWithValidTestData<RateOneOffPackLine>();
			AssertEquals(false, looseCargo.Lookups.RefPackTypes.ContainsCode(RefPackTypeCollection.ReservedContainerType));
		}
	}
}
