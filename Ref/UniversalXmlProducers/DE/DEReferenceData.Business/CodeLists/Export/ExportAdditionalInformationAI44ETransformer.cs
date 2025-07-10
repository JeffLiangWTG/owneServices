using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Export
{
	public class ExportAdditionalInformationAI44ETransformer : ManyToOneCodeListsWithAttributesParserXML
	{
		public ExportAdditionalInformationAI44ETransformer(string[] downloadLinks) : base(downloadLinks)
		{
		}

		protected override List<RefCusCodeList> ConvertAndCombineCodeLists(Dictionary<string, ParseResult<IKeyValuesWithAttributes>> parseResults)
			=> CombineListsWithLevelAttributes(parseResults, CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0902_ADDITIONAL_INFORMATION, new (string, string)[] { (CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0901_ADDITIONAL_INFORMATION, CodeListsConstants.AttributeValues.Header) });

		protected override string[] CustomsCodeListIdentifiers => new[] { CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0901_ADDITIONAL_INFORMATION, CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0902_ADDITIONAL_INFORMATION };

		protected override string CodeType => CodeListsConstants.Export.CodeTypes.EXPORT_AI44E_ADDITIONAL_INFORMATION;

		protected override string OutputFileNameSuffix => nameof(CodeListsConstants.Export.CodeTypes.EXPORT_AI44E_ADDITIONAL_INFORMATION);

		protected override XmlWriterConfiguration XmlWriterConfiguration => CodeListsHelper.GetRefCusCodeListWriterConfigurationWithAttributes(true, CodeType);

		protected override IEnumerable<string> InputAttributes => new List<string>();

		protected override IKeyValuesWithAttributes GetKeyValues(XElement entry)
		{
			var attributes = new List<KeyValueAttribute>();
			CodeListsHelper.AddToAttributes(attributes, CodeListsConstants.XMLEntryElementNames.LEVEL_ITEM, CodeListsConstants.AttributeValues.Item);
			return new BaseKeyValuesWithAttributesXML(entry, CodeListsConstants.XMLEntryElementNames.CODE, attributes);
		}
	}
}
