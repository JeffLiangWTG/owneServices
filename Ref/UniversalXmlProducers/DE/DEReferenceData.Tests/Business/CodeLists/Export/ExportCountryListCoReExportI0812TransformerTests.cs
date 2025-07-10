using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Export.Testing
{
	sealed class ExportCountryListCoReExportI0812TransformerTests : CodeListsXMLTests<RefCusCodeList, IKeyValues>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage,
		ITestInvalidDateErrorMessage
	{
		protected override string System => "Export";

		protected override string CustomsCodeListIdentifier => CodeListsConstants.Export.CustomsCodeListIdentifiers
			.EXPORT_I0812_COUNTRY_LIST_CO_REEXPORT;

		protected override string CodeType => nameof(CodeListsConstants.Export.CodeTypes.EXPORT_I0812_COUNTRY_LIST_CO_REEXPORT);

		protected override string DownloadUrl => CodeListsTestHelper.ExportDownloadURL(CustomsCodeListIdentifier, TestConstants.AESVersion3_0);

		protected override Func<string[], CodeListsParserXML<RefCusCodeList, IKeyValues>> ParserToRun => (downloadLinks) => new ExportCountryListCoReExportI0812Transformer(downloadLinks);

		protected override string InputFileName => nameof(CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0812_COUNTRY_LIST_CO_REEXPORT);

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: Zypern (Nordzypern)
StartDate: 2017-10-08T00:00:00
EndDate: 
";

		string ITestEmptyDescriptionErrorMessage.ExpectedEmptyDescriptionErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: QQ
Description: 
StartDate: 2009-06-27T00:00:00
EndDate: 
";

		string ITestInvalidDateErrorMessage.ExpectedInvalidDateErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: QV
Description: Nicht erm. Länder und Gebiete (EU)
StartDate: 2017-10-08T00:00:60
EndDate: 
";
	}
}
