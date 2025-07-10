using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Ncts
{
	public class NctsDocumentsDC44NTransformer : ManyToOneCodeListsWithAttributesParserXML
	{
		public NctsDocumentsDC44NTransformer(string[] downLoadLinks)
			: base(downLoadLinks)
		{
		}

		protected override string[] CustomsCodeListIdentifiers => new[] { CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0923_SUPPORTING_DOCUMENTS, CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0925_SUPPORTING_DOCUMENTS, CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0926_SUPPORTING_DOCUMENTS };

		protected override string CodeType => CodeListsConstants.Ncts.CodeTypes.NCTS_DC44N_SUPPORTING_DOCUMENTS;

		protected override string OutputFileNameSuffix => nameof(CodeListsConstants.Ncts.CodeTypes.NCTS_DC44N_SUPPORTING_DOCUMENTS);

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
		{
			(string codeType, string attributeToUpdate)[] orderedCodeTypeWithAttributeToUpdate =
			{
				(CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0925_SUPPORTING_DOCUMENTS, CodeListsConstants.AttributeValues.House),
				(CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0923_SUPPORTING_DOCUMENTS, CodeListsConstants.AttributeValues.Header)
			};

			return CombineListsWithLevelAttributes(parseResults, CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0926_SUPPORTING_DOCUMENTS, orderedCodeTypeWithAttributeToUpdate);
		}

		protected override IKeyValuesWithAttributes GetKeyValues(XElement entry)
		{
			var attributes = new List<KeyValueAttribute>();
			CodeListsHelper.AddToAttributes(attributes, CodeListsConstants.XMLEntryElementNames.LEVEL_ITEM, CodeListsConstants.AttributeValues.Item);
			foreach (var allowedAttribute in InputAttributes)
			{
				CodeListsHelper.AddAllowedToAttributesAsBool(attributes, allowedAttribute, entry.Element(allowedAttribute)?.Value);
			}

			return new BaseKeyValuesWithAttributesXML(entry, CodeListsConstants.XMLEntryElementNames.CODE, attributes);
		}
	}
}
