using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WinzorFramework;
using WinzorFramework.Extensions;
using WinzorFramework.JSInterop;
using WinzorFramework.Telemetry;

#nullable disable

namespace System.Windows.Forms;

public partial class DataGrid : Control, ISupportInitialize
{
	public DataGrid()
	{
		dotNetObjectReference = DotNetObjectReference.Create(this);
		NotificationIcon = new NotificationIcon() { NotificationAnchorControl = this };
		NotificationIcon.OnClickIcon += NotificationClickEventHandler;

		gridState = new Collections.Specialized.BitVector32(0x00042827);

		dataGridTables = new GridTableStylesCollection(this);
		layout = new LayoutData()
		{
			ColumnHeadersVisible = true,
			RowHeadersVisible = true,
			ParentRowsVisible = defaultParentRowsVisible,
		};
		parentRows = new DataGridParentRows(this);

		BackColor = SystemColors.Window;
		ForeColor = DefaultForeColor;
		borderStyle = defaultBorderStyle;

		currentChangedHandler = new EventHandler(DataSource_RowChanged);
		positionChangedHandler = new EventHandler(DataSource_PositionChanged);
		itemChangedHandler = new ItemChangedEventHandler(DataSource_ItemChanged);
		metaDataChangedHandler = new EventHandler(DataSource_MetaDataChanged);
		dataGridTableStylesCollectionChanged = new CollectionChangeEventHandler(TableStylesCollectionChanged);
		dataGridTables.CollectionChanged += dataGridTableStylesCollectionChanged;

		SetDataGridTable(defaultTableStyle, true);

		caption = new DataGridCaption(this);

		RecalculateFonts();
		Size = new Size(130, 80);
		Invalidate();
		PerformLayout();
	}

	protected override Size DefaultSize => new Size(130, 80);

	/// <summary>
	///  Raises the <see cref='Control.CreateHandle'/>
	///  event.
	/// </summary>
	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);

		// toolTipping
		toolTipProvider = new DataGridToolTip(this);
		toolTipId = 0;

		PerformLayout();
	}

	/// <summary>
	///  Raises the <see cref='Control.DestroyHandle'/>
	///  event.
	/// </summary>
	protected override void OnHandleDestroyed(EventArgs e)
	{
		base.OnHandleDestroyed(e);

		// toolTipping
		if (toolTipProvider != null)
		{
			toolTipProvider.Destroy();
			toolTipProvider = null;
		}
		toolTipId = 0;
	}

	/// <summary>
	///  Raises the <see cref='Control.Enter'/>
	///  event.
	/// </summary>
	protected override void OnEnter(EventArgs e)
	{
		if (gridState[GRIDSTATE_canFocus] && !gridState[GRIDSTATE_editControlChanging])
		{
			if (Bound)
			{
				Edit();
			}
			base.OnEnter(e);
		}
	}

	/// <summary>
	///  Raises the <see cref='Control.Leave'/>
	///  event.
	/// </summary>
	protected override void OnLeave(EventArgs e)
	{
		OnLeave_Grid();
		base.OnLeave(e);
	}

	void OnLeave_Grid()
	{
		gridState[GRIDSTATE_canFocus] = false;
		try
		{
			EndEdit();
			if (listManager != null && !gridState[GRIDSTATE_editControlChanging])
			{
				if (gridState[GRIDSTATE_inAddNewRow])
				{
					// if the user did not type anything in the addNewRow, then cancel the currentedit
					listManager.CancelCurrentEdit();
					// set the addNewRow back
					DataGridRow[] localGridRows = DataGridRows;
					localGridRows[DataGridRowsLength - 1] = new DataGridAddNewRow(this, myGridTable, DataGridRowsLength - 1);
					SetDataGridRows(localGridRows, DataGridRowsLength);
				}
				else
				{
					HandleEndCurrentEdit();
				}
			}
		}
		finally
		{
			gridState[GRIDSTATE_canFocus] = true;
			// inAddNewRow should be set to false if the control was not changing
			if (!gridState[GRIDSTATE_editControlChanging])
			{
				gridState[GRIDSTATE_inAddNewRow] = false;
			}
		}
	}

	void HandleEndCurrentEdit()
	{
		int currentRowSaved = currentRow;
		int currentColSaved = currentCol;

		string errorMessage = null;

		try
		{
			listManager.EndCurrentEdit();
		}
		catch (Exception e)
		{
			errorMessage = e.Message;
		}

		if (errorMessage != null)
		{
			DialogResult result = MessageBox.Show(null, string.Format(SR.DataGridPushedIncorrectValueIntoColumn,
					errorMessage), SR.DataGridErrorMessageBoxCaption, MessageBoxButtons.YesNo,
					MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);

			if (result == DialogResult.Yes)
			{
				currentRow = currentRowSaved;
				currentCol = currentColSaved;
				// also, make sure that we get the row selector on the currentrow, too
				InvalidateRowHeader(currentRow);
				Edit();
			}
			else
			{
				// if the user committed a row that used to be addNewRow and the backEnd rejects it,
				// and then it tries to navigate down then we should stay in the addNewRow
				// in this particular scenario, CancelCurrentEdit will cause the last row to be deleted,
				// and this will ultimately call InvalidateRow w/ a row number larger than the number of rows
				// so set the currentRow here:
				listManager.PositionChanged -= positionChangedHandler;
				listManager.CancelCurrentEdit();
				listManager.Position = currentRow;
				listManager.PositionChanged += positionChangedHandler;
			}
		}
	}

	void NotificationClickEventHandler(object sender, EventArgs e)
	{
		for (var rowIndex = 0; rowIndex < DataGridRowsLength; rowIndex++)
		{
			for (var colIndex = 0; colIndex < myGridTable.GridColumnStyles.Count; colIndex++)
			{
				if (myGridTable.GridColumnStyles[colIndex] is var column && column.RowColumnNotification(rowIndex)?.NotificationType == "error")
				{
					SuspendLayout();
					Select(rowIndex);
					EnsureVisible(rowIndex, colIndex);
					BeginEdit(column, rowIndex);
					ResumeLayout();
					return;
				}
			}
		}
	}

	/// <summary>
	///  Gets or sets the value of the cell at
	///  the specified the row and column.
	/// </summary>
	public object this[int rowIndex, int columnIndex]
	{
		get
		{
			EnsureBound();
			if (rowIndex < 0 || rowIndex >= DataGridRowsLength)
			{
				throw new ArgumentOutOfRangeException(nameof(rowIndex));
			}

			if (columnIndex < 0 || columnIndex >= myGridTable.GridColumnStyles.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(columnIndex));
			}

			CurrencyManager listManager = this.listManager;
			DataGridColumnStyle column = myGridTable.GridColumnStyles[columnIndex];
			return column.GetColumnValueAtRow(listManager, rowIndex);
		}
		set
		{
			EnsureBound();
			if (rowIndex < 0 || rowIndex >= DataGridRowsLength)
			{
				throw new ArgumentOutOfRangeException(nameof(rowIndex));
			}

			if (columnIndex < 0 || columnIndex >= myGridTable.GridColumnStyles.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(columnIndex));
			}

			CurrencyManager listManager = this.listManager;
			if (listManager.Position != rowIndex)
			{
				listManager.Position = rowIndex;
			}

			DataGridColumnStyle column = myGridTable.GridColumnStyles[columnIndex];
			column.SetColumnValueAtRow(listManager, rowIndex, value);

			NotifyRenderRequired();
		}
	}

	/// <summary>
	///  Gets or sets the value of a specified <see cref='DataGridCell'/>.
	/// </summary>
	public object this[DataGridCell cell]
	{
		get
		{
			return this[cell.RowNumber, cell.ColumnNumber];
		}
		set
		{
			this[cell.RowNumber, cell.ColumnNumber] = value;
		}
	}

	protected internal DataGridRow[] DataGridRows
	{
		get
		{
			if (dataGridRows == null)
			{
				CreateDataGridRows();
			}
			return dataGridRows;
		}
	}

	// Don't use dataGridRows, use the accessor!!!
	DataGridRow[] dataGridRows = Array.Empty<DataGridRow>();

	public CurrencyManager ListManager
	{
		get
		{
			//try to return something useful:
			if (listManager == null && BindingContext != null && DataSource != null)
			{
				return (CurrencyManager)BindingContext[DataSource, DataMember];
			}
			else
			{
				return listManager;
			}
		}
		set
		{
			throw new NotSupportedException(SR.DataGridSetListManager);
		}
	}
	CurrencyManager listManager;

	/// <summary>
	///  Gets or sets a value
	///  that specifies which links are shown and in what context.
	/// </summary>
	public bool AllowNavigation
	{
		get => gridState[GRIDSTATE_allowNavigation];
		set
		{
			if (AllowNavigation != value)
			{
				gridState[GRIDSTATE_allowNavigation] = value;
				// let the Caption know about this:
				Caption.BackButtonActive = !parentRows.IsEmpty() && (value);
				Caption.BackButtonVisible = Caption.BackButtonActive;
				RecreateDataGridRows();

				OnAllowNavigationChanged(EventArgs.Empty);
			}
		}
	}

	protected virtual void OnAllowNavigationChanged(EventArgs e)
	{
		AllowNavigationChanged?.Invoke(this, e);
	}
	public event EventHandler AllowNavigationChanged;

	/// <summary>
	///  Gets or sets a value indicating whether the grid can be resorted by clicking on
	///  a column header.
	/// </summary>
	public bool AllowSorting
	{
		get => gridState[GRIDSTATE_allowSorting];
		set
		{
			if (AllowSorting != value)
			{
				gridState[GRIDSTATE_allowSorting] = value;
				if (!value && listManager != null)
				{
					IList list = listManager.List;
					if (list is IBindingList)
					{
						((IBindingList)list).RemoveSort();
					}
				}
			}
		}
	}

	/// <summary>
	///  Gets or sets the text of the grid's caption.
	/// </summary>
	public string CaptionText
	{
		get => Caption.Text;
		set => Caption.Text = value;
	}

	/// <summary>
	///  Gets or sets a value that indicates
	///  whether the grid's caption is visible.
	/// </summary>
	public bool CaptionVisible
	{
		get => layout.CaptionVisible;
		set
		{
			if (layout.CaptionVisible != value)
			{
				layout.CaptionVisible = value;
				PerformLayout();
				Invalidate();
				OnCaptionVisibleChanged(EventArgs.Empty);
			}
		}
	}

	protected virtual void OnCaptionVisibleChanged(EventArgs e)
	{
		CaptionVisibleChanged?.Invoke(this, e);
	}
	public event EventHandler CaptionVisibleChanged;

	/// <summary>
	///  Gets or sets the background color of the caption area.
	/// </summary>
	public Color CaptionBackColor
	{
		get => Caption.BackColor;
		set
		{
			if (IsTransparentColor(value))
			{
				throw new ArgumentException(SR.DataGridTransparentCaptionBackColorNotAllowed);
			}

			Caption.BackColor = value;
		}
	}

	/// <summary>
	///  Gets or sets the font of the grid's caption.
	/// </summary>
	public Font CaptionFont
	{
		get => Caption.Font;
		set => Caption.Font = value;
	}

	public Font HeaderFont
	{
		get => (headerFont ?? Font);
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(HeaderFont));
			}

			if (!value.Equals(headerFont))
			{
				headerFont = value;
				RecalculateFonts();
				PerformLayout();
				Invalidate(layout.Inside);
			}
		}
	}
	Font headerFont; // this is ambient property to Font value

	public Color HeaderForeColor { get; set; }

	public Color AlternatingBackColor { get; set; }

	public Color LinkColor { get; set; }

	/// <summary>
	///  Gets or
	///  sets the border style.
	/// </summary>
	public BorderStyle BorderStyle
	{
		get => borderStyle;
		set
		{
			//valid values are 0x0 to 0x2.
			if (!ClientUtils.IsEnumValid(value, (int)value, (int)BorderStyle.None, (int)BorderStyle.Fixed3D))
			{
				throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(BorderStyle));
			}
			if (borderStyle != value)
			{
				borderStyle = value;
				PerformLayout();
				Invalidate();
				OnBorderStyleChanged(EventArgs.Empty);
			}
		}
	}
	BorderStyle borderStyle = defaultBorderStyle;
	const BorderStyle defaultBorderStyle = BorderStyle.Fixed3D;

	void OnBorderStyleChanged(EventArgs e)
	{
		BorderStyleChanged?.Invoke(this, e);
	}
	public event EventHandler BorderStyleChanged;

	/// <summary>
	///  Gets or sets the background color of the grid.
	/// </summary>
	public Color BackgroundColor
	{
		get => backgroundColor;
		set
		{
			if (value.IsEmpty)
			{
				throw new ArgumentException(string.Format(SR.DataGridEmptyColor, "BackgroundColor"));
			}

			if (UpdateProperty(ref backgroundColor, value))
			{
				OnBackgroundColorChanged(EventArgs.Empty);
			}
		}
	}
	Color backgroundColor = defaultBackgroundColor;
	static readonly Color defaultBackgroundColor = SystemColors.AppWorkspace;

	protected virtual void OnBackgroundColorChanged(EventArgs e)
	{
		BackgroundColorChanged?.Invoke(this, e);
	}
	public event EventHandler BackgroundColorChanged;

	/// <summary>
	///  Gets or
	///  sets the background color of all row and column headers.
	/// </summary>
	public Color HeaderBackColor
	{
		get => headerBackColor;
		set
		{
			if (value.IsEmpty)
			{
				throw new ArgumentException(string.Format(SR.DataGridEmptyColor, "HeaderBackColor"));
			}

			if (IsTransparentColor(value))
			{
				throw new ArgumentException(SR.DataGridTransparentHeaderBackColorNotAllowed);
			}

			UpdateProperty(ref headerBackColor, value);
		}
	}
	Color headerBackColor = defaultHeaderBackColor;
	static readonly Color defaultHeaderBackColor = SystemColors.Control;

	/// <summary>
	///  Gets or sets the color of the grid lines.
	/// </summary>
	public Color GridLineColor
	{
		get => gridLineColor;
		set
		{
			if (value.IsEmpty)
			{
				throw new ArgumentException(string.Format(SR.DataGridEmptyColor, "GridLineColor"));
			}

			UpdateProperty(ref gridLineColor, value);
		}
	}
	Color gridLineColor = defaultGridLineColor;
	static Color defaultGridLineColor => SystemColors.Control;

	/// <summary>
	///  Gets or sets a value indicating whether the grid displays in flat mode.
	/// </summary>
	public bool FlatMode
	{
		get => gridState[GRIDSTATE_isFlatMode];
		set
		{
			if (value != FlatMode)
			{
				gridState[GRIDSTATE_isFlatMode] = value;
				OnFlatModeChanged(EventArgs.Empty);
			}
		}
	}

	protected virtual void OnFlatModeChanged(EventArgs e)
	{
		FlatModeChanged?.Invoke(this, e);
	}
	public event EventHandler FlatModeChanged;

	/// <summary>
	///  Gets or sets a value indicating whether the parent rows of a table are
	///  visible.
	/// </summary>
	public bool ParentRowsVisible
	{
		get => layout.ParentRowsVisible;
		set
		{
			if (layout.ParentRowsVisible != value)
			{
				SetParentRowsVisibility(value);

				// update the caption: parentDownVisible == false corresponds to DownButtonDown == true;
				//
				caption.SetDownButtonDirection(!value);

				OnParentRowsVisibleChanged(EventArgs.Empty);
			}
		}
	}

	///  Scrolls the data area down to make room for the parent rows
	///  and lays out the different regions of the DataGrid.
	/// </summary>
	internal void SetParentRowsVisibility(bool visible)
	{
		// hide the Edit Box
		EndEdit();

		layout.ParentRowsVisible = visible;
		NotifyRenderRequired();
	}

	protected virtual void OnParentRowsVisibleChanged(EventArgs e)
	{
		ParentRowsVisibleChanged?.Invoke(this, e);
	}
	public event EventHandler ParentRowsVisibleChanged;

	/// <summary>
	///  Gets or sets the background color of parent rows.
	/// </summary>
	public Color ParentRowsBackColor
	{
		get => parentRows.BackColor;
		set
		{
			if (IsTransparentColor(value))
			{
				throw new ArgumentException(SR.DataGridTransparentParentRowsBackColorNotAllowed);
			}

			parentRows.BackColor = value;
		}
	}

	protected virtual Color DragMaskColor => default;

	protected virtual bool GridLayoutConfigurable => true;

	protected NotificationIcon NotificationIcon { get; set; }

	protected ErrorProvider ErrorProvider { get; set; }

	public void SetErrorProvider(ErrorProvider errorProvider)
	{
		ErrorProvider = errorProvider;
	}

	/// <summary>
	///  Gets or sets the foreground color of parent rows.
	/// </summary>
	public Color ParentRowsForeColor
	{
		get => parentRows.ForeColor;
		set => parentRows.ForeColor = value;
	}

	/// <summary>
	///  Gets
	///  or sets a value indicating if the grid's column headers are visible.
	/// </summary>
	public bool ColumnHeadersVisible
	{
		get => gridState[GRIDSTATE_columnHeadersVisible];
		set
		{
			if (ColumnHeadersVisible != value)
			{
				gridState[GRIDSTATE_columnHeadersVisible] = value;
				layout.ColumnHeadersVisible = value;
				PerformLayout();
			}
		}
	}

	/// <summary>
	///  Gets or sets a value indicating whether the data grid's row headers are
	///  visible.
	/// </summary>
	public bool RowHeadersVisible
	{
		get => gridState[GRIDSTATE_rowHeadersVisible];
		set
		{
			if (RowHeadersVisible != value)
			{
				gridState[GRIDSTATE_rowHeadersVisible] = value;
				PerformLayout();
				NotifyRenderRequired();
			}
		}
	}

	int HeaderWidth => myGridTable.IsDefault ? RowHeaderWidth : myGridTable.RowHeaderWidth;

	public int RowHeaderWidth
	{
		get => rowHeaderWidth;
		set
		{
			value = Math.Max(minRowHeaderWidth, value);
			if (UpdateProperty(ref rowHeaderWidth, value))
			{
				rowHeaderWidth = value;
				if (layout.RowHeadersVisible)
				{
					PerformLayout();
				}
			}
		}
	}
	int rowHeaderWidth = defaultRowHeaderWidth;
	const int defaultRowHeaderWidth = 35;

	internal int MinimumRowHeaderWidth() => minRowHeaderWidth;
	int minRowHeaderWidth;

	internal void ComputeMinimumRowHeaderWidth()
	{
		minRowHeaderWidth = errorRowBitmapWidth; // the size of the pencil, star and row selector images are the same as the image for the error bitmap
		if (ListHasErrors)
		{
			minRowHeaderWidth += errorRowBitmapWidth;
		}

		if (myGridTable != null && myGridTable.RelationsList.Count != 0)
		{
			minRowHeaderWidth += 15; // the size of the plus/minus glyph and spacing around it
		}
	}

	const int errorRowBitmapWidth = 15;

	public int PreferredRowHeight
	{
		get => preferredRowHeight;
		set
		{
			if (value < 0)
			{
				throw new ArgumentException(SR.DataGridRowRowHeight);
			}

			preferredRowHeight = value;
		}
	}
	static readonly int defaultFontHeight = Control.DefaultFontHeight;
	int preferredRowHeight = defaultFontHeight + 3;

	/// <summary>
	///  Gets
	///  or sets the default width of the grid columns in
	///  pixels.
	/// </summary>
	public int PreferredColumnWidth
	{
		get => preferredColumnWidth;
		set
		{
			if (value < 0)
			{
				throw new ArgumentException(SR.DataGridColumnWidth, "PreferredColumnWidth");
			}

			if (preferredColumnWidth != value)
			{
				preferredColumnWidth = value;
			}
		}
	}
	int preferredColumnWidth = defaultPreferredColumnWidth;
	const int defaultPreferredColumnWidth = 75;

	/// <summary>
	///  Gets the collection of tables for the grid.
	/// </summary>
	public GridTableStylesCollection TableStyles => dataGridTables;
	internal GridTableStylesCollection dataGridTables;

	protected ScrollBar HorizScrollBar => horizScrollBar;
	readonly ScrollBar horizScrollBar = new ScrollBar();

	protected ScrollBar VertScrollBar => vertScrollBar;
	readonly ScrollBar vertScrollBar = new ScrollBar();

	public int FirstVisibleColumn
	{
		get
		{
			return firstVisibleCol;
		}
		set
		{
			UpdateProperty(ref firstVisibleCol, value);
		}
	}

	public int VisibleColumnCount => numVisibleCols;
	int numVisibleCols;

	public int VisibleRowCount => visibleRowCount;
	int visibleRowCount;

	/// <summary>
	///  Will return the string that will be used as a delimiter between columns
	///  when copying rows contents to the Clipboard.
	///  At the moment, return "\t"
	/// </summary>
	protected virtual string GetOutputTextDelimiter() => "\t";

	internal int HorizontalOffset
	{
		get => currentScrollLeft;
		set
		{
			if (value < 0)
			{
				value = 0;
			}

			var totalWidth = GetColumnWidthSum() + HeaderWidth;
			var widthNotVisible = totalWidth - Width;
			if (value > widthNotVisible && widthNotVisible > 0)
			{
				value = widthNotVisible;
			}

			if (value == currentScrollLeft)
			{
				return;
			}

			horizScrollBar.Value = value;
			currentScrollLeft = value;

			firstVisibleCol = ComputeFirstVisibleColumn();
			ComputeVisibleColumns();

			if (gridState[GRIDSTATE_isScrolling])
			{
				// if the user did not click on the grid yet, then do not put the edit
				// control when scrolling
				if (currentCol >= firstVisibleCol && currentCol < firstVisibleCol + numVisibleCols - 1 && (gridState[GRIDSTATE_isEditing] || gridState[GRIDSTATE_isNavigating]))
				{
					Edit();
				}
				else
				{
					EndEdit();
				}

				// isScrolling is set to TRUE when the user scrolls.
				// once we move the edit box, we finished processing the scroll event, so set isScrolling to FALSE
				// to set isScrolling to TRUE, we need another scroll event.
				gridState[GRIDSTATE_isScrolling] = false;
			}
			else
			{
				EndEdit();
			}

			RegisterAfterRenderAction(async () => await (GetJSInterop<IGridJSInterop>()?.SetScrollLeftAsync(ElementReference, currentScrollLeft) ?? Task.CompletedTask));
		}
	}

	/// <summary>
	///  Selects a given row
	/// </summary>
	public void Select(int row)
	{
		Debug.WriteLineIf(CompModSwitches.DataGridSelection.TraceVerbose, "Selecting row " + row.ToString(CultureInfo.InvariantCulture));
		if (DataGridRows == null)
		{
			throw new NullReferenceException();
		}

		if(row < 0 || row >= DataGridRowsLength)
		{
			throw new ArgumentOutOfRangeException(nameof(row));
		}

		DataGridRow[] localGridRows = DataGridRows;
		if (!localGridRows[row].Selected)
		{
			localGridRows[row].Selected = true;
			numSelectedRows++;
		}

		// when selecting a row, hide the edit box
		EndEdit();
	}

	/// <summary>
	///  Gets a value indicating whether a
	///  specified row is selected.
	/// </summary>
	public bool IsSelected(int row)
	{
		if (DataGridRows == null || DataGridRows.Length <= row)
		{
			return false;
		}

		DataGridRow[] localGridRows = DataGridRows;
		return localGridRows[row].Selected;
	}

	/// <summary>
	///  Unselects a given row
	/// </summary>
	public void UnSelect(int row)
	{
		Debug.WriteLineIf(CompModSwitches.DataGridSelection.TraceVerbose, "DataGridSelection: Unselecting row " + row.ToString(CultureInfo.InvariantCulture));
		if (DataGridRows != null && DataGridRows.Length > row)
		{
			DataGridRow[] localGridRows = DataGridRows;
			if (localGridRows[row].Selected)
			{
				localGridRows[row].Selected = false;
				numSelectedRows--;
			}
		}
	}

	/// <summary>
	///  Asks the cursor to update.
	/// </summary>
	void UpdateListManager()
	{
		Debug.WriteLineIf(CompModSwitches.DataGridCursor.TraceVerbose, "DataGridCursor: Requesting EndEdit()");
		try
		{
			if (listManager != null)
			{
				EndEdit();
				listManager.EndCurrentEdit();
			}
		}
		catch
		{
		}
	}

	/// <summary>
	///  Turns off selection for all rows that are selected.
	/// </summary>
	protected void ResetSelection()
	{
		using var activity = TelemetryService.ActivitySource.StartActivity($"{GetType().Name}.{nameof(ResetSelection)}");
		if (numSelectedRows > 0 && dataGridRows != null && dataGridRows.Length > 0)
		{
			var localGridRows = dataGridRows;
			for (int i = 0; i < localGridRows.Length; ++i)
			{
				if (localGridRows[i].Selected)
				{
					localGridRows[i].Selected = false;
				}
			}
		}
		numSelectedRows = 0;
		lastRowSelected = -1;
	}
	int numSelectedRows;
	int lastRowSelected = -1;

	/// <summary>
	///  Attempts to
	///  put the grid into a state where editing is
	///  allowed.
	/// </summary>
	public bool BeginEdit(DataGridColumnStyle gridColumn, int rowNumber)
	{
		if (DataSource == null || myGridTable == null)
		{
			return false;
		}

		// We deny edit requests if we are already editing a cell.
		if (gridState[GRIDSTATE_isEditing])
		{
			return false;
		}
		else
		{
			int col = -1;
			if ((col = myGridTable.GridColumnStyles.IndexOf(gridColumn)) < 0)
			{
				return false;
			}

			CurrentCell = new DataGridCell(rowNumber, col);
			ResetSelection();
			Edit();
			return true;
		}
	}

	/// <summary>
	///  Begin in-place editing of a cell.  Any editing is commited
	///  before the new edit takes place.
	///
	///  This will always edit the currentCell
	///  If you want to edit another cell than the current one, just reset CurrentCell
	/// </summary>
	void Edit()
	{
		Edit(null);
	}

	void Edit(string displayText)
	{
		using var activity = TelemetryService.ActivitySource.StartActivity($"{GetType().Name}.{nameof(Edit)}");
		EnsureBound();

		bool cellIsVisible = true;

		EndEdit();

		Debug.WriteLineIf(CompModSwitches.DataGridEditing.TraceVerbose, "DataGridEditing: Edit, currentRow = " + currentRow.ToString(CultureInfo.InvariantCulture) +
																	   ", currentCol = " + currentCol.ToString(CultureInfo.InvariantCulture) + (displayText != null ? ", displayText= " + displayText : ""));

		DataGridRow[] localGridRows = DataGridRows;

		// what do you want to edit when there are no rows?
		if (DataGridRowsLength == 0)
		{
			return;
		}

		localGridRows[currentRow].OnEdit();
		editRow = localGridRows[currentRow];

		// if the list has no columns, then what good is an edit?
		if (myGridTable.GridColumnStyles.Count == 0)
		{
			return;
		}

		// what if the currentCol does not have a propDesc?
		editColumn = myGridTable.GridColumnStyles[currentCol];
		if (editColumn.PropertyDescriptor == null)
		{
			return;
		}

		var cellBounds = GetCellBounds(currentRow, currentCol);

		gridState[GRIDSTATE_isNavigating] = true;
		gridState[GRIDSTATE_isEditing] = false;

		// once we call editColumn.Edit on a DataGridTextBoxColumn
		// the edit control will become visible, and its bounds will get set.
		// both actions cause Edit.Parent.OnLayout
		// so we flag this change, cause we don't want to PerformLayout on the entire DataGrid
		// everytime the edit column gets edited
		gridState[GRIDSTATE_editControlChanging] = true;

		editColumn.Edit(ListManager,
						  currentRow,
						  cellBounds,
						  myGridTable.ReadOnly || ReadOnly || !policy.AllowEdit,
						  displayText,
						  cellIsVisible);

		// reset the gridState[GRIDSTATE_editControlChanging] to false
		gridState[GRIDSTATE_editControlChanging] = false;
	}
	DataGridColumnStyle editColumn;
	DataGridRow editRow;

	/// <summary>
	///  Ends any editing in progress by attempting to commit and then
	///  aborting if not possible.
	/// </summary>
	void EndEdit()
	{
		using var activity = TelemetryService.ActivitySource.StartActivity($"{GetType().Name}.{nameof(EndEdit)}");
		Debug.WriteLineIf(CompModSwitches.DataGridEditing.TraceVerbose, "DataGridEditing: EndEdit");

		if ((!gridState[GRIDSTATE_isEditing] && !gridState[GRIDSTATE_isNavigating]))
		{
			return;
		}

		if (!CommitEdit())
		{
			AbortEdit();
		}
	}

	/// <summary>
	///  Attempts to commit editing if a cell is being edited.
	///  Return true if successfully commited editing.
	///  Return false if editing can not be completed and the gird must
	///  remain in our current Edit state.
	/// </summary>
	bool CommitEdit()
	{
		using var activity = TelemetryService.ActivitySource.StartActivity($"{GetType().Name}.{nameof(CommitEdit)}");
		Debug.WriteLineIf(CompModSwitches.DataGridEditing.TraceVerbose, "DataGridEditing: \t  CommitEdit " + (editRow == null ? "" : editRow.RowNumber.ToString(CultureInfo.InvariantCulture)));

		// we want to commit the editing if
		// 1. the user was editing or navigating around the data grid and
		// 2. this is not the result of moving focus inside the data grid and
		// 3. if the user was scrolling
		if (!gridState[GRIDSTATE_isEditing] && !gridState[GRIDSTATE_isNavigating] || (gridState[GRIDSTATE_editControlChanging] && !gridState[GRIDSTATE_isScrolling]))
		{
			return true;
		}

		// the same rules from editColumn.OnEdit
		// flag that we are editing the Edit control, so if we get a OnLayout on the
		// datagrid side of things while the edit control changes its visibility and bounds
		// the datagrid does not perform a layout
		gridState[GRIDSTATE_editControlChanging] = true;

		if ((editColumn != null && editColumn.ReadOnly) || gridState[GRIDSTATE_inAddNewRow])
		{
			bool focusTheGrid = false;
			if (ContainsFocus)
			{
				focusTheGrid = true;
			}

			if (focusTheGrid && gridState[GRIDSTATE_canFocus])
			{
				Focus();
			}

			editColumn.ConcedeFocus();

			// set the focus back to the grid
			if (focusTheGrid && gridState[GRIDSTATE_canFocus] && CanFocus && !Focused)
			{
				Focus();
			}

			// reset the editControl flag
			gridState[GRIDSTATE_editControlChanging] = false;
			return true;
		}

		bool retVal = editColumn?.Commit(ListManager, currentRow) ?? true;

		// reset the editControl flag
		gridState[GRIDSTATE_editControlChanging] = false;

		if (retVal)
		{
			gridState[GRIDSTATE_isEditing] = false;
		}

		return retVal;
	}

	void AbortEdit()
	{
		Debug.WriteLineIf(CompModSwitches.DataGridEditing.TraceVerbose, "DataGridEditing: \t! AbortEdit");

		// the same rules from editColumn.OnEdit
		// while changing the editControl's visibility, do not
		// PerformLayout on the entire DataGrid
		gridState[GRIDSTATE_editControlChanging] = true;

		editColumn.Abort(editRow.RowNumber);

		// reset the editControl flag:
		gridState[GRIDSTATE_editControlChanging] = false;

		gridState[GRIDSTATE_isEditing] = false;
		editRow = null;
		editColumn = null;
	}

	internal void AddNewRow()
	{
		using var activity = TelemetryService.ActivitySource.StartActivity($"{GetType().Name}.Set{nameof(AddNewRow)}");
		EnsureBound();
		ResetSelection();
		UpdateListManager();
		gridState[GRIDSTATE_inListAddNew] = true;
		gridState[GRIDSTATE_inAddNewRow] = true;
		try
		{
			ListManager.AddNew();
		}
		catch
		{
			gridState[GRIDSTATE_inListAddNew] = false;
			gridState[GRIDSTATE_inAddNewRow] = false;
			PerformLayout();
			InvalidateInside();
			throw;
		}
		gridState[GRIDSTATE_inListAddNew] = false;
	}

	protected override void OnBindingContextChanged(EventArgs e)
	{
		if (DataSource != null && !gridState[GRIDSTATE_inSetListManager])
		{
			try
			{
				Set_ListManager(DataSource, DataMember, true, false);     // we do not want to create columns
																		  // if the columns are already created
																		  // the grid should not rely on OnBindingContextChanged
																		  // to create columns.
			}
			catch
			{
				// at runtime we will rethrow the exception
				if (Site == null || !Site.DesignMode)
				{
					throw;
				}

				MessageBox.Show(null, SR.DataGridExceptionInPaint, null,
					MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);

				ResetParentRows();

				Set_ListManager(null, string.Empty, true);
			}
		}

		base.OnBindingContextChanged(e);
	}

	/// <summary>
	///  Given a cursor, this will Create the right DataGridRows
	/// </summary>
#if DEBUG
	internal virtual
#endif
	void CreateDataGridRows()
	{
		CurrencyManager listManager = ListManager;
		DataGridTableStyle dgt = myGridTable;
		InitializeColumnWidths();

		if (listManager == null)
		{
			SetDataGridRows(Array.Empty<DataGridRow>(), 0);
			return;
		}

		int nDataGridRows = listManager.Count;
		if (policy.AllowAdd)
		{
			nDataGridRows++;
		}

		DataGridRow[] rows = new DataGridRow[nDataGridRows];
		for (int r = 0; r < listManager.Count; r++)
		{
			rows[r] = new DataGridRelationshipRow(this, dgt, r);
		}

		if (policy.AllowAdd)
		{
			addNewRow = new DataGridAddNewRow(this, dgt, nDataGridRows - 1);
			rows[nDataGridRows - 1] = addNewRow;
		}
		else
		{
			addNewRow = null;
		}
		SetDataGridRows(rows, nDataGridRows);
	}
	DataGridAddNewRow addNewRow;

#if DEBUG
	internal
#endif
	void RecreateDataGridRows()
	{
		// since some BusinessObjects will RefreshDatasource on Get, causing ColumnsToRender in DataGridRows to be empty,
		// we need to block RecreateDataGridRows during OnbeforeRender
		if (!DuringBeforeRender)
		{
			int nDataGridRows = 0;
			CurrencyManager listManager = ListManager;

			if (listManager != null)
			{
				nDataGridRows = listManager.Count;
				if (policy.AllowAdd)
				{
					nDataGridRows++;
				}
			}
			SetDataGridRows(null, nDataGridRows);
		}
	}

	/// <summary>
	///  Sets the array of DataGridRow objects used for
	///  all row-related logic in the DataGrid.
	/// </summary>
	internal void SetDataGridRows(DataGridRow[] newRows, int newRowsLength)
	{
		using var activity = TelemetryService.ActivitySource.StartActivity($"{GetType().Name}.{nameof(SetDataGridRows)}");

		var oldDataGridRows = dataGridRows;
		dataGridRows = newRows;
		dataGridRowsLength = newRowsLength;

		ResetUIState();

		OnAfterSetDataGridRows();

		if (oldDataGridRows != dataGridRows)
		{
			var rowsToDispose = oldDataGridRows?.Except(dataGridRows ?? Array.Empty<DataGridRow>()).ToArray();
			DisposeDataGridRows(rowsToDispose);
		}
	}
	void DisposeDataGridRows(DataGridRow[] rows)
	{
		if (rows == null)
		{
			return;
		}

		for (var i = 0; i < rows.Length; i++)
		{
			var row = rows[i];
			row.Dispose();
		}
	}

	protected internal override void OnBeforeRender()
	{
		DuringBeforeRender = true;
		using var activity = TelemetryService.ActivitySource.StartActivity($"{GetType().Name}.{nameof(OnBeforeRender)}");
		base.OnBeforeRender();

		var first = FirstVisibleRow;
		var visibleRowsCount = VisibleRowCount;
		var localDataGridRows = DataGridRows;
		var rowsLength = DataGridRowsLength;
		for (var i = first; i < first + visibleRowsCount && i < rowsLength; i++)
		{
			var dataGridRow = localDataGridRows[i];
			if (!dataGridRow.RenderedContentCreated)
			{
				dataGridRow.CreateRenderedContent();
			}
		}
		dataGridViewModel = new DataGridViewModel(GetDataGridColumnViewModels(), localDataGridRows);
		DuringBeforeRender = false;
	}

	protected virtual void OnAfterSetDataGridRows()
	{
	}

	protected virtual void OnAfterSetVisibleRowCount()
	{
	}

	protected int[] VisibleRowDataRowNumbers
	{
		get
		{
			var localRows = DataGridRows;
			var result = new int[VisibleRowCount];

			for (var i = 0; i < result.Length && i + FirstVisibleRow < localRows.Length; i++)
			{
				result[i] = localRows[i + FirstVisibleRow].RowNumber;
			}

			return result;
		}
	}

	DataGridColumnViewModel[] GetDataGridColumnViewModels()
	{
		// Adapted from PaintColumnHeaderText
		var columnViewModels = new List<DataGridColumnViewModel>();
		var localColumns = myGridTable.GridColumnStyles;

		// This method for getting sort properties is different from WinForms
		// Instead it is the CW1 logic from ZGrid PaintColumnHeaderBordersAndSortTriangles
		var sortProperties = new Dictionary<PropertyDescriptor, ListSortDirection>();
		var bindingList = ListManager != null ? ListManager.List as IBindingList : null;
		if (bindingList is IBindingListView bindingListView && bindingListView.SupportsAdvancedSorting && bindingListView.Count > 0)
		{
			sortProperties = bindingListView.SortDescriptions.Cast<ListSortDescription>().ToDictionary(item => item.PropertyDescriptor, item => item.SortDirection);
		}
		else if (bindingList != null && bindingList.SupportsSorting && bindingList.SortProperty != null)
		{
			sortProperties.Add(bindingList.SortProperty, bindingList.SortDirection);
		}

		for (var i = 0; i < localColumns.Count; i++)
		{
			var column = localColumns[i];

			if (column.PropertyDescriptor == null)
			{
				continue;
			}

			var sortDirection = sortProperties.TryGetValue(column.PropertyDescriptor, out var value) ? (ListSortDirection?)value : null;
			columnViewModels.Add(new DataGridColumnViewModel(column, i, column.HeaderText, sortDirection));
		}
		return columnViewModels.ToArray();
	}

	internal int DataGridRowsLength => dataGridRowsLength;
	int dataGridRowsLength;

	DataGridViewModel dataGridViewModel = new DataGridViewModel(Array.Empty<DataGridColumnViewModel>(), Array.Empty<DataGridRow>());

	/// <summary>
	///  Raises the <see cref='Control.Layout'/> event which
	///  repositions controls
	///  and updates scroll bars.
	/// </summary>
	protected override void OnLayout(LayoutEventArgs levent)
	{
		// if we get a OnLayout event while the editControl changes, then just ignore it
		if (gridState[GRIDSTATE_editControlChanging])
		{
			return;
		}

		Debug.WriteLineIf(CompModSwitches.DataGridLayout.TraceVerbose, "DataGridLayout: OnLayout");
		base.OnLayout(levent);

		if (gridState[GRIDSTATE_layoutSuspended])
		{
			return;
		}

		gridState[GRIDSTATE_canFocus] = false;
		try
		{
			if (IsHandleCreated)
			{
				if (layout.ParentRowsVisible)
				{
					parentRows.OnLayout();
				}

				ComputeLayout();
			}
		}
		finally
		{
			gridState[GRIDSTATE_canFocus] = true;
		}
	}

	/// <summary>
	///  Raises the <see cref='Control.Paint'/>
	///  event.
	/// </summary>
	protected override void OnPaint(PaintEventArgs pe)
	{
		try
		{
			if (layout.dirty)
			{
				ComputeLayout();
			}

			base.OnPaint(pe); // raise paint event
		}
		catch
		{
			// at runtime we will rethrow the exception
			if (Site == null || !Site.DesignMode)
			{
				throw;
			}

			gridState[GRIDSTATE_exceptionInPaint] = true;
			try
			{
				ResetParentRows();
				Set_ListManager(null, string.Empty, true);
			}
			finally
			{
				gridState[GRIDSTATE_exceptionInPaint] = false;
			}
		}
	}

	/// <summary>
	///  Requests an end to an edit operation taking place on the
	///  <see cref='DataGrid'/>
	///  control.
	/// </summary>
	public bool EndEdit(DataGridColumnStyle gridColumn, int rowNumber, bool shouldAbort)
	{
		bool ret = false;
		if (gridState[GRIDSTATE_isEditing])
		{
			if (gridColumn != editColumn)
			{
				Debug.WriteLineIf(CompModSwitches.DataGridEditing.TraceVerbose, "DataGridEditing: EndEdit requested on a column we are not editing.");
			}
			if (rowNumber != editRow.RowNumber)
			{
				Debug.WriteLineIf(CompModSwitches.DataGridEditing.TraceVerbose, "DataGridEditing: EndEdit requested on a row we are not editing.");
			}
			if (shouldAbort)
			{
				AbortEdit();
				ret = true;
			}
			else
			{
				ret = CommitEdit();
			}
		}
		return ret;
	}

	internal Control FindEditControl() => Controls.FirstOrDefault(c => c.ShouldRender);

	public HitTestInfo HitTest(int x, int y) => HitTest(new Point(x, y));

	public HitTestInfo HitTest(Point position) => currentHitTest ?? new HitTestInfo() { type = HitTestType.None, col = -1, row = -1 };

	protected internal HitTestInfo SetCurrentHitTestInfo(int rowCurrent, int colCurrent = -1 , HitTestType hitTestType = HitTestType.None) => currentHitTest = new HitTestInfo() { row = rowCurrent, col = colCurrent, type = hitTestType };

	public Rectangle GetCellBounds(int row, int col)
	{
		var localGridRows = DataGridRows;
		return localGridRows[row].GetCellBounds(col);
	}

	public Rectangle GetCellBounds(DataGridCell dgc) => GetCellBounds(dgc.RowNumber, dgc.ColumnNumber);

	/// <summary>
	///  Gets or sets index of the selected row.
	/// </summary>
	// will set the position in the ListManager
	public int CurrentRowIndex
	{
		get
		{
			if (originalState == null)
			{
				return listManager == null ? -1 : listManager.Position;
			}
			else
			{
				if (BindingContext == null)
				{
					return -1;
				}

				CurrencyManager originalListManager = (CurrencyManager)BindingContext[originalState.DataSource, originalState.DataMember];
				return originalListManager.Position;
			}
		}
		set
		{
			if (listManager == null)
			{
				throw new InvalidOperationException(SR.DataGridSetSelectIndex);
			}

			if (originalState == null)
			{
				listManager.Position = value;
				currentRow = value;
				return;
			}

			// if we have a this.ListManager, then this.BindingManager cannot be null
			CurrencyManager originalListManager = (CurrencyManager)BindingContext[originalState.DataSource, originalState.DataMember];
			originalListManager.Position = value;

			// this is for parent rows
			originalState.LinkingRow = originalState.DataGridRows[value];

			// Invalidate everything
			Invalidate();
		}
	}

	/// <summary>
	///  Gets or sets which cell has the focus. Not available at design time.
	/// </summary>
	public DataGridCell CurrentCell
	{
		get
		{
			return new DataGridCell(currentRow, currentCol);
		}
		set
		{
			using var activity = TelemetryService.ActivitySource.StartActivity($"{GetType().Name}.Set{nameof(CurrentCell)}");
			// if the OnLayout event was not set in the grid, then we can't
			// reliably set the currentCell on the grid.
			if (layout.dirty)
			{
				throw new ArgumentException(SR.DataGridSettingCurrentCellNotGood);
			}

			if (value.RowNumber == currentRow && value.ColumnNumber == currentCol)
			{
				return;
			}

			// should we throw an exception, maybe?
			if (DataGridRowsLength == 0 || myGridTable.GridColumnStyles == null || myGridTable.GridColumnStyles.Count == 0)
			{
				return;
			}

			EnsureBound();

			int currentRowSaved = currentRow;
			int currentColSaved = currentCol;
			bool wasEditing = gridState[GRIDSTATE_isEditing];
			bool cellChanged = false;

			// if the position in the listManager changed under the DataGrid,
			// then do not edit after setting the current cell
			bool doNotEdit = false;

			int newCol = value.ColumnNumber;
			int newRow = value.RowNumber;

			string errorMessage = null;

			int localGridRowsLength = DataGridRowsLength;

			try
			{
				int columnCount = myGridTable.GridColumnStyles.Count;
				if (newCol < 0)
				{
					newCol = 0;
				}

				if (newCol >= columnCount)
				{
					newCol = columnCount - 1;
				}

				DataGridRow[] localGridRows = DataGridRows;

				if (newRow < 0)
				{
					newRow = 0;
				}
				if (newRow >= localGridRowsLength)
				{
					newRow = localGridRowsLength - 1;
				}

				// Current Column changing
				if (currentCol != newCol)
				{
					cellChanged = true;
					int currentListManagerPosition = ListManager.Position;
					int currentListManagerCount = ListManager.List.Count;

					EndEdit();

					if (ListManager.Position != currentListManagerPosition ||
						currentListManagerCount != ListManager.List.Count)
					{
						// EndEdit changed the list.
						// Reset the data grid rows and the current row inside the datagrid.
						// And then exit the method.
						RecreateDataGridRows();
						if (ListManager.List.Count > 0)
						{
							currentRow = ListManager.Position;
							Edit();
						}
						else
						{
							currentRow = -1;
						}

						return;
					}

					currentCol = newCol;
					InvalidateRow(currentRow);
				}

				// Current Row changing
				if (currentRow != newRow)
				{
					cellChanged = true;
					int currentListManagerPosition = ListManager.Position;
					int currentListManagerCount = ListManager.List.Count;

					EndEdit();

					if (ListManager.Position != currentListManagerPosition ||
						currentListManagerCount != ListManager.List.Count)
					{
						// EndEdit changed the list.
						// Reset the data grid rows and the current row inside the datagrid.
						// And then exit the method.
						RecreateDataGridRows();
						if (ListManager.List.Count > 0)
						{
							currentRow = ListManager.Position;
							Edit();
						}
						else
						{
							currentRow = -1;
						}

						return;
					}

					if (currentRow < localGridRowsLength)
					{
						localGridRows[currentRow].OnRowLeave();
					}

					localGridRows[newRow].OnRowEnter();
					currentRow = newRow;
					if (currentRowSaved < localGridRowsLength)
					{
						InvalidateRow(currentRowSaved);
					}

					InvalidateRow(currentRow);

					if (currentRowSaved != listManager.Position)
					{
						doNotEdit = true;
						if (gridState[GRIDSTATE_isEditing])
						{
							AbortEdit();
						}
					}
					else if (gridState[GRIDSTATE_inAddNewRow])
					{
#if DEBUG
						int currentRowCount = DataGridRowsLength;
#endif
						// cancelCurrentEdit will change the position in the list
						// to the last element in the list. and the grid will get an on position changed
						// event, and will set the current cell to the last element in the dataSource.
						// so unhook the PositionChanged event from the listManager;
						ListManager.PositionChanged -= positionChangedHandler;
						ListManager.CancelCurrentEdit();
						ListManager.Position = currentRow;
						ListManager.PositionChanged += positionChangedHandler;

						localGridRows = DataGridRows;
						localGridRows[DataGridRowsLength - 1] = new DataGridAddNewRow(this, myGridTable, DataGridRowsLength - 1);
						SetDataGridRows(localGridRows, DataGridRowsLength);
						gridState[GRIDSTATE_inAddNewRow] = false;
					}
					else
					{
						ListManager.EndCurrentEdit();
						// some special care must be given when setting the
						// position in the listManager.
						// if EndCurrentEdit() deleted the current row
						// ( because of some filtering problem, say )
						// then we cannot go over the last row
						if (localGridRowsLength != DataGridRowsLength)
						{
							currentRow = (currentRow == localGridRowsLength - 1) ? DataGridRowsLength - 1 : currentRow;
						}

						if (currentRow == dataGridRowsLength - 1 && policy.AllowAdd)
						{
							AddNewRow();
						}
						else
						{
							ListManager.Position = currentRow;
						}
					}
				}
			}
			catch (Exception e)
			{
				errorMessage = e.Message;
			}

			if (errorMessage != null)
			{
				DialogResult result = MessageBox.Show(null,
					string.Format(SR.DataGridPushedIncorrectValueIntoColumn, errorMessage),
					SR.DataGridErrorMessageBoxCaption, MessageBoxButtons.YesNo,
					MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);

				if (result == DialogResult.Yes)
				{
					currentRow = currentRowSaved;
					currentCol = currentColSaved;
					// this will make sure the newRow will not paint the row selector.
					InvalidateRowHeader(newRow);
					// also, make sure that we get the row selector on the currentrow, too
					InvalidateRowHeader(currentRow);
					if (wasEditing)
					{
						Edit();
					}
				}
				else
				{
					// if the user committed a row that used to be addNewRow and the backEnd rejects it,
					// and then it tries to navigate down then we should stay in the addNewRow
					// in this particular scenario, CancelCurrentEdit will cause the last row to be deleted,
					// and this will ultimately call InvalidateRow w/ a row number larger than the number of rows
					// so set the currentRow here:
					if (currentRow == DataGridRowsLength - 1 && currentRowSaved == DataGridRowsLength - 2 && DataGridRows[currentRow] is DataGridAddNewRow)
					{
						newRow = currentRowSaved;
					}

					currentRow = newRow;
					listManager.PositionChanged -= positionChangedHandler;
					listManager.CancelCurrentEdit();
					listManager.Position = newRow;
					listManager.PositionChanged += positionChangedHandler;
					currentRow = newRow;
					currentCol = newCol;
					if (wasEditing)
					{
						Edit();
					}
				}
			}

			if (cellChanged)
			{
				EnsureVisible(currentRow, currentCol);
				OnCurrentCellChanged(EventArgs.Empty);

				// if the user changed the current cell using the UI, edit the new cell
				// but if the user changed the current cell by changing the position in the
				// listManager, then do not continue the edit
				if (!doNotEdit)
				{
					Edit();
				}
			}
		}
	}

	internal int currentRow;
	internal int currentCol;

	/// <summary>
	///  Gets a <see cref='T:System.Drawing.Rectangle'/>
	///  that specifies the four corners of the selected cell.
	/// </summary>
	public Rectangle GetCurrentCellBounds()
	{
		var current = CurrentCell;
		return GetCellBounds(current.RowNumber, current.ColumnNumber);
	}

	protected virtual void OnCurrentCellChanged(EventArgs e)
	{
		using var activity = TelemetryService.ActivitySource.StartActivity($"{GetType().Name}.Set{nameof(OnCurrentCellChanged)}");
		CurrentCellChanged?.Invoke(this, e);
	}
	public event EventHandler CurrentCellChanged;

	readonly ItemChangedEventHandler itemChangedHandler;
	readonly EventHandler positionChangedHandler;
	readonly EventHandler currentChangedHandler;
	readonly EventHandler metaDataChangedHandler;

	// will be used by the columns to tell the grid that editing has
	// taken place (ie, the grid is no longer in the editOrNavigateMode)
	// also, tell the current row to lose child focus
	internal protected virtual void ColumnStartedEditing(Rectangle bounds)
	{
		var localGridRows = DataGridRows;

		if (bounds.IsEmpty && editColumn is DataGridTextBoxColumn && currentRow != -1 && currentCol != -1)
		{
			// set the bounds on the control
			// this will only work w/ our DataGridTexBox control
			DataGridTextBoxColumn col = editColumn as DataGridTextBoxColumn;
			Rectangle editBounds = GetCellBounds(currentRow, currentCol);

			gridState[GRIDSTATE_editControlChanging] = true;
			try
			{
				col.TextBox.Bounds = editBounds;
			}
			finally
			{
				gridState[GRIDSTATE_editControlChanging] = false;
			}
		}

		if (gridState[GRIDSTATE_inAddNewRow])
		{
			int currentRowCount = DataGridRowsLength;
			DataGridRow[] newDataGridRows = new DataGridRow[currentRowCount + 1];
			for (int i = 0; i < currentRowCount; i++)
			{
				newDataGridRows[i] = localGridRows[i];
			}

			// put the AddNewRow
			newDataGridRows[currentRowCount] = new DataGridAddNewRow(this, myGridTable, currentRowCount);
			SetDataGridRows(newDataGridRows, currentRowCount + 1);

			if (editColumn.isEditRequired)
			{
				Edit();
			}
			// put this after the call to edit so that
			// CommitEdit knows that the gridState[GRIDSTATE_inAddNewRow] is true;
			gridState[GRIDSTATE_inAddNewRow] = false;
			gridState[GRIDSTATE_isEditing] = true;
			gridState[GRIDSTATE_isNavigating] = false;
			NotifyRenderRequired();
			return;
		}

		gridState[GRIDSTATE_isEditing] = true;
		gridState[GRIDSTATE_isNavigating] = false;
		InvalidateRowHeader(currentRow);

		// tell the current row to lose the childFocuse
		if (currentRow < localGridRows.Length)
		{
			localGridRows[currentRow].LoseChildFocus(layout.RowHeaders, isRightToLeft());
		}
	}

	internal protected virtual void ColumnStartedEditing(Control editingControl)
	{
		if (editingControl == null)
		{
			return;
		}

		ColumnStartedEditing(editingControl.Bounds);
	}

	public void SetDataBinding(object dataSource, string dataMember)
	{
		parentRows.Clear();
		originalState = null;
		caption.BackButtonActive = caption.DownButtonActive = caption.BackButtonVisible = false;
		caption.SetDownButtonDirection(!layout.ParentRowsVisible);

		Set_ListManager(dataSource, dataMember, false);
	}

	/// <summary>
	///  Gets or sets the data source that the grid is displaying data for.
	/// </summary>
	public object DataSource
	{
		get => dataSource;
		set
		{
			if (value != null && !(value is IList || value is IListSource))
			{
				throw new ArgumentException(SR.BadDataSourceForComplexBinding);
			}

			if (dataSource != null && dataSource.Equals(value))
			{
				return;
			}

			// when the designer resets the dataSource to null, set the dataMember to null, too
			if ((value == null || value == Convert.DBNull) && DataMember != null && DataMember.Length != 0)
			{
				dataSource = null;
				DataMember = string.Empty;
				return;
			}

			// if we are setting the dataSource and the dataMember is not a part
			// of the properties in the dataSource, then set the dataMember to ""
			//
			if (value != null)
			{
				EnforceValidDataMember(value);
			}

			Debug.WriteLineIf(CompModSwitches.DataGridCursor.TraceVerbose, "DataGridCursor: DataSource being set to " + ((value == null) ? "null" : value.ToString()));

			// when we change the dataSource, we need to clear the parent rows.
			// the same goes for all the caption UI: reset it when the datasource changes.
			//
			ResetParentRows();
			Set_ListManager(value, DataMember, false);
		}
	}
	object dataSource;

	/// <summary>
	///  Gets or sets the specific table in a DataSource for the control.
	/// </summary>
	public string DataMember
	{
		get => dataMember;
		set
		{
			if (dataMember != null && dataMember.Equals(value))
			{
				return;
			}

			Debug.WriteLineIf(CompModSwitches.DataGridCursor.TraceVerbose, "DataGridCursor: DataSource being set to " + (value ?? "null"));
			// when we change the dataMember, we need to clear the parent rows.
			// the same goes for all the caption UI: reset it when the datamember changes.
			//
			ResetParentRows();
			Set_ListManager(DataSource, value, false);
		}
	}
	string dataMember = string.Empty;

	internal void Set_ListManager(object newDataSource, string newDataMember, bool force)
	{
		Set_ListManager(newDataSource, newDataMember, force, true);        // true for forcing column creation
	}

	// prerequisite: the dataMember and the dataSource should be set to the new values
	// will do the following:
	// call EndEdit on the current listManager, will unWire the listManager events, will set the listManager to the new
	// reality, will wire the new listManager, will update the policy, will set the dataGridTable, will reset the ui state.
	internal void Set_ListManager(object newDataSource, string newDataMember, bool force, bool forceColumnCreation)
	{
		bool dataSourceChanged = DataSource != newDataSource;
		bool dataMemberChanged = DataMember != newDataMember;

		// if nothing happened, then why do any work?
		if (!force && !dataSourceChanged && !dataMemberChanged && gridState[GRIDSTATE_inSetListManager])
		{
			return;
		}

		gridState[GRIDSTATE_inSetListManager] = true;
		if (toBeDisposedEditingControl != null)
		{
			Controls.Remove(toBeDisposedEditingControl);
			toBeDisposedEditingControl = null;
		}
		try
		{
			// will endEdit on the current listManager
			UpdateListManager();

			// unwire the events:
			if (listManager != null)
			{
				UnWireDataSource();
			}

			CurrencyManager oldListManager = listManager;
			bool listManagerChanged = false;
			// set up the new listManager
			// CAUTION: we need to set up the listManager in the grid before setting the dataSource/dataMember props
			// in the grid. the reason is that if the BindingContext was not yet requested, and it is created in the BindingContext prop
			// then the grid will call Set_ListManager again, and eventually that means that the dataGrid::listManager will
			// be hooked up twice to all the events (PositionChanged, ItemChanged, CurrentChanged)
			if (newDataSource != null && BindingContext != null && !(newDataSource == Convert.DBNull))
			{
				listManager = (CurrencyManager)BindingContext[newDataSource, newDataMember];
			}
			else
			{
				listManager = null;
			}

			// update the dataSource and the dateMember
			dataSource = newDataSource;
			dataMember = newDataMember ?? "";

			listManagerChanged = (listManager != oldListManager);

			// wire the events
			if (listManager != null)
			{
				WireDataSource();
				// update the policy
				policy.UpdatePolicy(listManager, ReadOnly);
			}

			if (!Initializing)
			{
				if (listManager == null)
				{
					if (ContainsFocus && !Focused && Parent == null)
					{
						// if we unparent the active control then the form won't close
						for (int i = 0; i < Controls.Count; i++)
						{
							if (Controls[i].Focused)
							{
								toBeDisposedEditingControl = Controls[i];
								break;
							}
						}

						if (toBeDisposedEditingControl == horizScrollBar || toBeDisposedEditingControl == vertScrollBar)
						{
							toBeDisposedEditingControl = null;
						}
					}

					SetDataGridRows(null, 0);
					defaultTableStyle.GridColumnStyles.Clear();
					SetDataGridTable(defaultTableStyle, forceColumnCreation);

					if (toBeDisposedEditingControl != null)
					{
						Controls.Add(toBeDisposedEditingControl);
					}
				}
			}

			// PERF: if the listManager did not change, then do not:
			//      1. create new rows
			//      2. create new columns
			//      3. compute the errors in the list
			//
			// when the metaDataChanges, we need to recreate
			// the rows and the columns
			//
			if (listManagerChanged || gridState[GRIDSTATE_metaDataChanged])
			{
				if (listManager != null)
				{
					// get rid of the old gridColumns
					// we need to clear the old column collection even when navigating to
					// a list that has a table style associated w/ it. Why? because the
					// old column collection will be used by the parent rows to paint
					defaultTableStyle.GridColumnStyles.ResetDefaultColumnCollection();

					DataGridTableStyle newGridTable = dataGridTables[listManager.GetListName()];
					if (newGridTable == null)
					{
						SetDataGridTable(defaultTableStyle, forceColumnCreation);
					}
					else
					{
						SetDataGridTable(newGridTable, forceColumnCreation);
					}

					// set the currentRow in ssync w/ the position in the listManager
					currentRow = listManager.Position == -1 ? 0 : listManager.Position;
				}

				// when we create the rows we need to use the current dataGridTable
				RecreateDataGridRows();

				ComputeMinimumRowHeaderWidth();
				if (myGridTable.IsDefault)
				{
					RowHeaderWidth = Math.Max(minRowHeaderWidth, RowHeaderWidth);
				}
				else
				{
					myGridTable.RowHeaderWidth = Math.Max(minRowHeaderWidth, RowHeaderWidth);
				}

				ListHasErrors = DataGridSourceHasErrors();

				ResetUIState();

				OnDataSourceChanged(EventArgs.Empty);
			}
		}
		finally
		{
			gridState[GRIDSTATE_inSetListManager] = false;
		}
	}
	// currently focused control
	// we want to unparent it either when rebinding the grid or when the grid is disposed
	Control toBeDisposedEditingControl;

	protected virtual void OnDataSourceChanged(EventArgs e)
	{
		DataSourceChanged?.Invoke(this, e);
	}
	public event EventHandler DataSourceChanged;

	void WireDataSource()
	{
		listManager.CurrentChanged += currentChangedHandler;
		listManager.PositionChanged += positionChangedHandler;
		listManager.ItemChanged += itemChangedHandler;
		listManager.MetaDataChanged += metaDataChangedHandler;
	}

	void UnWireDataSource()
	{
		listManager.CurrentChanged -= currentChangedHandler;
		listManager.PositionChanged -= positionChangedHandler;
		listManager.ItemChanged -= itemChangedHandler;
		listManager.MetaDataChanged -= metaDataChangedHandler;
	}

	void DataSource_ItemChanged(object sender, ItemChangedEventArgs ea)
	{
		Debug.WriteLineIf(CompModSwitches.DataGridCursor.TraceVerbose, "DataGridCursor: DataSource_ItemChanged at index " + ea.Index.ToString(CultureInfo.InvariantCulture));

		// if ea.Index == -1, then we invalidate all rows.
		if (ea.Index == -1)
		{
			DataSource_Changed(sender, EventArgs.Empty);
		}
		else
		{
			// let's see how we are doing w/ the errors
			object errObj = listManager[ea.Index];
			bool oldListHasErrors = ListHasErrors;
			if (errObj is IDataErrorInfo)
			{
				if (((IDataErrorInfo)errObj).Error.Length != 0)
				{
					ListHasErrors = true;
				}
				else if (ListHasErrors)
				{
					// maybe there was an error that now is fixed
					ListHasErrors = DataGridSourceHasErrors();
				}
			}

			// Invalidate the row only if we did not change the ListHasErrors
			if (oldListHasErrors == ListHasErrors)
			{
				InvalidateRow(ea.Index);
			}

			// we need to update the edit box:
			// we update the text in the edit box only when the currentRow
			// equals the ea.Index
			if (editColumn != null && ea.Index == currentRow)
			{
				editColumn.UpdateUI(ListManager, ea.Index, null);
			}
		}
	}

	void DataSource_RowChanged(object sender, EventArgs ea)
	{
		// it may be the case that our cache was not updated
		// to the latest changes in the list : CurrentChanged is fired before
		// ListChanged.
		// So invalidate the row if there is something to invalidate
		DataGridRow[] rows = DataGridRows;
		if (currentRow < DataGridRowsLength)
		{
			InvalidateRow(currentRow);
		}
	}

	/// <summary>
	///  Fired by the DataSource when row position moves.
	/// </summary>
	void DataSource_PositionChanged(object sender, EventArgs ea)
	{
		Debug.WriteLineIf(CompModSwitches.DataGridCursor.TraceVerbose, "DataGridCursor: DataSource_PositionChanged to " + listManager.Position.ToString(CultureInfo.InvariantCulture));
		// the grid will get the PositionChanged event
		// before the OnItemChanged event when a row will be deleted in the backEnd;
		// we still want to keep the old rows when the user deletes the rows using the grid
		// and we do not want to do the same work twice when the user adds a row via the grid
		if (DataGridRowsLength > listManager.Count + (policy.AllowAdd ? 1 : 0) && !gridState[GRIDSTATE_inDeleteRow])
		{
			RecreateDataGridRows();
		}
		if (ListManager.Position != currentRow)
		{
			CurrentCell = new DataGridCell(listManager.Position, currentCol);
		}
	}

	internal void DataSource_MetaDataChanged(object sender, EventArgs e)
	{
		MetaDataChanged();
	}

	void DataSource_Changed(object sender, EventArgs ea)
	{
		Debug.WriteLineIf(CompModSwitches.DataGridCursor.TraceVerbose, "DataGridCursor: DataSource_Changed");

		// the grid will receive the dataSource_Changed event when
		// allowAdd changes on the dataView.
		policy.UpdatePolicy(ListManager, ReadOnly);
		if (gridState[GRIDSTATE_inListAddNew])
		{
			DataGridRow[] gridRows = DataGridRows;
			int currentRowCount = DataGridRowsLength;
			// put the added row:
			//
			gridRows[currentRowCount - 1] = new DataGridRelationshipRow(this, myGridTable, currentRowCount - 1);
			SetDataGridRows(gridRows, currentRowCount);
		}
		else if (gridState[GRIDSTATE_inAddNewRow] && !gridState[GRIDSTATE_inDeleteRow])
		{
			// when the backEnd adds a row and we are still gridState[GRIDSTATE_inAddNewRow]
			listManager.CancelCurrentEdit();
			gridState[GRIDSTATE_inAddNewRow] = false;
			RecreateDataGridRows();
		}
		else if (!gridState[GRIDSTATE_inDeleteRow])
		{
			RecreateDataGridRows();
			currentRow = Math.Min(currentRow, listManager.Count);
		}

		bool oldListHasErrors = ListHasErrors;
		ListHasErrors = DataGridSourceHasErrors();
		// if we changed the ListHasErrors, then the grid is already invalidated
		if (oldListHasErrors == ListHasErrors)
		{
			InvalidateInside();
		}
	}

	void MetaDataChanged()
	{
		// when we reset the Binding in the grid, we need to clear the parent rows.
		// the same goes for all the caption UI: reset it when the datasource changes.
		parentRows.Clear();
		caption.BackButtonActive = caption.DownButtonActive = caption.BackButtonVisible = false;
		caption.SetDownButtonDirection(!layout.ParentRowsVisible);

		gridState[GRIDSTATE_metaDataChanged] = true;
		try
		{
			if (originalState != null)
			{
				// set the originalState to null so that Set_ListManager knows that
				// it has to unhook the MetaDataChanged events
				Set_ListManager(originalState.DataSource, originalState.DataMember, true);
				originalState = null;
			}
			else
			{
				Set_ListManager(DataSource, DataMember, true);
			}
		}
		finally
		{
			gridState[GRIDSTATE_metaDataChanged] = false;
		}
	}

	bool DataGridSourceHasErrors()
	{
		if (listManager == null)
		{
			return false;
		}

		for (int i = 0; i < listManager.Count; i++)
		{
			object errObj = listManager[i];
			if (errObj is IDataErrorInfo)
			{
				string errString = ((IDataErrorInfo)errObj).Error;
				if (errString != null && errString.Length != 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool ReadOnly
	{
		get => gridState[GRIDSTATE_readOnlyMode];
		set
		{
			if (ReadOnly != value)
			{
				bool recreateRows = false;
				if (value)
				{
					// AllowAdd happens to have the same boolean value as whether we need to recreate rows.
					recreateRows = policy.AllowAdd;

					policy.AllowRemove = false;
					policy.AllowEdit = false;
					policy.AllowAdd = false;
				}
				else
				{
					recreateRows |= policy.UpdatePolicy(listManager, value);
				}
				gridState[GRIDSTATE_readOnlyMode] = value;
				DataGridRow[] dataGridRows = DataGridRows;
				if (recreateRows)
				{
					RecreateDataGridRows();

					// keep the selected rows
					DataGridRow[] currentDataGridRows = DataGridRows;
					int rowCount = Math.Min(currentDataGridRows.Length, dataGridRows.Length);
					for (int i = 0; i < rowCount; i++)
					{
						if (dataGridRows[i].Selected)
						{
							currentDataGridRows[i].Selected = true;
						}
					}
				}

				// the addnew row needs to be updated.
				PerformLayout();
				InvalidateInside();
				OnReadOnlyChanged(EventArgs.Empty);
			}
		}
	}

	protected internal async Task OnRowDropAsync(WinzorDragEventArgs arg, HitTestInfo hitTestInfo)
	{
		await InvokeWinzorDispatcherAsync(() =>
		{
			Cursor.Position = new Point(arg.ClientX, arg.ClientY);
			if (arg.ControlID != null && int.TryParse(arg.ControlID, out var listID))
			{
				OnDragDrop(
					new DragEventArgs(
						new DataObject(new Tuple<int, int>(listID, hitTestInfo.row)),
						0,
						arg.ClientX,
						arg.ClientY,
						DragDropEffects.Move,
						DragDropEffects.Move));
			}
		});
	}
	protected virtual void OnReadOnlyChanged(EventArgs e)
	{
		ReadOnlyChanged?.Invoke(this, e);
	}
	public event EventHandler ReadOnlyChanged;

	public event NavigateEventHandler Navigate;

	protected HitTestInfo currentHitTest;

	public virtual bool WholeRowSelectedOnClick => false;

	bool isRightMouseButtonDown;

#nullable enable
	protected internal virtual async Task OnMouseDownAsync(WebMouseEventArgs e, HitTestInfo hitTest, ElementReference? elementReference = null)
	{
		if (!Enabled)
		{
			return;
		}

		isRightMouseButtonDown = e.GetMouseButtons() == MouseButtons.Right;

		await InvokeWinzorDispatcherAsync(() =>
		{
			//In WinForms, the edit control would be rendered synchronously and would handle this moues event.
			//In Winzor, the edit control is not rendered on the client yet and so the event is dispatched to the DataGrid instead.
			//We should ignore this event to avoid running any unexpeced event handlers for the datagrid instance, such as DoubleClick.
			if (e.Detail > 1 && hitTest.Type == HitTestType.Cell &&
			(hitTest.Column == currentHitTest?.Column && currentHitTest?.Column == CurrentCell.ColumnNumber) &&
			(hitTest.Row == currentHitTest?.Row && currentHitTest?.Row == CurrentCell.RowNumber) && FindEditControl() != null)
			{
				return;
			}
			currentHitTest = hitTest;
			OnMouseDownCore(e);

			InvokeRenderDispatcher(async () =>
			{
				if (hitTest.Type == HitTestType.ColumnResize && e.GetMouseButtons() == MouseButtons.Left && e.Detail == 1 && elementReference is ElementReference columnElementReference)
				{
					await (GetJSInterop<IGridJSInterop>()?.ResizeColumnAsync(dotNetObjectReference, e, columnElementReference) ?? Task.CompletedTask);
				}

				if (hitTest.Type == HitTestType.Cell && ReadOnly && WholeRowSelectedOnClick && e.GetMouseButtons() == MouseButtons.Left && e.Detail == 1)
				{
					await (GetJSInterop<IGridJSInterop>()?.MultiSelectionRowsAsync(dotNetObjectReference, e, hitTest.Row, ElementReference) ?? Task.CompletedTask);
				}
			});
		});
	}

	protected internal virtual async Task OnContextMenuAsync(WebMouseEventArgs e, HitTestInfo hitTest, ElementReference? elementReference = null)
	{
		if (!Enabled)
		{
			return;
		}

		await InvokeWinzorDispatcherAsync(() =>
		{
			currentHitTest = hitTest;
			OnContextMenuCore(e);
		});
	}

	/// <summary>
	///  Raises the <see cref='Control.MouseDown'/> event.
	/// </summary>
	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);

		gridState[GRIDSTATE_childLinkFocused] = false;
		gridState[GRIDSTATE_dragging] = false;
		if (listManager == null)
		{
			return;
		}
		HitTestInfo location = HitTest(e.X, e.Y);
		Keys nModifier = ModifierKeys;
		bool isControlDown = (nModifier & Keys.Control) == Keys.Control && (nModifier & Keys.Alt) == 0;
		bool isShiftDown = (nModifier & Keys.Shift) == Keys.Shift;

		// Only left clicks for now
		if (e.Button != MouseButtons.Left)
		{
			return;
		}

		// Check column resize
		if (location.type == HitTestType.ColumnResize)
		{
			if (e.Clicks > 1)
			{
				ColAutoResize(location.col);
			}
			else
			{
				// In Winzor, we have already handled the ColumnResizeBegin in OnMouseDownAsync
				ColResizeBegin(e, location.col);
			}

			return;
		}

		// Check row resize
		if (location.type == HitTestType.RowResize)
		{
			if (e.Clicks > 1)
			{
				RowAutoResize(location.row);
			}
			else
			{
				RowResizeBegin(e, location.row);
			}
			return;
		}

		// Check column headers
		if (location.type == HitTestType.ColumnHeader && location.col > -1)
		{
			trackColumnHeader = myGridTable.GridColumnStyles[location.col].PropertyDescriptor;
			return;
		}

		if (location.type == HitTestType.Caption)
		{
			return;
		}

		// Check row headers
		if (location.type == HitTestType.RowHeader)
		{
			EndEdit();
			if (DataGridRows != null && DataGridRows.Length > location.row && !(DataGridRows[location.row] is DataGridAddNewRow))
			{
				int savedCurrentRow = currentRow;
				CurrentCell = new DataGridCell(location.row, currentCol);
				if (location.row != savedCurrentRow &&
					currentRow != location.row &&
					currentRow == savedCurrentRow)
				{
					// The data grid was not able to move away from its previous current row.
					// Be defensive and don't select the row.
					return;
				}
			}

			if (isControlDown)
			{
				if (IsSelected(location.row))
				{
					UnSelect(location.row);
				}
				else
				{
					Select(location.row);
				}
			}
			else
			{
				if (lastRowSelected == -1 || !isShiftDown)
				{
					ResetSelection();
					Select(location.row);
				}
				else
				{
					int lowerRow = Math.Min(lastRowSelected, location.row);
					int upperRow = Math.Max(lastRowSelected, location.row);

					// we need to reset the old SelectedRows.
					// ResetSelection() will also reset lastRowSelected, so we
					// need to save it
					int saveLastRowSelected = lastRowSelected;
					ResetSelection();
					lastRowSelected = saveLastRowSelected;

					if (DataGridRows != null && DataGridRows.Length > 0)
					{
						// select the rows in the range (lowerRow, upperRow
						DataGridRow[] rows = DataGridRows;
						for (int i = lowerRow; i <= upperRow; i++)
						{
							rows[i].Selected = true;
							numSelectedRows++;
						}
					}

					// hide the edit box:
					EndEdit();
					return;
				}
			}

			lastRowSelected = location.row;
			return;
		}
	}
#nullable disable
	PropertyDescriptor trackColumnHeader;

	internal async Task OnDragStartAsync(WebDragEventArgs args, HitTestInfo hitTest, ElementReference? elementReference = null)
	{
		if (!Enabled)
		{
			return;
		}

		await InvokeWinzorDispatcherAsync(() =>
		{
			currentHitTest = hitTest;
			Cursor.Position = new Point((int)args.ClientX, (int)args.ClientY);

			OnDragStart(
				new DragEventArgs(
					new DataObject(this),
					0,
					(int)args.ClientX,
					(int)args.ClientY,
					DragDropEffects.Move,
					DragDropEffects.Move));
		});
	}

	internal async Task OnDropAsync(WebDragEventArgs args, HitTestInfo hitTest, ElementReference? elementReference = null)
	{
		if (!Enabled)
		{
			return;
		}

		await InvokeWinzorDispatcherAsync(() =>
		{
			currentHitTest = hitTest;
			OnDragDrop(
					new DragEventArgs(
						new DataObject(this),
						0,
						(int)args.ClientX,
						(int)args.ClientY,
						DragDropEffects.Move,
						DragDropEffects.Move));
		});
	}

	internal async Task OnMouseUpAsync(WebMouseEventArgs e, HitTestInfo hitTest, ElementReference? elementReference = null)
	{
		if (!Enabled)
		{
			return;
		}

		await InvokeWinzorDispatcherAsync(() =>
		{
			currentHitTest = hitTest;
			OnMouseUpCore(e);
		});
	}

	/// <summary>
	///  Raises the <see cref='Control.MouseUp'/> event.
	/// </summary>
	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		gridState[GRIDSTATE_dragging] = false;
		if (listManager == null || myGridTable == null)
		{
			return;
		}

		if (gridState[GRIDSTATE_trackColResize])
		{
			ColResizeEnd(e);
		}

		if (gridState[GRIDSTATE_trackRowResize])
		{
			RowResizeEnd(e);
		}

		gridState[GRIDSTATE_trackColResize] = false;
		gridState[GRIDSTATE_trackRowResize] = false;

		var ci = HitTest(e.X, e.Y);

		// Check column headers
		if (ci.type == HitTestType.ColumnHeader)
		{
			PropertyDescriptor prop = myGridTable.GridColumnStyles[ci.col].PropertyDescriptor;
			if (prop == trackColumnHeader)
			{
				ColumnHeaderClicked(trackColumnHeader);
			}
		}

		trackColumnHeader = null;
	}

	protected internal async Task OnCellFocusInAsync(WinzorFocusInEventArgs args, int row, int column)
	{
		using var activity = TelemetryService.ActivitySource.StartActivity($"{GetType().Name}.{nameof(OnCellFocusInAsync)}");
		if (!Enabled || args.InitiatedFromServer)
		{
			return;
		}

		await InvokeWinzorDispatcherAsync(() =>
		{
			Focus();

			// Avoid multi-line selections being lost when using the context menu.
			// The right mouse button is checked for as the context menu is activated *after* this code.
			var returnOnMultiLine = (isRightMouseButtonDown || HasActiveContextMenu) && numSelectedRows >= 1 &&
									IsSelected(row);
			isRightMouseButtonDown = false;
			if (returnOnMultiLine || WholeRowSelectedOnClick)
			{
				return;
			}

			// Try to commit changes to cell if we were editing
			if (gridState[GRIDSTATE_isEditing])
			{
				if (!CommitEdit())
				{
					Edit(); // if we can't commit the value put the edit box so that the user sees where the focus is
					return;
				}
			}

			var target = new DataGridCell(row, column);
			if (CurrentCell.Equals(target))
			{
				EnsureVisible(row, column);
			}

			ResetSelection();
			CurrentCell = target;
			Edit();
		});
	}

	int CurrentColumn
	{
		get
		{
			return CurrentCell.ColumnNumber;
		}
		set
		{
			CurrentCell = new DataGridCell(currentRow, value);
		}
	}

	int CurrentRow
	{
		get
		{
			return CurrentCell.RowNumber;
		}
		set
		{
			CurrentCell = new DataGridCell(value, currentCol);
		}
	}

	/// <summary>
	///  Gets or sets a value that indicates whether a key should be processed
	///  further.
	/// </summary>
	protected override bool ProcessDialogKey(Keys keyData)
	{
		DataGridRow[] localGridRows = DataGridRows;
		if (listManager != null && DataGridRowsLength > 0 && localGridRows[currentRow].OnKeyPress(keyData))
		{
			// Current Row ate the keystroke
			return true;
		}

		switch (keyData & Keys.KeyCode)
		{
			case Keys.Up:
			case Keys.Down:
			case Keys.Left:
			case Keys.Right:
			case Keys.Next:
			case Keys.Prior:
			case Keys.A:
				KeyEventArgs ke = new KeyEventArgs(keyData);
				if (ProcessGridKey(ke))
				{
					return true;
				}

				break;
		}

		return base.ProcessDialogKey(keyData);
	}

	/// <summary>
	///  Processes keys for grid navigation.
	/// </summary>
	[Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1040:Check for mixed arithmatic between scaled and unscaled components", Justification = "<Pending>")]
	protected bool ProcessGridKey(KeyEventArgs ke)
	{
		if (listManager == null || myGridTable == null)
		{
			return false;
		}

		DataGridRow[] localGridRows = DataGridRows;
		KeyEventArgs biDiKe = ke;
		// check for Bi-Di
		//
		if (isRightToLeft())
		{
			switch (ke.KeyCode)
			{
				case Keys.Left:
					biDiKe = new KeyEventArgs((Keys.Right | ke.Modifiers));
					break;
				case Keys.Right:
					biDiKe = new KeyEventArgs((Keys.Left | ke.Modifiers));
					break;
				default:
					break;
			}
		}

		GridColumnStylesCollection cols = myGridTable.GridColumnStyles;
		int firstColumnMarkedVisible = 0;
		int lastColumnMarkedVisible = cols.Count;
		for (int i = 0; i < cols.Count; i++)
		{
			if (cols[i].PropertyDescriptor != null)
			{
				firstColumnMarkedVisible = i;
				break;
			}
		}

		for (int i = cols.Count - 1; i >= 0; i--)
		{
			if (cols[i].PropertyDescriptor != null)
			{
				lastColumnMarkedVisible = i;
				break;
			}
		}

		switch (biDiKe.KeyCode)
		{
			case Keys.Up:
				gridState[GRIDSTATE_childLinkFocused] = false;
				if (dataGridRowsLength == 0)
				{
					return true;
				}

				if (biDiKe.Control && !biDiKe.Alt)
				{
					if (biDiKe.Shift)
					{
						DataGridRow[] gridRows = DataGridRows;

						int savedCurrentRow = currentRow;
						CurrentRow = 0;

						ResetSelection();

						for (int i = 0; i <= savedCurrentRow; i++)
						{
							gridRows[i].Selected = true;
						}

						numSelectedRows = savedCurrentRow + 1;
						// hide the edit box
						//
						EndEdit();
						return true;
					}
					// do not make the parentRowsVisible = false;
					// ParentRowsVisible = false;
					ResetSelection();
					CurrentRow = 0;
					return true;
				}
				else if (biDiKe.Shift)
				{
					DataGridRow[] gridRows = DataGridRows;
					// keep a continous selected region
					if (gridRows[currentRow].Selected)
					{
						if (currentRow >= 1)
						{
							if (gridRows[currentRow - 1].Selected)
							{
								if (currentRow >= DataGridRowsLength - 1 || !gridRows[currentRow + 1].Selected)
								{
									numSelectedRows--;
									gridRows[currentRow].Selected = false;
								}
							}
							else
							{
								numSelectedRows += gridRows[currentRow - 1].Selected ? 0 : 1;
								gridRows[currentRow - 1].Selected = true;
							}
							CurrentRow--;
						}
					}
					else
					{
						numSelectedRows++;
						gridRows[currentRow].Selected = true;
						if (currentRow >= 1)
						{
							numSelectedRows += gridRows[currentRow - 1].Selected ? 0 : 1;
							gridRows[currentRow - 1].Selected = true;
							CurrentRow--;
						}
					}

					// hide the edit box:
					//
					EndEdit();
					return true;
				}
				else if (biDiKe.Alt)
				{
					// will need to collapse all child table links
					// -1 is for all rows, and false is for collapsing the rows
					SetRowExpansionState(-1, false);
					return true;
				}
				ResetSelection();
				CurrentRow -= 1;
				Edit();
				break;
			case Keys.Down:
				gridState[GRIDSTATE_childLinkFocused] = false;
				if (dataGridRowsLength == 0)
				{
					return true;
				}

				if (biDiKe.Control && !biDiKe.Alt)
				{
					if (biDiKe.Shift)
					{
						int savedCurrentRow = currentRow;
						CurrentRow = Math.Max(0, DataGridRowsLength - (policy.AllowAdd ? 2 : 1));
						DataGridRow[] gridRows = DataGridRows;

						ResetSelection();

						for (int i = savedCurrentRow; i <= currentRow; i++)
						{
							gridRows[i].Selected = true;
						}

						numSelectedRows = currentRow - savedCurrentRow + 1;
						// hide the edit box
						//
						EndEdit();
						return true;
					}
					// do not make the parentRowsVisible = true;
					// ParentRowsVisible = true;
					ResetSelection();
					CurrentRow = Math.Max(0, DataGridRowsLength - (policy.AllowAdd ? 2 : 1));
					return true;
				}
				else if (biDiKe.Shift)
				{
					DataGridRow[] gridRows = DataGridRows;

					// keep a continous selected region
					if (gridRows[currentRow].Selected)
					{
						// -1 because we index from 0
						if (currentRow < DataGridRowsLength - (policy.AllowAdd ? 1 : 0) - 1)
						{
							if (gridRows[currentRow + 1].Selected)
							{
								if (currentRow == 0 || !gridRows[currentRow - 1].Selected)
								{
									numSelectedRows--;
									gridRows[currentRow].Selected = false;
								}
							}
							else
							{
								numSelectedRows += gridRows[currentRow + 1].Selected ? 0 : 1;
								gridRows[currentRow + 1].Selected = true;
							}

							CurrentRow++;
						}
					}
					else
					{
						numSelectedRows++;
						gridRows[currentRow].Selected = true;
						// -1 because we index from 0, and -1 so this is not the last row
						// so it adds to -2
						if (currentRow < DataGridRowsLength - (policy.AllowAdd ? 1 : 0) - 1)
						{
							CurrentRow++;
							numSelectedRows += gridRows[currentRow].Selected ? 0 : 1;
							gridRows[currentRow].Selected = true;
						}
					}

					// hide the edit box:
					//
					EndEdit();
					return true;
				}
				else if (biDiKe.Alt)
				{
					// will need to expande all child table links
					// -1 is for all rows, and true is for expanding the rows
					SetRowExpansionState(-1, true);
					return true;
				}
				ResetSelection();
				// this is different from winforms, where Edit() gets invoked before CurrentRow += 1
				CurrentRow += 1;
				Edit();
				break;
			case Keys.Left:
				gridState[GRIDSTATE_childLinkFocused] = false;
				ResetSelection();
				if ((biDiKe.Modifiers & Keys.Modifiers) == Keys.Alt)
				{
					if (Caption.BackButtonVisible)
					{
						NavigateBack();
					}

					return true;
				}

				if ((biDiKe.Modifiers & Keys.Control) == Keys.Control)
				{
					// we should navigate to the first visible column
					CurrentColumn = firstColumnMarkedVisible;
					break;
				}

				if (currentCol == firstColumnMarkedVisible && currentRow != 0)
				{
					CurrentRow -= 1;
					int newCol = MoveLeftRight(myGridTable.GridColumnStyles, myGridTable.GridColumnStyles.Count, false);
					CurrentColumn = newCol;
				}
				else
				{
					int newCol = MoveLeftRight(myGridTable.GridColumnStyles, currentCol, false);
					if (newCol == -1)
					{
						if (currentRow == 0)
						{
							return true;
						}
						else
						{
							// go to the previous row:
							CurrentRow -= 1;
							CurrentColumn = lastColumnMarkedVisible;
						}
					}
					else
					{
						CurrentColumn = newCol;
					}
				}
				break;
			case Keys.Right:
				gridState[GRIDSTATE_childLinkFocused] = false;
				ResetSelection();
				if ((biDiKe.Modifiers & Keys.Control) == Keys.Control && !biDiKe.Alt)
				{
					// we should navigate to the last column that is marked as Visible
					CurrentColumn = lastColumnMarkedVisible;
					break;
				}

				if (currentCol == lastColumnMarkedVisible && currentRow != DataGridRowsLength - 1)
				{
					CurrentRow += 1;
					// navigate to the first visible column
					CurrentColumn = firstColumnMarkedVisible;
				}
				else
				{
					int newCol = MoveLeftRight(myGridTable.GridColumnStyles, currentCol, true);
					if (newCol == cols.Count + 1)
					{
						// navigate to the first visible column
						// and the next row
						//
						CurrentColumn = firstColumnMarkedVisible;
						CurrentRow++;
					}
					else
					{
						CurrentColumn = newCol;
					}
				}
				break;
			case Keys.A:
				gridState[GRIDSTATE_childLinkFocused] = false;
				if (biDiKe.Control && !biDiKe.Alt)
				{
					DataGridRow[] gridRows = DataGridRows;
					for (int i = 0; i < DataGridRowsLength; i++)
					{
						if (gridRows[i] is DataGridRelationshipRow)
						{
							gridRows[i].Selected = true;
						}
					}

					numSelectedRows = DataGridRowsLength - (policy.AllowAdd ? 1 : 0);
					// hide the edit box
					//
					EndEdit();
					return true;
				}
				return false;
			case Keys.Next:
				gridState[GRIDSTATE_childLinkFocused] = false;
				if (this.dataGridRowsLength == 0)
				{
					return true;
				}

				if (biDiKe.Shift)
				{
					int savedCurrentRow = currentRow;
					CurrentRow = Math.Min(DataGridRowsLength - (policy.AllowAdd ? 2 : 1), currentRow + visibleRowCount);

					DataGridRow[] gridRows = DataGridRows;
					for (int i = savedCurrentRow; i <= currentRow; i++)
					{
						if (!gridRows[i].Selected)
						{
							gridRows[i].Selected = true;
							numSelectedRows++;
						}
					}
					// hide edit box
					//
					EndEdit();
				}
				else if (biDiKe.Control && !biDiKe.Alt)
				{
					// map ctrl-pageDown to show the parentRows
					ParentRowsVisible = true;
				}
				else
				{
					ResetSelection();
					CurrentRow = Math.Min(DataGridRowsLength - (policy.AllowAdd ? 2 : 1), CurrentRow + visibleRowCount);
				}
				break;
			case Keys.Prior:
				if (this.dataGridRowsLength == 0)
				{
					return true;
				}

				gridState[GRIDSTATE_childLinkFocused] = false;
				if (biDiKe.Shift)
				{
					int savedCurrentRow = currentRow;
					CurrentRow = Math.Max(0, CurrentRow - visibleRowCount);

					DataGridRow[] gridRows = DataGridRows;
					for (int i = savedCurrentRow; i >= currentRow; i--)
					{
						if (!gridRows[i].Selected)
						{
							gridRows[i].Selected = true;
							numSelectedRows++;
						}
					}

					// hide the edit box
					//
					EndEdit();
				}
				else if (biDiKe.Control && !biDiKe.Alt)
				{
					// map ctrl-pageUp to hide the parentRows
					ParentRowsVisible = false;
				}
				else
				{
					ResetSelection();
					CurrentRow = Math.Max(0, CurrentRow - visibleRowCount);
				}
				break;
			case Keys.Home:
				gridState[GRIDSTATE_childLinkFocused] = false;
				if (this.dataGridRowsLength == 0)
				{
					return true;
				}

				ResetSelection();
				CurrentColumn = 0;
				if (biDiKe.Control && !biDiKe.Alt)
				{
					int currentRowSaved = currentRow;
					CurrentRow = 0;

					if (biDiKe.Shift)
					{
						// Ctrl-Shift-Home will select all the rows up to the first one
						DataGridRow[] gridRows = DataGridRows;
						for (int i = 0; i <= currentRowSaved; i++)
						{
							gridRows[i].Selected = true;
							numSelectedRows++;
						}
						// hide the edit box:
						EndEdit();
					}
					return true;
				}
				break;
			case Keys.End:
				gridState[GRIDSTATE_childLinkFocused] = false;
				if (this.dataGridRowsLength == 0)
				{
					return true;
				}

				ResetSelection();
				// go the the last visible column
				CurrentColumn = lastColumnMarkedVisible;

				if (biDiKe.Control && !biDiKe.Alt)
				{
					int savedCurrentRow = currentRow;
					CurrentRow = Math.Max(0, DataGridRowsLength - (policy.AllowAdd ? 2 : 1));

					if (biDiKe.Shift)
					{
						// Ctrl-Shift-Home will select all the rows up to the first one
						DataGridRow[] gridRows = DataGridRows;
						for (int i = savedCurrentRow; i <= currentRow; i++)
						{
							gridRows[i].Selected = true;
						}
						numSelectedRows = currentRow - savedCurrentRow + 1;
						// hide the edit box
						//
						EndEdit();
					}
					return true;
				}
				break;
		}

		return true;
	}

	protected override bool ProcessKeyPreview(ref Message m)
	{
		if (m.Msg == WM_KEYDOWN || m.Msg == WM_SYSKEYDOWN)
		{
			KeyEventArgs ke = new KeyEventArgs((Keys)(unchecked((int)(long)m.WParam)) | ModifierKeys);
			switch (ke.KeyCode)
			{
				case Keys.Up:
				case Keys.Down:
				case Keys.Right:
				case Keys.Left:
				case Keys.Prior:
				case Keys.Next:
				case Keys.Home:
				case Keys.End:
				case Keys.A:
					return ProcessGridKey(ke);
			}
			// Ctrl-Tab will be sent as a tab paired w/ a control on the KeyUp message
			//
		}
		else if (m.Msg == WM_KEYUP || m.Msg == WM_SYSKEYUP)
		{
			KeyEventArgs ke = new KeyEventArgs((Keys)(unchecked((int)(long)m.WParam)) | ModifierKeys);
			if (ke.KeyCode == Keys.Tab)
			{
				return ProcessGridKey(ke);
			}
		}

		return base.ProcessKeyPreview(ref m);
	}

	// convention:
	// if we return -1 it means that the user was going left and there were no visible columns to the left of the current one
	// if we return cols.Count + 1 it means that the user was going right and there were no visible columns to the right of the currrent
	int MoveLeftRight(GridColumnStylesCollection cols, int startCol, bool goRight)
	{
		int i;
		if (goRight)
		{
			for (i = startCol + 1; i < cols.Count; i++)
			{
				// if (cols[i].Visible && cols[i].PropertyDescriptor != null)
				if (cols[i].PropertyDescriptor != null)
				{
					return i;
				}
			}
			return i;
		}
		else
		{
			for (i = startCol - 1; i >= 0; i--)
			{
				// if (cols[i].Visible && cols[i].PropertyDescriptor != null)
				if (cols[i].PropertyDescriptor != null)
				{
					return i;
				}
			}
			return i;
		}
	}

	/// <summary>
	///  Sets whether a row is expanded or not.
	/// </summary>
	void SetRowExpansionState(int row, bool expanded)
	{
		if (row < -1 || row > DataGridRowsLength - (policy.AllowAdd ? 2 : 1))
		{
			throw new ArgumentOutOfRangeException(nameof(row));
		}

		DataGridRow[] localGridRows = DataGridRows;
		if (row == -1)
		{
			DataGridRelationshipRow[] expandableRows = GetExpandableRows();
			bool repositionEditControl = false;

			for (int r = 0; r < expandableRows.Length; ++r)
			{
				if (expandableRows[r].Expanded != expanded)
				{
					expandableRows[r].Expanded = expanded;
					repositionEditControl = true;
				}
			}
			if (repositionEditControl)
			{
				// we need to reposition the edit control
				if (gridState[GRIDSTATE_isNavigating] || gridState[GRIDSTATE_isEditing])
				{
					ResetSelection();
					Edit();
				}
			}
		}
		else if (localGridRows[row] is DataGridRelationshipRow expandableRow)
		{
			if (expandableRow.Expanded != expanded)
			{
				// we need to reposition the edit control
				if (gridState[GRIDSTATE_isNavigating] || gridState[GRIDSTATE_isEditing])
				{
					ResetSelection();
					Edit();
				}

				expandableRow.Expanded = expanded;
			}
		}
	}

	/// <summary>
	///  Not all rows in the DataGrid are expandable,
	///  this computes which ones are and returns an array
	///  of references to them.
	/// </summary>
	DataGridRelationshipRow[] GetExpandableRows()
	{
		int nExpandableRows = DataGridRowsLength;
		DataGridRow[] localGridRows = DataGridRows;
		if (policy.AllowAdd)
		{
			nExpandableRows = Math.Max(nExpandableRows - 1, 0);
		}

		DataGridRelationshipRow[] expandableRows = new DataGridRelationshipRow[nExpandableRows];
		for (int i = 0; i < nExpandableRows; i++)
		{
			expandableRows[i] = (DataGridRelationshipRow)localGridRows[i];
		}

		return expandableRows;
	}

	/// <summary>
	///  Navigates back to the table previously displayed in the grid.
	/// </summary>
	public void NavigateBack()
	{
		if (!CommitEdit() || parentRows.IsEmpty())
		{
			return;
		}
		// when navigating back, if the grid is inAddNewRow state, cancel the currentEdit.
		// we do not need to recreate the rows cause we are navigating back.
		// the grid will catch any exception that happens.
		if (gridState[GRIDSTATE_inAddNewRow])
		{
			gridState[GRIDSTATE_inAddNewRow] = false;
			try
			{
				listManager.CancelCurrentEdit();
			}
			catch
			{
			}
		}
		else
		{
			UpdateListManager();
		}

		DataGridState newState = parentRows.PopTop();

		ResetMouseState();

		newState.PullState(this, false);                // we do not want to create columns when navigating back

		// we need to have originalState != null when we process
		// Set_ListManager in the NavigateBack/NavigateTo methods.
		// otherwise the DataSource_MetaDataChanged event will not get registered
		// properly
		if (parentRows.GetTopParent() == null)
		{
			originalState = null;
		}

		DataGridRow[] localGridRows = DataGridRows;
		// what if the user changed the ReadOnly property
		// on the grid while the user was navigating to the child rows?
		//
		// what if the policy does not allow for allowAdd?
		//
		if ((ReadOnly || !policy.AllowAdd) == (localGridRows[DataGridRowsLength - 1] is DataGridAddNewRow))
		{
			int newDataGridRowsLength = (ReadOnly || !policy.AllowAdd) ? DataGridRowsLength - 1 : DataGridRowsLength + 1;
			DataGridRow[] newDataGridRows = new DataGridRow[newDataGridRowsLength];
			for (int i = 0; i < Math.Min(newDataGridRowsLength, DataGridRowsLength); i++)
			{
				newDataGridRows[i] = DataGridRows[i];
			}
			if (!ReadOnly && policy.AllowAdd)
			{
				newDataGridRows[newDataGridRowsLength - 1] = new DataGridAddNewRow(this, myGridTable, newDataGridRowsLength - 1);
			}
			SetDataGridRows(newDataGridRows, newDataGridRowsLength);
		}

		// when we navigate back from a child table,
		// it may be the case that in between the user added a tableStyle that is different
		// from the one that is currently in the grid
		// in that case, we need to reset the dataGridTableStyle in the rows
		localGridRows = DataGridRows;
		if (localGridRows != null && localGridRows.Length != 0)
		{
			DataGridTableStyle dgTable = localGridRows[0].DataGridTableStyle;
			if (dgTable != myGridTable)
			{
				for (int i = 0; i < localGridRows.Length; i++)
				{
					localGridRows[i].DataGridTableStyle = myGridTable;
				}
			}
		}

		// if we have the default table, when we navigate back
		// we also have the default gridColumns, w/ width = -1
		// we need to set the width on the new gridColumns
		//
		if (myGridTable.GridColumnStyles.Count > 0 && myGridTable.GridColumnStyles[0].Width == -1)
		{
			InitializeColumnWidths();
		}

		// reset the currentRow to the old position in the listmanager:
		currentRow = ListManager.Position == -1 ? 0 : ListManager.Position;

		// if the AllowNavigation changed while the user was navigating the
		// child tables, so that the new navigation mode does not allow childNavigation anymore
		// then reset the rows
		if (!AllowNavigation)
		{
			RecreateDataGridRows();
		}

		caption.BackButtonActive = (parentRows.GetTopParent() != null) && AllowNavigation;
		caption.BackButtonVisible = caption.BackButtonActive;
		caption.DownButtonActive = (parentRows.GetTopParent() != null);

		PerformLayout();
		Invalidate();

		Edit();
		OnNavigate(new NavigateEventArgs(false));
	}

	/// <summary>
	///  Raises the <see cref='Navigate'/>
	///  event.
	/// </summary>
	protected void OnNavigate(NavigateEventArgs e)
	{
		onNavigate?.Invoke(this, e);
	}
	readonly NavigateEventHandler onNavigate;

	void ResetMouseState()
	{
		oldRow = -1;
		gridState[GRIDSTATE_overCaption] = true;
	}
	// mouse move hot-tracking
	int oldRow = -1;

	/// <summary>
	///  Invalidates the parent rows area of the DataGrid
	/// </summary>
	internal void InvalidateParentRows()
	{
		if (layout.ParentRowsVisible)
		{
			Invalidate(layout.ParentRows);
		}
	}

	internal void ParentRowsDataChanged()
	{
		// do the reset work that is done in SetDataBindings, set_DataSource, set_DataMember;
		parentRows.Clear();
		caption.BackButtonActive = caption.DownButtonActive = caption.BackButtonVisible = false;
		caption.SetDownButtonDirection(!layout.ParentRowsVisible);
		object dSource = originalState.DataSource;
		string dMember = originalState.DataMember;
		// we don't need to set the GRIDSTATE_metaDataChanged bit, cause
		// the listManager from the originalState should be different from the current listManager
		//
		// set the originalState to null so that Set_ListManager knows that
		// it has to unhook the MetaDataChanged events
		originalState = null;
		Set_ListManager(dSource, dMember, true);
	}

	/// <summary>
	///  Determines the best fit size for the given column.
	/// </summary>
	void ColAutoResize(int col)
	{
		EndEdit();
		CurrencyManager listManager = this.listManager;
		if (listManager == null)
		{
			return;
		}

		var column = myGridTable.GridColumnStyles[col];
		var headerText = column.HeaderText;
		var headerFont = myGridTable.IsDefault ? HeaderFont : myGridTable.HeaderFont;
		// Hard coding in winform to add 6px
		var columnHeaderHeight = headerFontHeight + 6;
		// Another pixel is from here: https://github.com/dotnet/winforms/blob/6c74471e6d174da251e7c170647b0b0b35149b8e/src/System.Windows.Forms/src/System/Windows/Forms/DataGrid.cs#L5145
		var adjustWidth = TextRenderer.MeasureText(headerText, headerFont).Width + columnHeaderHeight + 1;
		var rowCount = listManager.Count;

		for (int row = 0; row < rowCount; ++row)
		{
			var value = column.GetColumnValueAtRow(listManager, row);
			var width = column.GetPreferredSize(CreateGraphicsInternal(), value).Width;
			if (width > adjustWidth)
			{
				adjustWidth = width;
			}
		}

		if (column.Width != adjustWidth)
		{
			column.Width = adjustWidth;
		}
	}

	void ColResizeBegin(MouseEventArgs e, int col)
	{
		gridState[GRIDSTATE_trackColResize] = true;
	}

	void ColResizeEnd(MouseEventArgs e)
	{
	}

	void RowAutoResize(int row)
	{
	}

	void RowResizeBegin(MouseEventArgs e, int row)
	{
		gridState[GRIDSTATE_trackRowResize] = true;
	}

	void RowResizeEnd(MouseEventArgs e)
	{
	}

	/// <summary>
	///  Fires the ColumnHeaderClicked event and handles column
	///  sorting.
	/// </summary>
	void ColumnHeaderClicked(PropertyDescriptor prop)
	{
		if (!CommitEdit())
		{
			return;
		}

		bool allowSorting;
		if (myGridTable.IsDefault)
		{
			allowSorting = AllowSorting;
		}
		else
		{
			allowSorting = myGridTable.AllowSorting;
		}

		if (!allowSorting)
		{
			return;
		}

		// if (CompModSwitches.DataGridCursor.OutputVerbose) Debug.WriteLine("DataGridCursor: We are about to sort column " + col.ToString());
		ListSortDirection direction = ListManager.GetSortDirection();
		PropertyDescriptor sortColumn = ListManager.GetSortProperty();
		if (sortColumn != null && sortColumn.Equals(prop))
		{
			direction = (direction == ListSortDirection.Ascending) ? ListSortDirection.Descending : ListSortDirection.Ascending;
		}
		else
		{
			// defaultSortDirection : ascending
			direction = ListSortDirection.Ascending;
		}

		if (listManager.Count == 0)
		{
			return;
		}

		ListManager.SetSort(prop, direction);
		ResetSelection();

		InvalidateInside();
	}

	protected internal virtual void UpdateRowNotification(int rowIndex, NotificationIcon icon)
	{
	}

	readonly DotNetObjectReference<DataGrid> dotNetObjectReference;

	protected override void Dispose(bool disposing)
	{
		dotNetObjectReference?.Dispose();
		currentHitTest = HitTestInfo.Nowhere;

		if (disposing)
		{
			if (vertScrollBar != null)
			{
				vertScrollBar.Dispose();
			}
			if (horizScrollBar != null)
			{
				horizScrollBar.Dispose();
			}
			if (toBeDisposedEditingControl != null)
			{
				toBeDisposedEditingControl.Dispose();
				toBeDisposedEditingControl = null;
			}

			DisposeDataGridRows(dataGridRows);
			NotificationIcon.Dispose();
			caption.Dispose();

			GridTableStylesCollection tableStyles = TableStyles;
			if (tableStyles != null)
			{
				for (int i = 0; i < tableStyles.Count; i++)
				{
					tableStyles[i].Dispose();
				}
			}
		}

		base.Dispose(disposing);
	}

	internal bool Initializing => inInit;
	bool inInit;

	/// <summary>
	///  Specifies the beginning of the initialization code.
	/// </summary>
	public void BeginInit()
	{
		if (inInit)
		{
			throw new InvalidOperationException(SR.DataGridBeginInit);
		}

		inInit = true;
	}

	/// <summary>
	///  Specifies the end of the initialization code.
	/// </summary>
	public void EndInit()
	{
		inInit = false;
		if (myGridTable == null && ListManager != null)
		{
			SetDataGridTable(TableStyles[ListManager.GetListName()], true);      // true for forcing column creation
		}
		if (myGridTable != null)
		{
			myGridTable.DataGrid = this;
		}
	}

	/// <summary>
	///  This method is called on methods that need the grid
	///  to be bound to a DataTable to work.
	/// </summary>
	void EnsureBound()
	{
		if (!Bound)
		{
			throw new InvalidOperationException(SR.DataGridUnbound);
		}
	}

	bool Bound => !(listManager == null || myGridTable == null);

	/// <summary>
	///  Sets the current GridTable for the DataGrid.
	///  This GridTable is the table which is currently
	///  being displayed on the grid.
	/// </summary>
	internal void SetDataGridTable(DataGridTableStyle newTable, bool forceColumnCreation)
	{
		// we have to listen to the dataGridTable for the propertyChangedEvent
		if (myGridTable != null)
		{
			// unwire the propertyChanged event
			UnWireTableStylePropChanged(myGridTable);

			if (myGridTable.IsDefault)
			{
				// reset the propertyDescriptors on the default table.
				myGridTable.GridColumnStyles.ResetPropertyDescriptors();

				// reset the relationship list from the default table
				myGridTable.ResetRelationsList();
			}
		}

		myGridTable = newTable;

		WireTableStylePropChanged(myGridTable);

		layout.RowHeadersVisible = newTable.IsDefault ? RowHeadersVisible : newTable.RowHeadersVisible;

		// we need to force the grid into the dataGridTableStyle
		// this way the controls in the columns will be parented
		// consider this scenario: when the user finished InitializeComponent, it added
		// a bunch of tables. all of those tables will have the DataGrid property set to this
		// grid. however, in InitializeComponent the tables will not have parented the
		// edit controls w/ the grid.

		// the code in DataGridTextBoxColumn already checks to see if the edits are parented
		// before parenting them.
		if (newTable != null)
		{
			newTable.DataGrid = this;
		}

		// pair the tableStyles and GridColumns
		if (listManager != null)
		{
			PairTableStylesAndGridColumns(listManager, myGridTable, forceColumnCreation);
		}

		// reset the relations UI on the newTable
		if (newTable != null)
		{
			newTable.ResetRelationsUI();
		}

		// set the gridState[GRIDSTATE_isNavigating] to false
		gridState[GRIDSTATE_isNavigating] = false;

		currentCol = 0;
		// if we add a tableStyle that mapps to the
		// current listName, then we should set the currentRow to the
		// position in the listManager
		if (listManager == null)
		{
			currentRow = 0;
		}
		else
		{
			currentRow = listManager.Position == -1 ? 0 : listManager.Position;
		}

		ResetUIState();
	}
	// SET myGridTable in SetDataGridTable ONLY
	internal DataGridTableStyle myGridTable;

	void WireTableStylePropChanged(DataGridTableStyle gridTable)
	{
		gridTable.GridLineColorChanged += new EventHandler(GridLineColorChanged);
		gridTable.GridLineStyleChanged += new EventHandler(GridLineStyleChanged);
		gridTable.HeaderBackColorChanged += new EventHandler(HeaderBackColorChanged);
		gridTable.HeaderFontChanged += new EventHandler(HeaderFontChanged);
		gridTable.HeaderForeColorChanged += new EventHandler(HeaderForeColorChanged);
		gridTable.LinkColorChanged += new EventHandler(LinkColorChanged);
		gridTable.LinkHoverColorChanged += new EventHandler(LinkHoverColorChanged);
		gridTable.PreferredColumnWidthChanged += new EventHandler(PreferredColumnWidthChanged);
		gridTable.RowHeadersVisibleChanged += new EventHandler(RowHeadersVisibleChanged);
		gridTable.ColumnHeadersVisibleChanged += new EventHandler(ColumnHeadersVisibleChanged);
		gridTable.RowHeaderWidthChanged += new EventHandler(RowHeaderWidthChanged);
		gridTable.AllowSortingChanged += new EventHandler(AllowSortingChanged);
	}

	void UnWireTableStylePropChanged(DataGridTableStyle gridTable)
	{
		gridTable.GridLineColorChanged -= new EventHandler(GridLineColorChanged);
		gridTable.GridLineStyleChanged -= new EventHandler(GridLineStyleChanged);
		gridTable.HeaderBackColorChanged -= new EventHandler(HeaderBackColorChanged);
		gridTable.HeaderFontChanged -= new EventHandler(HeaderFontChanged);
		gridTable.HeaderForeColorChanged -= new EventHandler(HeaderForeColorChanged);
		gridTable.LinkColorChanged -= new EventHandler(LinkColorChanged);
		gridTable.LinkHoverColorChanged -= new EventHandler(LinkHoverColorChanged);
		gridTable.PreferredColumnWidthChanged -= new EventHandler(PreferredColumnWidthChanged);
		gridTable.RowHeadersVisibleChanged -= new EventHandler(RowHeadersVisibleChanged);
		gridTable.ColumnHeadersVisibleChanged -= new EventHandler(ColumnHeadersVisibleChanged);
		gridTable.RowHeaderWidthChanged -= new EventHandler(RowHeaderWidthChanged);
		gridTable.AllowSortingChanged -= new EventHandler(AllowSortingChanged);
	}

	void GridLineColorChanged(object sender, EventArgs e)
	{
		Invalidate(layout.Data);
	}

	void GridLineStyleChanged(object sender, EventArgs e)
	{
		myGridTable.ResetRelationsUI();
		Invalidate(layout.Data);
	}

	void HeaderBackColorChanged(object sender, EventArgs e)
	{
		if (layout.RowHeadersVisible)
		{
			Invalidate(layout.RowHeaders);
		}

		if (layout.ColumnHeadersVisible)
		{
			Invalidate(layout.ColumnHeaders);
		}

		Invalidate(layout.TopLeftHeader);
	}

	void HeaderFontChanged(object sender, EventArgs e)
	{
		RecalculateFonts();
		PerformLayout();
		Invalidate(layout.Inside);
	}

	void HeaderForeColorChanged(object sender, EventArgs e)
	{
		if (layout.RowHeadersVisible)
		{
			Invalidate(layout.RowHeaders);
		}

		if (layout.ColumnHeadersVisible)
		{
			Invalidate(layout.ColumnHeaders);
		}

		Invalidate(layout.TopLeftHeader);
	}

	void LinkColorChanged(object sender, EventArgs e)
	{
		Invalidate(layout.Data);
	}

	void LinkHoverColorChanged(object sender, EventArgs e)
	{
		Invalidate(layout.Data);
	}

	void PreferredColumnWidthChanged(object sender, EventArgs e)
	{
		// reset the dataGridRows
		SetDataGridRows(null, DataGridRowsLength);
		// layout the horizontal scroll bar
		PerformLayout();
		// invalidate everything
		Invalidate();
	}

	void RowHeadersVisibleChanged(object sender, EventArgs e)
	{
		layout.RowHeadersVisible = myGridTable != null && myGridTable.RowHeadersVisible;
		PerformLayout();
		InvalidateInside();
	}

	void ColumnHeadersVisibleChanged(object sender, EventArgs e)
	{
		layout.ColumnHeadersVisible = myGridTable != null && myGridTable.ColumnHeadersVisible;
		PerformLayout();
		InvalidateInside();
	}

	void RowHeaderWidthChanged(object sender, EventArgs e)
	{
		if (layout.RowHeadersVisible)
		{
			PerformLayout();
			InvalidateInside();
		}
	}
	void AllowSortingChanged(object sender, EventArgs e)
	{
		if (!myGridTable.AllowSorting && listManager != null)
		{
			IList list = listManager.List;
			if (list is IBindingList)
			{
				((IBindingList)list).RemoveSort();
			}
		}
	}

	void ResetParentRows()
	{
		parentRows.Clear();
		originalState = null;
		caption.BackButtonActive = caption.DownButtonActive = caption.BackButtonVisible = false;
		caption.SetDownButtonDirection(!layout.ParentRowsVisible);
	}

	/// <summary>
	///  Re-initializes all UI related state.
	/// </summary>
	void ResetUIState()
	{
		ResetSelection();
		PerformLayout();
		NotifyRenderRequired();
	}

	internal static bool IsTransparentColor(Color color) => color.A < 255;

	/// <summary>
	///  Invalidates all the rows of a datagrid
	/// </summary>
	public new void Invalidate()
	{
		for (var i = 0; i < DataGridRows.Length; i++)
		{
			InvalidateRow(i);
		}
	}

	/// <summary>
	///  Invalidates the display region of a given DataGridColumn.
	/// </summary>
	internal void InvalidateColumn(int column)
	{
		NotifyRenderRequired();
	}

	/// <summary>
	///  Invalidates the scrollable area of the DataGrid.
	/// </summary>
	internal void InvalidateInside()
	{
		NotifyRenderRequired();
	}

	/// <summary>
	///  Invalidate the painting region for the row specified.
	/// </summary>
	protected internal void InvalidateRow(int rowNumber)
	{
		using var activity = TelemetryService.ActivitySource.StartActivity($"{GetType().Name}.Set{nameof(InvalidateRow)}");
		if (rowNumber >= 0 && rowNumber < ListManager.Count)
		{
			var row = dataGridRows[rowNumber];
			if (!DuringBeforeRender)
			{
				row.RenderedContentCreated = false;
			}
			row.Invalidate();
		}
	}

	void InvalidateRowHeader(int rowNumber)
	{
		if (IsRowVisible(rowNumber))
		{
			var row = dataGridRows[rowNumber];
			row.Invalidate();
		}
	}

	internal void RecalculateFonts()
	{
		try
		{
			linkFont = new Font(Font, FontStyle.Underline);
		}
		catch
		{
		}
		fontHeight = FontHeight;
		linkFontHeight = LinkFont.GetFontHeight();
		captionFontHeight = CaptionFont.GetFontHeight();

		if (myGridTable == null || myGridTable.IsDefault)
		{
			headerFontHeight = HeaderFont.GetFontHeight();
		}
		else
		{
			headerFontHeight = myGridTable.HeaderFont.GetFontHeight();
		}
	}
	int captionFontHeight = -1;
	int headerFontHeight = -1;

	internal new int FontHeight => fontHeight;
	int fontHeight = -1;

	/// <summary>
	///  Indicates whether the <see cref='LinkHoverColor'/> property should be
	///  persisted.
	/// </summary>
	internal Font LinkFont => linkFont;
	Font linkFont;

	internal int LinkFontHeight => linkFontHeight;
	int linkFontHeight = -1;

	/// <summary>
	///  Gets or sets the line style of the grid.
	/// </summary>
	public DataGridLineStyle GridLineStyle
	{
		get => gridLineStyle;
		set
		{
			//valid values are 0x0 to 0x1.
			if (!ClientUtils.IsEnumValid(value, (int)value, (int)DataGridLineStyle.None, (int)DataGridLineStyle.Solid))
			{
				throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(DataGridLineStyle));
			}
			if (UpdateProperty(ref gridLineStyle, value))
			{
				myGridTable.ResetRelationsUI();
			}
		}
	}
	DataGridLineStyle gridLineStyle = defaultGridLineStyle;
	const DataGridLineStyle defaultGridLineStyle = DataGridLineStyle.Solid;

	internal int GridLineWidth
	{
		get
		{
			return GridLineStyle == DataGridLineStyle.Solid ? 1 : 0;
		}
	}

	/// <summary>
	///  Constructs an updated Layout object.
	/// </summary>
	void ComputeLayout()
	{
		bool alignLeft = !isRightToLeft();
		Rectangle oldResizeRect = layout.ResizeBoxRect;

		// hide the EditBox
		EndEdit();

		LayoutData newLayout = new LayoutData(layout);

		bool newRowHeadersVisible = myGridTable.IsDefault ? RowHeadersVisible : myGridTable.RowHeadersVisible;
		int newRowHeaderWidth = myGridTable.IsDefault ? RowHeaderWidth : myGridTable.RowHeaderWidth;
		newLayout.RowHeadersVisible = newRowHeadersVisible;

		layout = newLayout;

		layout.dirty = false;
		Debug.WriteLineIf(CompModSwitches.DataGridLayout.TraceVerbose, "DataGridLayout: " + layout.ToString());

		ComputeVisibleRows();
		ComputeVisibleColumns();
	}

	public int HeaderHeight => GridLayoutConfigurable ? 19 : 17;

	int tableHeight;

	void ComputeVisibleRows()
	{
		var height = HeaderHeight;
		var localGridRows = DataGridRows;
		var gridRowsLength = DataGridRowsLength;

		tableHeight = height + localGridRows.Sum(row => row.Height);

		if (HorizScrollBar.Visible)
		{
			height += SystemInformation.HorizontalScrollBarHeight;
		}

		var newVisibleRowCount = 0;
		if (gridRowsLength > firstVisibleRow && height < Height)
		{
			var heightToFirstVisibleRow = 0;
			for (var i = 0; i < firstVisibleRow; i++)
			{
				heightToFirstVisibleRow += localGridRows[i].Height;
			}
			height += localGridRows[firstVisibleRow].Height - (currentScrollTop - heightToFirstVisibleRow);
			newVisibleRowCount++;

			for (var i = firstVisibleRow + 1; i < gridRowsLength; i++)
			{
				var row = localGridRows[i];
				if (height < Height)
				{
					newVisibleRowCount++;
					height += row.Height;
				}
				else
				{
					break;
				}
			}
		}

		UpdateProperty(ref visibleRowCount, newVisibleRowCount);
		OnAfterSetVisibleRowCount();
	}

	protected void OnVerticalScroll(float scrollTop)
	{
		gridState[GRIDSTATE_isScrolling] = true;

		currentScrollTop = (int)Math.Ceiling(scrollTop);
		vertScrollBar.Value = currentScrollTop;
		var first = ComputeFirstVisibleRow(currentScrollTop);
		FirstVisibleRow = first;
		ComputeVisibleRows();

		gridState[GRIDSTATE_isScrolling] = false;
	}

	int ComputeFirstVisibleRow(int scrollTop)
	{
		var localGridRows = DataGridRows;
		var nRows = localGridRows.Length;
		int first;
		var height = 0;
		for (first = 0; first < nRows; first++)
		{
			height += localGridRows[first].Height;
			if (height > scrollTop)
			{
				break;
			}
		}
		return first;
	}

	protected void OnHorizontalScroll(float scrollLeft)
	{
		gridState[GRIDSTATE_isScrolling] = true;

		currentScrollLeft = (int)Math.Ceiling(scrollLeft);
		horizScrollBar.Value = currentScrollLeft;
		firstVisibleCol = ComputeFirstVisibleColumn();
		ComputeVisibleColumns();

		gridState[GRIDSTATE_isScrolling] = false;
	}

	int ComputeFirstVisibleColumn(int scrollLeft = 0)
	{
		var curCol = 0;
		if (currentScrollLeft == 0)
		{
			negOffset = 0;
			return 0;
		}

		if (myGridTable != null && myGridTable.GridColumnStyles != null && myGridTable.GridColumnStyles.Count > 0)
		{
			var columns = myGridTable.GridColumnStyles;
			if (columns[0].Width == -1)
			{
				// the columns are not initialized yet
				negOffset = 0;
				return 0;
			}

			var nColumns = columns.Count;
			var width = 0;
			for (; curCol < nColumns; curCol++)
			{
				if (columns[curCol].PropertyDescriptor != null)
				{
					width += columns[curCol].Width;
				}

				if (width > currentScrollLeft)
				{
					break;
				}
			}

			if (curCol == nColumns)
			{
				negOffset = 0;
				return 0;
			}

			negOffset = columns[curCol].Width - (width - currentScrollLeft);
		}
		return curCol;
	}

	void ComputeVisibleColumns()
	{
		var columns = myGridTable.GridColumnStyles;
		var width = -negOffset;
		var newVisibleColumnCount = 0;
		var visibleWidth = Width - HeaderWidth - (VertScrollBar.Visible ? SystemInformation.VerticalScrollBarWidth : 0);
		var curCol = firstVisibleCol;

		if (visibleWidth < 0 || columns.Count == 0)
		{
			numVisibleCols = firstVisibleCol = 0;
			lastTotallyVisibleCol = -1;
			return;
		}

		while (width < visibleWidth && curCol < columns.Count)
		{
			if (columns[curCol].PropertyDescriptor != null)
			{
				width += columns[curCol].Width;
			}

			curCol++;
			newVisibleColumnCount++;
		}

		numVisibleCols = newVisibleColumnCount;

		lastTotallyVisibleCol = firstVisibleCol + numVisibleCols - 1;
		if (width > visibleWidth)
		{
			if (numVisibleCols <= 1 || (numVisibleCols == 2 && negOffset != 0))
			{
				// no column is entirely visible
				lastTotallyVisibleCol = -1;
			}
			else
			{
				lastTotallyVisibleCol--;
			}
		}
	}

	int currentScrollTop;
	int currentScrollLeft;

	internal void OnColumnCollectionChanged(object sender, CollectionChangeEventArgs e)
	{
		DataGridTableStyle table = (DataGridTableStyle)sender;
		if (table.Equals(myGridTable))
		{
			// if we changed the column collection, then we need to set the property
			// descriptors in the column collection.
			// unless the user set the propertyDescriptor in the columnCollection
			if (!myGridTable.IsDefault)
			{
				// if the element in the collectionChangeEventArgs is not null
				// and the action is refresh, then it means that the user
				// set the propDesc. we do not want to override this.
				if (e.Action != CollectionChangeAction.Refresh || e.Element == null)
				{
					PairTableStylesAndGridColumns(listManager, myGridTable, false);
				}
			}
			Invalidate();
			PerformLayout();
		}
	}

	void PairTableStylesAndGridColumns(CurrencyManager lm, DataGridTableStyle gridTable, bool forceColumnCreation)
	{
		PropertyDescriptorCollection props = lm.GetItemProperties();
		GridColumnStylesCollection gridCols = gridTable.GridColumnStyles;

		// ]it is possible to have a dataTable w/ an empty string for a name.
		if (!gridTable.IsDefault && string.Compare(lm.GetListName(), gridTable.MappingName, true, CultureInfo.InvariantCulture) == 0)
		{
			// we will force column creation only at runtime
			if (gridTable.GridColumnStyles.Count == 0 && !DesignMode)
			{
				// we have to create some default columns for each of the propertyDescriptors
				//
				if (forceColumnCreation)
				{
					gridTable.SetGridColumnStylesCollection(lm);
				}
				else
				{
					gridTable.SetRelationsList(lm);
				}
			}
			else
			{
				// it may the case that the user will have two lists w/ the same name.
				// When switching binding between those different lists, we need to invalidate
				// the propertyDescriptors from the current gridColumns
				for (int i = 0; i < gridCols.Count; i++)
				{
					gridCols[i].PropertyDescriptor = null;
				}

				// pair the propertyDescriptor from each column to the actual property descriptor
				// from the listManager
				for (int i = 0; i < props.Count; i++)
				{
					DataGridColumnStyle col = gridCols.MapColumnStyleToPropertyName(props[i].Name);
					if (col != null)
					{
						col.PropertyDescriptor = props[i];
					}
				}
				// TableStyle::SetGridColumnStylesCollection will also set the
				// relations list in the tableStyle.
				gridTable.SetRelationsList(lm);
			}
		}
		else
		{
			gridTable.SetGridColumnStylesCollection(lm);
			if (gridTable.GridColumnStyles.Count > 0 && gridTable.GridColumnStyles[0].Width == -1)
			{
				InitializeColumnWidths();
			}
		}
	}

	/// <summary>
	///  Initializes the values for column widths in the table.
	/// </summary>
	void InitializeColumnWidths()
	{
		if (myGridTable == null)
		{
			return;
		}

		GridColumnStylesCollection columns = myGridTable.GridColumnStyles;
		int numCols = columns.Count;

		// Resize the columns to a approximation of a best fit.
		// We find the best fit width of NumRowsForAutoResize rows
		// and use it for each column.
		int preferredColumnWidth = myGridTable.IsDefault ? PreferredColumnWidth : myGridTable.PreferredColumnWidth;
		// if we set the PreferredColumnWidth to something else than AutoColumnSize
		// then use that value
		for (int col = 0; col < numCols; col++)
		{
			// if the column width is not -1, then this column was initialized already
			if (columns[col].width != -1)
			{
				continue;
			}

			columns[col].width = preferredColumnWidth;
		}
	}

	bool ListHasErrors
	{
		get => gridState[GRIDSTATE_listHasErrors];
		set
		{
			if (ListHasErrors != value)
			{
				gridState[GRIDSTATE_listHasErrors] = value;
				ComputeMinimumRowHeaderWidth();
				if (!layout.RowHeadersVisible)
				{
					return;
				}

				if (value)
				{
					if (myGridTable.IsDefault)
					{
						RowHeaderWidth += errorRowBitmapWidth;
					}
					else
					{
						myGridTable.RowHeaderWidth += errorRowBitmapWidth;
					}
				}
				else
				{
					if (myGridTable.IsDefault)
					{
						RowHeaderWidth -= errorRowBitmapWidth;
					}
					else
					{
						myGridTable.RowHeaderWidth -= errorRowBitmapWidth;
					}
				}
			}
		}
	}

	void TableStylesCollectionChanged(object sender, CollectionChangeEventArgs ccea)
	{
		// if the users changed the collection of tableStyles
		if (sender != dataGridTables)
		{
			return;
		}

		if (listManager == null)
		{
			return;
		}

		if (ccea.Action == CollectionChangeAction.Add)
		{
			DataGridTableStyle tableStyle = (DataGridTableStyle)ccea.Element;
			if (listManager.GetListName().Equals(tableStyle.MappingName))
			{
				SetDataGridTable(tableStyle, true);                // true for forcing column creation
				SetDataGridRows(null, 0);
			}
		}
		else if (ccea.Action == CollectionChangeAction.Remove)
		{
			DataGridTableStyle tableStyle = (DataGridTableStyle)ccea.Element;
			if (myGridTable.MappingName.Equals(tableStyle.MappingName))
			{
				defaultTableStyle.GridColumnStyles.ResetDefaultColumnCollection();
				SetDataGridTable(defaultTableStyle, true);    // true for forcing column creation
				SetDataGridRows(null, 0);
			}
		}
		else
		{
			// we have to search to see if the collection of table styles contains one
			// w/ the same name as the list in the dataGrid

			DataGridTableStyle newGridTable = dataGridTables[listManager.GetListName()];
			if (newGridTable == null)
			{
				if (!myGridTable.IsDefault)
				{
					// get rid of the old gridColumns
					defaultTableStyle.GridColumnStyles.ResetDefaultColumnCollection();
					SetDataGridTable(defaultTableStyle, true);    // true for forcing column creation
					SetDataGridRows(null, 0);
				}
			}
			else
			{
				SetDataGridTable(newGridTable, true);              // true for forcing column creation
				SetDataGridRows(null, 0);
			}
		}
	}
	readonly CollectionChangeEventHandler dataGridTableStylesCollectionChanged;

	// PERF: we attempt to create a ListManager for the DataSource/DateMember combination
	// we do this in order to check for a valid DataMember
	// if the check succeeds, then it means that we actully put the listManager in the BindingContext's
	// list of BindingManagers. this is fine, cause if the check succeds, then Set_ListManager
	// will be called, and this will get the listManager from the bindingManagerBase hashTable kept in the BindingContext

	// this will work if the dataMember does not contain any dots ('.')
	// if the dataMember contains dots, then it will be more complicated: maybe part of the binding path
	// is valid w/ the new dataSource
	// but we can leave w/ this, cause in the designer the dataMember will be only a column name. and the DataSource/DataMember
	// properties are for use w/ the designer.
	void EnforceValidDataMember(object value)
	{
		if (DataMember == null || DataMember.Length == 0)
		{
			return;
		}

		if (BindingContext == null)
		{
			return;
		}
		try
		{
			BindingManagerBase bm = BindingContext[value, dataMember];
		}
		catch
		{
			dataMember = string.Empty;
		}
	}

	void EnsureVisible(int row, int col)
	{
		using var activity = TelemetryService.ActivitySource.StartActivity($"{GetType().Name}.{nameof(EnsureVisible)}");
		ScrollDown(row);

		int previousFirstVisibleCol = firstVisibleCol;
		int previousNegOffset = negOffset;
		int previousLastTotallyVisibleCol = lastTotallyVisibleCol;

		while (col < firstVisibleCol
			|| col == firstVisibleCol && negOffset != 0
			|| lastTotallyVisibleCol == -1 && col > firstVisibleCol
			|| lastTotallyVisibleCol > -1 && col > lastTotallyVisibleCol)
		{
			ScrollToColumn(col);

			if (previousFirstVisibleCol == firstVisibleCol &&
				previousNegOffset == negOffset &&
				previousLastTotallyVisibleCol == lastTotallyVisibleCol)
			{
				// nothing changed since the last iteration
				// don't get into an infinite loop
				break;
			}

			previousFirstVisibleCol = firstVisibleCol;
			previousNegOffset = negOffset;
			previousLastTotallyVisibleCol = lastTotallyVisibleCol;

			// continue to scroll to the right until the scrollTo column is the totally last visible column or it is the first visible column
		}
	}

	Rectangle GetRowRect(int rowNumber)
	{
		Rectangle inside = layout.Data;
		int offsetY = inside.Y;
		DataGridRow[] localGridRows = DataGridRows;
		for (int row = firstVisibleRow; row <= rowNumber; ++row)
		{
			if (offsetY > inside.Bottom)
			{
				break;
			}
			if (row == rowNumber)
			{
				Rectangle rowRect = new Rectangle(inside.X,
												  offsetY,
												  inside.Width,
												  localGridRows[row].Height);
				if (layout.RowHeadersVisible)
				{
					rowRect.Width += layout.RowHeaders.Width;
					rowRect.X -= isRightToLeft() ? 0 : layout.RowHeaders.Width;
				}
				return rowRect;
			}
			offsetY += localGridRows[row].Height;
		}
		return Rectangle.Empty;
	}

	bool IsEntireRowVisible(int targetRow)
	{
		bool result = false;
		if (firstVisibleRow <= targetRow)
		{
			var offsetY = HeaderHeight + (HorizScrollBar.Visible ? SystemInformation.HorizontalScrollBarHeight : 0);
			if (DataGridRows.Length > firstVisibleRow && offsetY < Height)
			{
				var heightToFirstVisibleRow = DataGridRows.Take(firstVisibleRow).Sum(row => row.Height);
				offsetY += DataGridRows[firstVisibleRow].Height - (currentScrollTop - heightToFirstVisibleRow);

				for (var i = firstVisibleRow + 1; i <= targetRow; ++i)
				{
					offsetY += DataGridRows[i].Height;
					if (offsetY > Height)
					{
						return result;
					}
				}
				result = offsetY <= Height;
			}
		}
		return result;
	}

	void ScrollDown(int targetRow)
	{
		if (IsEntireRowVisible(targetRow) || DataGridRows.Length == 0 || targetRow >= DataGridRows.Length)
		{
			return;
		}
		var localGridRows = DataGridRows;

		var scrollTop = 0;
		for (var row = 0; row < targetRow; ++row)
		{
			scrollTop += localGridRows[row].Height;
		}
		var newFirstVisibleRow = targetRow;
		var totalHeight = Height - HeaderHeight;
		if (HorizScrollBar.Visible)
		{
			totalHeight -= SystemInformation.HorizontalScrollBarHeight;
		}

		if (targetRow > FirstVisibleRow)
		{
			var offset = localGridRows[targetRow].Height;
			for (var row = targetRow - 1; row > -1; --row)
			{
				offset += localGridRows[row].Height;
				newFirstVisibleRow = row;
				if (offset >= totalHeight)
				{
					break;
				}
				scrollTop -= localGridRows[row].Height;
			}
		}

		FirstVisibleRow = newFirstVisibleRow;

		var wasEditing = gridState[GRIDSTATE_isEditing];
		currentScrollTop = scrollTop;
		ComputeVisibleRows();

		if (gridState[GRIDSTATE_isScrolling])
		{
			Edit();
			// isScrolling is set to TRUE when the user scrolls.
			// once we move the edit box, we finished processing the scroll event, so set isScrolling to FALSE
			// to set isScrolling to TRUE, we need another scroll event.
			gridState[GRIDSTATE_isScrolling] = false;
		}
		else
		{
			EndEdit();
		}

		RegisterAfterRenderAction(async () => await (GetJSInterop<IGridJSInterop>()?.SetScrollTopAsync(ElementReference, scrollTop) ?? Task.CompletedTask));

		if (wasEditing)
		{
			// invalidate the rowHeader for the
			InvalidateRowHeader(currentRow);
		}
	}

	void ScrollToColumn(int targetCol)
	{
		var colsToScroll = targetCol - firstVisibleCol;

		if (targetCol > lastTotallyVisibleCol && lastTotallyVisibleCol != -1)
		{
			colsToScroll = targetCol - lastTotallyVisibleCol;
		}

		// if only part of the currentCol is visible, then we should still scroll
		if (colsToScroll != 0 || negOffset != 0)
		{
			ScrollRight(colsToScroll);
		}
	}

	void ScrollRight(int columns)
	{
		var newCol = firstVisibleCol + columns;

		var gridColumns = myGridTable.GridColumnStyles;
		var gridColumnsCount = gridColumns.Count;
		var visibleColumnsCount = 0;

		for (var i = 0; i < gridColumnsCount; i++)
		{
			if (gridColumns[i].PropertyDescriptor != null)
			{
				visibleColumnsCount++;
			}
		}

		if (lastTotallyVisibleCol == visibleColumnsCount - 1 && columns > 0 ||
			firstVisibleCol == 0 && columns < 0 && negOffset == 0)
		{
			return;
		}

		var newColOffset = 0;
		newCol = Math.Min(newCol, gridColumnsCount - 1);
		for (var i = 0; i < newCol; i++)
		{
			if (gridColumns[i].PropertyDescriptor != null)
			{
				newColOffset += gridColumns[i].Width;
			}
		}

		HorizontalOffset = newColOffset;
	}

	protected virtual void GridHScrolled(object sender, ScrollEventArgs se)
	{
		if (!Enabled)
		{
			return;
		}
		if (DataSource == null)
		{
			return;
		}

		gridState[GRIDSTATE_isScrolling] = true;

		RegisterAfterRenderAction(async () => await (GetJSInterop<IGridJSInterop>()?.SetScrollLeftAsync(ElementReference, se.NewValue) ?? Task.CompletedTask));

		gridState[GRIDSTATE_isScrolling] = false;
	}

	int GetColumnWidthSum()
	{
		var sum = 0;
		if (myGridTable != null && myGridTable.GridColumnStyles != null)
		{
			var columns = myGridTable?.GridColumnStyles;
			sum = columns?.Cast<DataGridColumnStyle>().Sum(column => column.PropertyDescriptor != null ? column.Width : 0) ?? 0;
		}
		return sum;
	}

	internal void OnRowHeightChanged(DataGridRow row)
	{
		NotifyRenderRequired();
	}

	LayoutData layout = new LayoutData();

	readonly Policy policy = new Policy();

	internal DataGridCaption Caption => caption;
	readonly DataGridCaption caption;

	readonly DataGridTableStyle defaultTableStyle = new DataGridTableStyle(true);

	readonly DataGridParentRows parentRows;
	const bool defaultParentRowsVisible = true;

	public int FirstVisibleRow
	{
		get
		{
			return firstVisibleRow;
		}
		set
		{
			UpdateProperty(ref firstVisibleRow, value);
		}
	}

	internal int firstVisibleRow;
	internal int firstVisibleCol;

	// the width in pixels of the firstVisibleColumn which are not visible
	int negOffset;

	int lastTotallyVisibleCol;

	internal bool IsRowVisible(int rowNumber) => rowNumber >= firstVisibleRow && rowNumber < firstVisibleRow + visibleRowCount;

	internal bool IsEditing => gridState[GRIDSTATE_isEditing];

	internal bool DuringBeforeRender;

	// Set_ListManager uses the originalState to determine
	// if the grid should disconnect from all the MetaDataChangedEvents
	// keep "originalState != null" when navigating back and forth in the grid
	// and use Add/RemoveMetaDataChanged methods.
	DataGridState originalState;

	const int GRIDSTATE_allowSorting = 0x00000001;
	const int GRIDSTATE_columnHeadersVisible = 0x00000002;
	const int GRIDSTATE_rowHeadersVisible = 0x00000004;
	const int GRIDSTATE_trackColResize = 0x00000008;
	const int GRIDSTATE_trackRowResize = 0x00000010;
	const int GRIDSTATE_isLedgerStyle = 0x00000020;
	const int GRIDSTATE_isFlatMode = 0x00000040;
	const int GRIDSTATE_listHasErrors = 0x00000080;
	const int GRIDSTATE_dragging = 0x00000100;
	const int GRIDSTATE_inListAddNew = 0x00000200;
	const int GRIDSTATE_inDeleteRow = 0x00000400;
	const int GRIDSTATE_canFocus = 0x00000800;
	const int GRIDSTATE_readOnlyMode = 0x00001000;
	const int GRIDSTATE_allowNavigation = 0x00002000;
	const int GRIDSTATE_isNavigating = 0x00004000;
	const int GRIDSTATE_isEditing = 0x00008000;
	const int GRIDSTATE_editControlChanging = 0x00010000;
	const int GRIDSTATE_isScrolling = 0x00020000;
	const int GRIDSTATE_overCaption = 0x00040000;
	const int GRIDSTATE_childLinkFocused = 0x00080000;
	const int GRIDSTATE_inAddNewRow = 0x00100000;
	const int GRIDSTATE_inSetListManager = 0x00200000;
	const int GRIDSTATE_metaDataChanged = 0x00400000;
	const int GRIDSTATE_exceptionInPaint = 0x00800000;
	const int GRIDSTATE_layoutSuspended = 0x01000000;

	// PERF: take all the bools and put them into a state variable
	Collections.Specialized.BitVector32 gridState;                  // see GRIDSTATE_ consts above

	bool isRightToLeft() => RightToLeft == RightToLeft.Yes;

	protected internal bool IsClickingTopLeft => currentHitTest != null && (currentHitTest?.Type & HitTestType.RowHeader) != 0 && (currentHitTest?.Type & HitTestType.ColumnHeader) != 0;

	// for toolTip
	int toolTipId;
	DataGridToolTip toolTipProvider;

	// ToolTipping
	internal DataGridToolTip ToolTipProvider
	{
		get
		{
			return toolTipProvider;
		}
	}

	protected internal override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();
		GetJSInterop<IGridJSInterop>()?.PreloadInterop();
	}

	protected internal override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			await (GetJSInterop<IGridJSInterop>()?.InitializeGridEventsAsync(ElementReference, dotNetObjectReference) ?? Task.CompletedTask);
			if (horizScrollBar.Value > 0 || vertScrollBar.Value > 0)
			{
				await ElementReference.ScrollToAsync(JSRuntime, horizScrollBar.Value, vertScrollBar.Value);
			}
		}

		await base.OnAfterRenderAsync(firstRender);
	}
}
