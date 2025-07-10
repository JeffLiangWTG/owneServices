using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.OfficeCodes.Business
{
	public static class OfficeCodeHelper
	{
		public static XElement GetCustomsOfficesRootElement(this XDocument xmlDoc) => xmlDoc?.Descendants(Constants.OfficeCodes.RootElementName).FirstOrDefault(x => x.Attribute("name")?.Value == Constants.OfficeCodes.RootAttributeValue);

		public static string GetDateValueFromCustomsOfficeElement(this XElement xmlRootElement, XName dateElementName) => xmlRootElement?.Descendants(dateElementName).FirstOrDefault()?.Value;

		public static string GetValueFromDefaultElementWithAttribute(this XElement xmlRootElement, string nameAttributeValue) => xmlRootElement?.Elements(Constants.OfficeCodes.DefaultElementName).FirstOrDefault(x => x.Attribute("name")?.Value == nameAttributeValue)?.Value;
	}
}
