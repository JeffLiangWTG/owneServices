using System;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class CompleteAndFinaliseVASOrderProcessorTest : WhsTestCaseWithFactory
	{
		#region TestConstructor

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CompleteAndFinaliseVASOrderProcessor(null));
		}

		#endregion

		#region TestProcess

		public void TestProcess()
		{
			Notify.DefaultResponse = false; // make sure that we don't need to confirm finalise for processor to finalise
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);

			var factorySavingWasHit = false;
			vasOrder.Factory.Saving += (f) => factorySavingWasHit = true;
			IProcessor processor = new CompleteAndFinaliseVASOrderProcessor(vasOrder);
			processor.Process(Notify);
			AssertEquals("Should have error.", "Save all changes before Completing this VAS Order.\r\n", Notify.AsString);
			AssertEquals("VAS Order should not be finalised.", false, vasOrder.IsFinalised);
			AssertEquals("Should not have attempted to save the factory if finalisation failed.", false, factorySavingWasHit);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertEquals("Precondition: Initial transfer is transferring correct stock.", 1, initialTransfer.Lines.Count);

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);

			Factory.Save();
			AssertEquals("Precondition: Work Order is Not Complete or Finalised.", false, vasOrder.WVO_WorkCompletedTimeUtc.IsValid || vasOrder.IsFinalised);

			processor.Process(Notify);
			AssertEquals("Should have completed VAS Order.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			AssertEquals("Should have finalised VAS Order.", true, vasOrder.IsFinalised);
			AssertEquals("Should have saved VAS Order.", false, vasOrder.HasChanges);
		}

		#endregion

		#region TestProcess_WithCompletedVASOrder

		public void TestProcess_WithCompletedVASOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertEquals("Precondition: Initial transfer is transferring correct stock.", 1, initialTransfer.Lines.Count);

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);

			Factory.Save();
			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is Complete.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);

			bool factorySavingWasHit = false;
			vasOrder.Factory.Saving += (f) => factorySavingWasHit = true;
			IProcessor processor = new CompleteAndFinaliseVASOrderProcessor(vasOrder);
			processor.Process(Notify);
			AssertEquals("Should have error.", "Save all changes before Finalizing this VAS Order.\r\n", Notify.AsString);
			AssertEquals("VAS Order should not be finalised.", false, vasOrder.IsFinalised);
			AssertEquals("Should not have attempted to save the factory if finalisation failed.", false, factorySavingWasHit);

			Factory.Save();
			processor.Process(Notify);
			AssertEquals("Should have finalised VAS Order.", true, vasOrder.IsFinalised);
			AssertEquals("Should have saved VAS Order.", false, vasOrder.HasChanges);
		}

		#endregion

		#region TestProcess_WithSaveFailureOnCompletingVASOrder

		public void TestProcess_WithSaveFailureOnCompletingVASOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertEquals("Precondition: Initial transfer is transferring correct stock.", 1, initialTransfer.Lines.Count);

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);

			Factory.Save();
			Factory.Saved += delegate
			{ vasOrder.HasChanges = true; }; // replicate save failure by making hasChanges true after save.
			IProcessor processor = new CompleteAndFinaliseVASOrderProcessor(vasOrder);
			processor.Process(Notify);
			AssertEquals("Should not have finalised VAS Order.", false, vasOrder.IsFinalised);
		}

		#endregion
	}
}
