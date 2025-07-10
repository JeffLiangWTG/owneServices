using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Core;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public abstract class WhsPickJobInfoTestCase<TPickInfo> : DataObjectInfoTestCase<TPickInfo>
		where TPickInfo : WhsPickJobInfo, new()
	{
		#region Related Business Objects

		#region TestLines

		public void TestPickLines()
		{
			var pickLinesCollection = new WhsPickLineInfoCollection();
			pickLinesCollection.Add(new WhsPickLineInfo());
			pickLinesCollection.Add(new WhsPickLineInfo());

			var pickJob = GetNewPickInfo();
			AssertEquals("Precondition", 0, pickJob.Lines.Count);
		}

		#endregion

		#region TestOrders

		public void TestOrders()
		{
			var orderInfoCollection = new WhsDocketInfoCollection();
			orderInfoCollection.Add(new WhsDocketInfo());
			orderInfoCollection.Add(new WhsDocketInfo());

			var pickJob = GetNewPickInfo();
			AssertEquals("Precondition", 0, pickJob.Orders.Count);

			pickJob.Orders = orderInfoCollection;
			AssertEquals(orderInfoCollection, pickJob.Orders);
		}

		#endregion

		#region TestProductInfos

		public void TestProductInfos()
		{
			var pickJob = GetNewPickInfo();
			AssertEquals("Precondition", 0, pickJob.ProductInfos.Count);

			var productInfo1 = new WhsProductInfo();
			var productInfo2 = new WhsProductInfo();
			pickJob.ProductInfos.AddRange(new[] { productInfo1, productInfo2 });

			AssertEquals("Precondition", 2, pickJob.ProductInfos.Count);
			pickJob.ProductInfos.Single(p => p == productInfo1);
			pickJob.ProductInfos.Single(p => p == productInfo2);
		}

		#endregion

		#region TestProductPartAttributesInfos

		public void TestProductPartAttributesInfos()
		{
			var pickJob = GetNewPickInfo();
			AssertEquals("Precondition", 0, pickJob.ProductPartAttributesInfos.Count);

			var productPartAttribInfo1 = new WhsProductPartAttributesInfo();
			var productPartAttribInfo2 = new WhsProductPartAttributesInfo();
			pickJob.ProductPartAttributesInfos.AddRange(new[] { productPartAttribInfo1, productPartAttribInfo2 });

			AssertEquals("Precondition", 2, pickJob.ProductPartAttributesInfos.Count);
			pickJob.ProductPartAttributesInfos.Single(p => p == productPartAttribInfo1);
			pickJob.ProductPartAttributesInfos.Single(p => p == productPartAttribInfo2);
		}

		#endregion

		#region TestScannedRCASerialNumbers

		public void TestScannedRCASerialNumbers()
		{
			var pickJob = GetNewPickInfo();
			AssertEquals("Precondition", 0, pickJob.ScannedRCASerialNumbersPerProduct.Count);

			var product1 = Guid.NewGuid();
			var product2 = Guid.NewGuid();
			var client = Guid.NewGuid();
			pickJob.ScannedRCASerialNumbersPerProduct.Add(new ScannedRCASerialNumbersPerProductInfo(product1, client, new List<string> { "S1", "S2" }));
			pickJob.ScannedRCASerialNumbersPerProduct.Add(new ScannedRCASerialNumbersPerProductInfo(product2, client, new List<string> { "S3", "S4" }));

			AssertEquals("Precondition", 2, pickJob.ScannedRCASerialNumbersPerProduct.Count);
			var scannedRCASerialNumbersForPart1 = pickJob.ScannedRCASerialNumbersPerProduct.Single(s => s.ProductPK == product1);
			AssertContainsExactElementsInAnyOrder(new[] { "S1", "S2" }, scannedRCASerialNumbersForPart1.ScannedRCASerialNumbers);
			var scannedRCASerialNumbersForPart2 = pickJob.ScannedRCASerialNumbersPerProduct.Single(s => s.ProductPK == product2);
			AssertContainsExactElementsInAnyOrder(new[] { "S3", "S4" }, scannedRCASerialNumbersForPart2.ScannedRCASerialNumbers);
		}

		#endregion

		#region TestUnitConversionsPerProduct

		public void TestUnitConversionsPerProduct()
		{
			var pickJob = GetNewPickInfo();
			AssertEquals("Precondition", 0, pickJob.UnitConversionsPerProduct.Count);

			var unitConversions1 = new UnitConversionCollection();
			var unitConversions2 = new UnitConversionCollection();
			pickJob.UnitConversionsPerProduct.AddRange(new[] { unitConversions1, unitConversions2 });

			AssertEquals("Precondition", 2, pickJob.UnitConversionsPerProduct.Count);
			pickJob.UnitConversionsPerProduct.Single(p => p == unitConversions1);
			pickJob.UnitConversionsPerProduct.Single(p => p == unitConversions2);
		}

		#endregion

		#endregion

		#region Properties

		public void TestProperties()
		{
			var pickJobInfo = GetNewPickInfo();
			AssertEquals("TrolleyJobPK", Guid.Empty, pickJobInfo.PK);
			AssertEquals("IsPickByBiggestPackTypeEnabled", false, pickJobInfo.IsPickByBiggestPackTypeEnabled);
			AssertEquals("IsPickByUOMTypeEnabled", false, pickJobInfo.IsPickByUOMTypeEnabled);
			AssertEquals("IsPickHasBOMEnabledProduct", false, pickJobInfo.IsPickHasBOMEnabledProduct);
			AssertEquals("IsPickHasBOMEnabledProduct", false, pickJobInfo.IsUsingDirectedPackingConsolidation);

			var pk = Guid.NewGuid();
			pickJobInfo.PK = pk;
			pickJobInfo.IsPickByBiggestPackTypeEnabled = true;
			pickJobInfo.IsPickByUOMTypeEnabled = true;
			pickJobInfo.IsPickHasBOMEnabledProduct = true;
			pickJobInfo.IsUsingDirectedPackingConsolidation = true;
			AssertEquals("TrolleyJobPK", pk, pickJobInfo.PK);
			AssertEquals("IsPickByBiggestPackTypeEnabled", true, pickJobInfo.IsPickByBiggestPackTypeEnabled);
			AssertEquals("IsPickByUOMTypeEnabled", true, pickJobInfo.IsPickByUOMTypeEnabled);
			AssertEquals("IsPickHasBOMEnabledProduct", true, pickJobInfo.IsPickHasBOMEnabledProduct);
			AssertEquals("IsPickHasBOMEnabledProduct", true, pickJobInfo.IsUsingDirectedPackingConsolidation);
		}

		#endregion

		#region TestPickPKs

		public void TestPickPKs()
		{
			TestPickPKsCore();
		}

		protected abstract void TestPickPKsCore();

		#endregion

		#region TestPopulateUnitConversionsPerProduct

		public void TestPopulateUnitConversionsPerProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m);
			Factory.Save(); // to create stock

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 100m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", 2, pick.GetAllPickLines().Count());

			var pickJobInfo = GetNewPickInfo();
			AssertEquals("Precondition", 0, pickJobInfo.UnitConversionsPerProduct.Count);

			WhsPickJobInfo.PopulateUnitConversionsPerProduct(pick.GetAllPickLines(), pickJobInfo);
			AssertEquals("Should have populated UnitConversionsPerProduct.", 1, pickJobInfo.UnitConversionsPerProduct.Count);
			AssertEquals("Should have populated UnitConversionsPerProduct.", 2, pickJobInfo.UnitConversionsPerProduct.Single().Conversions.Count);
			AssertNotNull("Should have populated UnitConversionsPerProduct.", pickJobInfo.UnitConversionsPerProduct.Single().Conversions.Single(c => c.PackType == Constants.PkgUnit.Unit && c.Qty == 1m));
			AssertNotNull("Should have populated UnitConversionsPerProduct.", pickJobInfo.UnitConversionsPerProduct.Single().Conversions.Single(c => c.PackType == Constants.PkgUnit.Carton && c.Qty == 12m));

			AssertNoExceptionThrown(() => WhsPickJobInfo.PopulateUnitConversionsPerProduct(Enumerable.Empty<WhsPickLine>(), pickJobInfo));
		}

		#endregion

		#region TestIsPutawayOnly

		public void TestIsPutawayOnly()
		{
			TestIsPutawayOnlyCore();
		}

		protected abstract void TestIsPutawayOnlyCore();

		#endregion

		#region TestDockDoorLocation

		public void TestDockDoorLocation()
		{
			TestDockDoorLocationCore();
		}

		protected abstract void TestDockDoorLocationCore();

		public void TestDockDoorLocation_FixedWidthLocation()
		{
			TestDockDoorLocation_FixedWidthLocationCore();
		}

		protected abstract void TestDockDoorLocation_FixedWidthLocationCore();

		#endregion

		#region TestIsPackingStationAllowed

		public void TestIsPackingStationAllowed()
		{
			TestIsPackingStationAllowedCore(hasPackingStationLocation: true);
		}

		public void TestIsPackingStationAllowed_WarehouseWithNoPackingStation()
		{
			TestIsPackingStationAllowedCore(hasPackingStationLocation: false);
		}

		protected abstract void TestIsPackingStationAllowedCore(bool hasPackingStationLocation);

		public void TestIsPackingStationAllowed_PackingStationIsVoided()
		{
			TestIsPackingStationAllowed_PackingStationIsInvalidLocationStatusCore(LocationStatus.Codes.Void);
		}

		public void TestIsPackingStationAllowed_PackingStationIsForDamagedGoods()
		{
			TestIsPackingStationAllowed_PackingStationIsInvalidLocationStatusCore(LocationStatus.Codes.Damaged);
		}

		public void TestIsPackingStationAllowed_PackingStationIsForHeldGoods()
		{
			TestIsPackingStationAllowed_PackingStationIsInvalidLocationStatusCore(LocationStatus.Codes.Held);
		}

		protected abstract void TestIsPackingStationAllowed_PackingStationIsInvalidLocationStatusCore(string locationStatus);

		public void TestIsPackingStationAllowed_PickWithAssignedPackingStation()
		{
			TestIsPackingStationAllowed_PickWithAssignedPackingStationCore();
		}

		protected abstract void TestIsPackingStationAllowed_PickWithAssignedPackingStationCore();

		public void TestIsPackingStationAllowed_PickByBOM()
		{
			TestIsPackingStationAllowed_PickByBOMCore();
		}

		protected abstract void TestIsPackingStationAllowed_PickByBOMCore();

		public void TestIsPackingStationAllowed_DBHits()
		{
			TestIsPackingStationAllowed_DBHitsCore();
		}

		protected abstract void TestIsPackingStationAllowed_DBHitsCore();

		#endregion

		#region TestPackingStationPK

		public void TestPackingStationPK()
		{
			TestPackingStationPKCore();
		}

		protected abstract void TestPackingStationPKCore();

		#endregion

		#region TestIsUsingDirectedPackingConsolidation

		public void TestIsUsingDirectedPackingConsolidation_PickWithLooseInventory()
		{
			TestIsUsingDirectedPackingConsolidation_PickWithLooseInventoryCore();
		}

		public void TestIsUsingDirectedPackingConsolidation_PickWithLooseInventoryAlreadyPutaway()
		{
			TestIsUsingDirectedPackingConsolidation_PickWithLooseInventoryAlreadyPutawayCore();
		}

		protected abstract void TestIsUsingDirectedPackingConsolidation_PickWithLooseInventoryCore();
		protected abstract void TestIsUsingDirectedPackingConsolidation_PickWithLooseInventoryAlreadyPutawayCore();

		#endregion

		#region TestAllowPickDockDoorLocationOverride

		public void TestAllowPickDockDoorLocationOverride()
		{
			TestAllowPickDockDoorLocationOverrideCore();
		}

		public void TestAllowPickDockDoorLocationOverride_LinkedPicks()
		{
			TestAllowPickDockDoorLocationOverride_LinkedPicksCore();
		}

		protected abstract void TestAllowPickDockDoorLocationOverrideCore();
		protected abstract void TestAllowPickDockDoorLocationOverride_LinkedPicksCore();

		#endregion

		#region TestAssignedPutawayLocation

		public void TestAssignedPutawayLocation_PackingStation()
		{
			TestAssignedPutawayLocation_PackingStationCore();
		}

		public void TestAssignedPutawayLocation_DockDoor()
		{
			TestAssignedPutawayLocation_DockDoorCore();
		}

		public void TestAssignedPutawayLocation_ConsolidationLocation()
		{
			TestAssignedPutawayLocation_ConsolidationLocationCore();
		}

		public void TestAssignedPutawayLocation_NothingPutawayYet()
		{
			TestAssignedPutawayLocation_NothingPutawayYetCore();
		}

		protected abstract void TestAssignedPutawayLocation_PackingStationCore();
		protected abstract void TestAssignedPutawayLocation_DockDoorCore();
		protected abstract void TestAssignedPutawayLocation_ConsolidationLocationCore();
		protected abstract void TestAssignedPutawayLocation_NothingPutawayYetCore();

		#endregion

		#region Implementation

		protected new TPickInfo Parent => (TPickInfo)base.Parent;

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return GetNewPickInfo();
		}

		protected abstract TPickInfo GetNewPickInfo();

		#endregion
	}
}
