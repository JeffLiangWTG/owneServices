using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USExportDEAHeaderAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_PermitNumber()
		{
			ExportDEA.AddInfoValidation.ValidateUS_PermitNumber();
			AssertHasMessageErrorContaining(ExportDEA.US_PermitNumberInfo, MandatoryValidation.YouHaveNotEntered);

			ExportDEA.US_PermitNumber = "~";
			AssertNoMessageErrorContaining(ExportDEA.US_PermitNumberInfo, MandatoryValidation.YouHaveNotEntered);

			ExportDEA.InvoiceLine.US_DEAInd = ZString.Empty;
			ExportDEA.US_PermitNumber = ZString.Empty;
			AssertNoMessageErrorContaining(ExportDEA.US_PermitNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_RegistrationNumber()
		{
			ExportDEA.AddInfoValidation.ValidateUS_RegistrationNumber();
			AssertHasMessageErrorContaining(ExportDEA.US_RegistrationNumberInfo, MandatoryValidation.YouHaveNotEntered);

			ExportDEA.US_RegistrationNumber = "~";
			AssertNoMessageErrorContaining(ExportDEA.US_RegistrationNumberInfo, MandatoryValidation.YouHaveNotEntered);

			ExportDEA.InvoiceLine.US_DEAInd = ZString.Empty;
			ExportDEA.US_RegistrationNumber = ZString.Empty;
			AssertNoMessageErrorContaining(ExportDEA.US_RegistrationNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		#region Implementation

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (fInvoiceLine == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					var invoice = declaration.Invoices.AddNew();
					fInvoiceLine = invoice.InvoiceLines.AddNew();
					fInvoiceLine.US_DEAInd = OGAIndicatorList.Codes.Declared;
				}

				return fInvoiceLine;
			}
		}
		JobComInvoiceLine fInvoiceLine;

		DEAHeader ExportDEA
		{
			get
			{
				if (exportDEA == null || exportDEA.IsDeleting || exportDEA.IsDeleted)
				{
					exportDEA = InvoiceLine.DEAHeaders.AddNew();
				}

				return exportDEA;
			}
		}
		DEAHeader exportDEA;

		#endregion
	}
}
