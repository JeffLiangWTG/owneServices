using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public static class XElementExtensions
	{
		public static XElement Element(this XElement element, string elementName)
		{
			Argument.NotNull(element, nameof(element));
			Argument.NotNullOrEmpty(elementName, nameof(elementName));
			return element.Element(XName.Get(elementName));
		}

		public static IEnumerable<XElement> Elements(this XElement element, string elementName)
		{
			Argument.NotNull(element, nameof(element));
			Argument.NotNullOrEmpty(elementName, nameof(elementName));
			return element.Elements(XName.Get(elementName));
		}

		public static XAttribute Attribute(this XElement element, string attributeName)
		{
			Argument.NotNull(element, nameof(element));
			Argument.NotNullOrEmpty(attributeName, nameof(attributeName));
			return element.Attribute(XName.Get(attributeName));
		}
	}
}
