using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsOrderLineShortfallQuantityHelperTest : WhsTestCaseWithFactory
	{
		#region TestReCalculateShortFallConsideringBOMComponents

		public void TestReCalculateShortFallConsideringBOMComponents()
		{
			SetupData();

			var quantity = ShortfallQuantityHelper.ReCalculateShortFallConsideringBOMComponents(5m, mainProduct, data.Org1.PK, data.Whs1.PK);
			AssertEquals("Shortfall 3, as 5 main product + 2 mainProduct can be created from 6 bomComponentProduct1 and 2 bomComponentProduct2", 3m, quantity);
		}

		#endregion

		#region TestPossibleProductCanBeassembledFromItsComponent

		public void TestPossibleProductCanBeassembledFromItsComponent()
		{
			SetupData();

			var quantity = ShortfallQuantityHelper.PossibleProductQtyCanBeAssembledFromItsComponents(mainProduct, data.Org1.PK, data.Whs1.PK);
			AssertEquals("2 main product can be created from 6 bomComponentProduct1 and 18 bomComponentProduct2", 2m, quantity);
		}

		#endregion
		#region TestPossibleProductCanBeAssembledFromPickLineQuantityByItsComponent

		public void TestPossibleProductCanBeAssembledFromPickLineQuantityByItsComponent()
		{
			SetupData();

			var pick = Helper.CreatePickNew(orderLine.PickableDocket);
			var pickOrderInventoryForMainProduct = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().SingleOrDefault(i => i.SupplierPart.PK == mainProduct.PK);
			AssertEquals("Shortfall 3, as 5 main product + 2 mainProduct can be created from 2 components.", 3m, pickOrderInventoryForMainProduct.QuantityShort);
		}

		#endregion

		#region TestPossibleProductCanBeAssembledFromPickLineQuantityByItsComponent_DifferentUnits

		public void TestPossibleProductCanBeAssembledFromPickLineQuantityByItsComponent_DifferentUnits()
		{
			data = new TestDataSimpleEnvironment(Factory, 2, 1);
			mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			bomComponentProduct1 = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct1 = Helper.CreateProductBOM(mainProduct, bomComponentProduct1, 1m, Constants.PkgUnit.Unit);

			var bomComponentProduct2 = Helper.CreateProduct(data.Org1, "BOM2");
			Helper.CreateProductUnit(bomComponentProduct2, Constants.PkgUnit.Case, 2);
			var bomPartForMainProduct2 = Helper.CreateProductBOM(mainProduct, bomComponentProduct2, 2m, Constants.PkgUnit.Case);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 5m, inventoryLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct1, 20m, inventoryLocation);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct2, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var cartonSize = bomComponentProduct2.UnitConverter.Convert(1m, Constants.PkgUnit.Case, Constants.PkgUnit.Unit);
			AssertEquals("Unit Convert should return 2", 2m, cartonSize);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 15m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var pickOrderInventoryForMainProduct = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(i => i.SupplierPart.PK == mainProduct.PK);
			AssertEquals("Shortfall 5, as 5 main product + 5 mainProduct can be created from components (BOM1: 5, BOM2: 20 [5 X 2case X 2Unit per Case]", 5m, pickOrderInventoryForMainProduct.QuantityShort);
		}

		#endregion

		#region TestPossibleProductCanBeAssembled_IgnoresCommittedInventory

		public void TestPossibleProductCanBeAssembled_IgnoresCommittedInventory()
		{
			data = new TestDataSimpleEnvironment(Factory, 2, 1);
			mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			bomComponentProduct1 = Helper.CreateProduct(data.Org1, "BOM1");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct1, 3m, Constants.PkgUnit.Unit); //So 3 bomComponentProduct can create on mainProduct

			var bomComponentProduct2 = Helper.CreateProduct(data.Org1, "BOM2");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct2, 9m, Constants.PkgUnit.Unit); //So 9 bomComponentProduct can create on mainProduct

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 5m, inventoryLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct1, 20m, inventoryLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct2, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var orderForComponent = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", bomComponentProduct2, 3m);
			var pickForOrderForComponent = Helper.CreatePickNew(orderForComponent);

			// now as we picked 3 components#2 there are 17 left available, so only 1 kit can be built.

			var orderForKits = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			orderLine = Helper.CreateWhsOrderLine(orderForKits, mainProduct, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(orderForKits);
			var pickOrderInventoryForMainProduct = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(i => i.SupplierPart.PK == mainProduct.PK);
			AssertEquals("Shortfall should be 4, as 5 main product exists and 1 mainProduct can be created from components", 4m, pickOrderInventoryForMainProduct.QuantityShort);
		}

		#endregion

		#region TestPossibleProductCanBeAssembledFromItsComponent_DifferentUnits

		public void TestPossibleProductCanBeAssembledFromItsComponent_DifferentUnits()
		{
			data = new TestDataSimpleEnvironment(Factory, 2, 1);
			mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			bomComponentProduct1 = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct1 = Helper.CreateProductBOM(mainProduct, bomComponentProduct1, 1m, Constants.PkgUnit.Unit);

			var bomComponentProduct2 = Helper.CreateProduct(data.Org1, "BOM2");
			Helper.CreateProductUnit(bomComponentProduct2, Constants.PkgUnit.Case, 2);
			var bomPartForMainProduct2 = Helper.CreateProductBOM(mainProduct, bomComponentProduct2, 2m, Constants.PkgUnit.Case);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 5m, inventoryLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct1, 20m, inventoryLocation);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct2, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var cartonSize = bomComponentProduct2.UnitConverter.Convert(1m, Constants.PkgUnit.Case, Constants.PkgUnit.Unit);
			AssertEquals("Unit Convert should return 2", 2m, cartonSize);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 10m);
			Factory.Save();

			var quantity = ShortfallQuantityHelper.PossibleProductQtyCanBeAssembledFromItsComponents(mainProduct, data.Org1.PK, data.Whs1.PK);
			AssertEquals("Possible Product Can Be Assembled From Its Component should be 5", 5m, quantity);

			var pick = Helper.CreatePickNew(orderLine.PickableDocket);
			var orderlineForBomProduct1 = orderLine.ChildComponentLines.SingleOrDefault(i => i.SupplierPart.PK == bomComponentProduct1.PK);
			var orderlineForBomProduct2 = orderLine.ChildComponentLines.SingleOrDefault(i => i.SupplierPart.PK == bomComponentProduct2.PK);

			AssertPackAndUnits(orderlineForBomProduct1, 5m, 5m, Constants.PkgUnit.Unit);
			AssertPackAndUnits(orderlineForBomProduct2, 10m, 10m, Constants.PkgUnit.Case);
		}

		void AssertPackAndUnits(WhsPickableDocketLine orderlineForBomProduct, ZDecimal packQuantity, ZDecimal packUnit, ZString packType)
		{
			AssertEquals("Correct Pack Quantity for child line", packQuantity, orderlineForBomProduct.WE_PackQuantity);
			AssertEquals("Correct Pack Unit for child line", packUnit, orderlineForBomProduct.WE_PackQuantity);
			AssertEquals("Correct Pack Type for child line", packType, orderlineForBomProduct.WE_F3_NKPackType);
		}

		#endregion

		#region TestAvailableQuantityOfBOMComponentProduct

		public void TestAvailableQuantityOfBOMComponentProduct()
		{
			SetupData();

			var pick = Helper.CreatePickNew(orderLine.PickableDocket);
			var availableQuantity = ShortfallQuantityHelper.GetAvailableComponentsToPickQuantity(bomComponentProduct1.PK, orderLine.PickableDocket.Client.PK, orderLine.PickableDocket.Warehouse.PK);
			AssertEquals("Precondition", 14m, availableQuantity);
		}

		#endregion

		#region SetupData

		TestDataSimpleEnvironment data;
		OrgSupplierPart mainProduct;
		OrgSupplierPart bomComponentProduct1;
		WhsOrderLine orderLine;

		void SetupData()
		{
			data = new TestDataSimpleEnvironment(Factory, 2, 1);
			mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			bomComponentProduct1 = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct1 = Helper.CreateProductBOM(mainProduct, bomComponentProduct1, 3m, Constants.PkgUnit.Unit); //So 3 bomComponentProduct can create on mainProduct

			var bomComponentProduct2 = Helper.CreateProduct(data.Org1, "BOM2");
			var bomPartForMainProduct2 = Helper.CreateProductBOM(mainProduct, bomComponentProduct2, 9m, Constants.PkgUnit.Unit); //So 9 bomComponentProduct can create on mainProduct

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 5m, inventoryLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct1, 20m, inventoryLocation);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct2, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 10m);
			Factory.Save();
		}

		#endregion

		#region ShortfallQuantityHelper

		public WhsOrderLineShortfallQuantityHelper ShortfallQuantityHelper
		{
			get { return shortfallQuantityHelper ?? (shortfallQuantityHelper = new WhsOrderLineShortfallQuantityHelper(Factory)); }
		}
		WhsOrderLineShortfallQuantityHelper shortfallQuantityHelper;

		#endregion
	}
}
