using System;
using System.Collections.Specialized;
using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public class BaseInvoiceArrayBoundGrid : ZGridWithoutColumnStylesSerialisation
	{
		public event ListChangedEventHandler InvoiceListChanged;
		protected override void OnListChanged(ListChangedEventArgs e)
		{
			base.OnListChanged(e);
			if (e.ListChangedType == ListChangedType.ItemDeleted)
			{
				if (InvoiceListChanged != null)
				{
					InvoiceListChanged(this, e);
				}
			}
		}

		public void AddColumnToSkip(string mappingName)
		{
			ColumnsToSkip.Add(mappingName);
		}

		public StringCollection ColumnsToSkip
		{
			get
			{
				if (fColumnsToSkip == null)
				{
					fColumnsToSkip = new StringCollection();
				}
				return fColumnsToSkip;
			}
		}

		#region Implementation

		protected override void OnEnter(EventArgs e)
		{
			base.OnEnter(e);
			var currentCell = CurrentCell;
			var currentRow = currentCell.RowNumber;
			var currentColumn = currentCell.ColumnNumber;
			try
			{
				if (!ColumnHasTabStop(currentRow, currentColumn) && FindNextCellToTabInto(true, ref currentRow, ref currentColumn))
				{
					CurrentCell = new System.Windows.Forms.DataGridCell(currentRow, currentColumn);
				}
			}
			catch (NullReferenceException)
			{
				// do nothing
			}
		}

		protected override bool ColumnHasTabStop(int row, int column)
		{
			if (TableStyles.Count > 0 && TableStyles[0].GridColumnStyles.Count > column)
			{
				if (ColumnsToSkip.Contains(TableStyles[0].GridColumnStyles[column].MappingName))
				{
					return false;
				}
				return base.ColumnHasTabStop(row, column);
			}
			else
			{
				return true;
			}
		}

		StringCollection fColumnsToSkip;

		#endregion
	}
}
