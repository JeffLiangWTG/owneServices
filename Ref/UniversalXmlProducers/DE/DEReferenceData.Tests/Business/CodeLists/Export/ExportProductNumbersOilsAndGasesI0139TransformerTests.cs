using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Export.Testing
{
	sealed class ExportProductNumbersOilsAndGasesI0139TransformerTests : CodeListsXMLTests<RefCusCodeList, IKeyValues>,
		ITestEmptyCodeErrorMessage,
		ITestEmptyDescriptionErrorMessage,
		ITestInvalidDateErrorMessage
	{
		protected override string System => "Export";

		protected override string CustomsCodeListIdentifier =>
			CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0139_PRODUCT_NUMBERS_OILS_AND_GASES;

		protected override string CodeType => nameof(CodeListsConstants.Export.CodeTypes.EXPORT_I0139_PRODUCT_NUMBERS_OILS_AND_GASES);

		protected override string DownloadUrl => CodeListsTestHelper.ExportDownloadURL(CustomsCodeListIdentifier, TestConstants.AESVersion3_0);

		protected override Func<string[], CodeListsParserXML<RefCusCodeList, IKeyValues>> ParserToRun => (downLoadLinks) => new ExportProductNumbersOilsAndGasesI0139Transformer(downLoadLinks);

		protected override string InputFileName => nameof(CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0139_PRODUCT_NUMBERS_OILS_AND_GASES);

		string ITestEmptyCodeErrorMessage.ExpectedEmptyCodeErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: Benzole
StartDate: 2018-08-07T00:00:00
EndDate: 
";

		string ITestEmptyDescriptionErrorMessage.ExpectedEmptyDescriptionErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: 27071010
Description: 
StartDate: 2009-03-01T00:00:00
EndDate: 2009-08-07T23:59:59
";

		string ITestInvalidDateErrorMessage.ExpectedInvalidDateErrorMessage => $@"Unable to import record from {CustomsCodeListIdentifier} due to empty Code, Description or invalid Date.
DETAILS:
Code: 27073000
Description: Xylole
StartDate: 2018-08-37T00:00:00
EndDate: 
";
	}
}
