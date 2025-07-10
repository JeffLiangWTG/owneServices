using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Export
{
	public class ExportTransportDocumentsTD44ETransformer : CodeListsWithAttributesParserXML
	{
		public ExportTransportDocumentsTD44ETransformer(string[] downLoadLinks)
			: base(downLoadLinks)
		{
		}

		protected override string CustomsCodeListIdentifier => CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0941_TRANSPORT_DOCUMENT;

		protected override string CodeType => CodeListsConstants.Export.CodeTypes.EXPORT_TD44E_TRANSPORT_DOCUMENT;

		protected override string OutputFileNameSuffix => nameof(CodeListsConstants.Export.CodeTypes.EXPORT_TD44E_TRANSPORT_DOCUMENT);

		protected override XmlWriterConfiguration XmlWriterConfiguration => CodeListsHelper.GetRefCusCodeListWriterConfigurationWithAttributes(false, CodeType, "DE");

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
			CodeListsConstants.XMLEntryElementNames.VALUE,
			CodeListsConstants.XMLEntryElementNames.COPY
		};

		protected override IKeyValuesWithAttributes GetKeyValues(XElement entry)
		{
			var attributes = new List<KeyValueAttribute>();
			CodeListsHelper.AddToAttributes(attributes, CodeListsConstants.XMLEntryElementNames.LEVEL_HEADER, CodeListsConstants.AttributeValues.Header);
			foreach (var allowedAttribute in InputAttributes.Where(x => x != CodeListsConstants.XMLEntryElementNames.COPY)) 
			{
				CodeListsHelper.AddAllowedToAttributesAsBool(attributes, allowedAttribute, entry.Element(allowedAttribute)?.Value);
			}

			return new BaseKeyValuesWithAttributesXML(entry, CodeListsConstants.XMLEntryElementNames.CODE, attributes);
		}

		protected override RefCusCodeList CreateRefList(IKeyValuesWithAttributes keyValues) => CodeListsHelper.CreateRefListWithAttributes(keyValues);
	}
}

