using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public abstract class DataLoadWithFlexibleColumns : DataLoad
	{
		protected DataLoadWithFlexibleColumns()
		{
		}

		protected override void ImportComplete()
		{
			base.ImportComplete();
			Factory.Save();
		}

		protected bool HasColumn(string columnName)
		{
			return columns.ContainsKey(columnName);
		}

		bool GetBooleanValue(OCsvLine line, string columnName)
		{
			ZString result = GetFieldValue(line, columnName);
			return result.EqualsIgnoringCase("Y");
		}

		ZDecimal GetDecimalValue(OCsvLine line, string columnName, int maximumDigitsBeforeDecimal, int maximumDecimalPlaces, bool checkDecimalPlaces)
		{
			var value = GetFieldValue(line, columnName);
			var result = ZDecimal.ParseSafe(value, 0);

			int precision = maximumDigitsBeforeDecimal + (maximumDecimalPlaces > 0 ? maximumDecimalPlaces : 0);
			int scale = maximumDecimalPlaces;
			if (!result.IsWithinSqlPrecisionAndScale(precision, scale))
			{
				throw new ArgumentOutOfRangeException("Value of " + value + " is too large to store in " + columnName);
			}
			else if (checkDecimalPlaces && result.DecimalPlaces > maximumDecimalPlaces)
			{
				throw new ArgumentOutOfRangeException("Value of " + value + " has too many decimal places to store in " + columnName);
			}

			return result;
		}

		ZDate GetDateValue(OCsvLine line, string columnName)
		{
			var value = GetFieldValue(line, columnName);
			if (!ZDateTime.TryParseExact(value, out ZDateTime result, "yyyyMMdd"))
			{
				throw new ArgumentOutOfRangeException("Invalid Date format for " + value + ", cannot save to " + columnName + ". The expected format is 'yyyyMMdd'.");
			}
			return result.Date;
		}

		ZDateTime GetDateTimeValue(OCsvLine line, string columnName)
		{
			var value = GetFieldValue(line, columnName);
			if (!ZDateTime.TryParseExact(value, out ZDateTime result, "yyyyMMddHHmm"))
			{
				if (!ZDateTime.TryParseExact(value, out result, "yyyyMMdd"))
				{
					throw new ArgumentOutOfRangeException("Invalid DateTime format for " + value + ", cannot save to " + columnName + ". The expected format is 'yyyyMMddHHmm' or 'yyyyMMdd'.");
				}
			}
			return result;
		}

		ZString GetFieldValue(OCsvLine line, string columnName, int maxLength = 0)
		{
			if (columns.TryGetValue(columnName, out int index) && line.FieldValues.Length > index)
			{
				var result = line.FieldValues[index].Trim();
				if (maxLength > 0 && result.Length > maxLength)
				{
					throw new ArgumentOutOfRangeException(FormattableString.Invariant($"Value of {columnName} exceeds the max length({maxLength}): {result}"));
				}
				return result;
			}
			else
			{
				return ZString.Empty;
			}
		}

		protected ZString TryGetStringValue(OCsvLine line, string columnName, int maxLength = 0) => GetFieldValue(line, columnName, maxLength);

		protected ZDecimal TryGetDecimalValue(OCsvLine line, string columnName, int maximumDigitsBeforeDecimal, int maximumDecimalPlaces, bool checkDecimalPlaces = false)
		{
			return HasColumn(columnName) ? GetDecimalValue(line, columnName, maximumDigitsBeforeDecimal, maximumDecimalPlaces, checkDecimalPlaces) : ZDecimal.Zero;
		}

		protected ZByte TryGetByteValue(OCsvLine line, string columnName, byte maximumAllowedByteColumnValue)
		{
			return HasColumn(columnName) ? GetByteValue(line, columnName, maximumAllowedByteColumnValue) : ZByte.Zero;
		}

		protected ZDate? TryGetDateValue(OCsvLine line, string columnName)
		{
			if (HasColumn(columnName))
			{
				return GetDateValue(line, columnName);
			}
			return null;
		}

		protected bool TryGetValue<T>(OCsvLine line, string columnName, out T columnToSet)
		{
			if (HasColumn(columnName))
			{
				columnToSet = (T)Convert.ChangeType(TypeLoader[typeof(T)](line, columnName), typeof(T), CultureInfo.InvariantCulture);
				return true;
			}
			else
			{
				columnToSet = default(T);
				return false;
			}
		}

		protected object GetValueIsExists(OCsvLine line, string columnName, Type type)
		{
			return HasColumn(columnName) ? GetValue(line, columnName, type) : null;
		}

		protected object GetValue(OCsvLine line, string columnName, Type type)
		{
			return TypeLoader[type](line, columnName);
		}

		Dictionary<Type, Func<OCsvLine, string, object>> TypeLoader => new Dictionary<Type, Func<OCsvLine, string, object>>
		{
			{ typeof(string), (line, columnName) => GetFieldValue(line, columnName) },
			{ typeof(ZString), (line, columnName) => GetFieldValue(line, columnName) },
			{ typeof(ZString?), (line, columnName) => GetFieldValue(line, columnName) },
			{ typeof(bool), (line, columnName) => GetBooleanValue(line, columnName) },
			{ typeof(ZBool), (line, columnName) => GetBooleanValue(line, columnName) },
			{ typeof(ZBool?), (line, columnName) => GetBooleanValue(line, columnName) },
			{ typeof(ZInt), (line, columnName) => GetIntValue(line, columnName) },
			{ typeof(ZInt?), (line, columnName) => GetIntValue(line, columnName) },
			{ typeof(ZShort), (line, columnName) => GetShortValue(line, columnName) },
			{ typeof(ZShort?), (line, columnName) => GetShortValue(line, columnName) },
			{ typeof(ZDecimal), (line, columnName) => GetDecimalValue(line, columnName, 10, 5, false) },
			{ typeof(ZDecimal?), (line, columnName) => GetDecimalValue(line, columnName, 10, 5, false) },
			{ typeof(decimal), (line, columnName) => (decimal)GetDecimalValue(line, columnName, 10, 5, false) },
			{ typeof(ZDateTime), (line, columnName) => GetDateTimeValue(line, columnName) },
			{ typeof(ZDateTime?), (line, columnName) => GetDateTimeValue(line, columnName) },
		};

		ZInt GetIntValue(OCsvLine line, string columnName)
		{
			var value = GetFieldValue(line, columnName);
			if (!value.IsEmpty && !ZInt.CanParse(value))
			{
				throw new ArgumentOutOfRangeException(FormattableString.Invariant($"Value of {value} is invalid or too large to store in {columnName}"));
			}
			return ZInt.ParseSafe(value, 0);
		}

		ZShort GetShortValue(OCsvLine line, string columnName)
		{
			var value = GetFieldValue(line, columnName);
			var result = ZShort.Zero;
			if (!value.IsEmpty && !ZShort.TryParse(value, out result))
			{
				throw new ArgumentOutOfRangeException(FormattableString.Invariant($"Value of {value} is invalid or too large to store in {columnName}"));
			}
			return result;
		}

		ZByte GetByteValue(OCsvLine line, string columnName, byte maximumAllowedByteColumnValue)
		{
			var value = GetFieldValue(line, columnName);
			var result = ZByte.Zero;
			if ((!value.IsEmpty && !ZByte.TryParse(value, out result))
				|| result > maximumAllowedByteColumnValue)
			{
				throw new ArgumentOutOfRangeException(FormattableString.Invariant($"Value of {value} is invalid or too large to store in {columnName}"));
			}
			return result;
		}

		protected override void ParseHeaderLine(OCsvLine headerLine)
		{
			columns = new Dictionary<string, int>(headerLine.FieldValues.Length, StringComparer.OrdinalIgnoreCase);
			headerColumnCount = headerLine.FieldValues.Length;
			for (int i = 0; i < headerLine.FieldValues.Length; i++)
			{
				string columnHeading = headerLine.FieldValues[i].Trim();
				if (!string.IsNullOrEmpty(columnHeading))
				{
					if (columns.ContainsKey(columnHeading))
					{
						throw new ArgumentException(string.Format("Duplicate column (column number {0}) detected : " + columnHeading, i));
					}
					columns.Add(columnHeading, i);
				}
			}
		}
		int headerColumnCount;

		protected IEnumerable<string> ColumnNames
		{
			get { return columns.Keys; }
		}

		Dictionary<string, int> columns;

		protected void CheckElementCount(OCsvLine line)
		{
			if (line.FieldValues.Length > headerColumnCount)
			{
				throw new ArgumentOutOfRangeException(nameof(line), "Line has more data columns than header");
			}
		}

		protected sealed override bool IsFileHeaderValid(OCsvLine headerLine)
		{
			string templateHeading = CSVTemplateHeading;
			OCsvLine allHeadings = new OCsvLine(templateHeading);
			Dictionary<string, bool> allHeadingsDictionary = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
			foreach (string heading in allHeadings.FieldValues)
			{
				allHeadingsDictionary[heading] = true;
			}

			bool result = true;
			foreach (string heading in headerLine.FieldValues)
			{
				if (heading.Length > 0 && !allHeadingsDictionary.ContainsKey(heading.Trim()))
				{
					DisplayLogMessage(Res.GetString("f9fc3eb2-01e9-4388-b897-652a86e57097", "Unknown column heading : {0}", heading));
					result = false;
				}
			}

			if (result)
			{
				result = CheckMandatoryFields(allHeadingsDictionary, headerLine.FieldValues);
			}

			return result;
		}

		protected virtual bool CheckMandatoryFields(Dictionary<string, bool> headings, string[] headerLineValues)
		{
			return true;
		}
	}
}
