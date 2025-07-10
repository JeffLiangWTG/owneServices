using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Ncts
{
	public class NctsTransportDocumentsTD44NTransformer : ManyToOneCodeListsWithAttributesParserXML
	{
		public NctsTransportDocumentsTD44NTransformer(string[] downLoadLinks)
			: base(downLoadLinks)
		{
		}

		protected override string[] CustomsCodeListIdentifiers => new[] { CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0943_TRANSPORT_DOCUMENTS, CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0945_TRANSPORT_DOCUMENTS };

		protected override string CodeType => CodeListsConstants.Ncts.CodeTypes.NCTS_TD44N_TRANSPORT_DOCUMENTS;

		protected override string OutputFileNameSuffix => nameof(CodeListsConstants.Ncts.CodeTypes.NCTS_TD44N_TRANSPORT_DOCUMENTS);

		protected override XmlWriterConfiguration XmlWriterConfiguration => CodeListsHelper.GetRefCusCodeListWriterConfigurationWithAttributes(true, CodeType);

		protected override IEnumerable<string> InputAttributes => new[]
		{
			CodeListsConstants.XMLEntryElementNames.REFERENCE,
			CodeListsConstants.XMLEntryElementNames.ITEM_NUMBER,
			CodeListsConstants.XMLEntryElementNames.COMPLEMENT,
			CodeListsConstants.XMLEntryElementNames.DETAIL,
			CodeListsConstants.XMLEntryElementNames.AUTHORITY,
			CodeListsConstants.XMLEntryElementNames.ISSUING_DATE,
			CodeListsConstants.XMLEntryElementNames.VALIDITY_DATE,
			CodeListsConstants.XMLEntryElementNames.MEASUREMENT_UNIT,
			CodeListsConstants.XMLEntryElementNames.COMPLEMENTARY_UNIT,
			CodeListsConstants.XMLEntryElementNames.VALUE
		};

		protected override List<RefCusCodeList> ConvertAndCombineCodeLists(Dictionary<string, ParseResult<IKeyValuesWithAttributes>> parseResults)
			=> CombineListsWithLevelAttributes(parseResults, CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0945_TRANSPORT_DOCUMENTS, new (string, string)[] { (CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0943_TRANSPORT_DOCUMENTS, CodeListsConstants.AttributeValues.Header) });

		protected override IKeyValuesWithAttributes GetKeyValues(XElement entry)
		{
			var attributes = new List<KeyValueAttribute>();
			CodeListsHelper.AddToAttributes(attributes, CodeListsConstants.XMLEntryElementNames.LEVEL_HOUSE, CodeListsConstants.AttributeValues.House);
			foreach (var allowedAttribute in InputAttributes)
			{
				CodeListsHelper.AddAllowedToAttributesAsBool(attributes, allowedAttribute, entry.Element(allowedAttribute)?.Value);
			}

			return new BaseKeyValuesWithAttributesXML(entry, CodeListsConstants.XMLEntryElementNames.CODE, attributes);
		}
	}
}
