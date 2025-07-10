using System.Collections;
using System.ComponentModel;
using System.Drawing;
using WinzorFramework.Extensions;

#nullable disable

namespace System.Windows.Forms;

public class DataGridTableStyle
{
	/// <summary>
	///  Initializes a new instance of the <see cref='DataGridTableStyle'/> class.
	/// </summary>
	public DataGridTableStyle(bool isDefaultTableStyle)
	{
		gridColumns = new GridColumnStylesCollection(this, isDefaultTableStyle);
		gridColumns.CollectionChanged += new CollectionChangeEventHandler(OnColumnCollectionChanged);
		this.isDefaultTableStyle = isDefaultTableStyle;
	}

	public DataGridTableStyle() : this(false)
	{
	}

	/// <summary>
	///  Gets the name of this grid table.
	/// </summary>
	public string MappingName
	{
		get
		{
			return mappingName;
		}
		set
		{
			if (value == null)
			{
				value = string.Empty;
			}

			if (value.Equals(mappingName))
			{
				return;
			}

			string originalMappingName = MappingName;
			mappingName = value;

			// this could throw
			try
			{
				if (DataGrid != null)
				{
					DataGrid.TableStyles.CheckForMappingNameDuplicates(this);
				}
			}
			catch
			{
				mappingName = originalMappingName;
				throw;
			}
			OnMappingNameChanged(EventArgs.Empty);
		}
	}
	string mappingName = string.Empty;

	protected virtual void OnMappingNameChanged(EventArgs e)
	{
		MappingNameChanged?.Invoke(this, e);
	}
	public event EventHandler MappingNameChanged;

	public bool AllowSorting
	{
		get
		{
			return allowSorting;
		}
		set
		{
			if (isDefaultTableStyle)
			{
				throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, nameof(AllowSorting)));
			}

			if (allowSorting != value)
			{
				allowSorting = value;
				OnAllowSortingChanged(EventArgs.Empty);
			}
		}
	}
	bool allowSorting = defaultAllowSorting;
	const bool defaultAllowSorting = true;

	protected virtual void OnAllowSortingChanged(EventArgs e)
	{
		AllowSortingChanged?.Invoke(this, e);
	}
	public event EventHandler AllowSortingChanged;

	public Color HeaderBackColor
	{
		get
		{
			return headerBackColor;
		}
		set
		{
			if (isDefaultTableStyle)
			{
				throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, nameof(HeaderBackColor)));
			}

			if (DataGrid.IsTransparentColor(value))
			{
				throw new ArgumentException(SR.DataGridTableStyleTransparentHeaderBackColorNotAllowed, nameof(value));
			}

			if (value.IsEmpty)
			{
				throw new ArgumentException(string.Format(SR.DataGridEmptyColor, nameof(HeaderBackColor)), nameof(value));
			}

			if (!value.Equals(headerBackColor))
			{
				headerBackColor = value;
				OnHeaderBackColorChanged(EventArgs.Empty);
			}
		}
	}
	Color headerBackColor = defaultHeaderBackColor;
	static Color defaultHeaderBackColor => SystemColors.Control;

	protected virtual void OnHeaderBackColorChanged(EventArgs e)
	{
		HeaderBackColorChanged?.Invoke(this, e);
	}
	public event EventHandler HeaderBackColorChanged;

	public Color HeaderForeColor
	{
		get => headerForeColor;
		set
		{
			if (isDefaultTableStyle)
			{
				throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, nameof(HeaderForeColor)));
			}

			if (value.IsEmpty)
			{
				throw new ArgumentException(string.Format(SR.DataGridEmptyColor, nameof(HeaderForeColor)), nameof(value));
			}

			if (!value.Equals(headerForeColor))
			{
				headerForeColor = value;
				OnHeaderForeColorChanged(EventArgs.Empty);
			}
		}
	}
	Color headerForeColor = defaultHeaderForeColor;
	static Color defaultHeaderForeColor => SystemColors.ControlText;

	protected virtual void OnHeaderForeColorChanged(EventArgs e)
	{
		HeaderForeColorChanged?.Invoke(this, e);
	}
	public event EventHandler HeaderForeColorChanged;

	public Font HeaderFont
	{
		get => (headerFont ?? (DataGrid == null ? Control.DefaultFont : DataGrid.Font));
		set
		{
			if (isDefaultTableStyle)
			{
				throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, nameof(HeaderFont)));
			}

			if (value == null && headerFont != null || (value != null && !value.Equals(headerFont)))
			{
				headerFont = value;
				OnHeaderFontChanged(EventArgs.Empty);
			}
		}
	}
	internal Font headerFont; // this is ambient property to Font value.

	protected virtual void OnHeaderFontChanged(EventArgs e)
	{
		HeaderFontChanged?.Invoke(this, e);
	}
	public event EventHandler HeaderFontChanged;

	public int RowHeaderWidth
	{
		get => rowHeaderWidth;
		set
		{
			if (DataGrid != null)
			{
				value = Math.Max(DataGrid.MinimumRowHeaderWidth(), value);
			}

			if (rowHeaderWidth != value)
			{
				rowHeaderWidth = value;
				OnRowHeaderWidthChanged(EventArgs.Empty);
			}
		}
	}
	int rowHeaderWidth = defaultRowHeaderWidth;
	const int defaultRowHeaderWidth = 35;

	protected virtual void OnRowHeaderWidthChanged(EventArgs e)
	{
		RowHeaderWidthChanged?.Invoke(this, e);
	}
	public event EventHandler RowHeaderWidthChanged;

	/// <summary>
	///  Gets or sets a value indicating whether the data in the column cannot be edited.
	/// </summary>
	[DefaultValue(false)]
	public virtual bool ReadOnly
	{
		get => readOnly;
		set
		{
			if (readOnly != value)
			{
				readOnly = value;
				OnReadOnlyChanged(EventArgs.Empty);
			}
		}
	}
	bool readOnly;

	void OnReadOnlyChanged(EventArgs e)
	{
		ReadOnlyChanged?.Invoke(this, e);
	}
	public event EventHandler ReadOnlyChanged;

	public int PreferredColumnWidth
	{
		get
		{
			return preferredColumnWidth;
		}
		set
		{
			if (isDefaultTableStyle)
			{
				throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, nameof(PreferredColumnWidth)));
			}

			if (value < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, SR.DataGridColumnWidth);
			}

			if (preferredColumnWidth != value)
			{
				preferredColumnWidth = value;
				OnPreferredColumnWidthChanged(EventArgs.Empty);
			}
		}
	}
	internal int preferredColumnWidth = defaultPreferredColumnWidth;
	const int defaultPreferredColumnWidth = 75;

	protected virtual void OnPreferredColumnWidthChanged(EventArgs e)
	{
		PreferredColumnWidthChanged.Invoke(this, e);
	}
	public event EventHandler PreferredColumnWidthChanged;

	public bool RowHeadersVisible
	{
		get
		{
			return rowHeadersVisible;
		}
		set
		{
			if (rowHeadersVisible != value)
			{
				rowHeadersVisible = value;
				OnRowHeadersVisibleChanged(EventArgs.Empty);
			}
		}
	}
	bool rowHeadersVisible = true;

	protected virtual void OnRowHeadersVisibleChanged(EventArgs e)
	{
		RowHeadersVisibleChanged?.Invoke(this, e);
	}
	public event EventHandler RowHeadersVisibleChanged;

	public bool ColumnHeadersVisible
	{
		get
		{
			return columnHeadersVisible;
		}
		set
		{
			if (columnHeadersVisible != value)
			{
				columnHeadersVisible = value;
				OnColumnHeadersVisibleChanged(EventArgs.Empty);
			}
		}
	}
	bool columnHeadersVisible = true;

	protected virtual void OnColumnHeadersVisibleChanged(EventArgs e)
	{
		ColumnHeadersVisibleChanged?.Invoke(this, e);
	}
	public event EventHandler ColumnHeadersVisibleChanged;

	public int PreferredRowHeight
	{
		get => preferredRowHeight;
		set
		{
			if (isDefaultTableStyle)
			{
				throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, nameof(PreferredRowHeight)));
			}

			if (value < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, SR.DataGridRowRowHeight);
			}

			if (preferredRowHeight != value)
			{
				preferredRowHeight = value;
				OnPreferredRowHeightChanged(EventArgs.Empty);
			}
		}
	}
	int preferredRowHeight = defaultFontHeight + 3;
	internal static int defaultFontHeight => defaultFont.GetFontHeight();
	internal static Font defaultFont => Control.DefaultFont;

	protected virtual void OnPreferredRowHeightChanged(EventArgs e)
	{
		PreferredRowHeightChanged?.Invoke(this, e);
	}
	public event EventHandler PreferredRowHeightChanged;

	public event EventHandler GridLineColorChanged;

	public event EventHandler LinkColorChanged;

	public event EventHandler LinkHoverColorChanged;

	internal int FocusedRelation
	{
		get => focusedRelation;
		set
		{
			if (focusedRelation != value)
			{
				focusedRelation = value;
				if (focusedRelation == -1)
				{
					focusedTextWidth = 0;
				}
				else
				{
					var g = DataGrid.CreateGraphicsInternal();
					focusedTextWidth = (int)Math.Ceiling(g.MeasureString(((string)RelationsList[focusedRelation]), DataGrid.LinkFont).Width);
					g.Dispose();
				}
			}
		}
	}
	int focusedRelation = -1;

	internal int FocusedTextWidth => focusedTextWidth;
	int focusedTextWidth;

	public DataGrid DataGrid
	{
		get => dataGrid;
		set => SetInternalDataGrid(value, true);
	}
	DataGrid dataGrid;

	/// <summary>
	///  Gets or sets the <see cref='Forms.DataGrid'/>
	///  control displaying the table.
	/// </summary>
	internal void SetInternalDataGrid(DataGrid dG, bool force)
	{
		if (dataGrid != null && dataGrid.Equals(dG) && !force)
		{
			return;
		}
		else
		{
			dataGrid = dG;
			if (dG != null && dG.Initializing)
			{
				return;
			}

			int nCols = gridColumns.Count;
			for (int i = 0; i < nCols; i++)
			{
				gridColumns[i].SetDataGridInternalInColumn(dG);
			}
		}
	}

	internal bool IsDefault => isDefaultTableStyle;
	readonly bool isDefaultTableStyle;

	public virtual GridColumnStylesCollection GridColumnStyles => gridColumns;
	readonly GridColumnStylesCollection gridColumns;

	internal void InvalidateColumn(DataGridColumnStyle column)
	{
		int index = GridColumnStyles.IndexOf(column);
		if (index >= 0 && DataGrid != null)
		{
			DataGrid.InvalidateColumn(index);
		}
	}

	void OnColumnCollectionChanged(object sender, CollectionChangeEventArgs e)
	{
		gridColumns.CollectionChanged -= new CollectionChangeEventHandler(OnColumnCollectionChanged);

		try
		{
			DataGrid grid = DataGrid;
			DataGridColumnStyle col = e.Element as DataGridColumnStyle;
			if (e.Action == CollectionChangeAction.Add)
			{
				if (col != null)
				{
					col.SetDataGridInternalInColumn(grid);
				}
			}
			else if (e.Action == CollectionChangeAction.Remove)
			{
				if (col != null)
				{
					col.SetDataGridInternalInColumn(null);
				}
			}
			else
			{
				// if we get a column in this collectionChangeEventArgs it means
				// that the propertyDescriptor in that column changed.
				if (e.Element != null)
				{
					for (int i = 0; i < gridColumns.Count; i++)
					{
						gridColumns[i].SetDataGridInternalInColumn(null);
					}
				}
			}

			if (grid != null)
			{
				grid.OnColumnCollectionChanged(this, e);
			}
		}
		finally
		{
			gridColumns.CollectionChanged += new CollectionChangeEventHandler(OnColumnCollectionChanged);
		}
	}

	internal int GridLineWidth
	{
		get
		{
			return GridLineStyle == DataGridLineStyle.Solid ? 1 : 0;
		}
	}

	public DataGridLineStyle GridLineStyle
	{
		get
		{
			return gridLineStyle;
		}
		set
		{
			if (isDefaultTableStyle)
			{
				throw new ArgumentException(string.Format(SR.DataGridDefaultTableSet, nameof(GridLineStyle)));
			}

			//valid values are 0x0 to 0x1.
			if (!ClientUtils.IsEnumValid(value, (int)value, (int)DataGridLineStyle.None, (int)DataGridLineStyle.Solid))
			{
				throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(DataGridLineStyle));
			}
			if (gridLineStyle != value)
			{
				gridLineStyle = value;
				OnGridLineStyleChanged(EventArgs.Empty);
			}
		}
	}
	DataGridLineStyle gridLineStyle = defaultGridLineStyle;
	const DataGridLineStyle defaultGridLineStyle = DataGridLineStyle.Solid;

	protected virtual void OnGridLineStyleChanged(EventArgs e)
	{
		GridLineStyleChanged?.Invoke(this, e);
	}
	public event EventHandler GridLineStyleChanged;

	internal void SetRelationsList(CurrencyManager listManager)
	{
		PropertyDescriptorCollection propCollection = listManager.GetItemProperties();
		int propCount = propCollection.Count;
		if (relationsList.Count > 0)
		{
			relationsList.Clear();
		}

		for (int i = 0; i < propCount; i++)
		{
			PropertyDescriptor prop = propCollection[i];
			if (PropertyDescriptorIsARelation(prop))
			{
				// relation
				relationsList.Add(prop.Name);
			}
		}
	}

	internal void SetGridColumnStylesCollection(CurrencyManager listManager)
	{
		// when we are setting the gridColumnStyles, do not handle any gridColumnCollectionChanged events
		gridColumns.CollectionChanged -= new CollectionChangeEventHandler(OnColumnCollectionChanged);

		PropertyDescriptorCollection propCollection = listManager.GetItemProperties();

		// we need to clear the relations list
		if (relationsList.Count > 0)
		{
			relationsList.Clear();
		}

		int propCount = propCollection.Count;
		for (int i = 0; i < propCount; i++)
		{
			PropertyDescriptor prop = propCollection[i];
			// do not take into account the properties that are browsable.
			if (!prop.IsBrowsable)
			{
				continue;
			}

			if (PropertyDescriptorIsARelation(prop))
			{
				// relation
				relationsList.Add(prop.Name);
			}
			else
			{
				// column
				DataGridColumnStyle col = CreateGridColumn(prop, isDefaultTableStyle);
				if (isDefaultTableStyle)
				{
					gridColumns.AddDefaultColumn(col);
				}
				else
				{
					col.MappingName = prop.Name;
					col.HeaderText = prop.Name;
					gridColumns.Add(col);
				}
			}
		}

		// now we are able to handle the collectionChangeEvents
		gridColumns.CollectionChanged += new CollectionChangeEventHandler(OnColumnCollectionChanged);
	}

	internal void ResetRelationsUI()
	{
		//relationshipRect = Rectangle.Empty;
		focusedRelation = -1;
		//relationshipHeight = dataGrid.LinkFontHeight + relationshipSpacing;
	}

	/// <summary>
	///  Gets the
	///  list of relation objects for the grid table.
	/// </summary>
	internal ArrayList RelationsList => relationsList;
	readonly ArrayList relationsList = new ArrayList(2);

	internal void ResetRelationsList()
	{
		if (isDefaultTableStyle)
		{
			relationsList.Clear();
		}
	}

	static bool PropertyDescriptorIsARelation(PropertyDescriptor prop)
	{
		return typeof(IList).IsAssignableFrom(prop.PropertyType) && !typeof(Array).IsAssignableFrom(prop.PropertyType);
	}

	internal protected virtual DataGridColumnStyle CreateGridColumn(PropertyDescriptor prop, bool isDefault)
	{
		if (prop == null)
		{
			throw new ArgumentNullException(nameof(prop));
		}

		DataGridColumnStyle ret = null;
		Type dataType = prop.PropertyType;

		if (dataType.Equals(typeof(bool)))
		{
			//ret = new DataGridBoolColumn(prop, isDefault);
			// At some point we should return the DataGridBoolColumn as above to match winforms
			// For now lets just return a text column
			ret = new DataGridTextBoxColumn(prop, isDefault);
		}
		else if (dataType.Equals(typeof(string)))
		{
			ret = new DataGridTextBoxColumn(prop, isDefault);
		}
		else if (dataType.Equals(typeof(DateTime)))
		{
			ret = new DataGridTextBoxColumn(prop, "d", isDefault);
		}
		else if (dataType.Equals(typeof(short)) ||
				 dataType.Equals(typeof(int)) ||
				 dataType.Equals(typeof(long)) ||
				 dataType.Equals(typeof(ushort)) ||
				 dataType.Equals(typeof(uint)) ||
				 dataType.Equals(typeof(ulong)) ||
				 dataType.Equals(typeof(decimal)) ||
				 dataType.Equals(typeof(double)) ||
				 dataType.Equals(typeof(float)) ||
				 dataType.Equals(typeof(byte)) ||
				 dataType.Equals(typeof(sbyte)))
		{
			ret = new DataGridTextBoxColumn(prop, "G", isDefault);
		}
		else
		{
			ret = new DataGridTextBoxColumn(prop, isDefault);
		}
		return ret;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			GridColumnStylesCollection gridColumnStyles = GridColumnStyles;
			if (gridColumnStyles != null)
			{
				for (int i = 0; i < gridColumnStyles.Count; i++)
				{
					gridColumnStyles[i].Dispose();
				}
			}
		}
	}
}
