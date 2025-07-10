using System.ComponentModel;
using System.Drawing;
using System.Net;
using Microsoft.AspNetCore.Components;
using WinzorFramework.Extensions;

#nullable disable

namespace System.Windows.Forms;

public class DataGridTextBoxColumn : DataGridColumnStyle
{
	/// <summary>
	///  Initializes a new instance of the System.Windows.Forms.DataGridTextBoxColumn
	///  class.
	/// </summary>
	public DataGridTextBoxColumn() : this(null, null)
	{
	}

	/// <summary>
	///  Initializes a new instance of a System.Windows.Forms.DataGridTextBoxColumn with
	///  a specified System.Data.DataColumn.
	/// </summary>
	public DataGridTextBoxColumn(PropertyDescriptor prop)
	: this(prop, null, false)
	{
	}

	/// <summary>
	///  Initializes a new instance of a System.Windows.Forms.DataGridTextBoxColumn. with
	///  the specified System.Data.DataColumn and System.Windows.Forms.ComponentModel.Format.
	/// </summary>
	public DataGridTextBoxColumn(PropertyDescriptor prop, string format) : this(prop, format, false) { }

	public DataGridTextBoxColumn(PropertyDescriptor prop, string format, bool isDefault) : base(prop, isDefault)
	{
		edit = new DataGridTextBox
		{
			BorderStyle = BorderStyle.None,
			AcceptsReturn = true,
			Visible = false,
			TabStop = false
		};
		Format = format;
	}

	public DataGridTextBoxColumn(PropertyDescriptor prop, bool isDefault) : this(prop, null, isDefault) { }

	public virtual TextBox TextBox  => edit;

	public IFormatProvider FormatInfo
	{
		get => formatInfo;
		set
		{
			if (formatInfo == null || !formatInfo.Equals(value))
			{
				formatInfo = value;
			}
		}
	}
	IFormatProvider formatInfo;

	public string Format
	{
		get => format;
		set
		{
			if (value == null)
			{
				value = string.Empty;
			}

			if (format == null || !format.Equals(value))
			{
				format = value;

				// if the associated typeConverter cannot convert from string,
				// then we can't modify the column value. hence, make it readOnly
				//
				if (format.Length == 0)
				{
					if (typeConverter != null && !typeConverter.CanConvertFrom(typeof(string)))
					{
						ReadOnly = true;
					}
				}

				Invalidate();
			}
		}
	}
	string format;

	/// <summary>
	///  Adds a System.Windows.Forms.TextBox control to the System.Windows.Forms.DataGrid control's System.Windows.Forms.Control.ControlCollection
	/// </summary>
	protected override void SetDataGridInColumn(DataGrid value)
	{
		base.SetDataGridInColumn(value);
		if (edit.Parent != null)
		{
			edit.Parent.Controls.Remove(edit);
		}
		if (value != null)
		{
			value.Controls.Add(edit);
		}

		// we have to tell the edit control about its dataGrid
		edit.SetDataGrid(value);

		value.CurrentCellChanged += CurrentCellChanged;
	}

	void CurrentCellChanged(object sender, EventArgs e)
	{
		if (edit.Visible)
		{
			edit.DetachFromGrid();
		}
	}

	protected internal override MarkupString GetRenderContent(CurrencyManager source, int rowNum, bool alignToRight)
	{
		return (MarkupString)WebUtility.HtmlEncode(GetText(GetColumnValueAtRow(source, rowNum)));
	}

	protected internal virtual string GetText(object value)
	{
		if (value is DBNull)
		{
			return NullText;
		}
		else if (format != null && format.Length != 0 && (value is IFormattable))
		{
			try
			{
				return ((IFormattable)value).ToString(format, formatInfo);
			}
			catch
			{
				//
			}
		}
		else
		{
			// use the typeConverter:
			if (typeConverter != null && typeConverter.CanConvertTo(typeof(string)))
			{
				return (string)typeConverter.ConvertTo(value, typeof(string));
			}
		}
		return (value != null ? value.ToString() : "");
	}

	/// <summary>
	///  Prepares a cell for editing.
	/// </summary>
	protected internal override void Edit(CurrencyManager source,
								int rowNum,
								Rectangle bounds,
								bool readOnly,
								string displayText,
								bool cellIsVisible)
	{
		Rectangle originalBounds = bounds;

		edit.ReadOnly = readOnly || ReadOnly || DataGridTableStyle.ReadOnly;

		var text = GetText(GetColumnValueAtRow(source, rowNum));
		if (text != edit.Text)
		{
			edit.Text = text;
		}

		if (!edit.ReadOnly && displayText != null)
		{
			// tell the grid that we are changing stuff
			DataGridTableStyle.DataGrid.ColumnStartedEditing(bounds);
			// tell the edit control that the user changed it
			edit.IsInEditOrNavigateMode = false;
			edit.Text = displayText;
		}

		if (cellIsVisible)
		{
			bounds.Offset(this.xMargin, 2 * this.yMargin);
			bounds.Width -= this.xMargin;
			bounds.Height -= 2 * this.yMargin;

			edit.Bounds = bounds;

			edit.Visible = true;

			edit.TextAlign = Alignment;
		}
		else
		{
			edit.Bounds = Rectangle.Empty;
			// edit.Bounds = originalBounds;
			// edit.Visible = false;
		}

		edit.RightToLeft = DataGridTableStyle.DataGrid.RightToLeft;

		edit.Focus();

		if (!edit.ReadOnly)
		{
			oldValue = edit.Text;
		}

		// select the text even if the text box is read only
		// because the navigation code in the DataGridTextBox::ProcessKeyMessage
		// uses the SelectedText property
		if (displayText == null && edit.IsInEditOrNavigateMode)
		{
			edit.SelectAll();
		}
		else
		{
			int end = edit.Text.Length;
			edit.Select(end, 0);
		}

		if (edit.Visible)
		{
			DataGridTableStyle.DataGrid.Invalidate(originalBounds);
		}
	}

	/// <summary>
	///  Inititates a request to complete an editing procedure.
	/// </summary>
	protected internal override bool Commit(CurrencyManager dataSource, int rowNum)
	{
		// always hide the edit box
		// HideEditBox();
		edit.Bounds = Rectangle.Empty;

		if (edit.IsInEditOrNavigateMode)
		{
			return true;
		}

		try
		{
			object value = edit.Text;
			if (NullText.Equals(value))
			{
				value = Convert.DBNull;
				edit.Text = NullText;
			}
			else if (format != null && format.Length != 0 && FormatInfo != null && PropertyDescriptor != null && PropertyDescriptor.PropertyType != typeof(object))
			{
				IParsable parser = ParserFactory.GetParser(PropertyDescriptor.PropertyType);
				value = parser.Parse(edit.Text, FormatInfo);
				if (value is IFormattable)
				{
					edit.Text = ((IFormattable)value).ToString(format, formatInfo);
				}
				else
				{
					edit.Text = value.ToString();
				}
			}
			else if (typeConverter != null && typeConverter.CanConvertFrom(typeof(string)))
			{
				value = typeConverter.ConvertFromString(edit.Text);
				edit.Text = typeConverter.ConvertToString(value);
			}
			
			SetColumnValueAtRow(dataSource, rowNum, value);
		}
		catch
		{
			// MessageBox.Show("There was an error caught setting field \""
			//                 + this.PropertyDescriptor.Name + "\" to the value \"" + edit.Text + "\"\n"
			//                 + "The value is being rolled back to the original.\n"
			//                 + "The error was a '" + e.Message + "' "  + e.StackTrace
			//                 , "Error commiting changes...", MessageBox.IconError);
			// Debug.WriteLine(e.GetType().Name);
			RollBack();
			return false;
		}
		EndEdit();
		return true;
	}

	/// <summary>
	///  Ends an edit operation on the System.Windows.Forms.DataGridColumnStyle
	///  .
	/// </summary>
	protected void EndEdit()
	{
		edit.IsInEditOrNavigateMode = true;
		Invalidate();
	}

	void RollBack()
	{
		edit.Text = oldValue;
	}

	protected internal override string GetCellStyleString(CurrencyManager source, int rowNum) => $"background-color: {Color.Transparent.GetColorStyleValue()};";

	protected internal override string GetEditControlStyleString(CurrencyManager source, int rowNum) => "";

	/// <summary>
	///  Gets or sets the System.Windows.Forms.ComponentModel.Format for the System.Windows.Forms.DataGridTextBoxColumn
	/// </summary>
	public override PropertyDescriptor PropertyDescriptor
	{
		set
		{
			base.PropertyDescriptor = value;
			if (PropertyDescriptor != null)
			{
				if (PropertyDescriptor.PropertyType != typeof(object))
				{
					typeConverter = TypeDescriptor.GetConverter(PropertyDescriptor.PropertyType);
				}
			}
		}
	}

	/// <summary>
	///  Initiates a request to interrupt an edit procedure.
	/// </summary>
	protected internal override void Abort(int rowNum)
	{
		RollBack();
		HideEditBox();
		EndEdit();
	}

	/// <summary>
	///  Hides the System.Windows.Forms.TextBox
	///  control and moves the focus to the System.Windows.Forms.DataGrid
	///  control.
	/// </summary>
	protected void HideEditBox()
	{
		bool wasFocused = edit.Focused;
		edit.Visible = false;

		// it seems that edit.Visible = false will take away the focus from
		// the edit control. And this means that we will not give the focus to the grid
		// If all the columns would have an edit control this would not be bad
		// ( or if the grid is the only control on the form ),
		// but when we have a DataGridBoolColumn then the focus will be taken away
		// by the next control in the form.
		//
		// if (edit.Focused && this.DataGridTableStyle.DataGrid.CanFocus) {

		// when the user deletes the current ( ie active ) column from the
		// grid, the grid should still call EndEdit ( so that the changes that the user made
		// before deleting the column will go to the backEnd)
		// however, in that situation, we are left w/ the editColumn which is not parented.
		// the grid will call Edit to reset the EditColumn
		if (wasFocused && DataGridTableStyle != null && DataGridTableStyle.DataGrid != null && DataGridTableStyle.DataGrid.CanFocus)
		{
			DataGridTableStyle.DataGrid.Focus();
		}
	}

	/// <summary>
	///  Returns the optimum width and
	///  height of the cell in a specified row relative
	///  to the specified value.
	/// </summary>
	protected internal override Size GetPreferredSize(BGraphics g, object value)
	{
		Size extents = Size.Ceiling(g.MeasureString(GetText(value), DataGridTableStyle.DataGrid.Font));
		extents.Width += xMargin * 2 + DataGridTableStyle.GridLineWidth;
		extents.Height += yMargin;
		return extents;
	}

	// will hide the edit control
	/// <summary>
	///  Informs the column the focus is being conceded.
	/// </summary>
	protected internal override void ConcedeFocus()
	{
		edit.Bounds = Rectangle.Empty;
		// edit.Visible = false;
		// HideEditBox();
	}

	protected internal override void UpdateUI(CurrencyManager source, int rowNum, string displayText)
	{
		edit.Text = GetText(GetColumnValueAtRow(source, rowNum));
		if (!edit.ReadOnly && displayText != null)
		{
			edit.Text = displayText;
		}
	}

	/// <summary>
	///  Gets the height of a cell in a System.Windows.Forms.DataGridColumnStyle
	/// </summary>
	protected internal override int GetMinimumHeight()
	{
		// why + 3? cause we have to give some way to the edit box.
		return FontHeight + yMargin + 3;
	}

	protected internal override void ReleaseHostedControl()
	{
		if (edit.Parent != null)
		{
			edit.Parent.Controls.Remove(edit);
		}
	}

	TypeConverter typeConverter;

	readonly DataGridTextBox edit;
	string oldValue;

	readonly int xMargin = 2;
	readonly int yMargin = 1;
}
