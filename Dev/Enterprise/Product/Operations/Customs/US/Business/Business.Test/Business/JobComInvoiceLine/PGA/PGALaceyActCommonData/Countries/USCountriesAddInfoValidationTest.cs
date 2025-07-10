using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USCountriesAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_CountryCode()
		{
			LaceyCountry.AddInfoValidation.ValidateUS_CountryCode();
			AssertHasMessageErrorContaining(LaceyCountry.US_CountryCodeInfo, MandatoryValidation.YouHaveNotEntered);

			LaceyCountry.US_CountryCode = "XX";
			AssertNoMessageErrorContaining(LaceyCountry.US_CountryCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(LaceyCountry.US_CountryCodeInfo, ListValidation.InvalidCodeMessageError);

			LaceyCountry.US_CountryCode = Core.Constants.CountryCodes.UnitedStates;
			AssertNoMessageErrorContaining(LaceyCountry.US_CountryCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		#region Implementation

		LaceyCountry LaceyCountry
		{
			get
			{
				if (laceyCountry == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					var pga = invoiceLine.LaceyActLines.AddNew();
					laceyCountry = pga.LaceyCountries.AddNew();
				}
				return laceyCountry;
			}
		}
		LaceyCountry laceyCountry;

		#endregion
	}
}
