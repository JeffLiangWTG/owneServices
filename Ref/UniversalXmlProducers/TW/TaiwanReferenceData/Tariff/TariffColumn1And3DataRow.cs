using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public class TariffColumn1And3DataRow : FlatFileDataRow
	{
		const int NumberOfFields = 31;

		public TariffColumn1And3DataRow(string lineData) : base(NumberOfFields, lineData)
		{
			SchemaList.Add(Schema.TariffCode);
			SchemaList.Add(Schema.EndDate);
			SchemaList.Add(Schema.StartDate);
			SchemaList.Add(Schema.Column3SpecificRate);
			SchemaList.Add(Schema.Column3AdValoremRate);
			SchemaList.Add(Schema.Column1SpecificRate);
			SchemaList.Add(Schema.Column1AdValoremRate);
			SchemaList.Add(Schema.SpecificRateUnit);
			SchemaList.Add(Schema.Column3ProvisionalSpecificRate);
			SchemaList.Add(Schema.Column3ProvisionalAdValoremRate);
			SchemaList.Add(Schema.Column1ProvisionalSpecificRate);
			SchemaList.Add(Schema.Column1ProvisionalAdValoremRate);
			SchemaList.Add(Schema.ProvisionalEndDate);
			SchemaList.Add(Schema.ProvisionalStartDate);
			SchemaList.Add(Schema.QuantityUnit);
			SchemaList.Add(Schema.WeightUnit);
			SchemaList.Add(Schema.CustomsRequirementCode);
			SchemaList.Add(Schema.ImportRegulationCode1);
			SchemaList.Add(Schema.ImportRegulationCode2);
			SchemaList.Add(Schema.ImportRegulationCode3);
			SchemaList.Add(Schema.ImportRegulationCode4);
			SchemaList.Add(Schema.ImportRegulationCode5);
			SchemaList.Add(Schema.ImportRegulationCode6);
			SchemaList.Add(Schema.ImportRegulationCode7);
			SchemaList.Add(Schema.ExportRegulationCode1);
			SchemaList.Add(Schema.ExportRegulationCode2);
			SchemaList.Add(Schema.ExportRegulationCode3);
			SchemaList.Add(Schema.ExportRegulationCode4);
			SchemaList.Add(Schema.ExportRegulationCode5);
			SchemaList.Add(Schema.ExportRegulationCode6);
			SchemaList.Add(Schema.ExportRegulationCode7);
			SetFieldProperties(lineData);
		}

		#region Schema

		class Schema
		{
			public static readonly FlatFileFieldProperty TariffCode = new FlatFileFieldProperty(0, 11);
			public static readonly FlatFileFieldProperty EndDate = new FlatFileFieldProperty(1, 8);
			public static readonly FlatFileFieldProperty StartDate = new FlatFileFieldProperty(2, 8);
			public static readonly FlatFileFieldProperty Column3SpecificRate = new FlatFileFieldProperty(3, 10);
			public static readonly FlatFileFieldProperty Column3AdValoremRate = new FlatFileFieldProperty(4, 10);
			public static readonly FlatFileFieldProperty Column1SpecificRate = new FlatFileFieldProperty(5, 10);
			public static readonly FlatFileFieldProperty Column1AdValoremRate = new FlatFileFieldProperty(6, 10);
			public static readonly FlatFileFieldProperty SpecificRateUnit = new FlatFileFieldProperty(7, 3);
			public static readonly FlatFileFieldProperty Column3ProvisionalSpecificRate = new FlatFileFieldProperty(8, 10);
			public static readonly FlatFileFieldProperty Column3ProvisionalAdValoremRate = new FlatFileFieldProperty(9, 10);
			public static readonly FlatFileFieldProperty Column1ProvisionalSpecificRate = new FlatFileFieldProperty(10, 10);
			public static readonly FlatFileFieldProperty Column1ProvisionalAdValoremRate = new FlatFileFieldProperty(11, 10);
			public static readonly FlatFileFieldProperty ProvisionalEndDate = new FlatFileFieldProperty(12, 8);
			public static readonly FlatFileFieldProperty ProvisionalStartDate = new FlatFileFieldProperty(13, 8);
			public static readonly FlatFileFieldProperty QuantityUnit = new FlatFileFieldProperty(14, 3);
			public static readonly FlatFileFieldProperty WeightUnit = new FlatFileFieldProperty(15, 3);
			public static readonly FlatFileFieldProperty CustomsRequirementCode = new FlatFileFieldProperty(16, 12);
			public static readonly FlatFileFieldProperty ImportRegulationCode1 = new FlatFileFieldProperty(17, 4);
			public static readonly FlatFileFieldProperty ImportRegulationCode2 = new FlatFileFieldProperty(18, 4);
			public static readonly FlatFileFieldProperty ImportRegulationCode3 = new FlatFileFieldProperty(19, 4);
			public static readonly FlatFileFieldProperty ImportRegulationCode4 = new FlatFileFieldProperty(20, 4);
			public static readonly FlatFileFieldProperty ImportRegulationCode5 = new FlatFileFieldProperty(21, 4);
			public static readonly FlatFileFieldProperty ImportRegulationCode6 = new FlatFileFieldProperty(22, 4);
			public static readonly FlatFileFieldProperty ImportRegulationCode7 = new FlatFileFieldProperty(23, 4);
			public static readonly FlatFileFieldProperty ExportRegulationCode1 = new FlatFileFieldProperty(24, 4);
			public static readonly FlatFileFieldProperty ExportRegulationCode2 = new FlatFileFieldProperty(25, 4);
			public static readonly FlatFileFieldProperty ExportRegulationCode3 = new FlatFileFieldProperty(26, 4);
			public static readonly FlatFileFieldProperty ExportRegulationCode4 = new FlatFileFieldProperty(27, 4);
			public static readonly FlatFileFieldProperty ExportRegulationCode5 = new FlatFileFieldProperty(28, 4);
			public static readonly FlatFileFieldProperty ExportRegulationCode6 = new FlatFileFieldProperty(29, 4);
			public static readonly FlatFileFieldProperty ExportRegulationCode7 = new FlatFileFieldProperty(30, 4);
		}

		#endregion

		#region Properties

		public string TariffCode
		{
			get { return this[Schema.TariffCode.Name]; }
		}

		public DateTime EndDate
		{
			get { return GetFieldAsEndDateTime(Schema.EndDate.Name, "yyyyMMdd"); }
		}

		public DateTime StartDate
		{
			get { return GetFieldAsDateTime(Schema.StartDate.Name, "yyyyMMdd"); }
		}

		public decimal Column3SpecificRate
		{
			get { return GetFixedFieldAsDecimal(Schema.Column3SpecificRate.Name, 5); }
		}

		public decimal Column3AdValoremRate
		{
			get { return GetFixedFieldAsDecimal(Schema.Column3AdValoremRate.Name, 5); }
		}

		public decimal Column1SpecificRate
		{
			get { return GetFixedFieldAsDecimal(Schema.Column1SpecificRate.Name, 5); }
		}

		public decimal Column1AdValoremRate
		{
			get { return GetFixedFieldAsDecimal(Schema.Column1AdValoremRate.Name, 5); }
		}

		public string SpecificRateUnit
		{
			get { return this[Schema.SpecificRateUnit.Name]; }
		}

		public decimal Column3ProvisionalSpecificRate
		{
			get { return GetFixedFieldAsDecimal(Schema.Column3ProvisionalSpecificRate.Name, 5); }
		}

		public decimal Column3ProvisionalAdValoremRate
		{
			get { return GetFixedFieldAsDecimal(Schema.Column3ProvisionalAdValoremRate.Name, 5); }
		}

		public decimal Column1ProvisionalSpecificRate
		{
			get { return GetFixedFieldAsDecimal(Schema.Column1ProvisionalSpecificRate.Name, 5); }
		}

		public decimal Column1ProvisionalAdValoremRate
		{
			get { return GetFixedFieldAsDecimal(Schema.Column1ProvisionalAdValoremRate.Name, 5); }
		}

		public DateTime ProvisionalEndDate
		{
			get { return GetFieldAsEndDateTime(Schema.ProvisionalEndDate.Name, "yyyyMMdd"); }
		}

		public DateTime ProvisionalStartDate
		{
			get { return GetFieldAsDateTime(Schema.ProvisionalStartDate.Name, "yyyyMMdd"); }
		}

		public string QuantityUnit
		{
			get { return this[Schema.QuantityUnit.Name]; }
		}

		public string WeightUnit
		{
			get { return this[Schema.WeightUnit.Name]; }
		}

		public string CustomsRequirementCode
		{
			get { return this[Schema.CustomsRequirementCode.Name]; }
		}

		public string ImportRegulationCode1
		{
			get { return this[Schema.ImportRegulationCode1.Name]; }
		}

		public string ImportRegulationCode2
		{
			get { return this[Schema.ImportRegulationCode2.Name]; }
		}

		public string ImportRegulationCode3
		{
			get { return this[Schema.ImportRegulationCode3.Name]; }
		}

		public string ImportRegulationCode4
		{
			get { return this[Schema.ImportRegulationCode4.Name]; }
		}

		public string ImportRegulationCode5
		{
			get { return this[Schema.ImportRegulationCode5.Name]; }
		}

		public string ImportRegulationCode6
		{
			get { return this[Schema.ImportRegulationCode6.Name]; }
		}

		public string ImportRegulationCode7
		{
			get { return this[Schema.ImportRegulationCode7.Name]; }
		}

		public string ExportRegulationCode1
		{
			get { return this[Schema.ExportRegulationCode1.Name]; }
		}

		public string ExportRegulationCode2
		{
			get { return this[Schema.ExportRegulationCode2.Name]; }
		}

		public string ExportRegulationCode3
		{
			get { return this[Schema.ExportRegulationCode3.Name]; }
		}

		public string ExportRegulationCode4
		{
			get { return this[Schema.ExportRegulationCode4.Name]; }
		}

		public string ExportRegulationCode5
		{
			get { return this[Schema.ExportRegulationCode5.Name]; }
		}

		public string ExportRegulationCode6
		{
			get { return this[Schema.ExportRegulationCode6.Name]; }
		}

		public string ExportRegulationCode7
		{
			get { return this[Schema.ExportRegulationCode7.Name]; }
		}

		public List<string> CustomsRequirementList
		{
			get
			{
				if (string.IsNullOrEmpty(CustomsRequirementCode))
				{
					return new List<string>();
				}
				else
				{
					return new List<string>(CustomsRequirementCode.Trim().Split(' ').Distinct());
				}
			}
		}

		public string Column3AdValoremRateFormula
		{
			get
			{
				return GetDTARateFormula(Schema.Column3AdValoremRate.Name, Column3AdValoremRate);
			}
		}

		public string Column3SpecificRateFormula
		{
			get
			{
				return GetDTSRateFormula(Schema.Column3SpecificRate.Name, Column3SpecificRate, SpecificRateUnit);
			}
		}

		public string Column1AdValoremRateFormula
		{
			get
			{
				return GetDTARateFormula(Schema.Column1AdValoremRate.Name, Column1AdValoremRate);
			}
		}

		public string Column1SpecificRateFormula
		{
			get
			{
				return GetDTSRateFormula(Schema.Column1SpecificRate.Name, Column1SpecificRate, SpecificRateUnit);
			}
		}

		public string Column3ProvisionalAdValoremRateFormula
		{
			get
			{
				return GetDTARateFormula(Schema.Column3ProvisionalAdValoremRate.Name, Column3ProvisionalAdValoremRate);
			}
		}

		public string Column3ProvisionalSpecificRateFormula
		{
			get
			{
				return GetDTSRateFormula(Schema.Column3ProvisionalSpecificRate.Name, Column3ProvisionalSpecificRate, SpecificRateUnit);
			}
		}

		public string Column1ProvisionalAdValoremRateFormula
		{
			get
			{
				return GetDTARateFormula(Schema.Column1ProvisionalAdValoremRate.Name, Column1ProvisionalAdValoremRate);
			}
		}

		public string Column1ProvisionalSpecificRateFormula
		{
			get
			{
				return GetDTSRateFormula(Schema.Column1ProvisionalSpecificRate.Name, Column1ProvisionalSpecificRate, SpecificRateUnit);
			}
		}

		#region RateFormulaDerivedFrom
		public string Column3AdValoremRateFormulaDerivedFrom
		{
			get
			{
				return GetDTARateFormulaDerivedFrom(Schema.Column3AdValoremRate.Name, Column3AdValoremRate);
			}
		}

		public string Column3SpecificRateFormulaDerivedFrom
		{
			get
			{
				return GetDTSRateFormulaDerivedFrom(Schema.Column3SpecificRate.Name, Column3SpecificRate, SpecificRateUnit);
			}
		}

		public string Column1AdValoremRateFormulaDerivedFrom
		{
			get
			{
				return GetDTARateFormulaDerivedFrom(Schema.Column1AdValoremRate.Name, Column1AdValoremRate);
			}
		}

		public string Column1SpecificRateFormulaDerivedFrom
		{
			get
			{
				return GetDTSRateFormulaDerivedFrom(Schema.Column1SpecificRate.Name, Column1SpecificRate, SpecificRateUnit);
			}
		}

		public string Column3ProvisionalAdValoremRateFormulaDerivedFrom
		{
			get
			{
				return GetDTARateFormulaDerivedFrom(Schema.Column3ProvisionalAdValoremRate.Name, Column3ProvisionalAdValoremRate);
			}
		}

		public string Column3ProvisionalSpecificRateFormulaDerivedFrom
		{
			get
			{
				return GetDTSRateFormulaDerivedFrom(Schema.Column3ProvisionalSpecificRate.Name, Column3ProvisionalSpecificRate, SpecificRateUnit);
			}
		}

		public string Column1ProvisionalAdValoremRateFormulaDerivedFrom
		{
			get
			{
				return GetDTARateFormulaDerivedFrom(Schema.Column1ProvisionalAdValoremRate.Name, Column1ProvisionalAdValoremRate);
			}
		}

		public string Column1ProvisionalSpecificRateFormulaDerivedFrom
		{
			get
			{
				return GetDTSRateFormulaDerivedFrom(Schema.Column1ProvisionalSpecificRate.Name, Column1ProvisionalSpecificRate, SpecificRateUnit);
			}
		}
		#endregion

		public IEnumerable<RefCusTariffUOM> GetTariffUOM()
		{
			if (!string.IsNullOrEmpty(WeightUnit))
			{
				yield return new RefCusTariffUOM() { ZZ8_Type = "CU1", ZZ8_UOM = WeightUnit };
			}

			if (!string.IsNullOrEmpty(QuantityUnit))
			{
				yield return new RefCusTariffUOM() { ZZ8_Type = "CU2", ZZ8_UOM = QuantityUnit };
			}
		}

		public IEnumerable<RefCusTariffAttribute> GetRefCusTariffAttribute()
		{
			for (int i = 0; i < CustomsRequirementList.Count; i++)
			{
				yield return new RefCusTariffAttribute() { ZZ3_Name = "CustomsRequirements", ZZ3_Value = CustomsRequirementList[i] };
			}

			var importRegulationCodeList = new List<string>(new string[] { ImportRegulationCode1, ImportRegulationCode2, ImportRegulationCode3, ImportRegulationCode4, ImportRegulationCode5, ImportRegulationCode6, ImportRegulationCode7 });
			foreach (var importRegulationCode in importRegulationCodeList)
			{
				if (!string.IsNullOrWhiteSpace(importRegulationCode))
				{
					yield return new RefCusTariffAttribute() { ZZ3_Name = "ImportRegulations", ZZ3_Value = importRegulationCode };
				}
			}

			var exportRegulationCodeList = new List<string>(new string[] { ExportRegulationCode1, ExportRegulationCode2, ExportRegulationCode3, ExportRegulationCode4, ExportRegulationCode5, ExportRegulationCode6, ExportRegulationCode7 });
			foreach (var exportRegulationCode in exportRegulationCodeList)
			{
				if (!string.IsNullOrWhiteSpace(exportRegulationCode))
				{
					yield return new RefCusTariffAttribute() { ZZ3_Name = "ExportRegulations", ZZ3_Value = exportRegulationCode };
				}
			}
		}

		#endregion
	}
}
