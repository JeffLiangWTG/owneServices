using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class RepresentativeProviderTest : Customs.Business.Testing.DataProviderTestCase<RepresentativeProvider>
{
	public void TestNew()
	{
		CombineAssertions(() =>
		{
			AssertNull("Null OrgHeader", RepresentativeProvider.NewOrNull(null));
			AssertNotNull("OrgHeader is not null", RepresentativeProvider.NewOrNull(orgHeader));
		});
	}

	public void TestIdentificationNumber()
	{
		AssertEquals("Header EORI", "PL333333", Provider.IdentificationNumber);
	}

	public void TestStatus()
	{
		AssertEquals("2", Provider.Status);
	}

	public void TestContactPerson()
	{
		CombineAssertions(() =>
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
	}

	protected override RepresentativeProvider GetProvider() => RepresentativeProvider.NewOrNull(orgHeader);

	protected override void SetUp()
	{
		base.SetUp();
		orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_FullName = "Representative name";
		var cusCodeEori = orgHeader.CustomsCodes.AddNew();
		cusCodeEori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
		cusCodeEori.OK_CustomsRegNo = "333333";
	}

	OrgHeader orgHeader;
}
