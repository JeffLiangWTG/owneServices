using System;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.NZExchangeRateParser
{
	public static class XmlExtension
	{
		public static XElement GetNode(this XElement element, string name)
		{
			Argument.NotNull(element, nameof(element));
			Argument.NotNullOrEmpty(name, nameof(name));

			var node = element.Element(name);

			if (node == null)
			{
				Console.Error.WriteLine($"{name} Node Not Found");
			}

			return node;
		}
	}
}
