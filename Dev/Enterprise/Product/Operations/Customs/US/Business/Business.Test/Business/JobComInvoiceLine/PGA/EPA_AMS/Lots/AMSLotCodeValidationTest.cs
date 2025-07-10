using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class AMSLotCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCY_Code()
		{
			var lot = AMSLotCodes.AddNew();
			lot.CY_Code = "~";
			AssertHasMessageErrorContaining(lot.CY_CodeInfo, ListValidation.InvalidCodeMessageError);

			lot.CY_Code = LotNumberQualifierList.Codes._3;
			AssertNoMessageErrorContaining(lot.CY_CodeInfo, ListValidation.InvalidCodeMessageError);

			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var amsHeader  = invoiceLine.AMSLines.AddNew();
			amsHeader.US_Program = AMSProgramList.Codes.OR2;
			var lotCode = amsHeader.LotCodes.AddNew();
			lotCode.CY_Data = "1234";
			lotCode.Validation.ValidateCY_Code();
			AssertHasMessageErrorContaining(lotCode.CY_CodeInfo, MandatoryValidation.YouHaveNotEntered);
			lotCode.CY_Code = LotNumberQualifierList.Codes._3;
			AssertNoMessageErrorContaining(lotCode.CY_CodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCY_Data()
		{
			var lot = AMSLotCodes.AddNew();
			lot.CY_Code = LotNumberQualifierList.Codes._3;
			lot.CY_Data = "4-4851235";
			AssertNoMessageErrorContaining(lot.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);

			lot.CY_Data = ZString.Empty;
			AssertHasMessageErrorContaining(lot.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);
		}

		AMSLotCodeCollection AMSLotCodes
		{
			get
			{
				if (amsLotCodeCollection == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
					var ams = invoiceLine.AMSLines.AddNew();
					ams.US_Program = AMSProgramList.Codes.PN1;
					var amsLine = ams.AMSLines.AddNew();
					amsLotCodeCollection = amsLine.LotCodes;
				}
				return amsLotCodeCollection;
			}
		}
		AMSLotCodeCollection amsLotCodeCollection;
	}
}
