using System;
using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Import
{
	public class ImportDepartureAirportsEUIATTransformer : CodeListsWithAttributesParserXML
	{
		public ImportDepartureAirportsEUIATTransformer(string[] downloadLinks)
			: base(downloadLinks)
		{
		}

		protected override string CustomsCodeListIdentifier => CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_I0600_DEPARTURE_AIRPORTS;

		protected override string CodeType => CodeListsConstants.Import.CodeTypes.IMPORT_EUIAT_DEPARTURE_AIRPORTS;

		protected override string OutputFileNameSuffix => nameof(CodeListsConstants.Import.CodeTypes.IMPORT_EUIAT_DEPARTURE_AIRPORTS);

		protected override XmlWriterConfiguration XmlWriterConfiguration => CodeListsHelper.GetRefCusCodeListWriterConfigurationWithAttributes(false, CodeType, "EUN");

		protected override IEnumerable<string> InputAttributes => new[]
		{
			CodeListsConstants.XMLEntryElementNames.PERCENTAGE,
			CodeListsConstants.XMLEntryElementNames.ZONE
		};

		protected override string CodeAttributeName => CodeListsConstants.XMLEntryElementNames.AIRPORT;
		
		protected override IKeyValuesWithAttributes GetKeyValues(XElement entry)
		{
			var attributes = new List<KeyValueAttribute>();
			CodeListsHelper.AddToAttributes(attributes, CodeListsConstants.XMLEntryElementNames.PERCENTAGE, entry.Element(CodeListsConstants.XMLEntryElementNames.PERCENTAGE)?.Value);
			CodeListsHelper.AddToAttributes(attributes, CodeListsConstants.XMLEntryElementNames.ZONE, entry.Element(CodeListsConstants.XMLEntryElementNames.ZONE)?.Value);

			return new BaseKeyValuesWithAttributesXML(entry, CodeListsConstants.XMLEntryElementNames.AIRPORT, attributes);
		}

		protected override RefCusCodeList CreateRefList(IKeyValuesWithAttributes keyValues)
		{
			var result = CodeListsHelper.CreateRefListWithAttributes(keyValues);
			var oldDescription = result.ZZD_Description;
			if (oldDescription.Contains(","))
			{
				result.ZZD_Description = oldDescription.Substring(0, oldDescription.IndexOf(",", StringComparison.InvariantCulture));
			}
			return result;
		}
	}
}
