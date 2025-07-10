using System.Collections;
using System.ComponentModel;
using System.Drawing;
using Microsoft.AspNetCore.Components;
using WinzorFramework;

#nullable disable

namespace System.Windows.Forms;

public abstract class DataGridColumnStyle : Component
{
	/// <summary>
	///  In a derived class, initializes a new instance of the
	/// <see cref='DataGridColumnStyle'/> class.
	/// </summary>
	public DataGridColumnStyle()
	{
	}

	/// <summary>
	///  Initializes a new instance of the <see cref='DataGridColumnStyle'/>
	///  class with the specified <see cref='T:System.ComponentModel.PropertyDescriptor'/>.
	/// </summary>
	public DataGridColumnStyle(PropertyDescriptor prop) : this()
	{
		PropertyDescriptor = prop;
		if (prop != null)
		{
			readOnly = prop.IsReadOnly;
		}
	}

	//This is an additional property created in WINZOR to not call Edit in DataGrid for those Column Style where Edit is not required
	protected internal bool isEditRequired { get; set; } = true;

	protected DataGridColumnStyle(PropertyDescriptor prop, bool isDefault) : this(prop)
	{
		if (isDefault && prop != null)
		{
			// take the header name from the property name
			headerName = prop.Name;
			mappingName = prop.Name;
		}
	}

	/// <summary>
	///  Gets or sets the text of the column header.
	/// </summary>
	public virtual string HeaderText
	{
		get => headerName;
		set
		{
			if (value == null)
			{
				value = string.Empty;
			}

			if (!headerName.Equals(value))
			{
				headerName = value;
				OnHeaderTextChanged(EventArgs.Empty);
				// we only invalidate columns that are visible ( ie, their propertyDescriptor is not null)
				if (PropertyDescriptor != null)
				{
					Invalidate();
				}
			}
		}
	}
	string headerName = string.Empty;

	void OnHeaderTextChanged(EventArgs e)
	{
		HeaderTextChanged?.Invoke(this, e);
	}
	public event EventHandler HeaderTextChanged;

	/// <summary>
	///  Gets the System.Windows.Forms.DataGridTableStyle for the column.
	/// </summary>
	public virtual DataGridTableStyle DataGridTableStyle => dataGridTableStyle;
	DataGridTableStyle dataGridTableStyle;

	internal void SetDataGridTableInColumn(DataGridTableStyle value, bool force)
	{
		if (dataGridTableStyle != null && dataGridTableStyle.Equals(value) && !force)
		{
			return;
		}

		if (value != null && value.DataGrid != null && !value.DataGrid.Initializing)
		{
			SetDataGridInColumn(value.DataGrid);
		}

		dataGridTableStyle = value;
	}

	public string MappingName
	{
		get => mappingName;
		set
		{
			if (value == null)
			{
				value = string.Empty;
			}

			if (!mappingName.Equals(value))
			{
				string originalMappingName = mappingName;
				mappingName = value;
				try
				{
					dataGridTableStyle?.GridColumnStyles.CheckForMappingNameDuplicates(this);
				}
				catch
				{
					mappingName = originalMappingName;
					throw;
				}

				OnMappingNameChanged(EventArgs.Empty);
			}
		}
	}
	string mappingName = string.Empty;

	void OnMappingNameChanged(EventArgs e)
	{
		MappingNameChanged?.Invoke(this, e);
	}
	public event EventHandler MappingNameChanged;

	/// <summary>
	///  Gets or sets the <see cref='Data.DataColumn'/> that determines the
	///  attributes of data displayed by the <see cref='DataGridColumnStyle'/>.
	/// </summary>
	public virtual PropertyDescriptor PropertyDescriptor
	{
		get => propertyDescriptor;
		set
		{
			if (propertyDescriptor != value)
			{
				propertyDescriptor = value;
				OnPropertyDescriptorChanged(EventArgs.Empty);
			}
		}
	}
	PropertyDescriptor propertyDescriptor;

	void OnPropertyDescriptorChanged(EventArgs e)
	{
		PropertyDescriptorChanged?.Invoke(this, e);
	}
	public event EventHandler PropertyDescriptorChanged;

	/// <summary>
	///  Gets or sets the alignment of text in a column.
	/// </summary>
	public virtual HorizontalAlignment Alignment
	{
		get => alignment;
		set
		{
			if (!ClientUtils.IsEnumValid(value, (int)value, (int)HorizontalAlignment.Left, (int)HorizontalAlignment.Center))
			{
				throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(DataGridLineStyle));
			}

			if (alignment != value)
			{
				alignment = value;
				OnAlignmentChanged(EventArgs.Empty);
				Invalidate();
			}
		}
	}
	HorizontalAlignment alignment = HorizontalAlignment.Left;

	void OnAlignmentChanged(EventArgs e)
	{
		AlignmentChanged?.Invoke(this, e);
	}
	public event EventHandler AlignmentChanged;

	/// <summary>
	///  Gets or sets the width of the column.
	/// </summary>
	public virtual int Width
	{
		get => width;
		set
		{
			if (width != value)
			{
				width = value;
				DataGrid grid = DataGridTableStyle?.DataGrid;
				if (grid != null)
				{
					// rearrange the scroll bars
					grid.PerformLayout();

					// force the grid to repaint
					grid.InvalidateInside();
				}

				OnWidthChanged(EventArgs.Empty);
			}
		}
	}
	internal int width = -1;

	void OnWidthChanged(EventArgs e)
	{
		WidthChanged?.Invoke(this, e);
	}
	public event EventHandler WidthChanged;

	/// <summary>
	///  When overridden in a derived class, gets the optimum width and height of the
	///  specified value.
	/// </summary>
	protected internal abstract Size GetPreferredSize(BGraphics g, object value);

	/// <summary>
	///  Gets or sets a value indicating whether the data in the column cannot be edited.
	/// </summary>
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

	/// <summary>
	///  Gets or sets the text that is displayed when the column contains a null
	///  value.
	/// </summary>
	public virtual string NullText
	{
		get => nullText;
		set
		{
			if (nullText != value)
			{
				nullText = value;
				OnNullTextChanged(EventArgs.Empty);
				Invalidate();
			}
		}
	}
	string nullText = SR.DataGridNullText;

	void OnNullTextChanged(EventArgs e)
	{
		NullTextChanged?.Invoke(this, e);
	}
	public event EventHandler NullTextChanged;

	/// <summary>
	///  When overridden in a derived class, sets the <see cref='DataGrid'/>
	///  control that this column belongs to.
	/// </summary>
	protected virtual void SetDataGrid(DataGrid value)
	{
		SetDataGridInColumn(value);
	}

	/// <summary>
	///  When overridden in a derived class, sets the <see cref='DataGrid'/>
	///  for the column.
	/// </summary>
	protected virtual void SetDataGridInColumn(DataGrid value)
	{
		// we need to set up the PropertyDescriptor
		if (PropertyDescriptor == null && value != null)
		{
			CurrencyManager lm = value.ListManager;
			if (lm == null)
			{
				return;
			}

			PropertyDescriptorCollection propCollection = lm.GetItemProperties();
			int propCount = propCollection.Count;
			for (int i = 0; i < propCollection.Count; i++)
			{
				PropertyDescriptor prop = propCollection[i];
				if (!typeof(IList).IsAssignableFrom(prop.PropertyType) && prop.Name.Equals(HeaderText))
				{
					PropertyDescriptor = prop;
					return;
				}
			}
		}
	}

	protected internal virtual bool IsCurrentCellReadOnly => ReadOnly;

	protected internal abstract MarkupString GetRenderContent(CurrencyManager source, int rowNum, bool alignToRight);

	protected internal abstract string GetCellStyleString(CurrencyManager source, int rowNum);

	protected internal abstract string GetEditControlStyleString(CurrencyManager source, int rowNum);

	protected internal virtual NotificationIcon RowColumnNotification(int row) => null;

	/// <summary>
	///  When overridden in a derived class, initiates a request to interrrupt an edit
	///  procedure.
	/// </summary>
	protected internal abstract void Abort(int rowNum);

	/// <summary>
	///  When overridden in a derived class, inititates a request to complete an
	///  editing procedure.
	/// </summary>
	protected internal abstract bool Commit(CurrencyManager dataSource, int rowNum);

	/// <summary>
	///  When overridden in a deriving class, prepares a cell for editing.
	/// </summary>
	protected internal virtual void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly)
	{
		Edit(source, rowNum, bounds, readOnly, null, true);
	}

	/// <summary>
	///  Prepares the cell for editing, passing the specified <see cref='Data.DataView'/>,
	///  row number, <see cref='Rectangle'/>, argument indicating whether
	///  the column is read-only, and the text to display in the new control.
	/// </summary>
	protected internal virtual void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string displayText)
	{
		Edit(source, rowNum, bounds, readOnly, displayText, true);
	}

	/// <summary>
	///  When overridden in a deriving class, prepares a cell for editing.
	/// </summary>
	protected internal abstract void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string displayText, bool cellIsVisible);

	protected internal virtual void ColumnStartedEditing(Control editingControl)
	{
		DataGridTableStyle?.DataGrid?.ColumnStartedEditing(editingControl);
	}

	/// <summary>
	///  Redraws the column and causes a paint message to be sent to the control.
	/// </summary>
	protected virtual void Invalidate()
	{
		DataGridTableStyle?.InvalidateColumn(this);
	}

	/// <summary>
	///  Gets the value in the specified row from the specified System.Windows.Forms.ListManager.
	/// </summary>
	protected internal virtual object GetColumnValueAtRow(CurrencyManager source, int rowNum)
	{
		CheckValidDataSource(source);
		var descriptor = PropertyDescriptor ?? throw new InvalidOperationException(SR.DataGridColumnNoPropertyDescriptor);

		return descriptor.GetValue(source[rowNum]);
	}

	/// <summary>
	///  Checks if the specified DataView is valid.
	/// </summary>
	protected void CheckValidDataSource(CurrencyManager value)
	{
		if (value == null)
		{
			throw new ArgumentNullException(nameof(value));
		}

		// The code may delete a gridColumn that was editing.
		// In that case, we still have to push the value into the backend
		// and we only need the propertyDescriptor to push the value.
		// (take a look at gridEditAndDeleteEditColumn)
		if (PropertyDescriptor == null)
		{
			throw new InvalidOperationException(string.Format(SR.DataGridColumnUnbound, HeaderText));
		}
	}

	/// <summary>
	///  Sets the value in a specified row with the value from a specified see DataView.
	/// </summary>
	protected internal virtual void SetColumnValueAtRow(CurrencyManager source, int rowNum, object value)
	{
		CheckValidDataSource(source);
		var descriptor = PropertyDescriptor ?? throw new InvalidOperationException(SR.DataGridColumnNoPropertyDescriptor);

		if (source.Position != rowNum)
		{
			throw new ArgumentException(SR.DataGridColumnListManagerPosition, nameof(rowNum));
		}
		if (source[rowNum] is IEditableObject editableObject)
		{
			editableObject.BeginEdit();
		}

		descriptor.SetValue(source[rowNum], value);
	}

	internal void SetDataGridInternalInColumn(DataGrid value)
	{
		if (value == null || value.Initializing)
		{
			return;
		}

		SetDataGridInColumn(value);
	}

	/// <summary>
	///  When overridden in a derived class, directs the column to concede focus with
	///  an appropriate action.
	/// </summary>
	protected internal virtual void ConcedeFocus()
	{
	}

	/// <summary>
	///  When overridden in a derived class, updates the value of a specified row with
	///  the given text.
	/// </summary>
	protected internal virtual void UpdateUI(CurrencyManager source, int rowNum, string displayText)
	{
	}

	/// <summary>
	///  Gets the minimum height of a row.
	/// </summary>
	protected internal abstract int GetMinimumHeight();

	/// <summary>
	///  Gets the height of the column's font.
	/// </summary>
	protected int FontHeight => DataGridTableStyle?.DataGrid?.FontHeight ?? DataGridTableStyle.defaultFontHeight;

	protected internal virtual void ReleaseHostedControl()
	{
	}

	/// <summary>
	///  Provides a handler for determining which key was pressed, and whether to
	///  process it.
	/// </summary>
	internal virtual bool KeyPress(int rowNum, Keys keyData)
	{
		// if this is read only then do not do anything
		if (ReadOnly || (DataGridTableStyle != null && DataGridTableStyle.DataGrid != null && DataGridTableStyle.DataGrid.ReadOnly))
		{
			return false;
		}
		if (keyData == (Keys.Control | Keys.NumPad0) || keyData == (Keys.Control | Keys.D0))
		{
			EnterNullValue();
			return true;
		}

		return false;
	}

	/// <summary>
	///  When overriden in a derived class, enters a <see cref='T:System.DBNull.Value' qualify='true'/>
	///  into the column.
	/// </summary>
	protected internal virtual void EnterNullValue()
	{
	}
}
