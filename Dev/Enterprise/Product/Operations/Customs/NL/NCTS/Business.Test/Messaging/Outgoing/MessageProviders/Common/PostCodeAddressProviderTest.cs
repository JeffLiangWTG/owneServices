using System;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(PostCodeAddressProvider))]
sealed class PostCodeAddressProviderTest : Customs.Business.Testing.DataProviderTestCase<PostCodeAddressProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Passed null", () => new PostCodeAddressProvider(null));
	}

	public void TestCountry()
	{
		AssertEquals("Country", Core.Constants.CountryCodes.Netherlands, Provider.Country);
	}

	public void TestPostcode()
	{
		AssertEquals("PostCode", "1234AB", Provider.Postcode);
	}

	public void TestHouseNumber()
	{
		AssertEquals("HouseNumber", "5", Provider.HouseNumber);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var location = Factory.New<CusGoodsLocation>();
		location.AdditionalIdentifier = "5";
		var address = location.Address;
		address.E2_Postcode = "1234AB";
		address.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;
		provider = new PostCodeAddressProvider(location);
	}
	PostCodeAddressProvider provider;

	protected override PostCodeAddressProvider GetProvider() => provider;
}
