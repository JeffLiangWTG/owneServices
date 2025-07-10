using System;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class LocationOfGoodsAddressProviderTest : Customs.Business.Testing.DataProviderTestCase<LocationOfGoodsAddressProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null CusGoodsLocationAddress", "Value cannot be null.\r\nParameter name: address", () => new LocationOfGoodsAddressProvider(null));
	}

	public void TestStreetAndNumber() => AssertEquals("address 1", Provider.StreetAndNumber);

	public void TestPostcode() => AssertEquals("PostCode", Provider.Postcode);

	public void TestCity() => AssertEquals("City", Provider.City);

	public void TestCountry() => AssertEquals(CountryCodes.Poland, Provider.Country);

	public void TestStreetAndNumberMaxLength() => AssertEquals(70, Provider.StreetAndNumberMaxLength);

	protected override LocationOfGoodsAddressProvider GetProvider() => new LocationOfGoodsAddressProvider(address);

	protected override void SetUp()
	{
		base.SetUp();
		var cusGoodsLocation = Factory.New<CusGoodsLocation>();
		address = cusGoodsLocation.Address;
		address.E2_City = "City";
		address.E2_Postcode = "PostCode";
		address.E2_RN_NKCountryCode = CountryCodes.Poland;
		address.E2_Address1 = "address 1";
	}
	CusGoodsLocationAddress address;
}
