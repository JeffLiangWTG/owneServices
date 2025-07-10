#nullable disable

namespace System.Windows.Forms;

public class DataGridViewColumn
{
	public DataGridViewColumn(int index)
	{
		Index = index;
	}

	public string Name { get; set; }

	public string HeaderText { get; set; }

	public string DataPropertyName { get; set; }

	public Type ValueType { get; set; }

	public DataGridViewCellStyle DefaultCellStyle { get; set; } = new DataGridViewCellStyle();

	public int Index { get; }

	public int Width { get; set; }

	public float FillWeight { get; set; }

	public DataGridViewColumnSortMode SortMode { get; set; }

	public virtual bool ReadOnly { get; set; }

	public DataGridViewAutoSizeColumnMode AutoSizeMode { get; set; }
}
