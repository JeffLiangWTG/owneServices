using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Import.Testing
{
	sealed class ImportNatureOfTransactionTRNATTransformerTests : CodeListsXMLTests<RefCusCodeList, IKeyValues>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage
	{
		protected override string System => "Import";

		protected override string CustomsCodeListIdentifier =>
			CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_A1150_TRANSACTION_NATURE;

		protected override string CodeType => nameof(CodeListsConstants.Import.CodeTypes.IMPORT_TRNAT_TRANSACTION_NATURE);

		protected override string DownloadUrl => CodeListsTestHelper.ImportXmlDownloadUrl(CustomsCodeListIdentifier, TestConstants.AtlasVersion10_0);

		protected override Func<string[], CodeListsParserXML<RefCusCodeList, IKeyValues>> ParserToRun => (downloadLinks) => new ImportNatureOfTransactionTRNATTransformer(downloadLinks);

		protected override string InputFileName => nameof(CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_A1150_TRANSACTION_NATURE);

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: Endgültiger Kauf/Verkauf
";

		string ITestEmptyDescriptionErrorMessage.ExpectedEmptyDescriptionErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: 12
Description: 
";
	}
}
