using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USDrawbackNAFTAAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_DRWNAFTACountryImportEntry()
		{
			NAFTA.US_DRWNAFTACountryImportEntry = ZString.Empty;
			AssertHasMessageErrorContaining(NAFTA.US_DRWNAFTACountryImportEntryInfo, MandatoryValidation.YouHaveNotEntered);

			NAFTA.US_DRWNAFTACountryImportEntry = "~";
			AssertNoMessageErrorContaining(NAFTA.US_DRWNAFTACountryImportEntryInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_DRWNAFTACountryImportEntryDate()
		{
			NAFTA.US_DRWNAFTACountryImportEntryDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(NAFTA.US_DRWNAFTACountryImportEntryDateInfo, MandatoryValidation.YouHaveNotEntered);

			NAFTA.US_DRWNAFTACountryImportEntryDate = ZDateTime.Today;
			AssertNoMessageErrorContaining(NAFTA.US_DRWNAFTACountryImportEntryDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_DRWNAFTACountryImportDuty()
		{
			NAFTA.US_DRWNAFTACountryImportDuty = ZDecimal.Zero;
			AssertHasMessageErrorContaining(NAFTA.US_DRWNAFTACountryImportDutyInfo, MandatoryValidation.YouHaveNotEntered);

			NAFTA.US_DRWNAFTACountryImportDuty = 100m;
			AssertNoMessageErrorContaining(NAFTA.US_DRWNAFTACountryImportDutyInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_DRWNAFTACountryDutyRate()
		{
			NAFTA.US_DRWNAFTACountryDutyRate = ZDecimal.Zero;
			AssertHasMessageErrorContaining(NAFTA.US_DRWNAFTACountryDutyRateInfo, MandatoryValidation.YouHaveNotEntered);

			NAFTA.US_DRWNAFTACountryDutyRate = 100m;
			AssertNoMessageErrorContaining(NAFTA.US_DRWNAFTACountryDutyRateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_DRWNAFTACountryTariffNumber()
		{
			NAFTA.US_DRWNAFTACountryTariffNumber = ZString.Empty;
			AssertHasMessageErrorContaining(NAFTA.US_DRWNAFTACountryTariffNumberInfo, MandatoryValidation.YouHaveNotEntered);

			NAFTA.US_DRWNAFTACountryTariffNumber = "~";
			AssertNoMessageErrorContaining(NAFTA.US_DRWNAFTACountryTariffNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_DRWNAFTACountryOfExport()
		{
			NAFTA.US_DRWNAFTACountryOfExport = "~";
			AssertHasMessageErrorContaining(NAFTA.US_DRWNAFTACountryOfExportInfo, USDrawbackNAFTAAddInfoValidation.CountryOfExportMustBeCAOrMX);

			NAFTA.US_DRWNAFTACountryOfExport = Core.Constants.CountryCodes.Canada;
			AssertNoMessageErrorContaining(NAFTA.US_DRWNAFTACountryOfExportInfo, USDrawbackNAFTAAddInfoValidation.CountryOfExportMustBeCAOrMX);

			NAFTA.US_DRWNAFTACountryOfExport = ZString.Empty;
			AssertHasMessageErrorContaining(NAFTA.US_DRWNAFTACountryOfExportInfo, USDrawbackNAFTAAddInfoValidation.CountryOfExportMustBeCAOrMX);

			NAFTA.US_DRWNAFTACountryOfExport = Core.Constants.CountryCodes.Mexico;
			AssertNoMessageErrorContaining(NAFTA.US_DRWNAFTACountryOfExportInfo, USDrawbackNAFTAAddInfoValidation.CountryOfExportMustBeCAOrMX);
		}

		#region Implementation

		DrawbackNAFTA NAFTA
		{
			get
			{
				if (fNAFTA == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
					fNAFTA = invoiceLine.DrawbackNAFTAs.AddNew();
				}

				return fNAFTA;
			}
		}
		DrawbackNAFTA fNAFTA;

		#endregion
	}
}
