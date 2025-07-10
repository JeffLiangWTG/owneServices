namespace System.Windows.Forms;

class DataGridToolTip
{
	// the dataGrid which contains this toolTip
	[Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used to hold a reference")]
	readonly DataGrid dataGrid;

	public DataGridToolTip(DataGrid dataGrid)
	{
		this.dataGrid = dataGrid;
	}

	public void Destroy()
	{
	}
}
