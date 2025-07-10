using System.Xml.Linq;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders
{
	public class BaseRegulationLoader : LoaderBase<BaseRegulation>
	{
		protected override string ParentElement => "findBaseRegulationByDatesResponseHistory";

		protected override string ElementName => "BaseRegulation";

		protected override BaseRegulation CreateMinimumModelFromXElement(XElement element)
		{
			return new BaseRegulation
			{
				RegulationId = element.Element("baseRegulationId")?.Value,
				StartDate = GetDateTimeFromElement(element, "validityStartDate"),
			};
		}
	}
}
