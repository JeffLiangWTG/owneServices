using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Testing
{
	public abstract class WhsDocketFlatFileDataImporterTest<TBusinessObject> : FlatFileDataImporterTestCase
		where TBusinessObject : WhsDocket
	{
		public void TestImport()
		{
			SetupData();
			Factory.Save();
			var notify = new NotificationBuffer();
			var dataHasBeenImported = Importer.ImportData(PathToTestFile, notify, SourceInfo.EmptySourceInfo);

			AssertEquals(dataHasBeenImported, true);
			AssertEquals("Notify should not have errors", true, !notify.HasErrors);
		}

		protected abstract void SetupData();

		#region Importer

		protected WhsDocketFlatFileDataImporter<TBusinessObject> Importer => importer ?? (importer = (WhsDocketFlatFileDataImporter<TBusinessObject>)GetDataImporter());
		WhsDocketFlatFileDataImporter<TBusinessObject> importer;

		#endregion

		#region Helper

		protected WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}