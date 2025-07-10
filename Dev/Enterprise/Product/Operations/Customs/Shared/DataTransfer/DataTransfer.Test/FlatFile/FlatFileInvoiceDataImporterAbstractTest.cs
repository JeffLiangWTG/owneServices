using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.DataTransfer.Testing
{
	public abstract class FlatFileInvoiceDataImporterAbstractTest : DeclarationDataImporterTest
	{
		public void TestTotalRecordCount()
		{
			FlatFileInvoiceDataImporter flatFileImporter = GetFlatFileInvoiceDataImporter(TestFileHelper.GetPathForTestFiles("DataImporterTestFile.csv"));
			flatFileImporter.Import();
			AssertEquals(11, flatFileImporter.ProcessedRecordCount);
		}

		public void TestFailedRecordCount()
		{
			FlatFileInvoiceDataImporter flatFileImporter = GetFlatFileInvoiceDataImporter(TestFileHelper.GetPathForTestFiles("DataImporterTestFile.csv"));
			flatFileImporter.Import();
			AssertEquals(1, flatFileImporter.FailedRecordCount);
		}

		public void TestProcessedRecordCount()
		{
			FlatFileInvoiceDataImporter flatFileImporter = GetFlatFileInvoiceDataImporter(TestFileHelper.GetPathForTestFiles("DataImporterTestFile.csv"));
			flatFileImporter.Import();
			AssertEquals(11, flatFileImporter.ProcessedRecordCount);
		}

		public override void TestImport()
		{
			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUPPLIER1";
			OrgHeader owner = Factory.New<OrgHeader>();
			owner.OH_Code = "IMPORTER1";

			FlatFileInvoiceDataImporter importer = GetFlatFileInvoiceDataImporter(TestFileHelper.GetPathForTestFiles("DataImporterTestFile.csv"));
			importer.Import();

			AssertEquals(2, declaration.Invoices.Count);
			AssertEquals(2, declaration.Invoices[0].Charges.Count);
			AssertEquals(6, declaration.InvoiceLines.Count);

			BaseJobComInvoiceHeader invHead = declaration.Invoices[0];
			AssertEquals("20409252041234567890123456789012345", invHead.JZ_InvoiceNumber);
			AssertEquals(new ZDateTime(2004, 09, 28, 12, 6, 14), invHead.JZ_InvoiceDate);
			AssertEquals(16111.95M, invHead.JZ_InvoiceAmount);
			AssertEquals("USD", invHead.JZ_RX_NKInvoice_Currency);
			AssertEquals("DDU", invHead.JZ_IncoTerm);
			AssertEquals(supplier.PK, invHead.JZ_OH_Supplier);
			AssertEquals(owner.PK, invHead.JZ_OH_Buyer);

			ZQuery filter = new ZQuery(JobComInvoiceLineSchema.JI_PartNo, "PRODCODE");
			BaseJobComInvoiceLine invLine = (BaseJobComInvoiceLine)declaration.InvoiceLines.Find(filter)[0];

			AssertEquals("20409252041234567890123456789012345", invLine.JI_Calc_Invoice);
			AssertEquals("CRTA81253506", invLine.JI_OrderNumber);
			AssertEquals("PRODCODE", invLine.JI_PartNo);
			AssertEquals("Attrib1", invLine.JI_PartAttrib1);
			AssertEquals("Attrib2", invLine.JI_PartAttrib2);
			AssertEquals("Attrib3", invLine.JI_PartAttrib3);
			AssertEquals("DESCRIPTION", invLine.JI_Description);
			AssertEquals(1M, invLine.JI_InvoiceQuantity);
			AssertEquals("PCE", invLine.JI_InvoiceUQ);
			AssertEquals(0.064M, invLine.JI_Volume);
			AssertEquals("M3", invLine.JI_VolumeUQ);
			AssertEquals(4M, invLine.JI_Weight);
			AssertEquals("KG", invLine.JI_WeightUQ);
			AssertEquals(1908.06M, invLine.JI_LinePrice);
			//Product Code Should override the tariff
			AssertEquals("", invLine.JI_Tariff);
			AssertEquals(1.92M, invLine.JI_CustomsQuantity);
			AssertEquals("KG", invLine.JI_CustomsUnitQty);
			AssertEquals("ZA", invLine.JI_CountryOfOrigin);
			AssertEquals("Concession", invLine.JI_ConcessionOrder);
			AssertEquals("CustomText1", invLine.JI_CustomAttrib1);
			AssertEquals("CustomText2", invLine.JI_CustomAttrib2);
			AssertEquals("CustomText3", invLine.JI_CustomAttrib3);
			AssertEquals(new ZDateTime(2004, 09, 28, 10, 6, 14), invLine.JI_CustomDate1);
			AssertEquals(new ZDateTime(2004, 09, 28, 11, 6, 14), invLine.JI_CustomDate2);
			AssertEquals(new ZDateTime(2004, 09, 28, 12, 6, 14), invLine.JI_CustomDate3);
			AssertEquals(true, invLine.JI_CustomFlag1);
			AssertEquals(false, invLine.JI_CustomFlag2);
			AssertEquals(true, invLine.JI_CustomFlag3);
			AssertEquals(1M, invLine.JI_CustomDecimal1);
			AssertEquals(2M, invLine.JI_CustomDecimal2);
			AssertEquals(3M, invLine.JI_CustomDecimal3);
		}

		public void TestImportWithoutHeader()
		{
			using (declaration.InvoiceLines.SuspendListChanged())
			{
				var importer = GetFlatFileInvoiceDataImporter(TestFileHelper.GetPathForTestFiles("DataImporterTestFileWithoutHeader.csv"));
				importer.Import();
			}
			AssertEquals(1, declaration.Invoices.Count);
			AssertEquals(3, declaration.InvoiceLines.Count);
			var invHead = declaration.Invoices[0];
			AssertEquals((short)1, invHead.JobComInvoiceLines[0].JI_LineNo);
			AssertEquals((short)2, invHead.JobComInvoiceLines[1].JI_LineNo);
			AssertEquals((short)3, invHead.JobComInvoiceLines[2].JI_LineNo);
		}

		public void TestImportCSVFileWithOutOfOrderChargesLine()
		{
			FlatFileInvoiceDataImporter importer = GetFlatFileInvoiceDataImporter(TestFileHelper.GetPathForTestFiles("DataImporterTestFileWithOutOfOrderChargesLine.csv"));
			importer.Import();

			AssertEquals(0, declaration.Invoices.Count);
			AssertEquals(0, declaration.InvoiceLines.Count);
			AssertEquals("File is not valid for import.", importer.LogEntry);
		}

		public void TestImportCSVFileWithChargesButNoHeaderLine()
		{
			FlatFileInvoiceDataImporter importer = GetFlatFileInvoiceDataImporter(TestFileHelper.GetPathForTestFiles("DataImporterTestFileWithChargesButNoHeaderLines.csv"));
			importer.Import();

			AssertEquals(0, declaration.Invoices.Count);
			AssertEquals(0, declaration.InvoiceLines.Count);
			AssertEquals("File is not valid for import.", importer.LogEntry);
		}

		public void TestImportCSVFileWithNoInvoiceHeaderLines()
		{
			FlatFileInvoiceDataImporter importer = GetFlatFileInvoiceDataImporter(TestFileHelper.GetPathForTestFiles("DataImporterTestFileWithNoInvoiceHeaderLines.csv"));
			importer.Import();

			AssertEquals(1, declaration.Invoices.Count);
			AssertEquals(3, declaration.InvoiceLines.Count);
		}

		public void TestImportFromEmptyCSVFile()
		{
			FlatFileInvoiceDataImporter importer = GetFlatFileInvoiceDataImporter(TestFileHelper.GetPathForTestFiles("EmptyTestFile.csv"));
			importer.Import();

			AssertEquals(0, declaration.Invoices.Count);
			AssertEquals(0, declaration.InvoiceLines.Count);
		}

		[ExpectNoExceptions]
		public void TestImportInvoicesWithIncompleteLines()
		{
			FlatFileInvoiceDataImporter importer = GetFlatFileInvoiceDataImporter(TestFileHelper.GetPathForTestFiles("TestFileWithIncompleteLines.csv"));
			importer.Import();

			AssertEquals(1, declaration.Invoices.Count);
			AssertEquals(4, declaration.InvoiceLines.Count);
		}

		public void TestImportXls()
		{
			FlatFileInvoiceDataImporter importer = GetFlatFileInvoiceDataImporter(TestFileHelper.GetPathForTestFiles("DataImporterTestFile.xls"));
			importer.Import();

			AssertEquals(2, declaration.Invoices.Count);
			AssertEquals(2, declaration.Invoices[0].Charges.Count);
			AssertEquals(6, declaration.InvoiceLines.Count);

			BaseJobComInvoiceHeader invHead = declaration.Invoices[0];
			AssertEquals("20409252041234567890123456789012345", invHead.JZ_InvoiceNumber);
			AssertEquals(new ZDateTime(2004, 09, 28, 12, 6, 14), invHead.JZ_InvoiceDate);
			AssertEquals(16111.95M, invHead.JZ_InvoiceAmount);
			AssertEquals("USD", invHead.JZ_RX_NKInvoice_Currency);
			AssertEquals("DDU", invHead.JZ_IncoTerm);

			ZQuery filter = new ZQuery(JobComInvoiceLineSchema.JI_PartNo, "PRODCODE");
			BaseJobComInvoiceLine invLine = (BaseJobComInvoiceLine)declaration.InvoiceLines.Find(filter)[0];

			AssertEquals("20409252041234567890123456789012345", invLine.JI_Calc_Invoice);
			AssertEquals("CRTA81253506", invLine.JI_OrderNumber);
			AssertEquals("PRODCODE", invLine.JI_PartNo);
			AssertEquals("Attrib1", invLine.JI_PartAttrib1);
			AssertEquals("Attrib2", invLine.JI_PartAttrib2);
			AssertEquals("Attrib3", invLine.JI_PartAttrib3);
			AssertEquals("DESCRIPTION", invLine.JI_Description);
			AssertEquals(1M, invLine.JI_InvoiceQuantity);
			AssertEquals("PCE", invLine.JI_InvoiceUQ);
			AssertEquals(0.064M, invLine.JI_Volume);
			AssertEquals("M3", invLine.JI_VolumeUQ);
			AssertEquals(4M, invLine.JI_Weight);
			AssertEquals("KG", invLine.JI_WeightUQ);
			AssertEquals(1908.06M, invLine.JI_LinePrice);
			//Product Code Should override the tariff
			AssertEquals("", invLine.JI_Tariff);
			AssertEquals(1.92M, invLine.JI_CustomsQuantity);
			AssertEquals("KG", invLine.JI_CustomsUnitQty);
			AssertEquals("ZA", invLine.JI_CountryOfOrigin);
			AssertEquals("Concession", invLine.JI_ConcessionOrder);
			AssertEquals("CustomText1", invLine.JI_CustomAttrib1);
			AssertEquals("CustomText2", invLine.JI_CustomAttrib2);
			AssertEquals("CustomText3", invLine.JI_CustomAttrib3);
			AssertEquals(new ZDateTime(2004, 09, 28, 10, 6, 14), invLine.JI_CustomDate1);
			AssertEquals(new ZDateTime(2004, 09, 28, 11, 6, 14), invLine.JI_CustomDate2);
			AssertEquals(new ZDateTime(2004, 09, 28, 12, 6, 14), invLine.JI_CustomDate3);
			AssertEquals(true, invLine.JI_CustomFlag1);
			AssertEquals(false, invLine.JI_CustomFlag2);
			AssertEquals(true, invLine.JI_CustomFlag3);
			AssertEquals(1M, invLine.JI_CustomDecimal1);
			AssertEquals(2M, invLine.JI_CustomDecimal2);
			AssertEquals(3M, invLine.JI_CustomDecimal3);
		}

		public void TestImportXLSFileWithOutOfOrderChargesLine()
		{
			FlatFileInvoiceDataImporter importer = GetFlatFileInvoiceDataImporter(TestFileHelper.GetPathForTestFiles("DataImporterTestFileWithOutOfOrderChargesLine.xls"));
			importer.Import();

			AssertEquals(0, declaration.Invoices.Count);
			AssertEquals(0, declaration.InvoiceLines.Count);
			AssertEquals("File is not valid for import.", importer.LogEntry);
		}

		public void TestImportXLSFileWithChargesButNoHeaderLine()
		{
			FlatFileInvoiceDataImporter importer = GetFlatFileInvoiceDataImporter(TestFileHelper.GetPathForTestFiles("DataImporterTestFileWithChargesButNoHeaderLines.xls"));
			importer.Import();

			AssertEquals(0, declaration.Invoices.Count);
			AssertEquals(0, declaration.InvoiceLines.Count);
			AssertEquals("File is not valid for import.", importer.LogEntry);
		}

		public void TestImportXSLFileWithNoInvoiceHeaderLines()
		{
			FlatFileInvoiceDataImporter importer = GetFlatFileInvoiceDataImporter(TestFileHelper.GetPathForTestFiles("DataImporterTestFileWithNoInvoiceHeaderLines.xls"));
			importer.Import();

			AssertEquals(1, declaration.Invoices.Count);
			AssertEquals(3, declaration.InvoiceLines.Count);
		}

		public void TestImportInvoicesFromXLSFileWithIncompleteLines()
		{
			FlatFileInvoiceDataImporter importer = GetFlatFileInvoiceDataImporter(TestFileHelper.GetPathForTestFiles("TestFileWithIncompleteLines.xls"));
			importer.Import();

			AssertEquals(1, declaration.Invoices.Count);
			AssertEquals(3, declaration.InvoiceLines.Count);
		}

		public void TestSettingTheProductCodeFirst()
		{
			var filter = new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.Equal, "AUD");
			var audCurrency = Factory.LoadTop1<RefCurrency>(filter);

			var supplierOrg = Factory.New<OrgHeader>();
			supplierOrg.OH_FullName = "FREAKCO";
			supplierOrg.MainAddress.OA_Address1 = "123 bat st";
			supplierOrg.OH_RL_NKClosestPort = "GBLHR";

			AssertEquals("SupplierOrg is Invalid", false, supplierOrg.HasErrors);

			var supplierPart = Factory.New<Business.OrgSupplierPart>();
			supplierPart.OP_PartNum = "H368020039";
			supplierPart.OP_Desc = "POONSTICK";
			supplierPart.OP_StockKeepingUnit = "M";
			supplierPart.OP_WeightUQ = "KG";
			supplierPart.RelatedOrganisations.AddOrganisationIfNotExist(supplierOrg.PK, OrgPartRelation.RelationshipTypes.Supplier);

			supplierPart.RunPreSaveValidation();
			AssertEquals("Precondition: SupplierPart.HasErrors", false, supplierPart.HasErrors);

			var jobDec = Factory.New<BaseJobDeclaration>();
			jobDec.JE_ApplicationCode = GetApplicationCode;
			jobDec.JE_TransportMode = TransportTypeList.Codes.Air;
			jobDec.JE_VoyageFlightNo = "AA001";
			jobDec.JE_MasterBill = ZString.Empty;

			jobDec.JE_ExportDate = ZDateTime.Today;
			jobDec.Transports[0].JW_IsLinked = false;

			var invHeader = jobDec.Invoices.AddNew();
			invHeader.JZ_InvoiceNumber = "99999";
			invHeader.JZ_RX_NKInvoice_Currency = audCurrency.RX_Code;
			invHeader.JZ_InvoiceAmount = 500;

			var invLine = invHeader.JobComInvoiceLines.AddNew();
			invLine.JI_LineNo = 1;
			invLine.JI_LinePrice = 500;
			invLine.JI_InvoiceUQ = "KG";

			jobDec.RunPreSaveValidation();

			AssertNoErrors(jobDec);

			invLine.JI_CountryOfOrigin = "US";

			Factory.Save();

			jobDec.JE_OH_Supplier = supplierOrg.PK;
			invLine.JI_PartNo = supplierPart.OP_PartNum;

			jobDec.RunPreSaveValidation();
			AssertNoErrors(jobDec);

			AssertEquals("The origin of the invoice line should not be the RefCountry from the supplier", "US", invLine.JI_CountryOfOrigin);
		}

		public void TestGenerateInvoiceNumberIfNoneSpecified()
		{
			string file = TestFileHelper.GetPathForTestFiles("DataImporterTestFileWithNoInvoiceNumber.csv");
			FlatFileInvoiceDataImporter importer = GetFlatFileInvoiceDataImporter(file);
			importer.Import();

			importer = GetFlatFileInvoiceDataImporter(file);
			importer.Import();

			AssertEquals("Incorrect amount of processed lines", 6, importer.TotalRecordCount);

			AssertEquals("Invoice number should be EDIIMPORTED1", "EDIIMPORTED1", declaration.Invoices[0].JZ_InvoiceNumber);
			AssertEquals("Invoice number should be EDIIMPORTED1", "EDIIMPORTED2", declaration.Invoices[1].JZ_InvoiceNumber);

			AssertEquals("PreCondition:Customs UQ", "PCE", declaration.Invoices[0].JobComInvoiceLines[0].JI_CustomsUnitQty);
			AssertEquals("Customs Quantity is defaulted from Invoice Qty if UQs are the same", 1m, declaration.Invoices[0].JobComInvoiceLines[0].JI_InvoiceQuantity);
			AssertEquals("Customs Quantity is defaulted from Invoice Qty if UQs are the same", 1m, declaration.Invoices[0].JobComInvoiceLines[0].JI_CustomsQuantity);
		}

		public void TestNotSupportMultipleInvoiceHeadersLog()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var importerMock = new Mock<FlatFileInvoiceDataImporter>(new object[] { TestFileHelper.GetPathForTestFiles("DataImporterTestFile.csv"), declaration });
			importerMock.CallBase = true;
			importerMock.Protected().Setup<bool>("SupportMultipleInvoiceHeaders").Returns(false);
			var importer = importerMock.Object;
			var logs = new ZStringBuilder();
			importer.LogEvent += (sender, args) => logs.AppendIfNotEmpty(((FlatFileInvoiceDataImporter)sender).LogEntry);
			importer.Import();
			AssertMultilineASCIIEquals("Log", "Multiple invoice header lines found. Only one invoice header line is allowed.\r\nFile is not valid for import.", logs.ToStringWithNewLineBetweenAppends());
			importerMock.VerifyAll();
		}

		public void TestCalculateAmountCorrectlyWhenImportInvoicesMultipleTimes()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var importer = new FlatFileInvoiceDataImporter(TestFileHelper.GetPathForTestFiles("DataImporterTestFileWithNoInvoiceHeaderLines.csv"), declaration);
			importer.Import();
			AssertEquals(1, declaration.Invoices.Count);
			AssertEquals(10387.77m, declaration.Invoices[0].JZ_InvoiceAmount);

			importer.Import();
			AssertEquals(1, declaration.Invoices.Count);
			AssertEquals(20775.54m, declaration.Invoices[0].JZ_InvoiceAmount);
		}

		protected virtual FlatFileInvoiceDataImporter GetFlatFileInvoiceDataImporter(ZString sourceFile) => new FlatFileInvoiceDataImporter(sourceFile, declaration);

		protected virtual ZString GetApplicationCode => "CMR";

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
