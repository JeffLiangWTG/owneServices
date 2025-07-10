using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AESGNSSProviderTest : DataProviderTestCase<AESGNSSProvider>
{
	public void TestLatitude()
	{
		AssertEquals("15", Provider.Latitude);
	}

	public void TestLongitude()
	{
		AssertEquals("10", Provider.Longitude);
	}

	protected override AESGNSSProvider GetProvider() => new AESGNSSProvider(new ZGeography("10, 15"));
}
