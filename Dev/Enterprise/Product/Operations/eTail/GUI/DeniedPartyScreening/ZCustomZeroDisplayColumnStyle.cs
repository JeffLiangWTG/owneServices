using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.eTail.GUI
{
	public class ZCustomZeroDisplayColumnStyleInfo : ZCalcEditColumnStyleInfo
	{
		public override Type ColumnStyleType
		{
			get { return typeof(ZCustomZeroDisplayColumnStyle); }
		}
	}
	public class ZCustomZeroDisplayColumnStyle : ZCalcEditColumnStyle
	{
		public ZCustomZeroDisplayColumnStyle(ZCustomZeroDisplayColumnStyleInfo columnInfo) : base(columnInfo)
		{
		}

		protected override string FormatValueObjectCore(object source, object propertyValue)
		{
			if (propertyValue is INumericZType number && number.IsEmpty)
			{
				return Res.GetString("48dc0b5c-9312-4522-9103-e79b03574a5e", "N/A");
			}

			return base.FormatValueObjectCore(source, propertyValue);
		}

		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellVisible)
		{
			// Do nothing to prevent "0" from being displayed in the cell when it is clicked
		}
	}
}
