using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Ncts
{
	public class NctsAdditionalReferencesAR44NTransformer : ManyToOneCodeListsWithAttributesParserXML
	{
		public NctsAdditionalReferencesAR44NTransformer(string[] downloadLinks)
			: base(downloadLinks)
		{
		}

		protected override string[] CustomsCodeListIdentifiers => new[] { CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0913_ADDITIONAL_REFERENCES, CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0915_ADDITIONAL_REFERENCES, CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0916_ADDITIONAL_REFERENCES };

		protected override string CodeType => CodeListsConstants.Ncts.CodeTypes.NCTS_AR44N_ADDITIONAL_REFERENCES;

		protected override string OutputFileNameSuffix => nameof(CodeListsConstants.Ncts.CodeTypes.NCTS_AR44N_ADDITIONAL_REFERENCES);

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
				(CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0915_ADDITIONAL_REFERENCES, CodeListsConstants.AttributeValues.House),
				(CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0913_ADDITIONAL_REFERENCES, CodeListsConstants.AttributeValues.Header)
			};

			return CombineListsWithLevelAttributes(parseResults, CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0916_ADDITIONAL_REFERENCES, orderedCodeTypeWithAttributeToUpdate);
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
