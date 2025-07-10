using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class TaxFeeCalculatorTest : TestCaseWithFactory
	{
		[TestDate(2007, 3, 29)]
		public void TestPrimaryTariffTax()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0201300600"; // Moo
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			ZDecimal fee = new TaxFeeCalculator(Core.Constants.USCustoms.FeeCodes.Beef, false).CalculateFee(invoiceLine).Amount;
			AssertEquals(3.79m, fee);   // record correct value
		}

		public void TestPrimaryTariffFeeWhenShouldHaveFee()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0201300600";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			ZDecimal fee = new TaxFeeCalculator(Core.Constants.USCustoms.FeeCodes.Beef, false).CalculateFee(invoiceLine).Amount;
			AssertEquals("Should not be calculated for TIB entry", 0m, fee);

			fee = new TaxFeeCalculator(Core.Constants.USCustoms.FeeCodes.Beef, true).CalculateFee(invoiceLine).Amount;
			AssertEquals("Should be calculated for TIB entry when ignoreTIBExemptionCondition is set ", 3.79m, fee);
		}

		[TestDate(2007, 3, 29)]
		public void TestSecondaryTariffTax()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			JobComInvoiceLine secondaryInvoiceLine = invoiceLine.AddSecondaryInvoiceLine();
			secondaryInvoiceLine.JI_Tariff = "0201300600"; // Moo
			secondaryInvoiceLine.JI_CustomsQuantity = 1000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			ICusEntryLine entryLine = invoiceLine.CusEntryLine;

			bool beefFeeExist = false;

			foreach (IFee fee in entryLine.Fees)
			{
				if (fee.Code == Core.Constants.USCustoms.FeeCodes.Beef)
				{
					beefFeeExist = true;
					AssertEquals(3.79m, fee.Amount);
					break;
				}
			}

			Assert("beef fee exists", beefFeeExist);
		}

		[TestDate(2007, 3, 29)]
		public void TestSecondaryTariffTaxWhenTIBEntry()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			JobComInvoiceLine secondaryInvoiceLine = invoiceLine.AddSecondaryInvoiceLine();
			secondaryInvoiceLine.JI_Tariff = "0201300600"; // Moo
			secondaryInvoiceLine.JI_CustomsQuantity = 1000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			ICusEntryLine entryLine = invoiceLine.CusEntryLine;

			bool beefFeeExist = false;

			foreach (IFee fee in entryLine.Fees)
			{
				if (fee.Code == Core.Constants.USCustoms.FeeCodes.Beef)
				{
					beefFeeExist = true;
					break;
				}
			}

			AssertEquals("beef fee should not exisit for TIB entry", false, beefFeeExist);
		}

		public void TestAMSFeeExempt()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_CottonCertificateNo = "123456789";
			ZDecimal fee = new TaxFeeCalculator(Core.Constants.USCustoms.FeeCodes.Beef, false).CalculateFee(invoiceLine).Amount;
			AssertEquals(0m, fee);  // organic exemption
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}
