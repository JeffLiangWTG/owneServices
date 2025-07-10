using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Export.Testing
{
	sealed class ExportTransportDocumentsTD44ETransformerTests : CodeListsXMLTests<RefCusCodeList, IKeyValuesWithAttributes>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage,
		ITestInvalidDateErrorMessage
	{
		protected override string System => "Export";

		protected override string CustomsCodeListIdentifier =>
			CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0941_TRANSPORT_DOCUMENT;

		protected override string CodeType => nameof(CodeListsConstants.Export.CodeTypes.EXPORT_TD44E_TRANSPORT_DOCUMENT);

		protected override string DownloadUrl => CodeListsTestHelper.ExportDownloadURL(CustomsCodeListIdentifier, TestConstants.AESVersion3_0);

		protected override Func<string[], CodeListsParserXML<RefCusCodeList, IKeyValuesWithAttributes>> ParserToRun => (downLoadLinks) => new ExportTransportDocumentsTD44ETransformer(downLoadLinks);

		protected override string InputFileName => nameof(CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0941_TRANSPORT_DOCUMENT);

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: Sonstige Unterlagen ZELOS (Original)
StartDate: 2021-03-07T00:00:00
EndDate: 
";

		public string ExpectedEmptyDescriptionErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: 9ZZY
Description: 
StartDate: 2021-03-07T00:00:00
EndDate: 
";

		public string ExpectedInvalidDateErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: 9ZZZ
Description: Sonstiges
StartDate: 2021-13-07T00:00:00
EndDate: 
";
	}
}
