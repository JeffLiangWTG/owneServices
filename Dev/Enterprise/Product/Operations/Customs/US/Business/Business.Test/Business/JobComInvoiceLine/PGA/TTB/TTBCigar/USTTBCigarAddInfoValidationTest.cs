using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	class USTTBCigarAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_Quantity()
		{
			TTBLine.US_ProcessingCode = TTBTOBProcessingCodeList.Codes.T30;
			var cigar = TTBLine.Cigars.AddNew();
			cigar.US_Quantity = 10;
			AssertNoMessageError(cigar.US_QuantityInfo, ValidationConstants.TTB.QuantityMustBeGreaterThanZero);
			AssertNoMessageError(cigar.US_QuantityInfo, ValidationConstants.TTB.QuantityCannotBeGreaterThan999999);
			cigar.US_Quantity = -10;
			AssertHasMessageError(cigar.US_QuantityInfo, ValidationConstants.TTB.QuantityMustBeGreaterThanZero);
			AssertNoMessageError(cigar.US_QuantityInfo, ValidationConstants.TTB.QuantityCannotBeGreaterThan999999);
			cigar.US_Quantity = 0;
			AssertHasMessageError(cigar.US_QuantityInfo, ValidationConstants.TTB.QuantityMustBeGreaterThanZero);
			AssertNoMessageError(cigar.US_QuantityInfo, ValidationConstants.TTB.QuantityCannotBeGreaterThan999999);
			cigar.US_Quantity = 999999;
			AssertNoMessageError(cigar.US_QuantityInfo, ValidationConstants.TTB.QuantityMustBeGreaterThanZero);
			AssertNoMessageError(cigar.US_QuantityInfo, ValidationConstants.TTB.QuantityCannotBeGreaterThan999999);
			cigar.US_Quantity = 1000000;
			AssertNoMessageError(cigar.US_QuantityInfo, ValidationConstants.TTB.QuantityMustBeGreaterThanZero);
			AssertHasMessageError(cigar.US_QuantityInfo, ValidationConstants.TTB.QuantityCannotBeGreaterThan999999);
			TTBLine.US_ProcessingCode = TTBTOBProcessingCodeList.Codes.T51;
			cigar = TTBLine.Cigars.AddNew();
			cigar.US_Quantity = 1000000;
			AssertNoMessageErrors(cigar.US_QuantityInfo);
			cigar.US_Quantity = 0;
			AssertNoMessageErrors(cigar.US_QuantityInfo);
		}

		public void TestCheckUS_UnitPrice()
		{
			TTBLine.US_ProcessingCode = TTBTOBProcessingCodeList.Codes.T30;
			var cigar = TTBLine.Cigars.AddNew();
			cigar.US_UnitPrice = 1m;
			AssertNoMessageError(cigar.US_UnitPriceInfo, ValidationConstants.TTB.UnitPriceMustBeGreaterThanZero);
			AssertNoMessageError(cigar.US_UnitPriceInfo, ValidationConstants.TTB.UnitPriceMaximumSalePrice);
			cigar.US_UnitPrice = -1m;
			AssertHasMessageError(cigar.US_UnitPriceInfo, ValidationConstants.TTB.UnitPriceMustBeGreaterThanZero);
			AssertNoMessageError(cigar.US_UnitPriceInfo, ValidationConstants.TTB.UnitPriceMaximumSalePrice);
			cigar.US_UnitPrice = 0m;
			AssertHasMessageError(cigar.US_UnitPriceInfo, ValidationConstants.TTB.UnitPriceMustBeGreaterThanZero);
			AssertNoMessageError(cigar.US_UnitPriceInfo, ValidationConstants.TTB.UnitPriceMaximumSalePrice);
			cigar.US_UnitPrice = 76.323m;
			AssertNoMessageError(cigar.US_UnitPriceInfo, ValidationConstants.TTB.UnitPriceMustBeGreaterThanZero);
			AssertHasMessageError(cigar.US_UnitPriceInfo, ValidationConstants.TTB.UnitPriceMaximumSalePrice);
			cigar.US_IsSmall = true;
			AssertNoMessageError(cigar.US_UnitPriceInfo, ValidationConstants.TTB.UnitPriceMaximumSalePrice);
			AssertNoMessageError(cigar.US_UnitPriceInfo, ValidationConstants.TTB.UnitPriceMustBeGreaterThanZero);
			TTBLine.US_ProcessingCode = TTBTOBProcessingCodeList.Codes.T51;
			cigar = TTBLine.Cigars.AddNew();
			cigar.US_UnitPrice = 76.323m;
			AssertNoMessageErrors(cigar.US_UnitPriceInfo);
			cigar.US_UnitPrice = -1m;
			AssertNoMessageErrors(cigar.US_UnitPriceInfo);
			cigar.US_UnitPrice = 0m;
			AssertNoMessageErrors(cigar.US_UnitPriceInfo);
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
			get
			{
				if (ttbLine == null)
				{
					ttbLine = InvoiceLine.TTBLines.AddNew();
					ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.Tobacco;
				}
				return ttbLine;
			}
		}
		TTBLine ttbLine;

		#endregion
	}
}
