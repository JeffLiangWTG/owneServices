using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Export.Testing
{
	sealed class ExportCountryEligibleForExportEX17TransformerTests : CodeListsXMLTests<RefCusCodeList, IKeyValues>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage,
		ITestInvalidDateErrorMessage
	{
		protected override string System => "Export";

		protected override string CustomsCodeListIdentifier =>
			CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_C0207_COUNTRY_LIST_EX;

		protected override string CodeType => nameof(CodeListsConstants.Generic.CodeTypes.EX17_DESTINATION_COUNTRY_LIST);

		protected override string DownloadUrl => CodeListsTestHelper.ExportDownloadURL(CustomsCodeListIdentifier, TestConstants.AESVersion3_0);

		protected override Func<string[], CodeListsParserXML<RefCusCodeList, IKeyValues>> ParserToRun => (downloadLinks) => new ExportCountryEligibleForExportEX17Transformer(downloadLinks);

		protected override string InputFileName => nameof(CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_C0207_COUNTRY_LIST_EX);

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: Andorra
StartDate: 
EndDate: 
";

		string ITestEmptyDescriptionErrorMessage.ExpectedEmptyDescriptionErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: AE
Description: 
StartDate: 
EndDate: 
";

		string ITestInvalidDateErrorMessage.ExpectedInvalidDateErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: GB
Description: Vereinigtes Königreich
StartDate: 2021-13-01T00:00:00
EndDate: 2021-12-31T00:00:00
";
	}
}
