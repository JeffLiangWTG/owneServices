using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Export.Testing
{
	sealed class ExportCurrencyCURRETransformerTests : CodeListsXMLTests<RefCusCodeList, IKeyValues>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage,
		ITestInvalidDateErrorMessage
	{
		protected override string System => "Export";

		protected override string CustomsCodeListIdentifier =>
			CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_C0352_CURRENCY;

		protected override string CodeType => nameof(CodeListsConstants.Export.CodeTypes.EXPORT_CURRE_CURRENCY);

		protected override string DownloadUrl => CodeListsTestHelper.ExportDownloadURL(CustomsCodeListIdentifier, TestConstants.AESVersion3_0);

		protected override Func<string[], CodeListsParserXML<RefCusCodeList, IKeyValues>> ParserToRun => (downLoadLinks) => new ExportCurrencyCURRETransformer(downLoadLinks);

		protected override string InputFileName => nameof(CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_C0352_CURRENCY);

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: Dirham
StartDate: 1900-01-02T00:00:00
EndDate: 
";

		string ITestEmptyDescriptionErrorMessage.ExpectedEmptyDescriptionErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: AFN
Description: 
StartDate: 1900-01-02T00:00:00
EndDate: 
";
		string ITestInvalidDateErrorMessage.ExpectedInvalidDateErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: ALL
Description: Lek
StartDate: 1900-01-02T00:00:00
EndDate: 1900-13-02T00:00:00
";
	}
}
