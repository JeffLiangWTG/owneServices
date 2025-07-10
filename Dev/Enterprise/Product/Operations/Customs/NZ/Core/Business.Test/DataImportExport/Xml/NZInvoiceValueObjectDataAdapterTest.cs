using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.NZ.Business.Data.Testing
{
	[TestedType(typeof(NZInvoiceValueObjectDataAdapter))]
	public class NZInvoiceValueObjectDataAdapterTest : DataTransfer.Testing.InvoiceValueObjectDataAdapterTest
	{
		public void TestImportInvoiceHeaderAdditionalInfo()
		{
			new NZInvoiceDataTransferToolTest(InvoiceDataAdapter, GetInvoiceHeader, Factory).TestImportInvoiceHeaderAdditionalInfo();
		}

		public void TestImportInvoiceLinesAdditionalInfo()
		{
			new NZInvoiceDataTransferToolTest(InvoiceDataAdapter, GetInvoiceHeader, Factory).TestImportInvoiceLinesAdditionalInfo();
		}

		public void TestGetXmlInvoiceHeaderAdditionalInfo()
		{
			new NZInvoiceDataTransferToolTest(InvoiceDataAdapter, GetInvoiceHeader, Factory).TestGetXmlInvoiceHeaderAdditionalInfo();
		}

		public void TestExportInvoiceLinesAdditionalInfo()
		{
			new NZInvoiceDataTransferToolTest(InvoiceDataAdapter, GetInvoiceHeader, Factory).TestExportInvoiceLinesAdditionalInfo();
		}

		public void TestImportNZPreferenceCode()
		{
			new NZInvoiceDataTransferToolTest(InvoiceDataAdapter, GetInvoiceHeader, Factory).TestImportNZPreferenceCode();
		}

		public void TestExportNZPreferenceCode()
		{
			new NZInvoiceDataTransferToolTest(InvoiceDataAdapter, GetInvoiceHeader, Factory).TestExportNZPreferenceCode();
		}

		public void TestSetInvoiceLineCharges()
		{
			new NZInvoiceDataTransferToolTest(InvoiceDataAdapter, GetInvoiceHeader, Factory).TestSetInvoiceLineCharges();
		}

		protected override ZInt GSTVATRate
		{
			get { return 15; }
		}

		protected override void TestExportLandedCostingValuesCore()
		{
			var invoiceLine = (JobComInvoiceLine)SetupLandedCostHistoryForInvoiceLine();

			var xmlInvoiceHeader = InvoiceDataAdapter.ExportToValueObject(invoiceLine.InvoiceHeader, new ValueObjectExportContext(new NotificationBuffer()));
			var xmlInvoiceLine = xmlInvoiceHeader.InvoiceLines[0];

			var landedCostingXml = xmlInvoiceLine.LandedCosting;
			AssertEquals("LandedCosting is specified", true, xmlInvoiceLine.LandedCosting.IsSpecified);
			AssertEquals("Invoice Line Type", "ACT", landedCostingXml.LineType);
			AssertEquals("Excise shouldn't be specified", false, landedCostingXml.Excise.IsSpecified);

			AsserteFinancialValue("Total Cost Per Unit", landedCostingXml.TotalCostPerUnit, 118.38m, JobDeclaration.LocalCurrencyConstantCode);
			AsserteFinancialValue("Unit Price in Local Currency", landedCostingXml.UnitPriceInLocalCurrency, 100m, JobDeclaration.LocalCurrencyConstantCode);
			AsserteFinancialValue("Duty And Taxes Per Unit", landedCostingXml.DutiesAndTaxesPerUnit, 3.38m, JobDeclaration.LocalCurrencyConstantCode);
			AsserteFinancialValue("Entry Fees", landedCostingXml.EntryFees, 20m, JobDeclaration.LocalCurrencyConstantCode);
			AsserteFinancialValue("Landing Cost Per Unit", landedCostingXml.LandingCostPerUnit, 15m, JobDeclaration.LocalCurrencyConstantCode);

			AssertEquals("selldetails is specified", true, landedCostingXml.SellDetails.IsSpecified);
			AssertEquals("lcGroupCharges is specified", true, landedCostingXml.LCGroupCharges.IsSpecified);
			AssertEquals("lcMisc is not specified", false, landedCostingXml.LCMisc.IsSpecified);

			//group charge
			Assert("Group 1", AssertGroupCharges(1, landedCostingXml.LCGroupCharges, 100m, shouldBeNull: false, JobDeclaration.LocalCurrencyConstantCode));
			Assert("Group 2", AssertGroupCharges(2, landedCostingXml.LCGroupCharges, 200m, shouldBeNull: false, JobDeclaration.LocalCurrencyConstantCode));
			Assert("Group 3", AssertGroupCharges(3, landedCostingXml.LCGroupCharges, 300m, shouldBeNull: false, JobDeclaration.LocalCurrencyConstantCode));
			Assert("Group 4", AssertGroupCharges(4, landedCostingXml.LCGroupCharges, 400m, shouldBeNull: false, JobDeclaration.LocalCurrencyConstantCode));
			Assert("Group 5", AssertGroupCharges(5, landedCostingXml.LCGroupCharges, 500m, shouldBeNull: false, JobDeclaration.LocalCurrencyConstantCode));
			Assert("Group 6", AssertGroupCharges(6, landedCostingXml.LCGroupCharges, 0m, shouldBeNull: true, JobDeclaration.LocalCurrencyConstantCode));

			//sell Details
			AssertEquals("Sell Details", true, AssertSellDetails(1, landedCostingXml.SellDetails, 127.85m, 147.03m, 8m, 8m, JobDeclaration.LocalCurrencyConstantCode));
			AssertEquals("Sell Details", true, AssertSellDetails(2, landedCostingXml.SellDetails, 129.03m, 148.38m, 9m, 9m, JobDeclaration.LocalCurrencyConstantCode));
			AssertEquals("Sell Details", true, AssertSellDetails(3, landedCostingXml.SellDetails, 130.22m, 149.75m, 10m, 10m, JobDeclaration.LocalCurrencyConstantCode));

			AssertEquals("Other Duty", false, landedCostingXml.OtherDuty.IsSpecified);

			//special Tax
			AssertEquals("Special Tax", true, AssertSpecialTax(1, landedCostingXml.SpecialTax, 5m, JobDeclaration.LocalCurrencyConstantCode));
			AssertEquals("Special Tax", true, AssertSpecialTax(2, landedCostingXml.SpecialTax, 6m, JobDeclaration.LocalCurrencyConstantCode));
			AssertEquals("Special Tax", true, AssertSpecialTax(3, landedCostingXml.SpecialTax, 7m, JobDeclaration.LocalCurrencyConstantCode));
		}

		protected override BaseJobComInvoiceHeader NewBusinessObject()
		{
			return JobDec.Invoices.AddNew();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(GetEmptyInvoiceHeader(), FileReader.ExtractEmbeddedResourceToFile(testFilesResourceLocation, TempDir.DirectoryName, "NZEmptyInvoice.xml"), ValidationKind.None, "Empty Invoice");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return GetEmptyBizObjSample();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(GetNZInvoiceHeaderWithTestData(), FileReader.ExtractEmbeddedResourceToFile(testFilesResourceLocation, TempDir.DirectoryName, "NZPopulatedInvoice.xml"), ValidationKind.Xsd | ValidationKind.FactorySave, "Fully Populated Invoice");
		}

		protected BaseJobComInvoiceHeader GetNZInvoiceHeaderWithTestData()
		{
			var invoiceHeader = (JobComInvoiceHeader)GetInvoiceHeaderWithTestData();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoiceHeader.Charges[0].J7_RX_NKCurrency = Core.Constants.CurrencyCodes.NewZealand;

			return invoiceHeader;
		}

		protected override ValueObjectDataAdapter<BaseJobComInvoiceHeader, Xsd.InvoiceHeader> GetNewBizObjXmlDataAdapter()
		{
			return new NZInvoiceValueObjectDataAdapter(GetJobDeclaration());
		}

		protected override ZString TariffNumber
		{
			get { return "7215.50.20.31K"; }
		}

		protected override void TearDown()
		{
			base.TearDown();

			tempDir?.Dispose();
		}

		protected override string TestingCountry
		{
			get { return Core.Constants.CountryCodes.NewZealand; }
		}

		protected override IValueObjectDataAdapter GetInvoiceValueObjectDataAdapter()
		{
			return new NZInvoiceValueObjectDataAdapter(JobDec);
		}

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}

		NZInvoiceValueObjectDataAdapter InvoiceDataAdapter
		{
			get { return (NZInvoiceValueObjectDataAdapter)invoiceDataAdapter; }
		}

		protected override BaseCusClassification CreateTestCusClassification()
		{
			var result = base.CreateTestCusClassification();

			result.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;

			return result;
		}

		TempDirectory TempDir => tempDir ?? (tempDir = new TempDirectory());
		TempDirectory tempDir;

		TestFileReader FileReader => fileReader ?? (fileReader = new TestFileReader(typeof(NZInvoiceValueObjectDataAdapterTest)));
		TestFileReader fileReader;

		const string testFilesResourceLocation = "Enterprise.Customs.NZ.Business.Test.DataImportExport.Xml.Testing";
	}
}
