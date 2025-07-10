using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsVASOrderLineDataObjectReaderTest : WhsUniversalTestCase
	{
		#region TestPopulateBusinessObject_DuplicateProducts

		public void TestPopulateBusinessObject_DuplicateProducts()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			data.Part1.OP_PartNum = "DUPE";
			data.Part2.OP_PartNum = "DUPE";
			using (DuplicateProductTriggerSuspenderForTest.Suspend())
			{
				Factory.SaveForTesting();
			}

			// Setup DataObjects
			var vasOrderLine1DO = new OrderLine();
			vasOrderLine1DO.LineNumber = 2;
			vasOrderLine1DO.Product = new Product { Code = "DUPE" };
			vasOrderLine1DO.OrderedQty = 1m;

			// Import DataObject
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Cannot Import VAS Order Line 2.\r\nMultiple Product matched: DUPE for Client 111.",
				() => new WhsVASOrderLineDataObjectReader(vasOrderLine1DO, data.Org1, Logger, Factory).ReadIntoBusinessObject());
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
			vasOrderLine1DO.LineNumber = 2;
			vasOrderLine1DO.Product = new Product { Code = "NONE" };
			vasOrderLine1DO.OrderedQty = 1m;

			// Import DataObject
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Unable to match Product: NONE.",
				() => new WhsVASOrderLineDataObjectReader(vasOrderLine1DO, data.Org1, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestPopulateBusinessObject_SimpleCase

		public void TestPopulateBusinessObject_SimpleCase()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			Factory.SaveForTesting();

			// Setup DataObjects
			var vasOrderLine1DO = new OrderLine();
			vasOrderLine1DO.LineNumber = 1;
			vasOrderLine1DO.Product = new Product { Code = "P1" };
			vasOrderLine1DO.OrderedQty = 5m;
			vasOrderLine1DO.PackingDate = today.AddDays(-1);
			vasOrderLine1DO.ExpiryDate = today.AddDays(7);
			vasOrderLine1DO.PartAttribute1 = "PA-1";
			vasOrderLine1DO.PartAttribute2 = "PA-2";
			vasOrderLine1DO.PartAttribute3 = "PA-3";
			vasOrderLine1DO.SerialNumber = "SERN";

			var vasOrderLine2DO = new OrderLine();
			vasOrderLine2DO.Product = new Product { Code = "P2" };
			vasOrderLine2DO.OrderedQty = 10m;

			// Import DataObject
			var vasOrderLine1BO = new WhsVASOrderLineDataObjectReader(vasOrderLine1DO, data.Org1, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Precondition", vasOrderLine1BO);
			AssertEquals("LineNumber", 1, vasOrderLine1BO.WVL_LineNumber);
			AssertEquals("Product", data.Part1, vasOrderLine1BO.Product);
			AssertEquals("Quantity", 5m, vasOrderLine1BO.WVL_Quantity);
			AssertEquals("PackingDate", today.AddDays(-1), vasOrderLine1BO.WVL_PackingDate);
			AssertEquals("ExpiryDate", today.AddDays(7), vasOrderLine1BO.WVL_ExpiryDate);
			AssertEquals("PartAttribute1", "PA-1", vasOrderLine1BO.WVL_PartAttrib1);
			AssertEquals("PartAttribute2", "PA-2", vasOrderLine1BO.WVL_PartAttrib2);
			AssertEquals("PartAttribute3", "PA-3", vasOrderLine1BO.WVL_PartAttrib3);
			AssertEquals("SerialNumber", "SERN", vasOrderLine1BO.WVL_SerialNumber);

			var vasOrderLine2BO = new WhsVASOrderLineDataObjectReader(vasOrderLine2DO, data.Org1, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Precondition", vasOrderLine2BO);
			AssertEquals("LineNumber", 0, vasOrderLine2BO.WVL_LineNumber);
			AssertEquals("Product", data.Part2, vasOrderLine2BO.Product);
			AssertEquals("Quantity", 10m, vasOrderLine2BO.WVL_Quantity);
			AssertEquals("PackingDate", ZDate.Empty, vasOrderLine2BO.WVL_PackingDate);
			AssertEquals("ExpiryDate", ZDate.Empty, vasOrderLine2BO.WVL_ExpiryDate);
			AssertEquals("PartAttribute1", ZString.Empty, vasOrderLine2BO.WVL_PartAttrib1);
			AssertEquals("PartAttribute2", ZString.Empty, vasOrderLine2BO.WVL_PartAttrib2);
			AssertEquals("PartAttribute3", ZString.Empty, vasOrderLine2BO.WVL_PartAttrib3);
			AssertEquals("SerialNumber", ZString.Empty, vasOrderLine2BO.WVL_SerialNumber);
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
		}

		void ExecutePopulateBusinessObject_PackageQty(OrgHeader org, ZDecimal? orderedQty, ZDecimal? packageQty, PackageType packageQtyUnit, ZDecimal expectedHoldQty)
		{
			Logger.ClearLogs();

			var vasOrderLine = new OrderLine()
			{
				Product = new Product { Code = "P1" },
				OrderedQty = orderedQty,
				PackageQty = packageQty,
				PackageQtyUnit = packageQtyUnit,
				OriginalHoldCode = new CodeDescriptionPair9Char { Code = "" },
				CurrentHoldCode = new CodeDescriptionPair9Char { Code = "HEL" }
			};

			var vasHoldOrderLine = new WhsVASOrderLineDataObjectReader(vasOrderLine, org, Logger, Factory).ReadIntoBusinessObject();
			AssertEquals("Expecting correct quantity", expectedHoldQty, vasHoldOrderLine.WVL_Quantity);

			AssertEquals("Expecting no warning", false, Logger.HasWarnings);
			AssertEquals("Expecting no warning with message", string.Empty, Logger.GetWarnings());
		}

		void ExecutePopulateBusinessObject_PackageQty(OrgHeader org, ZDecimal? orderedQty, ZDecimal? packageQty, PackageType packageQtyUnit, string expectedMessage, bool isWarning)
		{
			Logger.ClearLogs();

			var vasOrderLine = new OrderLine()
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
				var vasHoldOrderLine = new WhsVASOrderLineDataObjectReader(vasOrderLine, org, Logger, Factory).ReadIntoBusinessObject();
				AssertEquals("Expecting 0 quantity", 0m, vasHoldOrderLine.WVL_Quantity);

				AssertEquals("Expecting a warning", true, Logger.HasWarnings);
				AssertEquals("Expecting a warning with message", expectedMessage, Logger.GetWarnings());
			}
			else
			{
				AssertExceptionThrown(
					"Expecting a read failure exception is thrown",
					typeof(DataObjectReadFailureException),
					expectedMessage,
					() => new WhsVASOrderLineDataObjectReader(vasOrderLine, org, Logger, Factory).ReadIntoBusinessObject());
			}
		}

		#endregion

		protected override TestDataForUniversal GetNewTestData() => new TestDataForUniversal(Factory, Logger, DataContextType.WarehouseOrder);
	}
}
