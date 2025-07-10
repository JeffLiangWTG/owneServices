// will be renamed after old class deleted
using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transit.Business
{
	public abstract class TransitLogTableHelper<BOType, ColumnEnum> where ColumnEnum : struct
	{
		public int MaximumColumnWidth { get; set; } = 50;
		public virtual int MinimumColumnWidth { get; set; } = 15;
		public int MinimumSpaceBetweenColumns { get; set; } = 4;
		public int MaximumRows { get; set; } = 50;

		public ZString GetTable(ZString tableMessage, IEnumerable<BOType> businessObjects, params ColumnEnum[] columnIDs)
		{
			var businessObjectArray = businessObjects?.ToArray();
			if (businessObjectArray != null && businessObjectArray.Length > 0)
			{
				var columns = new List<TransitLogColumn<ColumnEnum>>(columnIDs.Length);

				var numEntities = businessObjectArray.Length;
				var businessObjectsArrayUpToMaxRows = businessObjectArray.Take(MaximumRows);
				AddFetchHints(businessObjectsArrayUpToMaxRows);

				foreach (var columnID in columnIDs)
				{
					columns.Add(GetColumn(businessObjectsArrayUpToMaxRows, columnID));
				}

				return GetTable(tableMessage, columns.ToArray(), numEntities).ToString().TrimEnd();
			}

			return ZString.Empty;
		}

		protected ZString GetTable(ZString messageWithTable, TransitLogColumn<ColumnEnum>[] columns, int numEntities)
		{
			var table = new ZStringBuilder();

			if (columns != null && columns.Length > 0)
			{
				// Check that the columns are the same length
				var numberOfRows = columns.First().Values.Length;
				if (numberOfRows > 0 && columns.All(c => c.Values.Length == numberOfRows))
				{
					// Sort row orders
					SortRows(columns);

					ProcessRows(columns);

					// Fit the column widths
					DefaultAndPadCells(columns);

					table.AppendLine(messageWithTable);
					table.AppendLine(GetTableHeader(columns));

					for (var i = 0; i < numberOfRows; i++)
					{
						table.AppendLine(GetTableRow(columns, i));
					}

					if (numEntities > MaximumRows)
					{
						table.AppendLine(Res.GetString("ed95ba5d-ed03-43e4-818e-c40a8f2375fa", "<Displaying top {0} of {1} records>", MaximumRows, numEntities));
					}
				}
			}

			return table.ToString();
		}

		public TransitLogColumn<ColumnEnum> GetColumn(IEnumerable<BOType> businessObjects, ColumnEnum columnID)
			=> new TransitLogColumn<ColumnEnum>(GetHeader(columnID), businessObjects.Select(p => GetValue(p, columnID)).ToArray(), columnID);

		protected virtual void AddFetchHints(IEnumerable<BOType> businessObjects)
		{
		}

		protected abstract ZString GetValue(BOType businessObject, ColumnEnum columnID);

		protected abstract ZString GetHeader(ColumnEnum columnID);

		void DefaultAndPadCells(TransitLogColumn<ColumnEnum>[] columns)
		{
			for (var columnNum = 0; columnNum < columns.Length; columnNum++)
			{
				var column = columns[columnNum];
				DefaultColumnWidth(column);
				var shouldPadRight = columnNum < columns.Length - 1;
				column.Header = DefaultCell(column.Header, column.Width, shouldPadRight);

				for (var i = 0; i < column.Values.Length; i++)
				{
					column.Values[i] = DefaultCell(column.Values[i], column.Width, shouldPadRight);
				}
			}
		}

		ZString DefaultCell(ZString cell, int width, bool shouldPadRight)
		{
			if (string.IsNullOrEmpty(cell))
			{
				cell = "-";
			}
			else if (cell.Length > width - MinimumSpaceBetweenColumns)
			{
				// 4 is the minimum size for truncating. ZString replaces the last 3 characters with '...'
				var maximumCellWidth = Math.Max(MinimumSpaceBetweenColumns, width - MinimumSpaceBetweenColumns);
				cell = cell.Truncate(maximumCellWidth);
			}

			if (shouldPadRight)
			{
				cell = cell.PadRight(width);
			}

			return cell;
		}

		void DefaultColumnWidth(TransitLogColumn<ColumnEnum> column)
		{
			if (column.Width == 0)
			{
				column.Width = MinimumColumnWidth;
				column.Width = Math.Max(column.Header.Length + MinimumSpaceBetweenColumns, column.Width);
				column.Width = Math.Max(column.Values.Max(v => v.Length) + MinimumSpaceBetweenColumns, column.Width);
				column.Width = Math.Min(MaximumColumnWidth, column.Width);
			}
		}

		void SortRows(TransitLogColumn<ColumnEnum>[] columns)
		{
			var rows = GetRowsFromColumns(columns);
			var sortedRows = rows.OrderBy(t => t, new ZStringListComparer()).ToArray();

			// Overwrite the values in each column with the values from the sorted rows
			for (int rowNumber = 0; rowNumber < rows.Count; rowNumber++)
			{
				for (var columnNumber = 0; columnNumber < columns.Length; columnNumber++)
				{
					columns[columnNumber].Values[rowNumber] = sortedRows[rowNumber][columnNumber];
				}
			}
		}

		protected virtual void ProcessRows(TransitLogColumn<ColumnEnum>[] columns)
		{
		}

		static List<List<ZString>> GetRowsFromColumns(TransitLogColumn<ColumnEnum>[] columns)
		{
			var numberOfRows = columns.First().Values.Length;
			var rows = new List<List<ZString>>(numberOfRows);

			for (int i = 0; i < numberOfRows; i++)
			{
				rows.Add(new List<ZString>(columns.Length));

				foreach (var column in columns)
				{
					rows[i].Add(column.Values[i]);
				}
			}

			return rows;
		}

		class ZStringListComparer : Comparer<List<ZString>>
		{
			public override int Compare(List<ZString> x, List<ZString> y)
			{
				var comparer = Comparer<ZString>.Default;
				for (var i = 0; i < x.Count; i++)
				{
					var delta = comparer.Compare(x[i], y[i]);
					if (delta != 0)
					{
						return delta;
					}
				}
				return 0;
			}
		}

		static ZString GetTableHeader(IEnumerable<TransitLogColumn<ColumnEnum>> columns) => ZString.Join(ZString.Empty, columns.Select(c => c.Header).ToArray());

		static ZString GetTableRow(IEnumerable<TransitLogColumn<ColumnEnum>> columns, int rowNumber) => ZString.Join(ZString.Empty, columns.Select(c => c.Values[rowNumber]).ToArray());
	}
}
