using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsPickableDocketLineConcurrencyTest : TG_WhsDocketLine_ConcurrencyTest
	{
		#region TestConcurrencySaveException_DeletingDeletedLineFromAnotherInstance

		[UseSnapshotProtection]
		public void TestConcurrencySaveException_DeletingDeletedLineFromAnotherInstance()
		{
			var factory1 = new BusinessObjectFactory();
			var data = new TestDataSimpleEnvironment(factory1);
			var helper = new WhsTestHelperFunctions(factory1);
			factory1.Save();

			var pickableDocket = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var pickableDocketLine = helper.CreateWhsPickableDocketLine(pickableDocket, data.Part1, 10m);
			factory1.Save();

			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				var factory2 = new BusinessObjectFactory(connection2) { RefreshEnabled = false };
				var pickableDocketLineInAnotherFactory = factory2.Load<WhsPickableDocketLine>(pickableDocketLine.PK);
				pickableDocketLineInAnotherFactory.Delete();
				factory2.Save();

				AssertEquals("Precondition: orderLineInAnotherFactory is deleted.", true, pickableDocketLineInAnotherFactory.IsDeleted);
				AssertEquals("Precondition: orderLine is not deleted.", false, pickableDocketLine.IsDeleted);
			}

			pickableDocketLine.Delete();

			try
			{
				factory1.Save();
			}
			catch (ZSaveException ex)
			{
				AssertEquals("Concurrency exception is thrown.", typeof(ZSaveConcurrencyException), ex.GetType());
				ZExceptionReporting.HandleSaveException(ex); // simulate the form handling the exception
				AssertEquals("No 'DeletedRowInaccessibleException' when concurrency save exception is handled.", null, ErrorReporter.LastExceptionReported);
				AssertEquals("No errors reported when concurrency save exception is handled.", 0, ErrorReporter.TotalErrorCount);
				ErrorReporter.Clear();
			}

			AssertNoExceptionThrown("No exceptions are thrown on the 2nd save attempt.", factory1.Save);
			AssertEquals("orderLine is deleted.", true, pickableDocketLine.IsDeleted);
		}

		#endregion
	}
}
