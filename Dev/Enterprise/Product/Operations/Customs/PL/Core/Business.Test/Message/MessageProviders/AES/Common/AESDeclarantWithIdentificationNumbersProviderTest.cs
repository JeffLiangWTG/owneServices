using System;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

class AESDeclarantWithIdentificationNumbersProviderTest : Customs.Business.Testing.DataProviderTestCase<AESDeclarantWithIdentificationNumbersProvider>
{
	public void TestNewOrNull()
	{
		CombineAssertions(() =>
		{
			AssertNull("Null OrgAddress", AESDeclarantWithIdentificationNumbersProvider.NewOrNull(null, null));
			AssertNull("Null OrgHeader", AESDeclarantWithIdentificationNumbersProvider.NewOrNull(Factory.New<OrgAddress>(), null));
			AssertExceptionThrown<ArgumentNullException>("Null JobDeclaration", "Value cannot be null.\r\nParameter name: jobDeclaration",
				() => AESDeclarantWithIdentificationNumbersProvider.NewOrNull(orgAddress, null));
			AssertNotNull("All data is valid", AESDeclarantWithIdentificationNumbersProvider.NewOrNull(orgAddress, declaration));
		});
	}

	public void TestIdentificationNumber()
	{
		CombineAssertions(() =>
		{
			AssertNullOrEmpty("IdentificationNumber is null", GetProvider().IdentificationNumber);

			_ = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "443322", Core.Constants.CountryCodes.Poland);
			AssertEquals("IdentificationNumber is not null", "PL443322", GetProvider().IdentificationNumber);
		});
	}

	public void TestIdentificationNumbers()
	{
		AssertNotNull(GetProvider().IdentificationNumbers);
	}

	public void TestName()
	{
		orgAddress.OA_CompanyNameOverride = "ABC";
		orgHeader.OH_FullName = "PQR";

		CombineAssertions(() =>
		{
			AssertEquals("Name from OrgAddress", "ABC", GetProvider().Name);
			orgAddress.OA_CompanyNameOverride = ZString.Empty;
			AssertEquals("Name from Org", "PQR", GetProvider().Name);

			_ = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "443322", Core.Constants.CountryCodes.Poland);
			AssertNull("Identification number not empty", GetProvider().Name);
		});
	}

	public void TestAddress()
	{
		AssertNotNull("Identification number empty", GetProvider().Address);

		orgAddress.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "1234", "PL");
		AssertNull("Identification number not empty", GetProvider().Address);
	}

	public virtual void TestAddressType() => AssertType<AddressProvider>(GetProvider().Address);

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_FullName = "Declarant name";
		declaration.JE_OA_DeclarantAddress = orgHeader.PK;
		orgAddress = orgHeader.Addresses.AddNew();
	}

	protected OrgAddress orgAddress;
	protected OrgHeader orgHeader;
	protected JobDeclaration declaration;

	protected override AESDeclarantWithIdentificationNumbersProvider GetProvider() => AESDeclarantWithIdentificationNumbersProvider.NewOrNull(orgAddress, declaration);
}
