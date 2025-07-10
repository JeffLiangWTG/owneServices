using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Enterprise.MasterFiles.GUI
{
	public class StringToVisibilityConverter : IValueConverter
	{
		public bool IsCollapsed { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var sValue = (string)value;
			if (!string.IsNullOrEmpty(sValue))
			{
				return Visibility.Visible;
			}

			return IsCollapsed ? Visibility.Collapsed : Visibility.Hidden;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
