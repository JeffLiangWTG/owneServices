using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Import
{
	public class ImportMethodOfPaymentMOPTransformer : CodeListsParserXML<RefCusCodeList, IKeyValues>
	{
		public ImportMethodOfPaymentMOPTransformer(string[] downloadLinks)
			: base(downloadLinks)
		{
		}

		protected override string CustomsCodeListIdentifier => CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_A2060_METHOD_OF_PAYMENT;

		protected override string CodeType => CodeListsConstants.Import.CodeTypes.IMPORT_MOP_METHOD_OF_PAYMENT;

		protected override string OutputFileNameSuffix => nameof(CodeListsConstants.Import.CodeTypes.IMPORT_MOP_METHOD_OF_PAYMENT);

		protected override XmlWriterConfiguration XmlWriterConfiguration => CodeListsHelper.GetRefCusCodeListWriterConfigurationWithAttributes(false, CodeType);

		protected override RefCusCodeList CreateRefList(IKeyValues keyValues) => CodeListsHelper.CreateRefList(keyValues);

		protected override IKeyValues GetKeyValues(XElement entry) => new BaseKeyValuesXML(entry, CodeListsConstants.XMLEntryElementNames.CODE);
	}
}
