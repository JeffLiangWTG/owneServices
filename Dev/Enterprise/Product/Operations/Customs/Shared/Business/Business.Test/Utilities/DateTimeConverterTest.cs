using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class DateTimeConverterTest : TestCase
	{
		public void TestConvertToZDateTime()
		{
			var dateToConvert = DateTime.MinValue;
			AssertEquals("MinValue", ZDateTime.Empty, dateToConvert.ConvertToZDateTime());
			dateToConvert = new DateTime(2020, 5, 21);
			AssertEquals("MinValue", new ZDateTime(2020, 5, 21), dateToConvert.ConvertToZDateTime());

			var nullableDateToConvert = new DateTime?();
			AssertEquals("Null", ZDateTime.Empty, nullableDateToConvert.ConvertToZDateTime());
			nullableDateToConvert = dateToConvert;
			AssertEquals("MinValue", new ZDateTime(2020, 5, 21), dateToConvert.ConvertToZDateTime());
		}

		public void TestConvertToZDate()
		{
			var dateToConvert = DateTime.MinValue;
			AssertEquals("MinValue", ZDate.Empty, dateToConvert.ConvertToZDateTime());
			dateToConvert = new DateTime(2020, 5, 21);
			AssertEquals("MinValue", new ZDate(2020, 5, 21), dateToConvert.ConvertToZDateTime());

			var nullableDateToConvert = new DateTime?();
			AssertEquals("Null", ZDate.Empty, nullableDateToConvert.ConvertToZDateTime());
			nullableDateToConvert = dateToConvert;
			AssertEquals("MinValue", new ZDate(2020, 5, 21), dateToConvert.ConvertToZDateTime());
		}
	}
}
