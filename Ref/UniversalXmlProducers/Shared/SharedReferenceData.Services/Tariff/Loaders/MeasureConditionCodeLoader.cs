using System.Xml.Linq;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders
{
	public class MeasureConditionCodeLoader : LoaderBase<MeasureConditionCode>
	{
		protected override string ParentElement => "findMeasureConditionCodeByDatesResponse";

		protected override string ElementName => "MeasureConditionCode";

		protected override MeasureConditionCode CreateMinimumModelFromXElement(XElement element)
		{
			return new MeasureConditionCode
			{
				Id = element.Element("conditionCode")?.Value,
				Description = element.Element("measureConditionCodeDescription")?.Element("description")?.Value ?? string.Empty,
			};
		}
	}
}
