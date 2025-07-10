using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	class CountryIgnoreRegistrationNumberProviderTest : TestCaseWithFactory
	{
		public void TestGetTaxNumberWithoutCountryValidation()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var taxCode = orgHeader.CustomsCodes.AddNew();
			taxCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			taxCode.OK_RN_NKCodeCountry = CountryCodes.Germany;
			taxCode.OK_CustomsRegNo = "12345";

			var provider = new CountryIgnoreRegistrationNumberProvider(new OrgHeaderRegistrationNumberProvider(orgHeader));

			AssertEquals("12345", provider.GetTaxNumber(CountryCodes.Switzerland, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori));

			var taxCode2 = orgHeader.CustomsCodes.AddNew();
			taxCode2.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			taxCode2.OK_RN_NKCodeCountry = CountryCodes.Switzerland;
			taxCode2.OK_CustomsRegNo = "54321";

			AssertEquals("54321", provider.GetTaxNumber(CountryCodes.Switzerland, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori));
		}

		public void TestFindCountryByTaxNumber()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var taxCode = orgHeader.CustomsCodes.AddNew();
			taxCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			taxCode.OK_RN_NKCodeCountry = CountryCodes.Switzerland;
			taxCode.OK_CustomsRegNo = "12345";

			var provider = new CountryIgnoreRegistrationNumberProvider(new OrgHeaderRegistrationNumberProvider(orgHeader));

			AssertEquals(CountryCodes.Switzerland, provider.FindCountryByTaxNumber("12345", OrgCusCode.EuropeanUnionSharedCodeTypes.Eori));
		}
	}
}
