using System;
using System.Collections;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Business
{
	public abstract class WhsWarehouseTransactionSplitter
	{
		public IWhsWarehouseTransaction[] Split(IWhsWarehouseTransaction input)
		{
			// note this function modifies the order of Input.Lines when sorting
			ArrayList output = SplitCore(input);

			IWhsWarehouseTransaction[] result = (IWhsWarehouseTransaction[])output.ToArray(GetNewTransactionType());
			return result;
		}

		#region SplitCore

		protected ArrayList SplitCore(IWhsWarehouseTransaction input)
		{
			WhsWarehouseTransactionLineCollection sortedLines = (WhsWarehouseTransactionLineCollection)input.Lines.Clone();
			sortedLines.Sort(Comparer);

			ArrayList output = new ArrayList();
			BuildNewOutput(input, output, sortedLines);

			return output;
		}

		void BuildNewOutput(IWhsWarehouseTransaction input, ArrayList output, WhsWarehouseTransactionLineCollection sortedLines)
		{
			int fromLine = 0;
			int currLine = 1;

			while (currLine < sortedLines.Count)
			{
				if (ShouldSplitLine(sortedLines[currLine - 1], sortedLines[currLine]))
				{
					output.Add(CreateNewTransaction(input, sortedLines, fromLine, currLine - 1));
					fromLine = currLine;
				}
				++currLine;
			}
			output.Add(CreateNewTransaction(input, sortedLines, fromLine, currLine - 1));
		}

		IWhsWarehouseTransaction CreateNewTransaction(IWhsWarehouseTransaction input, WhsWarehouseTransactionLineCollection sortedLines, int fromLine, int toLine)
		{
			WhsWarehouseTransaction result = GetNewTransaction();
			CopyTransaction(input, result);
			CopyTransactionLines(result, sortedLines, fromLine, toLine);
			return result;
		}

		#endregion

		#region Copying

		void CopyTransaction(IWhsWarehouseTransaction from, WhsWarehouseTransaction to)
		{
			to.Client = (OrgHeader)from.Client;
			to.Date = from.Date;
			to.ExternalPK = from.ExternalPK;
			to.Reference = from.Reference;
			to.TransportCompany = (OrgHeader)from.TransportCompany;

			CopyTransactionExtra(from, to);
		}

		void CopyTransactionLines(WhsWarehouseTransaction to, WhsWarehouseTransactionLineCollection lines, int fromLine, int toLine)
		{
			//To.Lines = new WarehouseTransactionLineCollection();

			for (int c = 0, i = fromLine; i <= toLine; i++, c++)
			{
				to.Lines.Add(GetNewTransactionLine());
				CopyTransactionLine(lines[i], (WhsWarehouseTransactionLine)to.Lines[c]);
			}
		}

		void CopyTransactionLine(IWhsWarehouseTransactionLine from, WhsWarehouseTransactionLine to)
		{
			to.EntryKey = from.EntryKey;
			to.EntryLineNumber = from.EntryLineNumber;
			to.PartAttrib1 = from.PartAttrib1;
			to.PartAttrib2 = from.PartAttrib2;
			to.PartAttrib3 = from.PartAttrib3;
			to.Product = from.Product;
			to.Quantity = from.Quantity;
			to.QuantityUnit = from.QuantityUnit;
			to.Warehouse = from.Warehouse;

			CopyTransactionLineExtra(from, to);
		}

		protected virtual void CopyTransactionExtra(IWhsWarehouseTransaction from, WhsWarehouseTransaction to) { }
		protected virtual void CopyTransactionLineExtra(IWhsWarehouseTransactionLine from, WhsWarehouseTransactionLine to) { }

		#endregion

		#region To Override

		protected abstract bool ShouldSplitLine(IWhsWarehouseTransactionLine line1, IWhsWarehouseTransactionLine line2);
		protected abstract IComparer Comparer { get; }

		protected virtual Type GetNewTransactionType()
		{
			return typeof(IWhsWarehouseTransaction);
		}

		protected virtual WhsWarehouseTransaction GetNewTransaction()
		{
			return new WhsWarehouseTransaction();
		}

		protected virtual WhsWarehouseTransactionLine GetNewTransactionLine()
		{
			return new WhsWarehouseTransactionLine();
		}

		#endregion
	}
}
