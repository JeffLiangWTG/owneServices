using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class USScientificDataAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_PGACountryCode()
		{
			ScientificData.US_PGACountryCode = "~";
			AssertHasMessageError(ScientificData.US_PGACountryCodeInfo, ListValidation.InvalidCodeMessageError);

			ScientificData.US_PGACountryCode = "US";
			AssertNoMessageError(ScientificData.US_PGACountryCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrors(USScientificDataAddInfoValidation.PGACountryCodeIsRequired, ScientificData.US_PGACountryCodeInfo);

			ScientificData.US_PGACountryCode = "";
			AssertHasMessageErrors(USScientificDataAddInfoValidation.PGACountryCodeIsRequired, ScientificData.US_PGACountryCodeInfo);

			ScientificData.US_PGACountryCode = "**";
			AssertNoMessageError(ScientificData.US_PGACountryCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrors(USScientificDataAddInfoValidation.PGACountryCodeIsRequired, ScientificData.US_PGACountryCodeInfo);
		}

		public void TestCheckUS_PGAScientificGenusName()
		{
			ScientificData.US_PGAScientificGenusName = "AMORPHOS";
			AssertNoMessageErrors(USScientificDataAddInfoValidation.PGAScientificGenusNameIsRequired, ScientificData.US_PGAScientificGenusNameInfo);

			ScientificData.US_PGAScientificGenusName = "";
			AssertHasMessageErrors(USScientificDataAddInfoValidation.PGAScientificGenusNameIsRequired, ScientificData.US_PGAScientificGenusNameInfo);
		}

		public void TestCheckUS_PGAScientificSpeciesName()
		{
			ScientificData.US_PGAScientificSpeciesName = "AMORPHOS";
			AssertNoMessageErrors(USScientificDataAddInfoValidation.PGAScientificSpeciesNameIsRequired, ScientificData.US_PGAScientificSpeciesNameInfo);

			ScientificData.US_PGAScientificSpeciesName = "";
			AssertHasMessageErrors(USScientificDataAddInfoValidation.PGAScientificSpeciesNameIsRequired, ScientificData.US_PGAScientificSpeciesNameInfo);
		}

		ScientificData ScientificData
		{
			get
			{
				if (scientificData == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					var laceyAct = invoiceLine.LaceyActLines.AddNew();
					var constituentElement = laceyAct.PG04ConstituentElements.AddNew();
					scientificData = constituentElement.ScientificDataCollection.AddNew();
				}
				return scientificData;
			}
		}
		ScientificData scientificData;
	}
}
