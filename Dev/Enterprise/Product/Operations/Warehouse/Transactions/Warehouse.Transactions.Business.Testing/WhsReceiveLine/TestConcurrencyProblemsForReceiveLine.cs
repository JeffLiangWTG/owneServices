using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class TestConcurrencyProblemsForReceiveLine : TestCase
	{
		[UseSnapshotProtection]
		public void TestPutawayTransferID_NoInventory_TwoApplicationInstances()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Factory.Save();

			using (Db.DisposableActionForDbConnection())
			using (var connectionForSecondUser = Db.NewExtraConnectionToMainDb())
			{
				var secondFactory = new BusinessObjectFactory(connectionForSecondUser) { RefreshEnabled = false };
				var receiveLineInOtherFactory = secondFactory.Load<WhsReceiveLine>(receiveLine.PK);
				receiveLine.Delete();
				Factory.Save();
				AssertNoExceptionThrown("There should be no exceptions thrown when PutawayTransferID is invoked.", () => AssertEquals(ZString.Empty, receiveLineInOtherFactory.PutawayTransferID));
			}
		}

		#region Implementation

		#region Helpers

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;

		#endregion

		#region Factory

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		#endregion

		#endregion
	}
}
