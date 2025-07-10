using System;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public static class DateTimeConverter
	{
		public static ZDateTime ConvertToZDateTime(this DateTime? value) => value.HasValue ? ConvertToZDateTime(value.Value) : ZDateTime.Empty;

		public static ZDateTime ConvertToZDateTime(this DateTime value) => value.Equals(DateTime.MinValue) ? ZDateTime.Empty : new ZDateTime(value);

		public static ZDate ConvertToZDate(this DateTime? value) => value.HasValue ? ConvertToZDate(value.Value) : ZDate.Empty;

		public static ZDate ConvertToZDate(this DateTime value) => value.Equals(DateTime.MinValue) ? ZDate.Empty : new ZDate(value);
	}
}
