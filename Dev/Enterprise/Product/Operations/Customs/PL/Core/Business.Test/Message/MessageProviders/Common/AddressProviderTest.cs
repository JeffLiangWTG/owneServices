using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

class AddressProviderTest : Customs.Business.Testing.DataProviderTestCase<AddressProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null OrgAddress", "Value cannot be null.\r\nParameter name: docAddress",
			() => new AddressProvider(null));
	}

	public void TestPostCode()
	{
		docAddress.E2_AddressOverride = true;
		docAddress.E2_Postcode = "ABC";
		orgAddress.OA_PostCode = "PQR";

		CombineAssertions(() =>
		{
			AssertEquals("E2_AddressOverride", "ABC", GetProvider().PostCode);

			docAddress.E2_AddressOverride = false;
			docAddress.E2_OA_Address = ZGuid.Empty;
			AssertNull("Not E2_AddressOverride, E2_OA_Address empty", GetProvider().PostCode);
			docAddress.E2_OA_Address = orgAddress.PK;
			AssertEquals("Not E2_AddressOverride", "PQR", GetProvider().PostCode);
		});
	}

	public void TestCountryCode()
	{
		docAddress.E2_AddressOverride = true;
		docAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Poland;
		orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

		CombineAssertions(() =>
		{
			AssertEquals("E2_AddressOverride", "PL", GetProvider().CountryCode);

			docAddress.E2_AddressOverride = false;
			docAddress.E2_OA_Address = ZGuid.Empty;
			AssertNull("Not E2_AddressOverride, E2_OA_Address empty", GetProvider().CountryCode);
			docAddress.E2_OA_Address = orgAddress.PK;
			AssertEquals("Not E2_AddressOverride", "AU", GetProvider().CountryCode);
		});
	}

	public void TestCity()
	{
		docAddress.E2_AddressOverride = true;
		docAddress.E2_City = "ABC";
		orgAddress.OA_City = "PQR";

		CombineAssertions(() =>
		{
			AssertEquals("E2_AddressOverride", "ABC", GetProvider().City);

			docAddress.E2_AddressOverride = false;
			docAddress.E2_OA_Address = ZGuid.Empty;
			AssertNull("Not E2_AddressOverride, E2_OA_Address empty", GetProvider().City);
			docAddress.E2_OA_Address = orgAddress.PK;
			AssertEquals("Not E2_AddressOverride", "PQR", GetProvider().City);
		});
	}

	public void TestStreetAndNumber()
	{
		docAddress.E2_AddressOverride = true;
		docAddress.E2_Address1 = "E2_Address1";
		docAddress.E2_Address2 = "E2_Address2";
		orgAddress.OA_Address1 = "OA_Address1";
		orgAddress.OA_Address2 = "OA_Address2";

		CombineAssertions(() =>
		{
			AssertEquals("E2_AddressOverride", "E2_Address1 E2_Address2", GetProvider().StreetAndNumber);

			docAddress.E2_AddressOverride = false;
			docAddress.E2_OA_Address = ZGuid.Empty;
			AssertNull("Not E2_AddressOverride, E2_OA_Address empty", GetProvider().StreetAndNumber);
			docAddress.E2_OA_Address = orgAddress.PK;
			AssertEquals("Not E2_AddressOverride", "OA_Address1 OA_Address2", GetProvider().StreetAndNumber);
		});
	}

	protected override AddressProvider GetProvider() => new AddressProvider(docAddress);

	protected override void SetUp()
	{
		base.SetUp();
		orgHeader = Factory.New<OrgHeader>();
		orgAddress = orgHeader.Addresses.AddNew();
		docAddress = Factory.NewWithValidTestData<JobDocAddress>();
		docAddress.E2_OA_Address = orgAddress.PK;
	}

	OrgHeader orgHeader;
	protected OrgAddress orgAddress;
	protected JobDocAddress docAddress;
}
