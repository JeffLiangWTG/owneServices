using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.Business.US.Testing
{
	public class WhsInventoryValidationTest : WhsInventoryViewValidationTest
	{
		#region TestCheckWI_InDocketLineUnits_WithPerPackageQty

		public void TestCheckWI_InDocketLineUnits_WithPerPackageQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			inventory.PerPackageQty = 5m;
			AssertNoErrors("Precondition", inventory.WI_InDocketLineUnitsInfo);

			var expectedErrorMessage = "Units received must be divisible by Per Group Quantity.";
			inventory.WI_InDocketLineUnits = 8m;
			AssertHasError(inventory.WI_InDocketLineUnitsInfo, expectedErrorMessage);

			inventory.WI_InDocketLineUnits = 15m;
			AssertNoErrors(inventory.WI_InDocketLineUnitsInfo);

			inventory.WI_InDocketLineUnits = 13m;
			AssertHasError(inventory.WI_InDocketLineUnitsInfo, expectedErrorMessage);

			inventory.PerPackageQty = 0m;
			AssertNoErrors(inventory.WI_InDocketLineUnitsInfo);
		}

		#endregion

		#region TestCheckWI_SplitQuantity_MustBeDivisibleByPerPackageQtyAndMustSplitWholePackageGroup

		public void TestCheckWI_SplitQuantity_MustBeDivisibleByPerPackageQtyAndMustSplitWholePackageGroup()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, "123-1", "", 2m);
			AssertNoErrors("Precondition", inventory.WI_SplitQuantityInfo);

			inventory.WI_SplitQuantity = 3m;
			AssertHasError(inventory.WI_SplitQuantityInfo, "Split Quantity must be divisible by Per Group Quantity.");

			inventory.WI_SplitQuantity = 4m;
			AssertNoErrors(inventory.WI_SplitQuantityInfo);

			inventory.InDocketLine.WE_PackageGroupId = "ABC";
			inventory.Validation.ValidateWI_SplitQuantity();
			AssertHasError(inventory.WI_SplitQuantityInfo, "You must Split either all or none of the inventory when in a Package Group.");

			inventory.WI_SplitQuantity = 10m;
			AssertNoErrors(inventory.WI_SplitQuantityInfo);

			inventory.WI_SplitQuantity = 0m;
			AssertNoErrors(inventory.WI_SplitQuantityInfo);
		}

		#endregion

		#region Implementation

		protected override Environment.Business.Testing.WhsTestHelperFunctionsEnv GetNewTestHelperFunctions()
		{
			return new WhsTestHelperFunctionsUS(Factory);
		}

		protected new WhsTestHelperFunctionsUS Helper
		{
			get { return (WhsTestHelperFunctionsUS)base.Helper; }
		}

		#endregion
	}
}
