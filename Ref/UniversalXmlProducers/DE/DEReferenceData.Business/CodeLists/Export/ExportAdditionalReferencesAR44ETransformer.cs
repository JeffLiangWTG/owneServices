using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Export
{
	public class ExportAdditionalReferencesAR44ETransformer : ManyToOneCodeListsWithAttributesParserXML
	{
		public ExportAdditionalReferencesAR44ETransformer(string[] downloadLinks) : base(downloadLinks)
		{
		}

		protected override List<RefCusCodeList> ConvertAndCombineCodeLists(Dictionary<string, ParseResult<IKeyValuesWithAttributes>> parseResults)
			=> CombineListsWithLevelAttributes(parseResults, CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0912_ADDITIONAL_REFERENCES, new (string, string)[] { (CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0911_ADDITIONAL_REFERENCES, CodeListsConstants.AttributeValues.Header) });

		protected override string[] CustomsCodeListIdentifiers => new[] { CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0912_ADDITIONAL_REFERENCES, CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0911_ADDITIONAL_REFERENCES };

		protected override string CodeType => CodeListsConstants.Export.CodeTypes.EXPORT_AR44E_ADDITIONAL_REFERENCES;

		protected override string OutputFileNameSuffix => nameof(CodeListsConstants.Export.CodeTypes.EXPORT_AR44E_ADDITIONAL_REFERENCES);

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
			
			return new ExportI0912KeyValues(entry, CodeListsConstants.XMLEntryElementNames.CODE, attributes);
		}
	}

	class ExportI0912KeyValues : BaseKeyValuesWithAttributesXML, IKeyValuesWithAttributes
	{
		public ExportI0912KeyValues(XElement entry, string codeName, IList<KeyValueAttribute> attributes)
			: base(entry, codeName, attributes)
		{
		}

		public override string Code => code ?? (code = Entry.Element(CodeName)?.Value + Entry.Element(CodeListsConstants.XMLEntryElementNames.QUALIFIER)?.Value);
		string code;
	}
}
