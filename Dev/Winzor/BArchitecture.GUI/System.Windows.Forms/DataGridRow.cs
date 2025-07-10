using System.Collections.Immutable;
using System.Drawing;
using Microsoft.AspNetCore.Components;
using WinzorFramework;
using WinzorFramework.Telemetry;

#nullable disable

namespace System.Windows.Forms;

public abstract class DataGridRow : Control
{
	/// <summary>
	///  Initializes a new instance of a <see cref='DataGridRow'/> .
	/// </summary>
	public DataGridRow(DataGrid dataGrid, DataGridTableStyle dgTable, int rowNumber)
	{
		if (dataGrid == null || dgTable.DataGrid == null)
		{
			throw new ArgumentNullException(nameof(dataGrid));
		}

		if (rowNumber < 0)
		{
			throw new ArgumentException(SR.DataGridRowRowNumber, nameof(rowNumber));
		}
		number = rowNumber;

		this.dgTable = dgTable;
		height = MinimumRowHeight(dgTable);
		notificationIcon = new NotificationIcon() { NotificationAnchorControl = this };
	}

	internal void CreateRenderedContent()
	{
		using var activity = TelemetryService.ActivitySource.StartActivity($"{GetType().Name}.{nameof(CreateRenderedContent)}");
		RenderedContentCreated = true;

		var builder = ImmutableList.CreateBuilder<ColumnViewModel>();
		var columnStyles = dgTable.GridColumnStyles;
		for (var i = 0; i < columnStyles.Count; i++)
		{
			var columnIndex = i;
			var columnStyle = columnStyles[i];

			if (columnStyle.PropertyDescriptor == null)
			{
				continue;
			}

			var mouseDownCallback = EventCallback.Factory.Create<WebMouseEventArgs>(this, (args) => DataGrid.OnMouseDownAsync(args, new DataGrid.HitTestInfo() { type = DataGrid.HitTestType.Cell, col = columnIndex, row = RowNumber }));
			var mouseUpCallback = EventCallback.Factory.Create<WebMouseEventArgs>(this, (args) => DataGrid.OnMouseUpAsync(args, new DataGrid.HitTestInfo() { type = DataGrid.HitTestType.Cell, col = columnIndex, row = RowNumber }));
			var focusInCallback = EventCallback.Factory.Create<WinzorFocusInEventArgs>(this, (args) => DataGrid.OnCellFocusInAsync(args, RowNumber, columnIndex));
			var contextMenuCallback = EventCallback.Factory.Create<WebMouseEventArgs>(this, (args) => DataGrid.OnContextMenuAsync(args, new DataGrid.HitTestInfo() { type = DataGrid.HitTestType.Cell, col = columnIndex, row = RowNumber }));
			DataGridCell cellData;
			if (this is DataGridAddNewRow newRow && !newRow.DataBound)
			{
				cellData = new DataGridCell(new MarkupString(), string.Empty, string.Empty, null, mouseDownCallback, mouseUpCallback, focusInCallback, contextMenuCallback);
			}
			else
			{
				cellData = new DataGridCell(
					columnStyle.GetRenderContent(DataGrid.ListManager, RowNumber, false),
					columnStyle.GetCellStyleString(DataGrid.ListManager, RowNumber),
					columnStyle.GetEditControlStyleString(DataGrid.ListManager, RowNumber),
					columnStyle.RowColumnNotification(RowNumber),
					mouseDownCallback,
					mouseUpCallback,
					focusInCallback,
					contextMenuCallback
				);
			}
			builder.Add(new ColumnViewModel(columnStyle, columnIndex, cellData, columnStyle.IsCurrentCellReadOnly));
		}
		ColumnsToRender = builder.ToImmutable();

		if (this is not DataGridAddNewRow)
		{
			DataGrid.UpdateRowNotification(RowNumber, notificationIcon);
		}
	}
	protected ImmutableList<ColumnViewModel> ColumnsToRender { get; private set; } = ImmutableList<ColumnViewModel>.Empty;

	internal bool RenderedContentCreated { get; set; }

	/// <summary>
	///  Gets the <see cref='Forms.DataGrid'/> control the row belongs to.
	/// </summary>
	public DataGrid DataGrid => dgTable.DataGrid;

	internal DataGridTableStyle DataGridTableStyle
	{
		get => dgTable;
		set => dgTable = value;
	}
	protected DataGridTableStyle dgTable;

	/// <summary>
	///  Gets the row's number.
	/// </summary>
	public int RowNumber => number;
	internal protected int number;

	public override bool UseParentDivForLayout => false;

	internal NotificationIcon notificationIcon;

	/// <summary>
	///  Gets or sets a value indicating whether the row is selected.
	/// </summary>
	public virtual bool Selected
	{
		get => selected;
		set => UpdateProperty(ref selected, value);
	}
	bool selected;

	/// <summary>
	///  Gets or sets the height of the row.
	/// </summary>
	public new virtual int Height
	{
		get => height;
		set
		{
			// the height of the row should be at least 0.
			// this way, if the row has a relationship list and the user resizes the row such that
			// the new height does not accomodate the height of the relationship list
			// the row will at least show the relationship list ( and not paint on the portion of the row above this one )
			height = Math.Max(0, value);
			// when we resize the row, or when we set the PreferredRowHeigth on the
			// DataGridTableStyle, we change the height of the Row, which will cause to invalidate,
			// then the grid itself will do another invalidate call.
			dgTable.DataGrid.OnRowHeightChanged(this);
		}
	}
	int height;

	internal protected virtual int MinimumRowHeight(DataGridTableStyle dgTable)
	{
		return MinimumRowHeight(dgTable.GridColumnStyles);
	}

	internal protected virtual int MinimumRowHeight(GridColumnStylesCollection columns)
	{
		int h = dgTable.IsDefault ? DataGrid.PreferredRowHeight : dgTable.PreferredRowHeight;

		try
		{
			if (dgTable.DataGrid.DataSource != null)
			{
				int nCols = columns.Count;
				for (int i = 0; i < nCols; ++i)
				{
					// if (columns[i].Visible && columns[i].PropertyDescriptor != null)
					if (columns[i].PropertyDescriptor != null)
					{
						h = Math.Max(h, columns[i].GetMinimumHeight());
					}
				}
			}
		}
		catch
		{
		}
		return h;
	}

	internal abstract void LoseChildFocus(Rectangle rowHeaders, bool alignToRight);

	/// <summary>
	///  When overridden in a derived class, notifies the grid that an edit will
	///  occur.
	/// </summary>
	public virtual void OnEdit()
	{
	}

	/// <summary>
	///  When overridden in a derived class, causes the RowEnter event to occur.
	/// </summary>
	public virtual void OnRowEnter() { }
	public virtual void OnRowLeave() { }

	/// <summary>
	///  When overridden in a derived class, gets the <see cref='Rectangle'/>
	///  where a cell's contents gets painted.
	/// </summary>
	public virtual Rectangle GetCellBounds(int col)
	{
		Rectangle cellBounds = new Rectangle();
		GridColumnStylesCollection columns = dgTable.GridColumnStyles;
		if (columns != null)
		{
			int borderWidth = dgTable.GridLineWidth;
			cellBounds = new Rectangle(0,
								 0,
								 columns[col].Width - borderWidth,
								 Height - borderWidth);
		}
		return cellBounds;
	}

	/// <summary>
	///  When overridden in a derived class, called by the <see cref='Forms.DataGrid'/> control when a key press occurs on a row with focus.
	/// </summary>
	public virtual bool OnKeyPress(Keys keyData)
	{
		int currentColIndex = dgTable.DataGrid.CurrentCell.ColumnNumber;
		GridColumnStylesCollection columns = dgTable.GridColumnStyles;
		if (columns != null && currentColIndex >= 0 && currentColIndex < columns.Count)
		{
			DataGridColumnStyle currentColumn = columns[currentColIndex];
			if (currentColumn.KeyPress(RowNumber, keyData))
			{
				return true;
			}
		}
		return false;
	}

	protected override Control RenderParentNode => DataGrid;

	protected internal override void OnBeforeRender()
	{
		using var activity = TelemetryService.ActivitySource.StartActivity($"{GetType().Name}.{nameof(OnBeforeRender)}");
		if (!DataGrid.IsDisposed && DataGrid.IsRowVisible(RowNumber) && RowNumber < DataGrid.ListManager.Count && !RenderedContentCreated)
		{
			CreateRenderedContent();
		}

		base.OnBeforeRender();
	}

	internal bool RowHidden;

	protected string GetStyleString() => $"height:{Height}px;";

	protected string NotificationCss(string notification) => string.IsNullOrEmpty(notification) ? string.Empty : "datagrid__cell--" + notification;

	protected string RowCss(bool isSelected, bool isEditing) => $"datagrid__row {(RowHidden ? "datagrid__row--hidden" : string.Empty)} {(isSelected ? "datagrid__row--selected" : string.Empty)} {(isEditing ? "datagrid__row--edit" : string.Empty)}".Trim();

	protected string IndicatorCss(bool isRowEditing) => $"{(isRowEditing ? "datagrid__row-arrow--new" : "datagrid__row-star--new")}";

	protected async Task RowHeaderMouseDownAsync(WebMouseEventArgs args) => await DataGrid.OnMouseDownAsync(args, new DataGrid.HitTestInfo() { type = DataGrid.HitTestType.RowHeader, col = -1, row = RowNumber });

	protected async Task RowHeaderMouseUpAsync(WebMouseEventArgs args) => await DataGrid.OnMouseUpAsync(args, new DataGrid.HitTestInfo() { type = DataGrid.HitTestType.RowHeader, col = -1, row = RowNumber });

	protected async Task RowHeaderContextMenuAsync(WebMouseEventArgs args) => await DataGrid.OnContextMenuAsync(args, new DataGrid.HitTestInfo() { type = DataGrid.HitTestType.RowHeader, col = -1, row = RowNumber });

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
	protected record DataGridCell(MarkupString Html, string StyleString, string EditControlStyleString, NotificationIcon Notification, EventCallback<WebMouseEventArgs> MouseDown, EventCallback<WebMouseEventArgs> MouseUp, EventCallback<WinzorFocusInEventArgs> FocusIn, EventCallback<WebMouseEventArgs> ContextMenu);

	protected record ColumnViewModel(DataGridColumnStyle ColumnStyle, int ColumnIndex, DataGridCell CellData, bool isCellReadOnly);
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter

	protected override void Dispose(bool disposing)
	{
		if (!IsDisposed && disposing)
		{
			notificationIcon?.Dispose();
		}

		base.Dispose(disposing);
	}
}
