using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsReceiveLineValidationStrategyTestCase : WhsTestCaseWithFactory
	{
		#region TestConstructor

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => GetNewStrategy(null));
		}

		#endregion

		#region TestCheckPalletIDAssignedToPutawayTransfer_NotRunWhenStockOnHandIsZero

		public void TestCheckWE_PalletID_CheckPalletIDAssignedToPutawayTransfer_NotRunWhenStockOnHandIsZero()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocation = data.Whs1.FindLocation("DOCKDOOR");
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");
			var palletID = "PLT-123";
			var picker = Helper.CreateGlbStaff("RSL", "Russell");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, dockDoorLocation, palletID);
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, dockDoorLocation, nonDockDoorLocation, palletID, 100m);
			putawayTransferLine.RunPreSaveValidation();
			putawayTransferLine.GS_NKPickedBy = picker.GS_Code;
			putawayTransferLine.PickedTime = ZDateTimeOffset.Now;

			putawayTransfer.FinaliseDocket();
			Factory.Save();

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 0m, null, palletID);
			AssertEquals("Precondition: SOH should be 0", ZDecimal.Zero, receiveLine2.WE_StockOnHand);

			receiveLine2.ReceiveLineValidationStrategy.CheckPalletIDAssignedToPutawayTransfer(receiveLine2);
			AssertNoErrors("The receipt should NOT have any errors since this line has no SOH.", receiveLine2.WE_PalletIDInfo);
		}

		#endregion

		#region TestCheckWE_PalletID_CheckPalletIDAssignedToPutawayTransfer_Performance

		public void TestCheckWE_PalletID_CheckPalletIDAssignedToPutawayTransfer_Performance()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocation = data.Whs1.FindLocation("DOCKDOOR");
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");
			var palletID1 = "PLT-123";
			var palletID2 = "PLT-456";
			var amountOfLines = 50;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			for (var i = 0; i < amountOfLines; i++)
			{
				Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, dockDoorLocation, (i % 2) == 0 ? palletID1 : palletID2);
			}
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			putawayTransfer.WD_IsPutawayTransfer = true;

			for (var i = 0; i < amountOfLines; i++)
			{
				var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, dockDoorLocation, nonDockDoorLocation, (i % 2) == 0 ? palletID1 : palletID2, 1m);
				putawayTransferLine.RunPreSaveValidation();
			}

			var palletID3 = "PLT-789";
			var palletID4 = "PLT-582";
			for (var i = 0; i < amountOfLines; i++)
			{
				Helper.CreateWhsReceiveLine(receive, data.Part1, 3m, dockDoorLocation, (i % 2) == 0 ? palletID3 : palletID4);
			}
			Factory.Save();

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);

			BusinessObjectFactory.StartLogging();
			receiveInOtherFactory.RunPreSaveValidation();
			var loadLog = BusinessObjectFactory.DebugLog;
			BusinessObjectFactory.StopLogging();

			var logCountDocketLine = Regex.Matches(loadLog, Regex.Escape("LOAD Enterprise.Warehouse.Transactions.Business.WhsDocketLine")).Count;
			var logCountPickLine = Regex.Matches(loadLog, Regex.Escape("LOAD Enterprise.Warehouse.Transactions.Business.WhsPickLine")).Count;
			CombineAssertions(() =>
			{
				AssertEquals("Should load minimum WhsDocketLines.", 454, logCountDocketLine);
				AssertEquals("Should load minimum WhsPickLines.", 350, logCountPickLine);
			});
		}

		#endregion

		#region Implementation

		protected virtual WhsReceiveLineValidationStrategy GetNewStrategy(WhsReceiveLine inventoryLine)
		{
			return new WhsReceiveLineValidationStrategy(inventoryLine);
		}

		#endregion
	}
}
