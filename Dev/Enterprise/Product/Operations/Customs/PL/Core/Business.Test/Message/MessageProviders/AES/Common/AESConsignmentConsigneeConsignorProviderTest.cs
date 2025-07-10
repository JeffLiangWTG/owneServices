using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

class AESConsignmentConsigneeConsignorProviderTest : DataProviderTestCase<AESConsignmentConsigneeConsignorProvider>
{
	public void TestNew()
	{
		CombineAssertions(() =>
		{
			AssertNull("JobDocAddressis null", AESConsignmentConsigneeConsignorProvider.NewOrNull(null));
			AssertNull("OrgAddress is null", AESConsignmentConsigneeConsignorProvider.NewOrNull(Factory.New<JobDocAddress>()));
			AssertNotNull("JobDocAddress and OrgAddress are not null", AESConsignmentConsigneeConsignorProvider.NewOrNull(docAddress));
		});
	}

	public void TestIdentificationNumber()
	{
		var cusCode = orgAddress.CustomsCodes.AddNew();

		CombineAssertions(() =>
		{
			AssertNullOrEmpty("IdentificationNumber is null", GetProvider().IdentificationNumber);

			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Poland;
			cusCode.OK_CustomsRegNo = "123";
			AssertEquals("IdentificationNumber is not null", "PL123", GetProvider().IdentificationNumber);
		});
	}

	public void TestName()
	{
		docAddress.E2_AddressOverride = true;
		docAddress.E2_CompanyName = "ABC";
		orgHeader.OH_FullName = "PQR";

		CombineAssertions(() =>
		{
			AssertEquals("Name from DocAddress Company", "ABC", GetProvider().Name);
			docAddress.E2_AddressOverride = false;
			AssertEquals("Name from Org", "PQR", GetProvider().Name);

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

	protected override AESConsignmentConsigneeConsignorProvider GetProvider() => AESConsignmentConsigneeConsignorProvider.NewOrNull(docAddress);

	protected override void SetUp()
	{
		base.SetUp();
		orgHeader = Factory.New<OrgHeader>();
		orgAddress = orgHeader.Addresses.AddNew();
		docAddress = Factory.NewWithValidTestData<JobDocAddress>();
		docAddress.E2_OA_Address = orgAddress.PK;
	}

	protected OrgHeader orgHeader;
	protected OrgAddress orgAddress;
	protected JobDocAddress docAddress;
}
