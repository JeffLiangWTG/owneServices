using System;
using System.Globalization;
using System.Windows.Data;
using CargoWise.Types;

namespace Enterprise.MarketingManager.GUI.Converters
{
	[ValueConversion(typeof(string), typeof(ZByte))]
	public class StringToNumConverter : IValueConverter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType != typeof(ZByte))
			{
				throw new InvalidOperationException("The target must be a ZByte");
			}

			if (!(value is string))
			{
				throw new InvalidOperationException("The source must be a string");
			}

			ZByte result;

			return ZByte.TryParse((string)value, out result) ? result : ZByte.Zero;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType != typeof(string))
			{
				throw new InvalidOperationException("The target must be a string");
			}

			if (!(value is ZByte))
			{
				throw new InvalidOperationException("The source must be a ZByte");
			}

			return value.ToString();
		}
	}
}
