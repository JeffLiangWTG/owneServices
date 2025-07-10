using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

class EuEoriResolverTest : TestCaseWithFactory
{
	public void TestIdentificationNumberByAddress()
	{
		var orgAddress = Factory.New<OrgHeader>().Addresses.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("null address", string.Empty, EuEoriResolver.GetRegNoWithCountryCode((OrgAddress)null));

			AssertEquals("No EORI", string.Empty, EuEoriResolver.GetRegNoWithCountryCode(orgAddress));

			orgAddress.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "111", "GB");
			AssertEquals("Ignores non-EU EORI", string.Empty, EuEoriResolver.GetRegNoWithCountryCode(orgAddress));

			orgAddress.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "222", "PL");
			AssertEquals("Returns header EORI", "PL222", EuEoriResolver.GetRegNoWithCountryCode(orgAddress));

			orgAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "333", "AU");
			AssertEquals("Ignors non-EU premises EORI", "PL222", EuEoriResolver.GetRegNoWithCountryCode(orgAddress));

			var code = orgAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "444", "DE");
			AssertEquals("Returns premises EORI", "DE444", EuEoriResolver.GetRegNoWithCountryCode(orgAddress));

			code.OK_CustomsRegNo = "DE555";
			AssertEquals("Handles country code in EORI", "DE555", EuEoriResolver.GetRegNoWithCountryCode(orgAddress));
		});
	}

	public void TestGetRegNoWithCountryCodeByHeader()
	{
		var orgHeader = Factory.New<OrgHeader>();

		CombineAssertions(() =>
		{
			AssertEquals("No EORI", string.Empty, EuEoriResolver.GetRegNoWithCountryCode(orgHeader));

			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "111", "AU");
			AssertEquals("Ignores non-EU EORI", string.Empty, EuEoriResolver.GetRegNoWithCountryCode(orgHeader));

			var code = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "222", "PL");
			AssertEquals("Returns EORI", "PL222", EuEoriResolver.GetRegNoWithCountryCode(orgHeader));

			code.OK_CustomsRegNo = "PL333";
			AssertEquals("Handles country code in EORI", "PL333", EuEoriResolver.GetRegNoWithCountryCode(orgHeader));
		});
	}
}
