using System.Collections.Specialized;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business.Printing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsOutwardsProcessorTest : WhsOutwardsProcessorTestCase
	{
		#region General Tests

		public void TestProcessDeletesPreviousData()
		{
			var data = new TestDataForMultiWarehouseAttributes(Factory);
			var input = new WhsWarehouseTransaction();
			var line1 = new WhsWarehouseTransactionLine();
			var line2 = new WhsWarehouseTransactionLine();
			var line3 = new WhsWarehouseTransactionLine();
			var declarationPK = ZGuid.NewZGuid();
			input.Lines = WhsWarehouseTransactionLineCollection.GetNew(line1, line2, line3);
			input.Client = data.Org;
			input.ExternalPK = declarationPK;
			line1.EntryKey = "E111";
			line1.EntryLineNumber = 1;
			line2.PartAttrib1 = "PA212";
			line3.PartAttrib2 = "PA325";
			Factory.Save();
			var processor = new WhsOutwardsProcessor();
			var result = ProcessWithAllocateMock(input, processor);
			Assert("Incorrect Object Returned", result != null && result != input);
			AssertEquals(false, input.HasErrors);
			AssertEquals(false, result.HasErrors);
			AssertEquals(3, result.Lines.Count);
			// do this 5 times just for the hell of it
			for (int i = 0; i < 5; i++)
			{
				// load created orders
				var filter = new ZQuery(WhsDocketSchema.WD_ExWhsJobGuid, SQLComparisonOperator.Equal, declarationPK);
				var orders = new WhsOrderCollection(Factory, filter);
				AssertEquals(3, orders.Count);
				var order0 = orders[0];
				var order1 = orders[1];
				var order2 = orders[2];
				var pick0 = order0.Pick;
				var pick1 = order1.Pick;
				var pick2 = order2.Pick;
				AssertEquals(false, order0.IsDeleted);
				AssertEquals(false, order1.IsDeleted);
				AssertEquals(false, order2.IsDeleted);
				// now reprocess and assert old orders are deleted
				processor = new WhsOutwardsProcessor();
				result = ProcessWithAllocateMock(input, processor);
				AssertEquals(false, input.HasErrors);
				AssertEquals(false, result.HasErrors);
				AssertEquals(3, result.Lines.Count);
				AssertEquals(true, order0.IsDeleted);
				AssertEquals(true, order1.IsDeleted);
				AssertEquals(true, order2.IsDeleted);
				AssertEquals(true, pick0.IsDeleted);
				AssertEquals(true, pick1.IsDeleted);
				AssertEquals(true, pick2.IsDeleted);
			}
		}

		public void TestProcessContinueIfError()
		{
			Assert("incomplete test", true);
		}

		public void TestProcessHandlesNoInputClient()
		{
			Assert("incomplete test", true);
		}

		#endregion

		#region Error Tests

		#region General Errors

		public void TestProcessErrorMissingMandatoryDataNoLines()
		{
			WhsWarehouseTransaction input = new WhsWarehouseTransaction();
			WhsOutwardsProcessor processor = new WhsOutwardsProcessor();
			IWhsWarehouseTransaction result = processor.Process(Factory, input, false);
			AssertEquals("Incorrect Object Returned", input, result);
			AssertEquals(1, input.Problems.ErrorList.Count);
			AssertEquals(new NoLinesError().Message, input.Problems.ErrorList[0]);
		}

		public void TestProcessErrorMissingMandatoryDataLinesWithNoData()
		{
			WhsWarehouseTransaction input = new WhsWarehouseTransaction();
			WhsWarehouseTransactionLine line1 = new WhsWarehouseTransactionLine();
			input.Lines = WhsWarehouseTransactionLineCollection.GetNew(line1);
			WhsOutwardsProcessor processor = new WhsOutwardsProcessor();
			IWhsWarehouseTransaction result = processor.Process(Factory, input, false);
			AssertEquals("Incorrect Object Returned", input, result);
			AssertEquals(1, line1.QuantityProblems.ErrorList.Count);
			AssertEquals(new MissingLineDataError().Message, line1.QuantityProblems.ErrorList[0]);
		}

		public void TestProcessErrorMissingMandatoryDataLinesWithNoQuantity()
		{
			WhsWarehouseTransaction input = new WhsWarehouseTransaction();
			WhsWarehouseTransactionLine line1 = new WhsWarehouseTransactionLine();
			input.Lines = WhsWarehouseTransactionLineCollection.GetNew(line1);
			OrgHeader client = Helper.CreateClient();
			OrgSupplierPart part = Helper.CreateProduct(client, "P1");
			input.Client = client;
			line1.Product = part;
			WhsOutwardsProcessor processor = new WhsOutwardsProcessor();
			IWhsWarehouseTransaction result = processor.Process(Factory, input, false);
			AssertEquals("Incorrect Object Returned", input, result);
			AssertEquals(1, line1.QuantityProblems.ErrorList.Count);
			AssertEquals(new MissingQuantityError().Message, line1.QuantityProblems.ErrorList[0]);
		}

		public void TestProcessErrorPickExceptionIsCaught()
		{
			Assert("incomplete test", true);
		}

		#endregion

		#region Single Warehouse Errors

		public void TestProcessErrorUsingProductNoStockAtAllSingleWarehouse()
		{
			var data = new TestDataForSingleWarehouse(Factory);
			var input = new WhsWarehouseTransaction();
			var line1 = new WhsWarehouseTransactionLine();
			var line2 = new WhsWarehouseTransactionLine();
			input.Lines = WhsWarehouseTransactionLineCollection.GetNew(line1, line2);
			input.Client = data.Org;
			line1.Product = data.PartNS;
			line1.Quantity = 10m;
			line2.Product = data.PartNS;
			line2.Quantity = 10m;
			Factory.Save();
			var processor = new WhsOutwardsProcessor();
			var result = processor.Process(Factory, input, false);
			AssertEquals("Incorrect Object Returned", input, result);
			AssertEquals(true, input.HasErrors);
			AssertEquals(1, line1.QuantityProblems.ErrorList.Count);
			AssertEquals(new ShortfallError().Message, line1.QuantityProblems.ErrorList[0]);
			AssertEquals(1, line2.QuantityProblems.ErrorList.Count);
			AssertEquals(new ShortfallError().Message, line2.QuantityProblems.ErrorList[0]);
		}

		public void TestProcessErrorUsingProductPartialShortfallSingleWarehouse()
		{
			var data = new TestDataForSingleWarehouse(Factory);
			var input = new WhsWarehouseTransaction();
			var line1 = new WhsWarehouseTransactionLine();
			var line2 = new WhsWarehouseTransactionLine();
			var line3 = new WhsWarehouseTransactionLine();
			input.Lines = WhsWarehouseTransactionLineCollection.GetNew(line1, line2, line3);
			input.Client = data.Org;
			line1.Product = data.IReceiveLine111.Product;
			line1.Quantity = data.IReceiveLine111.Quantity; // ok
			line2.Product = data.IReceiveLine121.Product;
			var line2QtyInStock = data.IReceiveLine121.Quantity + data.IReceiveLine122.Quantity +
								  data.IReceiveLine123.Quantity;
			line2.Quantity = line2QtyInStock * 2m; // shortfall
			line3.Product = data.IReceiveLine111.Product;
			var line3QtyInStock = data.IReceiveLine111.Quantity + data.IReceiveLine112.Quantity +
								  data.IReceiveLine113.Quantity;
			line3.Quantity = line3QtyInStock * 2m; // shortfall
			line3.Warehouse = data.Address1;
			var processor = new WhsOutwardsProcessor();
			var result = processor.Process(Factory, input, false);
			AssertEquals("Incorrect Object Returned", input, result);
			AssertEquals(true, input.HasErrors);
			AssertEquals(false, line1.HasErrors);
			AssertEquals(1, line2.QuantityProblems.ErrorList.Count);
			var warehouseNameList = new StringCollection();
			warehouseNameList.Add(data.Whs1.WW_WarehouseName);
			AssertEquals(new ShortfallError(line2QtyInStock, true, warehouseNameList).Message,
				line2.QuantityProblems.ErrorList[0]);
			AssertEquals(1, line3.QuantityProblems.ErrorList.Count);
			AssertEquals(1, line3.WarehouseProblems.ErrorList.Count);
			AssertEquals(new ShortfallError(line3QtyInStock).Message, line3.QuantityProblems.ErrorList[0]);
			AssertEquals(new ShortfallError(line3QtyInStock).Message, line3.WarehouseProblems.ErrorList[0]);
		}

		public void TestProcessErrorUsingEntryKeyPartialShortfallSingleWarehouse()
		{
			TestDataForSingleWarehouse data = new TestDataForSingleWarehouse(Factory);
			WhsWarehouseTransaction input = new WhsWarehouseTransaction();
			WhsWarehouseTransactionLine line1 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line2 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line3 = new WhsWarehouseTransactionLine();
			input.Lines = WhsWarehouseTransactionLineCollection.GetNew(line1, line2, line3);
			input.Client = data.Org;
			line1.EntryKey = data.IReceiveLine111.EntryKey; // ok
			line1.EntryLineNumber = data.IReceiveLine111.EntryLineNumber;
			line2.EntryKey = "DOESNOTEXIST"; // wont be found
			line2.EntryLineNumber = 10;
			line3.EntryKey = data.IReceiveLine121.EntryKey;
			line3.EntryLineNumber = data.IReceiveLine121.EntryLineNumber;
			line3.Quantity = data.IReceiveLine121.Quantity * 2m; // shortfall
			WhsOutwardsProcessor processor = new WhsOutwardsProcessor();
			IWhsWarehouseTransaction result = processor.Process(Factory, input, false);
			AssertEquals("Incorrect Object Returned", input, result);
			AssertEquals(true, input.HasErrors);
			AssertEquals(false, line1.HasErrors);
			AssertEquals(1, line2.EntryKeyProblems.ErrorList.Count);
			AssertEquals(new ShortfallError().Message, line2.EntryKeyProblems.ErrorList[0]);
			StringCollection warehouseNameList = new StringCollection();
			warehouseNameList.Add(data.Whs1.WW_WarehouseName);
			AssertEquals(1, line3.EntryKeyProblems.ErrorList.Count);
			AssertEquals(new ShortfallError(data.IReceiveLine121.Quantity, true, warehouseNameList).Message,
				line3.EntryKeyProblems.ErrorList[0]);
		}

		public void TestProcessErrorUsingPartAttribsPartialShortfallSingleWarehouse()
		{
			TestDataForSingleWarehouseAttributes data = new TestDataForSingleWarehouseAttributes(Factory);
			WhsWarehouseTransaction input = new WhsWarehouseTransaction();
			WhsWarehouseTransactionLine line1 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line2 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line3 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line4 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line5 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line6 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line7 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line8 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line9 = new WhsWarehouseTransactionLine();
			input.Lines =
				WhsWarehouseTransactionLineCollection.GetNew(line1, line2, line3, line4, line5, line6, line7, line8,
					line9);
			input.Client = data.Org;
			// part attrib 1
			line1.PartAttrib1 = data.IReceiveLine111.PartAttrib1; // ok
			line2.PartAttrib1 = "DOESNOTEXIST"; // wont be found
			line3.PartAttrib1 = data.IReceiveLine121.PartAttrib1;
			line3.Quantity = data.IReceiveLine121.Quantity * 2m; // shortfall
			// part attrib 2
			line4.PartAttrib2 = data.IReceiveLine121.PartAttrib2; // ok
			line5.PartAttrib2 = "DOESNOTEXIST"; // wont be found
			line6.PartAttrib2 = data.IReceiveLine112.PartAttrib2;
			line6.Quantity = data.IReceiveLine112.Quantity * 2m; // shortfall
			// part attrib 3
			line7.PartAttrib3 = data.IReceiveLine131.PartAttrib3; // ok
			line8.PartAttrib3 = "DOESNOTEXIST"; // wont be found
			line9.PartAttrib3 = data.IReceiveLine122.PartAttrib3;
			line9.Quantity = data.IReceiveLine122.Quantity * 2m; // shortfall
			WhsOutwardsProcessor processor = new WhsOutwardsProcessor();
			IWhsWarehouseTransaction result = processor.Process(Factory, input, false);
			AssertEquals("Incorrect Object Returned", input, result);
			AssertEquals(true, input.HasErrors);
			// part attrib 1
			AssertEquals(false, line1.HasErrors);
			AssertEquals(1, line2.PartAttrib1Problems.ErrorList.Count);
			AssertEquals(new ShortfallError().Message, line2.PartAttrib1Problems.ErrorList[0]);
			StringCollection warehouseNameList = new StringCollection();
			warehouseNameList.Add(data.Whs1.WW_WarehouseName);
			AssertEquals(1, line3.PartAttrib1Problems.ErrorList.Count);
			AssertEquals(new ShortfallError(data.IReceiveLine121.Quantity, true, warehouseNameList).Message,
				line3.PartAttrib1Problems.ErrorList[0]);
			// part attrib 2
			AssertEquals(false, line4.HasErrors);
			AssertEquals(1, line5.PartAttrib2Problems.ErrorList.Count);
			AssertEquals(new ShortfallError().Message, line5.PartAttrib2Problems.ErrorList[0]);
			warehouseNameList.Clear();
			warehouseNameList.Add(data.Whs1.WW_WarehouseName);
			AssertEquals(1, line6.PartAttrib2Problems.ErrorList.Count);
			AssertEquals(new ShortfallError(data.IReceiveLine112.Quantity, true, warehouseNameList).Message,
				line6.PartAttrib2Problems.ErrorList[0]);
			// part attrib 3
			AssertEquals(false, line7.HasErrors);
			AssertEquals(1, line8.PartAttrib3Problems.ErrorList.Count);
			AssertEquals(new ShortfallError().Message, line8.PartAttrib3Problems.ErrorList[0]);
			warehouseNameList.Clear();
			warehouseNameList.Add(data.Whs1.WW_WarehouseName);
			AssertEquals(1, line9.PartAttrib3Problems.ErrorList.Count);
			AssertEquals(new ShortfallError(data.IReceiveLine122.Quantity, true, warehouseNameList).Message,
				line9.PartAttrib3Problems.ErrorList[0]);
		}

		#endregion

		#region Multi Warehouse Errors

		public void TestProcessErrorUsingProductPartialShortfallMultiWarehouse()
		{
			var data = new TestDataForMultiWarehouse(Factory);
			var input = new WhsWarehouseTransaction();
			var line1 = new WhsWarehouseTransactionLine();
			var line2 = new WhsWarehouseTransactionLine();
			var line3 = new WhsWarehouseTransactionLine();
			input.Lines = WhsWarehouseTransactionLineCollection.GetNew(line1, line2, line3);
			input.Client = data.Org;
			line1.Product = data.IReceiveLine111.Product;
			line1.Quantity = data.IReceiveLine111.Quantity; // ok
			line2.Product = data.IReceiveLine111.Product;
			var line2QtyInStock = data.IReceiveLine111.Quantity + data.IReceiveLine112.Quantity +
								  data.IReceiveLine113.Quantity;
			line2.Quantity = line2QtyInStock * 4m; // shortfall accross all warehouses
			line3.Product = data.IReceiveLine221.Product;
			var line3QtyInStock = data.IReceiveLine221.Quantity + data.IReceiveLine222.Quantity +
								  data.IReceiveLine223.Quantity;
			line3.Quantity = line3QtyInStock * 2m; // shortfall for selected warehouse
			line3.Warehouse = data.Address2;
			Factory.Save();
			var processor = new WhsOutwardsProcessor();
			var result = processor.Process(Factory, input, false);
			AssertEquals("Incorrect Object Returned", input, result);
			AssertEquals(true, input.HasErrors);
			AssertEquals(false, line1.HasErrors);
			AssertEquals(1, line2.QuantityProblems.ErrorList.Count);
			var warehouseNameList = new StringCollection();
			warehouseNameList.Add(data.Whs1.WW_WarehouseName);
			warehouseNameList.Add(data.Whs2.WW_WarehouseName);
			warehouseNameList.Add(data.Whs3.WW_WarehouseName);
			AssertEquals(new ShortfallError(line2QtyInStock * 3m, true, warehouseNameList).Message,
				line2.QuantityProblems.ErrorList[0]);
			AssertEquals(1, line3.QuantityProblems.ErrorList.Count);
			AssertEquals(1, line3.WarehouseProblems.ErrorList.Count);
			warehouseNameList.Remove(data.Whs2.WW_WarehouseName);
			AssertEquals(new ShortfallError(line3QtyInStock, true, warehouseNameList).Message,
				line3.QuantityProblems.ErrorList[0]);
			AssertEquals(new ShortfallError(line3QtyInStock, true, warehouseNameList).Message,
				line3.WarehouseProblems.ErrorList[0]);
		}

		#endregion

		#endregion

		#region Process Output Tests

		#region Product Tests

		public void TestProcessUsingProductSingleWarehouse()
		{
			var data = new TestDataForSingleWarehouse(Factory);
			data.Whs1.WW_AutoPrintPackingSlip = true;
			data.Whs2.WW_AutoPrintPackingSlip = true;
			data.Whs3.WW_AutoPrintPackingSlip = true;
			var input = new WhsWarehouseTransaction();
			var line1 = new WhsWarehouseTransactionLine();
			var line2 = new WhsWarehouseTransactionLine();
			input.Lines = WhsWarehouseTransactionLineCollection.GetNew(line1, line2);
			input.Client = data.Org;
			// test line where qty is less than total in warehouse
			line1.Product = data.Part1;
			line1.Quantity = data.IReceiveLine111.Quantity;
			// test line where qty equals total in warehouse, and warehouse is specified
			line2.Product = data.Part2;
			line2.Quantity = data.IReceiveLine121.Quantity + data.IReceiveLine122.Quantity +
							 data.IReceiveLine123.Quantity;
			line2.Warehouse = data.Address1;
			Factory.Save();
			var processor = new WhsOutwardsProcessor();
			WhsDocumentPrinter.LastPrintedDocumentName = "";
			var result = ProcessWithAllocateMock(input, processor);
			AssertEquals("", WhsDocumentPrinter.LastPrintedDocumentName);
			Assert("Incorrect Object Returned", result != null && result != input);
			AssertEquals(false, input.HasErrors);
			AssertEquals(false, result.HasErrors);
			AssertEquals(4, result.Lines.Count);
			AssertTransactionLine(result, data.Address1, data.IReceiveLine111.Product, data.IReceiveLine111.Quantity);
			AssertTransactionLine(result, data.IReceiveLine121, data.Address1);
			AssertTransactionLine(result, data.IReceiveLine122, data.Address1);
			AssertTransactionLine(result, data.IReceiveLine123, data.Address1);
		}

		public void TestProcessUsingProductMultiWarehouse()
		{
			TestDataForMultiWarehouse data = new TestDataForMultiWarehouse(Factory);
			WhsWarehouseTransaction input = new WhsWarehouseTransaction();
			WhsWarehouseTransactionLine line1 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line2 = new WhsWarehouseTransactionLine();
			input.Lines = WhsWarehouseTransactionLineCollection.GetNew(line1, line2);
			input.Client = data.Org;
			// test line where qty is less than total in warehouse - should pick oldest stock
			// 90 from Address3, 60 from Address2
			line1.Product = data.Part1;
			line1.Quantity = 120m;
			// test line where qty equals total in warehouse, and warehouse is specified
			line2.Product = data.Part2;
			line2.Quantity = 60m;
			line2.Warehouse = data.Address2;
			Factory.Save();
			var processor = new WhsOutwardsProcessor();
			var result = ProcessWithAllocateMock(input, processor);
			Assert("Incorrect Object Returned", result != null && result != input);
			AssertEquals(false, input.HasErrors);
			AssertEquals(false, result.HasErrors);
			AssertEquals(8, result.Lines.Count); // 6 for Line1, 2 for Line2
			// Line1
			AssertTransactionLine(result, data.IReceiveLine211, data.Address2, 10m);
			AssertTransactionLine(result, data.IReceiveLine212, data.Address2, 20m);
			AssertTransactionLine(result, data.IReceiveLine213, data.Address2, 30m);
			AssertTransactionLine(result, data.IReceiveLine311, data.Address3, 10m);
			AssertTransactionLine(result, data.IReceiveLine312, data.Address3, 20m);
			AssertTransactionLine(result, data.IReceiveLine313, data.Address3, 30m);
			// Line2
			AssertTransactionLine(result, data.IReceiveLine221, data.Address2, 30m);
			AssertTransactionLine(result, data.IReceiveLine223, data.Address2, 30m);
		}

		#endregion

		#region Entry Key Tests

		public void TestProcessUsingEntryKeySingleWarehouse()
		{
			TestDataForSingleWarehouse data = new TestDataForSingleWarehouse(Factory);
			WhsWarehouseTransaction input = new WhsWarehouseTransaction();
			WhsWarehouseTransactionLine line1 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line2 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line3 = new WhsWarehouseTransactionLine();
			input.Lines = WhsWarehouseTransactionLineCollection.GetNew(line1, line2, line3);
			input.Client = data.Org;
			// test line where qty is not specified - should allocate all
			line1.EntryKey = data.IReceiveLine111.EntryKey;
			line1.EntryLineNumber = data.IReceiveLine111.EntryLineNumber;
			// test line where qty is less than total in warehouse
			line2.EntryKey = data.IReceiveLine122.EntryKey;
			line2.EntryLineNumber = data.IReceiveLine122.EntryLineNumber;
			line2.Quantity = data.IReceiveLine122.Quantity / 2m;
			// test line where qty equals total in warehouse, and warehouse is specified
			line3.EntryKey = data.IReceiveLine121.EntryKey;
			line3.EntryLineNumber = data.IReceiveLine121.EntryLineNumber;
			line3.Quantity = data.IReceiveLine121.Quantity;
			line3.Warehouse = data.Address1;
			Factory.Save();
			var processor = new WhsOutwardsProcessor();
			var result = ProcessWithAllocateMock(input, processor);
			Assert("Incorrect Object Returned", result != null && result != input);
			AssertEquals(false, input.HasErrors);
			AssertEquals(false, result.HasErrors);
			AssertEquals(3, result.Lines.Count);
			AssertTransactionLine(result, data.IReceiveLine111, data.Address1,
				data.IReceiveLine111.Quantity + data.IReceiveLine112.Quantity);
			AssertTransactionLine(result, data.IReceiveLine122, data.Address1, data.IReceiveLine122.Quantity / 2m);
			AssertTransactionLine(result, data.IReceiveLine121, data.Address1);
		}

		public void TestProcessUsingEntryKeyMultiWarehouse()
		{
			TestDataForMultiWarehouse data = new TestDataForMultiWarehouse(Factory);
			WhsWarehouseTransaction input = new WhsWarehouseTransaction();
			WhsWarehouseTransactionLine line1 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line2 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line3 = new WhsWarehouseTransactionLine();
			input.Lines = WhsWarehouseTransactionLineCollection.GetNew(line1, line2, line3);
			input.Client = data.Org;
			// test line where qty is not specified - should allocate all
			line1.EntryKey = data.IReceiveLine111.EntryKey;
			line1.EntryLineNumber = data.IReceiveLine111.EntryLineNumber;
			// test line where qty is less than total in warehouse
			line2.EntryKey = data.IReceiveLine321.EntryKey;
			line2.EntryLineNumber = data.IReceiveLine321.EntryLineNumber;
			line2.Quantity = data.IReceiveLine321.Quantity;
			// test line where qty equals total in warehouse, and warehouse is specified
			// should pick from Address2, would normally pick from Address3 (line323b) if not specified
			line3.EntryKey = data.IReceiveLine223.EntryKey;
			line3.EntryLineNumber = data.IReceiveLine223.EntryLineNumber;
			line3.Quantity = data.IReceiveLine223.Quantity;
			line3.Warehouse = data.Address2;
			Factory.Save();
			var processor = new WhsOutwardsProcessor();
			var result = ProcessWithAllocateMock(input, processor);
			Assert("Incorrect Object Returned", result != null && result != input);
			AssertEquals(false, input.HasErrors);
			AssertEquals(false, result.HasErrors);
			AssertEquals(3, result.Lines.Count);
			AssertTransactionLine(result, data.IReceiveLine111, data.Address1,
				data.IReceiveLine111.Quantity + data.IReceiveLine112.Quantity);
			AssertTransactionLine(result, data.IReceiveLine321, data.Address3, data.IReceiveLine321.Quantity);
			AssertTransactionLine(result, data.IReceiveLine223, data.Address2);
		}

		#endregion

		#region Single Attribute Tests

		public void TestProcessUsingPartAttrib1SingleWarehouse()
		{
			TestDataForSingleWarehouseAttributes data = new TestDataForSingleWarehouseAttributes(Factory);
			WhsWarehouseTransaction input = new WhsWarehouseTransaction();
			WhsWarehouseTransactionLine line1 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line2 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line3 = new WhsWarehouseTransactionLine();
			input.Lines = WhsWarehouseTransactionLineCollection.GetNew(line1, line2, line3);
			input.Client = data.Org;
			// test line where qty is not specified - should allocate all
			line1.PartAttrib1 = data.IReceiveLine112.PartAttrib1;
			// test line where qty is less than total in warehouse
			line2.PartAttrib1 = data.IReceiveLine122.PartAttrib1;
			line2.Quantity = data.IReceiveLine122.Quantity / 4m;
			// test line where qty equals total in warehouse, and warehouse is specified
			line3.PartAttrib1 = data.IReceiveLine131.PartAttrib1;
			line3.Quantity = data.IReceiveLine131.Quantity;
			line3.Warehouse = data.Address1;
			Factory.Save();
			var processor = new WhsOutwardsProcessor();
			var result = ProcessWithAllocateMock(input, processor);
			Assert("Incorrect Object Returned", result != null && result != input);
			AssertEquals(false, input.HasErrors);
			AssertEquals(false, result.HasErrors);
			AssertEquals(4, result.Lines.Count);
			AssertTransactionLine(result, data.IReceiveLine112, line1, data.Address1);
			AssertTransactionLine(result, data.IReceiveLine113, line1, data.Address1);
			AssertTransactionLine(result, data.IReceiveLine122, line2, data.Address1,
				data.IReceiveLine122.Quantity / 4m);
			AssertTransactionLine(result, data.IReceiveLine131, line3, data.Address1);
		}

		public void TestProcessUsingPartAttrib2SingleWarehouse()
		{
			TestDataForSingleWarehouseAttributes data = new TestDataForSingleWarehouseAttributes(Factory);
			WhsWarehouseTransaction input = new WhsWarehouseTransaction();
			WhsWarehouseTransactionLine line1 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line2 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line3 = new WhsWarehouseTransactionLine();
			input.Lines = WhsWarehouseTransactionLineCollection.GetNew(line1, line2, line3);
			input.Client = data.Org;
			// test line where qty is not specified - should allocate all
			line1.PartAttrib2 = data.IReceiveLine122.PartAttrib2;
			// test line where qty is less than total in warehouse
			line2.PartAttrib2 = data.IReceiveLine112.PartAttrib2;
			line2.Quantity = data.IReceiveLine112.Quantity / 4m;
			// test line where qty equals total in warehouse, and warehouse is specified
			line3.PartAttrib2 = data.IReceiveLine131.PartAttrib2;
			line3.Quantity = data.IReceiveLine131.Quantity;
			line3.Warehouse = data.Address1;
			Factory.Save();
			var processor = new WhsOutwardsProcessor();
			var result = ProcessWithAllocateMock(input, processor);
			Assert("Incorrect Object Returned", result != null && result != input);
			AssertEquals(false, input.HasErrors);
			AssertEquals(false, result.HasErrors);
			AssertEquals(4, result.Lines.Count);
			AssertTransactionLine(result, data.IReceiveLine122, line1, data.Address1);
			AssertTransactionLine(result, data.IReceiveLine123, line1, data.Address1);
			AssertTransactionLine(result, data.IReceiveLine112, line2, data.Address1,
				data.IReceiveLine122.Quantity / 4m);
			AssertTransactionLine(result, data.IReceiveLine131, line3, data.Address1);
		}

		public void TestProcessUsingPartAttrib3SingleWarehouse()
		{
			TestDataForSingleWarehouseAttributes data = new TestDataForSingleWarehouseAttributes(Factory);
			WhsWarehouseTransaction input = new WhsWarehouseTransaction();
			WhsWarehouseTransactionLine line1 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line2 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line3 = new WhsWarehouseTransactionLine();
			input.Lines = WhsWarehouseTransactionLineCollection.GetNew(line1, line2, line3);
			input.Client = data.Org;
			// test line where qty is not specified - should allocate all
			line1.PartAttrib3 = data.IReceiveLine132.PartAttrib3;
			// test line where qty is less than total in warehouse
			line2.PartAttrib3 = data.IReceiveLine122.PartAttrib3;
			line2.Quantity = data.IReceiveLine122.Quantity / 4m;
			// test line where qty equals total in warehouse, and warehouse is specified
			line3.PartAttrib3 = data.IReceiveLine121.PartAttrib3;
			line3.Quantity = data.IReceiveLine121.Quantity;
			line3.Warehouse = data.Address1;
			Factory.Save();
			var processor = new WhsOutwardsProcessor();
			var result = ProcessWithAllocateMock(input, processor);
			Assert("Incorrect Object Returned", result != null && result != input);
			AssertEquals(false, input.HasErrors);
			AssertEquals(false, result.HasErrors);
			AssertEquals(4, result.Lines.Count);
			AssertTransactionLine(result, data.IReceiveLine132, line1, data.Address1);
			AssertTransactionLine(result, data.IReceiveLine133, line1, data.Address1);
			AssertTransactionLine(result, data.IReceiveLine122, line2, data.Address1,
				data.IReceiveLine122.Quantity / 4m);
			AssertTransactionLine(result, data.IReceiveLine121, line3, data.Address1);
		}

		public void TestProcessUsingPartAttrib1MultiWarehouse()
		{
			TestDataForMultiWarehouseAttributes data = new TestDataForMultiWarehouseAttributes(Factory);
			WhsWarehouseTransaction input = new WhsWarehouseTransaction();
			WhsWarehouseTransactionLine line1 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line2 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line3 = new WhsWarehouseTransactionLine();
			input.Lines = WhsWarehouseTransactionLineCollection.GetNew(line1, line2, line3);
			input.Client = data.Org;
			// test line where qty is not specified - should allocate all
			line1.PartAttrib1 = data.IReceiveLine112.PartAttrib1;
			// test line where qty is less than total in warehouse
			line2.PartAttrib1 = data.IReceiveLine313.PartAttrib1;
			line2.Quantity = data.IReceiveLine313.Quantity / 2m;
			// test line where qty equals total in warehouse, and warehouse is specified
			// should pick from Address2, would normally pick from Address3 (line313b) if not specified
			line3.PartAttrib1 = data.IReceiveLine223.PartAttrib1;
			line3.Quantity = data.IReceiveLine223.Quantity;
			line3.Warehouse = data.Address2;
			Factory.Save();
			var processor = new WhsOutwardsProcessor();
			var result = ProcessWithAllocateMock(input, processor);
			Assert("Incorrect Object Returned", result != null && result != input);
			AssertEquals(false, input.HasErrors);
			AssertEquals(false, result.HasErrors);
			AssertEquals(4, result.Lines.Count);
			AssertTransactionLine(result, data.IReceiveLine112, line1, data.Address1);
			AssertTransactionLine(result, data.IReceiveLine113, line1, data.Address1);
			AssertTransactionLine(result, data.IReceiveLine313, line2, data.Address3,
				data.IReceiveLine313.Quantity / 2m);
			AssertTransactionLine(result, data.IReceiveLine223, line3, data.Address2);
		}

		public void TestProcessUsingPartAttrib2MultiWarehouse()
		{
			TestDataForMultiWarehouseAttributes data = new TestDataForMultiWarehouseAttributes(Factory);
			WhsWarehouseTransaction input = new WhsWarehouseTransaction();
			WhsWarehouseTransactionLine line1 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line2 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line3 = new WhsWarehouseTransactionLine();
			input.Lines = WhsWarehouseTransactionLineCollection.GetNew(line1, line2, line3);
			input.Client = data.Org;
			// test line where qty is not specified - should allocate all
			line1.PartAttrib2 = data.IReceiveLine122.PartAttrib2;
			// test line where qty is less than total in warehouse
			line2.PartAttrib2 = data.IReceiveLine313.PartAttrib2;
			line2.Quantity = data.IReceiveLine313.Quantity / 2m;
			// test line where qty equals total in warehouse, and warehouse is specified
			// should pick from Address2, would normally pick from Address3 (line313b) if not specified
			line3.PartAttrib2 = data.IReceiveLine223.PartAttrib2;
			line3.Quantity = data.IReceiveLine223.Quantity;
			line3.Warehouse = data.Address2;
			Factory.Save();
			var processor = new WhsOutwardsProcessor();
			var result = ProcessWithAllocateMock(input, processor);
			Assert("Incorrect Object Returned", result != null && result != input);
			AssertEquals(false, input.HasErrors);
			AssertEquals(false, result.HasErrors);
			AssertEquals(4, result.Lines.Count);
			AssertTransactionLine(result, data.IReceiveLine122, line1, data.Address1);
			AssertTransactionLine(result, data.IReceiveLine123, line1, data.Address1);
			AssertTransactionLine(result, data.IReceiveLine313, line2, data.Address3,
				data.IReceiveLine313.Quantity / 2m);
			AssertTransactionLine(result, data.IReceiveLine223, line3, data.Address2);
		}

		public void TestProcessUsingPartAttrib3MultiWarehouse()
		{
			TestDataForMultiWarehouseAttributes data = new TestDataForMultiWarehouseAttributes(Factory);
			WhsWarehouseTransaction input = new WhsWarehouseTransaction();
			WhsWarehouseTransactionLine line1 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line2 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line3 = new WhsWarehouseTransactionLine();
			input.Lines = WhsWarehouseTransactionLineCollection.GetNew(line1, line2, line3);
			input.Client = data.Org;
			// test line where qty is not specified - should allocate all
			line1.PartAttrib3 = data.IReceiveLine132.PartAttrib3;
			// test line where qty is less than total in warehouse
			line2.PartAttrib3 = data.IReceiveLine321.PartAttrib3;
			line2.Quantity = data.IReceiveLine321.Quantity / 2m;
			// test line where qty equals total in warehouse, and warehouse is specified
			// should pick from Address2, would normally pick from Address3 (line313b) if not specified
			line3.PartAttrib3 = data.IReceiveLine223.PartAttrib3;
			line3.Quantity = data.IReceiveLine223.Quantity;
			line3.Warehouse = data.Address2;
			Factory.Save();
			var processor = new WhsOutwardsProcessor();
			var result = ProcessWithAllocateMock(input, processor);
			Assert("Incorrect Object Returned", result != null && result != input);
			AssertEquals(false, input.HasErrors);
			AssertEquals(false, result.HasErrors);
			AssertEquals(4, result.Lines.Count);
			AssertTransactionLine(result, data.IReceiveLine132, line1, data.Address1);
			AssertTransactionLine(result, data.IReceiveLine133, line1, data.Address1);
			AssertTransactionLine(result, data.IReceiveLine321, line2, data.Address3,
				data.IReceiveLine321.Quantity / 2m);
			AssertTransactionLine(result, data.IReceiveLine223, line3, data.Address2);
		}

		#endregion

		#region All Attribute Tests

		public void TestProcessUsingAllAttributesSingleWarehouse()
		{
			TestDataForSingleWarehouseAttributes data = new TestDataForSingleWarehouseAttributes(Factory);
			WhsWarehouseTransaction input = new WhsWarehouseTransaction();
			WhsWarehouseTransactionLine line1 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line2 = new WhsWarehouseTransactionLine();
			input.Lines = WhsWarehouseTransactionLineCollection.GetNew(line1, line2);
			input.Client = data.Org;
			// test line where qty is less than total in warehouse
			line1.Product = data.IReceiveLine111.Product;
			line1.Quantity = data.IReceiveLine111.Quantity / 2m;
			line1.EntryKey = data.IReceiveLine111.EntryKey;
			line1.EntryLineNumber = data.IReceiveLine111.EntryLineNumber;
			line1.PartAttrib1 = data.IReceiveLine111.PartAttrib1;
			line1.PartAttrib2 = data.IReceiveLine111.PartAttrib2;
			// test line where qty equals total in warehouse, and warehouse is specified
			line2.Product = data.IReceiveLine132.Product;
			line2.Quantity = data.IReceiveLine132.Quantity;
			line2.EntryKey = data.IReceiveLine132.EntryKey;
			line2.EntryLineNumber = data.IReceiveLine132.EntryLineNumber;
			line2.PartAttrib1 = data.IReceiveLine132.PartAttrib1;
			line2.PartAttrib2 = data.IReceiveLine132.PartAttrib2;
			line2.PartAttrib3 = data.IReceiveLine132.PartAttrib3;
			Factory.Save();
			var processor = new WhsOutwardsProcessor();
			var result = ProcessWithAllocateMock(input, processor);
			Assert("Incorrect Object Returned", result != null && result != input);
			AssertEquals(false, input.HasErrors);
			AssertEquals(false, result.HasErrors);
			AssertEquals(2, result.Lines.Count);
			AssertTransactionLine(result, data.IReceiveLine111, data.Address1, data.IReceiveLine111.Quantity / 2m);
			AssertTransactionLine(result, data.IReceiveLine132, data.Address1);
		}

		public void TestProcessUsingAllAttributesMultiWarehouse()
		{
			TestDataForMultiWarehouseAttributes data = new TestDataForMultiWarehouseAttributes(Factory);
			WhsWarehouseTransaction input = new WhsWarehouseTransaction();
			WhsWarehouseTransactionLine line1 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line2 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line3 = new WhsWarehouseTransactionLine();
			input.Lines = WhsWarehouseTransactionLineCollection.GetNew(line1, line2, line3);
			input.Client = data.Org;
			// test line where qty is not specified - should allocate all
			line1.EntryKey = data.IReceiveLine323.EntryKey;
			line1.EntryLineNumber = data.IReceiveLine323.EntryLineNumber;
			line1.PartAttrib1 = data.IReceiveLine323.PartAttrib1;
			line1.PartAttrib2 = data.IReceiveLine323.PartAttrib2;
			line1.PartAttrib3 = data.IReceiveLine323.PartAttrib3;
			// test line where qty is less than total in warehouse
			line2.Product = data.IReceiveLine213.Product;
			line2.Quantity = data.IReceiveLine213.Quantity / 2m;
			line2.EntryKey = data.IReceiveLine213.EntryKey;
			line2.EntryLineNumber = data.IReceiveLine213.EntryLineNumber;
			line2.PartAttrib1 = data.IReceiveLine213.PartAttrib1;
			line2.PartAttrib2 = data.IReceiveLine213.PartAttrib2;
			line2.PartAttrib3 = data.IReceiveLine213.PartAttrib3;
			// test line where qty equals total in warehouse, and warehouse is specified
			// should pick from Address2, would normally pick from Address3 (line313b) if not specified
			line3.Product = data.IReceiveLine223.Product;
			line3.Quantity = data.IReceiveLine223.Quantity;
			line3.EntryKey = data.IReceiveLine223.EntryKey;
			line3.EntryLineNumber = data.IReceiveLine223.EntryLineNumber;
			line3.PartAttrib1 = data.IReceiveLine223.PartAttrib1;
			line3.PartAttrib2 = data.IReceiveLine223.PartAttrib2;
			line3.PartAttrib3 = data.IReceiveLine223.PartAttrib3;
			line3.Warehouse = data.Address2;
			Factory.Save();
			var processor = new WhsOutwardsProcessor();
			var result = ProcessWithAllocateMock(input, processor);
			Assert("Incorrect Object Returned", result != null && result != input);
			AssertEquals(false, input.HasErrors);
			AssertEquals(false, result.HasErrors);
			AssertEquals(3, result.Lines.Count);
			AssertTransactionLine(result, data.IReceiveLine323, data.Address3,
				data.IReceiveLine323.Quantity + data.IReceiveLine324.Quantity);
			AssertTransactionLine(result, data.IReceiveLine213, data.Address2, data.IReceiveLine213.Quantity / 2m);
			AssertTransactionLine(result, data.IReceiveLine223, data.Address2);
		}

		#endregion

		#endregion

		#region Implementation

		IWhsWarehouseTransaction ProcessWithAllocateMock(WhsWarehouseTransaction input, WhsOutwardsProcessor processor)
		{
			IWhsWarehouseTransaction result;
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				result = processor.Process(Factory, input, false);
			}

			return result;
		}

		#endregion
	}
}
