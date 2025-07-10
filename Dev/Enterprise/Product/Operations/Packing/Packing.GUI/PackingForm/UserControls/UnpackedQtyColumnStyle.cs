using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Packing.GUI
{
	public class UnpackedQtyColumnStyle : ZCalcEditColumnStyle
	{
		public UnpackedQtyColumnStyle(UnpackedQtyColumnStyleInfo info)
			: base(info)
		{
		}

		#region Painting Items
#if !WINZOR

		protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int paintingRowNum, Brush backBrush, Brush foreBrush, bool alignedToRight)
		{
			var current = (PackableItemParentWrapper)source.List[paintingRowNum];
			var textBrush = (current != null && current.UnpackedQty > 0m) ? Brushes.Red : foreBrush;

			base.Paint(g, bounds, source, paintingRowNum, backBrush, textBrush, alignedToRight);
		}

#endif
		#endregion
	}

	public class UnpackedQtyColumnStyleInfo : ZCalcEditColumnStyleInfo
	{
		public override Type ColumnStyleType
		{
			get { return typeof(UnpackedQtyColumnStyle); }
		}
	}
}
