using System;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class CreateReturnVASOrderTransferProcessorTest : WhsTestCaseWithFactory
	{
		#region TestConstructor

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CreateReturnVASOrderTransferProcessor(null));
		}

		#endregion

		#region TestProcess

		public void TestProcess()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);

			var factorySavingWasHit = false;
			vasOrder.Factory.Saving += (f) => factorySavingWasHit = true;
			IProcessor processor = new CreateReturnVASOrderTransferProcessor(vasOrder);
			processor.Process(Notify);
			AssertEquals("Should have error.", "Save all changes before creating the Transfer out of the Service Area.\r\n", Notify.AsString);
			AssertNull("No Transfer should be created.", vasOrder.TransferOutOfServiceArea);
			AssertEquals("Should not have attempted to save the factory if creation of Transfer failed.", false, factorySavingWasHit);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertEquals("Precondition: Initial transfer is transferring correct stock.", 1, initialTransfer.Lines.Count);

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);

			Factory.Save();
			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is complete.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);

			Factory.Save();

			using (Helper.GetPutawayEngineManagerForVASTransferLineMock())
			{
				processor.Process(Notify);
			}
			AssertNotNull("Should have created return Transfer.", vasOrder.TransferOutOfServiceArea);
			AssertEquals("Don't save in LogSubscribers...", false, vasOrder.TransferOutOfServiceArea.IsInDatabase);
		}

		#endregion
	}
}
