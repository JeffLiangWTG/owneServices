using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Ncts
{
	public class NctsCountryCodesFullListNC008Transformer : CodeListsParserXML<RefCusCodeList, IKeyValues>
	{
		public NctsCountryCodesFullListNC008Transformer(string[] downloadLinks)
			: base(downloadLinks)
		{
		}
		protected override string CustomsCodeListIdentifier => CodeListsConstants.Ncts.CustomsCodeListIdentifiers.NCTS_C0008_COUNTRY_CODES_FULL_LIST;

		protected override string CodeType => CodeListsConstants.Ncts.CodeTypes.NCTS_NC008_COUNTRY_CODES_FULL_LIST;
		
		protected override string OutputFileNameSuffix => nameof(CodeListsConstants.Ncts.CodeTypes.NCTS_NC008_COUNTRY_CODES_FULL_LIST);

		protected override XmlWriterConfiguration XmlWriterConfiguration => CodeListsHelper.GetRefCusCodeListWriterConfiguration(CodeType);

		protected override RefCusCodeList CreateRefList(IKeyValues keyValues) => CodeListsHelper.CreateRefList(keyValues);

		protected override IKeyValues GetKeyValues(XElement entry) => new BaseKeyValuesXML(entry, CodeListsConstants.XMLEntryElementNames.CODE);
	}
}
