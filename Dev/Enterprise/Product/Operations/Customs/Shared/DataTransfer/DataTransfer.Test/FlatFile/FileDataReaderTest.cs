using NUnit.Framework;

namespace Enterprise.Customs.DataTransfer.Testing
{
	public abstract class FileDataReaderTest : TransactionedTestCase
	{
		public void TestFileName()
		{
			var testDataReader = new FileDataReaderTestClass("FileName");
			AssertEquals("FileName", testDataReader.GetFileName());
		}

		public abstract void TestRecords();

		protected override void TearDown()
		{
			base.TearDown();
			testFileHelper?.Dispose();
			testFileHelper = null;
		}

		protected TestFileHelper TestFileHelper => testFileHelper ??= new ();
		TestFileHelper testFileHelper;

		protected class FileDataReaderTestClass : FileDataReader
		{
			public FileDataReaderTestClass(string fileName) : base(fileName)
			{
			}

			public override string[][] Records => null;

			public string GetFileName() => FileName;
		}
	}
}
