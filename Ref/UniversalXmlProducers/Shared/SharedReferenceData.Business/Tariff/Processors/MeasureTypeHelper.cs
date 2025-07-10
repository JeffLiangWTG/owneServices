using System.Collections.Generic;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors
{
	public class MeasureTypeMapping
	{
		public string ConditionClass { get; set; }
		public string RateType { get; set; }
		public string RateCode { get; set; }
		public bool Skip { get; set; }
		public bool SupplementaryUnit { get; set; }
		public IEnumerable<string> Preferences { get; set; }
		public IEnumerable<string> AuthorisedUsePreferences { get; set; }
	}

	public static class RateTypes
	{
		public const string Duty = "DTY";
		public const string AntiDumping = "ADD";
		public const string Countervailing = "CVD";
		public const string Security = "SEC";
		public const string Excises = "EXC";
		public const string Levies = "LEV";
		public const string Export = "EXP";
		public const string Retribution = "MSC";
		public const string Interest = "INT";
	}

	public class MeasureTypeHelper
	{
		public MeasureTypeHelper(IMeasureMappingProvider measureMappingProvider)
		{
			this.measureMappingProvider = measureMappingProvider;
		}
		readonly IMeasureMappingProvider measureMappingProvider;

		Dictionary<string, MeasureTypeMapping> Mappings => mappings ?? (mappings = measureMappingProvider.GetMeasureTypeMappings());
		Dictionary<string, MeasureTypeMapping> mappings;

		MeasureTypeMapping GetMapping(string measureType)
		{
			var key = measureType ?? string.Empty;

			return Mappings.ContainsKey(key) ? Mappings[key] : null;
		}

		public string GetConditionClass(string measureType)
		{
			return GetMapping(measureType)?.ConditionClass ?? string.Empty;
		}

		public string GetRateType(string measureType)
		{
			return GetMapping(measureType)?.RateType ?? string.Empty;
		}

		public string GetRateCode(string measureType)
		{
			return GetMapping(measureType)?.RateCode ?? string.Empty;
		}

		public bool ShouldProcessMeasureType(string measureType, bool processConfiguredMeasureTypesOnly)
		{
			return !(GetMapping(measureType)?.Skip ?? processConfiguredMeasureTypesOnly);
		}

		public bool IsSupplementaryUnit(string measureType)
		{
			return GetMapping(measureType)?.SupplementaryUnit ?? false;
		}

		public IEnumerable<string> GetPreferences(string measureType)
		{
			return GetMapping(measureType)?.Preferences ?? new List<string> { null };
		}

		public IEnumerable<string> GetAuthorisedUsePreferences(string measureType)
		{
			return GetMapping(measureType)?.AuthorisedUsePreferences ?? new List<string>();
		}
	}
}
