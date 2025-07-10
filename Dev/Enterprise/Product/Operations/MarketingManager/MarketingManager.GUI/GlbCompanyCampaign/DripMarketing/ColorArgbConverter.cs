using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;
using CargoWise.Types;

namespace Enterprise.MarketingManager.GUI
{
	[ValueConversion(typeof(object), typeof(int))]
	public class ColorArgbConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is ZInt)
			{
				var argb = (ZInt)value;
				return argb != 0 ? "#" + Color.FromArgb(argb).Name.PadLeft(6, '0') : string.Empty;
			}

			return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null || string.IsNullOrEmpty(value.ToString()))
			{
				return 0;
			}

			return int.Parse(value.ToString().Replace("#", ""), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
		}
	}
}
