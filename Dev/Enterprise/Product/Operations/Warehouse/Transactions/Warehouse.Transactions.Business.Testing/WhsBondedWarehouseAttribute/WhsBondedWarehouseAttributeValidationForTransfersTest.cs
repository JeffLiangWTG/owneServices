using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsBondedWarehouseAttributeValidationForTransfersTest : WhsBondedWarehouseAttributeValidationTest
	{
		#region TestCheckWB_EntryKey

		protected override bool CheckEntryNumber
		{
			get { return false; }
		}

		#endregion

		#region Implementation

		protected override WhsDocketLine GetDocketLineParent(TestDataSimpleEnvironment data, WhsWarehouse whsOverride = null)
		{
			var whs = whsOverride ?? data.Whs1;
			var receiveLine = base.GetDocketLineParent(data, whs); // create inventory
			var receive = (WhsReceive)receiveLine.Docket;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receiveLine);

			var transfer = Helper.CreateWhsTransfer(data.Org1, whs);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, whs.DefaultLocation, whs.DefaultLocation);
			transferLine.CustomsData.WB_EntryKey = "EntryKey";
			transferLine.RunPreSaveValidation(); // to commit inventory

			return transferLine;
		}

		#endregion
	}
}
