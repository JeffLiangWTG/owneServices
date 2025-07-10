using System;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(AddressProvider))]
sealed class AddressProviderTest : Customs.Business.Testing.DataProviderTestCase<AddressProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Passed null using OrgAddress-constructor", () => new AddressProvider((OrgAddress)null));
		AssertExceptionThrown<ArgumentNullException>("Passed null using JobDocAddress-constructor", () => new AddressProvider((JobDocAddress)null));
	}

	public void TestCountry()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Country using OrgAddress", Core.Constants.CountryCodes.Netherlands, provider.Country);
			Setup_JobDocAddress();
			AssertEquals("Country using JobDocAddress", Core.Constants.CountryCodes.Belgium, provider.Country);
		});
	}

	public void TestPostCode()
	{
		CombineAssertions(() =>
		{
			AssertEquals("PostCode using OrgAddress", "1234AB", provider.PostCode);
			Setup_JobDocAddress();
			AssertEquals("PostCode using JobDocAddress", "2600", provider.PostCode);
		});
	}

	public void TestCity()
	{
		CombineAssertions(() =>
		{
			AssertEquals("City using OrgAddress", "Rotterdam", provider.City);
			Setup_JobDocAddress();
			AssertEquals("City using JobDocAddress", "Antwerpen", provider.City);
		});
	}

	public void TestStreetAndNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("StreetAndNumber using OrgAddress", "Wilhelmuskaai 5 Noord", provider.StreetAndNumber);
			Setup_JobDocAddress();
			AssertEquals("StreetAndNumber using JobDocAddress", "Wapenstilstandlaan 47 1st floor", provider.StreetAndNumber);
		});
	}

	void Setup_JobDocAddress()
	{
		base.SetUp();
		var address = Factory.New<JobDocAddress>();
		address.E2_Address1 = "Wapenstilstandlaan 47";
		address.E2_Address2 = " 1st floor";
		address.E2_Postcode = "2600";
		address.E2_City = "Antwerpen";
		address.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
		provider = new AddressProvider(address);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var address = Factory.New<OrgAddress>();
		address.OA_Address1 = "Wilhelmuskaai 5";
		address.OA_Address2 = " Noord";
		address.OA_PostCode = "1234AB";
		address.OA_City = "Rotterdam";
		address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;
		provider = new AddressProvider(address);
	}
	AddressProvider provider;

	protected override AddressProvider GetProvider() => provider;
}
