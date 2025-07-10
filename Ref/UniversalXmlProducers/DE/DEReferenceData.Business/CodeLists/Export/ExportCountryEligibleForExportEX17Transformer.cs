using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Export
{
	public class ExportCountryEligibleForExportEX17Transformer : CodeListsParserXML<RefCusCodeList, IKeyValues>
	{
		public ExportCountryEligibleForExportEX17Transformer(string[] downLoadLinks)
			: base(downLoadLinks)
		{
		}

		protected override string CustomsCodeListIdentifier => CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_C0207_COUNTRY_LIST_EX;

		protected override string CodeType => CodeListsConstants.Generic.CodeTypes.EX17_DESTINATION_COUNTRY_LIST;

		protected override string OutputFileNameSuffix => nameof(CodeListsConstants.Generic.CodeTypes.EX17_DESTINATION_COUNTRY_LIST);

		protected override XmlWriterConfiguration XmlWriterConfiguration => CodeListsHelper.GetRefCusCodeListWriterConfiguration(CodeType);

		protected override string XMLWriterDataSource => $"DE {CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_C0207_COUNTRY_LIST_EX}";

		protected override RefCusCodeList CreateRefList(IKeyValues keyValues) => CodeListsHelper.CreateRefList(keyValues);

		protected override IKeyValues GetKeyValues(XElement entry) => new BaseKeyValuesXML(entry, CodeListsConstants.XMLEntryElementNames.CODE);
	}
}
