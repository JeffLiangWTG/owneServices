using System.Xml.Linq;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders
{
	public class AdditionalCodeLoader : XmlElementReader<AdditionalCode>, IProcessorLoader
	{
		protected override string ParentElement => "findAdditionalCodeByDatesResponse";
		protected override string ElementName => "AdditionalCode";

		protected override bool IsValidElement(XElement element)
		{
			return element.Element("additionalCodeCode") != null;
		}

		protected override AdditionalCode CreateMinimumModelFromXElement(XElement element)
		{
			return new AdditionalCode
			{
				Code = element.Element("additionalCodeCode")?.Value,
			};
		}
	}
}
