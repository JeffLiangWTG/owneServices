using System;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class PostcodeAddressProviderTest : Customs.Business.Testing.DataProviderTestCase<PostcodeAddressProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null CusGoodsLocationAddress", "Value cannot be null.\r\nParameter name: address", () => new PostcodeAddressProvider(null, null));
	}

	public void TestHouseNumber() => AssertEquals("ABC", Provider.HouseNumber);

	public void TestPostcode() => AssertEquals("PostCode", Provider.Postcode);

	public void TestCountry() => AssertEquals(CountryCodes.Poland, Provider.Country);

	protected override PostcodeAddressProvider GetProvider() => new PostcodeAddressProvider(address, "ABC");

	protected override void SetUp()
	{
		base.SetUp();
		var cusGoodsLocation = Factory.New<CusGoodsLocation>();
		address = cusGoodsLocation.Address;
		address.E2_Postcode = "PostCode";
		address.E2_RN_NKCountryCode = CountryCodes.Poland;
	}
	CusGoodsLocationAddress address;
}
