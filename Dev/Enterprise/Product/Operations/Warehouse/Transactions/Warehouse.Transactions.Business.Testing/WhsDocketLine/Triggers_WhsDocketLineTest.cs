using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class Triggers_WhsDocketLineTest : WhsTestCaseWithFactory
	{
		#region TestTG_WhsDocketLine_StockOnHandIsBalanced

		[ExpectNoExceptions]
		public void TestTG_WhsDocketLine_StockOnHandIsBalanced_InsertUpdate()
		{
			TestTG_WhsDocketLine_StockOnHandIsBalancedCore("Ensure that SOH balance keeping trigger was fired on Insert / Update.", (receive) =>
			{
				var receiveLine = receive.Lines[0];
				receiveLine.WE_StockOnHand -= 1m;
			});
		}

		[ExpectNoExceptions]
		public void TestTG_WhsDocketLine_StockOnHandIsBalanced_InsertUpdate_Parent()
		{
			TestTG_WhsDocketLine_StockOnHandIsBalancedCore("Ensure that SOH balance keeping trigger was fired on Insert / Update of Parent.", (receive) =>
			{
				var receiveLine = receive.Lines[0];
				receiveLine.HeldCodeChangeQuantity = 3m;
				receiveLine.HeldCodeToChangeTo = "";
				receiveLine.ChangeInventoryHeldCode(true);
				Factory.Save();

				var heldLine = Factory.LoadTop1<WhsReceiveLine>(new ZQuery(WhsDocketLineSchema.WE_WE_ParentDocketLine, receiveLine.PK));
				heldLine.WE_IsOriginalInventory = true;
				heldLine.WE_WE_ParentDocketLine = ZGuid.Empty;
				heldLine.WE_WE_OriginalDocketLineForRating = heldLine.PK;
			});
		}

		[ExpectNoExceptions]
		public void TestTG_WhsDocketLine_StockOnHandIsBalanced_Delete()
		{
			WhsTestHelperFunctions.DisableTrigger("TG_WhsInventoryHoldChangeLog_PreventDelete", WhsInventoryHoldChangeLogSchema.Constants.TableName);

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var receiveLine = receive.Lines[0];
			receiveLine.HeldCodeChangeQuantity = 3m;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var holdCodeChangeLine = Factory.LoadTop1<WhsReceiveLine>(new ZQuery(WhsDocketLineSchema.WE_WE_ParentDocketLine, receiveLine.PK));
			var holdCodeLogLine = Factory.LoadTop1<WhsInventoryHoldChangeLog>(new ZQuery(WhsInventoryHoldChangeLogSchema.WHL_WE_ParentDocketLine, holdCodeChangeLine.PK));
			((IBusinessObjectInternals)holdCodeLogLine).Row.Delete();
			holdCodeLogLine.HasChanges = true;
			holdCodeChangeLine.Delete();

			var assertionMessage = "Ensure that SOH balance keeping trigger was fired on Insert / Update.";
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Attempt to delete Hold Code Change Line.", true), assertionMessage);
		}

		void TestTG_WhsDocketLine_StockOnHandIsBalancedCore(string errorMessage, Action<WhsReceive> modifyDocketLineToFailTrigger)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			modifyDocketLineToFailTrigger(receive);
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(ZConcurrencyCheckFailureException), $"TriggerLikelyConcurrencyError: {WhsPickLine.PreventAttemptToPutStockOnHandOutOfBalance}", true), errorMessage);
		}

		#endregion
	}
}
