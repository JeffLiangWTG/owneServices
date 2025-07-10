using CargoWise.EntityFramework.Testing;
using Enterprise.Billing.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.DataTransfer.Testing
{
	sealed class InvoiceDataTransferImporterTest : TestCaseWithFactory
	{
		public void TestImport()
		{
			string filename = TestFileHelper.GetPathForTesting("EmptyInvoice.xml");

			AssertNoExceptionThrown(() => new InvoiceDataTransferImporter(StandAloneInvoiceValueObjectDataAdapter.New(), false).Import(filename, new NotificationBuffer(), SourceInfo.EmptySourceInfo));
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
