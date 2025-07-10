using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class ACSConstituentElementAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_PGAPercentOfConstituentElementIsValidZDecimal()
		{
			PGA.US_PGACommercialDescription = "DESC";
			var constElement = PGA.PG04ConstituentElements.AddNew();
			constElement.US_PGAPercentOfConstituentElement = 88888.8888;
			constElement.AddInfoValidation.ValidateUS_PGAPercentOfConstituentElement();

			AssertHasError(constElement.US_PGAPercentOfConstituentElementInfo, "The number 88,888.8888 is too large, the maximum value allowed for selection is 999.999.");
		}
		public void TestCheckUS_PGANameOfTheConstituentElement()
		{
			ConstituentElement.US_PGANameOfTheConstituentElement = "";
			ConstituentElement.AddInfoValidation.ValidateUS_PGAPercentOfConstituentElement();
			AssertHasMessageError(ConstituentElement.US_PGANameOfTheConstituentElementInfo, "Name Of The Constituent Element is mandatory.");

			ConstituentElement.US_PGANameOfTheConstituentElement = "TEST";
			AssertNoMessageError(ConstituentElement.US_PGANameOfTheConstituentElementInfo, "Name Of The Constituent Element is mandatory.");
		}

		ConstituentElement ConstituentElement
		{
			get { return constituentElement ?? (constituentElement = PGA.PG04ConstituentElements.AddNew()); }
		}
		ConstituentElement constituentElement;

		PGA PGA
		{
			get
			{
				if (pga == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
					declaration.US_CertifyCargoRelease = true;
					declaration.US_EnableCRL = true;
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					pga = invoiceLine.LaceyActLines.AddNew();
				}

				return pga;
			}
		}
		PGA pga;
	}
}
