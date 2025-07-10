using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	class BaseJobComInvoiceLineFunctionalTest : TestCaseWithFactory
	{
		public void TestCustomsCountryCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Afghanistan))
			{
				AssertEquals("CustomsCountryCodeCore should be AF", "AF", invoiceLine.CustomsCountryCode);
			}
		}

		public void TestSetQuantityThenUnitPrice()
		{
			invoiceLine.JI_InvoiceQuantity = 10m;
			AssertEquals("UnitPrice", 0m, invoiceLine.UnitPrice);
			AssertEquals("Quantity", 10m, invoiceLine.JI_InvoiceQuantity);
			AssertEquals("Total", 0m, invoiceLine.JI_LinePrice);

			invoiceLine.UnitPrice = 100m;
			AssertEquals("UnitPrice", 100m, invoiceLine.UnitPrice);
			AssertEquals("Quantity", 10m, invoiceLine.JI_InvoiceQuantity);
			AssertEquals("Total", 1000m, invoiceLine.JI_LinePrice);
		}

		public void TestSetQuantityThenTotalPrice()
		{
			invoiceLine.JI_InvoiceQuantity = 10m;
			AssertEquals("UnitPrice", 0m, invoiceLine.UnitPrice);
			AssertEquals("Quantity", 10m, invoiceLine.JI_InvoiceQuantity);
			AssertEquals("Total", 0m, invoiceLine.JI_LinePrice);

			invoiceLine.JI_LinePrice = 1000m;
			AssertEquals("UnitPrice", 100m, invoiceLine.UnitPrice);
			AssertEquals("Quantity", 10m, invoiceLine.JI_InvoiceQuantity);
			AssertEquals("Total", 1000m, invoiceLine.JI_LinePrice);
		}

		public void TestShouldCopyOrderNumber()
		{
			var jobDecalration = Factory.New<BaseJobDeclaration>();
			var invoiceHeader = jobDecalration.Invoices.AddNew();
			var invoiceLine = jobDecalration.InvoiceLines.AddNew();
			invoiceLine.JI_OrderNumber = "000001";
			AssertEquals("Should contain the order number now", true, jobDecalration.DocsAndCartage.OrderItems.AsString.Contains("000001"));
			Factory.Save();

			var job2 = new BusinessObjectFactory().Load<BaseJobDeclaration>(jobDecalration.PK);
			var invoiceLine2 = job2.InvoiceLines.AddNew();
			invoiceLine2.JI_OrderNumber = "000002";
			AssertEquals("Should contain the order number1 now", true, job2.DocsAndCartage.OrderItems.AsString.Contains("000001"));
			AssertEquals("Should contain the order number2 now", true, job2.DocsAndCartage.OrderItems.AsString.Contains("000002"));
		}

		public void TestSetTotalPriceThenQuantity()
		{
			invoiceLine.JI_LinePrice = 1000m;
			AssertEquals("UnitPrice", 0m, invoiceLine.UnitPrice);
			AssertEquals("Quantity", 0m, invoiceLine.JI_InvoiceQuantity);
			AssertEquals("Total", 1000m, invoiceLine.JI_LinePrice);

			invoiceLine.JI_InvoiceQuantity = 10m;
			AssertEquals("UnitPrice", 100m, invoiceLine.UnitPrice);
			AssertEquals("Quantity", 10m, invoiceLine.JI_InvoiceQuantity);
			AssertEquals("Total", 1000m, invoiceLine.JI_LinePrice);
		}

		public void TestRoundingErrorForHighPrecisionUnitPrice()
		{
			invoiceLine.JI_InvoiceQuantity = 33.33333m;
			invoiceLine.UnitPrice = 3.0001m;
			AssertEquals("JI_LinePrice", 100.00m, invoiceLine.JI_LinePrice);
			AssertEquals("UnitPrice", 3.0001m, invoiceLine.UnitPrice);
		}

		public void TestDefaultUnitPrice()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "TestImporter";
			importer.OH_IsConsignor = true;
			importer.MiscServ.OM_EXDefaultInvoicePriceFromProductLastCost = "YES";

			var partApple = Factory.New<OrgSupplierPart>();
			partApple.FillWithValidTestData();
			partApple.OP_PartNum = "APPLE";
			partApple.OP_Desc = "Granny Smith";
			partApple.OP_LastCost = 0.65m;
			partApple.RelatedOrganisations.RemoveAndDeleteAll();
			partApple.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);

			var classificationGranny = Factory.New<BaseCusClassification>();
			classificationGranny.FillWithValidTestData();
			var appleGrannyPivot = Factory.New<BaseCusClassPartPivot>();
			appleGrannyPivot.CI_CC = classificationGranny.PK;
			appleGrannyPivot.CI_OP = partApple.PK;
			classificationGranny.CC_TariffNum = "0000.00.00 1";
			classificationGranny.CC_LookupCode = "GRANNY";
			classificationGranny.CC_ClassificationType = "BTH";
			Factory.Save();

			var dec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			dec.JE_OH_Importer = importer.PK;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;

			var line1 = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().InvoiceLines.AddNew();
			line1.JI_PartNo = "APPLE";
			line1.JI_InvoiceQuantity = 144;
			AssertEquals("UnitPrice should default for this line from Part last cost", 0.65m, line1.UnitPrice);
			AssertEquals("Quantity", 144m, line1.JI_InvoiceQuantity);
			AssertEquals("Total should now be calculated from qty * unit cost", 93.6m, line1.JI_LinePrice);

			line1.UnitPrice = 1.125m;
			AssertEquals("Subsequent changes to UnitPrice should cause recalculations as per normal", 1.125m, line1.UnitPrice);
			AssertEquals("Quantity", 144m, line1.JI_InvoiceQuantity);
			AssertEquals("Total", 162m, line1.JI_LinePrice);

			line1.JI_InvoiceQuantity = 1500m;
			AssertEquals("Subsequent changes to InvoiceQuantity should cause a recalculation of UnitPrice as per standard behaviour", 0.108m, line1.UnitPrice);
			AssertEquals("Quantity", 1500m, line1.JI_InvoiceQuantity);
			AssertEquals("Total should remain as ", 162m, line1.JI_LinePrice);

			line1.JI_LinePrice = 1835m;
			AssertEquals("Subsequent changes to the invoice LinePrice should recalculate values as per normal - Unit price:", 1.2233m, line1.UnitPrice);
			AssertEquals("Quantity", 1500m, line1.JI_InvoiceQuantity);
			AssertEquals("Total", 1835m, line1.JI_LinePrice);
		}

		public void TestSetCurrencyToLinePriceRefCurrencyIfNeeded()
		{
			var testDec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var testHeader = testDec.Invoices.AddNew();
			testHeader.JZ_InvoiceAmount = 500;
			testHeader.JZ_RX_NKInvoice_Currency = "USD";
			var headerCharge = testHeader.Charges.AddNew();
			headerCharge.J7_ChargeType = "COM";
			headerCharge.J7_Percentage = 10m;
			testDec.ResumeApportionment();

			var testLine = testHeader.InvoiceLines.AddNew();
			testLine.JI_LinePrice = 200m;
			var testCharge1 = testLine.Charges.AddNew();
			testCharge1.J7_Amount = 50m;
			var testCharge2 = testLine.Charges.AddNew();
			testCharge2.J7_Percentage = 10m;
			testDec.ResumeApportionment();

			var testChargeApportioned = testLine.ApportionedCharges.FirstOrDefault() as Common.JobComInvCharge;

			CombineAssertions(() =>
			{
				AssertEquals("1-1", "USD", testCharge1.J7_RX_NKCurrency);
				AssertEquals("1-2", "USD", testCharge2.J7_RX_NKCurrency);
				AssertEquals("1-3", "USD", testChargeApportioned.J7_RX_NKCurrency);

				testHeader.JZ_RX_NKInvoice_Currency = "JPY";
				testDec.ResumeApportionment();
				testChargeApportioned = testLine.ApportionedCharges.FirstOrDefault();
				AssertEquals("2-1", "USD", testCharge1.J7_RX_NKCurrency);
				AssertEquals("2-2", "JPY", testCharge2.J7_RX_NKCurrency);
				AssertEquals("2-3", "JPY", testChargeApportioned.J7_RX_NKCurrency);

				testDec.ResumeApportionment();
				testChargeApportioned = testLine.ApportionedCharges.FirstOrDefault();
				AssertEquals("3-1", "USD", testCharge1.J7_RX_NKCurrency);
				AssertEquals("3-2", "JPY", testCharge2.J7_RX_NKCurrency);
				AssertEquals("3-3", "JPY", testChargeApportioned.J7_RX_NKCurrency);

				testHeader.JZ_RX_NKInvoice_Currency = "XXX";
				testDec.ResumeApportionment();
				testChargeApportioned = testLine.ApportionedCharges.FirstOrDefault();

				AssertEquals("4-1", "USD", testCharge1.J7_RX_NKCurrency);
				AssertEquals("4-2", "JPY", testCharge2.J7_RX_NKCurrency);
				AssertEquals("4-3", "", testChargeApportioned.J7_RX_NKCurrency);

				testHeader.JZ_RX_NKInvoice_Currency = "XXX";
				testDec.ResumeApportionment();
				testChargeApportioned = testLine.ApportionedCharges.FirstOrDefault();
				AssertEquals("5-1", "USD", testCharge1.J7_RX_NKCurrency);
				AssertEquals("5-2", "JPY", testCharge2.J7_RX_NKCurrency);
				AssertEquals("5-3", "", testChargeApportioned.J7_RX_NKCurrency);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var dec = Factory.New<BaseJobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
		}

		protected BaseJobComInvoiceLine invoiceLine;
	}
}
