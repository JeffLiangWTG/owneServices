using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.ExportMeasures
{
	public interface IExportMeasureMapper
	{
		RefCusTariff GetMapping(TariffInput input);
	}

	public record TariffInput(string TariffCode, AdditionalCodeInput[] AdditionalCodes);

	public record AdditionalCodeInput(string Code, string Description, ApplicabilityInput[] Applicabilities);

	public record ApplicabilityInput(DateTime StartDate, string TradeGroup);
}
