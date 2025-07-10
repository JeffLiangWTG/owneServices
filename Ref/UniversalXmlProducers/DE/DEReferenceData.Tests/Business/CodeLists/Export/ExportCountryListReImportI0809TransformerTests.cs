using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Export.Testing
{
	sealed class ExportCountryListReImportI0809TransformerTests : CodeListsXMLTests<RefCusCodeList, IKeyValues>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage,
		ITestInvalidDateErrorMessage
	{
		protected override string System => "Export";

		protected override string CustomsCodeListIdentifier =>
			CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0809_COUNTRY_LIST_REIMPORT;

		protected override string CodeType => nameof(CodeListsConstants.Export.CodeTypes.EXPORT_I0809_COUNTRY_LIST_REIMPORT);

		protected override string DownloadUrl => CodeListsTestHelper.ExportDownloadURL(CustomsCodeListIdentifier, TestConstants.AESVersion3_0);

		protected override Func<string[], CodeListsParserXML<RefCusCodeList, IKeyValues>> ParserToRun => (downloadLinks) => new ExportCountryListReImportI0809Transformer(downloadLinks);

		protected override string InputFileName => nameof(CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0809_COUNTRY_LIST_REIMPORT);

		public string ExpectedEmptyCodeErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: Österreich
StartDate: 
EndDate: 
";

		public string ExpectedEmptyDescriptionErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: AX
Description: 
StartDate: 2005-12-07T00:00:00
EndDate: 2010-04-08T23:59:59
";

		public string ExpectedInvalidDateErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: DE
Description: Bundesrepublik Deutschland
StartDate: 
EndDate: 2001-23-02T23:59:59
";
	}
}
