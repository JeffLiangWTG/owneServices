using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.CNReferenceData.Business
{
	public class ExcelReadConfiguration
	{
		public static class ColumnConstants
		{
			public static List<(int, string, Func<string, string>)> GetColumnsConfig(Type constantType)
			{
				var result = new List<(int, string, Func<string, string>)>();

				foreach (var field in constantType.GetProperties())
				{
					var value = field.GetValue(null);
					if (value is ValueTuple<int, string> tuple)
					{
						result.Add((tuple.Item1, tuple.Item2, null));
					}
					else if (value is ValueTuple<int, string, Func<string, string>> tupleWithFunc)
					{
						result.Add((tupleWithFunc.Item1, tupleWithFunc.Item2, tupleWithFunc.Item3));
					}
				}

				return result;
			}

			public static class Tariff
			{
				public static (int Index, string Name) TariffCode { get; set; } = (1, "TariffCode");
				public static (int Index, string Name) Description { get; set; } = (2, "Description");
				public static (int Index, string Name) EnglishDescription { get; set; } = (3, "EnglishDescription");
				public static (int Index, string Name) ProvRate { get; set; } = (4, "ProvRate");
				public static (int Index, string Name) MFNRate { get; set; } = (6, "MFNRate");
				public static (int Index, string Name) NormalRate { get; set; } = (8, "NormalRate");
				public static (int Index, string Name) VATRate { get; set; } = (10, "VATRate");
				public static (int Index, string Name) DrawbackRate { get; set; } = (11, "DrawbackRate");
				public static (int Index, string Name) Unit1 { get; set; } = (12, "Unit1");
				public static (int Index, string Name) Unit2 { get; set; } = (14, "Unit2");
				public static (int Index, string Name, Func<string, string> PreOperation) CUSRequirements { get; set; } = (16, "CUSRequirements", s => s.PreOperateCondition());
				public static (int Index, string Name, Func<string, string> PreOperation) CIQRequirements { get; set; } = (17, "CIQRequirements", s => s.PreOperateCondition());
				public static (int Index, string Name) AdditionalInfo { get; set; } = (18, "AdditionalInfo");
			}

			public static class CIQ
			{
				public static (int Index, string Name) TariffCode { get; set; } = (1, "TariffCode");
				public static (int Index, string Name) CIQCode { get; set; } = (2, "CIQCode");
				public static (int Index, string Name) Description { get; set; } = (3, "Description");
				public static (int Index, string Name) EnglishDescription { get; set; } = (4, "EnglishDescription");
			}

			public static class DutyRate
			{
				public static (int Index, string Name) Tariff { get; set; } = (2, "Tariff");
				public static (int Index, string Name) APEC { get; set; } = (5, "APEC");
				public static (int Index, string Name) CL { get; set; } = (6, "CL");
				public static (int Index, string Name) PK { get; set; } = (7, "PK");
				public static (int Index, string Name) NZ { get; set; } = (8, "NZ");
				public static (int Index, string Name) SG { get; set; } = (9, "SG");
				public static (int Index, string Name) PE { get; set; } = (10, "PE");
				public static (int Index, string Name) CR { get; set; } = (11, "CR");
				public static (int Index, string Name) CH { get; set; } = (12, "CH");
				public static (int Index, string Name) IS { get; set; } = (13, "IS");
				public static (int Index, string Name) AU { get; set; } = (14, "AU");
				public static (int Index, string Name) KR { get; set; } = (15, "KR");
				public static (int Index, string Name) GE { get; set; } = (16, "GE");
				public static (int Index, string Name) MU { get; set; } = (17, "MU");
				public static (int Index, string Name) ASEAN { get; set; } = (18, "ASEAN");
				public static (int Index, string Name) KH { get; set; } = (19, "KH");
				public static (int Index, string Name) HK { get; set; } = (20, "HK");
				public static (int Index, string Name) MO { get; set; } = (21, "MO");
				public static (int Index, string Name) TW { get; set; } = (22, "TW");
				public static (int Index, string Name) LDCAPEC { get; set; } = (23, "LDCAPEC");
				public static (int Index, string Name) LDCLA { get; set; } = (24, "LDCLA");
				public static (int Index, string Name) LDCKH { get; set; } = (25, "LDCKH");
				public static (int Index, string Name) LDCMM { get; set; } = (26, "LDCMM");
				public static (int Index, string Name) LDC1 { get; set; } = (27, "LDC1");
				public static (int Index, string Name) LDC2 { get; set; } = (28, "LDC2");
				public static (int Index, string Name) RCEPASEAN { get; set; } = (0, "RCEPASEAN");
				public static (int Index, string Name) RCEPAU { get; set; } = (0, "RCEPAU");
				public static (int Index, string Name) RCEPJP { get; set; } = (0, "RCEPJP");
				public static (int Index, string Name) RCEPNZ { get; set; } = (0, "RCEPNZ");
			}

			public static class ExciseRate
			{
				public static (int Index, string Name) Tariff { get; set; } = (1, "Tariff");
				public static (int Index, string Name) Rate { get; set; } = (2, "Rate");
				public static (int Index, string Name) SpecialRate { get; set; } = (3, "SpecialRate");
			}

			public static class ExportDutyRate
			{
				public static (int Index, string Name) Tariff { get; set; } = (2, "Tariff");
				public static (int Index, string Name) Rate { get; set; } = (5, "Rate");
				public static (int Index, string Name) SpecialRate { get; set; } = (6, "SpecialRate");
			}

			public static class UsaAddRate
			{
				public static (int Index, string Name) Tariff { get; set; } = (1, "Tariff");
				public static (int Index, string Name) Rate { get; set; } = (4, "Rate");
				public static (int Index, string Name) EffectiveDate { get; set; } = (5, "EffectiveDate");
				public static (int Index, string Name) ExpiredDate { get; set; } = (6, "ExpiredDate");
			}
		}

		public string Description { get; set; }

		public string SourceFile { get; set; }

		public int FileIndex { get; set; }

		public int StartingRow { get; set; } = 1;

		public int LastRow { get; set; } = 20000;

		public int SheetIndex { get; set; } = 1;

		public int KeyColumnIndex { get; set; } = 1;

		public string KeyRegex { get; set; }

		public List<(int ColIndex, string Name, Func<string, string> PreOperation)> Columns { get; set; }

		public static ExcelReadConfiguration Tariff { get; set; } = new ExcelReadConfiguration()
		{
			Description = "Tariffs",
			FileIndex = 1,
			StartingRow = 3,
			KeyColumnIndex = 1,
			KeyRegex = @"^\d{8,10}$",
			Columns = ColumnConstants.GetColumnsConfig(typeof(ColumnConstants.Tariff))
		};

		public static ExcelReadConfiguration CIQ { get; set; } = new ExcelReadConfiguration()
		{
			Description = "CIQ Tariffs",
			FileIndex = 3,
			StartingRow = 2,
			LastRow = 50000,
			KeyColumnIndex = 1,
			KeyRegex = @"^\d{10}$",
			Columns = ColumnConstants.GetColumnsConfig(typeof(ColumnConstants.CIQ))
		};

		public static ExcelReadConfiguration DutyRate { get; set; } = new ExcelReadConfiguration()
		{
			Description = "Duty Rates",
			FileIndex = 7,
			StartingRow = 4,
			KeyColumnIndex = 2,
			KeyRegex = @"^\d{10}$",
			Columns = ColumnConstants.GetColumnsConfig(typeof(ColumnConstants.DutyRate))
		};

		public static ExcelReadConfiguration ExciseRate { get; set; } = new ExcelReadConfiguration()
		{
			Description = "Excise Rates",
			FileIndex = 4,
			StartingRow = 2,
			KeyColumnIndex = 1,
			KeyRegex = @"^\d{10}$",
			Columns = ColumnConstants.GetColumnsConfig(typeof(ColumnConstants.ExciseRate))
		};

		public static ExcelReadConfiguration ExportDutyRate { get; set; } = new ExcelReadConfiguration()
		{
			Description = "Export Duty Rates",
			FileIndex = 8,
			StartingRow = 2,
			KeyColumnIndex = 2,
			KeyRegex = @"^\d{10}$",
			Columns = ColumnConstants.GetColumnsConfig(typeof(ColumnConstants.ExportDutyRate))
		};

		public static ExcelReadConfiguration UsaAddRate { get; set; } = new ExcelReadConfiguration()
		{
			Description = "USA Addition Duty Rates",
			FileIndex = 6,
			StartingRow = 2,
			KeyColumnIndex = 1,
			KeyRegex = @"^\d{10}$",
			Columns = ColumnConstants.GetColumnsConfig(typeof(ColumnConstants.UsaAddRate))
		};
	}

	public class RowData
	{
		public int SourceRowIndex { get; set; }

		public string Key { get; set; }

		public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

		public bool Exported { get; set; }
	}
}
