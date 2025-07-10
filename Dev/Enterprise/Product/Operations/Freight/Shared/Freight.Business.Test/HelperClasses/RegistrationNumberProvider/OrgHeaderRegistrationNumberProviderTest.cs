using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class OrgHeaderRegistrationNumberProviderTest : TestCaseWithFactory
	{
		public void TestGetTaxNumber_BangladeshFallback()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var taxCode = orgHeader.CustomsCodes.AddNew();
			taxCode.OK_CodeType = "BRN";
			taxCode.OK_RN_NKCodeCountry = "BD";
			taxCode.OK_CustomsRegNo = "12345";

			IRegistrationNumberProvider provider = new OrgHeaderRegistrationNumberProvider(orgHeader);

			AssertEquals(string.Empty, provider.GetTaxNumber("BD", "AIN"));
		}

		public void TestGetTaxNumber_Canada()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var taxCode = orgHeader.CustomsCodes.AddNew();
			taxCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			taxCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Canada;
			taxCode.OK_CustomsRegNo = "8030";

			IRegistrationNumberProvider provider = new OrgHeaderRegistrationNumberProvider(orgHeader);

			AssertEquals(ZString.Empty, provider.GetTaxNumber(Core.Constants.CountryCodes.Canada, "AGT"));
		}

		public void TestGetTaxNumbers_OnlyByTypeCode()
		{
			IRegistrationNumberProvider providers = new OrgHeaderRegistrationNumberProvider(null);

			AssertEquals(0, providers.GetTaxNumbers("AIN").Count);

			var orgHeader = Factory.New<OrgHeader>();
			IRegistrationNumberProvider provider = new OrgHeaderRegistrationNumberProvider(orgHeader);

			AssertEquals(0, provider.GetTaxNumbers("").Count);
			AssertEquals(0, provider.GetTaxNumbers("AIN").Count);

			var taxCode1 = orgHeader.CustomsCodes.AddNew();
			taxCode1.OK_CodeType = "BRN";
			taxCode1.OK_RN_NKCodeCountry = "BD";
			taxCode1.OK_CustomsRegNo = "12345";

			var taxCode2 = orgHeader.CustomsCodes.AddNew();
			taxCode2.OK_CodeType = "BRN";
			taxCode2.OK_RN_NKCodeCountry = "SE";
			taxCode2.OK_CustomsRegNo = "54321";

			var pairsOfTaxNumbersAndCountries = provider.GetTaxNumbers("BRN");
			var pairsOfTaxNumberAndCountry = pairsOfTaxNumbersAndCountries.FirstOrDefault(c => c.taxNumber == "12345");
			AssertNotNull(pairsOfTaxNumberAndCountry);
			AssertEquals(pairsOfTaxNumberAndCountry.countryOfIssue, "BD");

			pairsOfTaxNumberAndCountry = pairsOfTaxNumbersAndCountries.FirstOrDefault(c => c.taxNumber == "54321");
			AssertNotNull(pairsOfTaxNumberAndCountry);
			AssertEquals(pairsOfTaxNumberAndCountry.countryOfIssue, "SE");
		}
	}
}
