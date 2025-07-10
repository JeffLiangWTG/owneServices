using System.Xml.Linq;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders
{
	public class ModificationRegulationLoader : LoaderBase<ModificationRegulation>
	{
		protected override string ParentElement => "findModificationRegulationByDatesResponseHistory";

		protected override string ElementName => "ModificationRegulation";

		protected override ModificationRegulation CreateMinimumModelFromXElement(XElement element)
		{
			return new ModificationRegulation
			{
				RegulationId = element.Element("modificationRegulationId")?.Value,
				StartDate = GetDateTimeFromElement(element, "validityStartDate"),
				BaseRegulationId = element.Element("baseRegulation")?.Element("baseRegulationId")?.Value ?? string.Empty,
			};
		}
	}
}
