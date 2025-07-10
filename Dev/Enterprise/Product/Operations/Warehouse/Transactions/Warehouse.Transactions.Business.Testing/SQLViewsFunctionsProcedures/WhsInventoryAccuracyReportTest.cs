using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsInventoryAccuracyReportTest : WhsTestCaseWithFactory
	{
		#region TestFilter

		#region TestWarehouseFilter

		public void TestWarehouseFilter()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var part1 = Helper.CreateProduct(client1, "1");
			var part2 = Helper.CreateProduct(client2, "2");
			Factory.Save();

			var warehouse1 = Helper.CreateWarehouse("W1", "A", 1, 1);
			var warehouse2 = Helper.CreateWarehouse("W2", "C", 1, 1);
			var warehouse3 = Helper.CreateWarehouse("W3", "B", 1, 1);
			var warehouseLocation1 = warehouse1.DefaultLocation;
			var warehouseLocation2 = warehouse2.DefaultLocation;
			Factory.Save();

			var ccLocation1 =
				CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation1, CycleCountGranularity.Codes.PalletIDOnly, ZDateTimeOffset.Now, ZDateTimeOffset.Now.AddMinutes(10), "AAA");
			var ccLocationVar1 = Helper.CreateWhsCycleCountLocationVariance(ccLocation1,
				CycleCountVarianceStatus.Codes.Approved, client1, part1, 0, 2, "");
			Factory.Save();

			var ccLocation2 =
				CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation2, CycleCountGranularity.Codes.PalletIDOnly, ZDateTimeOffset.Now, ZDateTimeOffset.Now.AddMinutes(10), "AAA");
			var ccLocationVar2 = Helper.CreateWhsCycleCountLocationVariance(ccLocation2,
				CycleCountVarianceStatus.Codes.Approved, client2, part2, 0, 2, "");
			Factory.Save();

			var result1 = LoadView(ZGuid.Empty, warehouse1.PK, ZGuid.Empty);
			AssertEquals("Should have 1 record that is match with warehouse1", 1, result1.Count);
			AssertRow(result1[0], client1, part1, warehouse1, ccLocation1, ccLocationVar1);

			var result2 = LoadView(ZGuid.Empty, warehouse2.PK, ZGuid.Empty);
			AssertEquals("Should have 1 record that is match with warehouse2", 1, result2.Count);
			AssertRow(result2[0], client2, part2, warehouse2, ccLocation2, ccLocationVar2);

			var result3 = LoadView(ZGuid.Empty, warehouse3.PK, ZGuid.Empty);
			AssertEquals("Should have 0 record that is match with warehouse3", 0, result3.Count);
		}

		#endregion

		#region TestClientFilter

		public void TestClientFilter()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var client3 = Helper.CreateClient("C3");
			var part1 = Helper.CreateProduct(client1, "P1");
			var part2 = Helper.CreateProduct(client2, "P2");
			Factory.Save();

			var warehouse1 = Helper.CreateWarehouse("W1", "A", 1, 1);
			var warehouse2 = Helper.CreateWarehouse("W2", "C", 1, 1);
			var warehouseLocation1 = warehouse1.DefaultLocation;
			var warehouseLocation2 = warehouse2.DefaultLocation;
			Factory.Save();

			var ccLocation1 =
				CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation1, CycleCountGranularity.Codes.PalletIDOnly, ZDateTimeOffset.Now, ZDateTimeOffset.Now.AddMinutes(10), "AAA");
			var ccLocationVar1 = Helper.CreateWhsCycleCountLocationVariance(ccLocation1,
				CycleCountVarianceStatus.Codes.Approved, client1, part1, 0, 2, "");
			Factory.Save();

			var ccLocation2 =
				CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation2, CycleCountGranularity.Codes.PalletIDOnly, ZDateTimeOffset.Now, ZDateTimeOffset.Now.AddMinutes(10), "AAA");
			var ccLocationVar2 = Helper.CreateWhsCycleCountLocationVariance(ccLocation2,
				CycleCountVarianceStatus.Codes.Approved, client2, part2, 0, 2, "");
			Factory.Save();

			var result1 = LoadView(client1.PK, ZGuid.Empty, ZGuid.Empty);
			AssertEquals("Should have 1 record that is match with client1", 1, result1.Count);
			AssertRow(result1[0], client1, part1, warehouse1, ccLocation1, ccLocationVar1);

			var result2 = LoadView(client2.PK, ZGuid.Empty, ZGuid.Empty);
			AssertEquals("Should have 1 record that is match with client2", 1, result2.Count);
			AssertRow(result2[0], client2, part2, warehouse2, ccLocation2, ccLocationVar2);

			var result3 = LoadView(client3.PK, ZGuid.Empty, ZGuid.Empty);
			AssertEquals("Should have 0 record that is match with client3", 0, result3.Count);
		}

		#endregion

		#region TestProductCategoryFilter

		public void TestProductFilter()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var client3 = Helper.CreateClient("C3");
			var part1 = Helper.CreateProduct(client1, "P1");
			var part2 = Helper.CreateProduct(client2, "P2");
			var part3 = Helper.CreateProduct(client3, "P3");
			Factory.Save();

			var warehouse1 = Helper.CreateWarehouse("W1", "A", 1, 1);
			var warehouse2 = Helper.CreateWarehouse("W2", "C", 1, 1);
			var warehouseLocation1 = warehouse1.DefaultLocation;
			var warehouseLocation2 = warehouse2.DefaultLocation;
			Factory.Save();

			var ccLocation1 =
				CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation1, CycleCountGranularity.Codes.PalletIDOnly, ZDateTimeOffset.Now, ZDateTimeOffset.Now.AddMinutes(10), "AAA");
			var ccLocationVar1 = Helper.CreateWhsCycleCountLocationVariance(ccLocation1,
				CycleCountVarianceStatus.Codes.Approved, client1, part1, 0, 2, "");
			Factory.Save();

			var ccLocation2 =
				CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation2, CycleCountGranularity.Codes.PalletIDOnly, ZDateTimeOffset.Now, ZDateTimeOffset.Now.AddMinutes(10), "AAA");
			var ccLocationVar2 = Helper.CreateWhsCycleCountLocationVariance(ccLocation2,
				CycleCountVarianceStatus.Codes.Approved, client2, part2, 0, 2, "");
			Factory.Save();

			var result1 = LoadView(ZGuid.Empty, ZGuid.Empty, part1.PK);
			AssertEquals("Should have 1 record that is match with part1", 1, result1.Count);
			AssertRow(result1[0], client1, part1, warehouse1, ccLocation1, ccLocationVar1);

			var result2 = LoadView(ZGuid.Empty, ZGuid.Empty, part2.PK);
			AssertEquals("Should have 1 record that is match with part2", 1, result2.Count);
			AssertRow(result2[0], client2, part2, warehouse2, ccLocation2, ccLocationVar2);

			var result3 = LoadView(ZGuid.Empty, ZGuid.Empty, part3.PK);
			AssertEquals("Should have 0 record that is match with part3", 0, result3.Count);
		}

		#endregion

		#region TestPeriodFilter

		public void TestPeriodFilter()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var client3 = Helper.CreateClient("C3");
			var part1 = Helper.CreateProduct(client1, "1");
			var part2 = Helper.CreateProduct(client2, "2");
			var part3 = Helper.CreateProduct(client1, "3");
			Factory.Save();

			var warehouse1 = Helper.CreateWarehouse("W1", "A", 1, 1);
			var warehouse2 = Helper.CreateWarehouse("W2", "B", 1, 1);
			var warehouse3 = Helper.CreateWarehouse("W2", "C", 1, 1);
			var warehouseLocation1 = warehouse1.DefaultLocation;
			var warehouseLocation2 = warehouse2.DefaultLocation;
			var warehouseLocation3 = warehouse3.DefaultLocation;
			Factory.Save();

			var ccLocation1 = CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation1,
				CycleCountGranularity.Codes.PalletIDOnly, new ZDateTimeOffset(ZDate.Today.AddYears(-2)),
				new ZDateTimeOffset(ZDate.Today.AddYears(+2)), "AAQ", null, 0);
			var ccLocationVar1 = Helper.CreateWhsCycleCountLocationVariance(ccLocation1,
				CycleCountVarianceStatus.Codes.Approved, client1, part1, 0, 2, "");

			var ccLocation2 = CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation2,
				CycleCountGranularity.Codes.ProductWithAllAttributes, new ZDateTimeOffset(ZDate.Today.AddYears(-3)),
				new ZDateTimeOffset(ZDate.Today.AddYears(+1)), "AAQ", null, 0);
			var ccLocationVar2 = Helper.CreateWhsCycleCountLocationVariance(ccLocation2,
				CycleCountVarianceStatus.Codes.Approved, client2, part2, 0, 7, "");

			var ccLocation3 = CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation3,
				CycleCountGranularity.Codes.PalletIDOnly, new ZDateTimeOffset(ZDate.Today.AddYears(-1)),
				new ZDateTimeOffset(ZDate.Today.AddDays(+1)), "AAQ", null, 0);
			var ccLocationVar3 = Helper.CreateWhsCycleCountLocationVariance(ccLocation3,
				CycleCountVarianceStatus.Codes.Approved, client3, part3, 0, 17, "");
			Factory.Save();

			var result1 = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			AssertEquals("Should have 3 records", 3, result1.Count);
			AssertRow(result1[0], client1, part1, warehouse1, ccLocation1, ccLocationVar1);
			AssertRow(result1[1], client2, part2, warehouse2, ccLocation2, ccLocationVar2);
			AssertRow(result1[2], client3, part3, warehouse3, ccLocation3, ccLocationVar3);

			var result2 = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZDateTimeOffset.Today.AddYears(0));
			AssertEquals("Should have 3 records", 3, result2.Count);
			AssertRow(result2[0], client1, part1, warehouse1, ccLocation1, ccLocationVar1);
			AssertRow(result2[1], client2, part2, warehouse2, ccLocation2, ccLocationVar2);
			AssertRow(result2[2], client3, part3, warehouse3, ccLocation3, ccLocationVar3);

			var result3 = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZDateTimeOffset.Today.AddDays(+364));
			AssertEquals("Should have 2 records", 2, result3.Count);
			AssertRow(result3[0], client1, part1, warehouse1, ccLocation1, ccLocationVar1);
			AssertRow(result3[1], client2, part2, warehouse2, ccLocation2, ccLocationVar2);

			var result4 = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZDateTimeOffset.Today.AddYears(+0),
				ZDateTimeOffset.Today.AddYears(+1));
			AssertEquals("Should have 2 records", 2, result4.Count);
			AssertRow(result4[0], client2, part2, warehouse2, ccLocation2, ccLocationVar2);
			AssertRow(result4[1], client3, part3, warehouse3, ccLocation3, ccLocationVar3);

			var result5 = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZDateTimeOffset.Today.AddYears(+0),
				ZDateTimeOffset.Today.AddDays(+1));
			AssertEquals("Should have 1 records", 1, result5.Count);
			AssertRow(result5[0], client3, part3, warehouse3, ccLocation3, ccLocationVar3);
		}

		#endregion

		#region TestInventoryAccuracyReportFunction

		public void TestInventoryAccuracyReportFunction()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var client3 = Helper.CreateClient("C3");

			var part1 = Helper.CreateProduct(client1, "P1");
			var part2 = Helper.CreateProduct(client2, "P2");
			var part3 = Helper.CreateProduct(client3, "P3");
			Factory.Save();

			var warehouse1 = Helper.CreateWarehouse("W1", "A", 1, 1);
			var warehouse2 = Helper.CreateWarehouse("W2", "C", 1, 1);
			var warehouse3 = Helper.CreateWarehouse("W3", "B", 1, 1);
			var warehouseLocation1 = warehouse1.DefaultLocation;
			var warehouseLocation2 = warehouse2.DefaultLocation;
			var warehouseLocation3 = warehouse3.DefaultLocation;
			Factory.Save();

			var ccLocation1 =
				CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation1, CycleCountGranularity.Codes.PalletIDOnly, ZDateTimeOffset.Now, ZDateTimeOffset.Now.AddMinutes(10), "AAA");
			var ccLocationVar1 = Helper.CreateWhsCycleCountLocationVariance(ccLocation1,
				CycleCountVarianceStatus.Codes.Approved, client1, part1, 0, 2, "");

			var ccLocation2 =
				CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation2, CycleCountGranularity.Codes.PalletIDOnly, ZDateTimeOffset.Now, ZDateTimeOffset.Now.AddMinutes(10), "AAA");
			var ccLocationVar2 = Helper.CreateWhsCycleCountLocationVariance(ccLocation2,
				CycleCountVarianceStatus.Codes.Approved, client2, part2, 0, 6, "");

			var ccLocation3 =
				CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation3, CycleCountGranularity.Codes.PalletIDOnly, ZDateTimeOffset.Now, ZDateTimeOffset.Now.AddMinutes(10), "AAA");
			var ccLocationVar3 = Helper.CreateWhsCycleCountLocationVariance(ccLocation3,
				CycleCountVarianceStatus.Codes.Approved, client3, part3, 0, 6, "");
			Factory.Save();

			var result = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			AssertEquals("Should have 3 record that are match", 3, result.Count);
			AssertRow(result[0], client1, part1, warehouse1, ccLocation1, ccLocationVar1);
			AssertRow(result[1], client2, part2, warehouse2, ccLocation2, ccLocationVar2);
			AssertRow(result[2], client3, part3, warehouse3, ccLocation3, ccLocationVar3);
		}

		#endregion

		#endregion

		#region TestNullClient

		public void TestNullClientNullProduct()
		{
			var warehouse1 = Helper.CreateWarehouse("W1", "A", 1, 1);
			var warehouse2 = Helper.CreateWarehouse("W2", "C", 1, 1);
			var warehouse3 = Helper.CreateWarehouse("W3", "B", 1, 1);
			var warehouseLocation1 = warehouse1.DefaultLocation;
			var warehouseLocation2 = warehouse2.DefaultLocation;
			var warehouseLocation3 = warehouse3.DefaultLocation;
			Factory.Save();

			var ccLocation1 =
				CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation1, CycleCountGranularity.Codes.PalletIDOnly, ZDateTimeOffset.Now, ZDateTimeOffset.Now.AddMinutes(10), "AAA");
			var ccLocationVar1 = Helper.CreateWhsCycleCountLocationVariance(ccLocation1,
				CycleCountVarianceStatus.Codes.Approved, "Sample Pallet ID", null, null, 0, "", "", "", "", null, null,
				2, null, false, "");

			var ccLocation2 = CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation2,
				CycleCountGranularity.Codes.ProductWithAllAttributes, ZDateTimeOffset.Now, ZDateTimeOffset.Now.AddMinutes(10), "AAA");
			var ccLocationVar2 = Helper.CreateWhsCycleCountLocationVariance(ccLocation2,
				CycleCountVarianceStatus.Codes.Approved, "Sample Pallet ID 2", null, null, 0, "", "", "", "", null,
				null, 4, null, false, "");

			var ccLocation3 =
				CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation3, CycleCountGranularity.Codes.ProductOnly, ZDateTimeOffset.Now, ZDateTimeOffset.Now.AddMinutes(10), "AAA");
			var ccLocationVar3 = Helper.CreateWhsCycleCountLocationVariance(ccLocation3,
				CycleCountVarianceStatus.Codes.Approved, "Sample Pallet ID 3", null, null, 0, "", "", "", "", null,
				null, 8, null, false, "");
			Factory.Save();

			var result = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			AssertEquals("Should have 3 record that are match", 3, result.Count);
			AssertRow(result[0], null, null, warehouse1, ccLocation1, ccLocationVar1);
			AssertRow(result[1], null, null, warehouse2, ccLocation2, ccLocationVar2);
			AssertRow(result[2], null, null, warehouse3, ccLocation3, ccLocationVar3);
		}

		#endregion

		#region TestGranularity

		public void TestGranularityPLT()
		{
			TestGranularity(CycleCountGranularity.Codes.PalletCount);
		}

		public void TestGranularityPID()
		{
			TestGranularity(CycleCountGranularity.Codes.PalletIDOnly);
		}

		public void TestGranularityPRD()
		{
			TestGranularity(CycleCountGranularity.Codes.ProductOnly);
		}

		public void TestGranularityPWP()
		{
			TestGranularity(CycleCountGranularity.Codes.ProductWithPalletID);
		}

		public void TestGranularityPWA()
		{
			TestGranularity(CycleCountGranularity.Codes.ProductWithAttributes);
		}

		public void TestGranularityPWS()
		{
			TestGranularity(CycleCountGranularity.Codes.ProductWithAllAttributes);
		}

		void TestGranularity(ZString granularity)
		{
			var isPLT = granularity == CycleCountGranularity.Codes.PalletCount;

			var client = Helper.CreateClient("C1");
			var part = isPLT ? null : Helper.CreateProduct(client, "P1");
			Factory.Save();

			var warehouse = Helper.CreateWarehouse("W1", "A", 1, 1);
			var warehouseLocation1 = warehouse.DefaultLocation;
			Factory.Save();

			var ccLocation = CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation1, granularity, ZDateTimeOffset.Now, ZDateTimeOffset.Now.AddMinutes(10), "AAA");
			var ccLocationVar = isPLT
				? Helper.CreateWhsCycleCountLocationVariance(ccLocation, CycleCountVarianceStatus.Codes.Approved,
					"Sample Pallet ID", client, null, 0, "", "", "", "", null, null, 2, null, false, "")
				: Helper.CreateWhsCycleCountLocationVariance(ccLocation, CycleCountVarianceStatus.Codes.Approved,
					client, part, 0, 2, "");
			Factory.Save();

			var result = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			var statement = "Should " + (isPLT ? "not" : "") + " display product number";
			AssertEquals(statement, isPLT ? "" : "P1", result[0]["Product"]);
			AssertRow(result[0], client, part, warehouse, ccLocation, ccLocationVar);
		}

		#endregion

		#region TestShouldOnlyReturnApprovedVariances

		public void TestApprovedVariances()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var part1 = Helper.CreateProduct(client1, "1");
			var part2 = Helper.CreateProduct(client2, "2");
			Factory.Save();

			var warehouse1 = Helper.CreateWarehouse("W1", "A", 1, 1);
			var warehouse2 = Helper.CreateWarehouse("W2", "B", 1, 1);
			var warehouseLocation1 = warehouse1.DefaultLocation;
			var warehouseLocation2 = warehouse2.DefaultLocation;
			Factory.Save();

			var ccLocation1 =
				CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation1, CycleCountGranularity.Codes.PalletIDOnly, ZDateTimeOffset.Now, ZDateTimeOffset.Now.AddMinutes(10), "AAA");
			Helper.CreateWhsCycleCountLocationVariance(ccLocation1, CycleCountVarianceStatus.Codes.Approved, client1,
				part1, 0, 2, "");
			Factory.Save();

			var result1 = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			AssertEquals("Should have 1 record of approved variance", 1, result1.Count);

			var ccLocation2 =
				CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation2, CycleCountGranularity.Codes.PalletIDOnly, ZDateTimeOffset.Now, ZDateTimeOffset.Now.AddMinutes(10), "AAA");
			Helper.CreateWhsCycleCountLocationVariance(ccLocation2, CycleCountVarianceStatus.Codes.Open, client2, part2,
				0, 2, "");
			Factory.Save();

			var result2 = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			AssertEquals("Should have 1 record of approved variance and not include records of opened variance", 1,
				result2.Count);
		}

		public void TestOpenVariances()
		{
			var client = Helper.CreateClient("C1");
			var part = Helper.CreateProduct(client, "1");
			Factory.Save();

			var warehouse = Helper.CreateWarehouse("W1", "A", 1, 1);
			var warehouseLocation1 = warehouse.DefaultLocation;
			Factory.Save();

			var ccLocation =
				CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation1, CycleCountGranularity.Codes.PalletIDOnly, ZDateTimeOffset.Now, ZDateTimeOffset.Now.AddMinutes(10), "AAA");
			Helper.CreateWhsCycleCountLocationVariance(ccLocation, CycleCountVarianceStatus.Codes.Open, client, part, 0,
				2, "");
			Factory.Save();

			var result = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			AssertEquals("Should have 0 record of approved variances", 0, result.Count);
		}

		public void TestRejectedVariances()
		{
			var client = Helper.CreateClient("C1");
			var part = Helper.CreateProduct(client, "1");
			Factory.Save();

			var warehouse1 = Helper.CreateWarehouse("W1", "A", 1, 1);
			var warehouseLocation1 = warehouse1.DefaultLocation;
			Factory.Save();

			var ccLocation =
				CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation1, CycleCountGranularity.Codes.PalletIDOnly, ZDateTimeOffset.Now, ZDateTimeOffset.Now.AddMinutes(10), "AAA");
			Helper.CreateWhsCycleCountLocationVariance(ccLocation, CycleCountVarianceStatus.Codes.Rejected, client,
				part, 0, 2, "");
			Factory.Save();

			var result = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			AssertEquals("Should have 0 record of approved variances", 0, result.Count);
		}

		#endregion

		#region TestCountedQty

		public void TestPositiveVariance()
		{
			var client = Helper.CreateClient("C1");
			var part = Helper.CreateProduct(client, "1");
			Factory.Save();

			var warehouse = Helper.CreateWarehouse("W1", "A", 1, 1);
			var warehouseLocation = warehouse.DefaultLocation;
			Factory.Save();

			var varianceQty = 1;
			var expectedQty = 2;
			var expectedCountedQty = varianceQty + expectedQty;
			var expectedAbsVarianceQty = varianceQty > 0 ? varianceQty : varianceQty * (-1);
			var ccLocation =
				CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation, CycleCountGranularity.Codes.PalletIDOnly, ZDateTimeOffset.Now, ZDateTimeOffset.Now.AddMinutes(10), "AAA");
			var ccLocationVar = Helper.CreateWhsCycleCountLocationVariance(ccLocation,
				CycleCountVarianceStatus.Codes.Approved, client, part, varianceQty, expectedQty, "");
			var whsAdjustment = Helper.CreateWhsAdjustment(client, warehouse);
			ccLocationVar.WCC_WD_RelatedAdjustment = whsAdjustment.PK;
			Factory.Save();

			var result = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			AssertEquals("Should return expected countedQty", (ZDecimal)expectedCountedQty, result[0]["CountedQty"]);
			AssertEquals("Should return absolute variance Qty", (ZDecimal)expectedAbsVarianceQty,
				result[0]["AbsVarianceQty"]);
			AssertRow(result[0], client, part, warehouse, ccLocation, ccLocationVar);
		}

		public void TestNegativeVariance()
		{
			var client = Helper.CreateClient("C1");
			var part = Helper.CreateProduct(client, "1");
			Factory.Save();

			var warehouse = Helper.CreateWarehouse("W1", "A", 1, 1);
			var warehouseLocation = warehouse.DefaultLocation;
			Factory.Save();

			var varianceQty = -1;
			var expectedQty = 5;
			var expectedCountedQty = varianceQty + expectedQty;
			var expectedAbsVarianceQty = varianceQty > 0 ? varianceQty : varianceQty * (-1);
			var ccLocation =
				CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation, CycleCountGranularity.Codes.PalletIDOnly, ZDateTimeOffset.Now, ZDateTimeOffset.Now.AddMinutes(10), "AAA");
			var ccLocationVar = Helper.CreateWhsCycleCountLocationVariance(ccLocation,
				CycleCountVarianceStatus.Codes.Approved, client, part, varianceQty, expectedQty, "");
			var whsAdjustment = Helper.CreateWhsAdjustment(client, warehouse);
			ccLocationVar.WCC_WD_RelatedAdjustment = whsAdjustment.PK;
			Factory.Save();

			var result = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			AssertEquals("Should return expected countedQty", (ZDecimal)expectedCountedQty, result[0]["CountedQty"]);
			AssertEquals("Should return absolute variance Qty", (ZDecimal)expectedAbsVarianceQty,
				result[0]["AbsVarianceQty"]);
			AssertRow(result[0], client, part, warehouse, ccLocation, ccLocationVar);
		}

		#endregion

		#region TestUnitPriceAndCurrency

		public void TestWhenNoUnitPrice()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var part1 = Helper.CreateProduct(client1, "P1");
			var part2 = Helper.CreateProduct(client2, "P2");
			var productClientRelation2 = Helper.CreateProductClientRelationShip(client2, part2);
			productClientRelation2.OU_UnitPrice = 1.23;
			productClientRelation2.OU_RX_NKUnitPriceCurrency = "AUD";
			Factory.Save();

			var warehouse1 = Helper.CreateWarehouse("W1", "A", 1, 1);
			var warehouse2 = Helper.CreateWarehouse("W2", "B", 1, 1);
			var warehouseLocation1 = warehouse1.DefaultLocation;
			var warehouseLocation2 = warehouse2.DefaultLocation;
			Factory.Save();

			var ccLocation1 =
				CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation1, CycleCountGranularity.Codes.PalletIDOnly, ZDateTimeOffset.Now, ZDateTimeOffset.Now.AddMinutes(10), "AAA");
			Helper.CreateWhsCycleCountLocationVariance(ccLocation1, CycleCountVarianceStatus.Codes.Approved, client1,
				part1, 0, 6, "");

			var ccLocation2 =
				CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation2, CycleCountGranularity.Codes.PalletIDOnly, ZDateTimeOffset.Now, ZDateTimeOffset.Now.AddMinutes(10), "AAA");
			var ccLocationVar = Helper.CreateWhsCycleCountLocationVariance(ccLocation2,
				CycleCountVarianceStatus.Codes.Approved, client2, part2, 0, 3, "");
			ccLocationVar.WCC_OP_Product = productClientRelation2.OU_OP;
			Factory.Save();

			var result = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			AssertEquals("Should return empty unit price currency", "", result[0]["UnitPriceCurrency"]);
			AssertEquals("Should return valid unit price currency", "AUD", result[1]["UnitPriceCurrency"]);
		}

		#endregion

		#region TestWithCommodityCode

		public void TestWithCommodityCode()
		{
			var commodity = new RefCommodityCode[2]
			{
				Factory.NewWithValidTestData<RefCommodityCode>(), Factory.NewWithValidTestData<RefCommodityCode>()
			};

			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var part1 = Helper.CreateProduct(client1, "P1");
			var part2 = Helper.CreateProduct(client2, "P2");
			Factory.Save();

			part1.OP_RH_NKCommodityCode = commodity[0].RH_Code;
			part2.OP_RH_NKCommodityCode = commodity[0].RH_Code;
			Factory.Save();

			var warehouse1 = Helper.CreateWarehouse("W1", "A", 1, 1);
			var warehouse2 = Helper.CreateWarehouse("W2", "B", 1, 1);
			var warehouseLocation1 = warehouse1.DefaultLocation;
			var warehouseLocation2 = warehouse2.DefaultLocation;
			Factory.Save();

			var ccLocation1 =
				CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation1, CycleCountGranularity.Codes.PalletIDOnly, ZDateTimeOffset.Now, ZDateTimeOffset.Now.AddMinutes(10), "AAA");
			var ccLocationVar1 = Helper.CreateWhsCycleCountLocationVariance(ccLocation1,
				CycleCountVarianceStatus.Codes.Approved, client1, part1, 0, 6, "");

			var ccLocation2 =
				CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation2, CycleCountGranularity.Codes.PalletIDOnly, ZDateTimeOffset.Now, ccLocation1.WCL_EndTime, "AAA");
			var ccLocationVar2 = Helper.CreateWhsCycleCountLocationVariance(ccLocation2,
				CycleCountVarianceStatus.Codes.Approved, client2, part2, 0, 3, "");
			Factory.Save();

			var result = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			AssertRow(result[0], client1, part1, warehouse1, ccLocation1, ccLocationVar1);
			AssertRow(result[1], client2, part2, warehouse2, ccLocation2, ccLocationVar2);
		}

		#endregion

		#region TestCycleCountVariance

		public void TestCycleCountVariance()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var client3 = Helper.CreateClient("C3");
			var part1 = Helper.CreateProduct(client1, "1");
			var part2 = Helper.CreateProduct(client2, "2");
			var part3 = Helper.CreateProduct(client1, "3");
			Factory.Save();

			var warehouse1 = Helper.CreateWarehouse("W1", "A", 1, 1);
			var warehouse2 = Helper.CreateWarehouse("W2", "B", 1, 1);
			var warehouse3 = Helper.CreateWarehouse("W2", "C", 1, 1);
			var warehouseLocation1 = warehouse1.DefaultLocation;
			var warehouseLocation2 = warehouse2.DefaultLocation;
			var warehouseLocation3 = warehouse3.DefaultLocation;
			Factory.Save();

			var ccLocation1 = CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation1,
				CycleCountGranularity.Codes.PalletIDOnly, new ZDateTimeOffset(ZDate.Today.AddYears(-2)),
				new ZDateTimeOffset(ZDate.Today.AddYears(+2)), "AAQ", null, 0);
			var ccLocationVar1 = Helper.CreateWhsCycleCountLocationVariance(ccLocation1,
				CycleCountVarianceStatus.Codes.Approved, "SamplePalletID1", client1, part1, 1, "SampleAttr11",
				"SampleAttr12", "SampleAttr13", "SampleSerial1", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(-2), 12,
				warehouseLocation1, false, "");
			var whsAdjustment1 = Helper.CreateWhsAdjustment(client1, warehouse1);
			ccLocationVar1.WCC_WD_RelatedAdjustment = whsAdjustment1.PK;
			Factory.Save();

			var ccLocation2 = CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation2,
				CycleCountGranularity.Codes.ProductWithAllAttributes, new ZDateTimeOffset(ZDate.Today.AddYears(-1)),
				new ZDateTimeOffset(ZDate.Today.AddDays(+1)), "BAQ", null, 0);
			var ccLocationVar2 = Helper.CreateWhsCycleCountLocationVariance(ccLocation2,
				CycleCountVarianceStatus.Codes.Approved, "SamplePalletID2", client2, part2, 2, "SampleAttr21",
				"SampleAttr22", "SampleAttr23", "SampleSerial2", ZDate.Today.AddDays(-30), ZDate.Today.AddDays(-25), 8,
				warehouseLocation2, false, "");
			var whsAdjustment2 = Helper.CreateWhsAdjustment(client2, warehouse2);
			ccLocationVar2.WCC_WD_RelatedAdjustment = whsAdjustment2.PK;
			Factory.Save();

			var ccLocation3 = CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation3,
				CycleCountGranularity.Codes.ProductOnly, new ZDateTimeOffset(ZDate.Today.AddYears(0)),
				new ZDateTimeOffset(ZDate.Today.AddDays(+3)), "CAQ", null, 0);
			var ccLocationVar3 = Helper.CreateWhsCycleCountLocationVariance(ccLocation3,
				CycleCountVarianceStatus.Codes.Approved, "SamplePalletID3", client3, part3, 3, "SampleAttr31",
				"SampleAttr32", "SampleAttr33", "SampleSerial3", ZDate.Today.AddDays(+0), ZDate.Today.AddDays(+1), 10,
				warehouseLocation3, false, "");
			var whsAdjustment3 = Helper.CreateWhsAdjustment(client3, warehouse3);
			ccLocationVar3.WCC_WD_RelatedAdjustment = whsAdjustment3.PK;
			Factory.Save();

			var result = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			AssertRow(result[0], client1, part1, warehouse1, ccLocation1, ccLocationVar1);
			AssertRow(result[1], client2, part2, warehouse2, ccLocation2, ccLocationVar2);
			AssertRow(result[2], client3, part3, warehouse3, ccLocation3, ccLocationVar3);
		}

		#endregion

		#region TestCycleCountWithNoVariance

		public void TestCycleCountWithNoVariance()
		{
			var warehouse1 = Helper.CreateWarehouse("W1", "A", 1, 1);
			var warehouse2 = Helper.CreateWarehouse("W2", "B", 1, 1);
			var warehouse3 = Helper.CreateWarehouse("W2", "C", 1, 1);
			var warehouse4 = Helper.CreateWarehouse("W3", "D", 1, 1);
			var warehouseLocation1 = warehouse1.DefaultLocation;
			var warehouseLocation2 = warehouse2.DefaultLocation;
			var warehouseLocation3 = warehouse3.DefaultLocation;
			var warehouseLocation4 = warehouse4.DefaultLocation;
			Factory.Save();

			var ccLocation1 = CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation1,
				CycleCountGranularity.Codes.PalletIDOnly, new ZDateTimeOffset(ZDate.Today.AddYears(-2)),
				new ZDateTimeOffset(ZDate.Today.AddYears(+2)), "AAQ", null, 0);
			Factory.Save();

			var ccLocation2 = CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation2,
				CycleCountGranularity.Codes.ProductWithAllAttributes, new ZDateTimeOffset(ZDate.Today.AddYears(-1)),
				new ZDateTimeOffset(ZDate.Today.AddDays(+1)), "BAQ", null, 0);
			Factory.Save();

			var ccLocation3 = CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation3,
				CycleCountGranularity.Codes.ProductWithAttributes, new ZDateTimeOffset(ZDate.Today.AddYears(0)),
				new ZDateTimeOffset(ZDate.Today.AddDays(+3)), "CAQ", null, 0);
			Factory.Save();

			// Cycle Count Location hasn't been completed
			var ccLocatio4 = CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation4,
				CycleCountGranularity.Codes.ProductWithAttributes, new ZDateTimeOffset(ZDate.Today.AddYears(0)),
				ZDateTimeOffset.Empty, "DAQ", null, 0);
			Factory.Save();

			var result1 = LoadView(ZGuid.Empty, warehouse1.PK, ZGuid.Empty);
			AssertRow(result1.Single(), null, null, warehouse1, ccLocation1, null);
			var result2 = LoadView(ZGuid.Empty, warehouse2.PK, ZGuid.Empty);
			AssertRow(result2.Single(), null, null, warehouse2, ccLocation2, null);
			var result3 = LoadView(ZGuid.Empty, warehouse3.PK, ZGuid.Empty);
			AssertRow(result3.Single(), null, null, warehouse3, ccLocation3, null);
			var result4 = LoadView(ZGuid.Empty, warehouse4.PK, ZGuid.Empty);
			AssertEquals("Cycle Count Location With no Variance and not completed should not been found.", 0, result4.Count);
		}

		#endregion

		#region TestFixedWidthLocationWarehouse

		public void TestFixedWidthLocationWarehouse()
		{
			var client = Helper.CreateClient("C1");
			var part = Helper.CreateProduct(client, "1");
			Factory.Save();

			var warehouse = Helper.CreateFixedWidthLocationWarehouse("ZZZ", 3, 2, 2);
			Helper.CreateRowAndGenerateLocations(warehouse, "LOCZ", 4, 3, 2);
			var warehouseLocation1 = warehouse.DefaultLocation;
			Factory.Save();

			var ccLocation = CreateWhsCycleCountLocationWithEndTimeFloor(warehouseLocation1, CycleCountGranularity.Codes.PalletIDOnly, ZDateTimeOffset.Now, ZDateTimeOffset.Now.AddMinutes(10), "AAA");
			var ccLocationVar = Helper.CreateWhsCycleCountLocationVariance(ccLocation, CycleCountVarianceStatus.Codes.Approved, client, part, 0, 2, "");
			Factory.Save();

			var result = LoadView(ZGuid.Empty, warehouse.PK, ZGuid.Empty);
			AssertEquals("Should have 1 record that is match with warehouse", 1, result.Count);
			AssertRow(result[0], client, part, warehouse, ccLocation, ccLocationVar);
		}

		#endregion

		#region CreateWhsCycleCountLocation

		WhsCycleCountLocation CreateWhsCycleCountLocationWithEndTimeFloor(WhsLocation location, ZString granularity, ZDateTimeOffset startTime, ZDateTimeOffset endTimeIn, string assignedTo = "", WhsCycleCountLocation rejectedCycleCount = null, byte priority = 0)
		{
			var endTime = endTimeIn.IsValid ? endTimeIn.ToSmallDateTimeFloor() : endTimeIn; // ToSmallDateTimeFloor to avoid precision issues when comparing after saving.
			return Helper.CreateWhsCycleCountLocation(location, granularity, startTime, endTime, assignedTo, rejectedCycleCount, priority);
		}

		#endregion

		#region AssertRow

		void AssertRow(DynamicBusinessObject result, OrgHeader client, OrgSupplierPart part, WhsWarehouse warehouse,
			WhsCycleCountLocation ccLocation, WhsCycleCountLocationVariance ccVariance)
		{
			var partRelation = (part != null && client != null)
				? Helper.CreateProductClientRelationShip(client, part)
				: null;
			var location = warehouse.DefaultLocation;
			var locationType = Factory.Load<WhsLocationType>(location.WLV_WLT_LocationType);
			var expectedCommodityPK = (part != null && !part.OP_RH_NKCommodityCode.IsEmpty)
				? part.CommodityCode.PK
				: ZGuid.Empty;
			var expectedUnitPrice = partRelation != null ? partRelation.OU_UnitPrice : 0.0;
			var expectUnitPriceCurrency = partRelation != null ? partRelation.OU_RX_NKUnitPriceCurrency : ZString.Empty;

			AssertEquals("ClientPK", client != null ? client.PK : ZGuid.Empty, result["ClientPK"]);
			AssertEquals("ClientCode", client != null ? client.OH_Code : ZString.Empty, result["ClientCode"]);
			AssertEquals("Client Fullname", client != null ? client.OH_FullName : ZString.Empty, result["Client"]);
			AssertEquals("LocationString", location.WLV_LocationString_UserFriendly, result["Location"]);
			AssertEquals("LocationDesc", locationType.WLT_Code + " - " + locationType.WLT_Description,
				result["LocationDesc"]);
			AssertEquals("LocnRow", location.WLV_RowName, result["LocnRow"]);
			AssertEquals("LocnLevel", location.WLV_FormattedLevel, result["LocnLevel"]);
			AssertEquals("LocnTray", location.WLV_FormattedTray, result["LocnTray"]);
			AssertEquals("Product", part != null ? part.OP_PartNum : ZString.Empty, result["Product"]);
			AssertEquals("ProductDescription", part != null ? part.OP_Desc : ZString.Empty,
				result["ProductDescription"]);
			AssertEquals("WarehousePK", warehouse.PK, result["WarehousePK"]);
			AssertEquals("WarehouseCode", warehouse.WW_WarehouseCode, result["WarehouseCode"]);
			AssertEquals("WarehouseName", warehouse.WW_WarehouseName, result["WarehouseName"]);
			AssertEquals("Unit Price", expectedUnitPrice, result["UnitPrice"]);
			AssertEquals("Unit Price Currency", expectUnitPriceCurrency, result["UnitPriceCurrency"]);
			AssertEquals("CommodityPK", expectedCommodityPK, result["CommodityPK"]);

			AssertEquals("Granularity", ccLocation.WCL_Granularity, result["Granularity"]);
			AssertEquals("ProductPK", ccVariance?.WCC_OP_Product ?? ZGuid.Empty, result["ProductPK"]);
			AssertEquals("PartAttrib1", ccVariance?.WCC_PartAttrib1 ?? "", result["PartAttrib1"]);
			AssertEquals("PartAttrib2", ccVariance?.WCC_PartAttrib2 ?? "", result["PartAttrib2"]);
			AssertEquals("PartAttrib3", ccVariance?.WCC_PartAttrib3 ?? "", result["PartAttrib3"]);
			AssertEquals("SerialNumber", ccVariance?.WCC_SerialNumber ?? "", result["SerialNumber"]);
			AssertEquals("ExpiryDate", ccVariance?.WCC_ExpiryDate ?? ZDate.Empty, result["ExpiryDate"]);
			AssertEquals("PackingDate", ccVariance?.WCC_PackingDate ?? ZDate.Empty, result["PackingDate"]);
			AssertEquals("EndTime", ccLocation.WCL_EndTime.FormatDateTimeOffset(), ((ZDateTimeOffset)result["EndTime"]).FormatDateTimeOffset());
			AssertEquals("PalletID", ccVariance?.WCC_PalletID ?? "", result["PalletID"]);

			AssertEquals("AssignedUser", ccLocation.WCL_GS_NKAssignedTo, result["AssignedUser"]);
			AssertEquals("ExpectedQty", ccVariance?.WCC_ExpectedQty ?? 0, result["ExpectedQty"]);
			AssertEquals("CountedQty", (ccVariance?.WCC_ExpectedQty ?? 0) + (ccVariance?.WCC_VarianceQty ?? 0), result["CountedQty"]);
			AssertEquals("VarianceQty", ccVariance?.WCC_VarianceQty ?? 0, result["VarianceQty"]);
			AssertEquals("AbsVarianceQty", Math.Abs(ccVariance?.WCC_VarianceQty ?? 0), result["AbsVarianceQty"]);
			AssertEquals("RecordWithNoVarianceTally", (ccVariance?.WCC_VarianceQty ?? 0) == 0 ? 1 : 0,
				result["RecordWithNoVarianceTally"]);
			AssertEquals("VarianceRecordTally", 1, result["VarianceRecordTally"]);
		}

		#endregion

		#region LoadView

		DynamicBusinessObjectCollection LoadView(ZGuid clientPK, ZGuid warehousePK, ZGuid productPK,
			ZDateTimeOffset? fromDate = null, ZDateTimeOffset? toDate = null)
		{
			var result = new DynamicBusinessObjectCollection(Factory);

			var sql = @"select * from WhsInventoryAccuracyReport(";
			sql += (clientPK.IsEmpty ? "null, " : "@ClientPK, ");
			sql += (warehousePK.IsEmpty ? "null, " : "@WarehousePK, ");
			sql += (productPK.IsEmpty ? "null, " : "@ProductPK, ");
			sql += (!fromDate.HasValue ? "'', " : "@FromDate, ");
			sql += (!toDate.HasValue ? "'')" : "@ToDate)");

			var sqlParams = new ZSqlParameterCollection();

			if (!clientPK.IsEmpty)
			{
				sqlParams.Add("@ClientPK", clientPK, OrgHeaderSchema.PK);
			}

			if (!warehousePK.IsEmpty)
			{
				sqlParams.Add("@WarehousePK", warehousePK, WhsWarehouseSchema.PK);
			}

			if (!productPK.IsEmpty)
			{
				sqlParams.Add("@ProductPK", productPK, OrgPartCategorySchema.PK);
			}

			if (fromDate.HasValue)
			{
				sqlParams.Add("@FromDate", fromDate, WhsCycleCountLocationSchema.WCL_EndTime);
			}

			if (toDate.HasValue)
			{
				sqlParams.Add("@ToDate", toDate, WhsCycleCountLocationSchema.WCL_EndTime);
			}

			if (sqlParams.Count != 0)
			{
				result.Load(sql, sqlParams);
			}
			else
			{
				result.Load(sql);
			}

			return result;
		}

		#endregion
	}
}
