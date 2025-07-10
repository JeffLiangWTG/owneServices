using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Import
{
	public class ImportNatureOfTransactionTRNATTransformer : CodeListsParserXML<RefCusCodeList, IKeyValues>
	{
		public ImportNatureOfTransactionTRNATTransformer(string[] downLoadLinks)
			: base(downLoadLinks)
		{
		}

		protected override string CustomsCodeListIdentifier => CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_A1150_TRANSACTION_NATURE;

		protected override string CodeType => CodeListsConstants.Import.CodeTypes.IMPORT_TRNAT_TRANSACTION_NATURE;

		protected override string OutputFileNameSuffix => nameof(CodeListsConstants.Import.CodeTypes.IMPORT_TRNAT_TRANSACTION_NATURE);

		protected override XmlWriterConfiguration XmlWriterConfiguration => CodeListsHelper.GetRefCusCodeListWriterConfiguration(CodeType);

		protected override RefCusCodeList CreateRefList(IKeyValues keyValues) => CodeListsHelper.CreateRefList(keyValues);

		protected override IKeyValues GetKeyValues(XElement entry) => new BaseKeyValuesXML(entry, CodeListsConstants.XMLEntryElementNames.CODE);
	}
}
