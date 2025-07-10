using System.Windows;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class HiddenToVisibilityConverterTest : TestCase
	{
		public void TestHiddenToVisibilityConverter()
		{
			var converter = new HiddenToVisibilityConverter();
			converter.IsCollapsed = false;
			AssertEquals(Visibility.Hidden, converter.Convert(false, null, null, null));

			converter.IsOpposite = true;
			AssertEquals(Visibility.Visible, converter.Convert(false, null, null, null));

			converter.IsCollapsed = true;
			AssertEquals(Visibility.Visible, converter.Convert(false, null, null, null));

			converter.IsCollapsed = false;
			AssertEquals(Visibility.Visible, converter.Convert(false, null, null, null));

			converter.IsOpposite = false;
			AssertEquals(Visibility.Hidden, converter.Convert(false, null, null, null));

			converter.IsCollapsed = true;
			AssertEquals(Visibility.Collapsed, converter.Convert(false, null, null, null));

			converter.IsOpposite = false;
			AssertEquals(Visibility.Visible, converter.Convert(true, null, null, null));

			converter.IsOpposite = true;
			AssertEquals(Visibility.Collapsed, converter.Convert(true, null, null, null));
		}
	}
}
