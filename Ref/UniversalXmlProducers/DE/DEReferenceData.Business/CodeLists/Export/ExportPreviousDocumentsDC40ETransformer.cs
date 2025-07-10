using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Export
{
	public class ExportPreviousDocumentsDC40ETransformer : ManyToOneCodeListsWithAttributesParserXML
	{
		public ExportPreviousDocumentsDC40ETransformer(string[] downLoadLinks)
			: base(downLoadLinks)
		{
		}

		protected override List<RefCusCodeList> ConvertAndCombineCodeLists(
			Dictionary<string, ParseResult<IKeyValuesWithAttributes>> parseResults)
			=> CombineListsWithLevelAttributes(parseResults,
				CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0932_PREVIOUS_DOCUMENTS,
				new (string, string)[]
				{
					(CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0931_PREVIOUS_DOCUMENTS,
						CodeListsConstants.AttributeValues.Header)
				});

		protected override string[] CustomsCodeListIdentifiers => new[]
		{
			CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0931_PREVIOUS_DOCUMENTS,
			CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0932_PREVIOUS_DOCUMENTS
		};

		protected override string CodeType => CodeListsConstants.Export.CodeTypes.EXPORT_DC40E_PREVIOUS_DOCUMENTS;

		protected override string OutputFileNameSuffix => nameof(CodeListsConstants.Export.CodeTypes.EXPORT_DC40E_PREVIOUS_DOCUMENTS);

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
