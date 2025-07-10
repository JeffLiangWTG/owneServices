using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

class AESConsigneeConsignorProviderTest : Customs.Business.Testing.DataProviderTestCase<AESConsigneeConsignorProvider>
{
	public void TestNew()
	{
		CombineAssertions(() =>
		{
			AssertNull("Null OrgAddress", AESConsigneeConsignorProvider.NewOrNull(null));
			AssertNull("OrgHeader is null", AESConsigneeConsignorProvider.NewOrNull(Factory.New<OrgAddress>()));
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "HeaderName";
			orgAddress = orgHeader.Addresses.AddNew();
			AssertNotNull("OrgHeader is not null", AESConsigneeConsignorProvider.NewOrNull(orgAddress));
		});
	}

	public void TestIdentificationNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("No EORI", string.Empty, GetProvider().IdentificationNumber);
			orgAddress.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "PL1234", "PL");
			AssertEquals("Header EORI", "PL1234", GetProvider().IdentificationNumber);
			orgAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "321", "PL");
			AssertEquals("Address EORI", "PL321", GetProvider().IdentificationNumber);
		});
	}

	public void TestName()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Header name", "HeaderName", GetProvider().Name);
			orgAddress.OA_CompanyNameOverride = "AddressName";
			AssertEquals("Address name", "AddressName", GetProvider().Name);
			orgAddress.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "1234", "PL");
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

	protected override AESConsigneeConsignorProvider GetProvider() => AESConsigneeConsignorProvider.NewOrNull(orgAddress);

	protected override void SetUp()
	{
		base.SetUp();
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_FullName = "HeaderName";
		orgAddress = orgHeader.Addresses.AddNew();
	}

	protected OrgAddress orgAddress;
}
