using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.Business.US.Testing
{
	class WhsStocktakeLineValidationForManuallyAddedLinesUSTest : WhsStocktakeLineForManuallyAddedLinesTestCase
	{
		#region TestCheckWU_PackageGroupId_MustBeUniqueAcrossCurrentStock

		//#warning
		// Not sure we actually need this
		//public void TestCheckWU_PackageGroupId_MustBeUniqueAcrossCurrentStock()
		//{
		//	var data = new TestDataSimpleEnvironment(Factory);
		//	data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
		//	Helper.EnableWarehouseForBond(data.Whs1, true);
		//	Helper.RemoveAreasOfTypeFromWarehouse(data.Whs1, "FRE");
		//	var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
		//	Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, "123-1", "ABC", 2m);
		//	receive.AllocateLocationsWithMock();
		//	receive.FinaliseDocketWithoutUserConfirmation();
		//	AssertIsFinalisedPrecondition(receive);
		//	Factory.Save();
		//
		//	var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
		//	var stocktakeLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, "", true);
		//	stocktakeLine.WU_BondedEntryKey = "123-1";
		//	stocktakeLine.WU_LastCount = 10m;
		//	stocktakeLine.WU_PackageGroupId = "ABC";
		//	stocktakeLine.WU_PerPackageQty = 2m;
		//	AssertNoErrors("Precondition", stocktakeLine.WU_PackageGroupIdInfo);
		//
		//	stocktake.CloseLines(new[] { stocktakeLine });
		//	AssertHasError(stocktakeLine.WU_PackageGroupIdInfo, "Package Group ID previously assigned. Assign new unique Package Group ID.");
		//	AssertEquals(false, stocktakeLine.IsClosed);
		//
		//	stocktakeLine.WU_PackageGroupId = "XYZ";
		//	stocktake.CloseLines(new[] { stocktakeLine });
		//	AssertNoErrors(stocktakeLine.WU_PackageGroupIdInfo);
		//	AssertEquals(true, stocktakeLine.IsClosed);
		//}

		#endregion

		#region TestEverythingInPackageGroupMustBeClosedTogether

		public void TestEverythingInPackageGroupMustBeClosedTogether()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeLine1 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, "", false);
			stocktakeLine1.WU_LastCount = 10m;
			stocktakeLine1.WU_PerPackageQty = 2m;
			stocktakeLine1.WU_PackageGroupId = "ABC";

			var stocktakeLine2 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part2, data.Whs1.DefaultLocation, "", true);
			stocktakeLine2.WU_LastCount = 10m;
			stocktakeLine2.WU_PerPackageQty = 2m;
			stocktakeLine2.WU_PackageGroupId = "ABC";

			AssertEquals(false, stocktake.CloseLines(new[] { stocktakeLine1 }));
			AssertHasError(stocktakeLine1.WU_PackageGroupIdInfo, "All Stocktake Lines in the same Package Group must be closed together.");

			AssertEquals(false, stocktake.CloseLines(new[] { stocktakeLine2 }));
			AssertHasError(stocktakeLine2.WU_PackageGroupIdInfo, "All Stocktake Lines in the same Package Group must be closed together.");

			AssertEquals(true, stocktake.CloseLines(new[] { stocktakeLine1, stocktakeLine2 }));
			AssertNoErrors(stocktakeLine1.WU_PackageGroupIdInfo);
			AssertNoErrors(stocktakeLine2.WU_PackageGroupIdInfo);
		}

		#endregion

		#region TestCheckWU_PerPackageQty_MustBeEnteredIfPackageGroupIDIsEntered

		public void TestCheckWU_PerPackageQty_MustBeEnteredIfPackageGroupIDIsEntered()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, true);
			stocktakeLine.WU_LastCount = 10m;
			AssertNoErrors("Precondition", stocktakeLine.WU_PerPackageQtyInfo);

			var expectedErrorMessage = "Per Group Quantity must be specified if Package Group ID is specified.";
			stocktakeLine.WU_PackageGroupId = "ABC";
			AssertHasError(stocktakeLine.WU_PerPackageQtyInfo, expectedErrorMessage);

			stocktakeLine.WU_PerPackageQty = 2m;
			AssertNoErrors(stocktakeLine.WU_PerPackageQtyInfo);

			stocktakeLine.WU_PerPackageQty = 0m;
			AssertHasError(stocktakeLine.WU_PerPackageQtyInfo, expectedErrorMessage);

			stocktakeLine.WU_PackageGroupId = "";
			AssertNoErrors(stocktakeLine.WU_PerPackageQtyInfo);

			stocktakeLine.WU_PerPackageQty = -10m;
			AssertHasError(stocktakeLine.WU_PerPackageQtyInfo, "Per Package Qty cannot be negative.");
		}

		#endregion

		#region TestCheckWU_PerPackageQty_PerPackageQtyHasSameDecimalsAsProduct

		public void TestCheckWU_PerPackageQty_PerPackageQtyHasSameDecimalsAsProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			data.Part1.OP_CountDecimalPlaces = 1;

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, true);
			stocktakeLine.WU_LastCount = 10m;
			AssertNoErrors("Precondition", stocktakeLine.WU_PerPackageQtyInfo);

			stocktakeLine.WU_PackageGroupId = "123";
			stocktakeLine.WU_PerPackageQty = 2.50m;
			AssertNoErrors(stocktakeLine.WU_PerPackageQtyInfo);

			stocktakeLine.WU_LastCount = 9;
			stocktakeLine.WU_PerPackageQty = 2.250m;
			AssertHasError(stocktakeLine.WU_PerPackageQtyInfo, "Per Group Quantity must have 1 decimal place(s) as specified on Product 'P1'.");

			data.Part1.OP_CountDecimalPlaces = 2;
			stocktakeLine.Validation.ValidateWU_PerPackageQty();
			AssertNoErrors(stocktakeLine.WU_PerPackageQtyInfo);

			data.Part1.OP_CountDecimalPlaces = 0;
			stocktakeLine.Validation.ValidateWU_PerPackageQty();
			AssertHasError(stocktakeLine.WU_PerPackageQtyInfo, "Per Group Quantity must have 0 decimal place(s) as specified on Product 'P1'.");
		}

		#endregion

		#region TestCheckPerPackageQty_SameProductInSamePackageMustHaveSamePerPackageQty

		public void TestCheckPerPackageQty_SameProductInSamePackageMustHaveSamePerPackageQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeLine1 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, true);
			var stocktakeLine2 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, true);
			stocktakeLine1.WU_LastCount = 10m;
			stocktakeLine2.WU_LastCount = 10m;
			stocktakeLine1.WU_BondedEntryKey = "444-1";
			stocktakeLine2.WU_BondedEntryKey = "444-2";
			stocktakeLine1.WU_PackageGroupId = "123";
			stocktakeLine2.WU_PackageGroupId = "456";
			stocktakeLine1.WU_PerPackageQty = 2m;
			stocktakeLine2.WU_PerPackageQty = 5m;
			stocktakeLine1.WU_PartAttrib1 = "Red";
			stocktakeLine2.WU_PartAttrib1 = "Red";
			AssertNoErrors("Precondition", stocktakeLine1.WU_PerPackageQtyInfo);
			AssertNoErrors("Precondition", stocktakeLine2.WU_PerPackageQtyInfo);

			var expectedErrorMessage = "Same product with the same Package Group ID must have the same Per Group Quantity.";
			stocktakeLine2.WU_PackageGroupId = "123";
			AssertHasError(stocktakeLine2.WU_PerPackageQtyInfo, expectedErrorMessage);

			stocktakeLine2.WU_PackageGroupId = "456";
			AssertNoErrors(stocktakeLine2.WU_PerPackageQtyInfo);

			stocktakeLine2.WU_PackageGroupId = "123";
			AssertHasError(stocktakeLine2.WU_PerPackageQtyInfo, expectedErrorMessage);

			stocktakeLine2.WU_PerPackageQty = 2m;
			AssertNoErrors(stocktakeLine2.WU_PerPackageQtyInfo);

			stocktakeLine2.WU_PerPackageQty = 5m;
			AssertHasError(stocktakeLine2.WU_PerPackageQtyInfo, expectedErrorMessage);

			stocktakeLine2.WU_PartAttrib1 = "Green";
			stocktakeLine2.Validation.ValidateWU_PerPackageQty();
			AssertNoErrors(stocktakeLine2.WU_PerPackageQtyInfo);
		}

		#endregion
	}
}
