#nullable disable

namespace System.Windows.Forms;

public class DataGridViewCell
{
	public DataGridViewCell(DataGridViewRow row, DataGridViewColumn column, int rowIndex, int columnIndex)
	{
		OwningRow = row;
		OwningColumn = column;
		RowIndex = rowIndex;
		ColumnIndex = columnIndex;
	}

	public object Value { get; set; }

	public virtual Type ValueType { get; set; }

	public DataGridViewRow OwningRow { get; }

	public DataGridViewColumn OwningColumn { get; }

	public virtual bool Selected { get; set; }

	public int ColumnIndex { get; }

	public int RowIndex { get; }
}
