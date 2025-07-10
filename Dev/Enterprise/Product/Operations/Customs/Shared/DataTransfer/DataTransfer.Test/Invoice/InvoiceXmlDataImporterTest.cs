using CargoWise.EntityFramework.Testing;
using Enterprise.Billing.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.DataTransfer.Testing
{
	sealed class InvoiceXmlDataImporterTest : TestCaseWithFactory
	{
		public void TestImporter()
		{
			var adapter = StandAloneInvoiceValueObjectDataAdapter.New();

			string filename = TestFileHelper.GetPathForTesting("EmptyInvoice.xml");

			AssertNoExceptionThrown(() => new InvoiceXmlDataImporter(adapter).ImportData(filename, new NotificationBuffer(), SourceInfo.EmptySourceInfo));
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
