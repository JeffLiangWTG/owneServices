using System.Collections;
using System.Collections.Specialized;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration.BondedWarehouse;

namespace Enterprise.Warehouse.Transactions.Business.Bonded
{
	/// <summary>
	/// OBSOLETE -- remove after Customs switch over to new UXML Bonded.
	/// </summary>
	public class WhsBondedOutwardsProcessor : WhsOutwardsProcessor
	{
		#region Process

		public IWhsBondedWarehouseTransaction Process(BusinessObjectFactory factory, IWhsBondedWarehouseTransaction input, bool continueIfError)
		{
			return (IWhsBondedWarehouseTransaction)base.Process(factory, input, continueIfError);
		}

		#endregion

		#region Build Output

		protected override bool IsStockOkForAllocation(WhsInventoryView inventory)
		{
			return inventory.Warehouse.IsWarehouseBondEnabled && !inventory.WI_BondedEntryKey.IsEmpty;
		}

		protected override WhsWarehouseTransaction GetOutput()
		{
			return new WhsBondedWarehouseTransaction();
		}

		protected override WhsWarehouseTransactionLineCollection GetOutputLines()
		{
			return new WhsBondedWarehouseTransactionLineCollection();
		}

		protected override void PostBuildOutputLines()
		{
			PostBuildData data = new PostBuildData(Output.Lines.Count);

			foreach (IWhsBondedWarehouseTransactionLine inputLine in Input.Lines)
			{
				data.InputLine = inputLine;
				PostBuildOutputLine(data);
			}

			foreach (IWhsBondedWarehouseTransactionLine outputLine in Output.Lines)
			{
				data.OutputLine = outputLine;
				CheckOutputLines(data);
			}

			if (data.AddedWarning)
			{
				Output.Problems.WarningList.Add(Res.GetString("107e8e89-41fb-4f7f-a2cb-d40c000ac6b8", "One or more lines do not have a Warehouse Quantity entered. Please enter a Warehouse Quantity on each line with a warning and then Synchronize again."));
			}
		}

		void PostBuildOutputLine(PostBuildData data)
		{
			data.MatchesInEachIteration.Clear();
			AddOutputMatches(data);

			if (data.InputLine.BondedWarehouseQuantity > 0)
			{
				CheckOutputLineBondedWhsQty(data);
			}
			else
			{
				AddOutputWarnings(data);
			}
		}

		void CheckOutputLines(PostBuildData data)
		{
			if (!data.SuccessfullyMatched.Contains(data.OutputLine.GetHashCode()) &&
				!data.OutputLine.BondedWarehouseQuantityProblems.HasWarnings && IsBondedWarehouseQuantityRequired(data.OutputLine))
			{
				FlagWarningUserHasNotEnteredUnits(data.OutputLine);
				data.AddedWarning = true;
			}
		}

		void CheckOutputLineBondedWhsQty(PostBuildData data)
		{
			if (data.MatchesInEachIteration.Count == 1)
			{
				OneBondedWhsQtyMatch(data);
			}
			else if (data.MatchesInEachIteration.Count > 1)
			{
				MoreThanOneBondedWhsQtyMatch(data);
			}
		}

		void OneBondedWhsQtyMatch(PostBuildData data)
		{
			WhsBondedWarehouseTransactionLine exactMatchingOutputLine = (WhsBondedWarehouseTransactionLine)data.MatchesInEachIteration[0];
			if (IsBondedWarehouseQuantityRequired(exactMatchingOutputLine) &&
				!data.SuccessfullyMatched.Contains(exactMatchingOutputLine.GetHashCode()))
			{
				data.SuccessfullyMatched.Add(exactMatchingOutputLine.GetHashCode(), exactMatchingOutputLine);
				exactMatchingOutputLine.BondedWarehouseQuantity = data.InputLine.BondedWarehouseQuantity;
				UpdateBondedWhsQty(exactMatchingOutputLine);
			}
		}

		void MoreThanOneBondedWhsQtyMatch(PostBuildData data)
		{
			foreach (IWhsBondedWarehouseTransactionLine line in data.MatchesInEachIteration)
			{
				if (IsBondedWarehouseQuantityRequired(line))
				{
					FlagWarningMoreThanOneLineIsMatching(line);
					data.AddedWarning = true;
				}
			}
		}

		void AddOutputMatches(PostBuildData data)
		{
			foreach (IWhsBondedWarehouseTransactionLine outputLine in Output.Lines)
			{
				if (IsInputLineMatchingOutputLine(data.InputLine, outputLine))
				{
					data.MatchesInEachIteration.Add(outputLine);
				}
			}
		}

		void AddOutputWarnings(PostBuildData data)
		{
			foreach (IWhsBondedWarehouseTransactionLine outputLine in data.MatchesInEachIteration)
			{
				if (IsBondedWarehouseQuantityRequired(outputLine))
				{
					FlagWarningUserHasNotEnteredUnits(outputLine);
					data.AddedWarning = true;
				}
			}
		}

		bool IsBondedWarehouseQuantityRequired(IWhsBondedWarehouseTransactionLine line)
		{
			WhsOrderLine orderLine = Factory.Load<WhsOrderLine>(line.UniqueKey);
			return (orderLine.CustomsData.ReceiveEntry.WB_BondedWhsQty > 0);
		}

		void FlagWarningMoreThanOneLineIsMatching(IWhsBondedWarehouseTransactionLine lineToFlag)
		{
			string error = Res.GetString("593f2b59-2c8e-443f-beda-c66d51716d85", "Could not calculate the number of {0}. Please re-enter the number of {1}.", lineToFlag.BondedWarehouseQuantityUnit, lineToFlag.BondedWarehouseQuantityUnit);
			AddWarningToLine(lineToFlag, error);
		}

		void FlagWarningUserHasNotEnteredUnits(IWhsBondedWarehouseTransactionLine lineToFlag)
		{
			string error = Res.GetString("42b9830e-22d0-402c-9807-6869e0fefa86", "Please enter the number of {0}.", lineToFlag.BondedWarehouseQuantityUnit);
			AddWarningToLine(lineToFlag, error);
		}

		void AddWarningToLine(IWhsBondedWarehouseTransactionLine lineToFlag, string error)
		{
			if (!lineToFlag.BondedWarehouseQuantityProblems.WarningList.Contains(error))
			{
				lineToFlag.BondedWarehouseQuantityProblems.WarningList.Add(error);
			}
		}

		bool IsInputLineMatchingOutputLine(IWhsBondedWarehouseTransactionLine inputLine, IWhsBondedWarehouseTransactionLine outputLine)
		{
			WhsBondedWarehouseTransactionLine bondedInputLine = WhsBondedWarehouseTransactionLine.Copy(inputLine);
			return bondedInputLine.CompareForBondedWhsQtyWithIsEmptyCheck(outputLine);
		}

		void UpdateBondedWhsQty(IWhsBondedWarehouseTransactionLine resultLine)
		{
			WhsOrderLine orderLine = Factory.Load<WhsOrderLine>(resultLine.UniqueKey);
			orderLine.CustomsData.WB_BondedWhsQty = resultLine.BondedWarehouseQuantity;
		}

		class PostBuildData
		{
			public PostBuildData(int hybridDictionaryCount)
			{
				AddedWarning = false;
				MatchesInEachIteration = new ArrayList();
				SuccessfullyMatched = new HybridDictionary(hybridDictionaryCount);
			}

			public bool AddedWarning;
			public ArrayList MatchesInEachIteration;
			public HybridDictionary SuccessfullyMatched;
			public IWhsBondedWarehouseTransactionLine InputLine;
			public IWhsBondedWarehouseTransactionLine OutputLine;
		}

		#endregion
	}
}
