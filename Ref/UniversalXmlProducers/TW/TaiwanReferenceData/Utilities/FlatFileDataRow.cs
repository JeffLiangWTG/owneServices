using System;
using System.Collections.Generic;
using System.Globalization;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public class FlatFileDataRow
	{
		protected string[] DataRow;
		protected string originalLine;

		public FlatFileDataRow(int fieldCount, string lineData)
		{
			DataRow = new string[fieldCount];
			originalLine = lineData;
		}

		List<FlatFileFieldProperty> schemaList;
		protected List<FlatFileFieldProperty> SchemaList
		{
			get
			{
				return schemaList ?? (schemaList = new List<FlatFileFieldProperty>());
			}
		}

		public string LineData => originalLine;

		public string this[int position]
		{
			get { return GetField(position); }
		}

		public string GetField(int position)
		{
			return GetFieldCore(position);
		}

		protected virtual string GetFieldCore(int position)
		{
			string Result = string.Empty;

			if (position < DataRow.Length)
			{
				Result = DataRow[position];
			}

			return Result;
		}

		public int GetFieldAsInt(int position)
		{
			return Utility.GetValueAsInt(GetFieldCore(position));
		}

		public decimal GetFixedFieldAsDecimal(int position, int decimalPlaces)
		{
			return GetFieldAsDecimal(position) / Convert.ToDecimal(Math.Pow(10, decimalPlaces), CultureInfo.InvariantCulture);
		}

		public decimal GetFieldAsDecimal(int position)
		{
			return Utility.GetValueAsDecimal(GetFieldCore(position));
		}

		public DateTime GetFieldAsDateTime(int position, string format)
		{
			if (!DateTime.TryParseExact(GetFieldCore(position), format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime Result))
			{
				Result = GetMinOrMaxDateTime(position);
			}

			return Result;
		}

		public DateTime GetFieldAsEndDateTime(int position, string format)
		{
			if (DateTime.TryParseExact(GetFieldCore(position), format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime Result))
			{
				Result += new TimeSpan(23, 59, 59);
			}
			else
			{
				Result = GetMinOrMaxDateTime(position);
			}
			return Result;
		}

		DateTime GetMinOrMaxDateTime(int position) => GetFieldCore(position).CompareTo("20790606") > 0 ? new DateTime(2079, 06, 06, 23, 59, 00) : DateTime.MinValue;

		protected virtual void SetFieldProperties(string lineData)
		{
			int position = 0;
			for (int index = 0; index < DataRow.Length; index++)
			{
				DataRow[index] = lineData.SafeSubstring(position, SchemaList[index].Length).Trim();
				position += SchemaList[index].Length;
			}
		}


		public void SetField(int position, string value)
		{
			SetFieldCore(position, value);
		}

		public void SetField(FlatFileFieldProperty FieldProperty, string value)
		{
			SetField(FieldProperty.Name, value.Substring(0, FieldProperty.Length));
		}

		public void SetField(FlatFileFieldProperty FieldProperty, int value)
		{
			SetField(FieldProperty.Name, value.ToString(new string('0', FieldProperty.Length), CultureInfo.InvariantCulture));
		}

		public void SetField(FlatFileFieldProperty FieldProperty, decimal value)
		{
			SetField(FieldProperty.Name, value.ToString(CultureInfo.InvariantCulture));
		}

		public void SetFixedDecimalField(FlatFileFieldProperty FieldProperty, decimal value, int decimalPlaces)
		{
			SetField(FieldProperty.Name, (value * Convert.ToDecimal(Math.Pow(10, decimalPlaces))).ToString(CultureInfo.InvariantCulture));
		}

		protected virtual void SetFieldCore(int position, string value)
		{
			DataRow[position] = value;
		}

		public void SetField(FlatFileFieldProperty FieldProperty, DateTime value, string format)
		{
			SetFieldCore(FieldProperty.Name, value.ToString(format, CultureInfo.InvariantCulture));
		}

		public void SetField(int position, DateTime value, string format)
		{
			SetFieldCore(position, value.ToString(format, CultureInfo.InvariantCulture));
		}

		public string GetDTARateFormula(int position, decimal rate)
		{
			return !string.IsNullOrWhiteSpace(GetFieldCore(position)) ? (rate != 0 ? string.Format(CultureInfo.InvariantCulture, "{0}*VFD", rate) : "0") : string.Empty;
		}

		public string GetDTSRateFormula(int position, decimal rate, string unit)
		{
			return !string.IsNullOrWhiteSpace(GetFieldCore(position)) ? string.Format(CultureInfo.InvariantCulture, "{0}*[{1}]", rate, unit) : string.Empty;
		}

		public string GetDTARateFormulaDerivedFrom(int position, decimal rate)
		{
			return !string.IsNullOrWhiteSpace(GetFieldCore(position)) ? (rate != 0 ? string.Format(CultureInfo.InvariantCulture, "{0}", rate) : "0") : string.Empty;
		}

		public string GetDTSRateFormulaDerivedFrom(int position, decimal rate, string unit)
		{
			return !string.IsNullOrWhiteSpace(GetFieldCore(position)) ? string.Format(CultureInfo.InvariantCulture, "{0}/{1}", rate, unit) : string.Empty;
		}
	}
}
