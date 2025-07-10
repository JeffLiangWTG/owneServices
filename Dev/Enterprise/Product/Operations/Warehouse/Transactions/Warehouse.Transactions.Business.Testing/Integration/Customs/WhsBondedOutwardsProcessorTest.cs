using CargoWise.Application;
using Enterprise.Warehouse.Integration.BondedWarehouse;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business.Bonded.Testing
{
	internal class WhsBondedOutwardsProcessorTest : WhsOutwardsProcessorTestCase
	{
		public void TestProcessUsingProductSingleWarehouse()
		{
			TestDataForBondedEntries data = new TestDataForBondedEntries(Factory);
			Factory.Save();
			WhsBondedWarehouseTransaction input = new WhsBondedWarehouseTransaction();
			WhsBondedWarehouseTransactionLine line1 = new WhsBondedWarehouseTransactionLine();
			WhsBondedWarehouseTransactionLine line2 = new WhsBondedWarehouseTransactionLine();
			input.Lines = WhsBondedWarehouseTransactionLineCollection.GetNew(line1, line2);
			input.Client = data.Org;
			// Test line where qty is less than total in warehouse
			line1.Product = data.Part2;
			line1.Quantity = data.IReceiveLine3.Quantity / 3.0m;
			// Test line where qty equals total in warehouse, and warehouse is specified
			line2.Product = data.Part1;
			line2.Quantity = data.IReceiveLine1.Quantity + data.IReceiveLine2.Quantity;
			line2.Warehouse = data.Address;
			var processor = new WhsBondedOutwardsProcessor();
			var result = ProcessInput(input, processor);
			Assert("Incorrect Object Returned", result != null && result != input);
			AssertEquals(false, input.HasErrors);
			AssertEquals(false, result.HasErrors);
			AssertEquals(3, result.Lines.Count);
			foreach (IWhsBondedWarehouseTransactionLine resultLine in result.Lines)
			{
				if (IsEntryKeyMatching(data.IReceiveLine1, resultLine))
				{
					AssertTransactionLine(resultLine, data.IReceiveLine1, data.Whs.WarehouseAddress);
					AssertBondedData(resultLine, data.IReceiveLine1);
				}
				else if (IsEntryKeyMatching(data.IReceiveLine2, resultLine))
				{
					AssertTransactionLine(resultLine, data.IReceiveLine2, data.Whs.WarehouseAddress);
					AssertBondedData(resultLine, data.IReceiveLine2);
				}
				else if (IsEntryKeyMatching(data.IReceiveLine3, resultLine))
				{
					AssertTransactionLine(resultLine, data.IReceiveLine3, data.Whs.WarehouseAddress,
						data.IReceiveLine3.Quantity / 3m);
					AssertBondedData(resultLine, data.IReceiveLine3);
				}
				else
				{
					Fail("No lines found with this entry number");
				}
			}
		}

		public void TestProcessForRebuildingOfLines()
		{
			TestDataForBondedEntries data = new TestDataForBondedEntries(Factory);
			Factory.Save();
			WhsBondedWarehouseTransaction input = new WhsBondedWarehouseTransaction();
			WhsBondedWarehouseTransactionLine line1 = new WhsBondedWarehouseTransactionLine();
			WhsBondedWarehouseTransactionLine line2 = new WhsBondedWarehouseTransactionLine();
			WhsBondedWarehouseTransactionLine line3 = new WhsBondedWarehouseTransactionLine();
			input.Lines = WhsBondedWarehouseTransactionLineCollection.GetNew(line1, line2, line3);
			input.Client = data.Org;
			// test line where qty is less than total in warehouse
			line1.Product = data.Part2;
			line1.Quantity = data.IReceiveLine3.Quantity / 4m;
			line1.EntryKey = data.IReceiveLine3.EntryKey;
			line1.EntryLineNumber = data.IReceiveLine3.EntryLineNumber;
			line2.Product = data.Part2;
			line2.Quantity = data.IReceiveLine3.Quantity / 4m;
			line2.EntryKey = data.IReceiveLine3.EntryKey;
			line2.EntryLineNumber = data.IReceiveLine3.EntryLineNumber;
			// test line where qty equals total in warehouse, and warehouse is specified
			line3.Product = data.Part1;
			line3.Quantity = data.IReceiveLine1.Quantity + data.IReceiveLine2.Quantity;
			line3.Warehouse = data.Address;
			var processor = new WhsBondedOutwardsProcessor();
			var result = ProcessInput(input, processor);
			Assert("Incorrect Object Returned", result != null && result != input);
			AssertEquals(false, input.HasErrors);
			AssertEquals(false, result.HasErrors);
			AssertEquals(3, result.Lines.Count);
			foreach (IWhsBondedWarehouseTransactionLine resultLine in result.Lines)
			{
				if (data.IReceiveLine1.EntryKey == resultLine.EntryKey &&
					data.IReceiveLine1.EntryLineNumber == resultLine.EntryLineNumber)
				{
					AssertTransactionLine(resultLine, data.IReceiveLine1, data.Whs.WarehouseAddress);
					AssertBondedData(resultLine, data.IReceiveLine1);
				}
				else if (data.IReceiveLine2.EntryKey == resultLine.EntryKey &&
						 data.IReceiveLine2.EntryLineNumber == resultLine.EntryLineNumber)
				{
					AssertTransactionLine(resultLine, data.IReceiveLine2, data.Whs.WarehouseAddress);
					AssertBondedData(resultLine, data.IReceiveLine2);
				}
				else if (data.IReceiveLine3.EntryKey == resultLine.EntryKey &&
						 data.IReceiveLine3.EntryLineNumber == resultLine.EntryLineNumber)
				{
					AssertTransactionLine(resultLine, data.IReceiveLine3, data.Whs.WarehouseAddress,
						data.IReceiveLine3.Quantity / 2m);
					AssertBondedData(resultLine, data.IReceiveLine3);
				}
				else
				{
					Fail("No lines found with this entry number");
				}
			}
		}

		public void TestProcessingOfBondedWhsQty()
		{
			TestDataForBondedEntries data = new TestDataForBondedEntries(Factory);
			Factory.Save();
			WhsBondedWarehouseTransaction input = new WhsBondedWarehouseTransaction();
			WhsBondedWarehouseTransactionLine line1 = new WhsBondedWarehouseTransactionLine();
			input.Lines = WhsBondedWarehouseTransactionLineCollection.GetNew(line1);
			input.Client = data.Org;
			line1.Product = data.Part1;
			line1.Quantity = 5m;
			line1.BondedWarehouseQuantity = 10m;
			line1.BondedWarehouseQuantityUnit = "L";
			var processor = new WhsBondedOutwardsProcessor();
			var result = ProcessInput(input, processor);
			AssertEquals(false, input.HasWarnings);
			AssertEquals(false, result.HasWarnings);
			AssertEquals(1, result.Lines.Count);
			IWhsBondedWarehouseTransactionLine outputLine = result.Lines[0];
			AssertEquals("BondedWhsQty based on N30", 10m, outputLine.BondedWarehouseQuantity);
			AssertEquals("BondedWhsQtyUnit based on N20", "UNT", outputLine.BondedWarehouseQuantityUnit);
		}

		public void TestBondedWhsQtyWarnings()
		{
			TestDataForBondedEntries data = new TestDataForBondedEntries(Factory);
			Factory.Save();
			WhsBondedWarehouseTransaction input = new WhsBondedWarehouseTransaction();
			WhsBondedWarehouseTransactionLine line1 = new WhsBondedWarehouseTransactionLine();
			WhsBondedWarehouseTransactionLine line2 = new WhsBondedWarehouseTransactionLine();
			input.Lines = WhsBondedWarehouseTransactionLineCollection.GetNew(line1, line2);
			input.Client = data.Org;
			line1.Product = data.Part2;
			line1.Quantity = data.IReceiveLine3.Quantity;
			line2.Product = data.Part1;
			line2.Quantity = data.IReceiveLine1.Quantity + data.IReceiveLine2.Quantity;
			line2.BondedWarehouseQuantity =
				data.IReceiveLine2.BondedWarehouseQuantity + data.IReceiveLine3.BondedWarehouseQuantity;
			line2.Warehouse = data.Address;
			var processor = new WhsBondedOutwardsProcessor();
			var result = ProcessInput(input, processor);
			AssertEquals(false, input.HasWarnings);
			AssertEquals(true, result.HasWarnings);
			AssertEquals(3, result.Lines.Count);
			AssertEquals("Header Warnings",
				"One or more lines do not have a Warehouse Quantity entered. Please enter a Warehouse Quantity on each line with a warning and then Synchronize again.",
				result.Problems.WarningList[0]);
			foreach (IWhsBondedWarehouseTransactionLine resultLine in result.Lines)
			{
				if (resultLine.Product == data.Part1)
				{
					Assert(resultLine.BondedWarehouseQuantityProblems.HasWarnings);
					AssertEquals("Exact Warning", "Please enter the number of UNT.",
						resultLine.BondedWarehouseQuantityProblems.WarningList[0]);
					AssertEquals("One Warning Per Line", 1,
						resultLine.BondedWarehouseQuantityProblems.WarningList.Count);
					AssertEquals("BondedWhsQty",
						resultLine.EntryKey == "E11AA1" && resultLine.EntryLineNumber == 1 ? 50m : 5m,
						resultLine.BondedWarehouseQuantity);
					AssertEquals("BondedWhsQtyUnit", "UNT", resultLine.BondedWarehouseQuantityUnit);
				}
				else if (resultLine.Product == data.Part2)
				{
					Assert(resultLine.BondedWarehouseQuantityProblems.HasWarnings);
					AssertEquals("Exact Warning", "Please enter the number of UNT.",
						resultLine.BondedWarehouseQuantityProblems.WarningList[0]);
					AssertEquals("One Warning Per Line", 1,
						resultLine.BondedWarehouseQuantityProblems.WarningList.Count);
					AssertEquals("BondedWhsQty", 15m, resultLine.BondedWarehouseQuantity);
					AssertEquals("BondedWhsQtyUnit", "UNT", resultLine.BondedWarehouseQuantityUnit);
				}
				else
				{
					Fail("Did not expect this product");
				}
			}
		}

		public void TestManyInputLineToSingleOutputLine()
		{
			TestDataForBondedEntries data = new TestDataForBondedEntries(Factory);
			Factory.Save();
			WhsBondedWarehouseTransaction input = new WhsBondedWarehouseTransaction();
			WhsBondedWarehouseTransactionLine line1 = new WhsBondedWarehouseTransactionLine();
			WhsBondedWarehouseTransactionLine line2 = new WhsBondedWarehouseTransactionLine();
			WhsBondedWarehouseTransactionLine line3 = new WhsBondedWarehouseTransactionLine();
			input.Lines = WhsBondedWarehouseTransactionLineCollection.GetNew(line1, line2, line3);
			input.Client = data.Org;
			line1.Product = data.Part1;
			line1.Quantity = 5m;
			line1.BondedWarehouseQuantity = 5m;
			line1.EntryKey = "E11AA1";
			line1.EntryLineNumber = 1;
			line2.Product = data.Part1;
			line2.Quantity = 4m;
			line2.BondedWarehouseQuantity = 4m;
			line2.EntryKey = "E11AA1";
			line2.EntryLineNumber = 1;
			line3.Product = data.Part1;
			line3.Quantity = 6m;
			line3.BondedWarehouseQuantity = 6m;
			line3.EntryKey = "E11AA1";
			line3.EntryLineNumber = 1;
			var processor = new WhsBondedOutwardsProcessor();
			var result = ProcessInput(input, processor);
			AssertEquals(false, input.HasWarnings);
			AssertEquals(true, result.HasWarnings);
			AssertEquals(1, result.Lines.Count);
			IWhsBondedWarehouseTransactionLine resultLine = result.Lines[0];
			AssertEquals("Header Warnings","One or more lines do not have a Warehouse Quantity entered. Please enter a Warehouse Quantity on each line with a warning and then Synchronize again.",
				result.Problems.WarningList[0]);
			Assert(resultLine.BondedWarehouseQuantityProblems.HasWarnings);
			AssertEquals("Exact Warning", "Please enter the number of UNT.",
				resultLine.BondedWarehouseQuantityProblems.WarningList[0]);
			AssertEquals("One Warning Per Line", 1, resultLine.BondedWarehouseQuantityProblems.WarningList.Count);
			AssertEquals("BondedWhsQty", 15m, resultLine.BondedWarehouseQuantity);
			AssertEquals("BondedWhsQtyUnit", "UNT", resultLine.BondedWarehouseQuantityUnit);
		}

		bool IsEntryKeyMatching(IWhsBondedWarehouseTransactionLine inputLine,
			IWhsBondedWarehouseTransactionLine resultLine)
		{
			return inputLine.EntryKey == resultLine.EntryKey && inputLine.EntryLineNumber == resultLine.EntryLineNumber;
		}

		void AssertBondedData(IWhsBondedWarehouseTransactionLine result, IWhsBondedWarehouseTransactionLine input)
		{
			AssertEquals("EntryKey", input.EntryKey, result.EntryKey);
			AssertEquals("EntryLineNumber", input.EntryLineNumber, result.EntryLineNumber);
			AssertEquals("EntryDate", input.EntryDate, result.EntryDate);
			AssertEquals("AddInfo", input.AddInfo, result.AddInfo);
			AssertEquals("CountryOfOrigin", input.CountryOfOrigin, result.CountryOfOrigin);
			AssertEquals("QuantityUnit", input.QuantityUnit, result.QuantityUnit);
			AssertEquals("CustomsQuantityUnit", input.CustomsQuantityUnit, result.CustomsQuantityUnit);
			AssertEquals("BondedWarehouseQuantityUnit", input.BondedWarehouseQuantityUnit,
				result.BondedWarehouseQuantityUnit);
			var prorationRatio = result.Quantity / input.Quantity;
			AssertEquals("TILV Amount", Utilities.Round(input.TILV.Amount * prorationRatio, 4), result.TILV.Amount);
			AssertEquals("TILV Currency", input.TILV.Currency.Code, result.TILV.Currency.Code);
			AssertEquals("ValueForDuty", Utilities.Round(input.ValueForDuty * prorationRatio, 4), result.ValueForDuty);
			AssertEquals("CustomsQuantity", Utilities.Round(input.CustomsQuantity * prorationRatio, 5),
				result.CustomsQuantity);
		}

		public void TestBuildOutput()
		{
			Assert("incomplete test", true);
		}

		IWhsBondedWarehouseTransaction ProcessInput(WhsBondedWarehouseTransaction input,
			WhsBondedOutwardsProcessor processor)
		{
			IWhsBondedWarehouseTransaction result;
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				result = processor.Process(Factory, input, false);
			}

			return result;
		}
	}
}
