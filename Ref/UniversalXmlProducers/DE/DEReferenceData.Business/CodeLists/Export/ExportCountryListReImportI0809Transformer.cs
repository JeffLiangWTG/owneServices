using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Export
{
	public class ExportCountryListReImportI0809Transformer : CodeListsParserXML<RefCusCodeList, IKeyValues>
	{
		public ExportCountryListReImportI0809Transformer(string[] downloadLinks)
			: base(downloadLinks)
		{ }

		protected override string CustomsCodeListIdentifier => CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0809_COUNTRY_LIST_REIMPORT;

		protected override string CodeType => CodeListsConstants.Export.CodeTypes.EXPORT_I0809_COUNTRY_LIST_REIMPORT;

		protected override string OutputFileNameSuffix => nameof(CodeListsConstants.Export.CodeTypes.EXPORT_I0809_COUNTRY_LIST_REIMPORT);

		protected override XmlWriterConfiguration XmlWriterConfiguration => CodeListsHelper.GetRefCusCodeListWriterConfiguration(CodeType);

		protected override RefCusCodeList CreateRefList(IKeyValues keyValues) => CodeListsHelper.CreateRefList(keyValues);

		protected override IKeyValues GetKeyValues(XElement entry) => new BaseKeyValuesXML(entry, CodeListsConstants.XMLEntryElementNames.CODE);
	}
}
