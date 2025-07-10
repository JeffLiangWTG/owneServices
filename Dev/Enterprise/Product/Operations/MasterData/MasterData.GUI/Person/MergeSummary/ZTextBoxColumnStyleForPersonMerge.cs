using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	class ZTextBoxColumnStyleForPersonMergeInfo : ZTextBoxColumnStyleInfo
	{
		public override Type ColumnStyleType => typeof(ZTextBoxColumnStyleForPersonMerge);
	}

	class ZTextBoxColumnStyleForPersonMerge : ZTextBoxColumnStyle
	{
		public ZTextBoxColumnStyleForPersonMerge(ZTextBoxColumnStyleInfo columnInfo) : base(columnInfo)
		{
		}
		public ZTextBoxColumnStyleForPersonMerge(ZTextBoxColumnStyleForPersonMerge columnStyle) : this(columnStyle.ColumnInfo)
		{
		}

#if !WINZOR

		PersonMergeZGrid mergeGrid;

		protected override void SetDataGrid(DataGrid grid)
		{
			mergeGrid = grid as PersonMergeZGrid;
			base.SetDataGrid(grid);
		}

		protected override void PaintText(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, string cellText, Font cellFont, Brush backBrush, Brush foreBrush, bool rightToLeft)
		{
			var customRowForeColour = mergeGrid?.GetCellForeColour(cellText);
			if (customRowForeColour.HasValue && customRowForeColour.Value.ToArgb() != 0)
			{
				foreBrush = BrushProvider.FromColor(customRowForeColour.Value);
			}
			base.PaintText(g, bounds, source, rowNum, cellText, cellFont, backBrush, foreBrush, rightToLeft);
		}
#endif
	}
}
