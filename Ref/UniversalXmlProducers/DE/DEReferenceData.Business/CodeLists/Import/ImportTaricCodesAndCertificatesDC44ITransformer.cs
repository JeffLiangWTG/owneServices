using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Import
{
	public class ImportTaricCodesAndCertificatesDC44ITransformer : ManyToOneCodeListsWithAttributesParserXML
	{
		public ImportTaricCodesAndCertificatesDC44ITransformer(string[] downloadLinks)
			: base(downloadLinks)
		{
		}

		protected override List<RefCusCodeList> ConvertAndCombineCodeLists(Dictionary<string, ParseResult<IKeyValuesWithAttributes>> parseResults) =>
			CombineListsWithAttributes(parseResults,
				CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_I0200_TARIC_CODES_AND_CERTIFICATES,
				new (string, string)[] { (CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_I0255_ZELOS_DOCUMENTS, CodeListsConstants.XMLEntryElementNames.OBLIGATION) },
				(list, attributesList, _) => CodeListsHelper.UpdateRefCusCodeListWithObligationAttribute(list, attributesList));

		protected override string[] CustomsCodeListIdentifiers =>
		[
			CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_I0200_TARIC_CODES_AND_CERTIFICATES,
			CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_I0255_ZELOS_DOCUMENTS
		];

		protected override string CodeType => CodeListsConstants.Import.CodeTypes.IMPORT_DC44I_TARIC_CODES_AND_CERTIFICATES;

		protected override string OutputFileNameSuffix => nameof(CodeListsConstants.Import.CodeTypes.IMPORT_DC44I_TARIC_CODES_AND_CERTIFICATES);

		protected override XmlWriterConfiguration XmlWriterConfiguration => CodeListsHelper.GetRefCusCodeListWriterConfigurationWithAttributes(true, CodeType);

		protected override string XMLWriterDataSource => $"DE {CodeListsConstants.Import.CodeTypes.IMPORT_DOC44I_TARIC_CODES_AND_CERTIFICATES}";

		protected override IEnumerable<string> InputAttributes => new[]
		{
			CodeListsConstants.XMLEntryElementNames.DIVISION
		};

		protected override string CodeAttributeName => codeName;

		protected override IKeyValuesWithAttributes GetKeyValues(XElement entry)
		{
			var divisionValue = entry.Element(CodeListsConstants.XMLEntryElementNames.DIVISION)?.Value;
			var obligationValue = entry.Element(CodeListsConstants.XMLEntryElementNames.OBLIGATION)?.Value;
			var attributes = new List<KeyValueAttribute>();
			CodeListsHelper.AddToAttributes(attributes, CodeListsConstants.XMLEntryElementNames.DIVISION, divisionValue);
			CodeListsHelper.AddToAttributes(attributes, CodeListsConstants.XMLEntryElementNames.LEVEL_ITEM, CodeListsConstants.AttributeValues.Item);
			CodeListsHelper.AddToAttributes(attributes, CodeListsConstants.XMLEntryElementNames.LEVEL_HEADER, CodeListsConstants.AttributeValues.Header, (x) => !DocumentCodesInvalidForHeaderLevel.Contains(entry.Element(CodeListsConstants.XMLEntryElementNames.DOCUMENT_CODE)?.Value) && divisionValue == "4");
			CodeListsHelper.AddToAttributes(attributes, CodeListsConstants.XMLEntryElementNames.OBLIGATION, "Y", _ => obligationValue == "1");

			codeName = entry.Element(CodeListsConstants.XMLEntryElementNames.DOCUMENT_CODE) != null ? CodeListsConstants.XMLEntryElementNames.DOCUMENT_CODE : CodeListsConstants.XMLEntryElementNames.CODE;

			return new BaseKeyValuesWithAttributesXML(entry, codeName, attributes);
		}

		string codeName;

		HashSet<string> DocumentCodesInvalidForHeaderLevel => documentCodesInvalidForHeaderLevel ?? (documentCodesInvalidForHeaderLevel = new HashSet<string> { "C626", "C627", "9DFC", "9DFD" });
		HashSet<string> documentCodesInvalidForHeaderLevel;
	}
}
