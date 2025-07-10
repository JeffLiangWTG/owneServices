using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Export
{
	public class ExportCountryCodesCommunityEX15Transformer : CodeListsParserXML<RefCusCodeList, IKeyValues>
	{
		public ExportCountryCodesCommunityEX15Transformer(string[] downLoadLinks)
			: base(downLoadLinks)
		{
		}

		protected override string CustomsCodeListIdentifier => CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_C0010_COUNTRY_CODES_COMMUNITY;

		protected override string CodeType => CodeListsConstants.Generic.CodeTypes.EX15_ORIGIN_COUNTRY_LIST;

		protected override string OutputFileNameSuffix => nameof(CodeListsConstants.Generic.CodeTypes.EX15_ORIGIN_COUNTRY_LIST);

		protected override XmlWriterConfiguration XmlWriterConfiguration => CodeListsHelper.GetRefCusCodeListWriterConfiguration(CodeType);

		protected override IKeyValues GetKeyValues(XElement entry) => new BaseKeyValuesXML(entry, CodeListsConstants.XMLEntryElementNames.CODE);

		protected override RefCusCodeList CreateRefList(IKeyValues keyValues) => CodeListsHelper.CreateRefList(keyValues);
	}
}

