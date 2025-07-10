using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal
{
	public static class Utils
	{
		public static string SafeGetString(this IDataReader reader, string name)
		{
			var colIndex = reader.GetOrdinal(name);
			return reader.IsDBNull(colIndex) ? string.Empty : reader.GetString(colIndex);
		}

		public static int GetIntegerNumber(string integerString, FormulaErrorListener errorListener)
		{
			int result = 0;
			var isConverted = int.TryParse(integerString, out result);
			if (!isConverted)
			{
				errorListener.Report(FormulaVisitErrorType.SyntaxError, string.Format(CultureInfo.InvariantCulture, (NoResString)"Failed to convert \"{0}\" to Integer", integerString));
			}
			return result;
		}

		public static void AddNewKeyOrAccumulateValue(this IDictionary<string, decimal> dictionary, string key, decimal value)
		{
			dictionary[key] = dictionary.TryGetValue(key, out var oldValue) ? oldValue + value : value;
		}

		/// <summary>
		/// Converts a date to an integer, following the international format yyyymmdd.
		/// yyyy0000 (year * 10000) +
		///     mm00 (month * 100) +
		///		  dd (day)
		/// </summary>
		public static int ToIsoDateOnlyNumericValue(this DateTime date)
		{
			return (date.Year * 10000) + (date.Month * 100) + (date.Day);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Const string for building xml")]
		internal static string GetXmlForValueList(string[] values)
		{
			if (values == null || values.All(x => x == null))
			{
				values = new[] { string.Empty };
			}

			return string.Join(string.Empty, values.Where(x => x != null).Distinct().OrderBy(x => x).Select(x => new XElement("v", x).ToString()));
		}

		internal static DataTable GetTvpTableForValueList(string[] values)
		{
			if (values == null || values.All(x => x == null))
			{
				values = new[] { string.Empty };
			}

			var tvp = new DataTable();
			tvp.Columns.Add(new DataColumn((NoResString)"Value", typeof(string)));  // dbo.TVP_nvarchar_15 column name

			foreach (var value in values.Where(x => x != null).Distinct().OrderBy(x => x))
			{
				tvp.Rows.Add(value);
			}

			return tvp;
		}
	}
}
