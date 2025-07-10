using System.Xml.Linq;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders
{
	public class GeographicalAreaLoader : XmlElementReader<GeographicalArea>, IProcessorLoader
	{
		protected override string ParentElement => "findGeographicalAreaByDatesResponse";

		protected override string ElementName => "GeographicalArea";

		protected override GeographicalArea CreateMinimumModelFromXElement(XElement element)
		{
			return new GeographicalArea
			{
				GeographicalAreaId = element.Element("geographicalAreaId")?.Value
			};
		}
	}
}
