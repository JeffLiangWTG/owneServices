using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Import
{
	public class ImportTaricUnitsOfQuantityCUSUQTransformer : CodeListsWithAttributesParserXML
	{
		public ImportTaricUnitsOfQuantityCUSUQTransformer(string[] downloadLinks)
			: base(downloadLinks)
		{ }

		protected override string CustomsCodeListIdentifier => CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_I0700_CUSTOMS_DECLARATION_UNITS_OF_QUANTITY;

		protected override string CodeType => CodeListsConstants.Import.CodeTypes.IMPORT_CUSUQ_CUSTOMS_DECLARATION_UNITS_OF_QUANTITY;

		protected override string OutputFileNameSuffix => nameof(CodeListsConstants.Import.CodeTypes.IMPORT_CUSUQ_CUSTOMS_DECLARATION_UNITS_OF_QUANTITY);

		protected override XmlWriterConfiguration XmlWriterConfiguration => CodeListsHelper.GetRefCusCodeListWriterConfigurationWithAttributes(false, CodeType);

		protected override IEnumerable<string> InputAttributes => new[] { CodeListsConstants.XMLEntryElementNames.QUALIFIER };

		protected override RefCusCodeList CreateRefList(IKeyValuesWithAttributes keyValues) => CodeListsHelper.CreateRefListWithAttributes(keyValues);

		protected override IKeyValuesWithAttributes GetKeyValues(XElement entry)
		{
			var attributes = new List<KeyValueAttribute>();
			var importI0700KeyValues = new ImportI0700KeyValues(entry, CodeListsConstants.XMLEntryElementNames.CODE, attributes);

			if (codesWithNoMergeAttribute.Contains(importI0700KeyValues.Code))
			{
				CodeListsHelper.AddToAttributes(attributes, CodeListsConstants.XMLEntryElementNames.NOMERGE, CodeListsConstants.AttributeValues.Yes);
			}

			return importI0700KeyValues;
		}

		readonly HashSet<string> codesWithNoMergeAttribute = new HashSet<string>(new string[] { "ASV", "ASVX", "062", "063" });
	}

	class ImportI0700KeyValues : BaseKeyValuesWithAttributesXML, IKeyValues
	{
		public ImportI0700KeyValues(XElement entry, string codeName, IList<KeyValueAttribute> attributes)
			: base(entry, codeName, attributes)
		{
		}

		public override string Code => code ?? (code = Entry.Element(CodeName)?.Value + Entry.Element(CodeListsConstants.XMLEntryElementNames.QUALIFIER)?.Value);
		string code;
	}
}
