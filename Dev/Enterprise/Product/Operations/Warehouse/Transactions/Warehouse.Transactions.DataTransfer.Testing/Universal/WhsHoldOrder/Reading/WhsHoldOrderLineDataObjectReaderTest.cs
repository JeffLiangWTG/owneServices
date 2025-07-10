using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsHoldOrderLineDataObjectReaderTest : WhsUniversalTestCase
	{
		#region TestPopulateBusinessObject

		public void TestPopulateBusinessObject()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			Factory.SaveForTesting();

			// Setup DataObjects
			var holdOrderLine1DO = new OrderLine();
			holdOrderLine1DO.LineNumber = 1;
			holdOrderLine1DO.Product = new Product { Code = "P1" };
			holdOrderLine1DO.OrderedQty = 5m;
			holdOrderLine1DO.ExpiryDate = today.AddDays(7);
			holdOrderLine1DO.PackingDate = today.AddDays(-1);
			holdOrderLine1DO.PartAttribute1 = "1";
			holdOrderLine1DO.PartAttribute2 = "2";
			holdOrderLine1DO.PartAttribute3 = "3";
			holdOrderLine1DO.SerialNumber = "SN";
			holdOrderLine1DO.OriginalHoldCode = new CodeDescriptionPair9Char { Code = "", Description = "None" };
			holdOrderLine1DO.CurrentHoldCode = new CodeDescriptionPair9Char { Code = InventoryHoldCodes.Codes.Damaged, Description = InventoryHoldCodes.Descriptions.Damaged };
			holdOrderLine1DO.CurrentHoldReason = "This is Ryan after all";

			var holdOrderLine2DO = new OrderLine();
			holdOrderLine2DO.Product = new Product { Code = "P2" };
			holdOrderLine2DO.OrderedQty = 10m;
			holdOrderLine2DO.OriginalHoldCode = new CodeDescriptionPair9Char { Code = InventoryHoldCodes.Codes.Held, Description = InventoryHoldCodes.Descriptions.Held };
			holdOrderLine2DO.CurrentHoldCode = new CodeDescriptionPair9Char { Code = "", Description = "None" };

			// Import DataObject
			var holdOrderLine1BO = new WhsHoldOrderLineDataObjectReader(holdOrderLine1DO, data.Org1, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Precondition", holdOrderLine1BO);
			AssertEquals(nameof(holdOrderLine1BO.Product), data.Part1, holdOrderLine1BO.Product);
			AssertEquals(nameof(holdOrderLine1BO.Quantity), 5m, holdOrderLine1BO.Quantity);
			AssertEquals(nameof(holdOrderLine1BO.PackingDate), today.AddDays(-1), holdOrderLine1BO.PackingDate);
			AssertEquals(nameof(holdOrderLine1BO.ExpiryDate), today.AddDays(7), holdOrderLine1BO.ExpiryDate);
			AssertEquals(nameof(holdOrderLine1BO.PartAttrib1), "1", holdOrderLine1BO.PartAttrib1);
			AssertEquals(nameof(holdOrderLine1BO.PartAttrib2), "2", holdOrderLine1BO.PartAttrib2);
			AssertEquals(nameof(holdOrderLine1BO.PartAttrib3), "3", holdOrderLine1BO.PartAttrib3);
			AssertEquals(nameof(holdOrderLine1BO.SerialNumber), "SN", holdOrderLine1BO.SerialNumber);
			AssertEquals(nameof(holdOrderLine1BO.FromHoldCode), ZString.Empty, holdOrderLine1BO.FromHoldCode);
			AssertEquals(nameof(holdOrderLine1BO.ToHoldCode), InventoryHoldCodes.Codes.Damaged, holdOrderLine1BO.ToHoldCode);
			AssertEquals(nameof(holdOrderLine1BO.HoldReason), "This is Ryan after all", holdOrderLine1BO.HoldReason);

			var holdOrderLine2BO = new WhsHoldOrderLineDataObjectReader(holdOrderLine2DO, data.Org1, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Precondition", holdOrderLine2BO);
			AssertEquals(nameof(holdOrderLine2BO.Product), data.Part2, holdOrderLine2BO.Product);
			AssertEquals(nameof(holdOrderLine2BO.Quantity), 10m, holdOrderLine2BO.Quantity);
			AssertEquals(nameof(holdOrderLine2BO.PackingDate), ZDate.Empty, holdOrderLine2BO.PackingDate);
			AssertEquals(nameof(holdOrderLine2BO.ExpiryDate), ZDate.Empty, holdOrderLine2BO.ExpiryDate);
			AssertEquals(nameof(holdOrderLine2BO.PartAttrib1), ZString.Empty, holdOrderLine2BO.PartAttrib1);
			AssertEquals(nameof(holdOrderLine2BO.PartAttrib2), ZString.Empty, holdOrderLine2BO.PartAttrib2);
			AssertEquals(nameof(holdOrderLine2BO.PartAttrib3), ZString.Empty, holdOrderLine2BO.PartAttrib3);
			AssertEquals(nameof(holdOrderLine2BO.SerialNumber), ZString.Empty, holdOrderLine2BO.SerialNumber);
			AssertEquals(nameof(holdOrderLine2BO.FromHoldCode), InventoryHoldCodes.Codes.Held, holdOrderLine2BO.FromHoldCode);
			AssertEquals(nameof(holdOrderLine2BO.ToHoldCode), ZString.Empty, holdOrderLine2BO.ToHoldCode);
			AssertEquals(nameof(holdOrderLine2BO.HoldReason), ZString.Empty, holdOrderLine2BO.HoldReason);
		}

		#endregion

		#region TestPopulateBusinessObject_DuplicateProducts

		public void TestPopulateBusinessObject_DuplicateProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			data.Part1.OP_PartNum = "DUPE";
			data.Part2.OP_PartNum = "DUPE";
			using (DuplicateProductTriggerSuspenderForTest.Suspend())
			{
				Factory.SaveForTesting();
			}

			// Setup DataObjects
			var holdOrderLine1DO = new OrderLine();
			holdOrderLine1DO.Product = new Product { Code = "DUPE" };
			holdOrderLine1DO.OrderedQty = 1m;

			// Import DataObject
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Cannot Import Hold Order Line 0.\r\nMultiple Product matched: DUPE for Client 111.",
				() => new WhsHoldOrderLineDataObjectReader(holdOrderLine1DO, data.Org1, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestPopulateBusinessObject_NoProductMatched

		public void TestPopulateBusinessObject_NoProductMatched()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			Factory.SaveForTesting();

			// Setup DataObjects
			var vasOrderLine1DO = new OrderLine();
			vasOrderLine1DO.Product = new Product { Code = "NONE" };
			vasOrderLine1DO.OrderedQty = 1m;

			// Import DataObject
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Unable to match Product: NONE.",
				() => new WhsHoldOrderLineDataObjectReader(vasOrderLine1DO, data.Org1, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestPopulateBusinessObject_InvalidFromHoldCode

		public void TestPopulateBusinessObject_InvalidFromHoldCode()
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			Factory.SaveForTesting();

			var holdOrderLine1DO = new OrderLine();
			holdOrderLine1DO.Product = new Product { Code = "P2" };
			holdOrderLine1DO.OrderedQty = 10m;
			holdOrderLine1DO.OriginalHoldCode = new CodeDescriptionPair9Char { Code = "BLA", Description = "BLA" };
			holdOrderLine1DO.CurrentHoldCode = new CodeDescriptionPair9Char { Code = "", Description = "None" };

			// Import DataObject - Should succesfully import. It is valid to have old inventory with a (now) non existent Hold Code. If neccessary, will fail to match on finalise.
			var holdOrderLine1BO = new WhsHoldOrderLineDataObjectReader(holdOrderLine1DO, data.Org1, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Precondition", holdOrderLine1BO);
			AssertEquals("FromHoldCode", "BLA", holdOrderLine1BO.FromHoldCode);
			AssertEquals("ToHoldCode", "", holdOrderLine1BO.ToHoldCode);
		}

		#endregion

		#region TestPopulateBusinessObject_InvalidToHoldCode

		public void TestPopulateBusinessObject_InvalidToHoldCode()
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			Factory.SaveForTesting();

			var holdOrderLine1DO = new OrderLine();
			holdOrderLine1DO.Product = new Product { Code = "P2" };
			holdOrderLine1DO.OrderedQty = 10m;
			holdOrderLine1DO.OriginalHoldCode = new CodeDescriptionPair9Char { Code = "", Description = "None" };
			holdOrderLine1DO.CurrentHoldCode = new CodeDescriptionPair9Char { Code = "BLA", Description = "Blahhhh" };

			AssertExceptionThrown("Should fail import, it is invalid to change a hold code to one that doesnt exist", typeof(DataObjectReadFailureException),
				"Invalid Hold Code: BLA - Blahhhh", () => new WhsHoldOrderLineDataObjectReader(holdOrderLine1DO, data.Org1, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestPopulateBusinessObject_HoldReasonWithoutHoldCode

		public void TestPopulateBusinessObject_HoldReasonWithoutHoldCode()
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			Factory.SaveForTesting();

			var holdOrderLineDO = new OrderLine();
			holdOrderLineDO.Product = new Product { Code = "P2" };
			holdOrderLineDO.OrderedQty = 10m;
			holdOrderLineDO.OriginalHoldCode = new CodeDescriptionPair9Char { Code = "HEL", Description = "Held" };
			holdOrderLineDO.CurrentHoldCode = new CodeDescriptionPair9Char { Code = "", Description = "None" };
			holdOrderLineDO.CurrentHoldReason = "I got no good reason";

			AssertExceptionThrown("Should fail import, it is invalid to set a Hold Reason without a Hold Code.", typeof(DataObjectReadFailureException),
				"Cannot import Hold Reason without a Hold Code.", () => new WhsHoldOrderLineDataObjectReader(holdOrderLineDO, data.Org1, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestPopulateBusinessObject_PackageQty

		public void TestPopulateBusinessObject_PackageQty()
		{
			// Arrange

			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);

			// Using default test conversion, 1 UNT = 2.0 KG.
			var packageQtyUnit = new PackageType { Code = "KG" };

			Factory.SaveForTesting();

			// Scenario 1: When OrderQty is not valid; Should use PackageQty.
			ExecutePopulateBusinessObject_PackageQty(data.Org1, null, 20m, packageQtyUnit, 10m);
			ExecutePopulateBusinessObject_PackageQty(data.Org1, 0m, 20m, packageQtyUnit, 10m);

			// Scenario 2: When PackageQty is not valid; Should use OrderQty.
			ExecutePopulateBusinessObject_PackageQty(data.Org1, 3m, null, packageQtyUnit, 3m);
			ExecutePopulateBusinessObject_PackageQty(data.Org1, 3m, 0m, packageQtyUnit, 3m);

			// Scenario 3: When both OrderQty and PackageQty are valid; Should use OrderQty.
			ExecutePopulateBusinessObject_PackageQty(data.Org1, 3m, 20m, packageQtyUnit, 3m);

			// Scenario 4: When both OrderQty and PackageQty are not valid; Should log error.
			ExecutePopulateBusinessObject_PackageQty(data.Org1, null, null, packageQtyUnit, "Order Quantity and Package Quantity are not valid (both have value 0). Line: 42.", true);
			ExecutePopulateBusinessObject_PackageQty(data.Org1, 0m, 0m, packageQtyUnit, "Order Quantity and Package Quantity are not valid (both have value 0). Line: 42.", true);

			// Scenario 5: When product doesn't have conversion unit for PackageType; Should use raw PackageQty value.
			ExecutePopulateBusinessObject_PackageQty(data.Org1, 0m, 42m, new PackageType { Code = "XYZ" }, "Unit conversion for package type 'XYZ' does not exist. Line: 42.", false);

			// Scenario 6: When OrderQty or PackageQty has negative value; Should log error.
			ExecutePopulateBusinessObject_PackageQty(data.Org1, -1m, null, packageQtyUnit, "Order Quantity or Package Quantity is not valid. Line: 42.", false);
			ExecutePopulateBusinessObject_PackageQty(data.Org1, null, -1m, packageQtyUnit, "Order Quantity or Package Quantity is not valid. Line: 42.", false);
		}

		void ExecutePopulateBusinessObject_PackageQty(OrgHeader org, ZDecimal? orderedQty, ZDecimal? packageQty, PackageType packageQtyUnit, ZDecimal expectedHoldQty)
		{
			Logger.ClearLogs();

			var orderLine = new OrderLine()
			{
				Product = new Product { Code = "P1" },
				OrderedQty = orderedQty,
				PackageQty = packageQty,
				PackageQtyUnit = packageQtyUnit,
				OriginalHoldCode = new CodeDescriptionPair9Char { Code = "" },
				CurrentHoldCode = new CodeDescriptionPair9Char { Code = "HEL" }
			};

			var holdOrderLine = new WhsHoldOrderLineDataObjectReader(orderLine, org, Logger, Factory).ReadIntoBusinessObject();
			AssertEquals("Expecting correct quantity", expectedHoldQty, holdOrderLine.Quantity);

			AssertEquals("Expecting no warning", false, Logger.HasWarnings);
			AssertEquals("Expecting no warning with message", string.Empty, Logger.GetWarnings());
		}

		void ExecutePopulateBusinessObject_PackageQty(OrgHeader org, ZDecimal? orderedQty, ZDecimal? packageQty, PackageType packageQtyUnit, string expectedMessage, bool isWarning)
		{
			Logger.ClearLogs();

			var orderLine = new OrderLine()
			{
				Product = new Product { Code = "P1" },
				OrderedQty = orderedQty,
				PackageQty = packageQty,
				PackageQtyUnit = packageQtyUnit,
				OriginalHoldCode = new CodeDescriptionPair9Char { Code = "" },
				CurrentHoldCode = new CodeDescriptionPair9Char { Code = "HEL" },
				LineNumber = 42
			};

			if (isWarning)
			{
				var holdOrderLine = new WhsHoldOrderLineDataObjectReader(orderLine, org, Logger, Factory).ReadIntoBusinessObject();
				AssertEquals("Expecting 0 quantity", 0m, holdOrderLine.Quantity);

				AssertEquals("Expecting a warning", true, Logger.HasWarnings);
				AssertEquals("Expecting a warning with message", expectedMessage, Logger.GetWarnings());
			}
			else
			{
				AssertExceptionThrown(
					"Expecting a read failure exception is thrown",
					typeof(DataObjectReadFailureException),
					expectedMessage,
					() => new WhsHoldOrderLineDataObjectReader(orderLine, org, Logger, Factory).ReadIntoBusinessObject());
			}
		}

		#endregion

		protected override TestDataForUniversal GetNewTestData() => new TestDataForUniversal(Factory, Logger, DataContextType.WarehouseOrder);
	}
}
