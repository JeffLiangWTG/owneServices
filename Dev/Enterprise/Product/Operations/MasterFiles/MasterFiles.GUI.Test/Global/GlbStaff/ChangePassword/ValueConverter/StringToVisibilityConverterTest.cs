using System.Windows;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Tests
{
	class StringToVisibilityConverterTest : TestCase
	{
		public void TestConverter()
		{
			var converter = new StringToVisibilityConverter();
			CombineAssertions(() =>
			{
				AssertEquals(Visibility.Hidden, converter.Convert(null, typeof(string), null, null));
				AssertEquals(Visibility.Visible, converter.Convert("abc", typeof(string), null, null));
				AssertEquals(Visibility.Hidden, converter.Convert(string.Empty, typeof(string), null, null));
			});

			converter.IsCollapsed = true;
			AssertEquals(Visibility.Collapsed, converter.Convert(string.Empty, typeof(string), null, null));
		}
	}
}
