using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

sealed class CC507CExitCarrierProviderTest : DataProviderTestCase<CC507CExitCarrierProvider>
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertNull("Null orgAddress", CC507CExitCarrierProvider.NewOrNull(null));
		AssertNotNull("Not Null orgAddress", CC507CExitCarrierProvider.NewOrNull(orgAddress));
	});

	public void TestIdentificationNumber() => CombineAssertions(() =>
	{
		AssertEquals("Eori does not exist", string.Empty, GetProvider().IdentificationNumber);

		orgAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "eoriNumber");
		AssertEquals("Eori exists", "PLeoriNumber", GetProvider().IdentificationNumber);
	});

	public void TestName() => CombineAssertions(() =>
	{
		orgHeader.OH_FullName = "ABC123";
		AssertEquals("OA_CompanyNameOverride is empty", "ABC123", GetProvider().Name);

		orgAddress.OA_CompanyNameOverride = "POI987";
		AssertEquals("OA_CompanyNameOverride is not empty", "POI987", GetProvider().Name);
	});

	public void TestIdentificationDataPL() => CombineAssertions(() =>
	{
		AssertNotNull("Eori does not exist", GetProvider().IdentificationDataPL);

		orgAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "eoriNumber");
		AssertNull("Eori exists", GetProvider().IdentificationDataPL);
	});

	public void TestAddress() => CombineAssertions(() =>
	{
		AssertNotNull("Eori does not exist", GetProvider().Address);

		orgAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "eoriNumber");
		AssertNull("Eori exists", GetProvider().Address);
	});

	public void TestContactPerson() => CombineAssertions(() =>
	{
		GlbStaff.CurrentUser.GS_PublishWorkPhone = true;
		GlbStaff.CurrentUser.GS_WorkPhone = "12345";
		AssertNotNull(Provider.ContactPerson);

		var tempName = GlbStaff.CurrentUser.GS_FullName;
		GlbStaff.CurrentUser.GS_FullName = string.Empty;
		AssertNull("Null if Staff Name is empty", GetProvider().ContactPerson);

		GlbStaff.CurrentUser.GS_FullName = tempName;
		GlbStaff.CurrentUser.GS_WorkPhone = string.Empty;
		AssertNull("Null if Staff Phone is empty", GetProvider().ContactPerson);
	});

	protected override CC507CExitCarrierProvider GetProvider() => CC507CExitCarrierProvider.NewOrNull(orgAddress);

	protected override void SetUp()
	{
		base.SetUp();
		orgHeader = Factory.New<OrgHeader>();
		orgAddress = orgHeader.Addresses.AddNew();
	}

	OrgHeader orgHeader;
	OrgAddress orgAddress;
}
