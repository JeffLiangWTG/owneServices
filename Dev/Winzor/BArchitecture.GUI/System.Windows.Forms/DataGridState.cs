#nullable disable

namespace System.Windows.Forms;

internal sealed class DataGridState
{
	public object DataSource;
	public string DataMember;
	public CurrencyManager ListManager;
	public DataGridRow[] DataGridRows = Array.Empty<DataGridRow>();
	public DataGrid DataGrid;
	public int DataGridRowsLength;
	public GridColumnStylesCollection GridColumnStyles;

	public int FirstVisibleRow;
	public int FirstVisibleCol;

	public int CurrentRow;
	public int CurrentCol;

	public DataGridRow LinkingRow;

	// this is needed so that the parent rows will remove notification from the list
	// when the datagridstate is no longer needed;
	public void RemoveChangeNotification()
	{
		ListManager.ItemChanged -= new ItemChangedEventHandler(DataSource_Changed);
		ListManager.MetaDataChanged -= new EventHandler(DataSource_MetaDataChanged);
	}

	/// <summary>
	///  Called by a grid when it wishes to match its transient
	///  state with the current DataGridState object.
	/// </summary>
	public void PullState(DataGrid dataGrid, bool createColumn)
	{
		// dataGrid.DataSource = DataSource;
		// dataGrid.DataMember = DataMember;
		dataGrid.Set_ListManager(DataSource, DataMember, true, createColumn);   // true for forcing new listManager,

		/*
		if (DataSource.Table.ParentRelations.Count > 0)
			dataGrid.PopulateColumns();
		*/

		dataGrid.firstVisibleRow = FirstVisibleRow;
		dataGrid.firstVisibleCol = FirstVisibleCol;
		dataGrid.currentRow = CurrentRow;
		dataGrid.currentCol = CurrentCol;
		dataGrid.SetDataGridRows(DataGridRows, DataGridRowsLength);
	}

	void DataSource_Changed(object sender, ItemChangedEventArgs e)
	{
		if (DataGrid != null && ListManager.Position == e.Index)
		{
			DataGrid.InvalidateParentRows();
			return;
		}

		if (DataGrid != null)
		{
			DataGrid.ParentRowsDataChanged();
		}
	}

	void DataSource_MetaDataChanged(object sender, EventArgs e)
	{
		if (DataGrid != null)
		{
			DataGrid.ParentRowsDataChanged();
		}
	}
}
