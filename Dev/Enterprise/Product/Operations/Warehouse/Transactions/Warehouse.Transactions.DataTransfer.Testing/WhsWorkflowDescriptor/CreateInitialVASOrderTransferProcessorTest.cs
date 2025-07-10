using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class CreateInitialVASOrderTransferProcessorTest : WhsTestCaseWithFactory
	{
		#region TestConstructor

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CreateInitialVASOrderTransferProcessor(null));
		}

		#endregion

		#region TestProcess

		public void TestProcess()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);

			var factorySavingWasHit = false;
			vasOrder.Factory.Saving += (f) => factorySavingWasHit = true;
			IProcessor processor = new CreateInitialVASOrderTransferProcessor(vasOrder);
			processor.Process(Notify);
			AssertEquals("Should have error.", "Save all changes before creating the Transfer to Service Area.\r\n", Notify.AsString);
			AssertNull("No Transfer should be created.", vasOrder.TransferIntoServiceArea);
			AssertEquals("Should not have attempted to save the factory because that would make the LWK defective.", false, factorySavingWasHit);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			Factory.Save();

			processor.Process(Notify);
			AssertNotNull("Should have created Transfer.", vasOrder.TransferIntoServiceArea);
			AssertEquals("Don't save during logSubscribers. This optimisation does nothing. NewsTransmitter will save regardless.", false, vasOrder.TransferIntoServiceArea.IsInDatabase);
		}

		#endregion
	}

	public class TestConcurrencyProblemsForCreateInitialVASOrderTransferProcessor : TestCase
	{
		[UseSnapshotProtection]
		public void TestProcess_ConcurrencyError_InitialTransferAlreadyCreated()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			var vasOrder = helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			factory.Save();

			vasOrder.Factory.Saving += (f) =>
			{
				using (Db.DisposableActionForDbConnection())
				using (var connectionForSecondUser = Db.NewExtraConnectionToMainDb())
				{
					var factory2 = new BusinessObjectFactory(connectionForSecondUser) { RefreshEnabled = false };
					var vasOrderInFactory2 = factory2.Load<WhsVASOrder>(vasOrder.PK);
					vasOrderInFactory2.GetOrCreateInitialTransfer(new TestNotificationBuffer());
					AssertNotNull("Should have created Transfer.", vasOrderInFactory2.TransferIntoServiceArea);
					factory2.Save();
				}
			};

			var processor = (IProcessor)new CreateInitialVASOrderTransferProcessor(vasOrder);
			var notify = new TestNotificationBuffer();
			AssertNoExceptionThrown(() => processor.Process(notify));
			AssertExceptionThrown(typeof(ZSaveConcurrencyException), factory.Save);
		}
	}
}
