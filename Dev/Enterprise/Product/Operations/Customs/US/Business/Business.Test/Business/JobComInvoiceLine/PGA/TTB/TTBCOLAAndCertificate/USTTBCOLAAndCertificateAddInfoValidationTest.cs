using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USTTBCOLAAndCertificateAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_COLAExemptionCode()
		{
			TTBLine.US_ProgramCode = TTBProgramCodeList.Codes.Beverage;
			COLAAndCertificate.US_COLA = "ASD";
			COLAAndCertificate.US_COLAExemptionCode = TTBExemptionCodeList.Codes.TTBEX2;
			AssertHasMessageError(COLAAndCertificate.US_COLAExemptionCodeInfo, ValidationConstants.TTB.EitherCOLAOrExemptionCodeIsRequiredButNotBoth);
			AssertNoMessageErrorContaining(COLAAndCertificate.US_COLAExemptionCodeInfo, ListValidation.InvalidCodeMessageError);
			COLAAndCertificate.US_COLA = "";
			AssertNoMessageError(COLAAndCertificate.US_COLAExemptionCodeInfo, ValidationConstants.TTB.EitherCOLAOrExemptionCodeIsRequiredButNotBoth);
			AssertNoMessageErrorContaining(COLAAndCertificate.US_COLAExemptionCodeInfo, ListValidation.InvalidCodeMessageError);
			TTBLine.US_ProgramCode = TTBProgramCodeList.Codes.Wine;
			COLAAndCertificate.AddInfoValidation.ValidateUS_COLAExemptionCode();
			AssertNoMessageError(COLAAndCertificate.US_COLAExemptionCodeInfo, ValidationConstants.TTB.EitherCOLAOrExemptionCodeIsRequiredButNotBoth);
			AssertHasMessageErrorContaining(COLAAndCertificate.US_COLAExemptionCodeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.ValidationModes = ValidationModes.None;
			COLAAndCertificate.AddInfoValidation.ValidateUS_COLAExemptionCode();
			AssertNoMessageError(COLAAndCertificate.US_COLAExemptionCodeInfo, ValidationConstants.TTB.EitherCOLAOrExemptionCodeIsRequiredButNotBoth);
			AssertNoMessageErrorContaining(COLAAndCertificate.US_COLAExemptionCodeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			COLAAndCertificate.AddInfoValidation.ValidateUS_COLAExemptionCode();
			AssertNoMessageError(COLAAndCertificate.US_COLAExemptionCodeInfo, ValidationConstants.TTB.EitherCOLAOrExemptionCodeIsRequiredButNotBoth);
			AssertHasMessageErrorContaining(COLAAndCertificate.US_COLAExemptionCodeInfo, ListValidation.InvalidCodeMessageError);

			TTBLine.US_ProgramCode = TTBProgramCodeList.Codes.Tobacco;
			COLAAndCertificate.US_COLAExemptionCode = "@#";
			AssertNoMessageError(COLAAndCertificate.US_COLAExemptionCodeInfo, ValidationConstants.TTB.EitherCOLAOrExemptionCodeIsRequiredButNotBoth);
			AssertNoMessageErrorContaining(COLAAndCertificate.US_COLAExemptionCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckHasForeignCertificate()
		{
			TTBLine.US_ProgramCode = TTBProgramCodeList.Codes.Beverage;
			COLAAndCertificate.HasForeignCertificate = true;
			AssertHasWarning(COLAAndCertificate.HasForeignCertificateInfo, ValidationConstants.TTB.MissingForeignCertificateCountry);
			COLAAndCertificate.US_ForeignCertificateCountry = Core.Constants.CountryCodes.NewZealand;
			AssertNoWarning(COLAAndCertificate.HasForeignCertificateInfo, ValidationConstants.TTB.MissingForeignCertificateCountry);
		}

		public void TestCheckUS_ForeignCertificateCountry()
		{
			TTBLine.US_ProgramCode = TTBProgramCodeList.Codes.Beverage;
			COLAAndCertificate.US_ForeignCertificateCountry = Core.Constants.CountryCodes.NewZealand;
			AssertNoMessageErrorContaining(COLAAndCertificate.US_ForeignCertificateCountryInfo, ListValidation.InvalidCodeMessageError);
			COLAAndCertificate.US_ForeignCertificateCountry = "@#";
			AssertHasMessageErrorContaining(COLAAndCertificate.US_ForeignCertificateCountryInfo, ListValidation.InvalidCodeMessageError);
			Declaration.ValidationModes = ValidationModes.None;
			COLAAndCertificate.AddInfoValidation.ValidateUS_ForeignCertificateCountry();
			AssertNoMessageErrorContaining(COLAAndCertificate.US_ForeignCertificateCountryInfo, ListValidation.InvalidCodeMessageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			COLAAndCertificate.AddInfoValidation.ValidateUS_ForeignCertificateCountry();
			AssertHasMessageErrorContaining(COLAAndCertificate.US_ForeignCertificateCountryInfo, ListValidation.InvalidCodeMessageError);
			TTBLine.US_ProgramCode = TTBProgramCodeList.Codes.Tobacco;
			COLAAndCertificate.US_ForeignCertificateCountry = "@#";
			AssertNoMessageErrorContaining(COLAAndCertificate.US_ForeignCertificateCountryInfo, ListValidation.InvalidCodeMessageError);
		}

		#region Implementation

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableENS = true;
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		JobComInvoiceHeader Invoice
		{
			get { return invoice ?? (invoice = Declaration.Invoices.AddNew()); }
		}
		JobComInvoiceHeader invoice;

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Invoice.JobComInvoiceLines.AddNew()); }
		}
		JobComInvoiceLine invoiceLine;

		TTBLine TTBLine
		{
			get { return ttbLine ?? (ttbLine = InvoiceLine.TTBLines.AddNew()); }
		}
		TTBLine ttbLine;

		TTBCOLAAndCertificate COLAAndCertificate
		{
			get
			{
				if (colaAndCertificate == null || colaAndCertificate.IsDeleted)
				{
					colaAndCertificate = TTBLine.COLAAndCertificates.AddNew();
				}
				return colaAndCertificate;
			}
		}
		TTBCOLAAndCertificate colaAndCertificate;

		#endregion
	}
}
