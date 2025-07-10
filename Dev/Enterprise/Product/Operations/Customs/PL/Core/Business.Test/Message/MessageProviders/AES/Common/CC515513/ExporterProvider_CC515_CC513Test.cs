using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class ExporterProvider_CC515_CC513Test : AESExporterProviderTest
{
	public void TestNameInTransitionPeriod()
	{
		orgAddress.OA_CompanyNameOverride = new('A', 70);
		orgHeader.OH_FullName = new('B', 70);

		CombineAssertions(() =>
		{
			AssertEquals("Name from OrgAddress", new string('A', 35), GetProvider().Name);
			orgAddress.OA_CompanyNameOverride = ZString.Empty;
			AssertEquals("Name from Org", new string('B', 35), GetProvider().Name);

			_ = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "443322", Core.Constants.CountryCodes.Poland);
			AssertNull("Identification number not empty", GetProvider().Name);
		});
	}

	public override void TestAddressType() => AssertType<AddressProvider_CC515_CC513>(GetProvider().Address);

	protected override AESExporterProvider GetProvider() =>
		ExporterProvider_CC515_CC513.NewOrNull(orgAddress, isAesTransitionPeriod: true);
}
