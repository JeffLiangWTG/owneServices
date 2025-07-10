#nullable disable

namespace System.Windows.Forms;

public class DataGridViewRow
{
	public DataGridViewRow(int index)
	{
		Index = index;
	}

	public int Index { get; }

	public int Height { get; set; }

	public DataGridViewCellStyle DefaultCellStyle { get; set; } = new DataGridViewCellStyle();

	public DataGridViewCellCollection Cells
	{
		get
		{
			if (rowCells == null)
			{
				rowCells = new DataGridViewCellCollection();
			}
			return rowCells;
		}
	}

	DataGridViewCellCollection rowCells;
}
