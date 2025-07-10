using System.Drawing;
using WinzorFramework;

namespace System.Windows.Forms;

public partial class DataGridRelationshipRow : DataGridRow
{
	public DataGridRelationshipRow(DataGrid dataGrid, DataGridTableStyle dgTable, int rowNumber)
	: base(dataGrid, dgTable, rowNumber)
	{
	}

	// will reset the FocusedRelation and will invalidate the
	// rectangle so that the linkFont is no longer shown
	internal override void LoseChildFocus(Rectangle rowHeaders, bool alignToRight)
	{
		// we only invalidate stuff if the row is expanded.
		if (FocusedRelation == -1 || !expanded)
		{
			return;
		}

		FocusedRelation = -1;
		NotifyRenderRequired();
	}

	int FocusedRelation
	{
		get => dgTable.FocusedRelation;
		set => dgTable.FocusedRelation = value;
	}

	bool expanded = defaultOpen;
	const bool defaultOpen = false;

	public virtual bool Expanded
	{
		get
		{
			return expanded;
		}
		set
		{
			if (expanded == value)
			{
				return;
			}

			if (expanded)
			{
				Collapse();
			}
			else
			{
				Expand();
			}
		}
	}

	void Collapse()
	{
		if (expanded)
		{
			expanded = false;
			// relationshipRect = Rectangle.Empty;
			FocusedRelation = -1;
			DataGrid.OnRowHeightChanged(this);
		}
	}

	void Expand()
	{
		if (!expanded
			&& DataGrid != null
			&& dgTable != null
			&& dgTable.RelationsList.Count > 0)
		{
			expanded = true;
			FocusedRelation = -1;

			// relationshipRect = Rectangle.Empty;
			DataGrid.OnRowHeightChanged(this);
		}
	}

	[Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in DataGridRelationshipRow.razor")]
	async Task HandleItemDropAsync(WinzorDragEventArgs arg)
	{
		if (DataGrid.AllowDrop)
		{
			var hitTestInfo = new DataGrid.HitTestInfo() { type = DataGrid.HitTestType.ParentRows, col = -1, row = RowNumber };
			await DataGrid.OnRowDropAsync(arg, hitTestInfo);
		}
	}
}
