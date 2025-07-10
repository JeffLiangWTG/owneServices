using System.Xml.Linq;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders
{
	public class MeasureLoader : XmlElementReader<Measure>, IProcessorLoader
	{
		protected override string ParentElement => "findMeasureByDatesResponseHistory";
		protected override string ElementName => "Measure";

		protected override bool IsValidElement(XElement element)
		{
			return element.Element("goodsNomenclature") != null;
		}

		protected override Measure CreateMinimumModelFromXElement(XElement element)
		{
			return new Measure
			{
				RegulationId = element.Element("measureGeneratingRegulationId")?.Value ?? string.Empty,
				ItemId = element.Element("goodsNomenclature")?.Element("goodsNomenclatureItemId")?.Value ?? string.Empty,
				MeasureType = element.Element("measureType")?.Element("measureTypeId")?.Value ?? string.Empty,
				GeographicalArea = element.Element("geographicalArea")?.Element("geographicalAreaId")?.Value ?? string.Empty,
			};
		}
	}
}
