using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

class AESExporterProviderTest : Customs.Business.Testing.DataProviderTestCase<AESExporterProvider>
{
	public void TestNew()
	{
		CombineAssertions(() =>
		{
			AssertNull("Null OrgHeader", AESExporterProvider.NewOrNull(null));
			AssertNull("OrgHeader is not null", AESExporterProvider.NewOrNull(Factory.New<OrgAddress>()));
			AssertNotNull("Valid data", AESExporterProvider.NewOrNull(orgAddress));
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

	protected override AESExporterProvider GetProvider() => AESExporterProvider.NewOrNull(orgAddress);

	protected override void SetUp()
	{
		base.SetUp();
		orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_FullName = "Exporter name";
		orgAddress = orgHeader.Addresses.AddNew();
	}

	protected OrgAddress orgAddress;
	protected OrgHeader orgHeader;
}
