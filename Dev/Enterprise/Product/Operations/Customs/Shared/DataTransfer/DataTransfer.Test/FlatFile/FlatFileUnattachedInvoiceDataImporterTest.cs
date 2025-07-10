using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DataTransfer.Testing
{
	sealed class FlatFileUnattachedInvoiceDataImporterTest : TestCaseWithFactory
	{
		public void TestImport()
		{
			TestCaseHelper.ClearTable(BaseJobComInvoiceHeader.Schema.TableName);
			TestCaseHelper.ClearTable(BaseInvoiceCharge.Schema.TableName);
			TestCaseHelper.ClearTable(BaseJobComInvoiceLine.Schema.TableName);

			ZString fileName = TestFileHelper.GetPathForTestFiles("DataImporterTestFile.csv");
			importer = new FlatFileUnattachedInvoiceDataImporter(fileName);
			importer.Import();

			AssertEquals("Should be 2 Invoices saved", 2, Factory.GetDatabaseCount(typeof(BaseJobComInvoiceHeader)));
			AssertEquals("Should be 2 Charges", 2, Factory.GetDatabaseCount(typeof(BaseInvoiceCharge)));
			AssertEquals("Should be 6 Invoice Lines", 6, Factory.GetDatabaseCount(typeof(BaseJobComInvoiceLine)));
		}

		public void TestCS00182123()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "OMRONKYO";

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "OMRELEJMH";

			var product = Factory.New<Business.OrgSupplierPart>();
			product.OP_StockKeepingUnit = "PCE";
			product.OP_PartNum = "A6H 0031F";
			product.OP_Desc = "TACTILE SWITCH";

			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_Relationship = "OWN";
			relation.OU_OH = importer.PK;

			var classification = Factory.New<BaseCusClassification>();
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			classification.CC_TariffNum = "8600783474";
			classification.CC_LookupCode = "TACTILE SWITCH";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_CC = classification.PK;

			Factory.Save();

			var fileName = TestFileHelper.GetPathForTestFiles("683_20120703172421.CSV");
			this.importer = new FlatFileUnattachedInvoiceDataImporter(fileName);
			this.importer.Import();

			var invoice = Factory.LoadTop1<BaseJobComInvoiceHeader>(new ZQuery(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, "C22178JB"));
			AssertNotNull(invoice);

			AssertEquals("IMP", invoice.JZ_StandAloneInvoiceDirection);
			AssertEquals("One invoice line", 1, invoice.JobComInvoiceLines.Count);

			var invoiceLine = invoice.JobComInvoiceLines[0];
			AssertEquals("JI_OP", product.PK, invoiceLine.JI_OP);
			AssertEquals("Tariff should be set from classification, rather than from a csv file.", "8600783474", invoiceLine.JI_Tariff);
			AssertEquals("Country of origin", "CN", invoiceLine.JI_CountryOfOrigin);
		}

		public void TestImportEmptyFile()
		{
			TestCaseHelper.ClearTable(BaseJobComInvoiceHeader.Schema.TableName);
			TestCaseHelper.ClearTable(BaseInvoiceCharge.Schema.TableName);
			TestCaseHelper.ClearTable(BaseJobComInvoiceLine.Schema.TableName);

			ZString fileName = TestFileHelper.GetPathForTestFiles("EmptyTestFile.csv");
			importer = new FlatFileUnattachedInvoiceDataImporter(fileName);
			importer.Import();

			AssertEquals("Should be 0 Invoices saved", 0, Factory.GetDatabaseCount(typeof(BaseJobComInvoiceHeader)));
			AssertEquals("Should be 0 Charges", 0, Factory.GetDatabaseCount(typeof(BaseInvoiceCharge)));
			AssertEquals("Should be 0 Invoice Lines", 0, Factory.GetDatabaseCount(typeof(BaseJobComInvoiceLine)));
		}

		FlatFileUnattachedInvoiceDataImporter importer;
		protected override void SetUp()
		{
			base.SetUp();
			importer = new FlatFileUnattachedInvoiceDataImporter("NameOfAFile.csv");
		}

		protected override void TearDown()
		{
			base.TearDown();
			testFileHelper?.Dispose();
			testFileHelper = null;
		}

		TestFileHelper TestFileHelper => testFileHelper ??= new ();
		TestFileHelper testFileHelper;
	}
}
