using System;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public class PostCodeColumnStyleInfo : ZMultiControlColumnStyleInfo
	{
		public PostCodeColumnStyleInfo()
			: base()
		{
		}

		public PostCodeColumnStyleInfo(string columnName, int width)
			: base(columnName, width)
		{
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType => typeof(PostCodeColumnStyle);
	}

	public class PostCodeColumnStyle : ZMultiControlColumnStyle
	{
		public PostCodeColumnStyle(PostCodeColumnStyleInfo info)
			: base(info)
		{
		}

		#region Test
#if DEBUG
		public void SetColumnValueAtRowExposed(CurrencyManager source, int rowNum, object value)
		{
			SetColumnValueAtRow(source, rowNum, value);
		}
#endif
		#endregion

		protected override void SetColumnValueAtRow(CurrencyManager source, int rowNum, object value)
		{
			var zoneItem = source.GetCurrent() as RateTransportZoneItem;
			zoneItem.CheckOrCreatePostCodeFromWebIfNotInDatabase(value.ToString());

			base.SetColumnValueAtRow(source, rowNum, value);
		}
	}
}
