namespace Enterprise.Customs.DataTransfer.Testing
{
	sealed class CsvInvoiceDataFileReaderTest : FileDataReaderTest
	{
		public override void TestRecords()
		{
			var dataFileReader = new CsvInvoiceDataFileReader(TestFileHelper.GetPathForTestFiles("DataImporterTestFile.csv"));
			AssertEquals(11, dataFileReader.Records.Length);
		}
	}
}
