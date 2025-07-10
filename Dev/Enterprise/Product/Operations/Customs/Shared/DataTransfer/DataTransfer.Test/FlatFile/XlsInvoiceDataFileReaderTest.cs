namespace Enterprise.Customs.DataTransfer.Testing
{
	sealed class XlsInvoiceDataFileReaderTest : FileDataReaderTest
	{
		public override void TestRecords()
		{
			var dataFileReader = new XlsInvoiceDataFileReader(TestFileHelper.GetPathForTestFiles("DataImporterTestFile.xls"));
			AssertEquals(10, dataFileReader.Records.Length);
		}
	}
}
