using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USExportTTBLineAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_Date()
		{
			ExportTTB.AddInfoValidation.ValidateUS_Date();
			AssertHasMessageErrorContaining(ExportTTB.US_DateInfo, MandatoryValidation.YouHaveNotEntered);

			ExportTTB.US_Date = ZDateTime.Today;
			AssertNoMessageErrorContaining(ExportTTB.US_DateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_Serial()
		{
			ExportTTB.AddInfoValidation.ValidateUS_SerialNumber();
			AssertHasMessageErrorContaining(ExportTTB.US_SerialNumberInfo, MandatoryValidation.YouHaveNotEntered);

			ExportTTB.US_SerialNumber = "1";
			AssertNoMessageErrorContaining(ExportTTB.US_SerialNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		#region Implementation

		TTBLine ExportTTB
		{
			get
			{
				if (exportTTB == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.InvoiceLines.AddNew();
					invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
					exportTTB = invoiceLine.TTBLines.AddNew();
				}

				return exportTTB;
			}
		}
		TTBLine exportTTB;

		#endregion
	}
}
