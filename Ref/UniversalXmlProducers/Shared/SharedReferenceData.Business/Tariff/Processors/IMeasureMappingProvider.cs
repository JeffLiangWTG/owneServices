using System.Collections.Generic;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors
{
	public interface IMeasureMappingProvider
	{
		Dictionary<string, MeasureTypeMapping> GetMeasureTypeMappings();
		string GetVatCode(Measure measure);
		string ConvertRateCode(string defaultRateCode, Measure measure);
		IEnumerable<string> ConvertPreferences(IEnumerable<string> defaultPreferences, string geographicalArea, IEnumerable<string> footnotes);
	}
}
