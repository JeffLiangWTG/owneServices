using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Export
{
	public class ExportCountryListCoReExportI0812Transformer : CodeListsParserXML<RefCusCodeList, IKeyValues>
	{
		public ExportCountryListCoReExportI0812Transformer(string[] downLoadLinks)
			: base(downLoadLinks)
		{
		}

		protected override string CustomsCodeListIdentifier => CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0812_COUNTRY_LIST_CO_REEXPORT;

		protected override string CodeType => CodeListsConstants.Export.CodeTypes.EXPORT_I0812_COUNTRY_LIST_CO_REEXPORT;

		protected override string OutputFileNameSuffix => nameof(CodeListsConstants.Export.CodeTypes.EXPORT_I0812_COUNTRY_LIST_CO_REEXPORT);

		protected override XmlWriterConfiguration XmlWriterConfiguration => CodeListsHelper.GetRefCusCodeListWriterConfiguration(CodeType);

		protected override RefCusCodeList CreateRefList(IKeyValues keyValues) => CodeListsHelper.CreateRefList(keyValues);

		protected override IKeyValues GetKeyValues(XElement entry) => new BaseKeyValuesXML(entry, CodeListsConstants.XMLEntryElementNames.CODE);
	}
}
