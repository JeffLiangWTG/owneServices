using System.ComponentModel;

namespace System.Windows.Forms;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
public record DataGridViewModel(DataGridColumnViewModel[] Columns, DataGridRow[] Rows);

public record DataGridColumnViewModel(DataGridColumnStyle ColumnStyle, int Index, string HeaderText, ListSortDirection? SortDirection);
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter

