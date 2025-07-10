using System.Drawing;

namespace System.Windows.Forms;

public partial class DataGridAddNewRow : DataGridRow
{
	public DataGridAddNewRow(DataGrid dGrid, DataGridTableStyle gridTable, int rowNum)
		: base(dGrid, gridTable, rowNum)
	{
	}

	public bool DataBound
	{
		get => dataBound;
		set => dataBound = value;
	}
	bool dataBound;

	// the addNewRow has nothing to do with losing focus
	internal override void LoseChildFocus(Rectangle rowHeader, bool alignToRight)
	{
	}

	public override void OnEdit()
	{
		if (!DataBound)
		{
			DataGrid.AddNewRow();
		}
	}

	public override void OnRowLeave()
	{
		if (DataBound)
		{
			DataBound = false;
		}
	}
}
