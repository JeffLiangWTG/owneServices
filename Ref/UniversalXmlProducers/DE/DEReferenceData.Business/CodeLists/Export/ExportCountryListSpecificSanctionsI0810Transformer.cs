using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Export
{
	public class ExportCountryListSpecificSanctionsI0810Transformer : CodeListsParserXML<RefCusCodeList, IKeyValues>
	{
		public ExportCountryListSpecificSanctionsI0810Transformer(string[] downLoadLinks)
			: base(downLoadLinks)
		{
		}

		protected override string CustomsCodeListIdentifier => CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0810_COUNTRY_LIST_SPECIFIC_SANCTIONS;

		protected override string CodeType => CodeListsConstants.Export.CodeTypes.EXPORT_I0810_COUNTRY_LIST_SPECIFIC_SANCTIONS;

		protected override string OutputFileNameSuffix => nameof(CodeListsConstants.Export.CodeTypes.EXPORT_I0810_COUNTRY_LIST_SPECIFIC_SANCTIONS);

		protected override XmlWriterConfiguration XmlWriterConfiguration => CodeListsHelper.GetRefCusCodeListWriterConfiguration(CodeType);

		protected override RefCusCodeList CreateRefList(IKeyValues keyValues) => CodeListsHelper.CreateRefList(keyValues);

		protected override IKeyValues GetKeyValues(XElement entry) => new BaseKeyValuesXML(entry, CodeListsConstants.XMLEntryElementNames.CODE);
	}
}
