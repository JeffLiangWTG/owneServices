using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Ncts
{
	public class NctsAdditionalInformationAI44NTransformer : ManyToOneCodeListsWithAttributesParserXML
	{
		public NctsAdditionalInformationAI44NTransformer(string[] downloadLinks)
			: base(downloadLinks)
		{
		}

		protected override string[] CustomsCodeListIdentifiers => new[] { CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0903_ADDITIONAL_INFORMATION, CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0905_ADDITIONAL_INFORMATION, CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0906_ADDITIONAL_INFORMATION };

		protected override string CodeType => CodeListsConstants.Ncts.CodeTypes.NCTS_AI44N_ADDITIONAL_INFORMATION;

		protected override string OutputFileNameSuffix => nameof(CodeListsConstants.Ncts.CodeTypes.NCTS_AI44N_ADDITIONAL_INFORMATION);

		protected override XmlWriterConfiguration XmlWriterConfiguration => CodeListsHelper.GetRefCusCodeListWriterConfigurationWithAttributes(true, CodeType);

		protected override List<RefCusCodeList> ConvertAndCombineCodeLists(Dictionary<string, ParseResult<IKeyValuesWithAttributes>> parseResults)
		{
			(string codeType, string attributeToUpdate)[] orderedCodeTypeWithAttributeToUpdate =
{
				(CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0905_ADDITIONAL_INFORMATION, CodeListsConstants.AttributeValues.House),
				(CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0903_ADDITIONAL_INFORMATION, CodeListsConstants.AttributeValues.Header)
			};

			return CombineListsWithLevelAttributes(parseResults, CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_I0906_ADDITIONAL_INFORMATION, orderedCodeTypeWithAttributeToUpdate);
		}

		protected override IEnumerable<string> InputAttributes => new List<string>();

		protected override IKeyValuesWithAttributes GetKeyValues(XElement entry)
		{
			var attributes = new List<KeyValueAttribute>();
			CodeListsHelper.AddToAttributes(attributes, CodeListsConstants.XMLEntryElementNames.LEVEL_ITEM, CodeListsConstants.AttributeValues.Item);
			return new BaseKeyValuesWithAttributesXML(entry, CodeListsConstants.XMLEntryElementNames.CODE, attributes);
		}
	}
}
