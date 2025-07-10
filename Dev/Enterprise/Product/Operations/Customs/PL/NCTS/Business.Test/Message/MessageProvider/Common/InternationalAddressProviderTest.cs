using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class InternationalAddressProviderTest : Customs.Business.Testing.DataProviderTestCase<InternationalAddressProvider>
{
	public void TestNewOrNull()
	{
		CombineAssertions(() =>
		{
			AssertNull("null address", InternationalAddressProvider.NewOrNull(null));

			AssertNotNull("all ok", InternationalAddressProvider.NewOrNull(address));
		});
	}

	public void TestStreetAndNumber() => AssertEquals("Address1 Address2", Provider.StreetAndNumber);

	public void TestStreetAndNumberMaxLength()
	{
		const int inNCTSTPPeriod = 35;
		const int outNCTSTPPeriod = 70;

		CombineAssertions(() =>
		{
			AssertEquals("StreetAndNumber max length in transition period", inNCTSTPPeriod, InternationalAddressProvider.NewOrNull(address, isInPhase5TransitionPeriod: true).StreetAndNumberMaxLength);
			AssertEquals("When max length dependencies are disabled max length should be outNCTSTPPeriod", outNCTSTPPeriod, InternationalAddressProvider.NewOrNull(address, useMaxLengthWithDependency: false, isInPhase5TransitionPeriod: true).StreetAndNumberMaxLength);
			AssertEquals("StreetAndNumber max length outside transition period", outNCTSTPPeriod, GetProvider().StreetAndNumberMaxLength);
		});
	}

	public void TestPostcode() => AssertEquals("12345", Provider.Postcode);

	public void TestCity() => AssertEquals("CityName", Provider.City);

	public void TestCountry() => AssertEquals(CountryCodes.Poland, Provider.Country);

	protected override InternationalAddressProvider GetProvider() => InternationalAddressProvider.NewOrNull(address);

	protected override void SetUp()
	{
		base.SetUp();

		address = Factory.New<OrgAddress>();
		address.OA_Address1 = "Address1";
		address.OA_Address2 = "Address2";
		address.OA_PostCode = "12345";
		address.OA_City = "CityName";
		address.OA_RN_NKCountryCode = CountryCodes.Poland;
	}
	OrgAddress address;
}
