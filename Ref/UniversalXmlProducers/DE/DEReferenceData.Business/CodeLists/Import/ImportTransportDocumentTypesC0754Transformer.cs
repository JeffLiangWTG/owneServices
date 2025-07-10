using System.Text.RegularExpressions;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Import
{
	public class ImportTransportDocumentTypesC0754Transformer : CodeListsParserXML<RefCusCodeList, IKeyValues>
	{
		public ImportTransportDocumentTypesC0754Transformer(string[] downloadLinks)
			: base(downloadLinks)
		{
		}

		protected override string CodeType => CodeListsConstants.Import.CodeTypes.IMPORT_C0754_TRANSPORT_DOCUMENT_TYPES;

		protected override string OutputFileNameSuffix => nameof(CodeListsConstants.Import.CodeTypes.IMPORT_C0754_TRANSPORT_DOCUMENT_TYPES);

		protected override XmlWriterConfiguration XmlWriterConfiguration => CodeListsHelper.GetRefCusCodeListWriterConfiguration(CodeType);

		protected override IKeyValues GetKeyValues(XElement entry) => new ExportC0754KeyValues(entry, CodeListsConstants.XMLEntryElementNames.CODE);

		protected override string CustomsCodeListIdentifier => CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_C0754_TRANSPORT_DOCUMENT_TYPES;

		protected override RefCusCodeList CreateRefList(IKeyValues keyValues) => CodeListsHelper.CreateRefList(keyValues);
	}

	class ExportC0754KeyValues : BaseKeyValuesXML
	{
		public ExportC0754KeyValues(XElement entry, string codeName) : base(entry, codeName)
		{
		}

        static string FormatDescription(string description) => string.IsNullOrEmpty(description) ? string.Empty : Regex.Replace(description, @"\t|\n|\r", "").Trim();

		public override string Description => FormatDescription(base.Description);
	}
}
