using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class ProductWithAttributesTest : WhsTestCaseWithFactory
	{
		#region TestDataIsComparable

		[TestDate(2013, 12, 5)]
		public void TestDataIsComparable()
		{
			var product1 = new ProductWithAttributes();
			var product2 = new ProductWithAttributes();
			AssertEquals(product1, product2);

			var dictionary = new HashSet<ProductWithAttributes>();
			dictionary.Add(product1);
			AssertEquals(true, dictionary.Contains(product2));

			var clientPK = ZGuid.NewZGuid();
			var productPK = ZGuid.NewZGuid();
			var product3 = new ProductWithAttributes(clientPK, productPK, "PA1", "PA2", "PA3", "SNN", ZDate.Today, ZDate.Today.AddDays(-5), "KEY-1", "ALO-1", 0m);
			var product4 = new ProductWithAttributes(clientPK, productPK, "PA1", "PA2", "PA3", "SNN", ZDate.Today, ZDate.Today.AddDays(-5), "KEY-1", "ALO-1", 0m);
			dictionary.Add(product3);
			AssertEquals(true, dictionary.Contains(product4));
		}

		#endregion

		#region TestGetProductWithAttributes

		[TestDate(2013, 12, 20)]
		public void TestGetProductWithAttributes()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, today.AddDays(10), today.AddDays(-10), "PA1", "PA2", "PA3", "BEK-1");
			inventory.WI_SerialNumber = "SNN";
			inventory.WI_AllocationKey = "ALO-1";
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m, today.AddDays(10), today.AddDays(-10), "PA1", "PA2", "PA3", "BEK-1", "DummyOutward-1");
			orderLine.WE_SerialNumber = "SNN";
			orderLine.WE_AllocationKey = "ALO-1";
			var pick = Helper.CreatePickNew(order);

			var inventoryProductWithAttributes = ProductWithAttributes.GetProductWithAttributes(inventory, useBondedEntryKey: true);
			var orderedInventoryProductWithAttributes = ProductWithAttributes.GetProductWithAttributes(pick.OrderedInventories[0]);
			var availableInventoryProductWithAttributes = ProductWithAttributes.GetProductWithAttributes(pick.OrderedInventories[0].AvailableInventories[0]);
			var orderLineProductWithAttributes = ProductWithAttributes.GetProductWithAttributes(orderLine);
			AssertEquals(inventoryProductWithAttributes, orderedInventoryProductWithAttributes);

			AssertProductWithAttributes(inventoryProductWithAttributes, data.Org1.PK, data.Part1.PK, today.AddDays(10), today.AddDays(-10), "PA1", "PA2", "PA3", "SNN", "BEK-1", "ALO-1");
			AssertProductWithAttributes(orderedInventoryProductWithAttributes, data.Org1.PK, data.Part1.PK, today.AddDays(10), today.AddDays(-10), "PA1", "PA2", "PA3", "SNN", "BEK-1", "ALO-1");
			AssertProductWithAttributes(availableInventoryProductWithAttributes, data.Org1.PK, data.Part1.PK, today.AddDays(10), today.AddDays(-10), "PA1", "PA2", "PA3", "SNN", "BEK-1", "ALO-1");
			AssertProductWithAttributes(orderLineProductWithAttributes, data.Org1.PK, data.Part1.PK, today.AddDays(10), today.AddDays(-10), "PA1", "PA2", "PA3", "SNN", "BEK-1", "ALO-1");
		}

		public void TestGetProductWithAttributes_WithoutBondedEntryKeyOnInventory()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, today.AddDays(10), today.AddDays(-10), "PA1", "PA2", "PA3", "BEK-1");
			inventory.WI_SerialNumber = "SNN";
			inventory.WI_AllocationKey = "ALO-1";
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, today.AddDays(10), today.AddDays(-10), "PA1", "PA2", "PA3", "BEK-1", "DummyOutward-1");
			orderLine1.WE_SerialNumber = "SNN";
			orderLine1.WE_AllocationKey = "ALO-1";
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, today.AddDays(10), today.AddDays(-10), "PA1", "PA2", "PA3", "", "DummyOutward-1");
			orderLine2.WE_SerialNumber = "SNN";
			orderLine2.WE_AllocationKey = "ALO-1";
			var pick = Helper.CreatePickNew(order);

			var inventoryProductWithAttributesNoBEK = ProductWithAttributes.GetProductWithAttributes(inventory, useBondedEntryKey: false);
			var orderedInventory1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.BondedEntryKey == "BEK-1");
			var orderedInventory2 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.BondedEntryKey == "");
			var orderedInventory1ProductWithAttributes = ProductWithAttributes.GetProductWithAttributes(orderedInventory1);
			var orderedInventory2ProductWithAttributes = ProductWithAttributes.GetProductWithAttributes(orderedInventory2);
			var availableInventoryProductWithAttributes = ProductWithAttributes.GetProductWithAttributes(orderedInventory1.AvailableInventories[0]);
			var orderLine1ProductWithAttributes = ProductWithAttributes.GetProductWithAttributes(orderLine1);
			var orderLine2ProductWithAttributes = ProductWithAttributes.GetProductWithAttributes(orderLine2);
			AssertNotEquals(inventoryProductWithAttributesNoBEK, orderedInventory1ProductWithAttributes);

			// Uncomment when serial number is implemented on AvailableInventory
			AssertEquals(orderedInventory1ProductWithAttributes, availableInventoryProductWithAttributes);
			AssertEquals(inventoryProductWithAttributesNoBEK, orderedInventory2ProductWithAttributes);
			AssertNotEquals(orderedInventory2ProductWithAttributes, availableInventoryProductWithAttributes);

			AssertNotEquals(inventoryProductWithAttributesNoBEK, orderLine1ProductWithAttributes);
			AssertEquals(inventoryProductWithAttributesNoBEK, orderLine2ProductWithAttributes);
			AssertProductWithAttributes(inventoryProductWithAttributesNoBEK, data.Org1.PK, data.Part1.PK, today.AddDays(10), today.AddDays(-10), "PA1", "PA2", "PA3", "SNN", "", "ALO-1");
		}

		void AssertProductWithAttributes(
			ProductWithAttributes productWithAttributes,
			ZGuid expectedClientPK,
			ZGuid expectedProductPK,
			ZDate expectedExpiryDate,
			ZDate expectedPackingDate,
			string expectedPA1,
			string expectedPA2,
			string expectedPA3,
			string expectedSerialNum,
			string expectedBEK,
			string expectedAllocationKey)
		{
			AssertEquals(nameof(ProductWithAttributes.ClientPK), expectedClientPK, productWithAttributes.ClientPK);
			AssertEquals(nameof(ProductWithAttributes.ProductPK), expectedProductPK, productWithAttributes.ProductPK);
			AssertEquals(nameof(ProductWithAttributes.ExpiryDate), expectedExpiryDate, productWithAttributes.ExpiryDate);
			AssertEquals(nameof(ProductWithAttributes.PackingDate), expectedPackingDate, productWithAttributes.PackingDate);
			AssertEquals(nameof(ProductWithAttributes.PartAttrib1), expectedPA1, productWithAttributes.PartAttrib1);
			AssertEquals(nameof(ProductWithAttributes.PartAttrib2), expectedPA2, productWithAttributes.PartAttrib2);
			AssertEquals(nameof(ProductWithAttributes.PartAttrib3), expectedPA3, productWithAttributes.PartAttrib3);
			AssertEquals(nameof(ProductWithAttributes.SerialNumber), expectedSerialNum, productWithAttributes.SerialNumber);
			AssertEquals(nameof(ProductWithAttributes.BondedEntryKey), expectedBEK, productWithAttributes.BondedEntryKey);
			AssertEquals(nameof(ProductWithAttributes.AllocationKey), expectedAllocationKey, productWithAttributes.AllocationKey);
		}

		#endregion

		#region TestIsMatchingWithEmptyCheck

		public void TestIsMatchingWithEmptyCheck()
		{
			var client1 = ZGuid.NewZGuid();
			var client2 = ZGuid.NewZGuid();
			var product1 = ZGuid.NewZGuid();
			var product2 = ZGuid.NewZGuid();
			var today = ZDate.Today;
			var empty = new ProductWithAttributes(ZGuid.Empty, ZGuid.Empty, "", "", "", "", ZDate.Empty, ZDate.Empty, "", "", 0);
			var inBetween1 = new ProductWithAttributes(client1, ZGuid.Empty, "", "", "", "", ZDate.Empty, ZDate.Empty, "", "", 0m);
			var inBetween2 = new ProductWithAttributes(client1, product1, "", "", "", "", ZDate.Empty, ZDate.Empty, "", "", 0m);
			var inBetween3 = new ProductWithAttributes(client1, product1, "PA1", "", "", "", ZDate.Empty, ZDate.Empty, "", "", 0m);
			var inBetween4 = new ProductWithAttributes(client1, product1, "PA1", "PA2", "", "", ZDate.Empty, ZDate.Empty, "", "", 0m);
			var inBetween5 = new ProductWithAttributes(client1, product1, "PA1", "PA2", "PA3", "", ZDate.Empty, ZDate.Empty, "", "", 0m);
			var inBetween6 = new ProductWithAttributes(client1, product1, "PA1", "PA2", "PA3", "SN", ZDate.Empty, ZDate.Empty, "", "", 0m);
			var inBetween7 = new ProductWithAttributes(client1, product1, "PA1", "PA2", "PA3", "SN", today.AddDays(5), ZDate.Empty, "", "", 0m);
			var inBetween8 = new ProductWithAttributes(client1, product1, "PA1", "PA2", "PA3", "SN", today.AddDays(5), today.AddDays(-5), "", "", 0m);
			var inBetween9 = new ProductWithAttributes(client1, product1, "PA1", "PA2", "PA3", "SN", today.AddDays(5), today.AddDays(-5), "KEY-1", "", 0m);
			var inBetween10 = new ProductWithAttributes(client1, product1, "PA1", "PA2", "PA3", "SN", today.AddDays(5), today.AddDays(-5), "KEY-1", "ALO-1", 0m);
			var differentCasing = new ProductWithAttributes(client1, product1, "pA1", "Pa2", "pA3", "sN", today.AddDays(5), today.AddDays(-5), "KeY-1", "AlO-1", 5m);
			var full = new ProductWithAttributes(client1, product1, "PA1", "PA2", "PA3", "SN", today.AddDays(5), today.AddDays(-5), "KEY-1", "ALO-1", 5m);
			var differentClient = new ProductWithAttributes(client2, product1, "PA1", "PA2", "PA3", "SN", today.AddDays(5), today.AddDays(-5), "KEY-1", "ALO-1", 5m);
			var differentProduct = new ProductWithAttributes(client1, product2, "PA1", "PA2", "PA3", "SN", today.AddDays(5), today.AddDays(-5), "KEY-1", "ALO-1", 5m);

			// Client and Product should match exactly.
			AssertEquals(false, ProductWithAttributes.IsMatchingWithEmptyCheck(empty, inBetween1));
			AssertEquals(false, ProductWithAttributes.IsMatchingWithEmptyCheck(inBetween1, inBetween2));

			AssertEquals(false, ProductWithAttributes.IsMatchingWithEmptyCheck(full, differentClient));
			AssertEquals(false, ProductWithAttributes.IsMatchingWithEmptyCheck(differentClient, full));

			AssertEquals(false, ProductWithAttributes.IsMatchingWithEmptyCheck(full, differentProduct));
			AssertEquals(false, ProductWithAttributes.IsMatchingWithEmptyCheck(differentProduct, full));

			// Ordered item without attributes should match any inventory with attributes.
			AssertEquals(true, ProductWithAttributes.IsMatchingWithEmptyCheck(inBetween2, inBetween2));
			AssertEquals(true, ProductWithAttributes.IsMatchingWithEmptyCheck(inBetween2, inBetween3));
			AssertEquals(true, ProductWithAttributes.IsMatchingWithEmptyCheck(inBetween2, inBetween4));
			AssertEquals(true, ProductWithAttributes.IsMatchingWithEmptyCheck(inBetween2, inBetween5));
			AssertEquals(true, ProductWithAttributes.IsMatchingWithEmptyCheck(inBetween2, inBetween6));
			AssertEquals(true, ProductWithAttributes.IsMatchingWithEmptyCheck(inBetween2, inBetween7));
			AssertEquals(true, ProductWithAttributes.IsMatchingWithEmptyCheck(inBetween2, inBetween8));
			AssertEquals(true, ProductWithAttributes.IsMatchingWithEmptyCheck(inBetween2, inBetween9));
			AssertEquals(true, ProductWithAttributes.IsMatchingWithEmptyCheck(inBetween2, inBetween10));
			AssertEquals(true, ProductWithAttributes.IsMatchingWithEmptyCheck(inBetween2, differentCasing));

			// Inventory without attributes should not match ordered items with attributes.
			AssertEquals(false, ProductWithAttributes.IsMatchingWithEmptyCheck(inBetween3, inBetween2));
			AssertEquals(false, ProductWithAttributes.IsMatchingWithEmptyCheck(inBetween4, inBetween2));
			AssertEquals(false, ProductWithAttributes.IsMatchingWithEmptyCheck(inBetween5, inBetween2));
			AssertEquals(false, ProductWithAttributes.IsMatchingWithEmptyCheck(inBetween6, inBetween2));
			AssertEquals(false, ProductWithAttributes.IsMatchingWithEmptyCheck(inBetween7, inBetween2));
			AssertEquals(false, ProductWithAttributes.IsMatchingWithEmptyCheck(inBetween8, inBetween2));
			AssertEquals(false, ProductWithAttributes.IsMatchingWithEmptyCheck(inBetween9, inBetween2));
			AssertEquals(false, ProductWithAttributes.IsMatchingWithEmptyCheck(inBetween10, inBetween2));

			// Per package qty doesn't matter for this comparison.
			AssertEquals(true, ProductWithAttributes.IsMatchingWithEmptyCheck(full, inBetween10));
			AssertEquals(true, ProductWithAttributes.IsMatchingWithEmptyCheck(inBetween10, full));

			// Casing should not matter.
			AssertEquals(true, ProductWithAttributes.IsMatchingWithEmptyCheck(full, differentCasing));
			AssertEquals(true, ProductWithAttributes.IsMatchingWithEmptyCheck(differentCasing, full));
		}

		#endregion

		#region TestEquals

		public void TestEquals()
		{
			var clientPK1 = ZGuid.NewZGuid();
			var clientPK2 = ZGuid.NewZGuid();
			var productPK1 = ZGuid.NewZGuid();
			var productPK2 = ZGuid.NewZGuid();
			var productWithAttributes1 = new ProductWithAttributes(clientPK1, productPK1, "PA1", "PA2", "PA3", "SN", ZDate.Today, ZDate.Today.AddDays(-5), "KEY-1", "ALO-1", 1m);
			var productWithAttributesDifferentClient = new ProductWithAttributes(clientPK2, productPK1, "PA1", "PA2", "PA3", "SN", ZDate.Today, ZDate.Today.AddDays(-5), "KEY-1", "ALO-1", 1m);
			var productWithAttributesDifferentProduct = new ProductWithAttributes(clientPK1, productPK2, "PA1", "PA2", "PA3", "SN", ZDate.Today, ZDate.Today.AddDays(-5), "KEY-1", "ALO-1", 1m);
			var productWithAttributesDifferentPA1 = new ProductWithAttributes(clientPK1, productPK1, "PA11", "PA2", "PA3", "SN", ZDate.Today, ZDate.Today.AddDays(-5), "KEY-1", "ALO-1", 1m);
			var productWithAttributesDifferentPA2 = new ProductWithAttributes(clientPK1, productPK1, "PA1", "PA22", "PA3", "SN", ZDate.Today, ZDate.Today.AddDays(-5), "KEY-1", "ALO-1", 1m);
			var productWithAttributesDifferentPA3 = new ProductWithAttributes(clientPK1, productPK1, "PA1", "PA2", "PA33", "SN", ZDate.Today, ZDate.Today.AddDays(-5), "KEY-1", "ALO-1", 1m);
			var productWithAttributesDifferentSN = new ProductWithAttributes(clientPK1, productPK1, "PA1", "PA2", "PA3", "SNN", ZDate.Today, ZDate.Today.AddDays(-5), "KEY-1", "ALO-1", 1m);
			var productWithAttributesDifferentExpiryDate = new ProductWithAttributes(clientPK1, productPK1, "PA1", "PA2", "PA3", "SN", ZDate.Today.AddDays(1), ZDate.Today.AddDays(-5), "KEY-1", "ALO-1", 1m);
			var productWithAttributesDifferentPackingDate = new ProductWithAttributes(clientPK1, productPK1, "PA1", "PA2", "PA3", "SN", ZDate.Today, ZDate.Today.AddDays(-4), "KEY-1", "ALO-1", 1m);
			var productWithAttributesDifferentBondedEntryKey = new ProductWithAttributes(clientPK1, productPK1, "PA1", "PA2", "PA3", "SN", ZDate.Today, ZDate.Today.AddDays(-5), "KEY-2", "ALO-1", 1m);
			var productWithAttributesDifferentAllocationKeyKey = new ProductWithAttributes(clientPK1, productPK1, "PA1", "PA2", "PA3", "SN", ZDate.Today, ZDate.Today.AddDays(-5), "KEY-1", "ALO-2", 1m);
			var productWithAttributesDifferentCasing = new ProductWithAttributes(clientPK1, productPK1, "pA1", "Pa2", "pA3", "sN", ZDate.Today, ZDate.Today.AddDays(-5), "KeY-1", "AlO-1", 1m);
			var productWithAttributesDifferentQuantity = new ProductWithAttributes(clientPK1, productPK1, "PA1", "PA2", "PA3", "SN", ZDate.Today, ZDate.Today.AddDays(-5), "KEY-1", "ALO-1", 2m);
			var productWithAttributes2 = new ProductWithAttributes(clientPK1, productPK1, "PA1", "PA2", "PA3", "SN", ZDate.Today, ZDate.Today.AddDays(-5), "KEY-1", "ALO-1", 1m);

			AssertEquals("productWithAttributes1 and productWithAttributes2 should be equal.", true, productWithAttributes1.Equals(productWithAttributes2));
			AssertEquals("productWithAttributes1 and productWithAttributesDifferentClient should not be equal.", false, productWithAttributes1.Equals(productWithAttributesDifferentClient));
			AssertEquals("productWithAttributes1 and productWithAttributesDifferentProduct should not be equal.", false, productWithAttributes1.Equals(productWithAttributesDifferentProduct));
			AssertEquals("productWithAttributes1 and productWithAttributesDifferentPA1 should not be equal.", false, productWithAttributes1.Equals(productWithAttributesDifferentPA1));
			AssertEquals("productWithAttributes1 and productWithAttributesDifferentPA2 should not be equal.", false, productWithAttributes1.Equals(productWithAttributesDifferentPA2));
			AssertEquals("productWithAttributes1 and productWithAttributesDifferentPA3 should not be equal.", false, productWithAttributes1.Equals(productWithAttributesDifferentPA3));
			AssertEquals("productWithAttributes1 and productWithAttributesDifferentSN should not be equal.", false, productWithAttributes1.Equals(productWithAttributesDifferentSN));
			AssertEquals("productWithAttributes1 and productWithAttributesDifferentExpiryDate should not be equal.", false, productWithAttributes1.Equals(productWithAttributesDifferentExpiryDate));
			AssertEquals("productWithAttributes1 and productWithAttributesDifferentPackingDate should not be equal.", false, productWithAttributes1.Equals(productWithAttributesDifferentPackingDate));
			AssertEquals("productWithAttributes1 and productWithAttributesDifferentBondedEntryKey should not be equal.", false, productWithAttributes1.Equals(productWithAttributesDifferentBondedEntryKey));
			AssertEquals("productWithAttributes1 and productWithAttributesDifferentAllocationKeyKey should not be equal.", false, productWithAttributes1.Equals(productWithAttributesDifferentAllocationKeyKey));
			AssertEquals("productWithAttributes1 and productWithAttributesDifferentCasing should not be equal.", false, productWithAttributes1.Equals(productWithAttributesDifferentCasing));
			AssertEquals("productWithAttributes1 and productWithAttributesDifferentQuantity should not be equal.", false, productWithAttributes1.Equals(productWithAttributesDifferentQuantity));
		}

		#endregion

		#region TestGetHashCode

		public void TestGetHashCode()
		{
			var clientPK1 = ZGuid.NewZGuid();
			var clientPK2 = ZGuid.NewZGuid();
			var productPK1 = ZGuid.NewZGuid();
			var productPK2 = ZGuid.NewZGuid();
			var productWithAttributes1 = new ProductWithAttributes(clientPK1, productPK1, "PA1", "PA2", "PA3", "SN", ZDate.Today, ZDate.Today.AddDays(-5), "KEY-1", "ALO-1", 1m);
			var productWithAttributesDifferentClient = new ProductWithAttributes(clientPK2, productPK1, "PA1", "PA2", "PA3", "SN", ZDate.Today, ZDate.Today.AddDays(-5), "KEY-1", "ALO-1", 1m);
			var productWithAttributesDifferentProduct = new ProductWithAttributes(clientPK1, productPK2, "PA1", "PA2", "PA3", "SN", ZDate.Today, ZDate.Today.AddDays(-5), "KEY-1", "ALO-1", 1m);
			var productWithAttributesDifferentPA1 = new ProductWithAttributes(clientPK1, productPK1, "PA11", "PA2", "PA3", "SN", ZDate.Today, ZDate.Today.AddDays(-5), "KEY-1", "ALO-1", 1m);
			var productWithAttributesDifferentPA2 = new ProductWithAttributes(clientPK1, productPK1, "PA1", "PA22", "PA3", "SN", ZDate.Today, ZDate.Today.AddDays(-5), "KEY-1", "ALO-1", 1m);
			var productWithAttributesDifferentPA3 = new ProductWithAttributes(clientPK1, productPK1, "PA1", "PA2", "PA33", "SN", ZDate.Today, ZDate.Today.AddDays(-5), "KEY-1", "ALO-1", 1m);
			var productWithAttributesDifferentSN = new ProductWithAttributes(clientPK1, productPK1, "PA1", "PA2", "PA3", "SNN", ZDate.Today, ZDate.Today.AddDays(-5), "KEY-1", "ALO-1", 1m);
			var productWithAttributesDifferentExpiryDate = new ProductWithAttributes(clientPK1, productPK1, "PA1", "PA2", "PA3", "SN", ZDate.Today.AddDays(1), ZDate.Today.AddDays(-5), "KEY-1", "ALO-1", 1m);
			var productWithAttributesDifferentPackingDate = new ProductWithAttributes(clientPK1, productPK1, "PA1", "PA2", "PA3", "SN", ZDate.Today, ZDate.Today.AddDays(-4), "KEY-1", "ALO-1", 1m);
			var productWithAttributesDifferentBondedEntryKey = new ProductWithAttributes(clientPK1, productPK1, "PA1", "PA2", "PA3", "SN", ZDate.Today, ZDate.Today.AddDays(-5), "KEY-2", "ALO-1", 1m);
			var productWithAttributesDifferentAllocationKeyKey = new ProductWithAttributes(clientPK1, productPK1, "PA1", "PA2", "PA3", "SN", ZDate.Today, ZDate.Today.AddDays(-5), "KEY-1", "ALO-2", 1m);
			var productWithAttributesDifferentQuantity = new ProductWithAttributes(clientPK1, productPK1, "PA1", "PA2", "PA3", "SN", ZDate.Today, ZDate.Today.AddDays(-5), "KEY-1", "ALO-1", 2m);
			var productWithAttributes2 = new ProductWithAttributes(clientPK1, productPK1, "PA1", "PA2", "PA3", "SN", ZDate.Today, ZDate.Today.AddDays(-5), "KEY-1", "ALO-1", 1m);

			Assert("PreCondition: productWithAttributes1.Equals(productWithAttributes2)", productWithAttributes1.Equals(productWithAttributes2));
			AssertEquals("productWithAttributes1 and productWithAttributes2 should have the same HashCode.", productWithAttributes1.GetHashCode(), productWithAttributes2.GetHashCode());
			AssertNotEquals("productWithAttributes1 and productWithAttributesDifferentClient should not have the same HashCode.", productWithAttributes1.GetHashCode(), productWithAttributesDifferentClient.GetHashCode());
			AssertNotEquals("productWithAttributes1 and productWithAttributesDifferentProduct should not have the same HashCode.", productWithAttributes1.GetHashCode(), productWithAttributesDifferentProduct.GetHashCode());
			AssertNotEquals("productWithAttributes1 and productWithAttributesDifferentPA1 should not have the same HashCode.", productWithAttributes1.GetHashCode(), productWithAttributesDifferentPA1.GetHashCode());
			AssertNotEquals("productWithAttributes1 and productWithAttributesDifferentPA2 should not have the same HashCode.", productWithAttributes1.GetHashCode(), productWithAttributesDifferentPA2.GetHashCode());
			AssertNotEquals("productWithAttributes1 and productWithAttributesDifferentPA3 should not have the same HashCode.", productWithAttributes1.GetHashCode(), productWithAttributesDifferentPA3.GetHashCode());
			AssertNotEquals("productWithAttributes1 and productWithAttributesDifferentSN should not have the same HashCode.", productWithAttributes1.GetHashCode(), productWithAttributesDifferentSN.GetHashCode());
			AssertNotEquals("productWithAttributes1 and productWithAttributesDifferentExpiryDate should not have the same HashCode.", productWithAttributes1.GetHashCode(), productWithAttributesDifferentExpiryDate.GetHashCode());
			AssertNotEquals("productWithAttributes1 and productWithAttributesDifferentPackingDate should not have the same HashCode.", productWithAttributes1.GetHashCode(), productWithAttributesDifferentPackingDate.GetHashCode());
			AssertNotEquals("productWithAttributes1 and productWithAttributesDifferentBondedEntryKey should not have the same HashCode.", productWithAttributes1.GetHashCode(), productWithAttributesDifferentBondedEntryKey.GetHashCode());
			AssertNotEquals("productWithAttributes1 and productWithAttributesDifferentAllocationKeyKey should not have the same HashCode.", productWithAttributes1.GetHashCode(), productWithAttributesDifferentAllocationKeyKey.GetHashCode());
			AssertNotEquals("productWithAttributes1 and productWithAttributesDifferentQuantity should not have the same HashCode.", productWithAttributes1.GetHashCode(), productWithAttributesDifferentQuantity.GetHashCode());
		}

		#endregion
	}
}
