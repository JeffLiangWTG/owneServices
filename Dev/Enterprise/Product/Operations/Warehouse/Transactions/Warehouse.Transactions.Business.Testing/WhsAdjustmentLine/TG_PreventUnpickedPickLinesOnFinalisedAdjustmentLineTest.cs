using System.Linq;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class TG_PreventUnpickedPickLinesOnFinalisedAdjustmentLineTest : TG_PreventUnpickedPickLinesOnFinalisedDocketLinesTest
	{
		public override void TestTrigger_FinaliseWhsDocketLineWithPickedPickline()
		{
			TestTrigger_FinaliseWhsDocketLineWithPickline(true);
		}

		[ExpectNoExceptions]
		public override void TestTrigger_FinaliseWhsDocketLineWithUnpickedPickline()
		{
			TestTrigger_FinaliseWhsDocketLineWithPickline(false);
		}

		void TestTrigger_FinaliseWhsDocketLineWithPickline(bool isPicked)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			var receiveLine = receive.Lines.Single();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "AD1", Notify);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, -5m, data.Whs1.DefaultLocation.WLV_LocationString, ZDateTimeOffset.Now);
			adjustmentLine.RunPreSaveValidation();  //	Create pickline

			Factory.Save();
			adjustmentLine.PickLines[0].WZ_PickedDateTime = isPicked ? ZDateTimeOffset.Today : ZDateTimeOffset.Empty;

			adjustmentLine.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised;
			adjustmentLine.WE_FinalisedDate = ZDateTimeOffset.Now;
			adjustment.WD_FinalisedDate = ZDateTimeOffset.Now;
			adjustment.WD_DocketStatus = DocketStatus.Codes.Finalised;
			AssertIsFinalisedPrecondition(adjustmentLine);
			AssertIsFinalisedPrecondition(adjustment);

			if (isPicked)
			{
				AssertNoExceptionThrown("Adjustment Line is finalised with a picked pickline. Should not throw exception", () => Factory.Save());
			}
			else
			{
				NUnit.Framework.Assert.That(() => Factory.Save(), CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WhsDocketLine.PreventUnPickedPickLinesOnFinalisedDocketLine), "Adjustment Line is finalised with an unpicked pickline. Should have thrown an exception");
			}
		}
	}
}
