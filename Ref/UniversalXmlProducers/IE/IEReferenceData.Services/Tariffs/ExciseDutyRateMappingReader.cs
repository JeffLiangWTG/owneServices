using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using CsvHelper;

namespace CargoWise.RefDbRepo.IEReferenceData.Services
{
	public static class ExciseDutyRateMappingReader
	{
		public static IReadOnlyCollection<ExciseDutyRateMapping> ReadAll(IApplicationConfig config) => GetMappingRaw(config).Select(Create).ToArray();

		static ExciseDutyRateMapping Create(string[] rawRow) => ExciseDutyRateMapping.Create(
			mappingType: Enum.TryParse(rawRow[0].Trim(), out ExciseDutyRateMappingType mappingType) ? mappingType : ExciseDutyRateMappingType.normal,
			taxType: rawRow[1],
			taxCode: rawRow[2],
			rawRow
		);

		static string[][] GetMappingRaw(IApplicationConfig config)
		{
			var result = new List<string[]>();

			var assembly = Assembly.GetExecutingAssembly();

			var manifestResourceNames = assembly.GetManifestResourceNames();
			var mappingsManifestResource = manifestResourceNames.FirstOrDefault(name => name.EndsWith(config.ExciseDuty_Rate_Mapping, StringComparison.InvariantCultureIgnoreCase));

			if (mappingsManifestResource != null)
			{
				using (var stream = assembly.GetManifestResourceStream(mappingsManifestResource))
				{
					if (stream != null)
					{
						using (var csvReader = new CsvReader(new StreamReader(stream)))
						{
							while (csvReader.Read())
							{
								result.Add(csvReader.Context.Record.Select(cell => cell.Trim()).ToArray());
							}
						}
					}
				}
			}

			return result.ToArray();
		}
	}

	public enum ExciseDutyRateMappingType
	{
		normal,
		constant,
		ignore
	}

	public class ExciseDutyRateMapping
	{
		public static ExciseDutyRateMapping Create(ExciseDutyRateMappingType mappingType, string taxType, string taxCode, string[] row) => new ExciseDutyRateMapping(mappingType, taxType, taxCode, row);

		ExciseDutyRateMapping(ExciseDutyRateMappingType mappingType, string taxType, string taxCode, string[] row)
		{
			MappingType = mappingType;
			TaxType = taxType;
			TaxCode = taxCode;
			MappingRow = row;
			MappingKey = SpecialSearchWordRules.First(rule => rule.Critical(MappingRow)).GetSearchWords(MappingRow).GetSearchKey();
		}

		public ExciseDutyRateMappingType MappingType { get; }
		public string TaxType { get; }
		public string TaxCode { get; }
		public string MappingKey { get; }
		public string SupplementaryMappingKey => MappingRow[3].GetSearchKey();
		public string[] MappingRow { get; }

		[ThreadStatic]
		static ImmutableArray<(Func<string[], bool>, Func<string[], string[]>)> specialSearchWordRules;
		static ImmutableArray<(Func<string[], bool> Critical, Func<string[], string[]> GetSearchWords)> SpecialSearchWordRules
		{
			get
			{
				if (specialSearchWordRules == null)
				{
					var funcs = new (Func<string[], bool>, Func<string[], string[]>)[] {
						(row => row[3].EndsWith(" - Carbon", StringComparison.InvariantCultureIgnoreCase), row => new[] { row[4], row[5], "Carbon" }),
						(row => true, row => new[] { row[4], row[5]})
					};
					specialSearchWordRules = ImmutableArray.Create(funcs);
				}
				return specialSearchWordRules;
			}
		}
	}
}
