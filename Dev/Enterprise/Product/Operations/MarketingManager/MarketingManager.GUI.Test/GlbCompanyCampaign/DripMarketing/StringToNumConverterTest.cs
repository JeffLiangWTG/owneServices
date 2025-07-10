using System;
using System.Globalization;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Converters.Testing
{
	public class StringToNumConverterTest : TestCase
	{
		public void TestConvert_Exceptions()
		{
			var converter = new StringToNumConverter();

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
				converter.Convert(5, typeof(ZByte), null, CultureInfo.CurrentCulture);
			});

			AssertNoExceptionThrown(delegate
			{
				converter.Convert(string.Empty, typeof(ZByte), null, CultureInfo.CurrentCulture);
			});
		}

		public void TestConvertBack_Exceptions()
		{
			var converter = new StringToNumConverter();

			AssertExceptionThrown<InvalidOperationException>(delegate
			{
				converter.ConvertBack(new ZByte(1), null, null, CultureInfo.CurrentCulture);
			});

			AssertExceptionThrown<InvalidOperationException>(delegate
			{
				converter.ConvertBack(new ZByte(1), typeof(ZInt), null, CultureInfo.CurrentCulture);
			});

			AssertExceptionThrown<InvalidOperationException>(delegate
			{
				converter.ConvertBack("abc", typeof(string), null, CultureInfo.CurrentCulture);
			});

			AssertNoExceptionThrown(delegate
			{
				converter.ConvertBack(new ZByte(1), typeof(string), null, CultureInfo.CurrentCulture);
			});
		}

		public void TestConvert()
		{
			var converter = new StringToNumConverter();

			AssertEquals(ZByte.Zero, converter.Convert(string.Empty, typeof(ZByte), null, CultureInfo.CurrentCulture));
			AssertEquals(ZByte.Zero, converter.Convert("abc", typeof(ZByte), null, CultureInfo.CurrentCulture));
			AssertEquals(new ZByte(4), converter.Convert("4", typeof(ZByte), null, CultureInfo.CurrentCulture));
		}

		public void TestConvertBack()
		{
			var converter = new StringToNumConverter();

			AssertEquals("0", converter.ConvertBack(ZByte.Zero, typeof(string), null, CultureInfo.CurrentCulture));
			AssertEquals("3", converter.ConvertBack(new ZByte(3), typeof(string), null, CultureInfo.CurrentCulture));
		}
	}
}
