using System;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class GNSSProviderTest : Customs.Business.Testing.DataProviderTestCase<GNSSProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null CusGoodsLocationAddress", "Value cannot be null.\r\nParameter name: address", () => new GNSSProvider(null));
	}

	public void TestLatitude() => AssertEquals("1.19", Provider.Latitude);

	public void TestLongitude() => AssertEquals("3.84", Provider.Longitude);

	protected override GNSSProvider GetProvider() => new GNSSProvider(address);

	protected override void SetUp()
	{
		base.SetUp();
		var cusGoodsLocation = Factory.New<CusGoodsLocation>();
		address = cusGoodsLocation.Address;
		address.E2_Latitude = 1.19m;
		address.E2_Longitude = 3.84m;
	}
	CusGoodsLocationAddress address;
}
