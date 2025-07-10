using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Business.Testing
{
	sealed class JobPackLineHarmonisedCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateJLH_RN_NKCountry()
		{
			var packline = Factory.New<PackLine>();
			var hc = packline.HarmonisedCodes.AddNew();
			hc.JLH_RN_NKCountry = "XX";
			AssertHasError(hc.JLH_RN_NKCountryInfo, "Enter a valid selection.");

			hc.JLH_RN_NKCountry = "";
			AssertHasError(hc.JLH_RN_NKCountryInfo, "Please enter a value.");

			hc.JLH_RN_NKCountry = "FR";
			AssertNoErrors(hc.JLH_RN_NKCountryInfo);
		}

		public void TestValidateJLH_Code()
		{
			var packline = Factory.New<PackLine>();
			var hc = packline.HarmonisedCodes.AddNew();
			hc.JLH_Code = "XX";
			AssertHasError(hc.JLH_CodeInfo, "Invalid Harmonized Code. Only numeric characters (minimum 4) and dots are allowed. Code is mandatory when Country/Region is entered.");

			hc.JLH_Code = "";
			AssertHasError(hc.JLH_CodeInfo, "Please enter a value.");

			hc.JLH_Code = "123.45";
			AssertNoErrors(hc.JLH_CodeInfo);

			hc.JLH_Code = "1.4";
			AssertHasError(hc.JLH_CodeInfo, "Invalid Harmonized Code. Only numeric characters (minimum 4) and dots are allowed. Code is mandatory when Country/Region is entered.");
		}

		public void TestDuplicateValidation()
		{
			const string expectedError = "The Country/Region and Code must be unique.";

			var packline1 = Factory.New<PackLine>();
			var packline2 = Factory.New<PackLine>();

			var hc1 = packline1.HarmonisedCodes.AddNew();
			hc1.JLH_RN_NKCountry = "AU";
			hc1.JLH_Code = "1234";

			var hc2 = packline1.HarmonisedCodes.AddNew();
			hc2.JLH_RN_NKCountry = "AU";
			hc2.JLH_Code = "1234";

			AssertHasError(hc2.JLH_RN_NKCountryInfo, expectedError);
			AssertHasError(hc2.JLH_CodeInfo, expectedError);

			hc2.JLH_RN_NKCountry = "DE";
			AssertNoError(hc2.JLH_RN_NKCountryInfo, expectedError);
			AssertNoError(hc2.JLH_CodeInfo, expectedError);

			var hc3 = packline2.HarmonisedCodes.AddNew();
			hc3.JLH_RN_NKCountry = "AU";
			hc3.JLH_Code = "1234";

			AssertNoError("Country/Code can be duplicated for another packline", hc3.JLH_RN_NKCountryInfo, expectedError);
			AssertNoError("Country/Code can be duplicated for another packline", hc3.JLH_CodeInfo, expectedError);
		}
	}
}
