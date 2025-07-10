using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms.Layout;

namespace System.Windows.Forms;

public class TableLayoutPanel : Panel
{
	readonly TableLayoutSettings tableLayoutSettings;

	public TableLayoutPanel()
	{
		tableLayoutSettings = TableLayout.CreateSettings(this);
	}

	public override LayoutEngine LayoutEngine => TableLayout.Instance;

	public TableLayoutSettings LayoutSettings
	{
		get => tableLayoutSettings;
		set
		{
			if (value is not null && value.IsStub)
			{
				// WINRES only scenario.
				// we only support table layout settings that have been created from a type converter.
				// this is here for localization (WinRes) support.
				using (new LayoutTransaction(this, this, PropertyNames.LayoutSettings))
				{
					tableLayoutSettings.ApplySettings(value);
				}
			}
			else
			{
				throw new NotSupportedException(SR.TableLayoutSettingSettingsIsNotSupported);
			}
		}
	}

	protected override ControlCollection CreateControlsInstance() => new TableLayoutControlCollection(this);

	public new TableLayoutControlCollection Controls => (TableLayoutControlCollection)base.Controls;

	public int ColumnCount
	{
		get => tableLayoutSettings.ColumnCount;
		set
		{
			tableLayoutSettings.ColumnCount = value;
			NotifyRenderRequired();
		}
	}

	public TableLayoutColumnStyleCollection ColumnStyles => tableLayoutSettings.ColumnStyles;

	public int RowCount
	{
		get => tableLayoutSettings.RowCount;
		set
		{
			tableLayoutSettings.RowCount = value;
			NotifyRenderRequired();
		}
	}

	public TableLayoutRowStyleCollection RowStyles => tableLayoutSettings.RowStyles;

	public TableLayoutPanelCellBorderStyle CellBorderStyle
	{
		get => tableLayoutSettings.CellBorderStyle;
		set
		{
			tableLayoutSettings.CellBorderStyle = value;
			NotifyRenderRequired();
		}
	}

	public int GetColumnSpan(Control control) => tableLayoutSettings.GetColumnSpan(control);

	public void SetColumnSpan(Control control, int value)
	{
		tableLayoutSettings.SetColumnSpan(control, value);
		NotifyRenderRequired();
	}

	public int GetRowSpan(Control control) => tableLayoutSettings.GetRowSpan(control);

	public void SetRowSpan(Control control, int value)
	{
		tableLayoutSettings.SetRowSpan(control, value);
		NotifyRenderRequired();
	}

	/// <summary>
	///  Sets the TableLayoutPanelCellPosition that represents the row and the column of the cell.
	/// </summary>
	public void SetCellPosition(Control control, TableLayoutPanelCellPosition position)
	{
		tableLayoutSettings.SetCellPosition(control, position);
	}

	/// <summary>
	///  Gets the TableLayoutPanelCellPosition that represents the row and the column of the cell that contains the control.
	/// </summary>
	public TableLayoutPanelCellPosition GetPositionFromControl(Control control)
	{
		return tableLayoutSettings.GetPositionFromControl(control);
	}

	/// <summary>
	///  When a layout fires, make sure we're painting all of our
	///  cell borders.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	protected override void OnLayout(LayoutEventArgs levent)
	{
		base.OnLayout(levent);
		Invalidate();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void ScaleCore(float dx, float dy)
	{
		base.ScaleCore(dx, dy);
		ScaleAbsoluteStyles(new SizeF(dx, dy));
	}

	/// <summary>
	///  Scale this form.  Form overrides this to enforce a maximum / minimum size.
	/// </summary>
	protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
	{
		base.ScaleControl(factor, specified);
		ScaleAbsoluteStyles(factor);
	}

	void ScaleAbsoluteStyles(SizeF factor)
	{
		TableLayout.ContainerInfo containerInfo = TableLayout.GetContainerInfo(this);
		int i = 0;

		// The last row/column can be larger than the
		// absolutely styled column width.
		int lastRowHeight = -1;
		int lastRow = containerInfo.Rows.Length - 1;
		if (containerInfo.Rows.Length > 0)
		{
			lastRowHeight = containerInfo.Rows[lastRow].MinSize;
		}

		int lastColumnHeight = -1;
		int lastColumn = containerInfo.Columns.Length - 1;
		if (containerInfo.Columns.Length > 0)
		{
			lastColumnHeight = containerInfo.Columns[containerInfo.Columns.Length - 1].MinSize;
		}

		foreach (ColumnStyle cs in ColumnStyles)
		{
			if (cs.SizeType == SizeType.Absolute)
			{
				if (i == lastColumn && lastColumnHeight > 0)
				{
					// the last column is typically expanded to fill the table. use the actual
					// width in this case.
					cs.Width = (float)Math.Round(lastColumnHeight * factor.Width);
				}
				else
				{
					cs.Width = (float)Math.Round(cs.Width * factor.Width);
				}
			}
			i++;
		}

		i = 0;

		foreach (RowStyle rs in RowStyles)
		{
			if (rs.SizeType == SizeType.Absolute)
			{
				if (i == lastRow && lastRowHeight > 0)
				{
					// the last row is typically expanded to fill the table. use the actual
					// width in this case.
					rs.Height = (float)Math.Round(lastRowHeight * factor.Height);
				}
				else
				{
					rs.Height = (float)Math.Round(rs.Height * factor.Height);
				}
			}
		}
	}

	/// <summary>
	///  Specifies if a TableLayoutPanel will gain additional rows or columns once its existing cells
	///  become full.  If the value is 'FixedSize' then the TableLayoutPanel will throw an exception
	///  when the TableLayoutPanel is over-filled.
	/// </summary>
	[SRDescription(nameof(SR.TableLayoutPanelGrowStyleDescr))]
	[SRCategory(nameof(SR.CatLayout))]
	[DefaultValue(TableLayoutPanelGrowStyle.AddRows)]
	public TableLayoutPanelGrowStyle GrowStyle
	{
		get => tableLayoutSettings.GrowStyle;
		set => tableLayoutSettings.GrowStyle = value;
	}

	public int[] GetColumnWidths()
	{
		var containerInfo = TableLayout.GetContainerInfo(this);
		if (containerInfo.Columns is null)
		{
			return Array.Empty<int>();
		}

		var cw = new int[containerInfo.Columns.Length];
		for (int i = 0; i < containerInfo.Columns.Length; i++)
		{
			cw[i] = containerInfo.Columns[i].MinSize;
		}

		return cw;
	}

	public int[] GetRowHeights()
	{
		var containerInfo = TableLayout.GetContainerInfo(this);
		if (containerInfo.Rows is null)
		{
			return Array.Empty<int>();
		}

		var rh = new int[containerInfo.Rows.Length];
		for (int i = 0; i < containerInfo.Rows.Length; i++)
		{
			rh[i] = containerInfo.Rows[i].MinSize;
		}

		return rh;
	}

	public TableLayoutPanelCellPosition GetCellPosition(Control control) => tableLayoutSettings.GetCellPosition(control);

	public Control? GetControlFromPosition(int column, int row) => (Control?)tableLayoutSettings.GetControlFromPosition(column, row);

	internal virtual Control? ParentInternal
	{
		get => Parent;
		set => Parent = value;
	}

	public int GetColumn(Control control) => tableLayoutSettings.GetColumn(control);

	public void SetColumn(Control control, int column)
	{
		tableLayoutSettings.SetColumn(control, column);
	}

	public int GetRow(Control control) => tableLayoutSettings.GetRow(control);

	public void SetRow(Control control, int row)
	{
		tableLayoutSettings.SetRow(control, row);
	}

	protected internal override string ScrollStyleString
	{
		get
		{
			if (CellBorderStyle != TableLayoutPanelCellBorderStyle.Single)
			{
				return base.ScrollStyleString;
			}
			return AutoScroll ? "overflow: auto" : "overflow: visible;";
		}
	}

	public void SetBorder(Control control)
	{
		control.ExtraStyleString = CellBorderStyle switch
		{
			TableLayoutPanelCellBorderStyle.Inset => "outline: 0.1px solid #A0A0A0; outline-offset: 3.9px;",
			TableLayoutPanelCellBorderStyle.Single => "outline: 0.1px solid #A0A0A0; outline-offset: 0.9px;",
			_ => control.ExtraStyleString
		};
	}

	public override Color FindControlRealBackColor()
	{
		return this.BackColor;
	}
}
