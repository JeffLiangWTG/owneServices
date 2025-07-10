using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USCPSCLotAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_LotNumber()
		{
			CPSCLot.US_LotNumberType = "1";
			CPSCLot.US_LotNumber = "ABC";
			AssertNoMessageErrorContaining(CPSCLot.US_LotNumberInfo, MandatoryValidation.YouHaveNotEntered);

			CPSCLot.US_LotNumber = "";
			AssertHasMessageErrorContaining(CPSCLot.US_LotNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_LotNumberType()
		{
			CPSCLot.US_LotNumberType = "A";
			AssertHasMessageErrorContaining(cpscLot.US_LotNumberTypeInfo, ListValidation.InvalidCodeMessageError);
			CPSCLot.US_LotNumberType = LotNumberQualifierList.Codes._1;
			AssertNoMessageErrorContaining(cpscLot.US_LotNumberTypeInfo, ListValidation.InvalidCodeMessageError);
			CPSCLot.US_LotNumberType = LotNumberQualifierList.Codes._3;
			AssertHasMessageErrorContaining(cpscLot.US_LotNumberTypeInfo, ListValidation.InvalidCodeMessageError);

			CPSCLot.US_LotNumber = "ABC";
			CPSCLot.US_LotNumberType = "";
			AssertHasMessageErrorContaining(cpscLot.US_LotNumberTypeInfo, MandatoryValidation.YouHaveNotEntered);

			CPSCLot.US_LotNumberType = LotNumberQualifierList.Codes._1;
			AssertNoMessageErrorContaining(cpscLot.US_LotNumberTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		Lot CPSCLot
		{
			get
			{
				if (cpscLot == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
					var cpscHeader = invoiceLine.CPSCHeaders.AddNew();
					cpscLot = cpscHeader.Lots.AddNew();
				}
				return cpscLot;
			}
		}
		Lot cpscLot;
	}
}
