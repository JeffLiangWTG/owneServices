using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsPickableDocketLineTriggerTestCase : WhsDocketLineTriggerTestCase
	{
		#region Test_PickableDocketTG_WhsDocketLine_TransactionAndPickedQtyIsCorrect

		[ExpectNoExceptions]
		public void Test_PickableDocket_TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Update()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateProductBOM(data.Part1, data.Part2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var docketLine = GetNewDocketLineForNewNonFinalisedDocket(data);
			var docket = (WhsPickableDocket)docketLine.Docket;
			var pick = Helper.CreatePickNew(docket);
			Factory.Save();
			AssertEquals("Precondition: Pickline sum is correct.", 1m, pick.GetAllPickLines().Sum(pl => pl.WZ_Units));

			// dodgy insert / modify adjustment line to ensure trigger fails
			docketLine.WE_TransactionQuantity = 0m;
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(ZConcurrencyCheckFailureException), $"TriggerLikelyConcurrencyError: {WarehouseErrorMessages.PreventTransactionAndPickQuantityDesyncTriggerID}", true), "When trying to over-pick Order or Work Order line the trigger should fail.");
		}

		#endregion

		#region CreatePick

		protected override WhsPick CreatePick(WhsWarehouse warehouse, WhsDocket docket)
		{
			var pick = Helper.CreatePickNew();
			pick.WP_WW_Whs = warehouse.PK;
			docket.WD_WP = pick.PK;
			docket.WD_DocketStatus = DocketStatus.Codes.Picking;
			return pick;
		}

		#endregion

		#region DoesCreateNewStock

		protected override bool DoesCreateNewStock => false;

		#endregion
	}
}
