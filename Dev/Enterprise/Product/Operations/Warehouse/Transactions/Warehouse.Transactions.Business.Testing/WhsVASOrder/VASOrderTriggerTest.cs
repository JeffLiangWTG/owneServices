using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class VASOrderTriggerTest : TestCase
	{
		#region TestCreateInitialTransferTrigger

		[UseSnapshotProtection]
		public void TestCreateInitialTransferTrigger()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);
			var notify = new TestNotificationBuffer();
			var data = new TestDataSimpleEnvironment(factory);
			var serviceArea = helper.CreateServiceAreaForVASOrder(data.Whs1);
			serviceArea.PickLocations.Cast<WhsLocation>().Single().WLV_MaxQuantity = 1;
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.DefaultLocation, "");

			var vasOrder1 = helper.CreateWhsVASOrderWithLine(serviceArea, data.Org1, data.Part1, 1m);
			var vasOrder2 = helper.CreateWhsVASOrderWithLine(serviceArea, data.Org1, data.Part1, 1m);
			factory.Save();

			var transferForVasOrder1 = vasOrder1.GetOrCreateInitialTransfer(notify);
			AssertNotNull(transferForVasOrder1);

			using (var anotherConnection = Db.NewExtraConnectionToMainDb())
			{
				try
				{
					var anotherFactory = new BusinessObjectFactory(anotherConnection) { RefreshEnabled = false };
					var vasOrder2FromAnotherFactory = anotherFactory.Load<WhsVASOrder>(vasOrder2.PK);
					var transferForVasOrder2 = vasOrder2FromAnotherFactory.GetOrCreateInitialTransfer(notify);
					AssertNotNull(transferForVasOrder2);

					factory.Save(); // to get transfer for vasOrder1 into database

					((IDbConnected)anotherFactory).Connection.BeginTransaction();
					anotherFactory.Save();
					Assert("Save should be failed in trigger", false);
				}
				catch (ZSaveConcurrencyException ex)
				{
					AssertEquals("Exception message should be correct", true, ex.Message.Contains("Attempt to overflow the location unit capacity."));
				}
			}

			// remove the first VAS Order then ensure the second VAS Order can use the location
			transferForVasOrder1.Delete();
			factory.Save();

			using (var anotherConnection = Db.NewExtraConnectionToMainDb())
			{
				var anotherFactory = new BusinessObjectFactory(anotherConnection) { RefreshEnabled = false };
				var vasOrder2FromAnotherFactory = anotherFactory.Load<WhsVASOrder>(vasOrder2.PK);
				var transferForVasOrder2 = vasOrder2FromAnotherFactory.GetOrCreateInitialTransfer(notify);
				AssertNotNull("Transfer for second VAS order successfully created.", transferForVasOrder2);

				AssertNoExceptionThrown(() => anotherFactory.Save());
			}
		}

		#endregion
	}
}
