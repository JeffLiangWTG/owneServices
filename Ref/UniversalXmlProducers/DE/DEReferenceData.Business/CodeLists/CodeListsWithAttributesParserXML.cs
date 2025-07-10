using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists
{
	public abstract class CodeListsWithAttributesParserXML : CodeListsParserXML<RefCusCodeList, IKeyValuesWithAttributes>
	{
		protected CodeListsWithAttributesParserXML(string[] downloadLinks) : base(downloadLinks)
		{
		}

		protected abstract IEnumerable<string> InputAttributes { get; }

		protected override void AppendInvalidDataErrorDetails(IKeyValuesWithAttributes keyValues, XElement entry)
		{
			base.AppendInvalidDataErrorDetails(keyValues, entry);
			foreach (var attribute in InputAttributes)
			{
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"{attribute}: {entry.Element(attribute)?.Value}");
			}
		}
	}
}
