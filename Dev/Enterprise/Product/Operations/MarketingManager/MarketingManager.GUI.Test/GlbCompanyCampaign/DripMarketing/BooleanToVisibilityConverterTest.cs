using System;
using System.Globalization;
using System.Windows;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public class BooleanToVisibilityConverterTest : TestCase
	{
		public void TestConvert_Exceptions()
		{
			var converter = new BooleanToVisibilityConverter();

			AssertExceptionThrown<InvalidOperationException>(delegate
			{
				converter.Convert(string.Empty, null, null, CultureInfo.CurrentCulture);
			});

			AssertExceptionThrown<InvalidOperationException>(delegate
			{
				converter.Convert(string.Empty, typeof(ZInt), null, CultureInfo.CurrentCulture);
			});

			AssertExceptionThrown<InvalidOperationException>(delegate
			{
				converter.Convert(5, typeof(Visibility), null, CultureInfo.CurrentCulture);
			});

			AssertNoExceptionThrown(delegate
			{
				converter.Convert(true, typeof(Visibility), null, CultureInfo.CurrentCulture);
			});
		}

		public void TestConvertBack_Exceptions()
		{
			var converter = new BooleanToVisibilityConverter();

			AssertExceptionThrown<InvalidOperationException>(delegate
			{
				converter.ConvertBack(true, null, null, CultureInfo.CurrentCulture);
			});

			AssertExceptionThrown<InvalidOperationException>(delegate
			{
				converter.ConvertBack(true, typeof(ZInt), null, CultureInfo.CurrentCulture);
			});

			AssertExceptionThrown<InvalidOperationException>(delegate
			{
				converter.ConvertBack("abc", typeof(Visibility), null, CultureInfo.CurrentCulture);
			});

			AssertNoExceptionThrown(delegate
			{
				converter.ConvertBack(Visibility.Hidden, typeof(bool), null, CultureInfo.CurrentCulture);
			});
		}

		public void TestConvert()
		{
			var converter = new BooleanToVisibilityConverter();

			AssertEquals(Visibility.Visible, converter.Convert(true, typeof(Visibility), null, CultureInfo.CurrentCulture));
			AssertEquals(Visibility.Hidden, converter.Convert(false, typeof(Visibility), null, CultureInfo.CurrentCulture));
		}

		public void TestConvertBack()
		{
			var converter = new BooleanToVisibilityConverter();

			AssertEquals(true, converter.ConvertBack(Visibility.Visible, typeof(bool), null, CultureInfo.CurrentCulture));
			AssertEquals(false, converter.ConvertBack(Visibility.Hidden, typeof(bool), null, CultureInfo.CurrentCulture));
		}
	}
}
