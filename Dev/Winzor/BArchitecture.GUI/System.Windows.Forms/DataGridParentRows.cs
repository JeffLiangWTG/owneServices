using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;

#nullable disable

namespace System.Windows.Forms;

class DataGridParentRows
{
	// storage for parent row states
	//
	readonly ArrayList parents = new ArrayList();
	int parentsCount;

	readonly DataGrid dataGrid;

	readonly ColorMap[] colorMap = new ColorMap[] { new ColorMap() };

	internal DataGridParentRows(DataGrid dataGrid)
	{
		colorMap[0].OldColor = Color.Black;
		this.dataGrid = dataGrid;
	}

	internal Color BackColor { get; set; }

	internal Color ForeColor { get; set; }

	internal void Clear()
	{
	}

	internal bool IsEmpty() => false;

	internal void OnLayout()
	{
	}

	/// <summary>
	///  Similar to GetTopParent() but also removes it.
	/// </summary>
	internal DataGridState PopTop()
	{
		if (parentsCount < 1)
		{
			return null;
		}

		SetParentCount(parentsCount - 1);
		DataGridState ret = (DataGridState)parents[parentsCount];
		ret.RemoveChangeNotification();
		parents.RemoveAt(parentsCount);
		return ret;
	}

	internal void SetParentCount(int count)
	{
		parentsCount = count;
		dataGrid.Caption.BackButtonVisible = (parentsCount > 0) && (dataGrid.AllowNavigation);
	}

	/// <summary>
	///  Retrieves the top most parent in the list of parents.
	/// </summary>
	internal DataGridState GetTopParent()
	{
		if (parentsCount < 1)
		{
			return null;
		}
		return (DataGridState)(((ICloneable)(parents[parentsCount - 1])).Clone());
	}
}
