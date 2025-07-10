using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class DeletedLineAmendmentTest : TestCaseWithFactory
	{
		public void TestCL_CustomsValue()
		{
			entryLine.CL_CustomsValue = 123.00M;
			AssertEquals("Customs Value", entryLine.CL_CustomsValue, lineAmendment.CL_CustomsValue);
		}

		public void TestCL_LineNumber()
		{
			entryLine.CL_LineNumber = 2;
			AssertEquals("Line Number", entryLine.CL_LineNumber, lineAmendment.CL_LineNumber);
		}

		public void TestTariff()
		{
			entryLine.CL_AdValoremTariff = "2345";
			AssertEquals("Tariff", "2345", lineAmendment.Tariff);
		}

		public void TestCustomsQuantity()
		{
			invoiceLine.JI_CustomsQuantity = 12;
			AssertEquals("Customs Qty", 12M, lineAmendment.CustomsQuantity);
		}

		public void TestCustomsUnitQty()
		{
			invoiceLine.JI_CustomsUnitQty = "KG";
			AssertEquals("Customs Unit Qty", "KG", lineAmendment.CustomsUnitQty);
		}

		public void TestDescription()
		{
			invoiceLine.JI_Description = "Description";
			AssertEquals("Description", "Description", lineAmendment.Description);
		}

		public void TestExtendedCommercialDescription()
		{
			invoiceLine.JI_ExtraInfoForClassification = "Extended Description";
			AssertEquals("ExtendedCommercialDescription", "Extended Description", lineAmendment.ExtendedCommercialDescription);
		}

		public void TestBondedWarehouseQuantity()
		{
			invoiceLine.JI_InvoiceQuantity = 123;
			AssertEquals("Bondend Why Qty", 123M, lineAmendment.BondedWarehouseQuantity);
		}

		public void TestBondedWarehouseUnitQuantity()
		{
			invoiceLine.JI_InvoiceUQ = "KG";
			AssertEquals("Bonded Why Unit Qty", "KG", lineAmendment.BondedWarehouseUnitQuantity);
		}

		public void TestCL_DutyPercent()
		{
			entryLine.CL_DutyPercent = 10.16m;
			AssertEquals("Duty Percent", 10.16m, lineAmendment.CL_DutyPercent);
		}

		public void TestDutyRateDescription()
		{
			entryLine.CL_DutyPercent = 10.16m;
			AssertEquals("DutyRateDescription", "10.16%", lineAmendment.DutyRateDescription);
		}

		public void TestCL_ParentTrailer()
		{
			entryLine.CL_ParentTrailer = "A";
			AssertEquals("Parent Trailer", "A", lineAmendment.CL_ParentTrailer);
		}

		public void TestDutyAmount()
		{
			entryLine.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 250m);
			AssertEquals("Duty Amount", 250m, lineAmendment.DutyAmount);
		}

		public void TestGSTVATAmount()
		{
			entryLine.Fees.AddOrUpdate(declaration.GSTOrVATCode, 301m);
			AssertEquals("GST or VAT Amount", 301m, lineAmendment.GSTVATAmount);
		}

		public void TestGSTVATDeferred()
		{
			entryLine.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.GSTVATDeferred, 506m);
			AssertEquals("GST or VAT deferred", 506m, lineAmendment.GSTVATDeferred);
		}

		public void TestInvoiceLines()
		{
			AssertEquals("InvoiceLines", 1, lineAmendment.InvoiceLines.Count);
			AssertEquals("InvoiceLines", invoiceLine.PK.ToString(), lineAmendment.InvoiceLines[0].PK.ToString());
		}

		public void TestRandomLine()
		{
			AssertEquals("RandomLine", invoiceLine.PK, lineAmendment.RandomLine.PK);
		}

		public void TestTotalLinePrice()
		{
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = invoiceLine.Declaration.LocalCurrencyCode;
			invoiceLine.JI_LinePrice = 1000m;
			AssertEquals("Total Line Price", 1000m, lineAmendment.TotalLinePrice.Amount);
			AssertEquals("Total Line Price", invoiceLine.Declaration.LocalCurrencyCode, lineAmendment.TotalLinePrice.Currency.Code);
		}

		public void TestTotalLinePriceInLocalCurrency()
		{
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = invoiceLine.Declaration.LocalCurrencyCode;
			invoiceLine.JI_LinePrice = 1000m;
			AssertEquals("Total Line Price", 1000m, lineAmendment.TotalLinePriceInLocalCurrency);
		}

		public void TestCustomsValue()
		{
			entryLine.CL_CustomsValue = 1000m;
			AssertEquals("Customs Value Amount", 1000m, lineAmendment.CustomsValue.Amount);
			AssertEquals("Customs Value Currency", invoiceLine.Declaration.LocalCurrencyCode, lineAmendment.CustomsValue.Currency.Code);
		}

		#region Implemenation

		protected override void SetUp()
		{
			base.SetUp();
			declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			lineAmendment = new DeletedLineAmendment(entryLine, entryHeader);
		}
		BaseJobDeclaration declaration;
		ICusEntryLine lineAmendment;
		BaseJobComInvoiceLine invoiceLine;
		CusEntryLine entryLine;

		#endregion
	}
}
