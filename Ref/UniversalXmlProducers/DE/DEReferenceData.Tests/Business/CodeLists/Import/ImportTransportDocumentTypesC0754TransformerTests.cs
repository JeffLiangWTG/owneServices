using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Import.Testing
{
	sealed class ImportTransportDocumentTypesC0754TransformerTests : CodeListsXMLTests<RefCusCodeList, IKeyValues>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage,
		ITestInvalidDateErrorMessage
	{
		protected override string System => "Import";

		protected override string CustomsCodeListIdentifier =>
			CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_C0754_TRANSPORT_DOCUMENT_TYPES;

		protected override string CodeType => nameof(CodeListsConstants.Import.CodeTypes.IMPORT_C0754_TRANSPORT_DOCUMENT_TYPES);

		protected override string DownloadUrl => CodeListsTestHelper.ImportXmlDownloadUrl(CustomsCodeListIdentifier, TestConstants.AtlasVersion10_1);

		protected override Func<string[], CodeListsParserXML<RefCusCodeList, IKeyValues>> ParserToRun => downloadLinks => new ImportTransportDocumentTypesC0754Transformer(downloadLinks);

		protected override string InputFileName =>
			nameof(CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_C0754_TRANSPORT_DOCUMENT_TYPES);

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: Luftfrachtbrief, ausgestellt von der Fluggesellschaft (Master air waybill)
";

		string ITestEmptyDescriptionErrorMessage.ExpectedEmptyDescriptionErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: N750
Description: 
";

		string ITestInvalidDateErrorMessage.ExpectedInvalidDateErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: N760
Description: Multimodal/kombiniert Transportdokument
StartDate: 2024-13-13T00:00:00
EndDate: 
";
	}
}
