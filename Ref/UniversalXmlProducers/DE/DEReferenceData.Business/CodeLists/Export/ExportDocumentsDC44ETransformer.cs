using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Export
{
	public class ExportDocumentsDC44ETransformer : ManyToOneCodeListsWithAttributesParserXML
	{
		public ExportDocumentsDC44ETransformer(string[] downloadLinks) : base(downloadLinks)
		{
		}

		protected override List<RefCusCodeList> ConvertAndCombineCodeLists(Dictionary<string, ParseResult<IKeyValuesWithAttributes>> parseResults)
			=> CombineListsWithLevelAttributes(parseResults, CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0922_DOCUMENTS, new (string, string)[] { (CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0921_DOCUMENTS, CodeListsConstants.AttributeValues.Header) });

		protected override string[] CustomsCodeListIdentifiers => new[] { CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0922_DOCUMENTS, CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0921_DOCUMENTS };

		protected override string CodeType => CodeListsConstants.Export.CodeTypes.EXPORT_DC44E_DOCUMENTS;

		protected override string OutputFileNameSuffix => nameof(CodeListsConstants.Export.CodeTypes.EXPORT_DC44E_DOCUMENTS);

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

			return new ExportI0922KeyValues(entry, CodeListsConstants.XMLEntryElementNames.CODE, attributes);
		}
	}

	class ExportI0922KeyValues : BaseKeyValuesWithAttributesXML, IKeyValuesWithAttributes
	{
		public ExportI0922KeyValues(XElement entry, string codeName, IList<KeyValueAttribute> attributes)
			: base(entry, codeName, attributes)
		{
		}

		public override string Code => code ?? (code = Entry.Element(CodeName)?.Value + Entry.Element(CodeListsConstants.XMLEntryElementNames.QUALIFIER)?.Value);
		string code;
	}
}
