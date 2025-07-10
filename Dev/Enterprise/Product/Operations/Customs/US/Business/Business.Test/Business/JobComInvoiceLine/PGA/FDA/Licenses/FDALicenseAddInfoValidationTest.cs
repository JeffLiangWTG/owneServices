using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class FDALicenseAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCountryAndStateCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();
			var license = fda.Licenses.AddNew();

			license.US_CountryCode = "!";
			AssertHasMessageErrorContaining(license.US_CountryCodeInfo, ListValidation.InvalidCodeMessageError);

			license.US_CountryCode = Core.Constants.CountryCodes.Canada;
			license.US_StateCode = "$";
			AssertNoMessageErrorContaining(license.US_CountryCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(license.US_StateCodeInfo, ListValidation.InvalidCodeMessageError);

			license.US_StateCode = ZString.Empty;
			AssertNoMessageErrorContaining(license.US_StateCodeInfo, ListValidation.InvalidCodeMessageError);
			foreach (ICodeDescription pair in new CanadaStatesList())
			{
				license.US_StateCode = pair.Code;
				AssertNoMessageErrors(license.US_StateCodeInfo);
			}
		}
	}
}
