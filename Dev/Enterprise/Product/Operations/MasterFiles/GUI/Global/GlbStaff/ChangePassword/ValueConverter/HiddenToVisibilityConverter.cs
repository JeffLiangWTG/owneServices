using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Enterprise.MasterFiles.GUI
{
	public class HiddenToVisibilityConverter : IValueConverter
	{
		public bool IsCollapsed { get; set; }

		public bool IsOpposite { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var bValue = IsOpposite ? !(bool)value : (bool)value;

			if (bValue)
			{
				return Visibility.Visible;
			}
			else
			{
				return IsCollapsed ? Visibility.Collapsed : Visibility.Hidden;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (Visibility)value == Visibility.Visible;
		}
	}
}
