using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AESRepresentativeProviderTest : Customs.Business.Testing.DataProviderTestCase<AESRepresentativeProvider>
{
	public void TestNew()
	{
		CombineAssertions(() =>
		{
			AssertNull("Null OrgHeader", AESRepresentativeProvider.NewOrNull(null));
			AssertNotNull("OrgHeader is not null", AESRepresentativeProvider.NewOrNull(orgHeader));
		});
	}

	public void TestIdentificationNumbers()
	{
		AssertNotNull(Provider.IdentificationNumbers);
	}

	protected override AESRepresentativeProvider GetProvider() => AESRepresentativeProvider.NewOrNull(orgHeader);

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
