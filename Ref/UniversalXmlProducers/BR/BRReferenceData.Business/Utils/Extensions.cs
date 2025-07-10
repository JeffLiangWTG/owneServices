using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public static class Extensions
	{
		public static string GetElementValueAsString(this XElement element, string elementName, int maxLength)
		{
			var value = element?.Element(elementName)?.Value;
			if (maxLength == 0)
			{
				return value;
			}
			else if (value != null && value.Length > maxLength)
			{
				value = value.Substring(0, maxLength);
			}

			return value;
		}

		public static XElement GetElement(this XElement element, string elementName)
		{
			var value = element?.Element(elementName);
			return value;
		}

		public static DateTime GetElementValueAsDateTime(this XElement element, string elementName)
		{
			try
			{
				CultureInfo culture = new CultureInfo("pt-BR");
				var sDate = element?.Element(elementName)?.Value;
				return Convert.ToDateTime(sDate, culture);
			}
			catch (FormatException)
			{
				return DateTime.Now;
			}
		}

		public static DateTime? GetXlsValueAsDateTime(this XlsFile xls, int row, string column) => GetXlsValueAsDateTime(xls, row, ColumnNumber(column));

		public static DateTime? GetXlsValueAsDateTime(this XlsFile xls, int row, int column)
		{
			try
			{
				var dateOA = xls.GetCellValue(row, column);
				DateTime? resultDate;
				if (dateOA is double date)
				{
					resultDate = DateTime.FromOADate(date);
				}
				else
				{
					CultureInfo culture = new CultureInfo("pt-BR");
					resultDate = Convert.ToDateTime(dateOA?.ToString(), culture);
				}
				return resultDate?.Year > 2079 ? null : resultDate;
			}
			catch (FormatException)
			{
				return null;
			}
		}

		public static string GetXlsValueAsString(this XlsFile xls, int row, string column, string defaultValueIfEmpty = "") => GetXlsValueAsString(xls, row, ColumnNumber(column), defaultValueIfEmpty);

		public static string GetXlsValueAsString(this XlsFile xls, int row, int column, string defaultValueIfEmpty = "") => xls.GetCellValue(row, column)?.ToString() ?? defaultValueIfEmpty;

		public static decimal? GetXlsValueAsDecimal(this XlsFile xls, int row, int column)
		{
			var strValue = GetXlsValueAsString(xls, row, column);
			return decimal.TryParse(strValue, out var value) ? value : new decimal?();
		}

		public static string KeepNumericsOnly(this string input) => new Regex("[^0-9]").Replace(input, string.Empty);

		public static int ColumnNumber(string colAdress)
		{
			int[] digits = new int[colAdress.Length];
			for (int i = 0; i < colAdress.Length; ++i)
			{
				digits[i] = Convert.ToInt32(colAdress[i]) - 64;
			}
			int mul = 1;
			int res = 0;
			for (int pos = digits.Length - 1; pos >= 0; --pos)
			{
				res += digits[pos] * mul;
				mul *= 26;
			}
			return res;
		}
	}
}
