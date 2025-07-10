using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{

	public class TariffColumn2DataRow : FlatFileDataRow
	{
		const int NumberOfFields = 11;

		public TariffColumn2DataRow(string lineData) : base(NumberOfFields, lineData)
		{
			SchemaList.Add(Schema.TariffCode);
			SchemaList.Add(Schema.CountryCode);
			SchemaList.Add(Schema.EndDate);
			SchemaList.Add(Schema.StartDate);
			SchemaList.Add(Schema.SpecificRate);
			SchemaList.Add(Schema.AdValoremRate);
			SchemaList.Add(Schema.ProvisionalEndDate);
			SchemaList.Add(Schema.ProvisionalStartDate);
			SchemaList.Add(Schema.ProvisionalSpecificRate);
			SchemaList.Add(Schema.ProvisionalAdValoremRate);
			SchemaList.Add(Schema.Region);
			SetFieldProperties(lineData);
		}

		#region Schema

		public class Schema
		{
			public static readonly FlatFileFieldProperty TariffCode = new FlatFileFieldProperty(0, 8);
			public static readonly FlatFileFieldProperty CountryCode = new FlatFileFieldProperty(1, 2);
			public static readonly FlatFileFieldProperty EndDate = new FlatFileFieldProperty(2, 8);
			public static readonly FlatFileFieldProperty StartDate = new FlatFileFieldProperty(3, 8);
			public static readonly FlatFileFieldProperty SpecificRate = new FlatFileFieldProperty(4, 10);
			public static readonly FlatFileFieldProperty AdValoremRate = new FlatFileFieldProperty(5, 10);
			public static readonly FlatFileFieldProperty ProvisionalEndDate = new FlatFileFieldProperty(6, 8);
			public static readonly FlatFileFieldProperty ProvisionalStartDate = new FlatFileFieldProperty(7, 8);
			public static readonly FlatFileFieldProperty ProvisionalSpecificRate = new FlatFileFieldProperty(8, 10);
			public static readonly FlatFileFieldProperty ProvisionalAdValoremRate = new FlatFileFieldProperty(9, 10);
			public static readonly FlatFileFieldProperty Region = new FlatFileFieldProperty(10, 10);
		}

		#endregion

		#region Properties

		public string TariffCode
		{
			get { return this[Schema.TariffCode.Name]; }
			set { SetField(Schema.TariffCode, value); }
		}

		public string CountryCode
		{
			get { return this[Schema.CountryCode.Name]; }
			set { SetField(Schema.CountryCode, value); }
		}

		public DateTime EndDate
		{
			get { return GetFieldAsEndDateTime(Schema.EndDate.Name, "yyyyMMdd"); }
			set { SetField(Schema.EndDate, value, "yyyyMMdd"); }
		}

		public DateTime StartDate
		{
			get { return GetFieldAsDateTime(Schema.StartDate.Name, "yyyyMMdd"); }
			set { SetField(Schema.StartDate, value, "yyyyMMdd"); }
		}

		public decimal SpecificRate
		{
			get { return GetFixedFieldAsDecimal(Schema.SpecificRate.Name, 5); }
			set { SetFixedDecimalField(Schema.SpecificRate, value, 5); }
		}

		public decimal AdValoremRate
		{
			get { return GetFixedFieldAsDecimal(Schema.AdValoremRate.Name, 5); }
			set { SetFixedDecimalField(Schema.AdValoremRate, value, 5); }
		}

		public decimal ProvisionalSpecificRate
		{
			get { return GetFixedFieldAsDecimal(Schema.ProvisionalSpecificRate.Name, 5); }
			set { SetFixedDecimalField(Schema.ProvisionalSpecificRate, value, 5); }
		}

		public decimal ProvisionalAdValoremRate
		{
			get { return GetFixedFieldAsDecimal(Schema.ProvisionalAdValoremRate.Name, 5); }
			set { SetFixedDecimalField(Schema.ProvisionalAdValoremRate, value, 5); }
		}

		public DateTime ProvisionalEndDate
		{
			get { return GetFieldAsEndDateTime(Schema.ProvisionalEndDate.Name, "yyyyMMdd"); }
			set { SetField(Schema.ProvisionalEndDate, value, "yyyyMMdd"); }
		}

		public DateTime ProvisionalStartDate
		{
			get { return GetFieldAsDateTime(Schema.ProvisionalStartDate.Name, "yyyyMMdd"); }
			set { SetField(Schema.ProvisionalStartDate, value, "yyyyMMdd"); }
		}

		public string Region
		{
			get { return this[Schema.Region.Name]; }
			set { SetField(Schema.Region, value); }
		}

		public bool IsLdcs
		{
			get { return Region == "LDCs"; }
		}

		private string tradeGroup;
		public string TradeGroup
		{
			get
			{
				return tradeGroup ?? (tradeGroup = CountryCode);
			}
			set
			{
				tradeGroup = value;
			}
		}

		public List<string> ExcludeTradeGroup { get; set; }

		public List<string> IncludeTradeGroup { get; set; }

		public string OrderNumber { get; set; }

		public bool IsOverridden { get; set; }

		public string AdValoremRateFormula()
		{
			return GetDTARateFormula(Schema.AdValoremRate.Name, AdValoremRate);
		}

		public string SpecificRateFormula(string specificRateUnit)
		{
			return GetDTSRateFormula(Schema.SpecificRate.Name, SpecificRate, specificRateUnit);
		}

		public string ProvisionalAdValoremRateFormula()
		{
			return GetDTARateFormula(Schema.ProvisionalAdValoremRate.Name, ProvisionalAdValoremRate);
		}

		public string ProvisionalSpecificRateFormula(string specificRateUnit)
		{
			return GetDTSRateFormula(Schema.ProvisionalSpecificRate.Name, ProvisionalSpecificRate, specificRateUnit);
		}

		#region RateFormulaDerivedFrom
		public string AdValoremRateFormulaDerivedFrom()
		{
			return GetDTARateFormulaDerivedFrom(Schema.AdValoremRate.Name, AdValoremRate);
		}

		public string SpecificRateFormulaDerivedFrom(string specificRateUnit)
		{
			return GetDTSRateFormulaDerivedFrom(Schema.SpecificRate.Name, SpecificRate, specificRateUnit);
		}

		public string ProvisionalAdValoremRateFormulaDerivedFrom()
		{
			return GetDTARateFormulaDerivedFrom(Schema.ProvisionalAdValoremRate.Name, ProvisionalAdValoremRate);
		}

		public string ProvisionalSpecificRateFormulaDerivedFrom(string specificRateUnit)
		{
			return GetDTSRateFormulaDerivedFrom(Schema.ProvisionalSpecificRate.Name, ProvisionalSpecificRate, specificRateUnit);
		}
		#endregion

		#endregion
	}

	public class TariffColumn2DataRowGroup
	{
		public List<TariffColumn2DataRow> tariffColumn2DataRows = new List<TariffColumn2DataRow>();

		protected TariffColumn2DataRow GetFirstOrDefault()
		{
			return tariffColumn2DataRows.Count > 0 ? tariffColumn2DataRows[0] : null;
		}

		public string TariffCode => GetFirstOrDefault()?.TariffCode ?? string.Empty;

		public string CountryCode => GetFirstOrDefault()?.CountryCode ?? string.Empty;

		public DateTime EndDate => GetFirstOrDefault()?.EndDate ?? DateTime.MinValue;

		public DateTime StartDate => GetFirstOrDefault()?.StartDate ?? DateTime.MinValue;

		public decimal SpecificRate => GetFirstOrDefault()?.SpecificRate ?? 0m;

		public decimal AdValoremRate => GetFirstOrDefault()?.AdValoremRate ?? 0m;

		public decimal ProvisionalSpecificRate => GetFirstOrDefault()?.ProvisionalSpecificRate ?? 0m;

		public decimal ProvisionalAdValoremRate => GetFirstOrDefault()?.ProvisionalAdValoremRate ?? 0m;

		public DateTime ProvisionalEndDate => GetFirstOrDefault()?.ProvisionalEndDate ?? DateTime.MinValue;

		public DateTime ProvisionalStartDate => GetFirstOrDefault()?.ProvisionalStartDate ?? DateTime.MinValue;

		public string AdValoremRateFormula() => GetFirstOrDefault()?.AdValoremRateFormula() ?? string.Empty;

		public string SpecificRateFormula(string specificRateUnit) => GetFirstOrDefault()?.SpecificRateFormula(specificRateUnit) ?? string.Empty;

		public string ProvisionalAdValoremRateFormula() => GetFirstOrDefault()?.ProvisionalAdValoremRateFormula() ?? string.Empty;

		public string ProvisionalSpecificRateFormula(string specificRateUnit) => GetFirstOrDefault()?.ProvisionalSpecificRateFormula(specificRateUnit) ?? string.Empty;

		public string AdValoremRateFormulaDerivedFrom() => GetFirstOrDefault()?.AdValoremRateFormulaDerivedFrom() ?? string.Empty;

		public string SpecificRateFormulaDerivedFrom(string specificRateUnit) => GetFirstOrDefault()?.SpecificRateFormulaDerivedFrom(specificRateUnit) ?? string.Empty;

		public string ProvisionalAdValoremRateFormulaDerivedFrom() => GetFirstOrDefault()?.ProvisionalAdValoremRateFormulaDerivedFrom() ?? string.Empty;

		public string ProvisionalSpecificRateFormulaDerivedFrom(string specificRateUnit) => GetFirstOrDefault()?.ProvisionalSpecificRateFormulaDerivedFrom(specificRateUnit) ?? string.Empty;
	}
}
