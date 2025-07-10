using System;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsCycleCountLocationHelperTest : WhsTestCaseWithFactory
	{
		public void TestGetOpenNegativeVariancesMatchingInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "REC1", data.Part1, 5m, location2, "");
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount1 = Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			Helper.CreateWhsCycleCountLocationVariance(cycleCount1, CycleCountVarianceStatus.Codes.Open, palletID: "", client: data.Org1, part: data.Part1, varianceQty: 5, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			var cycleCount2 = Helper.CreateWhsCycleCountLocation(location2, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var cycleCount2Variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount2, CycleCountVarianceStatus.Codes.Open, palletID: "", client: data.Org1, part: data.Part1, varianceQty: -5, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);
			Factory.Save();

			var inventory = receive.Inventory[0];
			var relatedNegativeVariance = WhsCycleCountLocationHelper.GetOpenNegativeVariancesMatchingInventory(Factory, inventory, -inventory.WI_TotalUnits);
			AssertNotNull(relatedNegativeVariance);
			AssertEquals(relatedNegativeVariance.PK, cycleCount2Variance.PK);
		}

		public void TestGetOpenNegativeVariancesMatchingInventory_WrongQty()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "REC1", data.Part1, 5m, location2, "");
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount1 = Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			Helper.CreateWhsCycleCountLocationVariance(cycleCount1, CycleCountVarianceStatus.Codes.Open, palletID: "", client: data.Org1, part: data.Part1, varianceQty: 5, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			var cycleCount2 = Helper.CreateWhsCycleCountLocation(location2, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			Helper.CreateWhsCycleCountLocationVariance(cycleCount2, CycleCountVarianceStatus.Codes.Open, palletID: "", client: data.Org1, part: data.Part1, varianceQty: -5, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);
			Factory.Save();

			var inventory = receive.Inventory[0];
			AssertNull(WhsCycleCountLocationHelper.GetOpenNegativeVariancesMatchingInventory(Factory, inventory, -1));
		}

		public void TestGetOpenNegativeVariancesMatchingInventory_WithPalletID()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "REC1", data.Part1, 5m, location2, "PLT");
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount1 = Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			Helper.CreateWhsCycleCountLocationVariance(cycleCount1, CycleCountVarianceStatus.Codes.Open, palletID: "PLT", client: data.Org1, part: data.Part1, varianceQty: 5, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			var cycleCount2 = Helper.CreateWhsCycleCountLocation(location2, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var cycleCount2Variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount2, CycleCountVarianceStatus.Codes.Open, palletID: "PLT", client: data.Org1, part: data.Part1, varianceQty: -5, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);
			Factory.Save();

			var inventory = receive.Inventory[0];
			var relatedNegativeVariance = WhsCycleCountLocationHelper.GetOpenNegativeVariancesMatchingInventory(Factory, inventory, -inventory.WI_TotalUnits);
			AssertNotNull(relatedNegativeVariance);
			AssertEquals(relatedNegativeVariance.PK, cycleCount2Variance.PK);
		}

		public void TestGetOpenNegativeVariancesMatchingInventory_WithAttribute_Attrib1()
		{
			TestGetOpenNegativeVariancesMatchingInventory_WithAttributeCore(
				AttributeNumber.One,
				(receiveLine) => receiveLine.WE_PartAttrib1 = "ABC",
				(variance) => variance.WCC_PartAttrib1 = "ABC");
		}

		public void TestGetOpenNegativeVariancesMatchingInventory_WithAttribute_Attrib2()
		{
			TestGetOpenNegativeVariancesMatchingInventory_WithAttributeCore(
				AttributeNumber.Two,
				(receiveLine) => receiveLine.WE_PartAttrib2 = "ABC",
				(variance) => variance.WCC_PartAttrib2 = "ABC");
		}

		public void TestGetOpenNegativeVariancesMatchingInventory_WithAttribute_Attrib3()
		{
			TestGetOpenNegativeVariancesMatchingInventory_WithAttributeCore(
				AttributeNumber.Three,
				(receiveLine) => receiveLine.WE_PartAttrib3 = "ABC",
				(variance) => variance.WCC_PartAttrib3 = "ABC");
		}

		public void TestGetOpenNegativeVariancesMatchingInventory_WithAttribute_Serial()
		{
			TestGetOpenNegativeVariancesMatchingInventory_WithAttributeCore(
				AttributeNumber.Serial,
				(receiveLine) => receiveLine.WE_SerialNumber = "ABC",
				(variance) => variance.WCC_SerialNumber = "ABC");
		}

		public void TestGetOpenNegativeVariancesMatchingInventory_WithAttribute_PackingDate()
		{
			var date = ZDate.Today;
			TestGetOpenNegativeVariancesMatchingInventory_WithAttributeCore(
				AttributeNumber.PackingDate,
				(receiveLine) => receiveLine.WE_PackingDate = date,
				(variance) => variance.WCC_PackingDate = date);
		}

		public void TestGetOpenNegativeVariancesMatchingInventory_WithAttribute_ExpiryDate()
		{
			var date = ZDate.Today;
			TestGetOpenNegativeVariancesMatchingInventory_WithAttributeCore(
				AttributeNumber.ExpiryDate,
				(receiveLine) => receiveLine.WE_ExpiryDate = date,
				(variance) => variance.WCC_ExpiryDate = date);
		}

		public void TestGetOpenNegativeVariancesMatchingInventory_WithAttributeCore(
			AttributeNumber attributeNumber,
			Action<WhsReceiveLine> receiveLineAttributeSetter,
			Action<WhsCycleCountLocationVariance> varianceAttributeSetter)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			Helper.SetClientAttributeType(data.Org1, attributeNumber, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeNumber, use: true, setReleaseCaptured: false);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, location2);
			receiveLineAttributeSetter(receiveLine);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount1 = Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var cycleCount1Variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount1, CycleCountVarianceStatus.Codes.Open, palletID: "", client: data.Org1, part: data.Part1, varianceQty: 5, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);
			varianceAttributeSetter(cycleCount1Variance);

			var cycleCount2 = Helper.CreateWhsCycleCountLocation(location2, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var cycleCount2Variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount2, CycleCountVarianceStatus.Codes.Open, palletID: "", client: data.Org1, part: data.Part1, varianceQty: -5, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);
			varianceAttributeSetter(cycleCount2Variance);
			Factory.Save();

			var inventory = receiveLine.Inventory[0];
			var relatedNegativeVariance = WhsCycleCountLocationHelper.GetOpenNegativeVariancesMatchingInventory(Factory, inventory, -inventory.WI_TotalUnits);
			AssertNotNull(relatedNegativeVariance);
			AssertEquals(relatedNegativeVariance.PK, cycleCount2Variance.PK);
		}
	}
}
