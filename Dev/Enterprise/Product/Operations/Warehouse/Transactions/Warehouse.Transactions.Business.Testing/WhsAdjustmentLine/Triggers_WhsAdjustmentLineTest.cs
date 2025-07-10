using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class Triggers_WhsAdjustmentLineTest : WhsTestCaseWithFactory
	{
		#region TestTG_WhsDocketLine_AdjustmentInHaveNoPickLines

		public void TestTG_WhsDocketLine_AdjustmentInHaveNoPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			var receiveLine = receive.Lines.Single();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "AD1", Notify);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, -5m, data.Whs1.DefaultLocation.WLV_LocationString, ZDateTimeOffset.Now);
			adjustment.FinaliseDocket();
			AssertIsFinalisedPrecondition(adjustment);
			Factory.Save();

			adjustmentLine.WE_TransactionQuantity = 5m;
			adjustmentLine.WE_WE_OriginalDocketLineForRating = adjustmentLine.PK;
			var exceptionThrown = false;
			try
			{
				Factory.Save();
			}
			catch (ZSaveException e)
			{
				AssertEquals(WhsDocketLine.PreventAdjustmentInLinesWithPickLinesTriggerID, e.InnerException.InnerException.Message);
				exceptionThrown = true;
			}

			AssertEquals("Trigger should have prevented save.", true, exceptionThrown);
		}

		#endregion
	}
}
