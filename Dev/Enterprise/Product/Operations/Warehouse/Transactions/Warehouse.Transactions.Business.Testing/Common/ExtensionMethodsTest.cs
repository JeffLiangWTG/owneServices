using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.Business.Common
{
	class ExtensionMethodsTest : TestCaseWithFactory
	{
		#region TestSynchroniseCachedBusinessObjectsWithDB_PickedPickLine

		public void TestSynchroniseCachedBusinessObjectsWithDB_PickedPickLine()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = helper.CreateWhsTransferLine(transfer, data.Part1, 15m, data.Whs1.DefaultLocation, data.Whs1.DefaultLocation);
			transferLine.RunPreSaveValidation();

			var pickLine = transferLine.PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pickLine);

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var pickLineInOtherFactory = otherFactory.Load<WhsPickLine>(pickLine.PK);
			var clonedPickLine = pickLineInOtherFactory.Clone(); // the transfer line must always have the correct pick line sum.

			using (((IWhsPickLineInternals)pickLineInOtherFactory).TemporarilyAllowUnpickingPickLineWithNoStockChange_DoNotUse())
			{
				pickLineInOtherFactory.Delete();
				AssertEquals("PickLine was deleted in other Factory.", true, pickLineInOtherFactory.IsDeleted);
			}

			otherFactory.Save();

			AssertNoExceptionThrown(() => Factory.SynchroniseCachedBusinessObjectsWithDB<WhsPickLine>(false, false));
			AssertEquals("PickLine was deleted.", true, pickLine.IsDeleted);
		}

		#endregion

		#region TestSynchroniseCachedBusinessObjectsWithDB_InTransitPickLine

		public void TestSynchroniseCachedBusinessObjectsWithDB_InTransitPickLine()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 15m);
			helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();
			AssertEquals("Precondition: PickLine is finalised.", true, pickLine.IsPickedFromPutawayLocation);

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var pickLineInOtherFactory = otherFactory.Load<WhsPickLine>(pickLine.PK);

			pickLineInOtherFactory.WZ_WE_OriginalPickedInventoryLine = ZGuid.Empty;
			pickLineInOtherFactory.Delete();
			AssertEquals("PickLine was deleted in other Factory.", true, pickLineInOtherFactory.IsDeleted);

			otherFactory.Save();

			AssertNoExceptionThrown(() => Factory.SynchroniseCachedBusinessObjectsWithDB<WhsPickLine>(false, false));
			AssertEquals("PickLine was deleted.", true, pickLine.IsDeleted);
		}

		#endregion

		#region TestReplaceOverflowExceptionWithRowError

		public void TestReplaceOverflowExceptionWithRowError()
		{
			var expectedErrorMessage = "Attempt to overflow capacity of TestProperty. Please validate your setup and restart the process.";

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.ReplaceOverflowExceptionWithRowError("TestProperty", () => throw new OverflowException("Bla"));
			AssertEquals("Should have only 1 row error.", 1, dummy.RowErrors.Count());
			AssertHasRowError("Should catch and wrap Overflow Exceptions.", dummy, expectedErrorMessage);

			dummy.ReplaceOverflowExceptionWithRowError("TestProperty", () => throw new OverflowException("Bla 123"));
			AssertEquals("Should not add same row error multiple times.", 1, dummy.RowErrors.Count());
			AssertHasRowError("Should catch and wrap Overflow Exceptions.", dummy, expectedErrorMessage);

			AssertExceptionThrown("Should only not catch Argument Exceptions.", typeof(ArgumentException), () => dummy.ReplaceOverflowExceptionWithRowError("TestProperty", () => throw new ArgumentException("Bla")));
		}

		#endregion
	}
}
