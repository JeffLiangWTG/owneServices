using System.ComponentModel;
using System.Drawing;
using WinzorFramework;

#nullable disable

namespace System.Windows.Forms;

public partial class DataGridView : Control, ISupportInitialize
{
	public DataGridView()
	{
		this.Controls.Add(datagrid);
	}
	readonly DataGrid datagrid = new DataGrid();

	public WrappedList<DataGridViewRow> Rows { get; } = new WrappedList<DataGridViewRow>();

	public DataGridViewColumnCollection Columns { get; } = new DataGridViewColumnCollection();

	public DataGridViewCell this[int columnIndex, int rowIndex] { get => null; set { } }

	const int defaultColumnHeadersHeight = 23;
	int columnHeadersHeight = defaultColumnHeadersHeight;

	public int ColumnHeadersHeight
	{
		get => columnHeadersHeight;
		set => columnHeadersHeight = value;
	}

	public DataGridViewColumnHeadersHeightSizeMode ColumnHeadersHeightSizeMode { get; set; }

	public int RowHeadersWidth { get; set; }

	public DataGridViewRowHeadersWidthSizeMode RowHeadersWidthSizeMode { get; set; }

	public DataGridViewAutoSizeRowsMode AutoSizeRowsMode { get; set; }

	public bool AllowUserToResizeColumns { get; set; }

	public bool AllowUserToResizeRows { get; set; }

	public Color BackgroundColor { get; set; }

	public bool AutoGenerateColumns { get; set; }

	public DataGridViewCellStyle DefaultCellStyle { get; set; }

	public ScrollBars ScrollBars { get; set; }

	public WrappedList<DataGridViewCell> SelectedCells { get; }

	public object DataSource { get; set; }

	public string DataMember { get; set; }

	public bool AllowUserToAddRows { get; set; }

	public bool AllowUserToDeleteRows { get; set; }

	public bool ReadOnly { get; set; }

	public void SelectAll()
	{
	}

	public DataGridViewCell CurrentCell { get; set; }

	public virtual bool BeginEdit(bool selectAll) => false;

	public DataGridViewClipboardCopyMode ClipboardCopyMode { get; set; }

	public DataGridViewEditMode EditMode { get; set; }

	public DataGridViewRow RowTemplate { get; set; }

	public virtual DataObject GetClipboardContent() => null;

	public void BeginInit()
	{
	}

	public void EndInit()
	{
	}

	public event DataGridViewColumnEventHandler ColumnAdded;

	public event DataGridViewDataErrorEventHandler DataError;

	public event DataGridViewRowsAddedEventHandler RowsAdded;
}
