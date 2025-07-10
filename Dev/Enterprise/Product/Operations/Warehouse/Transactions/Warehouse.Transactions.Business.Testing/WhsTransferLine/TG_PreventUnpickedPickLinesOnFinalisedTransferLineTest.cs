using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class TG_PreventUnpickedPickLinesOnFinalisedTransferLineTest : TG_PreventUnpickedPickLinesOnFinalisedDocketLinesTest
	{
		[ExpectNoExceptions]
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
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, "A", "A");
			transferLine.RunPreSaveValidation();
			Factory.Save();

			transferLine.PickLines[0].WZ_PickedDateTime = isPicked ? ZDateTimeOffset.Today : ZDateTimeOffset.Empty;
			transferLine.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised;
			transferLine.WE_FinalisedDate = ZDateTimeOffset.Now;

			using (new SemaphoreManager(transfer.FinaliseDocketSemaphore))
			{
				transferLine.WE_PutawayTime = ZDateTimeOffset.Now;
				transferLine.WE_StockOnHand = 5m;
				transferLine.WE_AdjustmentArrivalDate = ZDateTimeOffset.Today;
				transfer.WD_DocketStatus = DocketStatus.Codes.Finalised;
				transfer.WD_FinalisedDate = ZDateTimeOffset.Now;
			}

			AssertIsFinalisedPrecondition(transferLine);

			if (isPicked)
			{
				AssertNoExceptionThrown("Transfer Line is finalised with a picked pickline. Should not throw exception", () => Factory.Save());
			}
			else
			{
				NUnit.Framework.Assert.That(() => Factory.Save(), CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WhsDocketLine.PreventUnPickedPickLinesOnFinalisedDocketLine), "Transfer Line is finalised with an unpicked pickline. Should have thrown an exception");
			}
		}
	}
}
