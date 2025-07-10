using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	class USFWSLicenseAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_Number()
		{
			var license = Header.Licenses.AddNew();
			license.US_Type = FWSLicenseTypeList.Codes.FWSForeignCitesDocument;
			license.US_Number = "A";
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage("License Number");
			AssertNoMessageError(license.US_NumberInfo, messageError);
			license.US_Number = ZString.Empty;
			AssertHasMessageError(license.US_NumberInfo, messageError);
			license.US_Number = "12345697801234567890";
			AssertNoMessageError(license.US_NumberInfo, USFWSLicenseAddInfoValidation.FWCMaximumLength);
			license.US_Type = FWSLicenseTypeList.Codes.FWSeDecsConfirmationNumber;
			license.US_Number = "98765432109876543210";
			AssertHasMessageError(license.US_NumberInfo, USFWSLicenseAddInfoValidation.FWCMaximumLength);
			license.US_Number = "123-456- ABC";
			AssertHasMessageError(license.US_NumberInfo, USFWSLicenseAddInfoValidation.FWCMaximumLength);
			license.US_Number = "123456ABC";
			AssertNoMessageError(license.US_NumberInfo, USFWSLicenseAddInfoValidation.FWCMaximumLength);
		}

		public void TestCheckUS_Type()
		{
			var license1 = Header.Licenses.AddNew();
			Header.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			license1.US_Type = FWSLicenseTypeList.Codes.FWSeDecsConfirmationNumber;
			AssertHasMessageError(license1.US_TypeInfo, ValidationConstants.FWS.FWCShouldOnlyBeEnteredWhenProcessingCodeIsLDS);
			license1.US_Type = FWSLicenseTypeList.Codes.FWSUSCitesDocument;
			AssertNoMessageError(license1.US_TypeInfo, ValidationConstants.FWS.FWCShouldOnlyBeEnteredWhenProcessingCodeIsLDS);

			license1.US_Type = "#";
			AssertHasMessageErrorContaining(license1.US_TypeInfo, ListValidation.InvalidCodeMessageError);
			foreach (ICodeDescription pair in new FWSLicenseTypeList())
			{
				license1.US_Type = pair.Code;
				AssertNoMessageErrorContaining(license1.US_TypeInfo, ListValidation.InvalidCodeMessageError);
			}
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

		FWSHeader Header
		{
			get
			{
				if (fwsHeader == null)
				{
					fwsHeader = InvoiceLine.FWSHeaders.AddNew();
					fwsHeader.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
				}
				return fwsHeader;
			}
		}
		FWSHeader fwsHeader;

		#endregion
	}
}
