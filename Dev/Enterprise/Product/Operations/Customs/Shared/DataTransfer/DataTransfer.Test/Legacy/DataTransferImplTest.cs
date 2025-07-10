using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.DataTransfer.Testing
{
	public class DataTransferImplTest : TestCaseWithFactory
	{
		public void TestImportFileNotExist()
		{
			BaseJobDeclaration jobDec = Factory.New<BaseJobDeclaration>();
			DataTransferImpl dataTransferImpl = new DataTransferImpl();
			bool result = dataTransferImpl.ImportInvoices(jobDec, $@"c:\{Guid.NewGuid().ToString("N")}");

			AssertEquals(0, jobDec.Invoices.Count);
			AssertEquals(0, jobDec.InvoiceLines.Count);
			AssertEquals(false, result);
		}

		public void TestHandleImport_ArgumentException()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var mock = new Mock<DataTransferImpl>();
			mock
				.Protected()
				.Setup("GetFlatFileImporter", ItExpr.IsAny<string>(), ItExpr.IsAny<BaseJobDeclaration>())
				.Throws(new ArgumentException("HELLO WORLD"));
			var dataTransferImpl = mock.Object;
			var fileName = TestFileHelper.GetPathForTestFiles("DataImporterTestFile.csv");
			AssertEquals(false, dataTransferImpl.ImportInvoices(dec, fileName));
			AssertMultilineASCIIEquals("ErrorMessage", string.Format(@"Error processing file ({0}).
Exception message:HELLO WORLD", fileName), dataTransferImpl.ErrorMessage);
			mock.VerifyAll();
		}

		public void TestImportFromEmptyXLSFile()
		{
			BaseJobDeclaration toJobDec = Factory.New<BaseJobDeclaration>();
			DataTransferImpl dataTransferImpl = new DataTransferImpl();
			bool result = dataTransferImpl.ImportInvoices(toJobDec, TestFileHelper.GetPathForTestFiles("EmptyTestFile.xls"));

			AssertEquals(0, toJobDec.Invoices.Count);
			AssertEquals(0, toJobDec.InvoiceLines.Count);
			AssertEquals(true, result);
		}

		public void TestImportFromCsv()
		{
			BaseJobDeclaration toJobDec = Factory.New<BaseJobDeclaration>();
			DataTransferImpl dataTransferImpl = new DataTransferImpl();
			bool result = dataTransferImpl.ImportInvoices(toJobDec, TestFileHelper.GetPathForTestFiles("DataImporterTestFile.csv"));

			AssertEquals(2, toJobDec.Invoices.Count);
			AssertEquals(2, toJobDec.Invoices[0].Charges.Count);
			AssertEquals(6, toJobDec.InvoiceLines.Count);
			AssertEquals(true, result);
		}

		public void TestImportFromXls()
		{
			BaseJobDeclaration toJobDec = Factory.New<BaseJobDeclaration>();
			DataTransferImpl dataTransferImpl = new DataTransferImpl();
			bool result = dataTransferImpl.ImportInvoices(toJobDec, TestFileHelper.GetPathForTestFiles("DataImporterTestFile.xls"));

			AssertEquals(2, toJobDec.Invoices.Count);
			AssertEquals(2, toJobDec.Invoices[0].Charges.Count);
			AssertEquals(6, toJobDec.InvoiceLines.Count);
			AssertEquals(true, result);
		}

		public void TestImportFromXml()
		{
			BaseJobDeclaration toJobDec = Factory.New<BaseJobDeclaration>();
			DataTransferImpl dataTransferImpl = new DataTransferImpl();
			bool result = dataTransferImpl.ImportInvoices(toJobDec, TestFileHelper.GetPathForResourceName("CommercialInvoice.xml"));

			AssertEquals(1, toJobDec.Invoices.Count);
			AssertEquals(2, toJobDec.Invoices[0].Charges.Count);
			AssertEquals(2, toJobDec.InvoiceLines.Count);
			AssertEquals(true, result);
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
