using System.Xml.Linq;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders
{
	public class MeasureTypeLoader : LoaderBase<MeasureType>
	{
		protected override string ParentElement => "findMeasureTypeByDatesResponse";

		protected override string ElementName => "MeasureType";

		protected override MeasureType CreateMinimumModelFromXElement(XElement element)
		{
			return new MeasureType
			{
				Id = element.Element("measureTypeId")?.Value,
				Description = element.Element("measureTypeDescription")?.Element("description")?.Value ?? string.Empty,
			};
		}
	}
}
